using System;
using UnityEngine;

public class TrackingTarget : MonoBehaviour
{
    [SerializeField] private float weight = 1f;

    public float Weight { get { return weight; } set { weight = value; } }

    private void Start()
    {
        foreach (CameraMovement cam in FindObjectsByType<CameraMovement>())
        {
            cam.UpdateTrackingTargets();
        }
    }

    private void OnDestroy()
    {
        foreach (CameraMovement cam in FindObjectsByType<CameraMovement>())
        {
            cam.UpdateTrackingTargets();
        }
    }
}
