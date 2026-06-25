using UnityEngine;

public class HookPointBlue : MonoBehaviour, IHookPoint
{
    [SerializeField] private float pullSpeed = 15f;
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void OnHooked(PlayerGrappleV2 player, GrappleProjectile hook)
    {
        
    }

    public void WhilePulling(PlayerGrappleV2 player, GrappleProjectile hook)
    {
        player.Rigidbody.linearVelocity = (transform.position - player.transform.position).normalized * pullSpeed;
    }

    public void OnReached(PlayerGrappleV2 player, GrappleProjectile hook)
    {
        
    }
}
