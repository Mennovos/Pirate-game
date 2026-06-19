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
    [SerializeField, Range(0f, 180f)] private float maxYaw = 60f;

    [Header("Pitch (vertical) control")]
    [SerializeField, Range(0f, 180f)] private float maxPitchUp = 90f;    // positive = look up
    [SerializeField, Range(0f, 180f)] private float maxPitchDown = 45f;  // positive = look down
    [SerializeField, Min(0.1f)] private float aimDistance = 30f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Quaternion initialLocalRot;
    private Camera cam;
    private Animator animator;

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
        Ray ray = cam.ScreenPointToRay(mousePos);
        Vector3 worldPoint = ray.origin + ray.direction * aimDistance;

        Vector3 toTarget = worldPoint - transform.position;
        if (toTarget.sqrMagnitude < 0.0001f) toTarget = transform.forward;

        // Convert direction into local space of the player/root
        Vector3 localDir = transform.InverseTransformDirection(toTarget.normalized);

        // Yaw: left/right (x over z)
        float yaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);

        // Pitch: up/down (y over z) - positive means up
        float pitch = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
        // clamp with separate up/down limits
        pitch = Mathf.Clamp(pitch, -maxPitchDown, maxPitchUp);

        // Compose local rotation: pitch around X, yaw around Y
        Quaternion localRot = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion targetLocal = initialLocalRot * localRot;

        // Smoothly blend the bone's local rotation
        upperBodyBone.localRotation = Quaternion.Slerp(upperBodyBone.localRotation, targetLocal, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        if (debugLogs)
        {
            Debug.DrawRay(cam.transform.position, (worldPoint - cam.transform.position).normalized * 5f, Color.cyan, 0.02f);
        }
    }

    private void OnDisable()
    {
        if (upperBodyBone != null)
            upperBodyBone.localRotation = initialLocalRot;
    }
}