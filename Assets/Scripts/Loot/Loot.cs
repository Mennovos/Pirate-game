using System;
using UnityEngine;

public abstract class Loot : MonoBehaviour, IPickup
{
    [SerializeField] private GameObject indicatorPrefab;
    
    public RectTransform indicatorTransform;

    private GameObject indicator;
    
    private Transform mainCamera;
    
    
    private readonly Plane plane = new(Vector3.forward, Vector3.zero);

    
    private void Update()
    {
        if (indicator)
        {
            Ray ray = new Ray(mainCamera.position, mainCamera.forward);

            if (plane.Raycast(ray, out float entry_distance))
            {
                Vector2 center = ray.GetPoint(entry_distance);
                
                float rot = Mathf.Atan2(transform.position.y - center.y, transform.position.x -  center.x);
            
                indicator.transform.rotation = Quaternion.Euler(0, 0, rot * Mathf.Rad2Deg);
            }
        }
        
        if (transform.position.y < -20f) Destroy(gameObject);
    }


    private void Start()
    {
        mainCamera = Camera.main?.transform;
        
        if (indicatorPrefab && indicatorTransform)
            indicator = Instantiate(indicatorPrefab, indicatorTransform);
    }

    private void OnDestroy()
    {
        if (indicator)
            Destroy(indicator);
    }


    public abstract void onGrab(GameObject grabber);
}
