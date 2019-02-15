using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface IPointAdapter
    {
        bool Online { get; }
        bool Invalid(String cardSerial);
        int InvalidateSerials(ICustomer customer);
        void AddCard(ICustomer customer, string cardSerial);
        void UpdatePoint(PointObject pointObj);
        long GetPoint(ICustomer customer);
    }
}
