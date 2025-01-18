using UnityEngine;

public class IdlePlayerState : PlayerMovementState
{
    public override void Enter()
    {
        base.Enter();

        //if (animator.GetCurrentAnimatorStateInfo(0).IsName("JumpEnd"))
        //{
        //    // Wait for the JumpEnd animation to finish before pausing
        //    StartCoroutine(WaitForAnimationEndAndPause());
        //}
        //else
        //{
        //    PauseAnimation();
        //}
    }

    public override void UpdateState(float deltaTime)
    {

        // Transition to WalkingPlayerState if there's movement input and the player is on the ground
        if (player.m_MoveInput.magnitude > 0.0f && player.m_Character.isGrounded)
        {
            SignalTransition("WalkingPlayerState");
            return;
        }

        // Transition to JumpingPlayerState if the jump is queued and the player is on the ground
        if (player.m_JumpQueued && player.m_Character.isGrounded)
        {
            SignalTransition("JumpingPlayerState");
            return;
        }

        // Transition to FallingPlayerState if the player is in the air and velocity.y is below a threshold
        if (!player.m_Character.isGrounded && player.m_PlayerVelocity.y < 0.3f)
        {
            SignalTransition("FallingPlayerState");
            return;
        }

        player.TriggerFriction();
    }

    //    private void PauseAnimation()
    //    {
    //        animator.speed = 0.0f;
    //    }

    //    private System.Collections.IEnumerator WaitForAnimationEndAndPause()
    //    {
    //        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    //        PauseAnimation();
    //    }
}
