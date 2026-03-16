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
    private GameObject stormRainInstance;
    private Light stormLightningLight;
    private Coroutine stormLightningRoutine;
    private bool stormActive;
    private Transform stormFollowTarget;
    private Vector3 stormRainOffset;
    private float stormBaseAmbientIntensity;
    private float stormBaseFogDensity;
    private float stormLightningFogDensity;
    private AudioSource stormLightningAudioSource;
    private GameObject iceSnowInstance;
    private bool iceActive;
    private Transform iceFollowTarget;
    private Vector3 iceSnowOffset;
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
    private bool hasRecordedRun = false;
    [Range(0f, 1f)]
    [SerializeField] private float backgroundMusicVolume = 1f;

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
        bool isStormMode = (modeData.modeType == GameMode.Storm);
        bool isIceMode = (modeData.modeType == GameMode.Ice);
        Debug.Log($"[环境设置] 开始应用环境设置, 地狱模式: {isHellMode}, 黑夜模式: {isNightMode}, 雷暴模式: {isStormMode}, 冰封模式: {isIceMode}");
        
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
        else if (isNightMode || isStormMode)
        {
            string nightLabel = isStormMode ? "雷暴模式" : "黑夜模式";
            Debug.Log($"[环境设置] {nightLabel} - 开始设置黑色天空和雾效");
            
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

        if (isStormMode)
        {
            StartStormEffects(modeData);
        }
        else
        {
            StopStormEffects();
        }

        if (isIceMode)
        {
            StartIceEffects(modeData);
        }
        else
        {
            StopIceEffects();
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
        
        ApplyBackgroundMusicVolume();
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

        UpdateStormRainFollow();
        UpdateIceSnowFollow();
        
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void OnGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        RecordRunSnapshot();

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        Debug.Log($"游戏结束，得分 {finalScore:F1} 米");
    }

    public void RestartGame()
    {
        RecordRunSnapshot();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        RecordRunSnapshot();
        Time.timeScale = 1f;
        RestoreEnvironmentSettings();
        SceneManager.LoadScene("MainMenu");
    }
    
    void RestoreEnvironmentSettings()
    {
        StopStormEffects();
        StopIceEffects();

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

    void StartStormEffects(GameModeData modeData)
    {
        if (stormActive)
        {
            return;
        }

        stormActive = true;
        stormFollowTarget = playerController != null ? playerController.transform : null;
        stormRainOffset = modeData.stormRainOffset;
        stormBaseAmbientIntensity = RenderSettings.ambientIntensity;
        stormBaseFogDensity = RenderSettings.fogDensity;
        if (modeData.stormFogDensity > 0f)
        {
            RenderSettings.fogDensity = modeData.stormFogDensity;
        }
        stormBaseFogDensity = RenderSettings.fogDensity;
        stormLightningFogDensity = modeData.lightningFogDensity > 0f ? modeData.lightningFogDensity : stormBaseFogDensity;

        if (modeData.stormRainPrefab != null)
        {
            stormRainInstance = Instantiate(modeData.stormRainPrefab);
            stormRainInstance.name = "StormRain";
            stormRainInstance.transform.SetParent(transform, false);
            UpdateStormRainFollow();
        }

        stormLightningLight = new GameObject("StormLightningLight").AddComponent<Light>();
        stormLightningLight.type = LightType.Directional;
        stormLightningLight.color = modeData.lightningColor;
        stormLightningLight.intensity = 0f;
        stormLightningLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        if (modeData.lightningSound != null)
        {
            GameObject audioObj = new GameObject("StormLightningAudio");
            audioObj.transform.SetParent(transform, false);
            stormLightningAudioSource = audioObj.AddComponent<AudioSource>();
            stormLightningAudioSource.playOnAwake = false;
            stormLightningAudioSource.loop = false;
            stormLightningAudioSource.spatialBlend = 0f;
        }

        stormLightningRoutine = StartCoroutine(StormLightningLoop(modeData));
    }

    void StopStormEffects()
    {
        if (!stormActive)
        {
            return;
        }

        stormActive = false;

        if (stormLightningRoutine != null)
        {
            StopCoroutine(stormLightningRoutine);
            stormLightningRoutine = null;
        }

        if (stormLightningLight != null)
        {
            Destroy(stormLightningLight.gameObject);
            stormLightningLight = null;
        }

        if (stormLightningAudioSource != null)
        {
            Destroy(stormLightningAudioSource.gameObject);
            stormLightningAudioSource = null;
        }

        if (stormRainInstance != null)
        {
            Destroy(stormRainInstance);
            stormRainInstance = null;
        }

        stormFollowTarget = null;
    }

    void UpdateStormRainFollow()
    {
        if (!stormActive || stormRainInstance == null || stormFollowTarget == null)
        {
            return;
        }

        stormRainInstance.transform.position = stormFollowTarget.position + stormRainOffset;
    }

    void StartIceEffects(GameModeData modeData)
    {
        if (iceActive)
        {
            return;
        }

        iceActive = true;
        iceFollowTarget = modeData.iceSnowFollowPlayer && playerController != null ? playerController.transform : null;
        iceSnowOffset = modeData.iceSnowOffset;

        if (modeData.iceSnowPrefab != null)
        {
            iceSnowInstance = Instantiate(modeData.iceSnowPrefab);
            iceSnowInstance.name = "IceSnow";
            iceSnowInstance.transform.SetParent(transform, false);
            ConfigureIceSnowParticleSystems(iceSnowInstance, modeData);
            if (iceFollowTarget != null)
            {
                UpdateIceSnowFollow();
            }
            else
            {
                iceSnowInstance.transform.position = modeData.iceSnowWorldPosition;
            }
        }
    }

    void StopIceEffects()
    {
        if (!iceActive)
        {
            return;
        }

        iceActive = false;

        if (iceSnowInstance != null)
        {
            Destroy(iceSnowInstance);
            iceSnowInstance = null;
        }

        iceFollowTarget = null;
    }

    void UpdateIceSnowFollow()
    {
        if (!iceActive || iceSnowInstance == null || iceFollowTarget == null)
        {
            return;
        }

        iceSnowInstance.transform.position = iceFollowTarget.position + iceSnowOffset;
    }

    void ConfigureIceSnowParticleSystems(GameObject instance, GameModeData modeData)
    {
        if (instance == null)
        {
            return;
        }

        ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var system in systems)
        {
            if (system == null) continue;
            var shape = system.shape;
            if (shape.enabled)
            {
                shape.scale = modeData.iceSnowAreaSize;
            }

            if (!modeData.iceSnowFollowPlayer)
            {
                var main = system.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
            }
        }
    }

    void SpawnLightningVisual(GameModeData modeData)
    {
        if (modeData.stormLightningPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = GetLightningSpawnPosition(modeData);
        float lifetime = Mathf.Max(0.1f, modeData.lightningVisualLifetime);
        Quaternion baseRotation = Quaternion.Euler(modeData.lightningPrefabEuler);
        Quaternion yawRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject instance = Instantiate(modeData.stormLightningPrefab, spawnPos, yawRotation * baseRotation);
        DisableLightningArtifacts(instance);
        StartCoroutine(CleanupLightningVisual(instance, lifetime));
    }

    Vector3 GetLightningSpawnPosition(GameModeData modeData)
    {
        Vector3 basePos = stormFollowTarget != null ? stormFollowTarget.position : Vector3.zero;
        Vector3 forward = stormFollowTarget != null ? stormFollowTarget.forward : Vector3.forward;
        Vector3 right = stormFollowTarget != null ? stormFollowTarget.right : Vector3.right;

        float forwardMin = modeData.lightningForwardRange.x;
        float forwardMax = Mathf.Max(forwardMin, modeData.lightningForwardRange.y);
        float forwardOffset = Random.Range(forwardMin, forwardMax);
        float horizontal = Random.Range(-Mathf.Abs(modeData.lightningHorizontalRange), Mathf.Abs(modeData.lightningHorizontalRange));
        float height = Mathf.Max(0f, modeData.lightningSpawnHeight);

        return basePos + forward * forwardOffset + right * horizontal + Vector3.up * height;
    }

    void PlayLightningSound(GameModeData modeData)
    {
        if (stormLightningAudioSource == null || modeData.lightningSound == null)
        {
            return;
        }

        float baseVolume = backgroundMusicSource != null ? backgroundMusicSource.volume : backgroundMusicVolume;
        float louderThanBgm = Mathf.Clamp01(baseVolume + 0.15f);
        float targetVolume = Mathf.Clamp01(Mathf.Max(modeData.lightningSoundVolume, louderThanBgm));
        stormLightningAudioSource.volume = targetVolume;
        stormLightningAudioSource.pitch = Random.Range(0.95f, 1.05f);
        stormLightningAudioSource.PlayOneShot(modeData.lightningSound, 1f);
    }

    void DisableLightningArtifacts(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        Transform[] allTransforms = instance.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allTransforms)
        {
            if (child == null) continue;
            string name = child.name;
            if (name.Contains("BurntGround") || name.Contains("Burnt") || name.Contains("Scorch"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    System.Collections.IEnumerator CleanupLightningVisual(GameObject instance, float lifetime)
    {
        if (instance == null)
        {
            yield break;
        }

        yield return new WaitForSeconds(lifetime);

        if (instance == null)
        {
            yield break;
        }

        ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var system in systems)
        {
            if (system != null)
            {
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        TrailRenderer[] trails = instance.GetComponentsInChildren<TrailRenderer>(true);
        foreach (var trail in trails)
        {
            if (trail != null)
            {
                trail.Clear();
            }
        }

        Destroy(instance);
    }

    System.Collections.IEnumerator StormLightningLoop(GameModeData modeData)
    {
        while (stormActive)
        {
            float minInterval = Mathf.Max(0.2f, modeData.lightningIntervalRange.x);
            float maxInterval = Mathf.Max(minInterval, modeData.lightningIntervalRange.y);
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(FlashLightningSeries(modeData));
        }
    }

    System.Collections.IEnumerator FlashLightningSeries(GameModeData modeData)
    {
        if (stormLightningLight == null)
        {
            yield break;
        }

        int minFlashes = Mathf.Max(1, modeData.lightningFlashCountRange.x);
        int maxFlashes = Mathf.Max(minFlashes, modeData.lightningFlashCountRange.y);
        int flashCount = Random.Range(minFlashes, maxFlashes + 1);
        float gap = Mathf.Max(0.02f, modeData.lightningFlashGap);

        for (int i = 0; i < flashCount && stormActive; i++)
        {
            SpawnLightningVisual(modeData);
            PlayLightningSound(modeData);
            yield return StartCoroutine(FlashLightning(modeData));

            if (i < flashCount - 1)
            {
                yield return new WaitForSeconds(gap);
            }
        }
    }

    System.Collections.IEnumerator FlashLightning(GameModeData modeData)
    {
        if (stormLightningLight == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.05f, modeData.lightningFlashDuration);
        float half = duration * 0.5f;
        float targetIntensity = Mathf.Max(0f, modeData.lightningIntensity);
        float ambientBoost = Mathf.Max(0f, modeData.lightningAmbientBoost);
        float targetFogDensity = stormLightningFogDensity;

        float t = 0f;
        while (t < half && stormActive)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / half);
            stormLightningLight.intensity = Mathf.Lerp(0f, targetIntensity, pct);
            RenderSettings.ambientIntensity = stormBaseAmbientIntensity + ambientBoost * pct;
            RenderSettings.fogDensity = Mathf.Lerp(stormBaseFogDensity, targetFogDensity, pct);
            yield return null;
        }

        t = 0f;
        while (t < half && stormActive)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / half);
            stormLightningLight.intensity = Mathf.Lerp(targetIntensity, 0f, pct);
            RenderSettings.ambientIntensity = stormBaseAmbientIntensity + ambientBoost * (1f - pct);
            RenderSettings.fogDensity = Mathf.Lerp(targetFogDensity, stormBaseFogDensity, pct);
            yield return null;
        }

        stormLightningLight.intensity = 0f;
        RenderSettings.ambientIntensity = stormBaseAmbientIntensity;
        RenderSettings.fogDensity = stormBaseFogDensity;
    }
    
    void OnDestroy()
    {
        RestoreEnvironmentSettings();
    }

    float GetModeMusicVolumeMultiplier()
    {
        GameModeData modeData = GameModeManager.Instance != null ? GameModeManager.Instance.GetSelectedModeData() : null;
        if (modeData != null && modeData.modeType == GameMode.Hell)
        {
            return 0.6f;
        }
        return 1f;
    }

    void ApplyBackgroundMusicVolume()
    {
        if (backgroundMusicSource == null) return;
        float clampedVolume = Mathf.Clamp01(backgroundMusicVolume);
        backgroundMusicSource.volume = clampedVolume * GetModeMusicVolumeMultiplier();
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        backgroundMusicVolume = Mathf.Clamp01(volume);
        ApplyBackgroundMusicVolume();
    }

    public float GetBackgroundMusicVolume()
    {
        return backgroundMusicVolume;
    }

    public float GetGameTime() => gameTime;
    public bool IsGameOver() => isGameOver;
    public float GetFinalScore() => finalScore;

    public float GetCurrentDistance()
    {
        return playerController != null ? playerController.GetTotalDistance() : 0f;
    }

    void RecordRunSnapshot()
    {
        if (hasRecordedRun)
        {
            return;
        }

        if (playerController != null)
        {
            finalScore = playerController.GetTotalDistance();
        }

        GameMode selectedMode = GameModeManager.Instance != null ? GameModeManager.Instance.GetSelectedMode() : GameMode.Endless;
        CharacterData selectedCharacter = CharacterManager.Instance != null ? CharacterManager.Instance.GetSelectedCharacter() : null;
        GameDataManager.Instance.RecordRun(selectedMode, finalScore, selectedCharacter);
        hasRecordedRun = true;
    }
}
