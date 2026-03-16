using UnityEngine;

public enum GameMode
{
    Endless,
    Hell,
    Night,
    Storm,
    TimeLimit,
    Ice
}

[System.Serializable]
public class DifficultyStage
{
    public float distanceStart;
    public float distanceEnd;
    public float difficultyStart;
    public float difficultyEnd;
}

[CreateAssetMenu(fileName = "GameModeData", menuName = "Game/Game Mode Data")]
public class GameModeData : ScriptableObject
{
    [Header("模式信息")]
    public string modeName = "无尽模式";
    public GameMode modeType = GameMode.Endless;
    
    [Header("玩家属性倍率")]
    [Tooltip("进入对局时的血量倍率")]
    public float initialHealthMultiplier = 1f;
    
    [Tooltip("每秒回血量倍率")]
    public float regenRateMultiplier = 1f;
    
    [Header("动态难度设置")]
    [Tooltip("是否启用动态难度")]
    public bool useDynamicDifficulty = false;
    
    [Tooltip("难度阶段配置")]
    public DifficultyStage[] difficultyStages;
    
    [Header("统一驱动规则")]
    [Tooltip("最小数量倍率范围 (起始值)")]
    public float minCountMultiplierStart = 1.0f;
    
    [Tooltip("最小数量倍率范围 (结束值)")]
    public float minCountMultiplierEnd = 1.0f;
    
    [Tooltip("最大数量倍率范围 (起始值)")]
    public float maxCountMultiplierStart = 1.0f;
    
    [Tooltip("最大数量倍率范围 (结束值)")]
    public float maxCountMultiplierEnd = 1.0f;
    
    [Tooltip("间距倍率范围 (起始值)")]
    public float spacingMultiplierStart = 1.0f;
    
    [Tooltip("间距倍率范围 (结束值)")]
    public float spacingMultiplierEnd = 1.0f;
    
    [Header("环境设置")]
    [Tooltip("环境光照亮度倍率 (1.0=正常, 0.5=半亮)")]
    public float ambientLightMultiplier = 1f;
    
    [Tooltip("是否添加特殊灯光")]
    public bool addSpecialLight = false;
    
    [Tooltip("特殊灯光颜色")]
    public Color specialLightColor = Color.red;
    
    [Tooltip("特殊灯光强度")]
    public float specialLightIntensity = 0.5f;
    
    [Header("音乐设置")]
    [Tooltip("背景音乐")]
    public AudioClip backgroundMusic;

    [Header("Road Settings")]
    public GameObject roadPrefab;
    public Material roadMaterial;
    public Material wallMaterial;
    public GameObject[] grassPrefabs;
    public bool spawnGrassOnGeneratedSegments = true;

    [Header("Storm Settings")]
    public GameObject stormRainPrefab;
    public Vector3 stormRainOffset = new Vector3(0f, 12f, 0f);
    public Vector2 lightningIntervalRange = new Vector2(4f, 9f);
    public float lightningFlashDuration = 0.25f;
    public float lightningIntensity = 2.5f;
    public Color lightningColor = Color.white;
    public float lightningAmbientBoost = 1.2f;
    public GameObject stormLightningPrefab;
    public Vector3 lightningPrefabEuler = new Vector3(90f, 0f, 0f);
    public float lightningSpawnHeight = 16f;
    public float lightningHorizontalRange = 6f;
    public Vector2 lightningForwardRange = new Vector2(12f, 20f);
    public Vector2Int lightningFlashCountRange = new Vector2Int(1, 3);
    public float lightningFlashGap = 0.12f;
    public float lightningVisualLifetime = 1.8f;
    public AudioClip lightningSound;
    public float lightningSoundVolume = 0.85f;
    public float stormFogDensity = 0.12f;
    public float lightningFogDensity = 0.03f;

    [Header("Ice Mode Settings")]
    public float iceShooterWeightMultiplier = 1f;
    public float iceMushroomWeightMultiplier = 1f;
    public GameObject iceSnowPrefab;
    public Vector3 iceSnowOffset = new Vector3(0f, 12f, 0f);
    public bool iceSnowFollowPlayer = true;
    public Vector3 iceSnowWorldPosition = Vector3.zero;
    public Vector3 iceSnowAreaSize = new Vector3(60f, 30f, 60f);
    
    [Header("特殊规则")]
    [Tooltip("是否限时模式")]
    public bool isTimeLimited = false;
    
    [Tooltip("限时时长（秒）")]
    public float timeLimit = 180f;
    
    [Header("奖励设置")]
    [Tooltip("得分倍率")]
    public float scoreMultiplier = 1f;
    
    public void ApplyToGame(GameManager gameManager, PlayerController player)
    {
        if (player == null)
        {
            Debug.LogError("PlayerController is null!");
            return;
        }
        
        player.maxHealth *= initialHealthMultiplier;
        player.regenRate *= regenRateMultiplier;
        
        Debug.Log($"[游戏模式] 已应用模式 '{modeName}' 的设置");
        Debug.Log($"  - 初始血量倍率: {initialHealthMultiplier}x");
        Debug.Log($"  - 回血速度倍率: {regenRateMultiplier}x");
        Debug.Log($"  - 动态难度: {(useDynamicDifficulty ? "启用" : "禁用")}");
        Debug.Log($"  - 环境光照: {ambientLightMultiplier}x");
    }
    
    public float GetDifficultyAtDistance(float distance)
    {
        if (!useDynamicDifficulty || difficultyStages == null || difficultyStages.Length == 0)
        {
            return 1f;
        }
        
        foreach (var stage in difficultyStages)
        {
            if (distance >= stage.distanceStart && distance < stage.distanceEnd)
            {
                float t = (distance - stage.distanceStart) / (stage.distanceEnd - stage.distanceStart);
                return Mathf.Lerp(stage.difficultyStart, stage.difficultyEnd, t);
            }
        }
        
        if (difficultyStages.Length > 0)
        {
            var lastStage = difficultyStages[difficultyStages.Length - 1];
            if (distance >= lastStage.distanceEnd)
            {
                return lastStage.difficultyEnd;
            }
        }
        
        return 1f;
    }
    
    public float GetMinCountMultiplier(float difficulty)
    {
        if (!useDynamicDifficulty) return 1f;
        float maxDifficulty = difficultyStages != null && difficultyStages.Length > 0 ? 
            difficultyStages[difficultyStages.Length - 1].difficultyEnd : 5f;
        float t = Mathf.Clamp01((difficulty - 1f) / (maxDifficulty - 1f));
        return Mathf.Lerp(minCountMultiplierStart, minCountMultiplierEnd, t);
    }
    
    public float GetMaxCountMultiplier(float difficulty)
    {
        if (!useDynamicDifficulty) return 1f;
        float maxDifficulty = difficultyStages != null && difficultyStages.Length > 0 ? 
            difficultyStages[difficultyStages.Length - 1].difficultyEnd : 5f;
        float t = Mathf.Clamp01((difficulty - 1f) / (maxDifficulty - 1f));
        return Mathf.Lerp(maxCountMultiplierStart, maxCountMultiplierEnd, t);
    }
    
    public float GetSpacingMultiplier(float difficulty)
    {
        if (!useDynamicDifficulty) return 1f;
        float maxDifficulty = difficultyStages != null && difficultyStages.Length > 0 ? 
            difficultyStages[difficultyStages.Length - 1].difficultyEnd : 5f;
        float t = Mathf.Clamp01((difficulty - 1f) / (maxDifficulty - 1f));
        return Mathf.Lerp(spacingMultiplierStart, spacingMultiplierEnd, t);
    }
}
