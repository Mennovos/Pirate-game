using System.Collections;
using UnityEngine;

public enum CrabState
{
    Walking,
    ChargeAttack,
    Death
}

public class CrabEnemy : Enemy
{
    private static readonly int animator_Hit = Animator.StringToHash("Hit");
    private static readonly int animator_Death = Animator.StringToHash("Death");
    
    public float speed = 2f;
    public float chargeSpeed = 5f;
    public float maxHealth = 30f;
    public Vector3 pointA = new Vector3(-5f, 0f, 0f);
    public Vector3 pointB = new Vector3(5f, 0f, 0f);

    private CrabState currentState = CrabState.Walking;
    private float chargeTimer = 0f;
    private float currentHealth;
    private Vector3 targetPoint;
    private float lerpTime = 0f;
    private float journeyLength;

    [SerializeField] private float cooldown;
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float tiltSpeed = 5f;
    
    [Space]
    [SerializeField] private float timeToDestroyAfterDefeat;

    void Start()
    {
        currentHealth = maxHealth;
        transform.position = pointA;
        targetPoint = pointB;
        journeyLength = Vector3.Distance(pointA, pointB);
    }

    void Update()
    { 
        cooldown -= Time.deltaTime;
        if (currentState == CrabState.Death)
            return;

        chargeTimer += Time.deltaTime;

        switch (currentState)
        {
            case CrabState.Walking:
                animator.SetBool("Walking", true);
                lerpTime += Time.deltaTime * speed / journeyLength;

                if (lerpTime >= 1f)
                {
                    SwapTargetPoint();
                    lerpTime = 0f;
                }

                Vector3 startPoint = targetPoint == pointB ? pointA : pointB;
                Vector3 position = Vector3.Lerp(startPoint, targetPoint, lerpTime);
                position.y = transform.position.y;
                transform.position = position;

                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f, groundLayer))
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(Vector3.forward, hit.normal) * Quaternion.Euler(0f, -90f, 0f), 
                        Time.deltaTime * tiltSpeed);
                }

              
                break;

            case CrabState.ChargeAttack:
                animator.SetBool("Walking", false);
                animator.ResetTrigger("Attacking");
                animator.SetTrigger("Attacking");
                StartCoroutine(WalkingAfterCharge());
                break;
        }
    }

    private void SwapTargetPoint()
    {
        if (targetPoint == pointB)
        {
            targetPoint = pointA;
        }
        else
        {
            targetPoint = pointB;
        }
    }


    public override void attack(Vector2 impulse)
    {
        OnHit();
        
        if (currentState != CrabState.Death) animator.SetTrigger(animator_Hit);
        
        TakeDamage(impulse.magnitude);
    }


    public void TakeDamage(float damageAmount)
    {
        if (currentState == CrabState.Death)
            return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
        {
            Die();
            
            kill();
        }
    }

    public override void kill()
    {
        currentState = CrabState.Death;
        
        utilities.AddScore(scoreAmount);
        isAlive = false;
        
        animator.SetTrigger(animator_Death);

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(timeToDestroyAfterDefeat);
        
        Destroy(gameObject);
    }

    public void Die()
    {
        animator.SetTrigger("Die");
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsAlive()
    {
        return currentState != CrabState.Death;
    }
    private void OnTriggerEnter(Collider other)
    { 
        if (other.TryGetComponent(out PlayerHurtbox playerHurtbox) && cooldown <= 0f)
        {

            Debug.Log("Start attack");
            currentState = CrabState.ChargeAttack;
            cooldown = 2f;
        }

    }
    private IEnumerator WalkingAfterCharge()
    {
        yield return new WaitForSeconds(2f);
        currentState = CrabState.Walking;
    }
}