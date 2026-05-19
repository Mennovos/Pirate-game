using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Utilities : MonoBehaviour
{
    [SerializeField] private AnyKeyPressedEvent pressAnyKey;

    private bool wavesStarted;

    private void Start()
    {
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
        if(Keyboard.current.anyKey.wasPressedThisFrame && !wavesStarted)
        {
            pressAnyKey.Invoke();
            
            wavesStarted = true;
        }
    }

    [Serializable] private class AnyKeyPressedEvent : UnityEvent {}
}
