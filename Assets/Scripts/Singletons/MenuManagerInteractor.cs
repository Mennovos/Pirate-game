using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerInteractor : MonoBehaviour
{
    private MenuManager instance => MenuManager.Instance;


    [SerializeField] private Image levelPreview;
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button prevLevelButton;
    
    [SerializeField] private Sprite[] levelPreviewSprites;
    [SerializeField] private string[] levelNames;
    
    [SerializeField] private RectTransform winScreen;
    
    private int selectedLevelIndex;


    private void Start()
    {
        instance.OnWin += OnWin;
    }

    private void OnDestroy()
    {
        instance.OnWin -= OnWin;
    }


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
        levelName.text = levelNames[selectedLevelIndex];
        
        nextLevelButton.gameObject.SetActive(selectedLevelIndex + 1 < instance.LevelSceneNames.Length);
        prevLevelButton.gameObject.SetActive(selectedLevelIndex > 0);
    }


    private void OnWin()
    {
        winScreen.gameObject.SetActive(true);
    }
}
