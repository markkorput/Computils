using UnityEngine;

namespace Computils {
	[AddComponentMenu("Computils/Compute Buffer Facade")]
	public class ComputeBufferFacade : Generic.Facade<ComputeBuffer>, System.IDisposable
    {

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
		public ComputeBuffer GetValid()
        {
          return this.inst != null && this.inst.IsValid() ? this.inst : null;
        }
      
		public void Dispose() {
			if (this.inst == null) return;
			this.inst.Release();
			this.inst.Dispose();
			this.inst = null;         
		}
    }
}
