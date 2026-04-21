using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Vector2 maxOffset;
    
    public IEnemy summon()
    {
        Vector2 pos = (Vector2)transform.position 
                      + new Vector2(Random.Range(-maxOffset.x, maxOffset.x), Random.Range(-maxOffset.y, maxOffset.y));
        
        GameObject obj = Instantiate(enemyPrefab, pos, Quaternion.Euler(Vector3.zero));

        if (obj.TryGetComponent(out IEnemy enemy))
        {
            return enemy;
        }
        
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.crimson;
        Gizmos.DrawWireCube(transform.position, 2 * maxOffset);
        Gizmos.DrawCube(transform.position, 2 * maxOffset);
    }
}
