using System.Collections;
using UnityEngine;

public class TallNut : MonoBehaviour
{
    [Header("高坚果设置")]
    public float damage = 37f;
    
    private bool hasHit = false;
    private Collider cachedCollider;
    private Renderer[] renderers;
    
    void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    void OnTriggerEnter(Collider other)
    {
        // 对玩家造成伤害
        if (other.CompareTag("Player"))
        {
            if (hasHit) return;
            
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsGameOver())
            {
                player.TakeDamage(damage);
                hasHit = true;
                
                // 禁用碰撞体
                if (cachedCollider != null)
                {
                    cachedCollider.enabled = false;
                }
                
                // 闪烁后消失
                StartCoroutine(FlashAndDisappear());
            }
            return;
        }
        // 拦截并销毁所有类型的豌豆子弹
        else if (other.GetComponent<PeaProjectile>() != null)
        {
            Destroy(other.gameObject);
        }
        else if (other.GetComponent<IcePeaProjectile>() != null)
        {
            Destroy(other.gameObject);
        }
        else if (other.GetComponent<SpikeProjectile>() != null)
        {
            Destroy(other.gameObject);
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
}
