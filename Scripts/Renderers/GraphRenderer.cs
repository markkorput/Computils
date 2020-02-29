using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Renderers
{
	[AddComponentMenu("Computils/Renderers/Graph Renderer")]
    public class GraphRenderer : MonoBehaviour
    {
        private static class ShaderProps
        {
            public const string buf_values = "buf_values";         
			public const string MainColor = "MainColor";
			public const string Count = "Count";
			public const string localToWorldMat = "localToWorldMat";
			public const string Origin = "Origin";
			public const string Size = "Size";
			public const string ValRange = "ValRange";
        }

        public ComputeBufferFacade ValuesBufFacade;
        public Material RenderMaterial;
		public MeshTopology MeshTopology = MeshTopology.LineStrip;
		public Color MainColor = new Color(1, 1, 1, 1);
		[Tooltip("Optional; is specified the localToWorldMatrix of the BoxCollider2D's transform will be used as well as it offset and size properties to define the frame of the graph")]
		public BoxCollider2D Frame;
		public float ValMin = 0;
		public float ValMax = 10;

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
			var buf = this.ValuesBufFacade.GetValid();

			if (buf != null)
			{
				Render(this.RenderMaterial, buf, this.MeshTopology, this.MainColor, this.TransMat, this.Origin, this.Size, new Vector2(this.ValMin, this.ValMax));
#if UNITY_EDITOR
				this.VertCount = buf.count;
#endif
			}
        }

		private static void Render(Material mat, ComputeBuffer valuesBuf, MeshTopology topo, Color clr, Matrix4x4 matrix, Vector2 origin, Vector2 size, Vector2 valrange)
        {
            mat.SetPass(0);
			mat.SetBuffer(ShaderProps.buf_values, valuesBuf);
			mat.SetColor(ShaderProps.MainColor, clr);
			mat.SetInt(ShaderProps.Count, valuesBuf.count);
			mat.SetMatrix(ShaderProps.localToWorldMat, matrix);
			mat.SetVector(ShaderProps.Origin, origin);
			mat.SetVector(ShaderProps.Size, size);
			mat.SetVector(ShaderProps.ValRange, valrange);

			//mat.SetInt(ShaderProps.UseAlphaFactors, alphaFactorsBuf == null ? 0 : 1);
			//if (alphaFactorsBuf != null) mat.SetBuffer(ShaderProps.buf_alphafactors, alphaFactorsBuf);
         
            Graphics.DrawProceduralNow(topo, valuesBuf.count);
        }
      
		private Matrix4x4 TransMat { get {
				return this.Frame == null ? Matrix4x4.identity : this.Frame.transform.localToWorldMatrix;
			}}

		private Vector2 Origin { get {
				//Frame.transform.localToWorldMatrix * Frame.
				return this.Frame == null ? new Vector2(0,0) : this.Frame.offset - (Frame.size * 0.5f);
			}}

		private Vector2 Size
        {
            get
            {
                //Frame.transform.localToWorldMatrix * Frame.
				return this.Frame == null ? new Vector2(1,1) : this.Frame.size;
            }
        }
    }
}