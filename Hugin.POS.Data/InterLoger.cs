using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Hugin.POS.Common;

namespace Hugin.POS.Data
{
    class InterLoger : Logger
    {
        internal override StringWriter LogItems(ISalesDocument document, int docStatus, ref int number)
        {
            StringWriter logWriter = new StringWriter();
            String registerId = PosConfiguration.Get("RegisterId");
            //if (document.Id == 0 || document.IsEmpty) : Not possible, The function which calls LogItems checks that condition.
            String documentId = document.Id.ToString().PadLeft(6, '0') + "  ";

            if (!String.IsNullOrEmpty(document.SlipSerialNo))
            {
                String seriNo = document.SlipSerialNo;
                if (seriNo.Length > 2)
                    seriNo = seriNo.Substring(0, 2);

                String orderNo = document.SlipOrderNo;
                if (orderNo.Length > 6)
                    orderNo = orderNo.Substring(0, 6);

                documentId = String.Format(
                                        "{0}{1}",
                                        seriNo.PadLeft(2, ' '),
                                        orderNo.PadLeft(6, '0'));
            }
            else if (document.DocumentTypeId >= 0)
            {
#if WindowsCE
                int lastSlipNo = Document.GetLastSlipNo(PosConfiguration.Get("RegisterId"));
                lastSlipNo++;

                documentId = lastSlipNo.ToString().PadLeft(6, '0') + "  ";
#endif
            }

            String docTypeId, docCode, saleCode;
            if (document.Code == PosMessage.HR_CODE_RETURN)
            {
                docTypeId = "24";
                docCode = PosMessage.HR_INTER_CODE_RETURN;
                saleCode = "25,GAL";
            }
            else
            {
                docTypeId = document.DocumentTypeId < 0 ? "01" : "02";
                docCode = document.Code;
                saleCode = "04,SAT";
            }

            String cashierId = "0000";
            if (currentCashier != null)
                cashierId = currentCashier.Id;

            logWriter.WriteLine("1,{0:D5},{1},{2},{3}     {4},{5:D6}{6:D4}", 
                                                                            number++, 
                                                                            docTypeId, 
                                                                            docCode, 
                                                                            registerId, 
                                                                            cashierId, 
                                                                            documentId, 
                                                                            (lastZNo + 1));

            logWriter.WriteLine("1,{0:D5},03,TAR,{1:dd}/{1:MM}/{1:yyyy}  ,{1:HH:mm:ss}    ", number++, DateTime.Now);

            decimal totalAmount = 0;
            decimal documentTotalAmount = document.TotalAmount;
            decimal documentTotalVAT = document.TotalVAT;

            foreach (IFiscalItem item in document.Items)
            {
                try
                {
                    Decimal lineQuantity = 0m;
                    Decimal remainQuantity = Math.Abs(item.Quantity);
                    while (remainQuantity > 0)
                    {
                        logWriter.Write("1,{0:D5},", number++);
                        lineQuantity = (Math.Min(remainQuantity, 99));
                        int quantity = (int)lineQuantity;
                        int rem = (int)Math.Round((lineQuantity - quantity) * 1000, 0);

                        remainQuantity -= Math.Abs(lineQuantity);

                        totalAmount = Math.Round((item.TotalAmount / item.Quantity) * lineQuantity, 2);

                        if (item.Quantity >= 0)
                            logWriter.Write(saleCode + ",");
                        else
                            logWriter.Write("05,IPT,");

                        string[] itemAdjustments = item.GetAdjustments();
                        foreach (string adjustment in itemAdjustments)
                            totalAmount -= Math.Round((Decimal.Parse(adjustment.Split('|')[0]) / item.Quantity) * lineQuantity, 2);

                        logWriter.WriteLine("{0:D2}.{1:D3}{2:D6},{3:D2}{4,10}", Math.Abs(quantity), Math.Abs(rem),
                                                               item.Product.Id,
                                                               item.Product.Department.Id,
                                                               FormatDecimal(Math.Abs(totalAmount), 10));

                        //Barcode 1,rrrrr,20,BKD,bbbbbbbbbbbb,bbbbbbbbbbbb
                        if (Connector.Instance().CurrentSettings.GetProgramOption(Setting.BarcodeLineInMainLogFile) == PosConfiguration.ON)
                        {
                            string barcode = item.Product.Barcode.PadRight(20, ' ');
                            string BKDLine = string.Format("1,{0},", number++.ToString().PadLeft(5, '0'));
                            BKDLine += string.Format("38,BKD,{0},{1}", barcode.Substring(0, 12), barcode.Substring(12));
                            logWriter.WriteLine(BKDLine.PadRight(40, ' '));
                        }

                        //Satici 1,rrrrr,22,STC,kkk     nnnn,
                        if (item.SalesPerson != null)
                            logWriter.WriteLine("1,{0:D5},22,STC,{1,4}{2,8},{2,12}", number++, item.SalesPerson.Id, " ");
                        
                        //Seri no veya IMEI numaralarý 
                        //1,rrrrr,36,SNO,bbbbbbbbbbbb,bbbbbbbbbbbb
                        if (!String.IsNullOrEmpty(item.SerialNo))
                        {
                            string serialNo = item.SerialNo.PadRight(24, ' ');
                            logWriter.WriteLine("1,{0},36,SNO,{1},{2}", number++.ToString().PadLeft(5, '0'),
                                    serialNo.Substring(0, 12),
                                    serialNo.Substring(12).PadRight(12, ' '));
                        }

                        /******* ITEM ADJUSTMENTS    *********/

                        foreach (string adjustment in itemAdjustments)
                        {
                            string[] detail = adjustment.Split('|');// Amount | Percentage | CashierId
                            decimal amount = decimal.Parse(detail[0]);
                            string adjCode = amount > 0 ? "39" : "06";
                            string direction = amount > 0 ? "ART" : "IND";
                            amount = Math.Abs(Math.Round((amount / item.Quantity) * lineQuantity, 2));
                            logWriter.WriteLine("1,{0:D5},{1},{2},SNS {3} %{4},  {5:D10}", number++, adjCode, direction, detail[2], detail[1], FormatDecimal(amount, 10));

                        }
                    }
                }
                catch { }

            }


            /**********  DOCUMENT ADJUSTMENTS ****************/

            Decimal usedPointPrice = 0m;
            String pointCreditValue = Connector.Instance().CurrentSettings.GetProgramOption(Setting.ConnectPointToCredit).ToString();
            Boolean convertPointToPayment = false;

            if (pointCreditValue[0] == '1' && pointCreditValue.Length == 4)
            {
                convertPointToPayment = true;
            }

            totalAmount = documentTotalAmount;
            string[] documentAdjustments = document.GetAdjustments();
            foreach (string adjustment in documentAdjustments)
            {
                string[] detail = adjustment.Split('|');// Amount | Percentage | CashierId
                decimal amount = decimal.Parse(detail[0]);
                string adjCode = amount > 0 ? "39" : "06";
                string direction = amount > 0 ? "ART" : "IND";
                if (convertPointToPayment && detail[2] == "9998")
                {
                    usedPointPrice = Math.Abs(amount);
                    documentTotalAmount -= amount;
                    continue;
                }

                totalAmount -= amount;
                amount = Math.Abs(amount);
                logWriter.WriteLine("1,{0:D5},{1},{2},TOP {3} %{4},  {5:D10}", number++, adjCode, direction, detail[2], detail[1], FormatDecimal(amount, 10));

            }

            string totalLineCode = "08,TOP";
            string statusCode = " ";

            if (docStatus != 0)
            {
                docStatus -= 3;
                totalLineCode = "30,FIP";
                statusCode = docStatus.ToString();
            }

            //Belge toplami 1,rrrrr,08,TOP,            ,  tttttttttt
            logWriter.WriteLine("1,{0:D5},{1},           {2},  {3:D10}", number++,
                                                                         totalLineCode,
                                                                         statusCode,
                                                                         FormatDecimal(document.TotalAmount, 10));

            if (document.DocumentTypeId > -1)
            {
                decimal[,] taxRateTotals = document.TaxRateTotals;
                for (int i = 0; i < taxRateTotals.GetLength(0); i++)
                {
                    int taxRate = (int)(Math.Round(Department.TaxRates[(int)(taxRateTotals[i, 0])] * 100, 0));

                    logWriter.WriteLine("1,{0:D5},07,KDV,          {1:D2},  {2,10}", number++, taxRate, FormatDecimal(taxRateTotals[i, 1], 10));

                }
            }

            //Kasiyer 1,rrrrr,22,STC,kkk     nnnn,
            if (document.SalesPerson != null)
                logWriter.WriteLine("1,{0:D5},22,STC,{1,4}{2,8},{2,12}", number++, document.SalesPerson.Id, " ");

            if (convertPointToPayment && usedPointPrice > 0)
            {
                String paymentType = pointCreditValue[1] == '0' ? "KRD" : "CHK";
                logWriter.WriteLine("1,{0:D5},10,{1},          {2:D2},  {3:D10}", number++, paymentType, pointCreditValue.Substring(2, 2), FormatDecimal(usedPointPrice, 10));
            }

            /*****            PAYMENTS         *******/
            decimal documentBalance = document.TotalAmount;

            String[] checkpayments = document.GetCheckPayments();
            foreach (String checkpayment in checkpayments)
            {
                String[] detail = checkpayment.Split('|');// Amount | RefNumber
                detail[1] = detail[1].PadRight(14, ' ');
                decimal amount = Math.Min(Decimal.Parse(detail[0]), documentBalance);
                logWriter.WriteLine("1,{0:D5},20,CEK,{1,12},{2}{3,8}", number++, detail[1].Substring(0, 12), detail[1].Substring(12, 2), FormatDecimal(amount, 10));
                documentBalance = documentBalance - amount;
            }

            String[] currencypayments = document.GetCurrencyPayments();
            foreach (String currencypayment in currencypayments)
            {
                String[] detail = currencypayment.Split('|');// Amount | Exchange Rate | Name
                decimal amount = Math.Min(Decimal.Parse(detail[0]), documentBalance);
                long quantity = (long)Math.Round(amount * 100m / decimal.Parse(detail[1]), 0);
                int id = 0;
                foreach (ICurrency cur in Connector.Instance().GetCurrencies().Values)
                {
                    if (cur.Name != detail[2]) continue;
                    id = cur.Id;
                    break;
                }
                logWriter.WriteLine("1,{0:D5},21,DVZ,{1:D} {2,10},  {3,10}", number++, (char)id, quantity, FormatDecimal(amount, 10));
                documentBalance = documentBalance - amount;
            }

            String[] creditpayments = document.GetCreditPayments();
            foreach (String creditypayment in creditpayments)
            {
                String[] detail = creditypayment.Split('|');// Amount | Installments | Id
                decimal amount = Math.Min(Decimal.Parse(detail[0]), documentBalance);
                logWriter.WriteLine("1,{0:D5},10,KRD,{1,10}{2:D2},{3:D2}{4,10}", number++, " ", int.Parse(detail[2]), int.Parse(detail[1]), FormatDecimal(amount, 10));
                documentBalance = documentBalance - amount;
            }
            //cash payments must be at lasti because customerchanges always cash
            String[] cashpayments = document.GetCashPayments();
            foreach (String cashpayment in cashpayments)
            {
                decimal amount = Math.Min(Decimal.Parse(cashpayment), documentBalance);
                logWriter.WriteLine("1,{0:D5},09,NAK,            ,  {1,10}", number++, FormatDecimal(amount, 10));
                documentBalance = documentBalance - amount;
            }

            foreach (PointObject po in document.Points)
                logWriter.WriteLine("1,{0:D5},31,BNS,{1,12},{2,12:D10}", number++,
                    Connector.Instance().PointAdapter != null && Connector.Instance().PointAdapter.Online ? 1 : 0, po.Value);

            if (Connector.Instance().CurrentSettings.GetProgramOption(Setting.WriteDocumentID) == PosConfiguration.ON)
            {
                string BIDLine = PosConfiguration.Get("FiscalId") + (lastZNo + 1).ToString().PadLeft(4, '0') + document.Id.ToString().PadLeft(4, '0');
                BIDLine = string.Format("1,{0:D5},24,BID,{1},{2}", number++, BIDLine.Substring(0, 12), BIDLine.Substring(12, BIDLine.Length - 12)).PadRight(40, ' ');
                logWriter.WriteLine(BIDLine);
            }

            //Special Promotion Lines produced by promotion server
            foreach (string promoLog in document.PromoLogLines)
                logWriter.WriteLine("1,{0:D5},{1}", number++, promoLog);
            
            //TODO Belge sonu 1,rrrrr,11,SON,mmmmmmmmmmmm,mmmmmmmmssss
            logWriter.Write("1,{0:D5},11,SON,", number++);
            if (document.Customer != null)
            {
                string code = document.Customer.Code.PadRight(20);
                logWriter.Write("{0},{1}", code.Substring(0, 12), code.Substring(12, 8));
            }
            else
                logWriter.Write("            ,        ");

            logWriter.Write(document.IsOpenDocument ? "   A" : "    ");

            return logWriter;
        }

        private string FormatDecimal(decimal amount, int totalLength)
        {
            return String.Format("{0:0.00}", Math.Abs(amount)).Replace(',', '.').PadLeft(totalLength, '0');
        }
    }
}
