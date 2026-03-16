using UnityEngine;

namespace IdyllicFantasyNature
{
    public class WindControl : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField] private float _windStrength;
        [Range(0, 1)]
        [SerializeField] private float _windSpeed;
        [Range(0, 1)]
        [SerializeField] private float _windVariation;
        [Range(0, 1)]
        [Tooltip("how detailed the wind moves the object")]
        [SerializeField] private float _waveScale;
        [SerializeField] private Vector2 _windDirection;
        [Tooltip("material of the vegetation objects")]
        [SerializeField] private Material[] _material;

        /// <summary>
        /// the material gets the wind settings
        /// method only runs in the editor mode
        /// </summary>
        private void OnValidate()
        {
            for (int i = 0; i < _material.Length; i++)
            {
                _material[i].SetFloat("_Wind_Speed", _windSpeed);
                _material[i].SetFloat("_Wind_Variation", _windVariation);
                _material[i].SetFloat("_Wind_Strength", _windStrength);
                _material[i].SetFloat("_Wave_Scale", _waveScale);
                _material[i].SetVector("_Wind_Direction", _windDirection);
            }
        }
    }
}
