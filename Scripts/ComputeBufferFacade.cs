using System;
using System.Collections.Generic;
using UnityEngine;

namespace Computils {
	[AddComponentMenu("Computils/Compute Buffer Facade")]
	public class ComputeBufferFacade : Generic.Facade<ComputeBuffer>, System.IDisposable {

		#if UNITY_EDITOR
				[Header("Read-Only")]
				public int Count = 0;
		#endif

		#if UNITY_EDITOR
				new public void Set(ComputeBuffer buf) {
					base.Set(buf);
					if (buf != null) this.Count = buf.count;
				}
		#endif

		public ComputeBuffer GetValid() {
			return this.inst != null && this.inst.IsValid() ? this.inst : null;
		}

		public void Dispose() {
			if (this.inst == null) return;
			this.inst.Release();
			this.inst.Dispose();
			this.inst = null;         
		}

		public void GetValidAsync(Action<ComputeBuffer> callback) {
				var buf = this.GetValid();
            
				if (buf != null) {
					callback.Invoke(buf);
					return;
				}

				// register listener for when a buffer is set, that checks if its valid
				validAsyncResolverQueue.Enqueue(callback);
				this.SetEvent.AddListener(this.GetValidAsync_SetCallback);
		}

      
		private Queue<System.Action<ComputeBuffer>> validAsyncResolverQueue = new Queue<System.Action<ComputeBuffer>>();

		private void GetValidAsync_SetCallback() {         
			var newbuf = this.GetValid();
			if (newbuf == null) return;
         
			while(validAsyncResolverQueue.Count > 0) {
				validAsyncResolverQueue.Dequeue().Invoke(newbuf);
			}

			this.SetEvent.RemoveListener(this.GetValidAsync_SetCallback);
		}
	}
}
