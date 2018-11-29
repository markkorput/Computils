using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.Forces
{
    public class TargetsForce : Force
    {
		private static class ShaderProps {
			public const string Kernel = "CSTargets";

			public const string positions_buf = "positions_buf";
			public const string forces_buf = "forces_buf";
			public const string targets_buf = "targets_buf";
			public const string TargetsToWorldMatrix = "TargetsToWorldMatrix";
			public const string LinearFalloffRange = "LinearFalloffRange";
			public const string CubicFalloffRange = "CubicFalloffRange";
         
			public const string PositionsCount = "PositionsCount";
         
			public const string Strength = "Strength";
			public const string MinDistance = "MinDistance";

			public const string Additive = "Additive";
			public const string ResolutionX = "ResolutionX";
		}

		public ComputeBufferFacade TargetsFacade;
		[Tooltip("Optional; if set, this Transform's localToWorldMatrix is used to multiply each value in the TargetsFacade's ComputeBuffer")]
		public Transform TargetsParent;
		public ComputeShader Shader;
		public float Strength = 9.81f;
		public float MinDistance = 0.1f;
		[Tooltip("Optional, equal than or lower than zero; it's disabled, otherwise it specified the distance at which the strength of a the force of a target is zero")]
		public float LinearFalloffRange = -1.0f;
		public float CubicFalloffRange = -1;
		public bool Additive = false;      

		private int Kernel;
        private Vector2Int ThreadSize = new Vector2Int(4, 4);
        private uint count_ = 0;
        private Vector2Int UnitSize;

#if UNITY_EDITOR
        [Header("Read-Only")]
        public int Count = 0;
		public Vector2 Res;
		public Vector2 UnitRes;
#endif
      
		private void Start()
        {
            this.Kernel = this.Shader.FindKernel(ShaderProps.Kernel);         
        }

		override public void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf)
        {
			var targets_buf = (this.TargetsFacade == null ? null : this.TargetsFacade.GetValid());
			if (targets_buf == null) return;

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
				this.Count = (int)count;
				this.Res = resolution;
				this.UnitRes = this.UnitSize;
#endif
            }

			this.Shader.SetBuffer(Kernel, ShaderProps.positions_buf, positions_buf);
			this.Shader.SetBuffer(Kernel, ShaderProps.forces_buf, forces_buf);
			this.Shader.SetBuffer(Kernel, ShaderProps.targets_buf, targets_buf);
			this.Shader.SetMatrix(ShaderProps.TargetsToWorldMatrix, this.TargetsParent != null ? this.TargetsParent.localToWorldMatrix : Matrix4x4.identity);
			this.Shader.SetInt(ShaderProps.PositionsCount, positions_buf.count);
			this.Shader.SetFloat(ShaderProps.Strength, this.Strength);
			this.Shader.SetFloat(ShaderProps.LinearFalloffRange, this.LinearFalloffRange);
			this.Shader.SetFloat(ShaderProps.CubicFalloffRange, this.CubicFalloffRange);
			this.Shader.SetFloat(ShaderProps.MinDistance, this.MinDistance);
            this.Shader.SetInt(ShaderProps.ResolutionX, this.ThreadSize.x * this.UnitSize.x);
			this.Shader.SetBool(ShaderProps.Additive, this.Additive);
            this.Shader.Dispatch(Kernel, UnitSize.x, UnitSize.y, 1);
		}
    }
}