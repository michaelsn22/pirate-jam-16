using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyMasterBoss : State
{
    public NavMeshAgent agent;
    private Transform player;
    private GameObject playerObj;
    public LayerMask whatIsPlayer;
    bool alreadyAttacked;
    public float sightRange, rangedAttackRange, attackRange; //attack range = 5 as of last testing.
    private float hitboxRange = 5f;
    public bool playerInSightRange, playerInAttackRange, playerInRangedAttackRange;
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
    private float strafeRotationSpeed = 6f;
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
    [SerializeField] private ParticleSystem gunfireParticle;
    //[SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private GameObject projectilePrefab1; //homing missle
    [SerializeField] private GameObject projectilePrefab2; //default projectile.
    [SerializeField] private AudioClip tireScreechNoise;
    [SerializeField] private AudioClip gunfireNoise;
    private AudioSource ourAudioSource;
    private bool dashAttacking = false; //flag for checking if we are mid dash attack/melee run at the player. this is so we dont try to ranged attack while dashing...
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
        playerInRangedAttackRange = Physics.CheckSphere(transform.position, rangedAttackRange, whatIsPlayer);
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

        if (playerInSightRange && !playerInRangedAttackRange && !playerInAttackRange)
        {
            //Debug.Log("player in range");
            ChaseEnemy();
            return;
        }

        if (playerInSightRange && playerInRangedAttackRange && !playerInAttackRange && !dashAttacking)
        {
            //Debug.Log("player in ranged attack range");
            RangedAttackEnemy();
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * strafeRotationSpeed);
        }
    }

    private void RangedAttackEnemy()
    {
        agent.velocity = Vector3.zero;

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

        if (ourTrans != null && !dashAttacking)
        {
            //Debug.Log("moving to the transform");
            if (agent.isActiveAndEnabled)
            {
                agent.SetDestination(ourTrans.position);
            }
            
            //maintain correct orientation
            // Calculate the direction from the NPC to the player
            Vector3 directionToEnemy = ourTrans.position - transform.position;
            directionToEnemy.y = 0f; // Optional: Ignore vertical difference

            // Calculate the desired rotation based on the direction to the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);

            // Smoothly rotate the NPC towards the player
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * strafeRotationSpeed);
        }

        // Check if the enemy is alive before attacking
        HealthScript enemyComponent = ourTrans.gameObject.GetComponent<HealthScript>();
        if (enemyComponent != null && enemyComponent.CheckIfThisTargetIsAlive())
        {
            //transform.LookAt(ourTrans.transform);

            // Check if within attack range
            if (!alreadyAttacked)
            {
                alreadyAttacked = true;

                int attackDecider = Random.Range(1, 10);
                if (attackDecider >= 5)
                {
                    masterGameObject.transform.LookAt(ourTrans.transform);
                    StartCoroutine(FireSeriesOfBullets());
                }
                else
                {
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

    private IEnumerator FireSeriesOfBullets()
    {
        yield return new WaitForSeconds(0.1f);
        FireBullet(0);

        yield return new WaitForSeconds(0.3f);
        FireBullet(0);

        yield return new WaitForSeconds(0.3f);
        FireBullet(0);

        yield return new WaitForSeconds(0.3f);
        FireBullet(0);

        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }

    private void FireBullet(int bulletType)
    {
        switch(bulletType)
        {
            case 0:
                GameObject tempProjectileGameObject = Instantiate(projectilePrefab1, transform.position + transform.forward * 1f + Vector3.up * 0.5f, Quaternion.identity);
                tempProjectileGameObject.transform.position += new Vector3(0,0.5f,0);
                tempProjectileGameObject.transform.LookAt(ourTrans);

                //play a particle
                gunfireParticle.Play();

                ourAudioSource.clip = gunfireNoise;
                ourAudioSource.Play();
                break;
            case 1:
                GameObject tempProjectileGameObject2 = Instantiate(projectilePrefab2, transform.position + transform.forward * 0.2f + Vector3.up * 0.5f, Quaternion.identity);
                tempProjectileGameObject2.transform.position += new Vector3(0,0.5f,0);
                tempProjectileGameObject2.transform.LookAt(ourTrans);

                //play a particle
                gunfireParticle.Play();

                ourAudioSource.clip = gunfireNoise;
                ourAudioSource.Play();
                break;
        }
    }

    private IEnumerator FireBarrageOfBullets()
    {
        yield return new WaitForSeconds(0.1f);
        
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        FireBullet(1);
        yield return new WaitForSeconds(0.1f);
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
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

        if (agent != null && agent.isActiveAndEnabled && !StopAllActions)
        {
            // Calculate the direction from the NPC to the player
            Vector3 directionToPlayer = ourTrans.position - transform.position;
            directionToPlayer.y = 0f; // Optional: Ignore vertical difference

            // Calculate the desired rotation based on the direction to the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            
            // Smoothly rotate the NPC towards the player
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * strafeRotationSpeed);
        }

        // Check if the enemy is alive before attacking
        HealthScript enemyComponent = ourTrans.gameObject.GetComponent<HealthScript>();
        if (enemyComponent != null && enemyComponent.CheckIfThisTargetIsAlive())
        {
            float distanceToEnemy = Vector3.Distance(transform.position, ourTrans.position);
            //transform.LookAt(enemy.transform);

            // Check if within attack range
            if (distanceToEnemy > attackRange+3f && !dodging && !dashAttacking) //HANDLE STRAFING TOWARD THE PLAYER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
                if (agent.isActiveAndEnabled)
                {
                    agent.ResetPath();
                }
                
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
                    int randomAttack = Random.Range(1, 5); // Random number between 1 and 4
                    switch(randomAttack)
                    {
                        case 1:
                            StartCoroutine("JumpingAttack"); //25% chance (50% for this move)
                            break;
                        case 2:
                            StartCoroutine("JumpingAttack"); //25% chance (50% for this move)
                            break;
                        default:
                            StartCoroutine(BurstForwardAttack()); //(50% for this move)
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

    private IEnumerator JumpingAttack()
    {
        //CancelInvoke("ResetAttack");
        isWaiting = false;

        //disable agent functionality
        if (agent != null)
        {
            agent.isStopped = true; //Stop the NavMeshAgent
            agent.velocity = Vector3.zero; //Reset the agent's velocity
            agent.enabled = false;
        }

        if (ourTrans != null)
        {
            masterGameObject.transform.LookAt(new Vector3(ourTrans.transform.position.x, masterGameObject.transform.position.y, ourTrans.transform.position.z));
        }


        //do the logic to jump the agent vertically
        dashParticle.Play();
        StartCoroutine(JumpIntoTheAir());

        ourAudioSource.clip = tireScreechNoise;
        ourAudioSource.Play();


        yield return new WaitForSeconds(1.6f);

        //start to come down to the ground.
        //Debug.Log("descending");

        // Descend to the ground
        yield return StartCoroutine(DescendToGround());

        yield return new WaitForSeconds(0.6f);

        //reenable agent functionality
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false; // Restart the NavMeshAgent
            agent.velocity = Vector3.zero; // Ensure the agent's velocity is reset
        }


        yield return new WaitForSeconds(0.5f);
        isWaiting = true;
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
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

        dashAttacking = true;

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

        dashAttacking = false;

        //Debug.Log("telling attack to reset in 5!");
        Invoke(nameof(ResetAttackAnimation), 1f);
        Invoke(nameof(ResetAttack), timeBetweenAttacks);
    }
    private IEnumerator BurstForward()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true; // Stop the NavMeshAgent
            agent.velocity = Vector3.zero; // Reset the agent's velocity
        }

        Vector3 initialPosition = masterGameObject.transform.position;
        Vector3 targetPosition = new Vector3(initialPosition.x + masterGameObject.transform.forward.x * chargeBurstDistance * 2, initialPosition.y, initialPosition.z + masterGameObject.transform.forward.z * chargeBurstDistance * 2);
        float elapsedTime = 0f;

        //play a particle.
        dashParticle.Play();

        ourAudioSource.clip = tireScreechNoise;
        ourAudioSource.Play();

        while (elapsedTime < chargeBurstDuration)
        {
            
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

                dashAttacking = false;

                yield break;
            }

            //Debug.Log("charging!!");

            // Lerp only the X and Z positions
            Vector3 newPosition = Vector3.Lerp(new Vector3(initialPosition.x, 0, initialPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z), elapsedTime / 3f);
            masterGameObject.transform.position = new Vector3(newPosition.x, initialPosition.y, newPosition.z);

            elapsedTime += Time.deltaTime;
            damageCalcTime += Time.deltaTime;

            if (damageCalcTime >= damageCalcInterval)
            {
                DamageCalcDelayed();
                // Debug.Log("rolling damage");
                damageCalcTime = 0f;
            }

            yield return null;
        }

        masterGameObject.transform.position = targetPosition;

        yield return new WaitForSeconds(0.1f); // Wait for a short period

        if (agent != null && agent.isActiveAndEnabled)
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

    public IEnumerator JumpIntoTheAir()
    {
        Vector3 startPosition = masterGameObject.transform.position;
        float jumpDuration = 1.5f;
        float jumpHeight = 6f;
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            // Calculate the percentage of time elapsed
            float t = elapsedTime / jumpDuration;

            // Custom easing function for initial burst and slowdown near top
            float easedT = CustomEaseOutBack(t);

            // Calculate the current height
            float height = jumpHeight * easedT;

            // Calculate the current position
            Vector3 currentPosition = startPosition + Vector3.up * height;

            // Move the enemy to the current position
            masterGameObject.transform.position = currentPosition;

            // Update the elapsed time
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //Ensure the enemy ends up exactly at the peak height
        masterGameObject.transform.position = startPosition + Vector3.up * jumpHeight;
    }

    private float CustomEaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    public IEnumerator DescendToGround()
    {
        Vector3 startPosition = masterGameObject.transform.position;
        NavMeshHit hit;
        Vector3 endPosition;

        // Find the nearest point on the NavMesh
        if (NavMesh.SamplePosition(startPosition, out hit, 100f, NavMesh.AllAreas))
        {
            //endPosition = hit.position;
            endPosition = hit.position; // Move the end position up by 0.3 units
        }
        else
        {
            //Debug.Log("uh oh");
            // If no NavMesh found, use the current x and z, but y = 0
            endPosition = new Vector3(startPosition.x, 0, startPosition.z);
        }

        float descendDuration = 0.3f; // Adjust this value to control how long the descent takes
        float elapsedTime = 0f;

        while (elapsedTime < descendDuration)
        {
            // Calculate the percentage of time elapsed
            float t = elapsedTime / descendDuration;

            // Use a smooth easing function for the descent
            float easedT = EaseInOutCubic(t);

            // Calculate the current position
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, easedT);

            // Move the boss to the current position
            masterGameObject.transform.position = currentPosition;

            // Update the elapsed time
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the boss ends up exactly at the end position
        masterGameObject.transform.position = endPosition;
        //Debug.Log("reached the bottom");

        //play an explosion particle?
        //explosionParticle.Play();
        SpawnAoeParticle();
    }

    public void SpawnAoeParticle()
    {
        GetComponent<ExpandingNova>().TriggerWaveAttack();
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5 ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
    }
}
