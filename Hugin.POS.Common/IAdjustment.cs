using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public enum AdjustmentType : sbyte
    {
        Fee,
        PercentFee,
        Discount,
        PercentDiscount
    }
    public interface IAdjustment
    {
        IAdjustable Target { get;}

        Decimal NetAmount { get;set;}

        Decimal RequestValue { get;}

        AdjustmentType Method { get;}

        String Label { get;set;}

        Decimal[] GetTaxGroupAdjustments();

    }
}
