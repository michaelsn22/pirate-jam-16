using UnityEngine;

public class WalkingPlayerState : PlayerMovementState
{
    [SerializeField] private float topAnimationSpeed = 2.2f;

    public override void Enter()
    {
        base.Enter();

        //if (animator.GetCurrentAnimatorStateInfo(0).IsName("JumpEnd"))
        //{
        //    // Wait until the "JumpEnd" animation finishes before transitioning
        //    StartCoroutine(WaitForAnimationEndThenPlay("Walking"));
        //}
        //else
        //{
        //    PlayAnimation("Walking");
        //}
    }

    public override void Exit()
    {
        base.Exit();
        //animator.speed = 1.0f;
    }

    public override void Update()
    {

        //SetAnimationSpeed(player.Speed);

        // Transition to Idle state
        if (player.Speed < 1f && player.m_Character.isGrounded)
        {
            SignalTransition("IdlePlayerState");
        }

        // Transition to Jumping state
        if (player.m_JumpQueued && player.m_Character.isGrounded)
        {
            SignalTransition("JumpingPlayerState");
        }

        // Transition to Falling state
        if (!player.m_Character.isGrounded && player.m_PlayerVelocity.y <= 0)
        {
            SignalTransition("FallingPlayerState");
        }

        // Apply ground movement
        player.GroundMove();
        player.TriggerFriction();
    }

    //Using this section for animations later

    //private void SetAnimationSpeed(float speed)
    //{
    //    float alpha = Mathf.InverseLerp(0f, player.PlayerSettings.GroundSettings.MaxSpeed, speed);
    //    animator.speed = Mathf.Lerp(0f, topAnimationSpeed, alpha);
    //}

    //private void PlayAnimation(string animationName)
    //{
    //    if (animator != null)
    //    {
    //        animator.Play(animationName);
    //    }
    //}

    //private System.Collections.IEnumerator WaitForAnimationEndThenPlay(string nextAnimation)
    //{
    //    yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    //    PlayAnimation(nextAnimation);
    //}
}
