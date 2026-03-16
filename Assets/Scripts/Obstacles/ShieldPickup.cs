using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ShieldPickup : MonoBehaviour
{
    [Header("护盾设置")]
    public ShieldType shieldType = ShieldType.Normal;
    public float shieldDuration = 8f;
    public float visualScale = 1.2f;
    public Vector3 visualOffset = Vector3.zero;
    public float rotateSpeed = 60f;
    public Vector3 rotationAxis = Vector3.up;
    public float bobAmount = 0.12f;
    public float bobSpeed = 2.5f;
    public Color fallbackColor = new Color(0.4f, 0.8f, 1f, 0.55f);

    [Header("模型与特效")]
    public GameObject modelPrefab;
    public GameObject idleEffectPrefab;
    public GameObject pickupEffectPrefab;
    public Vector3 idleEffectOffset = Vector3.zero;
    public Vector3 pickupEffectOffset = Vector3.zero;
    public Vector3 modelRotationOffset = Vector3.zero;
    public Vector3 modelScale = Vector3.one;

    private Collider triggerCollider;
    private Transform visualRoot;
    private bool consumed = false;
    private float bobTimer = 0f;
    private bool visualsInitialized = false;
    private Vector3 baseVisualLocalPos = Vector3.zero;
    private Quaternion baseVisualLocalRot = Quaternion.identity;
    private Transform modelRoot;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider>();
        }
        triggerCollider.isTrigger = true;
        SphereCollider sphere = triggerCollider as SphereCollider;
        if (sphere != null)
        {
            sphere.radius = Mathf.Max(0.3f, visualScale * 0.6f);
        }
    }

    void Start()
    {
        if (!visualsInitialized)
        {
            BuildVisuals();
        }
    }

    public void RebuildVisuals()
    {
        BuildVisuals();
    }

    void BuildVisuals()
    {
        visualsInitialized = true;
        if (visualRoot != null)
        {
            if (Application.isPlaying) Destroy(visualRoot.gameObject);
            else DestroyImmediate(visualRoot.gameObject);
        }

        visualRoot = new GameObject("VisualRoot").transform;
        visualRoot.SetParent(transform, false);
        baseVisualLocalPos = visualOffset;
        baseVisualLocalRot = Quaternion.identity;
        visualRoot.localPosition = baseVisualLocalPos;
        visualRoot.localRotation = baseVisualLocalRot;

        if (modelRoot != null)
        {
            if (Application.isPlaying) Destroy(modelRoot.gameObject);
            else DestroyImmediate(modelRoot.gameObject);
        }
        modelRoot = new GameObject("ModelRoot").transform;
        modelRoot.SetParent(visualRoot, false);
        modelRoot.localPosition = Vector3.zero;
        modelRoot.localRotation = Quaternion.Euler(modelRotationOffset);

        if (modelPrefab != null)
        {
            GameObject model = Instantiate(modelPrefab, modelRoot);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.Scale(Vector3.one * visualScale, modelScale);
            RemoveChildColliders(model);
        }
        else
        {
            CreateFallbackShield(modelRoot);
        }

        AlignVisualToBounds();

        if (idleEffectPrefab != null)
        {
            GameObject fx = Instantiate(idleEffectPrefab, transform);
            fx.transform.localPosition = idleEffectOffset;
        }
    }

    void Update()
    {
        if (visualRoot == null) return;

        bobTimer += Time.deltaTime * bobSpeed;
        float yOffset = Mathf.Sin(bobTimer) * bobAmount;
        visualRoot.localPosition = baseVisualLocalPos + new Vector3(0f, yOffset, 0f);
        if (Mathf.Abs(rotateSpeed) > Mathf.Epsilon)
        {
            Vector3 axis = rotationAxis.sqrMagnitude > Mathf.Epsilon ? rotationAxis.normalized : Vector3.up;
            visualRoot.Rotate(axis, rotateSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            visualRoot.localRotation = baseVisualLocalRot;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsGameOver())
            {
                player.ActivateShield(shieldType, shieldDuration);
                PlayPickupEffect();
                Consume();
            }
        }
    }

    void Consume()
    {
        consumed = true;
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
        Destroy(gameObject, 0.2f);
    }

    void PlayPickupEffect()
    {
        if (pickupEffectPrefab == null) return;
        Vector3 pos = transform.position + pickupEffectOffset;
        GameObject fx = Instantiate(pickupEffectPrefab, pos, Quaternion.identity, transform);
        Destroy(fx, 3f);
    }

    void RemoveChildColliders(GameObject root)
    {
        if (root == null) return;
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                Destroy(col);
            }
        }
    }

    void CreateFallbackShield(Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(parent, false);
        sphere.transform.localScale = Vector3.Scale(Vector3.one * visualScale, modelScale);
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = fallbackColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", fallbackColor * 1.5f);
        }
        Destroy(sphere.GetComponent<Collider>());
    }

    void AlignVisualToBounds()
    {
        if (visualRoot == null) return;

        Renderer[] renderers = modelRoot != null ? modelRoot.GetComponentsInChildren<Renderer>(true) : visualRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 centerLocal = transform.InverseTransformPoint(bounds.center);
        baseVisualLocalPos = visualOffset - centerLocal;
        visualRoot.localPosition = baseVisualLocalPos;
        baseVisualLocalRot = Quaternion.identity;
        visualRoot.localRotation = baseVisualLocalRot;
        if (modelRoot != null)
        {
            modelRoot.localRotation = Quaternion.Euler(modelRotationOffset);
        }
    }
}
