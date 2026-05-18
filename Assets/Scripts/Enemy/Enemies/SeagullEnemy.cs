using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class SeagullEnemy : Enemy
{
    private static readonly int animator_State = Animator.StringToHash("State");

    private enum SeagullState
    {
        IDLE,
        DIVE,
        KNOCKBACK
    }

    [Space]
    [SerializeField, Min(0)] private float horizontalIdleVelocity;
    [SerializeField, Min(0)] private float verticalIdleVelocity;
    [SerializeField] private float verticalIdleVelocityPassive;
    [SerializeField, Min(0)] private float idleHeightFromGround;
    [SerializeField] private float idleMinX;
    [SerializeField] private float idleMaxX;
    [SerializeField, Min(0)] private float idleWallAvoidanceDistance;
    [SerializeField, Min(0)] private float idleVelocitySmoothing;

    [Space]
    [SerializeField, Min(0)] private float diveVelocity;
    
    [Space]
    [SerializeField, Min(0)] private float knockbackDuration;

    [Space]
    [SerializeField] private LayerMask whatIsTerrain;
    
    [Space]
    [SerializeField, Min(0)] private float minAttackCheckInterval;
    [SerializeField, Min(0)] private float maxAttackCheckInterval;
    [SerializeField, Range(0, 1)] private float attackChance;
    
    [Space]
    [SerializeField] private float diveSpinSpeed = 720f;
    [SerializeField] private float knockbackSpinSpeed = 360f;
    
    private SeagullState state = SeagullState.IDLE;
    
    private bool goingRight;
    private Vector2 diveTargetPos;

    private new void Awake()
    {
        base.Awake();
        goingRight = Random.value < 0.5f;

        StartCoroutine(attackSwitchCoroutine());
        StartCoroutine(knockbackSwitchCoroutine());
    }

    private void Update()
    {
        if (animator)
        {
            animator.SetInteger(animator_State, (int)state);
        }
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();
        
        switch (state)
        {
            case SeagullState.IDLE:
            {
                if (Physics.Raycast(transform.position, goingRight ? Vector3.right : Vector3.left,
                        idleWallAvoidanceDistance, whatIsTerrain, QueryTriggerInteraction.Collide))
                {
                    goingRight = !goingRight;
                }

                if (transform.position.x < idleMinX) goingRight = true;
                if (transform.position.x > idleMaxX) goingRight = false;
                
                Vector2 velocity = new Vector2(
                    goingRight ? horizontalIdleVelocity : -horizontalIdleVelocity, 
                    verticalIdleVelocityPassive);

                if (Physics.Raycast(transform.position, Vector3.down, idleHeightFromGround, 
                        whatIsTerrain, QueryTriggerInteraction.Collide))
                {
                    velocity.y = verticalIdleVelocity;
                }
                
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, velocity, idleVelocitySmoothing * Time.fixedDeltaTime);
                
                break;
            }

            case SeagullState.DIVE:
            {
                if (Vector2.Distance(transform.position, diveTargetPos) < 0.1f)
                {
                    state = SeagullState.IDLE;
                    break;
                }
                
                Vector2 direction = (diveTargetPos - (Vector2)transform.position).normalized;
                Vector2 velocity = direction * diveVelocity;
                
                rb.linearVelocity = velocity;
                
                break;
            }

            case SeagullState.KNOCKBACK:
            {
                break;
            }
        }

        if (state == SeagullState.DIVE)
        {
            float zRot = transform.eulerAngles.z;
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity, Vector3.up);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 
                zRot + diveSpinSpeed * Time.fixedDeltaTime);
        }
        else if (state == SeagullState.KNOCKBACK)
        {
            float zRot = transform.eulerAngles.z;
            transform.rotation = Quaternion.LookRotation(-rb.linearVelocity, Vector3.up);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 
                zRot + knockbackSpinSpeed * Time.fixedDeltaTime);
        } 
        else
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity, Vector3.up);
        }
    }


    private IEnumerator attackSwitchCoroutine()
    {
        while (true)
        {
            if (state == SeagullState.IDLE)
            {
                if (target)
                {
                    if (target.position.y < transform.position.y
                        && !Physics.Linecast(transform.position, target.position, whatIsTerrain,
                            QueryTriggerInteraction.Collide))
                    {
                        if (attackChance > Random.value)
                        {
                            state = SeagullState.DIVE;
                        
                            diveTargetPos = target.position;
                        }
                    }
                }
            }
            
            yield return new WaitForSeconds(Random.Range(minAttackCheckInterval, maxAttackCheckInterval));
        }
    }

    private IEnumerator knockbackSwitchCoroutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => state == SeagullState.KNOCKBACK);
            yield return new WaitForSeconds(knockbackDuration);
            state = SeagullState.IDLE;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (state == SeagullState.DIVE) state = SeagullState.IDLE;
    }


    public override void attack(Vector2 impulse)
    {
        state = SeagullState.KNOCKBACK;
        
        rb.linearVelocity = impulse / rb.mass;
    }
}
