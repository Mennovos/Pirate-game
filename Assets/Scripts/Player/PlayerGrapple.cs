using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrapple : MonoBehaviour
{
    [SerializeField] private LayerMask grappleLayerMask;
    [SerializeField] private float maxGrappleDistance = 100f;
    [SerializeField] private float clickRadius = 1.5f; // wider click tolerance
    [SerializeField] private float GrappleSpeed = 5f;
    [SerializeField] private Transform Grapplepoint;
    [SerializeField] private List<Transform> grapplePoints = new List<Transform>();
    [SerializeField] private GameObject player;

    [Header("Enemy pull settings")]
    [SerializeField, Min(0f)] private float enemyPullForce = 8f;        // acceleration applied to enemy while pulling
    [SerializeField, Min(0f)] private float enemyPullDuration = 0.6f;   // how long to pull
    [SerializeField, Min(0f)] private float enemyStopDistance = 1.5f;   // stop pulling when close enough

    private Controls controls;
    private bool grappleOnCooldown;
    [SerializeField] private float grappleCooldown = 0.5f;

    // track which enemies are currently being pulled
    private readonly List<Rigidbody> pulledEnemies = new List<Rigidbody>();

    private void Awake()
    {
        controls = new Controls();
        controls.Player.Grapple.performed += Grapple;
        grappleLayerMask = LayerMask.GetMask("Grappling");
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    public void Grapple(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (grappleOnCooldown) return;

        if (TryGetMouseRaycastHit(out RaycastHit hit))
        {
            Debug.Log($"Grapple hit: {hit.collider.name} at {hit.point}");

            // If this is an enemy (has Enemy component) try to pull it
            if (hit.collider.TryGetComponent(out Enemy enemy))
            {
                if (hit.collider.attachedRigidbody != null)
                {
                    StartCoroutine(PullEnemyRoutine(hit.collider.attachedRigidbody));
                }
                else
                {
                    // fallback: move transform if no rigidbody present
                    GameObject temp = new GameObject("GrapplePointTemp");
                    temp.transform.position = hit.point;
                    grapplePoints.Add(temp.transform);
                }
            }
            else
            {
                // Normal grapple target (non-enemy)
                GameObject temp = new GameObject("GrapplePointTemp");
                temp.transform.position = hit.point;
                grapplePoints.Add(temp.transform);
            }

            StartCoroutine(GrappleCooldownRoutine());
        }
        else
        {
            Debug.Log("Grapple: no valid target under mouse.");
        }
    }

    // Pull the enemy toward the player using physics when possible
    private IEnumerator PullEnemyRoutine(Rigidbody enemyRb)
    {
        if (enemyRb == null) yield break;
        if (pulledEnemies.Contains(enemyRb)) yield break;

        pulledEnemies.Add(enemyRb);

        float timer = 0f;
        // If the enemy's rigidbody is currently kinematic, we'll lerp its transform instead.
        bool wasKinematic = enemyRb.isKinematic;

        // If enemy is kinematic, ensure we do not interfere with other code by leaving isKinematic as-is.
        while (timer < enemyPullDuration)
        {
            if (enemyRb == null) break;

            Vector3 toPlayer = player.transform.position - enemyRb.position;
            float dist = toPlayer.magnitude;
            if (dist <= enemyStopDistance) break;

            Vector3 dir = toPlayer.normalized;

            if (enemyRb.isKinematic)
            {
                // smooth transform move for kinematic bodies
                float lerpT = (enemyPullForce * Time.fixedDeltaTime) / (dist + 0.001f);
                enemyRb.transform.position = Vector3.Lerp(enemyRb.transform.position, player.transform.position, Mathf.Clamp01(lerpT));
                yield return new WaitForFixedUpdate();
            }
            else
            {
                // apply acceleration toward player
                enemyRb.AddForce(dir * enemyPullForce, ForceMode.Acceleration);
                yield return new WaitForFixedUpdate();
            }

            timer += Time.fixedDeltaTime;
        }

        // optional: dampen velocity when finished
        if (enemyRb != null && !enemyRb.isKinematic)
        {
            enemyRb.linearVelocity = Vector3.zero;
            enemyRb.angularVelocity = Vector3.zero;
        }

        pulledEnemies.Remove(enemyRb);
    }

    // Try raycast first, then spherecast for larger click tolerance
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

        Debug.DrawRay(ray.origin, ray.direction * maxGrappleDistance, Color.red, 0.5f);

        if (Physics.Raycast(ray, out hit, maxGrappleDistance, grappleLayerMask))
            return true;

        if (clickRadius > 0f)
        {
            if (Physics.SphereCast(ray, clickRadius, out RaycastHit sphereHit, maxGrappleDistance, grappleLayerMask))
            {
                hit = sphereHit;
                Debug.DrawLine(ray.origin, hit.point, Color.yellow, 0.5f);
                return true;
            }
        }

        return false;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < grapplePoints.Count; i++)
        {
            if (grapplePoints[i] == null) { grapplePoints.RemoveAt(i); i--; continue; }

            player.transform.position = Vector3.Lerp(player.transform.position, grapplePoints[i].position, Time.deltaTime * GrappleSpeed);
            if (Vector3.Distance(player.transform.position, grapplePoints[i].position) < 1.5f)
            {
                if (grapplePoints[i].gameObject.name == "GrapplePointTemp")
                    Destroy(grapplePoints[i].gameObject);
                grapplePoints.RemoveAt(i);
                i--;
            }
        }
    }

    private IEnumerator GrappleCooldownRoutine()
    {
        grappleOnCooldown = true;
        yield return new WaitForSeconds(grappleCooldown);
        grappleOnCooldown = false;
    }
}
