using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Common
{
    public delegate void OnMessageHandler(object sender, OnMessageEventArgs e);

    public class OnMessageEventArgs : EventArgs
    {
        Exception exception;
        String message;
        bool isError;

        public OnMessageEventArgs(Exception pe)
        {
            this.exception = pe;
            isError = true;
        }
        public OnMessageEventArgs(String msg)
        {
            message = msg;
            isError = false;
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public String Message
        {
            get { return message; }
        }

        public bool IsError
        {
            get { return isError; }
        }
    }

    [Flags]
    public enum PrinterProperties
    {
        None = 0,
        HasExternalSlipPrinter = 1,
        ChangablePrinterPort = 2,
        All = HasExternalSlipPrinter | ChangablePrinterPort
    }

    /// <summary>
    /// IFiscalPrinter is the interface to handle a basic printer.
    /// Provides access to members that control the Printer Driver. 
    /// </summary>
    public interface IFiscalPrinter
    {
        /// <summary>
        /// Occurs when the Z Report has started
        /// </summary>
        event EventHandler BeforeZReport;
        /// <summary>
        /// Occurs when the Z Report has finished
        /// </summary>
        event EventHandler AfterZReport;
        /// <summary>
        /// Occurs when the receipt or slip require. 
        /// Printer waits until putting receipt or slip into printer
        /// </summary>
        event EventHandler DocumentRequested;
        /// <summary>
        /// Occurs when the printer date time change
        /// </summary>
        event EventHandler DateTimeChanged;

        /// <summary>
        /// Occurs when the fpu has warning
        /// </summary>
        event OnMessageHandler OnMessage;

        /// <summary>
        /// 
        /// </summary>
        ISalesDocument SaleDocument {get; set;}

        #region P-Key Commands
        /// <summary>
        /// Automaticly feeds receipt 
        /// </summary>
        /// <returns></returns>
        IPrinterResponse Feed();
        /// <summary>
        /// Cuts the paper 
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse CutPaper();
        /// <summary>
        /// Enters the program mode. Function is called before changing printer settings. 
        /// These settings are logo lines, auto cutter mode and tax rates.
        /// Function of <c>ExitProgramMode</c> must called after programing mode
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse EnterProgramMode();
        /// <summary>
        /// Exit the program mode if printer states in program mode. 
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse ExitProgramMode();
        /// <summary>
        /// Loads company graphic logo for printing top of receipt.
        /// Each printers are support different graphics format.
        /// For example, TM5000 printers are only support *.tlg format.
        /// </summary>
        /// <param name="filePath">Path of logo file</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse LoadGraphicLogo(String filePath);
        /// <summary>
        /// Gets a value indicating whether the current printer logo is same
        /// </summary>
        /// <param name="logo">Logo lines to be compare current printer logo</param>
        /// <returns>
        /// True  : if these logos are the same
        /// False : if these logos are different
        /// </returns>
        bool HasSameLogo(String[] logo);
        /// <summary>
        /// Gets or sets compony logo lines.
        /// To set Logo lines ,printer must be in program mode.
        /// </summary>
        String[] Logo { get; set;}
        /// <summary>
        /// Gets or sets auto cutter setting. 
        /// Paper is automaticly cutted end of receipt.
        /// 
        /// Value of 1 is opens the auto cutter mode
        /// Value of 0 is closes the auto cutter mode
        /// To set auto cutter mode ,printer must be in program mode.
        /// </summary>
        int AutoCutter { get; set;}
        /// <summary>
        /// Gets or sets graphic logo active status.
        /// 
        /// Value of 1 is activete the print graphic logo
        /// Value of 0 is disactivete the print graphic logo
        /// To set graphic logo mode,printer must be in program mode.
        /// </summary>
        int GraphicLogoActive { get; set;}
        /// <summary>
        /// Gets or sets receipt barcode active status.
        /// 
        /// Value of 1 is activete the print receipt barcode
        /// Value of 0 is disactivete the print receipt barcode
        /// To set graphic logo mode,printer must be in program mode.
        /// </summary>
        int ReceiptBarcodeActive { get; set; }
        /// <summary>
        /// Gets or sets a receipt limit amount
        /// ex. for 900 TL send 90000
        /// </summary>
        decimal ReceiptLimit { get; set; }
        /// <summary>
        /// Gets or sets printer time
        /// </summary>
        DateTime Time { get;set;}
        /// <summary>
        /// Gets or sets printer date time
        /// To set date-time ,printer must be in servide mode.
        /// </summary>
        DateTime DateTime { get; set;}
        /// <summary>
        /// Gets or sets end of receipt note lines 
        /// </summary>
        string[] EndOfReceiptNote { get; set; }

        #endregion

        #region R-Key Commands
        /// <summary>
        /// Ýf DailyMemory contains any currency payment, return true
        /// </summary>
        bool CurrencyPaymentContains();
        /// <summary>
        /// Get the cash amount which in the drawer
        /// </summary>
        Decimal CashAmountInDrawer { get;}
        /// <summary>
        /// Gets the amount of total change. Total change is difference between value paids and documents total
        /// </summary>
        Decimal TotalChange { get;}
        /// <summary>
        /// Withdraw as Cash
        /// </summary>
        /// <param name="amount">Value of cash amount</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Withdraw(Decimal amount);
        /// <summary>
        /// Withdraw as Check
        /// </summary>
        /// <param name="amount">Value of cash amount</param>
        /// <param name="refNumber">Check referans number</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Withdraw(Decimal amount, String refNumber);
        /// <summary>
        /// Withdraw as credit
        /// </summary>
        /// <param name="credit">credit type</param>
        /// <param name="amount">Value of credit amount</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Withdraw(Decimal amount, ICredit credit);
        /// <summary>
        /// Deposit as cash
        /// </summary>
        /// <param name="amount">Value of cash amount</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Deposit(Decimal amount);
        /// <summary>
        /// Gets a value indicating whether the current printer is fiscal
        /// </summary>
        Boolean IsFiscal { get;}
        /// <summary>
        /// Prints adjustment
        /// </summary>
        /// <param name="ai"></param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Print(IAdjustment ai);
        /// <summary>
        /// Corrects last item's adjustment 
        /// </summary>
        /// <param name="ai">Last applied adjustment</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Correct(IAdjustment ai);
        /// <summary>
        /// Voids applied adjustment 
        /// </summary>
        /// <param name="ai"></param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Print(IAdjustment[] ai);
        /// <summary>
        /// Prints and adds fiscal item to document
        /// </summary>
        /// <param name="fi">Sold item to be prints</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Print(IFiscalItem fi);
        /// <summary>
        /// Payment with Cash
        /// </summary>
        /// <param name="amount">Amount of payment</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Pay(Decimal amount);
        /// <summary>
        /// Payment with Check
        /// </summary>
        /// <param name="amount">Amount of payment</param>
        /// <param name="refNumber">Referans number of check</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Pay(Decimal amount, String refNumber);
        /// <summary>
        /// Payment with Credit
        /// </summary>
        /// <param name="amount">Amount of payment</param>
        /// <param name="credit">Information about Credit</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Pay(Decimal amount, ICredit credit, int installments);
        /// <summary>
        /// Payment with Currency
        /// </summary>
        /// <param name="amount">Amount of payment</param>
        /// <param name="currency">Information about Currency</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Pay(Decimal amount, ICurrency currency);
        /// <summary>
        /// Print document headers. 
        /// Document starts with this function
        /// </summary>
        /// <param name="document"></param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintHeader(ISalesDocument document);
        /// <summary>
        /// Prints document total
        /// </summary>
        /// <param name="document">Document to be prints total amount</param>
        /// <param name="hardcopy">
        /// True  : Prints document total 
        /// False : Document total does not print
        /// </param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintTotals(ISalesDocument document, bool hardcopy);
        /// <summary>
        /// Prints document footer
        /// Document ends after prints footer.
        /// </summary>
        /// <param name="document">Document to be prints footer</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintFooter(ISalesDocument document, bool isReceipt);
        /// <summary>
        /// Prints document subtotal. 
        /// </summary>
        /// <param name="document">Document to be prints subtotal</param>
        /// <param name="hardcopy">
        /// True  : Prints document subtotal 
        /// False : Document subtotal does not print
        /// </param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintSubTotal(ISalesDocument document, bool hardcopy);
        /// <summary>
        /// Prints remark line
        /// </summary>
        /// <param name="s">Value of remark</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintRemark(String s);
        /// <summary>
        /// Prints document footer notes.
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintFooterNotes();
        /// <summary>
        /// Checks cashier is valid or not. 
        /// </summary>
        /// <param name="ch">Checks Cashier which is authorized </param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse SaveCashier(ICashier ch);
        /// <summary>
        /// Sets cashier name to ECR
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        IPrinterResponse SaveCashier(int id, string name);
        /// <summary>
        /// Checks cashier is valid or not. 
        /// </summary>
        /// <param name="ch">Checks Cashier which is authorized </param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse CheckCashier(int id, string password);
        /// <summary>
        /// Signs in cashier. 
        /// </summary>
        /// <param name="ch">Cashier to be signs in </param>
        /// <returns>Returns printer response</returns>
        ICashier SignInCashier(int id, string password);
        /// <summary>
        /// Signs out current cashier
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse SignOutCashier();
        /// <summary>
        /// Voids current document
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Void();
        /// <summary>
        /// Corrects last sold item.
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Correct(IFiscalItem fi);
        /// <summary>
        /// Suspends current document.
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Suspend();
        /// <summary>
        /// Voids sold item
        /// </summary>
        /// <param name="fi">Fiscal item to be void</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse Void(IFiscalItem fi);
        /// <summary>
        /// void adjustment of fiscal item
        /// </summary>
        /// <param name="fi">Last applied adjustment</param>
        /// <returns></returns>
        IPrinterResponse Void(IAdjustment fi);
        /// <summary>
        /// Gets current document's subtotal amounts
        /// </summary>
        Decimal PrinterSubTotal { get;}
        /// <summary>
        /// Clear customer display.
        /// This function is only supported by FPU245 printer.
        /// </summary>
        void ClearDisplay();

        List<String> GetCashiers();
        void SaveCashiers(List<String> nameList, List<int> passwordList);

        IPrinterResponse SaveGMPConnectionInfo(string ipVal, int portVal);

        void StartFMTest();

        void TransferFile(string fileName);

        void TestGMP(int index);

        IPrinterResponse SaveNetworkSettings(string ip, string subnet, string gateway);

        #endregion

        #region X-Key Commands
        /// <summary>
        /// Prints register report.
        /// Each document types shows with department detail in different apparts.
        /// </summary>
        /// <param name="hardcopy">
        /// True  : Prints report
        /// False : Report does not print
        /// </param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintRegisterReport(bool hardcopy);
        /// <summary>
        /// Prints program report
        /// Departmet values are printed with their taxrates.
        /// Printer settings are also printed in <c>PrintProgramReport</c>
        /// </summary>
        /// <param name="hardcopy">
        /// True  : Prints report
        /// False : Report does not print
        /// </param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintProgramReport(bool hardcopy);
        /// <summary>
        /// Prints X report.
        /// X report includes all Z report contents but X report is not 
        /// fiscal report. X report provides to see Z report contents without 
        /// reset payments
        /// </summary>
        /// <param name="hardcopy">
        /// True  : Prints report
        /// False : Report does not print
        /// </param>
        /// <param name="hardcopy"></param>
        /// <returns>Returns printer response</returns>
        /// <seealso cref="PrintZReport"/>
        IPrinterResponse PrintXReport(bool hardcopy);
        /// <summary>
        /// Controls and prepares the printer for daily reporting
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrepareDailyReport();
        /// <summary>
        /// Prints custom report lines.
        /// </summary>
        /// <param name="reportText">Includes report details</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintCustomReport(String[] reportText);
        /// <summary>
        /// Prints custom report lines.
        /// </summary>
        /// <param name="reportText">Includes report details</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintCustomReceipt(String[] reportText);

        IPrinterResponse PrintXPluReport(int firstPLU, int lastPLU, bool hardcopy);

        IPrinterResponse PrintEndDayReport();

        #endregion

        #region Z-Key Commands
        /// <summary>
        /// Prints Z report. Z report is a typical end-of-day report 
        /// containing all balances of sold items per category and 
        /// the total amount that should be in the drawer.
        /// It resets day totals
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintZReport();

        IPrinterResponse PrintZReport(int count, decimal amount, bool isAffectDrawer);

        /// <summary>
        /// Gets last Z report date.
        /// </summary>
        /// <returns>Returns printer response</returns>
        /// <seealso cref="PrintXReport"/>
        DateTime LastZReportDate { get;}
        /// <summary>
        /// Gets last Z report number.
        /// </summary>
        int LastZReportNo { get;}
        /// <summary>
        /// Prints periodic report. Periodic report contains one or more Z report informations.
        /// </summary>
        /// <param name="firstZ">Firts Z number</param>
        /// <param name="lastZ">Last Z number</param>
        /// <param name="hardcopy">
        /// True  : Prints report
        /// False : Report does not print
        /// </param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintPeriodicReport(int firstZ, int lastZ, bool hardcopy);
        /// <summary>
        /// Prints periodic report. Report contains payment informations between two dates.
        /// </summary>
        /// <param name="firstDay">First day Z report range</param>
        /// <param name="lastDay">Last day Z report range</param>
        /// <param name="hardcopy">
        /// True  : Prints report
        /// False : Report does not print
        /// </param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintPeriodicReport(DateTime firstDay, DateTime lastDay, bool hardcopy);

        #endregion

        #region S-Key Commands
        /// <summary>
        /// Gets a value indicating whether the printer is in service mode.
        /// </summary>
        bool InServiceMode { get;}
        /// <summary>
        /// Starts service mode. Before set printer service values, printer must be in service mode.
        /// </summary>
        /// <returns></returns>
        IPrinterResponse EnterServiceMode(String password);
        /// <summary>
        /// Exits service mode. 
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse ExitServiceMode(String password);
        /// <summary>
        /// Formats fiscal memory.
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse FormatMemory();
        /// <summary>
        /// Prints Z contents from defining address.
        /// </summary>
        /// <param name="address">Start address</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintDailyMemory(int address);
        /// <summary>
        /// Prints fiscal memory contents begining defining address
        /// </summary>
        /// <param name="address">Start address</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintFiscalMemory(int address);
        /// <summary>
        /// Sets initialize options to FPU database
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse FactorySettings();
        /// <summary>
        /// Closes Fiscal Memory
        /// </summary>
        /// <returns>Printer response</returns>
        IPrinterResponse CloseFM();
        /// <summary>
        /// Sets or changes external device tcp/ip address
        /// </summary>
        /// <param name="tcpIp">IP address</param>
        /// <param name="port">Port number</param>
        /// <returns></returns>
        IPrinterResponse SetExDeviceAddress(String tcpIp, int port);
        /// <summary>
        /// Updates FPU Firmware
        /// </summary>
        /// <param name="tcpIp">IP address</param>
        /// <param name="port">Port number</param>
        /// <returns></returns>
        IPrinterResponse UpdateFirmware(String tcpIp, int port);
        #endregion

        #region EJ Commands

        /// <summary>
        /// Initialization of electronic journal
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse InitEJ();
        /// <summary>
        /// Prints summary of electronic journal. 
        /// Summary contains 
        /// -Electronic journal number
        /// -Fiscal Id
        /// -Compony logo lines
        /// -First Z report informations which are Z no, Document no,date and time
        /// -Last Z report informations which are Z no, Document no,date and time
        /// -First sold document informations which are Z no, Document no,date and time
        /// -Last sold document informations which are Z no, Document no,date and time
        /// -Electronic journal capacity summaries
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintEJSummary();
        /// <summary>
        /// Prints a document by documet's date time.
        /// </summary>
        /// <param name="documentTime">Document's date time.</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintEJDocument(DateTime documentTime, bool hardcopy);
        /// <summary>
        /// Prints old document's content by Z number and document's id.
        /// </summary>
        /// <param name="ZReportId">Z number</param>
        /// <param name="docId"></param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintEJDocument(int ZReportId, int docId, bool hardcopy);
        /// <summary>
        /// EJ report is copy of tickets which saved before in ej 
        /// report parameters are search criteria in ej
        /// function formats header, reads  receipt contents
        /// from ej and prints contents then formats footer
        /// </summary>
        /// <param name="ZReportId">Z number</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintEJZReport(int ZReportId);
        /// <summary>
        /// Prints periodic reports between two z numbers 
        /// </summary>
        /// <param name="ZStartId">First Z number where the reports starts</param>
        /// <param name="docStartId">First document id where the report starts</param>
        /// <param name="ZEndId">Last Z number where the reports ends</param>
        /// <param name="docEndId">Last document id  where the report ends</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintEJPeriodic(int ZStartId, int docStartId, int ZEndId, int docEndId, bool hardcopy);
        /// <summary>
        /// Prints periodic reports between two dates
        /// </summary>
        /// <param name="startTime">Start address</param>
        /// <param name="endTime">Finish address</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintEJPeriodic(DateTime startTime, DateTime endTime, bool hardcopy);
        /// <summary>
        /// Prints periodic reports daily
        /// </summary>
        /// <param name="day">Reporting date</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse PrintEJPeriodic(DateTime day, bool hardcopy);
        /// <summary>
        /// Defines virtual limit to electronic journal.
        /// This functions should be used for testing. 
        /// </summary>
        void SetZLimit(long zLimit);
        #endregion

        #region Other Commands
        /// <summary>
        /// Connects printer.
        /// </summary>
        void Connect();
        void CloseConnection();
        bool ConnectionIsOpen();
        /// <summary>
        /// Checks printer status
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse CheckPrinterStatus();
        /// <summary>
        /// Gets current document id.
        /// </summary>
        int CurrentDocumentId { get; }
        /// <summary>
        /// Gets or sets tax rates. Tax rates are changed in program mode.
        /// </summary>
        decimal[] TaxRates { get; set; }
        /// <summary>
        /// Gets or sets subcategory. 
        /// </summary>
        Category[] Category { get; set; }
        /// <summary>
        /// Gets or sets departments. 
        /// </summary>
        Department[] Departments { get; set; }
        /// <summary>
        /// Gets or sets Credits
        /// </summary>
        ICredit[] Credits { get; set; }
        /// <summary>
        /// Gets or sets Currencies
        /// </summary>
        ICurrency[] Currencies { get; set; }
        /// <summary>
        /// Gets or sets products. 
        /// </summary>
        void UpdateProducts();
        /// <summary>
        /// Opens drawer.
        /// </summary>
        /// <returns>Returns printer response</returns>
        IPrinterResponse OpenDrawer();
        /// <summary>
        /// Gets a value indicating whether the daily memory is empty.
        /// </summary>
        bool DailyMemoryIsEmpty { get;}
        /// <summary>
        /// Used to go to fiscal mode.
        /// </summary>
        /// <param name="fiscalizationDate">Fiscalization date</param>
        /// <returns>Returns printer response</returns>
        IPrinterResponse EnterFiscalMode(string password);
        /// <summary>
        /// Gets fiscal password.
        /// </summary>
        String FPUPassword { get;}
        /// <summary>
        /// Sets printer time.
        /// </summary>
        /// <param name="dt"></param>
        void SetSystemTime(DateTime dt);
        /// <summary>
        /// Gets information about last printed document.
        /// </summary>
        /// <param name="zReport">Gets last Z Report Info otherwise last receipt info returns</param>
        /// <returns>Returns printed document info class</returns>
        PrintedDocumentInfo GetLastDocumentInfo(bool zReport);
        /// <summary>
        /// Gets order number from printer
        /// </summary>
        /// <returns>Returns printer response</returns>
        String GetOrderNum();
        /// <summary>
        /// Print logs
        /// </summary>
        IPrinterResponse PrintLogs(string date);
        /// <summary>
        /// Creates sales database
        /// </summary>
        IPrinterResponse CreateDB();

        IPrinterResponse StartFM(int fiscalNo);

        void RefreshConnection();

        bool IsVx675 { get; }

        #endregion

        #region Confirmation
        /// <summary>
        /// Adjusts document.
        /// </summary>
        /// <param name="document">Document to be adjust</param>
        void AdjustPrinter(ISalesDocument document);
        /// <summary>
        /// Try to print document.
        /// </summary>
        /// <param name="document"></param>
        /// <returns>
        /// True  : If document prints
        /// False : If document doesn't print
        /// </returns>
        Boolean CanPrint(ISalesDocument document);
        /// <summary>
        /// Gets available reports.
        /// </summary>
        /// <param name="ejOnly">Gets only electronic journal reports if true</param>
        /// <returns></returns>
        IReport GetReports(bool ejOnly);
        /// <summary>
        /// Gets a value indicating whether the current printer is embedded system
        /// </summary>
        bool IsCompact { get;}
        /// <summary>
        /// Try to cancel printing report.
        /// Some reports support this function.
        /// </summary>
        /// <returns></returns>
        IPrinterResponse InterruptReport();
        /// <summary>
        /// Gets printer's properties.
        /// </summary>
        bool HasProperties(PrinterProperties properties);
        #endregion

        #region specific commands for europe
        /// <summary>
        /// if it is not required to send currencies, return value will be zero
        /// </summary>
        int MaxNumberOfCredits { get; }
        /// <summary>
        /// if it is not required to send currencies, return value will be zero
        /// </summary>
        int MaxNumberOfCurrencies { get; }

        /// <summary>
        /// Sends credits information to printer when initializing of printer.  
        /// </summary>
        /// <param name="credits">ICredit list to be set printer</param>
        /// <returns></returns>
        IPrinterResponse SendCreditInfo(ICredit[] credits);
        /// <summary>
        /// Sends currency information to printer when initializing of printer.  
        /// </summary>
        /// <param name="currencies">ICurrency list to be set printer</param>
        /// <returns></returns>
        IPrinterResponse SendCurrencyInfo(ICurrency[] currencies);
        /// <summary>
        /// works only in slip document
        /// </summary>
        /// <param name="strLine"></param>
        /// <returns></returns>
        IPrinterResponse StartSlip(ISalesDocument document);
        /// <summary>
        /// works only in slip document
        /// </summary>
        /// <param name="strLine"></param>
        /// <returns></returns>
        IPrinterResponse PrintSlipLine(String strLine);
        /// <summary>
        /// works only in slip document
        /// </summary>
        /// <param name="strLine"></param>
        /// <returns></returns>
        IPrinterResponse EndSlip(ISalesDocument document);
        #endregion

        IPrinterResponse GetEFTSlipCopy(int acquierId, int batchNo, int stanNo, int ZNo, int docNo);
        IPrinterResponse VoidEFTPayment(int acquierId, int batchNo, int stanNo);
        IPrinterResponse RefundEFTPayment(int acquierId);
        IPrinterResponse RefundEFTPayment(int acquierId, decimal amount);
        IPrinterResponse PrintEDocument(int docType, string[] lines);
        void ReleasePrinter();

    }
}
