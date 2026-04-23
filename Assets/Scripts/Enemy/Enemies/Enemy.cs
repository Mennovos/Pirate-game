using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IEnemy
{
    [SerializeField] protected Transform target;
    [SerializeField] private float attackDamage;
    
    protected Rigidbody rb;


    protected void Awake()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
        
        rb = GetComponent<Rigidbody>();
    }


    public virtual bool isDead()
    {
        return false;
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
        Destroy(gameObject);
    }
}
