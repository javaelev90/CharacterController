using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [Tooltip("The camera will follow this objects movement")]
    [SerializeField] GameObject objectToTrack;

    [Tooltip("The speed that the camera will follow the target with")]
    [SerializeField] float smoothMovement = 4f;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(
                transform.position,
                new Vector3(
                        objectToTrack.transform.position.x,
                        objectToTrack.transform.position.y,
                        transform.position.z
                ),
                smoothMovement * Time.deltaTime
        );
    }
}
