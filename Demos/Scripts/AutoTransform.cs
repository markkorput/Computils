using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Demos
{
	[System.Obsolete("Use FuseTools.AutoTransform...")]
	public class AutoTransform : MonoBehaviour
	{
		[Tooltip("Defaults to this object's transform")]
		public Transform Transform;
		public Vector3 Translate1;
		public Vector3 RotateEuler;
		public Vector3 Translate2;

		void Start() {
			if (this.Transform == null) this.Transform = this.transform;
		}
      
		void Update()
		{
			var trans = this.Transform;
			var dt = Time.deltaTime;
         
			trans.Translate(this.Translate1 * dt);

			var x = trans.right;
			var y = trans.up;
			var z = trans.forward; 
			trans.Rotate(x, RotateEuler.x);
			trans.Rotate(y, RotateEuler.y);
			trans.Rotate(z, RotateEuler.z);
			trans.Rotate(this.RotateEuler * dt);
			trans.Translate(this.Translate2 * dt);
		}
	}
}