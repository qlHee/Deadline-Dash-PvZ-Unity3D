using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI 设置")]
    public GameObject gameOverUI;

    private bool isGameOver;
    private float gameTime;
    private float finalScore;
    private PlayerController playerController;
    private Light originalAmbientLight;
    private Color originalAmbientColor;
    private float originalAmbientIntensity;
    private GameObject specialLight;
    private AudioSource backgroundMusicSource;
    private Dictionary<Light, float> originalLightIntensities = new Dictionary<Light, float>();
    private Material originalSkyboxMaterial;
    private Color originalSkyboxTint;
    private bool originalFogEnabled;
    private Color originalFogColor;
    private float originalFogDensity;
    private FogMode originalFogMode;
    private bool isTimeLimitMode = false;
    private float timeLimitTimer = 0f;

    void Start()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        Time.timeScale = 1f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            ApplySelectedCharacterAttributes();
        }
    }
    
    void ApplySelectedCharacterAttributes()
    {
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController not found, cannot apply character attributes.");
            return;
        }
        
        CharacterData selectedCharacter = CharacterManager.Instance.GetSelectedCharacter();
        if (selectedCharacter != null)
        {
            selectedCharacter.ApplyToPlayerController(playerController);
        }
        else
        {
            Debug.LogWarning("No character selected, using default attributes.");
        }
        
        ApplyGameModeSettings();
    }
    
    void ApplyGameModeSettings()
    {
        if (playerController == null)
        {
            Debug.LogWarning("PlayerController not found, cannot apply game mode settings.");
            return;
        }
        
        GameModeData modeData = GameModeManager.Instance.GetSelectedModeData();
        if (modeData != null)
        {
            modeData.ApplyToGame(this, playerController);
            ApplyEnvironmentSettings(modeData);
            ApplyBackgroundMusic(modeData);
            
            isTimeLimitMode = (modeData.modeType == GameMode.TimeLimit);
            if (isTimeLimitMode)
            {
                Debug.Log("[限时模式] 启用限时模式，每秒减少8点血量上限");
            }
        }
        else
        {
            Debug.LogWarning("No game mode selected, using default settings.");
        }
    }
    
    void ApplyEnvironmentSettings(GameModeData modeData)
    {
        bool isHellMode = (modeData.modeType == GameMode.Hell);
        bool isNightMode = (modeData.modeType == GameMode.Night);
        Debug.Log($"[环境设置] 开始应用环境设置, 地狱模式: {isHellMode}, 黑夜模式: {isNightMode}");
        
        originalAmbientColor = RenderSettings.ambientLight;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        
        Color newAmbientColor = originalAmbientColor * modeData.ambientLightMultiplier;
        float newAmbientIntensity = originalAmbientIntensity * modeData.ambientLightMultiplier;
        
        RenderSettings.ambientLight = newAmbientColor;
        RenderSettings.ambientIntensity = newAmbientIntensity;
        
        Debug.Log($"[环境设置] 环境光照亮度: {modeData.ambientLightMultiplier}x");
        
        Light[] sceneLights = FindObjectsOfType<Light>();
        foreach (Light sceneLight in sceneLights)
        {
            if (sceneLight.type == LightType.Directional)
            {
                originalLightIntensities[sceneLight] = sceneLight.intensity;
                sceneLight.intensity *= modeData.ambientLightMultiplier;
                Debug.Log($"[环境设置] 降低场景灯光强度: {sceneLight.name}, 原始: {originalLightIntensities[sceneLight]}, 新: {sceneLight.intensity}");
            }
        }
        
        if (isHellMode)
        {
            Debug.Log($"[环境设置] 地狱模式 - 开始添加红色灯光和天空");
            
            specialLight = new GameObject("HellModeLight");
            Light light = specialLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(228f/255f, 18f/255f, 51f/255f);
            light.intensity = 0.5f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Debug.Log($"[环境设置] 添加地狱灯光: RGB(228,18,51), 强度: 0.5");
            
            Color skyColor = new Color(70f/255f, 0f/255f, 22f/255f, 1f);
            
            originalSkyboxMaterial = RenderSettings.skybox;
            if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Tint"))
            {
                originalSkyboxTint = RenderSettings.skybox.GetColor("_Tint");
                RenderSettings.skybox.SetColor("_Tint", skyColor);
                Debug.Log($"[环境设置] 修改天空盒颜色: {skyColor}");
            }
            else
            {
                Debug.Log($"[环境设置] 没有天空盒，使用相机背景色");
                if (Camera.main != null)
                {
                    Camera.main.backgroundColor = skyColor;
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Debug.Log($"[环境设置] 修改相机背景色: {skyColor}");
                }
            }
        }
        else if (isNightMode)
        {
            Debug.Log($"[环境设置] 黑夜模式 - 开始设置黑色天空和雾效");
            
            if (modeData.addSpecialLight)
            {
                specialLight = new GameObject("NightModeLight");
                Light light = specialLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = modeData.specialLightColor;
                light.intensity = modeData.specialLightIntensity;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                Debug.Log($"[环境设置] 添加黑夜灯光: {modeData.specialLightColor}, 强度: {modeData.specialLightIntensity}");
            }
            
            Color blackSkyColor = Color.black;
            
            originalSkyboxMaterial = RenderSettings.skybox;
            if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Tint"))
            {
                originalSkyboxTint = RenderSettings.skybox.GetColor("_Tint");
                RenderSettings.skybox.SetColor("_Tint", blackSkyColor);
                Debug.Log($"[环境设置] 修改天空盒为黑色");
            }
            else
            {
                if (Camera.main != null)
                {
                    Camera.main.backgroundColor = blackSkyColor;
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Debug.Log($"[环境设置] 修改相机背景为黑色");
                }
            }
            
            originalFogEnabled = RenderSettings.fog;
            originalFogColor = RenderSettings.fogColor;
            originalFogDensity = RenderSettings.fogDensity;
            originalFogMode = RenderSettings.fogMode;
            
            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.black;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.08f;
            
            Debug.Log($"[环境设置] 启用黑色雾效, 密度: 0.08");
        }
    }
    
    void ApplyBackgroundMusic(GameModeData modeData)
    {
        Invoke(nameof(ApplyBackgroundMusicDelayed), 0.1f);
    }
    
    void ApplyBackgroundMusicDelayed()
    {
        GameModeData modeData = GameModeManager.Instance.GetSelectedModeData();
        if (modeData == null) return;
        
        AudioSource[] existingSources = FindObjectsOfType<AudioSource>();
        foreach (var source in existingSources)
        {
            if (source != null && source.isPlaying && source.loop)
            {
                source.Stop();
                Debug.Log($"[音乐设置] 停止音乐: {(source.clip != null ? source.clip.name : "未知")}");
            }
        }
        
        if (modeData.backgroundMusic == null)
        {
            Debug.Log($"[音乐设置] 当前模式没有配置背景音乐");
            return;
        }
        
        if (backgroundMusicSource == null)
        {
            GameObject musicObj = new GameObject("BackgroundMusic");
            backgroundMusicSource = musicObj.AddComponent<AudioSource>();
            backgroundMusicSource.loop = true;
            backgroundMusicSource.playOnAwake = false;
        }
        
        backgroundMusicSource.clip = modeData.backgroundMusic;
        
        if (modeData.modeType == GameMode.Hell)
        {
            backgroundMusicSource.volume = 0.6f;
        }
        else
        {
            backgroundMusicSource.volume = 1.0f;
        }
        
        backgroundMusicSource.Play();
        Debug.Log($"[音乐设置] 播放背景音乐: {modeData.backgroundMusic.name}, 音量: {backgroundMusicSource.volume}");
    }

    void Update()
    {
        if (!isGameOver)
        {
            gameTime += Time.deltaTime;
            
            if (isTimeLimitMode && playerController != null)
            {
                timeLimitTimer += Time.deltaTime;
                if (timeLimitTimer >= 1f)
                {
                    timeLimitTimer -= 1f;
                    playerController.maxHealth -= 8f;
                    
                    if (playerController.maxHealth <= 0f)
                    {
                        playerController.maxHealth = 0f;
                        playerController.TakeDamage(9999f);
                        Debug.Log("[限时模式] 血量上限降至0，玩家死亡");
                    }
                    else if (playerController.GetCurrentHealth() > playerController.maxHealth)
                    {
                        playerController.TakeDamage(playerController.GetCurrentHealth() - playerController.maxHealth);
                    }
                }
            }
        }
        
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void OnGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (playerController != null)
        {
            finalScore = playerController.GetTotalDistance();
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        Debug.Log($"游戏结束，得分 {finalScore:F1} 米");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        RestoreEnvironmentSettings();
        SceneManager.LoadScene("MainMenu");
    }
    
    void RestoreEnvironmentSettings()
    {
        if (originalAmbientColor != Color.clear)
        {
            RenderSettings.ambientLight = originalAmbientColor;
            RenderSettings.ambientIntensity = originalAmbientIntensity;
        }
        
        foreach (var kvp in originalLightIntensities)
        {
            if (kvp.Key != null)
            {
                kvp.Key.intensity = kvp.Value;
            }
        }
        originalLightIntensities.Clear();
        
        if (originalSkyboxMaterial != null && originalSkyboxMaterial.HasProperty("_Tint"))
        {
            originalSkyboxMaterial.SetColor("_Tint", originalSkyboxTint);
        }
        
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogMode = originalFogMode;
        
        if (specialLight != null)
        {
            Destroy(specialLight);
        }
        
        if (backgroundMusicSource != null)
        {
            Destroy(backgroundMusicSource.gameObject);
        }
    }
    
    void OnDestroy()
    {
        RestoreEnvironmentSettings();
    }

    public float GetGameTime() => gameTime;
    public bool IsGameOver() => isGameOver;
    public float GetFinalScore() => finalScore;

    public float GetCurrentDistance()
    {
        return playerController != null ? playerController.GetTotalDistance() : 0f;
    }
}
