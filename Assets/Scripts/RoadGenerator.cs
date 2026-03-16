using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    [Header("跑道设置")]
    public GameObject roadPrefab;
    public float roadLength = 50f;
    [Tooltip("跑道宽度（整数）")]
    [Range(6, 20)]
    public int roadWidth = 12;
    public float wallHeight = 3f;
    public int initialRoadCount = 5;
    public float spawnDistance = 100f;
    public float lookAheadTime = 4f;
    public float backCullDistance = 60f;

    [Header("材质设置")]
    public Material roadMaterial;
    public Material wallMaterial;

    [Header("草地装饰")]
    public GameObject[] grassPrefabs;
    public bool spawnGrassOnGeneratedSegments = true;
    public float grassEdgeOffset = 0.8f;
    public Vector2 grassSpacingRange = new Vector2(3f, 5f);
    public Vector2 grassScaleRange = new Vector2(0.8f, 1.3f);
    public float grassXJitter = 0.3f;
    public float grassZJitter = 0.5f;

    private Transform playerTransform;
    private readonly List<GameObject> activeRoads = new List<GameObject>();
    private float lastRoadEndZ;
    private PlayerController playerController;
    private readonly Dictionary<GameObject, Stack<GameObject>> grassPool = new Dictionary<GameObject, Stack<GameObject>>();

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[RoadGenerator] 未找到 Player，无法生成跑道。");
            enabled = false;
            return;
        }

        playerTransform = player.transform;
        playerController = player.GetComponent<PlayerController>();

        lastRoadEndZ = playerTransform.position.z - roadLength;

        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoadSegment();
        }

        EnsureAheadRoads();
    }

    void Update()
    {
        if (playerTransform == null) return;

        EnsureAheadRoads();

        RemoveOldRoads();
    }

    void EnsureAheadRoads()
    {
        float requiredAhead = GetRequiredAheadDistance();
        while (lastRoadEndZ - playerTransform.position.z < requiredAhead)
        {
            SpawnRoadSegment();
        }
    }

    void SpawnRoadSegment()
    {
        GameObject roadSegment;
        bool generatedInCode = roadPrefab == null;
        if (!generatedInCode)
        {
            roadSegment = Instantiate(roadPrefab, new Vector3(0f, 0f, lastRoadEndZ), Quaternion.identity, transform);
        }
        else
        {
            roadSegment = new GameObject("RoadSegment_" + activeRoads.Count);
            roadSegment.transform.SetParent(transform);
            roadSegment.transform.position = new Vector3(0f, 0f, lastRoadEndZ);
            BuildPrimitiveRoad(roadSegment.transform);
        }

        if (generatedInCode && spawnGrassOnGeneratedSegments)
        {
            PopulateGrass(roadSegment.transform);
        }

        activeRoads.Add(roadSegment);
        lastRoadEndZ += roadLength;
    }

    void BuildPrimitiveRoad(Transform parent)
    {
        CreateGround(parent);
        CreateWall(parent, "LeftWall", -roadWidth / 2f - 0.5f);
        CreateWall(parent, "RightWall", roadWidth / 2f + 0.5f);
    }

    void CreateGround(Transform parent)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(parent);
        ground.transform.localPosition = new Vector3(0f, -0.5f, roadLength / 2f);
        ground.transform.localScale = new Vector3(roadWidth, 1f, roadLength);
        TagUtility.TryAssignTag(ground, "Ground");

        Renderer renderer = ground.GetComponent<Renderer>();
        if (roadMaterial != null)
        {
            Material instanceMaterial = new Material(roadMaterial);
            instanceMaterial.mainTextureScale = new Vector2(roadWidth / 10f, roadLength / 10f);
            renderer.material = instanceMaterial;
        }
        else
        {
            renderer.material.color = Color.gray;
        }
    }

    void CreateWall(Transform parent, string name, float offsetX)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.localPosition = new Vector3(offsetX, wallHeight / 2f, roadLength / 2f);
        wall.transform.localScale = new Vector3(1f, wallHeight, roadLength);
        TagUtility.TryAssignTag(wall, "Wall");

        Renderer renderer = wall.GetComponent<Renderer>();
        if (wallMaterial != null)
        {
            renderer.material = wallMaterial;
        }
        else
        {
            renderer.material.color = new Color(0.3f, 0.3f, 0.8f);
        }
    }

    void PopulateGrass(Transform parent)
    {
        if (grassPrefabs == null || grassPrefabs.Length == 0)
        {
            return;
        }

        Transform grassRoot = EnsureGrassRoot(parent);

        for (int side = -1; side <= 1; side += 2)
        {
            float currentZ = 0f;
            while (currentZ < roadLength)
            {
                GameObject prefab = GetRandomGrassPrefab();
                if (prefab == null)
                {
                    break;
                }

                GameObject grass = GetGrassInstance(prefab, grassRoot);
                float sideSign = side;
                float xBase = (roadWidth / 2f + grassEdgeOffset) * sideSign;
                float jitterX = Random.Range(-grassXJitter, grassXJitter);
                float jitterZ = Random.Range(-grassZJitter, grassZJitter);
                float yRotation = Random.Range(0f, 360f);
                float scale = Random.Range(grassScaleRange.x, grassScaleRange.y);

                grass.transform.localPosition = new Vector3(xBase + jitterX, 0f, currentZ + jitterZ);
                grass.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
                grass.transform.localScale = Vector3.one * scale;

                currentZ += Random.Range(grassSpacingRange.x, grassSpacingRange.y);
            }
        }
    }

    Transform EnsureGrassRoot(Transform parent)
    {
        Transform root = parent.Find("GrassRoot");
        if (root == null)
        {
            var go = new GameObject("GrassRoot");
            root = go.transform;
            root.SetParent(parent, false);
        }
        return root;
    }

    GameObject GetRandomGrassPrefab()
    {
        if (grassPrefabs == null || grassPrefabs.Length == 0)
        {
            return null;
        }
        int index = Random.Range(0, grassPrefabs.Length);
        return grassPrefabs[index];
    }

    GameObject GetGrassInstance(GameObject prefab, Transform parent)
    {
        if (prefab == null)
        {
            return null;
        }

        if (!grassPool.TryGetValue(prefab, out Stack<GameObject> poolStack))
        {
            poolStack = new Stack<GameObject>();
            grassPool[prefab] = poolStack;
        }

        GameObject instance;
        if (poolStack.Count > 0)
        {
            instance = poolStack.Pop();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(prefab);
            var tag = instance.GetComponent<GrassPoolTag>();
            if (tag == null)
            {
                tag = instance.AddComponent<GrassPoolTag>();
            }
            tag.sourcePrefab = prefab;
        }

        instance.transform.SetParent(parent, false);
        return instance;
    }

    void ReturnGrassInstance(Transform instance)
    {
        if (instance == null) return;
        var tag = instance.GetComponent<GrassPoolTag>();
        if (tag == null || tag.sourcePrefab == null)
        {
            Destroy(instance.gameObject);
            return;
        }

        if (!grassPool.TryGetValue(tag.sourcePrefab, out Stack<GameObject> poolStack))
        {
            poolStack = new Stack<GameObject>();
            grassPool[tag.sourcePrefab] = poolStack;
        }

        instance.gameObject.SetActive(false);
        instance.SetParent(transform, false);
        poolStack.Push(instance.gameObject);
    }

    void RemoveOldRoads()
    {
        for (int i = activeRoads.Count - 1; i >= 0; i--)
        {
            if (activeRoads[i] == null)
            {
                activeRoads.RemoveAt(i);
                continue;
            }

            if (activeRoads[i].transform.position.z + roadLength < playerTransform.position.z - backCullDistance)
            {
                RecycleGrassInstances(activeRoads[i].transform);
                Destroy(activeRoads[i]);
                activeRoads.RemoveAt(i);
            }
        }
    }

    void RecycleGrassInstances(Transform segment)
    {
        if (segment == null) return;
        Transform grassRoot = segment.Find("GrassRoot");
        if (grassRoot == null) return;

        for (int i = grassRoot.childCount - 1; i >= 0; i--)
        {
            ReturnGrassInstance(grassRoot.GetChild(i));
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

    public float GetRoadWidth()
    {
        return roadWidth;
    }

    private class GrassPoolTag : MonoBehaviour
    {
        public GameObject sourcePrefab;
    }

    void OnValidate()
    {
        if (roadWidth < 6) roadWidth = 6;
        if (roadWidth > 20) roadWidth = 20;

        UpdateRelatedComponents();
    }

    void UpdateRelatedComponents()
    {
        ObstacleSpawner obstacleSpawner = GetComponent<ObstacleSpawner>();
        if (obstacleSpawner != null)
        {
            obstacleSpawner.UpdateRoadWidth(roadWidth);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.UpdateBoundaries(roadWidth);
            }
        }
    }
}
