using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
	[AddComponentMenu("Computils/Porcessors/UnshiftAndAppendFloat")]
	public class UnshiftAndAppendFloat : MonoBehaviour
	{
		private static class ShaderProps
		{
			public const string KernelSingle = "CSUnshiftAndAppendSingleRun";
			public const string KernelThreaded = "CSUnshiftAndAppendThreaded";
			public const string ValuesBuf = "valuesBuffer";
			public const string TargetBuf = "targetBuffer";

			public const string ResolutionX = "ResolutionX";
			public const string Count = "Count";
			public const string NewValue = "NewValue";
		}
      
		public ComputeBufferFacade Values;
		public ShaderRunner Runner;
		[Tooltip("Will optimize for speed at the cost of Memory; creates a second ComputeBuffer with the same size and swaps them back and forth")]
		public bool MultiThread = false;
		public bool OnUpdate = true;
		[RangeAttribute(0, 1)]
		public float NextValue = 0.5f;
		private int ThreadedKernel;
		private ComputeBuffer copyBuf;
		private bool threaded_ = false;

		void Start()
		{
			this.threaded_ = this.MultiThread; // can't change after initialization
         
			if (this.threaded_) {
				Runner.Setup(ShaderProps.KernelThreaded, 4, 4, ShaderProps.ResolutionX);
			} else {
				Runner.Setup(ShaderProps.KernelSingle, 1, 1);	
			}
		}

		void Update()
		{
			if (!this.OnUpdate) return;

			var buf = Values.GetValid();
			if (buf == null) return;

			if (this.threaded_)
			    this.UnshiftAndAppendThreaded(buf, this.NextValue);
			else 
				this.UnshiftAndAppendSingle(buf, this.NextValue);
		}

		private void UnshiftAndAppendSingle(ComputeBuffer buf, float newval)
		{
			Runner.Shader.SetInt(ShaderProps.Count, buf.count);
			Runner.Shader.SetFloat(ShaderProps.NewValue, newval);
			Runner.RunOnce(buf, ShaderProps.ValuesBuf);
		}         

		private void UnshiftAndAppendThreaded(ComputeBuffer buf, float newval)
        {
            Runner.Shader.SetInt(ShaderProps.Count, buf.count);
            Runner.Shader.SetFloat(ShaderProps.NewValue, newval);
         
			if (this.copyBuf == null || this.copyBuf.count != buf.count) copyBuf = new ComputeBuffer(buf.count, buf.stride);
            Runner.Shader.SetBuffer(Runner.Kernel, ShaderProps.TargetBuf, copyBuf);

            Runner.Run(buf, ShaderProps.ValuesBuf);
         
            // swap
            this.Values.Set(copyBuf);
            copyBuf = buf;
        }

		//public void Unshift(ComputeBuffer buf) {
		//	Runner.Shader.SetInt(ShaderProps.Count, buf.count);         
		//	Runner.Run(buf, ShaderProps.ValuesBuf);
		//}

		//public void Append(ComputeBuffer buf, float newval)
		//     {
		//Runner.Shader.SetInt(ShaderProps.Count, buf.count);
		//         Runner.Shader.SetFloat(ShaderProps.NewValue, newval);
		//Runner.Shader.SetBuffer(this.AppendKernel, ShaderProps.ValuesBuf, buf);
		//Runner.Shader.Dispatch(this.AppendKernel, 1, 1, 1);
		//}

		#region Public Methods
		public void SetValue(float v) { NextValue = v; }
		public void SetOnUpdate(bool v) { this.OnUpdate = v; }
      
		public void UnshiftAndAppend(ComputeBuffer buffer, float newval) {
			if (this.threaded_)
                this.UnshiftAndAppendThreaded(buffer, newval);
            else
                this.UnshiftAndAppendSingle(buffer, newval);
		}
		#endregion
	}
}