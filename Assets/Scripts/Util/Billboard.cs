using System;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] Camera cam;

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
    }
}
