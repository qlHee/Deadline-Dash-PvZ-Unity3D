using System;
using System.Text;
using TMPro;
using UnityEngine;
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
        if (panelRoot == null)
        {
            return;
        }

        bool nextState = !panelRoot.activeSelf;
        panelRoot.SetActive(nextState);
        if (nextState)
        {
            if (refreshOnOpen)
            {
                Refresh();
            }
            ApplyTabState();
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
        if (recordsText != null)
        {
            recordsText.text = BuildRecordsText();
        }

        if (bestText != null)
        {
            bestText.text = BuildBestText();
        }
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
        if (recordsText != null)
        {
            recordsText.gameObject.SetActive(showRecords);
        }

        if (bestText != null)
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

        GameObject recordsObj = CreateTextBlock(panelObj.transform, "RecordsText", new Vector2(0f, -10f), new Vector2(780f, 400f));
        recordsText = recordsObj.GetComponent<TMP_Text>();
        recordsText.alignment = TextAlignmentOptions.TopLeft;

        GameObject bestObj = CreateTextBlock(panelObj.transform, "BestText", new Vector2(0f, -10f), new Vector2(780f, 400f));
        bestText = bestObj.GetComponent<TMP_Text>();
        bestText.alignment = TextAlignmentOptions.TopLeft;

        closeButton = CreateButton(panelObj.transform, "CloseButton", "关闭", new Vector2(300f, -230f), buttonSize);
    }

    TMP_FontAsset FindSceneFont()
    {
        TMP_Text anyText = FindObjectOfType<TMP_Text>();
        return anyText != null ? anyText.font : null;
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
