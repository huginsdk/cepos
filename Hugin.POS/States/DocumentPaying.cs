using System;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    
    class DocumentPaying : DocumentState
    {
        private static DocumentState state = new DocumentPaying();

        public static DocumentState Instance()
        {
            return state;
        }
        /// <summary>
        /// After payment is started,document must be closed to sell or void new item.
        /// If cashier try to sell or void item,it does not permitted and alerted by the system to close sales.
        /// </summary>
        /// <param name="doc">
        /// Not used in this state.
        /// </param>
        /// <param name="item">
        /// Not used in this state.
        /// </param>
        /// <exception cref="SaleClosedException">
        /// Alerts cashier to close sales document in order to add new sales item.
        /// </exception>
        public override void AddItem(SalesDocument doc, FiscalItem item)
        {
            throw new SaleClosedException(String.Format("{0}\n{1}", PosMessage.RECEIVE_PAYMENT, 
                                                                    PosMessage.PROMPT_FINALIZE_SALE));
        }

        public override string ToString()
        {
            return PosMessage.PAYMENT;
        }

    }
}
