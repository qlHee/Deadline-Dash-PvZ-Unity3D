using UnityEngine;

[RequireComponent(typeof(RoadGenerator))]
public class OverheadSignGenerator : MonoBehaviour
{
    [Header("路牌设置")]
    public float signDistanceFromPlayerStart = 42f;
    public float signClearHeight = 6f;
    public float signBoardHeight = 5f;
    public float signBoardThickness = 0.2f;
    public float signWidthExtra = 4f;
    public float postThickness = 0.3f;
    public float postEdgeInset = 0.6f;
    public float textInset = 0.05f;

    [Header("路牌文字设置")]
    public float textCharacterSize = 0.01f;
    public int textFontSize = 90;
    public float textLineSpacing = 1.2f;
    public float textVerticalOffset = 0f;
    public float textScaleMultiplier = 1f;

    private RoadGenerator roadGenerator;
    private bool signSpawned;

    void Awake()
    {
        roadGenerator = GetComponent<RoadGenerator>();
    }

    void Start()
    {
        if (signSpawned || roadGenerator == null)
        {
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("[OverheadSignGenerator] 未找到 Player，无法生成路牌。");
            return;
        }

        float roadWidth = roadGenerator.GetRoadWidth();
        float signZ = playerObj.transform.position.z + signDistanceFromPlayerStart;

        SpawnOverheadSign(signZ, roadWidth);
        signSpawned = true;
    }

    void SpawnOverheadSign(float worldZ, float roadWidth)
    {
        GameObject signRoot = new GameObject("OverheadSign");
        signRoot.transform.SetParent(transform);
        signRoot.transform.position = new Vector3(0f, 0f, worldZ);

        float boardWidth = roadWidth + Mathf.Max(0f, signWidthExtra);
        float halfBoardWidth = boardWidth * 0.5f;
        float postYBottom = 0f;
        float postYTop = signClearHeight;
        float postHeight = postYTop - postYBottom;
        float postXOffset = halfBoardWidth - Mathf.Max(0f, postEdgeInset);

        // 左立柱
        CreatePost(signRoot.transform, "LeftPost", new Vector3(-postXOffset, postYBottom + postHeight / 2f, 0f), postHeight);
        // 右立柱
        CreatePost(signRoot.transform, "RightPost", new Vector3(postXOffset, postYBottom + postHeight / 2f, 0f), postHeight);

        // 路牌本体
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "Board";
        board.transform.SetParent(signRoot.transform);
        board.transform.localPosition = new Vector3(0f, signClearHeight + signBoardHeight / 2f, 0f);
        board.transform.localScale = new Vector3(boardWidth, signBoardHeight, signBoardThickness);
        board.GetComponent<Renderer>().material.color = new Color(0.04f, 0.35f, 0.12f);

        // 白色边框
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = "Border";
        border.transform.SetParent(board.transform);
        border.transform.localPosition = new Vector3(0f, 0f, -0.06f);
        border.transform.localScale = new Vector3(1.04f, 1.10f, 0.05f);
        border.GetComponent<Renderer>().material.color = Color.white;

        // 文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(board.transform);
        textObj.transform.localPosition = new Vector3(0f, textVerticalOffset, textInset);
        textObj.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        
        string modeName = "无尽模式";
        if (GameModeManager.Instance != null)
        {
            GameModeData modeData = GameModeManager.Instance.GetSelectedModeData();
            if (modeData != null)
            {
                modeName = modeData.modeName;
            }
        }
        
        textMesh.text = modeName;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = Mathf.Max(0.001f, textCharacterSize);
        textMesh.fontSize = Mathf.Max(1, textFontSize);
        textMesh.lineSpacing = Mathf.Max(0.1f, textLineSpacing);

        float scaleByWidth = Mathf.Clamp(boardWidth / 12f, 0.6f, 1.2f);
        float scaleByHeight = Mathf.Clamp(signBoardHeight / 3f, 0.6f, 1.2f);
        float textUniformScale = Mathf.Min(scaleByWidth, scaleByHeight) * Mathf.Max(0.001f, textScaleMultiplier);
        textObj.transform.localScale = new Vector3(textUniformScale / 1.2f, textUniformScale * 1.3f, 1f);
    }

    void CreatePost(Transform parent, string name, Vector3 localPos, float height)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.name = name;
        post.transform.SetParent(parent);
        post.transform.localPosition = localPos;
        post.transform.localScale = new Vector3(postThickness, height, postThickness);
        post.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);
    }
}
