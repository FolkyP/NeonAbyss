using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float rotationX;
    float rotationY;
    float sens;

    [SerializeField] GameSettings gameSettings;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if(gameSettings.isGameStopped)
        {   Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return; 
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sens = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        if(sens <= 0f) sens = 1f;

        float mouseX = Input.GetAxis("Mouse X") * sens;
        float mouseY = Input.GetAxis("Mouse Y") * sens;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        orientation.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }
}
