using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS.Printer.GUI
{
    class Formatter
    {
        private static List<String> currentLines;

        #region PageRules

        private static Location timeLocation;
        private static Location dateLocation;
        private static Location customerLocation;
        private static Location taxLocation;

        private static Location documentIdLocation;

        private static Location productNameLocation;
        private static Location productQuantityLocation;
        private static Location productVatLocation;
        private static Location productAmountLocation;

        private static int pageSize;
        private const int maxCustomerWidth = 30;

        private const int maxProductWidth = 20;
        private const int maxAmountWidth = 14;

        #endregion PageRules
              
       
        internal static void SetCoordinates()
        {
            // header coordinates
            String[] headerCoordinate = CurrentSettings.InvoiceCoordinates[0].Split(',');

            customerLocation = new Location(0, 1);
            taxLocation = new Location(0, 7);
            dateLocation = new Location(30, 1);
            timeLocation = new Location(30, 2);
                        
            documentIdLocation = new Location( customerLocation.X, Math.Min(customerLocation.Y - 1, taxLocation.Y - 1));
            
            //fiscal item coordinates

            productNameLocation = new Location(0, 12);
            productQuantityLocation = new Location(10, 12);
            productVatLocation = new Location(25, 12);
            productAmountLocation = new Location(40, 12);
                        
            WordConversion.linemax = productAmountLocation.X - productNameLocation.X;
            
        }


        private static int GetCurrentLine(int requestedPageLine)
        {
            return currentLines.Count;
        }

        private static String FormatLine(Location location, string value)
        {
            if (value.Length > FiscalPrinter.CHAR_PER_LINE)
                value = value.Substring(0, FiscalPrinter.CHAR_PER_LINE);

            String currentValue;

            while (currentLines.Count < location.Y)
            {
                currentLines.Add("".PadRight(FiscalPrinter.CHAR_PER_LINE));
            }
            int yLoc = location.Y - 1;
            int xLoc = location.X;

            currentValue = currentLines[yLoc].Substring(xLoc, value.Length);

            if (!currentValue.Equals("".PadRight(value.Length)))
            {
                String exMsg = String.Format("Printer.InvoicePage.FormatLine : Line Format Error\n Location= X:{0} y:{1} \nValue={2}",
                                                location.X,
                                                location.Y,
                                                value);
                throw new Exception(exMsg);
            }
            currentLines[yLoc] = currentLines[yLoc].Remove(xLoc, value.Length);
            currentLines[yLoc] = currentLines[yLoc].Insert(xLoc, value);

            return currentLines[yLoc];
        }

        public static List<String> FormatReceiptHeader(String name, int id)
        {
            currentLines = new List<string>();


            FormatLine(new Location(20, documentIdLocation.Y + 1), String.Format("{0} NO: {1:D4}", name, id));

            FormatLine(new Location(0, currentLines.Count), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("SAAT {0:t}", DateTime.Now));
            FormatLine(new Location(0, currentLines.Count + 1), "");

            return currentLines;
        }

        internal static List<string> FormatRegisterDocument(int registerDocTypes, decimal amount)
        {
            currentLines = new List<string>();
            List<string> lines = new List<string>();

            //---------------------
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), "".PadRight(FiscalPrinter.CHAR_PER_LINE, '-'));

            String label = "";
            if (registerDocTypes == 101)
                label = PosMessage.RECEIVE_CASH;
            else
                label = PosMessage.ENTER_CASH;

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), label);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(amount).ToString("C")));


            //---------------------
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), "".PadRight(FiscalPrinter.CHAR_PER_LINE, '-'));

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);
            String cashierInfo = String.Empty;
            ICashier cashier = FiscalPrinter.Cashier;

            if (CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON)
                cashierInfo = cashier.Name.TrimEnd();

            cashierInfo = String.Format("{0} : {1} {2}", PosMessage.CASHIER, cashier.Id, cashierInfo);

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), cashierInfo);

            lines.AddRange(currentLines);
            lines.AddRange(FormatEnd());
            return lines;
        }
        public static List<String> FormatHeader(ISalesDocument document)
        {
            currentLines = new List<string>();

            FormatLine(new Location(documentIdLocation.X, documentIdLocation.Y + 1), String.Format("{0} {1} : {2}", document.Name, PosMessage.DOCUMENT_FOLLOWING_ID, document.Id));

            FormatLine(new Location(dateLocation.X, dateLocation.Y + 1), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(timeLocation.X, timeLocation.Y + 1), String.Format("SAAT {0:t}", DateTime.Now));


            bool print =CurrentSettings.GetProgramOption(Setting.NotPrintCustomerLabels) == PosConfiguration.OFF;


            if (document.Customer != null)
            {
                //Musteri kimlik biligileri
                String[] identityItems = document.Customer.Identity;
                //Musteri adres biligileri
                String[] contactItems = document.Customer.Contact;
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 1), (print ? String.Format("{0,-12}: ", PosMessage.CUSTOMER_CODE) : "") + identityItems[0]);
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 2), (print ? String.Format("{0,-15}: ", PosMessage.NAME) : "") + identityItems[1]);
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 3), (print ? String.Format("{0,-15}: ", PosMessage.ADDRESS) : "") + contactItems[0]);
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 4), (print ? String.Format("{0,-15}: ", PosMessage.ADDRESS) : "") + contactItems[1]);
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 5), (print ? String.Format("{0,-15}: ", PosMessage.ADDRESS) : "") + contactItems[2]);

                FormatLine(new Location(taxLocation.X, taxLocation.Y + 1), (print ? String.Format("{0,-15}: ", PosMessage.TAX_NUMBER) : "") + contactItems[4]);
                FormatLine(new Location(taxLocation.X, taxLocation.Y + 2), (print ? String.Format("{0,-15}: ", PosMessage.TAX_INSTITUTION) : "") + contactItems[3]);
            }
            FormatLine(productNameLocation, "");//set cursor

            
            return currentLines;
        }

        public static List<String> FormatInfo(String line)
        {
            currentLines = new List<string>();
            int left = FiscalPrinter.CHAR_PER_LINE - line.Length;
            while (left < 0)
            {
                FormatLine(new Location(0, currentLines.Count + 1), line.Substring(0, FiscalPrinter.CHAR_PER_LINE));
                line = line.Substring(FiscalPrinter.CHAR_PER_LINE); 
                left = FiscalPrinter.CHAR_PER_LINE - line.Length;
            }
            left = left / 2;
            FormatLine(new Location(left, currentLines.Count + 1), line);            
            return currentLines;
        }
        public static List<String> Format(IAdjustment[] ai)
        {
            Decimal subtotalDiscount = 0;

            foreach (IAdjustment a in ai)
                subtotalDiscount += a.NetAmount;
            
            currentLines = new List<string>();

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Format("{0}", PosMessage.SUBTOTAL));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(FiscalPrinter.Document.TotalAmount).ToString("C")));

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), subtotalDiscount > 0 ? PosMessage.FEE : PosMessage.DISCOUNT);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(subtotalDiscount).ToString("C")));


            return currentLines;
        }

        public static List<String> Format(IAdjustment ai)
        {

            currentLines = new List<string>();

            if (ai.Target is ISalesDocument)
            {
                if (CurrentSettings.GetProgramOption(Setting.PrintSubtTotal) == PosConfiguration.OFF)
                {
                    FormatSubTotal(FiscalPrinter.Document, true);
                }
            }
            switch (ai.Method)
            {
                case AdjustmentType.PercentDiscount:
                case AdjustmentType.PercentFee:
                    FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), ai.Label);
                    FormatLine(new Location(productNameLocation.X + 12, currentLines.Count), String.Format("%{0}", ai.RequestValue.ToString().PadLeft(2, ' ')));
                    FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(ai.NetAmount).ToString("C")));
                    break;
                case AdjustmentType.Discount:
                case AdjustmentType.Fee:
                    FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), ai.Label);
                    FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(ai.NetAmount).ToString("C")));
                    break;
            }

            return currentLines;
        }

        public static List<String> Format(Adjustment ai)
        {

            currentLines = new List<string>();

            switch (ai.Type)
            {
                case AdjustmentType.PercentDiscount:
                case AdjustmentType.PercentFee:
                    FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), (ai.Type == AdjustmentType.PercentFee ? "+" : "-"));
                    FormatLine(new Location(productNameLocation.X + 1, currentLines.Count), String.Format("%{0}", ai.percentage.ToString().PadLeft(2, ' ')));
                    FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(ai.Amount).ToString("C")));
                    break;
                case AdjustmentType.Discount:
                case AdjustmentType.Fee:
                    FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), ai.Type == AdjustmentType.Discount ? "ÝNDÝRÝM" : "ARTTIRIM");
                    FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(ai.Amount).ToString("C")));
                    break;
            }

            return currentLines;
        }

        public static List<String> Format(IFiscalItem fi)
        {

            currentLines = new List<string>();

            if (fi.Quantity != 1)
            {
                string specifier = "Q";
                if (fi.Quantity % 1 > 0)
                    specifier = "0.000";
                FormatLine(new Location(productQuantityLocation.X, currentLines.Count + 1), String.Format("{0} {1} X {2}", new Number(fi.Quantity-fi.VoidQuantity).ToString(specifier), fi.Unit, new Number(fi.UnitPrice).ToString("C")));
            }

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Format("{0,-" + maxProductWidth + "}", fi.Name.Trim()));
            FormatLine(new Location(productVatLocation.X - 3, currentLines.Count), String.Format("%{0}", ((int)(Department.TaxRates[fi.TaxGroupId -1])).ToString().PadLeft(2, ' ')));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(fi.ListedAmount).ToString("C")));

            if (CurrentSettings.GetProgramOption(Setting.PrintProductBarcode) == PosConfiguration.ON)
                FormatRemark(fi.Barcode);

            return currentLines;
        }
       
        public static List<String> FormatPayment(Decimal amount, String label)
        {
            
            currentLines = new List<string>();

            String payment = label;
                        
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), payment);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(amount).ToString("C")));

            return currentLines;
        }

        public static List<String> FormatTotals(ISalesDocument document, bool hardcopy)
        {
            currentLines = new List<string>();

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.TOTALTAX);

            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(document.TotalVAT).ToString("C")));

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.TOTAL);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(document.TotalAmount).ToString("C")));

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);

            String[] inWords = WordConversion.ConvertLetter(document.TotalAmount).Split(new char[] { '\n' }); //Empty entries!! TODO CF

            for (int i = 0; i < inWords.Length; i++)
                FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), inWords[i]);

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);

            return currentLines;
        }

        public static List<String> FormatTotals(decimal TotalAmount, decimal VATTotal)
        {
            currentLines = new List<string>();

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.TOTALTAX);

            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(VATTotal).ToString("C")));

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.TOTAL);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(TotalAmount).ToString("C")));

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);

            String[] inWords = WordConversion.ConvertLetter(TotalAmount).Split(new char[] { '\n' }); //Empty entries!! TODO CF

            for (int i = 0; i < inWords.Length; i++)
                FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), inWords[i]);

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);

            return currentLines;
        }

        public static List<String> FormatEnd()
        {
            currentLines = new List<string>();

            // End of Receipt Note
            if(FiscalPrinter.Printer.EndOfReceiptNote != null &&
                FiscalPrinter.Printer.EndOfReceiptNote.Length > 0)
            {
                FormatRemark(" ");

                string note = "";
                foreach (String line in FiscalPrinter.Printer.EndOfReceiptNote)
                {
                    note = line;
                    if (note.Length > FiscalPrinter.CHAR_PER_LINE)
                        note = line.Substring(0, FiscalPrinter.CHAR_PER_LINE);
                    FormatRemark(note);
                }

                FormatRemark(" ");
            }

            FormatLine(new Location(0, currentLines.Count + 1), "EKÜ NO : 1234");
            FormatLine(new Location(FiscalPrinter.CHAR_PER_LINE - 11, currentLines.Count), String.Format("Z NO : {0:D4}", FiscalPrinter.Printer.LastZReportNo));
            FormatLine(new Location(0, currentLines.Count + 1), "GUI0000001");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            List<string> lines = new List<string>(currentLines);
            FormatInfo("-- SON --");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            lines.AddRange(currentLines);
            return lines;
        }
        public static List<String> FormatFooter(ISalesDocument document)
        {
            currentLines = new List<string>();
            List<string> lines = new List<string>();

            String cashierInfo = String.Empty;
            ICashier cashier = FiscalPrinter.Cashier;

            if (document.DocumentTypeId < 0)
            {
                if (CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON)
                    cashierInfo = cashier.Name.TrimEnd();

                cashierInfo = String.Format("{0} : {1} {2} {3} {4}", PosMessage.CASHIER, cashier.Id, cashierInfo, FiscalPrinter.RegisterId, PosConfiguration.Get("OfficeNo"));

                FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), cashierInfo);
                
                lines.AddRange(currentLines);
                lines.AddRange(FormatEnd());
                return lines;
            }

            if (document.CustomerChange > 0)
            {
                FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.CHANGE);
                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(document.CustomerChange).ToString("C")));
            }
            

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);

            String vat_message = PosMessage.VAT;

            decimal[,] taxRateTotals = FiscalPrinter.Document.TaxRateTotals;

            if (taxRateTotals.Length > 0)
            {
                FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.VAT_DISTRIBUTION);
                FormatLine(new Location(productVatLocation.X - vat_message.Length, currentLines.Count), vat_message);
                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", PosMessage.SALE));
            }
            for (int i = 0; i < taxRateTotals.GetLength(0); i++)
            {
                int taxRate = (int)(Math.Round(Department.TaxRates[(int)(taxRateTotals[i, 0])] * 100, 0));

                FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Format("{0} %{1}", PosMessage.SELLING_VAT, taxRate.ToString().PadLeft(2, ' ')));
                FormatLine(new Location(productVatLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", String.Format("*{0:C}", new Number(taxRateTotals[i, 1]))));
                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", String.Format("*{0:C}", new Number(taxRateTotals[i, 2]))));
            }
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Empty);

            if (CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON)
                cashierInfo = cashier.Name.TrimEnd();

            cashierInfo = String.Format("{0} : {1} {2} {3} {4}", PosMessage.CASHIER, cashier.Id, cashierInfo, FiscalPrinter.RegisterId, PosConfiguration.Get("OfficeNo"));

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), cashierInfo);
            lines.AddRange(currentLines);
            lines.AddRange(FormatEnd());
            return lines;
        }

        public static List<String> FormatSubTotal(ISalesDocument document, bool hardcopy)
        {
            currentLines = new List<string>();

            if (hardcopy)
            {
                FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.SUBTOTAL);
                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(document.TotalAmount).ToString("C")));
            }
            return currentLines;
        }

        public static String FormatRemark(String remark)
        {
            return FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), "## " + remark);
        }

        public static List<String> FormatBarcode()
        {
            currentLines = new List<string>();

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), "");
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), "             ### BARCODE ###            ");
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1),"            " + PosConfiguration.Get("FiscalId").Substring(2,8) + (FiscalPrinter.Printer.LastZReportNo +1).ToString().PadLeft(4, '0') + FiscalPrinter.Document.Id.ToString().PadLeft(4, '0') + "            ");
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), "             ### BARCODE ###            ");
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), "");

            return currentLines;
        }

        public static String FormatSlipLine(String line)
        {

            currentLines = new List<string>();
            return FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), line);
        }

        public static List<String> FormatFooterNotes()
        {
            currentLines = new List<string>();

            if (FiscalPrinter.Document.FootNote != null)
            {
                if (FiscalPrinter.Document.FootNote.Count > 0)
                {
                    FormatRemark(String.Empty.PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));

                    string note = "";
                    foreach (String line in FiscalPrinter.Document.FootNote)
                    {
                        note = line;
                        if (note.Length > 32)
                            note = line.Substring(0, FiscalPrinter.CHAR_PER_LINE);
                        FormatRemark(note.PadRight(FiscalPrinter.CHAR_PER_LINE, ' '));
                    }

                    FormatRemark(String.Empty.PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));
                }
            }

            return currentLines;
        }

        internal static List<string> FormatCollectionDocumentHeader(string cllctionSerial, DateTime dt, decimal amount, string subscriberNo, string instutionNAme, decimal comission)
        {
            currentLines = new List<string>();

            FormatLine(new Location(documentIdLocation.X, documentIdLocation.Y + 1), String.Format("{0} NO: {1:D4}", PosMessage.RECEIPT_TR, FiscalPrinter.Document.Id));

            FormatLine(new Location(dateLocation.X, dateLocation.Y + 1), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(timeLocation.X, timeLocation.Y + 1), String.Format("SAAT {0:t}", DateTime.Now));

            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), "              *BÝLGÝ FÝÞÝ*              ");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TÜRÜ", FiscalPrinter.Document.Name));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "KURUMU", instutionNAme));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TARÝH", dt.ToString("dd.MM.yyyy")));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "FATURA NO", cllctionSerial));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "ABONE NO", subscriberNo));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "FATURA TUTARI", amount));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "KOMÝSYON", comission));

            FormatLine(new Location(0, currentLines.Count + 1), "".PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));

            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Format("{0,-" + maxProductWidth + "}", "FATURA TAHSÝL ÜCRETÝ"));
            FormatLine(new Location(productVatLocation.X - 3, currentLines.Count), String.Format("%{0}", "18".PadLeft(2, ' ')));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(comission).ToString("C")));

            return currentLines;
        }

        internal static List<string> FormatParkDocumentHeader(string plate, DateTime parkingDT)
        {
            currentLines = new List<string>();

            FormatLine(new Location(documentIdLocation.X, documentIdLocation.Y + 1), String.Format("{0} NO: {1:D4}", PosMessage.RECEIPT_TR, FiscalPrinter.Document.Id));

            FormatLine(new Location(dateLocation.X, dateLocation.Y + 1), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(timeLocation.X, timeLocation.Y + 1), String.Format("SAAT {0:t}", DateTime.Now));

            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), "              *BÝLGÝ FÝÞÝ*              ");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TÜRÜ", FiscalPrinter.Document.Name));
            FormatLine(new Location(0, currentLines.Count + 1), "".PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));
            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-20}: {1}", "ARAÇ PLAKA NO", plate));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-20}: {1}", "ARAÇ GÝRÝÞ TARÝHÝ", parkingDT.ToString("dd-MM-yyyy")));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-20}: {1}", "ARAÇ GÝRÝÞ SAATÝ", parkingDT.ToString("HH:mm")));

            FormatLine(new Location(0, currentLines.Count + 1), "".PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));

            return currentLines;
        }

        internal static List<string> FormatFoodDocumentHeader()
        {
            currentLines = new List<string>();

            FormatLine(new Location(documentIdLocation.X, documentIdLocation.Y + 1), String.Format("{0} NO: {1:D4}", PosMessage.RECEIPT_TR, FiscalPrinter.Document.Id));

            FormatLine(new Location(dateLocation.X, dateLocation.Y + 1), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(timeLocation.X, timeLocation.Y + 1), String.Format("SAAT {0:t}", DateTime.Now));

            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), "              *BÝLGÝ FÝÞÝ*              ");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TÜRÜ", FiscalPrinter.Document.Name));

            return currentLines;
        }

        internal static List<string> FormatInvoiceHeader(int documentTypeId, string tcknVkn, string serial, DateTime issueDate)
        {
            currentLines = new List<string>();

            FormatLine(new Location(documentIdLocation.X, documentIdLocation.Y + 1), String.Format("{0} NO: {1:D4}", PosMessage.RECEIPT_TR, FiscalPrinter.Document.Id));

            FormatLine(new Location(dateLocation.X, dateLocation.Y + 1), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(timeLocation.X, timeLocation.Y + 1), String.Format("SAAT {0:t}", DateTime.Now));

            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), "              *BÝLGÝ FÝÞÝ*              ");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TÜRÜ", FiscalPrinter.Document.Name));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TCKN/VKN", tcknVkn));
            if(documentTypeId == 1)
                FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "SERÝ-SIRA NO", serial));

            FormatLine(new Location(0, currentLines.Count + 1), "".PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));

            return currentLines;
        }

        internal static List<string> FormatAdvanceDocumentHeader(string tcknVkn, string title, decimal totalAmount)
        {
            currentLines = new List<string>();

            FormatLine(new Location(documentIdLocation.X, documentIdLocation.Y + 1), String.Format("{0} NO: {1:D4}", PosMessage.RECEIPT_TR, FiscalPrinter.Document.Id));

            FormatLine(new Location(dateLocation.X, dateLocation.Y + 1), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(timeLocation.X, timeLocation.Y + 1), String.Format("SAAT {0:t}", DateTime.Now));

            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), "              *BÝLGÝ FÝÞÝ*              ");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TÜRÜ", FiscalPrinter.Document.Name));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TCKN/VKN", tcknVkn));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "AD-SOYAD/UNVAN", title));

            FormatLine(new Location(0, currentLines.Count + 1), "".PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-20}: *{1}", "TAHSÝL EDÝLEN TUTAR:", totalAmount));
            FormatLine(new Location(0, currentLines.Count + 1), "".PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));

            return currentLines;
        }

        internal static List<string> FormatCurrentAccountCollectionHeader(string tcknVkn, string custName, string docSerial, DateTime dt, decimal totalAmount)
        {
            currentLines = new List<string>();

            FormatLine(new Location(documentIdLocation.X, documentIdLocation.Y + 1), String.Format("{0} NO: {1:D4}", PosMessage.RECEIPT_TR, FiscalPrinter.Document.Id));

            FormatLine(new Location(dateLocation.X, dateLocation.Y + 1), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(timeLocation.X, timeLocation.Y + 1), String.Format("SAAT {0:t}", DateTime.Now));

            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), "              *BÝLGÝ FÝÞÝ*              ");
            FormatLine(new Location(0, currentLines.Count + 1), "");
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TÜRÜ", FiscalPrinter.Document.Name));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "MÜÞTERÝ TCKN", tcknVkn));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "AD-SOYAD/UNVAN", custName));
            FormatLine(new Location(0, currentLines.Count + 1), "TAHSÝL EDÝLEN BELGENÝN");
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "TARÝHÝ", dt.ToString("dd.MM.yyyy")));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-15}: {1}", "SERÝ NO'SU", docSerial));

            FormatLine(new Location(0, currentLines.Count + 1), "".PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));
            FormatLine(new Location(0, currentLines.Count + 1), String.Format("{0,-20}: *{1}", "TAHSÝL EDÝLEN TUTAR:", totalAmount));
            FormatLine(new Location(0, currentLines.Count + 1), "".PadLeft(FiscalPrinter.CHAR_PER_LINE, '-'));

            return currentLines;
        }

        public static List<String> FormatVoid(IAdjustment ai)
        {
            currentLines = new List<string>();

            if (ai.Target is ISalesDocument)
            {
                if (CurrentSettings.GetProgramOption(Setting.PrintSubtTotal) == PosConfiguration.OFF)
                {
                    FormatSubTotal(FiscalPrinter.Document, true);
                }
            }
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.CORRECTION);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(-1 * ai.NetAmount).ToString("C")));

            return currentLines;
        }

        public static List<String> FormatVoid(IFiscalItem fi)
        {
            currentLines = new List<string>();

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), PosMessage.VOID.PadLeft(PosMessage.VOID.Length + 4, ' '));

            if (fi.Quantity != -1)
            {
                string specifier = "Q";
                if (fi.Quantity % 1 < 0)
                    specifier = "0.000";
                FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Format("    {0} {1} X {2}", new Number(fi.Quantity).ToString(specifier), fi.Unit, new Number(fi.UnitPrice).ToString("C")));
            }
            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Format("{0,-" + maxProductWidth + "}", fi.Name.Trim()));
            FormatLine(new Location(productVatLocation.X - 3, currentLines.Count), String.Format("%{0}", ((int)(Department.TaxRates[fi.TaxGroupId - 1])).ToString().PadLeft(2, ' ')));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(fi.TotalAmount).ToString("C")));

            if (CurrentSettings.GetProgramOption(Setting.PrintProductBarcode) == PosConfiguration.ON)
                FormatRemark(fi.Barcode);

            return currentLines;
        }

        public static List<String> FormatCorrect(IFiscalItem fi)
        {
            currentLines = new List<string>();

            FormatLine(new Location(productNameLocation.X, currentLines.Count + 1), String.Format("{0,-" + maxProductWidth + "}", PosMessage.CORRECTION));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, currentLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(fi.TotalAmount).ToString("C")));

            return currentLines;
        }

        public static List<String> FormatVoid(ISalesDocument sDoc)
        {
            currentLines = new List<string>();

            FormatLine(new Location(productQuantityLocation.X, currentLines.Count + 1), "           ");
            if(sDoc.DocumentTypeId < 0)
                FormatLine(new Location(productQuantityLocation.X, currentLines.Count + 1), "BELGE ÝPTAL");
            else
                FormatLine(new Location(productQuantityLocation.X, currentLines.Count + 1), "*BÝLGÝ FÝÞÝ ÝPTAL*");
            FormatLine(new Location(productQuantityLocation.X, currentLines.Count + 1), "           ");

            return currentLines;
        }

        private static ISettings CurrentSettings
        {
            get { return Data.Connector.Instance().CurrentSettings; }
        }
    }
}
