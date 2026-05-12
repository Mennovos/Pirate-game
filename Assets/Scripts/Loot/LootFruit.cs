using UnityEngine;

public class LootFruit : MonoBehaviour, IPickup
{
    [SerializeField] private float healAmount;
    
    private bool isPickedUp = false;
    
    public void onGrab(GameObject grabber)
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
