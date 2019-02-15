using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;
using System.Reflection;

namespace Hugin.POS.Data
{
    internal enum LogType
    {
        Main,
        Void,
        Return,
        LastDocument
    }
    class Logger
    {
        internal const string MainLogName = "HAREKET";
        internal const string MainLogPrefix = "HR";
        internal const string VoidedLogName = "HRIPTAL";
        internal const string VoidedLogPrefix = "HI";
        internal const string ReturnsLogName = "HRIADE";
        internal const string ReturnsLogPrefix = "HD";
        internal const string DocumentLogName = "HRBELGE";

        String mainLogPath, dailyLogPrefix;
        
        protected LogType logType;

        readonly static String suffix = "." + PosConfiguration.Get("RegisterId");

        protected static int lastZNo;
        protected static int lastDocumentId;
        protected static ICashier currentCashier = null;

        private Logger logFormatter = null;
        private static Logger defaultFormatter = null;

        int sequenceNumber = 1;
        
        public Logger()
        {

        }
        internal Logger(LogType type)
        {
            this.logType = type;

            switch (type)
            {
                case LogType.Main:
                    this.mainLogPath = PosConfiguration.ArchivePath + MainLogName;
                    this.dailyLogPrefix = MainLogPrefix;
                    break;
                case LogType.Void:
                    this.mainLogPath = PosConfiguration.ArchivePath + VoidedLogName;
                    this.dailyLogPrefix = VoidedLogPrefix;
                    break;
                case LogType.Return:
                    this.mainLogPath = PosConfiguration.ArchivePath + ReturnsLogName;
                    this.dailyLogPrefix = ReturnsLogPrefix;
                    break;
                case LogType.LastDocument:
                    this.mainLogPath = PosConfiguration.ArchivePath + DocumentLogName;
                    break;
            }
            sequenceNumber = GetSequenceNumber(mainLogPath + suffix) + 1;

            if (PosConfiguration.Get("Logger") == "1" && type != LogType.LastDocument)
                logFormatter = new InterLoger();
            else
                logFormatter = new HuginLogger();

            defaultFormatter = new HuginLogger();
        }

        internal int SequenceNumber
        {
            get { return sequenceNumber; }
            set { sequenceNumber = value; }
        }

        internal static Logger LogFormatter
        {
            get { return Logger.defaultFormatter; }
        }

        internal void SaveLog(string logText)
        {
            StreamWriter localStream = null;
            StreamWriter transferStream = null;
            List<String> localFiles = new List<String>();
            List<String> transferFiles = new List<String>();

            String dailyLogPath = String.Format("{0}{1}{2:ddMMyy}", PosConfiguration.ArchivePath, dailyLogPrefix, DateTime.Today);

            localFiles.Add(mainLogPath + suffix);
            localFiles.Add(dailyLogPath + suffix);
            transferFiles.Add(mainLogPath + ".xfr");
            transferFiles.Add(dailyLogPath + ".xfr");

            foreach (String filePath in localFiles)
            {
                try
                {
                    using (localStream = new StreamWriter(filePath, true, PosConfiguration.DefaultEncoding))
                    {
                        localStream.WriteLine(logText);
                    }
                }
                catch (Exception ex)
                {
                    EZLogger.Log.Fatal("Failed to write logfiles. Error: {0}", ex.Message);
                    EZLogger.Log.Fatal("File : {0}", filePath);
                    EZLogger.Log.Fatal("Message: {0}", logText);
                }
                finally
                {
                    if (localStream != null) localStream.Close();
                }
            }

            foreach (String filePath in transferFiles)
            {
                try
                {
                    lock (Connector.TransferLock)
                    {
                        transferStream = new StreamWriter(filePath, true, PosConfiguration.DefaultEncoding);
                        transferStream.WriteLine(logText);
                    }
                }
                catch (Exception ex)
                {
                    EZLogger.Log.Fatal("Failed to write transfer file. Error: {0}", ex.Message);
                    EZLogger.Log.Fatal("File: {0}", filePath);
                    EZLogger.Log.Fatal("Message: {0}", logText);
                }
                finally
                {
                    if (transferStream != null) transferStream.Close();
                }
            }
        }

        internal string OnDocumentClosed(ISalesDocument document)
        {
            if (document.Id == 0 || document.IsEmpty)
                return "";

            //every document knows his sequence number, switch is not required
            int docStatus = this.logType == LogType.Main ? 0 : 1;// 0 : Sale, 1: Void
            StringWriter logWriter = logFormatter.LogItems(document, docStatus, ref sequenceNumber);

            SaveLog(logWriter.ToString());

            return logWriter.ToString();
        }

        internal string OnDocumentVoided(ISalesDocument document,int voidedReason)
        {
            if (document.Id == 0 || document.IsEmpty)
                return "";

            StringWriter logWriter = logFormatter.LogItems(document, voidedReason, ref sequenceNumber);

            SaveLog(logWriter.ToString());

            return logWriter.ToString();
        }

        internal void OnDocumentChanged(ISalesDocument document, int documentStatus)
        {
            if (document == null || 
                document.Id == 0 || 
                document.IsEmpty)
            {
                AppendDocumentLog(""); 
                return;
            }

            StringWriter logWriter = logFormatter.LogItems(document, documentStatus, ref sequenceNumber);
            AppendDocumentLog(logWriter.ToString());
        }

        private void AppendDocumentLog(string logText)
        {
            String path = String.Format("{0}{1}", mainLogPath, suffix);

            using (StreamWriter mainLocal = new StreamWriter(path, false, PosConfiguration.DefaultEncoding))
            {
                try
                {
                    mainLocal.WriteLine(logText);
                }
                catch (Exception)
                {
                    EZLogger.Log.Fatal("Failed to write last document (HRBELGE)");
                }
                finally
                {
                    if (mainLocal != null) mainLocal.Close();
                }
            }
        }

        internal void OnDocumentSuspended(ISalesDocument document, int zReportNo)
        {
            if (document.Id == 0 || document.IsEmpty)
                return;
            
            int refNum = 0;

            StringWriter logWriter = logFormatter.LogItems(document, 4, ref refNum);
        
            StreamWriter sw = null;

            String suspendLogPath = String.Format("{0}BEK{1:D4}{2:D4}{3}", PosConfiguration.ArchivePath, zReportNo, document.Id, suffix);

            // Art arta beklenen belge iþlemlerinde doc id ayný geleceði için (header basýlý doc ýd sabit) böyle bir yola gittik.
            // Üst üste append yapmamasý için
            int docId = document.Id;
            while(File.Exists(suspendLogPath))
            {
                docId++;
                suspendLogPath = String.Format("{0}BEK{1:D4}{2:D4}{3}", PosConfiguration.ArchivePath, zReportNo, docId, suffix);
            }

            try
            {
                using (sw = new StreamWriter(suspendLogPath, true, PosConfiguration.DefaultEncoding))
                {
                    sw.WriteLine(logWriter.ToString());
                }
            }
            finally
            {
                if (sw != null) sw.Close();
            }
        }

        internal static void SaveReport(string fileName, string logText)
        {
            String local = PosConfiguration.ReportPath + fileName;
            String transfer = PosConfiguration.ReportPath + fileName;
            String dateStr = PosConfiguration.ReportPath;
            StringBuilder reportHeader = new StringBuilder();
            StringBuilder reportFooter = new StringBuilder();

            if (!fileName.StartsWith("ZRAP"))
                dateStr += String.Format("{0}-{1:ddMMyyyyHHmmss}", fileName, DateTime.Now);
            else dateStr += "ZRAPORU";
            if (fileName.StartsWith("EKURAPORU"))
                dateStr = "";
            IOUtil.AppendAllText(local + suffix, logText, PosConfiguration.DefaultEncoding);
            if (dateStr != "")
                IOUtil.WriteAllText(dateStr + suffix, logText, PosConfiguration.DefaultEncoding);

            lock (Connector.TransferLock)
            {
                IOUtil.WriteAllText(transfer + ".xfr", logText, PosConfiguration.DefaultEncoding);
                if (dateStr != "")
                    IOUtil.WriteAllText(dateStr + ".xfr", logText, PosConfiguration.DefaultEncoding);
            }
        }

        internal void OnCashierLogin(ICashier cashier, int zReportNo)
        {
            String logText = String.Format("1,{0},14,LIN,        {1},{2:HH:mm:ss}", sequenceNumber++.ToString().PadLeft(5, '0'),
                                                                       cashier.Id,
                                                                       DateTime.Now);
            SaveLog(logText.PadRight(40));

            currentCashier = cashier;
            lastZNo = zReportNo;
        }
     
        internal void OnCashierLogout(ICashier cashier)
        {
            String logText = String.Format("1,{0},15,LOT,        {1},{2:HH:mm:ss}", sequenceNumber++.ToString().PadLeft(5, '0'),
                                                                        cashier.Id,
                                                                        DateTime.Now);
            SaveLog(logText.PadRight(40));

            currentCashier = null;
        }

        internal void OnDeposit(Decimal amount)
        {
            String logText = String.Format("1,{0},13,KGR,            ,  {1:D10}", sequenceNumber++.ToString().PadLeft(5, '0'),
                                                                         (long)Math.Round(amount * 100m));
            SaveLog(logText.PadRight(40));
        }

        internal void OnWithdrawal(Decimal amount)
        {
            String logText = String.Format("1,{0},12,KCK,            ,  {1:D10}", sequenceNumber++.ToString().PadLeft(5, '0'),
                                                                         (long)Math.Round(amount * 100m));
            SaveLog(logText.PadRight(40));
        }

        internal void OnWithdrawal(Decimal amount, String refNumber)
        {
            //do nothing on this dll
        }

        internal void OnWithdrawal(Decimal amount, ICredit credit)
        {
            //do nothing on this dll
        }
        
        internal void CheckZWritten(int zNo, int documentId)
        {
            int indexCode = 8;
            int indexPrm2 = 28;

            lastDocumentId = documentId;
            lastZNo = zNo;

            String[] lines = IOUtil.ReadAllLines(mainLogPath + suffix);
            int i = lines.Length - 1;
            while(i > 0)
            {
                
                String line = lines[i];

                try
                {
                    if (line.Substring(indexCode, 2) == "16")//ZRP
                    {
                        break;
                    }
                    if ((line.Substring(indexCode, 2) == "01") || //receipt
                        (line.Substring(indexCode, 2) == "02"))//slip document
                    {
                       //z must be between bigger doc and smaller doc
                        if (Convert.ToInt32(line.Substring(indexPrm2, 6)) > documentId)
                        {
                            OnZReportComplete(zNo, DateTime.Now);//add zrp line
                        }
                        break;
                    }
                }
                catch { }
                i--;

            }

        }

        internal void OnZReportComplete(int ZReportNo, DateTime ZReportDate)
        {
            //1,rrrrr,16,ZRP,DD/MM/YYYY  ,HH:MM:SS
            String logText = String.Format("1,{0},16,ZRP,{1:dd}/{1:MM}/{1:yyyy}  ,{1:HH:mm:ss}", sequenceNumber++.ToString().PadLeft(5, '0'),
                                                                            ZReportDate);
            SaveLog(logText.PadRight(40));
            lastZNo = ZReportNo-1;
            sequenceNumber = 1;

        }
   
        internal void OnNetworkDown()
        {
            //StreamWriter logWriter = GetLogWriter();
            //1,rrrrr,17,NET,DD/MM/YYYY  ,HH:MM:SS
            String logText = String.Format("1,{0},17,NET,{1:dd}/{1:MM}/{1:yyyy},{1:HH:mm:ss}", sequenceNumber++.ToString().PadLeft(5, '0'),
                                                                            DateTime.Now);
            SaveLog(logText.PadRight(40));
        }

        internal virtual StringWriter LogItems(ISalesDocument document, int docStatus, ref int number)
        {
            return defaultFormatter.LogItems(document, docStatus, ref number);
        }

        internal void ResetSequenceNumber()
        {
            sequenceNumber = 1;
        }

        internal int GetSequenceNumber(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (FileStream fs = File.OpenRead(fileName))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        if (fs.Length - 50 < 0)
                        {
                            return -1;
                        }
                        sr.BaseStream.Position = fs.Length - 50;
                        sr.ReadLine();
                        string[] items = sr.ReadToEnd().Split(',');
                        int seqNumber = -1;
                        if (items.Length > 1) Parser.TryInt(items[1], out seqNumber);
                        return seqNumber;
                    }
                }

            }
            return -1;
        }

        internal void PrintExeVersion(String exeVersion)
        {
            System.IO.FileInfo programExe = new System.IO.FileInfo(IOUtil.ProgramDirectory + IOUtil.AssemblyName);
            String logText = String.Format("1,{0},23,VER,{1:dd}/{1:MM}/{1:yyyy}  ,{2}", sequenceNumber++.ToString().PadLeft(5, '0'),
                                                           programExe.CreationTime,
                                                           exeVersion);
            SaveLog(logText.PadRight(40));
        }
    }
}
