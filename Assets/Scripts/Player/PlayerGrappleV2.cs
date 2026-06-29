using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerGrappleV2 : MonoBehaviour
{
    private Rigidbody rb;
    private Camera cam;

    [SerializeField] private GameObject grappleProjectilePrefab;
    
    private GrappleProjectile grappleProjectile;
    
    private Controls controls;
    
    public Rigidbody Rigidbody => rb;


    private readonly Plane plane = new(Vector3.forward, Vector3.zero);
    
    
    private void Awake()
    {
        TryGetComponent(out rb);
        cam = Camera.main;
        
        controls = new Controls();

        controls.Player.Grapple.started += OnGrapple;
        controls.Player.Grapple.canceled += OnGrapple;
        
        controls.Player.Attack.started += OnAttack;
        
        controls.Player.Enable();
    }

    private void OnDestroy()
    {
        controls.Player.Disable();
    }


    private void OnAttack(InputAction.CallbackContext context)
    {
        if (grappleProjectile)
        {
            Destroy(grappleProjectile.gameObject);
        }
    }

    private void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            if (grappleProjectile)
            {
                Destroy(grappleProjectile.gameObject);
            }
            else
            {
                if (Instantiate(grappleProjectilePrefab).TryGetComponent(out grappleProjectile))
                {
                    Vector2 mouse_pos = Input.mousePosition;

                    Ray ray = cam.ScreenPointToRay(mouse_pos);

                    if (plane.Raycast(ray, out float entry_distance))
                    {
                        Vector2 world_point = ray.GetPoint(entry_distance);

                        Vector3 to_target = world_point - (Vector2)transform.position;

                        grappleProjectile.Init(this, transform.position, to_target);
                    }
                }
            }
        }
        else
        {
            grappleProjectile.StartReturning();
        }
    }
}
