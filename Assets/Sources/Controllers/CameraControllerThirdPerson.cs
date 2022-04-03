using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerThirdPerson : MonoBehaviour
{

    [Header("Mouse settings")]
    [Tooltip("The movement sensitivy of the mouse")]
    [SerializeField] float mouseSensitivity = 1f;
    [Tooltip("If the mouse movement should be inverted or not")]
    [SerializeField] bool invertedMovement = false;

    [Header("Camera settings")]
    [SerializeField] GameObject targetToLookAt;
    [SerializeField] LayerMask cameraObstacles;
    [Tooltip("The camera zooms in when obstructed with this speed")]
    [SerializeField] float zoomInSpeed = 1f;
    [Tooltip("The camera zooms out when not obstructed with this speed")]
    [SerializeField] float zoomOutSpeed = 1f;
    [Tooltip("Camera position offset from target to look at")]
    [SerializeField] Vector3 cameraOffset;
    [Tooltip("The x-axis angle of the camera")]
    [Range(-30, 30)][SerializeField] int cameraAngle;
    [Tooltip("The x-axis rotation limits")]
    [Range(0, 80)][SerializeField] int upRotationLimitX = 60;
    [Range(0, 80)][SerializeField] int downRotationLimitX = 60;

    SphereCollider sphereCollider;
    float originalDistance;
    float currentRotationY = 0f;
    float currentRotationX = 0f;
    Vector3 nextLerpPosition;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        transform.position += transform.TransformPoint(cameraOffset);
        nextLerpPosition = transform.position;
        originalDistance = cameraOffset.magnitude;
    }

    void Update()
    {
        AdjustPosition();
        GetNewCameraRotation();
    }

    void AdjustPosition()
    {
        // Since the camera offset is relative to our position it can be used for direction
        Vector3 direction = transform.TransformVector(cameraOffset).normalized;
        Physics.SphereCast(
                //targetToLookAt.GetComponent<Controller3D>().colliderPointUp,
                targetToLookAt.transform.position + (Vector3.up * 0.2f),
                sphereCollider.radius,
                direction,
                out RaycastHit hit,
                originalDistance,
                cameraObstacles
                );

        // If hit change position to lerp to depending on hit location
        if (hit.collider)
        {
            Vector3 newPosition = targetToLookAt.transform.position + (Vector3.up * 0.2f) + ((hit.distance - sphereCollider.radius) * direction);
            float remainingDistance = Vector3.Distance(targetToLookAt.transform.position + (Vector3.up * 0.2f), newPosition);
            nextLerpPosition = remainingDistance >= float.Epsilon ? newPosition : targetToLookAt.transform.position + (Vector3.up * 0.2f);
            
            transform.position = Vector3.Lerp(transform.position, nextLerpPosition, zoomInSpeed * Time.deltaTime);
        }
        // else set lerp position to start location
        else
        {
            nextLerpPosition = targetToLookAt.transform.position + (Vector3.up * 0.2f) + (direction * originalDistance);
            float distance = Vector3.Distance(transform.position, nextLerpPosition);
            float zoomSpeed = (distance / originalDistance) * zoomOutSpeed;
            transform.position = Vector3.Lerp(transform.position, nextLerpPosition, zoomSpeed * Time.deltaTime);
        }
    }

    void GetNewCameraRotation()
    {
        currentRotationY += (invertedMovement ? 1 : -1) * Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        currentRotationX += (invertedMovement ? -1 : 1) * Input.GetAxisRaw("Mouse Y") * mouseSensitivity
                * Time.deltaTime * ((downRotationLimitX + upRotationLimitX) * 4f);
        currentRotationX = Mathf.Clamp(currentRotationX, cameraAngle - downRotationLimitX, cameraAngle + upRotationLimitX);

        transform.localRotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
    }
}
