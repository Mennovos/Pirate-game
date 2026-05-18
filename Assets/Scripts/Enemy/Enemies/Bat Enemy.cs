using UnityEngine;
using UnityEngine.Rendering;

public class BatEnemy : Enemy
{
    private enum BatState
    {
        IDLE,
        Chase,
        KNOCKBACK
    }
    private BatState currentState = BatState.IDLE;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float playerInRange;
    [SerializeField] private Vector3 BasePos;

    private Transform player;

    private void Awake()
    {
       base.Awake();
    }

    private void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
            switch(currentState)
            {
                case BatState.IDLE:
                   transform.position = Vector3.Lerp(transform.position, BasePos, moveSpeed * Time.fixedDeltaTime);
                if (Vector3.Distance(transform.position, player.position) <= playerInRange)
                {
                    currentState = BatState.Chase;
                }
                break;
                case BatState.Chase:
                        Vector3 direction = (player.position - transform.position).normalized;
                        transform.position += direction * moveSpeed * Time.fixedDeltaTime;
                    break;
            }
    }
    public override void attack(Vector2 impulse)
    {
        currentState = BatState.KNOCKBACK;

        rb.linearVelocity = impulse / rb.mass;
    }
}
