using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Selectors
{
	public class BoxSelector : MonoBehaviour
	{
		private static class ShaderProps {
			public const string Kernel = "CSLoadSelectFactors";
			public const string positions_buf = "positions_buf";
			public const string factors_buf = "factors_buf";         
			public const string ResolutionX = "ResolutionX";
			public const string AlignMat = "AlignMat";
			public const string Min = "Min";
			public const string Max = "Max";
			// public const string Falloff = "Falloff";
		}
      
		public ShaderRunner Runner;
		public Vector2Int ThreadSize = new Vector2Int(4,4);
		public ComputeBufferFacade Positions;
		[Tooltip("A buffer of single float values that should have the same length (or longer) as the Positions buffer and will receive a normalised the selector factor (0.0-1.0) for each position")]
		public ComputeBufferFacade Factors;
      
		public Transform Root;
		public BoxCollider Box;

		void Start()
		{
			Runner.Setup(ShaderProps.Kernel, (uint)ThreadSize.x, (uint)ThreadSize.y, ShaderProps.ResolutionX);
		}

		void Update()
		{
			var posbuf = this.Positions.GetValid();
			var facbuf = this.Factors.GetValid();
			if (posbuf == null || facbuf == null) return;
			this.UpdateSelectFactors(posbuf, facbuf);
		}

		public void UpdateSelectFactors(ComputeBuffer positions, ComputeBuffer factors) {
			Vector3 halfSize = Box.size * 0.5f;
			Vector3 Min = Box.center - halfSize;
			Vector3 Max = Box.center + halfSize;

			Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.factors_buf, factors);
			Runner.Shader.SetMatrix(ShaderProps.AlignMat, this.Root.worldToLocalMatrix);
			Runner.Shader.SetVector(ShaderProps.Min, Min);
			Runner.Shader.SetVector(ShaderProps.Max, Max);
			Runner.Run(positions, ShaderProps.positions_buf);
		}
	}
}