using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    
    class DocumentOpen : DocumentState
    {
        private static DocumentState state = new DocumentOpen();

        /// <summary>
        /// Document state instance
        /// </summary>
        /// <returns></returns>
        public static DocumentState Instance()
        {
            return state;
        }

        /// <summary>
        /// Add new fiscal item to document list.
        /// fiscal item: Void item or sales item. 
        /// </summary>
        /// <param name="doc">
        /// Document list.
        /// </param>
        /// <param name="item">
        /// Item that will be added to document list.
        /// </param>
        public override void AddItem(SalesDocument doc, FiscalItem item)
        {
            Decimal newTotal = 0;
            if (item.TotalAmount >= 10000000)
                throw new OverflowException(PosMessage.ITEM_TOTAL_AMOUNT_LIMIT_EXCEED_ERROR);
            if (item.TotalAmount + doc.TotalAmount >= 100000000)
                throw new OverflowException(PosMessage.DOCUMENT_TOTAL_AMOUNT_LIMIT_EXCEED_ERROR);
            if (doc.ProductTotals.ContainsKey(item.Product))
            {
                newTotal = doc.ProductTotals[item.Product] + item.TotalAmount;
                if (newTotal < 0)
                    throw new VoidException(PosMessage.VOID_AMOUNT_INVALID);
                else if (newTotal == 0)
                    doc.ProductTotals.Remove(item.Product);
                else
                    doc.ProductTotals[item.Product] += item.TotalAmount;

              
            }
            else { 
                if (item is VoidItem)
                    throw new VoidException(PosMessage.CANNOT_VOID_NO_PROPER_SALE);
                else doc.ProductTotals[item.Product] = item.TotalAmount;
            }

           doc.Items.Add(item);
        }

        public override string ToString()
        {
            return "";
        }

        
    }
}
