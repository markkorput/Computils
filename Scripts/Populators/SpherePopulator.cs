#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Sphere Verts Buffer Populator")]
    public class SpherePopulator : MonoBehaviour
    {
        public ComputeBufferFacade Facade;
		[Tooltip("Optional; source buffer who's amount we'll match (has priority over our Amount attribute")]
        public ComputeBufferFacade AmountFacade;
		public int Amount = 10000;
		public Vector3 Center;
		public float Radius = 1.0f;

		private void OnEnable()
        {
			if (AmountFacade != null)
            {
                AmountFacade.GetValidAsync().Then((amountbuf) =>
                {
                    this.Amount = amountbuf.count;
                    this.Populate(this.Amount, this.Center, this.Radius);
                }).Done();
            }
            else
            {
                this.Populate(this.Amount, this.Center, this.Radius);
            }         
        }
      
		public static Vector3[] GetVerts(int amount, Vector3 center, float radius) {
			Vector3[] verts = new Vector3[amount];
			var rnd = new System.Random();
         
			for (int i = 0; i < amount; i++) verts[i] = center + new Vector3(
				Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble()),
				Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble()),
				Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble())
			).normalized * radius * (float)rnd.NextDouble();
         
			return verts;
		}

		private void Populate(int amount, Vector3 center, float radius)
        {
            Vector3[] data = GetVerts(amount, center, radius);
            var buf = Facade.GetValid();
            buf = Utils.UpdateOrCreate(buf, data);
            //ComputeBuffer buf = Utils.Create(data);         
            Facade.Set(buf);
        }      
    }
}
