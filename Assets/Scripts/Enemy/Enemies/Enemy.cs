using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IEnemy
{
    [SerializeField] protected Animator animator;
    
    [Space]
    [SerializeField] protected Transform target;
    [SerializeField] private float attackDamage;
    
    private Vector3 oldTargetPos;
    protected Vector3 targetVelocity;
    
    protected Rigidbody rb;
    
    protected bool isAlive = true;


    protected void Awake()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
        
        rb = GetComponent<Rigidbody>();
    }

    protected void FixedUpdate()
    {
        if (!target) return;
        
        targetVelocity = (target.position - oldTargetPos) / Time.fixedDeltaTime;
        
        oldTargetPos = target.position;
    }


    public virtual bool isDead()
    {
        return !isAlive;
    }

    public virtual void attack(Vector2 impulse)
    {
        rb.linearVelocity = impulse / rb.mass;
    }

    public virtual float getAttackDamage()
    {
        return attackDamage;
    }

    public virtual void kill()
    {
        isAlive = false;
        Destroy(gameObject);
    }
}
