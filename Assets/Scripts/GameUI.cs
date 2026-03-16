using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
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

    [Header("UI元素")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI restartHintText;
    public Slider healthSlider;

    private PlayerController playerController;
    private GameManager gameManager;
    
    // 受伤红色边缘遮罩
    private RectTransform overlayRoot;
    private Image overlayTop;
    private Image overlayBottom;
    private Image overlayLeft;
    private Image overlayRight;
    private const float hurtThreshold = 37f;
    private const float overlayAlpha = 0.35f;
    private const float edgeThickness = 120f; // 边缘厚度（像素）

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();

        SetupHealthSliderVisuals();
        ResizeHealthSliderByMaxHealth();
        EnsureHurtOverlay();
    }

    void Update()
    {
        if (gameManager != null)
        {
            bool isGameOver = gameManager.IsGameOver();
            
            // 游戏进行中显示速度和距离
            if (!isGameOver)
            {
                if (playerController != null && speedText != null)
                {
                    float speed = playerController.GetForwardSpeed();
                    speedText.text = $"speed: {speed:F1} m/s";
                }

                if (distanceText != null)
                {
                    float distance = gameManager.GetCurrentDistance();
                    distanceText.text = $"distance: {distance:F1} m";
                }
            }

            if (playerController != null)
            {
                float currentHealth = playerController.GetCurrentHealth();
                float maxHealth = playerController.GetMaxHealth();

                if (healthSlider != null)
                {
                    if (!Mathf.Approximately(healthSlider.maxValue, maxHealth))
                    {
                        healthSlider.maxValue = maxHealth;
                        ResizeHealthSliderByMaxHealth();
                    }
                    healthSlider.value = currentHealth;
                }
                
                // 控制受伤遮罩
                UpdateHurtOverlay(currentHealth);
            }

            // 游戏结束时显示得分
            if (isGameOver && scoreText != null)
            {
                float score = gameManager.GetFinalScore();
                scoreText.text = $"Distance: {score:F1} m";
            }
            
            // 显示/隐藏游戏结束文本
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(isGameOver);
            }
            
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(isGameOver);
            }
            
            if (restartHintText != null)
            {
                restartHintText.gameObject.SetActive(isGameOver);
            }
        }
    }

    void SetupHealthSliderVisuals()
    {
        if (playerController == null || healthSlider == null) return;

        // 数值绑定
        healthSlider.minValue = 0f;
        healthSlider.wholeNumbers = false;
        healthSlider.maxValue = playerController.GetMaxHealth();
        healthSlider.value = playerController.GetCurrentHealth();

        // 移除手柄，避免交界处出现按钮形状
        if (healthSlider.handleRect != null && healthSlider.handleRect.gameObject != null)
        {
            healthSlider.handleRect.gameObject.SetActive(false);
            healthSlider.handleRect = null;
        }

        // 填充颜色设置为红色，其余所有 Image 设为透明
        Image fillImg = null;
        if (healthSlider.fillRect != null)
        {
            fillImg = healthSlider.fillRect.GetComponent<Image>();
            if (fillImg != null)
            {
                fillImg.color = Color.red;
                fillImg.type = Image.Type.Simple;
                fillImg.sprite = GetOnePixelSprite();
                fillImg.fillMethod = Image.FillMethod.Horizontal;
                fillImg.fillOrigin = 0;
                fillImg.fillAmount = 1f;
                fillImg.preserveAspect = false;
                var mask = fillImg.GetComponent<Mask>();
                if (mask != null) mask.enabled = false;
                var rmask = fillImg.GetComponent<RectMask2D>();
                if (rmask != null) rmask.enabled = false;
            }
        }

        // 根节点 Image 透明
        Image rootImg = healthSlider.GetComponent<Image>();
        if (rootImg != null)
        {
            rootImg.color = new Color(0f, 0f, 0f, 0f);
        }

        // 所有非填充 Image 透明，并选择一个作为极窄黑边框
        Image outlineTarget = null;
        Image[] allImages = healthSlider.GetComponentsInChildren<Image>(true);
        foreach (var img in allImages)
        {
            if (img == null) continue;
            if (fillImg != null && img == fillImg) continue;
            img.color = new Color(0f, 0f, 0f, 0f);
            if (outlineTarget == null && img.name == "Background")
            {
                outlineTarget = img;
            }
        }
        if (outlineTarget == null)
        {
            foreach (var img in allImages)
            {
                if (img != null && img != fillImg)
                {
                    outlineTarget = img;
                    break;
                }
            }
        }
        if (outlineTarget != null)
        {
            outlineTarget.type = Image.Type.Simple;
            outlineTarget.sprite = GetOnePixelSprite();
            outlineTarget.color = new Color(0f, 0f, 0f, 0f); // 自身透明，仅显示Outline
            var bgMask = outlineTarget.GetComponent<Mask>();
            if (bgMask != null) bgMask.enabled = false;
            var bgRMask = outlineTarget.GetComponent<RectMask2D>();
            if (bgRMask != null) bgRMask.enabled = false;
            var outline = outlineTarget.GetComponent<Outline>();
            if (outline == null)
            {
                outline = outlineTarget.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = new Color(0f, 0f, 0f, 1f);
            outline.effectDistance = new Vector2(0.5f, 0.5f); // 极窄黑边
            outline.useGraphicAlpha = false;
        }

        // 填充从左到右，FillArea 去除左右内边距，保证满/空贴边无间隙
        healthSlider.direction = Slider.Direction.LeftToRight;
        if (healthSlider.fillRect != null)
        {
            RectTransform fillArea = healthSlider.fillRect.parent as RectTransform;
            if (fillArea != null)
            {
                Vector2 aMin = fillArea.anchorMin;
                Vector2 aMax = fillArea.anchorMax;
                aMin.x = 0f; aMax.x = 1f;
                fillArea.anchorMin = aMin;
                fillArea.anchorMax = aMax;
                Vector2 offMin = fillArea.offsetMin;
                Vector2 offMax = fillArea.offsetMax;
                offMin.x = 0f; offMax.x = 0f;
                fillArea.offsetMin = offMin;
                fillArea.offsetMax = offMax;
                Vector2 pivot = fillArea.pivot;
                pivot.x = 0.5f;
                fillArea.pivot = pivot;
                var areaMask = fillArea.GetComponent<Mask>();
                if (areaMask != null) areaMask.enabled = false;
                var areaRMask = fillArea.GetComponent<RectMask2D>();
                if (areaRMask != null) areaRMask.enabled = false;
            }
            // 确保填充图层本身无左右偏移，避免满/空时出现缝隙
            RectTransform fillRect = healthSlider.fillRect as RectTransform;
            if (fillRect != null)
            {
                Vector2 fOffMin = fillRect.offsetMin;
                Vector2 fOffMax = fillRect.offsetMax;
                fOffMin.x = 0f; fOffMax.x = 0f;
                fillRect.offsetMin = fOffMin;
                fillRect.offsetMax = fOffMax;
            }
        }
    }

    void ResizeHealthSliderByMaxHealth()
    {
        if (playerController == null || healthSlider == null) return;
        float maxHealth = playerController.GetMaxHealth();
        float width = 4f * maxHealth; // 100 -> 400
        float height = 40f; // 固定40
        RectTransform rt = healthSlider.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }

    void EnsureHurtOverlay()
    {
        // 在当前 UI 根下创建一个用于边缘遮罩的容器
        if (overlayRoot != null) return;
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        GameObject root = new GameObject("HurtOverlay", typeof(RectTransform));
        root.transform.SetParent(rootCanvas.transform, false);
        overlayRoot = root.GetComponent<RectTransform>();
        overlayRoot.anchorMin = Vector2.zero;
        overlayRoot.anchorMax = Vector2.one;
        overlayRoot.offsetMin = Vector2.zero;
        overlayRoot.offsetMax = Vector2.zero;
        overlayRoot.SetAsLastSibling(); // 确保覆盖在最上层

        // 创建四个边
        overlayTop = CreateEdge("Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -edgeThickness));
        overlayBottom = CreateEdge("Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, edgeThickness));
        overlayLeft = CreateEdge("Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(edgeThickness, 0f));
        overlayRight = CreateEdge("Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-edgeThickness, 0f));

        SetEdgesActive(false);
    }

    Image CreateEdge(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 edgeOffset)
    {
        GameObject go = new GameObject($"HurtEdge-{name}", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(overlayRoot, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(Mathf.Min(0f, edgeOffset.x), Mathf.Min(0f, edgeOffset.y));
        rt.offsetMax = new Vector2(Mathf.Max(0f, edgeOffset.x), Mathf.Max(0f, edgeOffset.y));
        Image img = go.GetComponent<Image>();
        img.sprite = GetOnePixelSprite();
        img.type = Image.Type.Sliced;
        img.color = new Color(1f, 0f, 0f, overlayAlpha);
        return img;
    }

    void SetEdgesActive(bool active)
    {
        if (overlayTop != null) overlayTop.enabled = active;
        if (overlayBottom != null) overlayBottom.enabled = active;
        if (overlayLeft != null) overlayLeft.enabled = active;
        if (overlayRight != null) overlayRight.enabled = active;
    }

    void UpdateHurtOverlay(float currentHealth)
    {
        if (overlayRoot == null) return;
        bool show = currentHealth < hurtThreshold;
        SetEdgesActive(show);
    }
}

