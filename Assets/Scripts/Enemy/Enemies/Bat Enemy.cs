using System;
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

                    // Start the mash challenge: require 4 presses in 3 seconds, otherwise deal 10 damage
                    if (movement != null)
                    {
                        movement.StartMashingChallenge(requiredMashes: 4, timeWindow: 3f, damageOnFail: 10, onComplete: (success) =>
                        {
                            // Always turn off visual clutter when challenge completes
                            if (Visualclutter) Visualclutter.SetActive(false);

                            // If the player succeeded, return to idle; otherwise keep default behavior (damage already applied by Movement)
                            if (success)
                            {
                                currentState = BatState.IDLE;
                            }
                            else
                            {
                                // Failure already handled by Movement (damage). Return to idle after a short delay.
                                currentState = BatState.IDLE;
                            }

                            // Reset sucking init so future collisions behave correctly
                            suckingInitialized = false;
                        });
                    }
                }

                // While sucking, keep bat stopped
                rb.linearVelocity = Vector3.zero; // Stop the bat's movement

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
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get Movement component from the player (if we don't already have it)
            if (movement == null)
            {
                movement = collision.gameObject.GetComponent<Movement>();
            }

            currentState = BatState.SuckingBlood;
            suckingInitialized = false;
            coolDownTimerChase = 5f; // Reset the chase cooldown timer
        }
    }
}
