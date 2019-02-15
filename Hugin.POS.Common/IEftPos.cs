using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Common
{
    /// <summary>
    /// IEftPos is the interface to handle a basic eft pos.
    /// </summary>
    public interface IEftPos
    {
        /// <summary>
        /// Occurs when the eft pos has warning
        /// </summary>
        event OnMessageHandler OnMessage;
        /// <summary>
        /// Connects to Eft Pos 
        /// </summary>
        /// <param name="address">Includes eft pos port or ip information</param>
        void Connect(String address); 
        /// <summary>
        /// Pays by credit.
        /// </summary>
        /// <param name="amount">Payment amount</param>
        /// <param name="installment">Installment quantity</param>
		IEftResponse Pay (decimal amount, int installment);
    }
}
