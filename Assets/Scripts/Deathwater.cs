using UnityEngine;

public class Deathwater : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Health>().TakeDamage(9999); // Inflict massive damage to ensure death
        }
    }
}
