using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IEnemy
{
    [SerializeField] protected Animator animator;
    
    [Space]
    [SerializeField] protected Transform target;
    [SerializeField] private float attackDamage;
    [SerializeField] private float scoreAmount;

    [Space] 
    [SerializeField, Min(0f)] private Vector2 scaleRange = Vector2.one;
    
    [Space]
    [SerializeField] private List<AudioClip> hitSounds;
    [SerializeField] private AudioSource audioSource;
    
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
        PlayHitSound();
        
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
    public float getScoreAmount()
    {
        return (float) scoreAmount;
    }

    protected void PlayHitSound()
    {
        if (hitSounds.Count == 0 || !audioSource) return;
        
        int index = Random.Range(0, hitSounds.Count);
        AudioClip clip = hitSounds[index];
        
        audioSource.PlayOneShot(clip);
    }
}