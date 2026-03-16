using UnityEngine;

// 这个脚本附加在每个障碍物上，用于检测与玩家的碰撞
public class ObstacleCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 检测是否碰到玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log("障碍物碰到玩家！触发游戏结束");
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TriggerGameOver();
            }
        }
    }
}

