using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SunflowerPickup : MonoBehaviour
{
    [Header("回血设置")]
    public float healAmount = 35f;
    public KeyCode grabKey = KeyCode.X;
    public float rotateSpeed = 0f;
    public float bobAmount = 0.15f;
    public float bobSpeed = 2f;
    public float visualScale = 1.5f;
    public Vector3 visualOffset = Vector3.zero;
    public Vector3 modelRotationOffset = Vector3.zero;

    [Header("模型与特效")]
    public GameObject modelPrefab;
    public GameObject idleEffectPrefab;
    public GameObject collectEffectPrefab;
    public Vector3 idleEffectOffset = Vector3.zero;
    public Vector3 collectEffectOffset = Vector3.zero;
    public Vector3 idleEffectScale = Vector3.one;

    private PlayerController hoveredPlayer;
    private Collider triggerCollider;
    private Transform visualRoot;
    private Transform modelRoot;
    private float bobTimer = 0f;
    private bool consumed = false;
    private bool visualsInitialized = false;
    private Vector3 baseVisualLocalPos = Vector3.zero;
    private Quaternion baseVisualLocalRot = Quaternion.identity;
    private GameObject idleEffectInstance;

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
        if (idleEffectInstance != null)
        {
            if (Application.isPlaying) Destroy(idleEffectInstance);
            else DestroyImmediate(idleEffectInstance);
            idleEffectInstance = null;
        }

        visualRoot = new GameObject("VisualRoot").transform;
        visualRoot.SetParent(transform, false);
        baseVisualLocalPos = visualOffset;
        visualRoot.localPosition = baseVisualLocalPos;
        baseVisualLocalRot = Quaternion.identity;
        visualRoot.localRotation = baseVisualLocalRot;

        modelRoot = new GameObject("ModelRoot").transform;
        modelRoot.SetParent(visualRoot, false);
        modelRoot.localPosition = Vector3.zero;
        modelRoot.localRotation = Quaternion.Euler(modelRotationOffset);

        if (modelPrefab != null)
        {
            GameObject visual = Instantiate(modelPrefab, modelRoot);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * visualScale;
            RemoveChildColliders(visual);
        }
        else
        {
            CreateFallbackModel(modelRoot);
        }

        AlignVisualToBounds();

        if (idleEffectPrefab != null)
        {
            idleEffectInstance = Instantiate(idleEffectPrefab, transform);
            idleEffectInstance.transform.localPosition = idleEffectOffset;
            idleEffectInstance.transform.localScale = Vector3.Scale(idleEffectInstance.transform.localScale, idleEffectScale);
        }
    }

    void Update()
    {
        if (visualRoot != null)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float yOffset = Mathf.Sin(bobTimer) * bobAmount;
            visualRoot.localPosition = baseVisualLocalPos + new Vector3(0f, yOffset, 0f);
            if (Mathf.Abs(rotateSpeed) > Mathf.Epsilon)
            {
                visualRoot.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                visualRoot.localRotation = baseVisualLocalRot;
            }
        }

        if (!consumed && hoveredPlayer != null)
        {
            if (hoveredPlayer.IsGameOver())
            {
                hoveredPlayer = null;
            }
            else if (Input.GetKeyDown(grabKey))
            {
                TryHeal();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (other.CompareTag("Player"))
        {
            hoveredPlayer = other.GetComponent<PlayerController>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (hoveredPlayer != null && other.gameObject == hoveredPlayer.gameObject)
        {
            hoveredPlayer = null;
        }
    }

    void TryHeal()
    {
        if (hoveredPlayer == null) return;
        if (hoveredPlayer.RestoreHealth(healAmount))
        {
            consumed = true;
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
            PlayCollectEffect();
            Destroy(gameObject, 0.25f);
        }
    }

    void PlayCollectEffect()
    {
        if (collectEffectPrefab == null) return;
        Vector3 pos = transform.position + collectEffectOffset;
        GameObject fx = Instantiate(collectEffectPrefab, pos, Quaternion.identity, transform);
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

    void CreateFallbackModel(Transform parent)
    {
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.transform.SetParent(parent, false);
        stem.transform.localScale = new Vector3(visualScale * 0.3f, visualScale * 0.5f, visualScale * 0.3f);
        Renderer stemRenderer = stem.GetComponent<Renderer>();
        if (stemRenderer != null)
        {
            stemRenderer.material.color = new Color(0.2f, 0.6f, 0.2f);
        }
        Destroy(stem.GetComponent<Collider>());

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(parent, false);
        head.transform.localScale = Vector3.one * visualScale * 0.9f;
        head.transform.localPosition = new Vector3(0f, visualScale * 1.0f, 0f);
        Renderer headRenderer = head.GetComponent<Renderer>();
        if (headRenderer != null)
        {
            headRenderer.material.color = new Color(1f, 0.85f, 0.2f);
        }
        Destroy(head.GetComponent<Collider>());
    }

    void AlignVisualToBounds()
    {
        if (visualRoot == null) return;

        Renderer[] renderers = modelRoot != null ? modelRoot.GetComponentsInChildren<Renderer>(true) : null;
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
