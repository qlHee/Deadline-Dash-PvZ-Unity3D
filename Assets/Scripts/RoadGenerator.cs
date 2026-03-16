using UnityEngine;
using System.Collections.Generic;

public class RoadGenerator : MonoBehaviour
{
    [Header("跑道设置")]
    public GameObject roadPrefab;             // 跑道预制体（如果不使用，会自动创建）
    public float roadLength = 50f;            // 每段跑道的长度
    public float roadWidth = 10f;             // 跑道宽度
    public float wallHeight = 3f;             // 墙壁高度
    public int initialRoadCount = 5;          // 初始生成的跑道段数
    public float spawnDistance = 100f;        // 在玩家前方多远生成新跑道

    [Header("材质设置")]
    public Material roadMaterial;
    public Material wallMaterial;

    private Transform playerTransform;
    private List<GameObject> activeRoads = new List<GameObject>();
    private float lastRoadEndZ = 0f;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // 生成初始跑道
        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoadSegment();
        }
    }

    void Update()
    {
        // 检查是否需要生成新的跑道
        if (lastRoadEndZ - playerTransform.position.z < spawnDistance)
        {
            SpawnRoadSegment();
        }

        // 移除玩家后方过远的跑道
        RemoveOldRoads();
    }

    void SpawnRoadSegment()
    {
        GameObject roadSegment = new GameObject("RoadSegment_" + activeRoads.Count);
        roadSegment.transform.position = new Vector3(0, 0, lastRoadEndZ);
        roadSegment.transform.parent = transform;

        // 创建地面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.parent = roadSegment.transform;
        ground.transform.localPosition = new Vector3(0, -0.5f, roadLength / 2);
        ground.transform.localScale = new Vector3(roadWidth, 1f, roadLength);
        ground.tag = "Ground";
        
        if (roadMaterial != null)
        {
            ground.GetComponent<Renderer>().material = roadMaterial;
        }
        else
        {
            ground.GetComponent<Renderer>().material.color = Color.gray;
        }

        // 创建左墙
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.parent = roadSegment.transform;
        leftWall.transform.localPosition = new Vector3(-roadWidth / 2 - 0.5f, wallHeight / 2, roadLength / 2);
        leftWall.transform.localScale = new Vector3(1f, wallHeight, roadLength);
        leftWall.tag = "Wall";
        
        if (wallMaterial != null)
        {
            leftWall.GetComponent<Renderer>().material = wallMaterial;
        }
        else
        {
            leftWall.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.8f);
        }

        // 创建右墙
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.parent = roadSegment.transform;
        rightWall.transform.localPosition = new Vector3(roadWidth / 2 + 0.5f, wallHeight / 2, roadLength / 2);
        rightWall.transform.localScale = new Vector3(1f, wallHeight, roadLength);
        rightWall.tag = "Wall";
        
        if (wallMaterial != null)
        {
            rightWall.GetComponent<Renderer>().material = wallMaterial;
        }
        else
        {
            rightWall.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.8f);
        }

        activeRoads.Add(roadSegment);
        lastRoadEndZ += roadLength;
    }

    void RemoveOldRoads()
    {
        // 移除玩家后方50米以外的跑道
        for (int i = activeRoads.Count - 1; i >= 0; i--)
        {
            if (activeRoads[i].transform.position.z + roadLength < playerTransform.position.z - 50f)
            {
                GameObject roadToRemove = activeRoads[i];
                activeRoads.RemoveAt(i);
                Destroy(roadToRemove);
            }
        }
    }

    public float GetRoadWidth()
    {
        return roadWidth;
    }
}

