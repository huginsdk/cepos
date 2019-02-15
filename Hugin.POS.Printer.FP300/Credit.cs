using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    class Credit : ICredit
    {
        int id;
        string name;
        bool isTicket;
        bool payViaEft;
        bool isPointPayment;

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

        public Credit(int id, string name, bool payViaEFT, bool isTicket, bool isPointPayment)
        {
            this.id = id;
            this.name = name;
            this.payViaEft = payViaEFT;
            this.isTicket = isTicket;
            this.isPointPayment = isPointPayment;
        }
    }
}
