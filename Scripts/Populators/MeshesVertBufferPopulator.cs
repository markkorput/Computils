#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Meshes Verts Buffer Populator")]
    class MeshesVertBufferPopulator : MonoBehaviour
    {
        public enum MeshInterpretation { Vertices, Triangles };
      
        public ComputeBufferFacade Facade;
        public MeshFilter[] MeshFilters;
        public MeshInterpretation Interpretation = MeshInterpretation.Vertices;
        public float ScaleFactor = 1.0f;
      
#if BOUNDING_BOX
        [Header("Read-Only")]
        public Vector3 BoundingBoxMin;
        public Vector3 BoundingBoxMax;
#endif

        private void OnEnable()
        {
            Vector3[] verts = null;
         
            if (this.Interpretation.Equals(MeshInterpretation.Vertices))
                verts = GetVerts(this.ScaleFactor);

            if (this.Interpretation.Equals(MeshInterpretation.Triangles))
                verts = GetTriangleVerts(this.ScaleFactor);

            if (verts != null)
            {
                ComputeBuffer buf = Utils.Create(verts);
                Facade.Set(buf);
            
#if BOUNDING_BOX
                if (verts.Length > 0)
                {
                    BoundingBoxMin = BoundingBoxMax = verts[0];

                    foreach (var vert in verts)
                    {
                        if (vert.x < this.BoundingBoxMin.x) this.BoundingBoxMin.x = vert.x;
                        if (vert.y < this.BoundingBoxMin.y) this.BoundingBoxMin.y = vert.y;
                        if (vert.z < this.BoundingBoxMin.z) this.BoundingBoxMin.z = vert.z;
                        if (vert.x > this.BoundingBoxMax.x) this.BoundingBoxMax.x = vert.x;
                        if (vert.y > this.BoundingBoxMax.y) this.BoundingBoxMax.y = vert.y;
                        if (vert.z > this.BoundingBoxMax.z) this.BoundingBoxMax.z = vert.z;
                    }
                }
#endif
            }
        }
      
        private Vector3[] GetVerts(float ScaleFactor)
        {
			int ttl = 0;
			for (int mi = 0; mi < MeshFilters.Length; mi++)
			{
				ttl += MeshFilters[mi].mesh.vertices.Length;
			}
         
            Vector3[] data = new Vector3[ttl];
         
			int idx = 0;
         
			for (int mi = 0; mi < MeshFilters.Length; mi++) {
				var mesh = MeshFilters[mi].mesh;

				for (int i = 0; i < mesh.vertices.Length; i++)
				{
					data[idx] = mesh.vertices[i] * ScaleFactor;
					idx++;
				}
			}

            return data;
        }
      
        private Vector3[] GetTriangleVerts(float ScaleFactor)
        {
			int ttl = 0;
            for (int mi = 0; mi < MeshFilters.Length; mi++)
            {
				ttl += MeshFilters[mi].mesh.GetTriangles(0).Length;
            }
         
			Vector3[] data = new Vector3[ttl];
         
            int idx = 0;

            for (int mi = 0; mi < MeshFilters.Length; mi++)
            {
				var mesh = MeshFilters[mi].mesh;
				var vertIndices = mesh.GetTriangles(0);

				for (int i = 0; i < vertIndices.Length; i++)
                {
                    data[idx] = mesh.vertices[vertIndices[i]] * ScaleFactor;
                    idx++;
                }
            }
         
            return data;
        }
    }
}
