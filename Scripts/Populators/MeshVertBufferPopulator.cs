#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Mesh Verts Buffer Populator")]
    class MeshVertBufferPopulator : MonoBehaviour
    {
        public enum MeshInterpretation { Vertices, Triangles };
      
        public ComputeBufferFacade Facade;
        public MeshFilter MeshFilter;
        public MeshInterpretation Interpretation = MeshInterpretation.Triangles;
		public Transform Transformer;

#if BOUNDING_BOX
		[Header("Read-Only")]
		public Vector3 BoundingBoxMin;
		public Vector3 BoundingBoxMax;
#endif
      
		private void OnEnable()
        {
            Vector3[] verts = null;
         
            if (this.Interpretation.Equals(MeshInterpretation.Vertices))
				verts = GetVerts(MeshFilter.mesh, this.Transformer == null ? Matrix4x4.identity : this.Transformer.localToWorldMatrix);

            if (this.Interpretation.Equals(MeshInterpretation.Triangles))
				verts = GetTriangleVerts(MeshFilter.mesh, this.Transformer == null ? Matrix4x4.identity : this.Transformer.localToWorldMatrix);

            if (verts != null)
            {
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
                ComputeBuffer buf = Utils.Create(verts);
                Facade.Set(buf);
            }
        }
      
        public static Vector3[] GetVerts(Mesh mesh, Matrix4x4 transformMatrix)
        {
            Vector3[] data = new Vector3[mesh.vertices.Length];

			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				data[i] = transformMatrix * new Vector4(mesh.vertices[i].x, mesh.vertices[i].y, mesh.vertices[i].z, 1);
			}

            return data;
        }
      
		public static Vector3[] GetTriangleVerts(Mesh mesh, Matrix4x4 transformMatrix)
        {
            int[] vertIndices = mesh.GetTriangles(0);
            Vector3[] data = new Vector3[vertIndices.Length];

            for (int i = 0; i < vertIndices.Length; i++)
				data[i] = transformMatrix * new Vector4(mesh.vertices[vertIndices[i]].x, mesh.vertices[vertIndices[i]].y, mesh.vertices[vertIndices[i]].z, 1);
            
            return data;
        }
    }
}
