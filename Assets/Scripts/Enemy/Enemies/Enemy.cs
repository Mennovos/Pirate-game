using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : MonoBehaviour, IEnemy
{
    [SerializeField] protected Animator animator;
    
    [Space]
    [SerializeField] protected Transform target;
    [SerializeField] private float attackDamage;
    [SerializeField] protected float scoreAmount;

    [Space] 
    [SerializeField, Min(0f)] private Vector2 scaleRange = Vector2.one;
    
    [Space]
    [SerializeField] private List<AudioClip> hitSounds;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<GameObject> hitParticles;

    [Space] 
    [SerializeField] private List<Renderer> meshRenderers;
    [SerializeField] private Color colorOnHit = new Color(1f, 0.5f, 0.5f);
    [SerializeField] private float colorOnHitDuration = 0.1f;

    private Coroutine colorResetCoroutine;

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
        OnHit();
        
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

    protected void OnHit()
    {
        if (colorResetCoroutine != null) StopCoroutine(colorResetCoroutine);

        foreach (Renderer meshRenderer in meshRenderers)
        {
            foreach (Material mat in meshRenderer.materials)
            {
                mat.color = colorOnHit;
            }
        }
        
        colorResetCoroutine = StartCoroutine(ColorResetCoroutine());
        
        if (hitParticles.Count > 0)
        {
            foreach (GameObject particle in hitParticles)
            {
                Instantiate(particle, transform.position, Quaternion.Euler(Vector3.zero));
            }
        }
        
        if (hitSounds.Count == 0 || !audioSource) return;
        
        int index = Random.Range(0, hitSounds.Count);
        AudioClip clip = hitSounds[index];
        
        audioSource.PlayOneShot(clip);
    }

    private IEnumerator ColorResetCoroutine()
    {
        yield return new WaitForSeconds(colorOnHitDuration);

        foreach (Renderer meshRenderer in meshRenderers)
        {
            foreach (Material mat in meshRenderer.materials)
            {
                mat.color = Color.white;
            }
        }
        
        colorResetCoroutine = null;
    }
}