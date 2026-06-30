using System;
using UnityEngine;
using UnityEngine.Events;

public class EventOnTriggerEnter : MonoBehaviour
{
    [SerializeField] private bool triggerOnce;
    
    [SerializeField] private OnTriggerEnterEvent onTriggerEnter;
    
    private bool triggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        
        onTriggerEnter.Invoke();
        
        if (triggerOnce) triggered = true;
    }
    
    [Serializable] private class OnTriggerEnterEvent : UnityEvent {}
}
