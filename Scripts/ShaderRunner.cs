using System;
using UnityEngine;

namespace Computils
{
	[System.Serializable]
	public class ShaderRunner
	{
		public ComputeShader Shader;

#if UNITY_EDITOR
		[Header("Read-Only")]
		public int Count = 0;
		public Vector2 Res;
		public Vector2 UnitRes;
#endif
      
		private int kernel_;
		private Vector2Int threadSize_ = new Vector2Int(1, 1);
		private uint count_ = 0;
		private Vector2Int unitSize_;
		private string resx_ = null;
      
		public int Kernel { get { return kernel_; } }
		public string NameResolutionX { get { return resx_; } set { resx_ = value; }}
      
		public static ShaderRunner Create(ComputeShader shader, string kernelName, uint resx, uint resy, string xresName=null) {
			var runner = new ShaderRunner();
			runner.Shader = shader;
			runner.Setup(kernelName, resx, resy, xresName);
			return runner;
		}
      
		public void Setup(string kernelName, uint threadsX, uint threadsY) {
			this.threadSize_ = new Vector2Int((int)threadsX, (int)threadsY);
			this.kernel_ = this.Shader.FindKernel(kernelName);
		}

		public void Setup(string kernelName, uint threadsX, uint threadsY, String xresName) {
			this.Setup(kernelName, threadsX, threadsY);
			this.NameResolutionX = xresName;
		}

		public void Run(ComputeBuffer buf, string bufName)
		{
			// update our thread count/unity size/total resolution values if necessary
			if (buf.count != this.count_)
			{
				this.count_ = (uint)buf.count;
                Vector2Int resolution = new ThreadSize(this.count_, (uint)threadSize_.x, (uint)threadSize_.y).Resolution;
                uint count = (uint)resolution.x * (uint)resolution.y;
				this.unitSize_ = new ThreadSize(count, (uint)threadSize_.x, (uint)threadSize_.y).UnitSize;

#if UNITY_EDITOR
                // Update info in Unity editor for debugging...
                this.Count = (int)count;
                this.Res = resolution;
                this.UnitRes = this.unitSize_;
#endif            
			}
         
			this.Shader.SetBuffer(this.kernel_, bufName, buf);
         
            // IF our owner set our horizontal resolution variable name, we'll set it here
			if (this.resx_ != null) this.Shader.SetInt(this.resx_, this.threadSize_.x * this.unitSize_.x);
            // Dispatch
			this.Shader.Dispatch(Kernel, unitSize_.x, unitSize_.y, 1);
		}
      
		public void RunOnce(ComputeBuffer buf, string bufName) {
			this.Shader.SetBuffer(this.kernel_, bufName, buf);
			this.Shader.Dispatch(Kernel, 1, 1, 1);
         
#if UNITY_EDITOR
            // Update info in Unity editor for debugging...
			this.Count = buf.count;
#endif
		}
	}
}