using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Hugin.POS.PromotionServer
{
    class SoldItem
    {
        DataTable soldItems = new DataTable("ITEMS");
        DataTable promotions = new DataTable("PROMOTIONS");
        static SoldItem soldItem = null;

        public static SoldItem Instance()
        {
            if (soldItem == null)
                soldItem = new SoldItem();
            return soldItem;
        }

        private SoldItem()
        {
            soldItems.Columns.Add("LineNo", typeof(int));
            soldItems.Columns.Add("Plu", typeof(int));
            soldItems.Columns.Add("CategoryId", typeof(int));
            soldItems.Columns.Add("Quantity", typeof(decimal));
            soldItems.Columns.Add("Amount", typeof(decimal));
            soldItems.Columns.Add("DiscountAmount", typeof(decimal));
            soldItems.Columns.Add("PercentDiscount", typeof(int));
            
            promotions.Columns.Add("Id", typeof(int));
            promotions.Columns.Add("Promotion", typeof(BasePromotion));
            promotions.Columns.Add("IsApplied", typeof(bool));
        }

        internal void Add(int pluno, int lineno, Decimal lineQuantity, Decimal lineAmount)
        {
            Hugin.POS.Common.IProduct p = Promotion.FindProductByLabel(pluno);
            if (lineAmount > 0)
                soldItems.Rows.Add(lineno, pluno, int.Parse(p.Category), lineQuantity, lineAmount, 0m, 0);
            else
            {
                DataRow[] rows = soldItems.Select(String.Format("Plu = '{0}' And Quantity > 0", pluno));
                if (rows.Length > 0)
                {
                    decimal quantity = Math.Abs(lineQuantity);
                    foreach (DataRow row in rows)
                    {
                        decimal rowUnitPrice = Math.Round((decimal)row["Amount"] / (decimal)row["Quantity"], 2);
                        decimal apliedQuantity = Math.Min((decimal)row["Quantity"], quantity);
                        row["Quantity"] = (decimal)row["Quantity"] - apliedQuantity;
                        row["Amount"] = (decimal)row["Amount"] - Math.Round(apliedQuantity * rowUnitPrice, 2);
                        quantity -= apliedQuantity;
                        if (quantity <= 0)
                            break;
                    }
                }
                else
                {
                    Settings.Log("Ýptal Ürün\nPromosyon Hatasý");
                }
            }
        }
        
        internal decimal TotalAmount(BasePromotion basePRM)
        {
            return ((IPromotion)basePRM).TotalAmount();
        }

        internal decimal LineTotal(BasePromotion basePRM)
        {
            return ((IPromotion)basePRM).LineQuantity();
        }

        internal decimal LineTotal(int plu)
        {
            decimal total = 0m;
            System.Data.DataRow[] rows = SoldItem.Instance().Select("Plu = '" + plu + "'");
            if (rows.Length > 0)
                total = Convert.ToDecimal(SoldItem.Instance().Compute("Sum(Quantity)", "Plu = '" + plu + "'"));

            return total;
        }

        internal void SetPromotion(BasePromotion basePRM, bool isApplied)
        {
            DataRow[] rows = promotions.Select("Id = '" + basePRM.Id + "'");
            if (rows.Length > 0)
            {
                if (!(bool)rows[0]["IsApplied"])
                    rows[0]["IsApplied"] = isApplied;
            }
            else
            {
                promotions.Rows.Add(basePRM.Id, basePRM, isApplied);
            }
        }

        internal List<String> PromotedLines
        {
            get
            {
                List<String> promotedLines = new List<String>();
                DataRow[] items = soldItems.Select(String.Format("DiscountAmount > '{0}'", 0m));

                foreach (DataRow line in items)
                {
                    promotedLines.Add(
                            String.Format("06,IND,SAT {0:D4} %{1:D2},  {2:D10}",
                                            (int)line["LineNo"],
                                            (int)line["PercentDiscount"],
                                            (int)Math.Round((decimal)line["DiscountAmount"] * 100, 0)));
                }
                return promotedLines;
            }
        }

        internal string[] AppliedCodes
        {
            get
            {
                DataRow[] rows = promotions.Select("IsApplied ='" + true + "'");
                String[] promoCodes = new string[rows.Length];
                for (int i = 0; i < rows.Length; i++)
                    promoCodes[i] = ((BasePromotion)rows[i]["Promotion"]).PromotionCode;
                return promoCodes;
            }
        }

        internal BasePromotion[] Promotions
        {
            get
            {
                BasePromotion[] temp = new BasePromotion[promotions.Rows.Count];
                for (int i = 0; i < promotions.Rows.Count; i++)
                    temp[i] = (BasePromotion)promotions.Rows[i]["Promotion"];
                return temp;
            }
        }

        internal DataRow[] Select(string query)
        {
            return soldItems.Select(query);
        }

        internal object Compute(string expression, string filter)
        {
            return soldItems.Compute(expression, filter);
        }

        internal static SoldItem Initialize()
        {
            return soldItem = new SoldItem();
        }

        internal void GiveAsPromotion(BasePromotion basePRM)
        {
            decimal giftQuantity = basePRM.GiftProductQuantity;
            decimal appliedQuantity = 0m;
            string query = String.Format("Plu ={0} And Quantity > 0", basePRM.GiftProductLabelNo);
            DataRow[] rows = SoldItem.Instance().Select(query);
            if (rows.Length > 0)
            {
                foreach (DataRow row in rows)
                {
                    appliedQuantity = Math.Min(giftQuantity, (decimal)row["Quantity"]);
                    giftQuantity -= appliedQuantity;
                    row["DiscountAmount"] = (decimal)row["DiscountAmount"] + 
                                                Math.Round(appliedQuantity * ((decimal)row["Amount"] - (decimal)row["DiscountAmount"]) / (decimal)row["Quantity"], 2);
                    row["Quantity"] = (decimal)row["Quantity"] - appliedQuantity;
                    if (giftQuantity <= 0) break;
                }
            }
            SetPromotion(basePRM, appliedQuantity > 0);
        }

        internal decimal TotalDiscount()
        {
            return Convert.ToDecimal(soldItems.Compute("Sum(DiscountAmount)", ""));
        }

        internal List<int> GetSalesID(BasePromotion basePRM)
        {
            List<int> salesIDList = new List<int>();

            string query = String.Format("Plu ={0} And Quantity > 0", basePRM.GiftProductLabelNo);
            DataRow[] rows = SoldItem.Instance().Select(query);

            foreach(DataRow row in rows)
            {
                salesIDList.Add((int)row["LineNo"]);
            }

            return salesIDList;
        }
    }
}
