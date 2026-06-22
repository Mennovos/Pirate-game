using System;
using UnityEngine;

public class ActiveOnPause : MonoBehaviour
{
    [SerializeField] private bool invert;

    private void Start()
    {
        TimeManager.Instance.OnTogglePause += OnTogglePause;
        
        OnTogglePause(TimeManager.Instance.IsPaused);
    }

    private void OnTogglePause(bool paused)
    {
        gameObject.SetActive(paused ^ invert);
    }
}
