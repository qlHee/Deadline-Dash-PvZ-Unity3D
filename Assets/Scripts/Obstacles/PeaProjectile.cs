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
    [Header("燃烧豌豆对玩家的持续效果")]
    public float burningDurationForPlayer = 2f;
    public float burningDamagePerSecondForPlayer = 5f;
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

            if (isBurning && (player.WasFireShieldAbsorbedThisFrame() || player.TryConsumeFireShield()))
            {
                Destroy(gameObject);
                return;
            }

            bool damaged = player.TakeDamage(damage, isBurning ? DamageType.Fire : DamageType.Normal);
            if (damaged && isBurning)
            {
                player.ApplyBurningEffect(burningDurationForPlayer, burningDamagePerSecondForPlayer);
            }
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
            Material material = renderer.material;
            material.color = burningColor;
            
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", burningColor * 2f);
        }
        
        AddTrailEffect();
        AddParticleEffect();
    }
    
    private void AddTrailEffect()
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.3f;
        trail.endWidth = 0.05f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.27f, 0f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = gradient;
    }
    
    private void AddParticleEffect()
    {
        GameObject particleObj = new GameObject("BurningParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 1f;
        main.startSize = 0.1f;
        main.maxParticles = 20;
        
        var emission = ps.emission;
        emission.rateOverTime = 30f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient particleGradient = new Gradient();
        particleGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.8f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f), 0.5f),
                new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(particleGradient);
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
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
