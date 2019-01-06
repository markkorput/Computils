using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
    public class ForcesCollector : MonoBehaviour
    {
		private static class ShaderProps {
			public const string ForcesToPositionsKernel = "CSApplyForcesToPositions";
			public const string ForcesToVelocitiesKernel = "CSApplyForcesToVelocities";
			public const string positions_buf = "positions_buf";
			public const string forces_buf = "forces_buf";
			public const string velocities_buf = "velocities_buf";
			public const string masses_buf = "masses_buf";         
			public const string factors_buf = "factors_buf";
			public const string ResolutionX = "ResolutionX";
			public const string DeltaTime = "DeltaTime";
			public const string UseFactors = "UseFactors";
			public const string UseMasses = "UseMasses";
		}
      
		public ShaderRunner Runner;
		public ComputeBufferFacade ForcesFacade;
		[Tooltip("Optional; when not specified, will not use a Velocities, but instead apply the forces directly to the positions")]
		public ComputeBufferFacade VelocitiesFacade;
		[Tooltip("Optional; when not specified, will default to mass=1 for all particles")]
        public ComputeBufferFacade MassesFacade;      
		public ComputeBufferFacade PositionsFacade;
        [Tooltip("Defaults to all Forces found in self and children")]
		public Forces.Force[] Forces;
		[Tooltip("Optional; should point to a buffer with single float values with a normalised (0.0-1.0) factor for each position which act a multiplier for how much the forces affect the positions")]
		public ComputeBufferFacade ForceFactors;
		private ComputeBuffer dummyForcesBuf;
      
		private void Start()
        {
			this.Runner.Setup(
				this.VelocitiesFacade == null
				    ? ShaderProps.ForcesToPositionsKernel
				    : ShaderProps.ForcesToVelocitiesKernel,
				4, 4, ShaderProps.ResolutionX);

			if (this.Forces.Length == 0) this.Forces = GetComponentsInChildren<Forces.Force>();
        }

		void Update()
        {
			var forces_buf = ForcesFacade.GetValid();
			var positions_buf = PositionsFacade.GetValid();

			if (forces_buf != null && positions_buf != null)
			{
				if (forces_buf.count != positions_buf.count)
				{
					Debug.LogWarning("ForcesCollector: positions buffer count doesn't match forces buffer count");
				} else {
                    // Collect
					foreach (var force in Forces)
					{
						force.Apply(forces_buf, positions_buf);
					}

					if (this.VelocitiesFacade != null)
					{
						this.ApplyToVelocities(
							forces_buf,
                            this.VelocitiesFacade.GetValid(),
                            positions_buf,
                            this.MassesFacade == null ? null : this.MassesFacade.GetValid(), this.ForceFactors == null ? null : this.ForceFactors.GetValid());
					}
					else
					{
						this.ApplyToPositions(
							forces_buf,
                            positions_buf,
                            this.MassesFacade == null ? null : this.MassesFacade.GetValid(),
                            this.ForceFactors == null ? null : this.ForceFactors.GetValid());
					}
                }            
			}
		}

        /// <summary>
        /// Applies Forces directly to positions
        /// </summary>
        /// <param name="forces_buf">Forces buffer.</param>
        /// <param name="positions_buf">Positions buffer.</param>
        /// <param name="factors_buf">Factors buffer.</param>      
		public void ApplyToPositions(ComputeBuffer forces_buf, ComputeBuffer positions_buf, ComputeBuffer masses_buf = null, ComputeBuffer factors_buf = null)
        {
			this.Runner.Shader.SetFloat(ShaderProps.DeltaTime, Time.deltaTime);
			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.forces_buf, forces_buf);         

			bool useFactors = factors_buf != null;
			this.Runner.Shader.SetBool(ShaderProps.UseFactors, useFactors);
			if (factors_buf == null)
			{
				if (dummyForcesBuf == null) dummyForcesBuf = new ComputeBuffer(1, sizeof(float));
				factors_buf = dummyForcesBuf;
			}

			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.factors_buf, factors_buf);

			this.Runner.Shader.SetBool(ShaderProps.UseMasses, masses_buf != null);
			//if (masses_buf != null) this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.masses_buf, masses_buf);
			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.masses_buf, masses_buf != null ? masses_buf : factors_buf);

			this.Runner.Run(positions_buf, ShaderProps.positions_buf);
		}

		public void ApplyToVelocities(ComputeBuffer forces_buf, ComputeBuffer velocities_buf, ComputeBuffer positions_buf, ComputeBuffer masses_buf, ComputeBuffer factors_buf = null) {
			if (forces_buf == null || velocities_buf == null || positions_buf == null) return;
         
			this.Runner.Shader.SetFloat(ShaderProps.DeltaTime, Time.deltaTime);
            this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.forces_buf, forces_buf);
			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.velocities_buf, velocities_buf);
         
            bool useFactors = factors_buf != null;
            this.Runner.Shader.SetBool(ShaderProps.UseFactors, useFactors);
         
            if (factors_buf == null)
            {
                if (dummyForcesBuf == null) dummyForcesBuf = new ComputeBuffer(1, sizeof(float));
                factors_buf = dummyForcesBuf;
            }
         
            this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.factors_buf, factors_buf);
         
			this.Runner.Shader.SetBool(ShaderProps.UseMasses, masses_buf != null);
            //if (masses_buf != null) this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.masses_buf, masses_buf);
			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.masses_buf, masses_buf != null ? masses_buf : factors_buf);
         
            this.Runner.Run(positions_buf, ShaderProps.positions_buf);
		}
    }
}