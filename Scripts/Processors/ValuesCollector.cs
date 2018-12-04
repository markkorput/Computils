using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Computils.Processors
{
	public class ValuesCollector : ValueLayers.BaseValueLayer
    {
		private static class ShaderProps {
			public const string KernelOverwrite = "CSOverwrite";
			public const string KernelAdd = "CSAdd";
			public const string KernelSub = "CSSub";         
			public const string KernelMultiply = "CSMultiply";
            
			public const string dest_buf = "dest_buf";
			public const string src_buf = "src_buf";

			public const string ResolutionX = "ResolutionX";
		}

		[System.Serializable]
		public enum BlendMode { Overwrite, Add, Subtract, Multiply };
      
		[System.Serializable]
		public class Layer {
			public ValueLayers.BaseValueLayer ValueLayer;
			public BlendMode BlendMode = BlendMode.Add;
         
			public Layer(ValueLayers.BaseValueLayer l) { ValueLayer = l; }
		}
      
		public ComputeShader ValuesShader;
		public ComputeBufferFacade ValuesFacade;
		[Tooltip("Defaults to all ValueLayers found in self and children")]
        public Layer[] Layers;

		private ComputeBuffer intermediateBuf = null;
		private Dictionary<BlendMode, ShaderRunner> blendRunners = new Dictionary<BlendMode, ShaderRunner>();
      
		private ShaderRunner GetRunner(BlendMode mode) {
			ShaderRunner runner;
			if (blendRunners.TryGetValue(mode, out runner)) return runner;
			runner = new ShaderRunner();
			string kernel = ShaderProps.KernelAdd;
			switch(mode){
				case BlendMode.Multiply: kernel = ShaderProps.KernelMultiply; break;
				case BlendMode.Overwrite: kernel = ShaderProps.KernelOverwrite; break;
				case BlendMode.Subtract: kernel = ShaderProps.KernelSub; break;               
			}
         
			runner.Shader = this.ValuesShader;
			runner.Setup(kernel, 4, 4, ShaderProps.ResolutionX);
			blendRunners.Add(mode, runner);
			return runner;
		}

		private void Start()
        {
			if (this.Layers.Length == 0) {
				this.Layers = (from comp in GetComponentsInChildren<ValueLayers.BaseValueLayer>()
							   select new Layer(comp)).ToArray();
			}
        }

		void Update()
        {
			var values_buf = this.ValuesFacade.GetValid();
			if (values_buf == null) return;
			this.Apply(values_buf);
		}
      
		public void Apply(ComputeBuffer values_buf, ComputeBuffer intermediate_buf, Layer[] layers)
        {
			foreach(var layer in layers) {
				layer.ValueLayer.Apply(intermediate_buf);
            
				var shaderRunner = GetRunner(layer.BlendMode);
				shaderRunner.Shader.SetBuffer(shaderRunner.Kernel, ShaderProps.src_buf, intermediate_buf);
				shaderRunner.Run(values_buf, ShaderProps.dest_buf);
			}
		}

		public override void Apply(ComputeBuffer values_buf) {
    	    if (intermediateBuf != null && intermediateBuf.count < values_buf.count)
            {
                intermediateBuf.Release();
                intermediateBuf.Dispose();
                intermediateBuf = null;
            }

            if (intermediateBuf == null)
            {
                intermediateBuf = new ComputeBuffer(values_buf.count, values_buf.stride);
            }

            this.Apply(values_buf, intermediateBuf, this.Layers);
		}
    }
}