using UnityEngine;

namespace Computils {
    public class ComputeBufferFacade : Generic.Facade<ComputeBuffer>
    {
        public ComputeBuffer GetValid()
        {
          return this.inst != null && this.inst.IsValid() ? this.inst : null;
        }
    }
}
