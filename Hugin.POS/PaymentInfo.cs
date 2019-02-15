using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS
{
    public abstract class PaymentInfo
    {
        protected const long MAXAMOUNT = 999999999;
        protected const long MINAMOUNT = 0;

        public abstract String Name { get;}
        public abstract Decimal Amount { get;set;}
        public abstract int SequenceNo { get; set; }

        public Decimal MinimumPayment
        {
            get { return MINAMOUNT; }
        }
        public Decimal MaximumPayment
        {
            get { return MAXAMOUNT; }
        }
        public abstract PaymentInfo Clone();

        public PaymentInfo Void()
        {
            PaymentInfo pi = this.Clone();
            pi.Amount = pi.Amount * -1;
            return pi;
        }
    }
}
