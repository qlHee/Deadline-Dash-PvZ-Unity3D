using UnityEngine;

public class TallNut : MonoBehaviour
{
    [Header("高坚果设置")]
    public float damage = 37f;

    void OnTriggerEnter(Collider other)
    {
        // 对玩家造成伤害
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsGameOver())
            {
                player.TakeDamage(damage);
            }
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
}
