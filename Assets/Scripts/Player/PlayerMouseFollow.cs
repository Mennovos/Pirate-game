using System.Collections;
using UnityEngine;

public class PlayerMouseFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform upperBodyBone;

    [Header("Lean / Aim Settings")]
    [SerializeField] private float leanAngle = -25f;
    [SerializeField] private float maxYaw = 45f;
    [SerializeField] private float blendInTime = 0.10f;
    [SerializeField] private float blendOutTime = 0.18f;
    [SerializeField] private float holdTime = 0.06f;

    private Quaternion initialLocalRot;
    private Coroutine running;

    private void Awake()
    {
        if (upperBodyBone == null)
        {
            Debug.LogWarning("UpperBodyAttack: upperBodyBone not assigned. Attempting to find 'Spine' or 'Chest' child.");
            // try common bone names
            upperBodyBone = transform.Find("Spine") ?? transform.Find("Chest") ?? transform;
        }

        initialLocalRot = upperBodyBone.localRotation;
    }

    /// <summary>
    /// Trigger the upper-body attack using a world-space aim direction.
    /// </summary>
    public void DoUpperAttack(Vector3 aimWorldDirection)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(UpperAttackRoutine(aimWorldDirection.normalized));
    }

    /// <summary>
    /// Convenience: compute aim direction from camera + screen position (mouse) and do attack.
    /// </summary>
    public void DoUpperAttackAtMouse(Camera cam, Vector2 screenPosition)
    {
        if (cam == null) return;
        Ray ray = cam.ScreenPointToRay(screenPosition);
        // aim direction from player to long-distance point on ray
        Vector3 targetPoint = ray.origin + ray.direction * 50f;
        Vector3 dir = (targetPoint - transform.position);
        dir.y = 0f; // keep aim rotation horizontal only for yaw
        if (dir.sqrMagnitude < 0.001f)
            dir = transform.forward;
        DoUpperAttack(dir.normalized);
    }

    private IEnumerator UpperAttackRoutine(Vector3 aimDir)
    {
        // Compute yaw angle between player's forward and aimDir
        Vector3 forwardProj = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 aimProj = Vector3.ProjectOnPlane(aimDir, Vector3.up).normalized;
        float yaw = Vector3.SignedAngle(forwardProj, aimProj, Vector3.up);
        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);

        // target local rotation: initial rotated by yaw around local up, then pitched by leanAngle around local X
        Quaternion targetLocal = initialLocalRot * Quaternion.Euler(leanAngle, yaw, 0f);

        // Blend in
        float t = 0f;
        while (t < blendInTime)
        {
            float lerp = blendInTime > 0f ? (t / blendInTime) : 1f;
            upperBodyBone.localRotation = Quaternion.Slerp(initialLocalRot, targetLocal, Mathf.SmoothStep(0f, 1f, lerp));
            t += Time.deltaTime;
            yield return null;
        }

        upperBodyBone.localRotation = targetLocal;

        // Hold attack pose briefly
        if (holdTime > 0f) yield return new WaitForSeconds(holdTime);

        // Blend back to neutral
        t = 0f;
        Quaternion from = upperBodyBone.localRotation;
        while (t < blendOutTime)
        {
            float lerp = blendOutTime > 0f ? (t / blendOutTime) : 1f;
            upperBodyBone.localRotation = Quaternion.Slerp(from, initialLocalRot, Mathf.SmoothStep(0f, 1f, lerp));
            t += Time.deltaTime;
            yield return null;
        }

        upperBodyBone.localRotation = initialLocalRot;
        running = null;
    }
}