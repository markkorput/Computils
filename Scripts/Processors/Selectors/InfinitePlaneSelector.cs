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
			public const string CloseValue = "CloseValue";
			public const string FarValue = "FarValue";
			public const string Additive = "Additive";
		}
      
		public ShaderRunner Runner;
		public Vector2Int ThreadSize = new Vector2Int(4,4);
		public ComputeBufferFacade Positions;
		[Tooltip("A buffer of single float values that should have the same length (or longer) as the Positions buffer and will receive a normalised the selector factor (0.0-1.0) for each position")]
		public ComputeBufferFacade Factors;
      
		public Transform PlaneTransform;
		public float Falloff;
		public float CloseValue = 0.0f;
		public float FarValue = 1.0f;
		public bool Additive = false;
		public bool OnUpdate = true;

		void Start()
		{
			Runner.Setup(ShaderProps.Kernel, (uint)ThreadSize.x, (uint)ThreadSize.y, ShaderProps.ResolutionX);
		}

		void Update()
		{
			if (OnUpdate) Apply();
		}

		public void Apply() {
			var posbuf = this.Positions.GetValid();
			var facbuf = this.Factors.GetValid();
			if (posbuf == null || facbuf == null) return;
			this.UpdateSelectFactors(posbuf, facbuf);
		}

		public void UpdateSelectFactors(ComputeBuffer positions, ComputeBuffer factors) {
			Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.factors_buf, factors);
			Runner.Shader.SetMatrix(ShaderProps.AlignMat, this.PlaneTransform.worldToLocalMatrix);
			Runner.Shader.SetFloat(ShaderProps.Falloff, this.Falloff);
			Runner.Shader.SetFloat(ShaderProps.CloseValue, this.CloseValue);
			Runner.Shader.SetFloat(ShaderProps.FarValue, this.FarValue);
			Runner.Shader.SetBool(ShaderProps.Additive, this.Additive);
			Runner.Run(positions, ShaderProps.positions_buf);
		}
	}
}