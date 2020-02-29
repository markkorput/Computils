using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Renderers
{
	[AddComponentMenu("Computils/Renderers/Single Color Renderer")]
	public class SingleColorRenderer : MonoBehaviour
	{
		private static class ShaderProps
		{
			public const string buf_verts = "buf_verts";
			public const string buf_alphafactors = "buf_alphafactors";
			public const string MainColor = "MainColor";
			public const string UseAlphaFactors = "UseAlphaFactors";
			public const string LocalToWorldMatrix = "LocalToWorldMatrix";
		}

		public ComputeBufferFacade VertsBufFacade;
		[Tooltip("Optional; when set, its localToWorldMatrix is used to calculate vertex world positions")]
		public Transform VertsParent;
		[Tooltip("Optional; when set, it will use these factors as alpha multiplier")]
		public ComputeBufferFacade AlphaFactorsFacade;
		public Material RenderMaterial;
		public MeshTopology MeshTopology = MeshTopology.Points;
		public Color MainColor = new Color(1, 1, 1, 0.3f);

#if UNITY_EDITOR
		[Header("Read-Only")]
		public int VertCount = 0;
#endif

		private void Start()
		{
			this.RenderMaterial = new Material(this.RenderMaterial);
		}

		//After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
		//this just draws the "mesh" as a set of points
		void OnPostRender()
		{
			var buf = this.VertsBufFacade.GetValid();

			if (buf != null)
			{
				Render(this.RenderMaterial, buf, this.VertsParent, this.MeshTopology, this.MainColor, this.AlphaFactorsFacade == null ? null : this.AlphaFactorsFacade.GetValid());
#if UNITY_EDITOR
				this.VertCount = buf.count;
#endif
			}
		}

		private static void Render(Material mat, ComputeBuffer vertsBuffer, Transform Parent, MeshTopology topo, Color clr, ComputeBuffer alphaFactorsBuf = null)
		{
			mat.SetPass(0);
			mat.SetBuffer(ShaderProps.buf_verts, vertsBuffer);
			mat.SetColor(ShaderProps.MainColor, clr);
			mat.SetInt(ShaderProps.UseAlphaFactors, alphaFactorsBuf == null ? 0 : 1);
			mat.SetMatrix(ShaderProps.LocalToWorldMatrix, Parent == null ? Matrix4x4.identity : Parent.localToWorldMatrix);
			if (alphaFactorsBuf != null) mat.SetBuffer(ShaderProps.buf_alphafactors, alphaFactorsBuf);

			Graphics.DrawProceduralNow(topo, vertsBuffer.count);
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