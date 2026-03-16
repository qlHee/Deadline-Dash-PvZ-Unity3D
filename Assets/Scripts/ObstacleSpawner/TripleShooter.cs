using UnityEngine;

public class TripleShooter : MonoBehaviour
{
    [Header("射击设置")]
    public float fireInterval = 3f;
    public float projectileSpeed = 18f;
    public float projectileDamage = 27f;
    public float projectileLifetime = 5f;
    public Color projectileColor = new Color32(168, 217, 30, 255);
    public float spreadAngle = 30f;

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
        if (player != null && player.IsGameOver())
        {
            return;
        }

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
        Transform spawnOrigin = muzzle != null ? muzzle : transform;
        
        FireProjectile(spawnOrigin, Vector3.back);
        FireProjectile(spawnOrigin, Quaternion.Euler(0f, -spreadAngle, 0f) * Vector3.back);
        FireProjectile(spawnOrigin, Quaternion.Euler(0f, spreadAngle, 0f) * Vector3.back);
    }

    void FireProjectile(Transform spawnOrigin, Vector3 direction)
    {
        Vector3 spawnPosition = spawnOrigin.position;
        spawnPosition.y = projectileHeight;

        GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectileObject.name = "TriplePeaProjectile";
        projectileObject.transform.position = spawnPosition;
        projectileObject.transform.localScale = Vector3.one * 0.7f;

        Rigidbody rb = projectileObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        SphereCollider collider = projectileObject.GetComponent<SphereCollider>();
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

        PeaProjectile projectile = projectileObject.AddComponent<PeaProjectile>();
        projectile.Initialize(projectileDamage, projectileSpeed, direction, projectileLifetime);
    }

    Vector3 GetFireDirection()
    {
        return Vector3.back;
    }
}
