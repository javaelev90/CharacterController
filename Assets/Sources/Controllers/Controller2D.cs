using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : MonoBehaviour
{
    [Header("Input modifiers")]
    [Tooltip("Movement acceleration of character")]
    [SerializeField] float acceleration = 1.0f;
    [Tooltip("Movement deceleration of character")]
    [SerializeField] float deceleration = 1.0f;
    [Tooltip("Max movement speed of character")]
    [SerializeField] float terminalVelocity = 1.0f;
    [Tooltip("Force that creates upwards movement")]
    [SerializeField] float jumpForce = 5.0f;

    [Header("Collider parameters")]
    [Tooltip("Colliding with these layers")]
    [SerializeField] LayerMask collisionMask;

    [Tooltip("Collider margin")]
    [SerializeField] float skinWidth = 0.025f;

    [Tooltip("The distance from the ground when the controller should think of the player as on the ground.")]
    [SerializeField] float groundCheckDistance = 0.1f;

    [Header("Other")]
    [Tooltip("Controller gravity")]
    [SerializeField] float gravity = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] float staticFrictionCoefficient = 0.4f;
    [Range(0f, 1f)]
    [SerializeField] float kineticFrictionCoefficient = 0.3f;

    BoxCollider2D boxCollider2D;
    Vector2 boxColliderSize;
    Vector3 velocity;
    float turnModifier = 3f;

    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        groundCheckDistance += skinWidth;
        boxColliderSize = new Vector2(boxCollider2D.size.x + skinWidth, boxCollider2D.size.y + skinWidth);
    }

    void Update()
    {
        ApplyGravityForce();
        ApplyJumpForce();
        ApplyMovementForce();

        ApplyAirResistanceForce();

        CollisionForceAdjustment();

        transform.position += velocity * Time.deltaTime;

    }

    void ApplyGravityForce()
    {
        Vector2 gravityMovement = Vector2.down * gravity * Time.deltaTime;
        velocity += (Vector3)gravityMovement;
    }

    void ApplyJumpForce()
    {
        bool spaceKeyDown = Input.GetKeyDown(KeyCode.Space);
        Vector2 jumpMovement = (Vector3)Vector2.up * jumpForce;

        if (spaceKeyDown && IsGrounded())
        {
            velocity += (Vector3)jumpMovement;
        }
    }

    void ApplyMovementForce()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector2 direction = new Vector2(horizontal, 0.0f);
        float distance = acceleration * Time.deltaTime;
        Vector2 inputMovement = direction * distance;

        if (direction.magnitude > float.Epsilon)
        {
            Accelerate(inputMovement);
        }

    }

    void Accelerate(Vector2 inputMovement)
    {
        float dotProduct = Vector3.Dot(velocity, inputMovement);
        if(dotProduct < float.Epsilon)
        {
            inputMovement *= turnModifier;
        }

        velocity += (Vector3)inputMovement;
        if (velocity.magnitude > terminalVelocity)
        {
            velocity = Vector3.ClampMagnitude(velocity, terminalVelocity);
        } 
    }

    void ApplyFrictionForce(Vector3 normalForce, GameObject frictionObject)
    {
        // Moving object
        if (frictionObject.GetComponent<Actor2D>())
        {
            Vector3 frictionObjectVelocity = frictionObject.GetComponent<Actor2D>().velocity;
            if (Mathf.Abs(velocity.magnitude - frictionObjectVelocity.magnitude) < normalForce.magnitude * staticFrictionCoefficient)
            {
                velocity -= velocity.normalized * (velocity.magnitude - frictionObjectVelocity.magnitude);
            }
            else
            {
                ApplyKineticFriction(normalForce);
            }
        } 
        // Static object
        else
        {
            if (velocity.magnitude < normalForce.magnitude * staticFrictionCoefficient)
            {
                velocity = Vector2.zero;
            }
            else
            {
                ApplyKineticFriction(normalForce);
            }
        }
    }

    void ApplyKineticFriction(Vector3 normalForce)
    {
        velocity += -velocity.normalized * (normalForce.magnitude * kineticFrictionCoefficient);
    }

    void ApplyAirResistanceForce()
    {
        float airResistanceCoefficient = 0.2f;
        velocity *= Mathf.Pow(airResistanceCoefficient, Time.deltaTime);
    }

    void CollisionForceAdjustment()
    {
        int counter = 0;
        RaycastHit2D hit, normalHit;
        Vector2 normalForce;
        do
        {

            hit = Physics2D.BoxCast(
                    transform.position,
                    boxColliderSize,
                    0.0f, // Angle
                    velocity.normalized,
                    (velocity.magnitude * Time.deltaTime) + skinWidth,
                    collisionMask);

            if (hit)
            {
                normalHit = Physics2D.BoxCast(
                        transform.position,
                        boxColliderSize,
                        0.0f, // Angle
                        -hit.normal,
                        (velocity.magnitude * Time.deltaTime) + skinWidth,
                        collisionMask);

                if (normalHit)
                {
                    normalForce = ControllerHelper.CalculateNormalForce(velocity, hit.normal);

                    velocity += -(Vector3)normalHit.normal * (normalHit.distance - skinWidth);
                    velocity += (Vector3)normalForce;

                    ApplyFrictionForce(normalForce, normalHit.collider.gameObject);
                }

            }
            Debug.DrawLine(transform.position, (transform.position + velocity), Color.blue);
            if (velocity.magnitude * Time.deltaTime < skinWidth)
            {
                break;
            }
            if (counter > 20)
            {
                break;
            }
            counter++;
        } while (hit);
                
    }

    bool IsGrounded()
    {
        return Physics2D.BoxCast(
            transform.position,
            boxColliderSize,
            0.0f, // Angle
            Vector2.down,
            groundCheckDistance,
            collisionMask);
    }


}
