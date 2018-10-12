using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.Forces
{
    public class LinearPeerForces : Force
    {
		private static class ShaderProps {
			public const string Kernel = "CSPeerForce";
         
			public const string positions_buf = "positions_buf";
			public const string forces_buf = "forces_buf";
         
			public const string Strength = "Strength";
			public const string ZeroDist = "ZeroDist";
		}
      
		public ComputeShader Shader;      
		public float Strength = 9.81f;
		public float ZeroDist = 100.0f;
      
		private int Kernel;
        private Vector2Int ThreadSize = new Vector2Int(1, 1);
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
      
		override public void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf)
        {
			if (count_ != forces_buf.count)
            {
				this.count_ = (uint)forces_buf.count;
				this.UnitSize = new Vector2Int(
					Mathf.FloorToInt((float)count_ / ThreadSize.x),
					Mathf.FloorToInt((float)count_ / ThreadSize.y));

#if UNITY_EDITOR
                // Update info in Unity editor for debugging...
				this.ForcesCount = (int)count_;
				this.Res = new Vector2(count_, count_);
				this.UnitRes = this.UnitSize;
#endif
            }

			this.Shader.SetBuffer(Kernel, ShaderProps.positions_buf, positions_buf);
			this.Shader.SetBuffer(Kernel, ShaderProps.forces_buf, forces_buf);         
			this.Shader.SetFloat(ShaderProps.Strength, this.Strength);
			this.Shader.SetFloat(ShaderProps.ZeroDist, this.ZeroDist);         
			this.Shader.Dispatch(Kernel, UnitSize.x, UnitSize.y, 1);         
		}
    }
}