using System;
using UnityEngine;

namespace Computils
{
	class ThreadSize
	{
		private uint count;
		private uint threadx = 1, thready = 1, threadsZ = 1;

		private uint unitx, unity, resx, resy;

		public ThreadSize(uint count, uint x, uint y)
		{
			this.count = count;
			this.threadx = x;
			this.thready = y;
         
			// calc
			float v = (float)count / (float)threadx;

			this.unity = (uint)Mathf.FloorToInt(v / (float)thready);
			this.resy = this.unity * thready;

			this.resx = (uint)Mathf.FloorToInt((float)count / this.resy);
			this.unitx = (uint)Mathf.FloorToInt((float)resx / threadx);
			this.resx = unitx * threadx;
		}
      
		public Vector2Int UnitSize
		{
			get
			{
				return new Vector2Int((int)this.unitx, (int)this.unity);
			}
		}

		public Vector2Int Resolution
		{
			get
			{
				return new Vector2Int((int)this.resx, (int)this.resy);
			}
		}
	}
}