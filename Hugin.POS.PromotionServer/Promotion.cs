
//#define ISKULTUR

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using bp = Hugin.POS.PromotionServer.BasePromotion;
using Hugin.POS.Common;

namespace Hugin.POS.PromotionServer
{
    internal enum PromotionRange
    {
        ALL,
        CUSTOMER,
        NONE,
    }
    internal enum PaymentMethods
    {
        NONE, CASH, FOREIGNCURRENCY, CREDIT, CHECK
    }
    internal class Promotion
    {
        //Promosyonlarýn belge notu ile birlikte $M,$U1,$U2,$R bilgilerinide listeler.
        //Key:"PromoCode+M,U1,U2,R,N"
        Dictionary<String, String> remarks = new Dictionary<String, String>();
        PaymentMethods paymentMethod = PaymentMethods.NONE;
        String paymentType = String.Empty;
        String customerCode;
        Decimal documentTotalAmount;
        Decimal totalDiscount = 0;
        Decimal paymentAmount;
        PromotionRange promotionRange = PromotionRange.ALL;
        SoldItem soldItems = null;
        bool isFirstPayment = false;

        internal static ISecurityConnector SecurityConnector
        {
            get
            {
                return Security.Connector.Instance();
            }
        }

        internal Promotion(String[] saleList, bool isFirstPayment)
        {
            String[] lineitems;
            Decimal quantity;
            Decimal amount;
            Int32 pluno;
            Int32 lineno;
            this.isFirstPayment = isFirstPayment;

            soldItems = SoldItem.Initialize();

            for (int cntr = 0; cntr < saleList.Length; cntr++)
            {
                
                lineitems = saleList[cntr].Split(',');

                if (lineitems.Length < 2) continue;
                try
                {
                    switch (lineitems[3])
                    {
                        case Message.SAT:
                        case Message.IPT:
                            int quantityLength = lineitems[4].IndexOf('.') + 4;
                            pluno = Int32.Parse(lineitems[4].Substring(quantityLength, 6));
                            lineno = Int32.Parse(lineitems[1]);
                            quantity = Decimal.Parse(lineitems[4].Substring(0, quantityLength)) / 1000;
                            amount = Decimal.Parse(lineitems[5].Substring(2, 10)) / 100;

                            int sign = lineitems[3] == Message.SAT ? 1 : -1;

                            IProduct p = FindProductByLabel(pluno);
                            if (p == null) break;

                            int categoryId = Convert.ToInt32(p.Category);

                            soldItems.Add(pluno, lineno, sign * quantity, sign * amount);
                            break;
                        case Message.NAK:
                            paymentMethod = PaymentMethods.CASH;
                            paymentType = "NAK";
                            paymentAmount = Decimal.Parse(lineitems[5].Substring(2, 10)) / 100;
                            break;
                        case Message.KRD:
                            paymentMethod = PaymentMethods.CREDIT;
                            paymentType = String.Format("K{0,2}", lineitems[4].Substring(lineitems[4].Length - 2, 2));
                            paymentAmount = Decimal.Parse(lineitems[5].Substring(2, 10)) / 100;
                            break;
                        case Message.CHK:
                            paymentMethod = PaymentMethods.CHECK;
                            paymentType = "CEK";
                            paymentAmount = Decimal.Parse(lineitems[5].Substring(2, 10)) / 100;
                            break;
                        case Message.DVZ:
                            paymentMethod = PaymentMethods.FOREIGNCURRENCY;
                            paymentType = String.Format("D{0,2}", lineitems[4].Substring(0, 1));
                            paymentAmount = Decimal.Parse(lineitems[5].Substring(2, 10)) / 100;
                            break;
                        case Message.TOP:
                            documentTotalAmount = Decimal.Parse(lineitems[5].Substring(2, 10)) / 100;
                            break;
                        case Message.END:
                            if (lineitems[4].Trim() != String.Empty)
                                promotionRange = PromotionRange.CUSTOMER;
                            customerCode = lineitems[4].Trim() + lineitems[5].Substring(0, 8).Trim();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Settings.Log(String.Format("Hata Olustu! {0}", ex.Message));

                }
            }
            if (paymentMethod == PaymentMethods.NONE)
                paymentAmount = documentTotalAmount;

            /*if the no sale line for the void line*/

            //foreach (int catId in saleCategories.Keys)
            //{
            //    saleCategories[catId].ClearNonconsistentVoids();
            //}
        }


        internal String[] CreateProductPromotion()
        {
            long pointsEarned = 0;

            ICustomer customer = null;
            if (SecurityConnector.CurrentCustomer != null && customerCode == SecurityConnector.CurrentCustomer.Code)
                customer = SecurityConnector.CurrentCustomer;

            foreach (IPromotion pr in Settings.SalePromotions)
            {
                if (!pr.PromotionCoverAll(this.promotionRange)) continue;
                if (this.promotionRange == PromotionRange.CUSTOMER)
                    if (customer == null || !((BasePromotion)pr).CheckCustomerGroup(customer)) continue;
                BasePromotion basePRM = (BasePromotion)pr;
                if (basePRM.LimitType != LimitType.NoLimit) continue;

                //compare promotions with requirement totals
                if (CheckLimit(basePRM))
                {
                    soldItems.SetPromotion(basePRM, false);
                }
            }
            
            //Apply product and category promotion to document
            BasePromotion[] tempPromotions = soldItems.Promotions;
            foreach (BasePromotion prm in tempPromotions)
            {
                ((IPromotion)prm).ApplyPromotion();
                pointsEarned += ((IPromotion)prm).PointEarned;
            }

            List<String> promoLines = soldItems.PromotedLines;

            if (promoLines.Count > 0)
                promoLines.Add(String.Format("00,KOD,{0}", Settings.PromoKey));
            for (int cntr = 0; cntr < promoLines.Count; cntr++)
            {
                promoLines[cntr] = String.Format("1,{0:D5},{1}", cntr + 1, promoLines[cntr]);
            }


            return promoLines.ToArray();
        }

        internal String[] CreatePromotionList()
        {
            if (documentTotalAmount == 0) return new String[] { };

            List<String> promoRemarks = new List<string>();
            long pointsEarned = 0;

            ICustomer customer = null;
            if (SecurityConnector.CurrentCustomer != null && customerCode == SecurityConnector.CurrentCustomer.Code)
                customer = SecurityConnector.CurrentCustomer;
            
            if (customer != null)
                remarks.Add("M", customer.Name);

            foreach (IPromotion pr in Settings.SalePromotions)
            {
                if (!pr.PromotionCoverAll(this.promotionRange)) continue;
                if (this.promotionRange == PromotionRange.CUSTOMER)
                    if (customer == null || !((BasePromotion)pr).CheckCustomerGroup(customer)) continue;
                BasePromotion basePRM = (BasePromotion)pr;

                IProduct giftProduct = null;
                if (basePRM.GiftProductLabelNo>0)
                    giftProduct = FindProductByLabel(basePRM.GiftProductLabelNo);
                if (basePRM.LimitType == LimitType.NoLimit && giftProduct == null && basePRM.Points == 0) continue;
                //compare promotions with requirement totals
                if (CheckLimit(basePRM))
                {
                    soldItems.SetPromotion(basePRM, false);
                    if (giftProduct != null && soldItems.LineTotal(basePRM.GiftProductLabelNo) > 0m)
                    {
                        if (basePRM.LimitType == LimitType.Amount)
                        {
                            soldItems.GiveAsPromotion(basePRM);
                        }
                        else if (soldItems.LineTotal(basePRM) >= basePRM.RequiredQuantity)
                        {
                            //calculates how many times promotion will applied
                            int giftRatio = (int)(soldItems.LineTotal(basePRM) / basePRM.RequiredQuantity);
                            for (int i = 0; i < giftRatio; i++)
                                soldItems.GiveAsPromotion(basePRM);
                        }
                    }
                }

            }

            //Apply product and category promotion to document
            BasePromotion[] tempPromotions = soldItems.Promotions;
            foreach (BasePromotion prm in tempPromotions)
            {
                ((IPromotion)prm).ApplyPromotion();
                pointsEarned += ((IPromotion)prm).PointEarned;
            }
            //Gets total discount amount
            totalDiscount = soldItems.TotalDiscount();
            List<String> promoLines = soldItems.PromotedLines;

            String promocodeApplied = "";
            List<TotalPromotion> giftPromotions = new List<TotalPromotion>();

            //define best total promotion
            TotalPromotion bestTotalPromotion = null;
            //Decimal promotedTotal = paymentAmount - totalDiscount;
            Decimal promotedTotal = documentTotalAmount - totalDiscount;
            foreach (IPromotion pr in Settings.TotalPromotions)
            {
                if (!pr.PromotionCoverAll(this.promotionRange)) continue;
                if (!pr.CheckPaymentType(paymentType)) continue;
                if ((this.promotionRange == PromotionRange.CUSTOMER) && (customer == null || !((TotalPromotion)pr).CheckCustomerGroup(customer))) continue;

                TotalPromotion totalPromo = (TotalPromotion)pr;
                if (totalPromo.GiftProductLabelNo > 0)
                {
                    giftPromotions.Add((TotalPromotion)pr);

#if ISKULTUR
                    IProduct product = FindProductByLabel(totalPromo.GiftProductLabelNo);
                    Decimal quantity = soldItems.LineTotal(totalPromo.GiftProductLabelNo);

                    Decimal amountWithoutProduct = promotedTotal - product.UnitPrice * quantity;
                    if (amountWithoutProduct >= totalPromo.RequiredAmount)
                        promotedTotal = amountWithoutProduct;
#endif
                }
                else
                {
                    Decimal requiredAmount = totalPromo.HavePayment ? promotedTotal : (documentTotalAmount - totalDiscount);
                    //select best total promotion as discount amount
                    if (totalPromo.RequiredAmount <= requiredAmount)
                        bestTotalPromotion = bestTotalPromotion == null ? totalPromo : ComparePromotions(bestTotalPromotion, totalPromo, documentTotalAmount);
                }
            }

            Decimal discountTotalPromotion = 0, discountCustomerPromotion = 0;

            if (bestTotalPromotion != null)
            {
                // Promosyonun ödeme tipine baðlý deðilse promosyon ödeme parçalý olsa da aratoplama uygulanýyor.
                //if (!bestTotalPromotion.HavePayment)
                //{
                //    paymentAmount = documentTotalAmount;
                //    promotedTotal = paymentAmount - totalDiscount;
                //}
            }

            //Qunatity to be promotion applied
            int ratio = 0;
            if (bestTotalPromotion != null)
            {
                ratio = (int)((promotedTotal) / bestTotalPromotion.RequiredAmount);
                discountTotalPromotion = Math.Max(bestTotalPromotion.Discount, promotedTotal * bestTotalPromotion.PercentDiscount / 100m);
                if (bestTotalPromotion.RequiredAmount <= promotedTotal)
                    discountTotalPromotion = Math.Max(bestTotalPromotion.Discount * ratio, promotedTotal * bestTotalPromotion.PercentDiscount / 100m);
                else discountTotalPromotion = discountTotalPromotion > totalDiscount ? discountTotalPromotion : 0;

               //pointsEarned += (bestTotalPromotion.Points + bestTotalPromotion.ExtraPoints) * ratio;
            }

            //Apply gift product promotions
            Decimal promoReqAmount = promotedTotal;

#if ISKULTUR
            promoReqAmount -= discountTotalPromotion;
#endif

            while (true)
            {
                TotalPromotion tp = BestGiftPromotion(giftPromotions, promoReqAmount);
                if (tp == null) break;
                ratio = (int)((promoReqAmount) / tp.RequiredAmount);
                IProduct giftProduct = FindProductByLabel(tp.GiftProductLabelNo);
                Decimal lineQuantity = soldItems.LineTotal(tp.GiftProductLabelNo);
                for (int i = 0; i < ratio && lineQuantity > 0; i++)
                {
                    promoReqAmount -= tp.RequiredAmount;
                    decimal lineDiscount = Math.Max(tp.Discount, giftProduct.UnitPrice * tp.PercentDiscount / 100m);
                    long linePoint = (tp.Points + tp.ExtraPoints);

                    if (lineQuantity < 1)
                    {
                        lineDiscount = lineDiscount * lineQuantity;
                        linePoint = (long)(linePoint * lineQuantity);
                    }

#if ISKULTUR

                    promoLines.Add(
                            String.Format("06,IND,SAT {0:D4} %{1:D2},  {2:D10}",
                                            soldItems.GetSalesID((BasePromotion)tp)[i],
                                            (int)0,
                                            (int)Math.Round(lineDiscount * 100, 0)));

                    if (promoLines.Count > 0)
                        promoLines.Add(String.Format("00,KOD,{0}", Settings.PromoKey));
                    for (int cntr = 0; cntr < promoLines.Count; cntr++)
                    {
                        promoLines[cntr] = String.Format("1,{0:D5},{1}", cntr + 1, promoLines[cntr]);
                    }

                    pointsEarned += linePoint;
                    lineQuantity = soldItems.LineTotal(tp.GiftProductLabelNo);
#else
                    discountTotalPromotion += lineDiscount;
                    pointsEarned += linePoint;
                    soldItems.GiveAsPromotion(tp);
                    lineQuantity = soldItems.LineTotal(tp.GiftProductLabelNo);
#endif
                }
            }

#if ISKULTUR
            promotedTotal = documentTotalAmount;
#endif
            promotedTotal -= discountTotalPromotion;

            String[] appliedCodes = soldItems.AppliedCodes;
            foreach (String appliedCode in appliedCodes)
            {
                if (appliedCode.Length > 0 && !Str.Contains(promocodeApplied,(appliedCode.Trim() + ",")))
                {
                    promocodeApplied = promocodeApplied + appliedCode.Trim();
                }
            }
            TotalPromotion bestPointPromo = null;

            if (customer != null)
                discountCustomerPromotion = documentTotalAmount * customer.PromotionLimit / 100m;

            promotedTotal -= discountCustomerPromotion;
            if (discountCustomerPromotion > 0)
                promocodeApplied += "M";

            if (bestTotalPromotion != null)
                promocodeApplied += bestTotalPromotion.PromotionCode;

            //decimal ttlDiscount = paymentAmount - promotedTotal - totalDiscount;
            decimal ttlDiscount = 0;
            if(isFirstPayment)
                ttlDiscount = documentTotalAmount - promotedTotal - totalDiscount;


            if (paymentAmount > promotedTotal && isFirstPayment)
                paymentAmount = promotedTotal;

            bestPointPromo = BestPointPromotion(paymentAmount);

            if (bestPointPromo != null)
            {
                int pointMultiple = (int)((paymentAmount) / bestPointPromo.RequiredAmount);

                pointsEarned += (bestPointPromo.Points + bestPointPromo.ExtraPoints) * pointMultiple;
                if (!bestPointPromo.Equals(bestTotalPromotion))
                    promocodeApplied += bestPointPromo.PromotionCode;
            }

#if ISKULTUR
            int startIndex = promoLines.Count;
#else
            int startIndex = 0;
#endif

            if (ttlDiscount > 0)
                promoLines.Add(String.Format("06,IND,TOP {0:D4} %,  {1:D10}", 0, (Int64)(ttlDiscount * 100)));//%00
            if (promocodeApplied.Length > 0)
                promoLines.Add(String.Format("22,MSG,{0} {1}",PosMessage.PROMOTION_CODE, promocodeApplied));

            if (promoRemarks.Count > 0)
            {
                foreach (string pr in promoRemarks)
                    promoLines.Add("29,NOT," + pr);
            }
            if (pointsEarned > 0 && customer != null && !(Str.Contains(customer.Code,"*")))
                promoLines.Add(String.Format("22,PRM,{0:D9}", pointsEarned));

            if (customer != null && int.Parse(customer.Number) > 0)
                promoLines.Add(String.Format("23,PNT,{0:D9}", customer.Number));
            if (promoLines.Count > 0)
                promoLines.Add(String.Format("00,KOD,{0}", Settings.PromoKey));
            for (int cntr = startIndex; cntr < promoLines.Count; cntr++)
            {
                promoLines[cntr] = String.Format("1,{0:D5},{1}", cntr + 1, promoLines[cntr]);
            }

            return promoLines.ToArray();
        }

        private bool CheckLimit(BasePromotion basePRM)
        {
            switch (basePRM.LimitType)
            {
                case LimitType.NoLimit:
                    return SoldItem.Instance().TotalAmount(basePRM) > 0m;
                case LimitType.Amount:
                    return soldItems.TotalAmount(basePRM) >= basePRM.RequiredAmount;
                case LimitType.Quantity:
                    return soldItems.LineTotal(basePRM) >= basePRM.RequiredQuantity;
                default:
                    return false;
            }
        }
        private TotalPromotion BestGiftPromotion(List<TotalPromotion> giftPromotions, decimal promotedAmount)
        {
            TotalPromotion bestPromotion = null;
            foreach (TotalPromotion tp in giftPromotions)
            {
                IProduct giftProduct = FindProductByLabel(tp.GiftProductLabelNo);
                Decimal lineQuantity = soldItems.LineTotal(tp.GiftProductLabelNo);

                if (giftProduct != null &&
                        lineQuantity > 0 &&
                        tp.RequiredAmount <= promotedAmount)
                    bestPromotion = bestPromotion == null ? tp : ComparePromotions(bestPromotion, tp, promotedAmount);
            }
            return bestPromotion;
        }


        private TotalPromotion BestPointPromotion(decimal promotedTotal)
        {
            TotalPromotion bestPointPromotion = null;

            foreach (IPromotion pr in Settings.TotalPromotions)
            {
                if (!pr.PromotionCoverAll(this.promotionRange)) continue;
                if (!pr.CheckPaymentType(paymentType)) continue;
                if ((((TotalPromotion)pr).PercentDiscount == 0) && (((TotalPromotion)pr).Discount == 0))

                    if (pr is TotalPromotion && ((TotalPromotion)pr).RequiredAmount <= promotedTotal)
                    {
                        bestPointPromotion = bestPointPromotion == null ? (TotalPromotion)pr :
                                             ComparePromotionsPoints(bestPointPromotion, (TotalPromotion)pr, promotedTotal);
                    }
            }
            return bestPointPromotion;
        }

        private TotalPromotion ComparePromotionsPoints(TotalPromotion tp1, TotalPromotion tp2, decimal promotedTotal)
        {
            int ratio1 = (int)(promotedTotal / tp1.RequiredAmount);
            int ratio2 = (int)(promotedTotal / tp2.RequiredAmount);

            return tp1.Points * ratio1 > tp2.Points * ratio2 ? tp1 : tp2;
        }

        private TotalPromotion ComparePromotions(TotalPromotion tp1, TotalPromotion tp2, decimal promotedTotal)
        {
            decimal discountPromo1 = Math.Max(tp1.Discount, tp1.TotalAmount() * tp1.PercentDiscount / 100m);
            decimal discountPromo2 = Math.Max(tp2.Discount, tp2.TotalAmount() * tp2.PercentDiscount / 100m);

            if (discountPromo2 == discountPromo1)
            {
                TotalPromotion ep = tp2.Points + tp2.ExtraPoints > tp1.Points + tp1.ExtraPoints ? tp2 : tp1;
                return ep;
            }

            TotalPromotion bp = discountPromo2 > discountPromo1 ? tp2 : tp1;

            if (bp.RequiredAmount <= promotedTotal) return bp;

            TotalPromotion sp = discountPromo2 < discountPromo1 ? tp2 : tp1;

            if (sp.RequiredAmount > promotedTotal) return bp;

            return Math.Max(sp.Discount, sp.TotalAmount() * sp.PercentDiscount / 100m) + totalDiscount > Math.Max(bp.Discount, bp.TotalAmount() * bp.PercentDiscount / 100m) ? sp : bp;

        }

        internal static ICustomer FindCustomerByCard(String cardId)
        {
            return Settings.DataConnector.FindCustomerByCardNo(cardId);
        }

        internal static IProduct FindProductByLabel(int labelNo)
        {
            try
            {
                return Settings.DataConnector.FindProductByLabel(String.Format("{0:D6}", labelNo));
            }
            catch
            {
                return null;
            }
        }
    }
}
