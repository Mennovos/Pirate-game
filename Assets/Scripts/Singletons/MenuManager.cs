using UnityEngine;
using UnityEngine.InputSystem;
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
        controls.Enable();
        controls.Player.Pause.performed += OnPause;
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

    public void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("Pause button pressed");
        //TimeManager.Instance.SetPaused(!TimeManager.Instance.IsPaused);
        //Time.timeScale = TimeManager.Instance.IsPaused ? 0f : 1f;
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
