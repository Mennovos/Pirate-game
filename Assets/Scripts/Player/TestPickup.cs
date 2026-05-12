using UnityEngine;

public class TestPickup : MonoBehaviour, IPickup
{
    public void onGrab(GameObject grabber)
    {
        Debug.Log("Pickup grabbed by: " + grabber.name);
    }
}
