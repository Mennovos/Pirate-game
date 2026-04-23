using UnityEngine;

public class Deathwater : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Health health))
        {
            health.TakeDamage(9999); // Inflict massive damage to ensure death
        }

        if (other.TryGetComponent(out IEnemy enemy))
        {
            Debug.Log("killed");
            enemy.kill();
        }
    }
}
