using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class FishbearEnemy : Enemy
{
    private static readonly int animator_State = Animator.StringToHash("State");
    
    private enum FishbearState
    {
        IDLE,
        CRAB_ATTACK,
        CRAB_ATTACK_WINDUP,
        CRAB_ATTACK_WINDDOWN,
        DASH_ATTACK,
        DASH_ATTACK_WINDUP,
        DASH_ATTACK_WINDDOWN,
        CHOMP_ATTACK,
        PUSH_ATTACK,
        DORMANT,
        DEFEATED
    }
    
    private FishbearState state = FishbearState.DORMANT;
    
    [Space]
    [SerializeField] private float maxHealth;
    private float health;
    
    [SerializeField] private HealthBar healthBar;
    
    [Space]
    [SerializeField] private float timeToDestroyAfterDefeat = 15f;
    [SerializeField] private Vector3 velocityAfterDefeat = new(0f, 5f, 0f);
    
    [Space]
    [SerializeField, Min(0f)] private float wakeDistance;
    [SerializeField, Min(0f)] private float wakeDuration;
    [SerializeField] private WakeEvent onWakeEvent;
    
    [Header("Attack General")]
    [SerializeField] private Transform spawnerTransform;
    [SerializeField, Min(0)] private int crabAttackWeight = 1;
    [SerializeField, Min(0)] private int dashAttackWeight = 1;
    [SerializeField, Min(0)] private int idleWeight = 1;
    [SerializeField, Min(0f)] private float idleDuration;

    [Header("Attack Crab")] 
    [SerializeField] private GameObject crabPrefab;
    [SerializeField, Min(0f)] private float crabWindupTime;
    [SerializeField, Min(0f)] private float crabWinddownTime;
    [SerializeField, Min(0f)] private float crabIntervalTime;
    [SerializeField, Min(1)] private Vector2Int crabFanCountRange;
    [SerializeField, Min(1)] private Vector2Int crabFanSizeRange;
    [SerializeField, Min(0f)] private float crabFanAngle;
    
    [Header("Attack Dash")]
    [SerializeField, Min(0f)] private float dashSpeed;
    [SerializeField, Min(0f)] private float dashDamage;
    [SerializeField, Min(0f)] private float dashWindupTime;
    [SerializeField, Min(0f)] private float dashWinddownTime;
    [SerializeField] private Vector2 dashRebound;
    [SerializeField, Min(0f)] private float dashShakeStrength;
    [SerializeField, Min(0f)] private float dashShakeDuration;
    [SerializeField] private Vector3 dashWallCheckPosition;
    [SerializeField] private float dashWallCheckRadius;
    
    [Header("Attack Chomp")]
    [SerializeField, Min(0)] private int chompMashRequirement;
    [SerializeField, Min(0f)] private float chompDamage;
    [SerializeField, Min(0f)] private float chompDamageInterval;
    [SerializeField] private Canvas chompMashIndicator;
    
    [Header("Attack Push")] 
    [SerializeField, Min(0f)] private float pushDistance;
    [SerializeField, Min(0f)] private float pushDuration;
    [SerializeField, Min(0f)] private Vector2 pushSpeed;

    private new void Awake()
    {
        base.Awake();

        health = maxHealth;

        StartCoroutine(AttackCoroutine());
    }
    
    private void Update()
    {
        if (animator)
        {
            animator.SetInteger(animator_State, (int)state);
        }
    }
    
    public override void attack(Vector2 impulse)
    {
        if (state == FishbearState.CHOMP_ATTACK) return;
        
        TimeManager.Instance.AddHitstop(0.2f);
        OnHit();
        
        health -= impulse.magnitude;
        
        health = Mathf.Clamp(health, 0f, maxHealth);
        
        healthBar.setHealth(health/maxHealth);
        
        if (health <= 0f) kill();
    }

    public override void kill()
    {
        if (health <= 0f)
        {
            isAlive = false;
            
            Destroy(gameObject, timeToDestroyAfterDefeat);

            state = FishbearState.DEFEATED;
            
            rb.useGravity = true;
            rb.excludeLayers = ~0;

            rb.constraints = RigidbodyConstraints.FreezeRotation;

            rb.linearVelocity = velocityAfterDefeat;

            if (TryGetComponent(out TrackingTarget trackingTarget))
            {
                trackingTarget.Weight = 0f;
            }
            
            utilities.AddScore(scoreAmount);
            
            MenuManager.Instance.OnBossDeath("Fishbear");
        }
    }
    
    
    private IEnumerator AttackCoroutine()
    {
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) < wakeDistance);
        
        state = FishbearState.IDLE;
        
        onWakeEvent.Invoke();
        
        yield return new WaitForSeconds(wakeDuration);
        
        while (isAlive)
        {
            int num = Random.Range(0, crabAttackWeight + dashAttackWeight + idleWeight);

            if (num < crabAttackWeight)
            {
                yield return StartCoroutine(CrabAttack());
            }
            else if (num < crabAttackWeight + dashAttackWeight)
            {
                yield return StartCoroutine(DashAttack());
            }
            else
            {
                float endTime = Time.time + idleDuration;
                
                yield return new WaitUntil(() => Time.time >= endTime 
                            || Vector2.Distance(transform.position, target.position) < pushDistance);

                if (Vector2.Distance(transform.position, target.position) < pushDistance)
                {
                    state = FishbearState.PUSH_ATTACK;

                    if (target.TryGetComponent(out Rigidbody rb))
                    {
                        rb.linearVelocity = transform.forward * pushSpeed.x + Vector3.up * pushSpeed.y;
                    }
                    
                    yield return new WaitForSeconds(pushDuration);
                    
                    state = FishbearState.IDLE;
                }
            }
        }
    }
    

    private IEnumerator CrabAttack()
    {
        state = FishbearState.CRAB_ATTACK_WINDUP;
        yield return new WaitForSeconds(crabWindupTime);
        state = FishbearState.CRAB_ATTACK;
        
        int fans = Random.Range(crabFanCountRange.x, crabFanCountRange.y);

        for (int i = 0; i < fans; i++)
        {
            if (state == FishbearState.DEFEATED || !target) yield break;
            
            if (Vector2.Distance(transform.position, target.position) < pushDistance)
            {
                state = FishbearState.PUSH_ATTACK;

                if (target.TryGetComponent(out Rigidbody rb))
                {
                    rb.linearVelocity = transform.forward * pushSpeed.x + Vector3.up * pushSpeed.y;
                }
                    
                yield return new WaitForSeconds(pushDuration);
                    
                state = FishbearState.IDLE;
                
                yield break;
            }
            
            Vector2 direction = (target.position - spawnerTransform.position).normalized;
            int bullets = Random.Range(crabFanSizeRange.x, crabFanSizeRange.y);

            for (int j = 0; j < bullets; j++)
            {
                float angleOffset = (j / (bullets - 1f) - 0.5f) * crabFanAngle;
                
                GameObject bullet = Instantiate(crabPrefab, spawnerTransform.position, spawnerTransform.rotation);
                bullet.transform.forward = Quaternion.Euler(0, 0, angleOffset) * direction;
            }
            
            yield return new WaitForSeconds(crabIntervalTime);
        }
        
        state = FishbearState.CRAB_ATTACK_WINDDOWN;
        yield return new WaitForSeconds(crabWinddownTime);
        state = FishbearState.IDLE;
    }

    private IEnumerator DashAttack()
    {
        state = FishbearState.DASH_ATTACK_WINDUP;
        yield return new WaitForSeconds(dashWindupTime);
        state = FishbearState.DASH_ATTACK;

        while (!Physics.CheckSphere(transform.TransformPoint(dashWallCheckPosition), dashWallCheckRadius,
                   LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore))
        {
            if (Physics.CheckSphere(transform.TransformPoint(dashWallCheckPosition), dashWallCheckRadius,
                    LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore))
            {
                Collider[] colliders = Physics.OverlapSphere(transform.TransformPoint(dashWallCheckPosition), dashWallCheckRadius,
                    LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
                
                Collider playerCollider = colliders[0];

                if (playerCollider && state != FishbearState.DEFEATED)
                {
                    yield return StartCoroutine(ChompAttack(playerCollider));
                    
                    yield break;
                }
            }
            
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0)
                + transform.forward * dashSpeed;
            
            yield return null;
            
            if (Vector2.Distance(transform.position, target.position) < pushDistance)
            {
                state = FishbearState.PUSH_ATTACK;

                if (target.TryGetComponent(out Rigidbody rb))
                {
                    rb.linearVelocity = transform.forward * pushSpeed.x + Vector3.up * pushSpeed.y;
                }
                    
                yield return new WaitForSeconds(pushDuration);
                    
                state = FishbearState.IDLE;
                
                yield break;
            }
            
            if (state == FishbearState.CHOMP_ATTACK) yield break;
        }
        
        if (state == FishbearState.DEFEATED) yield break;
        
        state = FishbearState.DASH_ATTACK_WINDDOWN;
        cameraMovement.CameraShake(dashShakeStrength, dashShakeDuration);
        rb.linearVelocity = Vector3.up * dashRebound.y - transform.forward * dashRebound.x;
        yield return new WaitForSeconds(dashWinddownTime);
        transform.Rotate(Vector3.up, 180);
        state = FishbearState.IDLE;
    }

    private IEnumerator ChompAttack(Collider playerCollider)
    {
        playerCollider.TryGetComponent(out Movement playerMovement);
        playerCollider.TryGetComponent(out Health playerHealth);
        
        playerMovement.mashAmount = 1;
        
        state = FishbearState.CHOMP_ATTACK;
        
        chompMashIndicator.gameObject.SetActive(true);

        float nextDamageTime = Time.time + chompDamageInterval;

        while (true)
        {
            playerCollider.transform.position = transform.TransformPoint(dashWallCheckPosition);

            if (playerMovement.mashClicks() >= chompMashRequirement || state == FishbearState.DEFEATED) break;

            if (Time.time > nextDamageTime)
            {
                nextDamageTime += chompDamageInterval;
                
                playerHealth.TakeDamage(chompDamage);
            }
            
            yield return new WaitForFixedUpdate();
        }
        
        chompMashIndicator.gameObject.SetActive(false);
        
        if (state != FishbearState.DEFEATED) 
            state = FishbearState.IDLE;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(dashWallCheckPosition), dashWallCheckRadius);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pushDistance);
        
        Gizmos.color = Color.hotPink;
        Gizmos.DrawWireSphere(transform.position, wakeDistance);
    }
    
    
    [Serializable] private class WakeEvent : UnityEvent {}
}
