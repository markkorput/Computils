using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Selectors
{
	public class InfinitePlaneSelector : MonoBehaviour
	{
		private static class ShaderProps {
			public const string Kernel = "CSLoadSelectFactors";
			public const string positions_buf = "positions_buf";
			public const string factors_buf = "factors_buf";         
			public const string ResolutionX = "ResolutionX";
			public const string AlignMat = "AlignMat";
			public const string Falloff = "Falloff";
		}
      
		public ShaderRunner Runner;
		public ComputeBufferFacade Positions;
		[Tooltip("A buffer of single float values that should have the same length (or longer) as the Positions buffer and will receive a normalised the selector factor (0.0-1.0) for each position")]
		public ComputeBufferFacade Factors;
      
		public Transform PlaneTransform;
		public float Falloff;

		void Start()
		{
			Runner.Setup(ShaderProps.Kernel, 4, 4, ShaderProps.ResolutionX);
		}

		void Update()
		{
			var posbuf = this.Positions.GetValid();
			var facbuf = this.Factors.GetValid();
			if (posbuf == null || facbuf == null) return;
			this.UpdateSelectFactors(posbuf, facbuf);
		}

		public void UpdateSelectFactors(ComputeBuffer positions, ComputeBuffer factors) {
			Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.factors_buf, factors);
			Runner.Shader.SetMatrix(ShaderProps.AlignMat, this.PlaneTransform.worldToLocalMatrix);
			Runner.Shader.SetFloat(ShaderProps.Falloff, this.Falloff);
			Runner.Run(positions, ShaderProps.positions_buf);
		}
	}
}