using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("角色信息")]
    public string characterName = "运动僵尸";
    public string characterID = "athletic_zombie";
    
    [Header("移动属性")]
    public float minForwardSpeed = 15f;
    public float forwardSpeed = 26f;
    public float maxForwardSpeed = 43f;
    public float speedChangeRate = 2f;
    public float horizontalSpeed = 8f;
    
    [Header("跳跃属性")]
    public float jumpForce = 9f;
    public float gravity = 20f;
    
    [Header("生命属性")]
    public float maxHealth = 200f;
    public float regenDelay = 3f;
    public float regenRate = 15f;
    
    public void ApplyToPlayerController(PlayerController player)
    {
        if (player == null)
        {
            Debug.LogError("PlayerController is null!");
            return;
        }
        
        player.minForwardSpeed = minForwardSpeed;
        player.forwardSpeed = forwardSpeed;
        player.maxForwardSpeed = maxForwardSpeed;
        player.speedChangeRate = speedChangeRate;
        player.horizontalSpeed = horizontalSpeed;
        player.jumpForce = jumpForce;
        player.gravity = gravity;
        player.maxHealth = maxHealth;
        player.regenDelay = regenDelay;
        player.regenRate = regenRate;
        
        Debug.Log($"[角色属性] 已应用角色 '{characterName}' 的属性到玩家");
    }
}
