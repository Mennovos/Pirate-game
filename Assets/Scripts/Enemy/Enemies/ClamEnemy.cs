using System;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ClamEnemy : Enemy
{
    private enum ClamState
    {
        SPAWNING,
        IDLE,
        SHELL_CLOSED,
        SPITTING
    }
    
    private ClamState state = ClamState.SPAWNING;
    
    [Space]
    [SerializeField, Range(0f, 1f)] private float shellCloseChance;
    [SerializeField, Range(0f, 1f)] private float shellOpenChance;
    [SerializeField, Min(0f)] private float shellOpenCheckInterval;
    [SerializeField, Min(0f)] private float shellOpenMinTargetDistance;
    [SerializeField, Min(float.Epsilon)] private float shellClosedImpactMultiplier = 0.1f;
    
    [Space]
    [SerializeField] private LayerMask whatIsTerrain;

    [Space] 
    [SerializeField, Min(0f)] private float attackInterval;
    
    [Header("Spit Attack")] 
    [SerializeField] private GameObject spitProjectile;
    [SerializeField] private Transform spitOrigin;
    [SerializeField] private float spitVelocityCorrection = 0.5f;
    [SerializeField] private float spitGravityCorrection = 1f;
    [SerializeField, Min(1)] private Vector2Int spitBurstSizeRange;
    [SerializeField, Min(0f)] private float spitBurstInterval;

    private new void Awake()
    {
        base.Awake();

        StartCoroutine(AttackCoroutine());
    }

    public override void attack(Vector2 impulse)
    {
        if (state == ClamState.SHELL_CLOSED)
        {
            base.attack(shellClosedImpactMultiplier * impulse);
        }
        else
        {
            //TODO: check with team if this behaviour is correct
            base.attack(impulse);
        }
        
        if (Random.value < shellCloseChance) state = ClamState.SHELL_CLOSED;
    }


    private IEnumerator AttackCoroutine()
    {
        if (state == ClamState.SPAWNING)
        {
            yield return StartCoroutine(SpawnAnim());
        }
        
        state = ClamState.IDLE;

        while (isAlive)
        {
            if (state == ClamState.SHELL_CLOSED)
            {
                yield return new WaitForSeconds(shellOpenCheckInterval);

                if (Random.value < shellOpenChance)
                {
                    if (Vector3.Distance(transform.position, target.position) > shellOpenMinTargetDistance)
                    {
                        state = ClamState.IDLE;
                    }
                }
                
                continue;
            }
            
            if (!Physics.Linecast(transform.position, target.position, whatIsTerrain,
                    QueryTriggerInteraction.Collide))
            {
                if (Random.value < 1)
                {
                    state = ClamState.SPITTING;

                    int spitBurstSize = Random.Range(spitBurstSizeRange.x, spitBurstSizeRange.y);
                    
                    for (int i = 0; i < spitBurstSize; i++)
                    {
                        if (state == ClamState.SHELL_CLOSED) break;
                        
                        Spit();

                        yield return new WaitForSeconds(spitBurstInterval);
                    }
                    
                    if (state != ClamState.SHELL_CLOSED) state = ClamState.IDLE;
                }
            }
            
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator SpawnAnim()
    {
        //TODO
        yield return null;
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
