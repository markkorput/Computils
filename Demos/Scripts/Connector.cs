using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Computils.Demos
{
	/// <summary>
	/// Traces the shortest line through particles between two positions
	/// </summary>
	public class Connector : MonoBehaviour
	{
		public ComputeBufferFacade Particles;
		public int Steps = 16;
		public Processors.FindNearest FindNearest;
		public Transform From;
		public Transform To;

		[System.Serializable]
		public class IndexesEvent : UnityEvent<uint[]> {}
      
		[Header("Events")]
		public IndexesEvent TracesIndexes;

		void Update()
		{
			var partbuf = Particles.GetValid();
			if (partbuf == null) return;

			Vector3 FromToVec = To.position - From.position;
			Vector3 StepVec = FromToVec / (Steps + 1);
			Vector3 p = From.position + StepVec;

			List<uint> indexes = new List<uint>();
			for (int i = 0; i < Steps; i++) {            
				var nearestitem = this.FindNearest.GetNearest(partbuf, p, indexes.ToArray());
				if (nearestitem != null) indexes.Add(nearestitem.index);

				p += StepVec;
			}

			this.TracesIndexes.Invoke(indexes.ToArray());
		}
	}
}