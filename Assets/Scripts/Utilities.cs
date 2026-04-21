using UnityEngine;
using UnityEngine.SceneManagement;
public class Utilities : MonoBehaviour
{
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
}
