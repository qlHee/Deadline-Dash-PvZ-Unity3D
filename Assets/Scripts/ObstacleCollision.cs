using System.Collections;
using UnityEngine;

/// <summary>
/// 挂在障碍物上的伤害触发器，负责通知玩家扣血并淡出障碍。
/// </summary>
public class ObstacleCollision : MonoBehaviour
{
    [Header("伤害设置")]
    public float damage = 25f;

    private bool hasHit;
    private Collider cachedCollider;
    private Renderer[] renderers;

    void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasHit)
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ApplyObstacleDamage(damage);
        }

        hasHit = true;
        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }

        StartCoroutine(FlashAndDisappear());
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
