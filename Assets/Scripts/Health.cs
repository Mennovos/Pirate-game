using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private HealthBar healthBar;

    [Header("Deathscreen")]
    [SerializeField] private GameObject deathScreen;

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
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        if (deathScreen)
        {
            deathScreen.SetActive(true);
            Destroy(gameObject);
        }
    }
}
