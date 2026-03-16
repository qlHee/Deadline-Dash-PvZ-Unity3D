using UnityEngine;

namespace IdyllicFantasyNature
{
    [ExecuteInEditMode]
    public class VegetationBendControl : MonoBehaviour
    {
        [SerializeField] private bool _enableBendFeature = false;
        [Tooltip("The origin where the impact on the object starts")]
        [SerializeField] private Transform _bendOrigin;
        [Range(0.3f, 1)]
        [Tooltip("object starts to bend when the player is at a certain distance")]
        [SerializeField] private float _startBendRange;
        [Range(0, 1)]
        [SerializeField] private float _bendStrength;
        [Tooltip("material of the vegetation objects")]
        [SerializeField] private Material[] _material;

        // current world space position of the bending object 
        private Vector3 _currentBendPosition;

        private void Update()
        {
            if (_enableBendFeature)
            {
                moveOnVegetation();
            }

        }

        private void OnValidate()
        {
            BendSettings();
        }

        /// <summary>
        /// the material gets the object position to know when to bend
        /// only updates when player moves
        /// </summary>
        void moveOnVegetation()
        {
            if (_currentBendPosition != _bendOrigin.position)
            {
                for (int i = 0; i < _material.Length; i++)
                {
                    _material[i].SetVector("_Player_Position", _bendOrigin.position);
                }

                _currentBendPosition = _bendOrigin.position;
            }
        }

        /// <summary>
        /// the material gets the bend settings
        /// </summary>
        void BendSettings()
        {
            for (int i = 0; i < _material.Length; i++)
            {
                _material[i].SetFloat("_Bend_Strength", _bendStrength);
                _material[i].SetFloat("_Start_Bend_Range", _startBendRange);
            }
        }
    }
}
