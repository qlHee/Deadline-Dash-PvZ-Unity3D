using UnityEngine;

namespace IdyllicFantasyNature
{
    public class BushLeafTrigger : MonoBehaviour
    {
        [Tooltip("particle system that will be triggered")]
        [SerializeField] private ParticleSystem _leafParticleSystem;
        [Tooltip("the tag of the player gameobject with the collider")]
        [SerializeField] private string _playerTag;


        /// <summary>
        /// When the player leaves the trigger, the particle system is placed at the last player position and starts playing
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(_playerTag))
            {
                _leafParticleSystem.gameObject.transform.position = new Vector3(other.GetComponent<Transform>().position.x, _leafParticleSystem.gameObject.transform.position.y, other.GetComponent<Transform>().position.z);
                _leafParticleSystem.Play();
            }
        }
    }
}

