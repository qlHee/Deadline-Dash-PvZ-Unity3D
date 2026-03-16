using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随设置")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public float smoothSpeed = 5f;
    public bool lookAtTarget = true;

    [Header("相机角度")]
    public bool useFixedAngle = true;
    public Vector3 fixedRotation = new Vector3(20f, 0f, 0f);

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
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
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
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
