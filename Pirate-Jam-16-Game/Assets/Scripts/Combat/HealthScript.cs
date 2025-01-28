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
    public Image vignetteEffect;
    private float lerpDuration = 0.25f;
    private float maxAlpha = 48f / 255f; // 48 out of 255 in normalized form
    
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
        //Debug.Log(this.name+" is taking damage!");

        if (this.CompareTag("Player"))
        {
            RecievingDamage();
        }
		currentHealth -= damage;
        //DamagePopup.Create(healthBar.transform.position, damage);
        //play a hurt animation
        //do the check
	    if (currentHealth  <= 0)
        {
	        Die();
        }
    }

    public void RecievingDamage()
    {
        if (this.CompareTag("Player") && vignetteEffect != null)
        {
            //Debug.Log("playing vignette");
            StartCoroutine(LerpAlpha());
            return;
        }

        GlobalParticleSpawner.instance.PlayParticleAtLocation(objTransform, 2);
    }

    //vignette effect
    private IEnumerator LerpAlpha()
    {
        // Lerp from 0 to maxAlpha
        yield return StartCoroutine(LerpAlphaRoutine(0f, maxAlpha));

        // Lerp from maxAlpha back to 0
        yield return StartCoroutine(LerpAlphaRoutine(maxAlpha, 0f));
    }

    private IEnumerator LerpAlphaRoutine(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        Color currentColor = vignetteEffect.color;

        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / lerpDuration);
            
            currentColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
            vignetteEffect.color = currentColor;

            yield return null;
        }

        // Ensure we end exactly at the target alpha
        currentColor.a = endAlpha;
        vignetteEffect.color = currentColor;
    }

	public void Die()
    {
        //animation
        //disable the enemy
        //Debug.Log(this.name+" died");
        isThisTargetAlive = false;
        GlobalParticleSpawner.instance.PlayParticleAtLocation(objTransform, 1);

        if (this.CompareTag("Boss"))
        {
            //show text that the player beat the game or something?
            //maybe start a count down and exit to the menu too?
            Debug.Log("Boss has been defeated!");
        }
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
