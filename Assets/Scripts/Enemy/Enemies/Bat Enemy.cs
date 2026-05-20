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
    [SerializeField] private Vector3 BasePos;

    [SerializeField] private GameObject Visualclutter;
    public bool visualClutterActive = false;
    private Transform player;
    private Movement movement;

    private void Awake()
    {
        BasePos = transform.position;
        base.Awake();
    }


    private void FixedUpdate()
    {
        // keep track of the player
        player = GameObject.FindGameObjectWithTag("Player").transform;

        switch (currentState)
            {
                case BatState.IDLE:
                if (currentState == BatState.IDLE)
                {

                    // Move back to the base position
                    transform.position = Vector3.Lerp(transform.position, BasePos, moveBackSpeed * Time.fixedDeltaTime);

                    // Check if the player is within range to start chasing
                    if (Vector3.Distance(transform.position, player.position) <= playerInRange)
                    {
                        currentState = BatState.Chase;
                    }
                }
                break;

                case BatState.Chase:
                if (currentState == BatState.Chase)
                {
                    //chase the player
                    Vector3 direction = (player.position - transform.position).normalized;
                    transform.position += direction * moveSpeed * Time.fixedDeltaTime;

                    // Check if the player is out of range to return to idle
                    if (Vector3.Distance(transform.position, player.position) > playerInRange)
                    {
                        currentState = BatState.IDLE;
                    }
                }
                    break;
            case BatState.SuckingBlood:
               
                if (currentState == BatState.SuckingBlood)
                {
                    movement = FindFirstObjectByType<Movement>();

                    if (movement.mashClicks() < 4)
                    {
                        Visualclutter.SetActive(true);
                    }
                    if (movement.mashClicks() == 0) 
                    { 
                        Debug.Log("Player has mashed enough, exiting sucking blood state.");    
                        Visualclutter.SetActive(false);
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
        if(collision.gameObject.CompareTag("Player"))
        {
            currentState = BatState.SuckingBlood;
        }
    }
}
