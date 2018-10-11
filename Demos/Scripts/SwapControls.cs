using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Demos
{
	public class SwapControls : MonoBehaviour
	{
		public Processors.EaseTo EaseTo;
		public ComputeBufferFacade[] Buffers;
		public bool ApplyAtAwake = true;
		public KeyCode SwapKey = KeyCode.Space;

		uint idx_ = 0;

		public void Start()
		{
			if (EaseTo == null) EaseTo = GetComponent<Processors.EaseTo>();
		}

		public void Swap() {
			if (Buffers.Length == 0) return;
			var buf = Buffers[idx_ % (uint)Buffers.Length];
			idx_++;
         
			EaseTo.TargetsFacade = buf;
		}

		private void OnGUI()
        {
            Event evt = Event.current;         
            if (evt.isKey && Input.GetKeyDown(evt.keyCode)) this.OnKeyDown(evt);
        }
      
        private void OnKeyDown(Event evt)
        {
			if (evt.keyCode == this.SwapKey) this.Swap();
        }
	}
}