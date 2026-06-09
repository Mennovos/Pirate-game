using UnityEngine;

public class FishbearEnemy : Enemy
{
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
            
            //TODO: set state
            
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
