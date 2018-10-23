#if UNITY_EDITOR
#define BOUNDING_BOX
#endif

using UnityEngine;
using System.Collections;

namespace Computils.Populators
{
	[AddComponentMenu("Computils/Populators/Floats Buffer Populator")]
	class FloatPopulator : MonoBehaviour
	{
		public ComputeBufferFacade Facade;
		[Tooltip("Optional; source buffer who's amount we'll match (has priority over our Amount attribute")]
		public ComputeBufferFacade AmountFacade;
		public int Amount = 10000;
		public float StartMinValue = 0;
		public float StartMaxValue = 0;

		private void OnEnable()
		{
			if (AmountFacade != null)
			{
				AmountFacade.GetAsync().Then((amountbuf) =>
				{
					this.Amount = amountbuf.count;
					this.Populate(this.Amount, this.StartMinValue, this.StartMaxValue);
				}).Done();
			}
			else
			{
				this.Populate(this.Amount, this.StartMinValue, this.StartMaxValue);
			}
		}

		private void Populate(int amount, float valmin, float valmax)
		{
			float[] data = GetData(amount, this.StartMinValue, this.StartMaxValue);
			var buf = Facade.GetValid();
			buf = Utils.UpdateOrCreate(buf, data);
			//ComputeBuffer buf = Utils.Create(data);         
			Facade.Set(buf);
		}
      
		private static float[] GetData(int amount, float valmin, float valmax)
		{
			float[] verts = new float[amount];
			var rnd = new System.Random();

			for (int i = 0; i < amount; i++)
			{
				verts[i] = Mathf.Lerp(valmin, valmax, (float)rnd.NextDouble());
			}

			return verts;
		}

		#region Public Methods
		public void Populate()
		{
			this.Populate(this.Amount, this.StartMaxValue, this.StartMinValue);
		}
		#endregion
	}
}
