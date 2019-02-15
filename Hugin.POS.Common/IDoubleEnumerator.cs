using System;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS.Common
{
	/// <summary>
	/// IDoubleEnumerator interface allows traversing 
	/// a Collection both ways.
	/// </summary>
	public interface IDoubleEnumerator : IEnumerator
	{
		bool MovePrevious();
        void MoveLast();
        void MoveFirst();
        void ShowCurrent();
        void ShowCurrent(Target target);
	}
}
