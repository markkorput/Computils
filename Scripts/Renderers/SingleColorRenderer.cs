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
			public const string MainColor = "MainColor";
        }
      
        public ComputeBufferFacade VertsBufFacade;
        public Material RenderMaterial;
        public MeshTopology MeshTopology = MeshTopology.Points;
        public Color MainColor = new Color(1, 1, 1, 0.3f);
      
#if UNITY_EDITOR
        [Header("Read-Only")]
        public int VertCount = 0;
#endif
      
        //After all rendering is complete we dispatch the compute shader and then set the material before drawing with DrawProcedural
        //this just draws the "mesh" as a set of points
        void OnPostRender()
        {
			var buf = this.VertsBufFacade.GetValid();
         
			if (buf != null)
			{
				Render(this.RenderMaterial, buf, this.MeshTopology, this.MainColor);
#if UNITY_EDITOR
				this.VertCount = buf.count;
#endif
			}
        }
      
		private static void Render(Material mat, ComputeBuffer vertsBuffer, MeshTopology topo, Color clr)
        {
            mat.SetPass(0);
			mat.SetBuffer(ShaderProps.buf_verts, vertsBuffer);
			mat.SetColor(ShaderProps.MainColor, clr);
            Graphics.DrawProcedural(topo, vertsBuffer.count);
        }
    }
}