using System;
using System.Collections.Generic;
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

		private List<System.Action<Typ>> initCallbacks = null;

		public Typ Get() {
			return inst;
		}

		public void Set(Typ val)
		{
			bool wasNull = this.inst == null;
			bool change = !AreEqual(val, this.inst);
						this.inst = val;

						// invoke events
						this.SetEvent.Invoke();
			if (change) this.ChangeEvent.Invoke();
					
			if (wasNull && initCallbacks != null) {
				foreach (var func in initCallbacks) func.Invoke(this.inst);
				initCallbacks.Clear();
			}
		}
	
		protected static bool AreEqual(Typ a, Typ b) {
			return (a == null && b == null) || (a != null && b != null && a.Equals(b));
		}

		public void GetAsync(Action<Typ> callback) {
			if (this.inst != null)
			{
				callback.Invoke(this.inst);
				return;
			}
            
			this.AddInitCallback(callback);
		}

		private void AddInitCallback(System.Action<Typ> func) {
			if (initCallbacks == null) initCallbacks = new List<System.Action<Typ>>();
			initCallbacks.Add(func);
		}
	}
}
