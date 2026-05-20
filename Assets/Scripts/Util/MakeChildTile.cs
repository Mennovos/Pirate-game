using UnityEngine;

public class MakeTile : MonoBehaviour
{
    [SerializeField, Min(0)] private int size;
    
    [Space]
    [SerializeField] private Vector3 rightOffset;
    [SerializeField] private Vector3 forwardOffset;
    
    void Awake()
    {
        GameObject obj = transform.GetChild(0).gameObject;

        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (x == 0 && y == 0) continue;

                GameObject newObj = Instantiate(obj, transform);
                newObj.transform.localPosition = x * rightOffset + y * forwardOffset;
            }
        }
    }
}
