using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.ValueLayers
{
	public class ObjectDistance : BaseValueLayer
	{
		private static class ShaderProps
		{
			public const string Kernel = "CSObjectDistance";
			public const string ResolutionX = "ResolutionX";
			public const string values_buf = "values_buf";
			public const string positions_buf = "positions_buf";
			public const string LocalToWorldMatrix = "LocalToWorldMatrix";
			public const string ObjectPosition = "ObjectPosition";
			public const string ObjectRadius = "ObjectRadius";
		}

		public ComputeShader Shader;
		public Vector2Int ThreadSize = new Vector2Int(4,4);
		public ComputeBufferFacade PositionsFacade;
		public ComputeBufferFacade ValuesFacade;
		[Tooltip("Optional; its localToWorldMatrix will be used to calculate each particle's world position")]
		public Transform ParticlesParent;
		public Transform Object;
		public float ObjectRadius = 0.0f;

		private ShaderRunner Runner = null;

		public /*override*/ void Apply_(ComputeBuffer values_buf)
		{
			var positions_buf = this.PositionsFacade == null ? null : this.PositionsFacade.GetValid();
			if (positions_buf == null) return;

			if (this.Runner == null)
			{
				this.Runner = ShaderRunner.Create(this.Shader, ShaderProps.Kernel, (uint)ThreadSize.x, (uint)ThreadSize.y, ShaderProps.ResolutionX);
			}

			this.Runner.Shader.SetFloat(ShaderProps.ObjectRadius, this.ObjectRadius);
			this.Runner.Shader.SetVector(ShaderProps.ObjectPosition, this.Object.position);
			this.Runner.Shader.SetMatrix(ShaderProps.LocalToWorldMatrix, this.ParticlesParent != null
										 ? this.ParticlesParent.localToWorldMatrix
										 : Matrix4x4.identity);
			this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.positions_buf, positions_buf);

			this.Runner.Run(values_buf, ShaderProps.values_buf);
			Debug.Log("ObjectDistance ran");
		}

		private ShaderRunner CreateRunner()
		{
			var positions_buf = this.PositionsFacade == null ? null : this.PositionsFacade.GetValid();
			if (positions_buf == null) return null;

			return ShaderRunner.Create(
				new ShaderRunner.Opts()
					.Program(this.Shader, ShaderProps.Kernel)
					.Threading((uint)ThreadSize.x, (uint)ThreadSize.y, ShaderProps.ResolutionX)
					.MainBufferName(ShaderProps.values_buf)
					.Var(ShaderProps.ObjectRadius, () => this.ObjectRadius)
					.Var(ShaderProps.ObjectPosition, () => this.Object.position)
					.Var(ShaderProps.LocalToWorldMatrix, () =>
						 this.ParticlesParent != null
							? this.ParticlesParent.localToWorldMatrix
							: Matrix4x4.identity)
					.Buffer(ShaderProps.positions_buf, positions_buf)

				);
		}

		public override void Apply(ComputeBuffer values_buf)
		{
			if (this.Runner == null) {
				Debug.Log("Init runner");
				this.Runner = this.CreateRunner();
			}
			if (this.Runner != null) this.Runner.Run(values_buf);
		}

		public void Apply(ComputeBufferFacade valFacade) {
			var buf = valFacade == null ? null : valFacade.GetValid();
			if (buf == null) return;
			Apply(buf);
		}

		public void Apply() {
			this.Apply(this.ValuesFacade);
		}

		public void Reset() {
			this.Runner = null;
		}

		#region Public Action Methods
		public void SetObject(GameObject go) { this.Object = go.transform; }
		#endregion
	}
}