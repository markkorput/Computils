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
		public int Amount = 10000;
		public float StartMinValue = 0;
		public float StartMaxValue = 0;

		private void OnEnable()
        {
			float[] data = GetData(Amount, this.StartMinValue, this.StartMaxValue);
			ComputeBuffer buf = Utils.Create(data);
            Facade.Set(buf);
        }
      
		private static float[] GetData(int amount, float valmin, float valmax) {
			float[] verts = new float[amount];
			var rnd = new System.Random();

			for (int i = 0; i < amount; i++)
			{
				verts[i] = Mathf.Lerp(valmin, valmax, (float)rnd.NextDouble());
			}
         
			return verts;
		}
    }
}
