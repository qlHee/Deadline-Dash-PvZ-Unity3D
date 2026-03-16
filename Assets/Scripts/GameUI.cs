﻿using UnityEngine;
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

    [Header("UI 元素")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI restartHintText;
    public Slider healthSlider;

    private PlayerController playerController;
    private GameManager gameManager;
    
    [Header("残血遮罩")]
    public Image bloodOverlayImage;
    public float hurtThreshold = 37f;
    [Range(0f, 1f)]
    public float maxBloodOverlayAlpha = 0.75f;
    public float pulseAmplitude = 0.08f;
    public float pulseSpeed = 3f;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();

        SetupHealthSliderVisuals();
        ResizeHealthSliderByMaxHealth();
    }

    void Update()
    {
        if (gameManager != null)
        {
            bool isGameOver = gameManager.IsGameOver();
            
            // 游戏进行时刷新速度与距离
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
                
                // 根据血量控制遮罩
                UpdateBloodOverlay(currentHealth);
            }

            // 游戏结束后显示结算距离
            if (isGameOver && scoreText != null)
            {
                float score = gameManager.GetFinalScore();
                scoreText.text = $"distance: {score:F1} m";
            }
            
            // 控制结束提示的显隐
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

        // 初始化血条参数
        healthSlider.minValue = 0f;
        healthSlider.wholeNumbers = false;
        healthSlider.maxValue = playerController.GetMaxHealth();
        healthSlider.value = playerController.GetCurrentHealth();

        // 去掉 Handle，避免出现按钮效果
        if (healthSlider.handleRect != null && healthSlider.handleRect.gameObject != null)
        {
            healthSlider.handleRect.gameObject.SetActive(false);
            healthSlider.handleRect = null;
        }

        // 将填充部分设为红色，其余 Image 透明
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

        // Slider 自身背景设为透明
        Image rootImg = healthSlider.GetComponent<Image>();
        if (rootImg != null)
        {
            rootImg.color = new Color(0f, 0f, 0f, 0f);
        }

        // 其它 Image 设透明，仅保留一个作为描边
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
            outlineTarget.color = new Color(0f, 0f, 0f, 0f); // 本体透明，仅显示描边
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
            outline.effectDistance = new Vector2(0.5f, 0.5f); // 描边偏移
            outline.useGraphicAlpha = false;
        }

        // 让填充从左到右铺满，移除 FillArea 边距
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
            // 防止填充 Rect 本身产生水平偏移
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
        float height = 40f; // 高度固定 40
        RectTransform rt = healthSlider.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }

    void UpdateBloodOverlay(float currentHealth)
    {
        if (bloodOverlayImage == null)
        {
            return;
        }

        if (currentHealth >= hurtThreshold)
        {
            SetOverlayAlpha(0f);
            return;
        }

        float normalized = Mathf.InverseLerp(hurtThreshold, 0f, Mathf.Max(0f, currentHealth));
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        float targetAlpha = Mathf.Clamp01(maxBloodOverlayAlpha * normalized * pulse);
        SetOverlayAlpha(Mathf.Lerp(bloodOverlayImage.color.a, targetAlpha, Time.deltaTime * 5f));
    }

    void SetOverlayAlpha(float alpha)
    {
        Color c = bloodOverlayImage.color;
        c.a = alpha;
        bloodOverlayImage.color = c;
    }
}


