using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Computils.Processors.Downloaders
{
	public class Vector3Downloader : MonoBehaviour
	{
		public ComputeBufferFacade SourceFacade;

		[System.Serializable]
		public class VectorsEvent : UnityEvent<Vector3[]> {}
      
		public VectorsEvent ValuesEvent;
      
		private Vector3[] downloadedValues;
      
		public Vector3[] Values { get {
				return this.downloadedValues == null ? new Vector3[0] : downloadedValues; }}

		#region Public Action Methods
		public void Download(int amount) {
			var buf = SourceFacade == null ? null : SourceFacade.GetValid();
            if (buf == null) return;
         
			this.downloadedValues = new Vector3[amount];         
			buf.GetData(downloadedValues, 0, 0, amount);
         
			this.ValuesEvent.Invoke(this.downloadedValues);
		}
		#endregion
	}
}