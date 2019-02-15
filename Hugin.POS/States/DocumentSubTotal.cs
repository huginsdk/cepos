using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    
    class DocumentSubTotal : DocumentState
    {
        private static DocumentState state = new DocumentSubTotal();
                /// <summary>
        /// Shows document subtotal to cashier.
        /// Two types:
        ///     First: If customer autho. adjustment is active;show document subtotal wtih the discounted value.
        ///     Second: If customer autho is not active; show only discounted value.
        /// </summary>
        /// <returns>
        ///Document state:DocumentOpen,DocumentSubtotal or DocumentPayment. 
        ///</returns>
        public static DocumentState Instance()
        {
            if (cr.Document.IsEmpty) return state;

            String strMessage = String.Format("{0}\n{1:C}", PosMessage.SUBTOTAL, new Number(cr.Document.BalanceDue));
            if (cr.Document.Status == DocumentStatus.Active)
            {
                PromotionDocument doc = new PromotionDocument(cr.Document, null, PromotionType.Document);

                if (doc.HasAdjustment)
                {
                    strMessage = DisplayAdapter.AmountPairFormat(PosMessage.SUBTOTAL,
                                                                  cr.Document.BalanceDue,
                                                                  PosMessage.DISCOUNTED,
                                                                  doc.BalanceDue);
                    DisplayAdapter.Both.Show(strMessage); 
                    return state;
                }
            }

            DisplayAdapter.Both.Show(strMessage);

            if (!(cr.Document.State is DocumentPaying || cr.Document.State is DocumentSubTotal))
            {
                bool hardcopy = cr.DataConnector.CurrentSettings.GetProgramOption(Setting.PrintSubtTotal) == PosConfiguration.ON;
                cr.Printer.PrintSubTotal(cr.Document, hardcopy);
            }
            DisplayAdapter.Customer.Show("{0}\n{1:C}", PosMessage.SUBTOTAL, new Number(cr.Document.BalanceDue));
            return state;
        }
        /// <summary>
        /// Add fiscal item to document list.
        /// </summary>
        /// <param name="doc">
        /// Current document list.
        /// </param>
        /// <param name="item">
        /// Item that will be added.
        /// </param>
        public override void AddItem(SalesDocument doc, FiscalItem item)
        {
            doc.State = DocumentOpen.Instance();
            doc.State.AddItem(doc, item);
        }

        /// <summary>
        /// Subtotal String format.
        /// </summary>
        /// <returns>
        /// Subtotal.
        /// </returns>
        public override string ToString()
        {
            return "ARATOP";
        }

    }
}
