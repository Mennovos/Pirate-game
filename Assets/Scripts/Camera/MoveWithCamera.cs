using System;
using UnityEngine;

public class MoveWithCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    
    private float cameraDefaultDistance;

    private void Start()
    {
        cameraDefaultDistance = cameraTransform.position.z;
    }

    private void Update()
    {
        transform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, 0);
        transform.localScale = Vector3.one * cameraTransform.position.z / cameraDefaultDistance;
    }
}
