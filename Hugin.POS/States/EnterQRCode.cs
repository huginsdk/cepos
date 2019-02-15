using System;
using cr = Hugin.POS.CashRegister;
using System.Text;
using System.Threading;
using Hugin.POS.Common;
using System.Globalization;

namespace Hugin.POS.States
{
    class EnterQRCode : State
    {
        static StringBuilder input;
        private static IState state = new EnterQRCode();

        public static IState Instance(string defaultValue)
        {
            input = new StringBuilder(defaultValue);

            return state;
        }

        public override void BarcodePrefix()
        {
        }

        public override void Numeric(char c)
        {
            Append(c, true);
        }

        void Append(char c, bool p)
        {
            if (char.IsLetter(c) || char.IsDigit(c) || c == '|')
            {
                input.Append(c, 1);
                DisplayAdapter.Cashier.Append(c.ToString(), p);
            }
        }

        public override void Enter()
        {
            IProduct p = null;
            cr.Item.Quantity = 1;
            string barcodeNumber = null, productSerialNumber = null;
            string batchNumber = null;//parti no(lot no)
            DateTime expiryDate;
            string[] splittedInput = input.ToString().Split('|');

            try
            {
                if (splittedInput.Length != 0)
                {
                    if (splittedInput[0].StartsWith("01"))
                    {
                        /* 1D ilaç barkodlarý ürün üzerinde EAN13 barkod alfabesi ile 13 basamaklý olarak yer alýr.
                        * Karekod ilaç barkodda bu 13 basamaklý numaranýn baþýna 0 konularak 14 basamaklý GTIN(barkod) 
                        * numarasý oluþturulur.
                        * Kaynak : http://www.iegm.gov.tr/Folders/Docs/Beseri_Ila%C3%A7lar_Barkod_Uygulama_Klavuzu_v1_4_3c28249.pdf
                        */
                        barcodeNumber = splittedInput[0].Substring(3, 13);

                        cr.Item.Barcode = barcodeNumber;
                        if (splittedInput[0].Substring(16, 2) == "21")
                        {
                            productSerialNumber = splittedInput[0].Substring(18);
                            cr.Item.SerialNo = productSerialNumber;
                        }
                    }
                    if (splittedInput[1].StartsWith("17"))
                    {
                        string dateString = splittedInput[1].Substring(2, 6);

                        //Some day field may be 00 so we change them last day of its month
                        if (dateString.Substring(4, 2) == "00")
                        {
                            dateString = dateString.Remove(4, 2);
                            dateString = dateString.Insert(4, "01");
                            expiryDate = DateTime.ParseExact(dateString, "yyMMdd",
                                          CultureInfo.InvariantCulture);
                            expiryDate = expiryDate.AddMonths(1).AddDays(-1);
                        }
                        else
                        {
                            expiryDate = DateTime.ParseExact(dateString, "yyMMdd",
                                          CultureInfo.InvariantCulture);
                        }
                        cr.Item.ExpiryDate = expiryDate;

                        if (splittedInput[1].Substring(8, 2) == "10")
                        {
                            batchNumber = splittedInput[1].Substring(10);
                            cr.Item.BatchNumber = batchNumber;
                        }
                    }
                }

                try
                {
                    p = cr.DataConnector.FindProductByBarcode(barcodeNumber);
                }
                catch { }

                if (p == null)
                    throw new BarcodeNotFoundException();

            }
            catch (Exception)
            {
                cr.Log.Warning("Barcode not found: {0}", input);
                cr.State = AlertCashier.Instance(new Error(new BarcodeNotFoundException()));
                return;
            }

            cr.Execute(p);
        }

        public override void Seperator()
        {
            WriteChar(0, PosKey.Decimal);
        }
        public override void Alpha(char c)
        {
            Append(c, true);
        }
        public override void Document()
        {
            WriteChar(0, PosKey.Document);
        }
        public override void Customer()
        {
            WriteChar(0, PosKey.Customer);
        }
        public override void Report()
        {
            WriteChar(0, PosKey.Report);
        }
        public override void Program()
        {
            WriteChar(0, PosKey.Program);
        }
        public override void Command()
        {
            WriteChar(0, PosKey.Command);
        }
        public override void SalesPerson()
        {
            WriteChar(0, PosKey.SalesPerson);
        }
        public override void ReceiveOnAcct()
        {
            WriteChar(0, PosKey.ReceiveOnAcct);
        }
        public override void PayOut()
        {
            WriteChar(0, PosKey.PayOut);
        }
        public override void Void()
        {
            WriteChar(0, PosKey.Void);
        }
        public override void Adjust(AdjustmentType method)
        {
            switch (method)
            {
                case AdjustmentType.Discount:
                    WriteChar(0, PosKey.Discount); break;
                case AdjustmentType.Fee:
                    WriteChar(0, PosKey.Fee); break;
                case AdjustmentType.PercentDiscount:
                    WriteChar(0, PosKey.PercentDiscount); break;
                case AdjustmentType.PercentFee:
                    WriteChar(0, PosKey.PercentFee); break;
                default: break;

            }
        }
        public override void PriceLookup()
        {
            WriteChar(0, PosKey.PriceLookup);
        }
        public override void Price()
        {
            WriteChar(0, PosKey.Price);
        }
        public override void TotalAmount()
        {
            WriteChar(0, PosKey.Total);
        }
        public override void Repeat()
        {
            WriteChar(0, PosKey.Repeat);
        }
        public override void Pay(CreditPaymentInfo info)
        {
            int id = info.Id < 0 ? 0 : info.Id;
            WriteChar(id, PosKey.Credit);
        }
        public override void Pay(CurrencyPaymentInfo info)
        {

            WriteChar(0, PosKey.ForeignCurrency);
        }
        public override void Pay(CheckPaymentInfo info)
        {
            WriteChar(0, PosKey.Check);
        }
        public override void Pay(CashPaymentInfo info)
        {
            WriteChar(0, PosKey.Cash);
        }

        public override void SubTotal()
        {
            WriteChar(0, PosKey.SubTotal);
        }
        private void WriteChar(int order, PosKey key)
        {
            byte sequence = 0;
            char c = KeyMap.GetLetter(key, order, ref sequence);
            Append(c, true);
        }
    }
}
