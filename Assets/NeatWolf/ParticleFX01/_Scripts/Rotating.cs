using UnityEngine;
using System.Collections;

namespace ParticleFX01 {
	public class Rotating : MonoBehaviour {
		public Vector3 rotationSpeed=Vector3.zero;
		public Space relativeTo=Space.Self;
		
		// Update is called once per frame
		void Update () {
			transform.Rotate(rotationSpeed * Time.deltaTime, relativeTo);
		}

		public void ForcedRotation(float time)
		{
			transform.Rotate(rotationSpeed * time, relativeTo);
		}
		/*public void SetVerticalRotation(float rotation)
		{
			Vector3 _rot = transform.localEulerAngles;
			_rot.x = rotation;
			transform.localEulerAngles = _rot;
		}*/
	}
}