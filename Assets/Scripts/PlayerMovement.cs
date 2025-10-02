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


    public bool dashing;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Auto-detect height if using a capsule collider
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            playerHeight = col.height;
        }
    }
    private void StateHandler()
    {
        if(dashing)
        {
            state = MovementState.Dashing;
            moveSpeed = dashSpeed; 
        }
        else if(grounded && Input.GetKey(sprintKey))
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
    private void MyInput() {
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
        MovePl();
    }

    private void MovePl()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if(grounded)
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
        
            rb.velocity =new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Invoke(nameof(ResetJump), jumpCooldown);
        
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
}
