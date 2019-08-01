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
                AmountFacade.GetValidAsync().Then((amountbuf) =>
                {
                    this.Amount = amountbuf.count;
					this.PopulateNext(this.Amount);
                }).Done();
            }
            else
            {
				this.PopulateNext(this.Amount);
            }         
        }

		public virtual Vector3[] GetVerts(int amount) {
			return new Vector3[0]; // Override this method in child classes
		}
      
		protected void Populate(Vector3[] data, int offset = 0)
        {
            var buf = Facade.GetValid();
            buf = Utils.UpdateOrCreate(buf, data, offset);
            Facade.Set(buf);
        }

        #region Public Action Methods
        public void Populate(int amount)
        {
			var verts = GetVerts(amount);
            Populate(verts);
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
        #endregion
    }
}
