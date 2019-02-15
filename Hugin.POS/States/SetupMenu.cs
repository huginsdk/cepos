using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Reflection;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;
using System.Net;
using System.Collections.Generic;

namespace Hugin.POS.States
{
    class SetupMenu : List
    {
        private static IState state = new SetupMenu();
        private static string terminators = String.Empty;
        private static string readedBarcode = String.Empty;

        private static String[] printerLogo = null;
        private static decimal[] printerTaxRates = null;
        private static String logoLine = "";
        private static String vatIndex = "";

        private static string mangerName = String.Empty;
        private static int configManagerId = 0;

        public static new IState Instance()
        {
            return Instance(Start.Instance);
        }

        public static IState Instance(StateInstance ReturnCancel)
        {
            MenuList menuHeaders = CreateSetupMenu();
            List.Instance(menuHeaders, (ProcessSelectedItem)null, ReturnCancel);
            return state;

        }
        public static MenuList CreateSetupMenu()
        {
            MenuList menuHeaders = new MenuList();
            int index = 1;
            try
            {
                if (!cr.Printer.IsFiscal)
                    menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_FISCALIZATION)));
            }
            catch (Exception e)
            {
                cr.Log.Warning(e);
            }

            //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.SWITCH_MANAGER)));
            menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_VERSION)));

            if(cr.IsAuthorisedFor(Authorizations.Programing))
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_REGISTER)));
            /*
            if (cr.Printer != null)
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_DATETIME)));
            */
            if (cr.IsAuthorisedFor(Authorizations.FileOperationsAndTransfer))
            {
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_DATAFILES)));
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_REGISTERFILES)));
                if (cr.CurrentManager != null)
                {

                    menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_MANAGER)));
                    //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_UPDATE_CATEGORY)));

                    //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_FILE_TRANSFER)));
                    //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_GMP_TEST_COMMAND)));

                }

                if (cr.Printer != null)
                    menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_NEWPROGRAM)));
            }

            if (cr.Printer != null)
                //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_GMP_PORT)));

            if (cr.Printer != null)
            {
                //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_COMPORTSETTINGS)));
            }
            if (!DisplayAdapter.Both.HasAttribute(DisplayAttribute.TouchKeyboard))
                //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_RESETDISPLAY)));

            if (cr.Printer != null && cr.HasEJ && File.Exists("logo.bmp"))
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_LOADBITMAP)));
            
            if (cr.Printer != null && cr.IsAuthorisedFor(Authorizations.Programing))
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_BARCODE_TERMINATOR)));
            #if WindowsCE
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_BUZZER_ON_OFF)));

            #endif
            if(!cr.Printer.IsFiscal && cr.Printer is ITestablePrinter)
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_TEST_EJ)));

            if(DisplayAdapter.Both.HasAttribute(DisplayAttribute.DataManage))
                menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_DATA)));
            if (cr.Printer != null)
                //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_MATCH_EFT_POS)));
            if (cr.Printer != null)
                //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_NETWORK_SETTINGS)));

            if (cr.Printer != null)
            {
                //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_LOGO)));
                //menuHeaders.Add(new MenuLabel(String.Format("PROGRAM\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_VAT_RATE)));
            }

            return menuHeaders;
        }

        public static MenuList CreateConfigMenu()
        {
            MenuList menuHeaders = new MenuList();
            int index = 1;

            menuHeaders.Add(new MenuLabel(String.Format("DATA\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_VAT_RATE)));
            menuHeaders.Add(new MenuLabel(String.Format("DATA\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_DEPARTMENT)));
            menuHeaders.Add(new MenuLabel(String.Format("DATA\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_PRODUCT)));
            menuHeaders.Add(new MenuLabel(String.Format("DATA\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_LOGO)));
            menuHeaders.Add(new MenuLabel(String.Format("DATA\t{0,2}\n{1}", index++, PosMessage.PM_CONFIG_MANAGER)));

            return menuHeaders;
        }

        public static IState Continue()
        {
            ie.MovePrevious();
            List.Instance(ie);
            return state;
        }
        public override void Program()
        {
            base.DownArrow();
        }
        public override void Enter()
        {
            try
            {
                string message = ((MenuLabel)ie.Current).ToString();
                message = message.Substring(message.IndexOf('\n') + 1);
                switch (message)
                {
                    case PosMessage.PM_FISCALIZATION:
                        cr.State = States.EnterInteger.Instance(PosMessage.DATE,
                                                        int.Parse(String.Format("{0:ddMMyyyy}", DateTime.Now)),
                                                        new StateInstance<int>(GetFiscalizationDate),
                                                        new StateInstance(Continue));
                        break;
                    case PosMessage.PM_VERSION:
                        String version = String.Format("{0}\n{1}", PosMessage.PM_VERSION, Assembly.GetExecutingAssembly().GetName().Version);
                        cr.State = ConfirmCashier.Instance(new Confirm(version,
                                                                          new StateInstance(VersionDate),
                                                                          new StateInstance(Continue)));
                        break;
                    case PosMessage.PM_REGISTER:
                        if (cr.IsDesktopWindows)
                        {
                            cr.State = EnterString.Instance(PosMessage.BRANCH_ID,
                                                                    PosConfiguration.Get("OfficeNo"),
                                                                    new StateInstance<String>(OfficeNo),
                                                                    new StateInstance(Continue));
                        }
                        else
                        {
                            cr.State = EnterInteger.Instance(PosMessage.BRANCH_ID,
                                                                     int.Parse(PosConfiguration.Get("OfficeNo")),
                                                                     new StateInstance<int>(OfficeNo),
                                                                     new StateInstance(Continue));
                        }
                        break;
                    case PosMessage.PM_DATETIME:
                        DateTime now = cr.Printer.DateTime;
                        cr.State = EnterInteger.Instance(String.Format("{0} ({1:HH:mm})",PosMessage.RECEIPT_TIME, now),
                                                                int.Parse(String.Format("{0:HHmm}", now)),
                                                                new StateInstance<int>(SetRegisterTime),
                                                                new StateInstance(Continue));
                        break;
                    case PosMessage.PM_DATAFILES:
                        cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.PROMPT_DATA_FILE_UPDATED,
                                                                new StateInstance(DownloadData),
                                                                new StateInstance(Continue)));
                        break;
                    case PosMessage.PM_REGISTERFILES:
                        cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.PROMPT_REGISTER_FILE_TRANSFER,
                                                                new StateInstance(UploadData),
                                                                new StateInstance(Continue)));
                        break;
                    case PosMessage.PM_COMPORTSETTINGS:
                        cr.State = ConfirmPrinterPort();
                        break;
                    case PosMessage.PM_RESETDISPLAY:
                        DisplayAdapter.Customer.Reset();
                        cr.State = AlertCashier.Instance(new Confirm(PosMessage.RESETED_DISPLAY, new StateInstance(Continue)));

                        break;
                    case PosMessage.PM_LOADBITMAP:
                        cr.State = EnterPassword.Instance(PosMessage.SECURITY_CODE,
                                                                   new StateInstance<String>(CheckPasswordForLogo),
                                                                   new StateInstance(Continue));
                        break;
                    case PosMessage.PM_NEWPROGRAM:
                        cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_LOAD_NEW_PROGRAM,
                                                            new StateInstance(LoadNewProgram),
                                                            new StateInstance(Continue)));
                        break;
                    case PosMessage.PM_BARCODE_TERMINATOR:
                        CashRegisterInput.BarcodeReaded += new OnMessageHandler(CashRegisterInput_BarcodeReaded);
                        cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.ENTER_BARCODE,
                                                            new StateInstance(SetupBarcodeTerminator),
                                                            new StateInstance(Continue)));
                        break;
                    case PosMessage.PM_BUZZER_ON_OFF:

                        int buzzerOn = 0;
                        Parser.TryInt(PosConfiguration.Get("Buzzer"), out buzzerOn);

                        if (buzzerOn == PosConfiguration.OFF)
                            cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.PM_BUZZER_ON,
                                                                    new StateInstance(OpenBuzzer),
                                                                    new StateInstance(Continue)));
                        else
                            cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.PM_BUZZER_OFF,
                                                                    new StateInstance(CloseBuzzer),
                                                                    new StateInstance(Continue)));
                        break;
                    case PosMessage.PM_TEST_EJ:
                        cr.State = TestEJ();
                        break;
                    case PosMessage.PM_CONFIG_DATA:
                        if (!cr.Printer.DailyMemoryIsEmpty)
                        {
                            cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.LIMIT_EXCEEDED_OR_ZREQUIRED_EXCEPTION,
                                                                        new StateInstance(States.ReportMenu.PrintZReport)));
                        }
                        else
                        {
                            cr.State = States.EnterPassword.Instance(PosMessage.SECURITY_CODE,
                                                                       new StateInstance<String>(CheckPasswordForConfig),
                                                                       new StateInstance(Continue));
                        }
                        break;
                    case PosMessage.PM_CONFIG_VAT_RATE:
                        //EnterConfigMenu(1);
                        cr.State = ShowTaxRatesMenu();
                        break;
                    case PosMessage.PM_CONFIG_DEPARTMENT:
                        EnterConfigMenu(2);
                        break;
                    case PosMessage.PM_CONFIG_PRODUCT:
                        EnterConfigMenu(3);
                        break;
                    case PosMessage.PM_CONFIG_LOGO:
                        //EnterConfigMenu(4);
                        cr.State = ShowLogoMenu();
                        break;
                    case PosMessage.PM_CONFIG_MANAGER:
                        //cr.State = States.EnterString.Instance(PosMessage.SEARCH_QUERY,
                        //                new StateInstance<String>(FindCashiers),
                        //                new StateInstance(Continue));
                        cr.State = ShowManagerMenu();
                        break;
                    case PosMessage.PM_MATCH_EFT_POS:
                        StartMatchingEftPos();
                                                                           
                        break;
                    case PosMessage.PM_FILE_TRANSFER:
                        cr.State = EnterString.Instance(PosMessage.FILE_NAME,
                                                            new StateInstance<string>(TransferFile),
                                                            new StateInstance(Continue));
                        break;
                    case PosMessage.PM_CONFIG_GMP_PORT:
                        if (cr.CurrentManager.AuthorizationLevel != AuthorizationLevel.P)
                            cr.State = AlertCashier.Instance(new Confirm(PosMessage.NO_ACCESS_RIGHT, new StateInstance(SetupMenu.Instance)));
                        else
                        {
                            cr.State = States.EnterString.Instance(PosMessage.GMP_IP,
                                                                    "###.###.###.###",
                                                                    new StateInstance<string>(SetGMPIp),
                                                                    new StateInstance(Continue));
                        }
                        break;
                    case PosMessage.PM_GMP_TEST_COMMAND:
                        cr.State = EnterGMPTestCommandMenu();
                        break;
                    case PosMessage.PM_UPDATE_CATEGORY:
                        if (cr.CurrentManager.AuthorizationLevel != AuthorizationLevel.P)
                            cr.State = AlertCashier.Instance(new Confirm(PosMessage.NO_ACCESS_RIGHT, new StateInstance(SetupMenu.Instance)));
                        else
                        {
                            if(!cr.Printer.DailyMemoryIsEmpty)
                                throw new LimitExceededOrZRequiredException();

                            cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.PM_UPDATE_CATEGORY + "?", 
                                                                                    new StateInstance(UpdateCategory),
                                                                                    new StateInstance(Continue)));
                        }
                        break;

                    case PosMessage.SWITCH_MANAGER:
                        Login.AutoLogin = false;
                        cr.State = Login.LogoutManager();
                        break;

                    case PosMessage.PM_NETWORK_SETTINGS:
                        cr.State = NetworkSettingsMenu();
                        break;
                }
            }
            catch (Exception e)
            {
                cr.State = AlertCashier.Instance(new Error(e, ReturnCancel, ReturnCancel));
            }
        }
        private static IState DisplayError(String errorMsg)
        {
            Confirm err = new Confirm(errorMsg,
                     new StateInstance(Continue),
                     new StateInstance(Continue));
            return AlertCashier.Instance(err);
        }
        private static IState DisplayError(Exception exc)
        {
            Error err = new Error(exc,
                     new StateInstance(Continue),
                     new StateInstance(Continue));
            return AlertCashier.Instance(err);
        }
        #region Service Mode
        static DateTime fiscalizationDate = DateTime.Now;
        public static IState GetFiscalizationDate(int date)
        {
            try
            {
                string value = date.ToString();
                if (value.Length > 8)
                    throw new FormatException();
                fiscalizationDate = DateTime.Parse(ReportMenu.FormatDate(value), PosConfiguration.CultureInfo.DateTimeFormat);

                return States.EnterInteger.Instance(PosMessage.TIME,
                                                    int.Parse(String.Format("{0:HHmm}",DateTime.Now)),
                                                    new StateInstance<int>(GetFiscalizationTime),
                                                    new StateInstance(Continue));
            }
            catch (FormatException)
            {
                Confirm err = new Confirm(PosMessage.INVALID_DATE_INPUT,
                     new StateInstance(Continue),
                     new StateInstance(Continue));
                return AlertCashier.Instance(err);
            }
        }

        public static IState GetFiscalizationTime(int time)
        {
            if (time == -1) time = 0;
            fiscalizationDate = fiscalizationDate.AddHours(time / 100).AddMinutes(time % 100);
            return ConfirmFiscalMode();
        }

        public static IState ConfirmFiscalMode()
        {
            return cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.ENTER_FISCAL_MODE,
                                         new StateInstance(EnterSecurityPassword),
                                         new StateInstance(Continue)));
        }

        public static IState EnterSecurityPassword()
        {

            String orderNum = cr.Printer.GetOrderNum();
            String tmp = String.Format("(ÝK:{0})", orderNum);

            return EnterPassword.Instance(PosMessage.SERVICE_PASSWORD + tmp,
                                      new StateInstance<String>(CheckPassword),
                                      new StateInstance(Continue));

        }
        public static IState CheckPassword(String password)
        {
            if (password.Length < 1 || password.Length > cr.MAX_CASHIER_PASSWOR_LENGTH)
                return DisplayError(PosMessage.SERVICE_PASSWORD_INVALID);
            return EnterFiscalMode(password);
        }

        public static IState CheckPasswordForLogo(String password)
        {
            return password == PosConfiguration.Get("ServicePassword") ? LoadBitmapFile() : DisplayError(PosMessage.SERVICE_PASSWORD_INVALID);
        }

        public static IState EnterFiscalMode(string password)
        {
            DisplayAdapter.Cashier.Show(PosMessage.ENTERING_FISCAL_MODE);
            try
            {   
                cr.Printer.EnterFiscalMode(password);
                if (cr.Printer.IsFiscal)
                    return ConfirmCashier.Instance(new Confirm(PosMessage.ENTERED_FISCAL_MODE,
                                            new StateInstance(Instance),
                                            new StateInstance(Instance)));
                else
                {
                    return DisplayError(PosMessage.NOT_ENTER_FISCAL_MODE);
                }
            }
            catch (EJException ejx)
            {
                return DisplayError(ejx);
            }
        }
        #endregion Service Mode

        #region Version

        public static IState VersionDate()
        {
            //TODO:datetime.nowi son update zamani ile degistir
            System.IO.FileInfo programExe = new System.IO.FileInfo(IOUtil.ProgramDirectory + IOUtil.AssemblyName);
#if WindowsCE
            StateInstance confirmState = IPAddress;
#else
            StateInstance confirmState = Continue;
#endif

            return ConfirmCashier.Instance(new Confirm(String.Format("{0}\n{1:d} {1:t}", PosMessage.LAST_UPDATE, programExe.CreationTime),
                                new StateInstance(confirmState),
                                new StateInstance(Continue)));
        }

        public static IState IPAddress()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            String ipAddress = "";
            for (int i = 0; i < addr.Length; i++)
            {
                if (addr[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = addr[i].ToString();
                    break;
                }
            }

            if (!String.IsNullOrEmpty(ipAddress))
                return ConfirmCashier.Instance(new Confirm(String.Format("{0}\n{1}", PosMessage.TCP_IP_ADDRESS, ipAddress),
                                new StateInstance(Continue),
                                new StateInstance(Continue)));
            else
                return Continue();
        }
        #endregion Version

        #region Register Definition

        public static IState OfficeNo(int officeNo)
        {
            return OfficeNo(officeNo + "");
        }
        public static IState OfficeNo(String officeNo)
        {
            if (officeNo.Trim().Length != 3)
            {
                return DisplayError("");
            }
            PosConfiguration.Set("OfficeNo", officeNo);
            return EnterInteger.Instance(PosMessage.REGISTER_ID,
                                                int.Parse(cr.Id),
                                                new StateInstance<int>(SetRegisterNo),
                                                new StateInstance(Continue));
        }
        public static IState SetRegisterNo(int registerNo)
        {
            if (registerNo > 999)
            {
                return DisplayError(PosMessage.REGISTER_ID_MAX_LENGTH);
            }

            PosConfiguration.Set("RegisterId", String.Format("{0:D3}", registerNo));
            cr.Id = PosConfiguration.Get("RegisterId");
            String officePath = PosConfiguration.ServerUploadPath;
            EnterString.SetMaxLength(60);
            return EnterString.Instance(PosMessage.OFFICE_INDEX,
                                                 officePath.Replace("\\","/"),
                                                 new StateInstance<String>(SetOfficeDataPath),
                                                 new StateInstance(Continue));
        }

        public static IState SetOfficeDataPath(String officeDataPath)
        {
            PosConfiguration.Set("OfficePath", officeDataPath);
            return Continue();
        }

        #endregion Register Definition

        #region Date Time

        public static IState SetRegisterTime(int timeInt)
        {

            DateTime newTime = DateTime.Today.AddHours(timeInt / 100).AddMinutes(timeInt % 100);
            if (timeInt == -1)
            {
                return DisplayError(PosMessage.ENTER_VALUE);
            }
            //Eger time kasa saatinden 1 saatten fazla farkli ise 
            //veya son z raporundan sonra satis olmussa hata mesaji ver
            //cr.Time = timeInt;               
            try
            {
                cr.Printer.Time = newTime;
                return ConfirmCashier.Instance(new Confirm(String.Format("{0}\n{1:HH:mm}", PosMessage.TIME_CHANGED, newTime),
                                        new StateInstance(Continue),
                                        new StateInstance(Continue)));
            }
            catch (Exception ex)
            {
                return DisplayError(ex);
            }
        }


        #endregion  Date Time

        #region Office Data

        public static IState DownloadData()
        {
            try
            {
                DisplayAdapter.Cashier.Show(PosMessage.DATA_FILES_UPDATING);
                String settings = String.Empty;

                //TODO Linux case
                if (File.Exists(PosConfiguration.DataPath + PosConfiguration.SettingsFile))
                    settings = IOUtil.ReadAllText(PosConfiguration.DataPath + PosConfiguration.SettingsFile, PosConfiguration.DefaultEncoding);

                Connector.FxClient.DownloadDirectory(PosConfiguration.ServerDataPath, PosConfiguration.DataPath, "*.dat");

                //if program file does not exists before copy, delete program file
                if (settings.Length == 0)
                {
                    if (File.Exists(PosConfiguration.DataPath + PosConfiguration.SettingsFile))
                        File.Delete(PosConfiguration.DataPath + PosConfiguration.SettingsFile);
                }
                else
                    IOUtil.WriteAllText(PosConfiguration.DataPath + PosConfiguration.SettingsFile, settings);

                if (cr.Document == null)
                    cr.Instance();
                else
                {
                    cr.DataConnector.LoadAll();
                    String success = "";
                    if (cr.Printer.MaxNumberOfCurrencies >= 0)
                    {
                        success = String.Format("KAS {0:D3}, URUN {1:D6}\nDVZ {2:D3}, {3}...",
                        cr.DataConnector.GetLastSuccess(DataTypes.Cashier),
                        cr.DataConnector.GetLastSuccess(DataTypes.Product),
                        cr.DataConnector.GetLastSuccess(DataTypes.Currency),
                        PosMessage.CONTINUE);
                    }
                    else
                    {
                        success = String.Format("KAS {0:D3}, URUN {1:D6}\n{2}...",
                        cr.DataConnector.GetLastSuccess(DataTypes.Cashier),
                        cr.DataConnector.GetLastSuccess(DataTypes.Product),
                        PosMessage.CONTINUE);
                    }

                    return ConfirmCashier.Instance(new Confirm(success,
                                                        new StateInstance(DownloadDataContinue),
                                                        new StateInstance(Continue)));
                }
            }

            catch (InvalidProgramException ipe)
            {
                //InvalidProgramException can not occur at this state, because only data files are reloaded
                cr.Log.Fatal("SetupMenu.DownloadData: {0}", ipe.InnerException.StackTrace);

                return DisplayError(ipe);
            }
            catch (DirectoryNotFoundException dnfe)
            {
                cr.Log.Fatal(dnfe);
                return AlertCashier.Instance(new Error(dnfe, new StateInstance(Continue), new StateInstance(Continue)));
            }
            catch (Exception e)
            {
                cr.Log.Error(e);
                return DisplayError(PosMessage.NOT_TRANSFERED_DATA_FILES);
            }
            return ConfirmCashier.Instance(new Confirm(PosMessage.DATA_FILES_UPDATED,
                                        new StateInstance(Continue),
                                        new StateInstance(Continue)));
        }
        public static IState DownloadDataContinue()
        {
            String success = String.Format("KRD {0:D3}\nMUST {1:D6}",
                    cr.DataConnector.GetLastSuccess(DataTypes.Credit),
                    cr.DataConnector.GetLastSuccess(DataTypes.Customer));
            return ConfirmCashier.Instance(new Confirm(success,
                                                        new StateInstance(Continue),
                                                        new StateInstance(Continue)));
        }

        #endregion Office Data

        #region Register Files

        public static IState UploadData()
        {

            if (Connector.FxClient.DirectoryExists(PosConfiguration.ServerArchivePath, 5000))
            {
                try
                {
                    UploadFiles();

                    return AlertCashier.Instance(new Confirm(PosMessage.REGISTER_FILES_TRANSFERRED,
                                                new StateInstance(Continue),
                                                new StateInstance(Continue)));
                }
                catch (Exception)
                {
                    return DisplayError("DOSYALAR\nAKTARILAMADI");
                }
            }
            else
            {
                return DisplayError(new DirectoryNotFoundException());
            }

        }

        public static void UploadFiles()
        {
            String suffix = "." + cr.Id;
            System.Collections.Generic.List<String> filesToUpload = new System.Collections.Generic.List<string>();
            
            filesToUpload.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                String.Format("HR{0:ddMMyy}{1}", DateTime.Today, suffix)));
            filesToUpload.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                String.Format("HI{0:ddMMyy}{1}", DateTime.Today, suffix)));
            filesToUpload.AddRange(Dir.GetFiles(PosConfiguration.ArchivePath,
                String.Format("HD{0:ddMMyy}{1}", DateTime.Today, suffix)));

            FileInfo fileInfo;
            foreach (String fileName in filesToUpload)
            {
                fileInfo = new FileInfo(fileName);
                Connector.FxClient.UploadFile(PosConfiguration.ServerArchivePath + fileInfo.Name, fileName);
            }

            cr.DataConnector.UploadRegisterFile(cr.PrinterLastZ);
        }    
        #endregion Register Files

        #region Hardware

        private static IState ConfirmPrinterPort()
        {
            if (cr.Printer.HasProperties(PrinterProperties.ChangablePrinterPort))
            {
                return EnterString.Instance(PosMessage.PRINTER_COM_PORT,
                                                        PosConfiguration.Get("PrinterComPort"),
                                                        new StateInstance<String>(SetPrinterPort),
                                                        new StateInstance(Continue));
            }
            else
            {
                return ConfirmSlipPort();
            }
        }

        private static IState ConfirmSlipPort()
        {
            if (cr.Printer.HasProperties(PrinterProperties.HasExternalSlipPrinter))
            {
                return EnterString.Instance(String.Format(PosMessage.SLIP),
                                                               PosConfiguration.Get("SlipComPort"),
                                                               new StateInstance<String>(SetSlipPort),
                                                               new StateInstance(Continue));
            }
            else
            {
                return ConfirmScalePort();
            }
        }

        private static IState ConfirmScalePort()
        {
            if (PosConfiguration.HasProperties(PosProperties.Scale))
            {
                return EnterString.Instance(String.Format(PosMessage.SCALE),
                                                            PosConfiguration.Get("ScaleComPort"),
                                                            new StateInstance<String>(SetScalePort),
                                                            new StateInstance(Continue));
            }
            else
            {
                return ConfirmBarcodePort();
            }
        }

        private static IState ConfirmBarcodePort()
        {
            if (PosConfiguration.HasProperties(PosProperties.ExternalBarcode))
            {
                return EnterString.Instance(String.Format(PosMessage.BARCODE),
                                                            PosConfiguration.Get("BarcodeComPort"),
                                                            new StateInstance<String>(SetBarcodePort),
                                                            new StateInstance(Continue));
            }
            else
            {
                return ConfirmDisplayPort();
            }
        }

        private static IState ConfirmDisplayPort()
        {
            if (PosConfiguration.HasProperties(PosProperties.ExternalDisplay))
            {
                return EnterString.Instance(String.Format(PosMessage.DISPLAY),
                                                            PosConfiguration.Get("DisplayComPort"),
                                                            new StateInstance<String>(SetDisplayPort),
                                                            new StateInstance(Continue));
            }
            else
            {
                return Continue();
            }
        }

        public static IState SetPrinterPort(String port)
        {
            String existingPort = PosConfiguration.Get("PrinterComPort");
            PosConfiguration.Set("PrinterComPort", port);
            try
            {
                cr.SetPrinterPort(port);
            }
            catch (Exception ex)
            {
                PosConfiguration.Set("PrinterComPort", existingPort);
                cr.SetPrinterPort(port);
                return DisplayError(ex);
            }
            return ConfirmSlipPort();
        }

        public static IState SetSlipPort(String port)
        {
            String existingPort = PosConfiguration.Get("SlipComPort");
            PosConfiguration.Set("SlipComPort", port);
            try
            {
                cr.SetPrinterPort(port);
            }
            catch (Exception ex)
            {
                PosConfiguration.Set("SlipComPort", existingPort);
                cr.SetPrinterPort(port);
                return DisplayError(ex);
            }
            return ConfirmScalePort();
        }

        public static IState SetScalePort(String port)
        {
            PosConfiguration.Set("ScaleComPort", port);
            try
            {
                cr.Scale = ScaleCAS.Instance();
                if (cr.Scale != null)
                    cr.Scale.Connect();
            }
            catch (Exception)
            {
                DisplayError(PosMessage.SCALE_CONNECTION_ERROR);
            }
            return ConfirmBarcodePort();

        }

        public static IState SetBarcodePort(String port)
        {
            PosConfiguration.Set("BarcodeComPort", port);
            try
            {
                CashRegisterInput.ConnectBarcode();
            }
            catch (Exception)
            {
                DisplayError(PosMessage.BARCODE_CONNECTION_ERROR);
            }
            return ConfirmDisplayPort();

        }

        public static IState SetDisplayPort(String port)
        {
            PosConfiguration.Set("DisplayComPort", port);
            DisplayAdapter.Both.Reset();
            return Continue();
        }

        #endregion Hardware

        #region Reset Display

        public static IState DisplayTest()
        {
            return ConfirmCashier.Instance(new Confirm(PosMessage.DISPLAY_TEST,
                                new StateInstance(Continue),
                                new StateInstance(Continue)));
        }
        #endregion Reset Display

        #region New Program

        public static IState LoadNewProgram()
        {
            //Bu messajin FiscalPrinter da degilde burda olmasinin nedeni, kasiyer mesajini bu menuye koydugumuzdan dolayi.
            //Cunku ekrana once kasiyer mesajinin yani "YENI PROGRAM YUKLENIYOR..." mesajinin daha sonra exception mesajinin
            //gelmesi guzel degildir o yuzden.
            try
            {
                cr.DataConnector.GetNewProgram();
            }
            catch (DirectoryNotFoundException dnfe)
            {
                throw dnfe;
            }
            catch (FileNotFoundException)
            {
                throw new InvalidProgramException();
            }
            catch (Exception e)
            {
                cr.Log.Warning(e);
                cr.Log.Warning("Could not copy new program file from backoffice.");
            }
            try
            {
                if (cr.Document == null)
                    cr.Instance();
                else
                {
                    DisplayAdapter.Cashier.Show(PosMessage.LOADING_NEW_PROGRAM);
                    CashRegister.PendingFPUChanges = PendingFPUChange.UnRead;
                    CashRegister.LoadNewSettings();
                    //cr.Printer.LoadProgram();
                }
            }
            catch (LimitExceededOrZRequiredException ex)
            {
                cr.Printer.PrepareDailyReport();
                return States.ConfirmCashier.Instance(new Error(ex,
                                                     new StateInstance(States.ReportMenu.PrintZReport)));
            }
            catch (InvalidProgramException ide)
            {
                cr.Log.Fatal("SetupMenu.LoadNewProgram: {0}", ide.InnerException.StackTrace);
                throw ide;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                cr.Log.Fatal(dnfe);
                throw dnfe;
            }
            catch (PrinterOfflineException pe)
            {
                cr.Log.Warning(pe);
                IPrinterResponse response = null;
                try
                {
                    response = cr.Printer.CheckPrinterStatus();
                }
                catch (PowerFailureException pfe)
                {
                    cr.Log.Warning(pfe);
                    cr.Void();
                    throw new InvalidProgramException(PosMessage.NOT_PROGRAM_INSTALLED, pfe);
                }
            }
            try
            {
                // to do : cr.InvoicePage = new InvoicePage();
            }
            catch (Exception ex)
            {
                throw new InvalidProgramException(PosMessage.COORDINATE_ERROR, ex);
            }
            return cr.State = Start.Instance();
        }
        #endregion  New Program

        #region LoadBitmap

        public static IState LoadBitmapFile()
        {
            try
            {
                DisplayAdapter.Cashier.Show("GRAFÝK LOGO\nYÜKLENÝYOR...");
                cr.Printer.LoadGraphicLogo("logo.bmp");
                return Continue();
            }
            catch
            {
                return DisplayError("LOGO YUKLENEMEDI");
            }
        }
        #endregion

        #region Barcode Terminator
        private IState SetupBarcodeTerminator()
        {
            try
            {
                if (readedBarcode!="")
                    return BarcodeTerminator(readedBarcode);

                return Continue();
            }
            finally
            {
                CashRegisterInput.BarcodeReaded -= CashRegisterInput_BarcodeReaded;
            }
        }

        void CashRegisterInput_BarcodeReaded(object sender, OnMessageEventArgs e)
        {
            readedBarcode = e.Message;
        }

        public static IState BarcodeTerminator(String barcode)
        {
            terminators = "";
            foreach (Char c in barcode)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(c + "", "[a-zA-Z0-9]"))
                {
                    terminators += c.ToString();
                    terminators = terminators.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

                }
            }

            return cr.State = ConfirmCashier.Instance(new Confirm(String.Format("{0}{1,5}", PosMessage.CONFIRM_BARCODE_TERMINATOR, terminators),
                                            new StateInstance(ChangeBarcodeTerminator),
                                            new StateInstance(Continue)));
        }
        public static IState ChangeBarcodeTerminator()
        {
            PosConfiguration.Set("BarcodeTerminator", terminators);
            return Continue();
        }

        #endregion

        #region Buzzer
        public static IState OpenBuzzer()
        {
            PosConfiguration.Set("Buzzer", PosConfiguration.ON);
            return Continue();
        }

        public static IState CloseBuzzer()
        {
            PosConfiguration.Set("Buzzer", PosConfiguration.OFF);
            return Continue();
        }

        #endregion

        #region Config
        private static IState CheckPasswordForConfig(String password)
        {
            return password == PosConfiguration.Get("ServicePassword") ? ShowConfigMenu() : DisplayError(PosMessage.SERVICE_PASSWORD_INVALID);
        }

        private static IState ShowConfigMenu()
        {
            MenuList menuHeaders = CreateConfigMenu();
            List.Instance(menuHeaders, (ProcessSelectedItem)null, ReturnCancel);
            return state;

        }

        static string ipVal = "";
        private static IState SetGMPIp(string ip)
        {
            string[] spltIPAddrs = ip.Split(new char[] { '.' });

            for (int i = 0; i < spltIPAddrs.Length; i++)
            {
                ipVal += int.Parse(spltIPAddrs[i]).ToString().PadLeft(3, '0');
            }

            cr.State = States.EnterInteger.Instance(PosMessage.GMP_PORT,
                                                        new StateInstance<int>(SetGMPPort),
                                                        Continue);
            return cr.State;
        }

        private static IState SetGMPPort(int port)
        {

            cr.Printer.SaveGMPConnectionInfo(ipVal, port);

            return SetupMenu.Instance();
        }

        private static IState ShowManagerMenu()
        {
            if (cr.CurrentManager.AuthorizationLevel != AuthorizationLevel.P)
                return AlertCashier.Instance(new Confirm(PosMessage.NO_ACCESS_RIGHT, new StateInstance(SetupMenu.Instance)));

            MenuList cashierMenu = new MenuList();

            List<String> cashierList = cr.Printer.GetCashiers();
            int index = 1;

            cashierMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.MANAGER, index++, cashierList[0])));
            cashierMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.MANAGER, index++, cashierList[1])));
            cashierMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.MANAGER, index++, cashierList[2])));
            cashierMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.MANAGER, index++, cashierList[3])));
            cashierMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.MANAGER, index++, cashierList[4])));
            cashierMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.MANAGER, index++, cashierList[5])));

            return ListCommandMenu.Instance(cashierMenu, new ProcessSelectedItem<MenuLabel>(ManagerMenuAction), SetupMenu.Instance);
        }

        private static void ManagerMenuAction(Object menu)
        {
            String msg = ((MenuLabel)menu).ToString();

            List<String> cashierList = cr.Printer.GetCashiers();
            configManagerId = int.Parse((msg.Substring(msg.IndexOf("\t") + 1, 2)).Trim());

            cr.State = EnterString.Instance(PosMessage.MANAGER + "\t " + configManagerId,
                                              cashierList[configManagerId - 1],
                                              new StateInstance<string>(GetManagerPass),
                                              new StateInstance(Continue));

        }

        private static IState GetManagerPass(string name)
        {
            mangerName = name;
            cr.State = EnterInteger.Instance(PosMessage.ENTER_PASSWORD,
                                                new StateInstance<int>(SetManager),
                                                new StateInstance(Continue));
            return cr.State;
        }

        private static IState SetManager(int password)
        {
            if (password < -1 || password > 1000000)
            {
                DisplayAdapter.Cashier.Show(PosMessage.INVALID_PASSWORD_TRY_AGAIN);
                System.Threading.Thread.Sleep(1000);
                return GetManagerPass(mangerName);
            }

            ICashier manager = cr.DataConnector.CreateCashier(mangerName, configManagerId.ToString());
            manager.Password = password.ToString();
            cr.Printer.SaveCashier(manager);

            DisplayAdapter.Cashier.Show(PosMessage.SUCCESS_PROCESS);

            return ShowManagerMenu();
        }

        public static IState FindCashiers(String info)
        {
            List<String> cashierNameList = cr.Printer.GetCashiers();

            List<ICashier> cashierList = new List<ICashier>();
            if (cashierNameList == null || cashierNameList.Count == 0)
            {
                Confirm err = new Confirm(PosMessage.CASHIER_NOT_FOUND,
                 new StateInstance(Continue),
                 new StateInstance(Continue));
                return States.AlertCashier.Instance(err);

            }

            if (cr.CurrentCashier.AuthorizationLevel == AuthorizationLevel.P)
            {
                cashierList = cr.DataConnector.SearchCashiersByInfo(info);


                MenuList matchingCashiers = new MenuList();
                foreach (ICashier c in cashierList)
                {
                    matchingCashiers.Add(new MenuLabel(String.Format("KASÝYER KODU: \t{0,4}\n{1}", c.Id, c.Name)));
                }


                return List.Instance(matchingCashiers, new ProcessSelectedItem(SetCurrentCashier), Instance);
            }
            else
            {
                cashier = cr.CurrentCashier;

                cr.State = EnterPassword.Instance(PosMessage.ENTER_PASSWORD,
                                                new StateInstance<string>(ChangeCashierPassword),
                                                new StateInstance(Continue)
                                                  );

                return cr.State;
            }
        }

        static ICashier cashier = null;
        private static void SetCurrentCashier(Object o)
        {
            String[] splittedData = ((MenuLabel)o).ToString().Split(new char[] { '\t', '\n' });
            String id = String.Format("{0:D4}", splittedData[1].Trim());
            cashier = cr.DataConnector.FindCashierById(id);

            cr.State = EnterPassword.Instance(PosMessage.ENTER_PASSWORD,
                                            new StateInstance<string>(ChangeCashierPassword),
                                            new StateInstance(Continue)
                                              );
            
        }

        private static IState ChangeCashierPassword(string password)
        {
            cashier.Password=password;
            cr.Printer.SaveCashier(cashier);
            return Start.Instance();
        }

        private static void EnterConfigMenu(int configIndex)
        {
            MenuList menuDummy = new MenuList();

            switch (configIndex)
            {
                case 1:
                    menuDummy.Add(PosMessage.ENTER_CONFIG_1);
                    break;
                case 2:
                    menuDummy.Add(PosMessage.ENTER_CONFIG_2);
                    break;
                case 3:
                    menuDummy.Add(PosMessage.ENTER_CONFIG_3);
                    break;
                case 4:
                    menuDummy.Add(PosMessage.ENTER_CONFIG_4);
                    break;
                case 5:
                    menuDummy.Add(PosMessage.ENTER_CONFIG_5);
                    break;
            }
                
            menuDummy.MoveFirst();
            DisplayAdapter.Cashier.Show(menuDummy);
            DisplayAdapter.Cashier.Show(null as MenuList);

        }



        #endregion

        #region EJ Test
        private IState TestEJ()
        {
            cr.Log.Info("EKÜ TESTÝ BAÞLATILDI.");
            try
            {
                IPrinterResponse response = ((ITestablePrinter)cr.Printer).CheckEJ();
                String msg = "EKÜ BAÞARIYLA\nTEST EDÝLDÝ";
                cr.Log.Info("EKÜ TESTÝ BAÞARILI.");
                return AlertCashier.Instance(new Confirm(msg, Continue));
            }
            catch (Exception ex)
            {
                cr.Log.Info("EKÜ HATASI: {0}", ex.Message);
                throw ex;
            }
        }
        #endregion

        #region Matching EFT-POS
        private static void StartMatchingEftPos()
        {
            DisplayAdapter.Cashier.Show(PosMessage.WAITING_FOR_DEVICE_MATCHNG);

            cr.Printer.EnterServiceMode("");

            ReturnCancel = new  StateInstance(StopMatchingEftPos);
                    
        }

        private static IState StopMatchingEftPos()
        {
            cr.Printer.ExitServiceMode("");
            DisplayAdapter.Cashier.Show(PosMessage.OP_ENDED);
            System.Threading.Thread.Sleep(1500);
            ReturnCancel = Start.Instance;

            return SetupMenu.Instance();
        }

        
        #endregion 

        #region LOGO

        public static IState ShowLogoMenu()
        {

            MenuList logoMenu = new MenuList();
            if (printerLogo == null)
                printerLogo = cr.Printer.Logo;
            int index = 1;

            logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.LOGO_LINE, index++, printerLogo[0])));
            logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.LOGO_LINE, index++, printerLogo[1])));
            logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.LOGO_LINE, index++, printerLogo[2])));
            logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.LOGO_LINE, index++, printerLogo[3])));
            logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.LOGO_LINE, index++, printerLogo[4])));
            logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                PosMessage.LOGO_LINE, index++, printerLogo[5])));

            //return List.Instance(logoMenu, null , new StateInstance(ShowLogoMenu));
            return ListCommandMenu.Instance(logoMenu, new ProcessSelectedItem<MenuLabel>(LogoMenuAction), Instance);
            //return state;

        }

        private static void LogoMenuAction(Object menu)
        {
            String msg = ((MenuLabel)menu).ToString();

            logoLine = (msg.Substring(msg.IndexOf("\t") + 1, 2)).Trim();

            EnterString.SetMaxLength(48);
            cr.State = EnterString.Instance(PosMessage.LOGO_LINE + "\t " + logoLine,
                                              printerLogo[int.Parse(logoLine) - 1],
                                              new StateInstance<String>(SetLogo),
                                              new StateInstance(Continue));

        }

        public static IState SetLogo(String logo)
        {
            if (printerLogo == null)
                printerLogo = cr.Printer.Logo;
            string[] tempArray = printerLogo;

            Confirm err = new Confirm(PosMessage.INVALID_ENTRY,
                                           new StateInstance(Continue),
                                           new StateInstance(Continue));

            if (logo.Length > 48)
            {
                return AlertCashier.Instance(err);
            }
            else
            {
                try
                {
                    tempArray[int.Parse(logoLine) - 1] = logo;
                    cr.Printer.Logo = tempArray;
                }
                catch (Exception ex)
                {
                    cr.Log.Error("Exception occured.", ex);
                    return AlertCashier.Instance(new Error(ex,
                                               new StateInstance(ShowLogoMenu),
                                               new StateInstance(ShowLogoMenu)));
                }
            }
            return ShowLogoMenu();
        }

        #endregion

        #region TAX_RATES

        public static IState ShowTaxRatesMenu()
        {
            printerTaxRates = cr.Printer.TaxRates;

            MenuList menuHeaders = new MenuList();
            int index = 1;

            for (index = 1; index <= printerTaxRates.Length; index++)
            {
                if (printerTaxRates[index - 1] == decimal.MinusOne)
                {
                    menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                    PosMessage.VAT_RATE, index, "TANIMSIZ")));
                }
                else
                {
                    menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                    PosMessage.VAT_RATE, index, printerTaxRates[index - 1])));
                }
            }       

            //List.Instance(menuHeaders);
            return ListCommandMenu.Instance(menuHeaders, new ProcessSelectedItem<MenuLabel>(TaxRatesMenuAction), Continue);

            //return state;
        }

        private static void TaxRatesMenuAction(Object menu)
        {
            String msg = ((MenuLabel)menu).ToString();

            vatIndex = (msg.Substring(msg.IndexOf("\t") + 1, 2)).Trim();

            cr.State = EnterDecimal.Instance(PosMessage.VAT_RATE + "\t " + vatIndex,
                                                      printerTaxRates[int.Parse(vatIndex) - 1],
                                                      new StateInstance<decimal>(SetTaxRates),
                                                      new StateInstance(Continue));
        }

        public static IState SetTaxRates(decimal taxRate)
        {
            decimal[] tempArray = cr.Printer.TaxRates;

            Confirm err = new Confirm(PosMessage.INVALID_ENTRY,
                                           new StateInstance(Continue),
                                           new StateInstance(Continue));

            if (taxRate > 100)
            {
                return AlertCashier.Instance(err);
            }
            else
            {
                try
                {
                    tempArray[int.Parse(vatIndex) - 1] = taxRate;
                    cr.Printer.TaxRates = tempArray;
                }
                catch (Exception ex)
                {
                    cr.Log.Error("Exception occured.", ex);
                    return AlertCashier.Instance(new Error(ex,
                                               new StateInstance(ShowTaxRatesMenu),
                                               new StateInstance(ShowTaxRatesMenu)));
                }
            }
            cr.State = ShowTaxRatesMenu();
            return cr.State;
        }

        #endregion
        
        private static IState TransferFile(string fileName)
        {
            try
            {
                cr.Printer.TransferFile(fileName);
            }
            catch (Exception ex)
            {
                cr.Log.Error("Exception occured. {0}", ex);
                throw ex;
            }

            return Continue();
        }

        private static IState EnterGMPTestCommandMenu()
        {
            List<String> gmpCmndsList = new List<string>();
            gmpCmndsList.AddRange(new string[] {
            "DEBUG MODU",
            "FIRST INIT",
            "PARAMETRE YUKLEME",
            "LOG GONDER",
            "FÝÞ GÖNDER",
            "ÝPTAL FÝÞÝ GÖNDER",
            "Z GÖNDER",
            "FIRST INIT KONTROL ETME",
            "-",
            "PRINTER DEBUG ACIK",
            "PRINTER DEBUG KAPALI",
            "-",
            "-",
            "LOGS TO TSM"});

            MenuList menuHeaders = new MenuList();
            int index = 1;

            for (int i = 0; i < gmpCmndsList.Count; i++)
            {
                menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1}\n{2}",
                    PosMessage.PM_GMP_TEST_COMMAND, index++, gmpCmndsList[index - 2])));
            }

            return ListCommandMenu.Instance(menuHeaders, new ProcessSelectedItem<MenuLabel>(GMPTestMenuAction), new StateInstance(Instance));
        }

        private static void GMPTestMenuAction(Object menu)
        {
            String msg = ((MenuLabel)menu).ToString();

            int index =int.Parse((msg.Substring(msg.IndexOf("\t") + 1, 2)).Trim()) - 1;

            cr.Printer.TestGMP(index);
        }

        private static IState UpdateCategory()
        {
            DisplayAdapter.Cashier.Show(PosMessage.UPDATÝNG_CATEGORY);

            Category[] tmpArray = cr.Printer.Category;
            cr.Printer.Category = cr.DataConnector.LoadCategories().ToArray();

            return Continue();
        }

        private static IState NetworkSettingsMenu()
        {
            MenuList menuHeaders = new MenuList();
            int index = 1;

            menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1}\n{2}",
                    PosMessage.PM_NETWORK_SETTINGS, index++, "IP")));
            menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1}\n{2}",
                    PosMessage.PM_NETWORK_SETTINGS, index++, "SUBNET")));
            menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1}\n{2}",
                    PosMessage.PM_NETWORK_SETTINGS, index++, "GATEWAY")));
            menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1}\n{2}",
                    PosMessage.PM_NETWORK_SETTINGS, index++, PosMessage.SAVE_SETTINGS)));

            return ListCommandMenu.Instance(menuHeaders, new ProcessSelectedItem<MenuLabel>(NetworkSettingsMenuAction), new StateInstance(Instance));
        }
        static string strIp = String.Empty;
        static string strSubnet = String.Empty;
        static string strGateway = String.Empty;
        private static void NetworkSettingsMenuAction(Object menu)
        {
            String msg = ((MenuLabel)menu).ToString();

            int index = int.Parse((msg.Substring(msg.IndexOf("\t") + 1, 1)).Trim());

            switch (index)
            {
                case 1:
                    cr.State = States.EnterString.Instance("IP",
                                                            strIp,
                                                            new StateInstance<String>(SetIp),
                                                            new StateInstance(Continue));
                    break;
                case 2:
                    cr.State = States.EnterString.Instance("SUBNET",
                                                            "255.255.255.0",
                                                            new StateInstance<String>(SetSubnet),
                                                            new StateInstance(Continue));
                    break;
                case 3:
                    cr.State = States.EnterString.Instance("GATEWAY",
                                                            strGateway,
                                                            new StateInstance<String>(SetGateway),
                                                            new StateInstance(Continue));
                    break;
                case 4:
                    cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.SAVE_SETTINGS + "?\n(" + PosMessage.ENTER + "?)",
                                                                            new StateInstance(SaveNetworkSettings),
                                                                            new StateInstance(NetworkSettingsMenu)));
                    break;
            }
        }

        private static IState SetIp (String ip)
        {
            strIp = ip;
            return NetworkSettingsMenu();
        }

        private static IState SetSubnet(String subnet)
        {
            strSubnet = subnet;
            return NetworkSettingsMenu();
        }

        private static IState SetGateway(String gateway)
        {
            strGateway = gateway;
            return NetworkSettingsMenu();
        }

        private static IState SaveNetworkSettings()
        {
            try
            {
                cr.Printer.SaveNetworkSettings(strIp, strSubnet, strGateway);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            DisplayAdapter.Cashier.Show(PosMessage.SAVED_SUCCESFULLY);
            System.Threading.Thread.Sleep(500);
            return NetworkSettingsMenu();
        }
    }
}
