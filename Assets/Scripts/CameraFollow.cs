using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 8f, -15f);
    public float smoothSpeed = 5f;
    public float zAxisSmoothSpeed = 15f;
    public bool lookAtTarget = true;

    [Header("相机角度")]
    public bool useFixedAngle = true;
    public Vector3 fixedRotation = new Vector3(25f, 0f, 0f);

    private PlayerController playerController;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }
        else
        {
            playerController = target.GetComponent<PlayerController>();
        }

        if (target != null)
        {
            transform.position = target.position + offset;
            ApplyRotation();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        
        Vector3 currentPos = transform.position;
        float newX = Mathf.Lerp(currentPos.x, desiredPosition.x, smoothSpeed * Time.deltaTime);
        float newY = Mathf.Lerp(currentPos.y, desiredPosition.y, smoothSpeed * Time.deltaTime);
        float newZ = Mathf.Lerp(currentPos.z, desiredPosition.z, zAxisSmoothSpeed * Time.deltaTime);
        
        transform.position = new Vector3(newX, newY, newZ);
        ApplyRotation();
    }

    void ApplyRotation()
    {
        if (useFixedAngle)
        {
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
        else if (lookAtTarget && target != null)
        {
            transform.LookAt(target);
        }
    }
}
