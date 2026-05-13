using UnityEngine;

public class CrabClaw : MonoBehaviour
{
    private CrabEnemy crabEnemy;

    private void Awake()
    {
        crabEnemy = GetComponentInParent<CrabEnemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (crabEnemy.chargingAttack)
        {
            if (other.GetComponent<Health>() != null)
            {
                other.GetComponent<Health>().TakeDamage(10);
            }
        }
    }
}
