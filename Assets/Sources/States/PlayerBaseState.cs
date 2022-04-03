using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/PlayerBaseState")]
public class PlayerBaseState : State
{
    [Header("Input modifiers")]
    [Tooltip("Movement acceleration of character")]
    [SerializeField] float acceleration = 1.0f;
    [Tooltip("Max movement speed of character")]
    [SerializeField] float terminalVelocity = 1.0f;
    [Tooltip("Threshold velocity for when the player is no longer walking")]
    [SerializeField] protected float minWalkingVelocity = 0.1f;
    [Tooltip("Velocity boost modifier when player starts running in the oposite direction")]
    [SerializeField] float turnSpeedModifier = 3f;
    [Tooltip("Angle difference when the turn speed modifier should activate, -1 opposite direction, 1 same direction, 0 perpendicular direction")]
    [Range(-1, 1)][SerializeField] float turnSpeedActivationAngle = 0.6f;


    protected StateMachine stateMachine { get; set; }
    protected Controller3D controller { get; private set; }

    public override void Initialize(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.controller = (Controller3D)stateMachine.owner;
    }

    public override void Update()
    {
        controller.currentInput = Vector3.right * Input.GetAxisRaw("Horizontal") +
                Vector3.forward * Input.GetAxisRaw("Vertical");

        // The y-angle is the only one that should affect direction
        Quaternion cameraAngle = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);
        controller.currentInput = cameraAngle * controller.currentInput;
    }

    public override void EvaluateTransition()
    {
        if (controller.currentInput.magnitude >= minWalkingVelocity)
        {
            stateMachine.TransitionTo<PlayerWalkState>();
        }
    }

    public void ApplyMovementForce()
    {
        if (controller.currentInput.magnitude > 1f)
        {
            controller.currentInput = controller.currentInput.normalized;
        }

        if (controller.currentInput.magnitude > float.Epsilon)
        {
            Accelerate(controller.currentInput);
        }
    }

    void Accelerate(Vector3 inputMovement)
    {
        //Counteract camera rotation i.e. camera rotation should not affect velocity
        inputMovement = Vector3.ProjectOnPlane(inputMovement, controller.GetGroundCollision().normal);

        float dotProduct = Vector3.Dot(controller.velocity.normalized, inputMovement);
        if (dotProduct < turnSpeedActivationAngle)
        {
            inputMovement *= turnSpeedModifier;
        }

        controller.velocity += inputMovement * acceleration * Time.deltaTime;
        if (controller.velocity.magnitude > terminalVelocity)
        {
            controller.velocity = Vector3.ClampMagnitude(controller.velocity, terminalVelocity);
        }
    }
}
