using System.Linq;
using UnityEngine;

namespace Computils.Populators
{

	[AddComponentMenu("Computils/Populators/TransformPositionsPopulator")]
    public class TransformPositionsPopulator : MonoBehaviour
    {
        public ComputeBufferFacade Facade;
        public Transform Parent;
		public bool AtStart = true;
      
        private ComputeBuffer buf = null;
		private Transform[] transforms = null;

		private void Start()
		{
			if (this.AtStart) this.Populate();
		}
      
		void Update()
        {
			this.Populate();
        }
      
		public void Populate() {
			this.transforms = LoadTansforms(this.transforms, this.Parent);
            this.buf = Utils.UpdateOrCreate(this.buf, (from t in this.transforms select t.position).ToArray());
            this.Facade.Set(buf);
		}
      
		public static Transform[] LoadTansforms(Transform[] transforms, Transform parent)
        {
            int c = parent.childCount;
         
            // make sure we have an array of the right size allocated
			if (transforms == null || transforms.Length != c) transforms = new Transform[c];
         
            for (int i = 0; i < c; i++)
            {
				transforms[i] = parent.GetChild(i).transform;
            }

			return transforms;
        }
    }
}

