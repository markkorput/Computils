using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Demos
{
	public class AutoTransform : MonoBehaviour
	{      
		public Vector3 Translate1;
		public Vector3 RotateEuler;
		public Vector3 Translate2;

		// Update is called once per frame
		void Update()
		{
			var dt = Time.deltaTime;
         
			this.transform.Translate(this.Translate1 * dt);
         
			var x = this.transform.right;
			var y = this.transform.up;
			var z = this.transform.forward; 
			this.transform.Rotate(x, RotateEuler.x);
			this.transform.Rotate(y, RotateEuler.y);
			this.transform.Rotate(z, RotateEuler.z);
			this.transform.Rotate(this.RotateEuler * dt);
			this.transform.Translate(this.Translate2 * dt);
		}
	}
}