using System;
using UnityEngine;
using UnityEngine.Events;

public class EventOnDestroy : MonoBehaviour
{
    [SerializeField] private OnDestroyEvent onTriggerEnter;
    
    private void OnDestroy()
    {
        onTriggerEnter.Invoke();
    }
    
    [Serializable] private class OnDestroyEvent : UnityEvent {}
}
