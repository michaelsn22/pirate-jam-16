using UnityEngine;

public class FallingPlayerState : PlayerMovementState
{
    public override void Enter()
    {
        base.Enter();

        // Pause animation (no direct equivalent in Unity, but you can achieve it by stopping or pausing)
        //animator.speed = 0.0f;
    }

    public override void Exit()
    {
        base.Exit();

        // Reset animation speed
        //animator.speed = 1.0f;
    }

    public override void Update()
    {

        // Transition to IdlePlayerState if on the ground and velocity is low
        if (player.m_PlayerVelocity.magnitude < 1f && player.m_Character.isGrounded)
        {
            //animator.Play("JumpEnd");
            SignalTransition("IdlePlayerState");
            return;
        }

        // Transition to WalkingPlayerState if on the ground and velocity is high
        if (player.m_Character.isGrounded && player.Speed > 0f)
        {
            SignalTransition("WalkingPlayerState");
            return;
        }

        // Apply air movement logic
        player.AirMove();
    }
}
