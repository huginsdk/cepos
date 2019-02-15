using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Data
{
    class Credit:ICredit
    {
        int id;
        string name;
        bool isTicket = false;
        bool payViaEft = false;
        bool isPointPayment = false;

        static Dictionary<int, ICredit> credits;
        static Dictionary<int, ICredit> backupCredits;

        #region ICredit Members

        public int Id
        {
            get { return id; }
        }

        public String Name
        {
            get { return name; }
        }

        public bool PayViaEft
        {
            get { return payViaEft; }
        }

        public bool IsTicket
        {
            get { return isTicket; }
        }

        public bool IsPointPayment
        {
            get { return isPointPayment; }
        }

        #endregion

        /* it is not accessor like ' Dictionary<int, ICurrency> Credits{ get { return credits;} }
         * because if return value is a collection, items of collection can be changed 
         * when accessor is used
         */
        internal static Dictionary<int, ICredit> GetCredits()
        {
            return credits;
        }

        internal static void Backup()
        {
            backupCredits = credits;
            credits = new Dictionary<int, ICredit>();
        }

        internal static void Restore()
        {
            if (credits.Count == 0)
                credits = backupCredits;

            if (backupCredits != null)
                backupCredits.Clear();
        }

        internal static bool Add(String line)
        {
            try
            {
                Credit credit = new Credit();
                string[] keyvalue = line.Split('=');
                if (keyvalue.Length < 2) return false;

                Parser.TryInt(keyvalue[0], out credit.id);
                if (keyvalue[1].StartsWith("#T"))
                {
                    credit.name = keyvalue[1].Substring(2);
                    credit.isTicket = true;
                }else if (keyvalue[1].StartsWith("#E"))
                {
                    credit.name = keyvalue[1].Substring(2);
                    credit.payViaEft = true;
                }
                else if(keyvalue[1].StartsWith("#P"))
                {
                    credit.name = keyvalue[1].Substring(2);
                    credit.isPointPayment = true;
                }
                else
                {
                    credit.name = keyvalue[1];
                    credit.isTicket = false;
                }
                
                if (credit.name.Length > 20)
                    credit.name = credit.name.Substring(0, 20);

                if (credits.ContainsKey(credit.Id))
                    throw new Exception("Credit.Add : Id (" + credit.id + ") Exists");
                credits.Add(credit.Id, credit);

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
