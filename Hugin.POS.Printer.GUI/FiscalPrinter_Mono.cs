using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;

namespace Hugin.POS.Printer
{    
    public class FiscalPrinter:IFiscalPrinter
    {
        #region IFiscalPrinter Members

        public event EventHandler BeforeZReport;

        public event EventHandler AfterZReport;

        public event EventHandler DocumentRequested;

        public event EventHandler DateTimeChanged;

        public event OnMessageHandler OnMessage;

        private const PrinterProperties PRINTER_PROPERTIES = PrinterProperties.None;

        static string registerId = "";
        static IFiscalPrinter printer = null;
        static ISalesDocument salesDocument = null;
        static ICashier cashier = null;

        bool programMode = false;
        static long zlimit = 0;

        int graphicLogoActive = 0;
        int autoCutterActive = 0;
        string[] logo = new string[6];
        decimal[] taxRates = new decimal[Department.NUM_TAXGROUPS];

        String strLogoPath = "";
        decimal cashAmountInDrawer = 0;
        decimal totalChange = 0;

        int lastz = 0;
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

		const String pathOS = "/sdcard/HuginPOS/Gui/";
		private const string currentLog = pathOS + "CurrentDocument.gui";
		private const string lastDocumentLog = pathOS + "LastDocument.gui";
		private const string lastZLog = pathOS + "LastZ.gui";
		private const string cashierLog = pathOS + "Cashier.gui";
		private const string logoLog = pathOS + "Logo.gui";
		private const string taxLog = pathOS + "Tax.gui";
		private const string autoCutterLog = pathOS + "AutoCutter.gui";
		private const string graphicLogoLog = pathOS + "GraphicLogo.gui";
        
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
        public string[] Logo
        {
            get
            {
                if (File.Exists(logoLog))
				{
					try{
					logo = IOUtil.ReadAllLines(logoLog);
					}catch{}
				}
                if(logo.Length<6)
				{   
					logo = new string[] { "", "", "", "", "", "" };
				}
                return logo;
            }
            set
            {
                logo = value;
				IOUtil.WriteAllLines(logoLog, logo);
            }
        }

        public int AutoCutter
        {
            get
            {
                if (File.Exists(autoCutterLog))
				{
					autoCutterActive = 0;
					try{
					autoCutterActive = Convert.ToInt32(IOUtil.ReadAllText(autoCutterLog));
					}catch{}
				}
                return autoCutterActive;
            }
            set
            {
                autoCutterActive = value;
				IOUtil.WriteAllText(autoCutterLog, autoCutterActive.ToString());
            }
        }

        public int GraphicLogoActive
        {
            get
            {
                if (File.Exists(graphicLogoLog))
				{
					graphicLogoActive = 0;
					try{
					graphicLogoActive = Convert.ToInt32(IOUtil.ReadAllText(graphicLogoLog));
					}catch{}
				}
                return graphicLogoActive;
            }
            set
            {
                graphicLogoActive = value;
				IOUtil.WriteAllText(graphicLogoLog, graphicLogoActive.ToString());
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
            get { return cashAmountInDrawer; }
        }

        public decimal TotalChange
        {
            get { return totalChange; }
        }

        public IPrinterResponse Withdraw(Decimal amount)
        {
            cashPayment += amount;
            cashAmountInDrawer += amount;
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
            cashAmountInDrawer += amount;
            return toResponse;
        }

        public IPrinterResponse Deposit(Decimal amount)
        {
            if (cashAmountInDrawer < amount) throw new NegativeResultException();
            return toResponse;
        }

        public bool IsFiscal
        {
            get { return true;  }
        }

        public IPrinterResponse Print(IAdjustment ai)
        {
            guiDocument.AddLines(Formatter.Format(ai));
            if (ai.Method == AdjustmentType.Discount || ai.Method == AdjustmentType.PercentDiscount)
                WriteCurrentLog("Discount=" + Math.Abs(ai.NetAmount));
            else
                WriteCurrentLog("Fee=" + ai.NetAmount);

            return toResponse;         
        }

        public void ClearDisplay()
        {
        }
        public IPrinterResponse Correct(IAdjustment ai)
        {
            return Void(ai);
        }

        public IPrinterResponse Correct(IFiscalItem fi)
        {
            guiDocument.AddLines(Formatter.FormatVoid(fi));
            WriteCurrentLog("Item=" + fi.TotalAmount);
            return toResponse;
        }

        public IPrinterResponse Print(IAdjustment[] ai)
        {
            guiDocument.AddLines(Formatter.Format(ai));
            foreach (IAdjustment adj in ai)
            {
                if (adj.Method == AdjustmentType.Discount || adj.Method == AdjustmentType.PercentDiscount)
                    WriteCurrentLog("Discount=" + Math.Abs(adj.NetAmount));
                else
                    WriteCurrentLog("Fee=" + adj.NetAmount);
            }
            return toResponse;  
        }

        public IPrinterResponse Print(IFiscalItem fi)
        {
            guiDocument.AddLines(Formatter.Format(fi));
            WriteCurrentLog("Item=" + fi.TotalAmount);
            return toResponse;  
        }

        public IPrinterResponse Pay(Decimal amount)
        {
            guiDocument.AddLines(Formatter.FormatPayment(amount, PosMessage.CASH));

            WriteCurrentLog("Cash=" + amount);
            return toResponse;
        }
        public IPrinterResponse Pay(Decimal amount,String refNumber)
        {
            guiDocument.AddLines(Formatter.FormatPayment(amount, PosMessage.CHECK));

            WriteCurrentLog("Check=" + amount);
            return toResponse;
        }
        public IPrinterResponse Pay(Decimal amount, ICurrency currency)
        {
            String label = String.Empty;

            Number currencyPayment = new Number(amount / currency.ExchangeRate);

            if (currencyPayment.ToDecimal() > currencyPayment.ToInt())
                label = String.Format("{0} {1:C}", currency.Name, currencyPayment);

            label = String.Format("{0} {1}", currency.Name, currencyPayment);
            guiDocument.AddLines(Formatter.FormatPayment(amount, label));

            WriteCurrentLog("Currency=" + amount);
            return toResponse;
        }
        public IPrinterResponse Pay(Decimal amount, ICredit credit, int installments)
        {
            String label = credit.Name + (installments == 0 ? String.Empty : "/" + installments.ToString());

            guiDocument.AddLines(Formatter.FormatPayment(amount, label));

            WriteCurrentLog("Credit=" + amount);
            return toResponse;
        }

        public IPrinterResponse PrintHeader(ISalesDocument document)
        {
            StartCurrentLog(document.DocumentTypeId);

            document.Id = currentDocumentId;
            salesDocument = document;

            guiDocument.AddLine("");
            if (document.DocumentTypeId < 0)
                guiDocument.AddLines(Logo);

            if (document.DocumentTypeId < 0)
                guiDocument.AddLines(Formatter.FormatReceiptHeader(document.Name, document.Id));
            else
                guiDocument.AddLines(Formatter.FormatHeader(salesDocument));

            return toResponse;
        }


        public IPrinterResponse PrintTotals(ISalesDocument document, bool hardcopy)
        {

            PrinterResponse response = new PrinterResponse();
            int type = GetDocumentType();
            if (type > 100)
                throw new NoDocumentFoundException();
            if (type != document.DocumentTypeId)
                throw new DocumentTypeException();
            
            decimal total = CalculateTotal();

            if (document.TotalAmount != total)
                throw new SubtotalNotMatchException(Math.Abs(total - document.TotalAmount));
            response.Data = "" + total;

            salesDocument = document;
            guiDocument.AddLines(Formatter.FormatTotals(document, hardcopy));
            WriteCurrentLog("Total=" + document.TotalAmount);
            return response;

        }


        public IPrinterResponse PrintFooter(ISalesDocument document)
        {
            salesDocument = document;
            guiDocument.AddLines(Formatter.FormatFooter(document));
            documentSold++;
            totalSold += document.TotalAmount;
            MoveCurrentDocument(0);
            return toResponse;  
        }

        public IPrinterResponse PrintSubTotal(ISalesDocument document, bool hardcopy)
        {
            salesDocument = document;            
            guiDocument.AddLines(Formatter.FormatSubTotal(document, hardcopy));
            return toResponse;  
        }

        public IPrinterResponse PrintRemark(string s)
        {
            guiDocument.AddLine(Formatter.FormatRemark(s));
            return toResponse;  
        }

        public IPrinterResponse PrintFooterNotes()
        {
            guiDocument.AddLines(Formatter.FormatFooterNotes());
            return toResponse;  
        }

        public IPrinterResponse SignInCashier(ICashier ch)
        {
            string id = ch.Id;
            if (File.Exists(cashierLog))
            {
				id = IOUtil.ReadAllText(cashierLog).Trim();
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
			IOUtil.WriteAllText(cashierLog, ch.Id);
            guiDocument.AddLines(Logo);
            guiDocument.AddLines(Formatter.FormatReceiptHeader("FÝÞ", currentDocumentId));
            guiDocument.AddLines(Formatter.FormatInfo(String.Format("KASÝYER : {0} {1}", ch.Id, ch.Name).PadRight(40)));
            guiDocument.AddLines(Formatter.FormatInfo("GÝRÝÞ".PadRight(40)));
            guiDocument.AddLine("");
            guiDocument.AddLines(Formatter.FormatEnd());
            cashier = ch;
            return toResponse;
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
			IOUtil.WriteAllText(cashierLog, "");
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

        public IPrinterResponse PrintXReport(bool hardcopy)
        {
            PrepareDailyReport();
            StartCurrentLog(5000);
            guiDocument.AddLine("".PadLeft(SpecialReport.MaxCharsAtLine, '-'));
            guiDocument.AddLine("".PadLeft(SpecialReport.MaxCharsAtLine, ' '));
            guiDocument.AddLine(String.Format("{0} X  RAPORU {0}", "".PadLeft(14, '*')));
            System.Threading.Thread.Sleep(3000);
            guiDocument.AddLines(Formatter.FormatReceiptHeader("FÝÞ", currentDocumentId));

            guiDocument.AddLines(summary);
            guiDocument.AddLines(Formatter.FormatEnd());

            return toResponse;
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
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintZReport()
        {
            PrinterResponse response = new PrinterResponse();
            lastz++;
            if (BeforeZReport != null)
                BeforeZReport(lastz, new EventArgs());

            StartCurrentLog(4000);
            guiDocument.AddLine("".PadLeft(SpecialReport.MaxCharsAtLine, '-'));
            guiDocument.AddLine("".PadLeft(SpecialReport.MaxCharsAtLine, ' '));
            guiDocument.AddLine(String.Format("{0} Z  RAPORU {0}", "".PadLeft(14, '*')));
            guiDocument.AddLines(Formatter.FormatReceiptHeader("FÝÞ", currentDocumentId));

            guiDocument.AddLines(summary);
            guiDocument.AddLines(Formatter.FormatEnd());

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

            string[] lines = new string[2];
            lines[0] = "No:=" + lastz;
            lines[1] = "Date:=" + DateTime.Now.ToString();
            IOUtil.WriteAllLines(lastZLog, lines);

            if (File.Exists(currentLog))
                File.Delete(currentLog);

            if (File.Exists(lastDocumentLog))
                File.Delete(lastDocumentLog);

            return response;
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
					try{
                    string[] lines = IOUtil.ReadAllLines(lastZLog);
                    lastz = Convert.ToInt32(lines[0].Split('=')[1]);
					}catch{lastz = 1;}
                }
                return lastz;
            }
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

        public IPrinterResponse EnterServiceMode()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse ExitServiceMode()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse FormatMemory()
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

        public IPrinterResponse PrintEJDocument(DateTime documentTime)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJDocument(int ZReportId, int docId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJZReport(int ZReportId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJPeriodic(int ZStartId, int docStartId, int ZEndId, int docEndId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJPeriodic(DateTime startTime, DateTime endTime)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IPrinterResponse PrintEJPeriodic(DateTime day)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Connect()
        {
            guiDocument.Show();
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
				bool exists = false;
                if (File.Exists(taxLog))
                {
					try{
                    string[] strRates = IOUtil.ReadAllText(taxLog).Split(',');
                    for (int i = 0; i < Department.NUM_TAXGROUPS; i++)
                        taxRates[i] = decimal.Parse(strRates[i]) / 100m;
						exists = true;
					}catch{}
                }
				if (!exists)
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

                IOUtil.WriteAllText(taxLog, tr.Substring(1));
            }
        }

        public IPrinterResponse OpenDrawer()
        {
            return toResponse;
        }

        public bool DailyMemoryIsEmpty
        {
            get { return currentDocumentId < 2; }
        }

        public IPrinterResponse EnterFiscalMode(DateTime fiscalizationDate)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string FPUPassword
        {
            get { return "200200"; }
        }

        public void SetSystemTime(DateTime dt)
        {
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
            reports = new Report("RAPORLAR", "", false, level);

            level = DataConnector.CurrentSettings.GetAuthorizationLevel(Authorizations.ZReport);
            Report z = new Report(PosMessage.Z_REPORT, "PrintZReport", false, level);

            level = DataConnector.CurrentSettings.GetAuthorizationLevel(Authorizations.XReport);
            Report x = new Report(PosMessage.X_REPORT, "PrintXReport", true, level);

            z.Parent = reports;
            x.Parent = reports;

            return reports;
        }

        #endregion

        /// <summary>
        /// if it is not required to send currencies, return value will be zero
        /// </summary>
        public int MaxNumberOfCredits { get { return 0; } }
        /// <summary>
        /// if it is not required to send currencies, return value will be zero
        /// </summary>
        public int MaxNumberOfCurrencies { get { return 0; } }

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
            throw new NotSupportedException();
        }
        public IPrinterResponse PrintSlipLine(String strLine)
        {
            throw new NotSupportedException();
        }
        public IPrinterResponse EndSlip(ISalesDocument document)
        {
            throw new NotSupportedException();
        }

        private void MoveCurrentDocument (int endType)
		{
			WriteCurrentLog ("End=" + endType);
			if (File.Exists (lastDocumentLog)) {
				//File.Delete (lastDocumentLog);
				string current = IOUtil.ReadAllText(currentLog);
				IOUtil.WriteAllText(lastDocumentLog, current);
			}
			if (File.Exists (currentLog)) {
				IOUtil.WriteAllText (currentLog, "");
			}
        }


        private void StartCurrentLog (int documentTypeId)
		{
			currentDocumentId++;
			string[] lines = new string[2];
			lines [0] = "Id=" + currentDocumentId;
			lines [1] = "Type=" + documentTypeId;
			try {
				IOUtil.WriteAllLines (currentLog, lines);
			} catch(Exception ex) {
				String exs=ex.Message;
			}
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

        public void SetZLimit(long zLimit)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
