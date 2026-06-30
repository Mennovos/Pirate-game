using UnityEngine;

public class LootFruit : Loot
{
    [Space]
    [SerializeField] private float healAmount;
    [SerializeField] private GameObject healParticles;
    
    private bool isPickedUp = false;
    
    public override void onGrab(GameObject grabber)
    {
        if (!isPickedUp)
        {
            if (grabber.TryGetComponent(out Health health))
            {
                health.TakeDamage(-healAmount);
                
                Instantiate(healParticles, grabber.transform.position, grabber.transform.rotation);
            }
            
            isPickedUp = true; 
            
            Destroy(gameObject); // maybe add particles before this
        }
    }
}
