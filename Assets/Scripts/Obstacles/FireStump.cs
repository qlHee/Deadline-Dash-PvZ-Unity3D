using UnityEngine;

public class FireStump : MonoBehaviour
{
    [Header("火焰树桩设置")]
    public float damage = 17f;
    public float burningDuration = 3f;
    public float burningDamagePerSecond = 5f;

    void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    void HandleHit(Collider other)
    {
        if (other == null)
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null && !player.IsGameOver())
        {
            if (player.WasFireShieldAbsorbedThisFrame() || player.TryConsumeFireShield())
            {
                return;
            }
            if (player.TakeDamage(damage, DamageType.Fire))
            {
                player.ApplyBurningEffect(burningDuration, burningDamagePerSecond);
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            PeaProjectile normalPea = other.GetComponent<PeaProjectile>();
            if (normalPea != null)
            {
                normalPea.ConvertToBurningPea();
                return;
            }

            IcePeaProjectile icePea = other.GetComponent<IcePeaProjectile>();
            if (icePea != null)
            {
                icePea.ConvertToNormalPea();
                return;
            }
        }
    }
}
