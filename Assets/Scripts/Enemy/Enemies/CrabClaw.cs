using UnityEngine;

public class CrabClaw : MonoBehaviour
{
    [SerializeField] private float Damage = 10f;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerHurtbox playerHurtbox))
        {
            playerHurtbox.TakeDamage(Damage);
        }   
    }
}
