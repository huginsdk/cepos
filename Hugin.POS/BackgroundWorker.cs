using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Hugin.POS.States;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS
{
    public delegate void NetworkStatusChangeHandler();

    static class BackgroundWorker
    {
        private static bool blinkOn = false;
        private static Queue<String> serverRequestQueue = new Queue<string>();

        static readonly int inactivityTimeout = int.Parse(PosConfiguration.Get("InactivityTimeout"));

        static bool isConnectionUp = false;
        public static event NetworkStatusChangeHandler OnNetworkUp;
        public static event NetworkStatusChangeHandler OnNetworkDown;

        public static bool IsUp { get { return isConnectionUp; } }

        public static int QueueCount { get { return serverRequestQueue.Count; } }

#if (!WindowsCE)
        private static TcpOrder tcpOrder;
#endif


        internal static void ProcessTcpOrder()
        {
#if (!WindowsCE)
            try
            {
                tcpOrder.Play();//process if order received
            }
            catch { }
#endif
        }

        public static void Start()
        {
            System.Threading.Thread.Sleep(200);
            DocumentFileHelper[] orders = null;
            int sleepTime=5000;
            if (!Parser.TryInt(PosConfiguration.Get("SleepTime"), out sleepTime))
                sleepTime = 5000;

#if (!WindowsCE)
            tcpOrder = new TcpOrder();
#endif
            while (true)
            {
                if (Chassis.Engine.Terminate)
                    break;
                try
                {
                    CheckDataMessage();
                    cr.DataConnector.TransferOfflineData();

                    if (!isConnectionUp)
                    {
                        //CheckDataMessage() didnt throw exception
                        //means backoffice has become online
                        CashRegister.Log.Info("Network up");
                        isConnectionUp = true;
                        OnNetworkUp();
                    }
                }
                catch (BackOfficeUnavailableException bue)
                {
                    CashRegister.Log.Info("ARKA OFÝSE ULAÞILAMIYOR : " + bue.Message);
                    if (isConnectionUp)
                    {
                        isConnectionUp = false;
                        CashRegister.Log.Info("Network down");
                        if (OnNetworkDown != null)
                            OnNetworkDown();
                    }
                }
                catch (Exception e)
                {
                    cr.Log.Error("{0} on BackgroundWorker", e.GetType().ToString());
                }


                if (isConnectionUp)
                {
                    DisplayAdapter.Instance().LedOn(Leds.Online);
                }
                else
                {
                    DisplayAdapter.Instance().LedOff(Leds.Online);
                    DisplayAdapter.Instance().LedOff(Leds.Order);
                }

                try
                {
                    if (isConnectionUp && cr.DataConnector.CurrentSettings.GetProgramOption(Setting.FastPaymentControl) == PosConfiguration.ON)
                        orders = DocumentFileHelper.GetOpenOrders();
                    else if (orders != null)
                        orders = null;
                }
                catch (Exception ex)
                {
                    Debugger.Instance().AppendLine("Order Error: " + ex.Message);
                }
                if (orders != null && orders.Length > 0)
                {
                    DisplayAdapter.Instance().LedOn(Leds.Order);

                    if (PosConfiguration.Get("AutoOrderKey") == GetAutoOrderKey())
                    {
                        try
                        {
                            AutoLoadOrders(orders);
                        }
                        catch (Exception ex)
                        {
                            DisplayAdapter.Instance().LedOff(Leds.Order);
                            Debugger.Instance().AppendLine("Order Error: " + ex.Message);
                        }
                    }
                }
                else
                {
                    DisplayAdapter.Instance().LedOff(Leds.Order);
                }

                Thread.Sleep(sleepTime);
                while (cr.State == null)
                    Thread.Sleep(200); //I wish this had a comment

                TimeSpan ts = DateTime.Now - CashRegisterInput.lastKeyPressed;
                if (cr.State.IsIdle && cr.Document.IsEmpty)
                {
                    if (inactivityTimeout > 0 && ts.TotalSeconds > inactivityTimeout)
                    {
                        if (cr.State is States.Start && cr.CurrentCashier != null)
                        {
                            cr.Log.Info("Timeout occurred - logging out cashier id {0}.", cr.CurrentCashier.Id);
                            cr.State = States.Login.Instance();
                        }
                        //if (cr.IsDesktopWindows)
                        //    DisplayAdapter.Customer.ShowAdvertisement();
                        DisplayAdapter.Instance().Inactive = true;
                    }
                    //if (serverRequestQueue.Count == 0 && DisplayAdapter.Instance().Inactive)
                    //{
                    //    if (cr.DataConnector.CurrentSettings.IdleMessage.Length > 0)
                    //        DisplayAdapter.Customer.ShowAdvertisement();
                    //}
                    CheckMessageQueue();
                }
                else if (inactivityTimeout > 0 && ts.TotalSeconds > inactivityTimeout && cr.CurrentCashier != null)
                {
                    cr.Log.Info("Timeout occurred - logging out cashier id {0}.", cr.CurrentCashier.Id);
                    cr.State = States.Login.Instance();
                }

            }
        }

        internal static string GetAutoOrderKey()
        {
            string pwd = PosConfiguration.Get("FiscalId") + "HUGIN YAZILIM TEKNOLOJILERI";
            string hashkey = PosConfiguration.Get("FiscalId").Substring(2);

            return Encrypt(hashkey, pwd);
        }

        private static string Encrypt(string hashKey, string strQueryStringParameter)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider hash_func = new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] key = hash_func.ComputeHash(Encoding.ASCII.GetBytes(hashKey));
            byte[] IV = new byte[hashKey.Length];

            System.Security.Cryptography.SHA1CryptoServiceProvider sha_func = new System.Security.Cryptography.SHA1CryptoServiceProvider();

            byte[] temp = sha_func.ComputeHash(Encoding.ASCII.GetBytes(hashKey));

            for (int i = 0; i < hashKey.Length; i++)
                IV[i] = temp[hashKey.Length];

            byte[] toenc = System.Text.Encoding.UTF8.GetBytes(strQueryStringParameter);

            System.Security.Cryptography.TripleDESCryptoServiceProvider des = 
                                new System.Security.Cryptography.TripleDESCryptoServiceProvider();
            des.KeySize = 128;
            MemoryStream ms = new MemoryStream();

            System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(
                                                                ms, 
                                                                des.CreateEncryptor(key, IV), 
                                                                System.Security.Cryptography.CryptoStreamMode.Write
                                                                );
            cs.Write(toenc, 0, toenc.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        private static void AutoLoadOrders(DocumentFileHelper[] orders)
        {
            if (cr.State is States.ReportMenu)
                return;
            if (cr.Document != null && cr.Document.Status == DocumentStatus.Active && cr.Document.TotalAmount > 0)
                return;
            if (!(cr.Document is Receipt))
                return;
            if (cr.CurrentCashier == null)
                return;

            int docCount = cr.Printer.CurrentDocumentId;
            foreach (DocumentFileHelper helper in orders)
            {
                /* Check Data Message */
                try
                {
                    CheckDataMessage();
                    
                    ICashier cashier = cr.CurrentCashier;

                    if (serverRequestQueue.Count > 0)
                    {
                        CheckMessageQueue();
                    }
                    if (docCount > 9000) //when number of document for a z exceeds limit print z
                    {
                        States.ReportMenu.PrintZReport();
                        docCount = cr.Printer.CurrentDocumentId;
                    }

                    if (!cr.IsDesktopWindows)
                    {
                        try
                        {
                            if (((docCount % 1000) < 5) && File.Exists(PosConfiguration.LogPath))
                            {
                                File.Delete(PosConfiguration.LogPath);
                            }
                        }
                        catch 
                        {
                            cr.Log.Error("Log file was not deleted.");
                        }
                    }

                    if (cr.CurrentCashier == null)
                    {
                        Login.SignInCashier(cashier);
                        cr.CurrentCashier = cashier;
                        cr.State = States.Start.Instance();
                        return;
                    }
                }
                catch(Exception) { }
                /**/
                docCount++;

                SalesDocument document = null;
                try
                {
                    document = helper.LoadDocument();
                    ICustomer customer = document.Customer;
                    cr.Document = (SalesDocument)document.Clone();
                    cr.Document.Payments.AddRange(document.Payments);
                    foreach (Adjustment adj in document.Adjustments)
                        cr.Document.Adjust(adj);

                    //cr.Document.ResumedFromDocumentId = document.Id;

                    if (customer != null)
                        cr.Document.Customer = customer;
                    try
                    {
                        cr.State = States.KeyInputBlockingError.Instance();
                        cr.ChangeDocumentType();
                    }
                    catch (Exception ex)
                    {
                        cr.Log.Error("Sipariþ Yazdýrma Hatasý: {0}", ex.Message);
                        VoidDocument();
                        if (ex is EJException || ex is NoReceiptRollException)
                        {
                            Error err = new Error(ex);
                            DisplayAdapter.Cashier.Show(err.Message);
                        }
                    }
                    finally
                    {
                        if (!File.Exists(IOUtil.ProgramDirectory + "notdeleteorders.txt"))
                            helper.Remove(document);
                        cr.State = States.Start.Instance();
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (ReceiptLimitExceededException)
                {
                    if (document != null)
                        helper.Remove(document);
                    VoidDocument();
                }
            }
        }
        /// <summary>
        /// to void order document securely, if it is not closed
        /// </summary>
        private static void VoidDocument()
        {
            try
            {
                if (!cr.Document.IsEmpty)
                    cr.Document.Void();
            }
            catch { }
        }
        
        private static void BlinkLed()
        {
            while (blinkOn)
            {
                DisplayAdapter.Cashier.LedOff(Leds.Online);
                System.Threading.Thread.Sleep(250);
                DisplayAdapter.Cashier.LedOn(Leds.Online);
                System.Threading.Thread.Sleep(250);
            }
            DisplayAdapter.Cashier.LedOn(Leds.Online);
        }

        private static void CheckDataMessage()
        {
            string message = cr.DataConnector.BackOfficeCommand;
            /*  0: Received message is not in proper format
             * -1: Error occured while communicating with back office
             */

            //EZLogger.Log.Debug("Background worker message: {0}", message);
            if (message == "-1")
            {
                cr.State = States.AlertCashier.Instance(new Confirm("ARKA OFÝS ÝLE\nHABERLEÞME HATASI"));
                cr.Log.Debug("background working is going on...");
            }
            else if (message == "-2")
            {
                if (serverRequestQueue.Count > 0)
                    serverRequestQueue.Dequeue();//process cancelled by client
            }
            else if (!(message == "0" || serverRequestQueue.Contains(message)))
            {
                serverRequestQueue.Enqueue(message);
                //DisplayAdapter.Cashier.LedOn(Leds.OnlineBlink);
            }
        }
        private static bool isfterZreport = false;

        public static bool IsfterZreport { get => isfterZreport; set => isfterZreport = value; }

        private static void CheckMessageQueue()
        {
            while (serverRequestQueue.Count > 0)
            {
                System.Threading.Thread nThread = new Thread(new ThreadStart(BlinkLed));
                blinkOn = true;
                nThread.Start();

                IState currentState = cr.State;
                cr.State = WaitingState.Instance();
                try
                {
                    String request = serverRequestQueue.Dequeue();

                    cr.DataConnector.SendWaitingMessage("Ýþleminiz yapýlýyor...");

                    if (!DisplayAdapter.Cashier.Inactive)
                        DisplayAdapter.Cashier.Show("ARKA OFÝS\nÝÞLEMÝ YAPILIYOR...");

                    if(!isfterZreport) {// Z raporu alýndýktan hemen sonra bu kontrole gelirse 
                                        //döviz kontrolüne girmeyip normal seyrinde devam edecek. Deðilse döviz için kontrol edecek.
                        
                        if (request.Substring(1, 2) == "10")
                        {

                            try
                            {       //günlük hafýza dolu ise ve son z den sonra dövizli ödemeli bir satýþ varsa 
                                if (!(cr.Printer.DailyMemoryIsEmpty) && cr.Printer.CurrencyPaymentContains())
                                {

                                    cr.DataConnector.SendMessage(false, "Son Z raporundan sonra en az bir dövizli satýþ bulunduðu için dövizler güncellenmemiþtir.");
                                    CashRegister.Log.Info("SON Z SONRASI DÖVÝZLÝ SATIÞ VAR. DöVÝZLER GÜNCELLENMEDÝ.");

                                    cr.State = States.AlertCashier.Instance(new Confirm("DÖVÝZLÝ SATIÞ VAR.\nDÖV. GÜNCELLENMEDÝ.",
                                          new StateInstance(States.WaitingState.Instance),
                                                                 new StateInstance(States.WaitingState.Instance)));


                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                CashRegister.Log.Error(ex.Message.ToString());
                                continue;
                            }
   
                        }
                    }
                    isfterZreport = false;


                    int success = cr.DataConnector.ProcessRequest(request);

                    if (success == 0)//Data.dll is not responsible for request
                    {
                        String failResponse = String.Empty;
                        success = -1;
                        try
                        {
                            ProcessServerRequest(request);
                            success = 1;
                        }
                        catch (PowerFailureException pfe)
                        {
                            try  { Recover.RecoverPowerFailure(pfe); }
                            catch { };
                            Error err = new Error(pfe);
                            failResponse = err.Message.Replace('\n', '·');
                        }
                        catch (LimitExceededOrZRequiredException le)
                        {
                            cr.Log.Warning(le);
                            failResponse = PosMessage.LOGO_DEPARTMENT_CHANGE_Z_REPORT_REQUIRED.Replace('\n', '·');
                        }
                        catch (Exception ex)
                        {
                            Error err = new Error(ex);
                            failResponse = err.Message.Replace('\n', '·');
                        }

                        try
                        {
                            //1 : success, -1 fail
                            if (success == 1)
                            {
                                cr.DataConnector.SendMessage(true, "ÝÞLEM BAÞARIYLA TAMAMLANMIÞTIR.");
                                cr.Log.Success("Office request \"{0}\" completed", request.TrimEnd('0', '\n', '\r'));
                            }
                            else
                            {
                                cr.DataConnector.SendMessage(false, failResponse);
                                cr.Log.Error("Office request \"{0}\" failed: {1}", request.TrimEnd('0', '\n', '\r'), failResponse);
                            }
                        }
                        catch { }
                    }
                    if (!DisplayAdapter.Instance().Inactive)
                    {
                        if (success == -1)
                            DisplayAdapter.Cashier.Show("ARKA OFÝS ÝÞLEMÝ\nBAÞARISIZ OLDU!..");
                        else
                            DisplayAdapter.Cashier.Show("ARKA OFÝS ÝÞLEMÝ\nBAÞARIYLA TAMAMLANDI");
                    }

                }
                finally
                {
                    if (!(cr.State is ElectronicJournalError))
                    {
                        if (cr.State is States.Login)
                            currentState = States.Login.Instance();
                        else
                            cr.State = States.Start.Instance();
                        if (!(currentState is WaitingState))
                            cr.State = currentState;
                    }
                }
            }
            blinkOn = false;
        }

        internal static void ProcessServerRequest(String request)
        {
            cr.Printer.AdjustPrinter(new Receipt());
            int timeInt;
            string requestCode = request.Substring(1, 2);
            switch (requestCode)
            {
                case "01"://X Raporu
                    bool hardcopy = false;
                    hardcopy = Data.Connector.Instance().CurrentSettings.GetProgramOption(Setting.PrintReportHardcopy) == PosConfiguration.ON;
                    States.ReportMenu.PrintXReport(hardcopy);
                    break;
                case "02"://Z Raporu
                    States.ReportMenu.PrintZReport();
                    break;
                case "03": //M.Bellek Raporu (Z No)
                    int startZNo = Int32.Parse(request.Substring(3, 4));
                    int endZNo = Int32.Parse(request.Substring(7, 4));
                    States.ReportMenu.PrintPeriodicReportByZNumber(startZNo, endZNo, false);
                    break;
                case "04": //M.Bellek Raporu(Z Date)                                
                    String dateStr = ReportMenu.FormatDate(request.Substring(3, 6).Insert(4, String.Format("{0}", DateTime.Now.Year / 100)));
                    DateTime startZDate = DateTime.Parse(dateStr, PosConfiguration.CultureInfo.DateTimeFormat);
                    dateStr = ReportMenu.FormatDate(request.Substring(9, 6).Insert(4, String.Format("{0}", DateTime.Now.Year / 100)));
                    DateTime endZDate = DateTime.Parse(dateStr, PosConfiguration.CultureInfo.DateTimeFormat);
                    States.ReportMenu.PrintPeriodicReportByDate(startZDate, endZDate, false);
                    break;
                case "05": //Odeme Raporu
                    hardcopy = Data.Connector.Instance().CurrentSettings.GetProgramOption(Setting.PrintReportHardcopy) == PosConfiguration.ON;
                    States.ReportMenu.PrintRegisterReport(hardcopy);
                    break;
                case "06": //Yeni Program
                    //and check new settings
                    cr.DataConnector.GetNewProgram();
                    try
                    {
                        CashRegister.LoadNewSettings();
                        CashRegister.LoadCurrentSettings();
                    }
                    catch (FileNotFoundException)
                    {
                    }
                    break;
                case "12": //Yeni exe

                    if (Data.Connector.FxClient.DirectoryExists(PosConfiguration.ServerUpgradePath, 4500))
                    {
                        IState quitState = States.Login.Quit();
                        break;
                    }
                    throw new Exception("Yeni exe yüklenemedi.");
                case "15": //Saat ayarla
                    IState tempstate = SetupMenu.SetRegisterTime(int.Parse(request.Substring(9, 4)));
                    break;
                case "16": //Kasiyere mesaj
                    String cashierMessage = request.Substring(3);
                    DisplayAdapter.Cashier.Show(cashierMessage);
                    System.Threading.Thread.Sleep(2000);
                    break;
                case "19": //Kasayi ac 191 veya kilitle 190
                    if (cr.CurrentCashier == null)
                        throw new Exception("Kasiyer giriþi yok");
                    switch (request[3])
                    {
                        case '0':
                            cr.State = Login.Instance();
                            break;
                        case '1':
                            for (int c = 0; c < cr.CurrentCashier.Password.Length; c++)
                            {
                                cr.State.Numeric(cr.CurrentCashier.Password[c]);
                            }
                            cr.State.Enter();
                            break;
                    }
                    break;
                case "20": //Ekü Tek Belge / Z No & Fiþ No
                    startZNo = Int32.Parse(request.Substring(3, 4));
                    int docId = Int32.Parse(request.Substring(7, 4));
                    States.ReportMenu.PrintEJDocumentById(startZNo, docId, false);
                    break;
                case "21": //Ekü Tek Belge / Tarih & Saat 
                    dateStr = ReportMenu.FormatDate(request.Substring(3, 6).Insert(4, String.Format("{0}", DateTime.Now.Year / 100)));
                    startZDate = DateTime.Parse(dateStr, PosConfiguration.CultureInfo.DateTimeFormat);
                    timeInt = int.Parse(request.Substring(9, 4));
                    if (timeInt == -1) timeInt = 0;
                    startZDate = startZDate.AddHours(timeInt / 100).AddMinutes(timeInt % 100);
                    States.ReportMenu.PrintEJDocumentByTime(startZDate, false);
                    break;
                case "22": //Ekü Dönemsel / Ýki Z No Arasý
                    startZNo = Int32.Parse(request.Substring(3, 4));
                    int startDocId = Int32.Parse(request.Substring(7, 4));
                    endZNo = Int32.Parse(request.Substring(11, 4));
                    int endDocId = Int32.Parse(request.Substring(15, 4));
                    States.ReportMenu.PrintEJPeriodicByZNumber(startZNo, startDocId, endZNo, endDocId, false);
                    break;
                case "23": // Ekü Dönemsel / Ýki Tarih Arasý
                    dateStr = ReportMenu.FormatDate(request.Substring(3, 6).Insert(4, String.Format("{0}", DateTime.Now.Year / 100)));
                    startZDate = DateTime.Parse(dateStr, PosConfiguration.CultureInfo.DateTimeFormat);
                    timeInt = int.Parse(request.Substring(9, 4));
                    if (timeInt == -1) timeInt = 0;
                    startZDate = startZDate.AddHours(timeInt / 100).AddMinutes(timeInt % 100);

                    dateStr = ReportMenu.FormatDate(request.Substring(13, 6).Insert(4, String.Format("{0}", DateTime.Now.Year / 100)));
                    endZDate = DateTime.Parse(dateStr, PosConfiguration.CultureInfo.DateTimeFormat);
                    timeInt = int.Parse(request.Substring(19, 4));
                    if (timeInt == -1) timeInt = 0;
                    endZDate = endZDate.AddHours(timeInt / 100).AddMinutes(timeInt % 100);

                    States.ReportMenu.PrintEJPeriodicByDate(startZDate, endZDate, false);
                    break;
                case "24": //Ekü Dönemsel / Günlük
                    try
                    {
                        dateStr = ReportMenu.FormatDate(request.Substring(3, 6).Insert(4, String.Format("{0}", DateTime.Now.Year / 100)));
                        startZDate = DateTime.Parse(dateStr, PosConfiguration.CultureInfo.DateTimeFormat);
                        States.ReportMenu.GetEJDaily(startZDate, false);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Instance().AppendLine(ex.Message);
                    }
                    break;
                case "25": // Print sale report by cashier id and datetime
                    hardcopy = Data.Connector.Instance().CurrentSettings.GetProgramOption(Setting.PrintReportHardcopy) == PosConfiguration.ON;

                    dateStr = ReportMenu.FormatDate(request.Substring(3, 6).Insert(4, String.Format("{0}", DateTime.Now.Year / 100)));
                    startZDate = DateTime.Parse(dateStr, PosConfiguration.CultureInfo.DateTimeFormat);
                    timeInt = int.Parse(request.Substring(9, 4));
                    if (timeInt == -1) timeInt = 0;
                    startZDate = startZDate.AddHours(timeInt / 100).AddMinutes(timeInt % 100);

                    dateStr = ReportMenu.FormatDate(request.Substring(13, 6).Insert(4, String.Format("{0}", DateTime.Now.Year / 100)));
                    endZDate = DateTime.Parse(dateStr, PosConfiguration.CultureInfo.DateTimeFormat);
                    timeInt = int.Parse(request.Substring(19, 4));
                    if (timeInt == -1) timeInt = 0;
                    endZDate = endZDate.AddHours(timeInt / 100).AddMinutes(timeInt % 100);
                    States.ReportMenu.PrintRegisterReportSummary(cr.CurrentCashier, startZDate, endZDate, hardcopy);

                    break;
                default:
                    throw new Exception("Tanimsiz fonskiyon: " + request.Substring(1, 2));
            }

        }

        internal static void DownloadAssemblyFiles()
        {
            if (Data.Connector.FxClient.DirectoryExists(PosConfiguration.ServerUpgradePath, 4500))
            {
                List<String> files = new List<string>(PosConfiguration.ReferencedAssemlies);
                foreach (String name in files)
                {
                    try
                    {
                        if (Data.Connector.FxClient.FileExists(PosConfiguration.ServerUpgradePath + name))
                        {
                            if (File.Exists(PosConfiguration.UpgradePath + name))
                                File.Delete(PosConfiguration.UpgradePath + name);
                            Data.Connector.FxClient.DownloadFile(PosConfiguration.ServerUpgradePath + name,
                                                                 PosConfiguration.UpgradePath + name);
                        }
                    }
                    catch (Exception ex)
                    {
                        cr.Log.Error("File couldnot download: " + name);
                        cr.Log.Error(ex);
                    }
                }
            }
        }
    }
}
