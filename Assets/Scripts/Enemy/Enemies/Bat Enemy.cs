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
    [SerializeField] private float radius;

    private Transform player;

    private void Awake()
    {
       base.Awake();
       player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
            switch(currentState)
            {
                case BatState.IDLE:
                 
                    break;
                case BatState.Chase:
                    if (Vector3.Distance(transform.position, player.position) <= playerInRange)
                    {
                        Vector3 direction = (player.position - transform.position).normalized;
                        transform.position += direction * moveSpeed * Time.fixedDeltaTime;
                    }
                    break;
                case BatState.KNOCKBACK:
                   
                    break;
            }
    }


}
