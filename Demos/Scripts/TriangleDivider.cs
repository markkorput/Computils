using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Demos
{
	public class Tri
	{
		protected Vector3[] verts;

		public Tri(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			this.verts = new Vector3[] { v1, v2, v3 };
		}

		public float SurfaceSize { get { return GetTriangleSurfaceSize(verts[0], verts[1], verts[2]); } }
      
		protected static float GetTriangleSurfaceSize(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			// triangle surface = base * height * 0.5
			Vector3 basevec = v2 - v1;
			Vector3 diagonal = v3 - v1;

			float angle = Vector3.Angle(basevec, diagonal);
			float height = Mathf.Sin(angle) * diagonal.magnitude;
			return basevec.magnitude * height * 0.5f;
		}
	}

	public class EqualSubdivTri : Tri {
		private float size;

		public EqualSubdivTri(Vector3 v1, Vector3 v2, Vector3 v3, float size) : base(v1,v2,v3) {
			this.size = size;
		}
      
		public uint Count { get {
				return (uint)Mathf.CeilToInt(SurfaceSize / size);
			}}
      
		public Vector3 Side1 { get { return verts[1] - verts[0]; }}
		public Vector3 Side2 { get { return verts[2] - verts[0]; }}
      
		public uint Num1 { get { return (uint)Mathf.FloorToInt(Mathf.Max(1, 0.5f * (float)Count)); }}
		public uint Num2 { get { return Count - Num1; } }
      
		public Vector3 Step1 { get { return Side1 / Num1; }}
		public Vector3 Step2 { get { return Side2 / (Num2+1); } }

		public Vector3[] GetSubTriangles() {
			List<Vector3> list = new List<Vector3>();
			Vector3[] subverts;
         
			// Side1 Triangles
            //subverts = new Vector3[] { verts[0], verts[0] + Step1, verts[0] + Step2 };
            //for (int i = 0; i < Num1; i++)
            //{
            //    list.Add(subverts[0]);
            //    list.Add(subverts[1]);
            //    list.Add(subverts[2]);

            //    subverts[0] = subverts[1];
            //    subverts[1] += Step1;
            //    subverts[2] += Step2;
            //}
         
			// Side2 Triangles
			subverts = new Vector3[] { verts[0] + Step2, verts[0] + Step2 * 2, verts[0] + Step1 };
			for (int i = 0; i < Num2; i++) {
				list.Add(subverts[0]);
				list.Add(subverts[1]);
				list.Add(subverts[2]);

				subverts[0] = subverts[1];
				subverts[1] += Step2;
				subverts[2] += Step1;
			}
            
			return list.ToArray();
		}      
	}
   
	public class TriangleDivider : MonoBehaviour
	{
		public ComputeBufferFacade Source;
		public ComputeBufferFacade Destination;
		public float TrianglesSize = 0.3f;
		public bool AtStart = true;
		public bool EachUpdate = true;

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
			if (buf == null || buf.stride != sizeof(float)*3) return;

			Vector3[] sourceverts = new Vector3[buf.count];
			buf.GetData(sourceverts);


			Vector3[] newverts = EqualSizedTriangles(sourceverts, this.TrianglesSize);
         
			var dest = Destination.GetValid();
			dest = Populators.Utils.UpdateOrCreate(dest, newverts);
			Destination.Set(dest);
		}
      
		public static Vector3[] EqualSizedTriangles(Vector3[] triverts, float subTrianglesSize)
        {
            List<Vector3> veclist = new List<Vector3>();

            for (int i = 0; (i + 2) < triverts.Length; i += 3)
            {
                Vector3[] vectors = new EqualSubdivTri(
                    triverts[i + 0],
                    triverts[i + 1],
                    triverts[i + 2],
                    subTrianglesSize).GetSubTriangles();

                foreach (var v in vectors) veclist.Add(v);
            }

            return veclist.ToArray();
        }
    }
}