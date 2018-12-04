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
		public const uint CoroutineBatchCount = 1000;
        public enum MeshInterpretation { Vertices, Triangles, TriangleLines };
      
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
			StartCoroutine(this.Populate());
        }

		IEnumerator Populate() {
			Vector3[] verts = null;

			if (this.Interpretation.Equals(MeshInterpretation.Vertices))
			{
				verts = new Vector3[MeshFilter.mesh.vertices.Length];
				yield return LoadVerts(MeshFilter.mesh, verts, this.Transformer == null ? Matrix4x4.identity : this.Transformer.localToWorldMatrix);
			}
         
			if (this.Interpretation.Equals(MeshInterpretation.Triangles))
			{
				verts = new Vector3[MeshFilter.mesh.GetTriangles(0).Length];
				yield return LoadTriangleVerts(MeshFilter.mesh, verts, this.Transformer == null ? Matrix4x4.identity : this.Transformer.localToWorldMatrix);
			}

			if (this.Interpretation.Equals(MeshInterpretation.TriangleLines))
            {
                verts = new Vector3[MeshFilter.mesh.GetTriangles(0).Length*2];
                yield return LoadTriangleLineVerts(MeshFilter.mesh, verts, this.Transformer == null ? Matrix4x4.identity : this.Transformer.localToWorldMatrix);
            }

            if (verts != null)
            {
#if BOUNDING_BOX
				yield return LoadBoundingBox(verts);
#endif
                ComputeBuffer buf = Utils.Create(verts);
                Facade.Set(buf);
            }
		}
      
#if BOUNDING_BOX
		public IEnumerator LoadBoundingBox(Vector3[] verts) {
			if (verts.Length == 0) yield break;
         
            BoundingBoxMin = BoundingBoxMax = verts[0];
         
            // coroutine stuff
            uint batchIdx = 0;

            foreach (var vert in verts)
            {
                if (vert.x < this.BoundingBoxMin.x) this.BoundingBoxMin.x = vert.x;
                if (vert.y < this.BoundingBoxMin.y) this.BoundingBoxMin.y = vert.y;
                if (vert.z < this.BoundingBoxMin.z) this.BoundingBoxMin.z = vert.z;
                if (vert.x > this.BoundingBoxMax.x) this.BoundingBoxMax.x = vert.x;
                if (vert.y > this.BoundingBoxMax.y) this.BoundingBoxMax.y = vert.y;
                if (vert.z > this.BoundingBoxMax.z) this.BoundingBoxMax.z = vert.z;
            
                // coroutine stuff
                batchIdx++;
				if (batchIdx < CoroutineBatchCount) continue;
                yield return null;
                batchIdx = 0;
            }
        }
#endif

		public static IEnumerator LoadTriangleVerts(Mesh mesh, Vector3[] dest, Matrix4x4 transformMatrix) {
			int[] vertIndices = mesh.GetTriangles(0);
			uint count = (uint)Mathf.Min(vertIndices.Length, dest.Length/2);
         
			// coroutine stuff
			uint batchIdx = 0;
         
			for (int i = 0; i < count; i++)
			{
				dest[i] = transformMatrix * new Vector4(mesh.vertices[vertIndices[i]].x, mesh.vertices[vertIndices[i]].y, mesh.vertices[vertIndices[i]].z, 1);
            
				// coroutine stuff
                batchIdx++;
				if (batchIdx < CoroutineBatchCount) continue;
                yield return null;
                batchIdx = 0;            
			}
		}
      
		public static IEnumerator LoadTriangleLineVerts(Mesh mesh, Vector3[] dest, Matrix4x4 transformMatrix)
        {
            int[] vertIndices = mesh.GetTriangles(0);
            uint count = (uint)Mathf.Min(vertIndices.Length, dest.Length);

            // coroutine stuff
            uint batchIdx = 0;
         
			for (int i = 0; i < vertIndices.Length; i++)
            {
				var vert1 = mesh.vertices[vertIndices[i]];
				var vert2 = mesh.vertices[vertIndices[i % 3 == 2 ? i - 2 : i + 1]];

				dest[i*2] = transformMatrix * new Vector4(vert1.x, vert1.y, vert1.z, 1);
				dest[i * 2+1] = transformMatrix * new Vector4(vert2.x, vert2.y, vert2.z, 1);

                // coroutine stuff
                batchIdx++;
                if (batchIdx < CoroutineBatchCount) continue;
                yield return null;
                batchIdx = 0;
            }
        }

		public static IEnumerator LoadVerts(Mesh mesh, Vector3[] dest, Matrix4x4 transformMatrix)
        {
			uint count = (uint)Mathf.Min(mesh.vertices.Length, dest.Length);
         
			// coroutine stuff
            uint batchIdx = 0;
         
			for (int i = 0; i < count; i++)
            {
                dest[i] = transformMatrix * new Vector4(mesh.vertices[i].x, mesh.vertices[i].y, mesh.vertices[i].z, 1);

				// coroutine stuff
				batchIdx++;
				if (batchIdx < CoroutineBatchCount) continue;
                yield return null;
                batchIdx = 0;
            }
        }
      
        public static Vector3[] GetVerts(Mesh mesh, Matrix4x4 transformMatrix)
        {
            Vector3[] data = new Vector3[mesh.vertices.Length];
			LoadVerts(mesh, data, transformMatrix);
            return data;
        }

		public static Vector3[] GetTriangleVerts(Mesh mesh, Matrix4x4 transformMatrix)
        {
			Vector3[] data = new Vector3[mesh.GetTriangles(0).Length];
			LoadTriangleVerts(mesh, data, transformMatrix);
            return data;
        }
    }
}
