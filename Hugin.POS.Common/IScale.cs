using System;

namespace Hugin.POS.Common
{
    /// <summary>
    /// IScale is the interface to handle a scale.
    /// Provides access to members that control the scale driver. 
    /// </summary>
    public interface IScale 
	{
		/// <summary>
		/// Allows connect to scale. 
		/// Com port name is gets from config file in line of "ScaleComPort"
		/// </summary>
		void Connect();
		/// <summary>
		/// Gets weight to be set product weight
		/// </summary>
		/// <returns>Product weight</returns>
		Decimal GetWeight(decimal unitPrice);
		/// <summary>
		/// Gets a value indicating whether the current scale is connect
		/// True : Scale connection is available, otherwise is not.
		/// </summary>
		bool IsConnect { get;}
	}
}

