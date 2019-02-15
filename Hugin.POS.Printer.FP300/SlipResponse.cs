using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    class SlipResponse : IPrinterResponse
    {
        private String response = String.Empty;
        private DateTime createdTime;
        private string data;
        private string detail;
        private int printerStatus = 0;

        internal SlipResponse() { }

        internal SlipResponse(String response)
        {

            createdTime = DateTime.Now;

            printerStatus = int.Parse(response);

            detail = "";
            int divisor = 128;
            while (true)
            {
                detail = (printerStatus / divisor) + detail;
                printerStatus = printerStatus % divisor;
                divisor = divisor / 2;
                if (divisor == 0) break;
            }
        }

        internal DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }

        public string Data
        {
            get { return data; }
            set { data = value; }
        }

        public String Detail
        {
            get { return detail; }
            set { detail = value; }
        }

        public bool HasError
        {
            get { return false; }
        }

        internal int PrinterStatus
        {
            get
            {
                return printerStatus;
            }
        }

        public override String ToString()
        {
            return response;
        }

    }
}
