using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
    public class ForcesCollector : MonoBehaviour
    {
		private static class ShaderProps {
			public const string Kernel = "CSForces";
			public const string positions_buf = "positions_buf";
			public const string forces_buf = "forces_buf";
			public const string ResolutionX = "ResolutionX";
			public const string DeltaTime = "DeltaTime";
		}
      
		public ComputeBufferFacade ForcesFacade;
		public ComputeBufferFacade PositionsFacade;
		public ComputeShader Shader;
		public Forces.Force[] Forces;

		private int Kernel;
        private Vector2Int ThreadSize = new Vector2Int(4, 4);
        private uint count_ = 0;
        private Vector2Int UnitSize;

#if UNITY_EDITOR
        [Header("Read-Only")]
        public int ForcesCount = 0;
		public Vector2 Res;
		public Vector2 UnitRes;
#endif

		private void Start()
        {
            this.Kernel = this.Shader.FindKernel(ShaderProps.Kernel);
        }
      
        void Update()
        {
			var forces_buf = ForcesFacade.GetValid();
			var positions_buf = PositionsFacade.GetValid();

			if (forces_buf != null && positions_buf != null)
			{
				if (forces_buf.count != positions_buf.count)
				{
					Debug.LogWarning("ForcesCollector: positions buffer count doesn't match forces buffer count");
				} else {
                    // Collect
					foreach (var force in Forces)
					{
						force.Apply(forces_buf, positions_buf);
					}

					this.Apply(forces_buf, positions_buf);               
                }            
			}
		}

		public void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf)
        {
			// Update our UnitSize to match the number of verts in our vertBuf
            // (when multiplied with ThreadSize, which should match the values in the shader)
            if (count_ != forces_buf.count)
            {
                this.count_ = (uint)forces_buf.count;
                Vector2Int resolution = new ThreadSize(this.count_, (uint)ThreadSize.x, (uint)ThreadSize.y).Resolution;
                uint count = (uint)resolution.x * (uint)resolution.y;
                this.UnitSize = new ThreadSize(count, (uint)ThreadSize.x, (uint)ThreadSize.y).UnitSize;

#if UNITY_EDITOR
                // Update info in Unity editor for debugging...
                this.ForcesCount = (int)count;
				this.Res = resolution;
				this.UnitRes = this.UnitSize;
#endif
            }

            // This method should be overwritten by child classes
            this.Shader.SetBuffer(Kernel, ShaderProps.positions_buf, positions_buf);
            this.Shader.SetBuffer(Kernel, ShaderProps.forces_buf, forces_buf);         
			this.Shader.SetInt(ShaderProps.ResolutionX, this.UnitSize.x * this.ThreadSize.x);
			this.Shader.SetFloat(ShaderProps.DeltaTime, Time.deltaTime);
            this.Shader.Dispatch(Kernel, UnitSize.x, UnitSize.y, 1);     
		}
    }
}