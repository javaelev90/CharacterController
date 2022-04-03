using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/PlayerWalkState")]
public class PlayerWalkState : PlayerBaseState
{

    public override void Update()
    {
        base.Update();
        ApplyMovementForce();
    }

    public override void EvaluateTransition()
    {
        if (controller.currentInput.magnitude < minWalkingVelocity && controller.IsGrounded())
        {
            stateMachine.TransitionTo<PlayerBaseState>();
        }
        else if (!controller.IsGrounded())
        {
            stateMachine.TransitionTo<PlayerAirState>();
        }
    }

}
