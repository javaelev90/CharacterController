using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Actor3D : MonoBehaviour
{

    [SerializeField] Transform targetPoint;
    [SerializeField] float speedModifier = 2f;
    [SerializeField] float terminalVelocity = 2f;
    Vector3 targetPosition = Vector3.zero;
    Vector3 originalPosition = Vector3.zero;
    Vector3 target = Vector3.zero;
    float totalTravelDistance;

    Rigidbody rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        originalPosition = transform.position;
        targetPosition = targetPoint.position;
        target = targetPosition;
        totalTravelDistance = Vector3.Distance(transform.TransformPoint(originalPosition), transform.TransformPoint(targetPosition));
    }

    void FixedUpdate()
    {
        // Draw velocity direction and magnitude
        Debug.DrawLine(transform.position, (transform.position + rigidBody.velocity), Color.blue);
        MoveTowardsTarget();
    }

    void MoveTowardsTarget()
    {
        if(Vector3.Distance(transform.position, originalPosition) <= 0.1f)
        {
            target = targetPosition;
        } 
        else if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            target = originalPosition;
        }
        // Change velocity direction when close to target
        float sign = (Vector3.Distance(transform.TransformPoint(transform.position), transform.TransformPoint(target)) / totalTravelDistance) < 0.12f ? -1 : 1;
        Vector3 direction = sign * (target - transform.position).normalized;

        rigidBody.velocity += (direction * Time.deltaTime * speedModifier);
        rigidBody.velocity = rigidBody.velocity.normalized * Mathf.Clamp(rigidBody.velocity.magnitude, 0f, terminalVelocity);
    }
}
