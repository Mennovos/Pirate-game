using UnityEngine;

public class PelicanEnemy : Enemy
{
    private enum PelicanState
    {
        IDLE
    }
    
    private PelicanState state;
    
    [SerializeField] private float maxHealth;
    private float health;
    
    private void Awake()
    {
        base.Awake();
        
        state = PelicanState.IDLE;
        health = maxHealth;
    }

    public override void attack(Vector2 impulse)
    {
        health -= impulse.magnitude;
        
        health = Mathf.Clamp(health, 0f, maxHealth);
        
        if (health <= 0f) kill();
    }
}
