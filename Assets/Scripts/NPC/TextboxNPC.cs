using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextboxNPC : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textbox;
    [SerializeField, TextArea(5, 10)] private List<string> texts;
    
    [Space]
    [SerializeField] private AudioSource typingSoundSource;
    [SerializeField] private AudioClip typingSoundClip;

    [Space] 
    [SerializeField, Min(0f)] private float timePerCharacter = 0.1f;
    [SerializeField, Min(0f)] private float timeAddPerStop = 0.4f;
    [SerializeField, Min(0f)] private float timeAfterText = 5f;
    
    [Space]
    [SerializeField] private EndTextEvent onEndText;

    private void Start()
    {
        StartCoroutine(TextboxCoroutine());
    }

    private IEnumerator TextboxCoroutine()
    {
        textbox.text = "";
        
        //TODO: appear anim?

        foreach (string text in texts)
        {
            textbox.text = "";
            
            for (int i = 0; i < text.Length; i++)
            {
                textbox.text += text[i];
                
                if (text[i] != ' ' && typingSoundSource && typingSoundClip) 
                    typingSoundSource.PlayOneShot(typingSoundClip);
                
                if (".?!".Contains(text[i])) yield return new WaitForSecondsRealtime(timeAddPerStop);
                
                yield return new WaitForSecondsRealtime(timePerCharacter);
            }
            
            yield return new WaitForSecondsRealtime(timeAfterText);
        }
        
        //TODO: disappear anim?
        
        onEndText.Invoke();
        
        Destroy(gameObject);
    }
    
    
    [Serializable] private class EndTextEvent : UnityEvent {}
}
