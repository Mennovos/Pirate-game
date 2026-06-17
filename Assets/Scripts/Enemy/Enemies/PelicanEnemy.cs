using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PelicanEnemy : Enemy
{
    private static readonly int animator_State = Animator.StringToHash("State");
    
    private enum PelicanState
    {
        IDLE,
        SEAGULL_ATTACK,
        SEAGULL_ATTACK_WINDUP,
        SEAGULL_ATTACK_WINDDOWN,
        BULLET_ATTACK,
        BULLET_ATTACK_WINDUP,
        BULLET_ATTACK_WINDDOWN,
        SWOOP_ATTACK,
        DEFEATED
    }
    
    private PelicanState state = PelicanState.IDLE;
    
    [Space]
    [SerializeField] private float maxHealth;
    private float health;
    
    [SerializeField] private HealthBar healthBar;
    
    [Space]
    [SerializeField] private float timeToDestroyAfterDefeat = 15f;
    [SerializeField] private Vector3 velocityAfterDefeat = new(0f, 5f, 0f);
    
    [Header("Attack General")]
    [SerializeField] private Transform spawnerTransform;
    [SerializeField, Min(0)] private int seagullAttackWeight;
    [SerializeField, Min(0)] private int bulletAttackWeight;
    [SerializeField, Min(0)] private int swoopAttackWeight;
    [SerializeField, Min(0)] private int idleWeight;
    [SerializeField, Min(0f)] private float idleDuration;

    [Header("Attack Seagulls")] 
    [SerializeField] private GameObject seagullPrefab;
    [SerializeField, Min(0f)] private float seagullWindupTime;
    [SerializeField, Min(0f)] private float seagullWinddownTime;
    [SerializeField, Min(0f)] private float seagullIntervalTime;
    [SerializeField, Min(0f)] private float seagullSpawnImpulse;
    [SerializeField, Min(1)] private Vector2Int seagullCountRange;

    [Header("Attack Bullets")] 
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField, Min(0f)] private float bulletWindupTime;
    [SerializeField, Min(0f)] private float bulletWinddownTime;
    [SerializeField, Min(0f)] private float bulletIntervalTime;
    [SerializeField, Min(1)] private Vector2Int bulletFanCountRange;
    [SerializeField, Min(2)] private Vector2Int bulletFanSizeRange;
    [SerializeField, Min(0f)] private float bulletFanAngle;

    [Header("Attack Swoop")] 
    [SerializeField] private Vector2 swoopLeft;
    [SerializeField] private Vector2 swoopMiddle;
    [SerializeField] private Vector2 swoopRight;
    [SerializeField] private bool isRight;
    [SerializeField, Min(0f)] private float swoopDuration;
    
    private new void Awake()
    {
        base.Awake();
        
        health = maxHealth;
        
        transform.rotation = Quaternion.LookRotation(isRight ? Vector3.left : Vector3.right, Vector3.up);

        StartCoroutine(AttackCoroutine());
    }

    private void Update()
    {
        if (state != PelicanState.SWOOP_ATTACK && state != PelicanState.DEFEATED)
        {
            //TODO: point head to target
        }
        
        if (animator)
        {
            animator.SetInteger(animator_State, (int)state);
        }
    }

    public override void attack(Vector2 impulse)
    {
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
            
            state = PelicanState.DEFEATED;
            
            rb.useGravity = true;
            rb.excludeLayers = ~0;

            rb.constraints = RigidbodyConstraints.FreezeRotation;

            rb.linearVelocity = velocityAfterDefeat;

            if (TryGetComponent(out TrackingTarget trackingTarget))
            {
                trackingTarget.Weight = 0f;
            }
            
            utilities.AddScore(scoreAmount);
            
            MenuManager.Instance.OnBossDeath("Pelican");
        }
    }


    public override float getAttackDamage()
    {
        return state == PelicanState.SWOOP_ATTACK ? contactDamage : 0f;
    }


    private IEnumerator AttackCoroutine()
    {
        while (isAlive)
        {
            int num = Random.Range(0, seagullAttackWeight + bulletAttackWeight + swoopAttackWeight + idleWeight);

            if (num < seagullAttackWeight)
            {
                yield return StartCoroutine(SeagullAttack());
            }
            else if (num < seagullAttackWeight + bulletAttackWeight)
            {
                yield return StartCoroutine(BulletAttack());
            }
            else if (num < seagullAttackWeight + bulletAttackWeight + swoopAttackWeight)
            {
                yield return StartCoroutine(SwoopAttack());
            }
            else
            {
                yield return new WaitForSeconds(idleDuration);
            }
        }
    }
    

    private IEnumerator SeagullAttack()
    {
        state = PelicanState.SEAGULL_ATTACK_WINDUP;
        yield return new WaitForSeconds(seagullWindupTime);
        state = PelicanState.SEAGULL_ATTACK;
        
        int seagulls = Random.Range(seagullCountRange.x, seagullCountRange.y);

        for (int i = 0; i < seagulls; i++)
        {
            if (state == PelicanState.DEFEATED || !target) yield break;
            
            Vector2 direction = (target.position - spawnerTransform.position).normalized;
            
            GameObject seagull = Instantiate(seagullPrefab, spawnerTransform.position, spawnerTransform.rotation);
            Enemy enemy = seagull.GetComponent<Enemy>();
            enemy.attack(direction * seagullSpawnImpulse);
            
            yield return new WaitForSeconds(seagullIntervalTime);
        }
        
        state = PelicanState.SEAGULL_ATTACK_WINDDOWN;
        yield return new WaitForSeconds(seagullWinddownTime);
        state = PelicanState.IDLE;
    }

    private IEnumerator BulletAttack()
    {
        state = PelicanState.BULLET_ATTACK_WINDUP;
        yield return new WaitForSeconds(bulletWindupTime);
        state = PelicanState.BULLET_ATTACK;
        
        int fans = Random.Range(bulletFanCountRange.x, bulletFanCountRange.y);

        for (int i = 0; i < fans; i++)
        {
            if (state == PelicanState.DEFEATED || !target) yield break;
            
            Vector2 direction = (target.position - spawnerTransform.position).normalized;
            int bullets = Random.Range(bulletFanSizeRange.x, bulletFanSizeRange.y);

            for (int j = 0; j < bullets; j++)
            {
                float angleOffset = (j / (bullets - 1f) - 0.5f) * bulletFanAngle;
                
                GameObject bullet = Instantiate(bulletPrefab, spawnerTransform.position, spawnerTransform.rotation);
                bullet.transform.forward = Quaternion.Euler(0, 0, angleOffset) * direction;
            }
            
            yield return new WaitForSeconds(bulletIntervalTime);
        }
        
        state = PelicanState.BULLET_ATTACK_WINDDOWN;
        yield return new WaitForSeconds(bulletWinddownTime);
        state = PelicanState.IDLE;
    }

    private IEnumerator SwoopAttack()
    {
        state = PelicanState.SWOOP_ATTACK;
        
        Vector2 start = transform.position;
        Vector2 middle = swoopMiddle;
        Vector2 end = isRight ? swoopLeft : swoopRight;
        
        float startTime = Time.time;
        float endTime = startTime + swoopDuration;

        while (Time.time < endTime)
        {
            if (state == PelicanState.DEFEATED) yield break;
            
            float progress = Mathf.Clamp01((Time.time - startTime) / swoopDuration);

            float t = Mathf.Sin(Mathf.PI / 2f * progress) * Mathf.Sin(Mathf.PI / 2f * progress);

            transform.position = Vector3.Lerp(
                Vector3.Lerp(start, middle, t), 
                Vector3.Lerp(middle, end, t), 
                t
            );

            Vector3 dif = start * (2 * (t - 1f)) - middle * (4 * (t - 0.5f)) + end * (2 * t);

            transform.rotation = Quaternion.LookRotation(dif.normalized, Vector3.up);
            
            yield return null;
        }
        
        transform.position = end;

        isRight = !isRight;

        transform.rotation = Quaternion.LookRotation(isRight ? Vector3.left : Vector3.right, Vector3.up);

        state = PelicanState.IDLE;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.maroon;
        Gizmos.DrawSphere(swoopLeft, 0.1f);
        Gizmos.DrawSphere(swoopMiddle, 0.1f);
        Gizmos.DrawSphere(swoopRight, 0.1f);
        Gizmos.DrawLineStrip(GetSwoopLine(64), false);
    }

    private ReadOnlySpan<Vector3> GetSwoopLine(int steps)
    {
        Span<Vector3> points = new Vector3[steps];

        for (int i = 0; i < steps; i++)
        {
            float progress = i / (steps - 1f);

            points[i] = Vector3.Lerp(
                Vector3.Lerp(
                    swoopLeft, 
                    swoopMiddle, 
                    progress
                ),
                Vector3.Lerp(
                    swoopMiddle, 
                    swoopRight, 
                    progress
                ),
                progress
            );
        }

        return points;
    }
}
