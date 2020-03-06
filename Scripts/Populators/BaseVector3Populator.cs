using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	public class BaseVector3Populator : MonoBehaviour, IPopulator
    {
        public ComputeBufferFacade Facade;
		[Tooltip("Optional; source buffer who's amount we'll match (has priority over our Amount attribute")]
        public ComputeBufferFacade AmountFacade;
		public bool SpawnOnEnable = true;
		public int Amount = 10000;
      
		protected uint offset = 0;

		private void OnEnable()
        {
			if (!this.SpawnOnEnable) return;
         
			if (AmountFacade != null)
            {
                AmountFacade.GetValidAsync((amountbuf) =>
                {
					this.PopulateNext(amountbuf.count);
                });
            }
            else
            {
				this.PopulateNext(this.Amount);
            }         
        }

		public virtual Vector3[] GetVerts(int amount) {
			return new Vector3[amount]; // Override this method in child classes
		}
      
		protected void Populate(Vector3[] data, int offset = 0)
        {
            BaseVector3Populator.Populate(Facade, data, offset);
            this.Amount = data.Length;
        }

        #region Public Action Methods
        public void Populate(int amount)
        {
			var verts = GetVerts(amount);
            Populate(verts);
        }

        public static void Populate(ComputeBufferFacade facade, Vector3[] data, int offset=0) {
            var buf = facade.GetValid();
            buf = Utils.UpdateOrCreate(buf, data, offset);
            facade.Set(buf);
        }

        public static void Populate(ComputeBufferFacade facade, int amount) {
            Populate(facade, new Vector3[amount]);
        }

        public void Populate(int amount, int offset)
        {
			var verts = GetVerts(amount);
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

        public void PopulateWithDefaultAmount() {
            var c = this.AmountFacade.GetValid().count;
            if (c == 0) {
                Debug.LogWarning("Won't populate with zero length");
                return;
            }

            Populate(c);
        }

        public void EnforeAmount(int count) {
            var buf = this.Facade.GetValid();
            if (buf == null || buf.count == count) return;
            Populate(count);
        }

        public void EnforeFacadeAmount() {
            this.EnforeAmount(this.AmountFacade.GetValid().count);
        }
        #endregion
    }
}
