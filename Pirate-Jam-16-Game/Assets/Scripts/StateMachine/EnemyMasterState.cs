using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyMasterState : State
{
    public NavMeshAgent agent;
    private Transform player;
    private GameObject playerObj;
    public LayerMask whatIsPlayer;
    bool alreadyAttacked;
    public float sightRange, attackRange; //attack range = 5 as of last testing.
    private float hitboxRange = 5f;
    public bool playerInSightRange, playerInAttackRange, playerInRangedAttackRange;
    public bool isTakingDamage = false; //changed this from 'stopvariable' to 'isTakingDamage' for clarity.
    public Animator animator;
    private Transform ourTrans;
    private bool StopAllActions = false;
    private bool isWaiting = false;
    //testy
    private PlayerMovement cachedEnemyPlayerScript;
    private bool enemyAttacking = false;
    public bool dodging = false;
    private bool specialAttacking = false;
    private Collider[] hitEnemiesBuffer = new Collider[10]; // Adjust size as needed
    public bool isAttacking = false; //bool for other units to see that we are attacking.
    //movment burst vars
    private float strafeRotationSpeed = 4f;
    private bool dashing = false;
    public float timeBetweenAttacks = 5f;
    private bool strafeDecisionCompleted = false;
    private bool CachedReferences = false;
    //public float burstDistance = 0.5f; // Distance to move forward
    //public float burstDuration = 0.1f; // Duration of the burst in seconds

    public float chargeBurstDistance = 15f;
    public float chargeBurstDuration = 4f;
    private float damageCalcTime = 0f;
    private float damageCalcInterval = 0.25f;
    [SerializeField] private GameObject masterGameObject;
    [SerializeField] private ParticleSystem dashParticle;
    private AudioSource ourAudioSource;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 10f; //checking for collision with map elements so that we can return early.

    public override void Enter()
    {
        base.Enter();
        //Debug.Log("Entering Idle State");
    }
    private void CacheOurReferences()
    {
        player = GameObject.Find("Player").transform;
        playerObj = GameObject.Find("Player");
        ourAudioSource = GetComponent<AudioSource>();

        agent.radius = 0.5f;  // Reduce the agent's radius
        agent.stoppingDistance = 1f;  // Increase stopping distance
        agent.avoidancePriority = 20; //set so that we (this agent) cannot push the companion game objects with a higher priority.
    }
    public override void UpdateState(float deltaTime)
    {
        //Debug.Log("In Idle State");
        if (!CachedReferences)
        {
            CachedReferences = true;
            CacheOurReferences();
        }

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        /*
        if (!strafeDecisionCompleted && animator != null)
        {
            animator.SetBool("shouldStrafe", false);
            animator.SetBool("shouldStrafe2", false);
        }
        */

        if (!playerInSightRange)
        {
            ContinueToIdle();
            return;
        }

        if (playerInSightRange && !playerInAttackRange)
        {
            //Debug.Log("player in range");
            ChaseEnemy();
            return;
        }

        if (playerInSightRange && playerInAttackRange)
        {
            //Debug.Log("player in attack range");
            AttackEnemy();
            return;
        }
    }
    
    private void ContinueToIdle()
    {
        //Debug.Log("Idling");
        agent.velocity = Vector3.zero;
        //animator.Play("Idle");
    }

    private void ChaseEnemy()
    {
        //Debug.Log("chasing enemy");
        agent.speed = 5.0f;

        if (alreadyAttacked)
        {
            return;
        }
        
        /*
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
        {
            animator.SetBool("shouldWalk", true);
            animator.SetBool("shouldStrafe", false);
            animator.SetBool("shouldStrafe2", false);
            animator.SetBool("shouldStrafeLeft", false);
            animator.SetBool("shouldStrafeRight", false);
        }
        */

        

        //Get position of a potential enemy.
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);

        if (hitEnemies.Length > 0)
        {
            Transform closestTransform = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider col in hitEnemies)
            {
                float distanceToCollider = Vector3.Distance(transform.position, col.transform.position);
                if (distanceToCollider < closestDistance)
                {
                    closestDistance = distanceToCollider;
                    closestTransform = col.transform;
                }
            }

            if (closestTransform != null)
            {
                ourTrans = closestTransform;
            }
        }

        if (ourTrans != null && !dashing)
        {
            //Debug.Log("moving to the transform");
            agent.SetDestination(ourTrans.position);
            //maintain correct orientation
            // Calculate the direction from the NPC to the player
            Vector3 directionToEnemy = ourTrans.position - masterGameObject.transform.position;
            directionToEnemy.y = 0f; // Optional: Ignore vertical difference

            // Calculate the desired rotation based on the direction to the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);

            // Smoothly rotate the NPC towards the player
            masterGameObject.transform.rotation = Quaternion.Slerp(masterGameObject.transform.rotation, targetRotation, Time.deltaTime * strafeRotationSpeed);
        }
    }

    private void AttackEnemy()
    {
        /*
        //Debug.Log("choosing to attack an enemy");
        if (animator != null)
        {
            //Debug.Log("is this whats doing it?");
            animator.SetBool("shouldWalk", false);
            animator.SetBool("shouldRun", false);
            animator.SetBool("rangedAttack", false);
            animator.SetBool("rangedAttack2", false);
        }
        */

        //Debug.Log("attacking the player!");
        // Get position of a potential player or helper NPC.
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer); //(center, radius, layermask)
        if (hitEnemies.Length > 0)
        {
            Transform closestTransform = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider col in hitEnemies)
            {
                float distanceToCollider = Vector3.Distance(transform.position, col.transform.position);
                if (distanceToCollider < closestDistance)
                {
                    closestDistance = distanceToCollider;
                    closestTransform = col.transform;
                }
            }

            if (closestTransform != null)
            {
                ourTrans = closestTransform;
            }
        }

        if (agent != null && agent.isActiveAndEnabled && !StopAllActions && !dashing)
        {
            // Calculate the direction from the NPC to the player
            Vector3 directionToPlayer = ourTrans.position - transform.position;
            directionToPlayer.y = 0f; // Optional: Ignore vertical difference

            // Calculate the desired rotation based on the direction to the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            
            // Smoothly rotate the NPC towards the player
            masterGameObject.transform.rotation = Quaternion.Slerp(masterGameObject.transform.rotation, targetRotation, Time.deltaTime * strafeRotationSpeed);
        }

        // Check if the enemy is alive before attacking
        HealthScript enemyComponent = ourTrans.gameObject.GetComponent<HealthScript>();
        if (enemyComponent != null && enemyComponent.CheckIfThisTargetIsAlive())
        {
            float distanceToEnemy = Vector3.Distance(transform.position, ourTrans.position);
            //transform.LookAt(enemy.transform);

            // Check if within attack range
            if (distanceToEnemy > attackRange+3f && !dodging && !dashing) //HANDLE STRAFING TOWARD THE PLAYER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                agent.speed = 3.5f;
                agent.SetDestination(ourTrans.position);
        
                //Debug.Log("making a choice to strafe");
                //make this a choice to stfafe either directions
                /*
                animator.SetBool("shouldWalk", false);
                if (!strafeDecisionCompleted)
                {
                    isWaiting = false;
                    strafeDecisionCompleted = true;
                    float temp = Random.Range(1,10);
                    if (temp > 5)
                    {
                        animator.SetBool("shouldStrafe", true);
                    }
                    else
                    {
                        animator.SetBool("shouldStrafe2", true);
                    }
                    Invoke(nameof(ResetStrafeDecision), 2f);
                }
                */
            }
            else 
            {
                // Stop moving towards the enemy and attack
                agent.ResetPath();
                /*
                animator.SetBool("shouldWalk", false);
                animator.SetBool("shouldRun", false);
                animator.SetBool("shouldStrafe", false);
                animator.SetBool("shouldStrafe2", false);
                */

                //cache method
                //CheckEnemyAttacking2();
                /*
                if (enemyAttacking)
                {
                    //Debug.Log("enemy is attacking");
                }
                */

                if (!alreadyAttacked)
                {
                    alreadyAttacked = true;
                    //animator.SetBool("Attack1", true);
                    Invoke(nameof(DamageCalcDelayed), 0.5f);
                    //decide if we want to do a combo or not.
                    // Roll a random number to decide whether to combo or not
                    int randomAttack = Random.Range(1, 8); // Random number between 1 and 3
                    switch(randomAttack)
                    {
                        case 1:
                            //Debug.Log("bursting forward");
                            StartCoroutine(BurstForwardAttack());
                            break;
                        default:
                            //Debug.Log("bursting forward");
                            StartCoroutine(BurstForwardAttack());
                            //Invoke(nameof(ResetAttackAnimation), 1f);
                            //Invoke(nameof(ResetAttack), timeBetweenAttacks);
                            //StartCoroutine("ComboContinuer");
                            break;
                    }
                }
            }
        }
        else
        {
            // Enemy is not alive
            Debug.Log("ENEMY DIED");
            isWaiting = false;
        }
    }

    private void DamageCalcDelayed()
    {
        if (dodging) return; // Early exit if dodging

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, hitboxRange, hitEnemiesBuffer, whatIsPlayer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider enemyCollider = hitEnemiesBuffer[i];
            HealthScript enemy = enemyCollider.GetComponent<HealthScript>();
            if (enemy != null)
            {
                enemy.TakeDamage(10);
            }
        }
    }
    private void DamageCalcDelayedUnblockable()
    {
        if (dodging) return; // Early exit if dodging
        
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, hitboxRange, hitEnemiesBuffer, whatIsPlayer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider enemyCollider = hitEnemiesBuffer[i];
            HealthScript enemy = enemyCollider.GetComponent<HealthScript>();
            if (enemy != null)
            {
                //zzz
            }
        }
    }
    private void ResetAttack()
    {
        //Debug.Log("resetting attack!");
        alreadyAttacked = false;
        //animator.SetBool("Attack1", false);
        isWaiting = false;
        specialAttacking = false;
        isAttacking = false;
    }
    private void CheckEnemyAttacking()
    {
        CacheComponents();
        if (cachedEnemyPlayerScript != null)
        {
            //enemyAttacking = cachedEnemyPlayerScript.isAttacking;
        }
        else
        {
            enemyAttacking = false;
        }
    }

    private void ResetStrafeDecision()
    {
        strafeDecisionCompleted = false;
    }

    private void CheckEnemyAttacking2()
    {
        CacheComponents();
        //enemyAttacking = (cachedEnemyPlayerScript != null && cachedEnemyPlayerScript.isAttacking);
    }
    private void CacheComponents()
    {
        if (ourTrans != null)
        {
            cachedEnemyPlayerScript = ourTrans.gameObject.GetComponent<PlayerMovement>();
        }
    }

    private void ResetAttackAnimation()
    {
        //animator.SetBool("Attack1", false);
    }
    
    private void GetLocationOfNearestEnemy()
    {
        // Get position of a potential player or helper NPC.
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer); //(center, radius, layermask)
        if (hitEnemies.Length > 0)
        {
            Transform closestTransform = null;
            float closestDistance = Mathf.Infinity;
            foreach (Collider col in hitEnemies)
            {
                float distanceToCollider = Vector3.Distance(transform.position, col.transform.position);
                if (distanceToCollider < closestDistance)
                {
                    closestDistance = distanceToCollider;
                    closestTransform = col.transform;
                }
            }
            if (closestTransform != null)
            {
                ourTrans = closestTransform;
            }
        }
    }

    private IEnumerator BurstForwardAttack()
    {
        yield return new WaitForSeconds(0.1f);

        //Look at them, burst forward, look at them, burst forward.
        if (ourTrans != null)
        {
            masterGameObject.transform.LookAt(new Vector3(ourTrans.transform.position.x, masterGameObject.transform.position.y, ourTrans.transform.position.z));
        }
        
        StartCoroutine(BurstForward());

        yield return new WaitForSeconds(2.5f);

        //Debug.Log("doing next burst!");
        if (ourTrans != null)
        {
            masterGameObject.transform.LookAt(new Vector3(ourTrans.transform.position.x, masterGameObject.transform.position.y, ourTrans.transform.position.z));
        }
        StartCoroutine(BurstForward());

        yield return new WaitForSeconds(2.5f);

        //Debug.Log("doing 3rd burst!");
        if (ourTrans != null)
        {
            masterGameObject.transform.LookAt(new Vector3(ourTrans.transform.position.x, masterGameObject.transform.position.y, ourTrans.transform.position.z));
        }
        StartCoroutine(BurstForward());

        yield return new WaitForSeconds(1f);

        //Debug.Log("telling attack to reset in 5!");
        Invoke(nameof(ResetAttackAnimation), 1f);
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }
    private IEnumerator BurstForward()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        Vector3 initialPosition = masterGameObject.transform.position;
        Vector3 targetPosition = initialPosition + masterGameObject.transform.forward * chargeBurstDistance * 2;
        float elapsedTime = 0f;

        dashParticle.Play();
        ourAudioSource.Play();

        while (elapsedTime < chargeBurstDuration)
        {
            dashing = true;
            /*
            if (!agent.isOnNavMesh)
            {
                Debug.Log("Agent is not on NavMesh. Stopping dash.");
                StopAllCoroutines();
                CancelInvoke("ResetAttack");
                Invoke(nameof(ResetAttack), timeBetweenAttacks);

                // Re-enable agent
                if (agent != null && agent.isActiveAndEnabled)
                {
                    agent.isStopped = false;
                    agent.velocity = Vector3.zero;
                }

                yield break;
            }
            */

            // Check for ground collision
            if (Physics.Raycast(masterGameObject.transform.position, masterGameObject.transform.forward, out RaycastHit hit, raycastDistance, groundLayer))
            {
                //Debug.Log($"About to hit ground at distance: {hit.distance}. Stopping immediately.");
                StopAllCoroutines();
                CancelInvoke("ResetAttack");
                Invoke(nameof(ResetAttack), timeBetweenAttacks);

                // Immediately stop movement
                masterGameObject.transform.position = hit.point - masterGameObject.transform.forward * 0.1f; // Stop slightly before the hit point

                // Re-enable agent
                if (agent != null && agent.isActiveAndEnabled)
                {
                    agent.isStopped = false;
                    agent.velocity = Vector3.zero;
                }

                dashing = false;

                yield break;
            }

            // Movement logic
            Vector3 newPosition = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / chargeBurstDuration);
            masterGameObject.transform.position = new Vector3(newPosition.x, initialPosition.y, newPosition.z);

            elapsedTime += Time.deltaTime;
            damageCalcTime += Time.deltaTime;

            if (damageCalcTime >= damageCalcInterval)
            {
                DamageCalcDelayed();
                damageCalcTime = 0f;
            }

            yield return null;
        }
        dashing = false;

        masterGameObject.transform.position = targetPosition;

        yield return new WaitForSeconds(0.1f);

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            agent.velocity = Vector3.zero;
        }
    }

    private void StopAnimations()
    {
        isWaiting = false;
        isAttacking = false;
        CancelInvoke("DamageCalcDelayed");
        CancelInvoke("ResetAttack");
        /*
        animator.SetBool("Attack1", false);
        animator.SetBool("shouldWalk", false);
        animator.SetBool("shouldRun", false);
        animator.SetBool("shouldStrafe", false);
        animator.SetBool("shouldStrafe2", false);
        StopCoroutine("ComboContinuer");
        StopCoroutine("SpecialAttackHandler");
        StopCoroutine("JumpAttackHandler");
        StopCoroutine("JumpAttackHandler");
        //added this one in just incase... to stop ranged attacks.
        //StopCoroutine("RangedAttack1");
        animator.SetBool("JumpAttack", false);
        animator.SetBool("Attack1", false);
        animator.SetBool("Attack2", false);
        animator.SetBool("Attack3", false);
        animator.SetBool("Attack4", false);
        animator.SetBool("Attack5", false);
        animator.SetBool("Attack6", false);
        animator.SetBool("special1", false);
        animator.SetBool("special2", false);
        animator.SetBool("special3", false);
        animator.SetBool("special4", false);
        animator.SetBool("rangedAttack", false);
        animator.SetBool("rangedAttack2", false);
        animator.SetBool("dashAttack", false);
        animator.SetBool("shouldStrafeLeft", false); 
        animator.SetBool("shouldStrafeRight", false);
        animator.SetBool("shouldStrafe", false);
        animator.SetBool("shouldStrafe2", false);
        animator.SetBool("shouldWalk", false);
        animator.SetBool("shouldRun", false);
        */
    }
    public void PrepDeath()
    {
        StopAnimations();
        CancelInvoke();
        StopAllCoroutines();
    }

    public override void Exit()
    {
        base.Exit();
       //Debug.Log("Exiting Idle State");
    }
}
