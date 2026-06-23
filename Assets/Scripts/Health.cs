using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private HealthBar healthBar;
    
    [Space] 
    [SerializeField] private List<Renderer> meshRenderers;
    [SerializeField] private Color colorOnHit = new(1f, 0.5f, 0.5f);
    [SerializeField] private float colorOnHitDuration = 0.1f; // also invincibility duration
    
    [Space]
    [SerializeField] private Image onHitVignette;

    private Coroutine colorResetCoroutine;

    [Header("Deathscreen")]
    [SerializeField] private GameObject deathScreen;

    private void Start()
    {
        if (onHitVignette)
        {
            onHitVignette.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        healthBar.setHealth(currentHealth / maxHealth);
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (currentHealth < 0)
        {
            Die();
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (damage > 0)
        {
            if (colorResetCoroutine != null) return;

            foreach (Renderer meshRenderer in meshRenderers)
            {
                foreach (Material mat in meshRenderer.materials)
                {
                    mat.color = colorOnHit;
                }
            }

            if (onHitVignette)
            {
                onHitVignette.gameObject.SetActive(true);
            }
        
            colorResetCoroutine = StartCoroutine(ColorResetCoroutine());
        }
        
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        deathScreen.SetActive(true);
        Destroy(gameObject);
        Debug.Log("Entity has died.");
    }
    
    private IEnumerator ColorResetCoroutine()
    {
        yield return new WaitForSeconds(colorOnHitDuration);

        foreach (Renderer meshRenderer in meshRenderers)
        {
            foreach (Material mat in meshRenderer.materials)
            {
                mat.color = Color.white;
            }
        }
        
        if (onHitVignette)
        {
            onHitVignette.gameObject.SetActive(false);
        }
        
        colorResetCoroutine = null;
    }
}
