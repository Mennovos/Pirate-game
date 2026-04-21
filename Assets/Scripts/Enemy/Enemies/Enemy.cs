using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IEnemy
{
    [SerializeField] protected Transform target;
    
    protected Rigidbody rb;


    protected void Awake()
    {
        target ??= GameObject.FindGameObjectWithTag("Player").transform;
        
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
}
