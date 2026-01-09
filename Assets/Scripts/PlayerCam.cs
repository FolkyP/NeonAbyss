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
    public Transform cameraPivot; //  The point around which the camera rotates (usually player head)
    public float cameraDistance = 0.3f; //  Distance of camera from pivot
    public LayerMask collisionMask; //  Layers the camera should not pass through
    public float cameraRadius = 0.15f; //  For SphereCast to prevent clipping

    float rotationX;
    float rotationY;
    float sens;

    [SerializeField] GameSettings gameSettings;

    private void Awake()
    {
        Instance = this;
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

        HandleCameraCollision();
    }

    private void HandleCameraCollision()
    {
        if (cameraPivot == null) return;

        Vector3 desiredPosition = cameraPivot.position - transform.forward * cameraDistance;
        Vector3 direction = desiredPosition - cameraPivot.position;

        if (Physics.SphereCast(cameraPivot.position, cameraRadius, direction.normalized, out RaycastHit hit, cameraDistance, collisionMask))
        {
            // Move camera closer to avoid clipping
            transform.position = hit.point + hit.normal * cameraRadius;
        }
        else
        {
            transform.position = desiredPosition;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        originalpos = transform.localPosition;
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    public void SetYawImmediate(float yaw)
    {
        // rotationY a rotationX jsou privátní pole v té tøídì — tato metoda v té tøídì má na nì pøístup
        rotationY = yaw;
        orientation.rotation = Quaternion.Euler(0f, yaw, 0f);
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
    public void SetYawSoft(float yaw)
    {
        rotationY = Mathf.LerpAngle(rotationY, yaw, Time.deltaTime * 10f);
        orientation.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float strength = magnitude * (1.0f - (elapsed / duration));
            Vector2 offset2D = Random.insideUnitCircle * strength;
            transform.localPosition = originalpos + new Vector3(offset2D.x, offset2D.y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalpos;
        shakeCoroutine = null;
    }
}
