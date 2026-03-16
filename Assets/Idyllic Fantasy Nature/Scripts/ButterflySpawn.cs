using UnityEngine;

namespace IdyllicFantasyNature
{
    public class ButterflySpawn : MonoBehaviour
    {
        [Tooltip("child with the mesh renderer to make the butterfly invisible when the animation is not running")]
        [SerializeField] private GameObject _butterflyChild;
        // the spawn area script to get the data from the set range and cooldown
        private ButterflySpawnArea _area;
        // the cooldown to respawn the butterfly
        private float _cooldown = 0;
        // indicates if the animation is playing to stop the cooldown for the respawn
        private bool _isPlaying = false;
        // animator of the butterfly
        private Animator _animator;

        // Start is called before the first frame update
        void Start()
        {
            _animator = GetComponent<Animator>();
            _area = GetComponentInParent<ButterflySpawnArea>();

            // sets the animator off to not play the animation before the cooldown reaches 0
            _animator.enabled = false;

            // get the set cooldown
            _cooldown = Random.Range(_area.MinCooldown, _area.MaxCooldown);

            // makes the butterfly invisible until the animation starts playing
            _butterflyChild.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            ActivateButterfly();
        }


        /// <summary>
        /// activates the butterfly when the cooldown reaches 0 and the animation isn't already playing
        /// </summary>
        private void ActivateButterfly()
        {
            if (!_isPlaying)
            {
                if (_cooldown <= 0)
                {
                    // activates the animator to play the animation
                    _animator.enabled = true;
                    // resets the cooldown for the respawn
                    _cooldown = Random.Range(_area.MinCooldown, _area.MaxCooldown);
                    // makes the butterfly visible
                    _butterflyChild.SetActive(true);
                    // determines a random position for the butterfly in the spawn area
                    transform.position = new Vector3(Random.Range(_area.Collider.bounds.min.x, _area.Collider.bounds.max.x), Random.Range(_area.Collider.bounds.min.y, _area.Collider.bounds.max.y), Random.Range(_area.Collider.bounds.min.z, _area.Collider.bounds.max.z));

                    _isPlaying = true;

                }
                else
                {
                    _cooldown -= Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// animation event will be used at the end of the butterfly animation
        /// Resets the stats to enable the cooldown for the respawn
        /// </summary>
        public void AnimationEnded()
        {
            _isPlaying = false;
            _animator.enabled = false;
            _butterflyChild.SetActive(false);
        }
    }
}
