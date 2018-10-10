using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Demos
{
	[AddComponentMenu("Computils/Demos/Fps Mouse Look")]
	public class LookAt : MonoBehaviour
	{
		public Transform Target;
		public bool DefaultToMainCam = true;
      
		void Update()
		{
			var t = this.Target;
			if (t == null && this.DefaultToMainCam) t = Camera.main.transform;
			if (t == null) return;
			this.transform.LookAt(t);
		}
	}
}