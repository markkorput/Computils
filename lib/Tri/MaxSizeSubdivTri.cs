using System.Collections.Generic;
using UnityEngine;

namespace Computils.Tri
{
	public class MaxSizeSubdivTri : Tri
	{
		private float maxsize;

		public MaxSizeSubdivTri(Vector3 v1, Vector3 v2, Vector3 v3, float size) : base(v1, v2, v3)
		{
			this.maxsize = size;
		}

		public Vector3[] GetSubdivVerts()
		{
			var size = this.SurfaceSize;
			var count = this.SurfaceSize / this.maxsize;

			// divide in half
			if (count <= 2.0f) {
				return Tri.CreateWithMinimizedHeight(this.verts).Split(2);
			}

            // split in half and recursively get subdivisions from the created halfs
			if (count > 3.0f) {
				Vector3[] splitverts = Tri.CreateWithMinimizedHeight(this.verts).Split(2);
				return GetSubdivVerts(splitverts, this.maxsize);
			}
         
			// devide into three; we'll split into three pieces along the height axis,
			// but merge first two and resplit them along the newmerge triangles height axis
            // for more even-sides triangles
			var thirds_verts = Tri.CreateWithMinimizedHeight(this.verts).Split(3);
			// subdiv first two thirds again
			Vector3[] twothirds_verts = Tri.CreateWithMinimizedHeight(thirds_verts[0], thirds_verts[1], thirds_verts[5]).Split(2);
            // overwrite "original" first two thirds
			twothirds_verts.CopyTo(thirds_verts, 0);
			return thirds_verts;
		}
      
		public static Vector3[] GetSubdivVerts(Vector3 v1, Vector3 v2, Vector3 v3, float size)
        {
            return new MaxSizeSubdivTri(v1, v2, v3, size).GetSubdivVerts();
        }

		// Get subdiv verts for a list of verts (mesh)
		public static Vector3[] GetSubdivVerts(Vector3[] triverts, float size) {
			List<Vector3> verts = new List<Vector3>();
         
			for (int i = 0; (i + 2) < triverts.Length; i+=3) {
				var curverts = GetSubdivVerts(triverts[i + 0], triverts[i + 1], triverts[i + 2], size);
				foreach (var v in curverts) verts.Add(v);
			}

			return verts.ToArray();
		}
    }
}