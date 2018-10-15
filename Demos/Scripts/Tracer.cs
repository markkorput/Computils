using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Demos
{
	public class Tracer : MonoBehaviour
	{      
		[System.Serializable]
		public class TracePoint
		{
			public uint index;
			public Vector3 pos;
			public TracePoint(uint i, Vector3 p) { index = i; pos = p; }
		}

		public ComputeBufferFacade Particles;
		public ComputeBufferFacade Trace;
		public Processors.FindNearest FindNearest;
		public Processors.IndexPicker IndexPicker;
		public Transform TracePosTransform;
		public Vector3 TracePos;
		public bool RealtimePositions = false;

		public int MaxLength = 10;
		public KeyCode NextKey = KeyCode.N;
		public KeyCode ClearKey = KeyCode.C;

		private uint curLength = 0;
		private ComputeBuffer TraceBuffer;
      
		private TracePoint[] TracePoints;
		private bool ReadyForNext = false;

#if UNITY_EDITOR
        [System.Serializable]
		public class Dinfo {
			public int CurLength = 0;
			public TracePoint[] Points;
		}

		public Dinfo DebugInfo = new Dinfo();
#endif      

		private void Update()
		{
			// process input         
			if (Input.GetKeyDown(ClearKey)) this.Clear();
			if (Input.GetKeyDown(NextKey)) ReadyForNext = true;
         
			// (re-)initialize if necessary
			if (TraceBuffer == null || TraceBuffer.count != MaxLength) Init((uint)this.MaxLength);
         
            // append next nearest particle to trace
			if (curLength >= MaxLength || ReadyForNext)
			{            
				this.AppendNext();
				ReadyForNext = false;
			}

			// update realtime positions, if enabled
            if (this.RealtimePositions)
            {
                // grab all realtime positions by index from the particles buffer
                this.IndexPicker.Pick(this.Particles.Get(), this.TraceBuffer, (from p in TracePoints select p.index).ToArray());
            }         
		}

		private void AppendNext()
		{
			if (curLength == 0 && TracePosTransform != null) this.TracePos = this.TracePosTransform.position;
         
			// find closest
			TracePoint closest = GetNearest(this.TracePos);
			if (closest == null) return;
         
			TracePoints[curLength] = closest;
         
			// move TracePos "forward"
			//Vector3 prevpos = curLength == 0 ? TracePos : TracePoints[curLength - 1].pos;
			this.TracePos = this.TracePos = closest.pos; //closest.pos + (closest.pos - prev);
         
			// update all "following" points
			curLength += 1;
			for (uint i = curLength; i < TracePoints.Length; i++)
			{
				TracePoints[i].index = closest.index;
				TracePoints[i].pos = closest.pos;
			}
         
			// write all our recorded positions
			if (!this.RealtimePositions)
			{
				Vector3[] positions = (from p in TracePoints select p.pos).ToArray();
				TraceBuffer.SetData(positions);
			}
         
#if UNITY_EDITOR
			this.DebugInfo.CurLength = (int)this.curLength;
			this.DebugInfo.Points = this.TracePoints;
#endif         
		}

		private TracePoint GetNearest(Vector3 pos) {
			var buf = this.Particles.GetValid();
			if (buf == null) return null;
         
            // don't consider particles that are already part of our trace
			var blacklist = (from p in TracePoints select p.index).ToArray();
			var nearestItem = this.FindNearest.GetNearest(buf, pos, blacklist);

			return nearestItem == null ? null : new TracePoint(nearestItem.index, nearestItem.pos);
		}

		private void Init(uint maxlen)
		{
			if (TraceBuffer != null) {
				TraceBuffer.Release();
				TraceBuffer.Dispose();
			}
         
			TraceBuffer = Populators.Utils.Create(new Vector3[MaxLength]);
			Trace.Set(TraceBuffer);
         
			TracePoints = new TracePoint[MaxLength];
			for (int i = 0; i < MaxLength; i++) TracePoints[i] = new TracePoint(0, new Vector3(0, 0, 0));
		}
      
		private void Clear() {
			for (uint i = 0; i < this.MaxLength; i++) {
				TracePoints[i].pos = this.TracePos;
				TracePoints[i].index = 0;
			}
         
			Vector3[] positions = (from p in TracePoints select p.pos).ToArray();
            TraceBuffer.SetData(positions);
         
			this.curLength = 0;
		}
	}
}