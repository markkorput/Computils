using UnityEngine;
using System.Collections;

namespace Computils.Processors
{
	[AddComponentMenu("Computils/Processors/EaseTo")]
	public class EaseTo : MonoBehaviour
	{
		private static class ShaderProps
        {
			public const string Kernel = "CSEaseTo";
            public const string vertBuffer = "vertBuffer";
            public const string targetsBuffer = "targetsBuffer";
			public const string TargetsCount = "TargetsCount";
			public const string EaseFactor = "EaseFactor";
            public const string ResolutionX = "ResolutionX";
			public const string AnimateAll = "AnimateAll";
        }

        public ComputeShader Shader;
        public Computils.ComputeBufferFacade VertsFacade;
        public Computils.ComputeBufferFacade TargetsFacade;
        public float EaseFactor = 0.05f;
		[Tooltip("When true; will update all verts, also when there are more verts than targets")]
		public bool AnimateAll = true;

        private int Kernel;
        private Vector2Int ThreadSize = new Vector2Int(4, 4);
        private uint _count = 0;
        private Vector2Int UnitSize;
      
#if UNITY_EDITOR
        [Header("Read-Only")]
        public int VectorCount = 0;
#endif
      
        private void Start()
        {
            this.Kernel = this.Shader.FindKernel(ShaderProps.Kernel);
        }

        private void Update()
        {
			var vertBuf = this.VertsFacade == null ? null : this.VertsFacade.Get();
			var targetsBuf = this.TargetsFacade == null ? null : this.TargetsFacade.Get();

			if (vertBuf == null || targetsBuf == null) return; // TODO; log something?         
         
			// Update our UnitSize to match the number of verts in our vertBuf
			// (when multiplied with ThreadSize, which should match the values in the shader)
            if (_count != vertBuf.count)
            {
				this._count = (uint)vertBuf.count;
                Vector2Int resolution = new ThreadSize(this._count, (uint)ThreadSize.x, (uint)ThreadSize.y).Resolution;
                uint vertcount = (uint)resolution.x * (uint)resolution.y;
                this.UnitSize = new ThreadSize(vertcount, (uint)ThreadSize.x, (uint)ThreadSize.y).UnitSize;

#if UNITY_EDITOR
                // Update info in Unity editor for debugging...
                this.VectorCount = (int)vertcount;
#endif
            }

            this.Shader.SetBuffer(Kernel, ShaderProps.vertBuffer, this.VertsFacade.Get());
            this.Shader.SetBuffer(Kernel, ShaderProps.targetsBuffer, this.TargetsFacade.Get());
			this.Shader.SetInt(ShaderProps.TargetsCount, targetsBuf.count);
			this.Shader.SetFloat(ShaderProps.EaseFactor, this.EaseFactor);
            this.Shader.SetInt(ShaderProps.ResolutionX, this.ThreadSize.x * this.UnitSize.x);
			this.Shader.SetBool(ShaderProps.AnimateAll, this.AnimateAll);
            this.Shader.Dispatch(Kernel, UnitSize.x, UnitSize.y, 1);
        }
	}
}