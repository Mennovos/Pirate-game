using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerMouseFollow : MonoBehaviour
{
    [SerializeField] private Transform upperBodyBone;

    [SerializeField] private float rotationSmoothing = 5f;
    
    private Camera cam;
    private Quaternion initialLocalRot;
    
    
    private readonly Plane plane = new(Vector3.forward, Vector3.zero);
    

    private void Awake()
    {
        cam = Camera.main;

        if (upperBodyBone == null)
        {
            Debug.LogWarning("PlayerMouseFollow: upperBodyBone not set. Assign it in the inspector.");
            enabled = false;
            return;
        }

        initialLocalRot = upperBodyBone.localRotation;
    }

    private void LateUpdate()
    {
        if (!cam || !upperBodyBone) return;

        Vector2 mouse_pos = Input.mousePosition;

        Ray ray = cam.ScreenPointToRay(mouse_pos);

        if (plane.Raycast(ray, out float entry_distance))
        {
            Vector2 world_point = ray.GetPoint(entry_distance);

            Vector3 to_target = world_point - (Vector2)upperBodyBone.position;

            upperBodyBone.rotation = Quaternion.Slerp(upperBodyBone.rotation, 
                Quaternion.LookRotation(to_target, Vector3.up), Time.deltaTime * rotationSmoothing);
        }
    }

    private void OnDisable()
    {
        if (upperBodyBone != null)
            upperBodyBone.localRotation = initialLocalRot;
    }
}