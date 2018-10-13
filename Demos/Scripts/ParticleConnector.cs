using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Computils.Demos
{
	public class ParticleConnector : MonoBehaviour
	{
		private static class ShaderProps {
			public const string Kernel = "CSMain";
			public const string LineBuf = "selection_buf";
			public const string PositionsBuf = "positions_buf";
			public const string IndicesBuf = "indices_buf";

			public const string ResolutionX = "ResolutionX";
			public const string IndicesCount = "IndicesCount";
		}
      
		public ComputeBufferFacade Particles;
		public ComputeBufferFacade Line;
		public int[] ParticleIndices;
		public Processors.ShaderRunner PickerRunner;

		public KeyCode RandomizeKey = KeyCode.R;
      
		private ComputeBuffer indicesBuf;
      
		void Start()
		{
			PickerRunner.Setup(ShaderProps.Kernel, 4, 4);
			PickerRunner.NameResolutionX = ShaderProps.ResolutionX;
		}

		void Update()
		{
			var line_buf = Line.GetValid();
			var pos_buf = Particles.GetValid();
			if (line_buf == null || pos_buf == null) return;

			if (Input.GetKeyDown(this.RandomizeKey))
            {
				var rnd = new System.Random();
				for (int i = 0; i < this.ParticleIndices.Length; i++) {
					this.ParticleIndices[i] = rnd.Next(0, this.ParticleIndices.Length);
				}
            }
         
			this.indicesBuf = Populators.Utils.UpdateOrCreate(this.indicesBuf, (from idx in ParticleIndices select (uint)idx).ToArray());
			PickerRunner.Shader.SetBuffer(PickerRunner.Kernel, ShaderProps.IndicesBuf, this.indicesBuf);
			PickerRunner.Shader.SetBuffer(PickerRunner.Kernel, ShaderProps.PositionsBuf, pos_buf);
			PickerRunner.Shader.SetInt(ShaderProps.IndicesCount, indicesBuf.count);
            PickerRunner.Run(line_buf, ShaderProps.LineBuf);
		}
	}
}