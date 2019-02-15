using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class CashPaymentInfo : PaymentInfo, IMenuItem
    {
        Decimal amount;
        int sequenceNo;

        public CashPaymentInfo()
        {
            this.amount = 0;
            this.sequenceNo = -1;
        }
        public CashPaymentInfo(Decimal amount)
            :this()
        {
            this.amount = amount;
        }

        public override String Name
        {
            get { return PosMessage.CASH; }
        }

        public override Decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public override int SequenceNo
        {
            get
            {
                return sequenceNo;
            }
            set
            {
                sequenceNo = value;
            }
        }

        public override string ToString()
        {            
            return this.Name;
        }
        public override PaymentInfo Clone()
        {
            return (PaymentInfo)this.MemberwiseClone();
        }

        #region IMenuItem Members

        public void Show()
        {
            Show(Target.Cashier);
        }

        public void Show(Target t)
        {
            DisplayAdapter.Cashier.Show(PosMessage.CASH, amount);
        }

        #endregion
    }
}
