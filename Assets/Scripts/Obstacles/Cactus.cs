using System.Collections;
using UnityEngine;

public class Cactus : MonoBehaviour
{
    [Header("外观设置")]
    public GameObject model;
    [SerializeField] private float _scale = 1.5f;
    public float scale {
        get { return _scale; }
        set {
            _scale = value;
            if (Application.isPlaying) {
                UpdateScale();
            }
        }
    }

    [Header("伤害设置")]
    public float damage = 47f;

    [Header("射击设置")]
    public float fireInterval = 2f;
    public float projectileSpeed = 18f;
    public float projectileDamage = 27f;
    public float projectileLifetime = 5f;
    public Color projectileColor = new Color32(255, 223, 128, 255);

    [Header("尖刺高度设置")]
    public float spikeHeight = 2.5f;
    public GameObject spikeProjectileModel;

    private Transform muzzle;
    private PlayerController player;
    private float nextFireTime;
    private bool hasHit = false;
    private Renderer[] renderers;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        nextFireTime = Time.time + Random.Range(0f, Mathf.Max(0.1f, fireInterval));
        renderers = GetComponentsInChildren<Renderer>(true);
        UpdateScale();
    }

    void Update()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + Mathf.Max(0.1f, fireInterval);
        Fire();
    }

    public void AssignMuzzle(Transform muzzleTransform)
    {
        muzzle = muzzleTransform;
    }

    public void SetSpikeHeight(float height)
    {
        spikeHeight = Mathf.Max(0f, height);
    }

    public void SetProjectileDamage(float damage)
    {
        projectileDamage = Mathf.Max(0f, damage);
    }

    void Fire()
    {
        if (player != null && player.IsGameOver())
        {
            return;
        }

        Transform spawnOrigin = muzzle != null ? muzzle : transform;
        Vector3 direction = GetFireDirection();

        Vector3 spawnPosition = spawnOrigin.position;
        spawnPosition.y = spikeHeight;

        GameObject projectileObject;
        
        if (spikeProjectileModel != null)
        {
            projectileObject = Instantiate(spikeProjectileModel);
            projectileObject.name = "SpikeProjectile";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            projectileObject.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            projectileObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            projectileObject.name = "SpikeProjectile";
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            projectileObject.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
        }

        Rigidbody rb = projectileObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectileObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // 使用SphereCollider更可靠，性能也更好
        Collider collider = projectileObject.GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphereCollider = projectileObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.3f;
            collider = sphereCollider;
        }
        collider.isTrigger = true;

        // 尖刺不设置特殊标签，由SpikeProjectile组件自己处理碰撞
        // 不设置为Obstacle标签，避免与玩家的障碍物碰撞逻辑冲突

        Renderer renderer = projectileObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = projectileColor;
        }

        SpikeProjectile projectile = projectileObject.AddComponent<SpikeProjectile>();
        projectile.Initialize(projectileDamage, projectileSpeed, direction, projectileLifetime);
    }

    Vector3 GetFireDirection()
    {
        return Vector3.back;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (hasHit) return;
            
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.IsGameOver())
            {
                playerController.TakeDamage(damage);
                hasHit = true;
                
                // 禁用碰撞体
                BoxCollider boxCollider = GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.enabled = false;
                }
                
                // 闪烁后消失
                StartCoroutine(FlashAndDisappear());
            }
        }
    }
    
    IEnumerator FlashAndDisappear()
    {
        const float duration = 0.5f;
        const float interval = 0.1f;
        
        float elapsed = 0f;
        float nextToggle = interval;
        bool visible = true;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= nextToggle)
            {
                nextToggle += interval;
                visible = !visible;
                SetRenderersEnabled(visible);
            }
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void SetRenderersEnabled(bool enabled)
    {
        if (renderers == null) return;
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = enabled;
            }
        }
    }
    
    /// <summary>
    /// 更新对象的缩放，同时影响视觉效果和碰撞体
    /// </summary>
    public void UpdateScale()
    {
        // 更新视觉模型大小 - Y轴拉长1.1倍
        if (model != null)
        {
            Transform visualTransform = transform.Find(model.name + "(Clone)");
            if (visualTransform != null)
            {
                visualTransform.localScale = new Vector3(_scale, _scale * 1.1f, _scale);
            }
        }
        
        // 更新碰撞体大小和位置
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {   
            // 计算碰撞体高度（1.1倍）
            float colliderHeight = _scale * 2.0f * 1.1f;
            
            // 获取视觉模型的实际bounds，计算底部到父对象原点的距离
            Renderer renderer = GetComponentInChildren<Renderer>();
            float bottomOffset = 0f;
            if (renderer != null)
            {
                // 将世界坐标的底部转换为本地坐标
                float worldBottom = renderer.bounds.min.y;
                float localBottom = transform.InverseTransformPoint(new Vector3(0f, worldBottom, 0f)).y;
                bottomOffset = localBottom;
            }
            
            // 碰撞体的center需要从视觉模型底部开始，向上延伸colliderHeight
            boxCollider.size = new Vector3(_scale, colliderHeight, _scale);
            boxCollider.center = new Vector3(0f, bottomOffset + colliderHeight * 0.5f, 0f);
        }
        
        // 更新尖刺高度以匹配新的缩放（也要考虑1.1倍）
        if (muzzle != null)
        {
            muzzle.localPosition = new Vector3(0f, _scale * 1.5f * 1.1f, -_scale * 0.3f);
        }
    }
}
