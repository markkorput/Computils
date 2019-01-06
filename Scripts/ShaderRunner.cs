using System;
using System.Collections.Generic;
using UnityEngine;

namespace Computils
{
	[System.Serializable]
	public class ShaderRunner
	{
		public class Opts
		{
			public ComputeShader ComputeShader { get; private set; }
			public string KernelName { get; private set; }
			public Vector2Int ThreadSize { get; private set; }
			public string NameResolutionX { get; private set; }
			public string MainBufName { get; private set; }
			public List<System.Action<ComputeShader, int>> ShaderConfigurators { get; private set; }
         
			public Opts() {
				this.ThreadSize = new Vector2Int(1, 1);
				this.NameResolutionX = null;
				this.KernelName = null;
				this.ComputeShader = null;
				this.ShaderConfigurators = new List<Action<ComputeShader, int>>();
				this.MainBufName = null;
			}
         
			public Opts Shader(ComputeShader shader) {
				this.ComputeShader = shader;
				return this;
			}
         
			public Opts Kernel(string kernelName) {
				this.KernelName = kernelName;
				return this;
			}

			public Opts Program(ComputeShader shader, string kernelName) {
				this.ComputeShader = shader;
				this.KernelName = kernelName;
				return this;
			}
         
			public Opts MainBufferName(string name) {
				this.MainBufName = name;
				return this;
			}
         
			public Opts Threading(uint x, uint y, string resXname = null) {
				this.ThreadSize = new Vector2Int((int)x, (int)y);
				this.NameResolutionX = resXname;
				return this;
			}

			public Opts Var(string varname, System.Func<float> func) {
				this.ShaderConfigurators.Add((shader, kernelId) =>            
					shader.SetFloat(varname, func.Invoke()));
            
				return this;
			}
         
			public Opts Var(string varname, System.Func<Vector3> func) {
				this.ShaderConfigurators.Add((shader, kernelId) =>
                    shader.SetVector(varname, func.Invoke()));
            
                return this;
            }
         
			public Opts Var(string varname, System.Func<Matrix4x4> func) {
				this.ShaderConfigurators.Add((shader, kernelId) =>
                    shader.SetMatrix(varname, func.Invoke()));
            
                return this;
            }
         
			public Opts Buffer(string bufname, System.Func<ComputeBuffer> func) {
				this.ShaderConfigurators.Add((shader, kernelId) =>
					 shader.SetBuffer(kernelId, bufname, func.Invoke()));
            
				return this;
			}

			public Opts Buffer(string bufname, ComputeBuffer buf) {
				return this.Buffer(bufname, () => buf);
			}
		}
      
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
		private Opts opts;
      
		public int Kernel { get { return kernel_; } }
		public string NameResolutionX { get { return resx_; } set { resx_ = value; }}
      
		public static ShaderRunner Create(Opts opts) {
			var runner = new ShaderRunner();
			runner.opts = opts;
            return runner;
		}

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

		public void Run(ComputeBuffer mainBuf) {
			if (this.opts == null) return; // TODO; log warning?

			// get shader         
			var shader = opts.ComputeShader;
			if (shader == null) return;
         
			// kernel
            if (opts.KernelName != null) this.kernel_ = shader.FindKernel(opts.KernelName);
			//if (this.kernel_ == null) return;

			// execution/threading details
            // update our thread count/unity size/total resolution values if necessary
            if (mainBuf.count != this.count_)
            {
                this.count_ = (uint)mainBuf.count;
				Vector2Int resolution = new ThreadSize(this.count_, (uint)opts.ThreadSize.x, (uint)opts.ThreadSize.y).Resolution;
                uint count = (uint)resolution.x * (uint)resolution.y;
				this.unitSize_ = new ThreadSize(count, (uint)opts.ThreadSize.x, (uint)opts.ThreadSize.y).UnitSize;
            
#if UNITY_EDITOR
                // Update info in Unity editor for debugging...
                this.Count = (int)count;
                this.Res = resolution;
                this.UnitRes = this.unitSize_;
#endif
            }

            if (opts.NameResolutionX != null)
                shader.SetInt(opts.NameResolutionX, this.threadSize_.x * this.unitSize_.x);

			foreach (var cfg in opts.ShaderConfigurators)
                cfg.Invoke(shader, this.kernel_);         
         
			if (this.opts.MainBufName != null) {
				shader.SetBuffer(this.kernel_, this.opts.MainBufName, mainBuf);
			}
         
			shader.Dispatch(Kernel, unitSize_.x, unitSize_.y, 1);
		}
	}
}