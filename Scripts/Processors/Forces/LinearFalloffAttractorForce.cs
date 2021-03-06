﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.Forces
{
	public class LinearFalloffAttractorForce : Force
    {
		private static class ShaderProps {
			public const string Kernel = "CSForce";

			public const string positions_buf = "positions_buf";
			public const string forces_buf = "forces_buf";

			public const string PositionsCount = "PositionsCount";

			public const string Strength = "Strength";
			public const string Additive = "Additive";
			public const string ResolutionX = "ResolutionX";
			public const string Origin = "Origin";
			public const string ZeroDist = "ZeroDist";
		}

		public ComputeShader Shader;
		public Transform OriginTransform;
		public Vector3 Origin;
		public float Strength = 9.81f;
		public bool Additive = true;
		public float ZeroDist = 1.0f;

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
      
		override public void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf)
        {
			if (OriginTransform != null) {
				this.Origin = OriginTransform.position;
			}
         
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

			this.Shader.SetBuffer(Kernel, ShaderProps.positions_buf, positions_buf);
			this.Shader.SetBuffer(Kernel, ShaderProps.forces_buf, forces_buf);
			this.Shader.SetInt(ShaderProps.PositionsCount, positions_buf.count);
			this.Shader.SetFloat(ShaderProps.Strength, this.Strength);
			this.Shader.SetFloat(ShaderProps.ZeroDist, this.ZeroDist);
            this.Shader.SetInt(ShaderProps.ResolutionX, this.ThreadSize.x * this.UnitSize.x);
			this.Shader.SetBool(ShaderProps.Additive, this.Additive);
			this.Shader.SetVector(ShaderProps.Origin, this.Origin);
            this.Shader.Dispatch(Kernel, UnitSize.x, UnitSize.y, 1);
		}
    }
}