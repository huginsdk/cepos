using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hugin.POS.PromotionServer
{
    class SpecialPromotion : BasePromotion, IPromotion
    {        
        Dictionary<string, string> promoConditions;
        
        internal static void Load()
        {
            StreamReader sr = null;
            Exception lastException = null;
            SpecialPromotion pro;

            String path=Settings.DataPath + Settings.SPECIAL_PROMOTION_FILE_NAME;
            if (!File.Exists(path))
                return;
            try
            {
                sr = new StreamReader(path, Settings.DefaultEncoding);
                
                String line = String.Empty;
                String[] lineArray;
                int counter = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    try
                    {
                        if (line.Trim().Length == 0) break;
                        lineArray = line.Split(',');

                        pro = new SpecialPromotion();
                        pro.id = counter++;
                        pro.range = PromotionRange.CUSTOMER;
                                                
                        pro.SetDateRange(lineArray[0], lineArray[1], lineArray[2], lineArray[3]);
                        pro.requiredAmount = Convert.ToDecimal(lineArray[4]) / 100m;

                        string[] conditions = lineArray[5].Split('&');
                        pro.promoConditions = new Dictionary<string, string>();
                        foreach (String condition in conditions)
                        {
                            string[] cond = condition.Split('=');
                            if(cond.Length!=2)
                                continue;
                            string prop = cond[0].Trim().ToUpper();
                            string valu = cond[1].Trim().ToUpper();
                            if (valu == "TODAY")
                                valu = string.Format("{0:ddMMyyyy}", DateTime.Now);
                            if (!pro.promoConditions.ContainsKey(prop))
                                pro.promoConditions.Add(prop, valu);
                        }
                        pro.discount = Convert.ToDecimal(lineArray[6]) / 100m;
                        pro.percentDiscount = Convert.ToInt32(lineArray[7]);
                        if (pro.percentDiscount > 99)
                            pro.percentDiscount = 0;
                        pro.point = Convert.ToInt32(lineArray[8]);
                        pro.promoRemark = lineArray[9];
                        //pro.promotionNotes = lineArray[10];
                        pro.code = "S" + pro.id;
                        Settings.TotalPromotions.Add(pro);

                    }
                    catch (Exception e)
                    {
                        lastException = e;
                        Settings.Log("Ozelpromo.dat okuma hatasi! " + e.Message);
                        continue;
                    }
                }
            }

            finally
            {
                if (sr != null)
                    sr.Close();

            }
        }

        public override bool CheckCustomerGroup(Hugin.POS.Common.ICustomer customer)
        {
            foreach (String key in promoConditions.Keys)
            {
                String strValue = customer.GetCustomValue(key);
                if (strValue != promoConditions[key])
                    return false;
            }
            return true;
        }

        public decimal TotalAmount()
        {
            decimal subtotal = (decimal)SoldItem.Instance().Compute("Sum(Amount) - Sum(DiscountAmount)", "");
            if (base.GiftProductQuantity > 0)
            {
                Hugin.POS.Common.IProduct p = Promotion.FindProductByLabel(base.GiftProductLabelNo);
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
