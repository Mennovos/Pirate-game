using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyWhenAllDestroyed : MonoBehaviour
{
    [SerializeField] private List<GameObject> others;

    private Coroutine destroyCoroutine;
    
    private void Start()
    {
        destroyCoroutine = StartCoroutine(CheckDestroy());
    }

    private void OnDestroy()
    {
        StopCoroutine(destroyCoroutine);
    }

    private IEnumerator CheckDestroy()
    {
        while (true)
        {
            if (others.All(o => !o))
            {
                Destroy(gameObject);
            }
            
            yield return null;
        }
    }
}
