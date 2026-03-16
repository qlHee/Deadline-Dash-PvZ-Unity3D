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

    [Header("解锁设置")]
    public string puzzleExecutablePath;
    public bool refreshUnlocksOnFocus = true;
    
    [Header("按钮视觉效果")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.3f, 0.8f, 1f);
    public float selectedBorderWidth = 5f;
    public float normalBorderWidth = 2f;
    public Color lockedColor = new Color(0.6f, 0.6f, 0.6f, 0.85f);
    
    [Header("场景设置")]
    public string gameSceneName = "GameScene";
    
    private int currentSelectedIndex = 0;
    
    void Start()
    {
        InitializeCharacterButtons();
        InitializeGameModeButtons();
        InitializeOtherButtons();

        EnsureGameDataUI();
        RefreshCharacterUnlocks();
        
        int savedIndex = CharacterManager.Instance.GetSelectedCharacterIndex();
        SelectCharacter(savedIndex);
    }

    void OnEnable()
    {
        EnsureGameDataUI();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && refreshUnlocksOnFocus)
        {
            RefreshCharacterUnlocks();
        }
        if (hasFocus)
        {
            EnsureGameDataUI();
        }
    }
    
    void InitializeCharacterButtons()
    {
        if (characterButtons == null || characterButtons.Length == 0)
        {
            Debug.LogError("角色按钮未绑定！");
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
            getNewCharacterButton.interactable = true;
            getNewCharacterButton.onClick.RemoveListener(OnGetNewCharacterClicked);
            getNewCharacterButton.onClick.AddListener(OnGetNewCharacterClicked);
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

        bool isLocked = !button.interactable;
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            if (isLocked)
            {
                buttonImage.color = lockedColor;
            }
            else
            {
                buttonImage.color = isSelected ? selectedColor : normalColor;
            }
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

    void OnGetNewCharacterClicked()
    {
        string path = puzzleExecutablePath;
        if (string.IsNullOrEmpty(path))
        {
            path = SharedUnlockIO.GuessDefaultPuzzlePath();
        }

        if (!SharedUnlockIO.LaunchPuzzle(path))
        {
            Debug.LogWarning("无法启动解密程序，请检查可执行文件路径。");
        }
    }
    
    void EnsureGameDataUI()
    {
        if (gameRecordButton == null)
        {
            GameObject buttonObj = GameObject.Find("GameRecordsButton");
            if (buttonObj != null)
            {
                gameRecordButton = buttonObj.GetComponent<Button>();
            }
        }

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

    void RefreshCharacterUnlocks()
    {
        CharacterManager manager = CharacterManager.Instance;
        if (manager == null || manager.characters == null)
        {
            return;
        }

        int firstUnlocked = -1;
        for (int i = 0; i < manager.characters.Length && i < characterButtons.Length; i++)
        {
            CharacterData data = manager.characters[i];
            bool unlocked = data != null && SharedUnlockIO.IsCharacterUnlocked(data.characterID);
            if (unlocked && firstUnlocked < 0)
            {
                firstUnlocked = i;
            }

            if (characterButtons[i] != null)
            {
                SetButtonLockState(characterButtons[i], unlocked);
            }
        }

        if (firstUnlocked < 0)
        {
            firstUnlocked = 0;
        }

        int current = manager.GetSelectedCharacterIndex();
        if (current < 0 || current >= manager.characters.Length || !SharedUnlockIO.IsCharacterUnlocked(manager.characters[current].characterID))
        {
            SelectCharacter(firstUnlocked);
            manager.SelectCharacter(firstUnlocked);
        }
    }

    void SetButtonLockState(Button button, bool unlocked)
    {
        if (button == null) return;

        button.interactable = unlocked;
        CanvasGroup group = button.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = button.gameObject.AddComponent<CanvasGroup>();
        }
        group.alpha = unlocked ? 1f : 0.5f;
        group.interactable = unlocked;
        group.blocksRaycasts = unlocked;
    }
    
    void StartGameWithMode(GameMode mode)
    {
        CharacterData selectedCharacter = CharacterManager.Instance.GetSelectedCharacter();
        if (selectedCharacter == null)
        {
            Debug.LogError("未选择角色！");
            return;
        }
        
        GameModeManager.Instance.SelectMode(mode);
        
        Debug.Log($"[主菜单] 开始{mode}模式，选中角色: {selectedCharacter.characterName}");
        
        SceneManager.LoadScene(gameSceneName);
    }
}
