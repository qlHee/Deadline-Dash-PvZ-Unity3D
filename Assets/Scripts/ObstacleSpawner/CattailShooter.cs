using UnityEngine;

public class CattailShooter : MonoBehaviour
{
    [Header("外观设置")]
    public GameObject model;
    public float scale = 1.5f;

    [Header("伤害设置")]
    public float damage = 47f;

    [Header("射击设置")]
    public float fireInterval = 3f;
    public float projectileSpeed = 18f;
    public float projectileDamage = 27f;
    public float projectileLifetime = 5f;
    public Color projectileColor = new Color32(255, 223, 128, 255);
    public float targetLeadDistance = 10f;

    private Transform muzzle;
    private PlayerController player;
    private float nextFireTime;
    private float projectileHeight = 1.2f;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        nextFireTime = Time.time + Random.Range(0f, Mathf.Max(0.1f, fireInterval));
    }

    void Update()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + Mathf.Max(0.1f, fireInterval);
        Fire();
    }

    public void AssignMuzzle(Transform muzzleTransform)
    {
        muzzle = muzzleTransform;
    }

    public void SetProjectileHeight(float height)
    {
        projectileHeight = Mathf.Max(0f, height);
    }

    public void SetProjectileDamage(float damage)
    {
        projectileDamage = Mathf.Max(0f, damage);
    }

    void Fire()
    {
        if (player != null && player.IsGameOver())
        {
            return;
        }

        Transform spawnOrigin = muzzle != null ? muzzle : transform;
        Vector3 direction = GetFireDirection();

        Vector3 spawnPosition = spawnOrigin.position;
        spawnPosition.y = projectileHeight;

        GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        projectileObject.name = "SpikeProjectile";
        projectileObject.transform.position = spawnPosition;
        projectileObject.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        
        projectileObject.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);

        Rigidbody rb = projectileObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Collider collider = projectileObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        TagUtility.TryAssignTag(projectileObject, "Obstacle");

        Renderer renderer = projectileObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = projectileColor;
        }

        SpikeProjectile projectile = projectileObject.AddComponent<SpikeProjectile>();
        projectile.Initialize(projectileDamage, projectileSpeed, direction, projectileLifetime);
    }

    Vector3 GetFireDirection()
    {
        if (player == null)
        {
            return Vector3.back;
        }

        Vector3 targetPosition = player.transform.position + player.transform.forward * targetLeadDistance;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        
        if (direction.sqrMagnitude < 0.0001f)
        {
            return Vector3.back;
        }
        
        return direction.normalized;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null && !playerController.IsGameOver())
            {
                playerController.TakeDamage(damage);
            }
        }
    }
}
