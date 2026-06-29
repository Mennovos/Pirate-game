using UnityEngine;

public enum CrabState
{
    Walking,
    ChargeAttack,
    Death
}

[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(Animator))]
public class CrabEnemy : MonoBehaviour
{
    public float speed = 2f;
    public float distance = 5f;
    public float chargeSpeed = 5f;
    public float chargeCooldown = 3f;
    public float maxHealth = 30f;

    private Vector3 startPosition;
    private int direction = 1;
    private CrabState currentState = CrabState.Walking;
    private float chargeTimer = 0f;
    private float currentHealth;
    private Animator animator;
    private bool hasReversed = false;

    void Start()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (currentState == CrabState.Death)
            return;

        chargeTimer += Time.deltaTime;

        switch (currentState)
        {
            case CrabState.Walking:
                HandleWalking();
                if (chargeTimer >= chargeCooldown)
                {
                    ChangeState(CrabState.ChargeAttack);
                }
                break;

            case CrabState.ChargeAttack:
                HandleChargeAttack();
                break;
        }
    }

    private void HandleWalking()
    {
        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        float distanceFromStart = Mathf.Abs(transform.position.x - startPosition.x);

        if (distanceFromStart >= distance && !hasReversed)
        {
            direction *= -1;
            hasReversed = true;
        }
        else if (distanceFromStart < distance - 0.1f)
        {
            hasReversed = false;
        }
    }

    private void HandleChargeAttack()
    {
        transform.position += new Vector3(direction * chargeSpeed * Time.deltaTime, 0f, 0f);

        if (chargeTimer >= chargeCooldown + 1f)
        {
            ChangeState(CrabState.Walking);
        }
    }

    private void ChangeState(CrabState newState)
    {
        currentState = newState;
        chargeTimer = 0f;

        animator.SetInteger("State", (int)currentState);
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
        ChangeState(CrabState.Death);
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
}