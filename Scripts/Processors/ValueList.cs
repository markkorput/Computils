using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Computils.Processors
{
	/// <summary>
	/// ValuePicker takes two ComputeBuffers of the same length; one with Vector3 items (positions)
	/// and one with single float values. It fetches
    /// </summary>
	[AddComponentMenu("Computils/Porcessors/ValueList")]
	public class ValueList : MonoBehaviour
	{
		private static class ShaderProps
		{
			public const string KernelIndicesGTE = "CSIndicesGTE";
			public const string KernelIndicesLTE = "CSIndicesLTE";
			public const string values_buf = "values_buf";
			public const string indices_buf = "indices_buf";
			public const string feedback_buf = "feedback_buf";

			public const string ReferenceValue = "ReferenceValue";
			public const string ValuesCount = "ValuesCount";
			public const string MaxValues = "MaxValues";
		}

		public const int NO_MAX = -1;
		public enum ModeType { GreaterThanOrEqual, LessThanOrEqual };

		public ComputeShader Shader;

		public ComputeBufferFacade ValuesFacade;
		public ModeType Mode = ModeType.GreaterThanOrEqual;
		public float ReferenceValue;
        [Tooltip("less than zero will default to the length of the ValuesFacade buffer")]
		public int MaxValues = NO_MAX;

		[System.Serializable]      
		public class IndicesEvent : UnityEvent<uint[]> {}
		public IndicesEvent OnIndices;

		private ShaderRunner runner;
		private ComputeBuffer resultBuf, feedback_buf;
		private uint[] indices;      
      
		//void Start()
		//{
		//	this.runner = CreateShaderRunner(this.Mode);
		//}

		private ShaderRunner CreateShaderRunner(ModeType mode) {
			switch (mode)
            {
                case ModeType.GreaterThanOrEqual:
                    return ShaderRunner.Create(this.Shader, ShaderProps.KernelIndicesGTE, 1, 1);
                case ModeType.LessThanOrEqual:
                    return ShaderRunner.Create(this.Shader, ShaderProps.KernelIndicesLTE, 1, 1);
            }

			return null;         
		}

		private void Update()
		{
			this.UpdateIndices();
		}
      
		private bool LoadIndices() {
			// make sure we have a valid values buffer
			var values_buf = this.ValuesFacade.GetValid();
			if (values_buf == null) return false;
         
			// maximum number of indices to load (defaults to length of the values buffer)
			int max = this.MaxValues <= 0 ? values_buf.count : this.MaxValues;

            // create/resize reuslt buffer if necessary
			if (this.resultBuf == null || this.resultBuf.count < max) {
				if (this.resultBuf != null)
				{
					this.resultBuf.Release();
					this.resultBuf.Dispose();
				}
            
				this.resultBuf = new ComputeBuffer(max, sizeof(uint));
			}

            // Create feedback buffer if necessary         
			if (this.feedback_buf == null) {
				this.feedback_buf = new ComputeBuffer(1, sizeof(uint));
			}

            // make sure our shader runner is initializer
			if (this.runner == null)
            {
                this.runner = CreateShaderRunner(this.Mode);
            }
         
            // run GPU program
			this.runner.Shader.SetInt(ShaderProps.MaxValues, max);
			this.runner.Shader.SetInt(ShaderProps.ValuesCount, values_buf.count);
			this.runner.Shader.SetFloat(ShaderProps.ReferenceValue, this.ReferenceValue);

			this.runner.Shader.SetBuffer(this.runner.Kernel, ShaderProps.values_buf, values_buf);
			this.runner.Shader.SetBuffer(this.runner.Kernel, ShaderProps.feedback_buf, this.feedback_buf);

			this.runner.RunOnce(this.resultBuf, ShaderProps.indices_buf);
         
			// first read the number of found indices from the feedback buffer
			uint[] feedback_data = new uint[1];
			this.feedback_buf.GetData(feedback_data);
			uint count = feedback_data[0];

			// fetch the indices
			this.indices = new uint[count];
			this.resultBuf.GetData(this.indices, 0, 0, (int)count);

            // report success
			return true;
		}
      
		#region Public Methods
		public void UpdateIndices() {
			if (this.LoadIndices())
			    this.OnIndices.Invoke(indices);
		}      
		#endregion
	}
}