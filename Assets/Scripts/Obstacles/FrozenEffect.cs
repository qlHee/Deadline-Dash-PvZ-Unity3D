using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrozenEffect : MonoBehaviour
{
    [Header("冻结效果设置")]
    public Color freezeScreenColor = new Color(0.5f, 0.8f, 1f, 0.3f);
    public float duration = 2f;
    
    private static FrozenEffect instance;
    private Canvas overlayCanvas;
    private Image overlayImage;
    private Coroutine freezeCoroutine;
    
    public static FrozenEffect Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("FrozenEffectManager");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<FrozenEffect>();
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 创建UI覆盖层
        CreateOverlay();
    }
    
    private void CreateOverlay()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("FreezeOverlayCanvas");
        canvasObj.transform.SetParent(transform);
        
        overlayCanvas = canvasObj.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 100;
        
        // 添加CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // 创建图像
        GameObject imageObj = new GameObject("FreezeOverlayImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        overlayImage = imageObj.AddComponent<Image>();
        overlayImage.color = new Color(freezeScreenColor.r, freezeScreenColor.g, freezeScreenColor.b, 0); // 初始透明
        
        // 设置图像覆盖整个屏幕
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // 初始不可见
        overlayCanvas.enabled = false;
    }
    
    public void ApplyFreezeEffect(PlayerController player, float freezeDuration)
    {
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }
        freezeCoroutine = StartCoroutine(FreezeCoroutine(player, freezeDuration));
    }
    
    private IEnumerator FreezeCoroutine(PlayerController player, float freezeDuration)
    {
        // 冻结玩家
        player.ApplyFrozenEffect(freezeDuration);

        // 蓝色 UI 快速闪两下
        overlayCanvas.enabled = true;
        overlayImage.color = new Color(freezeScreenColor.r, freezeScreenColor.g, freezeScreenColor.b, 0);

        const int flashCount = 2;
        const float flashOnTime = 0.1f;
        const float flashOffTime = 0.05f;

        for (int i = 0; i < flashCount; i++)
        {
            // 立即显现
            overlayImage.color = freezeScreenColor;
            yield return new WaitForSeconds(flashOnTime);

            // 立即消失
            overlayImage.color = new Color(freezeScreenColor.r, freezeScreenColor.g, freezeScreenColor.b, 0);
            yield return new WaitForSeconds(flashOffTime);
        }

        overlayCanvas.enabled = false;
        freezeCoroutine = null;
    }
}
