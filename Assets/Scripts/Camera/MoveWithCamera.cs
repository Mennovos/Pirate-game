using System;
using UnityEngine;

public class MoveWithCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    
    private float cameraDefaultDistance;
    
    private readonly Plane plane = new(Vector3.forward, Vector3.zero);

    private void Start()
    {
        cameraDefaultDistance = cameraTransform.position.z;
    }

    private void Update()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (plane.Raycast(ray, out float entryDistance))
        {
            Vector2 center = ray.GetPoint(entryDistance);

            transform.position = center;
            transform.localScale = Vector3.one * cameraTransform.position.z / cameraDefaultDistance;
        }
    }
}
