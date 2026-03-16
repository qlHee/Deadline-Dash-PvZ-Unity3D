using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("角色选择按钮")]
    public Button[] characterButtons;
    
    [Header("游戏模式按钮")]
    public Button endlessModeButton;
    public Button hellModeButton;
    public Button nightModeButton;
    public Button stormModeButton;
    public Button iceModeButton;
    public Button timeLimitModeButton;
    
    [Header("其他按钮")]
    public Button getNewCharacterButton;
    public Button gameRecordButton;
    public GameDataUI gameDataUI;
    
    [Header("按钮视觉效果")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.3f, 0.8f, 1f);
    public float selectedBorderWidth = 5f;
    public float normalBorderWidth = 2f;
    
    [Header("场景设置")]
    public string gameSceneName = "GameScene";
    
    private int currentSelectedIndex = 0;
    
    void Start()
    {
        InitializeCharacterButtons();
        InitializeGameModeButtons();
        InitializeOtherButtons();

        EnsureGameDataUI();
        
        int savedIndex = CharacterManager.Instance.GetSelectedCharacterIndex();
        SelectCharacter(savedIndex);
    }
    
    void InitializeCharacterButtons()
    {
        if (characterButtons == null || characterButtons.Length == 0)
        {
            Debug.LogError("Character buttons not assigned!");
            return;
        }
        
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            if (characterButtons[i] != null)
            {
                characterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(index));
            }
        }
    }
    
    void InitializeGameModeButtons()
    {
        if (endlessModeButton != null)
        {
            endlessModeButton.onClick.AddListener(OnEndlessModeClicked);
        }
        
        if (hellModeButton != null)
        {
            hellModeButton.onClick.AddListener(OnHellModeClicked);
        }
        
        if (nightModeButton != null)
        {
            nightModeButton.onClick.AddListener(OnNightModeClicked);
        }
        
        if (stormModeButton != null)
        {
            stormModeButton.onClick.AddListener(OnStormModeClicked);
        }

        if (iceModeButton != null)
        {
            iceModeButton.onClick.AddListener(OnIceModeClicked);
        }
        
        if (timeLimitModeButton != null)
        {
            timeLimitModeButton.onClick.AddListener(OnTimeLimitModeClicked);
        }
    }
    
    void InitializeOtherButtons()
    {
        if (getNewCharacterButton != null)
        {
            getNewCharacterButton.interactable = false;
        }
        
        if (gameRecordButton != null)
        {
            gameRecordButton.interactable = true;
        }
    }
    
    void OnCharacterButtonClicked(int index)
    {
        SelectCharacter(index);
        CharacterManager.Instance.SelectCharacter(index);
    }
    
    void SelectCharacter(int index)
    {
        if (characterButtons == null || index < 0 || index >= characterButtons.Length)
        {
            return;
        }
        
        currentSelectedIndex = index;
        
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] == null) continue;
            
            bool isSelected = (i == index);
            UpdateButtonVisual(characterButtons[i], isSelected);
        }
    }
    
    void UpdateButtonVisual(Button button, bool isSelected)
    {
        if (button == null) return;
        
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isSelected ? selectedColor : normalColor;
        }
        
        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
        }
        
        if (isSelected)
        {
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(selectedBorderWidth, selectedBorderWidth);
            outline.enabled = true;
        }
        else
        {
            outline.effectColor = Color.gray;
            outline.effectDistance = new Vector2(normalBorderWidth, normalBorderWidth);
            outline.enabled = true;
        }
    }
    
    void OnEndlessModeClicked()
    {
        StartGameWithMode(GameMode.Endless);
    }
    
    void OnHellModeClicked()
    {
        StartGameWithMode(GameMode.Hell);
    }
    
    void OnNightModeClicked()
    {
        StartGameWithMode(GameMode.Night);
    }
    
    void OnTimeLimitModeClicked()
    {
        StartGameWithMode(GameMode.TimeLimit);
    }

    void OnStormModeClicked()
    {
        StartGameWithMode(GameMode.Storm);
    }

    void OnIceModeClicked()
    {
        StartGameWithMode(GameMode.Ice);
    }
    
    void EnsureGameDataUI()
    {
        if (gameDataUI == null)
        {
            gameDataUI = GetComponent<GameDataUI>();
        }

        if (gameDataUI == null)
        {
            gameDataUI = gameObject.AddComponent<GameDataUI>();
        }

        if (gameDataUI != null)
        {
            gameDataUI.BindUI(gameRecordButton);
        }
    }
    
    void StartGameWithMode(GameMode mode)
    {
        CharacterData selectedCharacter = CharacterManager.Instance.GetSelectedCharacter();
        if (selectedCharacter == null)
        {
            Debug.LogError("No character selected!");
            return;
        }
        
        GameModeManager.Instance.SelectMode(mode);
        
        Debug.Log($"[主菜单] 开始{mode}模式，选中角色: {selectedCharacter.characterName}");
        
        SceneManager.LoadScene(gameSceneName);
    }
}
