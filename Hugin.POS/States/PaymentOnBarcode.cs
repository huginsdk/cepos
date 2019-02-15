using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS.States
{
    class PaymentOnBarcode:SilentState
    {
        private static IState state = new PaymentOnBarcode();
        private static String strBarcodeList = String.Empty;

        public static IState Instance(String barcodes)
        {
            strBarcodeList = barcodes;
            return state;
        }

        public override void Enter()
        {
            String[] barcodes = Str.Split(strBarcodeList, PosConfiguration.BarcodeTerminator);
            Exception saleException = null;
            foreach (String barcode in barcodes)
            {
                try
                {
                    long numericCheck = 0;
                    if (Parser.TryLong(barcode, out numericCheck))
                    {
                        Execute(barcode);
                    }
                }
                catch (Exception ex)
                {
                    Chassis.Engine.SendErrorMessage(1);
                    saleException = ex;
                }

            }
            if (saleException != null)
                throw saleException;
        }

        private void Execute(string barcode)
        {
            Number input = new Number(barcode);
            IProduct p = null;
            PaymentInfo payment = null;
            try
            {
                String spercialBarcode = input.Length > 1 ? GetSpecial(input.ToString("B").Substring(0, 2)) : "";
                if (spercialBarcode != "")
                {
                    payment = PaymentWithBarcode((int)(input.ToDecimal() % 10));
                    input.RemoveLastDigit();
                }

                try
                {
                    p = cr.DataConnector.FindProductByBarcode(input.ToString("B"));
                }
                catch { }

                if (p == null && spercialBarcode != "")
                {
                    p = FindSpecialProduct(input);
                    BarcodeAdjustment adjustment = new BarcodeAdjustment(input);
                    switch (adjustment.Type)
                    {
                        case BarcodeType.ByGramma:
                        case BarcodeType.ByQuantity:
                            input = adjustment.Quantity;
                            Quantity();
                            break;
                        case BarcodeType.ByTotalAmount:
                            cr.State = States.EnterTotalAmount.Instance(adjustment.Amount);
                            cr.State.Enter();
                            break;
                        case BarcodeType.ByPrice:
                            cr.State = States.EnterUnitPrice.Instance(adjustment.Price);
                            cr.State.Enter();
                            break;
                    }
                }
                else if (p == null)
                    throw new BarcodeNotFoundException();
            }
            catch (Exception)
            {
                cr.Log.Warning("Barcode not found: {0}", input);
                cr.State = AlertCashier.Instance(new Error(new BarcodeNotFoundException()));
                return;
            }

            cr.Execute(p);

            if (payment != null)
            {
                payment.Amount = cr.Document.BalanceDue;
                cr.Document.Pay(payment);
            }
        }

        private PaymentInfo PaymentWithBarcode(int paymentType)
        {
            PaymentInfo pi = null;

            switch (paymentType)
            {
                case 0://Continue selling
                    break;
                case 1://Pay with cash
                    pi = new CashPaymentInfo();
                    break;
                case 2://Pay with check
                    pi = new CheckPaymentInfo();
                    break;
                default: //between 3 and 9 is credit payment
                    ICredit credit = cr.DataConnector.GetCredits()[paymentType - 2];
                    if (credit != null)
                        pi = new CreditPaymentInfo(credit);
                    else
                        throw new Exception(PosMessage.PAYMENT_INVALID);
                    break;
            }

            return pi;
        }

        private String GetSpecial(String key)
        {
            return cr.DataConnector.CurrentSettings.GetSpecialBarcode(key);
        }

        private IProduct FindSpecialProduct(Number input)
        {
            String barcode = GetSpecial(input.ToString().Substring(0, 2));
            int labelLength = Int32.Parse(barcode.Substring(4, 1));
            return cr.DataConnector.FindProductByBarcode(input.ToString().Substring(0, 2 + labelLength));
        }
    }
}
