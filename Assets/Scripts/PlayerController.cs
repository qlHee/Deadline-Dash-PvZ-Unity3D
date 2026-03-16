using UnityEngine;

public enum DamageType
{
    Normal,
    Fire,
    Ice
}

public enum ShieldType
{
    None,
    Normal,
    Fire,
    Ice
}


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float forwardSpeed = 10f;
    public float maxForwardSpeed = 20f;
    public float minForwardSpeed = 5f;
    public float speedChangeRate = 2f;
    public float horizontalSpeed = 8f;
    [Tooltip("左边界（根据道路宽度自动计算）")]
    [SerializeField] private float leftBoundary = -5f;
    [Tooltip("右边界（根据道路宽度自动计算）")]
    [SerializeField] private float rightBoundary = 5f;

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
    private bool isInvincible = false;
    private float invincibleEndTime = 0f;

    [Header("减速效果")]
    private bool isSlowed = false;
    private float slowEndTime = 0f;
    private float speedMultiplier = 1f;
    
    [Header("冻结效果")]
    private bool isFrozen = false;
    private float frozenEndTime = 0f;
    private Vector3 frozenPosition; // 冻结时玩家的位置

    [Header("燃烧效果")]
    private bool isBurning = false;
    private float burningEndTime = 0f;
    private float burningDamagePerSecond = 5f;
    private float lastBurningDamageTime = 0f;
    public GameObject burningEffectPrefab;
    public float burningEffectLifetime = 3f;
    public Color burningTintColor = new Color(1f, 0.3f, 0.1f, 1f);
    public Vector3 burningEffectOffset = new Vector3(0f, 0.2f, 0f);
    private GameObject burningEffectInstance;

    [Header("护盾设置")]
    public float defaultShieldDuration = 8f;
    public Transform shieldEffectAnchor;
    public Vector3 shieldEffectOffset = Vector3.zero;
    public Vector3 shieldEffectRotationOffset = Vector3.zero;
    public GameObject normalShieldEffectPrefab;
    public GameObject fireShieldEffectPrefab;
    public GameObject iceShieldEffectPrefab;
    public GameObject shieldBreakEffectPrefab;
    public Vector3 shieldEffectScale = Vector3.one;

    [Header("回血特效")]
    public GameObject healEffectPrefab;
    public float healEffectLifetime = 2f;
    public Vector3 healEffectOffset = new Vector3(0f, 0.6f, 0f);
    
    [Header("音效设置")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    private AudioSource audioSource;

    [Header("跳跃增益")]
    public GameObject jumpBoostEffectPrefab;
    public Vector3 jumpBoostEffectOffset = new Vector3(0f, 0.2f, 0f);
    public float fallbackJumpBoostDuration = 8f;
    public float defaultJumpBoostMultiplier = 5f;

    [Header("动画")]
    [SerializeField] private Animator animator;
    [SerializeField] private float walkSpeedThreshold = 7f;
    [SerializeField] private float locomotionDampTime = 0.1f;
    [Header("动作特效")]
    public GameObject slideEffectPrefab;
    public float slideEffectLifetime = 2f;
    public Vector3 slideEffectOffset = new Vector3(0f, 0.5f, 0.8f);
    public Vector3 slideEffectRotationOffset = Vector3.zero;
    public float slideEffectDelay = 0f;
    public GameObject grabEffectPrefab;
    public float grabEffectLifetime = 2f;
    public Vector3 grabEffectOffset = new Vector3(0f, 0.6f, 0.6f);
    public Vector3 grabEffectRotationOffset = Vector3.zero;
    public float grabEffectDelay = 0f;
    [Header("冻结外观")]
    public Color frozenTintColor = new Color(0.3f, 0.65f, 1f, 1f);

    private static readonly int AnimMoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int AnimGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int AnimJumpTriggerHash = Animator.StringToHash("Jump");
    private static readonly int AnimSlideTriggerHash = Animator.StringToHash("Slide");
    private static readonly int AnimGrabTriggerHash = Animator.StringToHash("Grab");
    private const float WalkBlendValue = 0.2f;

    private Rigidbody rb;
    private RigidbodyConstraints originalConstraints;
    private bool constraintsStored = false;
    private float originalAnimatorSpeed = 1f;
    private bool animatorSpeedStored = false;
    private Renderer[] rendererCache = null;
    private string[] rendererColorProperty = null;
    private Color[] rendererOriginalColor = null;
    private bool rendererCacheBuilt = false;
    private bool slowTintActive = false;
    private bool frozenTintActive = false;
    private bool burningTintActive = false;
    private Collider bodyCollider;
    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    private float jumpRequestTime = Mathf.NegativeInfinity;
    private ShieldType activeShield = ShieldType.None;
    private float shieldExpireTime = 0f;
    private GameObject shieldEffectInstance;
    private ShieldType shieldEffectType = ShieldType.None;
    private float jumpForceMultiplier = 1f;
    private bool isJumpBoostActive = false;
    private float jumpBoostEndTime = 0f;
    private GameObject jumpBoostEffectInstance;

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

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        targetLaneX = transform.position.x;
    }

    void Start()
    {
        targetSpeed = forwardSpeed;
        lastPosition = transform.position;
        currentHealth = maxHealth;
        targetLaneX = transform.position.x;
        jumpForceMultiplier = 1f;
        
        // 初始化音效系统
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
    }

    void Update()
    {
        if (isGameOver) return;

        if (Input.GetKeyDown(KeyCode.Space) && !isFrozen)
        {
            jumpRequestTime = Time.time;
        }

        // 滑铲（仅地面）
        if (Input.GetKeyDown(KeyCode.C) && isGrounded && !isFrozen && animator != null)
        {
            animator.ResetTrigger(AnimGrabTriggerHash);
            animator.SetTrigger(AnimSlideTriggerHash);
            if (slideEffectDelay > 0f)
            {
                StartCoroutine(SpawnActionEffectDelayed(slideEffectDelay, slideEffectPrefab, slideEffectLifetime, slideEffectOffset, slideEffectRotationOffset));
            }
            else
            {
                SpawnActionEffect(slideEffectPrefab, slideEffectLifetime, slideEffectOffset, slideEffectRotationOffset);
            }
        }

        // 抓取（空中也可） - 使用Shift键（左右都可）
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && !isFrozen && animator != null)
        {
            animator.ResetTrigger(AnimSlideTriggerHash);
            animator.SetTrigger(AnimGrabTriggerHash);
            if (grabEffectDelay > 0f)
            {
                StartCoroutine(SpawnActionEffectDelayed(grabEffectDelay, grabEffectPrefab, grabEffectLifetime, grabEffectOffset, grabEffectRotationOffset));
            }
            else
            {
                SpawnActionEffect(grabEffectPrefab, grabEffectLifetime, grabEffectOffset, grabEffectRotationOffset);
            }
        }

        UpdateInvincibleState();
        UpdateSlowEffect();
        UpdateFrozenEffect();
        UpdateBurningEffect();
        UpdateShieldState();
        UpdateJumpBoostState();
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
        
        // 如果玩家被冻结，则完全停止移动
        if (isFrozen)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = frozenPosition;
            return;
        }

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

        targetSpeed *= speedMultiplier;

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
                verticalVelocity = jumpForce * Mathf.Max(1f, jumpForceMultiplier);
                groundedNow = false;
                jumpRequestTime = Mathf.NegativeInfinity;
                TriggerJumpAnimation();
                PlayJumpSound();
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
        
        // 检测落地，播放落地音效
        if (!isGrounded && groundedNow)
        {
            PlayLandSound();
        }
        
        isGrounded = groundedNow;
        UpdateAnimator(currentSpeed);
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
            FireStump fireStump = other.GetComponent<FireStump>();
            if (fireStump != null)
            {
                if (!IsGameOver())
                {
                    if (TakeDamage(fireStump.damage, DamageType.Fire))
                    {
                        ApplyBurningEffect(fireStump.burningDuration, fireStump.burningDamagePerSecond);
                    }
                }
                return;
            }
            
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
        TakeDamage(damage, DamageType.Normal);
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

    public bool RestoreHealth(float healAmount)
    {
        if (isGameOver || healAmount <= 0f)
        {
            return false;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        SpawnHealEffect();
        if (currentHealth > previousHealth)
        {
            return true;
        }
        return false;
    }

    public bool TakeDamage(float damageAmount, DamageType damageType = DamageType.Normal)
    {
        if (isGameOver || damageAmount <= 0f)
        {
            return false;
        }

        if (isInvincible)
        {
            return false;
        }

        if (IsShieldBlockingDamage(damageType))
        {
            ConsumeShield(true);
            return false;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damageAmount);
        lastDamageTime = Time.time;

        if (currentHealth <= 0f)
        {
            GameOver();
        }
        return true;
    }

    public bool ApplyObstacleDamage(float damage, DamageType damageType = DamageType.Normal)
    {
        if (Time.time - lastObstacleDamageTime < obstacleDamageCooldown)
        {
            return false;
        }
        lastObstacleDamageTime = Time.time;
        return TakeDamage(damage, damageType);
    }

    void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        ConsumeShield(false);
        DestroyJumpBoostEffect();
        jumpForceMultiplier = 1f;
        isJumpBoostActive = false;
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

    bool IsShieldBlockingDamage(DamageType damageType)
    {
        if (activeShield == ShieldType.None)
        {
            return false;
        }

        if (Time.time >= shieldExpireTime)
        {
            ConsumeShield(false);
            return false;
        }

        switch (activeShield)
        {
            case ShieldType.Normal:
                return damageType == DamageType.Normal;
            case ShieldType.Fire:
                return damageType == DamageType.Fire || damageType == DamageType.Normal;
            case ShieldType.Ice:
                return damageType == DamageType.Ice || damageType == DamageType.Normal;
            default:
                return false;
        }
    }

    void ConsumeShield(bool spawnBreakFx)
    {
        DestroyShieldEffect(spawnBreakFx);
        activeShield = ShieldType.None;
        shieldExpireTime = 0f;
    }

    void DestroyShieldEffect(bool spawnBreakFx)
    {
        if (shieldEffectInstance != null)
        {
            Vector3 pos = shieldEffectInstance.transform.position;
            Quaternion rot = shieldEffectInstance.transform.rotation;
            Destroy(shieldEffectInstance);
            shieldEffectInstance = null;
            if (spawnBreakFx && shieldBreakEffectPrefab != null)
            {
                GameObject fx = Instantiate(shieldBreakEffectPrefab, pos, rot);
                Destroy(fx, 3f);
            }
        }
        shieldEffectType = ShieldType.None;
    }

    void RefreshShieldEffect()
    {
        DestroyShieldEffect(false);
        if (activeShield == ShieldType.None)
        {
            return;
        }

        GameObject prefab = GetShieldEffectPrefab(activeShield);
        if (prefab == null)
        {
            return;
        }

        Transform anchor = shieldEffectAnchor != null ? shieldEffectAnchor : transform;
        Vector3 worldPos = anchor.position + anchor.TransformVector(shieldEffectOffset);
        Quaternion worldRot = anchor.rotation * Quaternion.Euler(shieldEffectRotationOffset);
        shieldEffectInstance = Instantiate(prefab, worldPos, worldRot, anchor);
        if (shieldEffectInstance != null)
        {
            Vector3 originalScale = shieldEffectInstance.transform.localScale;
            Vector3 scaleMultiplier = new Vector3(
                shieldEffectScale.x != 0f ? shieldEffectScale.x : 1f,
                shieldEffectScale.y != 0f ? shieldEffectScale.y : 1f,
                shieldEffectScale.z != 0f ? shieldEffectScale.z : 1f
            );
            shieldEffectInstance.transform.localScale = Vector3.Scale(originalScale, scaleMultiplier);
        }
        shieldEffectType = activeShield;
    }

    GameObject GetShieldEffectPrefab(ShieldType type)
    {
        switch (type)
        {
            case ShieldType.Fire:
                return fireShieldEffectPrefab;
            case ShieldType.Ice:
                return iceShieldEffectPrefab;
            case ShieldType.Normal:
                return normalShieldEffectPrefab;
            default:
                return null;
        }
    }

    void UpdateShieldState()
    {
        if (activeShield == ShieldType.None)
        {
            DestroyShieldEffect(false);
            return;
        }

        if (Time.time >= shieldExpireTime)
        {
            ConsumeShield(false);
        }
        else if (shieldEffectInstance == null || shieldEffectType != activeShield)
        {
            RefreshShieldEffect();
        }
    }

    public void ActivateShield(ShieldType type, float duration)
    {
        if (type == ShieldType.None || isGameOver)
        {
            return;
        }

        float finalDuration = duration > 0f ? duration : defaultShieldDuration;
        activeShield = type;
        shieldExpireTime = Time.time + finalDuration;
        RefreshShieldEffect();
    }

    public void ApplyJumpBoost(float duration, float multiplier)
    {
        if (isGameOver)
        {
            return;
        }

        float finalDuration = duration > 0f ? duration : fallbackJumpBoostDuration;
        float finalMultiplier = multiplier > 0f ? multiplier : defaultJumpBoostMultiplier;
        jumpForceMultiplier = Mathf.Max(1f, finalMultiplier);
        jumpBoostEndTime = Time.time + Mathf.Max(0.1f, finalDuration);
        isJumpBoostActive = true;
        SpawnJumpBoostEffect();
    }

    void UpdateJumpBoostState()
    {
        if (!isJumpBoostActive)
        {
            return;
        }

        if (Time.time >= jumpBoostEndTime)
        {
            isJumpBoostActive = false;
            jumpForceMultiplier = 1f;
            DestroyJumpBoostEffect();
        }
    }

    void SpawnJumpBoostEffect()
    {
        if (jumpBoostEffectPrefab == null)
        {
            return;
        }

        DestroyJumpBoostEffect();
        Transform anchor = groundCheck != null ? groundCheck : transform;
        Vector3 worldPos = anchor.position + anchor.TransformVector(jumpBoostEffectOffset);
        jumpBoostEffectInstance = Instantiate(jumpBoostEffectPrefab, worldPos, transform.rotation, transform);
        jumpBoostEffectInstance.transform.SetParent(transform, true);
    }

    void DestroyJumpBoostEffect()
    {
        if (jumpBoostEffectInstance != null)
        {
            Destroy(jumpBoostEffectInstance);
            jumpBoostEffectInstance = null;
        }
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

    public void IncreaseMaxHealth(float amount)
    {
        if (amount <= 0f) return;
        maxHealth += amount;
        currentHealth += amount; // 同时增加当前血量
        Debug.Log($"[血量上限] 增加 {amount} 点，新上限: {maxHealth}");
    }

    public void ApplySlowEffect(float duration, float slowMultiplier)
    {
        // 如果玩家已被冻结，则不应用减速效果
        if (isFrozen) return;
        
        // 低温状态：清除所有高温状态
        ClearHotState();
        
        isSlowed = true;
        slowEndTime = Time.time + duration;
        speedMultiplier = Mathf.Clamp01(slowMultiplier);
        
        // 在减速期间也显示蓝色高亮
        if (!slowTintActive)
        {
            ApplyFrozenTint();
            slowTintActive = true;
        }
        Debug.Log($"[减速效果] 速度倍率: {speedMultiplier:F2}x, 持续时间: {duration}秒 (已清除高温状态)");
    }

    void UpdateSlowEffect()
    {
        if (isSlowed && Time.time >= slowEndTime)
        {
            isSlowed = false;
            speedMultiplier = 1f;
            
            // 仅在未冻结时移除减速期间的蓝色
            if (!isFrozen && slowTintActive)
            {
                RestoreFrozenTint();
                slowTintActive = false;
            }
            Debug.Log("[减速效果] 速度已恢复正常");
        }
    }

    public void SetInvincible(float duration)
    {
        isInvincible = true;
        invincibleEndTime = Time.time + duration;
    }

    void UpdateInvincibleState()
    {
        if (isInvincible && Time.time >= invincibleEndTime)
        {
            isInvincible = false;
        }
    }
    
    public void ApplyFrozenEffect(float duration)
    {
        // 低温状态：清除所有高温状态
        ClearHotState();
        
        // 完全冻结玩家
        isFrozen = true;
        frozenEndTime = Time.time + duration;
        frozenPosition = transform.position; // 记录当前位置

        if (rb != null)
        {
            if (!constraintsStored)
            {
                originalConstraints = rb.constraints;
                constraintsStored = true;
            }
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        frozenTintActive = true;
        ApplyFrozenTint();
        if (animator != null)
        {
            if (!animatorSpeedStored)
            {
                originalAnimatorSpeed = animator.speed;
                animatorSpeedStored = true;
            }
            animator.speed = 0f;
        }
        
        // 停止所有运动
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log($"[冻结效果] 玩家被完全冻结，持续时间: {duration}秒 (已清除高温状态)");
    }
    
    void UpdateFrozenEffect()
    {
        if (isFrozen && Time.time >= frozenEndTime)
        {
            isFrozen = false;
            
            if (rb != null && constraintsStored)
            {
                rb.constraints = originalConstraints;
            }
            RestoreFrozenTint();
            frozenTintActive = false;
            // 若减速效果仍在，重新上色
            if (isSlowed && !slowTintActive)
            {
                ApplyFrozenTint();
                slowTintActive = true;
            }
            if (animator != null && animatorSpeedStored)
            {
                animator.speed = originalAnimatorSpeed;
            }
            Debug.Log("[冻结效果] 冻结状态结束");
        }
    }
    
    void ApplyFrozenTint()
    {
        if (!rendererCacheBuilt)
        {
            BuildRendererCache();
        }
        
        if (rendererCache == null) return;
        
        for (int i = 0; i < rendererCache.Length; i++)
        {
            Renderer r = rendererCache[i];
            string prop = rendererColorProperty[i];
            if (r == null || string.IsNullOrEmpty(prop)) continue;
            
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor(prop, frozenTintColor);
            r.SetPropertyBlock(mpb);
        }
    }
    
    void RestoreFrozenTint()
    {
        if (rendererCache == null) return;
        
        for (int i = 0; i < rendererCache.Length; i++)
        {
            Renderer r = rendererCache[i];
            string prop = rendererColorProperty[i];
            if (r == null || string.IsNullOrEmpty(prop)) continue;
            
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            Color original = (rendererOriginalColor != null && rendererOriginalColor.Length > i) ? rendererOriginalColor[i] : Color.white;
            mpb.SetColor(prop, original);
            r.SetPropertyBlock(mpb);
        }
    }
    
    void BuildRendererCache()
    {
        rendererCache = GetComponentsInChildren<Renderer>();
        if (rendererCache == null || rendererCache.Length == 0)
        {
            rendererCacheBuilt = true;
            return;
        }
        
        rendererColorProperty = new string[rendererCache.Length];
        rendererOriginalColor = new Color[rendererCache.Length];
        
        for (int i = 0; i < rendererCache.Length; i++)
        {
            Renderer r = rendererCache[i];
            if (r == null || r.sharedMaterial == null)
            {
                rendererColorProperty[i] = null;
                rendererOriginalColor[i] = Color.white;
                continue;
            }
            
            Material mat = r.sharedMaterial;
            if (mat.HasProperty("_BaseColor"))
            {
                rendererColorProperty[i] = "_BaseColor";
                rendererOriginalColor[i] = mat.GetColor("_BaseColor");
            }
            else if (mat.HasProperty("_Color"))
            {
                rendererColorProperty[i] = "_Color";
                rendererOriginalColor[i] = mat.GetColor("_Color");
            }
            else
            {
                rendererColorProperty[i] = null;
                rendererOriginalColor[i] = Color.white;
            }
        }
        
        rendererCacheBuilt = true;
    }

    public void ApplyBurningEffect(float duration, float damagePerSecond)
    {
        // 高温状态：清除所有低温状态
        ClearColdState();
        
        isBurning = true;
        burningEndTime = Time.time + duration;
        burningDamagePerSecond = damagePerSecond;
        lastBurningDamageTime = Time.time;
        // 叠加特效与染色
        if (burningEffectPrefab != null && burningEffectInstance == null)
        {
            burningEffectInstance = Instantiate(burningEffectPrefab, transform);
            burningEffectInstance.transform.localPosition = burningEffectOffset;
            float effectLife = burningEffectLifetime > 0f ? burningEffectLifetime : duration;
            if (effectLife > 0f) Destroy(burningEffectInstance, effectLife);
        }
        ApplyBurningTint();
        burningTintActive = true;
        Debug.Log($"[燃烧效果] 持续时间: {duration}秒, 每秒伤害: {damagePerSecond} (已清除低温状态)");
    }

    void UpdateBurningEffect()
    {
        if (!isBurning)
        {
            return;
        }

        if (Time.time >= burningEndTime)
        {
            isBurning = false;
            ClearBurningEffect();
            Debug.Log("[燃烧效果] 燃烧结束");
            return;
        }

        if (Time.time - lastBurningDamageTime >= 1f)
        {
            lastBurningDamageTime = Time.time;
            TakeDamage(burningDamagePerSecond, DamageType.Fire);
        }
    }

    void ApplyBurningTint()
    {
        if (!rendererCacheBuilt)
        {
            BuildRendererCache();
        }

        if (rendererCache == null) return;

        for (int i = 0; i < rendererCache.Length; i++)
        {
            Renderer r = rendererCache[i];
            string prop = rendererColorProperty[i];
            if (r == null || string.IsNullOrEmpty(prop)) continue;

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor(prop, burningTintColor);
            r.SetPropertyBlock(mpb);
        }
    }

    void ClearBurningEffect()
    {
        if (burningEffectInstance != null)
        {
            Destroy(burningEffectInstance);
            burningEffectInstance = null;
        }
        if (burningTintActive)
        {
            // 如果冻结还在，则恢复冻结色，否则还原原色
            if (frozenTintActive)
            {
                ApplyFrozenTint();
            }
            else
            {
                RestoreFrozenTint();
            }
            burningTintActive = false;
        }
    }
    
    /// <summary>
    /// 清除所有高温状态（燃烧效果）
    /// </summary>
    void ClearHotState()
    {
        if (isBurning)
        {
            isBurning = false;
            ClearBurningEffect();
            Debug.Log("[状态互斥] 清除高温状态：燃烧效果已解除");
        }
    }
    
    /// <summary>
    /// 清除所有低温状态（冻结和减速效果）
    /// </summary>
    void ClearColdState()
    {
        bool hadColdState = false;
        
        // 清除冻结效果
        if (isFrozen)
        {
            isFrozen = false;
            
            if (rb != null && constraintsStored)
            {
                rb.constraints = originalConstraints;
            }
            
            if (animator != null && animatorSpeedStored)
            {
                animator.speed = originalAnimatorSpeed;
            }
            
            if (frozenTintActive)
            {
                RestoreFrozenTint();
                frozenTintActive = false;
            }
            
            hadColdState = true;
        }
        
        // 清除减速效果
        if (isSlowed)
        {
            isSlowed = false;
            speedMultiplier = 1f;
            
            if (slowTintActive)
            {
                RestoreFrozenTint();
                slowTintActive = false;
            }
            
            hadColdState = true;
        }
        
        if (hadColdState)
        {
            Debug.Log("[状态互斥] 清除低温状态：冻结和减速效果已解除");
        }
    }

    void UpdateAnimator(float currentForwardSpeed)
    {
        if (animator == null) return;

        float speedMagnitude = Mathf.Max(0f, currentForwardSpeed);
        if (speedMagnitude < 0.01f)
        {
            animator.SetFloat(AnimMoveSpeedHash, 0f, locomotionDampTime, Time.fixedDeltaTime);
        }
        else
        {
            float upperBound = Mathf.Max(forwardSpeed, walkSpeedThreshold + 0.01f);
            float normalizedSpeed = Mathf.InverseLerp(walkSpeedThreshold, upperBound, speedMagnitude);
            float blendedSpeed = Mathf.Lerp(WalkBlendValue, 1f, normalizedSpeed);
            animator.SetFloat(AnimMoveSpeedHash, blendedSpeed, locomotionDampTime, Time.fixedDeltaTime);
        }
        animator.SetBool(AnimGroundedHash, isGrounded);
    }

    void TriggerJumpAnimation()
    {
        if (animator == null) return;
        animator.ResetTrigger(AnimJumpTriggerHash);
        animator.SetTrigger(AnimJumpTriggerHash);
    }

    void SpawnActionEffect(GameObject prefab, float lifetime, Vector3 offset, Vector3 eulerOffset)
    {
        if (prefab == null) return;
        Vector3 pos = transform.position + transform.TransformVector(offset);
        Quaternion rot = transform.rotation * Quaternion.Euler(eulerOffset);
        GameObject fx = Instantiate(prefab, pos, rot);
        fx.transform.SetParent(transform, true); // 跟随玩家，保持相对位置
        if (lifetime > 0f)
        {
            Destroy(fx, lifetime);
        }
    }

    void SpawnHealEffect()
    {
        if (healEffectPrefab == null) return;
        Vector3 pos = transform.position + transform.TransformVector(healEffectOffset);
        GameObject fx = Instantiate(healEffectPrefab, pos, transform.rotation);
        fx.transform.SetParent(transform, true);
        if (healEffectLifetime > 0f)
        {
            Destroy(fx, healEffectLifetime);
        }
    }

    System.Collections.IEnumerator SpawnActionEffectDelayed(float delay, GameObject prefab, float lifetime, Vector3 offset, Vector3 eulerOffset)
    {
        yield return new WaitForSeconds(delay);
        SpawnActionEffect(prefab, lifetime, offset, eulerOffset);
    }

    public void UpdateBoundaries(float roadWidth)
    {
        float halfWidth = roadWidth / 2f;
        float margin = 1f;
        leftBoundary = -halfWidth + margin;
        rightBoundary = halfWidth - margin;
        
        targetLaneX = Mathf.Clamp(targetLaneX, leftBoundary, rightBoundary);
        
        if (rb != null)
        {
            Vector3 pos = rb.position;
            pos.x = Mathf.Clamp(pos.x, leftBoundary, rightBoundary);
            rb.position = pos;
        }
    }
    
    void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound, soundVolume);
        }
    }
    
    void PlayLandSound()
    {
        if (audioSource != null && landSound != null)
        {
            audioSource.PlayOneShot(landSound, soundVolume);
        }
    }
}
