using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
	/// <summary>
	/// Takes a buffer vertices (triangles) as input and produces
	/// and new buffer of vertices (triangles) with equal-size
	/// triangles that cover the input mesh
	/// </summary>
	[AddComponentMenu("Computils/Porcessors/EqualTriangles")]
	public class EqualTriangles : MonoBehaviour
	{
		private static class ShaderProps
		{
			public const string KernelCalculate = "CSCalculateMeshSurface";
			public const string KernelConvert = "CSConvert";
			// public const string ResolutionX = "ResolutionX";

			public const string mesh_buf = "mesh_buf";
			public const string target_buf = "target_buf";
			public const string calc_buf = "calc_buf";

			public const string TriangleSize = "TriangleSize";
			public const string MeshVertCount = "MeshVertCount";
			public const string TargetVertCount = "TargetVertCount";
		}

		public ComputeBufferFacade MeshBuffer;
		public ComputeBufferFacade TargetBuffer;
		public float TriangleSize = 0.5f;
		public ShaderRunner Runner;
		private ShaderRunner CalcRunner;
		private bool redo = false;

#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo
		{
			public float Surface = -1.0f;
			public int MeshTriCount = 0;
			public float TrianglesCounted = 0;
			public float AverageTriSize = -1.0f;
			public int TriCount = 0;
		};
      
		public Dinfo DebugInfo;
#endif

		private void OnEnable()
		{
			if (this.redo) Generate();
		}
      
		private void OnDisable()
		{
			this.redo = true;
		}
		void Start()
		{
			Runner.Setup(ShaderProps.KernelConvert, 1, 1);
			this.CalcRunner = new ShaderRunner();
			this.CalcRunner.Shader = this.Runner.Shader;
			this.CalcRunner.Setup(ShaderProps.KernelCalculate, 1, 1);

			this.Generate();
		}

		private void Generate() {
			var mesh_buf = this.MeshBuffer.GetValid();         
            if (mesh_buf == null) return;         
         
			float surface = CalculateMeshSurface(mesh_buf);
			int target_triangle_count = Mathf.CeilToInt(surface / this.TriangleSize);

#if UNITY_EDITOR
			DebugInfo.MeshTriCount = (Mathf.FloorToInt((float)mesh_buf.count / 3.0f));
			DebugInfo.TriCount = target_triangle_count;
			DebugInfo.Surface = surface;
#endif

			int target_vert_count = target_triangle_count * 3;
			var target_buf = this.TargetBuffer.GetValid();
         
			if (target_buf == null || target_buf.count != target_vert_count) {
				target_buf = new ComputeBuffer(target_vert_count, sizeof(float)*3);
				TargetBuffer.Set(target_buf);
			}

			Convert(mesh_buf, target_buf, this.TriangleSize);
		}

		public float CalculateMeshSurface(ComputeBuffer buf) {
			// this.Runner.Shader.SetBuffer()
			ComputeBuffer calc_buf = new ComputeBuffer(3, sizeof(float));
			calc_buf.SetData(new float[] { -1.0f, -1.0f, -1.0f });

			this.CalcRunner.Shader.SetBuffer(this.CalcRunner.Kernel, ShaderProps.calc_buf, calc_buf);
			this.CalcRunner.Shader.SetInt(ShaderProps.MeshVertCount, buf.count);
		    this.CalcRunner.RunOnce(buf, ShaderProps.mesh_buf);
         
			float[] calc_data = new float[3];
			calc_buf.GetData(calc_data);
#if UNITY_EDITOR
            // DebugInfo.Surface = calc_data[0];
			DebugInfo.AverageTriSize = calc_data[1];
			DebugInfo.TrianglesCounted = calc_data[2];
#endif
			return calc_data[0];
		}
      
		public void Convert(ComputeBuffer mesh_buf, ComputeBuffer target_buf, float TriangleSize)
        {
            this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.mesh_buf, mesh_buf);
			this.Runner.Shader.SetInt(ShaderProps.MeshVertCount, mesh_buf.count);
			this.Runner.Shader.SetInt(ShaderProps.TargetVertCount, target_buf.count);
			this.Runner.Shader.SetFloat(ShaderProps.TriangleSize, TriangleSize);
            this.Runner.RunOnce(target_buf, ShaderProps.target_buf);
        }
	}
}