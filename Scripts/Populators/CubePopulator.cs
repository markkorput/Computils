#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Cube Verts Buffer Populator")]
	public class CubePopulator : MonoBehaviour, IPopulator
	{
		public ComputeBufferFacade Facade;
		[Tooltip("Optional; source buffer who's amount we'll match (has priority over our Amount attribute")]
		public ComputeBufferFacade AmountFacade;
		public int Amount = 10000;
		public bool SpawnOnEnable = true;
		public Vector3 Center;
		public Vector3 Dimensions;
      
		private uint offset = 0;

		private void OnEnable()
		{
			if (!this.SpawnOnEnable) return;

			if (AmountFacade != null)
			{
				AmountFacade.GetValidAsync((amountbuf) =>
				{
					this.Amount = amountbuf.count;
					this.Populate(GetVerts(this.Amount, this.Center, this.Dimensions));
				});
			}
			else
			{
				this.Populate(GetVerts(this.Amount, this.Center, this.Dimensions));
			}
		}
      
		public static Vector3[] GetVerts(int amount, Vector3 center, Vector3 size)
		{
			Vector3[] verts = new Vector3[amount];
			var rnd = new System.Random();

			for (int i = 0; i < amount; i++) verts[i] = new Vector3(
				center.x + Mathf.Lerp(size.x * -0.5f, size.x * 0.5f, (float)rnd.NextDouble()),
				center.y + Mathf.Lerp(size.y * -0.5f, size.y * 0.5f, (float)rnd.NextDouble()),
				center.z + Mathf.Lerp(size.z * -0.5f, size.z * 0.5f, (float)rnd.NextDouble()));

			return verts;
		}

		private void Populate(Vector3[] data, int offset=0)
		{
			var buf = Facade.GetValid();         
			buf = Utils.UpdateOrCreate(buf, data, offset);
			Facade.Set(buf);
		}
      
		#region Public Action Methods
		public void Populate(int amount)
		{
			var verts = GetVerts(amount, this.Center, this.Dimensions);
			Populate(verts);
		}
      
		public void Populate(int amount, int offset)
        {
			var verts = GetVerts(amount, this.Center, this.Dimensions);
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
