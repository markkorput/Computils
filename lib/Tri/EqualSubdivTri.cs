using System.Collections.Generic;
using UnityEngine;

namespace Computils.Tri
{
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

        // WIP
		public Vector3[] GetSubdivVerts() {
            List<Vector3> list = new List<Vector3>();
            Vector3[] subverts;
         
            // Side1 Triangles
            subverts = new Vector3[] { verts[0], verts[0] + Step1, verts[0] + Step2 };
            for (int i = 0; i < Num1; i++)
            {
                list.Add(subverts[0]);
                list.Add(subverts[1]);
                list.Add(subverts[2]);

                subverts[0] = subverts[1];
                subverts[1] += Step1;
                subverts[2] += Step2;
            }
         
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
      
		public static Vector3[] GetSubdivVerts(Vector3 v1, Vector3 v2, Vector3 v3, float size) {
			return new EqualSubdivTri(v1, v2, v3, size).GetSubdivVerts();
		}

		// Get subdiv verts for a list of verts (mesh)
		public static Vector3[] GetSubdivVerts(Vector3[] triverts, float size)
		{
			List<Vector3> verts = new List<Vector3>();
         
			for (int i = 0; (i + 2) < triverts.Length; i += 3)
			{
				var curverts = GetSubdivVerts(triverts[i + 0], triverts[i + 1], triverts[i + 2], size);
				foreach (var v in curverts) verts.Add(v);
			}
         
			return verts.ToArray();
		}
    }
   
}
