using UnityEngine;

public class FishbearEnemy : Enemy
{
    private static readonly int animator_State = Animator.StringToHash("State");
    
    private enum FishbearState
    {
        IDLE,
        CRAB_ATTACK,
        CRAB_ATTACK_WINDUP,
        CRAB_ATTACK_WINDDOWN,
        DASH_ATTACK,
        DASH_ATTACK_WINDUP,
        DASH_ATTACK_WINDDOWN,
        CHOMP_ATTACK,
        DEFEATED
    }
    
    private FishbearState state = FishbearState.IDLE;
    
    [Space]
    [SerializeField] private float maxHealth;
    private float health;
    
    [SerializeField] private HealthBar healthBar;
    
    [Space]
    [SerializeField] private float timeToDestroyAfterDefeat = 15f;
    [SerializeField] private Vector3 velocityAfterDefeat = new(0f, 5f, 0f);

    private new void Awake()
    {
        base.Awake();

        health = maxHealth;
    }
    
    private void Update()
    {
        if (animator)
        {
            animator.SetInteger(animator_State, (int)state);
        }
    }
    
    public override void attack(Vector2 impulse)
    {
        TimeManager.Instance.AddHitstop(0.2f);
        OnHit();
        
        health -= impulse.magnitude;
        
        health = Mathf.Clamp(health, 0f, maxHealth);
        
        healthBar.setHealth(health/maxHealth);
        
        if (health <= 0f) kill();
    }

    public override void kill()
    {
        if (health <= 0f)
        {
            isAlive = false;
            
            Destroy(gameObject, timeToDestroyAfterDefeat);

            state = FishbearState.DEFEATED;
            
            rb.useGravity = true;
            rb.excludeLayers = ~0;

            rb.constraints = RigidbodyConstraints.FreezeRotation;

            rb.linearVelocity = velocityAfterDefeat;

            if (TryGetComponent(out TrackingTarget trackingTarget))
            {
                trackingTarget.Weight = 0f;
            }
            
            utilities.AddScore(scoreAmount);
        }
    }
}
