using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
    Animator animator;
    public float maxHealth = 100;
	public float currentHealth;
    public bool isThisTargetAlive = true;

    public Image healthBar;
    public Image healthBarBg;
    private Transform objTransform;
    
    void Start()
    {
        // Get the object's transform component
        objTransform = GetComponent<Transform>();
        if (this.GetComponent<Animator>() != null)
        {
            animator = this.GetComponent<Animator>();
        }
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth/maxHealth;
        }
        if (currentHealth <= 0)
        {
            if (healthBar != null)
            {
                healthBarBg.enabled = false;
                healthBar.fillAmount = 0;
            }
        }
    }

	public void TakeDamage(int damage)
    {
        Debug.Log(this.name+" is taking damage!");
		currentHealth -= damage;
        //DamagePopup.Create(healthBar.transform.position, damage);
        //play a hurt animation
        //do the check
	    if (currentHealth  <= 0)
        {
	        Die();
        }
    }


	public void Die()
    {
        //animation
        //disable the enemy
        //Debug.Log(this.name+" died");
        isThisTargetAlive = false;
        Destroy(gameObject);
    }

    public bool CheckIfThisTargetIsAlive()
    {
        return isThisTargetAlive;
    }

    public void HealSelf(int valueToHeal)
    {
        if (healthBar != null)
        {
            currentHealth += valueToHeal;
            //DamagePopup.CreateHeal(healthBar.transform.position, valueToHeal);
        }
        /*
        else{
            if (currentHealth > 0)
            {
                currentHealth = 0;
                healthBar.fillAmount = 0;
            }
            
        }
        */
        //Debug.Log("we healed for "+valueToHeal);
    }

    public void IncreaseMaxHealth(int valueToAdd)
    {
        maxHealth += valueToAdd;
        //Debug.Log("Max health was increased!");
    }
}
