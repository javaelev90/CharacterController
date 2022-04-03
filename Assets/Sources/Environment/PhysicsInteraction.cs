using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PhysicsInteraction : MonoBehaviour
{
    [Header("Object interaction")]
    [Range(0f, 1f)]
    [SerializeField] float staticFrictionCoefficient = 0.4f;
    [Range(0f, 1f)]
    [SerializeField] float kineticFrictionCoefficient = 0.3f;

    public Vector3 velocity { get; set; }

    void Start()
    {
        // Subscribe to custom oncollision event
        EventSystem.Subscribe(WorldEvent.OnCollisionEnter, HandleCollision);
    }

    void HandleCollision(object[] parameters)
    {
        RigidBodyWrapper rigidBodyWrapper = (RigidBodyWrapper)parameters[0];
        // TODO: implement collision handling
    }

    public Vector3 ApplyObjectInteractionForce(Vector3 normalForce, GameObject frictionObject)
    {
        // Moving object
        if (frictionObject.GetComponent<RigidBodyWrapper>())
        {
            RigidBodyWrapper rigidBodyWrapper = frictionObject.GetComponent<RigidBodyWrapper>();
            ApplyMovingObjectFriction(normalForce, rigidBodyWrapper);
        }
        // Static object
        else
        {
            ApplyStaticObjectFriction(normalForce);
        }
        return velocity;
    }

    void ApplyMovingObjectFriction(Vector3 normalForce, RigidBodyWrapper frictionObject)
    {
        Vector3 frictionObjectVelocity = frictionObject.body.velocity;
        if (velocity.magnitude - frictionObjectVelocity.magnitude < normalForce.magnitude * kineticFrictionCoefficient)
        {
            velocity -= velocity.normalized * (velocity.magnitude - frictionObjectVelocity.magnitude);
        }
        else
        {
            ApplyKineticFriction(normalForce);
        }
    }

    void ApplyStaticObjectFriction(Vector3 normalForce)
    {
        // If we are falling, no friction (this is not triggered very often, should it be kept?)
        if(Vector3.Dot(velocity, Vector3.down) > 0.85f)
        {
            return;
        }

        if (velocity.magnitude < normalForce.magnitude * staticFrictionCoefficient)
        {
            velocity = Vector3.zero;
        }
        ApplyKineticFriction(normalForce);
    }

    void ApplyKineticFriction(Vector3 normalForce)
    {
        velocity += -velocity.normalized * (normalForce.magnitude * kineticFrictionCoefficient);
    }
}
