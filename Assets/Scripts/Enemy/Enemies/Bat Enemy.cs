using Unity.Mathematics;
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

    // New flag to ensure we only initialize sucking once per entry
    private bool suckingInitialized = false;

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

                // Move back to the base position
                transform.position = Vector3.Lerp(transform.position, BasePos, moveBackSpeed * Time.fixedDeltaTime);

                // check if sucking blood is active and if the player has mashed enough to stop it
                if (movement != null && movement.mashClicks() == 5)
                {
                    if (Visualclutter) Visualclutter.SetActive(false);
                    suckingInitialized = false; // ensure flag reset
                }

                // Check if the player is within range to start chasing
                if (coolDownTimerChase < 0)
                {
                    if (target != null && Vector3.Distance(transform.position, target.position) <= playerInRange)
                    {
                        currentState = BatState.Chase;
                    }
                }
                else
                {
                    coolDownTimerChase -= Time.fixedDeltaTime;
                }
                break;

            case BatState.Chase:
                animator.SetBool("Flying", true);
                animator.SetBool("Idle", false);
                animator.SetBool("SuckingBlood", false);

                // chase the player
                if (target != null)
                {
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

                // Initialize sucking state only once when entering
                if (!suckingInitialized)
                {
                    suckingInitialized = true;

                    // Ensure we have a reference to Movement (fall back to finding player movement)
                    if (movement == null && target != null)
                    {
                        var playerObj = GameObject.FindGameObjectWithTag("Player");
                        if (playerObj != null)
                            movement = playerObj.GetComponent<Movement>();
                    }

                    // Activate visual clutter if assigned
                    if (Visualclutter) Visualclutter.SetActive(true);

                    // Pause player input once (movement.OnIspaused is idempotent here)
                    if (movement != null)
                    {
                        movement.OnIspaused();
                    }
                }

                // While sucking, keep bat stopped
                rb.linearVelocity = Vector3.zero; // Stop the bat's movement

                // If player mashed enough, stop sucking
                if (movement != null && movement.mashClicks() >= 5)
                {
                    if (Visualclutter) Visualclutter.SetActive(false);
                    suckingInitialized = false;
                    currentState = BatState.IDLE;
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
        // Only use player's Movement component if needed; do not overwrite the stored 'movement' reference blindly
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get Movement component from collided player (only if we don't already have one)
            if (movement == null)
            {
                movement = collision.gameObject.GetComponent<Movement>();
            }

            currentState = BatState.SuckingBlood;

            if (movement != null)
            {
                movement.mashAmount = 1;
            }

            coolDownTimerChase = 5f; // Reset the chase cooldown timer

            // reset sucking initialization so FixedUpdate will do the one-time setup
            suckingInitialized = false;
        }
    }
}
