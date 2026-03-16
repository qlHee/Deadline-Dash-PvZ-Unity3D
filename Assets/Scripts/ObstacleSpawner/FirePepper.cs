using System.Collections;
using UnityEngine;

/// <summary>
/// 火爆辣椒：靠近时膨胀+抖动后爆炸，随机纵向/横向燃烧区域，玩家可跳跃躲避。
/// </summary>
public class FirePepper : MonoBehaviour
{
    [Header("基础参数")]
    public float triggerRadius = 6f;
    public float growDuration = 0.8f;
    public float maxScaleMultiplier = 1.6f;
    public float wobbleDuration = 0.25f;
    public float wobbleMagnitude = 0.1f;

    [Header("伤害参数")]
    public float explosionDamage = 50f;
    public float burnDuration = 3f;
    public float burnDamagePerSecond = 12f;
    [Tooltip("燃烧高度，玩家跳跃高于此值则不受伤害")]
    public float burnHeight = 1.4f;

    [Header("区域尺寸")]
    public float verticalLength = 8f;
    public float verticalWidth = 1.5f;
    public float horizontalWidth = 12f;
    public float horizontalDepth = 3f;
    [Tooltip("特效段的间距（横向/纵向实例化用）")]
    public float effectSpacing = 1f;

    [Header("特效")]
    public GameObject effectPrefab;              // 一个槽：用于爆炸/燃烧，可通过旋转/缩放适配
    public float effectLifetime = 3f;
    public bool autoScaleEffect = true;
    [Tooltip("由 Spawner 传入，跑道 X 最小/最大，用于横向铺满")]
    public float laneMinX = -6f;
    public float laneMaxX = 6f;
    [Tooltip("燃烧间隔（秒），0.5 表示每 0.5s 触发一次持续伤害")]
    public float burnTickInterval = 0.5f;

    private Transform player;
    private bool isTriggered;
    private Vector3 originalScale;
    private float triggerTime;
    private Collider cachedCollider;
    private Renderer[] cachedRenderers;
    private Vector3 basePosition; // 基于模型的地表位置（x/z 用模型中心，y 用模型底部）
    private float lastBurnDamageTime = Mathf.NegativeInfinity;

    void Start()
    {
        TagUtility.TryAssignTag(gameObject, "Obstacle");
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        originalScale = transform.localScale;
        cachedCollider = GetComponent<Collider>();
        if (cachedCollider != null) cachedCollider.isTrigger = true;
        cachedRenderers = GetComponentsInChildren<Renderer>();

        basePosition = transform.position;
        if (cachedRenderers != null && cachedRenderers.Length > 0)
        {
            Bounds b = cachedRenderers[0].bounds;
            for (int i = 1; i < cachedRenderers.Length; i++)
            {
                b.Encapsulate(cachedRenderers[i].bounds);
            }
            basePosition = new Vector3(b.center.x, b.min.y, b.center.z);
        }
    }

    void Update()
    {
        if (isTriggered) return;
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= triggerRadius)
        {
            isTriggered = true;
            StartCoroutine(TriggerSequence());
        }
    }

    IEnumerator TriggerSequence()
    {
        triggerTime = Time.time;

        // 生长阶段
        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            float t = elapsed / growDuration;
            float scaleMul = Mathf.Lerp(1f, maxScaleMultiplier, t);
            transform.localScale = originalScale * scaleMul;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 抖动
        elapsed = 0f;
        Vector3 basePos = transform.position;
        while (elapsed < wobbleDuration)
        {
            transform.position = basePos + Random.insideUnitSphere * wobbleMagnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = basePos;

        ExplodeAndBurn();
    }

    void ExplodeAndBurn()
    {
        bool vertical = Random.value < 0.5f;
        Quaternion effectRot = vertical ? transform.rotation : Quaternion.identity;

        var areas = BuildDamageAreas(vertical);

        // 爆炸瞬时伤害（对每个分段）
        foreach (var area in areas)
        {
            ApplyDamageIfHit(area.center, area.size, explosionDamage);
        }

        // 特效（分段铺开）
        SpawnSegmentedEffects(areas, effectRot);

        // 持续燃烧
        StartCoroutine(BurnCoroutine(areas));

        // 隐藏自身模型
        DisableRenderers();
    }

    IEnumerator BurnCoroutine(System.Collections.Generic.List<Bounds> areas)
    {
        float endTime = Time.time + burnDuration;

        // 进入燃烧立即结算一次（防止横向快速踩过不受伤）
        DoBurnTick(areas, burnDamagePerSecond);
        lastBurnDamageTime = Time.time;
        bool wasInside = true;

        while (Time.time < endTime)
        {
            bool insideNow = IsPlayerInsideAny(areas);
            if (insideNow && !wasInside)
            {
                DoBurnTick(areas, burnDamagePerSecond);
                lastBurnDamageTime = Time.time;
            }
            wasInside = insideNow;

            // 当间隔满足时再结算
            if (Time.time - lastBurnDamageTime >= burnTickInterval)
            {
                DoBurnTick(areas, burnDamagePerSecond);
                lastBurnDamageTime = Time.time;
            }
            yield return null;
        }

        // 燃烧结束后销毁自身
        Destroy(gameObject);
    }

    bool IsPlayerInsideAny(System.Collections.Generic.List<Bounds> areas)
    {
        if (player == null) return false;
        foreach (var area in areas)
        {
            if (IsPlayerInside(area))
            {
                return true;
            }
        }
        return false;
    }

    bool IsPlayerInside(Bounds area)
    {
        Vector3 playerPos = player.position;
        if (!area.Contains(playerPos))
        {
            Collider pcCol = player.GetComponent<Collider>();
            if (pcCol == null) return false;
            if (!area.Intersects(pcCol.bounds)) return false;
        }

        Collider collider = player.GetComponent<Collider>();
        if (collider != null)
        {
            float areaTop = area.center.y + area.size.y * 0.5f;
            if (collider.bounds.min.y > areaTop)
            {
                return false;
            }
        }

        return true;
    }

    void DoBurnTick(System.Collections.Generic.List<Bounds> areas, float damage)
    {
        foreach (var area in areas)
        {
            ApplyDamageIfHit(area.center, area.size, damage);
        }
    }

    void ApplyDamageIfHit(Vector3 areaCenter, Vector3 areaSize, float damage)
    {
        if (player == null) return;

        Bounds bounds = new Bounds(areaCenter, areaSize);
        Vector3 playerPos = player.position;
        if (!bounds.Contains(playerPos))
        {
            // 简单再用玩家 collider 检测
            Collider pcCol = player.GetComponent<Collider>();
            if (pcCol == null) return;
            if (!bounds.Intersects(pcCol.bounds)) return;
        }

        // 跳跃躲避：若玩家底部高于燃烧高度则不受伤
        Collider collider = player.GetComponent<Collider>();
        if (collider != null)
        {
            float areaTop = areaCenter.y + areaSize.y * 0.5f;
            if (collider.bounds.min.y > areaTop)
            {
                return;
            }
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null && !pc.IsGameOver())
        {
            pc.ApplyObstacleDamage(damage);
        }
    }

    void SpawnSegmentedEffects(System.Collections.Generic.List<Bounds> areas, Quaternion rot)
    {
        if (effectPrefab == null) return;

        foreach (var area in areas)
        {
            SpawnEffectInstance(area.center, area.size, rot);
        }
    }

    System.Collections.Generic.List<Bounds> BuildDamageAreas(bool vertical)
    {
        var list = new System.Collections.Generic.List<Bounds>();
        float spacing = Mathf.Max(0.5f, effectSpacing);
        float halfStep = spacing * 0.5f;

        if (vertical)
        {
            float endZ = basePosition.z + verticalLength;
            for (float z = basePosition.z + halfStep; z <= endZ + halfStep * 0.5f; z += spacing)
            {
                Vector3 center = new Vector3(basePosition.x, basePosition.y, z);
                Vector3 size = new Vector3(verticalWidth, burnHeight, spacing);
                list.Add(new Bounds(center, size));
            }
        }
        else
        {
            float startX = laneMinX + halfStep;
            float endX = laneMaxX + halfStep * 0.5f;
            for (float x = startX; x <= endX; x += spacing)
            {
                Vector3 center = new Vector3(x, basePosition.y, basePosition.z);
                Vector3 size = new Vector3(spacing, burnHeight, horizontalDepth);
                list.Add(new Bounds(center, size));
            }
        }

        return list;
    }

    void SpawnEffectInstance(Vector3 pos, Vector3 areaSize, Quaternion rot)
    {
        GameObject fx = Instantiate(effectPrefab, pos, rot);
        if (autoScaleEffect)
        {
            fx.transform.localScale = new Vector3(areaSize.x, fx.transform.localScale.y, areaSize.z);
        }
        if (transform.parent != null)
        {
            fx.transform.SetParent(transform.parent, true);
        }
        if (effectLifetime > 0f)
        {
            Destroy(fx, effectLifetime);
        }
    }

    void DisableRenderers()
    {
        if (cachedRenderers == null) return;
        foreach (var r in cachedRenderers)
        {
            if (r != null) r.enabled = false;
        }
    }
}
