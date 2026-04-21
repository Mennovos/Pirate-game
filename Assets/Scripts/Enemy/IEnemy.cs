using UnityEngine;

public interface IEnemy
{
    public bool isDead();

    public void attack(Vector2 impulse);
}
