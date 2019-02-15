using System;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class Receipt : SalesDocument, IMenuItem, ICloneable
    {

        int id;

        public Receipt()
        {

        }

        public Receipt(SalesDocument doc)
            : base(doc)
        {
            if (TotalAmount > CashRegister.DataConnector.CurrentSettings.ReceiptLimit)
                throw new ReceiptLimitExceededException();
        }

        public override object Clone()
        {
            return new Receipt(this);
        }

        public sealed override int Id
        {
            get { return id; }
            set { id = value; }
        }

        public override String Name { get { return PosMessage.RECEIPT_TR; } }
        public override String Code { get { return PosMessage.HR_CODE_RECEIPT; } }

        public override bool CanAddItem(FiscalItem item)
        {
            if (TotalAmount + item.TotalAmount > CashRegister.DataConnector.CurrentSettings.ReceiptLimit)
                throw new ReceiptLimitExceededException();
            return base.CanAddItem(item);
        }

        public override bool CanAdjust(Adjustment adjustment)
        {
            if (TotalAmount + adjustment.NetAmount > CashRegister.DataConnector.CurrentSettings.ReceiptLimit)
                throw new ReceiptLimitExceededException();
            return base.CanAdjust(adjustment);
        }

    }
}


   
    

