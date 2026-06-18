using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private static MenuManager instance;


    public static MenuManager Instance
    {
        get
        {
            if (!instance)
            {
                GameObject go = new GameObject("[Menu Manager]");
                instance = go.AddComponent<MenuManager>();
            }
            
            return instance;
        }
    }
    
    [SerializeField] private string[] levelSceneNames;
    
    public string[] LevelSceneNames => levelSceneNames;
    
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else if (instance != this)
        {
            Destroy(this);
        }
    }


    public void OnReturnToMain()
    {
        SceneManager.LoadScene("Title Screen");
    }


    public void OnStartLevel(int level)
    {
        SceneManager.LoadScene(levelSceneNames[level]);
    }


    public void OnQuitGame()
    {
        Application.Quit();
    }


    public void OnBossDeath(string bossType)
    {
        switch (bossType)
        {
            case "Pelican":
            {
                //TODO: after delay go to win screen (with score?)
                break;
            }

            case "Fishbear":
            {
                //TODO: after delay go to win screen (with score?)
                break;
            }
        }
    }
}
