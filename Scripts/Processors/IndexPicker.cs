using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Computils.Processors
{
	[AddComponentMenu("Computils/Porcessors/IndexPicker")]
	public class IndexPicker : MonoBehaviour
	{
		private static class ShaderProps
		{
			public const string Kernel = "CSMain";
			public const string SelectionBuf = "selection_buf";
			public const string PositionsBuf = "positions_buf";
			public const string IndicesBuf = "indices_buf";

			public const string ResolutionX = "ResolutionX";
			public const string IndicesCount = "IndicesCount";
		}

		[System.Serializable]
        public class IntEvent : UnityEvent<int> { }

      
		public ComputeBufferFacade Particles;
		public ComputeBufferFacade Target;
		public int[] ParticleIndices;
		public ShaderRunner PickerRunner;
      
		public UnityEvent OnPicked;
		public IntEvent AmountPickedEvent;
      
		private ComputeBuffer indicesBuf;

		void Start()
		{
			PickerRunner.Setup(ShaderProps.Kernel, 4, 4);
			PickerRunner.NameResolutionX = ShaderProps.ResolutionX;
		}

		void Update()
		{
			var sel_buf = Target.GetValid();
			var pos_buf = Particles.GetValid();
			if (sel_buf == null || pos_buf == null) return;
			this.Pick(pos_buf, sel_buf, (from idx in ParticleIndices select (uint)idx).ToArray());
		}
      
		public void Pick(ComputeBuffer particles_buf, ComputeBuffer selection_buf, uint[] indices)
		{
			this.indicesBuf = Populators.Utils.UpdateOrCreate(this.indicesBuf, indices);
			PickerRunner.Shader.SetInt(ShaderProps.IndicesCount, indices.Length);
			PickerRunner.Shader.SetBuffer(PickerRunner.Kernel, ShaderProps.IndicesBuf, this.indicesBuf);
			PickerRunner.Shader.SetBuffer(PickerRunner.Kernel, ShaderProps.PositionsBuf, particles_buf);
			PickerRunner.Run(selection_buf, ShaderProps.SelectionBuf);

			this.OnPicked.Invoke();
			this.AmountPickedEvent.Invoke(indices.Length);
		}
      
		#region Public Action Methods
        /// <summary>
        /// Takes the given indices and runs the ComputeShader that populates our
		/// Target buffer with position (float3) values from the Particles buffer.
        /// </summary>
        /// <param name="indexes">Indexes.</param>
		public void Pick(uint[] indexes) {
			var partbuf = this.Particles.GetValid();
			if (partbuf == null) return;

			var linebuf = this.Target.GetValid();
			if (linebuf == null || linebuf.count < indexes.Length)
            {
				linebuf = Populators.Utils.UpdateOrCreate(linebuf, new Vector3[indexes.Length]);
				this.Target.Set(linebuf);
            }

			this.Pick(partbuf, linebuf, indexes);
		}
      
		public void Randomize() {
			var rnd = new System.Random();
            for (int i = 0; i < this.ParticleIndices.Length; i++)
            {
                this.ParticleIndices[i] = rnd.Next(0, this.ParticleIndices.Length);
            }         
		}
		#endregion
	}
}