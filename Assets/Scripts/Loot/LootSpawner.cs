using UnityEngine;
using Random = UnityEngine.Random;

public class LootSpawner : MonoBehaviour
{
    [SerializeField] private GameObject lootPrefab;
    [SerializeField] private Vector2 maxOffset;
    [SerializeField] private RectTransform indicatorTransform;
    
    public void summon()
    {
        Vector2 pos = (Vector2)transform.position 
                      + new Vector2(Random.Range(-maxOffset.x, maxOffset.x), Random.Range(-maxOffset.y, maxOffset.y));
        
        GameObject go = Instantiate(lootPrefab, pos, Quaternion.Euler(Vector3.zero));
        
        if (go.TryGetComponent(out Loot loot)) loot.indicatorTransform = indicatorTransform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gold;
        Gizmos.DrawWireCube(transform.position, 2 * maxOffset);
        Gizmos.DrawCube(transform.position, 2 * maxOffset);
    }
}
