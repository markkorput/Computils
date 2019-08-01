using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Processors.ValueLayers
{
	public abstract class BaseValueLayer : MonoBehaviour, IValueLayer
	{
		public abstract void Apply(ComputeBuffer values_buf);
	}
}