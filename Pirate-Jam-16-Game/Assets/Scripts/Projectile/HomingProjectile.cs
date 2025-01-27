using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public LayerMask defaultLayer;
    public enum MissileType
    {
        Red,
        Blue,
        Green,
        Yellow
    }

    public MissileType type;
    public float speed = 5f;
    public float homingStrength = 1f;

    private Transform player;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Set color based on type
        Renderer rend = GetComponent<Renderer>();
        switch (type)
        {
            case MissileType.Red:
                rend.material.color = Color.red;
                break;
            case MissileType.Blue:
                rend.material.color = Color.blue;
                break;
            case MissileType.Green:
                rend.material.color = Color.green;
                break;
            case MissileType.Yellow:
                rend.material.color = Color.yellow;
                speed *= 2; // Yellow missiles are faster
                break;
        }

        StartCoroutine(SelfDestruct());
    }
    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        //Vector3 direction = (player.position - transform.position).normalized;
        // Create a target position slightly above the player
        if (player != null)
        {
            Vector3 targetPosition = player.position + new Vector3(0, 0.1f, 0);

            // Calculate direction towards the elevated target position
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            rb.velocity = direction * speed;
            
            // Simple homing behavior
            rb.AddForce(direction * homingStrength);
        }
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            //Debug.Log("hit the ground. destroying self.");
            StopCoroutine("SelfDestruct");
            Destroy(gameObject);
            return;
        }
        //Debug.Log("hitting something");
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            //Debug.Log("hitting something on player layer");
            HealthScript player = collision.gameObject.GetComponent<HealthScript>();
            switch (type)
            {
                case MissileType.Red:
                    //Debug.Log("hit by red projectile!");
                    player.TakeDamage(30);
                    break;
                case MissileType.Blue:
                    break;
                case MissileType.Green:
                    break;
                case MissileType.Yellow:
                    //Debug.Log("hit by yellow projectile!");
                    player.TakeDamage(30);
                    break;
            }
            StopCoroutine("SelfDestruct");
            Destroy(gameObject);
        }

        
        //if we are the red missle, we can be redirected to the boss by the player.
        if (this.type == MissileType.Red)
        {
            if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
            {
                HealthScript boss = collision.gameObject.GetComponent<HealthScript>();
                if (boss != null)
                {
                    boss.TakeDamage(500);
                }
                StopCoroutine("SelfDestruct");
                Destroy(gameObject);
            }
        }
    }
}
