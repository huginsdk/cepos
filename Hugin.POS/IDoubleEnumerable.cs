using System;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS
{
	/// <summary>
	/// IDoubleEnumerable interface allows traversing
	///  a Collection both ways.
	/// </summary>
	public interface IDoubleEnumerable : IEnumerable
	{
		new IDoubleEnumerator GetEnumerator();
	}
}
