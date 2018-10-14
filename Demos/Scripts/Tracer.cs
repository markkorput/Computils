using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Demos
{
	public class Tracer : MonoBehaviour
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

		private class TracePoint
		{
			public uint index;
			public Vector3 pos;
			public TracePoint(uint i, Vector3 p) { index = i; pos = p; }
		}
      
		public ComputeBufferFacade Particles;
		public ComputeBufferFacade Trace;
		public Processors.FindNearest FindNearest;
		public Transform TracePosTransform;
		public Vector3 TracePos;

		public int MaxLength = 10;
		public KeyCode NextKey = KeyCode.N;
      
		private uint curLength = 0;
		private ComputeBuffer TraceBuffer;
      
		private TracePoint[] Points;
		private bool ReadyForNext = false;

		private void Update()
		{
			if (Input.GetKeyDown(NextKey)) ReadyForNext = true;
			if (curLength >= MaxLength || !ReadyForNext) return;         
			if (TraceBuffer == null || TraceBuffer.count != MaxLength) Init((uint)this.MaxLength);
         
			if (curLength == 0 && TracePosTransform != null) this.TracePos = this.TracePosTransform.position;
         
            // find closest
			TracePoint closest = GetNearest(this.TracePos);
			Points[curLength] = closest;

			Vector3 prev = curLength == 0 ? TracePos : Points[curLength - 1].pos;
			this.TracePos = closest.pos + (closest.pos - prev);
         
			curLength += 1;
			for (uint i = curLength; i < Points.Length; i++) Points[i].pos = closest.pos;
         
            // update buffer
			Vector3[] positions = (from p in Points select p.pos).ToArray();
			TraceBuffer.SetData(positions);

			ReadyForNext = false;
		}
      
		private TracePoint GetNearest(Vector3 pos) {
			var buf = this.Particles.GetValid();
			if (buf == null) return null;
			var nearestItem = this.FindNearest.GetNearest(buf, pos);
			if (nearestItem == null) return null;

			return new TracePoint(nearestItem.index, nearestItem.pos);
		}
      
		private void Init(uint maxlen)
		{
			if (TraceBuffer != null) {
				TraceBuffer.Release();
				TraceBuffer.Dispose();
			}
         
			TraceBuffer = Populators.Utils.Create(new Vector3[MaxLength]);
			Trace.Set(TraceBuffer);
         
			Points = new TracePoint[MaxLength];
			for (int i = 0; i < MaxLength; i++) Points[i] = new TracePoint(0, new Vector3(0, 0, 0));
		}
	}
}