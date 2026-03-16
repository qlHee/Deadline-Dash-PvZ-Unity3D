using UnityEngine;

public class Nut : MonoBehaviour
{
    [Header("坚果设置")]
    public float damage = 37f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsGameOver())
            {
                player.TakeDamage(damage);
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            PeaProjectile normalPea = other.GetComponent<PeaProjectile>();
            if (normalPea != null)
            {
                Destroy(other.gameObject);
                return;
            }

            IcePeaProjectile icePea = other.GetComponent<IcePeaProjectile>();
            if (icePea != null)
            {
                Destroy(other.gameObject);
                return;
            }
        }
    }
}
