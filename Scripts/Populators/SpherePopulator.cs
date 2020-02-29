#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Sphere Verts Buffer Populator")]
	public class SpherePopulator : MonoBehaviour, IPopulator
    {
        public ComputeBufferFacade Facade;
		[Tooltip("Optional; source buffer who's amount we'll match (has priority over our Amount attribute")]
        public ComputeBufferFacade AmountFacade;
		public bool SpawnOnEnable = true;
		public int Amount = 10000;
		public Vector3 Center;
		public float Radius = 1.0f;
		public float MinRadius = 0.0f;
      
		private uint offset = 0;

		private void OnEnable()
        {
			if (!this.SpawnOnEnable) return;
         
			if (AmountFacade != null)
            {
                AmountFacade.GetValidAsync((amountbuf) =>
                {
									this.Amount = amountbuf.count;
									this.PopulateNext(this.Amount);
                });
            }
            else
            {
				this.PopulateNext(this.Amount);
            }         
        }

		public static Vector3[] GetVerts(int amount, Vector3 center, float maxradius = 1.0f, float minRadius = 0.0f) {
			Vector3[] verts = new Vector3[amount];
			var rnd = new System.Random();

			float deltaRadius = (maxradius - minRadius);

			for (int i = 0; i < amount; i++)
			{
				float radius = minRadius + deltaRadius * (float)rnd.NextDouble();

				verts[i] = center + new Vector3(
					Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble()),
					Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble()),
					Mathf.Sin(Mathf.PI * 2.0f * (float)rnd.NextDouble())
				).normalized * radius;
			}

			return verts;
		}
         
		private void Populate(Vector3[] data, int offset = 0)
        {
            var buf = Facade.GetValid();
            buf = Utils.UpdateOrCreate(buf, data, offset);
            Facade.Set(buf);
        }
      
        #region Public Action Methods
        public void Populate(int amount)
        {
			var verts = GetVerts(amount, this.Center, this.Radius, this.MinRadius);
            Populate(verts);
        }
      
        public void Populate(int amount, int offset)
        {
			var verts = GetVerts(amount, this.Center, this.Radius, this.MinRadius);
            Populate(verts, offset);
        }
      
		public void PopulateNext(int amount) {
			var buf = this.Facade == null ? null : this.Facade.GetValid();

			int max = buf != null ? buf.count : (amount + (int)offset);
			amount = Mathf.Min(amount, (int)(max - this.offset));
         
			this.Populate(amount, (int)this.offset);
			this.offset += (uint)amount;
			if (this.offset >= max) this.offset = 0;
		}
        #endregion
    }
}
