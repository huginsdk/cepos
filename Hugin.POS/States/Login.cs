using System;
using System.Text;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using Hugin.POS.Common;
using System.Net;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml.Linq;
using System.Xml;

namespace Hugin.POS.States
{
    public delegate void LoginEventHandler(ICashier sender);
    class Login : SilentState
    {
        private const int DEFAULT_MANAGER_ID = 9;
        private const string DEFAULT_MANAGER_PWD = "14710";

        private const string UPDATE_FOLDER_NAME = "CEPOSNG_UPDATE";
        private const string FILES_TO_UPDATE = "FILES_TO_UPDATE.xml";

        private const int MAX_DWNLD_TIME = 120000;  // Max waiting period of time for download operation

        private static IState state = new Login();
        private static StateInstance ReturnConfirm;
        private static StateInstance ReturnCancel;
        private static StringBuilder password = new StringBuilder();
        private static int managerId = -1;
        private int loginAttempt = 0; //kasiyer sifre deneme sayisi
        private static int totalLogins = -1;
        private static ICashier manager = null;

        public static Object ScrollLock = new Object();
        public static event LoginEventHandler OnLogin;
        public static event LoginEventHandler OnLogout;

        public override bool IsIdle
        {
            get
            {
                return true;
            }
        }

        public static bool AutoLogin = true;

        public static IState Instance()
        {
            if (manager != null)
            {
                return InstanceAfterMngr();
            }
            managerId = -1;

#if ORDER
            return States.EnterPassword.Instance(PosMessage.ENTER_PASSWORD, CheckCashier2, Instance);
#else
            if (!AutoLogin)
            {
                /* Standart Login */
                AutoLogin = true;
                return States.EnterInteger.Instance(PosMessage.ENTER_MANAGER_ID, CheckManagerId);
            }
            else
            {
                /* Auto Login */

                string pwd = "";
                bool definedMngr = false;
                try
                {
                    managerId = int.Parse(PosConfiguration.DefaultManagerID);
                    pwd = PosConfiguration.DefaultManagerPWD;
                    definedMngr = true;
                }
                catch
                {
                    definedMngr = false;
                }

                if (definedMngr && !String.IsNullOrEmpty(pwd))
                {
                    return AcceptManager(pwd);
                }
                else
                {
                    Confirm err = new Confirm(String.Format("{0}\n{1}", PosMessage.NO_DEFINED_MANAGER, PosMessage.DEFAULT_LOGIN),
                            new StateInstance(DefaultLogin), new StateInstance(Instance));
                    return States.AlertCashier.Instance(err);
                }
            }
#endif
        }
        public override void Numeric(char c)
        {
            if (!cr.SecurityConnector.AcceptsPassword)
                return;
            password.Append(c.ToString());
            if (password.Length == 1 && loginAttempt != 0)
            {
                DisplayAdapter.Cashier.Show(PosMessage.ENTER_PASSWORD);
            }
            else if (password.Length == 6)
            {
                loginAttempt++;

                if (CheckCashier(password.ToString()) != null)
                {
                    loginAttempt = 0;
                    password = new StringBuilder();
                    return;
                }
                else if (loginAttempt == 3)
                {
                    Confirm err = new Confirm(String.Format("{0}\n{1}", PosMessage.INVALID_PASSWORD, PosMessage.REGISTER_LOCKED),
                        new StateInstance(States.LoginLocked.Instance));

                    password = new StringBuilder();
                    loginAttempt = 0;
                    cr.State = States.AlertCashier.Instance(err);
                    return;
                }
                else
                {
                    DisplayAdapter.Cashier.Show("{0}\n{1}", PosMessage.INVALID_PASSWORD, PosMessage.PROMPT_RETRY);
                    password = new StringBuilder();
                    return;
                }

            }

            DisplayAdapter.Cashier.Append("*");
        }
        private static IState InstanceAfterMngr()
        {
            DisplayAdapter.Customer.Show(PosMessage.WELCOME_LOCKED);
            DisplayAdapter.Cashier.Show(cr.SecurityConnector.CashierMessage);
            password.Length = 0;

            try
            {
                CheckAutoLogin(cr.SecurityConnector.CheckAutoLogin());
            }
            catch { }

            return state;
        }

        private static IState CheckManagerId(int id)
        {
            if (id == cr.MAX_CASHIER_ID)
            {
                return States.AlertCashier.Instance(new Confirm(PosMessage.INVALID_MANAGER_ID,
                                                new StateInstance(Instance)));
            }

            if (id < 1 || id > cr.MAX_MANAGER_ID)
            {
                return States.AlertCashier.Instance(new Confirm(PosMessage.INVALID_MANAGER_ID,
                                            new StateInstance(Instance)));
            }

            managerId = id;
            cr.State = States.EnterPassword.Instance(PosMessage.ENTER_PASS_MNGR, AcceptManager, new StateInstance(Instance));
            return cr.State;
        }

        private static IState DefaultLogin()
        {
            managerId = DEFAULT_MANAGER_ID;
            return AcceptManager(DEFAULT_MANAGER_PWD);
        }

        private static int tryCount = 0;
        private static IState AcceptManager(String mngrPwd)
        {
            DisplayAdapter.Cashier.Show("YÖNETÝCÝ GÝRÝÞÝ\nYAPILIYOR...");
            System.Threading.Thread.Sleep(500);

            try
            {
                manager = cr.Printer.SignInCashier(managerId, mngrPwd);
                cr.CurrentManager = manager;

                //Settings should be reloaded on first cashier login on
                if (totalLogins++ == -1 || manager.AuthorizationLevel == AuthorizationLevel.P)
                {
                    //compare currents settings file with printer
                    CashRegister.LoadCurrentSettings();
                    //and check new settings
                    try
                    {
                        cr.LastZReportDate = cr.Printer.LastZReportDate;
                        CashRegister.LoadNewSettings();
                    }
                    catch (FileNotFoundException)
                    {
                        //this is not a problem
                    }
                    catch (Exception)
                    {
                        //but think about this:
                        //if new program file is invalid when program starts, what should be done?
                    }

                    //program start and after on each Z report
                    ReportMenu.OnZReportComplete += new ZEventHandler(ResetLoginCount);
                }

                cr.State = InstanceAfterMngr();
            }
            catch (FMNewException fmne)
            {
                cr.Log.Error(fmne);
                int fiscalId = int.Parse(PosConfiguration.Get("FiscalId").Substring(2, 8));
                cr.State = States.EnterInteger.Instance(PosMessage.START_FM, fiscalId,
                                                        new StateInstance<int>(AcceptFiscalId),
                                                        new StateInstance(Instance));
            }
            catch (MissingCashierException mce)
            {
                cr.Log.Error(mce);
                int loginAttemptCount = int.Parse(mce.Message);

                if (loginAttemptCount == cr.MAX_PASSWORD_TRY)
                {
                    cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.MANAGER_LOGIN_LOCKED,
                                                                        new StateInstance(Instance)));
                }
                else
                {
                    cr.State = States.ConfirmCashier.Instance(new Confirm(String.Format("{0}\n{1}: {2}", PosMessage.INVALID_PASS_ENTRY,
                                                                                                       PosMessage.RIGHT_TO_REST,
                                                                                                       (5 - loginAttemptCount).ToString()),
                                                                        new StateInstance(Instance)));
                }
            }
            catch (CashierAutorizeException cae)
            {
                cr.Log.Error(cae);
                cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.NO_AUTHORIZATION_FOR_LOAD_SETTINGS,
                                                new StateInstance(LogoutManager),
                                                new StateInstance(Instance)));
            }
            catch (LimitExceededOrZRequiredException lzre)
            {
                cr.Log.Error(lzre);
                cr.State = States.ConfirmCashier.Instance(new Error(
                                                            lzre,
                                                            new StateInstance(PrintZReport),
                                                            new StateInstance(EscapeZReport)
                                                            ));
            }
            catch (BlockingException be)
            {
                cr.Log.Error(be);
                cr.State = PrinterBlockingError.Instance(new Error(be));
            }
            catch (ZRequiredException zre)
            {
                cr.Log.Error(zre);
                if (zre.ErrorCode == 134)
                    cr.State = States.ConfirmCashier.Instance(new Error(
                                                            zre,
                                                            new StateInstance(SendInterrupt),
                                                            new StateInstance(EscapeZReport)
                                                            ));
                else
                    cr.State = States.ConfirmCashier.Instance(new Error(
                                                            zre,
                                                            new StateInstance(PrintZReport),
                                                            new StateInstance(EscapeZReport)
                                                            ));
            }
            catch (Exception ex)
            {
                if (tryCount == 0)
                {
                    tryCount++;
                    return Instance();
                }

                cr.Log.Error(ex);
                cr.State = States.ConfirmCashier.Instance(new Error(ex,
                                                    new StateInstance(Instance)));
            }
            finally
            {
                managerId = -1;
            }

            return cr.State;
        }

        internal static IState LogoutManager()
        {
            manager = null;
            return Instance();
        }

        public static IState AcceptFiscalId(int fiscalId)
        {
            if (fiscalId <= 0 || fiscalId > 99999999)
            {
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.INVALID_FISCAL_ID,
                                                        new StateInstance(Instance)));
            }
            else
            {
                try
                {
                    cr.Printer.StartFM(fiscalId);

                    cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.FM_STARTED_SUCCSFLY,
                                                            new StateInstance(Instance)));
                    PosConfiguration.Set("FiscalId", String.Format("FA{0:D8}", fiscalId));

                }
                catch (PrinterException ex)
                {
                    if (ex.ErrorCode == 22)
                    {
                        cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.CERTIFICATES_CANNOT_UPLOAD));
                    }
                    else
                    {
                        cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.FM_CANNOT_STARTED));
                    }

                    cr.State = States.EnterInteger.Instance(PosMessage.START_FM, fiscalId,
                                                            new StateInstance<int>(AcceptFiscalId),
                                                            new StateInstance(Instance));
                }
            }

            return cr.State;
        }

        private static IState CheckCashier2(String password)
        {
            ICashier cashier = CheckCashier(password);

            return cr.State;

        }
        private static ICashier CheckCashier(String password)
        {
            ICashier cashier = null;
            try
            {
                cashier = cr.SecurityConnector.LoginCashier(String.Format("{0:D6}", password));
            }
            catch (Exception ex)
            {
                cr.State = States.AlertCashier.Instance(new Error(ex));
            }

            if (cashier == null)
                return cashier;

            if (cashier.AuthorizationLevel == AuthorizationLevel.Seller)
            {
                cr.State = States.AlertCashier.Instance(
                    new Error(
                    new Exception(
                    String.Format("{0}\n{1}", cashier.Name.Trim(), PosMessage.NO_ACCESS_RIGHT))
                    )
                    );
                return null;
            }

            cr.CurrentCashier = cashier;

            Confirm err = new Confirm(String.Format("{0}\n{1}\t{2}", cr.CurrentCashier.Name,
                                       PosMessage.ACCESS_LEVEL,
                                       cr.CurrentCashier.AuthorizationLevel),
                                       new StateInstance(LoginCashier),
                                       new StateInstance(EscapeCashier));

            cr.State = States.ConfirmCashier.Instance(err);

            // Set cashier name to ECR on current manager
            try
            {
                cr.Printer.SaveCashier(int.Parse(manager.Id), cashier.Name);
            }
            catch { }

            return cashier;

        }

        public override void Escape()
        {
            password.Length = 0;
            if (cr.CurrentCashier == null)
            {
                cr.State = Instance();
            }
            if (ReturnCancel != null)
            {
                cr.State = ReturnCancel();
                ReturnCancel = null;
            }
        }

        public override void Command()
        {
            if (cr.IsDesktopWindows)
            {
                MenuList menuHeaders = new MenuList();
                int index = 1;
                menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.SYSTEM_MANAGER, index++, PosMessage.SHUTDOWN_POS)));
                menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.SYSTEM_MANAGER, index++, PosMessage.RESTART_POS)));
                menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.SYSTEM_MANAGER, index++, PosMessage.SHUTDOWN_SYSTEM)));
                menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.SYSTEM_MANAGER, index++, PosMessage.RESTART_SYSTEM)));
                menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.SYSTEM_MANAGER, index++, PosMessage.SOUND_SETTINGS)));
                menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.SYSTEM_MANAGER, index++, PosMessage.UPDATE_FOLDER)));
                menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.SYSTEM_MANAGER, index++, PosMessage.UPDATE_ONLINE)));


                cr.State = States.ListCommandMenu.Instance(menuHeaders,
                                            new ProcessSelectedItem<MenuLabel>(SelectSystemMenuAction),
                                            new StateInstance(Instance));
            }
            else
            {
                cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_RESTART_PROGRAM,
                                                                        new StateInstance(Quit),
                                                                        new StateInstance(Instance)));
            }
        }

        private static void SelectSystemMenuAction(Object menu)
        {
            string message = ((MenuLabel)menu).ToString();
            message = message.Substring(message.IndexOf('\n') + 1);
            switch (message)
            {
                case PosMessage.SHUTDOWN_SYSTEM:
                    Chassis.ShutDown(false);
                    break;
                case PosMessage.RESTART_SYSTEM:
                    Chassis.ShutDown(true);
                    break;
                case PosMessage.RESTART_POS:
                    Quit();
                    break;
                case PosMessage.SHUTDOWN_POS:
                    cr.State = EnterPassword.Instance(PosMessage.AUTHORIZED_PASSWORD, new StateInstance<string>(ShutdownPOS), Instance);
                    break;
                case PosMessage.UPDATE_ONLINE:
                    cr.State = States.EnterPassword.Instance(PosMessage.AUTHORIZED_PASSWORD,
                                                                  new StateInstance<string>(CheckCashierPwdForUpdate),
                                                                  new StateInstance(Instance));
                    break;
                case PosMessage.UPDATE_FOLDER:
                    cr.State = States.EnterPassword.Instance(PosMessage.AUTHORIZED_PASSWORD,
                                                                  new StateInstance<string>(CheckCashierPwdForUpdate),
                                                                  new StateInstance(Instance));
                    break;
                case PosMessage.SOUND_SETTINGS:
                    cr.State = ShowVolumeRate();
                    break;
            }
        }

        private static IState ShowVolumeRate()
        {
            string rater = "";
            for(int i = 0; i < SoundManager.CurrentVolume; i++)
            {
                rater += "-";
            }

            DisplayAdapter.Cashier.Show(String.Format("{0}\n{1}", PosMessage.SOUND_LEVEL, rater.PadRight(20)));

            return state;
        }

        public override void UpArrow()
        {
            if (SoundManager.CurrentVolume < SoundManager.MAX_SOUND_LEVEL)
                SoundManager.VolumeUp();              
            else
                DisplayAdapter.Cashier.Show("MAKSÝMUM SES SEVÝYESÝ");

            ShowVolumeRate();
        }

        public override void DownArrow()
        {
            if (SoundManager.CurrentVolume != 0)
                SoundManager.VolumeDown();
            else
                DisplayAdapter.Cashier.Show("MÝNÝMUM SES SEVÝYESÝ");

            ShowVolumeRate();
        }

        private static IState OnlineUpdate()
        {
            try
            {
                DisplayAdapter.Cashier.Show(PosMessage.UPDATE_STATU);
                GetUpdateZipFile();                
                return Quit();
            }
            catch (Exception ex)
            {
                cr.State = States.AlertCashier.Instance(new Confirm(String.Format("{0}", PosMessage.UPDATE_ERROR)));
                cr.Log.Error(PosMessage.UPDATE_ERROR + ":" + ex.Message);
                return cr.State;
            }
        }

        private static void GetUpdateZipFile()
        {
            string downloadUri = PosConfiguration.OnlineUpdateUri;
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string downloadParams = string.Format("Default.aspx?version_no={0}&fiscal_id={1}", version, cr.FiscalRegisterNo);

            using (WebClient wc = new WebClient())
            {
                string zipFileName = string.Empty;
                try
                {
                    zipFileName = wc.DownloadString(downloadUri + downloadParams);
                }
                catch (WebException ex)
                {
                    var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    HttpStatusCode code = HttpStatusCode.BadRequest;
                    try
                    {
                        code = ((HttpWebResponse)ex.Response).StatusCode;
                        switch (code)
                        {
                            case HttpStatusCode.NotFound: //404
                                DisplayAdapter.Cashier.Show(PosMessage.UPDATE_ERROR_404);
                                cr.Log.Error(PosMessage.UPDATE_ERROR_404);
                                break;
                            case HttpStatusCode.NotAcceptable: //406
                                DisplayAdapter.Cashier.Show(PosMessage.UPDATE_ERROR_406);
                                cr.Log.Error(PosMessage.UPDATE_ERROR_406);
                                break;
                            case HttpStatusCode.InternalServerError: //500
                                DisplayAdapter.Cashier.Show(PosMessage.UPDATE_ERROR_500);
                                cr.Log.Error(PosMessage.UPDATE_ERROR_500);
                                break;
                            default:
                                break;
                        }
                    }
                    catch { }
                }

                string zipFileUri = string.Empty;
                zipFileUri = Path.Combine(downloadUri, zipFileName);
                if (!string.IsNullOrEmpty(zipFileUri))
                {
                    if (Directory.Exists(PosConfiguration.UpgradePath))
                    {
                        Directory.Delete(PosConfiguration.UpgradePath, true);
                    }
                    Directory.CreateDirectory(PosConfiguration.UpgradePath);
                    string zipFilePath = PosConfiguration.UpgradePath + zipFileUri.Substring(zipFileUri.LastIndexOf("\\"));
                    wc.DownloadFile(zipFileUri, zipFilePath);

                    int sleepTime = 0;
                    while (!File.Exists(zipFilePath) && sleepTime < MAX_DWNLD_TIME )
                    {
                        System.Threading.Thread.Sleep(500);
                        sleepTime += 500;
                    }
                    new FastZip().ExtractZip(zipFilePath, PosConfiguration.UpgradePath, "");
                }
            }
        }

        private static IState CheckCashierPwdForUpdate(string pwd)
        {
            ICashier cashier = null;
            try
            {
                cashier = cr.SecurityConnector.LoginCashier(String.Format("{0:D6}", pwd));
            }
            catch (Exception ex)
            {
                cr.State = States.AlertCashier.Instance(new Error(ex));
            }

            if (cashier == null || cashier.AuthorizationLevel < cr.GetAuthorizationLevel(Authorizations.UpdateOperation))
            {
                throw new CashierAutorizeException();
            }
            else
                return OnlineUpdate();
        }

        public static IState CheckPasswordForUpdateOnline(string pwd)
        {
            return pwd == PosConfiguration.Get("ServicePassword") ? OnlineUpdate() : DisplayError(PosMessage.SERVICE_PASSWORD_INVALID);
        }

        public static IState FolderUpdate()
        {
            try
            {
                GetUpdateZipFileFolder();
                return Quit();
            }
            catch (Exception ex)
            {
                cr.State = States.AlertCashier.Instance(new Confirm(String.Format("{0}", PosMessage.UPDATE_ERROR)));
                cr.Log.Error(ex);
                return cr.State;
            }
        }

        private static void GetUpdateZipFileFolder()
        {
            DisplayAdapter.Cashier.Show(PosMessage.UPDATE_STATU);
            cr.Log.Info("DOSYADAN GÜNCELLEME ÝÞLEMÝ BAÞLIYOR..");

            // Get Drives on terminal
            DriveInfo[] drives = DriveInfo.GetDrives();
            string zipFile = string.Empty;
            string xmlFile = string.Empty;

            System.Threading.Thread.Sleep(100);

            List<FileInfo> files = new List<FileInfo>();
            for (int i = 0; i < drives.Length; i++)
            {
                // Check drive is removable?
                if (drives[i].DriveType != DriveType.Removable) continue;

                if (Directory.Exists(Path.Combine(drives[i].RootDirectory.FullName, UPDATE_FOLDER_NAME)))
                {
                    string[] fileNames = Directory.GetFiles(Path.Combine(drives[i].RootDirectory.FullName, UPDATE_FOLDER_NAME));
                    FileInfo fi = null;
                    foreach (string fileName in fileNames)
                    {
                        try
                        {
                            fi = new FileInfo(fileName);
                            files.Add(fi);
                            if (fi.Extension == ".zip")
                            {
                                zipFile = fi.FullName;
                                cr.Log.Info("ZIP GUNCELLEME DOSYASI: " + zipFile);
                            }
                            else if (fi.Extension == ".xml")
                            {
                                xmlFile = fi.FullName;
                                cr.Log.Info("XML GUNCELLEME DOSYASI: " + xmlFile);
                            }
                        }
                        catch { }
                    }
                    break;
                }
                else
                {
                    cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.UPDATE_FOLDER_NOT_FOUND));
                    cr.Log.Info("GÜNCELLEME DOSYALARI BULUNAMADI");
                    return;
                }
            }

            List<string> filesToUpdateList = null;

            // Check exist xml file which include file names to update
            if (!String.IsNullOrEmpty(xmlFile))
            {
                filesToUpdateList = new List<string>();
                cr.Log.Info("GÜNCELLENECEK DOSYALAR: ");

                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFile);

                XmlNodeList elementList = doc.GetElementsByTagName("FileName");
                for(int i=0;i<elementList.Count;i++)
                {
                    filesToUpdateList.Add(elementList[i].InnerXml);
                    cr.Log.Info(elementList[i].InnerXml);
                }
            }
            else
            {
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.UPDATE_FOLDER_NOT_FOUND));
                cr.Log.Info("XML GÜNCELLEME DOSYALARI BULUNAMADI");
                return;
            }
           
            if (!string.IsNullOrEmpty(zipFile))
            {
                if (Directory.Exists(PosConfiguration.UpgradePath))
                {
                    Directory.Delete(PosConfiguration.UpgradePath, true);
                }
                Directory.CreateDirectory(PosConfiguration.UpgradePath);

                // Copy zip file from drive to Upgrade folder
                string zipFilePath = PosConfiguration.UpgradePath + zipFile.Substring(zipFile.LastIndexOf("\\") + 1);
                File.Copy(zipFile, zipFilePath);
                System.Threading.Thread.Sleep(100);

                // Unzip 
                new FastZip().ExtractZip(zipFilePath, PosConfiguration.UpgradePath, "");
                System.Threading.Thread.Sleep(100);

                // Remove unlisted files
                string[] filesInUpgrade = Directory.GetFiles(PosConfiguration.UpgradePath);
                foreach (string file in filesInUpgrade)
                {
                    string fileName = Path.GetFileName(file);
                    if (!filesToUpdateList.Contains(fileName))
                        File.Delete(file);
                }

                // Remove unlisted folders
                string[] foldersInUpgrade = Directory.GetDirectories(PosConfiguration.UpgradePath);
                foreach(string folder in foldersInUpgrade)
                {
                    string folderName = Path.GetFileName(folder);
                    if (!filesToUpdateList.Contains(folderName))
                    {
                        DirectoryInfo di = new DirectoryInfo(folder);
                        foreach (FileInfo fi in di.GetFiles())
                            fi.Delete();
                        foreach (DirectoryInfo dir in di.GetDirectories())
                            dir.Delete(true);
                        try
                        {
                            Directory.Delete(folder);
                        }
                        catch { }
                    }
                }
                cr.Log.Info("GÜNCELLEME ZIP DOSYAASI UNZIP OPERASYONU BAÞARILI");
            }
            else
            {
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.UPDATE_FOLDER_NOT_FOUND));
                cr.Log.Info("ZIP GÜNCELLEME DOSYALARI BULUNAMADI");
                return;               
            }
        }

        public static IState CheckPasswordForUpdateFolder(string pwd)
        {
            return pwd == PosConfiguration.Get("ServicePassword") ? FolderUpdate() : DisplayError(PosMessage.SERVICE_PASSWORD_INVALID);
        }

        private static IState DisplayError(String errorMsg)
        {
            Confirm err = new Confirm(errorMsg,
                     new StateInstance(Instance),
                     new StateInstance(Instance));
            return AlertCashier.Instance(err);
        }

        private static IState ShutdownPOS(string pass)
        {
            if (PosConfiguration.Get("ServicePassword") == pass)
            {
                DisplayAdapter.Cashier.Show(PosMessage.PROGRAM_CLOSING);
                System.Threading.Thread.Sleep(2500);
                Chassis.CloseApplication();
                return cr.State;
            }
            else
            {
                return cr.State = AlertCashier.Instance(new Confirm(PosMessage.INVALID_PASS_ENTRY, Instance, Instance));
            }
        }

        public static IState Quit()
        {
            DisplayAdapter.Cashier.Show(PosMessage.PROGRAM_FILES_UPDATING);
            DisplayAdapter.Cashier.Clear();
            Chassis.RestartProgram(true);
            return state;
        }

        public override void Program()
        {
            cr.State = States.SetupMenu.Instance();
        }

        public static IState EscapeCashier()
        {
            if (cr.CurrentCashier != null && OnLogout != null)
                OnLogout(cr.CurrentCashier);
            cr.CurrentCashier = null;
            return Instance();
        }
        public static void SignInCashier(ICashier cashier)
        {
            try
            {
                manager = cr.Printer.SignInCashier(managerId, password.ToString());
            }
            catch (CashierAlreadyAssignedException caae)
            {
                cr.Log.Error("CashierAlreadyAssignedException occured. {0}", caae.Message);

                if (!cashier.Id.Equals(caae.CashierId))
                {
                    cr.Printer.SignOutCashier();
                    SignInCashier(cashier);
                }
            }
            catch (CmdSequenceException cse)
            {
                if (cse.LastCommand == 37)
                {
                    cr.Printer.AdjustPrinter(new Invoice());
                    cr.Printer.Void();
                    cr.Printer.AdjustPrinter(new Receipt());
                }
            }
        }
        public static void SignOutCashier()
        {
            try
            {
                DisplayAdapter.Cashier.Show(PosMessage.CASHIER_LOGOUT);

                cr.Printer.SignOutCashier();

                if (cr.CurrentCashier != null && OnLogout != null)
                {
                    OnLogout(cr.CurrentCashier);
                }
                cr.CurrentCashier = null;
            }
            catch (EJException eje)
            {
                cr.CurrentCashier = null;
                throw eje;
            }
            catch (NoReceiptRollException nrre)
            {
                cr.CurrentCashier = null;
                throw nrre;
            }
            catch (Exception e)
            {
                if (cr.CurrentCashier != null)
                    cr.Log.Error("Exception occurred while trying to logout {0}", cr.CurrentCashier.Name);
                cr.Log.Warning(e);
            }
        }
        public static IState LoginCashier()
        {
            if (cr.CurrentCashier != null)
            {
                try
                {
                    try
                    {
                        //Check printer is online.
                        cr.Printer.CheckPrinterStatus();

                        int currentdocid = cr.Printer.CurrentDocumentId;

                        cr.PrinterLastZ = cr.Printer.LastZReportNo;

                        if (currentdocid == 0)
                        {
                            //Check printer is online.
                            cr.Printer.CheckPrinterStatus();
                            //Check Power Failure on z completed                        
                            cr.DataConnector.CheckZWritten(cr.PrinterLastZ, currentdocid);

                            cr.DataConnector.ResetSequenceNumber();
                            cr.Log.Info("Logger sequence number reset because FÝÞ NO=0 and power failure on Z Report ");
                        }

                    }
                    catch (ZRequiredException zre)
                    {
                        throw zre;
                    }
                    catch (PrinterException pe)
                    {
                        throw pe;
                    }
                    catch(Exception)
                    {
                    }
#if ORDER
                    cr.RegisterAuthorizationLevel = (AuthorizationLevel)cr.CurrentCashier.AuthorizationLevel;
                                                                                
#else

                    cr.RegisterAuthorizationLevel = (AuthorizationLevel)Math.Min((int)cr.CurrentCashier.AuthorizationLevel,
                                                                                 (int)manager.AuthorizationLevel);
#endif

                    if (OnLogin != null)
                        OnLogin(cr.CurrentCashier);

                    if (ReturnConfirm == null) ReturnConfirm = Start.Instance;

                    //Prompt if no z report in last 24 hrs TODO Make parameter
                    //Note: an exception in time functions could be problematic, but
                    //only likely exception is printer exception which would in almost 
                    //certainty have happened above on SignInCashier
                    if (cr.Printer.Time - cr.Printer.LastZReportDate > new TimeSpan(2, 0, 0, 0)
                        && cr.IsAuthorisedFor(Authorizations.ZReport)
                        && cr.Printer.DailyMemoryIsEmpty)
                    {
                        if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.AutoLogin) == 1)
                            cr.State = States.ReportMenu.PrintZReport();
                        else
                            cr.State = States.ConfirmCashier.Instance(new Confirm("Z RAPORU ALINMAMIS\nRAPOR ALINIZ(GiRiS)",
                                                                      new StateInstance(States.ReportMenu.PrintZReport),
                                                                      ReturnConfirm));

                    }
                    else
                        if (ReturnConfirm != null)
                            cr.State = ReturnConfirm();

                }
                catch (LimitExceededOrZRequiredException ex)
                {
                    return States.ConfirmCashier.Instance(new Error(
                        ex,
                        new StateInstance(States.ReportMenu.PrintZReport),
                        new StateInstance(EscapeZReport)
                        ));
                }
                catch (PrinterTimeoutException toe)
                {
                    cr.CurrentCashier = null;
                    cr.State = AlertCashier.Instance(new Error(toe, new StateInstance(Instance)));
                    cr.Log.Warning(toe);
                }
                catch (CashierAlreadyAssignedException caae)
                {
                    cr.Log.Warning(caae);
                }
                catch (PrinterOfflineException pe)
                {
                    cr.CurrentCashier = null;
                    cr.State = AlertCashier.Instance(new Error(pe, new StateInstance(Instance)));
                    cr.Log.Warning(pe);
                }
                catch (NoReceiptRollException nrre)
                {
                    throw nrre;
                }
                catch (Exception ex)
                {
                    cr.CurrentCashier = null;
                    throw ex;
                }
                finally
                {
                    ReturnConfirm = null;
                    ReturnCancel = null;
                }
            }
            return cr.State;
        }

        private static IState SendInterrupt()
        {
            // Prepare for writing Z report
            cr.DataConnector.PrepareZReport(CashRegister.Id);
            cr.Printer.PrepareDailyReport();

            if (!cr.Document.IsEmpty)
                cr.Document.Void();

            cr.Printer.InterruptReport();

            return Instance();
        }

        private static IState PrintZReport()
        {
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_Z_REPORT);
            BackgroundWorker.IsfterZreport = true;
            IPrinterResponse response = cr.Printer.PrintZReport();
            if (!response.HasError)
                cr.DataConnector.SaveReport("ZRAPORU", response.Detail);

            manager = null;

            return Instance();
        }
        private static IState EscapeZReport()
        {
            StreamReader sr = null;
            String line = "";
            String strTaxes = "";
            string[] keyvalue;
            SortedList<string, string> departments = new SortedList<string, string>();
            try
            {
                //if user pressed Escape when z report requested on new program,
                // reload old program file

                sr = new StreamReader(PosConfiguration.DataPath + PosConfiguration.SettingsFile, PosConfiguration.DefaultEncoding);

                //Load settings into a hashtable
                while ((line = @sr.ReadLine()) != null)
                {
                    //Skip trailing blank lines and comments
                    if (line.Trim().Length == 0 || line.StartsWith("'")) continue;

                    if (line.StartsWith("V"))
                        strTaxes = line.Split('=')[1].TrimEnd(' ', ',');

                    if (!line.StartsWith("D")) continue;

                    keyvalue = line.Split('=');

                    if (keyvalue.Length > 1 & !departments.ContainsKey(keyvalue[0]))
                        departments.Add(keyvalue[0], keyvalue[1].TrimEnd());
                }

                for (int tg = 0; tg < Department.NUM_TAXGROUPS; tg++)
                    Department.TaxRates[tg] = -1;

                int groupId = 0;
                String str = strTaxes;
                Department[] tempDepartments = new Department[Department.NUM_DEPARTMENTS];

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

                    Department.TaxRates[groupId++] = rate / 100m;

                    // Department info creation
                    Department d = new Department(index + 1, departments[String.Format("D{0:D2}", index + 1)]);
                    d.TaxGroupId = groupId;
                    tempDepartments[index] = d;
                    index++;
                }
              
                tempDepartments.CopyTo(Department.Departments, 0);
                cr.DataConnector.LoadProducts();
            }
            catch { }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
            if (ReturnConfirm == null) ReturnConfirm = Start.Instance;
            return ReturnConfirm();

        }

        private static void ResetLoginCount(int z)
        {
            totalLogins = 0;
        }

        public override void LabelKey(int label)
        {
            switch (label)
            {
                case Label.BackSpace:
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                    }
                    DisplayAdapter.Cashier.BackSpace();
                    break;
                case Label.Space:
                    base.LabelKey(label);
                    break;
                default:
                    base.LabelKey(label);
                    break;
            }
        }

        public override void OnEntry()
        {
            try
            {
                DisplayAdapter.Cashier.Show(null as ICredit);
            }
            catch { }
        }
        public override void Correction()
        {
            if (password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                DisplayAdapter.Cashier.BackSpace();
            }
        }
        static void CheckAutoLogin(ICashier cashier)
        {
            if (cr.CurrentCashier == null)
            {
                try
                {
                    CheckCashier((cashier.Password));
                    if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.AutoLogin) == 1)
                    {
                        cr.CurrentCashier = cashier;
                        state = LoginCashier();
                    }
                }
                catch (Exception ex)
                {
                    cr.Log.Error("Kasiyer giriþi yapýlmadý.");
                    cr.Log.Error(ex);
                }
            }
        }
    }

}
