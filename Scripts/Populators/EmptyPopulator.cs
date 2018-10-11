#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Empty Verts Buffer Populator")]
    class EmptyPopulator : MonoBehaviour
    {
        public ComputeBufferFacade Facade;
		public int Amount = 10000;
		public Vector3 StartValue;

		private void OnEnable()
        {
            Vector3[] verts = null;

			verts = GetVerts(Amount, this.StartValue);
            ComputeBuffer buf = Utils.Create(verts);
            Facade.Set(buf);
        }
      
		private static Vector3[] GetVerts(int amount, Vector3 val) {
			Vector3[] verts = new Vector3[amount];
			for (int i = 0; i < amount; i++) verts[i] = val;
			return verts;
		}
    }
}
