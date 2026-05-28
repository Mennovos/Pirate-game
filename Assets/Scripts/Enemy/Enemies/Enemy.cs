using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IEnemy
{
    [SerializeField] protected Animator animator;
    
    [Space]
    [SerializeField] protected Transform target;
    [SerializeField] private float attackDamage;
    [SerializeField] public float scoreAmount;

    [Space] 
    [SerializeField, Min(0f)] private Vector2 scaleRange = Vector2.one;
    
    private Vector3 oldTargetPos;
    protected Vector3 targetVelocity;
    
    protected Rigidbody rb;
    protected Utilities utilities;
    
    protected bool isAlive = true;


    protected void Awake()
    {
        transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);
        
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
        
        rb = GetComponent<Rigidbody>();
        utilities = FindAnyObjectByType<Utilities>();
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
        TimeManager.Instance.AddHitstop(0.1f);
        
        rb.linearVelocity = impulse / rb.mass;
    }

    public virtual float getAttackDamage()
    {
        return attackDamage;
    }

    public virtual void kill()
    {
        utilities.AddScore(scoreAmount);
        isAlive = false;
        Destroy(gameObject);
    }
    
}