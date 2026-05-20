using System;
using System.Collections;
using UnityEngine;
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
    [SerializeField, Range(0f, 1f)] private float spitAttackChance = 0.5f;
    
    [Header("Spawn Jump")]
    [SerializeField] private float spawnJumpStartZ;
    [SerializeField] private float spawnJumpStartHeight;
    [SerializeField] private float spawnJumpPeakHeight;
    [SerializeField] private Transform spawnJumpAnimatedTransform; // NOT SELF

    private new void Awake()
    {
        base.Awake();

        spawnJumpAnimatedTransform.position = transform.position + new Vector3(0, spawnJumpStartHeight, spawnJumpStartZ);
        
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

        yield return new WaitForSeconds(attackInterval);
        
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
                if (Random.value < spitAttackChance)
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
        float jumpDuration = Mathf.Sqrt(2 * (spawnJumpPeakHeight - spawnJumpStartHeight) / Physics.gravity.magnitude) 
                             + Mathf.Sqrt(2 * spawnJumpPeakHeight / Physics.gravity.magnitude);
        
        float startTime = Time.time;
        float endTime = Time.time + jumpDuration;
        
        float h = 1 - spawnJumpPeakHeight/spawnJumpStartHeight + Mathf.Sqrt(
                    (spawnJumpPeakHeight - spawnJumpStartHeight) * spawnJumpPeakHeight
            ) / spawnJumpStartHeight;

        while (Time.time < endTime)
        {
            float progress = (Time.time - startTime) / jumpDuration;
            
            float y = spawnJumpPeakHeight * (1 - (progress - h)*(progress - h)/((1 - h)*(1 - h)));
            float z = Mathf.Lerp(spawnJumpStartZ, 0f, progress);
            
            spawnJumpAnimatedTransform.position = transform.position + new Vector3(0f, y, z);
            
            yield return null;
        }

        spawnJumpAnimatedTransform.position = transform.position;
    }
    

    private void Spit()
    {
        GameObject projectile = Instantiate(spitProjectile, spitOrigin.position, Quaternion.Euler(Vector3.zero));
        BulletEnemy bullet = projectile.GetComponent<BulletEnemy>();
        
        float distToTarget = Vector2.Distance(spitOrigin.position, target.position);
        float timeToTarget = distToTarget / bullet.Speed;
        
        Vector3 targetPos = target.position
                + targetVelocity * (timeToTarget * spitVelocityCorrection)
                - (bullet.UsesGravity ? Physics.gravity * (timeToTarget * timeToTarget * spitGravityCorrection) / 2f : Vector3.zero);
        
        projectile.transform.forward = targetPos - spitOrigin.position;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.maroon;
        try
        {
            Gizmos.DrawLineStrip(GetSpawnJumpLine(64), false);
        }
        catch (Exception) {}
    }
    
    private ReadOnlySpan<Vector3> GetSpawnJumpLine(int steps)
    {
        Span<Vector3> points = new Vector3[steps];

        float h = 1 - spawnJumpPeakHeight/spawnJumpStartHeight + Mathf.Sqrt(
                    (spawnJumpPeakHeight - spawnJumpStartHeight) * spawnJumpPeakHeight
            ) / spawnJumpStartHeight;
        
        for (int i = 0; i < steps; i++)
        {
            float progress = i / (steps - 1f);

            float y = spawnJumpPeakHeight * (1 - (progress - h)*(progress - h)/((1 - h)*(1 - h)));
            float z = Mathf.Lerp(spawnJumpStartZ, 0f, progress);
            
            points[i] = transform.position + new Vector3(0f, y, z);
        }

        return points;
    }
}
