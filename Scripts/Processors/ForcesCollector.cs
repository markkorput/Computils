using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
    public class ForcesCollector : MonoBehaviour
    {
		public ComputeBufferFacade BufferFacade;
		public Forces.Force[] Forces;

        void Update()
        {
			var buf = BufferFacade.GetValid();
         
			if (buf != null)
			{
				foreach (var force in Forces)
				{               
					force.Apply(buf);
				}
			}
        }
    }
}