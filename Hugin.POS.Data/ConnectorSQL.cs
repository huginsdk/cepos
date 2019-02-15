using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;
using System.Net;
#if !Mono
using ICSharpCode.SharpZipLib.Zip;
#endif
using System.Runtime.InteropServices;

namespace Hugin.POS.Data
{
    public class Connector:IDataConnector
    {
        #region Definitions
        private const int MAX_PRODUCT_COUNT = 30000;
        internal const int TEMP_DOCUMENT_STATUS = 9;

        private static ISettings defaultSettings = null;
        private static ISettings currentSettings = null;
        private static ISettings newSettings = null;
        public event EventHandler ProductsUpdated;
        private static int maxCreditCount = 0;
        private static int maxCurrencyCount = 0;
        private static bool isArchiveDirChecked = false;

        private static Dictionary<DataTypes, int> lastSuccess = new Dictionary<DataTypes, int>();
        private static Dictionary<DataTypes, int> lastFail = new Dictionary<DataTypes, int>();

        IPointAdapter pointAdapter = null;

        private static Logger mainLog, voidedDocumentsLog, returnsLog, lastDocumentLog;
        private static object transferLock = null;

        private static IDataConnector connector = null;

        private static ITransferClient fxClient = null;
        #endregion

        #region Accessors
        public static ITransferClient FxClient
        {
            get { return Connector.fxClient; }
        }

        public static IDataConnector Instance()
        {
            if (connector == null)
                connector = new Connector();
            return connector;
        }
        #endregion

        private Connector()
        {
            Uri uri = new Uri(PosConfiguration.ServerDownloadPath);
            if (uri.Scheme == Uri.UriSchemeFile)
            {
                fxClient = new TransferClientFile();
            }
            else if (uri.Scheme == Uri.UriSchemeFtp)
            {
                fxClient = new TransferClientFtp();
            }
            else if (uri.Scheme == Uri.UriSchemeHttp)
            {

            }
        }


        #region Common Functions

        private static void SetSuccess(DataTypes type, int success)
        {
            lastSuccess[type] = success;
        }
        private static void SetFail(DataTypes type, int fail)
        {
            lastFail[type] = fail;
        }
        
        public int GetLastSuccess(DataTypes type)
        {
            if (lastSuccess.ContainsKey(type))
                return lastSuccess[type];
            return 0;
        }
        public int GetLastFail(DataTypes type)
        {
            if (lastFail.ContainsKey(type))
                return lastFail[type];
            return 0;
        }

        #endregion Common Functions

        #region Settings
		
		private static void Log (Exception ex)
		{
			/*
			 * this function is implemented to catch warnings
			 * which caused fraom unreferenced "ex" variables
			 */
		}
        public void LoadSettings()
        {
            try
            {
                int codePage;
                if (Parser.TryInt(PosConfiguration.Get("DataCodePage"), out codePage))
                {
                    try
                    {
                        PosConfiguration.DefaultEncoding = Encoding.GetEncoding(codePage);
                    }
                    catch(Exception ex)
                    {
						Log(ex);
                        EZLogger.Log.Error("DataCodePage deðeri bu platformda desteklenmiyor.");
                    }
                }

                if (!Directory.Exists(PosConfiguration.DataPath)) Directory.CreateDirectory(PosConfiguration.DataPath);
                if (!Directory.Exists(PosConfiguration.ArchivePath)) Directory.CreateDirectory(PosConfiguration.ArchivePath);
                if (!Directory.Exists(PosConfiguration.ReportPath)) Directory.CreateDirectory(PosConfiguration.ReportPath);
                if (!Directory.Exists(PosConfiguration.LogPath)) Directory.CreateDirectory(PosConfiguration.LogPath);
                if (!Directory.Exists(PosConfiguration.UpgradePath)) Directory.CreateDirectory(PosConfiguration.UpgradePath);


                if (fxClient.DirectoryExists(PosConfiguration.ServerDataPath, 3000))
                {
                    Dictionary<String, String> dataFileList = new Dictionary<string, string>();

                    dataFileList.Add(Settings.SettingsFile, PosConfiguration.ServerControlPath);
                    dataFileList.Add(Settings.ExchangeRateFile, PosConfiguration.ServerDataPath);
                    dataFileList.Add(Settings.CashierFile, PosConfiguration.ServerDataPath);
                    dataFileList.Add(Settings.CustomerFile, PosConfiguration.ServerDataPath);
                    dataFileList.Add(Settings.DiplomaticCustomerFile, PosConfiguration.ServerDataPath);
                    //dataFileList.Add(Settings.ProductFile, PosConfiguration.ServerDataPath);
                    dataFileList.Add(Settings.SerialNumberFile, PosConfiguration.ServerDataPath);
                    dataFileList.Add(Settings.CategoryPromotionFile, PosConfiguration.ServerDataPath);
                    dataFileList.Add(Settings.SubtotalPromotionFile, PosConfiguration.ServerDataPath);
                    dataFileList.Add(Settings.ProductPromotionFile, PosConfiguration.ServerDataPath);

                    foreach (string key in dataFileList.Keys)
                    {
                        try
                        {
                            DownloadIfMissingDataFile(key, dataFileList[key]);
                        }
                        catch 
                        {
                            EZLogger.Log.Error("Dosya arka ofisten indirilemedi({0})", dataFileList[key] + key);
                        }
                    }
                }
            }
            catch (FileNotFoundException fnfe)
            {
				Log(fnfe);
            }
            
            try {
                defaultSettings = new Settings();
            }
			catch(Exception ex) { Log(ex);}

            currentSettings = new Settings(Settings.SettingsFile);
            ApplyChanges();
            PosConfiguration.SettingsFile = currentSettings.FileName;
        }
        public ISettings LoadNewSettings()
        {
            newSettings = new Settings(Settings.NewSettingsFile);
            return newSettings;
        }

        public void AcceptNewSettings()
        {
            currentSettings = newSettings;//!check if properties are affected after null operation
            ApplyChanges();
            PosConfiguration.SettingsFile = currentSettings.FileName;
            if (File.Exists(PosConfiguration.DataPath + Settings.SettingsFile))
                File.Delete(PosConfiguration.DataPath + Settings.SettingsFile);
            if (File.Exists(PosConfiguration.DataPath + Settings.NewSettingsFile))
                File.Move(PosConfiguration.DataPath + Settings.NewSettingsFile, PosConfiguration.DataPath + Settings.SettingsFile);
            newSettings = null;
        }

        private static void ApplyChanges()
        {
            Connector.Instance().CurrentSettings.Departments.CopyTo(Department.Departments, 0);
            Connector.Instance().CurrentSettings.TaxRates.CopyTo(Department.TaxRates, 0);
            LoadCredits();
        }

        internal static ISettings DefaultSettings
        {
            get { return defaultSettings; }
        }

        public ISettings NewSettings
        {
            get { return newSettings; }
        }

        public ISettings CurrentSettings
        {
            get { return currentSettings; }
        }

        static void DownloadIfMissingDataFile(String fileName, String serverDirectory)
        {
            if (!File.Exists(PosConfiguration.DataPath + fileName) && File.Exists(serverDirectory + fileName))
                fxClient.DownloadFile(serverDirectory + fileName, PosConfiguration.DataPath + fileName);
        }
        
        #endregion Settings

        #region Product
        
        public IProduct CreateProduct(string name, Department department, decimal price)
        {
            return Product.CreateProduct(name, department, price);
        }

        public ICashier CreateCashier(string name, string id)
        {
            return Cashier.CreateCashier(name, id);
        }

        public ICustomer CreateCustomer(string code, string name, string address, string taxInstitution, string taxNumber)
        {
            return Customer.CreateCustomer(code, name, address, taxInstitution, taxNumber);
        }
 
        public void LoadProducts()
        {
            lastSuccess[DataTypes.Product] = 0;
            lastFail[DataTypes.Product] = 0;

            Product.MIN_UNITPRICE = Convert.ToDecimal(currentSettings.GetProgramOption(Setting.MinimumPrice)) / 100;

            String path = PosConfiguration.DataPath + Settings.ProductFile;
            if (File.Exists(path))
            {
                bool response = false;
                try
                {
                    // Create dumm table
                    response = Product.StartUpdate();
                    // Add products
                    if (response)
                    {
                        ReadProductFile(path);
                    }
                }
                catch (System.Exception ex)
                {
                    EZLogger.Log.Error("Ürün yüklemede hata ile karþýlaþýldý");
                }
                finally
                {
                    // Drop dummy table
                    response = Product.FinalizeUpdate();
                }
                if (response)
                {
                    string dumpFile = PosConfiguration.DataPath + Settings.ProductFile + "_last";
                    try
                    {
                        if (File.Exists(dumpFile))
                        {
                            File.Delete(dumpFile);
                        }
                        File.Move(path, dumpFile);
                    }
                    catch (System.Exception ex)
                    {
                    	
                    }
                }
            }


            string strQuery = String.Format("SELECT * FROM {0}", Product.ProductTable);
            System.Data.DataSet ds = DBAdapter.Instance().GetDataSet(strQuery);

            lastSuccess[DataTypes.Product] = ds.Tables[0].Rows.Count;
            EZLogger.Log.Info("Veritabanýnda bulunan ürün satýr sayýsý: {0}", ds.Tables[0].Rows);
            
            if (ProductsUpdated != null)
                ProductsUpdated(null, new EventArgs());

            EZLogger.Log.Info("Ürün yükleme bilgisi : Baþarýlý {0} Baþarýsýz {1}",
                lastSuccess[DataTypes.Product], lastFail[DataTypes.Product]);
        }
        public void UpdateProducts()
        {
            lastSuccess[DataTypes.Product] = 0;
            lastFail[DataTypes.Product] = 0;

            String path = PosConfiguration.DataPath + "Update" + Settings.ProductFile;
            if (!File.Exists(path))
            {
                EZLogger.Log.Error("Ürün güncelleme dosyasý bulunamadý");
                return;
            }

            // Reads product file, parses lines by Parse(...) function, updates productBarcodes, productNames, productLabels
            EZLogger.Log.Info("ÜRÜN DOSYASI GÜNCELLENÝYOR");
            ReadProductFile(path);

            if (ProductsUpdated != null)
                ProductsUpdated(null, new EventArgs());

            EZLogger.Log.Info("Ürün güncelleme bilgisi : Baþarýlý {0} Baþarýsýz {1}",
                lastSuccess[DataTypes.Product], lastFail[DataTypes.Product]);
        }

        private static void ReadProductFile(string path)
        {
            int successCount = 0;
            int failCount = 0;
            Product.LoadProductFileToDB(path, out successCount, out failCount);
            lastSuccess[DataTypes.Product] = successCount;
            lastFail[DataTypes.Product] = failCount;
        }

        public void AddToStock(String barcode, decimal quantity)
        {
            String localPath = PosConfiguration.ArchivePath + String.Format("SAYIM{0:ddMMyyyy}.{1}", DateTime.Now, PosConfiguration.Get("RegisterId"));
            String transferPath = PosConfiguration.ArchivePath + String.Format("SAYIM{0:ddMMyyyy}.xfr", DateTime.Now);

            //Add to local file
            IOUtil.AppendAllText(localPath, String.Format("{0},{1}\r\n",
                                                    barcode.PadRight(20, ' '),
                                                    quantity.ToString().Replace(',', '.').PadLeft(10, '0')));
            //Add to transfer file
            lock (TransferLock)
            {
                IOUtil.AppendAllText(transferPath, String.Format("{0},{1}\r\n",
                                                    barcode.PadLeft(20, '0'),
                                                    quantity.ToString().PadLeft(10, '0')));
            }

        }

        public IProduct FindProductByName(String name)
        {
            return Product.FindByName(name);
        }
        public IProduct FindProductByBarcode(String barcode)
        {
            return Product.FindByBarcode(barcode);
        }
        public IProduct FindProductByLabel(String plu)
        {
            return Product.FindByLabel(plu);
        }
        public List<IProduct> SearchProductByBarcode(String[] barcodeData)
        {
            return Product.SearchProductByBarcode(barcodeData);
        }
        public List<IProduct> SearchProductByLabel(String[] pluList)
        {
            return Product.SearchProductByLabel(pluList);
        }
        public List<IProduct> SearchProductByName(String nameData)
        {
            return Product.SearchProductByName(nameData);
        }

        #endregion Product

        #region Currency
        public int MaxCurrencyCount
        {
            get
            {
                return maxCurrencyCount;
            }
            set
            {
                maxCurrencyCount = value;
            }
        }

        public void LoadCurrencies()
        {
            lastSuccess[DataTypes.Currency] = 0;
            lastFail[DataTypes.Currency] = 0;
            String path = PosConfiguration.DataPath + Settings.ExchangeRateFile;
            if (!File.Exists(path))
            {
                EZLogger.Log.Error("Döviz dosyasý bulunamadý");
                return;
            }

            Currency.Backup();

            StreamReader sr = new StreamReader(path, PosConfiguration.DefaultEncoding);
            String line = "";
            while ((line = @sr.ReadLine()) != null)
            {
                //Skip trailing blank lines and comments
                if (line.Trim().Length == 0 || line.StartsWith("'")) continue;
                if (maxCurrencyCount!=0 && lastSuccess[DataTypes.Currency] >= maxCurrencyCount)
                    break;
                if (Currency.Add(line))
                    lastSuccess[DataTypes.Currency]++;
                else
                    lastFail[DataTypes.Currency]++;
            }
            sr.Close();

            Currency.Restore();

            EZLogger.Log.Info("Döviz yükleme bilgisi: Baþarýlý {0} Baþarýsýz {1}",
                lastSuccess[DataTypes.Currency], lastFail[DataTypes.Currency]);

            try
            {
                Document.LoadCurrencies(Currency.GetCurrencies());
            }
            catch { }
        }
        public Dictionary<int, ICurrency> GetCurrencies()
        {
            return Currency.GetCurrencies();
        }

        #endregion Currency

        #region Credit

        public int MaxCreditCount
        {
            get
            {
                return maxCreditCount;
            }
            set
            {
                maxCreditCount = value;
            }
        }
        private static void LoadCredits()
        {
            lastSuccess[DataTypes.Credit] = 0;
            lastFail[DataTypes.Credit] = 0;
            
            Credit.Backup();

            string[] lines = Connector.Instance().CurrentSettings.CreditLines;
            foreach (String line in lines)
            {
                if (maxCreditCount != 0 && lastSuccess[DataTypes.Credit] >= maxCreditCount)
                    break;
                if (Credit.Add(line.Substring(1)))
                    lastSuccess[DataTypes.Credit]++;
                else
                    lastFail[DataTypes.Credit]++;
            }
           
            Credit.Restore();

            EZLogger.Log.Info("Credits: Baþarýlý {0} Baþarýsýz {1}",
                lastSuccess[DataTypes.Credit], lastFail[DataTypes.Credit]);

        }

        public Dictionary<int, ICredit> GetCredits()
        {
            return Credit.GetCredits();
        }

        #endregion Credit

        #region Cashier

        public void LoadCashiers()
        {
            lastSuccess[DataTypes.Cashier] = 0;
            lastFail[DataTypes.Cashier] = 0;
            String path = PosConfiguration.DataPath + Settings.CashierFile;
            if (!File.Exists(path))
            {
                EZLogger.Log.Error("Kasiyer dosyasý bulunamadý");
                return;
            }

            Cashier.Backup();

            StreamReader sr = new StreamReader(path, PosConfiguration.DefaultEncoding);
            String line = "";

            while ((line = @sr.ReadLine()) != null)
            {
                //Skip trailing blank lines and comments
                if (line.Trim().Length == 0 || line[0] == '0') continue;

                if (Cashier.Add(line))
                    lastSuccess[DataTypes.Cashier]++;
                else
                    lastFail[DataTypes.Cashier]++;

            }
            sr.Close();

            Cashier.Restore();

            EZLogger.Log.Info("Kasiyer yükleme bilgisi: Baþarýlý {0} Baþarýsýz {1}",
                lastSuccess[DataTypes.Cashier], lastFail[DataTypes.Cashier]);
        }
        public ICashier FindCashierByPassword(String password)
        {
            return Cashier.FindByPassword(password);
        }
        public ICashier FindCashierById(String id)
        {
            return Cashier.FindById(id);
        }

        #endregion Cashier

        #region Customer

        public void LoadCustomers()
        {
            lastSuccess[DataTypes.Customer] = 0;
            lastFail[DataTypes.Customer] = 0;
            String path = PosConfiguration.DataPath + Settings.CustomerFile;
            if (!File.Exists(path))
            {
                EZLogger.Log.Error("Müþteri dosyasý bulunamadý");
                return;
            }

            Customer.Backup();

            StreamReader sr = new StreamReader(path, PosConfiguration.DefaultEncoding);
            String line = "";

            while ((line = @sr.ReadLine()) != null)
            {
                //Skip trailing blank lines and comments
                if (line.Trim().Length == 0 || line[0] == '0') continue;

                if (Customer.Add(line))
                    lastSuccess[DataTypes.Customer]++;
                else
                    lastFail[DataTypes.Customer]++;

            }
            sr.Close();

            Customer.Restore();

            EZLogger.Log.Info("Müþteri yükleme bilgisi: Baþarýlý {0} Baþarýsýz {1}",
                lastSuccess[DataTypes.Customer], lastFail[DataTypes.Customer]);
        }

        public ICustomer FindCustomerById(String customerNumber)
        {
            return Customer.FindById(customerNumber);
        }

        public ICustomer FindCustomerByCode(String code)
        {
            return Customer.FindByCode(code);
        }

        public ICustomer FindCustomerByName(String name)
        {
            return Customer.FindByName(name);
        }

        public List<ICustomer> SearchCustomersByInfo(String info)
        {
            return Customer.SearchCustomers(info);
        }

        public ICustomer SaveCustomer(String line)
        {
            String path = PosConfiguration.DataPath + Settings.NewCustomerFile + "." + PosConfiguration.Get("RegisterId");
            StreamWriter sw = null;

            Customer.Add(line);

            ICustomer c = Customer.CreateCustomer(line);

            if (!File.Exists(path))
                File.Create(path).Close();
            
            try
            {
                sw = File.AppendText(path);
                sw.WriteLine(line);

            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            return c;
        }

        #endregion Customer

        #region Serial


        public void LoadSerialNumbers()
        {
            lastSuccess[DataTypes.Serial] = 0;
            lastFail[DataTypes.Serial] = 0;

            String path = PosConfiguration.DataPath + Settings.SerialNumberFile;
            if (!File.Exists(path))
            {
                EZLogger.Log.Error("Serial dosyasý bulunamadý");
                return;
            }
            
            Serial.Backup();

            StreamReader sr = new StreamReader(path, PosConfiguration.DefaultEncoding);
            String line = "";

            while ((line = @sr.ReadLine()) != null)
            {
                if (Serial.Add(line))
                    lastSuccess[DataTypes.Serial]++;
                else
                    lastFail[DataTypes.Serial]++;

            }
            sr.Close();


            Serial.Restore();

            EZLogger.Log.Info("Serials: Baþarýlý {0} Baþarýsýz {1}",
                lastSuccess[DataTypes.Serial], lastFail[DataTypes.Serial]);

        }

        public bool AvailableSerialNumber(String serial)
        {
            return Serial.CheckSerial(serial);
        }

        #endregion

        #region All Data

        public void LoadAll()
        {
            // ! Care load functions not to give any exception
            // ! Are success and fail counts enough to say fatal error by pos
            LoadProducts();
            LoadCustomers();
            LoadCashiers();
            LoadCurrencies();
            LoadCredits();
        }

        #endregion All Data

        #region Communication

        public void Connect()
        {
            try
            {
                MessageFileWatcher.Start();
            }
            catch (Exception ex)
            {
				Log(ex);
                EZLogger.Log.Error("Message file watcher could not start");
            }
        }

        public bool IsOnline
        {
            get
            {
                if (fxClient.DirectoryExists(PosConfiguration.ServerDownloadPath, 5000) )
                    // && fxClient.DirectoryExists(PosConfiguration.ServerUploadPath, 5000))
                {
                    return true;
                    //TODO FileTransferClient in icine gommek lazim. ftp ve http icin daha pratik
                    //canwrite sorgulama sistemi olabilir
				
					/*
					 * unreachable code so it is commented
					try
                    {
                       
                        string[] dir = Directory.GetFiles(PosConfiguration.ServerArchivePath, "H*." + PosConfiguration.Get("RegisterId"));
                        if (dir.Length == 0) return true;
                        FileInfo fi = new FileInfo(dir[0]);
#if Mono
						return fi != null;
#elif WindowsCE
						return fi != null;
#else
						return !(fi.IsReadOnly);
#endif
                    }
                    catch
                    {
                        return false;
                    }
					*/
                }
                    //TODO bu ne ise yariyor??
                    // Arka Ofis klasörü map drive edilmiþse Windows un þöyle bir bug ý var,
                    // Baðlantý koptuðunda Bilgisayarým da map driver üzeinde baðlý deðil iþareti oluþuyordu.
                    // Baðlantý eðer düzelirse map driver hala baðlý deðil gözüküyordu ve kasa online 
                    // konumuna geçemiyordu. Burda baðlantýnýn düzelip düzelmediðini kontrol ediyoruz. 
                    // Windows u map drivera baðlanmasý için zorluyoruz.A.D.
                else return ForceConnectMapDrive(PosConfiguration.ServerUploadPath);
            }
        }

        public string BackOfficeCommand
        {
            get
            {
                return MessageFileWatcher.BackOfficeCommand;
            }
        }

        public void TransferOfflineData()
        {
            FileInfo[] archiveFiles = Dir.GetFilesInfo(PosConfiguration.ArchivePath, "*.xfr");
            FileInfo[] logFiles = Dir.GetFilesInfo(PosConfiguration.LogPath, "*.xfr");
            FileInfo[] reportFiles = Dir.GetFilesInfo(PosConfiguration.ReportPath, "*.xfr");

            String registerId = PosConfiguration.Get("RegisterId");
            if (archiveFiles.Length + reportFiles.Length + logFiles.Length > 0)
                lock (TransferLock)
                {
                    foreach (FileInfo transferFileInfo in archiveFiles)
                    {
                        String serverFileName = transferFileInfo.Name.Replace("xfr", registerId);
                        //TODO can we remove FileExists check
                        if (fxClient.FileExists(PosConfiguration.ServerArchivePath + serverFileName))
                            fxClient.AppendFile(PosConfiguration.ServerArchivePath + serverFileName, transferFileInfo.FullName);
                        else
                        {
                            if (File.Exists(PosConfiguration.ArchivePath + serverFileName))
                                fxClient.UploadFile(PosConfiguration.ServerArchivePath + serverFileName,     
                                                    PosConfiguration.ArchivePath + serverFileName);
                        }
                        transferFileInfo.Delete();
                    }
                    foreach (FileInfo transferFileInfo in logFiles)
                    {
                        String serverFileName = transferFileInfo.Name.Replace("xfr", registerId);
                        //TODO can we remove FileExists check
                        if (fxClient.FileExists(PosConfiguration.ServerUploadPath + serverFileName))
                            fxClient.AppendFile(PosConfiguration.ServerUploadPath + serverFileName, transferFileInfo.FullName);
                        transferFileInfo.Delete();
                    }
                    foreach (FileInfo transferFileInfo in reportFiles)
                    {
                        String serverFileName = transferFileInfo.Name.Replace("xfr", registerId);
                        fxClient.UploadFile(PosConfiguration.ServerReportPath + serverFileName, transferFileInfo.FullName);
                        transferFileInfo.Delete();
                        /*
                         if (File.Exists(PosConfiguration.ServerUploadPath + serverFileName))
                            File.Delete(PosConfiguration.ServerUploadPath + serverFileName);
                         File.Move(transferFileInfo.FullName, PosConfiguration.ServerUploadPath + serverFileName);
                         */ 
                    }
                }
        }

        public int ProcessRequest(String request)
        {
            return MessageFileWatcher.ProcessRequest(request);
        }

        public void SendWaitingMessage(String message)
        {
            MessageFileWatcher.SendWaitingMessage(message);
        }

        public void SendMessage(bool success, String message)
        {
            MessageFileWatcher.SendMessage(success, message);
        }

        #endregion Communication

        #region Log

        public void StartLog()
        {
            transferLock = new object();
            
            mainLog = new Logger( LogType.Main);
            voidedDocumentsLog = new Logger( LogType.Void );            
            returnsLog = new Logger( LogType.Return);
            lastDocumentLog = new Logger(LogType.LastDocument);

            mainLog.PrintExeVersion();
        }
        internal static object TransferLock
        {
            get { return transferLock; }
        }

        public void OnDocumentClosed(ISalesDocument document)
        {
            mainLog.OnDocumentClosed(document);
            lastDocumentLog.OnDocumentChanged(null, 0);
        }

        public void OnDocumentVoided(ISalesDocument document, int voidedReason)
        {
            voidedDocumentsLog.OnDocumentVoided(document, voidedReason);
            lastDocumentLog.OnDocumentChanged(null, 0);
        }

        public void OnDocumentSuspended(ISalesDocument document, int zReportNo)
        {
            voidedDocumentsLog.OnDocumentSuspended(document, zReportNo);
            lastDocumentLog.OnDocumentChanged(null, 0);
        }

        public void OnReturnDocumentClosed(ISalesDocument document)
        {
            returnsLog.OnDocumentClosed(document);
            if (currentSettings.GetProgramOption(Setting.AddOnlyReturnFile) == PosConfiguration.OFF)
                mainLog.OnDocumentClosed(document);
            lastDocumentLog.OnDocumentChanged(null, 0);
        }

        public void OnDocumentUpdated(ISalesDocument document,int documentStatus)
        {
            lastDocumentLog.OnDocumentChanged(document, documentStatus);
        }

        public void OnCashierLogin(ICashier cashier, int zReportNo)
        {
            if (!isArchiveDirChecked) 
                CleanArchiveFolder();
            mainLog.OnCashierLogin(cashier, zReportNo);
        }

        public void OnCashierLogout(ICashier cashier)
        {
            mainLog.OnCashierLogout(cashier);
        }

        public void OnDeposit(Decimal amount)
        {
            mainLog.OnDeposit(amount);
        }
        /// <summary>
        /// for cash withdrwal
        /// </summary>
        /// <param name="amount"></param>
        public void OnWithdrawal(Decimal amount)
        {
            mainLog.OnWithdrawal(amount);
        }
        /// <summary>
        /// For check withdrawal
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="refNumber"></param>
        public void OnWithdrawal(Decimal amount, String refNumber)
        {
            mainLog.OnWithdrawal(amount, refNumber);
        }
        /// <summary>
        /// for credit withdrawal
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="credit"></param>
        public void OnWithdrawal(Decimal amount, ICredit credit)
        {
            mainLog.OnWithdrawal(amount, credit);
        }
        public void CheckZWritten(int zNo, int documentId)
        {
            mainLog.CheckZWritten(zNo, documentId);
        }

        public void OnZReportComplete(int zReportNo, DateTime ZReportDate,bool isFiscal)
        {
            if (isFiscal && Connector.Instance().CurrentSettings.GetProgramOption(Setting.BarcodeLineInMainLogFile) == PosConfiguration.OFF)
            {
                try
                {
                    Document.LoadBarcodeListFile();
                }
                catch (Exception ex)
				{
					Log(ex);
                    EZLogger.Log.Error("Barcode list file couldn't write.");
                }
            }
            mainLog.OnZReportComplete(zReportNo, ZReportDate);
            lastDocumentLog.OnDocumentChanged(null, 0);
            if (isFiscal)
            {
                try
                {
                    UploadRegisterFilesToServer(zReportNo, false);
                }
                catch (Exception ex)
                {
					Log(ex);
                }
            }
        }

        public void OnNetworkDown()
        {
            mainLog.OnNetworkDown();
        }
        public string FormatLines(ISalesDocument document)
        {
            int tempref = 0;
            return Logger.LogFormatter.LogItems(document, TEMP_DOCUMENT_STATUS, ref tempref).ToString();
        }

        public void SaveReport(String filename, String reportText)
        {
            Logger.SaveReport(filename, reportText);
        }

        public void ResetSequenceNumber()
        {
            mainLog.ResetSequenceNumber();
            voidedDocumentsLog.ResetSequenceNumber();
            returnsLog.ResetSequenceNumber();
        }

        public void UploadRegisterFilesToServer(int ZReportId, bool allFiles)
        {
            EZLogger.Log.Stop();
            String logFilename = EZLogger.Log.LogFileName;
            //if ((Common.IOUtil.GetAttributes(logFilename) & FileAttributes.NotContentIndexed) == FileAttributes.NotContentIndexed)
            {
                List<String> files = new List<String>();
                //Using NotContentIndexed attribute to indicate log file as "contains error or fatal messages"
                //Would have used archive flag instead of NotContentIndexed but it keeps getting changed by C# file libraries
                //This section zips up necessary log files and does a threaded upload to hugin webserver for troubleshooting

                String postfix = String.Format("{0:ddMMyy}.{1}", DateTime.Today, PosConfiguration.Get("RegisterId").PadLeft(3, '0'));

                //Mainlogfile
                files.Add(String.Format("{0}{1}{2}", PosConfiguration.ArchivePath, Logger.MainLogPrefix, postfix));

                if (allFiles)
                {
                    //voidedLogFileName
                    files.Add(String.Format("{0}{1}{2}", PosConfiguration.ArchivePath, Logger.VoidedLogPrefix, postfix));

                    //returnsLogFileName 
                    files.Add(String.Format("{0}{1}{2}", PosConfiguration.ArchivePath, Logger.ReturnsLogPrefix, postfix));
                }

                //lastZFileName 
                files.Add(String.Format("{0}{1}{2:D4}.{3}", PosConfiguration.ReportPath, "ZRAP", ZReportId, PosConfiguration.Get("RegisterId").PadLeft(3, '0')));

                //settingsFileName 
                files.Add(String.Format("{0}{1}", PosConfiguration.DataPath, PosConfiguration.SettingsFile));

                //barcodeFileName 
                if (currentSettings.GetProgramOption(Setting.BarcodeLineInMainLogFile) == PosConfiguration.OFF)
                    files.Add(String.Format("{0}{1}", PosConfiguration.DataPath, Settings.BarcodeFile));

                List<FileInfo> inputFiles = new List<FileInfo>();

                foreach (String fileName in files)
                {
                    if (File.Exists(fileName))
                        inputFiles.Add(new FileInfo(fileName));
                }

                String outputFile = String.Format("{0}-{1:yyMMddHHmmss}.zip", PosConfiguration.Get("FiscalId"), DateTime.Now);
                try
                {
                    ZipOutputStream s = new ZipOutputStream(File.Create(outputFile));

                    s.SetLevel(9); // 0 - store only to 9 - means best compression
                    byte[] buffer = new byte[4096];

                    foreach (FileInfo logFileInfo in inputFiles)
                    {
                        if (!File.Exists(logFileInfo.FullName)) continue;
                        ZipEntry entry = new ZipEntry(logFileInfo.Name);
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);

                        FileStream fs = File.OpenRead(logFileInfo.FullName);
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);

                        fs.Close();
                    }
                    s.Finish();

                    // Close is important to wrap things up and unlock the file.
                    s.Close();

                    System.Threading.Thread t = new System.Threading.Thread(delegate()
                     {
                         try
                         {
                             //todo: move this code to function.
                             string url = "http://diag.hugin.com.tr/uploader.php";
                             string file = outputFile;
                             string paramName = "file";
                             string contentType = "application/octet-stream";

                             string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                             byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                             HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
                             wr.ContentType = "multipart/form-data; boundary=" + boundary;
                             wr.Method = "POST";
                             wr.KeepAlive = true;
                             wr.AllowWriteStreamBuffering = true;
                             wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

                             Stream rs = wr.GetRequestStream();
                             Debugger.Instance().AppendLine("Stream get.");

                             rs.Write(boundarybytes, 0, boundarybytes.Length);

                             string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                             string header = string.Format(headerTemplate, paramName, file, contentType);
                             byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                             rs.Write(headerbytes, 0, headerbytes.Length);

                             FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                             buffer = new byte[4096];
                             int bytesRead = 0;
                             while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                             {
                                 rs.Write(buffer, 0, bytesRead);
                             }
                             fileStream.Close();

                             byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                             rs.Write(trailer, 0, trailer.Length);
                             rs.Close();

                             WebResponse wresp = null;
                             try
                             {
                                 wresp = wr.GetResponse();
                                 String result = new StreamReader(wresp.GetResponseStream()).ReadToEnd();

                                 if (Str.Contains(result, "failed"))
                                     Debugger.Instance().AppendLine("Hareket, log, program dosyalarý upload edilemedi:" + result);
                                 else
                                     Debugger.Instance().AppendLine("zip dosyasý baþarýyla upload edildi:" + result);
                             }
                             catch (Exception ex)
                             {
                                 if (wresp != null)
                                 {
                                     wresp.Close();
                                     wresp = null;
                                 }
                                 Debugger.Instance().AppendLine("Hareket, log, program dosyalarý upload edilemedi");
                                 Debugger.Instance().AppendLine(ex.Message);

                             }
                             finally
                             {
                                 wr = null;
                             }
                         }
                         catch (Exception ex)
                         {
							Log(ex);
                         }
                         finally
                         {
                             if (File.Exists(outputFile))
                                 File.Delete(outputFile);
                         }
                     }
                     );
                    t.Start();
                }
                catch (Exception ex) { Debugger.Instance().AppendLine("File upload error : " + ex.Message); }
            }


            String archivePath = String.Format("{0}tmp/", PosConfiguration.ArchivePath);
            if (!Directory.Exists(archivePath)) Directory.CreateDirectory(archivePath);
            String targetPath = String.Format("{0}tmp/HE{1}", PosConfiguration.ArchivePath,
                                                                  ZReportId.ToString().PadLeft(4, '0'));
            String modifiedTargetPath = targetPath + ".log";
            for (int i = 0; File.Exists(modifiedTargetPath); i++)
                modifiedTargetPath = targetPath + "_" + i + ".log";

            if (File.Exists(logFilename))
                File.Move(logFilename, modifiedTargetPath);

#if WindowsCE
            if (IOUtil.ProgramDirectory == PosConfiguration.LocalPath)
            {
                String[] mainFiles = new String[4];
                mainFiles[0] = String.Format("{0}{1}.{2}", PosConfiguration.ArchivePath,
                                                                Logger.MainLogName,
                                                                PosConfiguration.Get("RegisterId").PadLeft(3, '0')
                                                                );
                mainFiles[1] = String.Format("{0}{1}.{2}", PosConfiguration.ArchivePath,
                                                                Logger.VoidedLogName,
                                                                PosConfiguration.Get("RegisterId").PadLeft(3, '0')
                                                                );
                mainFiles[2] = String.Format("{0}{1}.{2}", PosConfiguration.ArchivePath,
                                                                Logger.ReturnsLogName,
                                                                PosConfiguration.Get("RegisterId").PadLeft(3, '0')
                                                                );
                mainFiles[3] = String.Format("{0}{1}", PosConfiguration.DataPath, Settings.BarcodeFile);

                foreach (String fileName in mainFiles)
                {
                    if (File.Exists(fileName))
                        IOUtil.WriteAllText(fileName, "");
                }
            }
#endif
            Debugger.Instance().AppendLine("file uploading is finished.");

            EZLogger.Log.Start();
        }
        private void CleanArchiveFolder()
        {
            try
            {
                //tmp klasöründe bulunan bir aydan önceki log dosyalarý silinir.
                List<FileInfo> files = new List<FileInfo>(new DirectoryInfo(PosConfiguration.ArchivePath + "tmp").GetFiles("HE*.log"));
#if WindowsCE
                if (IOUtil.ProgramDirectory == PosConfiguration.LocalPath)
                {
                    files.AddRange(new DirectoryInfo(PosConfiguration.ArchivePath).GetFiles(Logger.MainLogPrefix + "*." + PosConfiguration.Get("RegisterId")));
                    files.AddRange(new DirectoryInfo(PosConfiguration.ArchivePath).GetFiles(Logger.ReturnsLogPrefix + "*." + PosConfiguration.Get("RegisterId")));
                    files.AddRange(new DirectoryInfo(PosConfiguration.ArchivePath).GetFiles(Logger.VoidedLogPrefix + "*." + PosConfiguration.Get("RegisterId")));
                }
#endif
                foreach (FileInfo file in files)
                {
                    TimeSpan span = DateTime.Today.Subtract(file.LastWriteTime.Date);
                    if (span.Days > 30)
                        File.Delete(file.FullName);
                }
            }
            catch { }

            isArchiveDirChecked = true;
        }

        #endregion Log

        #region Program

        public void GetNewProgram()
        {
            fxClient.DownloadFile(PosConfiguration.ServerControlPath + Settings.SettingsFile,
                PosConfiguration.DataPath + Settings.NewSettingsFile);

        }

        #endregion Program

        #region RegisterFiles

        public void UploadRegisterFile(int lastZNo)
        {
            UploadRegisterFilesToServer(lastZNo, true);
        }
        #endregion RegisterFiles

        #region Report

        public void PrepareZReport(String registerId) { Document.PrepareZReport(registerId); }
        public void AfterZReport(String registerId) { Document.AfterZReport(registerId); }
        public decimal GetRegisterCash(String registerId) { return Document.GetRegisterCash(registerId); }
        public int GetLastDocumentId(string registerId) { return Document.GetLastDocumentId(registerId); }
        public string GetReportXml(String registerId) { return Document.GetReportXml(registerId); }
        public string GetReportXml(string registerId, DateTime day) { return Document.GetReportXml(registerId, day); }
        public string GetReportXml(string registerId, string cashierCode, DateTime firstDate, DateTime lastDate) { return Document.GetReportXml(registerId, cashierCode, firstDate, lastDate); }

        #endregion Report

        #region Customer Point
        public IPointAdapter PointAdapter
        {
            get
            {
                if (pointAdapter == null)
                {
                    String pointServerUri = PosConfiguration.Get("PointServerUri");
                    if (pointServerUri != "")
                    {
                        try
                        {
                            int index = pointServerUri.IndexOf(',');
                            String prefix = pointServerUri.Substring(0, index);
                            String connString = pointServerUri.Substring(++index);

                            switch (prefix)
                            {
                                case "WS":
#if Mono
								throw new Exception("WS Point is not supported");
#else
                                    pointAdapter = new WSPointAdapter(connString);
                                    break;
#endif
                                case "MSACCESS":
                                case "SDFFILE":
                                    pointAdapter = new DBPointAdapter(connString);
                                    break;
                                case "MSSQL":
                                    pointAdapter = new SQLPointAdapter(connString);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            EZLogger.Log.Error("Puan dosyasýna baðlanýlamadý. Hata : " + ex.Message);
                        }
                    }
                }
                return pointAdapter;
            }
        }
        #endregion

        #region MapDrive Helper
        private static Dictionary<string, NETRESOURCE> networkDrivers = null;
        private static bool ForceConnectMapDrive(string path)
        {
            try
            {
                if (networkDrivers == null)
                    networkDrivers = WNetResource(null);
                int rc = 0;
                foreach (string key in networkDrivers.Keys)
                {
                    if (key != path.Substring(0, 2).ToUpper()) continue;
                    rc = WNetAddConnection2(networkDrivers[key], null, null, (int)RESOURCE_USAGE.RESOURCEUSAGE_CONNECTABLE);
                    if (rc == 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
				Log(ex);
			}
            return false;
        }

        static Dictionary<string, NETRESOURCE> WNetResource(object resource)
        {
            Dictionary<string, NETRESOURCE> result = new Dictionary<string, NETRESOURCE>();

            int iRet;
            IntPtr ptrHandle = new IntPtr();
            try
            {

                iRet = WNetOpenEnum(
                    RESOURCE_SCOPE.RESOURCE_REMEMBERED, RESOURCE_TYPE.RESOURCETYPE_DISK, RESOURCE_USAGE.RESOURCEUSAGE_ALL,
                    resource, out ptrHandle);
                if (iRet != 0)
                    return null;

                int entries = -1;
                int buffer = 16384;
                IntPtr ptrBuffer = Marshal.AllocHGlobal(buffer);
                NETRESOURCE nr;

                iRet = WNetEnumResource(ptrHandle, ref entries, ptrBuffer, ref buffer);
                while ((iRet == 0) || (entries > 0))
                {
                    Int32 ptr = ptrBuffer.ToInt32();
                    for (int i = 0; i < entries; i++)
                    {
                        nr = (NETRESOURCE)Marshal.PtrToStructure(new IntPtr(ptr), typeof(NETRESOURCE));
                        if ((int)RESOURCE_USAGE.RESOURCEUSAGE_CONTAINER == (nr.Usage
                            & (int)RESOURCE_USAGE.RESOURCEUSAGE_CONTAINER))
                        {
                            //call recursively to get all entries in a container
                            WNetResource(nr);
                        }
                        ptr += Marshal.SizeOf(nr);
                        result.Add(nr.LocalName, nr);
                    }

                    entries = -1;
                    buffer = 16384;
                    iRet = WNetEnumResource(ptrHandle, ref entries, ptrBuffer, ref buffer);
                }

                Marshal.FreeHGlobal(ptrBuffer);
                iRet = WNetCloseEnum(ptrHandle);
            }
            catch (Exception)
            {
            }

            return result;
        }
        [StructLayoutAttribute(LayoutKind.Sequential)]
    
        public class NETRESOURCE
        {
            public int Scope;
            public int RType;
            public int Display;
            public int Usage;

            [MarshalAs(UnmanagedType.LPTStr, SizeConst = 200)]
            public string LocalName;
            [MarshalAs(UnmanagedType.LPTStr, SizeConst = 200)]
            public string RemoteName;
            [MarshalAs(UnmanagedType.LPTStr, SizeConst = 200)]
            public string Comment;
            [MarshalAs(UnmanagedType.LPTStr, SizeConst = 200)]
            public string Provider;
        }

        [DllImport("mpr.dll", CharSet = CharSet.Auto)]
        private static extern int WNetAddConnection2(NETRESOURCE NetResource,
            string Password, string UserName, UInt32 Flags);

        [DllImport("mpr.dll", CharSet = CharSet.Auto)]
        private static extern int WNetCancelConnection(string Name, bool Force);

        [DllImport("MPR.dll", CharSet = CharSet.Auto)]
        static extern int WNetEnumResource(IntPtr hEnum, ref int lpcCount, IntPtr lpBuffer, ref int lpBufferSize);

        [DllImport("MPR.dll", CharSet = CharSet.Auto)]
        static extern int WNetOpenEnum(RESOURCE_SCOPE dwScope, RESOURCE_TYPE dwType, RESOURCE_USAGE dwUsage,
            [MarshalAs(UnmanagedType.AsAny)][In] object lpNetResource, out IntPtr lphEnum);

        [DllImport("MPR.dll", CharSet = CharSet.Auto)]
        static extern int WNetCloseEnum(IntPtr hEnum);

        public enum RESOURCE_SCOPE : uint
        {
            RESOURCE_CONNECTED = 0x00000001,
            RESOURCE_GLOBALNET = 0x00000002,
            RESOURCE_REMEMBERED = 0x00000003,
            RESOURCE_RECENT = 0x00000004,
            RESOURCE_CONTEXT = 0x00000005
        }
        public enum RESOURCE_TYPE : uint
        {
            RESOURCETYPE_ANY = 0x00000000,
            RESOURCETYPE_DISK = 0x00000001,
            RESOURCETYPE_PRINT = 0x00000002,
            RESOURCETYPE_RESERVED = 0x00000008,
        }
        public enum RESOURCE_USAGE : uint
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        }
        public enum RESOURCE_DISPLAYTYPE : uint
        {
            RESOURCEDISPLAYTYPE_GENERIC = 0x00000000,
            RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001,
            RESOURCEDISPLAYTYPE_SERVER = 0x00000002,
            RESOURCEDISPLAYTYPE_SHARE = 0x00000003,
            RESOURCEDISPLAYTYPE_FILE = 0x00000004,
            RESOURCEDISPLAYTYPE_GROUP = 0x00000005,
            RESOURCEDISPLAYTYPE_NETWORK = 0x00000006,
            RESOURCEDISPLAYTYPE_ROOT = 0x00000007,
            RESOURCEDISPLAYTYPE_SHAREADMIN = 0x00000008,
            RESOURCEDISPLAYTYPE_DIRECTORY = 0x00000009,
            RESOURCEDISPLAYTYPE_TREE = 0x0000000A,
            RESOURCEDISPLAYTYPE_NDSCONTAINER = 0x0000000B
        }


        #endregion
   }
}
