using UnityEngine;

public class FireStump : MonoBehaviour
{
    [Header("火焰树桩设置")]
    public float damage = 17f;
    public float burningDuration = 3f;
    public float burningDamagePerSecond = 5f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsGameOver())
            {
                if (player.TakeDamage(damage, DamageType.Fire))
                {
                    player.ApplyBurningEffect(burningDuration, burningDamagePerSecond);
                }
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
