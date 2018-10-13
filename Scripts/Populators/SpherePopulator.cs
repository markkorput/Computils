#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Sphere Verts Buffer Populator")]
    class SpherePopulator : MonoBehaviour
    {
        public ComputeBufferFacade Facade;
		public int Amount = 10000;
		public Vector3 Center;
		public float Radius = 1.0f;

		private void OnEnable()
        {
            Vector3[] verts = null;

			verts = GetVerts(Amount, this.Center, this.Radius);
            ComputeBuffer buf = Utils.Create(verts);
            Facade.Set(buf);
        }
      
		private static Vector3[] GetVerts(int amount, Vector3 center, float radius) {
			Vector3[] verts = new Vector3[amount];
			var rnd = new System.Random();
         
			for (int i = 0; i < amount; i++) verts[i] = center + new Vector3(
				Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble()),
				Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble()),
				Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble())
			).normalized * radius * (float)rnd.NextDouble();
         
			return verts;
		}
    }
}
