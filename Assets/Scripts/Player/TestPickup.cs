using UnityEngine;

public class TestPickup : MonoBehaviour, IPickup
{
    public void onGrab()
    {
        Debug.Log("Pickup grabbed");
    }
}
