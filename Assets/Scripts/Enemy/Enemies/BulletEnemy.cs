using UnityEngine;

public class BulletEnemy : Enemy
{
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField, Min(0f)] private float speed;

    [Space] 
    [SerializeField] private bool parryable;
    [SerializeField] private bool homing;
    [SerializeField] private float homingStrength;

    public float Speed => speed;
    public bool UsesGravity => rb.useGravity;
    
    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        
        // destroy self after 30 seconds as failsafe when nothing else destroys it
        Destroy(gameObject, 30f);
    }

    private void FixedUpdate()
    {
        if (homing)
        {
            rb.linearVelocity = Vector3.LerpUnclamped(
                rb.linearVelocity.normalized,
                (target.position - transform.position).normalized,
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
