﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.Forces
{
    public class GravityForce : Force
    {
		private static class ShaderProps {
			public const string Kernel = "CSForceTo";

			public const string positions_buf = "positions_buf";
			public const string forces_buf = "forces_buf";

			public const string PositionsCount = "PositionsCount";
			public const string Strength = "Strength";
			public const string Additive = "Additive";
			public const string ResolutionX = "ResolutionX";
			public const string Origin = "Origin";
		}
      
		public ComputeBufferFacade Positions;
		public ComputeShader Shader;      
      
		public float Strength = 9.81f;
		public Vector3 Origin;
		public bool Additive = false;
      
		private int Kernel;
        private Vector2Int ThreadSize = new Vector2Int(4, 4);
        private uint count_ = 0;
        private Vector2Int UnitSize;

#if UNITY_EDITOR
        [Header("Read-Only")]
        public int ForcesCount = 0;
#endif
      
		private void Start()
        {
            this.Kernel = this.Shader.FindKernel(ShaderProps.Kernel);
        }
      
		public new void Apply(ComputeBuffer buffer) {
			var posBuf = this.Positions.GetValid();
			if (posBuf == null) return;

			// Update our UnitSize to match the number of verts in our vertBuf
            // (when multiplied with ThreadSize, which should match the values in the shader)
            if (count_ != buffer.count)
            {
				this.count_ = (uint)buffer.count;
                Vector2Int resolution = new ThreadSize(this.count_, (uint)ThreadSize.x, (uint)ThreadSize.y).Resolution;
                uint count = (uint)resolution.x * (uint)resolution.y;
                this.UnitSize = new ThreadSize(count, (uint)ThreadSize.x, (uint)ThreadSize.y).UnitSize;

#if UNITY_EDITOR
                // Update info in Unity editor for debugging...
				this.ForcesCount = (int)count;
#endif
            }

			// This method should be overwritten by child classes
			this.Shader.SetBuffer(Kernel, ShaderProps.positions_buf, posBuf);
			this.Shader.SetBuffer(Kernel, ShaderProps.forces_buf, buffer);
			this.Shader.SetInt(ShaderProps.PositionsCount, posBuf.count);
			this.Shader.SetFloat(ShaderProps.Strength, this.Strength);
            this.Shader.SetInt(ShaderProps.ResolutionX, this.ThreadSize.x * this.UnitSize.x);
			this.Shader.SetBool(ShaderProps.Additive, this.Additive);
			this.Shader.SetVector(ShaderProps.Origin, this.Origin);
            this.Shader.Dispatch(Kernel, UnitSize.x, UnitSize.y, 1);         
		}
    }
}