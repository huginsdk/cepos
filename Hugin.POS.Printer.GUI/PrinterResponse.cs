using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Printer.GUI
{
    internal class PrinterResponse : IPrinterResponse
    {
        #region IPrinterResponse Members

        bool hasError = false;
        string data = "";
        string detail = "";
        public bool HasError
        {
            get {return hasError; }
        }

        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        public string Detail
        {
            get
            {
                return detail;
            }
            set
            {
                detail = value;
            }
        }

        #endregion
    }
}
