using UnityEngine;
using System.Collections;

namespace Computils.Populators {
	public class TransformMatrixBufferPopulator : MonoBehaviour
	{
		public ComputeBufferFacade Facade;
		public Transform Parent;

		private ComputeBuffer buf = null;
		private Matrix4x4[] matrices = null;

		void Update()
		{
			this.matrices = LoadChildMatrices(this.matrices, this.Parent);
			this.buf = Utils.UpdateOrCreate(this.buf, matrices);
			this.Facade.Set(buf);
		}
      
		private static Matrix4x4[] LoadChildMatrices(Matrix4x4[] matrices, Transform parent)
		{
			int c = parent.childCount;
         
			// make sure we have an array of the right size allocated
			if (matrices == null || matrices.Length != c) matrices = new Matrix4x4[c];

			for (int i = 0; i < c; i++)
			{
				matrices[i] = parent.GetChild(i).transform.localToWorldMatrix;
			}
         
			return matrices;
		}
	}
}

