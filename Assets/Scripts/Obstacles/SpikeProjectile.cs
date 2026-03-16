using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpikeProjectile : MonoBehaviour
{
    [Header("伤害设置")]
    public float damage = 27f;

    private float speed = 15f;
    private float lifetime = 5f;
    private Vector3 direction = Vector3.back;
    private float spawnTime;

    private bool initialized;
    private float roadHalfWidth = 5f;

    void Awake()
    {
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    public void Initialize(float damageAmount, float travelSpeed, Vector3 travelDirection, float lifeDuration)
    {
        damage = Mathf.Max(0f, damageAmount);
        speed = Mathf.Max(0.1f, travelSpeed);
        direction = travelDirection.sqrMagnitude < 0.0001f ? Vector3.back : travelDirection.normalized;
        lifetime = Mathf.Max(0.1f, lifeDuration);
        spawnTime = Time.time;
        initialized = true;
    }

    void Update()
    {
        if (!initialized)
        {
            Initialize(damage, speed, direction, lifetime);
        }

        transform.position += direction * speed * Time.deltaTime;

        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (Mathf.Abs(transform.position.x) > roadHalfWidth)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || player.IsGameOver())
            {
                return;
            }

            player.TakeDamage(damage, DamageType.Normal);
            Destroy(gameObject);
        }
        else if (other.GetComponent<Nut>() != null || other.GetComponent<TallNut>() != null)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
