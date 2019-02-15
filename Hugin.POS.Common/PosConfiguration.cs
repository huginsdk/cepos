/*
	Settings class
	--	
	Read and write settings to an XML .config file. Does not use 
	ConfigurationSettings.AppSettings since it's not supported on 
	.NET Compact Framework. 
	
	Uses same schema as app.config file. Example:

		<configuration>
			<appSettings>
				<add key="Name" value="Live Oak" />
				<add key="LogEvents" value="True" />
			</appSettings>
		</configuration>	
	
	Default settings file name is the same as app.config, 
	appends .config to the end of the assembly name. Example:
	
		<appname.exe>.config
*/

//#define SPEEDY_NONFISCAL
//#define VIEWAT

using System;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;


namespace Hugin.POS.Common
{
    /// <summary>
    /// Read and write .config settings.
    /// </summary>

    public enum Setting : int
    {
        LogLevel = 0,
        Autocutter = 1,
        AutoCustomerAdjustment = 2,
        PrintProductBarcode = 3,
        OpenDrawerOnPayment = 4,
        PrintSubtTotal = 5,
        PromptCreditInstallments = 6,
        ShowCashierNameOnReceipt = 7,
        ShowFooterNote = 8,
        PrintBarcode = 9,
        MinimumPrice = 10,
        NotPrintCustomerLabels = 11,
        PrintGraphicLogo = 12,
        PointPrice = 13,
        DUMMY_1 = 14,
        BarcodeLineInMainLogFile = 15,
        DisplayHeaderMessageMode = 16,              // Gui display shows cashier message if it is set
        PrintDocumentRemark = 17,                   // Prints remark lines indicating whether the document's type
        DepartmentNameOnly = 18,                   // Use department name instead of product name in updated products
        DUMMY_2 = 19,
        AskInvoiceNumber = 20,                      // Ask Invoice Serial And Order no
        PrintRegisterReportBeforeZReport = 21,      // Give Register Report Automatically after Z Report(uses for Draw Payment Values)
        ConnectPointToCredit = 22,                  // Connect Point Using to Credit or Check Payment
        AddOnlyReturnFile = 23,                     // Add Return Document to only Return(HD) Files
        WriteDocumentID = 24,                       // Write Document ID(BID Line) to Transaction File
        AskDocumentState = 25,                      // Ask document payment state to cashier. if document is open, add "A" character to line of "SON" in main log file.
        PrintSecondaryPricePromotion = 26,          // Prints "#KAZANCINIZ (secondary price - default price)" if secondary price applied
        CustomerCodeOnRemark = 27,                  // Prints customer code at document remark if promotion applied
        AssingCustomerInSelling = 28,               // Alows to assign customer after selling start
        ApplyPromotionReturnDocument = 29,          // Apply promotion to return document 
        CustomerSearch = 30,
        PointValidMonths = 31,
        DUMMY = 32,
        FastPaymentControl = 33,
        AutoLogin = 34,
        DefineBarcodeLabelKeys = 35,
        AskPriceForReturnItems = 36,
        ShowProductImage = 37,                      // Shows product's image.
        PrintReportHardcopy = 38,                   // Prints sale reports hardcopy if report request done by backoffice
        WebServicePromotion =39,                    // Get customer from promotionserver
        ReturnReason = 40,                          // Cashier enters the reason of return document, it is writed in IAN line
        MealVATRates = 41,                          // Only can be sold that has these VAT rates on food document
        PrintInvoicesInternal = 42                  // If you want to print your invoices on internal printer (like FP300..), set this parameter. So your invoices will be redirected to internal printer. Your printer must support to print these invoices.
    }

    public enum Authorizations
    {
        Discount = 0,
        VoidSale = 1,
        Fee = 2,
        VoidDocument = 3,
        XReport = 4,
        ZReport = 5,

        CashInCashOut = 6,
        LabelAmountChanging = 7,
        EJReportAuth = 8,
        SuspendAndRepeatDocAuth = 9,
        AdvanceAndReturnDocAuth = 10,
        InstallOptAuth = 11,
        EFTVoidAndRefundAuth = 12,
        FileOperationsAndTransfer = 13,
        Programing = 14,
        UpdateOperation = 15
    }

    public enum IntegrationMode
    {
        DATECS = 1,
        POS_5200,
        S60D_KEYBOARD
    }

    [Flags]
    public enum DocumentRemarkType
    {
        NoRemark = 0,
        Receipt = 1,
        Invoice = 2,
        ReturnDocument = 4,
        Waybill = 8,
    }

    [Flags]
    public enum OpenDrawerPaymentType
    {
        None = 0,
        Cash = 1,
        ForeignCurrency = 2,
        Credit = 4,
        Check = 8,
    }

    [Flags]
    public enum PosProperties
    {
        None = 0,
        SlipPrinter = 1,
        ExternalDisplay = 2,
        Scale = 4,
        EftPOS = 8,
        ExternalBarcode = 16,
        All = SlipPrinter | ExternalDisplay | Scale | EftPOS | ExternalBarcode
    }

    public static class PosConfiguration
    {

#if SPEEDY_NONFISCAL
        private const PosProperties PROPERTIES = PosProperties.Scale | PosProperties.ExternalBarcode;
#elif VIEWAT
        private const PosProperties PROPERTIES = PosProperties.None;
#else
        private const PosProperties PROPERTIES = PosProperties.All;
#endif

        public const int ON = 1;
        public const int OFF = 0;
        //public const string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static Encoding DefaultEncoding = GetDefEncoding();//Encoding.GetEncoding(1254);
        public static System.Globalization.CultureInfo CultureInfo = GetDefCulture();// new System.Globalization.CultureInfo("tr-tr", false);
        
        private static string settingsFile = "";//will be set by data dll
        
        private static ConfigurationManager config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


        #region Path

        public static Encoding GetDefEncoding()
        {
            try
            {
                return Encoding.GetEncoding(1254);
            }
            catch
            {
                return Encoding.GetEncoding(1252);
            }
        }

        public static System.Globalization.CultureInfo GetDefCulture()
        {
            try
            {
                return new System.Globalization.CultureInfo("tr-tr", false); ;
            }
            catch
            {
                return new System.Globalization.CultureInfo("en-US", false); ;
            }
        }

        public static String ServerUpgradePath
        {
            get { return String.Format("{0}{1}/", PosConfiguration.ServerUploadPath, "Upgrade"); }
        }

        public static String ServerDownloadPath
        {
            get { return String.Format("{0}{1}", PosConfiguration.ServerUploadPath, "Data/"); }
        }

        public static String ServerDataPath
        {
            get
            {
                if (PosConfiguration.Get("OfficeDataPath") == "")
                    return ServerDownloadPath;
                return PosConfiguration.Get("OfficeDataPath");
            }
        }
        public static String ServerReportPath
        {
            get
            {
                if (PosConfiguration.Get("OfficeReportPath") == "")
                    return ServerUploadPath;
                return PosConfiguration.Get("OfficeReportPath");
            }
        }
        public static String ServerControlPath
        {
            get
            {
                if (PosConfiguration.Get("OfficeControlPath") == "")
                    return ServerDownloadPath;
                return PosConfiguration.Get("OfficeControlPath");
            }
        }
        public static String ServerArchivePath
        {
            get
            {
                if (PosConfiguration.Get("OfficeArchivePath") == "")
                    return ServerUploadPath;
                return PosConfiguration.Get("OfficeArchivePath");
            }
        }

        public static String LicenseKey
        {
            get
            {
                return PosConfiguration.Get("LicenseKey");
            }
        }

        public static String DefaultManagerID
        {
            get
            {
                return PosConfiguration.Get("DefaultManagerID");
            }
        }

        public static String DefaultManagerPWD
        {
            get
            {
                return PosConfiguration.Get("DefaultManagerPWD");
            }
        }

        public static String ServerUploadPath
        {
            get { return PosConfiguration.Get("OfficePath"); }
        }

        public static String ServerOrderPath
        {
            get
            {
                if (PosConfiguration.Get("OfficeOrderPath") == "")
                    return ServerDownloadPath;
                return PosConfiguration.Get("OfficeOrderPath");
            }
        }

        public static String ServerTablePath
        {
            get { return PosConfiguration.Get("OfficePath") + "Sipariþ/"; }
        }

        public static int SCLoggerLevel
        {
            get
            {
                String val = PosConfiguration.Get("SCLoggerLevel");
                int retVal = 3;
                try
                {
                    retVal = int.Parse(val);
                }
                catch { }
                return retVal;
            }
        }

        public static string SCLogDirectory
        {
            get
            {
                return PosConfiguration.Get("SCLogDirectory");
            }
        }

        public static bool IsPrinterGUIActive
        {
            get
            {
                if (Convert.ToInt32(PosConfiguration.Get("PrinterGUIActive")) == 1)
                    return true;
                else
                    return false;
            }
        }

        public static int ScreenIdentity
        {
            get
            {
                return int.Parse(PosConfiguration.Get("ScreenIdentity"));
            }
        }

        public static bool EDocumentAutoFill
        {
            get
            {
                try
                {
                    if (Int32.Parse(PosConfiguration.Get("EDocumentAutoFill")) == 0)
                        return false;
                    else
                        return true;
                }
                catch { return false; }
            }
        }

        public static IntegrationMode IntegrationMode
        {
            get
            {
                try
                {
                    return (IntegrationMode)Int32.Parse(PosConfiguration.Get("IntegrationMode"));
                }
                catch
                {
                    return IntegrationMode.DATECS;
                }
            }
        }

        //Local Directory paths
        public static String LocalPath
        {
            get
            {
                if (PosConfiguration.Get("LocalPath") == "")
                    return IOUtil.ProgramDirectory;
                return PosConfiguration.Get("LocalPath");
            }
        }
        
        public static String ArchivePath
        {
            get { return LocalPath + "Arsiv/"; }
        }

        public static String DataPath
        {
            get { return IOUtil.ProgramDirectory + "Data/"; }
        }

        public static String ReportPath
        {
            get { return LocalPath + "Rapor/"; }
        }

        public static String LogPath
        {
            get { return LocalPath + "Log/"; }
        }

        public static String UpgradePath
        {
            get { return IOUtil.ProgramDirectory + "Upgrade/"; }
        }

        public static String ImagePath
        {
            get { return LocalPath + "Image/"; }
        }

		public static String FontPath
		{
			get { return LocalPath + "Fonts/"; }
		}

		public static String GuiPath
		{
			get{ return LocalPath + "Gui/"; }
		}

        public static String OnlineUpdateUri
        {
            get { return PosConfiguration.Get("OnlineUpdateUri"); }
        }

        public static String[] ReferencedAssemlies
        {
            get
            {
                return new String[]{
                                    IOUtil.AssemblyName,
                                    "Hugin.POS.Printer.dll",
                                    "Hugin.POS.Display.dll",
                                    "Hugin.POS.Data.dll",
                                    "Hugin.POS.Common.dll",
                                    "Hugin.POS.PromotionServer.dll",
                                    "Hugin.POS.Security.dll",
                                    "ICSharpCode.SharpZipLib.dll"
                                    };
            }
        }

        public static String BarcodeTerminator
        {
            get { return PosConfiguration.Get("BarcodeTerminator").Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t"); }
        }


        #endregion Path

        public static bool HasProperties(PosProperties properties)
        {
            return (PROPERTIES & properties) == properties;
        }

        public static String SettingsFile
        {
            get { return settingsFile; }
            set { settingsFile=value; }
        }

        public static String Get(String key)
        {
            String s = config.Get(key);
            return (s == null) ? String.Empty : s;
        }

        public static void Set(String key, String value)
        {
            String existingValue = config.Get(key);
            if (existingValue == value) return;
            if (existingValue != "")
                config.Set(key, value);
            else
                config.Add(key, value);
        }

        public static void Set(String key, int value)
        {
            Set(key, value.ToString());
        }

        public static int GetListAutoSelect(Object o)
        {
            String value = Get("ListAutoSelect");
            if (value.Length == 0) return 0;
            string[] keys = value.Split(';');
            string className = o.GetType().Name;
            foreach (string s in keys)
                if (s.StartsWith(className))
                    return int.Parse(s.Split(':')[1]);
            return 0;
        }
    }
}