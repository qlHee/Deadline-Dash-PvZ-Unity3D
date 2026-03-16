using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float forwardSpeed = 10f;
    public float maxForwardSpeed = 20f;
    public float minForwardSpeed = 5f;
    public float speedChangeRate = 2f;
    public float horizontalSpeed = 8f;
    public float leftBoundary = -4f;
    public float rightBoundary = 4f;

    [Header("跳跃设置")]
    public float jumpForce = 8f;
    public float gravity = 20f;
    public float groundedGravity = 0.5f;
    public float jumpBufferTime = 0.1f;

    [Header("Character Physics")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public float groundCheckExtraDistance = 0.05f;
    public LayerMask groundLayerMask = ~0;

    [Header("生命设置")]
    public float maxHealth = 100f;
    public float regenDelay = 7f;
    public float regenRate = 15f;

    [Header("游戏状态")]
    private bool isGameOver = false;
    private float targetSpeed;
    private float totalDistance = 0f;
    private Vector3 lastPosition;
    private float currentHealth;
    private float lastDamageTime = Mathf.NegativeInfinity;
    private float lastObstacleDamageTime = Mathf.NegativeInfinity;
    private const float obstacleDamageCooldown = 0.2f;
    private float targetLaneX;

    private Rigidbody rb;
    private Collider bodyCollider;
    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    private float jumpRequestTime = Mathf.NegativeInfinity;

    private const float defaultObstacleDamage = 25f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody component.");
            enabled = false;
            return;
        }

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints |= RigidbodyConstraints.FreezeRotation;

        bodyCollider = GetComponent<Collider>();
        if (bodyCollider == null)
        {
            bodyCollider = GetComponentInChildren<Collider>();
        }

        targetLaneX = transform.position.x;
    }

    void Start()
    {
        targetSpeed = forwardSpeed;
        lastPosition = transform.position;
        currentHealth = maxHealth;
        targetLaneX = transform.position.x;
    }

    void Update()
    {
        if (isGameOver) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequestTime = Time.time;
        }

        HandleHealthRegen();
        UpdateDistanceTravelled();
    }

    void FixedUpdate()
    {
        if (isGameOver) return;

        HandleMovement();
    }

    void UpdateDistanceTravelled()
    {
        float distanceMoved = transform.position.z - lastPosition.z;
        if (distanceMoved > 0f)
        {
            totalDistance += distanceMoved;
        }
        lastPosition = transform.position;
    }

    void HandleMovement()
    {
        float deltaTime = Time.fixedDeltaTime;

        ClampHorizontalPosition();

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

        float sourceSpeed = Mathf.Abs(rb.velocity.z) > 0.01f ? rb.velocity.z : forwardSpeed;
        float currentSpeed = Mathf.Lerp(sourceSpeed, targetSpeed, deltaTime * speedChangeRate);

        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        targetLaneX += horizontalInput * horizontalSpeed * deltaTime;
        targetLaneX = Mathf.Clamp(targetLaneX, leftBoundary, rightBoundary);

        float desiredHorizontalVelocity = (targetLaneX - rb.position.x) / Mathf.Max(deltaTime, 0.0001f);
        if (Mathf.Abs(desiredHorizontalVelocity) < 0.01f)
        {
            desiredHorizontalVelocity = 0f;
        }

        bool groundedNow = CheckGrounded();
        bool jumpBuffered = Time.time - jumpRequestTime <= jumpBufferTime;

        if (groundedNow)
        {
            verticalVelocity = Mathf.Max(verticalVelocity, -groundedGravity);

            if (jumpBuffered)
            {
                verticalVelocity = jumpForce;
                groundedNow = false;
                jumpRequestTime = Mathf.NegativeInfinity;
            }
        }
        else
        {
            verticalVelocity -= gravity * deltaTime;
        }

        Vector3 desiredVelocity = new Vector3(
            desiredHorizontalVelocity,
            verticalVelocity,
            currentSpeed
        );

        rb.velocity = desiredVelocity;
        isGrounded = groundedNow;
    }

    void ClampHorizontalPosition()
    {
        float clampedX = Mathf.Clamp(rb.position.x, leftBoundary, rightBoundary);
        if (!Mathf.Approximately(clampedX, rb.position.x))
        {
            rb.position = new Vector3(clampedX, rb.position.y, rb.position.z);
        }
        targetLaneX = Mathf.Clamp(targetLaneX, leftBoundary, rightBoundary);
    }

    bool CheckGrounded()
    {
        if (groundCheck != null)
        {
            return Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayerMask,
                QueryTriggerInteraction.Ignore
            );
        }

        if (bodyCollider != null)
        {
            Vector3 origin = bodyCollider.bounds.center;
            float distance = bodyCollider.bounds.extents.y + groundCheckExtraDistance;
            return Physics.Raycast(
                origin,
                Vector3.down,
                distance,
                groundLayerMask,
                QueryTriggerInteraction.Ignore
            );
        }

        Vector3 fallbackOrigin = transform.position + Vector3.up * groundCheckExtraDistance;
        return Physics.Raycast(
            fallbackOrigin,
            Vector3.down,
            groundCheckExtraDistance * 2f,
            groundLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            ApplyObstacleCollision(collision.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            ApplyObstacleCollision(other.gameObject);
        }
    }

    void ApplyObstacleCollision(GameObject obstacleObj)
    {
        if (Time.time - lastObstacleDamageTime < obstacleDamageCooldown)
        {
            return;
        }

        lastObstacleDamageTime = Time.time;
        float damage = GetObstacleDamage(obstacleObj);
        TakeDamage(damage);
    }

    float GetObstacleDamage(GameObject obstacleObj)
    {
        if (obstacleObj == null) return defaultObstacleDamage;
        ObstacleCollision obstacleCollision = obstacleObj.GetComponent<ObstacleCollision>();
        if (obstacleCollision != null)
        {
            return obstacleCollision.damage;
        }
        return defaultObstacleDamage;
    }

    void HandleHealthRegen()
    {
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
            return;
        }

        if (Time.time - lastDamageTime < regenDelay)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + regenRate * Time.deltaTime);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isGameOver || damageAmount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damageAmount);
        lastDamageTime = Time.time;

        if (currentHealth <= 0f)
        {
            GameOver();
        }
    }

    public void ApplyObstacleDamage(float damage)
    {
        if (Time.time - lastObstacleDamageTime < obstacleDamageCooldown)
        {
            return;
        }
        lastObstacleDamageTime = Time.time;
        TakeDamage(damage);
    }

    void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Debug.Log("游戏结束：调用 GameOver()");
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnGameOver();
        }
    }

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
        return rb != null ? rb.velocity.z : 0f;
    }

    public float GetTotalDistance()
    {
        return totalDistance;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }
}
