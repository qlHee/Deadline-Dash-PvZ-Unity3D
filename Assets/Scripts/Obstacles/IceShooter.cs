using UnityEngine;

public class IceShooter : MonoBehaviour
{
    [Header("射击设置")]
    public float fireInterval = 2f;
    public float projectileSpeed = 18f;
    public float projectileDamage = 27f;
    public float projectileLifetime = 5f;
    public Color projectileColor = new Color32(100, 180, 255, 255);

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

        GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectileObject.name = "IcePeaProjectile";
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

        IcePeaProjectile projectile = projectileObject.AddComponent<IcePeaProjectile>();
        projectile.Initialize(projectileDamage, projectileSpeed, direction, projectileLifetime);
    }

    Vector3 GetFireDirection()
    {
        return Vector3.back;
    }
}
