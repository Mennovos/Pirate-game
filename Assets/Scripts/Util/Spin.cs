using System;
using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] private float spinSpeed;

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}
