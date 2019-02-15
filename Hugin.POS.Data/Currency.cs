using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Data
{
    class Currency:ICurrency
    {
        int id;
        String name;
        Decimal exchangeRate;

        static Dictionary<int, ICurrency> currencies;
        static Dictionary<int, ICurrency> backupCurrencies;

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

        /* it is not accessor like ' Dictionary<int, ICurrency> Currencies{ get { return currencies;} }
         * because if return value is a collection, items of collection can be changed 
         * when accessor is used
         */ 
        internal static Dictionary<int, ICurrency> GetCurrencies()
        {
            return currencies;
        }

        internal static void Backup()
        {
            backupCurrencies = currencies;
            currencies = new Dictionary<int, ICurrency>();
        }

        internal static void Restore()
        {
            if (currencies.Count == 0)
                currencies = backupCurrencies;

            if (backupCurrencies != null)
                backupCurrencies.Clear();
        }
        internal static bool Add(String line)
        {
            try
            {
                string[] keyvalue = line.Split(',');
                if (keyvalue.Length < 3)
                    throw new Exception("Currency.Add : Invalid Line");

                Currency currency = new Currency();
                currency.id = (int)keyvalue[0][0];
                currency.name = keyvalue[1].TrimEnd();
                string temp = keyvalue[2].Replace(".", Number.DecimalSeperator);
                currency.exchangeRate = Decimal.Parse(temp) / 100;

                if (currencies.ContainsKey(currency.id))
                    throw new Exception("Currency.Add : Id (" + currency.id + ") Exists");
                currencies.Add(currency.id, currency);

                return true;
            }
            catch (Exception e)
            {
                PosException lastException = new PosException(e.Message, e);
                lastException.Data.Add("ErrorLine", line);
                EZLogger.Log.Warning(lastException);
                return false;
            }
        }
    }
}
