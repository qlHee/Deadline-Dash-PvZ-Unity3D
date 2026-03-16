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

    [Header("障碍尺寸（仅 Primitive 备用）")]
    public float minObstacleSize = 1f;
    public float maxObstacleSize = 2.5f;

    [Header("碰撞伤害")]
    public float cubeDamage = 25f;
    public float cylinderDamage = 30f;
    public float sphereDamage = 20f;

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
        Cube,
        Cylinder,
        Sphere
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
            SpawnSingleObstacle(GetRandomX(), rowInfo);
            return;
        }

        int maxBlockable = Mathf.Max(1, laneCenters.Count - Mathf.Clamp(minFreeLanes, 0, laneCenters.Count - 1));
        int lanesToBlock = Random.Range(1, maxBlockable + 1);
        ShuffleLaneOrder();

        for (int i = 0; i < lanesToBlock; i++)
        {
            SpawnSingleObstacle(laneCenters[laneOrder[i]], rowInfo);
        }
    }

    void SpawnFreeformRow(List<RowObstacleInfo> rowInfo)
    {
        int obstacleCount = Random.Range(1, maxFreeformPerRow + 1);

        for (int i = 0; i < obstacleCount; i++)
        {
            const int maxAttempts = 8;
            float candidateX = 0f;
            bool found = false;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                candidateX = GetRandomX();
                if (IsFarFromExisting(candidateX, rowInfo, minFreeformSpacing, estimatedHalfWidth))
                {
                    found = true;
                    break;
                }
            }

            if (!found && rowInfo.Count > 0)
            {
                candidateX = GetRandomX();
            }

            SpawnSingleObstacle(candidateX, rowInfo);
        }
    }

    void SpawnSingleObstacle(float laneX, List<RowObstacleInfo> rowInfo)
    {
        ObstacleType type = (ObstacleType)Random.Range(0, 3);
        float size = Random.Range(minObstacleSize, maxObstacleSize);

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

        ObstacleCollision obstacleCollision = obstacle.GetComponent<ObstacleCollision>();
        if (obstacleCollision == null)
        {
            obstacleCollision = obstacle.AddComponent<ObstacleCollision>();
        }
        obstacleCollision.damage = GetDamageByType(type);

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
        if (obstaclePrefabs != null && obstaclePrefabs.Count > 0)
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
            case ObstacleType.Cube:
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.transform.localScale = new Vector3(size, size, size);
                obstacle.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                break;
            case ObstacleType.Cylinder:
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                obstacle.transform.localScale = new Vector3(size, size, size);
                if (Random.value > 0.5f)
                {
                    obstacle.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                }
                break;
            case ObstacleType.Sphere:
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obstacle.transform.localScale = new Vector3(size, size, size);
                break;
        }
        return obstacle;
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
            case ObstacleType.Cylinder:
                return Mathf.Max(0f, cylinderDamage);
            case ObstacleType.Sphere:
                return Mathf.Max(0f, sphereDamage);
            case ObstacleType.Cube:
            default:
                return Mathf.Max(0f, cubeDamage);
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
