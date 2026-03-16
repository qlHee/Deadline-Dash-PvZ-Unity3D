using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("障碍物设置")]
    public Material obstacleMaterial;
    public float spawnDistance = 80f;          // 在玩家前方多远开始生成障碍物
    public float minSpawnDistance = 70f;       // 最小生成距离
    public float despawnDistance = 30f;        // 玩家后方多远移除障碍物
    public float safeZoneDistance = 20f;       // 玩家附近的安全区域（不生成不删除）

    [Header("障碍物间距")]
    public float minObstacleDistance = 8f;     // 障碍物之间的最小距离
    public float maxObstacleDistance = 15f;    // 障碍物之间的最大距离

    [Header("障碍物尺寸范围")]
    public float minObstacleSize = 1f;         // 最小障碍物尺寸
    public float maxObstacleSize = 2.5f;       // 最大障碍物尺寸

    [Header("障碍物伤害")]
    public float cubeDamage = 25f;
    public float cylinderDamage = 30f;
    public float sphereDamage = 20f;

    [Header("跑道设置")]
    public float roadWidth = 10f;              // 跑道宽度
    public float roadMargin = 1.5f;            // 边缘留白

    private Transform playerTransform;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private float nextSpawnZ = 30f;            // 下一个障碍物生成的Z位置
    private PlayerController playerController;

    // 障碍物类型枚举
    enum ObstacleType
    {
        Cube,
        Cylinder,
        Sphere
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = playerTransform.GetComponent<PlayerController>();
        
        // 初始化下一个生成位置
        nextSpawnZ = playerTransform.position.z + minSpawnDistance;

        // 预生成一些障碍物
        for (int i = 0; i < 10; i++)
        {
            SpawnObstacle();
        }
    }

    void Update()
    {
        if (playerController != null && playerController.IsGameOver())
            return;

        // 检查是否需要生成新障碍物
        while (nextSpawnZ - playerTransform.position.z < spawnDistance)
        {
            SpawnObstacle();
        }

        // 移除玩家后方的障碍物（但要避开安全区域）
        RemoveOldObstacles();
    }

    void SpawnObstacle()
    {
        // 随机选择障碍物类型
        ObstacleType type = (ObstacleType)Random.Range(0, 3);
        
        // 随机生成障碍物尺寸
        float size = Random.Range(minObstacleSize, maxObstacleSize);
        
        // 随机生成X位置（在跑道范围内）
        float minX = -(roadWidth / 2) + roadMargin;
        float maxX = (roadWidth / 2) - roadMargin;
        float randomX = Random.Range(minX, maxX);
        
        // 创建障碍物
        GameObject obstacle = null;
        Vector3 position = new Vector3(randomX, size / 2, nextSpawnZ);
        
        switch (type)
        {
            case ObstacleType.Cube:
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.transform.localScale = new Vector3(size, size, size);
                // 随机旋转增加变化
                obstacle.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                break;
                
            case ObstacleType.Cylinder:
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                obstacle.transform.localScale = new Vector3(size, size, size);
                // 圆柱体可以横着或竖着
                if (Random.value > 0.5f)
                {
                    obstacle.transform.rotation = Quaternion.Euler(0, 0, 90);
                }
                break;
                
            case ObstacleType.Sphere:
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obstacle.transform.localScale = new Vector3(size, size, size);
                break;
        }
        
        if (obstacle != null)
        {
            obstacle.name = "Obstacle_" + type.ToString() + "_" + activeObstacles.Count;
            obstacle.transform.position = position;
            obstacle.tag = "Obstacle";
            obstacle.transform.parent = transform;
            
            // 确保障碍物有碰撞体，并设置为Trigger
            Collider collider = obstacle.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;  // 设置为Trigger模式以确保碰撞检测
            }
            
            // 添加碰撞检测脚本并设置伤害
            ObstacleCollision obstacleCollision = obstacle.AddComponent<ObstacleCollision>();
            obstacleCollision.damage = GetDamageByType(type);
            
            // 设置材质和颜色
            Renderer renderer = obstacle.GetComponent<Renderer>();
            if (obstacleMaterial != null)
            {
                renderer.material = obstacleMaterial;
            }
            else
            {
                // 随机颜色
                Color[] colors = new Color[] 
                { 
                    Color.red, 
                    new Color(1f, 0.5f, 0f), // 橙色
                    Color.yellow, 
                    Color.magenta,
                    new Color(0.5f, 0f, 0.5f) // 紫色
                };
                renderer.material.color = colors[Random.Range(0, colors.Length)];
            }
            
            activeObstacles.Add(obstacle);
        }
        
        // 计算下一个障碍物的位置
        float distance = Random.Range(minObstacleDistance, maxObstacleDistance);
        nextSpawnZ += distance;
    }

    void RemoveOldObstacles()
    {
        // 移除玩家后方足够远且不在安全区域内的障碍物
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null) continue;
            
            float obstacleZ = activeObstacles[i].transform.position.z;
            float distanceFromPlayer = obstacleZ - playerTransform.position.z;
            
            // 只移除在玩家后方且超过安全距离的障碍物
            if (distanceFromPlayer < -despawnDistance)
            {
                GameObject obstacleToRemove = activeObstacles[i];
                activeObstacles.RemoveAt(i);
                Destroy(obstacleToRemove);
            }
        }
    }

    // 清除所有障碍物（游戏结束时调用）
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
}

