using System;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    class Currency:ICurrency
    {
        int id;
        String name;
        Decimal exchangeRate;

        #region ICurrency Members

        public int Id
        {
            get { return id; }
        }

        public String Name
        {
            get { return name; }
        }

        public Decimal ExchangeRate
        {
            get { return exchangeRate; }
        }

        #endregion

        public Currency(int id, string name, decimal exchangeRate)
        {
            this.id = id;
            this.name = name;
            this.exchangeRate = exchangeRate;
        }
    }
}
