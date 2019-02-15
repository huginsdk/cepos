using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;
#if !MONO
using System.IO.Ports;
#endif
using System.Net;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Drawing;
using Hugin.Common;
using Hugin.POS.CompactPrinter.FP300;
using System.Runtime.InteropServices;

namespace Hugin.POS.Printer
{
    public enum PaymentType
    {
        CASH,
        CREDIT,
        CHECK,
        FCURRENCY
    }
    public enum FPProgramOptions
    {
        CUTTER,
        PAY_WITH_EFT,
        RECEIPT_LIMIT,
        GRAPHIC_LOGO,
        RECEIPT_BARCODE
    }

    public class Adjustment
    {
        public AdjustmentType Type = AdjustmentType.Discount;
        public decimal Amount = 0m;
        public int percentage = 0;
        public string NoteLine1;
        public string NoteLine2;
    }
    public class PaymentInfo
    {
        public PaymentType Type = 0;
        public int Index = 0;
        public decimal PaidTotal = 0.00m;
        public bool viaByEFT = false;
        public int SequenceNo;
    }

    public class JSONItem
    {
        public int Id;
        public decimal Quantity;
        public decimal Price;
        public string Name;
        public int DeptId;
        public int Status;
        public Adjustment Adj = null;
        public string NoteLine1;
        public string NoteLine2;
    }

    public class EndOfReceipt
    {
        public bool CloseReceiptFlag = true;
        public bool BarcodeFlag = false;
        public string Barcode;
    }

    public class JSONDocument
    {
        public List<JSONItem> FiscalItems = new List<JSONItem>();
        public List<Adjustment> Adjustments = new List<Adjustment>();
        public List<PaymentInfo> Payments = new List<PaymentInfo>();
        public List<String> FooterNotes = new List<string>();
        public EndOfReceipt EndOfReceiptInfo = new EndOfReceipt();
    }

    class DrawerInfo
    {
        // Totals
        public int TotalDocumentCount = 0;
        public decimal TotalAmount = 0.00m;
        // Fiscal Receipt
        public int FiscalReceiptCount = 0;
        public decimal FiscalReceiptTotalAmount = 0.00m;
        // Void Document
        public int VoidedDocumentCont = 0;
        public decimal VoidedAmount = 0.00m;
        // Discount
        public decimal DiscountTotalAmount = 0.00m;
        // Cash In 
        public int CashInType = 1;
        public int CashInQuantity = 0;
        public decimal CashInTotalAmount = 0.00m;
        // Cash Out
        public int CashOutType = 2;
        public int CashOutQuantity = 0;
        public decimal CashOutTotalAmount = 0.00m;
        // Payments
        public List<PaymentInfo> Payments = null;
    }
    
    [Serializable]
    public class Product
    {
        public int PluNo = 0;
        public string Barcode = "";
        public string Name = "";
        public decimal Price = 0.00m;
        public int Department = 0;
        public int Weighable = 0;
        public int SubCategory = 0;
    }
    public class FiscalPrinter : IServerPrinter
    {
        #region IFiscalPrinter Members

        public event EventHandler BeforeZReport;
        public event EventHandler AfterZReport;
        public event EventHandler DocumentRequested;
        public event EventHandler DateTimeChanged;
        public event OnMessageHandler OnMessage;

        private static IFiscalPrinter printer = null, slipPrinter, receiptPrinter;
        private static InvoicePage invoicePage = null;
        private static ISalesDocument document = null;
        private static ICashier cashier = null;
        private static Encoding defaultEncoding = null;
        private static IConnection connection = null;
        private static int lineNumber = 0;
        private static bool isFiscal = false;
        private static string registerId = "001";
        private static int port = 4444;
        private static Dictionary<int, Product> products = null;
        private static string productFileName = Directory.GetCurrentDirectory() + "/Product.xml";
        private static string newProductFileName = Directory.GetCurrentDirectory() + "/Product_New.xml";

        private const string currentLog = "CurrentDocument.gui";

        protected static string CurrentEJId = String.Empty;

        protected const int PRINTER_LINE_LENGTH = 48;
        protected const int LOGO_LINE_LENGTH_300 = 48;
        protected const int LOGO_LINE_LENGTH_202 = 32;
        protected const int MAX_LOGO_LINE = 6;
        protected const int MAX_END_OF_RECEIPT_LINE = 6;

        protected const int MAX_CREDIT_VX675 = 4;
        protected const int MAX_CREDIT = 8;

        protected const int MAX_FCURRENCY = 4;
        protected const int AMOUNT_LENGTH = 3;
        protected const int MAX_LINE_PER_RECEIPT = 250;

        protected const int MAX_REPEAT_RESPONSE = 3;
        protected const int FPU_TIMEOUT = 8000;
        protected const int REPORT_TIMEOUT = 60000;
 
        private static byte[] tripleKey = null;
        private static ICompactPrinter compactPrinter = null;
        private static List<String> reportLines = null;

        internal static bool isAfterInvalidPayment = false;

        internal static InvoicePage InvoicePage
        {
            get { return invoicePage; }
            set { invoicePage = value; }
        }

        internal static ICompactPrinter CompactPrinter
        {
            get { return compactPrinter; }
        }

        internal static int LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }


        public static IFiscalPrinter Printer
        {
            get
            {

                if (printer == null)
                {
                    RegisterId = PosConfiguration.Get("RegisterId");

                    if (PosConfiguration.IsPrinterGUIActive)
                    {
                        printer = Hugin.POS.Printer.GUI.FiscalPrinter.Printer;
                    }
                    else
                    {
                        receiptPrinter = new ReceiptPrinter();

                        if (PosConfiguration.Get("SlipComPort") != "")
                        {
                            slipPrinter = new SlipPrinter();
                        }

                        printer = receiptPrinter;
                    }
                }
                return printer;
            }
        }

        public void CloseConnection()
        {
            connection.Close();
        }

        public bool ConnectionIsOpen()
        {
            return connection.IsOpen;
        }

        public void Connect()
        {
            try
            {
                if (connection != null && connection.IsOpen)
                {
                    connection.Close();
                }
                string address = PosConfiguration.Get("PrinterAddress");
                if (!String.IsNullOrEmpty(address))
                {
                    string[] splitAddr = Str.Split(address, ':');

                    if (!int.TryParse(splitAddr[1], out port))
                    {
                        port = 4444;
                    }
                    connection = new TCPConnection(splitAddr[0], port);
                }
#if !MONO
                else
                {
                    address = PosConfiguration.Get("PrinterComPort");
                    if (!String.IsNullOrEmpty(address))
                    {
                        int baudRate = 115200;
                        string[] splitAddr = Str.Split(address, ',');

                        if (splitAddr.Length > 1)
                        {
                            if (!int.TryParse(splitAddr[1], out baudRate))
                            {
                                baudRate = 115200;
                            }
                        }

                        connection = new SerialConnection(splitAddr[0], baudRate);
                    }
                }
#endif
                if (connection == null)
                {
                    throw new Exception(PosMessage.PRINTER_CONNETTION_ERROR);
                }

                connection.Open();
                FiscalPrinter.Log.Debug("Connected successfully");

                try
                {
                    ((SlipPrinter)slipPrinter).Connect(PosConfiguration.Get("SlipComPort").Split(','));
                    slipPrinter.OnMessage += this.OnMessage;
                }
                catch (Exception)
                {
                    //slipPrinter = null;
                }
            }
            catch (Exception ex)
            {
                throw new PrinterException(PosMessage.CANNOT_ACCESS_PRINTER, ex);
            }

            //TODO: if it is new FM, force Start FM command

            OnPrinterMessage(PosMessage.WAIT_FOR_MATCHING);
            // Match with external device
            MatchWithFPU();

            Log.Success("Matched Successfully");
            OnPrinterMessage(PosMessage.MATCHED_SUCCESSFULL);
            System.Threading.Thread.Sleep(50);
            // Fiscal Status
            try
            {
                CheckPrinterStatus();
                GetLastDocumentInfo(true);
                isFiscal = true;
            }
            catch (ServiceRequiredException sre)
            {
                throw sre;
            }
            catch (EcrNonFiscalException)
            {
                isFiscal = false;
            }
            catch (NoProperZFound)
            {
                isFiscal = true;
            }
            catch (FMNewException fmne)
            {
                Log.Error(fmne);
                throw fmne;
            }
            catch (ZRequiredException zre)
            {
                Log.Error(zre);
                throw zre;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            try
            {
                int docId = CurrentDocumentId;
            }
            catch (CmdSequenceException)
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        void OnPrinterMessage(String message)
        {
            if (this.OnMessage != null)
            {
                OnMessage(this, new OnMessageEventArgs(message));
            }
        }

        private bool CheckItemCount()
        {
            if (lineNumber >= MAX_LINE_PER_RECEIPT)
            {
                throw new ReceiptRowCountExceedException();
            }
            return true;
        }

        public void UpdateProducts()
        {
            products = new Dictionary<int, Product>();
            if (File.Exists(productFileName))
            {
                LoadProductFile();
            }

            List<IProduct> productList = DataConnector.SearchProductByName("");// Get all
            List<IProduct> updateList = new List<IProduct>();

            for (int i = 0; i < productList.Count; i++)
            {
                if (products.ContainsKey(productList[i].Id))
                {
                    Product p = products[productList[i].Id];
                    int weighable = productList[i].Status == ProductStatus.Weighable ? 1 : 0;
                    if (p.Name != productList[i].Name ||
                        p.Barcode != productList[i].Barcode ||
                        p.Department != productList[i].Department.TaxGroupId ||
                        p.SubCategory != productList[i].Department.Id ||
                        p.Weighable != weighable)
                    {
                        updateList.Add(productList[i]);
                    }
                }
                else
                {
                    updateList.Add(productList[i]);
                }
            }
            if (updateList.Count > 0)
            {
                OnPrinterMessage(PosMessage.PLEASE_WAIT + "\nÜRÜN GÜNCELLEME");
            }
            int success = 0;
            foreach (IProduct p in updateList)
            {
                try
                {
                    SendProduct(p);
                    success++;

                    Product prd = new Product();
                    prd.PluNo = p.Id;
                    prd.Name = p.Name;
                    prd.Barcode = p.Barcode;
                    prd.Price = p.UnitPrice;
                    prd.Barcode = p.Barcode;
                    prd.Department = p.Department.TaxGroupId;
                    prd.SubCategory = p.Department.Id;
                    prd.Weighable = p.Status == ProductStatus.Weighable ? 1 : 0;

                    if (products.ContainsKey(p.Id))
                    {
                        products[p.Id] = prd;
                    }
                    else
                    {
                        products.Add(prd.PluNo, prd);
                    }

                }
                catch (InvalidOperationException ioe)
                {
                    Log.Error(String.Format("Error occur on saving Product in FPU. ProductID:{0} \n Hata : {1}",
                                            p.Id,
                                            ioe.Message));
                }
                catch (ParameterException pe)
                {
                    Log.Error(String.Format("Error occur on saving Product in FPU. ProductID:{0} \n Hata : {1}",
                                            p.Id,
                                            pe.Message));
                }
            }
            if (updateList.Count > 0)
            {
                SaveProductToFile();

                OnPrinterMessage(String.Format("YÜKELEN PLU:{0,-6}\nBAÞARISIZ:{1,-6}",
                                    success,
                                    updateList.Count - success));
            }
        }

        private void SaveProductToFile()
        {
            //create the serialiser to create the xml
            XmlSerializer serialiser = new XmlSerializer(typeof(List<Product>));

            // Create the TextWriter for the serialiser to use
            TextWriter Filestream = new StreamWriter(newProductFileName);

            //write to the file
            serialiser.Serialize(Filestream, new List<Product>(products.Values));

            // Close the file
            Filestream.Close();

            if (File.Exists(productFileName))
            {
                File.Delete(productFileName);
            }

            File.Move(newProductFileName, productFileName);
        }


        private void LoadProductFile()
        {
            List<Product> data = new List<Product>();
            using (StreamReader stringReader = new StreamReader(productFileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Product>));
                data = ((List<Product>)serializer.Deserialize(stringReader));
            }

            foreach (Product p in data)
            {
                products.Add(p.PluNo, p);
            }
        }

        public static T FromXML<T>(string xml)
        {
            using (StringReader stringReader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
        }

        public string ToXML<T>(T obj)
        {
            using (StringWriter stringWriter = new StringWriter(new StringBuilder()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }
        private IPrinterResponse SendProduct(IProduct p)
        {
            List<byte> bytes = new List<byte>();
            //PRODUCT ID 
            int pluNo = Convert.ToInt32(p.Id);

            //NAME
            String pluName = p.Name;

            //DEPT ID
            int deptId = p.Department.TaxGroupId + 1;

            //PRICE
            decimal price = p.UnitPrice;

            //WEIGHABLE            
            int weighable = 0;
            if (p.Status == ProductStatus.Weighable)
            {
                weighable = 1;
            }

            //BARCODE
            String barcode = p.Barcode;

            //SUB CAT ID
            int subCatId = p.Department.Id;

            // Send Command
            return new CPResponse(compactPrinter.SaveProduct(pluNo, pluName, deptId, price, weighable, barcode, subCatId));
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
        private string GetIPAddress()
        {

            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }


        private string GetMBId()
        {
            String motherBoard = "MB" + PosConfiguration.Get("FiscalId");
#if !MONO
            System.Management.ManagementObjectSearcher mos =
                new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            System.Management.ManagementObjectCollection moc = mos.Get();

            foreach (System.Management.ManagementObject mo in moc)
            {
                motherBoard = (string)mo["SerialNumber"];
            }
#endif

            return motherBoard;
        }

        void compactPrinter_OnReportLine(object sender, OnReportLineEventArgs e)
        {
            if (reportLines == null)
                reportLines = new List<string>();

            reportLines.Add(e.Line);
        }

        private void MatchWithFPU()
        {
            // Matching 
            try
            {
                DeviceInfo serverInfo = new DeviceInfo();
                serverInfo.Brand = "HUGIN"; //System.Environment.MachineName;
                serverInfo.IP = System.Net.IPAddress.Parse(GetIPAddress());
                serverInfo.IPProtocol = IPProtocol.IPV4;
                serverInfo.Model = "HUGIN COMPACT"; //CreateMD5(GetMBId()).Substring(0, 8);// "FP300";
                serverInfo.Port = Convert.ToInt32(port);
                serverInfo.TerminalNo = FiscalRegisterNo.PadLeft(8, '0');
                serverInfo.Version = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).LastWriteTime.ToShortDateString();
                serverInfo.SerialNum = CreateMD5(GetMBId()).Substring(0, 8);

                if (connection.IsOpen)
                {
                    bool retVal = false;
                    try
                    {
                        compactPrinter = new CompactPrinter.FP300.CompactPrinter();
                        compactPrinter.FiscalRegisterNo = FiscalRegisterNo;
                        compactPrinter.LogerLevel = PosConfiguration.SCLoggerLevel;
                        if (!String.IsNullOrEmpty(PosConfiguration.SCLogDirectory))
                            compactPrinter.LogDirectory = PosConfiguration.SCLogDirectory;
                        retVal = compactPrinter.Connect(connection.ToObject(), serverInfo);

                        if (retVal != true)
                            throw new ExternalDevMatchException();

                        compactPrinter.OnReportLine += new OnReportLineHandler(compactPrinter_OnReportLine);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            catch
            {
                throw new ExternalDevMatchException();
            }
            finally
            {
                connection.FPUTimeout = FPU_TIMEOUT;
            }
        }

        internal static IDataConnector DataConnector
        {
            get
            {
                return Data.Connector.Instance();
            }
        }
        protected static ISettings CurrentSettings
        {
            get { return Data.Connector.Instance().CurrentSettings; }
        }
        internal static string FiscalRegisterNo
        {
            get { return PosConfiguration.Get("FiscalId"); }
        }
        internal static Encoding DefaultEncoding
        {
            get
            {
                try
                {
                    if (defaultEncoding == null)
                    {
                        defaultEncoding = SetDefaultCodePage();
                    }
                }
                catch
                {
                }
                return defaultEncoding;
            }
            set
            {
                try
                {
                    defaultEncoding = value;
                }
                catch
                {
                    SetDefaultCodePage();
                }
            }
        }

        private static Encoding SetDefaultCodePage()
        {
            int[] defaultPageCodes = new int[] { 1254, 857, 1252 };
            foreach (int pageCode in defaultPageCodes)
            {
                try
                {
                    defaultEncoding = Encoding.GetEncoding(pageCode);
                    break;
                }
                catch
                {
                }
            }
            if (defaultEncoding == null)
                defaultEncoding = Encoding.ASCII;

            return defaultEncoding;
        }
        internal static string RegisterId
        {
            get { return registerId; }
            set { registerId = value; }
        }
        public static ISalesDocument Document
        {
            get { return document; }
            set { document = value; }
        }
        internal static ICashier Cashier
        {
            get { return cashier; }
            set { cashier = value; }
        }
        public static byte[] TripleKey
        {
            get { return tripleKey; }
        }
        public IPrinterResponse Feed()
        {
            IPrinterResponse response = null;
            return response;
        }

        public IPrinterResponse CutPaper()
        {
            IPrinterResponse response = null;
            return response;
        }

        public IPrinterResponse EnterProgramMode()
        {
            IPrinterResponse response = null;
            return response;
        }

        public IPrinterResponse ExitProgramMode()
        {
            IPrinterResponse response = null;
            return response;
        }

        public IPrinterResponse LoadGraphicLogo(string filePath)
        {
            return new CPResponse(compactPrinter.LoadGraphicLogo(Image.FromFile(filePath)));
        }
        public bool HasSameLogo(String[] userLogo)
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
                CPResponse response = null;
                String[] logoLines = new string[MAX_LOGO_LINE];
                for (int i = 0; i < MAX_LOGO_LINE; i++)
                {
                    response = new CPResponse(compactPrinter.GetLogo(i));

                    string tmpLogo = response.GetNextParam();
                    logoLines[i] = tmpLogo.Split('\0')[0];
                }
                return logoLines;
            }
            set
            {
                string[] logoLines = value;
                for (int i = 0; i < logoLines.Length; i++)
                {
                    String logoLine = logoLines[i].Trim();

                    //if (logoLines[i].Length != PRINTER_LINE_LENGTH)
                    //{
                    //    int remain = PRINTER_LINE_LENGTH - logoLines[i].Length;
                    //    int leftside = (int)(remain / 2);
                    //    logoLine = logoLines[i].Insert(0, "".PadLeft(leftside, ' '));
                    //    logoLine = logoLine.Insert(logoLine.Length, "".PadRight(remain-leftside, ' '));

                    //}

                    CPResponse response = new CPResponse(compactPrinter.SetLogo(i, logoLine));
                }
            }
        }

        public bool IsVx675
        {
            get
            {
                return compactPrinter.IsVx675;
            }
        }

        public string[] EndOfReceiptNote
        {
            get
            {
                CPResponse response = null;
                String[] lines = new string[MAX_END_OF_RECEIPT_LINE];

                for (int i = 1; i <= MAX_END_OF_RECEIPT_LINE; i++)
                {
                    response = new CPResponse(compactPrinter.GetEndOfReceiptNote(i));

                    string tmpLine = response.GetNextParam();
                    lines[i-1] = tmpLine.Split('\0')[0];
                }
                return lines;
            }

            set
            {
                CPResponse response = null;
                string[] lines = value;

                for(int i = 0; i< MAX_END_OF_RECEIPT_LINE; i++)
                {
                    if (i < lines.Length)
                    {
                        string line = lines[i].Trim();
                        response = new CPResponse(compactPrinter.SetEndOfReceiptNote(i + 1, line));
                    }
                    else
                        response = new CPResponse(compactPrinter.SetEndOfReceiptNote(i+1, " "));
                }
            }
        }

        public int AutoCutter
        {
            get
            {
                try
                {
                    String value = GetPrmOptions(FPProgramOptions.CUTTER);
                    return int.Parse(value, System.Globalization.NumberStyles.Integer);
                }
                catch (System.Exception)
                {
                    return 0;
                }
            }
            set
            {
                if (value != this.AutoCutter)
                {
                    SavePrgrmOptions(FPProgramOptions.CUTTER, Convert.ToString(value));
                }
            }
        }

        public int GraphicLogoActive
        {
            get
            {
                try
                {
                    String value = GetPrmOptions(FPProgramOptions.GRAPHIC_LOGO);
                    return int.Parse(value, System.Globalization.NumberStyles.Integer);
                }
                catch (System.Exception)
                {
                    return 0;
                }
            }
            set
            {
                if (value != this.GraphicLogoActive)
                {
                    SavePrgrmOptions(FPProgramOptions.GRAPHIC_LOGO, Convert.ToString(value));
                }
            }
        }

        public int ReceiptBarcodeActive
        {
            get
            {
                try
                {
                    String value = GetPrmOptions(FPProgramOptions.RECEIPT_BARCODE);
                    return int.Parse(value, System.Globalization.NumberStyles.Integer);
                }
                catch (System.Exception)
                {
                    return 0;
                }
            }
            set
            {
                if (value != this.ReceiptBarcodeActive)
                {
                    SavePrgrmOptions(FPProgramOptions.RECEIPT_BARCODE, Convert.ToString(value));
                }
            }
        }

        public decimal ReceiptLimit
        {
            get
            {
                try
                {
                    String value = GetPrmOptions(FPProgramOptions.RECEIPT_LIMIT);
                    return Decimal.Parse(value);
                }
                catch (System.Exception)
                {
                    return 0;
                }
            }
            set
            {
                if (value != this.ReceiptLimit)
                {
                    SavePrgrmOptions(FPProgramOptions.RECEIPT_LIMIT, Convert.ToString(value));
                }
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
                {
                    throw new LimitExceededOrZRequiredException();
                }

                DateTime fpuTime = this.DateTime;
                TimeSpan newTimeDelta = fpuTime - value;

                if (Math.Abs(newTimeDelta.TotalHours) > 1)
                    throw new TimeLimitException();

                DateTime lastZDate = LastZReportDate;
                if (value < lastZDate)
                    throw new TimeZReportException();

                DateTime = value;
            }
        }

        public DateTime DateTime
        {
            get
            {
                DateTime dt = DateTime.Now;
                try
                {
                    dt = GetPrinterDate();
                }
                catch { }
                return dt;
            }
            set
            {
                if (InServiceMode)
                {
                    CPResponse response = new CPResponse(compactPrinter.SetDateTime(value, value));
                    SetSystemTime(value);
                }
            }
        }

        public decimal CashAmountInDrawer
        {
            get
            {
                DrawerInfo di = GetDrawerInfo();

                decimal paymentCashTotal = 0;

                foreach (PaymentInfo pi in di.Payments)
                {
                    if (pi.Type == PaymentType.CASH)
                    {
                        paymentCashTotal = pi.PaidTotal;
                        break;
                    }
                }
                if ((CurrentSettings.GetProgramOption(Setting.OpenDrawerOnPayment) & (int)OpenDrawerPaymentType.Cash) == (int)OpenDrawerPaymentType.Cash)
                    OpenDrawer();
                return paymentCashTotal + di.CashInTotalAmount - di.CashOutTotalAmount;
            }
        }

        public bool CurrencyPaymentContains()
        {
            DrawerInfo di = GetDrawerInfo();

            foreach (PaymentInfo pi in di.Payments)
            {
                if (pi.Type == PaymentType.FCURRENCY)
                {
                    return true;
                }
            }

            return false;
        }

        public ISalesDocument SaleDocument
        {
            get { return document; }
            set
            {
                document = value;
            }
        }

        public decimal TotalChange
        {
            get { return 0; }
        }

        private string GetPrmOptions(FPProgramOptions po)
        {
            String prmValue = String.Empty;

            CPResponse response = new CPResponse(compactPrinter.GetProgramOptions((int)(po)));
            prmValue = response.GetNextParam();

            return prmValue;
        }

        private void SavePrgrmOptions(FPProgramOptions po, String value)
        {
            CPResponse response = new CPResponse(compactPrinter.SaveProgramOptions((int)(po), value));
        }

        private DrawerInfo GetDrawerInfo()
        {
            DrawerInfo di = new DrawerInfo();

            String paramVal = String.Empty;

            // Send Command
            CPResponse response = new CPResponse(compactPrinter.GetDrawerInfo());

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // Total document count
                di.TotalDocumentCount = int.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // Total amount
                di.TotalAmount = Decimal.Parse(paramVal);
            }

            // Get total fiscal receipt group data
            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // Total fiscal receipt count
                di.FiscalReceiptCount = int.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // Total fiscal sale amount
                di.FiscalReceiptTotalAmount = Decimal.Parse(paramVal);
            }

            // Get total voided receipt data block
            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // Total voided document count
                di.VoidedDocumentCont = int.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // Total voided amount
                di.VoidedAmount = Decimal.Parse(paramVal);
            }

            // Get discount data block
            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // Discount amount
                di.DiscountTotalAmount = Decimal.Parse(paramVal);
            }

            //Get Collect Data Block
            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // CashIn Type
                di.CashInType = int.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // CashIn Quantity
                di.CashInQuantity = int.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // CashIn Amount
                di.CashInTotalAmount = Decimal.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // CashOut Type
                di.CashOutType = int.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // CashOut Quantity
                di.CashOutQuantity = int.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                // CashOut Amount
                di.CashOutTotalAmount = Decimal.Parse(paramVal);
            }

            // Get payment data block
            di.Payments = new List<PaymentInfo>();
            PaymentInfo pi = new PaymentInfo();

            while (true)
            {
                paramVal = response.GetNextParam();
                if (!String.IsNullOrEmpty(paramVal))
                {
                    // Payment type Cash, Credit, Foreign Currency, Check
                    pi = new PaymentInfo();
                    pi.Type = (PaymentType)int.Parse(paramVal);

                    paramVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(paramVal))
                    {
                        // Sub Payment Index, 1-CreditA, 2-CreditB...
                        pi.Index = int.Parse(paramVal);
                    }
                    paramVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(paramVal))
                    {
                        // Paid total
                        pi.PaidTotal = Decimal.Parse(paramVal);
                        di.Payments.Add(pi);
                    }
                }
                else
                    break;
            }

            return di;
        }

        public PrintedDocumentInfo GetLastDocumentInfo(bool lastZ)
        {
            PrintedDocumentInfo li = new PrintedDocumentInfo();

            CPResponse response = new CPResponse(compactPrinter.GetLastDocumentInfo(lastZ));

            String paramVal = String.Empty;

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                li.DocId = int.Parse(paramVal);
            }
            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                li.ZNo = int.Parse(paramVal);
            }
            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                li.EjNo = int.Parse(paramVal);
            }
            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                li.Type = (ReceiptTypes)int.Parse(paramVal);
            }
            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                li.DocDateTime = DateTime.Parse(paramVal);
            }

            paramVal = response.GetNextParam();
            if (!String.IsNullOrEmpty(paramVal))
            {
                //String time = paramVal.Trim(':');
                li.DocDateTime = li.DocDateTime.AddHours(int.Parse(paramVal.Substring(0, 2)));
                li.DocDateTime = li.DocDateTime.AddMinutes(int.Parse(paramVal.Substring(3, 2)));
            }

            return li;
        }

        private DateTime GetPrinterDate()
        {
            return compactPrinter.GetDateTime();
        }

        public IPrinterResponse Withdraw(Decimal amount)
        {
            if (CashAmountInDrawer < amount) throw new NegativeResultException();

            return new CPResponse(compactPrinter.CashOut(amount));
        }

        public IPrinterResponse Withdraw(Decimal amount, String refNumber)
        {
            throw new Exception(PosMessage.INVALID_OPERATION);
        }

        public IPrinterResponse Withdraw(Decimal amount, ICredit credit)
        {
            throw new Exception(PosMessage.INVALID_OPERATION);
        }

        public IPrinterResponse Deposit(Decimal amount)
        {
            CPResponse response = new CPResponse(compactPrinter.CashIn(amount));

            if ((CurrentSettings.GetProgramOption(Setting.OpenDrawerOnPayment) & (int)OpenDrawerPaymentType.Cash) == (int)OpenDrawerPaymentType.Cash)
                OpenDrawer();

            return response;
        }

        public bool IsFiscal
        {
            get { return isFiscal; }
            set { isFiscal = value; }
        }

        public IPrinterResponse Print(IAdjustment ai)
        {
            if (CheckItemCount())
            {
                lineNumber++;
            }
            return null;
        }

        public void ClearDisplay()
        {
        }
        public IPrinterResponse Correct(IAdjustment ai)
        {
            if (CheckItemCount())
            {
                lineNumber++;
            }
            return null;
        }

        public IPrinterResponse Correct(IFiscalItem fi)
        {
            if (CheckItemCount())
            {
                lineNumber++;
            }
            return null;
        }

        public IPrinterResponse Print(IAdjustment[] ai)
        {

            if (CheckItemCount())
            {
                lineNumber++;
            }
            return null;
        }

        public IPrinterResponse Print(IFiscalItem fi)
        {
            if (CheckItemCount())
            {
                lineNumber++;
            }
            return null;
        }

        public IPrinterResponse PrintTotals(ISalesDocument saleDocument, bool hardcopy)
        {
            CPResponse response = new CPResponse(compactPrinter.CheckPrinterStatus());

            State state = response.FPUState;

            if (state == State.SELLING ||
                state == State.PAYMENT ||
                state == State.SUBTOTAL)
            {

                response = new CPResponse(compactPrinter.PrintSubtotal(false));

                Decimal subtotal = Decimal.Parse(response.GetNextParam());

                //if (subtotal != saleDocument.BalanceDue)
                //{
                //    throw new SubtotalNotMatchException(subtotal - document.BalanceDue);
                //}

            }
            return response;
        }

        public IPrinterResponse PrintSubTotal(ISalesDocument document, bool hardcopy)
        {
            return null;
        }

        public IPrinterResponse PrintRemark(string s)
        {
            return null;
        }

        public IPrinterResponse PrintFooterNotes()
        {
            return null;
        }

        public ICashier SignInCashier(int id, string password)
        {
            CPResponse response = null;
            try
            {
                response = new CPResponse(compactPrinter.SignInCashier(id, password));
            }
            catch (CashierAutorizeException cae)
            {
                throw new MissingCashierException(cae.Message);
            }

            string name = "";
            AuthorizationLevel level = AuthorizationLevel.S;

            // NOTE BLOCK
            name = response.GetParamByIndex(3);

            switch (int.Parse(response.GetParamByIndex(7)))
            {

                case 0:
                    level = AuthorizationLevel.S;
                    break;
                case 1:
                    level = AuthorizationLevel.P;
                    break;
                case 2:
                    level = AuthorizationLevel.Z;
                    break;
                case 3:
                    level = AuthorizationLevel.Z & AuthorizationLevel.P;
                    break;
            }


            cashier = new FPUCashier(id + "", name, password, level);

            try
            {
                DateTime fpuTime = this.Time;
                TimeSpan ts = fpuTime - DateTime.Now;
                if (Math.Abs(ts.TotalSeconds) > 10)
                    SetSystemTime(fpuTime);
            }
            catch (Exception ex)
            {
                FiscalPrinter.Log.Error(String.Format("{0} {1}", "Bilgisayarin saati ayarlanamdi!", ex));
            }

            return cashier;
        }

        public IPrinterResponse SaveCashier(ICashier ch)
        {
            int id      = int.Parse(ch.Id);
            string name = ch.Name.TrimEnd();
            string pass = ch.Password;

            return new CPResponse(compactPrinter.SaveCashier(id, name, pass));
        }

        public IPrinterResponse SaveCashier(int id, string name)
        {
            return new CPResponse(compactPrinter.SaveCashier(id, name.TrimEnd(), String.Empty));
        }

        public IPrinterResponse CheckCashier(int id, string password)
        {
            CPResponse response = null;

            response = new CPResponse(compactPrinter.CheckCashierIsValid(id, password));

            return response;
        }

        public List<String> GetCashiers()
        {
            List<String> cashierList = new List<String>();

            for (int i = 0; i < ProgramConfig.MAX_CASHIER_COUNT; i++)
            {
                try
                {
                    CPResponse response = new CPResponse(compactPrinter.GetCashier(i));

                    if (response.ErrorCode == 0)
                    {
                        String cName = response.GetNextParam();
                        cashierList.Add(cName);
                    }
                }
                catch (System.Exception)
                {

                }
            }

            return cashierList;
        }

        public void SaveCashiers(List<String> nameList, List<int> passwordList)
        {
            for (int i = 0; i < ProgramConfig.MAX_CASHIER_COUNT; i++)
            {
                try
                {

                    if (nameList[i] != null && nameList[i] != "")
                    {
                        String name = nameList[i];

                        //PASSWORD
                        String password = String.Empty;
                        if (passwordList[i] > 0 && passwordList[i] < 1000000)
                        {
                            password = passwordList[i].ToString();
                        }

                        CPResponse response = new CPResponse(compactPrinter.SaveCashier(i, name, password));
                    }
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
        }

        public IPrinterResponse SignOutCashier()
        {
            //StartCurrentLog(3000);
            cashier = null;
            return null;
        }

        public IPrinterResponse Void()
        {
            CPResponse response = new CPResponse(compactPrinter.CheckPrinterStatus());
            if (response.FPUState == State.SELLING || response.FPUState == State.ON_PWR_RCOVR || response.FPUState == State.SUBTOTAL)
            {
                response = new CPResponse(compactPrinter.VoidReceipt());
            }
            return response;
        }

        public IPrinterResponse Suspend()
        {
            document = null;
            return null;
        }

        public IPrinterResponse Void(IAdjustment ai)
        {
            if (CheckItemCount())
            {
                lineNumber++;
            }
            return null;
        }
        public IPrinterResponse Void(IFiscalItem fi)
        {
            if (CheckItemCount())
            {
                lineNumber++;
            }
            return null;
        }

        public decimal PrinterSubTotal
        {
            get
            {
                return 0;
            }
        }

        /*public IPrinterResponse PrintRegisterReport(bool hardcopy)
        {
            throw new Exception("The method or operation is not implemented.");
        }*/

        public IPrinterResponse PrintRegisterReport(DateTime day, bool hardcopy)
        {
#if !WindowsCE
            return PrintRegisterReport(SaleReport.GetSaleReport(day), hardcopy);
#else
            return null;
#endif
        }

        public IPrinterResponse PrintRegisterReport(ICashier cashier, DateTime firstDay, DateTime lastDay, bool hardcopy)
        {
#if !WindowsCE
            return PrintRegisterReport(SaleReport.GetSaleReport(cashier.Id, firstDay, lastDay, false), hardcopy);
#else
            return null;
#endif
        }

        public IPrinterResponse PrintRegisterReportSummary(ICashier cashier, DateTime firstDay, DateTime lastDay, bool hardcopy)
        {
#if !WindowsCE
            return PrintRegisterReport(SaleReport.GetSaleReport(cashier.Id, firstDay, lastDay, true), hardcopy);
#else
            return null;
#endif
        }


        public IPrinterResponse PrintRegisterReport(bool hardcopy)
        {
            return PrintRegisterReport(SaleReport.GetSaleReport(), hardcopy);
        }

        private IPrinterResponse PrintRegisterReport(List<String> registerReport, bool hardcopy)
        {
            IPrinterResponse response = null;

            if (hardcopy)
            {
                response = PrintCustomReport(registerReport.ToArray());

                if (CurrentSettings.GetProgramOption(Setting.OpenDrawerOnPayment) == PosConfiguration.ON)
                    OpenDrawer();
            }
            else
            {
                response = null;
            }

            /*response.Detail = "";
            foreach (String info in registerReport)
                response.Detail += "\n" + info;

            if (FiscalPrinter.HasEJ && !ElectronicJournal.IsNull)
                ElectronicJournal.Instance().AppendDocument();*/

            return response;

        }

        public IPrinterResponse PrintProgramReport(bool hardcopy)
        {
            IPrinterResponse response = null;

            List<String> programReport = ProgramReport.GetProgramReport();

            if (hardcopy)
            {
                response = PrintCustomReport(programReport.ToArray());
            }
            //response.Detail = "";
            //foreach (String info in programReport)
            //    response.Detail += "\n" + info;

            return response;
        }

        private List<string> GetSpecialReport(string header)
        {
            List<string> content = new List<string>();

            return content;
        }

        public IPrinterResponse PrepareDailyReport()
        {
            return null;
        }

        public IPrinterResponse PrintCustomReport(string[] reportText)
        {
            StartCustomReport(7);

            return PrintCustomLines(reportText);
        }

        public IPrinterResponse PrintCustomReceipt(string[] reportText)
        {
            StartCustomReport(3);

            return PrintCustomLines(reportText);
        }

        private IPrinterResponse StartCustomReport(int documentType)
        {
            return new CPResponse(compactPrinter.StartNFDocument(documentType));
        }

        private IPrinterResponse PrintCustomLines(string[] reportText)
        {
            CPResponse response = new CPResponse(compactPrinter.WriteNFLine(reportText));

            return response;
        }

        private CPResponse SendReport(CPResponse response)
        {
            if (!String.IsNullOrEmpty(response.GetParamByIndex(3)))
            {
                reportLines = new List<string>(response.GetParamByIndex(3).Split('\n'));
                foreach (string line in reportLines)
                {
                    response.AddDetail(line);
                }
            }

            reportLines = null;

            return response;
        }

        public CPResponse WaitForReportContent()
        {

            CPResponse response = null;

            System.Threading.Thread.Sleep(8000);// Wait for printer to ready , 8 seconds

            int tryCount = 0;
            while (tryCount < 60) 
            {
                try
                {


                    response = SendReport(new CPResponse(compactPrinter.GetReportContent()));

                    if (!String.IsNullOrEmpty(response.Detail))
                        break;
                    else
                        System.Threading.Thread.Sleep(500); tryCount++;  // Then if printer not ready , check every 500ms , totally 30seconds

                }
                catch (PowerFailureException pfe)
                {
                    Log.Error(pfe);
                    return response;
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(500); tryCount++;
                    if (response != null)
                    {
                        Log.Error(ex);
                        return response;
                    }

                }
            }
            return response;
        }

        public IPrinterResponse PrintEndDayReport()
        {
            return SendReport(new CPResponse(compactPrinter.PrintEndDayReport()));
        }

        public IPrinterResponse PrintXReport(bool hardcopy)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }

            CPResponse response = new CPResponse(compactPrinter.PrintXReport(copy));
            return WaitForReportContent();
        }


        public IPrinterResponse PrintXPluReport(int firstPLU, int lastPLU, bool hardcopy)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }

            return SendReport(new CPResponse(compactPrinter.PrintXPluReport(firstPLU, lastPLU, copy)));
        }

        public IPrinterResponse PrintSystemInfoReport(bool hardcopy)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }

            return SendReport(new CPResponse(compactPrinter.PrintSystemInfoReport(copy)));
        }

        public IPrinterResponse PrintReceiptTotalReport(bool hardcopy)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }

            return SendReport(new CPResponse(compactPrinter.PrintReceiptTotalReport(copy)));
        }

        public IPrinterResponse PrintZReport()
        {
            if (BeforeZReport != null)
                BeforeZReport(this, new EventArgs());

            CPResponse firstResponse = new CPResponse(compactPrinter.PrintZReport());
            CPResponse response = null;

            response = WaitForReportContent();

            if (CurrentSettings.GetProgramOption(Setting.OpenDrawerOnPayment) == PosConfiguration.ON)
                OpenDrawer();

            if (AfterZReport != null)
                AfterZReport(response, new EventArgs());

            return response;
        }

        public IPrinterResponse PrintZReport(int count, decimal amount, bool isAffectDrawer)
        {
            if (BeforeZReport != null)
                BeforeZReport(this, new EventArgs());
            CPResponse firstResponse = new CPResponse(compactPrinter.PrintZReport(count, amount, isAffectDrawer));
            CPResponse response = null;

            response = WaitForReportContent();

            if (CurrentSettings.GetProgramOption(Setting.OpenDrawerOnPayment) == PosConfiguration.ON)
                OpenDrawer();

            if (AfterZReport != null)
                AfterZReport(response, new EventArgs());

            return response;
        }

        public IPrinterResponse PrintPeriodicReport(int firstZ, int lastZ, bool hardcopy)
        {
            return PrintPeriodicZZReport(firstZ, lastZ, hardcopy, false);
        }
        public IPrinterResponse PrintPeriodicReportDetail(int firstZ, int lastZ, bool hardcopy)
        {
            return PrintPeriodicZZReport(firstZ, lastZ, hardcopy, true);
        }
        private IPrinterResponse PrintPeriodicZZReport(int firstZ, int lastZ, bool hardcopy, bool detail)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }
            return SendReport(new CPResponse(compactPrinter.PrintPeriodicZZReport(firstZ, lastZ, copy, detail)));
        }
        public IPrinterResponse PrintPeriodicReport(DateTime firstDay, DateTime lastDay, bool hardcopy)
        {
            return PrintPeriodicDateReport(firstDay, lastDay, hardcopy, false);
        }
        public IPrinterResponse PrintPeriodicReportDetail(DateTime firstDay, DateTime lastDay, bool hardcopy)
        {
            return PrintPeriodicDateReport(firstDay, lastDay, hardcopy, true);
        }
        private IPrinterResponse PrintPeriodicDateReport(DateTime firstDay, DateTime lastDay, bool hardcopy, bool detail)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }
            return SendReport(new CPResponse(compactPrinter.PrintPeriodicDateReport(firstDay, lastDay, copy, detail)));
        }
        public bool InServiceMode
        {
            get
            {
                bool retVal = false;
                try
                {
                    CPResponse response = new CPResponse(compactPrinter.CheckPrinterStatus());
                    if (response.FPUState == State.IN_SERVICE)
                    {
                        retVal = true;
                    }
                }
                catch (BlockingException)
                {
                }
                return retVal;
            }
        }

        public DateTime LastZReportDate
        {
            get
            {
                DateTime lastZDate = FiscalPrinter.Printer.Time;
                try
                {
                    lastZDate = GetLastDocumentInfo(true).DocDateTime;
                }
                catch (System.Exception)
                {

                }
                return lastZDate;
            }
        }

        public int LastZReportNo
        {
            get
            {
                int lastZNo = 0;
                try
                {
                    lastZNo = GetLastDocumentInfo(true).ZNo;
                }
                catch (System.Exception)
                {

                }
                return lastZNo;
            }
        }
        public IPrinterResponse EnterServiceMode(String password)
        {
            return new CPResponse(compactPrinter.EnterServiceMode(password));
        }

        public IPrinterResponse ExitServiceMode(String password)
        {
            return new CPResponse(compactPrinter.ExitServiceMode(password));
        }

        public IPrinterResponse FormatMemory()
        {
            return new CPResponse(compactPrinter.ClearDailyMemory());
        }

        public IPrinterResponse FactorySettings()
        {
            return new CPResponse(compactPrinter.FactorySettings());
        }

        public IPrinterResponse CloseFM()
        {
            return new CPResponse(compactPrinter.CloseFM());
        }

        public IPrinterResponse SetExDeviceAddress(string tcpIp, int port)
        {
            return new CPResponse(compactPrinter.SetExternalDevAddress(tcpIp, port));
        }

        public IPrinterResponse UpdateFirmware(string tcpIp, int port)
        {
            return new CPResponse(compactPrinter.UpdateFirmware(tcpIp, port));
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
            return new CPResponse(compactPrinter.StartEJ());
        }

        public IPrinterResponse PrintEJSummary()
        {
            return new CPResponse(compactPrinter.PrintEJDetail(3));
        }

        public IPrinterResponse PrintEJDocument(DateTime documentTime, bool hardcopy)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }

            return new CPResponse(compactPrinter.PrintEJPeriodic(documentTime, copy));
        }

        public IPrinterResponse PrintEJDocument(int ZReportId, int docId, bool hardcopy)
        {
            return PrintEJPeriodic(ZReportId, docId, ZReportId, docId, hardcopy);
        }

        public IPrinterResponse PrintEJZReport(int ZReportId)
        {
            return PrintEJPeriodic(ZReportId, 0, ZReportId, 0, true);
        }

        public IPrinterResponse PrintEJPeriodic(int ZStartId, int docStartId, int ZEndId, int docEndId, bool hardcopy)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }

            return new CPResponse(compactPrinter.PrintEJPeriodic(ZStartId, docStartId, ZEndId, docEndId, copy));
        }

        public IPrinterResponse PrintEJPeriodic(DateTime startTime, DateTime endTime, bool hardcopy)
        {
            int copy = 2;
            if (hardcopy)
            {
                copy = 3;
            }

            return new CPResponse(compactPrinter.PrintEJPeriodic(startTime, endTime, copy));
        }

        public IPrinterResponse PrintEJPeriodic(DateTime day, bool hardcopy)
        {
            DateTime dtStart = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            DateTime dtEnd = new DateTime(day.Year, day.Month, day.Day, 23, 59, 0);
            return PrintEJPeriodic(dtStart, dtEnd, hardcopy);
        }

        public IPrinterResponse CheckPrinterStatus()
        {
            return new CPResponse(compactPrinter.CheckPrinterStatus());
        }

        public int CurrentDocumentId
        {
            get
            {
                int currentDocumentId = -1;

                PrintedDocumentInfo li = GetLastDocumentInfo(false);

                CPResponse response = (CPResponse)CheckPrinterStatus();
                if (response.FPUState == State.IDLE || response.FPUState == State.LOGIN)
                    currentDocumentId = li.DocId + 1;
                else
                    currentDocumentId = li.DocId;

                if (li.Type == ReceiptTypes.Z_REPORT)
                {
                    currentDocumentId = 1;
                }
                return currentDocumentId;
            }
        }

        public decimal[] TaxRates
        {
            get
            {
                decimal[] taxRates = new decimal[Department.NUM_TAXGROUPS];
                for (int i = 0; i < taxRates.Length; i++)
                {
                    taxRates[i] = decimal.MinusOne;
                }

                for (int i = 0; i < taxRates.Length; i++)
                {
                    try
                    {
                        CPResponse response = new CPResponse(compactPrinter.GetVATRate(i));

                        if (response.ErrorCode == 0)
                        {
                            taxRates[i] = int.Parse(response.GetNextParam());
                        }
                    }
                    catch { }
                }

                return taxRates;
            }
            set
            {
                CPResponse response = null;
                decimal[] currentTR = TaxRates;


                for (int i = 0; i < Department.NUM_TAXGROUPS; i++)
                {
                    try
                    {
                        if (value[i] != currentTR[i])
                        {
                            // Check conflict on next indexes on ECR
                            for (int j = i; j < currentTR.Length; j++)
                            {
                                if (value[i] == -1)
                                    break;

                                if (value[i] == currentTR[j] && j > i)
                                {
                                    Random rnd = new Random();
                                    int diffRate = rnd.Next(1, 100);
                                    while (Array.IndexOf(currentTR, diffRate) != -1 || Array.IndexOf(value, diffRate) != -1)
                                    {
                                        diffRate = rnd.Next();
                                    }

                                    response = new CPResponse(compactPrinter.SetVATRate(j, diffRate));
                                    if (response.ErrorCode == 0)
                                        currentTR[j] = diffRate;
                                    break;
                                }
                            }

                            if (value[i] == -1)
                                response = new CPResponse(compactPrinter.SetVATRate(i, 100));
                            else
                                response = new CPResponse(compactPrinter.SetVATRate(i, value[i]));
                            currentTR[i] = value[i];
                        }
                    }
                    catch(InvalidVATRateException ivre)
                    {
                        FiscalPrinter.Log.Error("Exception occured. {0}", ivre);
                        currentTR[i] = Decimal.MinusOne;
                    }
                    catch(UndefinedTaxRateException uvre)
                    {
                        throw uvre;
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        Department[] departments = null;
        public Department[] Departments
        {
            get
            {
                departments = new Department[Department.NUM_TAXGROUPS];

                for (int i = 0; i < Department.NUM_TAXGROUPS; i++)
                {
                    CPResponse response = new CPResponse(compactPrinter.GetDepartment(i + 1));

                    Department dept = null;

                    if (response.ErrorCode == 0)
                    {
                        dept = new Department(i + 1, response.GetNextParam());
                        dept.TaxGroupId = int.Parse(response.GetNextParam());
                    }

                    departments[i] = dept;

                }

                return departments;
            }
            set
            {
                for (int i = 0; i < Department.NUM_TAXGROUPS; i++)
                {
                    if (value[i] != null && departments[i] != null)
                    {
                        if (!ComapareDepartmentsIsEqual(value[i], departments[i]))
                        {
                            List<byte> bytes = new List<byte>();

                            //DEPARTMENT NO
                            int deptNo = value[i].Id;

                            //DEPT NAME
                            String deptName = value[i].Name;

                            //VAT GROUP
                            int vatGrId = value[i].TaxGroupId;

                            //PRICE
                            decimal price = 0;

                            //WEIGHABLE
                            int weighable = 0;

                            // Send Department Command
                            CPResponse response = new CPResponse(compactPrinter.SetDepartment(deptNo, deptName, vatGrId, price, weighable));

                        }
                    }
                }
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
                    if (compactPrinter != null && compactPrinter.IsVx675)
                        maxCredit = MAX_CREDIT_VX675;

                    credits = new ICredit[maxCredit];

                    CPResponse response;
                    for (int i = 0; i < maxCredit; i++)
                    {
                        response = new CPResponse(compactPrinter.GetCreditInfo(i));

                        Credit credit = null;

                        if (response.ErrorCode == 0)
                        {
                            credit = new Credit(i + 1, response.GetNextParam(), false, false, false);
                        }

                        credits[i] = credit;
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

                    CPResponse response;
                    for(int i = 0; i < MAX_FCURRENCY; i++)
                    {
                        response = new CPResponse(compactPrinter.GetCurrencyInfo(i));

                        Currency currency = null;

                        if (response.ErrorCode == 0)
                            currency = new Currency(i + 1, response.GetNextParam(), Convert.ToDecimal(response.GetNextParam()));

                        currencies[i] = currency;
                    }
                }
                return currencies;
            }
            set
            {
                SendCurrencyInfo(value);
            }
        }

        private bool ComapareDepartmentsIsEqual(Department department_1, Department department_2)
        {
            bool retVal = true;

            if (department_1 == null && department_2 == null)
            {
                return retVal;
            }
            //if (department_1 == null || department_2 == null)
            //{
            //    return false;
            //}
            if (department_1.Name != department_2.Name)
            {
                retVal = false;
            }
            if (retVal == true && department_1.TaxGroupId != department_2.TaxGroupId)
            {
                retVal = false;
            }

            return retVal;
        }

        List<Category> categories = null;
        public Category[] Category
        {
            get
            {
                if (categories == null || categories.Count == 0)
                {
                    categories = new List<Category>();

                    // Main Categories
                    for (int i = 0; i < ProgramConfig.MAX_MAIN_CATEGORY_COUNT; i++)
                    {
                        try
                        {
                            CPResponse response = new CPResponse(compactPrinter.GetMainCategory(i));

                            if (response.ErrorCode == 0)
                            {
                                Category mc = new Category();
                                mc.MainCatNo = int.Parse(response.GetNextParam());
                                mc.Name = response.GetNextParam();
                                mc.MainCatNo = i + 1;
                                mc.CatNo = 0;
                                categories.Add(mc);
                            }
                        }
                        catch (System.Exception)
                        {
                            FiscalPrinter.Log.Error("Main Categories couldn't get");
                            throw new PrinterStatusException();
                        }
                    }
                    // Sub Categories
                    for (int i = 0; i < 90; i++)
                    {
                        try
                        {
                            CPResponse response = new CPResponse(compactPrinter.GetSubCategory(i));

                            if (response.ErrorCode == 0)
                            {
                                Category sc = new Category();
                                sc.CatNo = int.Parse(response.GetNextParam());
                                sc.Name = response.GetNextParam();
                                sc.MainCatNo = int.Parse(response.GetNextParam());
                                categories.Add(sc);
                            }
                        }
                        catch (System.Exception)
                        {
                            FiscalPrinter.Log.Error("Sub Categories couldn't get");
                            throw new PrinterStatusException();
                        }
                    }
                }
                return categories.ToArray();
            }
            set
            {
                CPResponse response = null;

                try
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (CompareCategory(value[i], categories[i])) continue;

                        if (value[i].CatNo == 0)
                        {
                            response = new CPResponse(compactPrinter.SetMainCategory(value[i].MainCatNo, value[i].Name));
                        }
                        else
                        {
                            response = new CPResponse(compactPrinter.SetSubCategory(value[i].CatNo, value[i].Name, value[i].MainCatNo));
                        }
                    }
                }
                catch (System.Exception)
                {
                    FiscalPrinter.Log.Error("Categories couldn't save");
                    throw new PrinterStatusException();
                }
            }
        }

        private bool CompareCategory(Category obj1, Category obj2)
        {
            bool response = false;

            if (obj1 == null && obj2 == null)
            {
                response = true;
            }
            else if (this != null &&
                obj2 != null &&
                obj1.CatNo == ((Category)obj2).CatNo &&
                obj1.MainCatNo == ((Category)obj2).MainCatNo &&
                obj1.Name == ((Category)obj2).Name)
            {
                response = true;
            }

            return response;
        }

        public IPrinterResponse OpenDrawer()
        {
            CPResponse response = null;
            try
            {
                // SEND COMMAND
                response = new CPResponse(compactPrinter.OpenDrawer());
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public bool DailyMemoryIsEmpty
        {
            get
            {
                return GetLastDocumentInfo(false).Type == ReceiptTypes.Z_REPORT;
            }
        }

        public IPrinterResponse EnterFiscalMode(string password)
        {
            CPResponse response = null;
            int timeout = connection.FPUTimeout;

            try
            {
                connection.FPUTimeout = REPORT_TIMEOUT;
                response = new CPResponse(compactPrinter.Fiscalize(password));
                FiscalPrinter.Log.Debug("Entered fiscal mode");
            }
            finally
            {
                connection.FPUTimeout = timeout;
            }
            try
            {
                GetLastDocumentInfo(true);
                isFiscal = true;
            }
            catch (EcrNonFiscalException)
            {
                isFiscal = false;
            }
            catch (Exception)
            {
                isFiscal = true;
            }

            return response;
        }

        public string FPUPassword
        {
            get { return "200200"; }
        }

        internal static EZLogger Log
        {
            get { return EZLogger.Log; }
        }

        public void SetSystemTime(DateTime dt)
        {
            SYSTEMTIME sysTime = new SYSTEMTIME();
            sysTime.SetDateTime(dt);
            try
            {
                //Microsoft.VisualBasic.DateAndTime.TimeOfDay = dt;

                bool val = ConfigureSystemTime.SetLocalTime(ref sysTime);
                if (!val)
                    throw new Exception("Uygulama izni yetersiz, tarih/saat deðiþtirilemedi");

                if (DateTimeChanged != null)
                    DateTimeChanged(dt, new EventArgs());                
            }
            catch (Exception ex)
            {
                FiscalPrinter.Log.Warning(String.Format("{0} {1}", "PC Time could not be set", ex));
                throw new SystemException(PosMessage.TIME_CHANGED);
            }
        }

        public void AdjustPrinter(ISalesDocument saleDocument)
        {
            if (saleDocument.DocumentTypeId == (int)DocumentTypes.INVOICE
                || saleDocument.DocumentTypeId == (int)DocumentTypes.RETURN_DOCUMENT)
            {
                try
                {
                    ((SlipPrinter)slipPrinter).Connect(PosConfiguration.Get("SlipComPort").Split(','));
                    slipPrinter.OnMessage += this.OnMessage;
                }
                catch (Exception e)
                {
                    //slipPrinter = null;
                }

                if (slipPrinter == null)
                    throw new NoSlipPrinterOnCOM();

                try
                {
                    if (((SlipPrinter)(slipPrinter)).CheckConnection() == false)
                        throw new NoSlipPortException();
                }
                catch { throw new NoSlipPortException(); }

                FiscalPrinter.Document = saleDocument;
                SlipPrinter.Invoicepage = new InvoicePage();
                SlipPrinter.Invoicepage.SubTotal = saleDocument.TotalAmount;
                printer = slipPrinter;
            }
            else
            {
                printer = receiptPrinter;
            }

            document = saleDocument;
        }

        public void ReleasePrinter()
        {
            printer = receiptPrinter;
        }

        public bool CanPrint(ISalesDocument document)
        {
            switch ((DocumentTypes)document.DocumentTypeId)
            {
                case DocumentTypes.RECEIPT:
                case DocumentTypes.INVOICE:
                case DocumentTypes.E_INVOICE:
                case DocumentTypes.E_ARCHIEVE:
                case DocumentTypes.MEAL_TICKET:
                case DocumentTypes.CAR_PARKING:
                case DocumentTypes.ADVANCE:
                case DocumentTypes.COLLECTION_INVOICE:
                case DocumentTypes.RETURN_DOCUMENT:
                case DocumentTypes.CURRENT_ACCOUNT_COLLECTION:
                case DocumentTypes.SELF_EMPLYOMENT_INVOICE:
                    return true;
                default: return false;
            }
        }

        public bool IsCompact
        {
            get { return true; }
        }
        public IPrinterResponse InterruptReport()
        {
            CPResponse response = new CPResponse(compactPrinter.InterruptReport());
            CPResponse responseReportContent = null;

            int tryCount = 0;
            while(tryCount < 3)
            {
                try
                {
                    if (ReceiptTypes.Z_REPORT == GetLastDocumentInfo(false).Type)
                    {
                        responseReportContent = WaitForReportContent();

                        if (AfterZReport != null)
                            AfterZReport(responseReportContent, new EventArgs());
                    }
                    break;
                }
                catch { System.Threading.Thread.Sleep(500); tryCount++; }
            }

            return response;
        }
        public IReport GetReports(bool ejOnly)
        {
            CheckFiscalStatus();

            AuthorizationLevel level = AuthorizationLevel.X;
            IReport reports = new Report("RAPORLAR", "", false, level, ReportGroup.NONE, "");

            level = CurrentSettings.GetAuthorizationLevel(Authorizations.XReport);
            IReport XReports = new Report("X RAPORLARI", "", false, level, ReportGroup.NONE, "");

            level = CurrentSettings.GetAuthorizationLevel(Authorizations.ZReport);
            IReport ZReports = new Report("Z RAPORLARI", "", false, level, ReportGroup.NONE, "");

            level = CurrentSettings.GetAuthorizationLevel(Authorizations.EJReportAuth);
            IReport EJReports = new Report("EKÜ RAPORLARI", "", false, level, ReportGroup.NONE, "");

            if (!ejOnly)
            {
                level = CurrentSettings.GetAuthorizationLevel(Authorizations.ZReport);
                Report z = new Report(PosMessage.Z_REPORT, "PrintZReport", false, level, ReportGroup.Z_REPORTS, "Z_RAPORU");

                Report financialZ = null;
                Report financialDate = null;
                Report financialZDetail = null;
                Report financialDateDetail = null;

                Report receiptTotal = null;

                if (isFiscal)
                {
                    financialZ = new Report(PosMessage.FINANCIAL_BETWEEN_Z, "PrintPeriodicReport", true, level, ReportGroup.Z_REPORTS, "MB_RAPORU_ZZ");
                    financialZ.Parameters.Add(PosMessage.FIRST_Z_NO, typeof(int));
                    financialZ.Parameters.Add(PosMessage.LAST_Z_NO, typeof(int));
                    financialZ.SetInterruptable(true);

                    financialDate = new Report(PosMessage.FINANCIAL_BETWEEN_DATE, "PrintPeriodicReport", true, level, ReportGroup.Z_REPORTS, "MB_RAPORU_TARIH");
                    financialDate.Parameters.Add(PosMessage.FIRST_DATE, typeof(Date));
                    financialDate.Parameters.Add(PosMessage.LAST_DATE, typeof(Date));
                    financialDate.SetInterruptable(true);


                    financialZDetail = new Report(PosMessage.FINANCIAL_BETWEEN_Z_DETAIL, "PrintPeriodicReportDetail", true, level, ReportGroup.Z_REPORTS, "MB_RAPORU_ZZ_DETAY");
                    financialZDetail.Parameters.Add(PosMessage.FIRST_Z_NO, typeof(int));
                    financialZDetail.Parameters.Add(PosMessage.LAST_Z_NO, typeof(int));
                    financialZDetail.SetInterruptable(true);

                    financialDateDetail = new Report(PosMessage.FINANCIAL_BETWEEN_DATE_DETAIL, "PrintPeriodicReportDetail", true, level, ReportGroup.Z_REPORTS, "MB_RAPORU_TARIH_DETAY");
                    financialDateDetail.Parameters.Add(PosMessage.FIRST_DATE, typeof(Date));
                    financialDateDetail.Parameters.Add(PosMessage.LAST_DATE, typeof(Date));
                    financialDateDetail.SetInterruptable(true);

                    receiptTotal = new Report(PosMessage.RECEIPT_TOTAL_REPORT, "PrintReceiptTotalReport", true, level, ReportGroup.Z_REPORTS, "FIS_TOPLAMLARI_RAPORU");
                    receiptTotal.SetInterruptable(true);

                }
                level = CurrentSettings.GetAuthorizationLevel(Authorizations.XReport);
                Report x = new Report(PosMessage.X_REPORT, "PrintXReport", true, level, ReportGroup.X_REPORTS, "X_RAPORU");

                //Report xPlu = new Report(PosMessage.X_PLU_REPORT, "PrintXPluReport", true, level, ReportGroup.X_REPORTS);
                //xPlu.Parameters.Add(PosMessage.FIRST_PLU_NO, typeof(int));
                //xPlu.Parameters.Add(PosMessage.LAST_PLU_NO, typeof(int));
                //xPlu.SetInterruptable(false);

                //Report payment = new Report(PosMessage.PAYMENT_REPORT, "PrintRegisterReport", true, level);

                Report salesCurrent = new Report(PosMessage.PAYMENT_REPORT_CURRENT, "PrintRegisterReport", true, level, ReportGroup.EJ_REPORTS, "EKU_RAPORU");

                Report salesDaily = new Report(PosMessage.PAYMENT_REPORT_DAILY, "PrintRegisterReport", true, level, ReportGroup.EJ_REPORTS, String.Format("EKURAPORU", DateTime.Now));
                salesDaily.Parameters.Add(PosMessage.DATE, typeof(Date));

                Report salesPeriodicDate = new Report(PosMessage.PAYMENT_REPORT_DATE, "PrintRegisterReport", true, level, ReportGroup.EJ_REPORTS, "EKURAPORU");

                Report salesPeriodicDateSummary = new Report(PosMessage.PAYMENT_REPORT_WITH_DETAIL, "PrintRegisterReport", true, level, ReportGroup.EJ_REPORTS, "EKURAPORU");
                salesPeriodicDateSummary.Parameters.Add(PosMessage.CASHIER, typeof(ICashier));
                salesPeriodicDateSummary.Parameters.Add(PosMessage.FIRST_DATE, typeof(DateTime));
                salesPeriodicDateSummary.Parameters.Add(PosMessage.LAST_DATE, typeof(DateTime));

                Report salesPeriodicDateTotal = new Report(PosMessage.PAYMENT_REPORT_JUST_TOTALS, "PrintRegisterReportSummary", true, level, ReportGroup.EJ_REPORTS, "EKURAPORU");
                salesPeriodicDateTotal.Parameters.Add(PosMessage.CASHIER, typeof(ICashier));
                salesPeriodicDateTotal.Parameters.Add(PosMessage.FIRST_DATE, typeof(DateTime));
                salesPeriodicDateTotal.Parameters.Add(PosMessage.LAST_DATE, typeof(DateTime));

                Report sales = new Report(PosMessage.PAYMENT_REPORT, "", false, level, ReportGroup.EJ_REPORTS, "EKURAPORU");

                Report program = new Report(PosMessage.PROGRAM_REPORT, "PrintProgramReport", true, level, ReportGroup.X_REPORTS, "PROGRAM_RAPORU");

                Report systemInfo = new Report(PosMessage.SYSTEM_INFO_REPORT, "PrintSystemInfoReport", true, level, ReportGroup.X_REPORTS, "SISTEM_BILGI_RAPORU");

                Report endDay = new Report(PosMessage.END_DAY_REPORT, "PrintEndDayReport", false, level, ReportGroup.Z_REPORTS, "GUN_SONU_RAPORU");

                endDay.Parent = ZReports;
                z.Parent = ZReports;
                if (isFiscal)
                {
                    financialZ.Parent = ZReports;
                    financialDate.Parent = ZReports;
                    financialZDetail.Parent = ZReports;
                    financialDateDetail.Parent = ZReports;

                    receiptTotal.Parent = ZReports;
                }
                x.Parent = XReports;
                //xPlu.Parent = XReports;
                systemInfo.Parent = XReports;
                
                //payment.Parent = reports;

                salesPeriodicDateSummary.Parent = salesPeriodicDate;
                salesPeriodicDateTotal.Parent = salesPeriodicDate;


                /* Satýþ ve Program bilgi raporlarý */
                //salesCurrent.Parent = sales;
                //salesDaily.Parent = sales;
                //salesPeriodicDate.Parent = sales;
                //sales.Parent = reports;

                //program.Parent = reports;
            }

            if (isFiscal)
            {
                level = CurrentSettings.GetAuthorizationLevel(Authorizations.EJReportAuth);

                Report ejSummary = new Report(PosMessage.EJ_SUMMARY_REPORT, "PrintEJSummary", false, level, ReportGroup.EJ_REPORTS, "");


                Report ejDocZandDocId = new Report(PosMessage.EJ_DOCUMENT_Z_DOCID, "PrintEJDocument", true, level, ReportGroup.EJ_REPORTS, "EKU_BELGE_RAPORU");
                ejDocZandDocId.Parameters.Add(PosMessage.Z_NO, typeof(int));
                ejDocZandDocId.Parameters.Add(PosMessage.DOCUMENT_ID, typeof(int));

                Report ejZ = new Report(PosMessage.EJ_DOCUMENT_ZREPORT, "PrintEJZReport", false, level, ReportGroup.EJ_REPORTS, "EKU_BELGE_RAPORU");
                ejZ.Parameters.Add(PosMessage.Z_NO, typeof(int));

                Report ejDocDateTime = new Report(PosMessage.EJ_DOCUMENT_DATE_TIME, "PrintEJDocument", true, level, ReportGroup.EJ_REPORTS, "EKU_BELGE_RAPORU");
                ejDocDateTime.Parameters.Add(PosMessage.DATE, typeof(DateTime));

                Report ejDocument = new Report(PosMessage.EJ_DOCUMENT_REPORT, "", false, level, ReportGroup.EJ_REPORTS, "EKU_BELGE_RAPORU");
                ejDocZandDocId.Parent = ejDocument;
                ejZ.Parent = ejDocument;
                ejDocDateTime.Parent = ejDocument;

                Report ejPeriodicZ = new Report(PosMessage.EJ_PERIODIC_ZREPORT, "PrintEJPeriodic", true, level, ReportGroup.EJ_REPORTS, "EKU_RAPORU");
                ejPeriodicZ.Parameters.Add(PosMessage.FIRST_Z_NO, typeof(int));
                ejPeriodicZ.Parameters.Add(PosMessage.FIRST_DOCUMENT, typeof(int));
                ejPeriodicZ.Parameters.Add(PosMessage.LAST_Z_NO, typeof(int));
                ejPeriodicZ.Parameters.Add(PosMessage.LAST_DOCUMENT, typeof(int));
                ejPeriodicZ.SetInterruptable(true);

                Report ejPeriodicDate = new Report(PosMessage.EJ_PERIODIC_DATE, "PrintEJPeriodic", true, level, ReportGroup.EJ_REPORTS, "EKU_RAPORU");
                ejPeriodicDate.Parameters.Add(PosMessage.FIRST_DATE, typeof(DateTime));
                ejPeriodicDate.Parameters.Add(PosMessage.LAST_DATE, typeof(DateTime));
                ejPeriodicDate.SetInterruptable(true);

                Report ejPeriodicDaily = new Report(PosMessage.EJ_PERIODIC_DAILY, "PrintEJPeriodic", true, level, ReportGroup.EJ_REPORTS, "EKU_RAPORU");
                ejPeriodicDaily.Parameters.Add(PosMessage.DATE, typeof(Date));
                ejPeriodicDaily.SetInterruptable(true);

                Report ejPeriodic = new Report(PosMessage.EJ_PERIODIC_REPORT, "", false, level, ReportGroup.EJ_REPORTS, "EKU_RAPORU");

                long fiscalId = 0;
                if (Parser.TryLong(PosConfiguration.Get("FiscalId").Substring(2), out fiscalId))
                {
                    if (fiscalId > 10000000 && fiscalId < 10000020)
                    {
                        Report ejSetZLimit = new Report(PosMessage.EJ_LIMIT_SETTING, "SetZLimit", false, level, ReportGroup.EJ_REPORTS, "EKU_RAPORU");
                        ejSetZLimit.Parameters.Add(PosMessage.EJ_LIMIT_SETTING, typeof(int));
                        ejSetZLimit.Parent = EJReports;
                    }
                }

                ejPeriodicZ.Parent = ejPeriodic;
                ejPeriodicDate.Parent = ejPeriodic;
                ejPeriodicDaily.Parent = ejPeriodic;

                ejSummary.Parent = EJReports;
                ejDocument.Parent = EJReports;
                ejPeriodic.Parent = EJReports;

            }

            ZReports.Parent = reports;
            XReports.Parent = reports;
            EJReports.Parent = reports;

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
                if (compactPrinter != null && compactPrinter.IsVx675)
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
            CPResponse response = null;
            String name = "";
            for (int i = 0; i < MaxNumberOfCredits; i++)
            {
                response = new CPResponse(compactPrinter.SetCreditInfo(i, null));

                name = response.GetNextParam();

                if (credits[i] != null && name != credits[i].Name)
                {
                    response = new CPResponse(compactPrinter.SetCreditInfo(i, credits[i].Name));
                }
            }

            return response;
        }

        public IPrinterResponse SendCurrencyInfo(ICurrency[] currencies)
        {
            CPResponse response = null;
            int index = 0;
            foreach (ICurrency currency in currencies)
            {
                try
                {
                    //NAME
                    String name = currency.Name;

                    //PRICE
                    decimal price = currency.ExchangeRate;
;
                    // Send command
                    response = new CPResponse(compactPrinter.SetCurrencyInfo(index, name, price));

                    index++;

                }
                    // 92 hata kodu - 
                    // kasiyer yetkisi yetersiz hatalarý
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            return response;
        }
        public IPrinterResponse StartSlipInfoReceipt(ISalesDocument document)
        {
            CPResponse response = null;
            List<byte> bytes = new List<byte>();

            // Amount
            decimal amount = document.TotalAmount;

            //Send command
            response = new CPResponse(compactPrinter.PrintDocumentHeader(document.Customer.Contact[4], amount, document.DocumentTypeId));
            return response;
        }

        public IPrinterResponse PrintSlipInfoReceipt(ISalesDocument document)
        {
            List<PaymentInfo> payments = GetPayments(Document);
            CPResponse response = null;
            Decimal subTotal = 0.00m;
            Decimal paidTotal = 0.00m;

            foreach (PaymentInfo pi in payments)
            {
                //PAYMENT TYPE
                int paymntType = (int)pi.Type + 1;

                try
                {
                    // SEND COMMAND
                    response = new CPResponse(compactPrinter.PrintPayment(paymntType, pi.Index, pi.PaidTotal));
                }
                catch (PrinterException pe)
                {
                    if (OnMessage != null)
                        OnMessage(this, new OnMessageEventArgs(pe)); ;
                    WaitFixPrinter();
                }

                if (response.ErrorCode == 0)
                {
                    subTotal = Decimal.Parse(response.GetNextParam());

                    paidTotal = Decimal.Parse(response.GetNextParam());
                }
            }
            if (subTotal > paidTotal)
            {
                throw new IncompletePaymentException(subTotal - paidTotal);
            }
            return null;
        }

        public IPrinterResponse EndSlipInfoReceipt()
        {
            CPResponse response = new CPResponse(compactPrinter.CloseNFReceipt());
            return null;
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

        public void SetZLimit(long zLimit)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool HasProperties(PrinterProperties properties)
        {
            return (properties & PrinterProperties.None) == properties; 
        }


        public byte[] SendRawMessage(byte[] messageBuffer)
        {
            //FPUResponse response = Send(new FPURequest(Command.RAW_MESSAGE, messageBuffer));

            //List<byte> respData = new List<byte>(response.Body);

            //// CRC
            //respData.Add(MessageBuilder.CalculateLRC(respData));
            //// All Length
            //int allLen = respData.Count;
            //respData.Insert(0, (byte)(allLen % 256));
            //respData.Insert(0, (byte)(allLen / 256));

            //return respData.ToArray();

            return null;
        }

        public String GetOrderNum()
        {
            String orderNum = "";
            CPResponse response = new CPResponse(compactPrinter.GetServiceCode());

            if (response.ErrorCode == 0)
            {
                orderNum = response.GetNextParam();
            }

            return orderNum;
        }

        public IPrinterResponse PrintLogs(string date)
        {
            int day = int.Parse(date.Substring(0,2));
            int month = int.Parse(date.Substring(2,2));
            int year = int.Parse(date.Substring(4,4));

            return new CPResponse(compactPrinter.PrintLogs(new DateTime(year, month, day)));
        }

        public IPrinterResponse CreateDB()
        {
            return new CPResponse(compactPrinter.CreateDB());
        }

        //private void WriteKeyToFile(byte[] key)
        //{
        //    String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MatchKey.txt");
        //    try
        //    {
        //        System.IO.FileStream fs =
        //           new System.IO.FileStream(path, System.IO.FileMode.Create,
        //                                    System.IO.FileAccess.Write);

        //        fs.Write(key, 0, key.Length);

        //        fs.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Error
        //    }
        //}

        //private byte[] ReadKeyFromFile()
        //{
        //    return File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MatchKey.txt"));                    
        //}

        protected static bool CanOpenDrawer(ISalesDocument document)
        {
            if (compactPrinter.IsVx675)
                return false;
            try
            {
                OpenDrawerPaymentType value = (OpenDrawerPaymentType)CurrentSettings.GetProgramOption(Setting.OpenDrawerOnPayment);

                if (document.GetCashPayments().Length > 0)
                {
                    if (value.CompareTo(value ^ OpenDrawerPaymentType.Cash) > 0) return true;
                }
                if (document.GetCheckPayments().Length > 0)
                {
                    if (value.CompareTo(value ^ OpenDrawerPaymentType.Check) > 0) return true;
                }
                if (document.GetCreditPayments().Length > 0)
                {
                    if (value.CompareTo(value ^ OpenDrawerPaymentType.Credit) > 0) return true;
                }
                if (document.GetCurrencyPayments().Length > 0)
                {
                    if (value.CompareTo(value ^ OpenDrawerPaymentType.ForeignCurrency) > 0) return true;
                }
            }
            catch { }

            return false;
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
                    pi.Index = -1;
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
                pi.SequenceNo = int.Parse(detail[4]);

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

            payments.Sort(delegate(PaymentInfo x, PaymentInfo y)
            {
                return x.SequenceNo.CompareTo(y.SequenceNo);
            });

            return payments;
        }

        public IPrinterResponse WaitFixPrinter()
        {
            CPResponse response = null;
            while (true)
            {
                try
                {
                    response = new CPResponse(compactPrinter.CheckPrinterStatus());
                    break;
                }
                catch (PrinterException pe)
                {
                    if (pe is BlockingException)
                    {
                        throw pe;
                    }
                    if (pe is ClearRequiredException)
                    {
                        try
                        {
                            response = new CPResponse(compactPrinter.InterruptReport());
                            break;
                        }
                        catch (System.Exception)
                        {

                        }
                    }
                    System.Threading.Thread.Sleep(500);
                }
            }
            return response;
        }

        public IPrinterResponse StartFM(int fiscalNumber)
        {
            CPResponse response = null;

            try
            {
                response = new CPResponse(compactPrinter.StartFM(fiscalNumber));
            }
            catch (PrinterException pe)
            {
                throw pe;
            }
            return response;
        }

        public void RefreshConnection()
        {
            try
            {
                if(!connection.IsOpen)
                    this.Connect();
                if (connection.IsOpen)
                {
                    CPResponse response = new CPResponse(compactPrinter.CheckPrinterStatus());

                    if (response.FPUState == State.LOGIN)
                    {
                        SignInCashier(int.Parse(cashier.Id), cashier.Password);
                    }
                    response = new CPResponse(compactPrinter.CheckPrinterStatus());
                    // Elektrik kesintisi
                    if (response.FPUState == State.ON_PWR_RCOVR)
                    {
                        FiscalPrinter.Printer.InterruptReport();
                    }
                }
            }
            catch(Exception ex)
            {
                if (ex.Message == "Invalid Data")
                {
                    //File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MatchKey.txt"));
                    this.Connect();
                    CPResponse response = new CPResponse(compactPrinter.CheckPrinterStatus());
                    // Yeni baðlantý sonrasý login
                    if (response.FPUState == State.LOGIN)
                    {
                        SignInCashier(int.Parse(cashier.Id), cashier.Password);
                    }
                    response = new CPResponse(compactPrinter.CheckPrinterStatus());
                    // Elektrik kesintisi
                    if (response.FPUState == State.ON_PWR_RCOVR)
                    {
                        FiscalPrinter.Printer.InterruptReport();
                    }
                }
                else
                {
                    throw new PrinterException(PosMessage.PRINTER_CONNETTION_ERROR);
                }
            }
        }

        public IPrinterResponse SaveGMPConnectionInfo(string ipVal, int portVal)
        {
            return new CPResponse(compactPrinter.SaveGMPConnectionInfo(ipVal, port));
        }

        public void StartFMTest()
        {
            CPResponse response = new CPResponse(compactPrinter.StartFMTest());
        }

        public void TransferFile(string fileName)
        {
            CPResponse response = new CPResponse(compactPrinter.TransferFile(fileName));
        }

        public void TestGMP(int index)
        {
            CPResponse response = new CPResponse(compactPrinter.SetEJLimit(index));
        }

        public void CheckFiscalStatus()
        {
            if (!isFiscal)
            {
                try
                {
                    GetLastDocumentInfo(true);
                    isFiscal = true;
                }
                catch (EcrNonFiscalException)
                {
                    isFiscal = false;
                }
                catch (Exception)
                {
                    isFiscal = true;
                }
            }
        }

        public IPrinterResponse SaveNetworkSettings(string ip, string subnet, string gateway)
        {
            CPResponse response = new CPResponse(compactPrinter.SaveNetworkSettings(ip, subnet, gateway));
            return response;
        }

        internal static JSONDocument ConvertSalesDocumentToJSONDocument(ISalesDocument salesDoc)
        {
            JSONDocument JSONDoc = new JSONDocument();

            // Fiscal Items
            List<IFiscalItem> soldItems = new List<IFiscalItem>();

            soldItems = salesDoc.Items.FindAll(delegate(IFiscalItem fi)
            {
                return fi.Quantity > fi.VoidQuantity;
            });

            foreach (IFiscalItem fi in soldItems)
            {
                // Add item
                List<byte> productBytes = new List<byte>();

                decimal qtty = fi.Quantity - fi.VoidQuantity;

                decimal totalAdjAmount = 0;
                foreach (string adjOnItem in fi.GetAdjustments())
                {
                    string[] values = adjOnItem.Split('|');
                    totalAdjAmount += decimal.Parse(values[0]);
                }
                decimal unitPrice = fi.UnitPrice;
                if (fi.GetAdjustments().Length > 0 && totalAdjAmount != 0)
                    unitPrice = Math.Round(unitPrice - (totalAdjAmount / qtty), 2);

                JSONItem jsonItem = new JSONItem();
                jsonItem.Id = fi.Product.Id;
                jsonItem.Quantity = qtty;
                jsonItem.Price = unitPrice;
                jsonItem.Name = fi.Product.Name;
                jsonItem.DeptId = fi.Product.Department.Id;
                jsonItem.Status = (int)fi.Product.Status;

                JSONDoc.FiscalItems.Add(jsonItem);

                // Add adjustment on item
                Adjustment adj = new Adjustment();

                totalAdjAmount = 0.0m;
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
                    JSONDoc.FiscalItems[JSONDoc.FiscalItems.Count - 1].Adj = adj;
                }
            }

            // Add adjustment on document subtotal
            List<string> docAdjustments = new List<string>(Document.GetAdjustments());
            if (docAdjustments.Count > 0)
            {
                foreach (String adjLine in docAdjustments)
                {
                    JSONDoc.Adjustments.Add(ParseAdjLine(adjLine));
                }
            }

            // Add payments
            List<PaymentInfo> payments = GetPayments(Document);
            foreach (PaymentInfo pi in payments)
            {
                JSONDoc.Payments.Add(pi);
            }

            // Add Footer Notes
            if (Document.FootNote.Count > 0)
            {
                foreach (string line in Document.FootNote)
                {
                    JSONDoc.FooterNotes.Add(line);
                }
            }

            // Serialize obj to JSON string
            //return Newtonsoft.Json.JsonConvert.SerializeObject(JSONDoc); ;

            return JSONDoc;
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

        public IPrinterResponse GetEFTSlipCopy(int acquierId, int batchNo, int stanNo, int ZNo, int docNo)
        {
            return new CPResponse(compactPrinter.GetEFTSlipCopy(acquierId, batchNo, stanNo, ZNo, docNo));
        }

        public IPrinterResponse VoidEFTPayment(int acquierId, int batchNo, int stanNo)
        {
            return new CPResponse(compactPrinter.VoidEFTPayment(acquierId, batchNo, stanNo));
        }

        public IPrinterResponse RefundEFTPayment(int acquierId)
        {
            return new CPResponse(compactPrinter.RefundEFTPayment(acquierId));
        }

        public IPrinterResponse RefundEFTPayment(int acquierId, decimal amount)
        {
            return new CPResponse(compactPrinter.RefundEFTPayment(acquierId, amount));
        }
    }
}
