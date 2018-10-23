using UnityEngine;
using System.Collections;

namespace Computils.Processors
{
	public class BoxRespawner : MonoBehaviour
	{
		private static class ShaderProps {
			public const string Kernel = "CSRespawn";
			public const string ResolutionX = "ResolutionX";
         
			public const string ParticlesBuffer = "positions_buf";
			public const string BirthTimesBuffer = "times_buf";
   
			public const string Time = "Time";
			public const string MaxAge = "MaxAge";
			public const string MaxAgeVariation = "MaxAgeVariation";
			public const string BoxMatrix = "BoxMatrix";
		}
      
		public ComputeBufferFacade Particles;
		public ComputeBufferFacade BirthTimes;
		public float MaxAge = 5.0f;
		public float MaxAgeVariation = 1.0f;
		public Transform BoxTranform;
      
		public ShaderRunner Runner;

		public KeyCode ToggleKey = KeyCode.None;
		private bool toggleActive = true;
      
		void Start()
		{
			Runner.Setup(ShaderProps.Kernel, 4, 4);
			Runner.NameResolutionX = ShaderProps.ResolutionX;
		}
      
		void Update()
		{
			if (!toggleActive) return;
			var birthsBuf = BirthTimes.GetValid();
			var particlesBuf = Particles.GetValid();
			if (birthsBuf == null || particlesBuf == null) return;
         
			// provide additional data
			this.Runner.Shader.SetFloat(ShaderProps.Time, Time.realtimeSinceStartup);
			this.Runner.Shader.SetFloat(ShaderProps.MaxAge, this.MaxAge);
			this.Runner.Shader.SetFloat(ShaderProps.MaxAgeVariation, this.MaxAgeVariation);
			this.Runner.Shader.SetMatrix(ShaderProps.BoxMatrix, this.BoxTranform.localToWorldMatrix);
			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.BirthTimesBuffer, birthsBuf);
         
            // run
			this.Runner.Run(particlesBuf, ShaderProps.ParticlesBuffer);
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