using UnityEngine;

public class JumpingPlayerState : PlayerMovementState
{
    public override void Enter()
    {
        base.Enter();

        // Apply jump force
        player.m_PlayerVelocity.y = player.m_PlayerSettings.m_JumpForce;

        player.m_JumpQueued = false;
    }

    public override void Exit()
    {
        base.Exit();
        //animator.speed = 1.0f; // Reset animation speed
    }

    public override void UpdateState(float deltaTime)
    {

        // Apply air movement and gravity
        player.AirMove();

        // Transition to FallingPlayerState when the player starts descending
        if (player.m_PlayerVelocity.y < 0 || !player.m_JumpQueued)
        {
            SignalTransition("FallingPlayerState");
        }
    }
}
