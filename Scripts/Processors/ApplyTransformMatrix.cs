using UnityEngine;
using System.Collections;

namespace Computils.Processors
{
	[AddComponentMenu("Computils/Processors/ApplyTransformMatrix")]
	public class ApplyTransformMatrix : MonoBehaviour
	{
		private static class ShaderProps
        {
			public const string Kernel = "CSApplyTransformMatrix";
            public const string vertBuffer = "vertBuffer";
            public const string targetsBuffer = "targetsBuffer";
			public const string TargetsCount = "TargetsCount";
            public const string ResolutionX = "ResolutionX";
            public const string transformMatrix = "transformMatrix";
        }

        public ComputeShader Shader;
        public Computils.ComputeBufferFacade VertsFacade;
        public Computils.ComputeBufferFacade TargetsFacade;
        public Transform LocalToWorld;
        public Transform WorldToLocal;
        public bool OnUpdate = true;
        public bool AutoResize = true;

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
            if (OnUpdate) Apply();
        }


        public void Apply() {
			var vertBuf = this.VertsFacade == null ? null : this.VertsFacade.Get();
			var targetsBuf = this.TargetsFacade == null ? null : this.TargetsFacade.Get();

			if (vertBuf == null) {
                Debug.LogWarning("Source buffer is null");
                return; // TODO; log something?
            }

            if (targetsBuf == null || targetsBuf.count != vertBuf.count) {
                if (!AutoResize) {
                    Debug.LogWarning("Different buffer sizes, can't Apply Transform Matrix");
                    return;
                }

                Populators.BaseVector3Populator.Populate(this.TargetsFacade, vertBuf.count);
                targetsBuf = TargetsFacade.GetValid();
            }
         
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

            Matrix4x4 transformMatrix = 
                this.LocalToWorld != null
                    ? this.LocalToWorld.localToWorldMatrix
                    : (this.WorldToLocal != null ?
                        this.WorldToLocal.worldToLocalMatrix
                        : Matrix4x4.identity);

            this.Shader.SetBuffer(Kernel, ShaderProps.vertBuffer, this.VertsFacade.Get());
            this.Shader.SetBuffer(Kernel, ShaderProps.targetsBuffer, this.TargetsFacade.Get());
			this.Shader.SetInt(ShaderProps.TargetsCount, targetsBuf.count);
            this.Shader.SetInt(ShaderProps.ResolutionX, this.ThreadSize.x * this.UnitSize.x);
            this.Shader.SetMatrix(ShaderProps.transformMatrix, transformMatrix);
            this.Shader.Dispatch(Kernel, UnitSize.x, UnitSize.y, 1);
        }
	}
}