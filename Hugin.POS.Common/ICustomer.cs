using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface ICustomer
    {
        String Code { get; }
        String CustomerGroup { get;}
        String Number { get; }
        String Name { get;}
        String[] Identity { get; }
        String[] Contact { get; }
        long Points { get;}
        int PromotionLimit { get; }
        DocumentTypes DefaultDocumentType { get; }
        bool IsDiplomatic { get;}
        long UpdatePoint(PointObject pointObj);
        String GsmNumber { get; set;}

        string GetCustomValue(string customKey);
    }
}
