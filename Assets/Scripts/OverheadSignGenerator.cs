using UnityEngine;

[RequireComponent(typeof(RoadGenerator))]
public class OverheadSignGenerator : MonoBehaviour
{
	[Header("路牌设置")]
	[Tooltip("距离玩家初始位置的Z轴距离")]
	public float signDistanceFromPlayerStart = 42f;
	[Tooltip("路牌板面底部距地面高度")]
	public float signClearHeight = 6f;
	[Tooltip("路牌板面高度")]
	public float signBoardHeight = 5f;
	[Tooltip("路牌板面厚度")]
	public float signBoardThickness = 0.2f;
	[Tooltip("路牌比跑道拓宽的总量（左右各扩展一半）")]
	public float signWidthExtra = 4f;
	[Tooltip("立柱截面厚度")]
	public float postThickness = 0.3f;
	[Tooltip("立柱距离路牌左右边缘的缩进量")]
	public float postEdgeInset = 0.6f;
	[Tooltip("文字相对板面前移距离，避免与板面Z-fighting")]
	public float textInset = 0.05f;

	[Header("路牌文字设置")]
	[Tooltip("TextMesh 的 characterSize")]
	public float textCharacterSize = 0.01f;
	[Tooltip("TextMesh 的 fontSize")]
	public int textFontSize = 90;
	[Tooltip("TextMesh 的 lineSpacing")]
	public float textLineSpacing = 1.2f;
	[Tooltip("文字沿板面Y轴的偏移（正值向上）")]
	public float textVerticalOffset = 0f;
	[Tooltip("统一缩放 Text 对象，1为默认大小")]
	public float textScaleMultiplier = 1f;

	private RoadGenerator roadGenerator;
	private bool signSpawned = false;

	private void Awake()
	{
		roadGenerator = GetComponent<RoadGenerator>();
	}

	private void Start()
	{
		if (signSpawned)
		{
			return;
		}

		if (roadGenerator == null)
		{
			Debug.LogWarning("[OverheadSignGenerator] 未找到 RoadGenerator，无法生成路牌。");
			return;
		}

		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		if (playerObj == null)
		{
			Debug.LogWarning("[OverheadSignGenerator] 未找到带有 Player 标签的物体，无法生成路牌。");
			return;
		}

		float roadWidth = roadGenerator.GetRoadWidth();
		float signZ = playerObj.transform.position.z + signDistanceFromPlayerStart;

		SpawnOverheadSign(signZ, roadWidth);
		signSpawned = true;
	}

	private void SpawnOverheadSign(float worldZ, float roadWidth)
	{
		GameObject signRoot = new GameObject("OverheadSign");
		signRoot.transform.parent = transform;
		signRoot.transform.position = new Vector3(0f, 0f, worldZ);

		float boardWidth = roadWidth + Mathf.Max(0f, signWidthExtra);
		float boardHeight = signBoardHeight;
		float boardThickness = signBoardThickness;

		float postYBottom = 0f;
		float postYTop = signClearHeight;
		float postHeight = postYTop - postYBottom;
		float halfBoardWidth = boardWidth / 2f;
		float postXOffset = halfBoardWidth - Mathf.Max(0f, postEdgeInset);

		// 左立柱
		GameObject leftPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
		leftPost.name = "LeftPost";
		leftPost.transform.parent = signRoot.transform;
		leftPost.transform.localPosition = new Vector3(-postXOffset, postYBottom + postHeight / 2f, 0f);
		leftPost.transform.localScale = new Vector3(postThickness, postHeight, postThickness);
		leftPost.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);

		// 右立柱
		GameObject rightPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
		rightPost.name = "RightPost";
		rightPost.transform.parent = signRoot.transform;
		rightPost.transform.localPosition = new Vector3(postXOffset, postYBottom + postHeight / 2f, 0f);
		rightPost.transform.localScale = new Vector3(postThickness, postHeight, postThickness);
		rightPost.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.4f);

		// 路牌板面
		GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
		board.name = "Board";
		board.transform.parent = signRoot.transform;
		board.transform.localPosition = new Vector3(0f, signClearHeight + boardHeight / 2f, 0f);
		board.transform.localScale = new Vector3(boardWidth, boardHeight, boardThickness);
		board.GetComponent<Renderer>().material.color = new Color(0.04f, 0.35f, 0.12f);

		// 白色边框
		GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
		border.name = "Border";
		border.transform.parent = board.transform;
		border.transform.localPosition = Vector3.zero;
		border.transform.localScale = new Vector3(1.04f, 1.10f, 0.05f);
		var borderRenderer = border.GetComponent<Renderer>();
		borderRenderer.material.color = Color.white;
		border.transform.localPosition = new Vector3(0f, 0f, -0.06f);

		// 文本
		GameObject textObj = new GameObject("Text");
		textObj.transform.parent = board.transform;
		textObj.transform.localPosition = new Vector3(0f, textVerticalOffset, textInset);
		textObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

		var textMesh = textObj.AddComponent<TextMesh>();
		textMesh.text =
			"Controls:\n" +
			"Move Left: ← / A\n" +
			"Move Right: → / D\n" +
			"Accelerate: ↑ / W\n" +
			"Brake: ↓ / S\n" +
			"Jump: Spacebar";
		textMesh.color = Color.white;
		textMesh.anchor = TextAnchor.MiddleCenter;
		textMesh.alignment = TextAlignment.Center;
		textMesh.characterSize = Mathf.Max(0.001f, textCharacterSize);
		textMesh.fontSize = Mathf.Max(1, textFontSize);
		textMesh.lineSpacing = Mathf.Max(0.1f, textLineSpacing);

		float scaleByWidth = Mathf.Clamp(boardWidth / 12f, 0.6f, 1.2f);
		float scaleByHeight = Mathf.Clamp(boardHeight / 3.0f, 0.6f, 1.2f);
		float textUniformScale = Mathf.Min(scaleByWidth, scaleByHeight) * Mathf.Max(0.001f, textScaleMultiplier);
		textObj.transform.localScale = new Vector3(textUniformScale/(1.2f), textUniformScale * (1.3f), 1f);
	}
}


