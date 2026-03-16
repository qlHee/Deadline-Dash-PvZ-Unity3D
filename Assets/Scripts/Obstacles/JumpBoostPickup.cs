using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class JumpBoostPickup : MonoBehaviour
{
    [Header("跳跃增益")]
    public float jumpMultiplier = 5f;
    public float duration = 8f;
    public float visualScale = 1.2f;
    public Vector3 visualOffset = Vector3.zero;
    public float rotateSpeed = 50f;
    public float bobAmount = 0.15f;
    public float bobSpeed = 3f;
    public Color fallbackColor = new Color(0.9f, 0.4f, 1f, 0.7f);

    [Header("模型与特效")]
    public GameObject modelPrefab;
    public GameObject idleEffectPrefab;
    public GameObject pickupEffectPrefab;
    public Vector3 idleEffectOffset = Vector3.zero;
    public Vector3 pickupEffectOffset = Vector3.zero;
    [Header("Player Pickup Effect")]
    public GameObject playerPickupEffectPrefab;
    public float playerEffectLifetime = 1f;
    public Vector3 playerEffectOffset = Vector3.zero;

    private Collider triggerCollider;
    private Transform visualRoot;
    private bool consumed = false;
    private float bobTimer = 0f;
    private bool visualsInitialized = false;
    private Vector3 baseVisualLocalPos = Vector3.zero;

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
        visualRoot.localPosition = baseVisualLocalPos;

        if (modelPrefab != null)
        {
            GameObject model = Instantiate(modelPrefab, visualRoot);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one * visualScale;
            RemoveChildColliders(model);
        }
        else
        {
            CreateFallbackModel(visualRoot);
        }

        AlignVisualToBounds();

        if (idleEffectPrefab != null)
        {
            GameObject fx = Instantiate(idleEffectPrefab, visualRoot);
            fx.transform.localPosition = idleEffectOffset;
        }
    }

    void Update()
    {
        if (visualRoot == null) return;

        bobTimer += Time.deltaTime * bobSpeed;
        float yOffset = Mathf.Sin(bobTimer) * bobAmount;
        visualRoot.localPosition = baseVisualLocalPos + new Vector3(0f, yOffset, 0f);
        visualRoot.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsGameOver())
            {
                player.ApplyJumpBoost(duration, jumpMultiplier);
                PlayPlayerEffect(player.transform);
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
        if (visualRoot != null)
        {
            visualRoot.gameObject.SetActive(false);
        }
        Destroy(gameObject, 0.05f);
    }

    void PlayPickupEffect()
    {
        if (pickupEffectPrefab == null) return;
        Vector3 pos = transform.position + pickupEffectOffset;
        GameObject fx = Instantiate(pickupEffectPrefab, pos, Quaternion.identity);
        Destroy(fx, 3f);
    }

    void PlayPlayerEffect(Transform target)
    {
        if (playerPickupEffectPrefab == null || target == null) return;
        Vector3 pos = target.position + target.TransformVector(playerEffectOffset);
        GameObject fx = Instantiate(playerPickupEffectPrefab, pos, target.rotation, target);
        if (playerEffectLifetime > 0f)
        {
            Destroy(fx, playerEffectLifetime);
        }
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

    void CreateFallbackModel(Transform parent)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.SetParent(parent, false);
        capsule.transform.localScale = new Vector3(visualScale * 0.7f, visualScale, visualScale * 0.7f);
        Renderer renderer = capsule.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = fallbackColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", fallbackColor * 1.4f);
        }
        Destroy(capsule.GetComponent<Collider>());
    }

    void AlignVisualToBounds()
    {
        if (visualRoot == null) return;

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 centerLocal = transform.InverseTransformPoint(bounds.center);
        baseVisualLocalPos = visualOffset - centerLocal;
        visualRoot.localPosition = baseVisualLocalPos;
    }
}
