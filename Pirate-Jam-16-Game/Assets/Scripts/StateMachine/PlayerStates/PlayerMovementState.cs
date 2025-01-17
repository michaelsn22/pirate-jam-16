using UnityEngine;

public abstract class PlayerMovementState : State
{
    protected PlayerMovement player;
    protected Animator animator; // Unity's equivalent to AnimationPlayer

    private void Awake()
    {
        // Wait for the owner (PlayerActor) to be ready
        player = GetComponentInParent<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError("PlayerMovementState: Unable to find PlayerActor in parent!");
            return;
        }

        // Get Animator from PlayerActor
        //animator = player.GetComponent<Animator>();
        //if (animator == null)
        //{
        //    Debug.LogError("PlayerMovementState: Animator component not found on PlayerActor!");
        //}
    }

}
