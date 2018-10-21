using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
	[AddComponentMenu("Computils/Porcessors/UnshiftAndAppendFloat")]
	public class UnshiftAndAppendFloat : MonoBehaviour
	{
		private static class ShaderProps {
			public const string UnshiftKernel = "CSUnshift";
			public const string AppendKernel = "CSAppend";
			public const string ValuesBuf = "valuesBuffer";
			public const string ResolutionX = "ResolutionX";

			public const string Count = "Count";
			public const string NewValue = "NewValue";
		}
      
		public ComputeBufferFacade Values;
		public ShaderRunner Runner;
		public bool OnUpdate = true;
        [RangeAttribute(0,1)]
		public float NextValue = 0.5f;
      

		private int AppendKernel;

		void Start()
		{
			Runner.Setup(ShaderProps.UnshiftKernel, 1, 1);
			Runner.NameResolutionX = ShaderProps.ResolutionX;
         
			AppendKernel = Runner.Shader.FindKernel(ShaderProps.AppendKernel);
		}

		void Update()
		{
			if (!this.OnUpdate) return;
         
			var buf = Values.GetValid();         
			if (buf == null) return;

			this.Unshift(buf);
			this.Append(buf, this.NextValue);
		}
      
		public void Unshift(ComputeBuffer buf) {
			Runner.Shader.SetInt(ShaderProps.Count, buf.count);         
			Runner.Run(buf, ShaderProps.ValuesBuf);
		}

		public void Append(ComputeBuffer buf, float newval)
        {
			Runner.Shader.SetInt(ShaderProps.Count, buf.count);
            Runner.Shader.SetFloat(ShaderProps.NewValue, newval);
			Runner.Shader.SetBuffer(this.AppendKernel, ShaderProps.ValuesBuf, buf);
			Runner.Shader.Dispatch(this.AppendKernel, 1, 1, 1);
        }
      
		public void SetValue(float v) { NextValue = v; }
		public void SetOnUpdate(bool v) { this.OnUpdate = v; }      
	}
}