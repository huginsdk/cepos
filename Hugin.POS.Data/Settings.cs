using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;

namespace Hugin.POS.Data
{
    class Settings:ISettings
    {
        #region Constants

        internal const string CashierFile = "Kasiyer.dat";
        internal const string CustomerFile = "Cari.dat";
        internal const string DiplomaticCustomerFile = "Diplomat.dat";
        internal const string NewCustomerFile = "Cari";
        internal const string ExchangeRateFile = "Dovizkur.dat";
        internal const string ProductFile = "Urun.dat";
        internal const string SettingsFile = "Program.dat";
        internal const string NewSettingsFile = "Program.new";
        internal const string DefaultSettingsFile = "Program.def";
        internal const string SubtotalPromotionFile = "Promo.dat";
        internal const string ProductPromotionFile = "Prourun.dat";
        internal const string CategoryPromotionFile = "Proreyon.dat";
        internal const string CustomerPointsFile = "Puan.mdb";
        internal const string SerialNumberFile = "Serial.dat";
        internal const String DBRestoreFile = "DBRestore.xfs";
        internal const String BarcodeFile = "Barkod.dat";
        internal const String CategoryFile = "Reyon.dat";

        #endregion Constants

        #region Decleration

        private String fileName = "";

        private String idleMessage = "";

        private String[] logoLines = new String[6];
        private Department[] departments = new Department[Department.NUM_DEPARTMENTS];
        private Decimal[] taxRates = new Decimal[Department.NUM_TAXGROUPS];

        private Decimal receiptLimit = 0;
        private String[] invoiceCoordinates = new String[2];
        private List<String> documentRemarks = new List<string>();
        private AccountingParty supplierInfo = new AccountingParty();

        private Dictionary<int, String> keyChars = new Dictionary<int, string>();
        private Dictionary<int, String> keyLabels = new Dictionary<int, string>();

        private Dictionary<Authorizations, AuthorizationLevel> authorizationLevels = new Dictionary<Authorizations, AuthorizationLevel>();
        private Dictionary<Setting, int> programOptions = new Dictionary<Setting, int>();//option index, option value

        private String quantityBarcodes = "";
        private String totalAmountBarcodes = "";
        private String weightBarcodes = "";
        private Dictionary<String, String> specialBarcode = new Dictionary<String, String>();

        private List<String> creditLines = new List<String>();
        private List<String> pluPageLines = new List<String>();

        public const int REMARK_LINE_LEN = 6;

        #endregion Decleration
               
        
        //loads default settings
        internal Settings()
        {
            this.fileName = Settings.SettingsFile;
            SortedList<string, string> defaultSettings = LoadSettingsFile(PosConfiguration.DataPath + DefaultSettingsFile);

            int i = 0;
            keyChars.Clear();
            keyLabels.Clear();

            foreach (String key in defaultSettings.Keys)
            {
                switch (key[0])
                {
                    case 'S':
                        i = Int32.Parse(key.Substring(1));
                        invoiceCoordinates[i - 1] = defaultSettings[key];
                        break;
                    case 'H':
                        i = Int32.Parse(key.Substring(1)) - 1;
                        //charMatrix[i] = defaultSettings[key].ToCharArray();
                        if (!keyChars.ContainsKey(i))
                            keyChars.Add(i, defaultSettings[key]);
                        break;
                    case 'E':
                        i = Int32.Parse(key.Substring(1)) - 1;
                        //labelMatrix[i] = defaultSettings[key].Split(',');
                        if (!keyLabels.ContainsKey(i))
                            keyLabels.Add(i, defaultSettings[key]);
                        break;
                    case 'A':
                        try
                        {
                            Authorizations auth = (Authorizations)(key[1] - '0');//char '0'=48, so extract it
                            AuthorizationLevel level = (AuthorizationLevel)Enum.Parse(typeof(AuthorizationLevel), defaultSettings[key], true);

                            if (authorizationLevels.ContainsKey(auth))
                                authorizationLevels[auth] = level;
                            else
                                authorizationLevels.Add(auth, level);
                        }
                        catch { }
                        break;
                    case 'P':
                        if (Parser.TryInt(key.Substring(1), out i))
                        {
                            if (programOptions.ContainsKey((Setting)i))
                                programOptions[(Setting)i] = int.Parse(defaultSettings[key]);
                            else
                                programOptions.Add((Setting)i, int.Parse(defaultSettings[key]));
                        }
                        break;
                    default: break;
                }
            }

        }

        internal Settings(String fileName)
        {
            this.fileName = fileName;

            SortedList<string, string> settings = LoadSettingsFile(PosConfiguration.DataPath + fileName);

            //some options may be ignored by user, so set defaults
            CopyDefaultSettings();

            String line = "";
            int i;

            try
            {
                line = "Y";
                receiptLimit = Decimal.Parse(settings["Y"]) / 100;

                line = "V";

                String[] tempLogo = new String[this.logoLines.Length];
                List<decimal> tempTaxGroups = new List<decimal>();
                Department[] tempDepartments = new Department[Department.NUM_DEPARTMENTS];

                // solve V line for tax rates and departments 
                String str = settings["V"].TrimEnd(' ', ',');
                string strCpy = str;
                int groupId = 0;

                for (int tg = 0; tg < Department.NUM_TAXGROUPS; tg++)
                    tempTaxGroups.Add(-1);

                List<string> taxDataList = new List<string>(str.Split(','));
                if (taxDataList.Count > Department.NUM_TAXGROUPS)
                {
                    throw new Exception("Number of tax rates can not be bigger than " + Department.NUM_TAXGROUPS);
                }

                int index = 0;
                int addedIndex = 0;
                foreach (string taxRate in taxDataList)
                {
                    int rate = -1;
                    if (!Parser.TryInt(taxRate, out rate))
                        continue;

                    // Check list exists current rate, if not add new rate
                    if (tempTaxGroups.IndexOf(rate) == -1)
                        tempTaxGroups[addedIndex++] = rate;

                    groupId = tempTaxGroups.IndexOf(rate) + 1;

                    // Department info creation
                    Department d = new Department(index + 1, settings[String.Format("D{0:D2}", index + 1)]);
                    d.TaxGroupId = groupId;
                    tempDepartments[index] = d;
                    index++;
                }

                //Array.Sort(taxRates);
                line = "M";
                PosMessage.WELCOME = String.Format("{0}\n{1}", settings["M1"], settings["M2"]);
                PosMessage.WELCOME_LOCKED = String.Format("{0}\n{1}", settings["M3"], settings["M4"]);
                if (settings.ContainsKey("M5"))
                    idleMessage = settings["M5"].ToString();

                line = "Bx";
                quantityBarcodes = settings["BA"].ToString();
                totalAmountBarcodes = settings["BT"].ToString();
                weightBarcodes = settings["BG"].ToString();

                //Sort the keys and load remaining settings in alpha order

                Dictionary<Setting, int> tempPrgOptions = new Dictionary<Setting, int>();

                String[] remarks = new String[30];

                Dictionary<int, String> tmpKeyLabels = new Dictionary<int, string>();
                Dictionary<int, String> tmpKeyChars = new Dictionary<int, string>();

                foreach (String key in settings.Keys)
                {
                    line = key;
                    switch (key[0])
                    {
                        case 'L':
                            i = Int32.Parse(key.Substring(1));
                            tempLogo[i - 1] = settings[key];
                            break;
                        case 'S':
                            i = Int32.Parse(key.Substring(1));
                            invoiceCoordinates[i - 1] = settings[key];
                            break;
                        case 'K':
                            if (key.StartsWith("KDV")) continue;

                            if (settings[key].Length > 0)
                                creditLines.Add(key + "=" + settings[key]);
                            break;
                        case 'H':
                            i = Int32.Parse(key.Substring(1)) - 1;
                            //charMatrix[i] = settings[key].ToCharArray();
                            if (!tmpKeyChars.ContainsKey(i))
                                tmpKeyChars.Add(i, settings[key]);
                            break;
                        case 'E':
                            i = Int32.Parse(key.Substring(1)) - 1;
                            //labelMatrix[i] = settings[key].Split(',');
                            if (!tmpKeyLabels.ContainsKey(i))
                                tmpKeyLabels.Add(i, settings[key]);
                            break;
                        case 'A':
                            try
                            {
                                if (Parser.TryInt(key.Substring(1), out i))
                                {
                                    Authorizations auth = (Authorizations)(i);
                                    AuthorizationLevel level = (AuthorizationLevel)Enum.Parse(typeof(AuthorizationLevel), settings[key], true);

                                    if (authorizationLevels.ContainsKey(auth))
                                        authorizationLevels[auth] = level;
                                    else
                                        authorizationLevels.Add(auth, level);
                                }
                            }
                            catch { }
                            break;
                        case 'P':
                            if (Parser.TryInt(key.Substring(1), out i))
                            {
                                if (tempPrgOptions.ContainsKey((Setting)i))
                                    tempPrgOptions[(Setting)i] = int.Parse(settings[key]);
                                else
                                    tempPrgOptions.Add((Setting)i, int.Parse(settings[key]));
                            }
                            break;
                        case 'B':
                            Split(key, settings[key]);
                            break;
                        case 'R':
                            i = Int32.Parse(key.Substring(1)) - 1;
                            if (i >= 30)
                            {
                                line = "R";
                                throw new InvalidProgramException("BELGE SONU NOT SAYISI 30'DAN FAZLA OLAMAZ");
                            }
                            remarks.SetValue(settings[key], i);
                            break;
                        case 'C':
                            if (settings[key].Length > 0)
                                pluPageLines.Add(settings[key]);
                            break;
                        case 'T':
                            if (settings[key].Length > 0)
                            {
                                i = Int32.Parse(key.Substring(1));
                                supplierInfo.SetValue((AccountingPartyTag)i, settings[key]);
                            }
                            break;
                    }
                }
                //eliminate blank remarks
                documentRemarks = new List<string>();
                for (int rc = 0; rc < remarks.Length; rc++)
                {
                    if (remarks[rc] != null)
                        documentRemarks.Add(remarks[rc]);
                }

                if(documentRemarks.Count != REMARK_LINE_LEN)
                {
                    if(documentRemarks.Count < REMARK_LINE_LEN)
                    {
                        while (documentRemarks.Count != REMARK_LINE_LEN)
                            documentRemarks.Add("");
                    }
                    else
                    {
                        while (documentRemarks.Count != REMARK_LINE_LEN)
                            documentRemarks.RemoveAt(documentRemarks.Count -1);
                    }
                }

                line = "Copy";

                // No error occured while solving new program so update

                if (tmpKeyChars.Count > 0)
                {
                    keyChars.Clear();

                    foreach (int key in tmpKeyChars.Keys)
                        keyChars.Add(key, tmpKeyChars[key]);
                }
                if (tmpKeyLabels.Count > 0)
                {
                    keyLabels.Clear();

                    foreach (int key in tmpKeyLabels.Keys)
                        keyLabels.Add(key, tmpKeyLabels[key]);
                }

                tempLogo.CopyTo(this.logoLines, 0);
                tempTaxGroups.CopyTo(this.taxRates, 0);
                tempDepartments.CopyTo(this.departments, 0);
                programOptions = tempPrgOptions;
                if (!programOptions.ContainsKey(0))
                    programOptions.Add(0, 63);
            }
            catch (Exception ex)
            {
                EZLogger.Log.Error("Error at '" + line + "' part : " + ex.Message);
                throw new InvalidProgramException("Settings file is invalid", ex);
            }

        }

        private void CopyDefaultSettings()
        {
            if (Connector.DefaultSettings == null) return;

            Settings defaultSettings = (Settings)Connector.DefaultSettings;

            for (int i = 0; i < invoiceCoordinates.Length; i++)
                invoiceCoordinates[i] = defaultSettings.invoiceCoordinates[i];

            keyChars = new Dictionary<int, string>();
            foreach (int key in defaultSettings.keyChars.Keys)
                keyChars.Add(key, defaultSettings.keyChars[key]);

            keyLabels = new Dictionary<int, string>();
            foreach (int key in defaultSettings.keyLabels.Keys)
                keyLabels.Add(key, defaultSettings.keyLabels[key]);

            foreach (Authorizations key in defaultSettings.authorizationLevels.Keys)
                authorizationLevels.Add(key, defaultSettings.authorizationLevels[key]);

            foreach (Setting key in defaultSettings.programOptions.Keys)
                programOptions.Add(key, defaultSettings.programOptions[key]);

        }

        private static SortedList<string, string> LoadSettingsFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException("File not found" + filepath);

            String line = "";
            StreamReader sr = new StreamReader(filepath, PosConfiguration.DefaultEncoding);
            SortedList<string, string> settings = new SortedList<string, string>();

            string[] keyvalue;
            try
            {
                while ((line = @sr.ReadLine()) != null)
                {
                    //Skip trailing blank lines and comments
                    if (line.Trim().Length == 0 || line.StartsWith("'")) continue;
                    keyvalue = line.Split('=');
                    if (keyvalue.Length > 1 & !settings.ContainsKey(keyvalue[0]))
                        settings.Add(keyvalue[0], keyvalue[1].TrimEnd());
                }

                return settings;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        #region ISettings Members

        public string FileName
        {
            get { return fileName; }
        }

        public string IdleMessage
        {
            get { return idleMessage; }
        }

        public string[] LogoLines
        {
            get { return logoLines; }
        }

        public Department[] Departments
        {
            get { return departments; }
        }

        public Decimal[] TaxRates
        {
            get { return taxRates; }
        }

        public String[] CreditLines
        {
            get { return creditLines.ToArray(); }
        }

        public String[] PLUPageLines
        {
            get { return pluPageLines.ToArray(); }
        }

        public decimal ReceiptLimit
        {
            get { return receiptLimit; }
        }
        //i think, invoice coordinates should be more specific
        public string[] InvoiceCoordinates
        {
            get { return invoiceCoordinates; }
        }

        public string[] DocumentRemarks
        {
            get { return documentRemarks.ToArray(); }
        }

        public AccountingParty SupplierInfo
        {
            get { return supplierInfo; }
        }

        public Dictionary<int,string> CharMatrix
        {
            get { return keyChars; }
        }

        public Dictionary<int, string> LabelMatrix
        {
            get { return keyLabels; }
        }

        public AuthorizationLevel GetAuthorizationLevel(Authorizations operation)
        {
            if (authorizationLevels.ContainsKey(operation))
                return authorizationLevels[operation];
            throw new AuthorizitionNotDefinedException();
        }

        public int GetProgramOption(Setting optionId)
        {
            if (programOptions.ContainsKey(optionId))
                return programOptions[optionId];
            return 0;
        }

        public string QuantityBarcodes
        {
            get { return quantityBarcodes; }
        }

        public string TotalAmountBarcodes
        {
            get {return totalAmountBarcodes; }
        }

        public string WeightBarcodes
        {
            get { return weightBarcodes; }
        }

        public string GetSpecialBarcode(string key)
        {
            if (specialBarcode.ContainsKey(key))
                return specialBarcode[key];
            else return "";
        }
        #endregion

        private void Split(String prefix, String data)
        {
            String key = "";
            String value = "";

            for (int i = 0; i < data.Length; i += 4)
            {
                key = data.Substring(i, 2);
                value = String.Format("{0}{1}", prefix, data.Substring(i, 4));
                
                if (specialBarcode != null && value.Length == 6 && value[4] != '0')
                {
                    if (!specialBarcode.ContainsKey(key))
                        specialBarcode.Add(key, value);
                }

            }
        }
    }
}
