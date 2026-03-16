using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform target;                  // 跟随目标（玩家）
    public Vector3 offset = new Vector3(0, 5, -10);  // 相机相对玩家的偏移
    public float smoothSpeed = 5f;            // 平滑跟随速度
    public bool lookAtTarget = true;          // 是否始终看向目标

    [Header("固定视角设置")]
    public bool useFixedAngle = true;         // 使用固定角度
    public Vector3 fixedRotation = new Vector3(20, 0, 0);  // 固定旋转角度

    void Start()
    {
        // 如果没有设置目标，自动查找玩家
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        // 设置初始位置
        if (target != null)
        {
            transform.position = target.position + offset;
            
            if (useFixedAngle)
            {
                transform.rotation = Quaternion.Euler(fixedRotation);
            }
            else if (lookAtTarget)
            {
                transform.LookAt(target);
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;
        
        // 平滑移动到目标位置
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 设置相机朝向
        if (useFixedAngle)
        {
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
        else if (lookAtTarget)
        {
            transform.LookAt(target);
        }
    }
}

