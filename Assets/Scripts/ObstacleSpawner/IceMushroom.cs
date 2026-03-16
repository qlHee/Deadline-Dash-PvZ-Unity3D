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
                
                // 应用伤害
                player.TakeDamage(damage);
                
                // 创建冻结效果
                // 确保初始化 FrozenEffect 并调用冻结方法
                FrozenEffect.Instance.freezeScreenColor = freezeScreenColor;
                FrozenEffect.Instance.ApplyFreezeEffect(player, freezeDuration);
            }
        }
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
