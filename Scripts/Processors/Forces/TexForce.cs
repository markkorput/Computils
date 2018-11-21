using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.Forces
{
    public class TexForce : Force
    {
		private static class ShaderProps {
			public const string Kernel = "CSTexForce";
         
			public const string positions_buf = "positions_buf";
			public const string forces_buf = "forces_buf";

			public const string PositionsCount = "PositionsCount";
			public const string ResolutionX = "ResolutionX";
			public const string Additive = "Additive";


			public const string Texture = "tex";
			public const string TexCoordOrigin = "TexCoordOrigin";
			public const string TexCoordFactorX = "TexCoordFactorX";
			public const string TexCoordFactorY = "TexCoordFactorY";
			public const string TexCoordFactorZ = "TexCoordFactorZ";
		}

		public ComputeShader Shader;
		public Texture Texture;
		public Transform ParticlesParent;
		public bool Additive = false;

		[Header("Particle Position To Tex Coord")]
		public Vector2 TexCoordOrigin = new Vector2(0.5f, 0.5f);
		public Vector2 TexCoordFactorX = new Vector2(1, 0); // by default the x coord of the particle determines the x part of the tex-coord
		public Vector2 TexCoordFactorY = new Vector2(0, 0); // and the z coord of the particle determines the y part of the tex-coord
		public Vector2 TexCoordFactorZ = new Vector2(0, 1);
      
		private int Kernel;
        private Vector2Int ThreadSize = new Vector2Int(4, 4);
        private uint count_ = 0;
        private Vector2Int UnitSize;
      
#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo {
			public int Count = 0;
		}

		public Dinfo DebugInfo;
#endif
      
		private void Start()
        {
            this.Kernel = this.Shader.FindKernel(ShaderProps.Kernel);
        }
      
		override public void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf)
        {
			if (this.Texture == null) return;
         
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
				this.DebugInfo.Count = (int)count;
				//this.Res = resolution;
				//this.UnitRes = this.UnitSize;
#endif
            }
         
			this.Shader.SetBuffer(Kernel, ShaderProps.positions_buf, positions_buf);
			this.Shader.SetBuffer(Kernel, ShaderProps.forces_buf, forces_buf);
			this.Shader.SetInt(ShaderProps.PositionsCount, positions_buf.count);
			this.Shader.SetInt(ShaderProps.ResolutionX, this.ThreadSize.x * this.UnitSize.x);
            this.Shader.SetBool(ShaderProps.Additive, this.Additive);

			this.Shader.SetVector(ShaderProps.TexCoordOrigin, this.TexCoordOrigin);
			this.Shader.SetVector(ShaderProps.TexCoordFactorX, this.TexCoordFactorX);
			this.Shader.SetVector(ShaderProps.TexCoordFactorY, this.TexCoordFactorX);
			this.Shader.SetVector(ShaderProps.TexCoordFactorZ, this.TexCoordFactorX);
         
			this.Shader.SetTexture(Kernel, ShaderProps.Texture, this.Texture);
            this.Shader.Dispatch(Kernel, UnitSize.x, UnitSize.y, 1);
		}

		public void SetTexture(Texture texture) {
			this.Texture = texture;
		}
    }
}