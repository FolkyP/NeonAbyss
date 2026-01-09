using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float dashSpeed; // Speed during dashing

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier; // Multiplier for speed in the air
    bool readyToJump = true;

    public float groundDrag;
    public Transform orientation;
    public float horizontalInput;
    public float verticalInput;
    Vector3 moveDirection;

    Rigidbody rb;
    public MovementState state;
    public enum MovementState
    {
        Walking,
        Sprinting,
        Dashing,
        Airborne
    }
    [Header("Keybinds")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    public bool movementLocked = false;
    public bool dashing;
    public GameSettings gameSettings;

    private Collider playerCollider;
    float slashAdvanceTimee = 0.1f;

    private float baseWalk;
    private float baseSprint;
    private float baseDash;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Auto-detect height if using a capsule collider
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            playerHeight = col.height;
            playerCollider = col;
        }
        else
        {
            playerCollider = GetComponent<Collider>();
        }
    }
    private void StateHandler()
    {
        if (dashing)
        {
            state = MovementState.Dashing;
            moveSpeed = dashSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.Airborne;
            moveSpeed = walkSpeed * airMultiplier; // Use air multiplier for speed in the air
        }
    }
    private void MyInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Input.GetKey(forwardKey)) verticalInput += 1f;
        if (Input.GetKey(backwardKey)) verticalInput -= 1f;
        if (Input.GetKey(leftKey)) horizontalInput -= 1f;
        if (Input.GetKey(rightKey)) horizontalInput += 1f;

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            Debug.Log("Jumping");
            readyToJump = false; // Prevent multiple jumps in quick succession
            Jump();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (gameSettings.isGameOn == false) return;
        if (GameSettings.Instance.isOverDriveActive)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            StateHandler();
            return;
        }

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        MyInput();
        SpeedControl();
        StateHandler();
        if (state == MovementState.Walking || state == MovementState.Sprinting)
        {
            rb.drag = groundDrag; // Apply ground drag when grounded
        }
        else
        {
            rb.drag = 0; // No drag when in the air
        }
    }
    private void FixedUpdate()
    {
        if (GameSettings.Instance.isOverDriveActive)
        {
            // úplné zastavení pohybu po zemi
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }
        MovePl();
    }

    private void MovePl()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }
    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
    public void Jump()
    {

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Invoke(nameof(ResetJump), jumpCooldown);

    }
    private void ResetJump()
    {
        readyToJump = true;
    }
    public IEnumerator PhaseDashThroughEnemy(
    GameObject enemy,
    float delayBeforeDash,
    float dashThroughDistance,
    float dashDuration,
    GameObject meleePrefabToSpawn,
    Quaternion prefabRotation,
    System.Action onDashStart
)
    {
        if (enemy == null)
            yield break;

        float slashTime = Mathf.Max(0f, delayBeforeDash - slashAdvanceTimee);

        // èekání do slashe
        if (slashTime > 0f)
            yield return new WaitForSeconds(slashTime);

        // SLASH – døív než dash
        onDashStart?.Invoke();

        // zbytek èasu do dash startu
        float remaining = delayBeforeDash - slashTime;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);


        if (enemy == null)
            yield break;

        // 2) pøiprava: ignoruj kolize mezi hráèem a všemi enemy collidery
        Collider[] enemyColliders = enemy.GetComponentsInChildren<Collider>();
        List<Collider> ignored = new List<Collider>();
        if (playerCollider == null)
            playerCollider = GetComponent<Collider>();

        if (playerCollider == null)
            Debug.LogWarning("PlayerMovement: playerCollider missing — cannot ignore collisions with enemy.");

        foreach (var ec in enemyColliders)
        {
            if (ec == null) continue;
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, ec, true);
                ignored.Add(ec);
            }
        }

        // 3) zamkni movement a nastav dashing flagy
        movementLocked = true;
        dashing = true;

        // 4) spoèítej cíl — projdi skrz enemy na jeho zadní stranu
        Vector3 startPos = transform.position;
        Vector3 enemyCenter = enemy.transform.position;
        Vector3 forward = enemy.transform.forward;
        Vector3 targetPos = enemyCenter + forward.normalized * dashThroughDistance;
        // udrž Y hráèe (nezvedat/doèasnì)
        targetPos.y = startPos.y;

        // 5) fyzikální pøesun bìhem FixedUpdate pomocí MovePosition
        float elapsed = 0f;
        float fixedStep = Time.fixedDeltaTime;
        while (elapsed < dashDuration)
        {
            elapsed += fixedStep;
            float t = Mathf.Clamp01(elapsed / dashDuration);
            Vector3 next = Vector3.Lerp(startPos, targetPos, t);
            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }
        // poslední korekce
        rb.MovePosition(targetPos);

        //yield return new WaitForSeconds(0.15f);

        //if (enemy != null)
        //{
        //    yield return StartCoroutine(
        //        SmoothRotateTowards(
        //            enemy.transform.position,
        //            0.3f // délka rotace
        //        )
        //    );
        //}



        dashing = false;
        movementLocked = false;
        yield break;
    }
    private IEnumerator SmoothRotateTowards(
    Vector3 worldTargetPos,
    float duration
)
    {
        Vector3 dir = worldTargetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            yield break;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            Quaternion rot = Quaternion.Slerp(startRot, targetRot, t);
            transform.rotation = rot;

            // sladìní kamery (yaw)
            if (PlayerCam.Instance != null)
                PlayerCam.Instance.SetYawSoft(rot.eulerAngles.y);

            yield return null;
        }

        transform.rotation = targetRot;

        if (PlayerCam.Instance != null)
            PlayerCam.Instance.SetYawImmediate(targetRot.eulerAngles.y);
    }
    public void CacheBaseSpeeds()
    {
        baseWalk = walkSpeed;
        baseSprint = sprintSpeed;
        baseDash = dashSpeed;
    }
    public void ApplySpeedMultiplier(float multiplier)
    {
        walkSpeed = baseWalk * multiplier;
        sprintSpeed = baseSprint * multiplier;
        dashSpeed = baseDash * multiplier;
    }

    public void RestoreBaseSpeeds()
    {
        walkSpeed = baseWalk;
        sprintSpeed = baseSprint;
        dashSpeed = baseDash;
    }

}
