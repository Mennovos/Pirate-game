using UnityEngine;

public class LootFruit : Loot
{
    [Space]
    [SerializeField] private float healAmount;
    
    private bool isPickedUp = false;
    
    public override void onGrab(GameObject grabber)
    {
        if (!isPickedUp)
        {
            if (grabber.TryGetComponent(out Health health))
            {
                health.TakeDamage(-healAmount);
            }
            
            isPickedUp = true; 
            
            Destroy(gameObject); // maybe add particles before this
        }
    }
}
