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
		[Tooltip("Required; the target buffer that we'll populate")]
		public ComputeBufferFacade Facade;
		[Tooltip("Optional; source buffer who's amount we'll match (has priority over our Amount attribute")]
		public ComputeBufferFacade AmountFacade;
		[Tooltip("This amount is ignored and will be overwritten if the AmountFacade is initialized")]
		public int Amount = 10000;

		[Header("Values Options")]
		public float StartMinValue = 0;
		public float StartMaxValue = 0;

		private void OnEnable()
		{
			if (AmountFacade != null)
			{
				AmountFacade.GetValidAsync((amountbuf) =>
				{
					this.Amount = amountbuf.count;
					this.Populate(this.Amount, this.StartMinValue, this.StartMaxValue);
				});
			}
			else
			{
				this.Populate(this.Amount, this.StartMinValue, this.StartMaxValue);
			}
		}

		private void Populate(int amount, float valmin, float valmax)
		{
			float[] data = GetData(amount, valmin, valmax);
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

		public void PopulateWithNewAmount() {
			var amountbuf = this.AmountFacade.GetValid();
			if (amountbuf == null) return;
			this.Amount = amountbuf.count;
			this.Populate(this.Amount, this.StartMaxValue, this.StartMinValue);
		}

		public static ComputeBuffer Populate(ComputeBufferFacade facade, float[] data) {
			var buf = facade.GetValid();
			buf = Utils.UpdateOrCreate(buf, data);
			facade.Set(buf);
			return buf;
		}

		#endregion
	}
}
