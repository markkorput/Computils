using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Renderers
{
	[AddComponentMenu("Computils/Renderers/Mesh Particles Renderer")]
	public class MeshParticlesRenderer : MonoBehaviour
	{
		private static class ShaderProps
		{
			public const string buf_particle_positions = "buf_particle_positions";
			public const string buf_mesh = "buf_mesh";
			public const string buf_alphafactors = "buf_alphafactors";
			public const string MainColor = "MainColor";
			public const string UseAlphaFactors = "UseAlphaFactors";
			public const string MeshVertCount = "MeshVertCount";
			public const string ParticleCount = "ParticleCount";
			public const string ParticleModelMatrix = "ParticleModelMatrix";

		}
      
        [Tooltip("Expect a buffer with float3 position values")]
		public ComputeBufferFacade ParticlesPositionsFacade;
		[Tooltip("Expects a buffer with float3 vertex position values (for example a buffer populated using MeshVertBufferPopulator)")]
		public ComputeBufferFacade MeshFacade;
		[Tooltip("Optional; when set, it will use these factors as alpha multiplier, assuming an alpha value for each particle position")]
		public ComputeBufferFacade AlphaFactorsFacade;
        public Material RenderMaterial;
		public MeshTopology MeshTopology = MeshTopology.Points;
		public Color MainColor = new Color(1, 1, 1, 0.3f);
        [Tooltip("This transform's localToWolrd matrix will be used to transform particle positions")]
		public Transform ModelMatrixParent;

#if UNITY_EDITOR
        [System.Serializable]
		public class Dinfo {
		    public int ParticlesCount;
		    public int MeshVertCount;
		    public int TotalVertCount;
		}
      
		public Dinfo DebugInfo;
#endif
      
		private void Start()
		{
			this.RenderMaterial = new Material(this.RenderMaterial);
		}

		//After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
		//this just draws the "mesh" as a set of points
		void OnPostRender()
		{
			var partbuf = this.ParticlesPositionsFacade == null ? null : this.ParticlesPositionsFacade.GetValid();
			var meshbuf = this.MeshFacade == null ? null : this.MeshFacade.GetValid();

			if (partbuf == null || meshbuf == null) return;
         
			Render(partbuf,
                   meshbuf,
			       this.MeshTopology,
			       this.RenderMaterial,
			       this.MainColor,
			       this.ModelMatrixParent == null ? Matrix4x4.identity : this.ModelMatrixParent.localToWorldMatrix,
			       this.AlphaFactorsFacade == null ? null : this.AlphaFactorsFacade.GetValid());

#if UNITY_EDITOR
			this.DebugInfo.ParticlesCount = partbuf.count;         
			this.DebugInfo.MeshVertCount = meshbuf.count;
			this.DebugInfo.TotalVertCount = partbuf.count * meshbuf.count;
#endif
            
		}
      
		private static void Render(
            ComputeBuffer partbuf,
            ComputeBuffer meshbuf,         
            MeshTopology topo,
            Material mat,
            Color clr,
			Matrix4x4 particleModelMatrix,
			ComputeBuffer alphaFactorsBuf = null)
		{
			mat.SetPass(0);
			mat.SetBuffer(ShaderProps.buf_particle_positions, partbuf);
			mat.SetBuffer(ShaderProps.buf_mesh, meshbuf);
			mat.SetInt(ShaderProps.MeshVertCount, meshbuf.count);
			mat.SetInt(ShaderProps.ParticleCount, partbuf.count);
			mat.SetColor(ShaderProps.MainColor, clr);
			mat.SetInt(ShaderProps.UseAlphaFactors, alphaFactorsBuf == null ? 0 : 1);
			mat.SetMatrix(ShaderProps.ParticleModelMatrix, particleModelMatrix);

			if (alphaFactorsBuf != null) mat.SetBuffer(ShaderProps.buf_alphafactors, alphaFactorsBuf);

			int total_vert_count = partbuf.count * meshbuf.count;
			Graphics.DrawProcedural(topo, total_vert_count);
		}

		#region Public Methods
		public void CycleTopology() {
			MeshTopology topo = MeshTopology.Lines;
         
			switch (this.MeshTopology) {
				// case MeshTopology.Points: topo = MeshTopology.Lines; break;
				case MeshTopology.Lines: topo = MeshTopology.LineStrip; break;
				case MeshTopology.LineStrip: topo = MeshTopology.Triangles; break;
				case MeshTopology.Triangles: topo = MeshTopology.Quads; break;
				case MeshTopology.Quads: topo = MeshTopology.Points; break;
			}
         
			this.MeshTopology = topo;
		}
      
		public void SetTopology(MeshTopology topo)
		{
			this.MeshTopology = topo;
		}
		#endregion
	}
}