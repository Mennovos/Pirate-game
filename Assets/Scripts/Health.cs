using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private Image healthBar;

    [Header("Deathscreen")]
    [SerializeField] private GameObject deathScreen;

    void Update()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
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
        deathScreen.SetActive(true);
        Destroy(gameObject);
        Debug.Log("Entity has died.");
    }
}
