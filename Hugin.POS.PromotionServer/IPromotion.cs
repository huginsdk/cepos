using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.PromotionServer
{
    internal interface IPromotion
    {
        bool PromotionCoverAll(PromotionRange range);
        String PromotionCode { get;}
        int Points { get;}
        int ExtraPoints { get;}
        bool CheckPaymentType(String type);
        decimal TotalAmount();
        decimal LineQuantity();
        void ApplyPromotion();
        long PointEarned { get;}
    }
}
