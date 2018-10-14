using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
	public class FindNearest : MonoBehaviour
	{
		private static class ShaderProps
		{
			public const string Kernel = "CSFindNearest";
			public const string ResolutionX = "ResolutionX";

			public const string Pos = "Pos";
			public const string positions_buf = "positions_buf";
			public const string index_feedback_buf = "index_feedback_buf";
			public const string distance_feedback_buf = "distance_feedback_buf";
		}
      
		public class NearestItem {
			public uint index;
			public float distance;
			public Vector3 pos;
			public NearestItem(uint i, float d, Vector3 p) { index = i; distance = d; pos = p; }
		}
      
		public ShaderRunner Runner;

#if UNITY_EDITOR
		public ComputeBufferFacade Particles;      
        public Transform PositionTransform;
        public Vector3 Pos;

        [Header("Read-Only")]
		public int ClosestIdx = -1;
		public float ClosestDist = 0.0f;
#endif
		private ComputeBuffer indexFeedbackBuf, distanceFeedbackBuf;
      
		void Start()
        {
            Runner.Setup(ShaderProps.Kernel, 4, 4);
            Runner.NameResolutionX = ShaderProps.ResolutionX;
        }

#if UNITY_EDITOR
		private void Update()
		{         
			var buf = this.Particles.GetValid();
            if (buf == null) return;

			if (this.PositionTransform != null) this.Pos = this.PositionTransform.position;
            var nearestItem = this.GetNearest(buf, this.Pos);

			if (nearestItem != null)
			{
				this.ClosestIdx = (int)nearestItem.index;
				this.ClosestDist = nearestItem.distance;
			}
		}
#endif    

		public NearestItem GetNearest(ComputeBuffer buf, Vector3 pos) {
            if (this.PositionTransform != null) this.Pos = this.PositionTransform.position;
         
            indexFeedbackBuf = Populators.Utils.UpdateOrCreate(this.indexFeedbackBuf, new uint[] { 0 });
            distanceFeedbackBuf = Populators.Utils.UpdateOrCreate(this.distanceFeedbackBuf, new float[] { -1.0f, 0,0,0 });

            this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.distance_feedback_buf, distanceFeedbackBuf);
            this.Runner.Shader.SetBuffer(this.Runner.Kernel, ShaderProps.index_feedback_buf, indexFeedbackBuf);
            this.Runner.Shader.SetVector(ShaderProps.Pos, this.Pos);
            this.Runner.Run(buf, ShaderProps.positions_buf);

            uint[] indexes = new uint[1];
            indexFeedbackBuf.GetData(indexes);
            float[] distInfo = new float[4];
			distanceFeedbackBuf.GetData(distInfo);

			return new NearestItem(indexes[0], distInfo[0], new Vector3(distInfo[1], distInfo[2], distInfo[3]));
		}
	}
}