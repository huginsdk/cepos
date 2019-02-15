using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS
{
    public enum BarcodeType
    {
        ByTotalAmount,
        ByQuantity,
        ByGramma,
        ByPrice
    }
    public class BarcodeAdjustment
    {
        public Decimal Amount = 0;
        public Decimal Price = 0;
        public Number Quantity;
        public BarcodeType Type;

        public const String WeightPrefix = "BG";
        public const String TotalAmountPrefix = "BT";
        public const String QuantityPrefix = "BA";
        public const String PricePrefix = "BF";

        public BarcodeAdjustment(Number input)
        {

            String key = GetSpecialBarcodeKey(input.ToString().Substring(0, 2));
            switch (key)
            {
                case WeightPrefix:
                    this.Quantity = new Number(Decimal.Parse(BarcodeValue(input)) / 1000);
                    this.Type = BarcodeType.ByGramma;
                    break;

                case QuantityPrefix:
                    this.Quantity = new Number(Int32.Parse(BarcodeValue(input)));
                    this.Type = BarcodeType.ByQuantity;
                    if (Quantity.ToDecimal() > 100)
                        throw new OutofQuantityLimitException();
                    break;

                case TotalAmountPrefix:
                    this.Amount = Decimal.Parse(BarcodeValue(input)) / 100;
                    this.Type = BarcodeType.ByTotalAmount;
                    break;

                case PricePrefix:
                    this.Price = Decimal.Parse(BarcodeValue(input)) / 100;
                    this.Type = BarcodeType.ByPrice;
                    break;
            }
        }
        private static String GetSpecial(String key)
        {
            return CashRegister.DataConnector.CurrentSettings.GetSpecialBarcode(key);// SpecialBarcodes.SplitedBarcode.ContainsKey(key);
        }
        private static String GetSpecialBarcodeKey(String key)
        {
            String barcode = GetSpecial(key);
            if (barcode != "")
                return barcode.Substring(0, 2);
            return barcode;
        }
        private int LabelLength(Number input)
        {
            String barcode = GetSpecial(input.ToString().Substring(0, 2));
            return Int32.Parse(barcode.Substring(4, 1));
        }
        private int ValueLength(Number input)
        {
            String barcode = GetSpecial(input.ToString().Substring(0, 2));
            return Int32.Parse(barcode.Substring(5, 1));
        }
        private String BarcodeValue(Number input)
        {
            return input.ToString().Substring(2 + LabelLength(input), ValueLength(input));
        }

        internal static bool IsWeightType(Number input)
        {
            String key = GetSpecialBarcodeKey(input.ToString().Substring(0, 2));

            return key == WeightPrefix;            
         }
    }
}
