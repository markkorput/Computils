using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.Forces
{
    public class ConstantForce : Force
    {
		private static class ShaderProps {
			public const string Kernel = "CSForce";
            
			public const string forces_buf = "forces_buf";
			public const string Force = "Force";
			public const string MaxVariation = "MaxVariation";
			public const string Additive = "Additive";
			public const string ResolutionX = "ResolutionX";
		}
      
		public ShaderRunner Runner;
		public Transform FromTransform;
		public Transform ToTransform;
		public Vector3 Force;
		public float MaxVariation = 0.1f;
		public bool Additive = false;
      
		private void Start()
        {
			Runner.Setup(ShaderProps.Kernel, 4, 4);
			Runner.NameResolutionX = ShaderProps.ResolutionX;
        }
      
		override public void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf)
        {
			if (FromTransform != null && ToTransform != null) this.Force = ToTransform.position - FromTransform.position;

			this.Runner.Shader.SetVector(ShaderProps.Force, this.Force);
			this.Runner.Shader.SetFloat(ShaderProps.MaxVariation, this.MaxVariation);
			this.Runner.Shader.SetBool(ShaderProps.Additive, this.Additive);
			this.Runner.Run(forces_buf, ShaderProps.forces_buf);
		}
    }
}