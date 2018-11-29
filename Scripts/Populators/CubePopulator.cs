#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Cube Verts Buffer Populator")]
    public class CubePopulator : MonoBehaviour
    {
        public ComputeBufferFacade Facade;
		[Tooltip("Optional; source buffer who's amount we'll match (has priority over our Amount attribute")]
        public ComputeBufferFacade AmountFacade;
		public int Amount = 10000;
		public Vector3 Center;
		public Vector3 Dimensions;

		private void OnEnable()
        {
            if (AmountFacade != null)
            {
                AmountFacade.GetValidAsync().Then((amountbuf) =>
                {
                    this.Amount = amountbuf.count;
					this.Populate(this.Amount, this.Center, this.Dimensions);
                }).Done();
            }
            else
            {
				this.Populate(this.Amount, this.Center, this.Dimensions);
            }         
        }
      
		public static Vector3[] GetVerts(int amount, Vector3 center, Vector3 size) {
			Vector3[] verts = new Vector3[amount];
			var rnd = new System.Random();

			for (int i = 0; i < amount; i++) verts[i] = new Vector3(
				center.x + Mathf.Lerp(size.x * -0.5f, size.x * 0.5f, (float)rnd.NextDouble()),
				center.y + Mathf.Lerp(size.y * -0.5f, size.y * 0.5f, (float)rnd.NextDouble()),
				center.z + Mathf.Lerp(size.z * -0.5f, size.z * 0.5f, (float)rnd.NextDouble()));

			return verts;
		}

		private void Populate(int amount, Vector3 center, Vector3 size)
        {
			Vector3[] data = GetVerts(amount, center, size);
            var buf = Facade.GetValid();
            buf = Utils.UpdateOrCreate(buf, data);
            //ComputeBuffer buf = Utils.Create(data);         
            Facade.Set(buf);
        }      
    }
}
