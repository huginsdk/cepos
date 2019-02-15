using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class CurrencyPaymentInfo : PaymentInfo, IMenuItem
    {
        ICurrency currency;
        Decimal amount;
        int sequenceNo;

        public CurrencyPaymentInfo()
        {
            currency = null;
            amount = 0;
            this.sequenceNo = -1;
        }

        public CurrencyPaymentInfo(ICurrency currency)
            :this()
        {
            this.currency = currency;
        }

        public CurrencyPaymentInfo(ICurrency currency, decimal amount)
            :this(currency)
        {
            this.amount = amount;
        }

        public int Id
        {
            get {
                if (currency == null)
                    return -1;
                return currency.Id;
            }
        }
        public ICurrency Currency
        {
            get { return currency; }
        }
        public override String Name
        {
            get
            {
                if (currency == null)
                    return "";
                return currency.Name;
            }
        }

        public override Decimal Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public Decimal ExchangeRate
        {
            get { return currency.ExchangeRate; }
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
            
            Number currencyPayment = new Number(this.Amount / this.ExchangeRate);

            /*string[] tmpArray = currencyPayment.ToString().Split(currencyPayment.OverrideDecimalSeperator.ToCharArray());
            char[] charArray = tmpArray[1].ToCharArray();
            string tmp = tmpArray[0] + currencyPayment.OverrideDecimalSeperator + charArray[0] + charArray[1];
            currencyPayment = new Number(tmp);*/

            if (currencyPayment.ToDecimal() > currencyPayment.ToInt())
                return String.Format("{0} {1:C}", this.Name, currencyPayment);

            return String.Format("{0} {1}", this.Name, currencyPayment);
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
            if (t == Target.Cashier)
                DisplayAdapter.Cashier.Show(this.currency);
            else if (t == Target.Customer)
                DisplayAdapter.Customer.Show(this.currency);
            else
                DisplayAdapter.Both.Show(this.currency);
        }

        #endregion

        public static MenuList GetCurrencies()
        {
            Dictionary<int, ICurrency> currencies =CashRegister.DataConnector.GetCurrencies();
            MenuList menu = new MenuList();
            foreach (int key in currencies.Keys)
                menu.Add(new CurrencyPaymentInfo(currencies[key]));
            return menu;
        }
    }
}
