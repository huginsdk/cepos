using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    class InvoicePage
    {
        private static List<InvoicePage> invoicePages;
        private List<String> pageLines;

        private Decimal subTotal;
        private bool slipRequestMode = true;//says if document needs lines to write 
        private bool totalPrinted = false;
        
        public static int REMARKCOUNT = 0;
        static SlipException coordinateError = null;

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

        private static int maxCharsInLine = 40;
        private const int maxPageSize = 80;
        private int minPageSize = 24;

        #endregion PageRules

        //to do :RequestNextSlip and CallMethod are removed so work on them, ie: they are necessary? or how can solve without them 

        internal InvoicePage()
        {
            slipRequestMode = true;
            if (SlipPrinter.Invoicepage == null)
            {
                try
                {
                    SetCoordinates();
                    invoicePages = new List<InvoicePage>();
                    coordinateError = null;
                }
                catch (SlipException se)
                {
                    FiscalPrinter.Log.Warning(se);
                    coordinateError = se;
                }
            }
            else
            {
                if (coordinateError != null)
                    throw coordinateError;
                pageLines = new List<string>();
                pageLines.Capacity = pageSize;
            }
        }
        internal static int PageSize
        {
            get { return pageSize; }
        }
        internal static int MaxCharsInLine
        {
            get { return maxCharsInLine; }
        }
        private void SetCoordinates()
        {
            // header coordinates
            String[] headerCoordinate = CurrentSettings.InvoiceCoordinates[0].Split(',');

            timeLocation = new Location(Int32.Parse(headerCoordinate[0].Substring(0, 2)), Int32.Parse(headerCoordinate[0].Substring(2, 2)));
            dateLocation = new Location(Int32.Parse(headerCoordinate[1].Substring(0, 2)), Int32.Parse(headerCoordinate[1].Substring(2, 2)));
            customerLocation = new Location(Int32.Parse(headerCoordinate[2].Substring(0, 2)), Int32.Parse(headerCoordinate[2].Substring(2, 2)));
            taxLocation = new Location(Int32.Parse(headerCoordinate[3].Substring(0, 2)), Int32.Parse(headerCoordinate[3].Substring(2, 2)));

            if (timeLocation.X < 0 || dateLocation.X < 0 || customerLocation.X < 0 || taxLocation.X < 0)
                throw new NegativeCoordinateException(); //throw new SlipCoordinateException("KOORDiNATLAR SIFIRDAN\nKÜÇÜK OLMAMALIDIR");

            // EPSON TM-H5000
            if (timeLocation.X == 70)
                maxCharsInLine = 80;

            documentIdLocation = new Location(customerLocation.X, customerLocation.Y);
            documentIdLocation.Y = Math.Min(customerLocation.Y - 1, taxLocation.Y - 1);
            if (headerCoordinate.Length > 4)
                documentIdLocation.X = Int32.Parse(headerCoordinate[4].Substring(0, 2));

            if (Math.Abs(customerLocation.Y - taxLocation.Y) < 5)
                throw new CustomerTaxCoordinateException();  //throw new SlipCoordinateException("MUSTERI - VERGI \nY KOORDINAT HATASI");

            if (timeLocation.Y >= customerLocation.Y && 
                timeLocation.Y <= customerLocation.Y + 5 && 
                timeLocation.X - customerLocation.X < maxCustomerWidth)
                    throw new CustomerTimeCoordinateException();  //throw new SlipCoordinateException("SAAT - MUSTERI BILGI\nKOORDINAT HATASI");
            
            if (timeLocation.Y >= taxLocation.Y && 
                timeLocation.Y <= taxLocation.Y + 2 && 
                timeLocation.X - taxLocation.X < maxCustomerWidth)
                    throw new TimeTaxCoordinateException();  //throw new SlipCoordinateException("SAAT - MUSTERI VERGI\nKOORDINAT HATASI");

            if (dateLocation.Y >= customerLocation.Y && 
                dateLocation.Y <= customerLocation.Y + 5 && 
                dateLocation.X - customerLocation.X < maxCustomerWidth)
                    throw new CustomerDateCoordinateException();  //throw new SlipCoordinateException("TARÝH - MUSTERI BILGI\nKOORDINAT HATASI");
            
            if (dateLocation.Y >= taxLocation.Y && 
                dateLocation.Y <= taxLocation.Y + 2 && 
                dateLocation.X - taxLocation.X < maxCustomerWidth)
                    throw new DateTaxCoordinateException();  //throw new SlipCoordinateException("TARÝH - MUSTERI VERGI\nKOORDINAT HATASI");

            if ((timeLocation.X + 10) > maxCharsInLine || (dateLocation.X + 10) > maxCharsInLine)
                throw new CoordinateOutOfInvoiceException();  //throw new SlipCoordinateException("SAAT KOORD. FATURA\nGENISLIGINDEN FAZLA");

            //fiscal item coordinates

            String[] productCoordinate =CurrentSettings.InvoiceCoordinates[1].Split(',');

            productNameLocation = new Location(Int32.Parse(productCoordinate[0].Substring(0, 2)), Int32.Parse(productCoordinate[0].Substring(2, 2)));
            productQuantityLocation = new Location(Int32.Parse(productCoordinate[1]), Int32.Parse(productCoordinate[0].Substring(2, 2)) - 1);
            productAmountLocation = new Location(Int32.Parse(productCoordinate[2]), Int32.Parse(productCoordinate[0].Substring(2, 2)));
            productVatLocation = new Location(Int32.Parse(productCoordinate[3]), Int32.Parse(productCoordinate[0].Substring(2, 2)));

            if (productNameLocation.X < 0 || productQuantityLocation.X < 0 || productVatLocation.X < 0 || productAmountLocation.X < 0)
                throw new NegativeCoordinateException(); //throw new SlipCoordinateException("KOORDiNATLAR SIFIRDAN\\nKÜÇÜK OLMAMALIDIR");

            pageSize = Int32.Parse(productCoordinate[4]);
            if (pageSize > maxPageSize) pageSize = maxPageSize;
            
            foreach (decimal d in CurrentSettings.TaxRates)
            {
                if (d > 0)
                {
                    minPageSize++;
                }
            }
            if (pageSize < minPageSize)
                throw new CoordinateOutOfInvoiceException("MÝNÝMUM SATIR\nSAYISI : " + minPageSize);

            WordConversion.linemax = productAmountLocation.X - productNameLocation.X;
            //InvoicePage Exceptions.

            if (productVatLocation.X - productNameLocation.X < (maxProductWidth + 3))
                throw new NameVATCoordinateException(); //throw new SlipCoordinateException("URUN ISIM - URUN KDV\nKOORDINAT HATASI");
            
            if (productAmountLocation.X - productVatLocation.X < maxAmountWidth)
                throw new AmountVATCoordinateException(); //throw new SlipCoordinateException("URUN MIKTAR - URUN KDV\nKOORDINAT HATASI");

            if (productNameLocation.Y < (documentIdLocation.Y + 9))
                throw new ProductCoordinateException(); //throw new SlipCoordinateException("URUN - FATURA BASLANGIC\nKOORDINAT HATASI");

            if (productQuantityLocation.X > maxCharsInLine || productAmountLocation.X > maxCharsInLine)
                throw new CoordinateOutOfInvoiceException(); //throw new SlipCoordinateException("URUN KOORD. FATURA\nGENISLIGINDEN FAZLA");
        }

        internal Decimal SubTotal
        {
            get { return subTotal; }
            set { subTotal = value; }
        }

        internal int Id
        {
            get { return invoicePages.Count; }
        }
      
        internal InvoicePage CurrentPage
        {
            get { return invoicePages[invoicePages.Count - 1]; }
            set { invoicePages.Add(value); }
        }
    
        public List<String> PageLines
        {
            get { return pageLines; }
        }
    
        /// <summary>
        /// when no slip error occured, printerresponce checks the if page is really full
        /// </summary>
        internal bool IsPageEnd
        {
            get
            {
                if (invoicePages == null || invoicePages.Count <= 0) return false;
                return slipRequestMode;
            }
            set { slipRequestMode = value; }
        }

        private int GetCurrentLine(int requestedPageLine)
        {
            if (pageSize < (CurrentPage.PageLines.Count + requestedPageLine) + 4)
            {
                SlipPrinter fprinter = FiscalPrinter.Printer as SlipPrinter;
                fprinter.RequestSlip();
                GetCurrentLine(requestedPageLine);
            }

            return CurrentPage.PageLines.Count;
        }

        private String FormatLine(Location location, string value)
        {
            String currentValue;

            while (CurrentPage.PageLines.Count < location.Y)
            {
                CurrentPage.PageLines.Add("".PadRight(maxCharsInLine));
            }
            int yLoc = location.Y - 1;
            int xLoc = location.X;
            if ((xLoc + value.Length) > CurrentPage.PageLines[yLoc].Length)
                throw new CoordinateOutOfInvoiceException();
            currentValue = CurrentPage.PageLines[yLoc].Substring(xLoc, value.Length);

            if (!currentValue.Equals("".PadRight(value.Length)))
                throw new CoordinateOutOfInvoiceException("Printer.InvoicePage.FormatLine : Line Format Error");

            CurrentPage.PageLines[yLoc] = CurrentPage.PageLines[yLoc].Remove(xLoc, value.Length);
            CurrentPage.PageLines[yLoc] = CurrentPage.PageLines[yLoc].Insert(xLoc, value);

            return CurrentPage.PageLines[yLoc];
        }

        public List<String> FormatHeader(ISalesDocument document)
        {
            if (document.IsEmpty && invoicePages.Count > 0)
                invoicePages.Clear();

            invoicePages.Add(new InvoicePage());

            FormatLine(new Location(documentIdLocation.X, documentIdLocation.Y + 1), String.Format("{0} {1} : {2}", document.Name, PosMessage.DOCUMENT_FOLLOWING_ID, document.Id));

            FormatLine(new Location(dateLocation.X, dateLocation.Y + 1), String.Format("{0:dd/MM/yyyy}", DateTime.Now));
            FormatLine(new Location(timeLocation.X, timeLocation.Y + 1), String.Format("{0} {1:t}",PosMessage.RECEIPT_TIME, DateTime.Now));


            bool print = CurrentSettings.GetProgramOption(Setting.NotPrintCustomerLabels) == PosConfiguration.OFF;

            if (!String.IsNullOrEmpty(document.SlipOrderNo) && !String.IsNullOrEmpty(document.SlipSerialNo))
            {
                string slipSerial = document.SlipSerialNo + document.SlipOrderNo;
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 1), (print ? String.Format("{0,-15}: ", PosMessage.SERIAL) : "") + slipSerial.Trim());
            }

            string tcknVkn = "";
            String label = "";
            if (document.Customer != null && !String.IsNullOrEmpty(document.Customer.Contact[4]))
                tcknVkn = document.Customer.Contact[4];
            else
                tcknVkn = document.TcknVkn;

            if (tcknVkn.Trim().Length == 11) label = "TCKN";
            if (tcknVkn.Trim().Length == 10) label = "VKN";
            FormatLine(new Location(customerLocation.X, customerLocation.Y + 2), (print ? String.Format("{0,-15}: ", label) : "") + tcknVkn.Trim());

            if (document.Customer != null)
            {
                //Musteri kimlik biligileri
                String[] identityItems = document.Customer.Identity;
                //Musteri adres biligileri
                String[] contactItems = document.Customer.Contact;
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 3), (print ? String.Format("{0,-12}: ", PosMessage.CUSTOMER_CODE) : "") + identityItems[0]);
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 4), (print ? String.Format("{0,-15}: ", PosMessage.NAME) : "") + identityItems[1]);
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 5), (print ? String.Format("{0,-15}: ", PosMessage.ADDRESS) : "") + contactItems[0]);
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 6), (print ? String.Format("{0,-15}: ", PosMessage.ADDRESS) : "") + contactItems[1]);
                FormatLine(new Location(customerLocation.X, customerLocation.Y + 7), (print ? String.Format("{0,-15}: ", PosMessage.ADDRESS) : "") + contactItems[2]);

                FormatLine(new Location(taxLocation.X, taxLocation.Y + 2), (print ? String.Format("{0,-15}: ", PosMessage.TAX_NUMBER) : "") + contactItems[4]);
                FormatLine(new Location(taxLocation.X, taxLocation.Y + 3), (print ? String.Format("{0,-15}: ", PosMessage.TAX_INSTITUTION) : "") + contactItems[3]);
            }


            FormatLine(productNameLocation, "");//set cursor

            if (this.Id > 1 && !totalPrinted)
            {
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), PosMessage.DEPOSITED_AMOUNT);
                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                    String.Format("{0," + maxAmountWidth + "}", "*" + new Number(SubTotal).ToString("C")));
            }
            return CurrentPage.PageLines.GetRange(0, CurrentPage.PageLines.Count);
        }

        public List<String> Format(IAdjustment[] ai)
        {
            Decimal subtotalDiscount = 0;

            foreach (IAdjustment a in ai)
                subtotalDiscount += a.NetAmount;

            int currentLine = GetCurrentLine(2);

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Format("{0}", PosMessage.SUBTOTAL));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                String.Format("{0," + maxAmountWidth + "}", "*" + new Number(FiscalPrinter.Document.TotalAmount).ToString("C")));

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), 
                subtotalDiscount > 0 ? PosMessage.FEE : PosMessage.DISCOUNT);

            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                String.Format("{0," + maxAmountWidth + "}", "*" + new Number(subtotalDiscount).ToString("C")));


            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public List<String> Format(Adjustment ai)
        {
            int currentLine = GetCurrentLine(2);
            //if (ai.Target is ISalesDocument)
            //{
            //    if (CurrentSettings.GetProgramOption(Setting.PrintSubtTotal) == PosConfiguration.OFF)
            //    {
            //        FormatSubTotal(FiscalPrinter.Document);
            //    }
            //}
            switch (ai.Type)
            {
                case AdjustmentType.PercentDiscount:
                case AdjustmentType.PercentFee:
                    string sign = ai.Type == AdjustmentType.PercentDiscount ? "-" : "+";
                    FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Format("{0}%{1}", sign, ai.percentage.ToString().PadLeft(2, '0')));
                    FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(ai.Amount).ToString("C")));
                    break;
                case AdjustmentType.Discount:
                case AdjustmentType.Fee:
                    string type = ai.Type == AdjustmentType.Discount ? PosMessage.REDUCTION : PosMessage.FEE;
                    FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), type);
                    FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(ai.Amount).ToString("C")));
                    break;
            }

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public List<String> Format(IFiscalItem fi)
        {
            int currentLine = 0;
            if (CurrentSettings.GetProgramOption(Setting.PrintProductBarcode) == PosConfiguration.ON)
                currentLine = GetCurrentLine(3);
            else currentLine = GetCurrentLine(2);
            if (fi.Quantity != 1)
            {
                string specifier = "Q";
                if (fi.Quantity % 1 > 0)
                    specifier = "0.000";
                FormatLine(new Location(productQuantityLocation.X, CurrentPage.PageLines.Count + 1), String.Format("{0} {1} X {2}", new Number(fi.Quantity - fi.VoidQuantity).ToString(specifier), fi.Unit, new Number(fi.UnitPrice).ToString("C")));
            }

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Format("{0,-" + maxProductWidth + "}", fi.Name.Trim()));
            FormatLine(new Location(productVatLocation.X - 3, CurrentPage.PageLines.Count), String.Format("%{0:D2}", (int)(Department.TaxRates[fi.TaxGroupId - 1])));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(fi.TotalAmount - fi.VoidAmount).ToString("C")));

            if (CurrentSettings.GetProgramOption(Setting.PrintProductBarcode) == PosConfiguration.ON)
                FormatRemark(fi.Barcode);

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public List<String> FormatPayment(Decimal amount, String label)
        {
            String payment = label;
            int currentLine = GetCurrentLine(1);

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), payment);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                String.Format("{0," + maxAmountWidth + "}", "*" + new Number(amount).ToString("C")));

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public List<String> FormatTotals(ISalesDocument document)
        {
            int currentLine = GetCurrentLine(6);

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Empty);
            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), PosMessage.TAX_BOLD);

            FormatLine(new Location(productAmountLocation.X - maxAmountWidth - 2, CurrentPage.PageLines.Count),
                String.Format("²{0," + maxAmountWidth + "}³", "*" + new Number(document.TotalVAT).ToString("C")));

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), PosMessage.SHORT_TOTAL_BOLD);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth - 2, CurrentPage.PageLines.Count),
                String.Format("²{0," + maxAmountWidth + "}³", "*" + new Number(document.TotalAmount).ToString("C")));

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Empty);

            String[] inWords = WordConversion.ConvertLetter(document.TotalAmount).Split(new char[] { '\n' }); //Empty entries!! TODO CF

            for (int i = 0; i < inWords.Length; i++)
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), inWords[i]);

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Empty);

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public List<String> FormatFooter(ISalesDocument document)
        {
            int currentLine = 3 + FiscalPrinter.Document.TaxRateTotals.GetLength(0);
            
            if (FiscalPrinter.Document.FootNote != null)
            {
                currentLine += FiscalPrinter.Document.FootNote.Count; ;
            }

            if (document.CustomerChange > 0)
            {
                currentLine++;
            }

            currentLine = GetCurrentLine(currentLine);
            
            String cashierInfo = String.Empty;

            if (document.CustomerChange > 0)
            {
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), PosMessage.CHANGE);
                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                    String.Format("{0," + maxAmountWidth + "}", "*" + new Number(document.CustomerChange).ToString("C")));
            }
            FormatFooterNotes();

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Empty);

            String vat_message = PosMessage.VAT;

            
            decimal[,] taxRateTotals = FiscalPrinter.Document.TaxRateTotals;

            if (taxRateTotals.Length > 0)
            {
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), PosMessage.VAT_DISTRIBUTION);
                FormatLine(new Location(productVatLocation.X - vat_message.Length, CurrentPage.PageLines.Count), vat_message);
                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), String.Format("{0," + maxAmountWidth + "}", PosMessage.SALE));
            }
            for (int i = 0; i < taxRateTotals.GetLength(0); i++)
            {
                int taxRate = (int)(Math.Round(Department.TaxRates[(int)(taxRateTotals[i, 0])], 0));

                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), 
                    String.Format("{0} %{1}", PosMessage.SELLING_VAT, taxRate.ToString().PadLeft(2, '0')));

                FormatLine(new Location(productVatLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                    String.Format("{0," + maxAmountWidth + "}", String.Format("*{0:C}", new Number(taxRateTotals[i, 1]))));

                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                    String.Format("{0," + maxAmountWidth + "}", String.Format("*{0:C}", new Number(taxRateTotals[i, 2]))));
            }
            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Empty);

            try
            {
                ICashier cashier = FiscalPrinter.Cashier;
                if (CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON)
                    cashierInfo = cashier.Name.TrimEnd();

                cashierInfo = String.Format("{0} : {1} {2}", PosMessage.CASHIER, cashier.Id, cashierInfo);
            }
            catch { }

            if (!String.IsNullOrEmpty(cashierInfo))
            {
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), cashierInfo);
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), string.Format("{0} {1}: {2}",
                                                                                                        PosMessage.PAGE,
                                                                                                        PosMessage.NO,
                                                                                                        Id));
            }
                        
            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public List<String> FormatSubTotal(ISalesDocument document)
        {

            int currentLine = GetCurrentLine(1);
            
            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), PosMessage.SUBTOTAL);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                String.Format("{0," + maxAmountWidth + "}", "*" + new Number(document.TotalAmount).ToString("C")));
            
            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public List<String> FormatPageDeposit(Decimal amount)
        {
            int currentLine = CurrentPage.PageLines.Count;

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Empty);
            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), string.Format("{0}", PosMessage.SUBTOTAL));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(amount).ToString("C")));

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        internal List<String> FormatPageNo(bool subTotalPrinted)
        {
            totalPrinted = !subTotalPrinted;
            int currentLine = CurrentPage.PageLines.Count;
            if (!subTotalPrinted)
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Empty);
            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), string.Format("{0} {1}: {2}",
                                                                                                            PosMessage.PAGE,
                                                                                                            PosMessage.NO,
                                                                                                            invoicePages.Count));

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public String FormatRemark(String remark)
        {
            int currentLine = GetCurrentLine(InvoicePage.REMARKCOUNT);

            if (Str.Contains(remark, '\t'))
            {
                String[] pairs = Str.Split(remark, '\t');

                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), "## " + pairs[0]);
                FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), 
                                        pairs[1].PadLeft(maxAmountWidth, ' '));
            }
            else
            {
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), "## " + remark);
            }

            InvoicePage.REMARKCOUNT -= 1;
            return CurrentPage.PageLines[currentLine];
        }

        public List<String> FormatFooterNotes()
        {
            if (FiscalPrinter.Document.FootNote != null)
            {
                int currentLine = GetCurrentLine(FiscalPrinter.Document.FootNote.Count);

                if (FiscalPrinter.Document.FootNote.Count > 0)
                {
                    FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), "## " + String.Empty.PadLeft(32, '-') + " ##");

                    foreach (String line in FiscalPrinter.Document.FootNote)
                        FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), "## " + line.PadRight(32, ' ') + " ##");

                    FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), "## " + String.Empty.PadLeft(32, '-') + " ##");
                }
                return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
            }
            return CurrentPage.PageLines.GetRange(0, 0);
        }

        public List<String> FormatVoid(IAdjustment ai)
        {
            int currentLine = GetCurrentLine(2);

            if (ai.Target is ISalesDocument)
            {
                if (CurrentSettings.GetProgramOption(Setting.PrintSubtTotal) == PosConfiguration.OFF)
                {
                    FormatSubTotal(FiscalPrinter.Document);
                }
            }
            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), PosMessage.CORRECTION);
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(-1 * ai.NetAmount).ToString("C")));

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        public List<String> FormatVoid(IFiscalItem fi)
        {
            int currentLine = 0;
            if (CurrentSettings.GetProgramOption(Setting.PrintProductBarcode) == PosConfiguration.ON)
                currentLine = GetCurrentLine(4);
            else currentLine = GetCurrentLine(3);

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), PosMessage.VOID.PadLeft(PosMessage.VOID.Length + 4, ' '));

            if (fi.Quantity != -1)
            {
                string specifier = "Q";
                if (fi.Quantity % 1 < 0)
                    specifier = "0.000";
                FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Format("{0} {1} X {2}", new Number(fi.Quantity).ToString(specifier), fi.Unit, new Number(fi.UnitPrice).ToString("C")));
            }
            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Format("{0,-" + maxProductWidth + "}", fi.Name.Trim()));
            FormatLine(new Location(productVatLocation.X - 3, CurrentPage.PageLines.Count), String.Format("%{0:D2}", (int)(Department.TaxRates[fi.TaxGroupId - 1])));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), String.Format("{0," + maxAmountWidth + "}", "*" + new Number(fi.TotalAmount).ToString("C")));

            if (CurrentSettings.GetProgramOption(Setting.PrintProductBarcode) == PosConfiguration.ON)
                FormatRemark(fi.Barcode);

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }

        private static ISettings CurrentSettings
        {
            get { return Data.Connector.Instance().CurrentSettings; }
        }

        public void ClearInvoice()
        {
            InvoicePage.invoicePages.Clear();
            this.IsPageEnd = true;
            SubTotal = 0;
        }

        public List<String> FormatCorrection(IFiscalItem fi)
        {
            int currentLine = GetCurrentLine(2);

            FormatLine(new Location(productNameLocation.X, CurrentPage.PageLines.Count + 1), String.Format("{0,-" + maxProductWidth + "}", PosMessage.CORRECTION));
            FormatLine(new Location(productAmountLocation.X - maxAmountWidth, CurrentPage.PageLines.Count), String.Format("{0," + maxAmountWidth + "}", "*-" + new Number(fi.TotalAmount).ToString("C")));

            return CurrentPage.PageLines.GetRange(currentLine, CurrentPage.PageLines.Count - currentLine);
        }
    }
}
