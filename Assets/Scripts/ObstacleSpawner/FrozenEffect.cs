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
        // 显示蓝色覆盖
        overlayCanvas.enabled = true;
        
        // 淡入效果
        float fadeTime = 0.2f;
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            float alpha = Mathf.Lerp(0, freezeScreenColor.a, elapsedTime / fadeTime);
            overlayImage.color = new Color(freezeScreenColor.r, freezeScreenColor.g, freezeScreenColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        overlayImage.color = freezeScreenColor;
        
        // 使用新的冻结方法完全冻结玩家
        player.ApplyFrozenEffect(freezeDuration);
        
        // 等待冻结时间
        yield return new WaitForSeconds(freezeDuration - fadeTime * 2);
        
        // 淡出效果
        elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            float alpha = Mathf.Lerp(freezeScreenColor.a, 0, elapsedTime / fadeTime);
            overlayImage.color = new Color(freezeScreenColor.r, freezeScreenColor.g, freezeScreenColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        overlayImage.color = new Color(freezeScreenColor.r, freezeScreenColor.g, freezeScreenColor.b, 0);
        
        // 隐藏覆盖
        overlayCanvas.enabled = false;
        
        freezeCoroutine = null;
    }
}
