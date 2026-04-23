using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IPickup pickup))
        {
            pickup.onGrab();
        }
    }
}
