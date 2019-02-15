using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class CreditPaymentInfo : PaymentInfo, IMenuItem
    {
        ICredit credit;
        Decimal amount;
        int installments;
        bool isPaymentMade;
        String remark;
        int sequenceNo;

        public CreditPaymentInfo()
        {
            credit = null;
            amount = 0;
            installments = 0;
            this.sequenceNo = -1;
        }

        public CreditPaymentInfo(ICredit credit)
            :this()
        {
            this.credit = credit;
        }

        public CreditPaymentInfo(ICredit credit, Decimal amount)
            :this(credit)
        {
            this.amount = amount;
        }

        public CreditPaymentInfo(ICredit credit, Decimal amount, int installments):
            this(credit,amount)
        {
            this.installments = installments;
        }

        public int Id
        {
            get {
                if (credit == null)
                    return -1;
                return credit.Id; 
            }
        }

        public ICredit Credit
        {
            get { return credit; }
        }

        public override String Name
        {
            get
            {
                if (credit == null)
                    return ""; 
                return credit.Name; 
            }
        }

        public override Decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public int Installments
        {
            get { return installments; }
            set { installments = value; }
        }

        public String Remark
        {
            get { return remark; }
            set { remark = value; }
        }

        public bool IsPaymentMade
        {
            get { return isPaymentMade; }
            set { isPaymentMade = value; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override PaymentInfo Clone()
        {
            return (PaymentInfo)this.MemberwiseClone();
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

        #region IMenuItem Members

        public void Show()
        {
            Show(Target.Cashier);
        }

        public void Show(Target t)
        {
            if (t == Target.Cashier)
                DisplayAdapter.Cashier.Show(this.credit);
            else if (t == Target.Customer)
                DisplayAdapter.Customer.Show(this.credit);
            else
                DisplayAdapter.Both.Show(this.credit);
        }

        #endregion

        public static MenuList GetCredits()
        {
            Dictionary<int, ICredit> credits = CashRegister.DataConnector.GetCredits();
            MenuList menu=new MenuList();
            foreach (int key in credits.Keys)
                menu.Add(new CreditPaymentInfo(credits[key]));
            return menu;
        }
    }
}
