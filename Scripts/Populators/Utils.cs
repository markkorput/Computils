using System.Collections;
using UnityEngine;


namespace Computils.Populators
{
	public class Utils
	{      

		public static ComputeBuffer Create(uint[] data)
        {
            ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(uint));
            buffer.SetData(data);
            return buffer;
        }

        public static ComputeBuffer Create(float[] data)
        {
            ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float));
            buffer.SetData(data);
            return buffer;
        }

		public static ComputeBuffer Create(Vector3[] data, int offset=0)
		{
			ComputeBuffer buffer = new ComputeBuffer(data.Length+offset, sizeof(float) * 3);
			buffer.SetData(data, 0, offset, data.Length);
			return buffer;         
		}

		public static ComputeBuffer Create(Matrix4x4[] data)
        {
			ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float) * 4 * 4);
			buffer.SetData(data);
            return buffer;
        }

		public static ComputeBuffer UpdateOrCreate(ComputeBuffer buf, Matrix4x4[] data)
        {
            int count = data.Length;

            if (buf != null && buf.count == count)
            {
                buf.SetData(data);
                return buf;
            }

            if (buf != null)
            {
                buf.Release();
                buf.Dispose();

            }
            
            return Create(data);
        }

		public static ComputeBuffer UpdateOrCreate(ComputeBuffer buf, Vector3[] data)
        {
            int count = data.Length;

            if (buf != null && buf.count == count)
            {
                buf.SetData(data);
                return buf;
            }
         
            if (buf != null)
            {
                buf.Release();
                buf.Dispose();

            }
         
            return Create(data);
        }

		public static ComputeBuffer UpdateOrCreate(ComputeBuffer buf, uint[] data)
        {
            int count = data.Length;
         
            if (buf != null && buf.count == count)
            {
                buf.SetData(data);
                return buf;
            }

            if (buf != null)
            {
                buf.Release();
                buf.Dispose();

            }

            return Create(data);
        }
      
		public static ComputeBuffer UpdateOrCreate(ComputeBuffer buf, float[] data)
        {
            int count = data.Length;

			if (buf == null) return Create(data);

			if (buf.count != count || buf.stride != sizeof(float))
            {
                buf.Release();
                buf.Dispose();
				return Create(data);
            }
         
			buf.SetData(data);
			return buf;
        }
      
		public static ComputeBuffer UpdateOrCreate(ComputeBuffer buf, Vector3[] data, int offset) {
			int count = data.Length;

			if (buf != null && buf.count >= (offset + data.Length))
            {
				buf.SetData(data, 0, offset, data.Length);
                return buf;
            }

            if (buf != null)
            {
                buf.Release();
                buf.Dispose();            
            }
         
            return Create(data, offset);
		}

		public static ComputeBuffer UpdateCircular(ComputeBuffer buf, Vector3[] data, int offset)
		{
			int count = data.Length;
			int done = 0;

			int srcOffset = 0;
			int destOffset = offset;

			while (done < count)
			{
				int batch = Mathf.Min((count-done), buf.count - offset);
				buf.SetData(data, srcOffset, destOffset, batch);

				done += batch;
				srcOffset += batch;
				destOffset += batch;
				if (destOffset >= buf.count) destOffset = 0;
			}
         
			return buf;
		}
	} 
}