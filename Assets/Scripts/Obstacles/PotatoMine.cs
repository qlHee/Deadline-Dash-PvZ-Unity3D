using UnityEngine;

/// <summary>
/// 土豆地雷：远处时埋入地面，靠近时慢慢上升；碰到玩家触发特效并造成伤害。
/// 将本脚本挂在土豆地雷预制体上，并配置碰撞体（isTrigger=true）。
/// </summary>
public class PotatoMine : MonoBehaviour
{
    [Header("基础参数")]
    public float damage = 40f;
    [Tooltip("玩家靠近到该距离内开始上升")]
    public float activationDistance = 8f;
    [Tooltip("离开距离时重新下沉")]
    public float deactivationDistance = 10f;
    [Tooltip("埋入比例，0-1，表示高度有多少在地底下")]
    [Range(0f, 1f)] public float buriedHeightRatio = 0.66f;
    public float ascendSpeed = 2.5f;
    public float descendSpeed = 3f;

    [Header("特效")]
    public GameObject triggerEffectPrefab;   // 触发时的特效
    public float triggerEffectLifetime = 2f;

    private Transform player;
    private Vector3 groundPosition;
    private Vector3 buriedPosition;
    private bool isTriggered = false;
    private bool wasPlayerNear = false;
    private Collider[] cachedColliders;
    private bool initialized = false;

    void Start()
    {
        if (!initialized)
        {
            SetPositions(transform.position);
        }

        cachedColliders = GetComponentsInChildren<Collider>();
        if (cachedColliders != null && cachedColliders.Length > 0)
        {
            foreach (var col in cachedColliders)
            {
                if (col != null) col.isTrigger = true;
            }
        }
        else
        {
            // 没有碰撞体时补一个触发体
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = Mathf.Max(0.3f, GetBuriedDepth() * 0.5f);
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // 供 Spawner 在对齐地面后调用，保证地表位置正确
    public void InitializePositions(Vector3 surfacePosition)
    {
        SetPositions(surfacePosition);
        initialized = true;
    }

    private void SetPositions(Vector3 surfacePosition)
    {
        groundPosition = surfacePosition;
        buriedPosition = groundPosition + Vector3.down * GetBuriedDepth();
        transform.position = buriedPosition;
    }

    void Update()
    {
        if (isTriggered) return;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool playerNear = distance <= activationDistance;
        bool playerFar = distance >= deactivationDistance;

        Vector3 targetPos = transform.position;
        if (playerNear)
        {
            targetPos = groundPosition;
        }
        else if (playerFar || wasPlayerNear)
        {
            targetPos = buriedPosition;
        }

        float speed = targetPos.y > transform.position.y ? ascendSpeed : descendSpeed;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        wasPlayerNear = playerNear && !playerFar;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null && !pc.IsGameOver())
            {
                isTriggered = true;

                // 确保爆炸位置在地表
                transform.position = groundPosition;

                if (!(pc.WasFireShieldAbsorbedThisFrame() || pc.TryConsumeFireShield()))
                {
                    pc.ApplyObstacleDamage(damage, DamageType.Fire);
                }
                SpawnTriggerEffect(GetExplosionPosition());
                // 触发后禁用碰撞，避免重复
                if (cachedColliders != null)
                {
                    foreach (var col in cachedColliders)
                    {
                        if (col != null) col.enabled = false;
                    }
                }
                Destroy(gameObject, 0.05f);
            }
        }
    }

    private Vector3 GetExplosionPosition()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }
            return new Vector3(b.center.x, b.center.y, b.center.z);
        }
        return groundPosition;
    }

    private void SpawnTriggerEffect(Vector3 pos)
    {
        if (triggerEffectPrefab == null) return;

        Quaternion rot = transform.rotation;
        GameObject fx = Instantiate(triggerEffectPrefab, pos, rot);
        // 不挂在自己上，避免随销毁一起消失；放到父节点保持层级
        if (transform.parent != null)
        {
            fx.transform.SetParent(transform.parent, true);
        }
        if (triggerEffectLifetime > 0f)
        {
            Destroy(fx, triggerEffectLifetime);
        }
    }

    private float GetBuriedDepth()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
        {
            return 1f * buriedHeightRatio;
        }

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            b.Encapsulate(renderers[i].bounds);
        }

        return b.size.y * buriedHeightRatio;
    }
}
