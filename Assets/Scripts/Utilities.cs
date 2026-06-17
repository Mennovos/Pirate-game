using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class Utilities : MonoBehaviour
{
    [SerializeField] private AnyKeyPressedEvent pressAnyKey;

    private bool wavesStarted;

    [Header("Text for score")]
    [SerializeField] private TextMeshProUGUI ScoreAmount;

    [Space] 
    [SerializeField] private GameObject scoreParticle;
    [SerializeField] private Transform scoreParticleSpawn;
    
    private float score;
    private float scoreToAdd;
    
    private void Start()
    {
        ScoreAmount.text = "Score: " + score;
        wavesStarted = false;
        
        StartCoroutine(ScoreCoroutine());
    }

    void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame && !wavesStarted)
        {
            pressAnyKey.Invoke();
            
            wavesStarted = true;
        }

    }

    public void AddScore(float amount)
    {
        scoreToAdd +=amount;
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
       ScoreAmount.text = "Score: " + score;
    }


    private IEnumerator ScoreCoroutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => scoreToAdd > 0);
            
            score += 1;
            scoreToAdd--;

            if (scoreParticle && scoreParticleSpawn) 
                Instantiate(scoreParticle, scoreParticleSpawn.position, Quaternion.Euler(Vector3.zero));
            
            UpdateScoreUI();
        }
    }
    

    [Serializable] private class AnyKeyPressedEvent : UnityEvent {}
}
