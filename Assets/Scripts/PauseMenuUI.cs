using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    static Sprite onePixelSprite;
    static Sprite GetOnePixelSprite()
    {
        if (onePixelSprite == null)
        {
            onePixelSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }
        return onePixelSprite;
    }

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.Escape;

    [Header("UI")]
    public GameObject menuRoot;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Slider bgmSlider;

    [Header("Behavior")]
    public bool autoBindFromMenuRoot = true;
    public bool autoBuildMenu = false;
    public bool applyDefaultBgmOnStart = true;

    [Header("Layout")]
    public Vector2 panelSize = new Vector2(520f, 420f);
    public Vector2 buttonSize = new Vector2(360f, 60f);

    [Header("Audio")]
    [Range(0f, 1f)]
    public float defaultBgmVolume = 1f;

    private GameManager gameManager;
    private MusicManager musicManager;
    private bool isPaused;
    private float cachedTimeScale = 1f;
    private TMP_FontAsset fallbackFont;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        musicManager = FindObjectOfType<MusicManager>();

        if (!EnsureMenuRoot())
        {
            return;
        }

        if (autoBindFromMenuRoot)
        {
            AutoBindFromMenuRoot();
        }

        if (applyDefaultBgmOnStart)
        {
            ApplyDefaultBgmVolume();
        }

        menuRoot.SetActive(false);
        HookupUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePause();
        }
    }

    void HookupUI()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }

        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.wholeNumbers = false;
            bgmSlider.value = GetCurrentBgmVolume();
            bgmSlider.onValueChanged.AddListener(SetBgmVolume);
        }
        else
        {
            Debug.LogWarning("PauseMenuUI: bgmSlider not assigned.");
        }
    }

    bool EnsureMenuRoot()
    {
        if (menuRoot == null)
        {
            if (autoBuildMenu)
            {
                BuildMenu();
            }
            else
            {
                Debug.LogWarning("PauseMenuUI: menuRoot not assigned. Please link your manual UI.");
            }
        }
        return menuRoot != null;
    }

    void AutoBindFromMenuRoot()
    {
        if (menuRoot == null) return;

        if (resumeButton == null)
        {
            resumeButton = FindButtonByName("ResumeButton");
        }

        if (restartButton == null)
        {
            restartButton = FindButtonByName("RestartButton");
        }

        if (mainMenuButton == null)
        {
            mainMenuButton = FindButtonByName("MainMenuButton");
        }

        if (bgmSlider == null)
        {
            bgmSlider = menuRoot.GetComponentInChildren<Slider>(true);
        }
    }

    Button FindButtonByName(string buttonName)
    {
        if (menuRoot == null) return null;
        Button[] buttons = menuRoot.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button btn = buttons[i];
            if (btn != null && btn.name == buttonName)
            {
                return btn;
            }
        }
        return null;
    }

    TMP_FontAsset GetFallbackFont()
    {
        if (fallbackFont != null)
        {
            return fallbackFont;
        }

        if (TMP_Settings.defaultFontAsset != null)
        {
            fallbackFont = TMP_Settings.defaultFontAsset;
            return fallbackFont;
        }

        TextMeshProUGUI existing = FindObjectOfType<TextMeshProUGUI>();
        if (existing != null)
        {
            fallbackFont = existing.font;
        }

        return fallbackFont;
    }

    float GetCurrentBgmVolume()
    {
        if (gameManager != null)
        {
            return gameManager.GetBackgroundMusicVolume();
        }

        if (musicManager != null)
        {
            return musicManager.GetVolume();
        }

        return defaultBgmVolume;
    }

    void ApplyDefaultBgmVolume()
    {
        float target = Mathf.Clamp01(defaultBgmVolume);
        if (gameManager != null)
        {
            gameManager.SetBackgroundMusicVolume(target);
        }

        if (musicManager != null)
        {
            musicManager.SetVolume(target);
        }
    }

    void SetBgmVolume(float value)
    {
        if (gameManager != null)
        {
            gameManager.SetBackgroundMusicVolume(value);
        }

        if (musicManager != null)
        {
            musicManager.SetVolume(value);
        }
    }

    public void TogglePause()
    {
        if (gameManager != null && gameManager.IsGameOver())
        {
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        if (!EnsureMenuRoot()) return;
        cachedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isPaused = true;
        if (menuRoot != null)
        {
            menuRoot.transform.SetAsLastSibling();
            menuRoot.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        float resumeScale = cachedTimeScale > 0f ? cachedTimeScale : 1f;
        Time.timeScale = resumeScale;
        isPaused = false;
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        if (gameManager != null)
        {
            gameManager.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    void OnDisable()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    void BuildMenu()
    {
        fallbackFont = GetFallbackFont();
        Canvas targetCanvas = FindObjectOfType<Canvas>();
        if (targetCanvas == null)
        {
            GameObject canvasObj = new GameObject("PauseMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            targetCanvas = canvasObj.GetComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        GameObject root = new GameObject("PauseMenu", typeof(RectTransform));
        root.transform.SetParent(targetCanvas.transform, false);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        Image dim = root.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.6f);
        dim.sprite = GetOnePixelSprite();
        dim.type = Image.Type.Simple;

        GameObject panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = panelSize;
        panelRt.anchoredPosition = Vector2.zero;
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1f, 1f, 1f, 0.95f);
        panelImage.sprite = GetOnePixelSprite();
        panelImage.type = Image.Type.Simple;

        resumeButton = CreateButton(panel.transform, "ResumeButton", "继续游戏", buttonSize, new Vector2(0f, 80f));
        restartButton = CreateButton(panel.transform, "RestartButton", "重新开始", buttonSize, new Vector2(0f, 0f));
        mainMenuButton = CreateButton(panel.transform, "MainMenuButton", "退回到主界面", buttonSize, new Vector2(0f, -80f));
        CreateLabel(panel.transform, "BgmLabel", "调节背景音乐大小", 24, new Vector2(panelSize.x, 32f), new Vector2(0f, -150f));
        bgmSlider = CreateSlider(panel.transform, "BgmSlider", new Vector2(buttonSize.x, 20f), new Vector2(0f, -190f));

        menuRoot = root;
    }

    TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int fontSize, Vector2 size, Vector2 anchoredPos)
    {
        GameObject labelObj = new GameObject(name, typeof(RectTransform));
        labelObj.transform.SetParent(parent, false);
        RectTransform rt = labelObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        TMP_FontAsset fontAsset = GetFallbackFont();
        if (fontAsset != null)
        {
            label.font = fontAsset;
        }
        label.raycastTarget = false;
        return label;
    }

    Button CreateButton(Transform parent, string name, string text, Vector2 size, Vector2 anchoredPos)
    {
        GameObject buttonObj = new GameObject(name, typeof(RectTransform));
        buttonObj.transform.SetParent(parent, false);
        RectTransform rt = buttonObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        image.sprite = GetOnePixelSprite();
        image.type = Image.Type.Simple;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.85f, 0.9f, 1f, 1f);
        colors.pressedColor = new Color(0.75f, 0.8f, 0.95f, 1f);
        button.colors = colors;

        TextMeshProUGUI label = CreateLabel(buttonObj.transform, "Label", text, 28, size, Vector2.zero);
        RectTransform labelRt = label.rectTransform;
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        return button;
    }

    Slider CreateSlider(Transform parent, string name, Vector2 size, Vector2 anchoredPos)
    {
        GameObject sliderObj = new GameObject(name, typeof(RectTransform));
        sliderObj.transform.SetParent(parent, false);
        RectTransform rt = sliderObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        Image bg = sliderObj.AddComponent<Image>();
        bg.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        bg.sprite = GetOnePixelSprite();
        bg.type = Image.Type.Simple;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.offsetMin = new Vector2(10f, 0f);
        fillAreaRt.offsetMax = new Vector2(-10f, 0f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.25f, 0.55f, 0.85f, 1f);
        fillImage.sprite = GetOnePixelSprite();
        fillImage.type = Image.Type.Simple;
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = new Vector2(0f, 0f);
        handleAreaRt.anchorMax = new Vector2(1f, 1f);
        handleAreaRt.offsetMin = new Vector2(10f, 0f);
        handleAreaRt.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1f, 1f, 1f, 1f);
        handleImage.sprite = GetOnePixelSprite();
        handleImage.type = Image.Type.Simple;
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20f, 20f);
        handleRt.anchorMin = new Vector2(0f, 0.5f);
        handleRt.anchorMax = new Vector2(0f, 0.5f);
        handleRt.anchoredPosition = Vector2.zero;

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImage;

        return slider;
    }
}
