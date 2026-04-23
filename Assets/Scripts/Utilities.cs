using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Utilities : MonoBehaviour
{
    [SerializeField] private GameObject Pressanykey;
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
        if(Keyboard.current.anyKey.wasPressedThisFrame)
        {
           Pressanykey.SetActive(false);
        }
    }
}
