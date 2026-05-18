using UnityEngine;

public class BulletEnemy : Enemy
{
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField, Min(0f)] private float speed;
    [SerializeField] private bool destroyAfterTime;

    [Space] 
    [SerializeField] private bool parryable;
    [SerializeField] private bool homing;
    [SerializeField] private float homingStrength;
    [SerializeField] private float homingVelocityCorrection = 1f;

    public float Speed => speed;
    public bool UsesGravity => rb.useGravity;
    
    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        
        // destroy self after 30 seconds as failsafe when nothing else destroys it
        if (destroyAfterTime) Destroy(gameObject, 30f);
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();
        
        if (homing)
        {
            float distToTarget = Vector2.Distance(transform.position, target.position);
            float timeToTarget = distToTarget / speed;

            Vector3 targetPos = target.position
                                + targetVelocity * (timeToTarget * homingVelocityCorrection);
            
            rb.linearVelocity = Vector3.LerpUnclamped(
                rb.linearVelocity.normalized,
                (targetPos - transform.position).normalized,
                Time.fixedDeltaTime * homingStrength
            ).normalized * speed;
        }
        
        rb.linearVelocity = rb.linearVelocity.normalized * speed;
        
        transform.forward = rb.linearVelocity.normalized;
    }

    public override void attack(Vector2 impulse)
    {
        if (parryable)
        {
            rb.linearVelocity = impulse.normalized * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayerMask.value) != 0)
        {
            kill();
        }
    }
}
