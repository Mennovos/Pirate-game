using System;
using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] private Vector3 spinSpeed;

    private void Update()
    {
        transform.Rotate(spinSpeed * Time.deltaTime);
    }
}
