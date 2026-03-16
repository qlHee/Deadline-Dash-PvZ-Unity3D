using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("障碍生成范围")]
    public Material obstacleMaterial;
    public float spawnDistance = 80f;
    public float minSpawnDistance = 70f;
    public float despawnDistance = 30f;
    public float safeZoneDistance = 20f;

    [Header("障碍间距")]
    public float minObstacleDistance = 8f;
    public float maxObstacleDistance = 15f;


    [Header("射手伤害")]
    public float peashooterDamage = 37f;

    [Header("豌豆射手")]
    public float peaProjectileHeight = 1.2f;
    public float peaProjectileDamage = 27f;
    public float peashooterScale = 1.5f;
    public GameObject peashooterModel;

    [Header("寒冰射手")]
    public float iceShooterDamage = 37f;
    public float iceShooterScale = 1.5f;
    public GameObject iceShooterModel;

    [Header("双重射手")]
    public float doubleShooterDamage = 37f;
    public float doubleShooterScale = 1.5f;
    public GameObject doubleShooterModel;

    [Header("三重射手")]
    public float tripleShooterDamage = 37f;
    public float tripleShooterScale = 1.5f;
    public GameObject tripleShooterModel;

    [Header("火焰树桩")]
    public float fireStumpDamage = 17f;
    public float fireStumpScale = 1.5f;
    public float fireStumpBurningTime = 3f;
    public float fireStumpBurningDamage = 5f;
    public GameObject fireStumpModel;

    [Header("坚果")]
    public float nutDamage = 37f;
    public float nutScale = 1.5f;
    public GameObject nutModel;

    [Header("高坚果")]
    public float tallNutDamage = 37f;
    public float tallNutScale = 2.5f;
    public GameObject tallNutModel;

    [Header("猫尾草")]
    public float cattailShooterDamage = 47f;
    public float cattailShooterScale = 1.5f;
    public float cattailProjectileDamage = 27f;
    public GameObject cattailShooterModel;

    [Header("仙人掘")]
    public float cactusDamage = 47f;
    public float cactusScale = 1.5f;
    public float cactusProjectileDamage = 27f;
    public float cactusSpikeHeight = 2.5f;
    public GameObject cactusModel;

    [Header("冰菇")]
    public float iceMushroomDamage = 17f;
    public float iceMushroomScale = 1.5f;
    public float iceMushroomFreezeDuration = 2f;
    public GameObject iceMushroomModel;
    public GameObject iceMushroomSelfEffectPrefab;
    public float iceMushroomSelfEffectLifetime = 2f;
    public GameObject iceMushroomSelfEffectPrefab2;
    public float iceMushroomSelfEffectLifetime2 = 2f;
    public GameObject iceMushroomPlayerEffectPrefab;
    public float iceMushroomPlayerEffectLifetime = 2f;
    
    [Header("土豆地雷")]
    public float potatoMineDamage = 40f;
    public float potatoMineScale = 1.2f;
    public float potatoMineActivationDistance = 8f;
    public float potatoMineDeactivationDistance = 10f;
    [Range(0f, 1f)] public float potatoMineBuriedHeightRatio = 0.66f;
    public float potatoMineAscendSpeed = 2.5f;
    public float potatoMineDescendSpeed = 3f;
    public GameObject potatoMinePrefab;
    public GameObject potatoMineTriggerEffectPrefab;
    
    [Header("火爆辣椒")]
    public float firePepperTriggerRadius = 6f;
    public float firePepperGrowDuration = 0.8f;
    public float firePepperMaxScaleMultiplier = 1.6f;
    public float firePepperWobbleDuration = 0.25f;
    public float firePepperWobbleMagnitude = 0.1f;
    public float firePepperExplosionDamage = 50f;
    public float firePepperBurnDuration = 3f;
    public float firePepperBurnDamagePerSecond = 12f;
    public float firePepperBurnHeight = 1.4f;
    public float firePepperVerticalLength = 8f;
    public float firePepperVerticalWidth = 1.5f;
    public float firePepperHorizontalWidth = 12f;
    public float firePepperHorizontalDepth = 3f;
    public GameObject firePepperPrefab;
    public GameObject firePepperEffectPrefab;
    public float firePepperEffectLifetime = 3f;
    public bool firePepperAutoScaleEffect = true;
    
    [Header("樱桃炸弹")]
    public float cherryBombDamage = 47f;
    public float cherryBombScale = 1.5f;
    public float cherryBombTriggerRadius = 5f;
    public float cherryBombExplosionRadius = 10f;
    public float cherryBombExplosionDelay = 1f; // 添加爆炸延时时间配置
    public GameObject cherryBombModel;
    public GameObject explosionEffectPrefab;
    

    [Header("尖刺子弹")]
    public float spikeProjectileDamage = 27f;
    public GameObject spikeProjectileModel;
    [Header("回血向日葵")]
    public float sunflowerHealAmount = 35f;
    public float sunflowerScale = 1.5f;
    public float sunflowerRotateSpeed = 0f;
    public Vector3 sunflowerVisualOffset = Vector3.zero;
    public Vector3 sunflowerIdleEffectOffset = Vector3.zero;
    public Vector3 sunflowerIdleEffectScale = Vector3.one;
    public Vector3 sunflowerCollectEffectOffset = Vector3.zero;
    public GameObject sunflowerModel;
    public Vector3 sunflowerModelRotation = Vector3.zero;
    public GameObject sunflowerIdleEffect;
    public GameObject sunflowerCollectEffect;
    [Header("护盾拾取")]
    public float shieldPickupScale = 1.2f;
    public float shieldDuration = 8f;
    public float shieldRotateSpeed = 30f;
    public Vector3 shieldRotationAxis = Vector3.up;
    public Vector3 shieldVisualOffset = Vector3.zero;
    public Vector3 shieldModelRotation = Vector3.zero;
    public Vector3 shieldModelScale = Vector3.one;
    public Vector3 shieldIdleEffectOffset = Vector3.zero;
    public Vector3 shieldPickupEffectOffset = Vector3.zero;
    public GameObject normalShieldModel;
    public GameObject fireShieldModel;
    public GameObject iceShieldModel;
    public GameObject normalShieldIdleEffect;
    public GameObject fireShieldIdleEffect;
    public GameObject iceShieldIdleEffect;
    public GameObject shieldPickupEffectPrefab;
    public Color normalShieldColor = new Color(1f, 0.85f, 0.2f, 0.8f);
    public Color fireShieldColor = new Color(1f, 0.45f, 0.2f, 0.8f);
    public Color iceShieldColor = new Color(0.4f, 0.8f, 1f, 0.8f);
    [Header("跳跃增益拾取")]
    public float jumpPickupScale = 1.2f;
    public float jumpBoostDuration = 8f;
    public float jumpBoostMultiplier = 5f;
    public float jumpRotateSpeed = 40f;
    public Vector3 jumpVisualOffset = Vector3.zero;
    public Vector3 jumpIdleEffectOffset = Vector3.zero;
    public Vector3 jumpPickupEffectOffset = Vector3.zero;
    public GameObject jumpPickupModel;
    public GameObject jumpPickupIdleEffect;
    public GameObject jumpPickupCollectEffect;
    public GameObject jumpPickupPlayerEffect;
    public float jumpPlayerEffectLifetime = 1f;
    public Vector3 jumpPlayerEffectOffset = Vector3.zero;
    [Header("增益拾取控制")]
    [Range(0f, 1f)] public float supportPickupChance = 0.25f;
    public float supportPickupMinDistance = 35f;
    public float sunflowerSpawnWeight = 0.35f;
    public float shieldSpawnWeight = 0.45f;
    public float jumpSpawnWeight = 0.2f;

    [Header("道路设置")]
    [Tooltip("道路宽度从 RoadGenerator 自动获取")]
    [SerializeField] private float roadWidth = 12f;
    public float roadMargin = 1f;

    [Header("虚拟网格系统")]
    [Tooltip("网格大小固定为1x1米")]
    private const float gridSize = 1f;
    [Tooltip("每行最少生成障碍物数量")]
    [Range(1, 5)]
    public int minObstaclesPerRow = 1;
    [Tooltip("每行最多生成障碍物数量")]
    [Range(1, 10)]
    public int maxObstaclesPerRow = 3;
    [Tooltip("在Scene和Game视图中显示虚拟网格")]
    public bool showGrid = false;
    [Tooltip("网格显示的前后范围")]
    public float gridDisplayRange = 100f;

    [Header("模型障碍（优先使用）")]
    public List<GameObject> obstaclePrefabs = new List<GameObject>();
    public Vector3 prefabScale = Vector3.one;
    public bool autoAddCollider = true;
    public bool forceTriggerCollider = true;
    [Tooltip("用于随机排布时的预估半宽，帮助避免重叠")]
    public float estimatedHalfWidth = 0.6f;

    [Header("动态生成距离")]
    public float lookAheadTime = 3f;

    [Header("动态难度系统")]
    [Tooltip("游戏难度开始明显提升的得分阈值")]
    public float baseScoreThreshold = 300f; // 提高到300，使难度提升更加缓慢
    [Tooltip("难度系数最大限制")]
    public float maxDifficultyFactor = 3f; // 降低到3，限制最高难度
    [Tooltip("难度变化平滑系数")]
    [Range(0.01f, 0.5f)]
    public float difficultyTransitionSpeed = 0.03f; // 降低到0.03，使过渡更加平滑

    // 缓存原始设置
    private int initialMinObstaclesPerRow;
    private int initialMaxObstaclesPerRow;
    private float initialMinObstacleDistance;
    private float initialMaxObstacleDistance;

    // 难度系数
    private float currentDifficultyFactor = 0f;
    private float targetDifficultyFactor = 0f;
    
    // 障碍物难度等级权重
    private Dictionary<int, float> difficultyLevelWeights = new Dictionary<int, float>();
    private float lastPlayerScore = 0f;

    private Transform playerTransform;
    private PlayerController playerController;
    private readonly List<GameObject> activeObstacles = new List<GameObject>();
    private readonly Dictionary<float, HashSet<int>> occupiedGrids = new Dictionary<float, HashSet<int>>();
    private readonly Dictionary<float, Dictionary<int, ObstacleType>> rowObstacleTypes = new Dictionary<float, Dictionary<int, ObstacleType>>();
    private readonly List<ObstacleType> recentObstacleHistory = new List<ObstacleType>();
    private readonly List<bool> recentEdgeObstacles = new List<bool>();
    private float nextSpawnZ = 30f;
    private RoadGenerator roadGenerator;
    private int gridCountX;
    private float lastSupportSpawnZ = Mathf.NegativeInfinity;

    private enum ObstacleType
    {
        Peashooter,
        IceShooter,
        DoubleShooter,
        TripleShooter,
        FireStump,
        Nut,
        TallNut,
        CattailShooter,
        Cactus,
        IceMushroom,
        CherryBomb,
        PotatoMine,
        FirePepper,
        Sunflower,
        ShieldPickup,
        JumpBoost
    }

    private struct GridPosition
    {
        public int gridX;
        public float worldX;
        public float worldZ;
    }

    void OnValidate()
    {
        if (maxObstacleDistance < minObstacleDistance)
        {
            maxObstacleDistance = minObstacleDistance;
        }
        if (minObstaclesPerRow < 1) minObstaclesPerRow = 1;
        if (maxObstaclesPerRow < minObstaclesPerRow) maxObstaclesPerRow = minObstaclesPerRow;
    }

    void Start()
    {
        roadGenerator = GetComponent<RoadGenerator>();
        if (roadGenerator != null)
        {
            roadWidth = roadGenerator.GetRoadWidth();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[ObstacleSpawner] 未找到 Player，无法生成障碍。");
            enabled = false;
            return;
        }

        playerTransform = player.transform;
        playerController = player.GetComponent<PlayerController>();
        nextSpawnZ = playerTransform.position.z + Mathf.Max(minSpawnDistance, safeZoneDistance);

        // 初始化动态难度系统
        InitializeDifficultySystem();

        CalculateGridCount();
        PrewarmObstacles();
    }

    void InitializeDifficultySystem()
    {
        // 缓存原始参数
        initialMinObstaclesPerRow = minObstaclesPerRow;
        initialMaxObstaclesPerRow = maxObstaclesPerRow;
        initialMinObstacleDistance = minObstacleDistance;
        initialMaxObstacleDistance = maxObstacleDistance;
        
        // 降低初始难度 - 减少障碍物数量，增加间距
        minObstaclesPerRow = Mathf.Max(1, initialMinObstaclesPerRow - 1); // 确保至少有1个
        maxObstaclesPerRow = Mathf.Max(minObstaclesPerRow, initialMaxObstaclesPerRow - 1);
        minObstacleDistance = initialMinObstacleDistance * 1.2f; // 增加20%间距
        maxObstacleDistance = initialMaxObstacleDistance * 1.2f;
        
        // 初始化难度等级权重 - 初期只出现难度等级1的障碍物
        difficultyLevelWeights[1] = 1.0f; // 难度等级 1 的初始权重
        difficultyLevelWeights[2] = 0.0f; // 难度等级 2 初始不出现
        difficultyLevelWeights[3] = 0.0f; // 难度等级 3 初始不出现

        // 初始化难度系数
        currentDifficultyFactor = 0f;
        targetDifficultyFactor = 0f;
        lastPlayerScore = 0f;
        
        Debug.Log("[难度系统] 初始化完成 - 降低初始难度");
    }

    void Update()
    {
        if (playerController != null && playerController.IsGameOver())
        {
            return;
        }

        // 更新动态难度系统
        UpdateDifficultySystem();

        float requiredAhead = GetRequiredAheadDistance();
        while (nextSpawnZ - playerTransform.position.z < requiredAhead)
        {
            SpawnObstacleRow();
        }

        RemoveOldObstacles();
    }

    void UpdateDifficultySystem()
    {
        if (playerController == null) return;

        // 获取当前玩家得分（这里用总距离作为得分）
        float playerScore = 0f;
        
        // 从PlayerController中获取玩家总行走距离作为得分
        playerScore = playerController.GetTotalDistance();

        // 调试信息
        if (playerScore > 0 && playerScore % 50f < 0.5f)
        {
            Debug.Log($"[难度系统] 当前玩家得分: {playerScore:F1}米");
        }

        // 每得分变化5米才更新一次难度参数，避免频繁计算
        if (Mathf.Abs(playerScore - lastPlayerScore) < 5f) return;

        lastPlayerScore = playerScore;

        // 计算目标难度系数 - 用户指定的8阶段系统（v7）
        if (playerScore <= 200f) {
            // 阶段1: 0-200米，难度 1.0 -> 2.5
            float progress = Mathf.Clamp01(playerScore / 200f);
            targetDifficultyFactor = Mathf.Lerp(1.0f, 2.5f, progress);
        } else if (playerScore <= 400f) {
            // 阶段2: 200-400米，难度 2.5 -> 3.0
            float progress = Mathf.Clamp01((playerScore - 200f) / 200f);
            targetDifficultyFactor = Mathf.Lerp(2.5f, 3.0f, progress);
        } else if (playerScore <= 800f) {
            // 阶段3: 400-800米，难度 3.0 -> 3.5
            float progress = Mathf.Clamp01((playerScore - 400f) / 400f);
            targetDifficultyFactor = Mathf.Lerp(3.0f, 3.5f, progress);
        } else if (playerScore <= 1600f) {
            // 阶段4: 800-1600米，难度 3.0 -> 3.6
            float progress = Mathf.Clamp01((playerScore - 800f) / 800f);
            targetDifficultyFactor = Mathf.Lerp(3.0f, 3.6f, progress);
        } else if (playerScore <= 3200f) {
            // 阶段5: 1600-3200米，难度 3.6 -> 3.9
            float progress = Mathf.Clamp01((playerScore - 1600f) / 1600f);
            targetDifficultyFactor = Mathf.Lerp(3.6f, 3.9f, progress);
        } else if (playerScore <= 6400f) {
            // 阶段6: 3200-6400米，难度 3.9 -> 4.2
            float progress = Mathf.Clamp01((playerScore - 3200f) / 3200f);
            targetDifficultyFactor = Mathf.Lerp(3.9f, 4.2f, progress);
        } else if (playerScore <= 12800f) {
            // 阶段7: 6400-12800米，难度 4.2 -> 4.5
            float progress = Mathf.Clamp01((playerScore - 6400f) / 6400f);
            targetDifficultyFactor = Mathf.Lerp(4.2f, 4.5f, progress);
        } else {
            // 阶段8: 12800米以上，常量 9.0
            targetDifficultyFactor = 9.0f;
        }
        
        // 平滑过渡当前难度系数
        currentDifficultyFactor = Mathf.Lerp(currentDifficultyFactor, targetDifficultyFactor, difficultyTransitionSpeed);

        // 调整障碍物生成参数
        float difficulty = currentDifficultyFactor;
        // 统一用难度系数驱动所有参数（不再按阶段硬编码）
        // 新曲线的最大难度为9.0，这里将归一化区间设置为[1.0, 9.0]
        float t = Mathf.InverseLerp(1.0f, 9.0f, Mathf.Clamp(difficulty, 1.0f, 9.0f));

        // 1. 按难度系数调整每行障碍物数量（难度越高数量越多）
        // 早期(<~4.0)基本不放大，>=4后再加速放大，避免前期行内数量暴增
        float countRamp = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(4.0f, 9.0f, difficulty));
        float maxCountScale = Mathf.Lerp(1.0f, 1.25f, countRamp);
        float minCountScale = Mathf.Lerp(1.0f, 1.17f, countRamp);
        int newMinCount = Mathf.Max(1, Mathf.FloorToInt(initialMinObstaclesPerRow * minCountScale));
        int newMaxCount = Mathf.Max(newMinCount, Mathf.FloorToInt(initialMaxObstaclesPerRow * maxCountScale));
        minObstaclesPerRow = newMinCount;
        maxObstaclesPerRow = newMaxCount;
        
        // 确保最小值不超过最大值
        minObstaclesPerRow = Mathf.Min(minObstaclesPerRow, maxObstaclesPerRow);

        // 2. 按难度系数调整障碍物间距（难度越高间距越小）
        // 早期更稀疏：从1.30逐步收敛到1.00，保证前期不会过密
        float distanceRamp = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(2.5f, 9.0f, difficulty));
        float distanceScale = Mathf.Lerp(1.30f, 1.00f, distanceRamp);
        minObstacleDistance = initialMinObstacleDistance * distanceScale;
        maxObstacleDistance = initialMaxObstacleDistance * distanceScale;

        // 3. 按难度系数调整难度等级权重（仅保留出现门槛，权重随难度变化）
        // 基于难度的插值参数
        float wt = t;
        // 等级1：难度越高，占比越低
        difficultyLevelWeights[1] = Mathf.Lerp(1.10f, 0.50f, wt);
        // 等级2：200米后出现，随难度上升
        difficultyLevelWeights[2] = (playerScore >= 200f) ? Mathf.Lerp(0.40f, 1.00f, wt) : 0f;
        // 等级3：推迟到800米后逐步开放，前期强限制
        if (playerScore < 800f)
        {
            difficultyLevelWeights[3] = 0f;
        }
        else if (playerScore < 1600f)
        {
            float pf = Mathf.Clamp01((playerScore - 800f) / 800f); // 800-1600m 线性增长
            difficultyLevelWeights[3] = Mathf.Lerp(0.05f, 0.20f, pf) * Mathf.Lerp(0.8f, 1.0f, wt);
        }
        else
        {
            difficultyLevelWeights[3] = Mathf.Lerp(0.20f, 1.20f, wt);
        }

        if (difficulty > 0.5f && difficulty % 0.5f < 0.05f)
        {
            Debug.Log($"[难度系统] 当前难度系数: {difficulty:F2}, " +
                      $"障碍物数量: {minObstaclesPerRow}-{maxObstaclesPerRow}, " +
                      $"障碍物间距: {minObstacleDistance:F1}-{maxObstacleDistance:F1}, " +
                      $"难度等级权重: L1={difficultyLevelWeights[1]:F1}, L2={difficultyLevelWeights[2]:F1}, L3={difficultyLevelWeights[3]:F1}");
        }
    }

    void SpawnObstacleRow()
    {
        if (gridCountX <= 0)
        {
            Debug.LogWarning("[ObstacleSpawner] 网格数量为0，无法生成障碍物");
            nextSpawnZ += maxObstacleDistance;
            return;
        }

        int obstacleCount = Random.Range(minObstaclesPerRow, maxObstaclesPerRow + 1);
        obstacleCount = Mathf.Min(obstacleCount, gridCountX);

        if (obstacleCount <= 0)
        {
            nextSpawnZ += Random.Range(minObstacleDistance, maxObstacleDistance);
            return;
        }

        HashSet<int> usedGrids = new HashSet<int>();
        Dictionary<int, ObstacleType> currentRowTypes = new Dictionary<int, ObstacleType>();
        int attemptsCount = 0;
        int maxAttempts = obstacleCount * 3; // 限制尝试次数，防止无限循环
        bool supportSpawnedThisRow = false;
        bool forceSupportThisRow = ShouldForceSupportPickup();

        for (int i = 0; i < obstacleCount && attemptsCount < maxAttempts; attemptsCount++)
        {
            // 先选择障碍物类型
            ObstacleType type;
            if (forceSupportThisRow && !supportSpawnedThisRow)
            {
                type = GetRandomSupportPickupType();
            }
            else
            {
                type = GetRandomObstacleTypeWithConstraints();
            }
            if (supportSpawnedThisRow && IsSupportPickup(type))
            {
                continue;
            }
            
            // 判断该类型是否不能出现在边缘
            bool excludeEdge = (type == ObstacleType.IceMushroom || type == ObstacleType.Cactus);
            
            // 判断该类型是否只能出现在边缘两格（樱桃炸弹）
            bool restrictToEdgeTwoGrids = (type == ObstacleType.CherryBomb);
            
            // 根据类型选择合适的网格位置
            bool forceEdge = !excludeEdge && ShouldForceEdgePlacement();
            GridPosition gridPos = GetRandomAvailableGrid(usedGrids, forceEdge, excludeEdge, restrictToEdgeTwoGrids);
            if (gridPos.gridX == -1) 
            {
                // 没有可用格子，中止当前行的障碍物生成
                Debug.Log("[ObstacleSpawner] 根据新规则，没有可用格子生成障碍物，已生成" + usedGrids.Count + "个障碍物");
                break;
            }

            SpawnObstacleAtGrid(type, gridPos);
            
            usedGrids.Add(gridPos.gridX);
            currentRowTypes[gridPos.gridX] = type;
            if (IsSupportPickup(type))
            {
                supportSpawnedThisRow = true;
                lastSupportSpawnZ = gridPos.worldZ;
            }
            
            recentObstacleHistory.Add(type);
            if (recentObstacleHistory.Count > 10)
            {
                recentObstacleHistory.RemoveAt(0);
            }

            bool isEdge = (gridPos.gridX == 0 || gridPos.gridX == gridCountX - 1);
            recentEdgeObstacles.Add(isEdge);
            if (recentEdgeObstacles.Count > 10)
            {
                recentEdgeObstacles.RemoveAt(0);
            }
            
            i++; // 只有成功生成了障碍物才增加计数
        }

        if (usedGrids.Count > 0)
        {
            occupiedGrids[nextSpawnZ] = usedGrids;
            rowObstacleTypes[nextSpawnZ] = currentRowTypes;
        }

        float distance = Random.Range(minObstacleDistance, maxObstacleDistance);
        nextSpawnZ += distance;
    }

    void CalculateGridCount()
    {
        float usableWidth = roadWidth - roadMargin * 2f;
        gridCountX = Mathf.FloorToInt(usableWidth / gridSize);
        Debug.Log($"[ObstacleSpawner] 虚拟网格系统初始化: 道路宽度={roadWidth}, 可用宽度={usableWidth}, 网格数量={gridCountX}");
    }

    bool ShouldForceEdgePlacement()
    {
        const int minEdgeInTen = 3;
        
        if (recentEdgeObstacles.Count < 10)
        {
            return false;
        }

        int edgeCount = 0;
        for (int i = 0; i < recentEdgeObstacles.Count; i++)
        {
            if (recentEdgeObstacles[i])
            {
                edgeCount++;
            }
        }

        return edgeCount < minEdgeInTen;
    }

    GridPosition GetRandomAvailableGrid(HashSet<int> usedGrids, bool forceEdge = false, bool excludeEdge = false, bool restrictToEdgeTwoGrids = false)
    {
        List<int> availableGrids = new List<int>();
        List<int> edgeGrids = new List<int>();
        
        // 查找前一行、当前行和后一行的已占用网格
        HashSet<int> occupiedOrAdjacentGrids = new HashSet<int>();
        
        // 步骤1: 标记当前行已占用的格子
        foreach (int grid in usedGrids)
        {
            // 当前格子标记为已占用
            occupiedOrAdjacentGrids.Add(grid);
        }
        
        // 步骤2: 标记当前行已占用格子的相邻格子（左右）
        foreach (int grid in usedGrids)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int neighborX = grid + dx;
                if (neighborX >= 0 && neighborX < gridCountX)
                {
                    occupiedOrAdjacentGrids.Add(neighborX);
                }
            }
        }
        
        // 步骤3: 检查前后相邻行的障碍物
        // 我们就检查规则中所说的相邻行，也就是最小障碍物距离的前后行
        float[] adjacentRows = new float[] {
            nextSpawnZ - minObstacleDistance,  // 前一行
            nextSpawnZ + minObstacleDistance   // 后一行
        };
        
        foreach (float rowZ in adjacentRows)
        {
            if (occupiedGrids.ContainsKey(rowZ))
            {
                foreach (int grid in occupiedGrids[rowZ])
                {
                    // 标记定位置格子为不可用
                    occupiedOrAdjacentGrids.Add(grid);
                    
                    // 标记周围的格子为不可用
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int neighborX = grid + dx;
                        if (neighborX >= 0 && neighborX < gridCountX)
                        {
                            occupiedOrAdjacentGrids.Add(neighborX);
                        }
                    }
                }
            }
        }
        
        // 然后确认所有行中最近的前后两行是否存在不符合分布的障碍物
        foreach (float rowZ in occupiedGrids.Keys)
        {
            // 只检查距离当前行小于最小障碍物距离的行，这些行上的障碍物会影响到当前行
            if (Mathf.Abs(rowZ - nextSpawnZ) < minObstacleDistance)
            {
                foreach (int grid in occupiedGrids[rowZ])
                {
                    // 标记周围的格子为不可用
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int neighborX = grid + dx;
                        if (neighborX >= 0 && neighborX < gridCountX)
                        {
                            occupiedOrAdjacentGrids.Add(neighborX);
                        }
                    }
                }
            }
        }
        
        // 找出可用的格子
        for (int x = 0; x < gridCountX; x++)
        {
            if (!occupiedOrAdjacentGrids.Contains(x))
            {
                bool isEdge = (x == 0 || x == gridCountX - 1);
                
                // 如果需要排除边缘且当前是边缘位置，则跳过
                if (excludeEdge && isEdge)
                {
                    continue;
                }
                
                // 如果需要限制在边缘两格（樱桃炸弹），只允许最左两格和最右两格
                if (restrictToEdgeTwoGrids)
                {
                    bool isInEdgeTwoGrids = (x <= 1 || x >= gridCountX - 2);
                    if (!isInEdgeTwoGrids)
                    {
                        continue;
                    }
                }
                
                availableGrids.Add(x);
                if (isEdge)
                {
                    edgeGrids.Add(x);
                }
            }
        }

        if (availableGrids.Count == 0)
        {
            return new GridPosition { gridX = -1, worldX = 0f, worldZ = nextSpawnZ };
        }

        int selectedGrid;
        if (forceEdge && edgeGrids.Count > 0)
        {
            selectedGrid = edgeGrids[Random.Range(0, edgeGrids.Count)];
        }
        else
        {
            selectedGrid = availableGrids[Random.Range(0, availableGrids.Count)];
        }

        float worldX = GridToWorldX(selectedGrid);

        return new GridPosition
        {
            gridX = selectedGrid,
            worldX = worldX,
            worldZ = nextSpawnZ
        };
    }

    float GridToWorldX(int gridX)
    {
        float usableWidth = roadWidth - roadMargin * 2f;
        float gridStartX = -(usableWidth / 2f);
        return gridStartX + (gridX + 0.5f) * gridSize;
    }

    void SpawnObstacleAtGrid(ObstacleType type, GridPosition gridPos)
    {
        float size = 1.5f; // 默认尺寸
        if (type == ObstacleType.Peashooter) size = Mathf.Max(0.1f, peashooterScale);
        else if (type == ObstacleType.IceShooter) size = Mathf.Max(0.1f, iceShooterScale);
        else if (type == ObstacleType.DoubleShooter) size = Mathf.Max(0.1f, doubleShooterScale);
        else if (type == ObstacleType.TripleShooter) size = Mathf.Max(0.1f, tripleShooterScale);
        else if (type == ObstacleType.FireStump) size = Mathf.Max(0.1f, fireStumpScale);
        else if (type == ObstacleType.Nut) size = Mathf.Max(0.1f, nutScale);
        else if (type == ObstacleType.TallNut) size = Mathf.Max(0.1f, tallNutScale);
        else if (type == ObstacleType.CattailShooter) size = Mathf.Max(0.1f, cattailShooterScale);
        else if (type == ObstacleType.Cactus) size = Mathf.Max(0.1f, cactusScale);
        else if (type == ObstacleType.IceMushroom) size = Mathf.Max(0.1f, iceMushroomScale);
        else if (type == ObstacleType.CherryBomb) size = Mathf.Max(0.1f, cherryBombScale);
        else if (type == ObstacleType.PotatoMine) size = Mathf.Max(0.1f, potatoMineScale);
        else if (type == ObstacleType.FirePepper) size = Mathf.Max(0.1f, 1f);
        else if (type == ObstacleType.Sunflower) size = Mathf.Max(0.1f, sunflowerScale);
        else if (type == ObstacleType.ShieldPickup) size = Mathf.Max(0.1f, shieldPickupScale);
        else if (type == ObstacleType.JumpBoost) size = Mathf.Max(0.1f, jumpPickupScale);

        GameObject obstacle = CreateObstacle(type, size);
        if (obstacle == null) return;
        bool isSupportPickup = IsSupportPickup(type);

        obstacle.name = $"Obstacle_{activeObstacles.Count}_Grid{gridPos.gridX}";
        obstacle.transform.SetParent(transform);
        obstacle.transform.position = new Vector3(gridPos.worldX, 0f, gridPos.worldZ);

        AlignToGround(obstacle);
        // 如果是土豆地雷，确保地表位置按对齐后的高度初始化
        PotatoMine mineScript = obstacle.GetComponent<PotatoMine>();
        if (mineScript != null)
        {
            mineScript.InitializePositions(obstacle.transform.position);
        }

        Collider collider = EnsureCollider(obstacle);
        if (collider != null && forceTriggerCollider)
        {
            collider.isTrigger = true;
        }

        if (!isSupportPickup)
        {
            TagUtility.TryAssignTag(obstacle, "Obstacle");
        }

        // 排除有自己碰撞处理的障碍物：FireStump, Nut, TallNut, FirePepper, Cactus, CattailShooter
        if (!isSupportPickup && type != ObstacleType.FireStump && type != ObstacleType.Nut && type != ObstacleType.TallNut && 
            type != ObstacleType.FirePepper && type != ObstacleType.Cactus && type != ObstacleType.CattailShooter)
        {
            ObstacleCollision obstacleCollision = obstacle.GetComponent<ObstacleCollision>();
            if (obstacleCollision == null)
            {
                obstacleCollision = obstacle.AddComponent<ObstacleCollision>();
            }
            obstacleCollision.damage = GetDamageByType(type);
        }

        Renderer renderer = obstacle.GetComponentInChildren<Renderer>();
        if (renderer != null && obstaclePrefabs.Count == 0)
        {
            if (obstacleMaterial != null)
            {
                renderer.material = obstacleMaterial;
            }
            else
            {
                renderer.material.color = GetRandomFallbackColor();
            }
        }

        activeObstacles.Add(obstacle);
    }


    GameObject CreateObstacle(ObstacleType type, float size)
    {
        bool shouldUsePrefab = obstaclePrefabs != null && obstaclePrefabs.Count > 0 && type != ObstacleType.Peashooter && type != ObstacleType.IceShooter && type != ObstacleType.DoubleShooter && type != ObstacleType.TripleShooter && type != ObstacleType.FireStump && type != ObstacleType.Nut && type != ObstacleType.TallNut && type != ObstacleType.CattailShooter && type != ObstacleType.Cactus && type != ObstacleType.IceMushroom && type != ObstacleType.CherryBomb && type != ObstacleType.PotatoMine && type != ObstacleType.FirePepper && type != ObstacleType.Sunflower && type != ObstacleType.ShieldPickup && type != ObstacleType.JumpBoost;
        if (shouldUsePrefab)
        {
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            if (prefab == null) return null;
            GameObject instance = Instantiate(prefab);
            instance.transform.localScale = Vector3.Scale(instance.transform.localScale, prefabScale);
            return instance;
        }

        GameObject obstacle = null;
        switch (type)
        {
            case ObstacleType.Peashooter:
                obstacle = CreatePeashooter(size);
                break;
            case ObstacleType.IceShooter:
                obstacle = CreateIceShooter(size);
                break;
            case ObstacleType.DoubleShooter:
                obstacle = CreateDoubleShooter(size);
                break;
            case ObstacleType.TripleShooter:
                obstacle = CreateTripleShooter(size);
                break;
            case ObstacleType.FireStump:
                obstacle = CreateFireStump(size);
                break;
            case ObstacleType.Nut:
                obstacle = CreateNut(size);
                break;
            case ObstacleType.TallNut:
                obstacle = CreateTallNut(size);
                break;
            case ObstacleType.CattailShooter:
                obstacle = CreateCattailShooter(size);
                break;
            case ObstacleType.Cactus:
                obstacle = CreateCactus(size);
                break;
            case ObstacleType.IceMushroom:
                obstacle = CreateIceMushroom(size);
                break;
            case ObstacleType.CherryBomb:
                obstacle = CreateCherryBomb(size);
                break;
            case ObstacleType.PotatoMine:
                obstacle = CreatePotatoMine(size);
                break;
            case ObstacleType.FirePepper:
                obstacle = CreateFirePepper(size);
                break;
            case ObstacleType.Sunflower:
                obstacle = CreateSunflowerPickup(size);
                break;
            case ObstacleType.ShieldPickup:
                obstacle = CreateShieldPickup(size);
                break;
            case ObstacleType.JumpBoost:
                obstacle = CreateJumpBoostPickup(size);
                break;
        }
        return obstacle;
    }

    GameObject CreatePeashooter(float size)
    {
        GameObject peashooter = new GameObject("Peashooter");

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(peashooter.transform, false);

        if (peashooterModel != null)
        {
            GameObject visual = Instantiate(peashooterModel, peashooter.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = peashooter.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
        }
        else
        {
            float stemHeight = Mathf.Max(1f, size * 1.3f);
            float stemRadius = Mathf.Max(0.2f, size * 0.25f);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(peashooter.transform, false);
            stem.transform.localScale = new Vector3(stemRadius, stemHeight * 0.5f, stemRadius);
            Renderer stemRenderer = stem.GetComponent<Renderer>();
            if (stemRenderer != null)
            {
                stemRenderer.material.color = new Color(0.2f, 0.65f, 0.2f);
            }
            Collider stemCollider = stem.GetComponent<Collider>();
            if (stemCollider != null)
            {
                Destroy(stemCollider);
            }

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(peashooter.transform, false);
            float headSize = Mathf.Max(0.6f, size * 0.7f);
            head.transform.localScale = Vector3.one * headSize;
            head.transform.localPosition = new Vector3(0f, stemHeight, 0f);
            Renderer headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                headRenderer.material.color = new Color(0.45f, 0.9f, 0.35f);
            }
            Collider headCollider = head.GetComponent<Collider>();
            if (headCollider != null)
            {
                Destroy(headCollider);
            }

            muzzle.transform.localPosition = head.transform.localPosition + new Vector3(0f, 0f, -headSize * 0.6f);
        }

        Peashooter shooter = peashooter.AddComponent<Peashooter>();
        shooter.AssignMuzzle(muzzle.transform);
        shooter.SetProjectileHeight(peaProjectileHeight);
        shooter.SetProjectileDamage(peaProjectileDamage);
        
        // 确保有碰撞体（用于玩家碰撞检测）
        BoxCollider collider = peashooter.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = peashooter.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
        collider.size = new Vector3(size, size * 2f, size);
        collider.center = new Vector3(0f, size, 0f);

        return peashooter;
    }

    GameObject CreateIceShooter(float size)
    {
        GameObject iceShooter = new GameObject("IceShooter");

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(iceShooter.transform, false);

        if (iceShooterModel != null)
        {
            GameObject visual = Instantiate(iceShooterModel, iceShooter.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = iceShooter.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
        }
        else
        {
            float stemHeight = Mathf.Max(1f, size * 1.3f);
            float stemRadius = Mathf.Max(0.2f, size * 0.25f);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(iceShooter.transform, false);
            stem.transform.localScale = new Vector3(stemRadius, stemHeight * 0.5f, stemRadius);
            Renderer stemRenderer = stem.GetComponent<Renderer>();
            if (stemRenderer != null)
            {
                stemRenderer.material.color = new Color(0.2f, 0.65f, 0.2f);
            }
            Collider stemCollider = stem.GetComponent<Collider>();
            if (stemCollider != null)
            {
                Destroy(stemCollider);
            }

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(iceShooter.transform, false);
            float headSize = Mathf.Max(0.6f, size * 0.7f);
            head.transform.localScale = Vector3.one * headSize;
            head.transform.localPosition = new Vector3(0f, stemHeight, 0f);
            Renderer headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                headRenderer.material.color = new Color(0.45f, 0.9f, 0.35f);
            }
            Collider headCollider = head.GetComponent<Collider>();
            if (headCollider != null)
            {
                Destroy(headCollider);
            }

            muzzle.transform.localPosition = head.transform.localPosition + new Vector3(0f, 0f, -headSize * 0.6f);
        }

        IceShooter shooter = iceShooter.AddComponent<IceShooter>();
        shooter.AssignMuzzle(muzzle.transform);
        shooter.SetProjectileHeight(peaProjectileHeight);
        shooter.SetProjectileDamage(peaProjectileDamage);
        
        // 确保有碰撞体（用于玩家碰撞检测）
        BoxCollider collider = iceShooter.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = iceShooter.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
        collider.size = new Vector3(size, size * 2f, size);
        collider.center = new Vector3(0f, size, 0f);

        return iceShooter;
    }

    GameObject CreateDoubleShooter(float size)
    {
        GameObject doubleShooter = new GameObject("DoubleShooter");

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(doubleShooter.transform, false);

        if (doubleShooterModel != null)
        {
            GameObject visual = Instantiate(doubleShooterModel, doubleShooter.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = doubleShooter.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
        }
        else
        {
            float stemHeight = Mathf.Max(1f, size * 1.3f);
            float stemRadius = Mathf.Max(0.2f, size * 0.25f);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(doubleShooter.transform, false);
            stem.transform.localScale = new Vector3(stemRadius, stemHeight * 0.5f, stemRadius);
            Renderer stemRenderer = stem.GetComponent<Renderer>();
            if (stemRenderer != null)
            {
                stemRenderer.material.color = new Color(0.2f, 0.65f, 0.2f);
            }
            Collider stemCollider = stem.GetComponent<Collider>();
            if (stemCollider != null)
            {
                Destroy(stemCollider);
            }

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(doubleShooter.transform, false);
            float headSize = Mathf.Max(0.6f, size * 0.7f);
            head.transform.localScale = Vector3.one * headSize;
            head.transform.localPosition = new Vector3(0f, stemHeight, 0f);
            Renderer headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                headRenderer.material.color = new Color(0.45f, 0.9f, 0.35f);
            }
            Collider headCollider = head.GetComponent<Collider>();
            if (headCollider != null)
            {
                Destroy(headCollider);
            }

            muzzle.transform.localPosition = head.transform.localPosition + new Vector3(0f, 0f, -headSize * 0.6f);
        }

        DoubleShooter shooter = doubleShooter.AddComponent<DoubleShooter>();
        shooter.AssignMuzzle(muzzle.transform);
        shooter.SetProjectileHeight(peaProjectileHeight);
        shooter.SetProjectileDamage(peaProjectileDamage);
        
        // 确保有碰撞体（用于玩家碰撞检测）
        BoxCollider collider = doubleShooter.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = doubleShooter.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
        collider.size = new Vector3(size, size * 2f, size);
        collider.center = new Vector3(0f, size, 0f);

        return doubleShooter;
    }

    GameObject CreateTripleShooter(float size)
    {
        GameObject tripleShooter = new GameObject("TripleShooter");

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(tripleShooter.transform, false);

        if (tripleShooterModel != null)
        {
            GameObject visual = Instantiate(tripleShooterModel, tripleShooter.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = tripleShooter.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
            
            muzzle.transform.localPosition = new Vector3(0f, size * 1.0f, -size * 0.5f);
        }
        else
        {
            float stemHeight = Mathf.Max(1f, size * 1.3f);
            float stemRadius = Mathf.Max(0.2f, size * 0.25f);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(tripleShooter.transform, false);
            stem.transform.localScale = new Vector3(stemRadius, stemHeight * 0.5f, stemRadius);
            Renderer stemRenderer = stem.GetComponent<Renderer>();
            if (stemRenderer != null)
            {
                stemRenderer.material.color = new Color(0.2f, 0.65f, 0.2f);
            }
            Collider stemCollider = stem.GetComponent<Collider>();
            if (stemCollider != null)
            {
                Destroy(stemCollider);
            }

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(tripleShooter.transform, false);
            float headSize = Mathf.Max(0.6f, size * 0.7f);
            head.transform.localScale = Vector3.one * headSize;
            head.transform.localPosition = new Vector3(0f, stemHeight, 0f);
            Renderer headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                headRenderer.material.color = new Color(0.45f, 0.9f, 0.35f);
            }
            Collider headCollider = head.GetComponent<Collider>();
            if (headCollider != null)
            {
                Destroy(headCollider);
            }

            muzzle.transform.localPosition = head.transform.localPosition + new Vector3(0f, 0f, -headSize * 0.6f);
        }

        TripleShooter shooter = tripleShooter.AddComponent<TripleShooter>();
        shooter.AssignMuzzle(muzzle.transform);
        shooter.SetProjectileHeight(peaProjectileHeight);
        shooter.SetProjectileDamage(peaProjectileDamage);
        
        // 确保有碰撞体（用于玩家碰撞检测）
        BoxCollider collider = tripleShooter.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = tripleShooter.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
        collider.size = new Vector3(size, size * 2f, size);
        collider.center = new Vector3(0f, size, 0f);

        return tripleShooter;
    }

    GameObject CreateFireStump(float size)
    {
        GameObject fireStump = new GameObject("FireStump");

        if (fireStumpModel != null)
        {
            GameObject visual = Instantiate(fireStumpModel, fireStump.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = fireStump.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
        }
        else
        {
            GameObject stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stump.name = "Stump";
            stump.transform.SetParent(fireStump.transform, false);
            stump.transform.localScale = new Vector3(size, size * 0.5f, size);
            Renderer stumpRenderer = stump.GetComponent<Renderer>();
            if (stumpRenderer != null)
            {
                stumpRenderer.material.color = new Color(0.6f, 0.3f, 0.1f);
            }
            Collider stumpCollider = stump.GetComponent<Collider>();
            if (stumpCollider != null)
            {
                Destroy(stumpCollider);
            }
        }

        FireStump fireStumpScript = fireStump.AddComponent<FireStump>();
        fireStumpScript.damage = fireStumpDamage;
        fireStumpScript.burningDuration = fireStumpBurningTime;
        fireStumpScript.burningDamagePerSecond = fireStumpBurningDamage;

        return fireStump;
    }

    GameObject CreateNut(float size)
    {
        GameObject nut = new GameObject("Nut");

        if (nutModel != null)
        {
            GameObject visual = Instantiate(nutModel, nut.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题：计算模型bounds的中心，调整localPosition使模型中心对齐到父对象原点
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = nut.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
        }
        else
        {
            GameObject nutBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nutBody.name = "NutBody";
            nutBody.transform.SetParent(nut.transform, false);
            nutBody.transform.localScale = new Vector3(size, size, size);
            nutBody.transform.localPosition = new Vector3(0f, size * 0.5f, 0f);
            Renderer nutRenderer = nutBody.GetComponent<Renderer>();
            if (nutRenderer != null)
            {
                nutRenderer.material.color = new Color(0.6f, 0.4f, 0.2f);
            }
            Collider childCollider = nutBody.GetComponent<Collider>();
            if (childCollider != null)
            {
                Destroy(childCollider);
            }
        }

        BoxCollider nutCollider = nut.AddComponent<BoxCollider>();
        nutCollider.isTrigger = true;
        nutCollider.size = new Vector3(size, size, size);
        nutCollider.center = new Vector3(0f, size * 0.5f, 0f);

        Nut nutScript = nut.AddComponent<Nut>();
        nutScript.damage = nutDamage;

        return nut;
    }

    GameObject CreateTallNut(float size)
    {
        GameObject tallNut = new GameObject("TallNut");

        if (tallNutModel != null)
        {
            GameObject visual = Instantiate(tallNutModel, tallNut.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题：计算模型bounds的中心，调整localPosition使模型中心对齐到父对象原点
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = tallNut.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
        }
        else
        {
            GameObject tallNutBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tallNutBody.name = "TallNutBody";
            tallNutBody.transform.SetParent(tallNut.transform, false);
            tallNutBody.transform.localScale = new Vector3(size, size * 1.5f, size);
            tallNutBody.transform.localPosition = new Vector3(0f, size * 0.75f, 0f);
            Renderer tallNutRenderer = tallNutBody.GetComponent<Renderer>();
            if (tallNutRenderer != null)
            {
                tallNutRenderer.material.color = new Color(0.5f, 0.35f, 0.15f);
            }
            Collider childCollider = tallNutBody.GetComponent<Collider>();
            if (childCollider != null)
            {
                Destroy(childCollider);
            }
        }

        BoxCollider tallNutCollider = tallNut.AddComponent<BoxCollider>();
        tallNutCollider.isTrigger = true;
        tallNutCollider.size = new Vector3(size, size * 1.5f, size);
        tallNutCollider.center = new Vector3(0f, size * 0.75f, 0f);

        TallNut tallNutScript = tallNut.AddComponent<TallNut>();
        tallNutScript.damage = tallNutDamage;

        return tallNut;
    }

    GameObject CreateCattailShooter(float size)
    {
        GameObject cattailShooter = new GameObject("CattailShooter");

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(cattailShooter.transform, false);

        if (cattailShooterModel != null)
        {
            GameObject visual = Instantiate(cattailShooterModel, cattailShooter.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            // 我们不在这里设置scale，而是由UpdateScale方法统一处理
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = cattailShooter.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
            
            // 我们不在这里设置位置，而是由UpdateScale方法处理
        }
        else
        {
            float stemHeight = Mathf.Max(1f, size * 1.3f);
            float stemRadius = Mathf.Max(0.2f, size * 0.25f);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(cattailShooter.transform, false);
            stem.transform.localScale = new Vector3(stemRadius, stemHeight * 0.5f, stemRadius);
            Renderer stemRenderer = stem.GetComponent<Renderer>();
            if (stemRenderer != null)
            {
                stemRenderer.material.color = new Color(0.4f, 0.7f, 0.3f);
            }
            Collider stemCollider = stem.GetComponent<Collider>();
            if (stemCollider != null)
            {
                Destroy(stemCollider);
            }

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(cattailShooter.transform, false);
            float headSize = Mathf.Max(0.6f, size * 0.7f);
            head.transform.localScale = Vector3.one * headSize;
            head.transform.localPosition = new Vector3(0f, stemHeight, 0f);
            Renderer headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                headRenderer.material.color = new Color(0.8f, 0.6f, 0.4f);
            }
            Collider headCollider = head.GetComponent<Collider>();
            if (headCollider != null)
            {
                Destroy(headCollider);
            }

            muzzle.transform.localPosition = head.transform.localPosition + new Vector3(0f, 0f, -headSize * 0.6f);
        }

        Rigidbody cattailRb = cattailShooter.AddComponent<Rigidbody>();
        cattailRb.isKinematic = true;
        cattailRb.useGravity = false;

        // 添加碰撞体，但不设置具体尺寸，由UpdateScale处理
        BoxCollider cattailCollider = cattailShooter.AddComponent<BoxCollider>();
        cattailCollider.isTrigger = true;

        // 初始化组件并设置属性
        CattailShooter shooter = cattailShooter.AddComponent<CattailShooter>();
        shooter.model = cattailShooterModel;
        shooter.damage = cattailShooterDamage;
        shooter.projectileDamage = cattailProjectileDamage;
        shooter.spikeProjectileModel = spikeProjectileModel;
        shooter.AssignMuzzle(muzzle.transform);
        shooter.SetProjectileHeight(peaProjectileHeight);
        
        // 设置scale属性，这会触发UpdateScale方法
        shooter.scale = size;

        return cattailShooter;
    }

    GameObject CreateCactus(float size)
    {
        GameObject cactus = new GameObject("Cactus");

        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(cactus.transform, false);

        if (cactusModel != null)
        {
            GameObject visual = Instantiate(cactusModel, cactus.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            // 不设置scale，由UpdateScale方法统一处理
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = cactus.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
            
            // 不设置位置，由UpdateScale方法处理
        }
        else
        {
            float cactusHeight = Mathf.Max(1.5f, size * 2.0f);
            float cactusRadius = Mathf.Max(0.3f, size * 0.3f);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Body";
            body.transform.SetParent(cactus.transform, false);
            body.transform.localScale = new Vector3(cactusRadius, cactusHeight * 0.5f, cactusRadius);
            body.transform.localPosition = new Vector3(0f, cactusHeight * 0.5f, 0f);
            Renderer bodyRenderer = body.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = new Color(0.2f, 0.6f, 0.2f);
            }
            Collider bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                Destroy(bodyCollider);
            }

            muzzle.transform.localPosition = new Vector3(0f, cactusHeight * 0.8f, -cactusRadius * 1.2f);
        }

        Rigidbody cactusRb = cactus.AddComponent<Rigidbody>();
        cactusRb.isKinematic = true;
        cactusRb.useGravity = false;

        // 添加碰撞体，但不设置具体尺寸，由UpdateScale处理
        BoxCollider cactusCollider = cactus.AddComponent<BoxCollider>();
        cactusCollider.isTrigger = true;

        // 初始化组件并设置属性
        Cactus cactusScript = cactus.AddComponent<Cactus>();
        cactusScript.model = cactusModel;
        cactusScript.damage = cactusDamage;
        cactusScript.projectileDamage = cactusProjectileDamage;
        cactusScript.spikeHeight = cactusSpikeHeight;
        cactusScript.spikeProjectileModel = spikeProjectileModel;
        cactusScript.AssignMuzzle(muzzle.transform);
        cactusScript.SetSpikeHeight(cactusSpikeHeight);
        
        // 设置scale属性，这会触发UpdateScale方法
        cactusScript.scale = size;

        return cactus;
    }
    
    GameObject CreateIceMushroom(float size)
    {
        GameObject iceMushroom = new GameObject("IceMushroom");

        if (iceMushroomModel != null)
        {
            GameObject visual = Instantiate(iceMushroomModel, iceMushroom.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            // 不在这里设置scale，而是由UpdateScale方法统一处理
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = iceMushroom.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
        }
        else
        {
            // 创建默认的菇菇形状
            float stemHeight = Mathf.Max(0.5f, size * 0.5f);
            float stemRadius = Mathf.Max(0.1f, size * 0.15f);
            float capRadius = Mathf.Max(0.3f, size * 0.5f);
            
            // 创建菇染
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(iceMushroom.transform, false);
            stem.transform.localScale = new Vector3(stemRadius, stemHeight * 0.5f, stemRadius);
            stem.transform.localPosition = new Vector3(0f, stemHeight * 0.5f, 0f);
            Renderer stemRenderer = stem.GetComponent<Renderer>();
            if (stemRenderer != null)
            {
                stemRenderer.material.color = new Color(0.9f, 0.9f, 1.0f);
            }
            Collider stemCollider = stem.GetComponent<Collider>();
            if (stemCollider != null)
            {
                Destroy(stemCollider);
            }
            
            // 创建菇帽
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Cap";
            cap.transform.SetParent(iceMushroom.transform, false);
            cap.transform.localScale = new Vector3(capRadius * 1.3f, capRadius * 0.8f, capRadius * 1.3f);
            cap.transform.localPosition = new Vector3(0f, stemHeight + capRadius * 0.4f, 0f);
            Renderer capRenderer = cap.GetComponent<Renderer>();
            if (capRenderer != null)
            {
                capRenderer.material.color = new Color(0.6f, 0.8f, 1.0f);
            }
            Collider capCollider = cap.GetComponent<Collider>();
            if (capCollider != null)
            {
                Destroy(capCollider);
            }
            
            // 添加白点装饰
            for (int i = 0; i < 5; i++)
            {
                GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dot.name = $"Dot{i}";
                dot.transform.SetParent(iceMushroom.transform, false);
                
                float dotSize = size * 0.1f;
                dot.transform.localScale = Vector3.one * dotSize;
                
                float angle = i * 72f;
                float dotRadius = capRadius * 0.7f;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * dotRadius;
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * dotRadius;
                float y = stemHeight + capRadius * 0.7f;
                
                dot.transform.localPosition = new Vector3(x, y, z);
                
                Renderer dotRenderer = dot.GetComponent<Renderer>();
                if (dotRenderer != null)
                {
                    dotRenderer.material.color = new Color(1.0f, 1.0f, 1.0f);
                }
                
                Collider dotCollider = dot.GetComponent<Collider>();
                if (dotCollider != null)
                {
                    Destroy(dotCollider);
                }
            }
        }
        
        // 添加刀体和物理组件
        Rigidbody iceMushroomRb = iceMushroom.AddComponent<Rigidbody>();
        iceMushroomRb.isKinematic = true;
        iceMushroomRb.useGravity = false;
        
        // 添加碰撞体，但不设置具体尺寸，由UpdateScale处理
        SphereCollider mushroomCollider = iceMushroom.AddComponent<SphereCollider>();
        mushroomCollider.isTrigger = true;
        
        // 添加冰菇脚本并设置属性
        IceMushroom mushroomScript = iceMushroom.AddComponent<IceMushroom>();
        mushroomScript.model = iceMushroomModel;
        mushroomScript.damage = iceMushroomDamage;
        mushroomScript.freezeDuration = iceMushroomFreezeDuration;
        mushroomScript.mushroomEffectPrefab = iceMushroomSelfEffectPrefab;
        mushroomScript.mushroomEffectLifetime = iceMushroomSelfEffectLifetime;
        mushroomScript.mushroomEffectPrefab2 = iceMushroomSelfEffectPrefab2;
        mushroomScript.mushroomEffectLifetime2 = iceMushroomSelfEffectLifetime2;
        mushroomScript.playerEffectPrefab = iceMushroomPlayerEffectPrefab;
        mushroomScript.playerEffectLifetime = iceMushroomPlayerEffectLifetime;
        
        // 设置scale属性，这会触发UpdateScale方法
        mushroomScript.scale = size;
        
        return iceMushroom;
    }
    
    GameObject CreatePotatoMine(float size)
    {
        GameObject mine;
        if (potatoMinePrefab != null)
        {
            mine = Instantiate(potatoMinePrefab);
        }
        else
        {
            mine = new GameObject("PotatoMine");
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(mine.transform, false);
            visual.transform.localScale = Vector3.one * size;
            Collider visualCol = visual.GetComponent<Collider>();
            if (visualCol != null) Destroy(visualCol);
            SphereCollider col = mine.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = size * 0.5f;
        }

        PotatoMine script = mine.GetComponent<PotatoMine>();
        if (script == null) script = mine.AddComponent<PotatoMine>();
        script.damage = potatoMineDamage;
        script.activationDistance = potatoMineActivationDistance;
        script.deactivationDistance = potatoMineDeactivationDistance;
        script.buriedHeightRatio = potatoMineBuriedHeightRatio;
        script.ascendSpeed = potatoMineAscendSpeed;
        script.descendSpeed = potatoMineDescendSpeed;
        if (potatoMineTriggerEffectPrefab != null)
        {
            script.triggerEffectPrefab = potatoMineTriggerEffectPrefab;
        }

        mine.transform.localScale = Vector3.one * size;
        return mine;
    }

    GameObject CreateFirePepper(float size)
    {
        GameObject pepper;
        if (firePepperPrefab != null)
        {
            pepper = Instantiate(firePepperPrefab);
        }
        else
        {
            pepper = new GameObject("FirePepper");
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(pepper.transform, false);
            visual.transform.localScale = new Vector3(size * 0.5f, size, size * 0.5f);
            Collider visualCol = visual.GetComponent<Collider>();
            if (visualCol != null) Destroy(visualCol);
            CapsuleCollider col = pepper.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.height = size * 2f;
            col.radius = size * 0.3f;
        }

        FirePepper script = pepper.GetComponent<FirePepper>();
        if (script == null) script = pepper.AddComponent<FirePepper>();
        script.triggerRadius = firePepperTriggerRadius;
        script.growDuration = firePepperGrowDuration;
        script.maxScaleMultiplier = firePepperMaxScaleMultiplier;
        script.wobbleDuration = firePepperWobbleDuration;
        script.wobbleMagnitude = firePepperWobbleMagnitude;
        script.explosionDamage = firePepperExplosionDamage;
        script.burnDuration = firePepperBurnDuration;
        script.burnDamagePerSecond = firePepperBurnDamagePerSecond;
        script.burnHeight = firePepperBurnHeight;
        script.verticalLength = firePepperVerticalLength;
        script.verticalWidth = firePepperVerticalWidth;
        script.horizontalWidth = firePepperHorizontalWidth;
        script.horizontalDepth = firePepperHorizontalDepth;
        script.effectPrefab = firePepperEffectPrefab;
        script.effectLifetime = firePepperEffectLifetime;
        script.autoScaleEffect = firePepperAutoScaleEffect;
        script.laneMinX = GetMinX();
        script.laneMaxX = GetMaxX();

        pepper.transform.localScale = Vector3.one * size;
        return pepper;
    }

    GameObject CreateSunflowerPickup(float size)
    {
        GameObject pickup = new GameObject("SunflowerPickup");
        SunflowerPickup sunflower = pickup.AddComponent<SunflowerPickup>();
        sunflower.healAmount = Mathf.Max(1f, sunflowerHealAmount);
        sunflower.visualScale = size;
        sunflower.visualOffset = sunflowerVisualOffset;
        sunflower.rotateSpeed = sunflowerRotateSpeed;
        sunflower.modelRotationOffset = sunflowerModelRotation;
        sunflower.modelPrefab = sunflowerModel;
        sunflower.idleEffectPrefab = sunflowerIdleEffect;
        sunflower.collectEffectPrefab = sunflowerCollectEffect;
        sunflower.idleEffectOffset = sunflowerIdleEffectOffset;
        sunflower.idleEffectScale = sunflowerIdleEffectScale;
        sunflower.collectEffectOffset = sunflowerCollectEffectOffset;
        sunflower.RebuildVisuals();
        return pickup;
    }

    GameObject CreateShieldPickup(float size)
    {
        GameObject pickup = new GameObject("ShieldPickup");
        ShieldPickup shield = pickup.AddComponent<ShieldPickup>();
        ShieldType type = GetRandomShieldType();
        shield.shieldType = type;
        shield.shieldDuration = shieldDuration;
        shield.visualScale = size;
        shield.visualOffset = shieldVisualOffset;
        shield.rotateSpeed = shieldRotateSpeed;
        shield.rotationAxis = shieldRotationAxis;
        shield.modelRotationOffset = shieldModelRotation;
        shield.modelScale = shieldModelScale;
        shield.pickupEffectPrefab = shieldPickupEffectPrefab;
        shield.idleEffectOffset = shieldIdleEffectOffset;
        shield.pickupEffectOffset = shieldPickupEffectOffset;

        switch (type)
        {
            case ShieldType.Fire:
                shield.modelPrefab = fireShieldModel;
                shield.idleEffectPrefab = fireShieldIdleEffect;
                shield.fallbackColor = fireShieldColor;
                break;
            case ShieldType.Ice:
                shield.modelPrefab = iceShieldModel;
                shield.idleEffectPrefab = iceShieldIdleEffect;
                shield.fallbackColor = iceShieldColor;
                break;
            default:
                shield.modelPrefab = normalShieldModel;
                shield.idleEffectPrefab = normalShieldIdleEffect;
                shield.fallbackColor = normalShieldColor;
                break;
        }
        shield.RebuildVisuals();
        return pickup;
    }

    GameObject CreateJumpBoostPickup(float size)
    {
        GameObject pickup = new GameObject("JumpBoostPickup");
        JumpBoostPickup jumpPickup = pickup.AddComponent<JumpBoostPickup>();
        jumpPickup.visualScale = size;
        jumpPickup.duration = jumpBoostDuration;
        jumpPickup.jumpMultiplier = jumpBoostMultiplier;
        jumpPickup.visualOffset = jumpVisualOffset;
        jumpPickup.rotateSpeed = jumpRotateSpeed;
        jumpPickup.modelPrefab = jumpPickupModel;
        jumpPickup.idleEffectPrefab = jumpPickupIdleEffect;
        jumpPickup.pickupEffectPrefab = jumpPickupCollectEffect;
        jumpPickup.idleEffectOffset = jumpIdleEffectOffset;
        jumpPickup.pickupEffectOffset = jumpPickupEffectOffset;
        jumpPickup.playerPickupEffectPrefab = jumpPickupPlayerEffect;
        jumpPickup.playerEffectLifetime = jumpPlayerEffectLifetime;
        jumpPickup.playerEffectOffset = jumpPlayerEffectOffset;
        jumpPickup.RebuildVisuals();
        return pickup;
    }

    bool ShouldForceSupportPickup()
    {
        if (supportPickupChance <= 0f)
        {
            return false;
        }
        if ((nextSpawnZ - lastSupportSpawnZ) < supportPickupMinDistance)
        {
            return false;
        }
        return Random.value <= supportPickupChance;
    }

    ObstacleType GetRandomSupportPickupType()
    {
        float sunflowerWeight = Mathf.Max(0f, sunflowerSpawnWeight);
        float shieldWeight = Mathf.Max(0f, shieldSpawnWeight);
        float jumpWeight = Mathf.Max(0f, jumpSpawnWeight);
        float total = sunflowerWeight + shieldWeight + jumpWeight;
        if (total <= Mathf.Epsilon)
        {
            return ObstacleType.ShieldPickup;
        }

        float roll = Random.value * total;
        if (roll < sunflowerWeight)
        {
            return ObstacleType.Sunflower;
        }
        roll -= sunflowerWeight;
        if (roll < shieldWeight)
        {
            return ObstacleType.ShieldPickup;
        }
        return ObstacleType.JumpBoost;
    }
    
    GameObject CreateCherryBomb(float size)
    {
        GameObject cherryBomb = new GameObject("CherryBomb");

        if (cherryBombModel != null)
        {
            GameObject visual = Instantiate(cherryBombModel, cherryBomb.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            // 修复模型中心偏移问题
            Renderer visualRenderer = visual.GetComponentInChildren<Renderer>();
            if (visualRenderer != null)
            {
                Bounds localBounds = visualRenderer.bounds;
                Vector3 centerOffset = cherryBomb.transform.InverseTransformPoint(localBounds.center);
                visual.transform.localPosition = -centerOffset;
            }
        }
        else
        {
            // 创建简单的樱桃炸弹形状（如果没有指定模型）
            float cherryRadius = size * 0.4f;
            
            // 创建底部茎
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(cherryBomb.transform, false);
            stem.transform.localScale = new Vector3(size * 0.1f, size * 0.3f, size * 0.1f);
            stem.transform.localPosition = new Vector3(0f, size * 0.3f, 0f);
            Renderer stemRenderer = stem.GetComponent<Renderer>();
            if (stemRenderer != null)
            {
                stemRenderer.material.color = new Color(0.2f, 0.6f, 0.2f);
            }
            Collider stemCollider = stem.GetComponent<Collider>();
            if (stemCollider != null)
            {
                Destroy(stemCollider);
            }
            
            // 创建左侧樱桃
            GameObject leftCherry = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftCherry.name = "LeftCherry";
            leftCherry.transform.SetParent(cherryBomb.transform, false);
            leftCherry.transform.localScale = Vector3.one * cherryRadius;
            leftCherry.transform.localPosition = new Vector3(-cherryRadius * 0.7f, size * 0.6f, 0f);
            Renderer leftRenderer = leftCherry.GetComponent<Renderer>();
            if (leftRenderer != null)
            {
                leftRenderer.material.color = new Color(0.8f, 0.1f, 0.1f);
            }
            Collider leftCollider = leftCherry.GetComponent<Collider>();
            if (leftCollider != null)
            {
                Destroy(leftCollider);
            }
            
            // 创建右侧樱桃
            GameObject rightCherry = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightCherry.name = "RightCherry";
            rightCherry.transform.SetParent(cherryBomb.transform, false);
            rightCherry.transform.localScale = Vector3.one * cherryRadius;
            rightCherry.transform.localPosition = new Vector3(cherryRadius * 0.7f, size * 0.6f, 0f);
            Renderer rightRenderer = rightCherry.GetComponent<Renderer>();
            if (rightRenderer != null)
            {
                rightRenderer.material.color = new Color(0.8f, 0.1f, 0.1f);
            }
            Collider rightCollider = rightCherry.GetComponent<Collider>();
            if (rightCollider != null)
            {
                Destroy(rightCollider);
            }
            
            // 添加两个叶子装饰
            GameObject leaf1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf1.name = "Leaf1";
            leaf1.transform.SetParent(cherryBomb.transform, false);
            leaf1.transform.localScale = new Vector3(size * 0.15f, size * 0.05f, size * 0.15f);
            leaf1.transform.localPosition = new Vector3(size * 0.1f, size * 0.4f, 0f);
            leaf1.transform.localRotation = Quaternion.Euler(0f, 0f, 30f);
            Renderer leaf1Renderer = leaf1.GetComponent<Renderer>();
            if (leaf1Renderer != null)
            {
                leaf1Renderer.material.color = new Color(0.1f, 0.7f, 0.1f);
            }
            Collider leaf1Collider = leaf1.GetComponent<Collider>();
            if (leaf1Collider != null)
            {
                Destroy(leaf1Collider);
            }
            
            GameObject leaf2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf2.name = "Leaf2";
            leaf2.transform.SetParent(cherryBomb.transform, false);
            leaf2.transform.localScale = new Vector3(size * 0.15f, size * 0.05f, size * 0.15f);
            leaf2.transform.localPosition = new Vector3(-size * 0.1f, size * 0.4f, 0f);
            leaf2.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
            Renderer leaf2Renderer = leaf2.GetComponent<Renderer>();
            if (leaf2Renderer != null)
            {
                leaf2Renderer.material.color = new Color(0.1f, 0.7f, 0.1f);
            }
            Collider leaf2Collider = leaf2.GetComponent<Collider>();
            if (leaf2Collider != null)
            {
                Destroy(leaf2Collider);
            }
        }
        
        // 添加碰撞体，但不设置具体尺寸，由UpdateScale处理
        SphereCollider cherryCollider = cherryBomb.AddComponent<SphereCollider>();
        cherryCollider.isTrigger = true;
        
        // 添加刚体
        Rigidbody rb = cherryBomb.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // 添加CherryBomb脚本组件并设置属性
        CherryBomb cherryBombScript = cherryBomb.AddComponent<CherryBomb>();
        cherryBombScript.model = cherryBombModel;
        cherryBombScript.explosionDamage = cherryBombDamage;
        cherryBombScript.triggerRadius = cherryBombTriggerRadius;
        cherryBombScript.explosionRadius = cherryBombExplosionRadius;
        cherryBombScript.explosionDelay = cherryBombExplosionDelay;
        cherryBombScript.explosionEffectPrefab = explosionEffectPrefab;
        
        // 设置scale属性，这会触发UpdateScale方法
        cherryBombScript.scale = size;
        
        return cherryBomb;
    }
    
 
    ShieldType GetRandomShieldType()
    {
        ShieldType[] pool = new ShieldType[]
        {
            ShieldType.Normal,
            ShieldType.Fire,
            ShieldType.Ice
        };
        return pool[Random.Range(0, pool.Length)];
    }

    bool IsSupportPickup(ObstacleType type)
    {
        return type == ObstacleType.Sunflower ||
               type == ObstacleType.ShieldPickup ||
               type == ObstacleType.JumpBoost;
    }

    void RemoveChildColliders(GameObject root)
    {
        if (root == null) return;
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (collider != null)
            {
                Destroy(collider);
            }
        }
    }

    void RemoveOldObstacles()
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            float obstacleZ = activeObstacles[i].transform.position.z;
            float distanceFromPlayer = obstacleZ - playerTransform.position.z;

            if (distanceFromPlayer < -despawnDistance)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }

        RemoveOldGridData();
    }

    void PrewarmObstacles()
    {
        float requiredAhead = GetRequiredAheadDistance();
        while (nextSpawnZ - playerTransform.position.z < requiredAhead)
        {
            SpawnObstacleRow();
        }
    }

    float GetRequiredAheadDistance()
    {
        float dynamicAhead = 0f;
        if (playerController != null)
        {
            float liveSpeed = Mathf.Abs(playerController.GetForwardSpeed());
            float expectedSpeed = Mathf.Max(liveSpeed, playerController.maxForwardSpeed);
            dynamicAhead = expectedSpeed * lookAheadTime;
        }
        return Mathf.Max(spawnDistance, dynamicAhead);
    }

    ObstacleType GetRandomObstacleTypeWithConstraints()
    {
        const int maxOccurrencesInTen = 4;
        ObstacleType[] allTypes = (ObstacleType[])System.Enum.GetValues(typeof(ObstacleType));
        List<ObstacleType> availableTypes = new List<ObstacleType>();
        
        // 步骤1：按照连续性规则过滤可用障碍物类型
        foreach (ObstacleType type in allTypes)
        {
            int occurrenceCount = 0;
            for (int i = 0; i < recentObstacleHistory.Count; i++)
            {
                if (recentObstacleHistory[i] == type)
                {
                    occurrenceCount++;
                }
            }

            if (occurrenceCount >= maxOccurrencesInTen)
            {
                continue;
            }
            if (IsSupportPickup(type) && occurrenceCount >= 2)
            {
                continue;
            }
            availableTypes.Add(type);
        }

        if (availableTypes.Count == 0)
        {
            return allTypes[Random.Range(0, allTypes.Length)];
        }
        
        // 步骤2：基于难度等级将可用类型转化为权重列表
        List<ObstacleType> weightedTypes = new List<ObstacleType>();

        // 为每种障碍物类型计算达到需要的权重次数
        foreach (ObstacleType type in availableTypes)
        {
            // 获取该类型障碍物的难度等级
            int difficultyLevel = GetObstacleDifficultyLevel(type);
            
            // 如果难度等级有效（在1-3范围内），则使用难度权重系统
            if (difficultyLevel >= 1 && difficultyLevel <= 3 && difficultyLevelWeights.ContainsKey(difficultyLevel))
            {
                // 计算此类型障碍物应添加的次数（即权重）
                float weight = difficultyLevelWeights[difficultyLevel] * 10f; // 乘以10转换为整数范围
                if (IsSupportPickup(type))
                {
                    weight *= 0.3f;
                }
                int weightCount = Mathf.RoundToInt(weight);

                // 至少添加一次，确保所有类型都有被选中的机会
                for (int i = 0; i < Mathf.Max(1, weightCount); i++)
                {
                    weightedTypes.Add(type);
                }
            }
            else
            {
                // 对于未知难度等级的类型，添加一次
                weightedTypes.Add(type);
            }
        }

        // 步骤3：检查后方是否有豌豆射手类障碍物
        bool hasShooterBehind = false;
        float shooterDistance = float.MaxValue; 
        
        // 检查范围为3-7个障碍物距离内的障碍物
        float minDetectionDistance = minObstacleDistance * 3;
        float maxDetectionDistance = minObstacleDistance * 7;
        
        foreach (float rowZ in rowObstacleTypes.Keys)
        {
            float distance = rowZ - nextSpawnZ;
            if (distance > 0 && distance <= maxDetectionDistance)
            {
                Dictionary<int, ObstacleType> rowTypes = rowObstacleTypes[rowZ];
                foreach (ObstacleType type in rowTypes.Values)
                {
                    if (type == ObstacleType.Peashooter || 
                        type == ObstacleType.DoubleShooter || 
                        type == ObstacleType.TripleShooter)
                    {
                        hasShooterBehind = true;
                        shooterDistance = Mathf.Min(shooterDistance, distance);
                        break;
                    }
                }
                
                if (hasShooterBehind)
                    break;
            }
        }
        
        // 步骤4：如果后方有豌豆射手类障碍物，提高火焰树桩的出现概率
        if (hasShooterBehind && availableTypes.Contains(ObstacleType.FireStump))
        {
            // 根据射手距离计算权重：射手距离越近，火焰树桩权重越大
            float distanceFactor = 1.0f - (shooterDistance / maxDetectionDistance);
            int weightBoost = Mathf.CeilToInt(5 * distanceFactor);
            
            // 向列表中多次添加火焰树桩，增加其被选中的概率
            for (int i = 0; i < weightBoost; i++)
            {
                weightedTypes.Add(ObstacleType.FireStump);
            }
        }
        
        // 使用加权列表随机选择
        if (weightedTypes.Count > 0)
        {
            return weightedTypes[Random.Range(0, weightedTypes.Count)];
        }
        else
        {
            // 备用方案：使用原始可用列表
            return availableTypes[Random.Range(0, availableTypes.Count)];
        }
    }

    // 获取障碍物类型的难度等级
    private int GetObstacleDifficultyLevel(ObstacleType type)
    {
        switch (type)
        {
            // 难度等级1（简单）
            case ObstacleType.Peashooter:
            case ObstacleType.IceShooter:
            case ObstacleType.Nut:
            case ObstacleType.PotatoMine:
            case ObstacleType.Sunflower:
            case ObstacleType.ShieldPickup:
            case ObstacleType.JumpBoost:
                return 1;
                
            // 难度等级2（中等）
            case ObstacleType.DoubleShooter:
            case ObstacleType.TallNut:
            case ObstacleType.FireStump:
            case ObstacleType.IceMushroom:
            case ObstacleType.Cactus:
            case ObstacleType.FirePepper:
                return 2;
                
            // 难度等级3（困难）
            case ObstacleType.TripleShooter:
            case ObstacleType.CattailShooter:
            case ObstacleType.CherryBomb:
                return 3;
                
            default:
                return 1; // 默认为简单难度
        }
    }


    float GetEstimatedHalfWidth(ObstacleType type)
    {
        if (type == ObstacleType.Peashooter)
        {
            return Mathf.Max(0.25f, peashooterScale * 0.5f);
        }
        else if (type == ObstacleType.IceShooter)
        {
            return Mathf.Max(0.25f, iceShooterScale * 0.5f);
        }
        else if (type == ObstacleType.DoubleShooter)
        {
            return Mathf.Max(0.25f, doubleShooterScale * 0.5f);
        }
        else if (type == ObstacleType.TripleShooter)
        {
            return Mathf.Max(0.25f, tripleShooterScale * 0.5f);
        }
        else if (type == ObstacleType.FireStump)
        {
            return Mathf.Max(0.25f, fireStumpScale * 0.5f);
        }
        else if (type == ObstacleType.Nut)
        {
            return Mathf.Max(0.25f, nutScale * 0.5f);
        }
        else if (type == ObstacleType.TallNut)
        {
            return Mathf.Max(0.25f, tallNutScale * 0.5f);
        }
        else if (type == ObstacleType.CattailShooter)
        {
            return Mathf.Max(0.25f, cattailShooterScale * 0.5f);
        }
        else if (type == ObstacleType.Cactus)
        {
            return Mathf.Max(0.25f, cactusScale * 0.5f);
        }
        else if (type == ObstacleType.IceMushroom)
        {
            return Mathf.Max(0.25f, iceMushroomScale * 0.5f);
        }
        else if (type == ObstacleType.CherryBomb)
        {
            return Mathf.Max(0.25f, cherryBombScale * 0.5f);
        }
        else if (type == ObstacleType.FirePepper)
        {
            return Mathf.Max(0.25f, firePepperHorizontalWidth * 0.5f);
        }
        else if (type == ObstacleType.PotatoMine)
        {
            return Mathf.Max(0.25f, potatoMineScale * 0.5f);
        }
        else if (type == ObstacleType.Sunflower)
        {
            return Mathf.Max(0.25f, sunflowerScale * 0.5f);
        }
        else if (type == ObstacleType.ShieldPickup)
        {
            return Mathf.Max(0.25f, shieldPickupScale * 0.5f);
        }
        else if (type == ObstacleType.JumpBoost)
        {
            return Mathf.Max(0.25f, jumpPickupScale * 0.5f);
        }
        return Mathf.Max(0.25f, estimatedHalfWidth);
    }

    float GetMinX()
    {
        return -(roadWidth / 2f) + roadMargin;
    }

    float GetMaxX()
    {
        return (roadWidth / 2f) - roadMargin;
    }

    float GetRandomX()
    {
        return Random.Range(GetMinX(), GetMaxX());
    }


    void AlignToGround(GameObject obstacle)
    {
        Renderer renderer = obstacle.GetComponentInChildren<Renderer>();
        if (renderer == null) return;
        Bounds bounds = renderer.bounds;
        float bottom = bounds.min.y;
        obstacle.transform.position += new Vector3(0f, -bottom, 0f);
    }


    Collider EnsureCollider(GameObject obstacle)
    {
        Collider collider = obstacle.GetComponentInChildren<Collider>();
        if (collider != null || !autoAddCollider)
        {
            return collider;
        }

        MeshFilter filter = obstacle.GetComponentInChildren<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
        {
            MeshCollider meshCollider = obstacle.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
            meshCollider.convex = true;
            return meshCollider;
        }

        BoxCollider boxCollider = obstacle.AddComponent<BoxCollider>();
        Renderer renderer = obstacle.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            ApplyBoundsToCollider(renderer.bounds, obstacle.transform, boxCollider);
        }
        return boxCollider;
    }

    void ApplyBoundsToCollider(Bounds bounds, Transform obstacleTransform, BoxCollider boxCollider)
    {
        Vector3 centerWorld = bounds.center;
        Vector3 sizeWorld = bounds.size;
        boxCollider.center = obstacleTransform.InverseTransformPoint(centerWorld);
        Vector3 localSize = obstacleTransform.InverseTransformVector(new Vector3(sizeWorld.x, 0f, 0f));
        localSize.x = Mathf.Abs(localSize.x);
        localSize.y = Mathf.Abs(obstacleTransform.InverseTransformVector(new Vector3(0f, sizeWorld.y, 0f)).y);
        localSize.z = Mathf.Abs(obstacleTransform.InverseTransformVector(new Vector3(0f, 0f, sizeWorld.z)).z);
        boxCollider.size = new Vector3(localSize.x, localSize.y, localSize.z);
    }

    Color GetRandomFallbackColor()
    {
        Color[] colors = new Color[]
        {
            Color.red,
            new Color(1f, 0.5f, 0f),
            Color.yellow,
            Color.magenta,
            new Color(0.5f, 0f, 0.5f)
        };
        return colors[Random.Range(0, colors.Length)];
    }

    public void ClearAllObstacles()
    {
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }
        activeObstacles.Clear();
    }

    float GetDamageByType(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Peashooter:
                return Mathf.Max(0f, peashooterDamage);
            case ObstacleType.IceShooter:
                return Mathf.Max(0f, iceShooterDamage);
            case ObstacleType.DoubleShooter:
                return Mathf.Max(0f, doubleShooterDamage);
            case ObstacleType.TripleShooter:
                return Mathf.Max(0f, tripleShooterDamage);
            case ObstacleType.FireStump:
                return Mathf.Max(0f, fireStumpDamage);
            case ObstacleType.Nut:
                return Mathf.Max(0f, nutDamage);
            case ObstacleType.TallNut:
                return Mathf.Max(0f, tallNutDamage);
            case ObstacleType.IceMushroom:
                return Mathf.Max(0f, iceMushroomDamage);
            case ObstacleType.CherryBomb:
                return Mathf.Max(0f, cherryBombDamage);
            case ObstacleType.FirePepper:
                return Mathf.Max(0f, firePepperExplosionDamage);
            case ObstacleType.PotatoMine:
                return Mathf.Max(0f, potatoMineDamage);
            case ObstacleType.Sunflower:
            case ObstacleType.ShieldPickup:
            case ObstacleType.JumpBoost:
                return 0f;
            default:
                return 30f; // 默认伤害值
        }
    }

    void RemoveOldGridData()
    {
        float cullZ = playerTransform.position.z - despawnDistance - 50f;
        List<float> keysToRemove = new List<float>();

        foreach (var key in occupiedGrids.Keys)
        {
            if (key < cullZ)
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            occupiedGrids.Remove(key);
            if (rowObstacleTypes.ContainsKey(key))
            {
                rowObstacleTypes.Remove(key);
            }
        }
    }

    public void UpdateRoadWidth(float newWidth)
    {
        roadWidth = newWidth;
        CalculateGridCount();
        Debug.Log($"[ObstacleSpawner] 道路宽度更新为: {roadWidth}, 网格数量: {gridCountX}");
    }
    
    // 调试函数，用于测试火炬树桩和豌豆射手的关系规则
    public void DebugFireStumpShooterRelation()
    {
        // 模拟有豌豆射手在后方的情况
        Dictionary<int, ObstacleType> fakeShooterRow = new Dictionary<int, ObstacleType>();
        fakeShooterRow.Add(2, ObstacleType.Peashooter);
        rowObstacleTypes[nextSpawnZ + minObstacleDistance * 4] = fakeShooterRow;
        
        // 运行100次选择测试
        Dictionary<ObstacleType, int> typeCount = new Dictionary<ObstacleType, int>();
        for (int i = 0; i < 100; i++)
        {
            ObstacleType selectedType = GetRandomObstacleTypeWithConstraints();
            if (!typeCount.ContainsKey(selectedType))
            {
                typeCount[selectedType] = 0;
            }
            typeCount[selectedType]++;
        }
        
        // 输出统计结果
        Debug.Log("火炬树桩和豌豆射手关系规则测试结果：");
        foreach (var pair in typeCount)
        {
            Debug.Log($"{pair.Key}: {pair.Value}次");
        }
        
        // 清理测试数据
        rowObstacleTypes.Remove(nextSpawnZ + minObstacleDistance * 4);
    }

    void OnDrawGizmos()
    {
        if (!showGrid) return;

        float currentRoadWidth = roadWidth;
        if (roadGenerator != null)
        {
            currentRoadWidth = roadGenerator.GetRoadWidth();
        }

        float usableWidth = currentRoadWidth - roadMargin * 2f;
        int currentGridCountX = Mathf.FloorToInt(usableWidth / gridSize);

        if (currentGridCountX <= 0) return;

        float gridStartX = -(usableWidth / 2f);
        float yOffset = 0.1f;

        Transform playerTrans = playerTransform;
        if (playerTrans == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTrans = player.transform;
            }
        }

        float centerZ = playerTrans != null ? playerTrans.position.z : 0f;
        float startZ = Mathf.Floor((centerZ - gridDisplayRange) / gridSize) * gridSize;
        float endZ = Mathf.Ceil((centerZ + gridDisplayRange) / gridSize) * gridSize;

        Gizmos.color = Color.red;

        for (float z = startZ; z <= endZ; z += gridSize)
        {
            float leftX = gridStartX;
            float rightX = gridStartX + currentGridCountX * gridSize;
            Vector3 start = new Vector3(leftX, yOffset, z);
            Vector3 end = new Vector3(rightX, yOffset, z);
            DrawThickLine(start, end, 0.05f);
        }

        for (int x = 0; x <= currentGridCountX; x++)
        {
            float worldX = gridStartX + x * gridSize;
            Vector3 start = new Vector3(worldX, yOffset, startZ);
            Vector3 end = new Vector3(worldX, yOffset, endZ);
            DrawThickLine(start, end, 0.05f);
        }

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        for (float z = startZ; z <= endZ; z += gridSize)
        {
            Vector3 leftCenter = new Vector3(gridStartX + gridSize * 0.5f, yOffset, z + gridSize * 0.5f);
            Vector3 rightCenter = new Vector3(gridStartX + (currentGridCountX - 1) * gridSize + gridSize * 0.5f, yOffset, z + gridSize * 0.5f);
            Gizmos.DrawCube(leftCenter, new Vector3(gridSize * 0.9f, 0.01f, gridSize * 0.9f));
            Gizmos.DrawCube(rightCenter, new Vector3(gridSize * 0.9f, 0.01f, gridSize * 0.9f));
        }

    }

    void DrawThickLine(Vector3 start, Vector3 end, float thickness)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up) * thickness;
        
        Vector3 p1 = start + perpendicular;
        Vector3 p2 = start - perpendicular;
        Vector3 p3 = end - perpendicular;
        Vector3 p4 = end + perpendicular;

        Gizmos.DrawLine(p1, p4);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(start, end);
    }
}
