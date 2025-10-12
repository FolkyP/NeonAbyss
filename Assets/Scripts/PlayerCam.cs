using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public static PlayerCam Instance;
    private Vector3 originalpos;
    private Coroutine shakeCoroutine;

    public float sensX;
    public float sensY;

    public Transform orientation;

    float rotationX;
    float rotationY;
    float sens;

    [SerializeField] GameSettings gameSettings;

    private void Awake()
    {
        Instance = this;
        // cache the initial local position as a sensible default,
        // but we'll capture the current baseline each time a shake starts.
        originalpos = transform.localPosition;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (gameSettings != null && gameSettings.isGameStopped)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sens = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        if (sens <= 0f) sens = 1f;

        float mouseX = Input.GetAxis("Mouse X") * sens;
        float mouseY = Input.GetAxis("Mouse Y") * sens;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // NOTE: Do NOT overwrite originalpos here every frame.
        // originalpos will be captured when a shake starts so other camera motions (like bobbing)
        // are respected.
    }

    /// <summary>
    /// Start camera shake. Captures the camera's current local position as the baseline for the shake.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        // capture current transform.localPosition as baseline so we don't fight other movement
        originalpos = transform.localPosition;
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float strength = magnitude * (1.0f - (elapsed / duration));

            // Use 2D circle to avoid moving forward/back along Z.
            Vector2 offset2D = Random.insideUnitCircle * strength;
            transform.localPosition = originalpos + new Vector3(offset2D.x, offset2D.y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // restore to baseline
        transform.localPosition = originalpos;
        shakeCoroutine = null;
    }
}
