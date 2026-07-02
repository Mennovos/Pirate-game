using UnityEngine;

public class LootScore : Loot
{
    [Space]
    [SerializeField] private float scoreAmount;
    
    private Utilities utilities;
    
    private bool isPickedUp = false;

    private void Awake()
    {
        utilities = FindAnyObjectByType<Utilities>();
    }

    public override void onGrab(GameObject grabber)
    {
        if (!isPickedUp)
        {
            utilities.AddScore(scoreAmount);
            
            isPickedUp = true; 
            
            Destroy(gameObject); // maybe add particles before this
        }
    }
}
