using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float forwardSpeed = 10f;          // 向前移动速度
    public float maxForwardSpeed = 20f;       // 最大向前速度
    public float minForwardSpeed = 5f;        // 最小向前速度
    public float speedChangeRate = 2f;        // 加速/减速的速率
    public float horizontalSpeed = 8f;        // 左右移动速度
    public float leftBoundary = -4f;          // 左边界
    public float rightBoundary = 4f;          // 右边界

    [Header("跳跃设置")]
    public float jumpForce = 8f;              // 跳跃力度
    public float gravity = 20f;               // 重力
    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    private CharacterController characterController;

    [Header("游戏状态")]
    private bool isGameOver = false;
    private float targetSpeed;
    private float totalDistance = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        targetSpeed = forwardSpeed;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (isGameOver) return;

        HandleMovement();
        
        // 计算移动距离（只计算向前的距离）
        float distanceMoved = transform.position.z - lastPosition.z;
        if (distanceMoved > 0)
        {
            totalDistance += distanceMoved;
        }
        lastPosition = transform.position;
    }

    void HandleMovement()
    {
        // 处理加速和减速
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            targetSpeed = maxForwardSpeed;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            targetSpeed = minForwardSpeed;
        }
        else
        {
            targetSpeed = forwardSpeed;
        }

        // 平滑过渡速度
        float currentSpeed = Mathf.Lerp(
            characterController.velocity.z != 0 ? characterController.velocity.z : forwardSpeed,
            targetSpeed,
            Time.deltaTime * speedChangeRate
        );

        // 处理左右移动
        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        float horizontalMovement = horizontalInput * horizontalSpeed * Time.deltaTime;
        
        // 限制左右移动范围
        float newX = Mathf.Clamp(transform.position.x + horizontalMovement, leftBoundary, rightBoundary);
        horizontalMovement = newX - transform.position.x;

        // 处理跳跃和重力
        if (isGrounded)
        {
            verticalVelocity = -0.5f; // 保持在地面上的小力
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = jumpForce;
                isGrounded = false;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // 组合移动向量
        Vector3 moveVector = new Vector3(
            horizontalMovement / Time.deltaTime,
            verticalVelocity,
            currentSpeed
        );

        // 应用移动
        characterController.Move(moveVector * Time.deltaTime);

        // 检测是否着地
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            isGrounded = true;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 检测碰撞障碍物
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("OnControllerColliderHit: 碰到障碍物");
            GameOver();
        }

        // 检测地面
        if (hit.normal.y > 0.7f)
        {
            isGrounded = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 使用Trigger作为额外的碰撞检测方式
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("OnTriggerEnter: 碰到障碍物");
            GameOver();
        }
    }

    void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Debug.Log("游戏结束！调用GameOver()");
        
        // 通知游戏管理器
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnGameOver();
        }
    }

    // 公开方法供外部调用
    public void TriggerGameOver()
    {
        GameOver();
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public float GetForwardSpeed()
    {
        return characterController.velocity.z;
    }

    public float GetTotalDistance()
    {
        return totalDistance;
    }
}

