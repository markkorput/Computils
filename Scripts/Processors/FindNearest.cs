using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors
{
	public class FindNearest : MonoBehaviour
	{
		private static class ShaderProps
		{
			public const string CalcKernel = "CSCalcDists";
			public const string FindKernel = "CSFindNearest";
			public const string ResolutionX = "ResolutionX";

			public const string Pos = "Pos";
			public const string BlacklistLength = "BlacklistLength";
			public const string DistancesCount = "DistancesCount";

			public const string positions_buf = "positions_buf";
			public const string distances_buf = "distances_buf";
			public const string index_feedback_buf = "index_feedback_buf";
			public const string feedback_buf = "feedback_buf";
			public const string blacklist_buf = "blacklist_buf";
		}

		public class NearestItem {
			public uint index;
			public float distance;
			public Vector3 pos;
			public NearestItem(uint i, float d, Vector3 p) { index = i; distance = d; pos = p; }
		}

		public ShaderRunner CalcRunner;
		public ShaderRunner FindRunner;

#if UNITY_EDITOR
		public ComputeBufferFacade Particles;      
        public Transform PositionTransform;
        public Vector3 Pos;

        [Header("Read-Only")]
		public int ClosestIdx = -1;
		public float ClosestDist = 0.0f;
		public Vector3 ClosestPos;
		public float ConsideredCount = 0.0f;
#endif
		private ComputeBuffer indexFeedbackBuf = null;
		private ComputeBuffer distanceFeedbackBuf = null;
		private ComputeBuffer blacklistBuf = null;
		private ComputeBuffer distsBuf = null;
		private bool RunnersInitialized = false;
		private float[] distsList = new float[0];

		void Start()
        {
			CalcRunner.Setup(ShaderProps.CalcKernel, 4, 4, ShaderProps.ResolutionX);
			FindRunner.Setup(ShaderProps.FindKernel, 1, 1, ShaderProps.ResolutionX);

			RunnersInitialized = true;
        }

#if UNITY_EDITOR
		private void Update()
		{         
			var buf = this.Particles != null ? this.Particles.GetValid() : null;
            if (buf == null) return;

			if (this.PositionTransform != null) this.Pos = this.PositionTransform.position;
            var nearestItem = this.GetNearest(buf, this.Pos);
         
			if (nearestItem != null)
			{
				this.ClosestIdx = (int)nearestItem.index;
				this.ClosestDist = nearestItem.distance;
				this.ClosestPos = nearestItem.pos;
			}
		}
#endif    

		public NearestItem GetNearest(ComputeBuffer buf, Vector3 pos, uint[] blacklistIndices = null) {
			if (!RunnersInitialized) this.Start();

			// CALCULATE (distances)
         
			this.distsBuf = Populators.Utils.UpdateOrCreate(this.distsBuf, new float[buf.count]);
            this.CalcRunner.Shader.SetBuffer(this.CalcRunner.Kernel, ShaderProps.distances_buf, distsBuf);
			this.CalcRunner.Shader.SetVector(ShaderProps.Pos, pos);
			// execute calculate shader kernel
            this.CalcRunner.Run(buf, ShaderProps.positions_buf);

			// FIND NEAREST
         
			// var nearest = GetNearestCPU(buf, distsBuf);
			var nearest = GetNearestGPU(buf, this.distsBuf, blacklistIndices);
         
#if UNITY_EDITOR
            this.Pos = pos;
			this.ClosestIdx = (int)nearest.index;
			this.ClosestDist = nearest.distance;
			this.ClosestPos = nearest.pos;
#endif

			return nearest;
		}
      
		private NearestItem GetNearestCPU(ComputeBuffer positionsBuf, ComputeBuffer distancesBuf, uint[] blacklistIndices = null) {         
			if (distsList.Length != distancesBuf.count) distsList = new float[distancesBuf.count];
			List<uint> bList = new List<uint>(blacklistIndices == null ? new uint[0] : blacklistIndices);

			distancesBuf.GetData(distsList);

			bool isFirst = true;
			float closestDist = 0.0f;
			uint closestIdx = 0;

			for (uint i = 0; i < (uint)distancesBuf.count; i++) {
				if (bList.Contains(i)) continue;
                
				var dist = distsList[i];

				if (isFirst) {
					closestIdx = i;
					closestDist = dist;

					isFirst = false;
					continue;
				}
            
				if (dist < closestDist) {
					closestIdx = i;
                    closestDist = dist;               
				}
			}

			return isFirst ? null : new NearestItem(closestIdx, closestDist, new Vector3(0, 0, 0)); // TODO; we don't have the position here
		}

		private NearestItem GetNearestGPU(ComputeBuffer buf, ComputeBuffer distsBuf, uint[] blacklistIndices = null) {
			// 0 = distance, 1 = pos.x, 2 = pos.y, 3 = pos.z, 4 = considered count
			distanceFeedbackBuf = Populators.Utils.UpdateOrCreate(this.distanceFeedbackBuf, new float[] { 9999.0f, 0.0f, 0.0f, 0.0f, 0.0f }); 
            indexFeedbackBuf = Populators.Utils.UpdateOrCreate(this.indexFeedbackBuf, new uint[] { 0 });
         
            // blacklist
            int blacklistLength = blacklistIndices == null ? 0 : blacklistIndices.Length;
            if (blacklistLength > 0)
            {
                blacklistBuf = Populators.Utils.UpdateOrCreate(this.blacklistBuf, blacklistIndices == null ? new uint[0] : blacklistIndices);
                this.FindRunner.Shader.SetBuffer(this.FindRunner.Kernel, ShaderProps.blacklist_buf, blacklistBuf);
            }

            this.FindRunner.Shader.SetInt(ShaderProps.BlacklistLength, blacklistLength);

            this.FindRunner.Shader.SetBuffer(this.FindRunner.Kernel, ShaderProps.distances_buf, distsBuf);
            this.FindRunner.Shader.SetBuffer(this.FindRunner.Kernel, ShaderProps.feedback_buf, distanceFeedbackBuf);
            this.FindRunner.Shader.SetBuffer(this.FindRunner.Kernel, ShaderProps.index_feedback_buf, indexFeedbackBuf);

            // execute finder kernel
			this.FindRunner.Run(buf, ShaderProps.positions_buf);         


            // on run
			// this.FindRunner.Shader.SetInt(ShaderProps.DistancesCount, distsBuf.count);
			// this.FindRunner.Run(buf, ShaderProps.positions_buf);

         
            // fetch results
            uint[] indexes = new uint[1];
            indexFeedbackBuf.GetData(indexes);
			float[] distInfo = new float[5]; // 0 = distance, 1 = pos.x, 2 = pos.y, 3 = pos.z, 4 = considered count         
            distanceFeedbackBuf.GetData(distInfo);
         
#if UNITY_EDITOR
			this.ConsideredCount = distInfo[4];
#endif
         
			if (distInfo[0] < 0.0f) return null;
            return new NearestItem(indexes[0], distInfo[0], new Vector3(distInfo[1], distInfo[2], distInfo[3]));
		}
	}
}