using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Computils.Tri
{
	public class SubdivTriangle : MonoBehaviour
	{
		public enum Methd { Equal, MaxSize };
      
		public ComputeBufferFacade Source;
		public ComputeBufferFacade Destination;      
		public float SubdivsSize = 0.3f;
		public Methd Method = Methd.MaxSize;
		public bool AtStart = true;
		public bool EachUpdate = true;
      
		public UnityEvent AfterSubdiv;

		private void Start()
		{
			if (AtStart) Subdivide();
		}

		private void Update()
		{
			if (this.EachUpdate) this.Subdivide();
		}
      
		public void Subdivide() {
			var buf = Source.GetValid();
			if (buf == null || buf.stride != sizeof(float) * 3) return;
         
			Vector3[] sourceverts = new Vector3[buf.count];
			buf.GetData(sourceverts);
         
			Vector3[] newverts = new Vector3[0];
			if (this.Method.Equals(Methd.MaxSize)) newverts = MaxSizeSubdivTri.GetSubdivVerts(sourceverts, this.SubdivsSize);
			if (this.Method.Equals(Methd.Equal)) newverts = EqualSubdivTri.GetSubdivVerts(sourceverts, this.SubdivsSize);
         
			var dest = Destination.GetValid();
			dest = Populators.Utils.UpdateOrCreate(dest, newverts);
			Destination.Set(dest);

			this.AfterSubdiv.Invoke();
		}
    }
}