using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Computils.Populators
{
	public interface IPopulator 
	{
		void Populate(int amount);
		void Populate(int amount, int offset);
        /// <summary>
        /// Consecutive calls to this method will 
        /// </summary>
        /// <param name="amount">Amount.</param>
		void PopulateNext(int amount);
	}
}