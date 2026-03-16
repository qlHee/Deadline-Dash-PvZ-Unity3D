using UnityEngine;

// 这个脚本附加在每个障碍物上，用于检测与玩家的碰撞
public class ObstacleCollision : MonoBehaviour
{
    [Header("伤害设置")]
    public float damage = 25f;

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
        // 检测是否碰到玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log("障碍物碰到玩家！造成伤害");
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ApplyObstacleDamage(damage);
            }
            // 触发闪烁并销毁
            if (!hasHit)
            {
                hasHit = true;
                if (cachedCollider != null) cachedCollider.enabled = false;
                StartCoroutine(FlashAndDisappear());
            }
        }
    }

    System.Collections.IEnumerator FlashAndDisappear()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        bool visible = true;

        // 简单闪烁：每0.1s切换显隐
        const float interval = 0.1f;
        float nextToggle = interval;

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

        // 最终销毁
        Destroy(gameObject);
    }

    void SetRenderersEnabled(bool enabled)
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].enabled = enabled;
        }
    }
}

