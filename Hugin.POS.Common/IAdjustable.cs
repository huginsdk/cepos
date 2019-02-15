using System;

namespace Hugin.POS.Common
{
    public interface IAdjustable
    {
        Decimal Adjust(IAdjustment adjustment);
        Decimal TotalAmount { get;set;}
        Decimal ListedAmount { get;}
    }
}
