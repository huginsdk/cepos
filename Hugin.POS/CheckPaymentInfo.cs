using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class CheckPaymentInfo : PaymentInfo,IMenuItem
    {
        Decimal amount;
        String refNumber;
        int sequenceNo;

        public CheckPaymentInfo()
        {
            this.amount = 0;
            this.refNumber = "";
            this.sequenceNo = -1;
        }
        public CheckPaymentInfo(Decimal amount, String refNumber)
            :this()
        {
            this.amount = amount;
            this.refNumber = refNumber;
        }

        public String RefNumber
        {
            get { return refNumber; }
            set { refNumber = value; }
        }

        public override String Name
        {
            get { return PosMessage.CHECK; }
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
            DisplayAdapter.Cashier.Show(PosMessage.CHECK, amount);
        }

        #endregion
    }
}
