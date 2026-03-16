using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PeaProjectile : MonoBehaviour
{
    private float damage = 27f;
    private float speed = 15f;
    private float lifetime = 5f;
    private Vector3 direction = Vector3.back;
    private float spawnTime;

    private bool initialized;
    private float roadHalfWidth = 5f;
    private bool isBurning = false;
    private Color burningColor = new Color32(255, 69, 0, 255);

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

            player.TakeDamage(damage);
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

    public void ConvertToBurningPea()
    {
        if (isBurning) return;
        
        isBurning = true;
        damage *= 2f;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = burningColor;
        }
    }

    public void ConvertToIcePea()
    {
        if (isBurning)
        {
            isBurning = false;
            damage /= 2f;
        }

        Destroy(gameObject);
        
        GameObject icePeaObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        icePeaObject.name = "IcePeaProjectile";
        icePeaObject.transform.position = transform.position;
        icePeaObject.transform.localScale = transform.localScale;

        Rigidbody rb = icePeaObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.velocity = GetComponent<Rigidbody>().velocity;

        SphereCollider collider = icePeaObject.GetComponent<SphereCollider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        icePeaObject.tag = gameObject.tag;

        Renderer renderer = icePeaObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color32(100, 180, 255, 255);
        }

        IcePeaProjectile icePea = icePeaObject.AddComponent<IcePeaProjectile>();
        icePea.Initialize(damage, speed, direction, lifetime - (Time.time - spawnTime));
    }
}
