using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.Forces
{
    public class FrictionForce : Force
    {
		private static class ShaderProps {
			public const string Kernel = "CSFrictionForce";

			public const string forces_buf = "forces_buf";
			public const string velocities_buf = "velocities_buf";
			public const string FrictionFactor = "FrictionFactor";
			public const string Additive = "Additive";
			public const string ResolutionX = "ResolutionX";
		}

		public ShaderRunner Runner;
		public ComputeBufferFacade VelocitiesFacade;
		public float FrictionFactor = 0.1f;
		public bool Additive = false;

		private void Start()
        {
			Runner.Setup(ShaderProps.Kernel, 4, 4);
			Runner.NameResolutionX = ShaderProps.ResolutionX;
        }
      
		override public void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf)
        {
			var velbuf = this.VelocitiesFacade == null ? null : this.VelocitiesFacade.GetValid();
			if (velbuf == null) return;
         
			this.Runner.Shader.SetFloat(ShaderProps.FrictionFactor, this.FrictionFactor);
			this.Runner.Shader.SetBool(ShaderProps.Additive, this.Additive);
			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.velocities_buf, velbuf);
			this.Runner.Run(forces_buf, ShaderProps.forces_buf);
		}
    }
}