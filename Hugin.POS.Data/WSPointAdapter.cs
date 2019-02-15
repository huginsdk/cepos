using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
/*
#if WindowsCE
using Data.HuginWSCF;
#else
using Hugin.POS.Data.HuginWS;
#endif
*/
namespace Hugin.POS.Data
{
    class WSPointAdapter : IPointAdapter
    {
        private static bool isOnline = false;
        /*
        HuginWebService posService = new HuginWebService();
        */
        internal WSPointAdapter(String path)
        {
            /*
            posService.Url = path;
            isOnline = true;
          */
        }

        public bool Online
        {
            get
            {
                return isOnline;
            }
        }
        public void AddCard(ICustomer customer, string cardSerial)
        {
            //posService.AddCustomer(customer.Number, cardSerial)
        }

        public void UpdatePoint(PointObject pointObj)
        {
            //posService.UpdatePoint(customer.Number, fiscalRegisterNo, zno, docId, documentDate, point, definition)
        }

        public long GetPoint(ICustomer customer)
        {
            //return posService.GetPoint(customer.Number);

            return -1;
        }
        public bool Invalid(string cardSerial)
        {
            //return posService.Invalid(cardSerial);
            return false;
        }

        public int InvalidateSerials(ICustomer customer)
        {
            //return posService.InvalidateSerials(customer.Number);
            return -1;
        }

    }
}
