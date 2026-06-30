using System.Collections;
using UnityEditorInternal;
using UnityEngine;

public enum CrabState
{
    Walking,
    ChargeAttack,
    Death
}

public class CrabEnemy : Enemy
{
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
                transform.position = Vector3.Lerp(startPoint, targetPoint, lerpTime);

              
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


    public void TakeDamage(float damageAmount)
    {
        if (currentState == CrabState.Death)
            return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0f)
        {
            Die();
        }
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
        if (other.CompareTag("Player") && cooldown <= 0f)
        {
            currentState = CrabState.ChargeAttack;
            cooldown = 2f;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
         if (collision.gameObject.CompareTag("Player"))
        {
            // Assuming the player has a Health component
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }
    private IEnumerator WalkingAfterCharge()
    {
        yield return new WaitForSeconds(2f);
        currentState = CrabState.Walking;
    }
}