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

    [Header("圆柱体设置")]
    public float cylinderSize = 1.5f;
    public float cylinderDamage = 30f;

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

    [Header("仙人掌")]
    public float cactusDamage = 47f;
    public float cactusScale = 1.5f;
    public float cactusProjectileDamage = 27f;
    public float cactusSpikeHeight = 2.5f;
    public GameObject cactusModel;

    [Header("尖刺子弹")]
    public float spikeProjectileDamage = 27f;

    [Header("道路设置")]
    public float roadWidth = 10f;
    public float roadMargin = 1.5f;

    [Header("车道生成")]
    public bool useLaneSpawning = true;
    [Range(1, 5)]
    public int laneCount = 3;
    [Tooltip("保证至少多少条车道保持畅通")]
    [Range(0, 2)]
    public int minFreeLanes = 1;
    [Range(0f, 1f)]
    public float laneSpawnProbability = 0.6f;

    [Header("随机生成")]
    public bool allowFreeformSpawn = true;
    [Range(1, 4)]
    public int maxFreeformPerRow = 2;
    public float minFreeformSpacing = 1.5f;

    [Header("模型障碍（优先使用）")]
    public List<GameObject> obstaclePrefabs = new List<GameObject>();
    public Vector3 prefabScale = Vector3.one;
    public bool autoAddCollider = true;
    public bool forceTriggerCollider = true;
    [Tooltip("用于随机排布时的预估半宽，帮助避免重叠")]
    public float estimatedHalfWidth = 0.6f;

    [Header("动态生成距离")]
    public float lookAheadTime = 3f;

    private Transform playerTransform;
    private PlayerController playerController;
    private readonly List<GameObject> activeObstacles = new List<GameObject>();
    private readonly List<float> laneCenters = new List<float>();
    private readonly List<int> laneOrder = new List<int>();
    private readonly List<RowObstacleInfo> rowPlacementBuffer = new List<RowObstacleInfo>(8);
    private float nextSpawnZ = 30f;
    private bool laneCacheDirty = true;

    private enum ObstacleType
    {
        Cylinder,
        Peashooter,
        IceShooter,
        DoubleShooter,
        TripleShooter,
        FireStump,
        Nut,
        TallNut,
        CattailShooter,
        Cactus
    }

    private struct RowObstacleInfo
    {
        public float centerX;
        public float halfWidth;
    }

    void OnValidate()
    {
        if (laneCount < 1) laneCount = 1;
        minFreeLanes = Mathf.Clamp(minFreeLanes, 0, laneCount - 1);
        if (maxObstacleDistance < minObstacleDistance)
        {
            maxObstacleDistance = minObstacleDistance;
        }
        laneCacheDirty = true;
    }

    void Start()
    {
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

        RebuildLaneCache();
        PrewarmObstacles();
    }

    void Update()
    {
        if (playerController != null && playerController.IsGameOver())
        {
            return;
        }

        float requiredAhead = GetRequiredAheadDistance();
        while (nextSpawnZ - playerTransform.position.z < requiredAhead)
        {
            SpawnObstacleRow();
        }

        RemoveOldObstacles();
    }

    void SpawnObstacleRow()
    {
        rowPlacementBuffer.Clear();

        bool spawnLaneRow = useLaneSpawning && (!allowFreeformSpawn || Random.value < laneSpawnProbability);
        if (spawnLaneRow)
        {
            SpawnLaneRow(rowPlacementBuffer);
        }
        else if (allowFreeformSpawn)
        {
            SpawnFreeformRow(rowPlacementBuffer);
        }
        else
        {
            SpawnLaneRow(rowPlacementBuffer);
        }

        float distance = Random.Range(minObstacleDistance, maxObstacleDistance);
        nextSpawnZ += distance;
    }

    void SpawnLaneRow(List<RowObstacleInfo> rowInfo)
    {
        if (laneCacheDirty)
        {
            RebuildLaneCache();
        }

        if (laneCenters.Count == 0)
        {
            SpawnSingleObstacle(GetRandomObstacleType(), GetRandomX(), rowInfo);
            return;
        }

        int maxBlockable = Mathf.Max(1, laneCenters.Count - Mathf.Clamp(minFreeLanes, 0, laneCenters.Count - 1));
        int lanesToBlock = Random.Range(1, maxBlockable + 1);
        ShuffleLaneOrder();

        for (int i = 0; i < lanesToBlock; i++)
        {
            SpawnSingleObstacle(GetRandomObstacleType(), laneCenters[laneOrder[i]], rowInfo);
        }
    }

    void SpawnFreeformRow(List<RowObstacleInfo> rowInfo)
    {
        int obstacleCount = Random.Range(1, maxFreeformPerRow + 1);

        for (int i = 0; i < obstacleCount; i++)
        {
            ObstacleType type = GetRandomObstacleType();
            float candidateHalfWidth = GetEstimatedHalfWidth(type);
            const int maxAttempts = 8;
            float candidateX = 0f;
            bool found = false;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                candidateX = GetRandomX();
                if (IsFarFromExisting(candidateX, rowInfo, minFreeformSpacing, candidateHalfWidth))
                {
                    found = true;
                    break;
                }
            }

            if (!found && rowInfo.Count > 0)
            {
                candidateX = GetRandomX();
            }

            SpawnSingleObstacle(type, candidateX, rowInfo);
        }
    }

    void SpawnSingleObstacle(ObstacleType type, float laneX, List<RowObstacleInfo> rowInfo)
    {
        float size = cylinderSize;
        if (type == ObstacleType.Peashooter)
        {
            size = Mathf.Max(0.1f, peashooterScale);
        }
        else if (type == ObstacleType.IceShooter)
        {
            size = Mathf.Max(0.1f, iceShooterScale);
        }
        else if (type == ObstacleType.DoubleShooter)
        {
            size = Mathf.Max(0.1f, doubleShooterScale);
        }
        else if (type == ObstacleType.TripleShooter)
        {
            size = Mathf.Max(0.1f, tripleShooterScale);
        }
        else if (type == ObstacleType.FireStump)
        {
            size = Mathf.Max(0.1f, fireStumpScale);
        }
        else if (type == ObstacleType.Nut)
        {
            size = Mathf.Max(0.1f, nutScale);
        }
        else if (type == ObstacleType.TallNut)
        {
            size = Mathf.Max(0.1f, tallNutScale);
        }

        GameObject obstacle = CreateObstacle(type, size);
        if (obstacle == null)
        {
            return;
        }

        obstacle.name = $"Obstacle_{activeObstacles.Count}";
        obstacle.transform.SetParent(transform);
        obstacle.transform.position = new Vector3(laneX, 0f, nextSpawnZ);

        CenterRendererOnX(obstacle, laneX);
        AlignToGround(obstacle);
        ClampWithinRoad(obstacle);

        Collider collider = EnsureCollider(obstacle);
        if (collider != null && forceTriggerCollider)
        {
            collider.isTrigger = true;
        }

        TagUtility.TryAssignTag(obstacle, "Obstacle");

        if (type != ObstacleType.FireStump)
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

        if (rowInfo != null)
        {
            Renderer latestRenderer = obstacle.GetComponentInChildren<Renderer>();
            rowInfo.Add(new RowObstacleInfo
            {
                centerX = latestRenderer != null ? latestRenderer.bounds.center.x : obstacle.transform.position.x,
                halfWidth = GetHalfWidth(latestRenderer)
            });
        }

        activeObstacles.Add(obstacle);
    }

    GameObject CreateObstacle(ObstacleType type, float size)
    {
        bool shouldUsePrefab = obstaclePrefabs != null && obstaclePrefabs.Count > 0 && type != ObstacleType.Peashooter && type != ObstacleType.IceShooter && type != ObstacleType.DoubleShooter && type != ObstacleType.TripleShooter && type != ObstacleType.FireStump && type != ObstacleType.Nut && type != ObstacleType.TallNut && type != ObstacleType.CattailShooter && type != ObstacleType.Cactus;
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
            case ObstacleType.Cylinder:
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                obstacle.transform.localScale = new Vector3(size, size, size);
                break;
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
        }
        else
        {
            GameObject nutBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nutBody.name = "NutBody";
            nutBody.transform.SetParent(nut.transform, false);
            nutBody.transform.localScale = new Vector3(size, size, size);
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

        Rigidbody nutRb = nut.AddComponent<Rigidbody>();
        nutRb.isKinematic = true;
        nutRb.useGravity = false;

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
        }
        else
        {
            GameObject tallNutBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tallNutBody.name = "TallNutBody";
            tallNutBody.transform.SetParent(tallNut.transform, false);
            tallNutBody.transform.localScale = new Vector3(size, size * 1.5f, size);
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

        Rigidbody tallNutRb = tallNut.AddComponent<Rigidbody>();
        tallNutRb.isKinematic = true;
        tallNutRb.useGravity = false;

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
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            muzzle.transform.localPosition = new Vector3(0f, size * 1.0f, -size * 0.5f);
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

        BoxCollider cattailCollider = cattailShooter.AddComponent<BoxCollider>();
        cattailCollider.isTrigger = true;
        cattailCollider.size = new Vector3(size, size * 1.5f, size);
        cattailCollider.center = new Vector3(0f, size * 0.75f, 0f);

        CattailShooter shooter = cattailShooter.AddComponent<CattailShooter>();
        shooter.model = cattailShooterModel;
        shooter.scale = cattailShooterScale;
        shooter.damage = cattailShooterDamage;
        shooter.projectileDamage = cattailProjectileDamage;
        shooter.AssignMuzzle(muzzle.transform);
        shooter.SetProjectileHeight(peaProjectileHeight);

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
            visual.transform.localScale = Vector3.one * size;
            RemoveChildColliders(visual);
            
            muzzle.transform.localPosition = new Vector3(0f, size * 1.5f, -size * 0.3f);
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

        BoxCollider cactusCollider = cactus.AddComponent<BoxCollider>();
        cactusCollider.isTrigger = true;
        cactusCollider.size = new Vector3(size, size * 2.0f, size);
        cactusCollider.center = new Vector3(0f, size * 1.0f, 0f);

        Cactus cactusScript = cactus.AddComponent<Cactus>();
        cactusScript.model = cactusModel;
        cactusScript.scale = cactusScale;
        cactusScript.damage = cactusDamage;
        cactusScript.projectileDamage = cactusProjectileDamage;
        cactusScript.spikeHeight = cactusSpikeHeight;
        cactusScript.AssignMuzzle(muzzle.transform);
        cactusScript.SetSpikeHeight(cactusSpikeHeight);

        return cactus;
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

    bool IsFarFromExisting(float candidateX, List<RowObstacleInfo> existing, float padding, float candidateHalfWidth)
    {
        for (int i = 0; i < existing.Count; i++)
        {
            float minDistance = existing[i].halfWidth + candidateHalfWidth + padding;
            if (Mathf.Abs(existing[i].centerX - candidateX) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    ObstacleType GetRandomObstacleType()
    {
        return (ObstacleType)Random.Range(0, System.Enum.GetValues(typeof(ObstacleType)).Length);
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

    void CenterRendererOnX(GameObject obstacle, float targetCenterX)
    {
        Renderer renderer = obstacle.GetComponentInChildren<Renderer>();
        if (renderer == null) return;
        float currentCenterX = renderer.bounds.center.x;
        float offset = targetCenterX - currentCenterX;
        obstacle.transform.position += new Vector3(offset, 0f, 0f);
    }

    void AlignToGround(GameObject obstacle)
    {
        Renderer renderer = obstacle.GetComponentInChildren<Renderer>();
        if (renderer == null) return;
        Bounds bounds = renderer.bounds;
        float bottom = bounds.min.y;
        obstacle.transform.position += new Vector3(0f, -bottom, 0f);
    }

    void ClampWithinRoad(GameObject obstacle)
    {
        Renderer renderer = obstacle.GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        Bounds bounds = renderer.bounds;
        float halfWidth = bounds.extents.x;
        float minCenterX = GetMinX() + halfWidth;
        float maxCenterX = GetMaxX() - halfWidth;
        float currentCenterX = bounds.center.x;
        float clampedCenter = Mathf.Clamp(currentCenterX, minCenterX, maxCenterX);
        obstacle.transform.position += new Vector3(clampedCenter - currentCenterX, 0f, 0f);
    }

    float GetHalfWidth(Renderer renderer)
    {
        if (renderer == null) return 0.5f;
        return Mathf.Max(0.25f, renderer.bounds.extents.x);
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
            case ObstacleType.Cylinder:
            default:
                return Mathf.Max(0f, cylinderDamage);
        }
    }

    void ShuffleLaneOrder()
    {
        for (int i = laneOrder.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            int temp = laneOrder[i];
            laneOrder[i] = laneOrder[swapIndex];
            laneOrder[swapIndex] = temp;
        }
    }

    void RebuildLaneCache()
    {
        laneCenters.Clear();
        laneOrder.Clear();

        if (laneCount <= 0)
        {
            laneCacheDirty = false;
            return;
        }

        float minX = GetMinX();
        float maxX = GetMaxX();
        float usableWidth = Mathf.Max(0.1f, maxX - minX);

        if (laneCount == 1)
        {
            laneCenters.Add((minX + maxX) * 0.5f);
        }
        else
        {
            float spacing = usableWidth / (laneCount - 1);
            for (int i = 0; i < laneCount; i++)
            {
                laneCenters.Add(minX + spacing * i);
            }
        }

        for (int i = 0; i < laneCount; i++)
        {
            laneOrder.Add(i);
        }

        laneCacheDirty = false;
    }
}
