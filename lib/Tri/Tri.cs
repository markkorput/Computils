using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Tri
{
	public class Tri
	{
		protected Vector3[] verts;

		public Tri(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			this.verts = new Vector3[] { v1, v2, v3 };
		}

		public Vector3 BaseVec { get { return verts[1] - verts[0]; }}
      
		public float Height { get {
			Vector3 diagonal = verts[2] - verts[0];
            float angle = Vector3.Angle(BaseVec, diagonal);
            return Mathf.Sin(angle) * diagonal.magnitude;
		}}
      
		public float SurfaceSize
		{
			get
			{
				return this.BaseVec.magnitude * this.Height * 0.5f;
			}
		}
      
		public Vector3[] Split(uint count=2) {
			Vector3[] splitverts = new Vector3[count * 3];
         
			Vector3 basestep = BaseVec / count;

			for (uint i = 0; i < count; i++) {
				splitverts[i * 3 + 0] = this.verts[2]; // current height vert becomes base base vert
				splitverts[i * 3 + 1] = this.verts[0] + basestep * i; // current first base vert becomes second base vert
				splitverts[i * 3 + 2] = this.verts[0] + basestep * (i+1); // current second base vert becoms height vert            
			}
         
			return splitverts;
		}      
      
		public static float GetSurfaceSize(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			return new Tri(v1, v2, v3).SurfaceSize;         
		}

		public static Tri CreateWithMinimizedHeight(Vector3[] vs)
		{
			return CreateWithMinimizedHeight(vs[0], vs[1], vs[2]);
		}

		public static Tri CreateWithMinimizedHeight(Vector3 v1, Vector3 v2, Vector3 v3)
            {
			return 
				new List<Tri>(new Tri[]{
    				new Tri(v1, v2, v3),
    				new Tri(v1, v3, v2),
    				new Tri(v2, v3, v1) })
			    .OrderByDescending((t) => t.Height)
				.First();
		}
	}
}