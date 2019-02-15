using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;

namespace Hugin.POS.Printer.GUI
{
    public enum State
    {
        IDLE = 1,
        SELLING,
        SUBTOTAL,
        PAYMENT,
        OPEN_SALE,
        INFO_RCPT,
        CUSTOM_RCPT,
        IN_SERVICE,
        SRV_REQUIRED,
        LOGIN,
        NONFISCAL,
        ON_PWR_RCOVR,
        INVOICE,
        CONFIRM_REQUIRED
    }

    public enum PaymentType
    {
        CASH,
        CREDIT,
        CHECK,
        FCURRENCY
    }

    public class PaymentInfo
    {
        public PaymentType Type = 0;
        public int Index = 0;
        public decimal PaidTotal = 0.00m;
        public bool viaByEFT = false;
        public int SequenceNo;
        public string[] AdditionalInfo;
    }

    public class Adjustment
    {
        public AdjustmentType Type = AdjustmentType.Discount;
        public decimal Amount = 0m;
        public int percentage = 0;
        public string NoteLine1;
        public string NoteLine2;
    }

    public class FiscalPrinter:IFiscalPrinter
    {
        #region IFiscalPrinter Members

        public event EventHandler BeforeZReport;

        public event EventHandler AfterZReport;

        public event EventHandler DocumentRequested;

        public event EventHandler DateTimeChanged;

        public event OnMessageHandler OnMessage;

        private const PrinterProperties PRINTER_PROPERTIES = PrinterProperties.None;
        internal const int CHAR_PER_LINE = 40;
        internal const int CHAR_PER_BOLD_LINE = 17;
        private const int MAX_CASHIER_COUNT = 10;
        protected const int MAX_CREDIT_VX675 = 4;
        protected const int MAX_CREDIT = 8;
        protected const int MAX_FCURRENCY = 4;

        static string registerId = "";
        static IFiscalPrinter printer = null;
        static ISalesDocument salesDocument = null;
        static ICashier cashier = null;

        bool programMode = false;
        static long zlimit = 0;

        int graphicLogoActive = 0;
        int receiptBarcodeActive = 0;
        int autoCutterActive = 0;
        decimal receiptLimit = 0;
        string[] logo = new string[6];
        string[] endOfReceiptLines = new string[6];
        Department[] departments = new Department[Department.NUM_TAXGROUPS];
        decimal[] taxRates = new decimal[Department.NUM_TAXGROUPS];

        String strLogoPath = "";
        decimal cashAmountInDrawer = 0;
        decimal totalChange = 0;

        int lastz = 0;
        int LastZDocID = 0;
        DateTime lastZReportDate = DateTime.MinValue;
        int documentSold = 0;
        int documentVoided = 0;
        int documentSuspended = 0;
        decimal totalSold = 0;
        decimal totalVoided = 0;
        decimal totalSuspended = 0;
        decimal foreignCurrencyPayment = 0;
        decimal cashPayment = 0;
        decimal checkPayment = 0;

        GuiPrinterForm guiDocument = null;
        int currentDocumentId = 0;

        private PrinterResponse toResponse;
        private const string currentLog = "CurrentDocument.gui";
        private const string lastDocumentLog = "LastDocument.gui";
        private const string lastZLog = "LastZ.gui";

        private static ISalesDocument document = null;

        List<String> summary = new List<string>();

        private FiscalPrinter()
        {
            RegisterId = PosConfiguration.Get("RegisterId");

            guiDocument = new GuiPrinterForm();
            Formatter.SetCoordinates();
            toResponse = new PrinterResponse();

            lastz = LastZReportNo;
            lastZReportDate = LastZReportDate;
            currentDocumentId = CurrentDocumentId;
        }
        public static IFiscalPrinter Printer
        {
            get
            {
                if (printer == null)
                {
                    printer = new FiscalPrinter();
                }
                return printer;
            }
        }


        public bool HasProperties(PrinterProperties properties)
        {
            return (properties & PRINTER_PROPERTIES) == properties;
        }

        internal static IDataConnector DataConnector
        {
            get
            {
                return Data.Connector.Instance();
            }
        }

        internal static string RegisterId
        {
            get { return registerId; }
            set { registerId = value; }
        }
        internal static ISalesDocument Document
        {
            get { return salesDocument; }
            set { salesDocument = value; }
        }
        internal static ICashier Cashier
        {
            get { return cashier; }
            set { cashier = value; }
        }

        public IPrinterResponse Feed()
        {
            PrinterResponse response = null;
            return response;
        }
        public IPrinterResponse CutPaper()
        {
            PrinterResponse response = null;
            guiDocument.AddLine(Formatter.FormatSlipLine("_".PadRight(40, '_')));
            return response;
        }

        public IPrinterResponse EnterProgramMode()
        {
            programMode = true;
            return toResponse;
        }

        public IPrinterResponse ExitProgramMode()
        {
            programMode = false;
            return toResponse;
        }

        public IPrinterResponse LoadGraphicLogo(string filePath)
        {
            strLogoPath = filePath;
            //to do put image when printing ticket
            return toResponse;
        }
        public bool HasSameLogo(String [] userLogo)
        {
            string[] logo = Logo;
            for (int logoCounter = 0; logoCounter < 6; logoCounter++)
            {
                if (!logo[logoCounter].Equals(userLogo[logoCounter]))
                    return false;
            }
            return true;
        }

        public bool IsVx675
        {
            get
            {
                return false;
            }
        }

        public Department[] Departments
        {
            get
            {
                if(File.Exists("Department.gui"))
                {
                    string[] lines = IOUtil.ReadAllLines("Department.gui");

                    int counter = 0;
                    string name = "";
                    string taxGrpId = "";
                    int id = 1;
                    foreach(string line in lines)
                    {
                        counter++;
                        if(counter%2 == 0)
                        {
                            name = line;
                            Department dep = new Department(id, name);
                            dep.TaxGroupId = Convert.ToInt32(taxGrpId);
                            departments[id - 1] = dep;
                            id++;
                        }
                        else
                        {
                            taxGrpId = line;
                        }
                    }
                }

                return departments;
            }
            set
            {
                int index = 0;
                List<string> lines = new List<string>();
                foreach(Department dep in value)
                {
                    departments[index] = dep;
                    index++;

                    if (dep != null)
                    {
                        lines.Add((dep.TaxGroupId + 1).ToString());
                        lines.Add(dep.Name);
                    }
                }

                IOUtil.WriteAllLines("Department.gui", lines.ToArray());
            }
        }

        public string[] Logo
        {
            get
            {
                if (File.Exists("Logo.gui"))
                    logo = IOUtil.ReadAllLines("Logo.gui");
                else
                    logo = new string[] { "", "", "", "", "", "" };
                return logo;
            }
            set
            {
                logo = value;
                IOUtil.WriteAllLines("Logo.gui", logo);
            }
        }

        public string[] EndOfReceiptNote
        {
            get
            {
                if (File.Exists("EndOfReceipt.gui"))
                    endOfReceiptLines = IOUtil.ReadAllLines("EndOfReceipt.gui");
                else
                    endOfReceiptLines = new string[] { "", "", "", "", "", "" };
                return endOfReceiptLines;
            }
            set
            {
                List<string> tmp = new List<string>();
                foreach (string s in value)
                    tmp.Add(Str.FixTurkishUpperCase(s));
                IOUtil.WriteAllLines("EndOfReceipt.gui", tmp.ToArray());
            }
        }

        List<Category> categories = null;
        public Category[] Category
        {
            get
            {
                if (categories == null)
                    return new List<Category>().ToArray();
                else
                    return categories.ToArray();
            }
            set
            {
                categories = new List<Common.Category>(value);
            }
        }

        ICredit[] credits = null;
        public ICredit[] Credits
        {
            get
            {
                if (credits == null)
                {
                    int maxCredit = MAX_CREDIT;
                    if (IsVx675)
                        maxCredit = MAX_CREDIT_VX675;

                    credits = new ICredit[maxCredit];

                    Dictionary<int, ICredit> creditsDic = DataConnector.GetCredits();
                    int kp = 0;
                    foreach (KeyValuePair<int, ICredit> kpv in creditsDic)
                    {
                        credits[kp] = kpv.Value;
                        kp++;
                    }
                }
                return credits;
            }
            set
            {
                SendCreditInfo(value);
            }
        }

        ICurrency[] currencies = null;
        public ICurrency[] Currencies
        {
            get
            {
                if(currencies == null)
                {
                    currencies = new ICurrency[MAX_FCURRENCY];

                    Dictionary<int, ICurrency> currencyDic = DataConnector.GetCurrencies();
                    int kp = 0;
                    foreach(KeyValuePair<int, ICurrency> kvp in currencyDic)
                    {
                        currencies[kp] = kvp.Value;
                        kp++;
                    }
                }
                return currencies;
            }
            set
            {
                SendCurrencyInfo(value);
            }
        }

        public int AutoCutter
        {
            get
            {
                if (File.Exists("AutoCutter.gui"))
                    autoCutterActive = Convert.ToInt32(IOUtil.ReadAllText("AutoCutter.gui"));
                return autoCutterActive;
            }
            set
            {
                autoCutterActive = value;
                IOUtil.WriteAllText("AutoCutter.gui", autoCutterActive.ToString());
            }
        }

        public int GraphicLogoActive
        {
            get
            {
                if (File.Exists("GraphicLogo.gui"))
                    graphicLogoActive = Convert.ToInt32(IOUtil.ReadAllText("GraphicLogo.gui"));
                return graphicLogoActive;
            }
            set
            {
                graphicLogoActive = value;
                IOUtil.WriteAllText("GraphicLogo.gui", graphicLogoActive.ToString());
            }
        }

        public int ReceiptBarcodeActive
        {
            get
            {
                if (File.Exists("ReceiptBarcodeActive.gui"))
                    receiptBarcodeActive = Convert.ToInt32(IOUtil.ReadAllText("ReceiptBarcodeActive.gui"));
                return receiptBarcodeActive;
            }
            set
            {
                receiptBarcodeActive = value;
                IOUtil.WriteAllText("ReceiptBarcodeActive.gui", receiptBarcodeActive.ToString());
            }
        }

        public decimal ReceiptLimit
        {
            get
            {
                if (File.Exists("ReceiptLimit.gui"))
                    receiptLimit = Convert.ToDecimal(IOUtil.ReadAllText("ReceiptLimit.gui"));
                return receiptLimit;
            }
            set
            {
                receiptLimit = value;
                IOUtil.WriteAllText("ReceiptLimit.gui", receiptLimit.ToString());
            }
        }

        public DateTime Time
        {
            get
            {
                return DateTime;
            }
            set
            {
                if (CurrentDocumentId > 0)
                    throw new LimitExceededOrZRequiredException();

                DateTime fpuTime = this.DateTime;
                TimeSpan newTimeDelta = fpuTime - value;
                if (Math.Abs(newTimeDelta.TotalHours) > 1)
                    throw new TimeLimitException();
                if (value < lastZReportDate)
                    throw new TimeZReportException();

                DateTime = value;
            }
        }

        public DateTime DateTime
        {
            get
            {
                return DateTime.Now;
            }
            set
            {
                SetSystemTime(value);
            }
        }

        public decimal CashAmountInDrawer
        {
            get { return DataConnector.GetRegisterCash(RegisterId); }
        }

        public bool CurrencyPaymentContains() { return false; }

        public decimal TotalChange
        {
            get { return totalChange; }
        }

        public ISalesDocument SaleDocument
        {
            get { return document; }
            set
            {
                document = value;
            }
        }

        public IPrinterResponse Withdraw(Decimal amount)
        {
            if (CashAmountInDrawer < amount) throw new NegativeResultException();
            if (amount > 0)
                amount = -1 * amount;

            guiDocument.AddLines(new List<string>(Logo));
            StartCurrentLog(101);
            guiDocument.AddLines(Formatter.FormatReceiptHeader(PosMessage.RECEIPT_TR, currentDocumentId));
            guiDocument.AddLines(Formatter.FormatRegisterDocument(101, amount));
            cashPayment += amount;
            MoveCurrentDocument(101);
            
            return toResponse;
        }

        public IPrinterResponse Withdraw(Decimal amount, String refNumber)
        {
            checkPayment += amount;
            return toResponse;
        }

        public IPrinterResponse Withdraw(Decimal amount, ICredit credit)
        {
            foreignCurrencyPayment += amount;
            return toResponse;
        }

        public IPrinterResponse Deposit(Decimal amount)
        {
            //if (cashAmountInDrawer < amount) throw new NegativeResultException();
            guiDocument.AddLines(new List<string>(Logo));
            StartCurrentLog(100);
            guiDocument.AddLines(Formatter.FormatReceiptHeader(PosMessage.RECEIPT_TR, currentDocumentId));
            guiDocument.AddLines(Formatter.FormatRegisterDocument(100, amount));
            cashPayment += amount;
            MoveCurrentDocument(100);
            
            if (DataConnector.CurrentSettings.GetProgramOption(Setting.OpenDrawerOnPayment) == PosConfiguration.ON)
                OpenDrawer();

            return toResponse;
        }

        public bool IsFiscal
        {
            get { return true;  }
        }

        public IPrinterResponse Print(IAdjustment ai)
        {
            //guiDocument.AddLines(Formatter.Format(ai));
            //if (ai.Method == AdjustmentType.Discount || ai.Method == AdjustmentType.PercentDiscount)
            //    WriteCurrentLog("Discount=" + Math.Abs(ai.NetAmount));
            //else
            //    WriteCurrentLog("Fee=" + ai.NetAmount);

            //return toResponse;         
            return null;
        }

        public void ClearDisplay()
        {
        }
        public IPrinterResponse Correct(IAdjustment ai)
        {
            //return Void(ai);
            return null;
        }

        public IPrinterResponse Correct(IFiscalItem fi)
        {
            //guiDocument.AddLines(Formatter.FormatVoid(fi));
            //WriteCurrentLog("Item=" + fi.TotalAmount);
            //return toResponse;
            return null;
        }

        public IPrinterResponse Print(IAdjustment[] ai)
        {
            //guiDocument.AddLines(Formatter.Format(ai));
            //foreach (IAdjustment adj in ai)
            //{
            //    if (adj.Method == AdjustmentType.Discount || adj.Method == AdjustmentType.PercentDiscount)
            //        WriteCurrentLog("Discount=" + Math.Abs(adj.NetAmount));
            //    else
            //        WriteCurrentLog("Fee=" + adj.NetAmount);
            //}
            //return toResponse;  
            return null;
        }

        public IPrinterResponse Print(IFiscalItem fi)
        {
            //guiDocument.AddLines(Formatter.Format(fi));
            //WriteCurrentLog("Item=" + fi.TotalAmount);
            //return toResponse;  
            return null;
        }

        public IPrinterResponse Pay(Decimal amount)
        {
            //guiDocument.AddLines(Formatter.FormatPayment(amount, PosMessage.CASH));

            //WriteCurrentLog("Cash=" + amount);
            //return toResponse;
            return null;
        }
        public IPrinterResponse Pay(Decimal amount,String refNumber)
        {
            //guiDocument.AddLines(Formatter.FormatPayment(amount, PosMessage.CHECK));

            //WriteCurrentLog("Check=" + amount);
            //return toResponse;
            return null;
        }
        public IPrinterResponse Pay(Decimal amount, ICurrency currency)
        {
            //String label = String.Empty;

            //Number currencyPayment = new Number(amount / currency.ExchangeRate);

            //if (currencyPayment.ToDecimal() > currencyPayment.ToInt())
            //    label = String.Format("{0} {1:C}", currency.Name, currencyPayment);

            //label = String.Format("{0} {1}", currency.Name, currencyPayment);
            //guiDocument.AddLines(Formatter.FormatPayment(amount, label));

            //WriteCurrentLog("Currency=" + amount);
            //return toResponse;
            return null;
        }
        public IPrinterResponse Pay(Decimal amount, ICredit credit, int installments)
        {
            //String label = credit.Name + (installments == 0 ? String.Empty : "/" + installments.ToString());

            //guiDocument.AddLines(Formatter.FormatPayment(amount, label));

            //WriteCurrentLog("Credit=" + amount);
            //return toResponse;
            return null;
        }

        public IPrinterResponse PrintHeader(ISalesDocument document)
        {
            StartCurrentLog(document.DocumentTypeId);

            document.Id = currentDocumentId;
            salesDocument = document;

            guiDocument.AddLine("");
            guiDocument.AddLines(Logo);

            if (document.DocumentTypeId < 0)
                guiDocument.AddLines(Formatter.FormatReceiptHeader(document.Name, document.Id));
            else
            {
                //guiDocument.AddLines(Formatter.FormatHeader(salesDocument));

                switch (document.DocumentTypeId)
                {
                    case 1:
                    case 2:
                    case 3:
                        // INVOICE, E-INVOICE, E-ARCHIVE
                        String tcknVkn = "";
                        String serial = "";
                        if (document.DocumentTypeId == 1)
                        {
                            string slipSerial = document.SlipSerialNo.Length > 2 ? document.SlipSerialNo.Substring(0, 2) : document.SlipSerialNo;
                            string slipOrder = document.SlipOrderNo.Length > 6 ? document.SlipOrderNo.Substring(0, 6) : document.SlipOrderNo;

                            serial = slipSerial + slipOrder;
                        }

                        if (!String.IsNullOrEmpty(document.TcknVkn))
                            tcknVkn = document.TcknVkn.Trim();
                        else if (document.Customer != null)
                            tcknVkn = document.Customer.Contact[4].Trim();
                        else
                            throw new ParameterException("TCKN/VKN EKSÝK");

                        PrintDocumentHeader(document.DocumentTypeId, tcknVkn, serial, document.IssueDate);
                        break;
                    case 4:
                        // FOOD DOCUMENT
                        PrintFoodDocumentHeader();

                        break;
                    case 5:
                        // CAR PARKING
                        string plate = "";
                        DateTime parkingDT;

                        plate = document.CustomerTitle;
                        parkingDT = document.IssueDate;

                        PrintParkDocument(plate, parkingDT);

                        break;
                    case 6:
                        // ADVANCE
                        string title = "";
                        String tcknVkn2 = "";

                        if (document.Customer != null)
                            title = document.Customer.Name;
                        else
                            title = document.CustomerTitle;

                        if (!String.IsNullOrEmpty(document.TcknVkn))
                            tcknVkn2 = document.TcknVkn.Trim();
                        else if (document.Customer != null)
                            tcknVkn2 = document.Customer.Contact[4].Trim();
                        else
                            throw new ParameterException("TCKN/VKN EKSÝK");

                        PrintAdvanceDocumentHeader(tcknVkn2, title, document.TotalAmount);

                        break;
                    case 7:
                        // COLLECTION INVOICE

                        String cllctionSerial = "";
                        cllctionSerial = document.SlipSerialNo.Substring(0, 2) + document.SlipOrderNo.Substring(0, 6);

                        string instutionNAme = document.ReturnReason;

                        string subscriberNo = document.CustomerTitle;

                        decimal comission = document.ComissionAmount;

                        decimal amount = document.TotalAmount - comission;

                        DateTime dt = document.IssueDate;

                        PrintCollectionDocumentHeader(cllctionSerial, dt, amount, subscriberNo, instutionNAme, comission);

                        break;
                    case 9:
                        // CURRENT ACCOUNT DOCUMENT

                        string docSerial = document.SlipSerialNo + document.SlipOrderNo;

                        string custName = "";
                        if (document.Customer != null)
                            custName = document.Customer.Name;
                        else
                            custName = document.CustomerTitle;

                        string tcknVkn3 = "";
                        if (!String.IsNullOrEmpty(document.TcknVkn))
                            tcknVkn3 = document.TcknVkn.Trim();
                        else if (document.Customer != null)
                            tcknVkn3 = document.Customer.Contact[4].Trim();
                        else
                            throw new ParameterException("TCKN/VKN EKSÝK");

                        DateTime dt2 = document.IssueDate;

                        PrintCurrentAccountCollectionDocumentHeader(tcknVkn3, custName, docSerial, dt2, document.TotalAmount);

                        break;
                }
            }

            return toResponse;
        }

        private void PrintCurrentAccountCollectionDocumentHeader(string tcknVkn, string custName, string docSerial, DateTime dt, decimal totalAmount)
        {
            guiDocument.AddLines(Formatter.FormatCurrentAccountCollectionHeader(tcknVkn, custName, docSerial, dt, totalAmount));
        }

        private void PrintCollectionDocumentHeader(string cllctionSerial, DateTime dt, decimal amount, string subscriberNo, string instutionNAme, decimal comission)
        {
            guiDocument.AddLines(Formatter.FormatCollectionDocumentHeader(cllctionSerial, dt, amount, subscriberNo, instutionNAme, comission));
        }

        private void PrintAdvanceDocumentHeader(string tcknVkn, string title, decimal totalAmount)
        {
            guiDocument.AddLines(Formatter.FormatAdvanceDocumentHeader(tcknVkn, title, totalAmount));
        }

        private void PrintParkDocument(string plate, DateTime parkingDT)
        {
            guiDocument.AddLines(Formatter.FormatParkDocumentHeader(plate, parkingDT));
        }

        private void PrintFoodDocumentHeader()
        {
            guiDocument.AddLines(Formatter.FormatFoodDocumentHeader());
        }

        private void PrintDocumentHeader(int documentTypeId, string tcknVkn, string serial, DateTime issueDate)
        {
            guiDocument.AddLines(Formatter.FormatInvoiceHeader(documentTypeId, tcknVkn, serial, issueDate));
        }

        public IPrinterResponse PrintTotals(ISalesDocument document, bool hardcopy)
        {

            //PrinterResponse response = new PrinterResponse();
            //int type = GetDocumentType();
            //if (type > 100)
            //    throw new NoDocumentFoundException();
            //if (type != document.DocumentTypeId)
            //    throw new DocumentTypeException();

            //decimal total = CalculateTotal();

            //if (document.TotalAmount != total)
            //    throw new SubtotalNotMatchException(Math.Abs(total - document.TotalAmount));
            //response.Data = "" + total;

            //salesDocument = document;
            //guiDocument.AddLines(Formatter.FormatTotals(document, hardcopy));
            //WriteCurrentLog("Total=" + document.TotalAmount);
            //return response;

            return null;
        }


        public IPrinterResponse PrintFooter(ISalesDocument document, bool isReceipt)
        {
            //salesDocument = document;
            //guiDocument.AddLines(Formatter.FormatFooter(document));
            //documentSold++;
            //totalSold += document.TotalAmount;
            //MoveCurrentDocument(0);
            //return toResponse;  

            try
            {
                Document = document;

                /*
                 * PRINT HEADER
                 */
                if (PrinterState == State.IDLE)
                {
                    try
                    {
                        PrintHeader(document);

                        // Avans ve Fatura tahsilatý ise ödeme türüne bak
                        if (document.DocumentTypeId == 6 || document.DocumentTypeId == 7)
                        {
                            List<PaymentInfo> payments = FiscalPrinter.GetPayments(Document);

                            //Ödeme yok ise ayrýl
                            if (payments.Count == 0)
                                return toResponse;
                        }
                    }
                    catch (PrinterException pe)
                    {
                        if (OnMessage != null)
                            OnMessage(this, new OnMessageEventArgs(pe));
                    }
                }

                if (PrinterState == State.SELLING || PrinterState == State.INVOICE || PrinterState == State.PAYMENT)
                {
                    if (CalculateTotalPaid() == 0 && document.DocumentTypeId != 6 && document.DocumentTypeId != 7 && 
                        document.DocumentTypeId != 9 && document.DocumentTypeId != 5)
                    {
                        //PRINT FISCAL ITEMS
                        List<IFiscalItem> soldItems = new List<IFiscalItem>();

                        soldItems = Document.Items.FindAll(delegate (IFiscalItem fi)
                        {
                            return fi.Quantity > fi.VoidQuantity;
                        });

                        foreach (IFiscalItem fi in soldItems)
                        {
                            try
                            {
                                guiDocument.AddLines(Formatter.Format(fi));
                                WriteCurrentLog("Item=" + fi.ListedAmount);

                                Adjustment adj = new Adjustment();

                                decimal totalAdjAmount = 0;
                                foreach (string adjOnItem in fi.GetAdjustments())
                                {
                                    string[] values = adjOnItem.Split('|');
                                    totalAdjAmount += decimal.Parse(values[0]);
                                }

                                if (totalAdjAmount != 0)
                                {
                                    // Ürüne tek indirim/arttýrým uygulanabiliyorsa;
                                    string[] adjustmentsOnItem = fi.GetAdjustments();
                                    adj = ParseAdjLine(adjustmentsOnItem[adjustmentsOnItem.Length - 1]); // last adj after corrections                       
                                }

                                if (adj != null && adj.Amount != 0)
                                {
                                    //response = PrintAdjustment(adj);
                                    guiDocument.AddLines(Formatter.Format(adj));
                                    if (adj.Type == AdjustmentType.Discount || adj.Type == AdjustmentType.PercentDiscount)
                                        WriteCurrentLog("Discount=" + Math.Abs(adj.Amount));
                                    else
                                        WriteCurrentLog("Fee=" + adj.Amount);
                                }
                            }
                            catch (PrinterException pe)
                            {
                                if (OnMessage != null)
                                    OnMessage(this, new OnMessageEventArgs(pe));
                            }
                        }

                        // PRINT SUBTOTAL                        
                        salesDocument = document;            
                        guiDocument.AddLines(Formatter.FormatSubTotal(document, true));
                        WriteCurrentLog("SubTotal=" + document.TotalAmount);

                        List<string> docAdjustments = new List<string>(Document.GetAdjustments());
                        if (docAdjustments.Count > 0)
                        {
                            decimal totalDocAdj = 0;
                            foreach (string adjOnDoc in docAdjustments)
                            {
                                string[] values = adjOnDoc.Split('|');
                                totalDocAdj += decimal.Parse(values[0]);
                            }

                            Adjustment subTotalAdj = new Adjustment();
                            if (totalDocAdj != 0)
                            {
                                string[] adjustmentsOnDoc = Document.GetAdjustments();
                                subTotalAdj = ParseAdjLine(adjustmentsOnDoc[adjustmentsOnDoc.Length - 1]); // last adj after corrections                       
                            }

                            if (subTotalAdj != null && subTotalAdj.Amount != 0)
                            {
                                guiDocument.AddLines(Formatter.Format(subTotalAdj));
                                if (subTotalAdj.Type == AdjustmentType.Discount || subTotalAdj.Type == AdjustmentType.PercentDiscount)
                                    WriteCurrentLog("Discount=" + Math.Abs(subTotalAdj.Amount));
                                else
                                    WriteCurrentLog("Fee=" + subTotalAdj.Amount);
                            }
                        }

                        // PRINT TOTALS
                        if (document.TotalAmount != decimal.Zero)
                        {
                            int type = GetDocumentType();
                            if (type > 100)
                                throw new NoDocumentFoundException();
                            if (type != document.DocumentTypeId)
                                throw new DocumentTypeException();

                            decimal total = CalculateTotal();

                            if (document.TotalAmount != total)
                                throw new SubtotalNotMatchException(Math.Abs(total - document.TotalAmount));

                            salesDocument = document;
                            guiDocument.AddLines(Formatter.FormatTotals(document, true));
                            WriteCurrentLog("Total=" + document.TotalAmount);
                        }
                    }

                    if (Document.DocumentTypeId == (int)DocumentTypes.ADVANCE ||
                           Document.DocumentTypeId == (int)DocumentTypes.COLLECTION_INVOICE ||
                           Document.DocumentTypeId == (int)DocumentTypes.CURRENT_ACCOUNT_COLLECTION)
                    {
                        // SUBTOTAL
                        salesDocument = document;
                        guiDocument.AddLines(Formatter.FormatSubTotal(document, true));
                    }

                    if(Document.DocumentTypeId == (int)DocumentTypes.COLLECTION_INVOICE)
                    {
                        if (document.TotalAmount != decimal.Zero)
                        {
                            // TOTALS
                            int type = GetDocumentType();
                            if (type > 100)
                                throw new NoDocumentFoundException();
                            if (type != document.DocumentTypeId)
                                throw new DocumentTypeException();     

                            salesDocument = document;
                            guiDocument.AddLines(Formatter.FormatTotals(document.ComissionAmount, document.TotalVAT));
                            WriteCurrentLog("Total=" + document.TotalAmount);
                        }
                    }

                    // PRINT PAYMENTS
                    {
                        List<PaymentInfo> payments = FiscalPrinter.GetPayments(Document);
                        Decimal paidTotal = 0.00m;

                        bool add = false;
                        foreach (PaymentInfo pi in payments)
                        {
                            try
                            {
                                if (paidTotal == CalculateTotalPaid())
                                    add = true;
                                if (add)
                                {
                                    switch(pi.Type)
                                    {
                                        case PaymentType.CASH:
                                            guiDocument.AddLines(Formatter.FormatPayment(pi.PaidTotal, PosMessage.CASH));

                                            WriteCurrentLog("Cash=" + pi.PaidTotal);
                                            break;
                                        case PaymentType.CHECK:
                                            guiDocument.AddLines(Formatter.FormatPayment(pi.PaidTotal, PosMessage.CHECK));

                                            WriteCurrentLog("Check=" + pi.PaidTotal);
                                            break;
                                        case PaymentType.CREDIT:
                                            String label = "KREDI " + pi.Index + (pi.SequenceNo == 0 ? String.Empty : "/" + pi.SequenceNo.ToString());

                                            guiDocument.AddLines(Formatter.FormatPayment(pi.PaidTotal, label));

                                            WriteCurrentLog("Credit=" + pi.PaidTotal);
                                            break;
                                        case PaymentType.FCURRENCY:
                                            String label2 = String.Empty;

                                            Number currencyPayment = new Number(pi.PaidTotal);
                                            Number totalPaid = new Number(pi.PaidTotal * Decimal.Parse(pi.AdditionalInfo[1]));

                                            if (currencyPayment.ToDecimal() > currencyPayment.ToInt())
                                                label2 = String.Format("{0} {1:C}", pi.AdditionalInfo[0], currencyPayment);

                                            label2 = String.Format("{0} {1}", pi.AdditionalInfo[0], currencyPayment);
                                            guiDocument.AddLines(Formatter.FormatPayment(totalPaid.ToDecimal(), label2));

                                            WriteCurrentLog("Currency=" + totalPaid);
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                if (!add)
                                    paidTotal += pi.PaidTotal;


                            }
                            catch (NegativeResultException)
                            {
                                throw new NegativeResultException();
                            }
                            catch (PrinterException pe)
                            {
                                if (OnMessage != null)
                                    OnMessage(this, new OnMessageEventArgs(pe));
                            }
                        }
                    }

                }
                // PRINT FOOTER NOTES
                guiDocument.AddLines(Formatter.FormatFooterNotes());

                // RECEIPT BARCODE
                if(FiscalPrinter.Printer.ReceiptBarcodeActive == 1)
                {
                    guiDocument.AddLines(Formatter.FormatBarcode());
                }

                // CLOSE 
                if (PrinterState == State.OPEN_SALE)
                {
                    guiDocument.AddLines(Formatter.FormatEnd());
                    MoveCurrentDocument(200 + Document.DocumentTypeId);
                    salesDocument = null;
                }

                return toResponse;
            }
            catch (NegativeResultException)
            {
                throw new NegativeResultException();
            }
            catch (InvalidPLUNoException pe)
            {
                throw pe;
            }
            catch (Exception ex)
            {
                throw new PrintDocumentException(PosMessage.DOCUMENT_NOT_PRINTED, ex);
            }
        }

        public IPrinterResponse PrintSubTotal(ISalesDocument document, bool hardcopy)
        {
            //salesDocument = document;            
            //guiDocument.AddLines(Formatter.FormatSubTotal(document, hardcopy));
            //return toResponse;  
            return null;
        }

        public IPrinterResponse PrintRemark(string s)
        {
            //guiDocument.AddLine(Formatter.FormatRemark(s));
            //return toResponse;  
            return null;
        }

        public IPrinterResponse PrintFooterNotes()
        {
            //guiDocument.AddLines(Formatter.FormatFooterNotes());
            //return toResponse;  
            return null;
        }

        public IPrinterResponse SignInCashier(ICashier ch)
        {
            string id = ch.Id;
            if (File.Exists("Cashier.gui"))
            {
                id = IOUtil.ReadAllText("Cashier.gui").Trim();
                cashier = DataConnector.FindCashierById(id);
                if (id == ch.Id)
                {
                    cashier = ch;
                    return toResponse;
                }
                else if (id != "")
                    throw new CashierAlreadyAssignedException("already assigned", id);
            }

            StartCurrentLog(2000);
            IOUtil.WriteAllText("Cashier.gui", ch.Id);
            guiDocument.AddLines(Logo);
            guiDocument.AddLines(Formatter.FormatReceiptHeader("FÝÞ", currentDocumentId));
            guiDocument.AddLines(Formatter.FormatInfo(String.Format("KASÝYER : {0} {1}", ch.Id, ch.Name).PadRight(40)));
            guiDocument.AddLines(Formatter.FormatInfo("GÝRÝÞ".PadRight(40)));
            guiDocument.AddLine("");
            guiDocument.AddLines(Formatter.FormatEnd());
            cashier = ch;
            return toResponse;
        }

        public ICashier SignInCashier(int cashierID, string pwd)
        {
            string id = cashierID.ToString();
            if (File.Exists("Cashier.gui"))
            {
                id = IOUtil.ReadAllText("Cashier.gui").Trim();
                cashier = DataConnector.FindCashierById(id.PadLeft(4, '0'));
                if (id == cashierID.ToString() ||id == cashierID.ToString().PadLeft(4, '0'))
                {
                    return cashier;
                }
                //else if (id != "")
                //    throw new CashierAlreadyAssignedException("already assigned", id);
            }

            cashier = DataConnector.FindCashierById(cashierID.ToString().PadLeft(4, '0'));
            if (cashier == null)
                return cashier;

            StartCurrentLog(2000);
            IOUtil.WriteAllText("Cashier.gui", cashier.Id);
            guiDocument.AddLines(Logo);
            guiDocument.AddLines(Formatter.FormatReceiptHeader("FÝÞ", currentDocumentId));
            guiDocument.AddLines(Formatter.FormatInfo(String.Format("KASÝYER : {0} {1}", cashier.Id, cashier.Name).PadRight(40)));
            guiDocument.AddLines(Formatter.FormatInfo("GÝRÝÞ".PadRight(40)));
            guiDocument.AddLine("");
            guiDocument.AddLines(Formatter.FormatEnd());
            return cashier;
        }

        public List<String> GetCashiers()
        {
            List<String> cashierList = new List<String>();

            for (int i = 0; i < MAX_CASHIER_COUNT; i++)
            {
                try
                {
                    ICashier ch = DataConnector.FindCashierById(i.ToString());

                    if (ch != null && !String.IsNullOrEmpty(ch.Name))
                        cashierList.Add(ch.Name);
                    else
                        cashierList.Add("");
                }
                catch (System.Exception)
                {

                }
            }

            return cashierList;
        }

        public IPrinterResponse SignOutCashier()
        {
            StartCurrentLog(3000);
            guiDocument.AddLines(Logo);
            guiDocument.AddLines(Formatter.FormatReceiptHeader("FÝÞ", currentDocumentId));
            if(cashier!=null)
            guiDocument.AddLines(Formatter.FormatInfo(String.Format("KASÝYER : {0} {1}", cashier.Id, cashier.Name).PadRight(40)));
            guiDocument.AddLines(Formatter.FormatInfo("ÇIKIÞ".PadRight(40)));
            guiDocument.AddLine("");
            guiDocument.AddLines(Formatter.FormatEnd());
            cashier = null;
            IOUtil.WriteAllText("Cashier.gui", "");
            return toResponse;  
        }

        public IPrinterResponse Void()
        {
            if (salesDocument == null && !File.Exists(currentLog))
                throw new NoDocumentFoundException();
            documentVoided++;
            totalVoided += PrinterSubTotal;
            if (File.Exists(currentLog) && salesDocument == null)
            {
                guiDocument.AddLines(Formatter.FormatInfo("ELEKTRÝK KESÝNTÝSÝ"));
                guiDocument.AddLines(Formatter.FormatInfo("BELGE IPTAL"));
            }
            else
                guiDocument.AddLines(Formatter.FormatVoid(salesDocument));
            guiDocument.AddLines(Formatter.FormatEnd());
            MoveCurrentDocument(1);
            salesDocument = null;

            return toResponse;
        }

        public IPrinterResponse Suspend()
        {
            if (salesDocument == null && !File.Exists(currentLog))
                throw new NoDocumentFoundException();
            documentSuspended++;
            totalSuspended += PrinterSubTotal;
            guiDocument.AddLines(Formatter.FormatVoid(salesDocument));
            guiDocument.AddLines(Formatter.FormatEnd());
            MoveCurrentDocument(2);
            salesDocument = null;
            return toResponse;
        }

        public IPrinterResponse Void(IAdjustment ai)
        {
            if (ai != null)
            {
                guiDocument.AddLines(Formatter.FormatVoid(ai));
                if (ai.Method == AdjustmentType.Discount || ai.Method == AdjustmentType.PercentDiscount)
                    WriteCurrentLog("VoidDiscount=" + Math.Abs(ai.NetAmount));
                else
                    WriteCurrentLog("VoidFee=" + ai.NetAmount);
            }
            return toResponse;
        }
        public IPrinterResponse Void(IFiscalItem fi)
        {
            guiDocument.AddLines(Formatter.FormatVoid(fi));
            WriteCurrentLog("Item=" + fi.TotalAmount);
            return toResponse;
        }

        public decimal PrinterSubTotal
        {
            get {
                return CalculateTotal();
            }
        }

        public IPrinterResponse PrintRegisterReport(bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintProgramReport(bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintXPluReport(int firstPLU, int lastPLU, bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEndDayReport()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintXReport(bool hardcopy)
        {
            PrinterResponse response = new PrinterResponse();

            StartCurrentLog(5000);

            List<String> content = GetSpecialReport(PosMessage.X_REPORT);
            if (hardcopy)
                guiDocument.AddLines(content);
            response.Data = currentDocumentId.ToString("0000");

            foreach (string line in content)
                response.Detail += line + "\r\n";

            return response;
        }

        private List<string> GetSpecialReport(string header)
        {
            List<string> content = new List<string>();
            content.Add("");
            content.AddRange(Logo);
            content.Add("");
            content.AddRange(Formatter.FormatReceiptHeader(PosMessage.RECEIPT_TR, currentDocumentId));

            content.Add(SaleReport.SurroundTitle(header));
            content.AddRange(SaleReport.GetSaleReport());
            content.Add("");
            content.AddRange(Formatter.FormatEnd());
            content.Add("");

            return content;
        }


        public IPrinterResponse PrepareDailyReport()
        {
            summary = new List<string>();
            summary.Add("".PadLeft(SpecialReport.MaxCharsAtLine, ' '));
            summary.Add(String.Format("{0,-" + SpecialReport.MaxCharsAtLine + "}", "TOPLAM"));
            summary.Add(String.Format("{0,6}{1,34}", documentSold, String.Format("*{0:f}", totalSold)));
            summary.Add("".PadLeft(SpecialReport.MaxCharsAtLine, ' '));
            /**/
            SpecialReport report = new SpecialReport();
            if (report.Items[0].Trim().Length == 0)
            {
                summary.Add("".PadLeft(SpecialReport.MaxCharsAtLine, ' '));
                summary.Add("".PadLeft(SpecialReport.MaxCharsAtLine, ' '));
            }
            else
            {
                summary.Add("ÝPTAL EDÝLEN FÝÞ");
                summary.Add(report.Items[0]);
            }
            for (int ln = 40; ln < 12; ln++)
            {
                if (report.Items[ln].Trim().Length == 0)
                    continue;
                summary.Add(report.Items[ln]);
            }
            summary.Add("".PadLeft(SpecialReport.MaxCharsAtLine, ' '));
            summary.Add(String.Format("{0} ÇEKMECE BÝLGÝLERÝ {0}", "".PadLeft(10, '-')));

            for (int ln = 10; ln < 26; ln++)
            {
                if (report.Items[ln].Trim().Length == 0)
                    continue;
                summary.Add(report.Items[ln]);
            }

            summary.Add(String.Format("{0,-" + SpecialReport.MaxCharsAtLine + "}", "KASA KREDÝ"));
            for (int ln = 26; ln < 40; ln++)
            {
                if (report.Items[ln].Trim().Length == 0)
                    continue;
                summary.Add(report.Items[ln]);
            }
            /**/
            summary.Add("".PadLeft(SpecialReport.MaxCharsAtLine, '-'));
            //guiDocument.AddLine(System.String.Format("{0," + SpecialReport.MaxCharsAtLine + "}", System.String.Format("Z NO : {0:D4}", lastz)));

            return toResponse;
        }

        public IPrinterResponse PrintCustomReport(string[] reportText)
        {
            List<string> content = new List<string>();
            content.Add("");
            content.AddRange(Logo);
            content.Add("");
            content.AddRange(Formatter.FormatReceiptHeader(PosMessage.RECEIPT_TR, currentDocumentId));

            content.Add(SaleReport.SurroundTitle(PosMessage.SPECIAL_REPORT));
            content.AddRange(reportText);
            content.Add("");
            content.AddRange(Formatter.FormatEnd());
            content.Add("");

            PrinterResponse response = new PrinterResponse();

            StartCurrentLog(5000);

            
            guiDocument.AddLines(content);

            response.Data = currentDocumentId.ToString("0000");

            foreach (string line in content)
                response.Detail += line + "\r\n";

            return response;
        }

        public IPrinterResponse PrintCustomReceipt(string[] reportText)
        {
            List<string> content = new List<string>();
            content.Add("");
            content.AddRange(Logo);
            content.Add("");
            content.AddRange(Formatter.FormatReceiptHeader(PosMessage.RECEIPT_TR, currentDocumentId));

            content.AddRange(reportText);
            content.Add("");
            content.AddRange(Formatter.FormatEnd());
            content.Add("");

            PrinterResponse response = new PrinterResponse();

            StartCurrentLog(5000);


            guiDocument.AddLines(content);

            response.Data = currentDocumentId.ToString("0000");

            foreach (string line in content)
                response.Detail += line + "\r\n";

            return response;
        }

        public IPrinterResponse PrintZReport()
        {
            PrinterResponse response = new PrinterResponse();
            lastz++;
            if (BeforeZReport != null)
                BeforeZReport(lastz, new EventArgs());

            //StartCurrentLog(4000);
            
            List<String> content = GetSpecialReport(PosMessage.Z_REPORT);

            guiDocument.AddLines(content);

            lastZReportDate = DateTime.Now;
            cashAmountInDrawer = 0;
            cashPayment = 0;
            checkPayment = 0;
            foreignCurrencyPayment = 0;
            documentSold = 0;
            documentSuspended = 0;
            documentVoided = 0;
            currentDocumentId = 0;
            
            response.Data = lastz.ToString();
            response.Detail = "BU Z RAPORU, YAPAY YAZICI ÝLE ALINMIÞTIR";

            if (AfterZReport != null)
                AfterZReport(response, new EventArgs());

            string[] lines = new string[3];
            lines[0] = "Id:=" + CurrentDocumentId;
            lines[1] = "No:=" + lastz;
            lines[2] = "Date:=" + DateTime.Now.ToString();
            IOUtil.WriteAllLines(lastZLog, lines);

            if (File.Exists(currentLog))
                File.Delete(currentLog);

            if (File.Exists(lastDocumentLog))
                File.Delete(lastDocumentLog);

            return response;
        }

        public IPrinterResponse PrintZReport(int count, decimal amount, bool isAffectDrawer)
        {
            return PrintZReport();
        }

        public DateTime LastZReportDate
        {
            get
            {
                if (File.Exists(lastZLog))
                {
                    string[] lines = IOUtil.ReadAllLines(lastZLog);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Date"))
                        {
                            lastZReportDate = Convert.ToDateTime(line.Split('=')[1]);
                            return lastZReportDate;
                        }
                    }
                }

                if (lastZReportDate == DateTime.MinValue)
                    lastZReportDate = DateTime.Now.AddMinutes(-2);

                return lastZReportDate;
            }
        }

        public int LastZReportNo
        {
            get
            {
                if (File.Exists(lastZLog))
                {
                    string[] lines = IOUtil.ReadAllLines(lastZLog);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("No"))
                        {
                            lastz = Convert.ToInt32(line.Split('=')[1]);
                            return lastz;
                        }
                    }
                }
                return lastz;
            }
        }

        public int LastZReportId
        {
            get
            {
                if (File.Exists(lastZLog))
                {
                    string[] lines = IOUtil.ReadAllLines(lastZLog);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Id"))
                        {
                            LastZDocID = Convert.ToInt32(line.Split('=')[1]);
                            return LastZDocID;
                        }
                    }
                }
                return LastZDocID;
            }
        }

        public PrintedDocumentInfo GetLastDocumentInfo(bool lastZ)
        {
            PrintedDocumentInfo pdi = new PrintedDocumentInfo();
            if(lastZ)
            {
                pdi.DocId = LastZDocID;
                pdi.ZNo = lastz;
                pdi.EjNo = 1;
                pdi.Type = ReceiptTypes.Z_REPORT;
                pdi.DocDateTime = LastZReportDate;
            }
            else
            {
                if(File.Exists(lastDocumentLog))
                {
                    string[] lines = IOUtil.ReadAllLines(lastDocumentLog);
                    foreach(string line in lines)
                    {
                        if(line.StartsWith("Id"))
                        {
                            pdi.DocId = Convert.ToInt32(line.Split('=')[1]);
                        }
                        else if(line.StartsWith("Type"))
                        {
                            pdi.Type = (ReceiptTypes)Convert.ToInt32(line.Split('=')[1]);
                        }
                        else if(line.StartsWith("Date"))
                        {
                            pdi.DocDateTime = Convert.ToDateTime(line.Split('=')[1]);
                        }
                    }
                }
                pdi.ZNo = lastz;
                pdi.EjNo = 1;
                
            }
            return pdi;
        }

        public String GetOrderNum()
        {
            return "12345";
        }

        public IPrinterResponse PrintLogs(string date)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse CreateDB()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse StartFM(int fiscalNumber)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintPeriodicReport(int firstZ, int lastZ, bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintPeriodicReport(DateTime firstDay, DateTime lastDay, bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool InServiceMode
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public IPrinterResponse EnterServiceMode(String password)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse ExitServiceMode(String password)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse FormatMemory()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        
        public IPrinterResponse FactorySettings()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse CloseFM()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse SetExDeviceAddress(string tcpIp, int port)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse UpdateFirmware(string tcpIp, int port)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public IPrinterResponse PrintDailyMemory(int address)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintFiscalMemory(int address)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse InitEJ()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJSummary()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJDocument(DateTime documentTime, bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJDocument(int ZReportId, int docId, bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJZReport(int ZReportId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJPeriodic(int ZStartId, int docStartId, int ZEndId, int docEndId, bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJPeriodic(DateTime startTime, DateTime endTime, bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJPeriodic(DateTime day, bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Connect()
        {
            guiDocument.Show();
        }

        public void CloseConnection()
        {
            guiDocument.Close();
        }

        public bool ConnectionIsOpen()
        {
            if (guiDocument.IsAccessible)
                return true;
            else
                return false;
        }

        public void RefreshConnection()
        {

        }

        public IPrinterResponse CheckPrinterStatus()
        {
            return toResponse;
        }

        public int CurrentDocumentId
        {
            get
            {
                string logFile = currentLog;
                if (!File.Exists(currentLog))
                    logFile = lastDocumentLog;
                
                if (!File.Exists(logFile))
                {
                    currentDocumentId = 0;
                    return currentDocumentId;
                }

                StreamReader sr = new StreamReader(logFile);
                try
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] values = line.Split('=');
                        if (values[0] == "Id")
                        {
                            currentDocumentId = Convert.ToInt32(values[1]);
                            break;
                        }
                    }
                }
                catch { }
                finally { sr.Close(); }
                return currentDocumentId;
            }
        }

        public decimal[] TaxRates
        {
            get
            {
                if (File.Exists("Tax.gui"))
                {
                    string[] strRates = IOUtil.ReadAllText("Tax.gui").Split(',');
                    for (int i = 0; i < Department.NUM_TAXGROUPS; i++)
                        taxRates[i] = decimal.Parse(strRates[i]) / 100m;
                }
                else
                {
                    for (int i = 0; i < Department.NUM_TAXGROUPS; i++)
                        taxRates[i] = 0;
                }

                return taxRates;
            }
            set
            {
                taxRates=value;
                String tr="";
                for (int i = 0; i < Department.NUM_TAXGROUPS; i++)
                    tr += "," + (int)(taxRates[i] * 100);

                IOUtil.WriteAllText("Tax.gui", tr.Substring(1));
            }
        }

        public IPrinterResponse OpenDrawer()
        {
            return toResponse;
        }

        public bool DailyMemoryIsEmpty
        {
            get { return CurrentDocumentId < 2; }
        }

        public IPrinterResponse EnterFiscalMode(DateTime fiscalizationDate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse EnterFiscalMode(string password)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string FPUPassword
        {
            get { return "200200"; }
        }

        public void SetSystemTime(DateTime dt)
        {
            SYSTEMTIME sysTime = new SYSTEMTIME();
            sysTime.SetDateTime(dt);
            try
            {
                if (DateTimeChanged != null)
                    DateTimeChanged(dt, new EventArgs());
                ConfigureSystemTime.SetLocalTime(ref sysTime);
            }
            catch (Exception ex)
            {
                EZLogger.Log.Warning(String.Format("{0} {1}", "PC Time could not be set", ex));
                throw new SystemException("ÖNOFIS SAATI\nAYARLANAMADI");
            }
        }

        public void AdjustPrinter(ISalesDocument document)
        {
            salesDocument = document;
        }

        public bool CanPrint(ISalesDocument document)
        {
            return true;
        }

        public bool IsCompact
        {
            get { return true; }
        }
        public IPrinterResponse InterruptReport()
        {
            return new PrinterResponse();
        }
        public IReport GetReports(bool ejOnly)
        {
            IReport reports = null;
            AuthorizationLevel level = AuthorizationLevel.X;
            reports = new Report("RAPORLAR", "", false, level, ReportGroup.NONE);

            level = DataConnector.CurrentSettings.GetAuthorizationLevel(Authorizations.ZReport);
            Report z = new Report(PosMessage.Z_REPORT, "PrintZReport", false, level, ReportGroup.Z_REPORTS);

            level = DataConnector.CurrentSettings.GetAuthorizationLevel(Authorizations.XReport);
            Report x = new Report(PosMessage.X_REPORT, "PrintXReport", true, level, ReportGroup.X_REPORTS);

            z.Parent = reports;
            x.Parent = reports;

            return reports;
        }

        #endregion

        /// <summary>
        /// if it is not required to send currencies, return value will be zero
        /// </summary>
        public int MaxNumberOfCredits
        {
            get
            {
                if (printer != null && IsVx675)
                    return MAX_CREDIT_VX675;
                else
                    return MAX_CREDIT;
            }
        }
        /// <summary>
        /// if it is not required to send currencies, return value will be zero
        /// </summary>
        public int MaxNumberOfCurrencies { get { return MAX_FCURRENCY; } }

        public IPrinterResponse SendCreditInfo(ICredit[] credits)
        {
            IPrinterResponse response = null;

            return response;
        }
        public IPrinterResponse SendCurrencyInfo(ICurrency[] currencies)
        {
            IPrinterResponse response = null;

            return response;
        }
        public IPrinterResponse StartSlip(ISalesDocument document)
        {            
            guiDocument.AddLine("");                        
            return toResponse;
        }
        public IPrinterResponse PrintSlipLine(String strLine)
        {
            guiDocument.AddLine(Formatter.FormatSlipLine(strLine));
            return toResponse;  
        }
        public IPrinterResponse EndSlip(ISalesDocument document)
        {
            return toResponse;  
        }

        private void MoveCurrentDocument(int endType)
        {
            WriteCurrentLog("End=" + endType);
            if(File.Exists(lastDocumentLog))
                File.Delete(lastDocumentLog);
            if (File.Exists(currentLog))
                File.Move(currentLog, lastDocumentLog);
        }


        private void StartCurrentLog(int documentTypeId)
        {
            currentDocumentId++;
            string[] lines = new string[3];
            lines[0] = "Id=" + currentDocumentId;
            lines[1] = "Type=" + documentTypeId;
            lines[2] = "Date=" + DateTime.Now.ToString();
            IOUtil.WriteAllLines(currentLog, lines);
        }

        private void WriteCurrentLog(string line)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(currentLog,true);
                sw.WriteLine(line);
            }
            catch { }
            finally
            {
                if (sw != null) sw.Close();
            }
        }

        private int GetDocumentType()
        {
            StreamReader sr = null;
            int type = 1000;
            try
            {
                sr = new StreamReader(currentLog);
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("Type"))
                    {
                        type = int.Parse(line.Substring(line.IndexOf("=") + 1));
                        break;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (sr != null) sr.Close();
            }
            return type;
        }

        private State PrinterState
        {
            get
            {
                if (cashier == null)
                    return State.LOGIN;

                State pState = State.IDLE;
                string lastLineHead = "";

                if (!File.Exists(currentLog))
                    return pState;

                using (StreamReader sr = new StreamReader(currentLog))
                {
                    string line = "";
                    
                    while((line = sr.ReadLine()) != null)
                    {
                        string[] splittedLine = line.Split('=');
                        if (splittedLine.Length != 2) continue;

                        lastLineHead = splittedLine[0];                      
                    }
                }
              
                switch(lastLineHead)
                {
                    case "Id":
                    case "Type":
                    case "Date":
                    case "Item":
                    case "Discount":
                    case "Fee":
                    case "VoidDiscount":
                    case "VoidFee":
                        if (Document.DocumentTypeId == (int)DocumentTypes.CAR_PARKING)
                            pState = State.OPEN_SALE;
                        else
                            pState = State.SELLING;
                        break;
                    case "Cash":
                    case "Credit":
                    case "Currency":
                    case "Check":
                        if (Document.DocumentTypeId == (int)DocumentTypes.ADVANCE ||
                           Document.DocumentTypeId == (int)DocumentTypes.COLLECTION_INVOICE ||
                           Document.DocumentTypeId == (int)DocumentTypes.CURRENT_ACCOUNT_COLLECTION)
                        {
                            if ((CalculateTotal() + Document.TotalAmount) == Decimal.Zero)
                                pState = State.OPEN_SALE;
                            else
                                pState = State.PAYMENT;
                        }
                        else
                        {
                            if (CalculateTotal() != Decimal.Zero)
                                pState = State.PAYMENT;
                            else
                                pState = State.OPEN_SALE;
                        }
                        break;
                    
                }

                return pState;
            }
        }

        private decimal CalculateTotal()
        {
            StreamReader sr = null;
            decimal total = 0;
            try
            {
                sr = new StreamReader(currentLog);
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    string[] values = line.Split('=');
                    if (values.Length != 2) continue;

                    switch (values[0])
                    {
                        case "Item":
                            total += Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Discount":
                            total -= Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Fee":
                            total += Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "VoidDiscount":
                            total += Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "VoidFee":
                            total -= Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Cash":
                            total -= Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Credit":
                            total -= Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Currency":
                            total -= Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Check":
                            total -= Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch(Exception)
            {
            }
            finally
            {
                if (sr != null) sr.Close();
            }
            return total;
        }

        private decimal CalculateTotalPaid()
        {
            StreamReader sr = null;
            decimal total = 0;
            try
            {
                sr = new StreamReader(currentLog);
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    string[] values = line.Split('=');
                    if (values.Length != 2) continue;

                    switch (values[0])
                    {
                        case "Cash":
                            total += Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Credit":
                            total += Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Currency":
                            total += Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        case "Check":
                            total += Decimal.Parse(line.Substring(line.IndexOf("=") + 1));
                            break;
                        default:
                            break;
                    }
}
            }
            catch(Exception)
            {
            }
            finally
            {
                if (sr != null) sr.Close();
            }
            return total;
        }

        public void SetZLimit(long zLimit)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void UpdateProducts()
        {
            throw new NotImplementedException();
        }

        public IPrinterResponse CheckCashier(ICashier ch)
        {
            return toResponse;
        }

        public IPrinterResponse CheckCashier(int cashierID, string pwd)
        {
            return toResponse;
        }

        public IPrinterResponse SaveCashier(ICashier ch)
        {
            return toResponse;
        }

        public IPrinterResponse SaveCashier(int cashierID, string pwd)
        {
            return toResponse;
        }

        public void SaveCashiers(List<String> nameList, List<int> passwordList)
        {
            return;
        }

        public IPrinterResponse SaveGMPConnectionInfo(string ipVal, int portVal)
        {
            return toResponse;
        }

        public void StartFMTest()
        {
            return;
        }
        public void TransferFile(string fileName)
        {
            return;
        }

        public void TestGMP(int index)
        {
            return;
        }

        public IPrinterResponse SaveNetworkSettings(string ip, string subnet, string gateway)
        {
            return toResponse;
        }  

        public IPrinterResponse GetEFTSlipCopy(int acquierId, int batchNo, int stanNo, int ZNo, int docNo)
        {
            return toResponse;
        }

        public IPrinterResponse VoidEFTPayment(int acquierId, int batchNo, int stanNo)
        {
            return toResponse;
        }

        public IPrinterResponse RefundEFTPayment(int acquierId)
        {
            return toResponse;
        }

        public IPrinterResponse RefundEFTPayment(int acquierId, decimal amount)
        {
            return toResponse;
        }

        public IPrinterResponse PrintEDocument(int docType, string[] lines)
        {
            return toResponse;
        }

        public void ReleasePrinter()
        {
        }


        public static List<PaymentInfo> GetPayments(ISalesDocument Document)
        {
            List<PaymentInfo> payments = new List<PaymentInfo>();
            decimal paidTotal = 0.00m;
            PaymentInfo pi = null;

            //PAYMENTS WITH CHECK
            String[] checkpayments = Document.GetCheckPayments();
            foreach (String checkpayment in checkpayments)
            {
                String[] detail = checkpayment.Split('|');// Amount | RefNumber | SequenceNo
                if (detail[1].Length > 12)
                    detail[1] = detail[1].Substring(0, 12);
                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(detail[0]);
                if (detail[1] == "")
                    pi.Index = 0;
                else
                    pi.Index = int.Parse(detail[1]);
                pi.Type = PaymentType.CHECK;
                pi.SequenceNo = int.Parse(detail[2]);
                payments.Add(pi);

                paidTotal += pi.PaidTotal;
            }

            //PAYMENTS WITH CURRENCIES
            String[] currencypayments = Document.GetCurrencyPayments();
            foreach (String currencypayment in currencypayments)
            {
                String[] detail = currencypayment.Split('|');// Amount | Exchange Rate | Name | SequenceNo
                decimal amount = Decimal.Parse(detail[0]);
                decimal quantity = Math.Round(amount / decimal.Parse(detail[1]), 2);

                int id = 0;
                Dictionary<int, ICurrency> currencies = DataConnector.GetCurrencies();
                foreach (ICurrency currency in currencies.Values)
                {
                    if (currency.Name == detail[2])
                        break;
                    id++;
                }
                pi = new PaymentInfo();
                pi.PaidTotal = quantity;
                pi.Type = PaymentType.FCURRENCY;
                pi.Index = id;
                pi.SequenceNo = int.Parse(detail[3]);

                pi.AdditionalInfo = new string[2];
                pi.AdditionalInfo[0] = detail[2];
                pi.AdditionalInfo[1] = detail[1];

                payments.Add(pi);

                paidTotal += amount;
            }

            //PAYMENTS WITH CREDITS
            String[] creditpayments = Document.GetCreditPayments();
            foreach (String creditypayment in creditpayments)
            {
                String[] detail = creditypayment.Split('|');// Amount | Installments | Id | PayViaEFT | SequenceNo
                int id = int.Parse(detail[2]) - 1;

                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(detail[0]);
                pi.Type = PaymentType.CREDIT;
                pi.Index = id;
                pi.viaByEFT = Boolean.Parse(detail[3]);
                pi.SequenceNo = int.Parse(detail[1]);

                payments.Add(pi);

                paidTotal += pi.PaidTotal;
            }

            //PAYMENTS WITH CASH
            String[] cashpayments = Document.GetCashPayments();
            foreach (String cashpayment in cashpayments)
            {
                String[] detail = cashpayment.Split('|'); // Amount | SequenceNo

                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(detail[0]);
                pi.Type = PaymentType.CASH;
                pi.SequenceNo = int.Parse(detail[1]);
                payments.Add(pi);

                paidTotal += pi.PaidTotal;
            }

            payments.Sort(delegate (PaymentInfo x, PaymentInfo y)
            {
                return x.SequenceNo.CompareTo(y.SequenceNo);
            });

            return payments;
        }

        internal static Adjustment ParseAdjLine(string adjLine)
        {
            string[] splitted = adjLine.Split('|');
            decimal amount = decimal.Parse(splitted[0]);

            Adjustment adj = new Adjustment();

            if (splitted[1] == "--")
            {
                adj.Amount = amount;
                if (amount < 0)
                {
                    adj.Type = AdjustmentType.Discount;
                    adj.Amount = adj.Amount * (-1);
                }
                else
                {
                    adj.Type = AdjustmentType.Fee;
                }
            }
            else
            {
                adj.Amount = decimal.Parse(splitted[0]);
                adj.percentage = int.Parse(splitted[1]);
                if (amount < 0)
                {
                    adj.Type = AdjustmentType.PercentDiscount;
                }
                else
                {
                    adj.Type = AdjustmentType.PercentFee;
                }
            }

            return adj;
        }
    }
}
