using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDataUI : MonoBehaviour
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

    [Header("UI")]
    public Button openButton;
    public GameObject panelRoot;
    public Button closeButton;
    public Button recordsTabButton;
    public Button bestTabButton;
    public TMP_Text recordsText;
    public TMP_Text bestText;
    public ScrollRect recordsScroll;
    public ScrollRect bestScroll;

    [Header("Behavior")]
    public bool autoBuild = true;
    public bool autoBind = true;
    public bool refreshOnOpen = true;

    [Header("Layout")]
    public Vector2 panelSize = new Vector2(860f, 520f);
    public Vector2 buttonSize = new Vector2(160f, 44f);
    public Vector2 tabButtonSize = new Vector2(140f, 36f);

    TMP_FontAsset fallbackFont;
    bool showRecords = true;

    void Awake()
    {
        if (fallbackFont == null)
        {
            fallbackFont = TMP_Settings.defaultFontAsset;
            TMP_FontAsset sceneFont = FindSceneFont();
            if (sceneFont != null)
            {
                fallbackFont = sceneFont;
            }
        }

        if (panelRoot == null && autoBuild)
        {
            BuildPanel();
        }

        if (autoBind)
        {
            AutoBind();
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (autoBind)
        {
            AutoBind();
        }
        TryPreparePanel();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoBind)
        {
            AutoBind();
        }
        TryPreparePanel();
    }

    void TryPreparePanel()
    {
        if (openButton == null && autoBind)
        {
            AutoBind();
        }

        if (openButton == null)
        {
            return;
        }

        EnsurePanel();
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    void EnsurePanel()
    {
        if (panelRoot == null || recordsText == null || bestText == null || recordsScroll == null || bestScroll == null)
        {
            if (panelRoot != null)
            {
                Destroy(panelRoot);
            }

            BuildPanel();
            BindButtons();
        }
    }

    void AutoBind()
    {
        if (openButton == null)
        {
            var buttonObj = GameObject.Find("GameRecordsButton");
            if (buttonObj != null)
            {
                openButton = buttonObj.GetComponent<Button>();
            }
        }

        BindButtons();
    }

    public void BindUI(Button openBtn)
    {
        if (openBtn != null)
        {
            openButton = openBtn;
        }

        BindButtons();
    }

    void BindButtons()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(TogglePanel);
            openButton.onClick.AddListener(TogglePanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HidePanel);
            closeButton.onClick.AddListener(HidePanel);
        }

        if (recordsTabButton != null)
        {
            recordsTabButton.onClick.RemoveListener(ShowRecordsTab);
            recordsTabButton.onClick.AddListener(ShowRecordsTab);
        }

        if (bestTabButton != null)
        {
            bestTabButton.onClick.RemoveListener(ShowBestTab);
            bestTabButton.onClick.AddListener(ShowBestTab);
        }
    }

    public void TogglePanel()
    {
        EnsurePanel();
        if (panelRoot == null)
        {
            return;
        }

        bool nextState = !panelRoot.activeSelf;
        panelRoot.SetActive(nextState);
        if (nextState)
        {
            bool shouldRefresh = refreshOnOpen || recordsText == null || string.IsNullOrEmpty(recordsText.text);
            if (shouldRefresh)
            {
                Refresh();
            }
            ApplyTabState();
            ResetScrollPositions();
        }
    }

    public void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void Refresh()
    {
        EnsurePanel();
        if (recordsText != null)
        {
            recordsText.text = BuildRecordsText();
            UpdateScrollContent(recordsScroll, recordsText);
        }

        if (bestText != null)
        {
            bestText.text = BuildBestText();
            UpdateScrollContent(bestScroll, bestText);
        }

        Canvas.ForceUpdateCanvases();
    }

    void ShowRecordsTab()
    {
        showRecords = true;
        ApplyTabState();
    }

    void ShowBestTab()
    {
        showRecords = false;
        ApplyTabState();
    }

    void ApplyTabState()
    {
        if (recordsScroll != null)
        {
            recordsScroll.gameObject.SetActive(showRecords);
        }
        else if (recordsText != null)
        {
            recordsText.gameObject.SetActive(showRecords);
        }

        if (bestScroll != null)
        {
            bestScroll.gameObject.SetActive(!showRecords);
        }
        else if (bestText != null)
        {
            bestText.gameObject.SetActive(!showRecords);
        }

        if (recordsTabButton != null)
        {
            SetTabButtonState(recordsTabButton, showRecords);
        }

        if (bestTabButton != null)
        {
            SetTabButtonState(bestTabButton, !showRecords);
        }

        ResetScrollPositions();
    }

    void ResetScrollPositions()
    {
        if (recordsScroll != null)
        {
            recordsScroll.verticalNormalizedPosition = 1f;
        }

        if (bestScroll != null)
        {
            bestScroll.verticalNormalizedPosition = 1f;
        }
    }

    void SetTabButtonState(Button button, bool active)
    {
        Image image = button != null ? button.GetComponent<Image>() : null;
        if (image != null)
        {
            image.color = active ? new Color(0.2f, 0.5f, 0.9f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.85f);
        }
    }

    string BuildRecordsText()
    {
        RunRecord[] records = GameDataManager.Instance.GetRunHistory().ToArray();
        if (records.Length == 0)
        {
            return "游戏记录\n\n暂无记录。";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("游戏记录");
        sb.AppendLine();
        sb.AppendLine("时间 | 模式 | 僵尸 | 距离");

        for (int i = records.Length - 1; i >= 0; i--)
        {
            RunRecord record = records[i];
            string timeLabel = FormatTime(record.timestampUtc);
            string modeLabel = ModeLabel(record.mode);
            string characterLabel = string.IsNullOrEmpty(record.characterName) ? "未知" : record.characterName;
            sb.AppendLine($"{timeLabel} | {modeLabel} | {characterLabel} | {record.distance:F1}米");
        }

        return sb.ToString();
    }

    string BuildBestText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("最高记录");
        sb.AppendLine();

        AppendBestLine(sb, GameMode.Endless, "无尽模式");
        AppendBestLine(sb, GameMode.Ice, "冰封模式");
        AppendBestLine(sb, GameMode.Hell, "地狱模式");
        AppendBestLine(sb, GameMode.Night, "黑夜模式");
        AppendBestLine(sb, GameMode.Storm, "雷暴模式");
        AppendBestLine(sb, GameMode.TimeLimit, "限时模式");

        return sb.ToString();
    }

    void AppendBestLine(StringBuilder sb, GameMode mode, string label)
    {
        float best = GameDataManager.Instance.GetBestDistance(mode);
        if (best <= 0f)
        {
            sb.AppendLine($"{label}: --");
        }
        else
        {
            sb.AppendLine($"{label}: {best:F1}米");
        }
    }

    string ModeLabel(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Hell:
                return "地狱模式";
            case GameMode.Night:
                return "黑夜模式";
            case GameMode.Storm:
                return "雷暴模式";
            case GameMode.Ice:
                return "冰封模式";
            case GameMode.TimeLimit:
                return "限时模式";
            default:
                return "无尽模式";
        }
    }

    string FormatTime(long timestampUtc)
    {
        if (timestampUtc <= 0)
        {
            return "Unknown";
        }

        DateTimeOffset dt = DateTimeOffset.FromUnixTimeSeconds(timestampUtc).ToLocalTime();
        return dt.ToString("yyyy-MM-dd HH:mm:ss");
    }

    void BuildPanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("GameDataUI: Canvas not found, cannot build panel.");
            return;
        }

        GameObject panelObj = new GameObject("GameDataPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        panelRoot = panelObj;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.sprite = GetOnePixelSprite();
        panelImage.color = new Color(0f, 0f, 0f, 0.65f);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;

        if (fallbackFont == null)
        {
            fallbackFont = TMP_Settings.defaultFontAsset;
        }

        recordsTabButton = CreateButton(panelObj.transform, "RecordsTab", "游戏记录", new Vector2(-90f, 230f), tabButtonSize);
        bestTabButton = CreateButton(panelObj.transform, "BestTab", "最高记录", new Vector2(90f, 230f), tabButtonSize);

        Vector2 scrollSize = new Vector2(780f, 400f);
        CreateScrollView(panelObj.transform, "RecordsScroll", new Vector2(0f, -10f), scrollSize, out recordsScroll, out recordsText);
        CreateScrollView(panelObj.transform, "BestScroll", new Vector2(0f, -10f), scrollSize, out bestScroll, out bestText);

        closeButton = CreateButton(panelObj.transform, "CloseButton", "关闭", new Vector2(300f, -230f), buttonSize);
        panelRoot.SetActive(false);
    }

    TMP_FontAsset FindSceneFont()
    {
        TMP_Text anyText = FindObjectOfType<TMP_Text>();
        return anyText != null ? anyText.font : null;
    }

    GameObject CreateScrollView(Transform parent, string name, Vector2 anchoredPos, Vector2 size, out ScrollRect scrollRect, out TMP_Text text)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 0.5f);
        rootRt.anchorMax = new Vector2(0.5f, 0.5f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.anchoredPosition = anchoredPos;
        rootRt.sizeDelta = size;

        Image rootImage = root.AddComponent<Image>();
        rootImage.sprite = GetOnePixelSprite();
        rootImage.color = new Color(0f, 0f, 0f, 0f);

        scrollRect = root.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(root.transform, false);
        RectTransform viewportRt = viewport.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.sprite = GetOnePixelSprite();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        viewportImage.raycastTarget = false;
        viewport.AddComponent<RectMask2D>();

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = Vector2.zero;

        text = content.AddComponent<TextMeshProUGUI>();
        text.font = fallbackFont;
        text.fontSize = 22f;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.text = "";
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;
        contentRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);

        scrollRect.viewport = viewportRt;
        scrollRect.content = contentRt;

        Scrollbar scrollbar = CreateScrollbar(root.transform, "Scrollbar");
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = -2f;

        return root;
    }

    void UpdateScrollContent(ScrollRect scroll, TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        RectTransform viewport = scroll != null ? scroll.viewport : null;
        float width = viewport != null ? viewport.rect.width : text.rectTransform.rect.width;
        if (width <= 0f)
        {
            width = panelSize.x - 80f;
        }

        text.ForceMeshUpdate();
        Vector2 preferred = text.GetPreferredValues(text.text, width, 0f);
        RectTransform rt = text.rectTransform;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        float minHeight = viewport != null ? viewport.rect.height : 0f;
        float height = Mathf.Max(preferred.y, minHeight);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    Scrollbar CreateScrollbar(Transform parent, string name)
    {
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(parent, false);
        RectTransform barRt = bar.AddComponent<RectTransform>();
        barRt.anchorMin = new Vector2(1f, 0f);
        barRt.anchorMax = new Vector2(1f, 1f);
        barRt.pivot = new Vector2(1f, 0.5f);
        barRt.sizeDelta = new Vector2(16f, 0f);
        barRt.anchoredPosition = new Vector2(-4f, 0f);

        Image barImage = bar.AddComponent<Image>();
        barImage.sprite = GetOnePixelSprite();
        barImage.color = new Color(0f, 0f, 0f, 0.35f);

        Scrollbar scrollbar = bar.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        GameObject slidingArea = new GameObject("SlidingArea");
        slidingArea.transform.SetParent(bar.transform, false);
        RectTransform slidingRt = slidingArea.AddComponent<RectTransform>();
        slidingRt.anchorMin = Vector2.zero;
        slidingRt.anchorMax = Vector2.one;
        slidingRt.offsetMin = Vector2.zero;
        slidingRt.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slidingArea.transform, false);
        RectTransform handleRt = handle.AddComponent<RectTransform>();
        handleRt.anchorMin = Vector2.zero;
        handleRt.anchorMax = Vector2.one;
        handleRt.offsetMin = new Vector2(2f, 2f);
        handleRt.offsetMax = new Vector2(-2f, -2f);

        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = GetOnePixelSprite();
        handleImage.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);

        scrollbar.handleRect = handleRt;
        scrollbar.targetGraphic = handleImage;
        scrollbar.size = 0.3f;

        return scrollbar;
    }

    GameObject CreateTextBlock(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TMP_Text text = go.AddComponent<TextMeshProUGUI>();
        text.font = fallbackFont;
        text.fontSize = 22f;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.text = "";
        return go;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.sprite = GetOnePixelSprite();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);

        Button btn = go.AddComponent<Button>();

        GameObject labelObj = new GameObject("Text");
        labelObj.transform.SetParent(go.transform, false);
        RectTransform labelRt = labelObj.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        TMP_Text text = labelObj.AddComponent<TextMeshProUGUI>();
        text.font = fallbackFont;
        text.text = label;
        text.fontSize = 20f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        return btn;
    }
}
