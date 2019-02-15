using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Hugin.POS.Common;
namespace Hugin.POS.PromotionServer
{
    internal class TotalPromotion : BasePromotion, IPromotion
    {
        internal static void Load()
        {
            StreamReader sr = null;
            Exception lastException = null;
            TotalPromotion pro;

            try
            {
                sr = new StreamReader(Settings.DataPath + Settings.TOTAL_PROMOTION_FILE_NAME, Settings.DefaultEncoding);

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
                        pro = new TotalPromotion();
                        lineNo++;

                        switch (lineArray[0])
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
                        pro.SetDateRange(lineArray[1], lineArray[2], lineArray[3], lineArray[4]);
                        pro.requiredAmount = Decimal.Parse(lineArray[5]) / 100m;
                        pro.percentDiscount = Int32.Parse(lineArray[6]) == -1 ? 100 : Int32.Parse(lineArray[6]);
                        
                        pro.code = "T" + (lineNo).ToString();
                        pro.discount = decimal.Parse(lineArray[7]) / 100m;

                        pro.paymentTypes = new List<string>();
                        pro.customerGroups = new List<string>();
                      
                        pro.point = Int32.Parse(lineArray[8]);
                        pro.extraPoint = Int32.Parse(lineArray[9]);

                        long groupId;
                        for (int counter = 10; counter < lineArray.Length; counter++)
                        {
                            if (lineArray[counter] == String.Empty) continue;

                            if (lineArray[counter].StartsWith("M"))
                            {
                                if (Parser.TryLong(lineArray[counter].Substring(1), out groupId))
                                    pro.customerGroups.Add(lineArray[counter].Trim().Substring(1));
                            }
                            else if (lineArray[counter].Length == 7 && lineArray[counter][0] == 'P')
                            {
                                Parser.TryInt(lineArray[counter].Substring(1), out pro.giftProductLabelNo);
                                pro.giftProductQuantity = 1;
                            }
                            else
                                pro.paymentTypes.Add(lineArray[counter]);
                        }
                        Settings.TotalPromotions.Add(pro);
                    }
                    catch (ParameterRelationException)
                    {
                        //Date range is invalid
                    }
                    catch (Exception e)
                    {
                        lastException = e;
                        Settings.Log("Promo.dat okuma hatasi! " + e.Message);
                        continue;
                    }
                }
            }
            if (lastException != null)
            {
                throw new InvalidDataException(lastException.Message);
            }
        }

        public override bool CheckPaymentType(String type)
        {
            if (paymentTypes == null || paymentTypes.Count == 0) return true;

            if (type.StartsWith("K") && paymentTypes.Contains("K00")) return true;
            if (type.StartsWith("D") && paymentTypes.Contains("D00")) return true;

            return paymentTypes.Contains(type);
        }


        public decimal TotalAmount()
        {
            decimal subtotal = (decimal)SoldItem.Instance().Compute("Sum(Amount) - Sum(DiscountAmount)", "");
            if (base.GiftProductQuantity > 0)
            {
                IProduct p =Promotion.FindProductByLabel(base.GiftProductLabelNo);
                int giftRatio = (int)(subtotal / base.requiredAmount);
                subtotal = p.UnitPrice * Math.Min(SoldItem.Instance().LineTotal(base.GiftProductLabelNo), giftRatio);
            }
            return subtotal;
        }

        public decimal LineQuantity()
        {
            return 0m;
        }

        public void ApplyPromotion()
        {
        }

        public long PointEarned
        {
            get { return 0; }
        }

    }
}
