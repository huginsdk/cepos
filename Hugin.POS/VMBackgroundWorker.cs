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

        internal static void ProcessTcpOrder()
        {

        }

        public static void Start()
        {
            System.Threading.Thread.Sleep(200);
            DocumentFileHelper[] orders = null;

#if (WindowsCE)
            try
            {
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"ControlPanel\BackLight", true).SetValue("BatteryTimeOut", inactivityTimeout, Microsoft.Win32.RegistryValueKind.DWord);
            }
            finally { }
#endif
            while (true)
            {
                if (Chassis.Engine.Terminate)
                    break;
                try
                {
                    CheckDataMessage();
                }
                catch (BackOfficeUnavailableException boe)
                {
                    CashRegister.Log.Info("Back office unavailable {0}", boe.Message);
                }
                catch (Exception e)
                {
                    cr.Log.Error("{0} on BackgroundWorker", e.GetType().ToString());             
                }
                
                Thread.Sleep(5000);
                while (cr.State == null)
                    Thread.Sleep(200); //I wish this had a comment

                if (cr.State.IsIdle && cr.Document.IsEmpty)
                {
                    CheckMessageQueue();
                }
                
            }
        }

        /// <summary>
        /// todo: blink can be used to inform front office while processing data
        /// </summary>
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
            EZLogger.Log.Debug("Background worker message: {0}", message);
            if (message == "-1")
                CashRegister.State = States.AlertCashier.Instance(new Confirm("ARKA OFÝS ÝLE\nHABERLEÞME HATASI"));
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
            switch (request.Substring(1, 2))
            {
                case "01"://X Raporu
                    States.ReportMenu.PrintXReport(false);
                    break;
                case "02"://X Raporu
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
                    States.ReportMenu.PrintRegisterReport(false);
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
                /*case "20"://ej single document
                    try
                    {
                        int zNo = Convert.ToInt32(request.Substring(3, 4));
                        int docId = Convert.ToInt32(request.Substring(7, 4));
                        ReportMenu.PrintEJDocumentById(zNo, docId);
                    }
                    catch { throw new Exception("Rapor alýnamadý."); }
                    break;*/
                default:
                    throw new Exception("Tanimsiz fonskiyon: " + request.Substring(1, 2));
            }

        }


        internal static void DownloadAssemblyFiles()
        {
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
    }
}
