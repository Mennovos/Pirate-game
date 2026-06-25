using UnityEngine;

public interface IHookPoint
{
    public Vector3 GetPosition();

    public void OnHooked(PlayerGrappleV2 player, GrappleProjectile hook);
    public void WhilePulling(PlayerGrappleV2 player, GrappleProjectile hook);
    public void OnReached(PlayerGrappleV2 player, GrappleProjectile hook);
}
