using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrapple : MonoBehaviour
{
    [SerializeField] private LayerMask grappleLayerMask;
    [SerializeField] private float maxGrappleDistance = 100f;
    [SerializeField] private float clickRadius = 1.5f; // <-- radius for wider click tolerance
    [SerializeField] private float GrappleSpeed = 5f;
    [SerializeField] private Transform Grapplepoint;
    [SerializeField] private List<Transform> grapplePoints = new List<Transform>();
    [SerializeField] private GameObject player;

    private Controls controls;
    private bool grappleOnCooldown;
    [SerializeField] private float grappleCooldown = 0.5f;

    private void Awake()
    {
        controls = new Controls();
        controls.Player.Grapple.performed += Grapple;
        grappleLayerMask = LayerMask.GetMask("Grappling");
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    // Input callback: fire grapple immediately at mouse world position
    public void Grapple(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (grappleOnCooldown) return;

        if (TryGetMouseRaycastHit(out RaycastHit hit))
        {

            // create a transient transform target at the hit point
            GameObject temp = new GameObject("GrapplePointTemp");
            temp.transform.position = hit.point;
            grapplePoints.Add(temp.transform);

            StartCoroutine(GrappleCooldownRoutine());
        }
        else
        {
            Debug.Log("Grapple: no valid target under mouse.");
        }
    }

    // Try raycast first, then a spherecast as a wider click/touch tolerance
    private bool TryGetMouseRaycastHit(out RaycastHit hit)
    {
        hit = default;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main camera not found.");
            return false;
        }

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(mousePos);

        // Visualize intended ray
        Debug.DrawRay(ray.origin, ray.direction * maxGrappleDistance, Color.red, 0.5f);

        // 1) Straight raycast (most precise)
        if (Physics.Raycast(ray, out hit, maxGrappleDistance, grappleLayerMask))
        {
            return true;
        }

        // 2) SphereCast fallback for larger click/tap tolerance
        if (clickRadius > 0f)
        {
            if (Physics.SphereCast(ray, clickRadius, out RaycastHit sphereHit, maxGrappleDistance, grappleLayerMask))
            {
                // Optionally prefer the closest hit point to ray origin
                hit = sphereHit;
                // Draw debug sphere at hit
                Debug.DrawLine(ray.origin, hit.point, Color.yellow, 0.5f);
                return true;
            }

            // If you want to see the spherecast in Scene view:
            // DebugExtension.DebugWireSphere(ray.GetPoint(maxGrappleDistance * 0.5f), Color.yellow, clickRadius);
        }

        return false;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < grapplePoints.Count; i++)
        {
            if (grapplePoints[i] == null) { grapplePoints.RemoveAt(i); i--; continue; }

            player.transform.position = Vector3.Lerp(player.transform.position, grapplePoints[i].position, GrappleSpeed);
            if (Vector3.Distance(player.transform.position, grapplePoints[i].position) < 2f)
            {
                if (grapplePoints[i].gameObject.name == "GrapplePointTemp")
                    Destroy(grapplePoints[i].gameObject);
                grapplePoints.RemoveAt(i);
                i--;
            }
        }
    }

    private System.Collections.IEnumerator GrappleCooldownRoutine()
    {
        grappleOnCooldown = true;
        yield return new WaitForSeconds(grappleCooldown);
        grappleOnCooldown = false;
    }
}
