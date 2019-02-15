using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public struct PointObject
    {
        public ICustomer Customer;
        public Int64 Value;
        public DateTime DocumentDate;
        public String OfficeID;
        public String RegisterID;
        public String RegisterFiscalID;
        public String Description;
        public Int32 DocumentTypeID;
        public Int32 ZNo;
        public Int32 DocumentNo;
        public Decimal DocumentTotal;

        public override string ToString()
        {
            String format = "{0}&{1}&{2}&{3}&{4}&{5}&{6}&{7}&{8}&{9}&{10}";
            String toString = String.Format(format,
                                                this.Value,
                                                this.Customer.Code,
                                                this.DocumentDate,
                                                this.OfficeID,
                                                this.RegisterID,
                                                this.RegisterFiscalID,
                                                this.ZNo,
                                                this.DocumentNo,
                                                this.DocumentTotal,
                                                this.DocumentTypeID,
                                                this.Description
                                                );

            return toString;
        }
    }
}
