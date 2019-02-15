using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;
#if !Mono
using ICSharpCode.SharpZipLib.Zip;
#endif
namespace Hugin.POS.Data
{
    class MessageFileWatcher
    {
        private const int tryLimit = 5;

        private static int SuccessCount = 0;
        private static int FailCount = 0;
        private static Object messageLock = new Object();
        private static DateTime lastReadTime;

        public static string MessageFileName
        {
            get { return PosConfiguration.ServerControlPath + "Mesaj." + PosConfiguration.Get("RegisterId"); }
        }

        const String SERVER_REQUEST_WAITING = "200000000000000";
        const String SERVER_REQUEST_COMPLETE = "300000000000000";
        const String SERVER_REQUEST_FAILED = "400000000000000";
		
		private static void Log (Exception ex)
		{
			/*
			 * this function is implemented to catch warnings
			 * which caused fraom unreferenced "ex" variables
			 */
		}
        internal static void Start()
        {
            //Update last write time to process if document has a command when BackOfficeCommand is called

            lastReadTime = DateTime.MinValue;
            Connector.FxClient.AppendString(MessageFileName,"");
        }

        internal static String BackOfficeCommand
        {
            get
            {
                DateTime lastMessageTime = DateTime.MinValue;

                lock (messageLock)
                {
                    try
                    {
                        if (!Connector.FxClient.FileExists(MessageFileName))
                        {
                            if (Connector.Instance().IsOnline)
                                EZLogger.Log.Warning("Arka ofis program uyarý: Mesaj dosyasý bulunamadý!");
                            throw new BackOfficeUnavailableException("Mesaj dosyasýna ulaþýlamýyor : " + MessageFileName);
                        }

                        lastMessageTime = Connector.FxClient.FileGetLastWriteTime(MessageFileName);
                    }
                    catch (System.IO.FileNotFoundException fe)
                    {
						Log(fe);
                        //This means even though connectivity exists Mesaj file was deleted
                        //(probably by backoffice program)
                        EZLogger.Log.Warning("Arka ofis program uyarý: Mesaj dosyasý silinmemeli!");
                    }

                    if (lastMessageTime.Equals(lastReadTime)) return "0";

                    EZLogger.Log.Debug("Server message read on {0}, last message time {1}", DateTime.Now, lastMessageTime);
                    EZLogger.Log.Debug("Timespan between last two messages {0}", lastMessageTime - lastReadTime);

                    lastReadTime = lastMessageTime;

                    try
                    {
                        String message = TryToRead(MessageFileName).TrimEnd('\r', '\n');
                        if (message.IndexOf('\n') > 0)
                            message = message.Substring(0, message.IndexOf('\n'));
                        EZLogger.Log.Debug("New server message received: {0}", message);

                        String response = SERVER_REQUEST_WAITING + "\n";
                        if (message.StartsWith("1"))
                            response += "Kasa uygun olduðunda iþleminiz yapýlacaktýr";
                        else if (message == "300000000000000")// process cancelled by client (if did not written by server)
                            return "-2";
                        else
                        {
                            response += "Mesaj uygun formatta deðil.";
                            message = "0";
                        }

                        TryToWrite(MessageFileName, response);
                        EZLogger.Log.Debug("Response for server message : {0}", response);
                        return message;
                    }
                    catch (BackOfficeUnavailableException boue)
                    {
                        throw boue;
                    }
                    catch (Exception ex)
                    {
                        EZLogger.Log.Fatal(ex.Message);
                        if (!Connector.Instance().IsOnline)
                            throw new BackOfficeUnavailableException();

                        return "-1";
                    }
                }
            }
        }

        internal static int ProcessRequest(String request)
        {
            try
            {
                String success = String.Empty;
                switch (request.Substring(1, 2))
                {
                    case "00":
                        String zipName = "Data.zip";
                        String targetPath = PosConfiguration.DataPath;

                        if (request.Length > 3 && request[3] == '1')
                        {
                            zipName = "Image.zip";
                            targetPath = PosConfiguration.ImagePath;
                        }

                        String zipfile = PosConfiguration.ServerDownloadPath + zipName;

                        if (!Directory.Exists(targetPath))
                        {
                            SendMessage(false, targetPath + "' YOLU BULUNAMADI...");
                            return -1;
                        }
                        if (!File.Exists(zipfile))
                        {
                            SendMessage(false, zipfile + "' DOSYASI BULUNAMADI...");
                            return -1;
                        }

                        GetDataZip(zipfile, targetPath);

                        if (request.Length > 3 && request[3] == '1')
                        {
                            success = String.Format("IMAGE KLASÖRÜ GÜNCELLEMESÝ: {0:D6} BAÞARILI, {1:D6} BAÞARISIZ", SuccessCount, FailCount);
                        }
                        else
                        {
                            Connector.Instance().LoadAll();

                            success = String.Format("ZÝP ÝLE DATA YÜKLEME : KAS {0:D3}, MUST {1:D6}, DVZ {2:D3}, CRD {3:D3}, URUN {4:D6}",
                                Connector.Instance().GetLastSuccess(DataTypes.Cashier),
                                Connector.Instance().GetLastSuccess(DataTypes.Customer),
                                Connector.Instance().GetLastSuccess(DataTypes.Currency),
                                Connector.Instance().GetLastSuccess(DataTypes.Credit),
                                Connector.Instance().GetLastSuccess(DataTypes.Product));
                        }
                        SendMessage(true, success);
                        return 1;//successful

                    case "07": //Urun Dosyasi
                        if (request.Length > 3 && request[3] == '2')
                        {
                            Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.ProductFile,
                                                            PosConfiguration.DataPath + "Update" + Settings.ProductFile);
                            try
                            {
                                Connector.Instance().UpdateProducts();
                                success = String.Format("ÜRÜN GÜNCELLEMESÝ: {0:D6} BAÞARILI, {1:D6} BAÞARISIZ",
                                    Connector.Instance().GetLastSuccess(DataTypes.Product),
                                    Connector.Instance().GetLastFail(DataTypes.Product));
                                SendMessage(true, success);
                                return 1;
                            }
                            catch (IOException io)
                            {
                                //after update_product file is processed, product file is updated, 
                                if (Connector.Instance().GetLastSuccess(DataTypes.Product) > 0)
                                {
                                    SendMessage(false, "ÜRÜNLER ÞUAN ÝÇÝN GÜNCELLENDÝ AMA GÜNCELLENEN ÜRÜNLER ÜRÜN DOSYASINA EKLENEMEDÝ");
                                }
                                else
                                    SendMessage(false, io.Message);
                                return -1;//report fail result to the exe using data.dll

                            }
                        }
                        else
                        {
                            Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.ProductFile,
                                                            PosConfiguration.DataPath + Settings.ProductFile);

                            if (Connector.Instance().CurrentSettings.GetProgramOption(Setting.DepartmentNameOnly) == PosConfiguration.ON)
                                Product.departmentNameOnly = true;
                            try
                            {
                                Connector.Instance().LoadProducts();
                            }
                            finally
                            {
                                Product.departmentNameOnly = false;
                            }

                            success = String.Format("ÜRÜN YÜKLEMESÝ: {0:D6} BAÞARILI, {1:D6} BAÞARISIZ",
                                Connector.Instance().GetLastSuccess(DataTypes.Product),
                                Connector.Instance().GetLastFail(DataTypes.Product));
                            SendMessage(true, success);
                            return 1;
                        }
                    case "08": //Musteri (Cari) Dosyalari
                        if (request.Length > 3 && request[3] == '2')
                        {
                            Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.CustomerFile,
                                                        PosConfiguration.DataPath + "Update" + Settings.CustomerFile);

                            try
                            {
                                Connector.Instance().UpdateCustomers();
                                success = String.Format("MÜÞTERÝ GÜNCELLEMESÝ: {0:D6} BAÞARILI, {1:D6} BAÞARISIZ",
                                    Connector.Instance().GetLastSuccess(DataTypes.Customer),
                                    Connector.Instance().GetLastFail(DataTypes.Customer));
                                SendMessage(true, success);
                                return 1;
                            }
                            catch (IOException io)
                            {
                                //after update_customer file is processed, customer file is updated, 
                                if (Connector.Instance().GetLastSuccess(DataTypes.Customer) > 0)
                                {
                                    SendMessage(false, "MÜÞTERÝLER ÞUAN ÝÇÝN GÜNCELLENDÝ AMA GÜNCELLENEN MÜÞTERÝLER MÜÞTERÝ DOSYASINA EKLENEMEDÝ");
                                }
                                else
                                    SendMessage(false, io.Message);
                                return -1;//report fail result to the exe using data.dll

                            }
                        }
                        else
                        {

                            Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.CustomerFile,
                                                        PosConfiguration.DataPath + Settings.CustomerFile);

                            Connector.Instance().LoadCustomers();
                            success = String.Format("MÜÞTERÝ YÜKLEMESÝ: {0:D6} BAÞARILI, {1:D6} BAÞARISIZ",
                                Connector.Instance().GetLastSuccess(DataTypes.Customer),
                                Connector.Instance().GetLastFail(DataTypes.Customer));
                            SendMessage(true, success);
                            return 1;
                        }

                    case "09": //Kasiyer Dosyasi

                        Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.CashierFile,
                                                        PosConfiguration.DataPath + Settings.CashierFile);

                        Connector.Instance().LoadCashiers();
                        success = String.Format("KASÝYER YÜKLEMESÝ: {0:D6} BAÞARILI, {1:D6} BAÞARISIZ",
                            Connector.Instance().GetLastSuccess(DataTypes.Cashier),
                            Connector.Instance().GetLastFail(DataTypes.Cashier));
                        SendMessage(true, success);
                        return 1;
                    case "10": //Doviz Dosyasi
                        Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.ExchangeRateFile,
                                                        PosConfiguration.DataPath + Settings.ExchangeRateFile);

                        Data.Connector.Instance().LoadCurrencies();
                        success = String.Format("DÖVÝZ YÜKLEMESÝ: {0:D6} BAÞARILI, {1:D6} BAÞARISIZ",
                            Connector.Instance().GetLastSuccess(DataTypes.Currency),
                            Connector.Instance().GetLastFail(DataTypes.Currency));
                        SendMessage(true, success);
                        return 1;
                    case "11": //Kasa hareketleri
                        List<String> requestedFileNames = new List<string>();
                        String suffix = "." + PosConfiguration.Get("RegisterId");
                        String destinationPath = PosConfiguration.ServerArchivePath;
                        switch (request.Substring(3, 1))
                        {
                            case "0": //Tum hareketler
                                if (!Str.Contains(request, "00000000000")) goto case "daterange";
                                requestedFileNames.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                                                                               "*" + suffix));
                                goto case "copy";
                            case "1": //Online hareketler
                                if (!Str.Contains(request, "00000000000")) goto case "daterange";
                                requestedFileNames.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                                                                               String.Format("*{0:ddMMyy}{1}", DateTime.Today, suffix)));
                                goto case "copy";
                            case "2": //Gunluk hareketler
                                if (!Str.Contains(request, "00000000000")) goto case "daterange";
                                requestedFileNames.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                                               String.Format("HR{0:ddMMyy}{1}", DateTime.Today, suffix)));
                                goto case "copy";
                            case "3":
                                if (!Str.Contains(request, "00000000000")) goto case "daterange";
                                //Gunluk iade ve iptaller
                                requestedFileNames.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                                                                               String.Format("HI{0:ddMMyy}{1}", DateTime.Today, suffix)));
                                requestedFileNames.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                                                                               String.Format("HD{0:ddMMyy}{1}", DateTime.Today, suffix)));
                                goto case "copy";
                            case "4": //Sistem hareketleri
                                requestedFileNames.AddRange(Dir.GetFiles(PosConfiguration.LogPath,
                                                                               String.Format("HRERROR{1}", DateTime.Today, suffix)));
                                destinationPath = PosConfiguration.ServerUploadPath;
                                goto case "copy";
                            case "daterange":

                                //Tarihe gore hareketler
                                String firstDay = request.Substring(7, 2) + request.Substring(5, 2) + request.Substring(3, 2);
                                String lastDay = request.Substring(13, 2) + request.Substring(11, 2) + request.Substring(9, 2);
                                String shortName;

                                requestedFileNames.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                                                                               "*" + suffix));
                                List<String> cloneFileNames = new List<string>(requestedFileNames);
                                foreach (string fileName in cloneFileNames)
                                {
                                    shortName = new FileInfo(fileName).Name.Substring(2, 6);
                                    shortName = shortName.Substring(4, 2) +
                                                shortName.Substring(2, 2) +
                                                shortName.Substring(0, 2);
                                    if (shortName.CompareTo(firstDay) < 0 || shortName.CompareTo(lastDay) > 0)
                                        requestedFileNames.Remove(fileName);
                                }
                                goto case "copy";
                            case "copy":
                                int i = 0;
                                FileInfo fileInfo;
                                foreach (String fileName in requestedFileNames)
                                {
                                    fileInfo = new FileInfo(fileName);
                                    Connector.FxClient.UploadFile(destinationPath + fileInfo.Name, fileName);
                                    i++;
                                }
                                SendMessage(true, i + "ADET DOSYA KOPYALANDI");
                                return 1;
                            default:
                                SendMessage(false,"TANIMSIZ FONKSÝYON: " + request.Substring(1, 3));
                                return -1;
                        }
                    //break;

                    case "13": //Promosyon dosyalarýný guncelle
                        Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.SubtotalPromotionFile,
                                                        PosConfiguration.DataPath + Settings.SubtotalPromotionFile);
                        Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.ProductPromotionFile,
                                                        PosConfiguration.DataPath + Settings.ProductPromotionFile);
                        Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.CategoryPromotionFile,
                                                        PosConfiguration.DataPath + Settings.CategoryPromotionFile);
                        SendMessage(true, "PROMOSYON DOSYALARI KOPYALANDI.");
                        return 1;
                    case "14": //Ürün Seri Numaralarý (Serial.dat) Dosyasý

                        Connector.FxClient.DownloadFile(PosConfiguration.ServerDataPath + Settings.SerialNumberFile,
                                                        PosConfiguration.DataPath + Settings.SerialNumberFile);

                        Connector.Instance().LoadSerialNumbers();
                        success = String.Format("SERÝ NUMARA YÜKLEMESÝ: {0:D6} BAÞARILI, {1:D6} BAÞARISIZ",
                            Connector.Instance().GetLastSuccess(DataTypes.Customer),
                            Connector.Instance().GetLastFail(DataTypes.Customer));
                        SendMessage(true, success);
                        return 1;

                    default:
                        return 0;//Operation could not be processed by Data.dll
                }
            }
            catch (Exception e)
            {
                EZLogger.Log.Error("Exception occured. {0}", e);
                try
                {
                    SendMessage(false, e.Message.Replace('\n', '·'));
                }
                catch { }
                return -1;
            }
            //return 0;
        }


        private static void GetDataZip(String zipfile, String targetPath)
        {
            Stream zippedStream = null;
            try
            {
                String tempPath = targetPath + Path.GetFileName(zipfile);
                
                if (File.Exists(tempPath))
                    File.Delete(tempPath);

                File.Copy(zipfile, tempPath);

                zippedStream = File.OpenRead(tempPath);

                Unzip(targetPath, zippedStream);

                zippedStream.Close();

                File.Delete(tempPath);

            }
            finally
            {
                if (zippedStream != null) zippedStream.Close();
            }
        }

        private static void Unzip(string pathToExtract, Stream zippedStream)
        {
            ZipInputStream zipStream = new ZipInputStream(zippedStream);
            ZipEntry entry;
            SuccessCount = 0;
            FailCount = 0;

            while (true)
            {
                entry = zipStream.GetNextEntry();

                if (entry == null) break;

                string directoryName = Path.GetDirectoryName(entry.Name);
                string fileName = Path.GetFileName(entry.Name);

                if (fileName.Trim().Length == 0)
                {
                    continue;//it may be a directory
                    //Directory.CreateDirectory(pathToExtract + directoryName);
                }

                //skip program file
                if (fileName.EndsWith(PosConfiguration.SettingsFile) ||
                    fileName.EndsWith(PosConfiguration.SettingsFile.ToLower()) ||
                    fileName.EndsWith(PosConfiguration.SettingsFile.ToUpper()))
                    continue;

                if (fileName != String.Empty)
                {
                    try
                    {
                        if (File.Exists(pathToExtract + fileName)) File.Delete(pathToExtract + fileName);
                        using (FileStream streamWriter = File.Create(pathToExtract + fileName))
                        {

                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = zipStream.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        SuccessCount++;
                    }
                    catch
                    {
                        FailCount++;
                    }

                }
            }
        }
        private static void TryToWrite(string messageFileName, string contents)
        {
            int tryCounter = 1;
            while (true)
            {
                try
                {
                    Connector.FxClient.UploadString(messageFileName, contents, 4500);
                    lastReadTime = Connector.FxClient.FileGetLastWriteTime(messageFileName);
                    break;
                }
                catch (Exception ex)
                {
                    EZLogger.Log.Info("Could not write to \"{0}\", try {1}", messageFileName, tryCounter++);

                    if (tryCounter > tryLimit) throw ex;

                    System.Threading.Thread.Sleep(20);
                }

            }
        }

        private static string TryToRead(string path)
        {
            int tryCounter = 1;
            while (true)
            {
                try
                {
                    return Connector.FxClient.DownloadString(path, 4500);
                }
                catch (Exception ex)
                {
                    EZLogger.Log.Info("Could not read \"{0}\", try {1}", MessageFileName, tryCounter++);

                    if (tryCounter > tryLimit) throw ex;

                    System.Threading.Thread.Sleep(20);
                }
            }
        }

        internal static void SendWaitingMessage(string message)
        {
            TryToWrite(MessageFileName, SERVER_REQUEST_WAITING + "\n" + message);
        }
        internal static void SendMessage(bool success, string message)
        {
            if (success)
                message = SERVER_REQUEST_COMPLETE + "\n" + message;
            else
                message = SERVER_REQUEST_FAILED + "\n" + message;

            TryToWrite(MessageFileName, message);
        }
    }
}
