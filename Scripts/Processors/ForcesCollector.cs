using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
    public class ForcesCollector : MonoBehaviour
    {
		private static class ShaderProps {
			public const string Kernel = "CSForces";
			public const string positions_buf = "positions_buf";
			public const string forces_buf = "forces_buf";
			public const string factors_buf = "factors_buf";
			public const string ResolutionX = "ResolutionX";
			public const string DeltaTime = "DeltaTime";
			public const string UseFactors = "UseFactors";
		}

		public ShaderRunner Runner;
		public ComputeBufferFacade ForcesFacade;
		public ComputeBufferFacade PositionsFacade;
		public Forces.Force[] Forces;
		[Tooltip("Optional; should point to a buffer with single float values with a normalised (0.0-1.0) factor for each position which act a multiplier for how much the forces affect the positions")]
		public ComputeBufferFacade ForceFactors;
		private ComputeBuffer dummyForcesBuf;

		public KeyCode ToggleKey = KeyCode.None;

		private bool toggleActive = true;

		private void Start()
        {
			this.Runner.Setup(ShaderProps.Kernel, 4, 4, ShaderProps.ResolutionX);
        }
      
		void Update()
        {
			if (!toggleActive) return;
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

					this.Apply(forces_buf, positions_buf, this.ForceFactors == null ? null : this.ForceFactors.GetValid());
                }            
			}
		}
      
		public void Apply(ComputeBuffer forces_buf, ComputeBuffer positions_buf, ComputeBuffer factors_buf = null)
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
         
			this.Runner.Run(positions_buf, ShaderProps.positions_buf);
		}

		private void OnGUI()
        {
            Event evt = Event.current;
            if (evt.isKey && Input.GetKeyDown(evt.keyCode)) this.OnKeyDown(evt);
        }
      
        private void OnKeyDown(Event evt)
        {
			if (evt.keyCode == this.ToggleKey) this.toggleActive = !this.toggleActive;
        }
    }
}