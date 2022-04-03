using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerFirstPerson : MonoBehaviour
{
    [Header("Mouse settings")]
    [Tooltip("The movement sensitivy of the mouse")]
    [SerializeField] float mouseSensitivity = 1f;
    [Tooltip("If the mouse movement should be inverted or not")]
    [SerializeField] bool invertedMovement = false;

    [Header("Camera settings")]
    [Tooltip("Camera view limit right/left in degrees")]
    [Range(60, 120)][SerializeField] float viewLimitX = 90f;
    [Tooltip("Camera view limit up/down in degrees")]
    [Range(60, 120)][SerializeField] float viewLimitY = 60f;

    [SerializeField] GameObject targetToLookAt;

    float currentRotationX = 0f;
    float currentRotationY = 0f;

    void Update()
    {
        GetNewCameraRotation();
        RestrictCameraRotation();
        SetCameraRotation();
    }

    void LateUpdate()
    {
        UpdateTargetRotation();
    }

    void SetCameraRotation()
    {
        transform.localRotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
    }

    void UpdateTargetRotation()
    {
        float cameraRotationY = transform.rotation.y;
        targetToLookAt.transform.rotation = Quaternion.Euler(0f, cameraRotationY, 0f);
    }

    void GetNewCameraRotation()
    {
        currentRotationY -= (invertedMovement ? 1 : -1) * Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        currentRotationX += (invertedMovement ? 1 : -1) * Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
    }

    void RestrictCameraRotation()
    {
        currentRotationX = Mathf.Clamp(currentRotationX, currentRotationX +(-viewLimitY / 2), currentRotationX + (viewLimitY / 2)); 
        currentRotationY = Mathf.Clamp(currentRotationY, currentRotationY + (-viewLimitY / 2), currentRotationY + (viewLimitY / 2));
    }
}
