using System;
using UnityEngine;

public class CameraShakeOnDestroy : MonoBehaviour
{
    [SerializeField] float strength;
    [SerializeField] float duration;

    private void OnDestroy()
    {
        if (Camera.main.TryGetComponent(out CameraMovement cameraMovement))
        {
            cameraMovement.CameraShake(strength, duration);
        }
    }
}
