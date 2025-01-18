using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    Rigidbody rb;
    private int attackDamage = 30;

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * 35f, ForceMode.Impulse);
        rb.AddForce(transform.up*6f, ForceMode.Impulse); 

        //start an invoke to hide the object after a duration.
        Invoke(nameof(DestroyObj), 4f);
    }

    private void DestroyObj()
    {
        Destroy(gameObject);
        //gameObject.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HealthScript>() != null && other.tag == "Player")
        {
            //Debug.Log("Projectile has hit a collider of name: "+other.gameObject.name);
            other.GetComponent<HealthScript>().TakeDamage(attackDamage);

            CancelInvoke("DestroyObj");
            Destroy(gameObject);
        }
    }
}
