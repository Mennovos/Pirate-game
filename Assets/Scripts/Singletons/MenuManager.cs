using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private static MenuManager instance;
    [SerializeField] private GameObject menuManger;

    private Controls controls;

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
        controls = new Controls();
        controls.Player.Pause.performed += ctx => OnPause();
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

    public void OnPause()
    {
        
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
