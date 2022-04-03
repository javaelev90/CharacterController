using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/PlayerAirState")]
public class PlayerAirState : PlayerBaseState
{
    [Tooltip("Percentage of how much input should affect velocity while in the air")]
    [Range(0,1)][SerializeField] float airStrafeModifier = 0.5f;
    
    public override void Update()
    { 
        base.Update();
        controller.currentInput *= airStrafeModifier;
        ApplyMovementForce();
    }

    public override void EvaluateTransition()
    {
        if (controller.IsGrounded())
        {
            stateMachine.TransitionTo<PlayerBaseState>();
        }
    }
}
