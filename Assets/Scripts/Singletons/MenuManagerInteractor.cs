using UnityEngine;

public class MenuManagerInteractor : MonoBehaviour
{
    private MenuManager instance => MenuManager.Instance;

    public void OnReturnToMain()
    {
        instance.OnReturnToMain();
    }

    public void OnStartLevel(int level)
    {
        instance.OnStartLevel(level);
    }

    public void OnQuitGame()
    {
        instance.OnQuitGame();
    }
}
