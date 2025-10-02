using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dashing : MonoBehaviour
{
    [SerializeField] GameSettings gameSettings;
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public Transform PlayerCam;
    private PlayerMovement playerMovement;

    [Header("Dash Settings")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    public float dashCooldown;
    private float dashTimer;

    public KeyCode dashKey = KeyCode.LeftAlt;

    [Header("UI")]
    public GameObject dashButton;       // The button object (parent with children)
    public Image cooldownFillImage;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    private void Update()
    {
        if (gameSettings.isGameStopped) return;

        if (Input.GetKeyDown(dashKey))
        {
            Dash();
        }

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;

            // Update UI when on cooldown
            float alpha = 0.4f; // semi-transparent while cooling down
            SetSkillNormalColor(dashButton, alpha);

            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = dashTimer / dashCooldown;
            }
        }
        else
        {
            // Ready state
            SetSkillNormalColor(dashButton, 1f);

            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = 0f;
            }
        }


    }
    private void SetSkillNormalColor(GameObject iconParent, float alpha)
    {
        if (iconParent == null) return;

        // Change all RawImages
        RawImage[] rawImages = iconParent.GetComponentsInChildren<RawImage>();
        foreach (RawImage img in rawImages)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

        // Change all Images
        Image[] images = iconParent.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }


    private void Dash()
    {
        if (dashTimer > 0) return; // Prevent dashing if cooldown is active
        dashTimer = dashCooldown; // Reset cooldown

        playerMovement.dashing = true;

        // Get movement input from your PlayerMovement script
        float horizontal = playerMovement.horizontalInput; // e.g., A/D or left/right keys
        float vertical = playerMovement.verticalInput;     // e.g., W/S or forward/back keys

        Vector3 moveDirection = orientation.forward * vertical + orientation.right * horizontal;

        // If no input, dash forward by default
        if (moveDirection == Vector3.zero)
            moveDirection = orientation.forward;

        moveDirection.Normalize();

        Vector3 forceToApply = moveDirection * dashForce + orientation.up * dashUpwardForce;
        delayDashForce = forceToApply;

        Invoke(nameof(DelayedDashForce), 0.025f);
        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayDashForce;
    private void DelayedDashForce()
    {
        rb.AddForce(delayDashForce, ForceMode.Impulse);

    }
    private void ResetDash()
    {
        playerMovement.dashing = false;
    }
}
