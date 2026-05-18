using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClamEnemy : Enemy
{
    private enum ClamState
    {
        IDLE,
        SHELL_CLOSED
    }
    
    private ClamState state = ClamState.IDLE;
    
    [Space]
    [SerializeField, Range(0, 1)] private float shellCloseChance;

    [Header("Spit Attack")] 
    [SerializeField] private GameObject spitProjectile;
    [SerializeField] private Transform spitOrigin;
    [SerializeField] private float spitVelocityCorrection = 1f;
    [SerializeField] private float spitGravityCorrection = 1f;

    private new void Awake()
    {
        base.Awake();
    }

    public override void attack(Vector2 impulse)
    {
        if (state == ClamState.SHELL_CLOSED)
        {
            // attacked while shell closed
        }
        else
        {
            // attacked while shell open
        }
        
        if (Random.value < shellCloseChance) state = ClamState.SHELL_CLOSED;
    }

    private void Spit()
    {
        GameObject projectile = Instantiate(spitProjectile, spitOrigin.position, Quaternion.Euler(Vector3.zero));
        BulletEnemy bullet = projectile.GetComponent<BulletEnemy>();
        
        float distToTarget = Vector2.Distance(spitOrigin.position, target.position);
        float timeToTarget = distToTarget / bullet.Speed;
        
        Vector3 targetPos = target.position
                + targetVelocity * timeToTarget * spitVelocityCorrection
                - (bullet.UsesGravity ? Physics.gravity * timeToTarget * timeToTarget * spitGravityCorrection / 2f : Vector3.zero);
        
        projectile.transform.forward = targetPos - spitOrigin.position;
    }
}
