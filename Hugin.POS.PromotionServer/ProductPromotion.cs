using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Hugin.POS.Common;
using System.Data;


namespace Hugin.POS.PromotionServer
{
    class ProductPromotion : BasePromotion, IPromotion
    {
        private long pointEarned = 0;

        internal static void Load()
        {
            StreamReader sr = null;
            Exception lastException = null;
            ProductPromotion pro;

            try
            {
                sr = new StreamReader(Settings.DataPath + Settings.PRODUCT_PROMOTION_FILE_NAME, Settings.DefaultEncoding);
            }
            catch (FileNotFoundException fnfe)
            {
                if (sr != null) sr.Close();
                throw new InvalidDataException(fnfe.Message);

            }
            using (sr)
            {

                String line = String.Empty;
                String[] lineArray;
                int lineNo = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    try
                    {
                        if (line.Trim().Length == 0) break;
                        lineArray = line.Split(',');
                        pro = new ProductPromotion();
                        lineNo++;
                        pro.id = Int32.Parse(lineArray[0]);
                        switch (lineArray[1])
                        {
                            case "H":
                                pro.range = PromotionRange.ALL;
                                break;
                            case "M":
                                pro.range = PromotionRange.CUSTOMER;
                                break;
                            case "I":
                                pro.range = PromotionRange.ALL;
                                pro.limitType = LimitType.Amount;
                                break;
                            case "N":
                                pro.range = PromotionRange.CUSTOMER;
                                pro.limitType = LimitType.Amount;
                                break;
                        }
                        pro.SetDateRange(lineArray[2], lineArray[3], lineArray[4], lineArray[5]);
                        pro.percentDiscount = Int32.Parse(lineArray[6]) == -1 ? 100 : Int32.Parse(lineArray[6]);
                        pro.discount = decimal.Parse(lineArray[7]) / 100m;
                        pro.point = Int32.Parse(lineArray[8]);
                        pro.extraPoint = Int32.Parse(lineArray[9]);
                        pro.requiredQuantity = Math.Max(Int32.Parse(lineArray[10]), 1);
                        pro.giftProductLabelNo = Int32.Parse(lineArray[11]);
                        pro.giftProductQuantity = Int32.Parse(lineArray[12]);

                        if (pro.RequiredQuantity > 1 && pro.GiftProductLabelNo == 0)
                            pro.limitType = LimitType.Quantity;

                        if (pro.limitType == LimitType.Amount)
                        {
                            pro.requiredAmount = pro.Discount;
                            pro.discount = 0;
                        }

                        pro.customerGroups = new List<string>();
                        long groupId;
                        for (int counter = 13; counter < lineArray.Length; counter++)
                        {
                            if (lineArray[counter].StartsWith("M"))
                            {
                                if (Parser.TryLong(lineArray[counter].Substring(1), out groupId))
                                    pro.customerGroups.Add(lineArray[counter]);
                            }
                            else if (pro.PromoRemark == "")
                                pro.promoRemark = lineArray[counter];
                        }

                        pro.code = "U" + (lineNo).ToString();
                        Settings.SalePromotions.Add(pro);
                    }
                    catch (ParameterRelationException)
                    {
                        //Date range is invalid
                    }
                    catch (Exception e)
                    {
                        lastException = e;
                        Settings.Log("ProUrun.dat okuma hatasi! " + e.Message);
                        continue;
                    }
                }


            }
            if (lastException != null)
            {
                throw new InvalidDataException(lastException.Message);
            }
        }

        public decimal TotalAmount()
        {
            decimal total = 0m;
            System.Data.DataRow[] rows = SoldItem.Instance().Select("Plu = '" + this.Id + "'");
            if (rows.Length > 0)
                total = Convert.ToDecimal(SoldItem.Instance().Compute("Sum(Amount)", "Plu = '" + this.Id + "'"));

            return total;
        }

        public decimal LineQuantity()
        {
            decimal total = 0m;
            System.Data.DataRow[] rows = SoldItem.Instance().Select("Plu = '" + this.Id + "'");
            if (rows.Length > 0)
                total = Convert.ToDecimal(SoldItem.Instance().Compute("Sum(Quantity)", "Plu = '" + this.Id + "'"));

            return total;
        }

        public void ApplyPromotion()
        {
            decimal ratio = 0m;
            decimal lineDiscount = 0m, percDiscount = 0m;

            switch (base.LimitType)
            {
                case LimitType.Quantity:
                    ratio = (int)(LineQuantity() / base.RequiredQuantity);
                    lineDiscount = ratio * base.Discount;
                    percDiscount = Rounder.RoundDecimal((TotalPercantageDiscount() / LineQuantity()) * (ratio * base.RequiredQuantity), 2, true);
                    break;
                case LimitType.Amount:
                    ratio = (TotalAmount() / base.RequiredAmount);
                    lineDiscount = LineQuantity() * base.Discount;
                    percDiscount = Rounder.RoundDecimal(TotalPercantageDiscount(), 2, true);
                    break;
                default:
                    ratio = LineQuantity();
                    lineDiscount = ratio * base.Discount;
                    percDiscount = Rounder.RoundDecimal(TotalPercantageDiscount(), 2, true);
                    break;
            }

            bool isPercentage = false;
            if (percDiscount > lineDiscount)
            {
                lineDiscount = percDiscount;
                isPercentage = true;
            }
            DataRow[] items = SoldItem.Instance().Select(String.Format("Plu = '{0}'", base.Id));

            if (base.limitType != LimitType.Quantity && isPercentage)
            {
                foreach (DataRow rowItems in items)
                {
                    rowItems["DiscountAmount"] = Rounder.RoundDecimal(((decimal)rowItems["Amount"] * base.PercentDiscount) / 100, 2, true);
                    rowItems["PercentDiscount"] = base.PercentDiscount;
                }
            }
            else
            {
                decimal discItemQuantity = base.RequiredQuantity * ratio;
                decimal unitDisc = Rounder.RoundDecimal(lineDiscount / discItemQuantity, 2, true);
                decimal diff = lineDiscount - Rounder.RoundDecimal(unitDisc * discItemQuantity, 2, true);
                decimal amount = 0m;

                foreach (DataRow rowItems in items)
                {
                    decimal lineDiscQuantity = (decimal)rowItems["Quantity"];
                    if (lineDiscQuantity <= 0) continue;
                    int percRate = base.PercentDiscount;
                    if ((decimal)rowItems["Quantity"] > discItemQuantity)
                    {
                        lineDiscQuantity = discItemQuantity;
                        amount = (unitDisc * lineDiscQuantity) + diff;
                        percRate = 0;
                    }
                    else
                    {
                        amount = (unitDisc * lineDiscQuantity) + diff;
                    }

                    diff = 0;
                    discItemQuantity -= lineDiscQuantity;
                    rowItems["DiscountAmount"] = (decimal)rowItems["DiscountAmount"] + amount;
                    if (isPercentage)
                        rowItems["PercentDiscount"] = percRate;
                    if (discItemQuantity == 0) break;
                }
            }

            decimal appliedDisc = (decimal)SoldItem.Instance().Compute("Sum(DiscountAmount)", String.Format("Plu = '{0}'", base.Id));
            pointEarned = (long)ratio * (base.Points + base.ExtraPoints);
            SoldItem.Instance().SetPromotion(this, appliedDisc + pointEarned > 0);
        }

        private decimal TotalPercantageDiscount()
        {
            return base.PercentDiscount * TotalAmount() / 100m;
        }

        public long PointEarned
        {
            get { return pointEarned; }
        }
    }
}
