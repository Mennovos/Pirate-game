using System;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] Camera cam;

    private void Start()
    {
        if (!cam) cam = Camera.main;
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
    }
}
