using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.ValueLayers
{
	public class Map : BaseValueLayer
	{
		private static class ShaderProps
		{
			public const string Kernel = "CSMap";
			public const string ResolutionX = "ResolutionX";
			public const string dest_buf = "dest_buf";
			public const string mapSource_buf = "mapSource_buf";
         
			public const string FromMin = "FromMin";
			public const string FromMax = "FromMax";
			public const string ToMin = "ToMin";
            public const string ToMax = "ToMax";
			public const string Clamp = "Clamp";
		}      

		public ComputeShader Shader;
        [Tooltip("Optional, by default use the the ComputeBuffer given by the ValuesCollector")]
		public ComputeBufferFacade SourceValuesFacade;
		public float FromMin = 0.0f;
		public float FromMax = 1.0f;
		public float ToMin = 1.0f;
		public float ToMax = 1.0f;
		public bool Clamp = false;
      
		private ShaderRunner Runner = null;

		public override void Apply(ComputeBuffer values_buf)
		{
			if (this.Runner == null) {
				this.Runner = ShaderRunner.Create(this.Shader, ShaderProps.Kernel, 4, 4, ShaderProps.ResolutionX);
			}

			var source_buf = this.SourceValuesFacade == null ? null : this.SourceValuesFacade.GetValid();
			if (source_buf == null) source_buf = values_buf;

			this.Runner.Shader.SetFloat(ShaderProps.FromMin, this.FromMin);
			this.Runner.Shader.SetFloat(ShaderProps.FromMax, this.FromMax);
			this.Runner.Shader.SetFloat(ShaderProps.ToMin, this.ToMin);
			this.Runner.Shader.SetFloat(ShaderProps.ToMax, this.ToMax);
			this.Runner.Shader.SetFloat(ShaderProps.ToMax, this.ToMax);
			this.Runner.Shader.SetBool(ShaderProps.Clamp, this.Clamp);
			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.mapSource_buf, source_buf);
			this.Runner.Run(values_buf, ShaderProps.dest_buf);
		}
	}
}