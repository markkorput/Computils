using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.Forces
{
    public class Force : MonoBehaviour
    {
		public virtual void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf) {
			Debug.LogWarning("Force.Apply should be overwritten");
		}
    }
}