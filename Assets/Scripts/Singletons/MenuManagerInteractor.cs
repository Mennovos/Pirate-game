using UnityEngine;
using UnityEngine.UI;

public class MenuManagerInteractor : MonoBehaviour
{
    private MenuManager instance => MenuManager.Instance;


    [SerializeField] private Image levelPreview;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button prevLevelButton;
    
    [SerializeField] private Sprite[] levelPreviewSprites;
    
    private int selectedLevelIndex;
    

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


    public void StartSelectedLevel()
    {
        instance.OnStartLevel(selectedLevelIndex);
    }

    public void SelectNextLevel()
    {
        selectedLevelIndex++;
        // maybe clamp? should be impossible to overflow though (unless set up wrong)
        
        UpdateLevelPreview();
    }

    public void SelectPrevLevel()
    {
        selectedLevelIndex--;
        // maybe clamp? should be impossible to overflow though (unless set up wrong)
        
        UpdateLevelPreview();
    }

    public void UpdateLevelPreview()
    {
        levelPreview.sprite = levelPreviewSprites[selectedLevelIndex];
        
        nextLevelButton.gameObject.SetActive(selectedLevelIndex + 1 < instance.LevelSceneNames.Length);
        prevLevelButton.gameObject.SetActive(selectedLevelIndex > 0);
    }
}
