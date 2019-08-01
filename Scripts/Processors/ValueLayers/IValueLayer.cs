using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.ValueLayers
{
	public interface IValueLayer
	{
		void Apply(ComputeBuffer values_buf);
	}
}