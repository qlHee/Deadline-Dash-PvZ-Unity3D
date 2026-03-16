using UnityEngine;
using System.Collections;

public class CherryBomb : MonoBehaviour
{
    [Header("樱桃炸弹设置")]
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
    public float triggerRadius = 5f; // 触发距离
    public float explosionRadius = 10f; // 爆炸范围
    public float explosionDamage = 47f; // 爆炸伤害
    [Tooltip("触发后到爆炸前的延迟时间（秒）")]
    public float explosionDelay = 1f; // 爆炸前的延迟时间
    public float maxScaleMultiplier = 1.5f; // 闪烁时最大的放大倍数
    
    [Header("爆炸特效")]
    public GameObject explosionEffectPrefab; // 爆炸特效预制体
    public Color flashColor = Color.red; // 闪烁颜色
    public int flashCount = 4; // 闪烁次数
    
    private bool isTriggered = false; // 是否已触发
    private PlayerController player;
    private Vector3 originalScale;
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] flashMaterials;
    
    void Start()
    {
        // 设置游戏对象标签
        gameObject.tag = "Obstacle";
        
        // 获取所有渲染器
        renderers = GetComponentsInChildren<Renderer>();
        
        // 保存原始材质和创建闪烁材质
        originalMaterials = new Material[renderers.Length];
        flashMaterials = new Material[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                originalMaterials[i] = renderers[i].material;
                flashMaterials[i] = new Material(renderers[i].material);
                flashMaterials[i].color = flashColor;
            }
        }
        
        // 保存原始缩放
        originalScale = transform.localScale;
        
        // 寻找玩家
        player = FindObjectOfType<PlayerController>();
        
        // 更新碰撞体和视觉效果大小
        UpdateScale();
    }
    
    void Update()
    {
        if (isTriggered || player == null || player.IsGameOver())
        {
            return;
        }
        
        // 检查玩家是否在触发范围内
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= triggerRadius)
        {
            isTriggered = true;
            StartCoroutine(ExplosionSequence());
        }
    }
    
    IEnumerator ExplosionSequence()
    {
        // 闪烁和缩放动画
        float elapsedTime = 0f;
        bool isFlashing = false;
        float flashInterval = explosionDelay / (flashCount * 2);
        
        while (elapsedTime < explosionDelay)
        {
            // 计算当前缩放因子
            float scaleProgress = elapsedTime / explosionDelay;
            float currentScaleFactor = 1f + (maxScaleMultiplier - 1f) * scaleProgress;
            transform.localScale = originalScale * currentScaleFactor;
            
            // 闪烁效果
            if (elapsedTime >= (isFlashing ? (flashInterval * 2) : flashInterval))
            {
                isFlashing = !isFlashing;
                
                // 切换材质
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].material = isFlashing ? flashMaterials[i] : originalMaterials[i];
                    }
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 爆炸
        Explode();
    }
    
    void Explode()
    {
        // 创建爆炸效果
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosionEffect, 2f); // 2秒后销毁特效
        }
        else
        {
            // 如果没有指定爆炸特效，创建一个简单的特效
            CreateDefaultExplosionEffect();
        }
        
        // 检测爆炸范围内的玩家并造成伤害
        if (player != null && !player.IsGameOver())
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= explosionRadius)
            {
                player.TakeDamage(explosionDamage);
            }
        }
        
        // 销毁自身
        Destroy(gameObject);
    }
    
    void CreateDefaultExplosionEffect()
    {
        // 创建一个简单的爆炸特效
        GameObject explosionObj = new GameObject("ExplosionEffect");
        explosionObj.transform.position = transform.position;
        
        // 创建粒子系统
        ParticleSystem particleSystem = explosionObj.AddComponent<ParticleSystem>();
        
        // 配置粒子系统
        var main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(Color.red, Color.yellow);
        main.startSize = 0.5f;
        main.startSpeed = 5f;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = particleSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 100) });
        
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.red, 0.0f), 
                new GradientColorKey(Color.yellow, 0.5f),
                new GradientColorKey(Color.black, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        
        colorOverLifetime.color = gradient;
        
        // 播放粒子系统
        particleSystem.Play();
        
        // 2秒后销毁特效对象
        Destroy(explosionObj, 2f);
    }
    
    // 当玩家直接碰到樱桃炸弹时立即爆炸
    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        
        if (other.CompareTag("Player"))
        {
            PlayerController playerCollider = other.GetComponent<PlayerController>();
            if (playerCollider != null && !playerCollider.IsGameOver())
            {
                isTriggered = true;
                // 直接触碰时立即爆炸，不需要延时
                Explode();
            }
        }
    }
    
    // 绘制触发范围和爆炸范围的可视化线框（仅在编辑器中显示）
    void OnDrawGizmosSelected()
    {
        // 绘制触发范围（黄色）
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        
        // 绘制爆炸范围（红色）
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
    
    /// <summary>
    /// 更新对象的缩放，同时影响视觉效果和碰撞体
    /// </summary>
    public void UpdateScale()
    {
        // 更新视觉模型大小
        if (model != null)
        {
            Transform visualTransform = transform.Find(model.name + "(Clone)");
            if (visualTransform != null)
            {
                visualTransform.localScale = Vector3.one * _scale;
            }
        }
        
        // 更新碰撞体大小
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {   
            sphereCollider.radius = _scale * 0.5f;
            sphereCollider.center = new Vector3(0f, _scale * 0.6f, 0f);
        }
        
        // 更新爆炸相关参数
        originalScale = transform.localScale;
    }
}
