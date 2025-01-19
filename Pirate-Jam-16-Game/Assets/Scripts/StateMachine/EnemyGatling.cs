using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyGatling : State
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
    public float burstDuration = 1f; // Duration of the burst in seconds
    private float strafeRotationSpeed = 4f;
    private bool dashing = false;
    public float timeBetweenAttacks = 5f;
    private bool strafeDecisionCompleted = false;
    //end of old vars
    private bool CachedReferences = false;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject masterGameObject;
    [SerializeField] private Transform barrelTransform;

    public override void Enter()
    {
        base.Enter();
        //Debug.Log("Entering Idle State");
    }
    private void CacheOurReferences()
    {
        player = GameObject.Find("Player").transform;
        playerObj = GameObject.Find("Player");

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

        if (ourTrans != null)
        {
            //Debug.Log("moving to the transform");
            agent.SetDestination(ourTrans.position);
            //maintain correct orientation
            // Calculate the direction from the NPC to the player
            Vector3 directionToEnemy = ourTrans.position - transform.position;
            directionToEnemy.y = 0f; // Optional: Ignore vertical difference

            // Calculate the desired rotation based on the direction to the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);

            // Smoothly rotate the NPC towards the player
            masterGameObject.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * strafeRotationSpeed);
        }
    }

    private void AttackEnemy()
    {
        //Debug.Log("attacking enemy");
        /*
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

        if (agent != null && agent.isActiveAndEnabled && !StopAllActions)
        {
            // Calculate the direction from the NPC to the player
            Vector3 directionToPlayer = ourTrans.position - transform.position;
            directionToPlayer.y = 0f; // Optional: Ignore vertical difference

            // Calculate the desired rotation based on the direction to the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            
            // Smoothly rotate the NPC towards the player
            masterGameObject.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * strafeRotationSpeed);
        }

        // Check if the enemy is alive before attacking
        HealthScript enemyComponent = ourTrans.gameObject.GetComponent<HealthScript>();
        if (enemyComponent != null && enemyComponent.CheckIfThisTargetIsAlive())
        {
            float distanceToEnemy = Vector3.Distance(transform.position, ourTrans.position);
            //transform.LookAt(ourTrans.transform);

            // Check if within attack range
            if (distanceToEnemy > attackRange+8f && !dodging && !dashing) //HANDLE STRAFING TOWARD THE PLAYER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                agent.speed = 3.5f;
                agent.SetDestination(ourTrans.position);
        
                //Debug.Log("making a choice to strafe");
                //make this a choice to stfafe either directions
                //animator.SetBool("shouldWalk", false);
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

                    masterGameObject.transform.LookAt(ourTrans.transform);
                    StartCoroutine(FireBarrageOfBullets());
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

    private IEnumerator FireBarrageOfBullets()
    {
        yield return new WaitForSeconds(0.1f);
        
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        FireBullet();
        yield return new WaitForSeconds(0.1f);
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void FireBullet()
    {
        //spawn prefab aimed at player.
        GameObject tempArrowGameObject = Instantiate(arrowPrefab, barrelTransform.position, Quaternion.identity);
        //tempArrowGameObject.transform.position += new Vector3(0,2.5f,0);
        tempArrowGameObject.transform.LookAt(ourTrans);
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
    private IEnumerator BurstForward()
    {
        Transform playerTransform = playerObj.GetComponent<Transform>();
        // Debug.Log("doing burst movement!");
        if (agent != null)
        {
            agent.isStopped = true; // Stop the NavMeshAgent
            agent.velocity = Vector3.zero; // Reset the agent's velocity
        }
        Vector3 initialPosition = transform.position;
        
        // Ignore the Y component of the player's position
        Vector3 targetPosition = new Vector3(ourTrans.position.x, transform.position.y, ourTrans.position.z);
        float elapsedTime = 0f;
        while (elapsedTime < burstDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / burstDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
        // Optional: Reset the position after the burst
        yield return new WaitForSeconds(0.1f); // Wait for a short period
        if (agent != null)
        {
            agent.isStopped = false; // Restart the NavMeshAgent
            agent.velocity = Vector3.zero; // Ensure the agent's velocity is reset
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
