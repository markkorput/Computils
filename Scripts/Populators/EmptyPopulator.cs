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
		public ComputeBufferFacade AmountFacade;
		public int Amount = 10000;
		public Vector3 StartValue;

		private void OnEnable()
        {
            Vector3[] verts = null;


			if (AmountFacade == null)
			{
				this.Populate(this.Amount);
			} else {
				this.AmountFacade.GetValidAsync().Then((amountbuf) =>
				{
					this.Amount = amountbuf.count;
					this.Populate(this.Amount);
				}).Done();
			}
        }
      
		private static Vector3[] GetVerts(int amount, Vector3 val) {
			Vector3[] verts = new Vector3[amount];
			for (int i = 0; i < amount; i++) verts[i] = val;
			return verts;
		}

		public void Populate(int amount) {
			var verts = GetVerts(amount, this.StartValue);
            ComputeBuffer buf = Utils.Create(verts);
            Facade.Set(buf);         
		}
    }
}
