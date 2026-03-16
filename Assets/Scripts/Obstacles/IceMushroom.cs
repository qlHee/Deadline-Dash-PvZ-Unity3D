using UnityEngine;
using System.Collections;

public class IceMushroom : MonoBehaviour
{
    [Header("冰菇设置")]
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
    public float damage = 17f;
    public float freezeDuration = 2f;
    
    [Header("冻结效果")]
    public Color freezeScreenColor = new Color(0.5f, 0.8f, 1f, 0.3f); // 淡蓝色
    
    [Header("触发特效")]
    public GameObject mushroomEffectPrefab; // 冰菇自身触发特效
    public float mushroomEffectLifetime = 2f;
    public GameObject mushroomEffectPrefab2; // 冰菇自身额外特效
    public float mushroomEffectLifetime2 = 2f;
    public GameObject playerEffectPrefab;   // 作用在玩家身上的特效
    public float playerEffectLifetime = 2f;

    private bool hasTriggered = false;
    
    void Start()
    {
        // 确保设置正确的游戏对象标签
        gameObject.tag = "Obstacle";
        
        // 更新碰撞体和视觉效果大小
        UpdateScale();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsGameOver())
            {
                hasTriggered = true;

                bool iceShieldBlocked = player.WasIceShieldAbsorbedThisFrame() || player.TryConsumeIceShield();
                SpawnEffect(mushroomEffectPrefab, transform, mushroomEffectLifetime);
                SpawnEffect(mushroomEffectPrefab2, transform, mushroomEffectLifetime2);

                if (iceShieldBlocked)
                {
                    return;
                }

                // 应用伤害
                bool damaged = player.TakeDamage(damage, DamageType.Ice);
                if (damaged)
                {
                    // 创建冻结效果
                    // 确保初始化 FrozenEffect 并调用冻结方法
                    FrozenEffect.Instance.freezeScreenColor = freezeScreenColor;
                    FrozenEffect.Instance.ApplyFreezeEffect(player, freezeDuration);
                    SpawnPlayerEffect(player.transform, playerEffectLifetime);
                }
            }
        }
    }
    
    private void SpawnEffect(GameObject prefab, Transform target, float lifetime)
    {
        if (prefab == null || target == null) return;

        GameObject fx = Instantiate(prefab, target.position, Quaternion.identity);
        fx.transform.SetParent(target, true); // 绑定到目标以便跟随

        if (lifetime > 0f)
        {
            Destroy(fx, lifetime);
        }
    }
    
    private void SpawnPlayerEffect(Transform target, float lifetime)
    {
        if (target == null) return;

        if (playerEffectPrefab != null)
        {
            SpawnEffect(playerEffectPrefab, target, lifetime);
            return;
        }

        GameObject fx = CreateDefaultPlayerFreezeEffect(target);
        if (fx != null && lifetime > 0f)
        {
            Destroy(fx, lifetime);
        }
    }
    
    // 当没有指定玩家特效时的默认冰冻特效（简单蓝色粒子环绕）
    private GameObject CreateDefaultPlayerFreezeEffect(Transform target)
    {
        GameObject fx = new GameObject("DefaultPlayerFreezeEffect");
        fx.transform.SetParent(target, true);
        
        // 尽量放在角色身体中部（使用碰撞体中心作为参考，避免落到地面下）
        Vector3 localOffset = Vector3.up * 0.5f;
        Collider col = target.GetComponentInChildren<Collider>();
        if (col != null)
        {
            localOffset = target.InverseTransformPoint(col.bounds.center);
        }
        fx.transform.localPosition = localOffset;
        
        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.6f, 0.9f, 1f, 0.8f), new Color(0.2f, 0.5f, 1f, 0.5f));
        main.startSize = 0.4f;
        main.startLifetime = 0.6f;
        main.startSpeed = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 80;
        main.loop = true;

        var emission = ps.emission;
        emission.rateOverTime = 40f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.6f, 0.9f, 1f), 0f),
                new GradientColorKey(new Color(0.2f, 0.5f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // 绑定一个可用的粒子材质，避免默认材质在 SRP 下出现粉色
        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }
        if (shader != null)
        {
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", Color.white);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", Color.white);
            }
            psr.material = mat;
        }

        ps.Play();
        return fx;
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
            sphereCollider.radius = _scale * 0.6f;
            sphereCollider.center = new Vector3(0f, _scale * 0.6f, 0f);
        }
    }
}
