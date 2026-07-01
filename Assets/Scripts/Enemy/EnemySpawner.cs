using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Vector2 maxOffset;
    
    public IEnemy summon()
    {
        Vector2 pos = (Vector2)transform.position 
                      + new Vector2(
                          Random.Range(-maxOffset.x * transform.lossyScale.x, maxOffset.x * transform.lossyScale.x), 
                          Random.Range(-maxOffset.y * transform.lossyScale.y, maxOffset.y * transform.lossyScale.y));
        
        GameObject obj = Instantiate(enemyPrefab, pos, Quaternion.Euler(Vector3.zero));

        obj.SetActive(true);
        
        if (obj.TryGetComponent(out IEnemy enemy))
        {
            return enemy;
        }
        
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.crimson;
        Gizmos.DrawWireCube(transform.position, 2 * maxOffset * transform.lossyScale);
        Gizmos.DrawCube(transform.position, 2 * maxOffset * transform.lossyScale);
    }
}
