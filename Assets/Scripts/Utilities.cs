using System;
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

    private float score;
    private void Start()
    {
        ScoreAmount.text = "Score: " + score;
        wavesStarted = false;
    }
    public void Home()
    {
        SceneManager.LoadScene(0);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1f;
    }
    public void QuitGame()
    {
        Application.Quit();
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
        amount += score;
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
       ScoreAmount.text = "Score: " + score;
    }

    [Serializable] private class AnyKeyPressedEvent : UnityEvent {}
}
