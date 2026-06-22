using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerMouseFollow : MonoBehaviour
{
    [Header("Bone / Auto Lookup")]
    [Tooltip("Optional: assign upper-body bone (spine/chest). If empty, will try Animator.GetBoneTransform(HumanBodyBones.UpperChest/Chest/Spine).")]
    [SerializeField] private Transform upperBodyBone;

    [Header("Follow settings")]
    [SerializeField, Min(0f)] private float smoothSpeed = 10f;

    // X axis (horizontal) control
    [Tooltip("Horizontal sensitivity multiplier (affects how far the upper body turns left/right).")]
    [SerializeField, Min(0f)] private float xSensitivity = 1f;
    [Tooltip("Invert horizontal axis.")]
    [SerializeField] private bool invertX = false;
    [SerializeField, Range(0f, 360f)] private float maxYaw = 60f;

    [Header("Pitch (vertical) control")]
    [SerializeField, Range(0f, 360f)] private float maxPitchUp = 90f;    // positive = look up (0..360 allowed)
    [SerializeField, Range(0f, 360f)] private float maxPitchDown = 45f;  // positive = look down (0..360 allowed)
    [SerializeField, Min(0.1f)] private float aimDistance = 30f;

    [Header("Flip settings")]
    [Tooltip("Enable flipping the whole player 180° when the mouse is in the top area (or above player if disabled).")]
    [SerializeField] private bool enableFlipOnMouseAbove = true;
    [Tooltip("If true: flip when mouse is in the TOP portion of the screen. If false: flip when mouse is above player's screen Y.")]
    [SerializeField] private bool useScreenTopThreshold = true;
    [Tooltip("Normalized screen Y (0..1) used when 'useScreenTopThreshold' is true. Example 0.66 = top third.")]
    [SerializeField, Range(0f, 1f)] private float screenTopThreshold = 0.66f;
    [Tooltip("Normalized vertical offset (0..0.5) added to player's screen Y before comparing to mouse Y (used when useScreenTopThreshold is false).")]
    [SerializeField, Range(0f, 0.5f)] private float screenYOffset = 0.02f;
    [Tooltip("When flipping, automatically invert X axis so horizontal aiming stays consistent.")]
    [SerializeField] private bool autoInvertXOnFlip = true;
    [Tooltip("When flipping, swap the up/down pitch limits (makes maxPitchDown behave as the upward limit).")]
    [SerializeField] private bool autoInvertYOnFlip = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Quaternion initialLocalRot;
    private Camera cam;
    private Animator animator;
    private bool isFlipped;

    private void Awake()
    {
        cam = Camera.main;
        animator = GetComponentInChildren<Animator>();

        if (upperBodyBone == null && animator != null && animator.isHuman)
        {
            upperBodyBone = animator.GetBoneTransform(HumanBodyBones.UpperChest)
                         ?? animator.GetBoneTransform(HumanBodyBones.Chest)
                         ?? animator.GetBoneTransform(HumanBodyBones.Spine);
        }

        if (upperBodyBone == null)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("upper") || n.Contains("chest") || n.Contains("spine") || n.Contains("torso"))
                {
                    upperBodyBone = t;
                    break;
                }
            }
        }

        if (upperBodyBone == null)
        {
            Debug.LogWarning("PlayerMouseFollow: upperBodyBone not found. Assign it in the inspector.");
            enabled = false;
            return;
        }

        initialLocalRot = upperBodyBone.localRotation;

        if (debugLogs) Debug.Log($"PlayerMouseFollow Awake: bone='{upperBodyBone.name}', cam={(cam? cam.name : "null")}");
    }

    // Use LateUpdate so the Animator's updates are applied first, then we override the bone.
    private void LateUpdate()
    {
        if (cam == null || upperBodyBone == null) return;

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;

        // Handle flipping first (so transform.forward used below matches current facing)
        if (enableFlipOnMouseAbove)
            UpdateFlipByMouseY(mousePos);

        Ray ray = cam.ScreenPointToRay(mousePos);
        Vector3 worldPoint = ray.origin + ray.direction * aimDistance;

        Vector3 toTarget = worldPoint - transform.position;
        if (toTarget.sqrMagnitude < 0.0001f) toTarget = transform.forward;

        // Convert direction into local space of the player/root
        Vector3 localDir = transform.InverseTransformDirection(toTarget.normalized);

        // Yaw: left/right (x over z)
        float xMult = invertX ? -1f : 1f;
        float yaw = Mathf.Atan2(localDir.x * xSensitivity * xMult, localDir.z) * Mathf.Rad2Deg;
        // yaw in [-180,180]; clamp to [-maxYaw,maxYaw] (maxYaw up to 360)
        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);

        // Pitch: up/down (y over z) - positive means up
        float pitch = Mathf.Atan2(localDir.y, Mathf.Max(0.0001f, localDir.z)) * Mathf.Rad2Deg;

        // Determine which pitch limits to use (swap when flipped if requested)
        float currentMaxUp = maxPitchUp;
        float currentMaxDown = maxPitchDown;
        if (isFlipped && autoInvertYOnFlip)
        {
            // swap meaning: treat maxPitchDown as the upward limit and maxPitchUp as the downward limit
            currentMaxUp = maxPitchDown;
            currentMaxDown = maxPitchUp;
        }

        // clamp with the selected up/down limits (limits can be up to 360)
        pitch = Mathf.Clamp(pitch, -currentMaxDown, currentMaxUp);

        // Compose local rotation: pitch around X, yaw around Y
        Quaternion localRot = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion targetLocal = initialLocalRot * localRot;

        // Smoothly blend the bone's local rotation
        upperBodyBone.localRotation = Quaternion.Slerp(upperBodyBone.localRotation, targetLocal, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        if (debugLogs)
        {
            float normY = (mousePos.y / Screen.height);
            Debug.DrawRay(cam.transform.position, (worldPoint - cam.transform.position).normalized * 5f, Color.cyan, 0.02f);
            Debug.Log($"Mouse normY={normY:F2} flipped={isFlipped} yaw={yaw:F1} pitch={pitch:F1} upLimit={currentMaxUp} downLimit={currentMaxDown}");
        }
    }

    private void UpdateFlipByMouseY(Vector2 mousePos)
    {
        bool mouseAbove;

        if (useScreenTopThreshold)
        {
            // Flip when mouse is in the top portion of the screen
            float normalizedY = mousePos.y / Screen.height;
            mouseAbove = normalizedY > screenTopThreshold;

            if (debugLogs)
                Debug.Log($"Flip check (top threshold): mouseYNorm={normalizedY:F2} threshold={screenTopThreshold:F2}");
        }
        else
        {
            // Flip when mouse is above the player's screen Y (old behavior)
            Vector3 playerScreenPos = cam.WorldToScreenPoint(transform.position);
            float offsetPixels = Screen.height * screenYOffset;
            mouseAbove = mousePos.y > playerScreenPos.y + offsetPixels;

            if (debugLogs)
                Debug.Log($"Flip check (above player): mouseY={mousePos.y:F0} playerY={playerScreenPos.y:F0} offset={offsetPixels:F0}");
        }

        if (mouseAbove && !isFlipped)
            StartFlip(true);
        else if (!mouseAbove && isFlipped)
            StartFlip(false);
    }

    private void StartFlip(bool flip)
    {
        if (flip == isFlipped) return;

        // Rotate root 180 degrees around Y to flip facing
        transform.Rotate(0f, 180f, 0f, Space.Self);

        // Optionally toggle invertX so horizontal input feels consistent after flip
        if (autoInvertXOnFlip) invertX = !invertX;

        // After changing parent orientation, recapture the current local rotation as the new baseline
        // This prevents jumps in aiming after flip.
        initialLocalRot = upperBodyBone.localRotation;

        isFlipped = flip;

        if (debugLogs) Debug.Log($"Player flipped={isFlipped}, new invertX={invertX}");
    }

    private void OnDisable()
    {
        if (upperBodyBone != null)
            upperBodyBone.localRotation = initialLocalRot;
    }
}