using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller3D : MonoBehaviour
{

    [Header("Jumping parameters")]
    [Tooltip("Force that creates upwards movement")]
    [SerializeField] float jumpForce = 5.0f;
    [Tooltip("The distance from the ground when the controller should think of the player as on the ground.")]
    [SerializeField] float groundCheckDistance = 0.02f;

    [Header("Collider parameters")]
    [Tooltip("Colliding with these layers")]
    [SerializeField] LayerMask collisionMask;
    [Tooltip("Collider margin")]
    [SerializeField] float skinWidth = 0.03f;

    [Header("Physics")]
    [SerializeField] float gravity = 9.82f;
    [Range(0f, 1f)]
    [SerializeField] float airResistanceCoefficient = 0.1f;

    [Header("States")]
    [SerializeField] List<State> states;

    [Header("Required objects")]
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] PhysicsInteraction physicsInteraction;
    [SerializeField] GameObject playerVisuals;
    [SerializeField] GameObject playerColliders;

    public Vector3 currentInput { get; set; }
    public Vector3 velocity { 
        get { return physicsInteraction.velocity; }
        set { physicsInteraction.velocity = value; }
    }
    public Vector3 colliderPointUp { get; private set; }
    public Vector3 colliderPointDown { get; private set; }

    StateMachine stateMachine { get; set; }

    RaycastHit[] collisionResults = new RaycastHit[20];

    void Awake()
    {
        physicsInteraction ??= GetComponent<PhysicsInteraction>();
        capsuleCollider ??= GetComponent<CapsuleCollider>();
        groundCheckDistance += skinWidth;
        UpdateColliderPosition();
        stateMachine = new StateMachine(this, states);
        capsuleCollider.radius *= 1.2f;
    }

    void Update()
    {
        UpdateColliderPosition();
        ApplyGravityForce();
        ApplyJumpForce();
        ApplyAirResistanceForce();
        stateMachine.Run();
        // Draw velocity direction and magnitude
        Debug.DrawLine(playerVisuals.transform.position, (playerVisuals.transform.position + velocity), Color.blue);
        playerVisuals.transform.position = Vector3.Lerp(playerVisuals.transform.position, playerColliders.transform.position, Time.deltaTime/Time.fixedDeltaTime);
        TurnGameObject();
    }

    void FixedUpdate()
    {
        CollisionForceAdjustment2();
        playerColliders.transform.position += velocity * Time.fixedDeltaTime;
        //playerVisuals.transform.position += velocity * Time.fixedDeltaTime;
    }

    void TurnGameObject()
    {
        Vector3 dir = new Vector3(velocity.x, 0f, velocity.z);
        // This will keep old rotation if the xz-velocity turns to zero, like when not moving
        if(dir.magnitude > float.Epsilon)
        {
            playerVisuals.transform.rotation = Quaternion.LookRotation(dir);
            playerColliders.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void UpdateColliderPosition()
    {
        colliderPointUp = playerColliders.transform.position + Vector3.up * ((capsuleCollider.height / 2 - (capsuleCollider.radius)));
        colliderPointDown = playerColliders.transform.position + Vector3.down * ((capsuleCollider.height / 2 - (capsuleCollider.radius)));
    }

    void ApplyGravityForce()
    {
        velocity += Vector3.down * gravity * Time.deltaTime;
    }

    void ApplyJumpForce()
    {
        bool spaceKeyDown = Input.GetKeyDown(KeyCode.Space);
        Vector3 jumpMovement = Vector3.up * jumpForce;

        if (spaceKeyDown && IsGrounded())
        {
            velocity += jumpMovement;
        }
    }

    void ApplyAirResistanceForce()
    {
        velocity *= Mathf.Pow(Mathf.Abs(1 - airResistanceCoefficient), Time.deltaTime);
    }

    void CollisionForceAdjustment()
    {
        UpdateColliderPosition();
        int counter = 0, resultCount = 0;
        Vector3 normalForce;

        do {
            resultCount = Physics.CapsuleCastNonAlloc(
                    colliderPointUp,
                    colliderPointDown,
                    capsuleCollider.radius + skinWidth,
                    velocity.normalized,
                    collisionResults,
                    Mathf.Infinity,
                    collisionMask);
            
            if(resultCount > 0)
            {
                RaycastHit hit = collisionResults[0];
                for (int i = 0; i < resultCount; i++)
                {
                    if(hit.distance > collisionResults[i].distance)
                    {
                        hit = collisionResults[i];
                    }
                }

                float distanceToColliderNeg = skinWidth / Vector3.Dot(velocity.normalized, hit.normal);
                float allowedMovementDistance = hit.distance + distanceToColliderNeg;

                // If the distance is closer than the velocity range
                if (allowedMovementDistance > velocity.magnitude * Time.fixedDeltaTime)
                {
                    break;
                }
                // If we are allowed to move
                if (allowedMovementDistance > 0.0f)
                {
                    transform.position += velocity.normalized * allowedMovementDistance;
                }

                /* If the hit is now within the movement distance the surface
                 * normalforce and other forces like friction should be applied */
                if (hit.distance <= ((velocity.magnitude * Time.fixedDeltaTime) + skinWidth))
                {
                    normalForce = ControllerHelper.CalculateNormalForce(velocity, hit.normal);
                    velocity += normalForce;
                    velocity = physicsInteraction.ApplyObjectInteractionForce(normalForce, hit.collider.gameObject);
                }

                AdjustForOverlap();
                UpdateColliderPosition();
            }
            
            if (counter > 20)
            {
                break;
            }
            counter++;

        } while (resultCount > 0);
        // Do this since we might miss collisions
        if (counter == 0)
        {
            AdjustForOverlap();
        }

    }

    void CollisionForceAdjustment2()
    {
        UpdateColliderPosition();
        int counter = 0;
        Vector3 normalForce;
        while (Physics.CapsuleCast(
                    colliderPointUp,
                    colliderPointDown,
                    capsuleCollider.radius + skinWidth,
                    velocity.normalized,
                    out RaycastHit hit,
                    Mathf.Infinity,
                    collisionMask))
        {

            float distanceToColliderNeg = skinWidth / Vector3.Dot(velocity.normalized, hit.normal);
            float allowedMovementDistance = hit.distance + distanceToColliderNeg;

            // If the distance is closer than the velocity range
            if (allowedMovementDistance > velocity.magnitude * Time.fixedDeltaTime)
            {
                break;
            }
            // If we are allowed to move
            if (allowedMovementDistance > 0.0f)
            {
                playerColliders.transform.position += velocity.normalized * allowedMovementDistance;
            }

            /* If the hit is now within the movement distance the surface
             * normalforce and other forces like friction should be applied */
            if (hit.distance <= ((velocity.magnitude * Time.fixedDeltaTime) + skinWidth))
            {
                normalForce = ControllerHelper.CalculateNormalForce(velocity, hit.normal);
                velocity += normalForce;
                velocity = physicsInteraction.ApplyObjectInteractionForce(normalForce, hit.collider.gameObject);
            }

            AdjustForOverlap();
            UpdateColliderPosition();
            if (counter > 20)
            {
                break;
            }
            counter++;

        }
        // Do this since we might miss collisions
        if(counter == 0)
        {
            AdjustForOverlap();
        }

    }

    void AdjustForOverlap()
    {
        Vector3 separationVector = Vector3.zero;
        int counter = 0;
        do
        {
            UpdateColliderPosition();
            Collider[] colliders = Physics.OverlapCapsule(
                colliderPointUp,
                colliderPointDown,
                capsuleCollider.radius,
                collisionMask);

            float minMagnitude = float.MaxValue;

            //Find shortest vector to move out of the overlap with
            foreach (Collider collider in colliders)
            {
                Vector3 direction = Vector3.zero;
                float distance = 0f;
                Physics.ComputePenetration(
                        capsuleCollider,
                        playerColliders.transform.position,
                        playerColliders.transform.rotation,
                        collider,
                        collider.transform.position,
                        collider.transform.rotation,
                        out direction,
                        out distance);

                Vector3 tempVector = direction * distance;
                if (tempVector.magnitude < minMagnitude)
                {
                    separationVector = tempVector;
                    minMagnitude = tempVector.magnitude;
                }
            }

            playerColliders.transform.position += separationVector + separationVector.normalized * skinWidth;
            velocity += ControllerHelper.CalculateNormalForce(velocity, separationVector.normalized);

            // To prevent infinite collisions loop
            if (counter > 20)
            {
                break;
            }
            counter++;

        } while (separationVector.magnitude > float.Epsilon);
    }

    public bool IsGrounded()
    {
        return GetGroundCollision().collider != null;
    }

    public RaycastHit GetGroundCollision()
    {
        Physics.CapsuleCast(
                    colliderPointUp,
                    colliderPointDown,
                    capsuleCollider.radius + skinWidth,
                    Vector3.down,
                    out RaycastHit hit,
                    groundCheckDistance,
                    collisionMask);
        if (hit.collider != null)
        {
            velocity += ControllerHelper.CalculateNormalForce(velocity, hit.normal);
        }

        return hit;
    }
}
