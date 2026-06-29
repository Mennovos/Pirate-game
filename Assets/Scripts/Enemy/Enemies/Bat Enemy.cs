using NUnit.Framework;
using UnityEngine;

public class BatEnemy : Enemy
{
    private enum BatState
    {
        IDLE,
        Chase,
        KNOCKBACK,
        SuckingBlood
    }

    [SerializeField] private BatState currentState = BatState.IDLE;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveBackSpeed;
    [SerializeField] private float playerInRange;
    [SerializeField] private float coolDownTimerChase;

    [SerializeField] private Vector3 BasePos;

    [SerializeField] private GameObject Visualclutter;
    public bool visualClutterActive = false;
    private Movement movement;
   

    private new void Awake()
    {
        movement = FindAnyObjectByType<Movement>();
        BasePos = transform.position;
        base.Awake();
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();
        switch (currentState)
            {
                case BatState.IDLE:
                animator.SetBool("Flying", false);
                animator.SetBool("Idle", true);
                animator.SetBool("SuckingBlood", false);
                if (currentState == BatState.IDLE)
                {

                    // Move back to the base position
                    transform.position = Vector3.Lerp(transform.position, BasePos, moveBackSpeed * Time.fixedDeltaTime);

                    // check if sucking blood is active and if the player has mashed enough to stop it
                    if (movement.mashClicks() == 5)
                    {
                        Visualclutter.SetActive(false);
                    }

                    // Check if the player is within range to start chasing
                    if (coolDownTimerChase < 0)
                    {
                        if (Vector3.Distance(transform.position, target.position) <= playerInRange)
                        {
                            currentState = BatState.Chase;
                        }
                    }
                    else
                    {
                        coolDownTimerChase -= Time.fixedDeltaTime;
                    }
                }
                break;

                case BatState.Chase:
                animator.SetBool("Flying", true);
                animator.SetBool("Idle", false);
                animator.SetBool("SuckingBlood", false);
                if (currentState == BatState.Chase)
                {
                    //chase the player
                    Vector3 direction = (target.position - transform.position).normalized;
                    transform.position += direction * moveSpeed * Time.fixedDeltaTime;

                    // Check if the player is out of range to return to idle
                    if (Vector3.Distance(transform.position, target.position) > playerInRange)
                    {
                        currentState = BatState.IDLE;
                    }
                }
                    break;
            case BatState.SuckingBlood:
                animator.SetBool("Flying", false);
                animator.SetBool("Idle", false);
                animator.SetBool("SuckingBlood", true);
                if (currentState == BatState.SuckingBlood)
                {

                    if (movement.mashClicks() < 4)
                    {
                        Visualclutter.SetActive(true);
                       movement.OnIspaused();
                    }
                    if (movement.mashClicks() == 5)
                    {
                        currentState = BatState.IDLE;
                    }
                }
                break;
        }
    }
    public override void attack(Vector2 impulse)
    {
        currentState = BatState.KNOCKBACK;

        rb.linearVelocity = impulse / rb.mass;
    }
    private void OnCollisionEnter(Collision collision)
    {
        movement = collision.gameObject.GetComponent<Movement>();
        if (collision.gameObject.CompareTag("Player"))
        {
            currentState = BatState.SuckingBlood;
            movement.mashAmount = 1;
            coolDownTimerChase = 5f; // Reset the chase cooldown timer
        }
    }
}
