using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Selectors
{
	public class IntervalSelector : MonoBehaviour
	{
		private static class ShaderProps {
			public const string Kernel = "CSLoadSelectFactors";
			public const string positions_buf = "positions_buf";
			public const string factors_buf = "factors_buf";
			public const string Interval = "Interval";
			public const string ResolutionX = "ResolutionX";
		}

		public ShaderRunner Runner;
		public ComputeBufferFacade Positions;
		[Tooltip("A buffer of single float values that should have the same length (or longer) as the Positions buffer and will receive a normalised the selector factor (0.0-1.0) for each position")]
		public ComputeBufferFacade Factors;
		public Vector3 Interval;
      
#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo {
			
		}

		public Dinfo DebugInfo;
#endif
      
		void Start()
		{
			Runner.Setup(ShaderProps.Kernel, 4, 4);
			Runner.NameResolutionX = ShaderProps.ResolutionX;
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
			Runner.Shader.SetVector(ShaderProps.Interval, this.Interval);
			Runner.Run(positions, ShaderProps.positions_buf);
		}
	}
}