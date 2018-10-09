using UnityEngine;
using UnityEngine.Events;

namespace Computils.Generic {
	/// <summary>
    /// A simple wrapper around any type of object to be represented as a unity component
    /// </summary>
	public class Facade<Typ> : MonoBehaviour
    {
        protected Typ inst;

		public UnityEvent SetEvent;
		public UnityEvent ChangeEvent;

        public Typ Get() {
          return inst;
        }
      
        public void Set(Typ val)
        {
			bool change = !AreEqual(val, this.inst);
            this.inst = val;
         
            // invoke events
            this.SetEvent.Invoke();
			if (change) this.ChangeEvent.Invoke();
        }
      
		protected static bool AreEqual(Typ a, Typ b) {
			return (a == null && b == null) || (a != null && b != null && a.Equals(b));
		}
    }
}
