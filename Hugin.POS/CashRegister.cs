using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Hugin.POS.Printer;
using Hugin.POS.Common;
using Hugin.POS.Data;
using System.Text;

namespace Hugin.POS
{

    [Flags]
    public enum PendingFPUChange
    {
        UnRead = 0,
        NoChange = 1,
        AutoCutter = 2,
        Logo = 4,
        TaxRate = 8,
        GraphicLogo = 16,
        Credit = 32,
        Department = 64,
        ReceiptBarcode = 128,
        EndOfReceiptNote = 256,
        ReceiptLimit = 512,
        Currency = 1024
    }

    class CashRegister
    {
        private static IState state;
        private static FiscalItem currentItem;
        private static SalesDocument document;
        private static String fiscalRegisterNo;
        private static String id;
        private static AuthorizationLevel registerAuthorizationLevel;
        private static ICashier currentCashier;
        private static ICashier currentManager;
        private static IScale scale;
        private static IPosClient promoClient;
        private static IEftPos eftPos;
        private static DateTime lastZReportDate = DateTime.Now;
        private static IOrderPrinter orderPrinter = null;
        private static PendingFPUChange pendingFpuChanges;
        private static string lastMsg = "";
        private static DateTime lastMsgTime = DateTime.Now;
        private static int printerLastZ = -1;
        private static List<int> mealVATRates = null;

        public static event EventHandler DocumentChanged;

        public static bool IsSaleSelected = false;

        private static bool tcknChecked = false;

        public const string PROMOTION_CASHIER_ID = "9999";
        public const string POINT_CASHIER_ID = "9998";
        public const string PROMOTION_ITEM_CASHIER_ID = "9997";

        public const int MAX_CASHIER_PASSWOR_LENGTH = 8;
        public const int MAX_CASHIER_ID = 99;
        public const int MAX_MANAGER_ID = 10;

        public const int MAX_PASSWORD_TRY = 5;

        public static String Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Gets Current Cashier assigned to FPU. 
        /// </summary>
        public static ICashier CurrentCashier
        {
            get
            {
                return currentCashier;
            }
            set
            {
                currentCashier = value;
            }
        }

        /// <summary>
        /// Gets Current Manager assigned to FPU. 
        /// </summary>
        public static ICashier CurrentManager
        {
            get
            {
                return currentManager;
            }
            set
            {
                currentManager = value;
            }
        }

        /// <summary>
        /// Gets current eft pos methods
        /// </summary>
        internal static IEftPos EftPos
        {
            get { return eftPos; }
        }

        /// <summary>
        /// - Checks cashier's authorization whether cashier authorized for given operation
        /// </summary>
        /// <param name="operationName">
        /// - Operation name that will be checked 
        /// </param>
        /// <returns></returns>
        public static bool IsAuthorisedFor(Authorizations operation)
        {
            bool response = RegisterAuthorizationLevel >= GetAuthorizationLevel(operation);
            if (!response)
                Log.Info("Operation {0} not authorised for Cashier Id: {1}", operation, currentCashier.Id);
            return response;
        }

        public static IState PriceLookup(String searchQry)
        {
            if (searchQry.Length < 2)
                return States.AlertCashier.Instance(new Confirm(PosMessage.NOT_ENOUGH_CHARS_FOR_PRICECHECK));
            List<IProduct> matchingFiscalItems = new List<IProduct>();
            MenuList listItems = new MenuList();

            matchingFiscalItems.AddRange(DataConnector.SearchProductByBarcode(new string[] { searchQry }));
            matchingFiscalItems.AddRange(DataConnector.SearchProductByLabel(new string[] { searchQry }));
            matchingFiscalItems.AddRange(DataConnector.SearchProductByName(searchQry));

            Decimal quantity = 0.0m;
            if (matchingFiscalItems.Count == 0)
            {
                try
                {
                    matchingFiscalItems.Add(FindSpecialProduct(new Number(searchQry)));

                    BarcodeAdjustment adjustment = new BarcodeAdjustment(new Number(searchQry));
                    switch (adjustment.Type)
                    {
                        case BarcodeType.ByGramma:
                        case BarcodeType.ByQuantity:
                            quantity = adjustment.Quantity.ToDecimal();
                            break;
                        case BarcodeType.ByTotalAmount:
                            State = States.EnterTotalAmount.Instance(adjustment.Amount);
                            State.Enter();
                            break;
                        case BarcodeType.ByPrice:
                            State = States.EnterUnitPrice.Instance(adjustment.Price);
                            State.Enter();
                            break;
                    }
                }
                catch { }
            }
            if(quantity == 0.0m)
                quantity = Item.Quantity;

            if (Scale != null)
            {
                //Check list to has list weighable product
                bool weighableExist = matchingFiscalItems.Exists(delegate(IProduct weighableProduct)
                {
                    return weighableProduct.Status == ProductStatus.Weighable;
                });
                if (weighableExist)
                {
                    //If list has an one product,show its unit price and total amount on scale's screen
                    decimal tempUnitPrice = matchingFiscalItems.Count == 1 ? matchingFiscalItems[0].UnitPrice : 1.00m;
                    quantity = Scale.GetWeight(tempUnitPrice);
                    if (quantity <= 0)
                        quantity = 1;
                }
            }

            foreach (IProduct p in matchingFiscalItems)
            {
                FiscalItem fi = (SalesItem)Item.Clone();
                try
                {
                    fi.Product = p;
                    if (p.Status == ProductStatus.Weighable)
                    {
                        fi.Quantity = quantity;
                    }
                    listItems.Add(fi);
                }
                catch { }
            }

            if(listItems.Count == 0) SoundManager.Sound(SoundType.NOT_FOUND);

            return (listItems.Count == 0)
                    ? States.AlertCashier.Instance(new Error(new ProductNotFoundException()))
                    : States.ListProductPriceLookup.Instance(listItems, new ProcessSelectedItem<FiscalItem>(Execute));
        }

        static IProduct FindSpecialProduct(Number input)
        {
            String barcode = GetSpecial(input.ToString().Substring(0, 2));
            int labelLength = Int32.Parse(barcode.Substring(4, 1));
            return DataConnector.FindProductByBarcode(input.ToString().Substring(0, 2 + labelLength));
        }

        static String GetSpecial(String key)
        {
            return DataConnector.CurrentSettings.GetSpecialBarcode(key);
        }

        public static void Instance()
        {
            EZLogger.Log = new Common.EZLogger(PosConfiguration.LogPath + "HRERROR." + PosConfiguration.Get("RegisterId"), true, 0xFFFF);
            Debugger.Instance().AppendLine("Debug started");
            States.AlertCashier.SetTimeouts();
            try
            {
                Id = PosConfiguration.Get("RegisterId");
                FiscalRegisterNo = PosConfiguration.Get("FiscalId");
            }
            catch (FiscalIdException)
            {
                state = States.FiscalIdBlock.Instance(PosConfiguration.Get("FiscalId"));
                return;
            }
            bool bStatus = Log.Start();

            Debugger.Instance().AppendLine("Program started, Version");
            Log.Success("Program started, Version : " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            Debugger.Instance().AppendLine("Started Settings: " + DateTime.Now.ToLongTimeString());
            if (state == null || state is States.WaitingState)
            {
                try
                {
                    DisplayAdapter.Cashier.Show(PosMessage.STARTUP_MESSAGE);

                    if (PosConfiguration.IsPrinterGUIActive)
                    {
                        DataConnector.MaxCreditCount = 8;
                        DataConnector.MaxCurrencyCount = 4;
                    }
                    else
                    {
                        //Sets currencies and credits as printer's capacity.
                        DataConnector.MaxCreditCount = Printer.MaxNumberOfCredits;
                        DataConnector.MaxCurrencyCount = Printer.MaxNumberOfCurrencies;
                    }

                    //load settings at minimal
                    DataConnector.LoadSettings();

                    Log.Levels = (uint)DataConnector.CurrentSettings.GetProgramOption(Setting.LogLevel);

                    int outid = -1;
                    if (!Parser.TryInt(Id, out outid) || outid < 1 || Id.Length != 3)
                        Chassis.FatalErrorOccured("KASA NO 001-999\nARASINDA OLMALIDIR");

                    string officeNo = PosConfiguration.Get("OfficeNo");
                    if (officeNo.Length != 3)
                        Chassis.FatalErrorOccured("ÞUBE KODU 3\nKARAKTERDEN OLUÞUR");

                }
                catch (InvalidProgramException ipe)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    state = States.CashRegisterLoadError.Instance(new InvalidSettingsFileException());
                    Log.Error(ipe);
                    return;
                }
                catch (FileNotFoundException fnfe)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    state = States.CashRegisterLoadError.Instance(new InvalidSettingsFileException());
                    Log.Error(fnfe);
                    return;
                }
            }

            Log.Success("Settings loaded for {0}. Log level {1}", FiscalRegisterNo, Log.Levels);
            state = null;

            DataConnector.LoadCurrencies();

            DataConnector.LoadCashiers();
            if (DataConnector.GetLastSuccess(DataTypes.Cashier) == 0)
                state = States.CashRegisterLoadError.Instance(new InvalidCashierFileException());

            DataConnector.LoadProducts();
            if (DataConnector.GetLastSuccess(DataTypes.Product) == 0)
                state = States.CashRegisterLoadError.Instance(new InvalidProductFileException());

            DataConnector.LoadSerialNumbers();

            DataConnector.LoadCustomers();
            
            try
            {
                promoClient = CashRegister.GetPromoClient();
            }
            catch
            {

                Log.Error("Promosyon hizmeti saglayan sunucu bulunamadi!!!");
            }

            if (PosConfiguration.HasProperties(PosProperties.Scale))
            {
                if (PosConfiguration.Get("ScaleComPort") != String.Empty)
                {
                    try
                    {
                        scale = ScaleCAS.Instance();
                        scale.Connect();
                    }
                    catch (Exception ex)
                    {
                        scale = null;
                        Log.Error("Scale connection error: {0}", ex.Message);
                    }
                }
            }

            if (PosConfiguration.HasProperties(PosProperties.EftPOS))
            {
                if (PosConfiguration.Get("EftPosComPort") != String.Empty)
                {
                    try
                    {
                        eftPos = ModuleManeger.LoadModule("Hugin.POS.EftPos", "EftPos") as IEftPos;

                        eftPos.Connect(PosConfiguration.Get("EftPosComPort"));
                        eftPos.OnMessage += new OnMessageHandler(Printer_OnMessage);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Eft Pos baðlantýsý gerçekleþtirilemedi. Hata:{0}", ex.Message);
                    }
                }
            }

            DataConnector.LogSaved += new LogSavedEventHandler(DataConnector_LogSaved);

            DisplayAdapter.Customer.Show(PosMessage.WELCOME_LOCKED);
            DisplayAdapter.SaleSelected += new SalesSelectedHandler(Display_SaleSelected);
            DisplayAdapter.SalesFocusLost += new EventHandler(Display_SalesFocusLost);

            SalesDocument.OnClose += new OnCloseEventHandler(SalesDocument_OnClose);
            SalesDocument.OnSuspend += new OnCloseEventHandler(SalesDocument_OnSuspend);
            SalesDocument.OnVoid += new OnCloseEventHandler(SalesDocument_OnVoid);
            SalesDocument.ItemSold += new SaleEventHandler(SalesDocument_ItemSold);
            SalesDocument.ItemUpdated += new SaleEventHandler(SalesDocument_ItemUpdated);
            SalesDocument.PaymentMade += new PaymentEventHandler(SalesDocument_PaymentMade);
            States.Login.OnLogin += new Hugin.POS.States.LoginEventHandler(Login_OnLogin);
            States.Login.OnLogout += new Hugin.POS.States.LoginEventHandler(Login_OnLogout);
            DataConnector.StartLog(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            state = States.WaitingState.Instance();
            BackgroundWorker.OnNetworkUp += new NetworkStatusChangeHandler(DataConnector.Connect);

            DocumentFileHelper helper = null;
            try
            {
                helper = DocumentFileHelper.GetUnsavedDocument();
                document = helper.LoadDocument();
            }
            catch { }

            if (document == null)
                document = new Receipt();
            else
            {
               
#if ORDER
                if (helper.Status == DocumentStatus.Paying)
                    document.ReturnReason = "WAITING_FOR_PAYMENT";
                else
                {
                    try
                    {
                        document.Void();
                    }
                    catch { }
                }
#endif
            }

            DisplayAdapter.Cashier.Show(PosMessage.CONNECTING_TO_PRINTER);
            SetPrinterPort(PosConfiguration.Get("PrinterComPort"));

            try
            {
                if (PosConfiguration.Get("OrderPrinterAddress") != String.Empty)
                {
                    orderPrinter = ModuleManeger.LoadModule("Hugin.POS.OrderPrinter", "OrderPrinter") as IOrderPrinter;
                    orderPrinter.Connect(PosConfiguration.Get("OrderPrinterAddress"));
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
            }
#if !ORDER
            if (!Printer.CanPrint(document))
                document = new Invoice();

            if (helper != null)
            {
                if (helper.Status == DocumentStatus.Suspended)
                {
                    SalesDocument_OnSuspend(document);
                }
                else if (helper.Status == DocumentStatus.Closed)
                {
                    SalesDocument_OnClose(document);
                }
                else
                {
                    try
                    {
                        document.Void();
                    }
                    catch (ServiceRequiredException sre)
                    {
                        state = States.PrinterBlockingError.Instance(new Error(sre));
                        Log.Warning(sre);
                    }
                    catch (ClearRequiredException cre)
                    {
                        Printer.InterruptReport();
                        document.Close();
                        Log.Warning(cre);
                    }
                    catch { }
                }
            }
#endif
            try
            {
                Printer.CheckPrinterStatus();
            }
            catch (EJException eje)
            {
                SoundManager.Sound(SoundType.FATAL_ERROR);
                State = States.ElectronicJournalError.Instance(eje);
            }
            catch (ServiceRequiredException sre)
            {
                SoundManager.Sound(SoundType.NEED_PROCESS);
                state = States.PrinterBlockingError.Instance(new Error(sre));
                Log.Warning(sre);
            }
            catch (Exception)
            {
            }
            if (state is States.WaitingState) State = States.Login.Instance();
            currentItem = new SalesItem();
            
        }

        static void Login_OnLogin(ICashier sender)
        {
            DataConnector.OnCashierLogin((ICashier)sender, PrinterLastZ);
        }

        static void Login_OnLogout(ICashier sender)
        {
            DataConnector.OnCashierLogout((ICashier)sender);
        }

        private static void SetPrinterDB()
        {         
            try
            {
                if (Printer.MaxNumberOfCurrencies > 0)
                {
                    int count = Math.Min(Printer.MaxNumberOfCurrencies, DataConnector.GetCurrencies().Count);
                    ICurrency[] currencies = new ICurrency[count];
                    
                    int indx=0;
                    foreach (ICurrency curr in DataConnector.GetCurrencies().Values)
                    {
                        currencies[indx] = curr;
                        indx++;

                        if (indx >= count) break;
                    }

                    Printer.SendCurrencyInfo(currencies);
                    Log.Info("Döviz bilgileri kasaya baþarýyla yüklendi.");
                }
            }
            catch (Exception)
            {
                Log.Error("Döviz bilgileri kasaya yüklenemedi.");
            }
            //try
            //{
            //   Printer.UpdateProducts();
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("Ürün Güncelleme Yapýlamadý.");
            //}
        }

        private static IPosClient GetPromoClient()
        {
            if (promoClient != null) promoClient.LogOff();
#if Mono
			return null;
#else
            return  new PromotionServer.PromotionClient();
#endif
        }

        public static PendingFPUChange PendingFPUChanges
        {
            get { return pendingFpuChanges; }
            set { pendingFpuChanges = value; }
        }

        private static void LoadProgram()
        {
            bool requredLoadProduct = false;
#if !ORDER
            if (!Document.IsEmpty)
                Document.Void();
#endif
            Printer.EnterProgramMode();
            try
            {
                if (PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.TaxRate) > 0)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.VAT_RATE_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.VAT_RATE_UPDATING);
                    System.Threading.Thread.Sleep(500);
                    Printer.TaxRates = DataConnector.CurrentSettings.TaxRates;
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.TaxRate;
                    requredLoadProduct = true;
                }

                if (PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.Department) > 0)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.DEPARTMENT_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.DEPARTMENT_UPDATING);
                    System.Threading.Thread.Sleep(500);
                    Printer.Departments = DataConnector.CurrentSettings.Departments;
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.Department;
                    requredLoadProduct = true;
                }

                if (PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.Logo) > 0)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.LOGO_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.LOGO_UPDATING);
                    System.Threading.Thread.Sleep(500);
                    Printer.Logo = DataConnector.CurrentSettings.LogoLines;
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.Logo;
                }

                if (PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.GraphicLogo) > 0)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.GRAPHIC_LOGO_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.GRAPHIC_LOGO_UPDATING);
                    System.Threading.Thread.Sleep(500);
                    Printer.GraphicLogoActive = DataConnector.CurrentSettings.GetProgramOption(Setting.PrintGraphicLogo);
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.GraphicLogo;
                }

                if (PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.AutoCutter) > 0)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.AUTO_CUTTER_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.AUTO_CUTTER_UPDATING);
                    System.Threading.Thread.Sleep(500);
                    Printer.AutoCutter = DataConnector.CurrentSettings.GetProgramOption(Setting.Autocutter);
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.AutoCutter;
                }

                if (PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.ReceiptBarcode) > 0)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.RECEIPT_BARCODE_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.RECEIPT_BARCODE_UPDATING);
                    System.Threading.Thread.Sleep(500);
                    Printer.ReceiptBarcodeActive = DataConnector.CurrentSettings.GetProgramOption(Setting.PrintBarcode);
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.ReceiptBarcode;
                }

                if (PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.ReceiptLimit) > 0)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.RECEIPT_LIMIT_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.RECEIPT_LIMIT_UPDATING);
                    System.Threading.Thread.Sleep(500);
                    Printer.ReceiptLimit = DataConnector.CurrentSettings.ReceiptLimit;
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.ReceiptLimit;
                }               

                if (PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.Credit) > 0)
                {
                    Dictionary<int,ICredit> creditsDic = DataConnector.GetCredits();
                    ICredit[] currentCredits = new ICredit[creditsDic.Count];
                    int kp = 0;
                    foreach(KeyValuePair<int, ICredit> kpv in creditsDic)
                    {
                        currentCredits[kp] = kpv.Value;
                        kp++;
                    }

                    DisplayAdapter.Cashier.Show(PosMessage.CREDITS_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.CREDITS_UPDATING);
                    System.Threading.Thread.Sleep(500);

                    Printer.Credits = currentCredits;
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.Credit;
                }

                if(PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.Currency) > 0)
                {
                    Dictionary<int, ICurrency> currencyDic = DataConnector.GetCurrencies();
                    ICurrency[] currentCurrencies = new ICurrency[currencyDic.Count];
                    int ci = 0;
                    foreach(KeyValuePair<int, ICurrency> kvp in currencyDic)
                    {
                        currentCurrencies[ci] = kvp.Value;
                        ci++;
                    }

                    DisplayAdapter.Cashier.Show(PosMessage.CURRENCIES_DIFFERENT);
                    System.Threading.Thread.Sleep(1500);
                    DisplayAdapter.Cashier.Show(PosMessage.CURRENCIES_UPDATING);
                    System.Threading.Thread.Sleep(500);

                    Printer.Currencies = currentCurrencies;
                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.Currency;
                }

                if(PendingFPUChanges.CompareTo(PendingFPUChanges ^ PendingFPUChange.EndOfReceiptNote) > 0)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.END_OF_RECEIPT_NOTE_SAVING);
                    System.Threading.Thread.Sleep(500);

                    Printer.EndOfReceiptNote = DataConnector.CurrentSettings.DocumentRemarks;

                    PendingFPUChanges = PendingFPUChanges ^ PendingFPUChange.EndOfReceiptNote;
                }
                
                if (requredLoadProduct)
                {
                    DataConnector.LoadProducts();//change on departments
                }
            }
            catch (PrinterOfflineException)
            {
                SoundManager.Sound(SoundType.FAILED);
                throw new NoReceiptRollException();// DisplayAdapter.Cashier.Show(new Error("FiÞ KAGIDI YOK"));
            }
            finally
            {
                Printer.ExitProgramMode();
            }

            Log.Levels = (uint)DataConnector.CurrentSettings.GetProgramOption(Setting.LogLevel);
        }

        public static void LoadCurrentSettings()
        {
            bool memoryEmpty = Printer.DailyMemoryIsEmpty;
            PendingFPUChanges = PendingFPUChange.NoChange;

            SoundManager.SetVolume();

            //CurrentSettings should has been installed before, but check it
            if (DataConnector.CurrentSettings == null)
                DataConnector.LoadSettings();

            if (currentManager.AuthorizationLevel == AuthorizationLevel.P)
            {
                System.Threading.Thread.Sleep(100);
                DisplayAdapter.Cashier.Show(PosMessage.FPU_SETTINGS_CHECK);

                #region VAT Rates
                decimal[] vatRates = Printer.TaxRates;
                if (vatRates[0] < 0)
                    PendingFPUChanges = PendingFPUChanges | PendingFPUChange.TaxRate;
                else
                {
                    for (int index = 0; index < vatRates.Length; index++)
                    {
                        if (DataConnector.CurrentSettings.TaxRates[index] != vatRates[index])
                        {
                            PendingFPUChanges = PendingFPUChanges | PendingFPUChange.TaxRate;
                            if (!memoryEmpty)
                                throw new LimitExceededOrZRequiredException();
                            break;
                        }
                    }
                }
                #endregion

                #region Department
                Department[] departments = Printer.Departments;
                if (departments[0] == null)
                {
                    PendingFPUChanges = PendingFPUChanges | PendingFPUChange.Department;
                }
                else
                {
                    for (int tr = 0; tr < departments.Length; tr++)
                    {
                        if (DataConnector.CurrentSettings.Departments[tr] != null && departments[tr] != null)
                        {
                            if (!departments[tr].Name.Equals(DataConnector.CurrentSettings.Departments[tr].Name) ||
                                departments[tr].TaxGroupId != DataConnector.CurrentSettings.Departments[tr].TaxGroupId)
                            {
                                PendingFPUChanges = PendingFPUChanges | PendingFPUChange.Department;
                                if (!memoryEmpty)
                                    throw new LimitExceededOrZRequiredException();
                                break;
                            }
                        }
                    }
                }
                #endregion              

                #region Credits
                ICredit[] credits = Printer.Credits;
                Dictionary<int, ICredit> creditsDic = DataConnector.GetCredits();
                ICredit[] currentCredits = new ICredit[Printer.MaxNumberOfCredits];
                int kp = 0;
                foreach (KeyValuePair<int, ICredit> kpv in creditsDic)
                {
                    currentCredits[kp] = kpv.Value;
                    kp++;
                }
                if (credits[0] == null)
                    PendingFPUChanges = PendingFPUChanges | PendingFPUChange.Credit;
                else
                {
                    for (int tr = 0; tr < credits.Length; tr++)
                    {
                        if (currentCredits[tr] != null && credits[tr] != null)
                        {
                            if (!credits[tr].Name.Trim().Equals(currentCredits[tr].Name.Trim()))
                            {
                                PendingFPUChanges = PendingFPUChanges | PendingFPUChange.Credit;
                                if (!memoryEmpty)
                                    throw new LimitExceededOrZRequiredException();
                                break;
                            }
                        }
                    }
                }
                #endregion

                if (!Printer.IsVx675)
                {
                    #region Foreign Currencies
                    ICurrency[] currencies = Printer.Currencies;
                    Dictionary<int, ICurrency> currencyDic = DataConnector.GetCurrencies();
                    ICurrency[] currentCurrencies = new ICurrency[Printer.MaxNumberOfCurrencies];
                    kp = 0;
                    foreach(KeyValuePair<int, ICurrency> kvp in currencyDic)
                    {
                        currentCurrencies[kp] = kvp.Value;
                        kp++;
                    }
                    if (currencies[0] == null)
                        PendingFPUChanges = PendingFPUChanges | PendingFPUChange.Currency;
                    else
                    {
                        for(int tr = 0; tr< currencies.Length; tr++ )
                        {
                            if(currentCurrencies[tr] != null && currencies[tr] != null)
                            {
                                if(!currencies[tr].Name.Trim().Equals(currentCurrencies[tr].Name.Trim()) ||
                                    currencies[tr].ExchangeRate != currentCurrencies[tr].ExchangeRate)
                                {
                                    PendingFPUChanges = PendingFPUChanges | PendingFPUChange.Currency;
                                    if (!memoryEmpty)
                                        throw new LimitExceededOrZRequiredException();
                                    break;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Logo

                    String[] logos = Printer.Logo;
                    if (logos[0] == null)
                        PendingFPUChanges = PendingFPUChanges | PendingFPUChange.Logo;
                    else
                    {
                        for (int index = 0; index < logos.Length; index++)
                        {
                            if (DataConnector.CurrentSettings.LogoLines[index] != null && logos[index] != null)
                            {
                                if (DataConnector.CurrentSettings.LogoLines[index].Trim() != logos[index].Trim())
                                {
                                    PendingFPUChanges = PendingFPUChanges | PendingFPUChange.Logo;
                                    break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region End of Receipt Note
                    string[] printerNotes = Printer.EndOfReceiptNote;
                    if (printerNotes[0] == null)
                        PendingFPUChanges = PendingFPUChanges | PendingFPUChange.EndOfReceiptNote;
                    else
                    {
                        for(int index = 0; index<DataConnector.CurrentSettings.DocumentRemarks.Length; index++)
                        {
                            if(DataConnector.CurrentSettings.DocumentRemarks[index] != null && printerNotes[index] != null)
                            {
                                if(Str.FixTurkishUpperCase(DataConnector.CurrentSettings.DocumentRemarks[index].Trim()) != printerNotes[index].Trim())
                                {
                                    PendingFPUChanges = PendingFPUChanges | PendingFPUChange.EndOfReceiptNote;
                                    if (!memoryEmpty)
                                        throw new LimitExceededOrZRequiredException();
                                    break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Program Options
                    if (Printer.AutoCutter != DataConnector.CurrentSettings.GetProgramOption(Setting.Autocutter))
                        PendingFPUChanges = PendingFPUChanges | PendingFPUChange.AutoCutter;
                    if (Printer.GraphicLogoActive != DataConnector.CurrentSettings.GetProgramOption(Setting.PrintGraphicLogo))
                        PendingFPUChanges = PendingFPUChanges | PendingFPUChange.GraphicLogo;
                    if (Printer.ReceiptBarcodeActive != DataConnector.CurrentSettings.GetProgramOption(Setting.PrintBarcode))
                        PendingFPUChanges = PendingFPUChanges | PendingFPUChange.ReceiptBarcode;
                    if(Printer.ReceiptLimit != DataConnector.CurrentSettings.ReceiptLimit)
                        PendingFPUChanges = PendingFPUChanges | PendingFPUChange.ReceiptLimit;
                    #endregion
                }

                if (PendingFPUChanges != PendingFPUChange.NoChange)
                    LoadProgram();

                if (memoryEmpty)
                    SetPrinterDB();
            }            

            try
            {
                KeyMap.LoadCharMatrix(DataConnector.CurrentSettings.CharMatrix);
            }
            catch { }
        }

        public static void LoadNewSettings()
        {
            ISettings sttngsNew = DataConnector.LoadNewSettings();

            ISettings sttngsOld = DataConnector.CurrentSettings;

            bool memoryEmpty = Printer.DailyMemoryIsEmpty;
            PendingFPUChanges = PendingFPUChange.UnRead;

            for (int i = 0; i < sttngsOld.Departments.Length; i++)
            {
                if (sttngsOld.Departments[i] == null && sttngsNew.Departments[i] == null)
                    break;

                if ( (sttngsOld.Departments[i] == null && sttngsNew.Departments[i] != null) ||
                     (sttngsOld.Departments[i] != null && sttngsNew.Departments[i] == null) ||
                    !sttngsOld.Departments[i].Equals(sttngsNew.Departments[i]))
                {
                    PendingFPUChanges = PendingFPUChanges | PendingFPUChange.TaxRate;
                    //if (!memoryEmpty)
                    //    throw new LimitExceededOrZRequiredException();
                }
            }

            if (sttngsOld.GetProgramOption(Setting.Autocutter) != sttngsNew.GetProgramOption(Setting.Autocutter))
                PendingFPUChanges = PendingFPUChanges | PendingFPUChange.AutoCutter;

            if (sttngsOld.GetProgramOption(Setting.PrintGraphicLogo) != sttngsNew.GetProgramOption(Setting.PrintGraphicLogo))
                PendingFPUChanges = PendingFPUChanges | PendingFPUChange.GraphicLogo;

            DataConnector.AcceptNewSettings();

            if (PendingFPUChanges != PendingFPUChange.UnRead)
                LoadProgram();

            if (memoryEmpty)
                SetPrinterDB();
        }

        internal static void SetPrinterPort(String port)
        {
            try
            {
                Printer.BeforeZReport += new EventHandler(Printer_BeforeZReport);
                Printer.AfterZReport += new EventHandler(Printer_AfterZReport);
                Printer.DateTimeChanged += new EventHandler(Printer_DateTimeChanged);
                Printer.DocumentRequested += new EventHandler(Printer_DocumentRequested);
                Printer.OnMessage += new OnMessageHandler(Printer_OnMessage);

                Printer.Connect();
#if !ORDER
                if (document is Receipt && document.Status == DocumentStatus.Voided)
                    document = new Receipt();
#endif
                Log.Success("Connected to printer. {0}", port);

                state = States.WaitingState.Instance();
#if !ORDER
                //if (currentCashier == null)
                //{
                //    VerifyReceiptTotal();
                //}
#endif
            }
            catch (ServiceRequiredException sre)
            {
                SoundManager.Sound(SoundType.NEED_PROCESS);
                state = States.PrinterBlockingError.Instance(new Error(sre));
                Log.Warning(sre);
            }
            catch (PowerFailureException pfe)
            {
                SoundManager.Sound(SoundType.FAILED);
                AdjustPrinter(document);
                try
                {
                    Recover.RecoverPowerFailure(pfe);

                    if (!(document is Receipt))
                        document = new Receipt();
                    AdjustPrinter(document);
                    SetPrinterPort(port);
                }
                catch (EJException eje)
                {
                    State = States.ElectronicJournalError.Instance(eje);
                }
            }
            catch (UnfixedSlipException use)
            {
                SoundManager.Sound(SoundType.FAILED);
                Recover.RecoverUnfixedSlip(use);
                SetPrinterPort(port);
            }
            catch (BlockingException ex)
            {
                SoundManager.Sound(SoundType.FAILED);
                state = States.PrinterBlockingError.Instance(new Error(ex));
                Log.Warning(ex);
            }
            catch (FiscalIdException)
            {
                SoundManager.Sound(SoundType.FATAL_ERROR);
                state = States.FiscalIdBlock.Instance(FiscalRegisterNo);
            }
            catch (EJException eje)
            {
                SoundManager.Sound(SoundType.FATAL_ERROR);
                if (state is States.ElectronicJournalError)
                    throw eje;
                state = States.ElectronicJournalError.Instance(eje, VerifyReceiptTotal);
                Log.Warning(eje);
            }
            catch (FMNewException fmne)
            {
                SoundManager.Sound(SoundType.NEED_PROCESS);
                int fiscalId = int.Parse(PosConfiguration.Get("FiscalId").Substring(2, 8));
                State = States.EnterInteger.Instance(PosMessage.START_FM, fiscalId,
                                                        new StateInstance<int>(States.Login.AcceptFiscalId));
                Log.Error(fmne);
            }
            catch (ExternalDevMatchException edme)
            {
                SoundManager.Sound(SoundType.FAILED);
                DisplayAdapter.Cashier.Show(PosMessage.CANNOT_MATCH_EXT_DEV);
                //File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MatchKey.txt"));
                //Printer.Connect();
                //State = States.ConfirmCashier.Instance(new Confirm(PosMessage.RE_MATCH_WITH_FPU, new StateInstance(States.Start.Instance)));
                State = States.PrinterConnectionError.Instance(edme);
                Log.Error(edme);
            }
            catch (ZRequiredException zre)
            {
                SoundManager.Sound(SoundType.NEED_PROCESS);
                Log.Error(zre);
                if (zre.ErrorCode == 134)
                    State = States.ConfirmCashier.Instance(new Error(
                                                            zre,
                                                            new StateInstance(SendInterrupt)
                                                            ));
            }
            catch (PrinterException pe)
            {
                SoundManager.Sound(SoundType.FAILED);
                String message = new Error(pe).Message;
                State = States.PrinterConnectionError.Instance(pe);
                Log.Warning(pe);
            }
            catch (DataChangedException)
            {
                Log.Warning("Kasa-program belge uyuþmamasý ve data deðiþikliði.");
            }
            catch (Exception ex)
            {
                SoundManager.Sound(SoundType.FAILED);
                state = States.ConfirmCashier.Instance(new Error(ex, ConfirmError, ConfirmError));
                Log.Warning(ex);
            }
            
        }

        private static IState SendInterrupt()
        {
            // This InterruptReport for 24 hours Z Report so prepare for it
            DataConnector.PrepareZReport(CashRegister.Id);

            DisplayAdapter.Cashier.Show(PosMessage.WRITING_Z_REPORT);
            Printer.InterruptReport();

            return States.Start.Instance();
        }

        static void Printer_DateTimeChanged(object sender, EventArgs e)
        {
            //States.AlertCashier.Instance(new Confirm(
            //                String.Format("{0}\n{1:HH:mm}", PosMessage.TIME_CHANGED, ((DateTime)sender))));
            //State = States.Start.Instance();
        }

        static void Printer_OnMessage(object sender, OnMessageEventArgs e)
        {
            try
            {
                String msg = "";
                if (e.IsError)
                {
                    Error err = new Error(e.Exception);
                    msg = err.Message;
                }
                else
                {
                    msg = e.Message;
                }
                TimeSpan ts = DateTime.Now.Subtract(lastMsgTime);
                lastMsg = msg;
                lastMsgTime = DateTime.Now;

                if (ts.TotalSeconds < 2)
                    return;
                Confirm c = new Confirm(msg);
                DisplayAdapter.Cashier.Show(c);
                System.Threading.Thread.Sleep(500);
                DisplayAdapter.Cashier.ClearError();
                DisplayAdapter.Cashier.Show(c.Message);
                /*to show cashier msg after z report*/
                if (State is States.Login)
                    States.Login.Instance();
                /**/
            }
            catch (Exception)
            {
            }
        }
        private static IState OnError()
        {
            return state;
        }
        private static IState ConfirmError()
        {
            SetPrinterPort(PosConfiguration.Get("PrinterComPort"));
            return state;
        }
        internal static void Void()
        {
            Printer.Void();
        }
        public static IScale Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public static DateTime LastZReportDate
        {
            get { return lastZReportDate; }
            set { lastZReportDate = value; }
        }

        static void SalesDocument_OnVoid(object sender)
        {
#if ORDER

            DataConnector.OnDocumentVoided(sender as ISalesDocument, (int)((SalesDocument)sender).Status);

#else

            if (Printer.CurrentDocumentId != Document.Id || Document is ReturnDocument)
            {
                DataConnector.OnDocumentVoided(sender as ISalesDocument, (int)((SalesDocument)sender).Status);
            }
#endif
            if (IsDesktopWindows && promoClient != null)
                promoClient.Messages((int)PromoMessageCode.VoidDocument, Str.Split(DataConnector.FormatLines(document), "\r\n"));

            InitializeDocument();
        }

        static void SalesDocument_OnSuspend(object sender)
        {
            try
            {
                int zNo = 0;
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        zNo = PrinterLastZ + 1;// Printer current z no;
                        break;
                    }
                    catch (Exception ex)
                    {
                        SoundManager.Sound(SoundType.FAILED);
                        Debugger.Instance().AppendLine("Z: " + ex.Message);
                        System.Threading.Thread.Sleep(100);
                    }
                }

                DataConnector.OnDocumentSuspended(sender as ISalesDocument, zNo);

                if (IsDesktopWindows && promoClient != null)
                    promoClient.Messages((int)PromoMessageCode.SuspendDocument, Str.Split(DataConnector.FormatLines(document), "\r\n"));
            }
            catch (Exception ex)
            {
                SoundManager.Sound(SoundType.FAILED);
                throw new DocumentSuspendException(ex);
            }
            InitializeDocument();
        }

        static void DataConnector_LogSaved(object sender, LogSavedEventArgs e)
        {
            string log = e.Log;
            string[] logLines = log.Split('\n');

            document.CurrentLog = new List<string>(logLines);

        }

        static void SalesDocument_OnClose(object sender)
        {
            ISalesDocument document = sender as ISalesDocument;
            if (sender is ReturnDocument)
                DataConnector.OnReturnDocumentClosed(document);
            else
                DataConnector.OnDocumentClosed(document);

            if (IsDesktopWindows && promoClient != null)
                promoClient.Messages((int)PromoMessageCode.CloseDocument, Str.Split(DataConnector.FormatLines(document), "\r\n"));

            InitializeDocument();
        }

        static void SalesDocument_PaymentMade(object sender, PaymentEventArgs e)
        {
            if (sender != document) return;
            DataConnector.OnDocumentUpdated(document, (int)document.Status);
        }

        static void SalesDocument_ItemSold(object sender, SaleEventArgs e)
        {
            DataConnector.OnDocumentUpdated((SalesDocument)sender, (int)document.Status);
        }

        static void SalesDocument_ItemUpdated(object sender, SaleEventArgs e)
        {
            DataConnector.OnDocumentUpdated((SalesDocument)sender, (int)document.Status);
        }

        static void currentItem_OnTotalAmountUpdated(object sender, PriceUpdateEventArgs e)
        {
            if (sender != document.LastItem) return;
            DataConnector.OnDocumentUpdated(document, (int)document.Status);
        }

        static void Display_SaleSelected(object sender, SalesSelectedEventArgs e)
        {
            string[] args;
            try
            {
                // "SaleIndex | ProductName | VATRate | Quantity x UnitPrice | Total Amount"
                args = e.RetVal.Split('|');
                DisplayAdapter.Cashier.Show(String.Format("{0}\n{1}\t{2}", args[1], args[3], args[4]));
            }
            catch { return; }

            System.Collections.Generic.List<IProduct> sList = new System.Collections.Generic.List<IProduct>();

            sList = DataConnector.SearchProductByName(args[1]);
            if (sList.Count == 0)
            {
                state = States.AlertCashier.Instance(new Error(new ProductNotFoundException()));
                return;
            }

            foreach (IProduct p in sList)
            {
                FiscalItem fi = (FiscalItem)currentItem.Clone();
                fi.Product = p;

                string[] arr = args[3].Split('x');
                fi.Quantity = Decimal.Parse(arr[0]);
                currentItem = fi;
            }

            // Set as last item also
            foreach(FiscalItem fi in document.Items)
            {
                if (fi.Name.Trim() == currentItem.Name.Trim() &&
                    fi.Quantity == currentItem.Quantity)
                    document.LastItem = fi;
            }

            //IsSaleSelected = true;
        }

        static void Display_SalesFocusLost(object sender, EventArgs e)
        {
            //IsSaleSelected = false;
        }

        static void InitializeDocument()
        {
            document = new Receipt();
            if (Printer.CanPrint(document))
                return;
            document = new Invoice();
        }

        static void Printer_DocumentRequested(object sender, EventArgs e)
        {
            String docName = document.Name;
            if (document is ReturnDocument)
                docName = PosMessage.INVOICE;
            DisplayAdapter.Cashier.Show(String.Format("{0}\n{1}", docName, PosMessage.PUT_IN));
            System.Threading.Thread.Sleep(100);
        }
        static void Printer_BeforeZReport(object sender, EventArgs e)
        {
            DataConnector.PrepareZReport(CashRegister.Id);
            Printer.PrepareDailyReport();

            if (!Document.IsEmpty)
                Document.Void();
            //if (CurrentCashier != null)
            //    States.Login.SignOutCashier();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_Z_REPORT);
        }


        static void Printer_AfterZReport(object sender, EventArgs e)
        {
            if (PosConfiguration.IsPrinterGUIActive)
            {
                IPrinterResponse response = (IPrinterResponse)sender;

                DataConnector.AfterZReport(CashRegister.Id);

                String reportName = "Z_RAPORU";
                if (response.Detail != null)
                    DataConnector.SaveReport(reportName, response.Detail);

                int reportNo = int.Parse(response.Data == null ? "0" : response.Data);
                if (States.ReportMenu.OnZReportComplete != null)
                    States.ReportMenu.OnZReportComplete(reportNo);

                DateTime ZReportDate = DateTime.Now;
                DataConnector.OnZReportComplete(reportNo, ZReportDate, Printer.IsFiscal);

                String strZLine = String.Format("16,ZRP,{0:dd}/{0:MM}/{0:yyyy}  ,{0:HH:mm:ss}{1:0000}", ZReportDate, reportNo);

                if (IsDesktopWindows && promoClient != null)
                    promoClient.Messages((int)PromoMessageCode.ZReport, new String[] { strZLine });

                document = new Receipt();
                lastZReportDate = Printer.LastZReportDate;
                state = States.Login.Instance();
            }
            else
            {
                CPResponse response = (CPResponse)sender;

                DataConnector.AfterZReport(CashRegister.Id);

                String reportName = "Z_RAPORU";
                if (response.Detail != null)
                    DataConnector.SaveReport(reportName, response.Detail);
                int reportNo = int.Parse(response.GetParamByIndex(2) == null ? "0" : response.GetParamByIndex(2));
                if (States.ReportMenu.OnZReportComplete != null)
                    States.ReportMenu.OnZReportComplete(reportNo);

                DateTime ZReportDate = DateTime.Now;
                DataConnector.OnZReportComplete(reportNo, ZReportDate, Printer.IsFiscal);

                String strZLine = String.Format("16,ZRP,{0:dd}/{0:MM}/{0:yyyy}  ,{0:HH:mm:ss}{1:0000}", ZReportDate, reportNo);

                if (IsDesktopWindows && promoClient != null)
                    promoClient.Messages((int)PromoMessageCode.ZReport, new String[] { strZLine });

                document = new Receipt();
                lastZReportDate = Printer.LastZReportDate;
                state = States.Login.Instance();
            }
        }

        internal static IState State
        {
            get { return state; }
            set {
                if (state != null)
                {
                    Log.Debug("State exit {0}", state.GetType());
                    state.OnExit();
                } 
                state = value;
                Log.Debug("State entry {0}", value.GetType());
                state.OnEntry();
            }
        }

        public static SalesDocument Document {
			get {
				return document; 
			}
            set
            {
                if (document != null && !document.IsEmpty
                                     && (document.Status == DocumentStatus.Active ||
                                         document.Status == DocumentStatus.Paying ||
                                         document.Status == DocumentStatus.Transferred)
                                     )
                    document.Void();
                document = value;
                if (DocumentChanged != null)
                    DocumentChanged(document, new EventArgs());

                if (document.Customer == null)
                    DisplayAdapter.Cashier.LedOff(Leds.Customer);
                else
                    DisplayAdapter.Cashier.LedOn(Leds.Customer);
            }
        }
        internal static FiscalItem Item
        {
            get { return currentItem; }
            set { currentItem = value; }
        }

        internal static int PrinterLastZ
        {
            get {
                if (printerLastZ < 0)
                {
                    printerLastZ = Printer.LastZReportNo;
                }
                return printerLastZ;
            }
            set { printerLastZ = value; }
        }

        internal static IFiscalPrinter Printer
        {
            get
            {              
                return FiscalPrinter.Printer;
            }
        }

        internal static IOrderPrinter OrderPrinter
        {
            get
            {
                return orderPrinter;
            }
        }

        internal static IDataConnector DataConnector
        {
            get
            {
                return Data.Connector.Instance();
            }
        }

        internal static ISecurityConnector SecurityConnector
        {
            get
            {
                return Security.Connector.Instance();
            }
        }

        internal static String FiscalRegisterNo
        {
            get { return fiscalRegisterNo; }
            set { fiscalRegisterNo = value; }
        }

        public static Boolean HasEJ
        {
            get { return FiscalRegisterNo.StartsWith("F"); }
        }

        public static Common.EZLogger Log { 
            get {return Common.EZLogger.Log;}
        }

        public static AuthorizationLevel GetAuthorizationLevel(Authorizations operation)
        {
            try
            {
                return DataConnector.CurrentSettings.GetAuthorizationLevel(operation);
            }
            catch
            {
                return AuthorizationLevel.Z;
            }
        }

        internal static void Execute(FiscalItem fi)
        {
            if (fi.Product.Id < 0) return;
            currentItem = fi;
            if (currentItem is SalesItem)
                SellCurrentItem();
            else VoidCurrentItem();
        }
        internal static void Execute(Object o)
        {
            if (o is IProduct)
                Execute((IProduct)o);
            else
                Execute((FiscalItem)o);
        }
        internal static void Execute(IProduct p)
        {
            //Assign product to current item and sell/void the current item           
            currentItem.Product = p;
            decimal unitprice_itemsold = 0;

            Dictionary<Decimal, Decimal> unitprices = new Dictionary<decimal, decimal>();

            if (currentItem is SalesItem)
            {
                SellCurrentItem();
            }
            else if (currentItem is VoidItem)
            {
                /* if the sale list contains at least two different unit prices of items 
                 * whose product is wanted to cancel
                 * and 
                 * whose remaining quantity is bigger than quantity to cancel
                 * then confirm cashier to enter menu list to select the item cashier wants to cancel
                 */
                foreach (FiscalItem item in document.Items)
                {
                    if (!(item is SalesItem) || item.Product.Id != p.Id) continue;

                    SalesItem si = (SalesItem)item;

                    if(si.Adjustments.Count>0)
                    {
                        if ((si.Quantity - si.VoidQuantity) != Math.Abs(currentItem.Quantity))
                        {
                            continue;
                        }
                    }
                    if (unitprices.ContainsKey(si.UnitPrice))
                        unitprices[si.UnitPrice] += si.Quantity - si.VoidQuantity;
                    else
                        unitprices.Add(si.UnitPrice, si.Quantity - si.VoidQuantity);

                }

                foreach (Decimal unitprice in unitprices.Keys)
                {
                    if (unitprices[unitprice] < Math.Abs(currentItem.Quantity)) continue;
                        
                    if (unitprice_itemsold == 0) unitprice_itemsold = unitprice;

                    else if (unitprice != unitprice_itemsold)
                    {
                        Confirm error = new Confirm(PosMessage.ENTER_LIST_FOR_SPECIAL_PRICED_PRODUCT_VOID,
                                                 new StateInstance<Hashtable>(States.Selling.VoidAdjustedItem));
                        error.Data.Add("Product", p);
                        error.Data.Add("Quantity", Math.Abs(currentItem.Quantity));
                        error.Data.Add("UnitPrices", unitprices);
                        state = States.ConfirmCashier.Instance(error);
                        return;
                    }
                }
                /* if the conditions above does not occur directly cancel the item
                 * but the unitprice of the voiditem must be changed as unitprice of item 
                 * which is at the list and whose quantity is enough
                 */
                if (unitprice_itemsold == 0)
                {
                    throw new VoidException(PosMessage.CANNOT_VOID_NO_PROPER_SALE);
                }
                ((VoidItem)currentItem).UnitPrice = unitprice_itemsold;
                VoidCurrentItem();
            }
            else
                throw new InvalidOperationException();

        }

        internal static void UpdateSelectedSaleQuantity(Number newQuantity)
        {
            try
            {
                document.UpdateItem(currentItem, newQuantity);
                currentItem.Quantity = newQuantity.ToDecimal();
            }
            catch(Exception ex)
            { throw ex; }
            

            CashRegister.DataConnector.OnDocumentUpdated(document, (int)document.Status);
            DisplayAdapter.Cashier.ChangeDocumentStatus(document, DisplayDocumentStatus.OnChange);
            DisplayAdapter.Cashier.Show(currentItem);

            State = States.Selling.Instance();
        }

        internal static void SellCurrentItem()
        {
            if (Math.Round(Item.TotalAmount, 2) <= 0)
            {
                currentItem.Reset();
                state = States.AlertCashier.Instance(new Confirm(PosMessage.MIN_AMOUNT_ERROR));
                return;
            }

            // Adding last sales person to item's sales person
            if (Document.LastItem != null && Document.LastItem.SalesPerson != null)
                currentItem.SalesPerson = Document.LastItem.SalesPerson;

            #region Yemek fiþi/kartý satýþlarda KDV oraný kontrolü
            if (document is MealTicket)
            {
                if (mealVATRates == null)
                {
                    mealVATRates = new List<int>();
                    int vats = DataConnector.CurrentSettings.GetProgramOption(Setting.MealVATRates);

                    string tmp = "";
                    if (vats.ToString().Length % 2 != 0)
                    {
                        tmp = vats.ToString().Insert(0, "0");
                    }
                    else
                        tmp = vats.ToString();

                    int iterarion = tmp.Length / 2;
                    for (int i = 0; i < iterarion; i++)
                    {
                        string s = tmp.Substring(i * 2, 2);
                        mealVATRates.Add(int.Parse(s));
                    }
                }

                bool itemCanSale = false;
                foreach (int vat in mealVATRates)
                {
                    if (DataConnector.CurrentSettings.TaxRates[currentItem.TaxGroupId - 1] == vat)
                    {
                        itemCanSale = true;
                    }
                }
                if (!itemCanSale)
                {
                    state = States.AlertCashier.Instance(new Confirm(PosMessage.ALERT_INVALID_VAT_ON_MEAL_TICKET));
                    return;
                }
            }
            #endregion

            #region Fiyatli satista kasiyer limit kontrolu
                if ((Item.TotalAmount != Item.ListedAmount) &&
                    (Item.Product.Status != ProductStatus.Programmable))
                {
                    Adjustment adjustment = new Adjustment(Item,
                                                (Item.TotalAmount > Item.ListedAmount) ? AdjustmentType.PercentFee : AdjustmentType.PercentDiscount,
                                                0);
                    if (!CurrentCashier.IsAuthorisedFor(adjustment))
                    {
                        currentItem.Reset();
                        state = States.AlertCashier.Instance(new Confirm(String.Format("{0} {1}", adjustment.Label, PosMessage.INSUFFICIENT_LIMIT)));
                        return;
                    }
                }
            #endregion

            #region Programlanabilen Ürünlerin fiyatýnýn kasiyer tarafýndan girilmesi
            if (currentItem.Product.Status == ProductStatus.Programmable || 
                    (!(State is States.ListFiscalItem) && 
                    document is ReturnDocument && 
                    Connector.Instance().CurrentSettings.GetProgramOption(Setting.AskPriceForReturnItems) == 1))
            {
                state = States.EnterDecimal.Instance(PosMessage.ENTER_PRODUCT_PRICE,
                                                     currentItem.Product.UnitPrice,
                                                     new StateInstance<Decimal>(GetProgrammableProductPrice));
                return;
            }
            #endregion

            #region KareKod Barkodlu satista Ýlac Seri Numarasý tekrari kontrolu
            if (currentItem.Product.RequiredField == ProductRequiredField.All)
            {
                //Check all fields were defined.
                if(String.IsNullOrEmpty(currentItem.SerialNo) ||
                    String.IsNullOrEmpty(currentItem.BatchNumber) ||
                    currentItem.ExpiryDate == null)
                {
                    state = States.AlertCashier.Instance(
                                new Confirm(String.Format("{0}", PosMessage.SALE_FROM_QR_CODE)));
                    return;
                }
                foreach (FiscalItem fi in document.Items)
                {
                    if (fi is SalesItem)
                    {
                        if (fi.SerialNo == currentItem.SerialNo && fi.Name == currentItem.Name)
                        {
                            if (!(document.Items.Exists(delegate(FiscalItem fiTemp)
                                { 
                                    return ((fiTemp.SerialNo == currentItem.SerialNo) && 
                                            (fiTemp.VoidQuantity != 1) &&
                                             (fiTemp is SalesItem)); 
                                }))) 
                                continue;

                            currentItem.Reset();
                            state = States.AlertCashier.Instance(new Confirm(String.Format("{0}", PosMessage.SERIAL_NUMBER_ALREADY_EXIST)));
                            return;
                        }
                    }
                }
            }
            #endregion


            state = EnterProductSerial();

        }


        private static IState GetProgrammableProductPrice(Decimal price)
        {
            if (price == 0)
            {
                States.AlertCashier.Instance(new Confirm(PosMessage.ZERO_PLU_PRICE_ERROR));
                return state = States.Start.Instance();
            }
            currentItem.UnitPrice = price;
            
            return state = EnterProductSerial();
        }

        private static IState EnterProductSerial()
        {
            if ((currentItem.Product.RequiredField == ProductRequiredField.SerialNumber) && 
                    (currentItem.Quantity == 1) &&
                    (String.IsNullOrEmpty(currentItem.SerialNo)))

                return States.EnterString.Instance(PosMessage.ENTER_PRODUCT_SERIALNO,
                                         new StateInstance<String>(GetProductSerialNumber));
            else
                return State = AddSalesPersonEachProduct();
        }

        private static IState GetProductSerialNumber(string serialNo)
        {
            if (!DataConnector.AvailableSerialNumber(serialNo))
                return States.AlertCashier.Instance(new Confirm(PosMessage.SERIAL_NUMBER_NOT_FOUND));

            currentItem.SerialNo = serialNo;

            return state = AddSalesPersonEachProduct();
        }

        private static IState AddSalesPersonEachProduct()
        {
            if (!document.AddSalesPersonEachSales)
                return state = ContinueSellCurrentItem();


            return State = States.EnterClerkNumber.Instance(PosMessage.CLERK_ID,
                                                new StateInstance<ICashier>(ConfirmItemSalesPerson),
                                                new StateInstance(ContinueSellCurrentItem));

        }

        private static IState ConfirmItemSalesPerson(ICashier cashier)
        {
            Confirm confirm = new Confirm(String.Format("{0}{1}", PosMessage.CLERK_FOR_ITEM, cashier.Name.TrimEnd()),
                                              new StateInstance<Hashtable>(SaveSalesPerson),
                                              new StateInstance(States.EnterPassword.Instance));
            confirm.Data.Add("SalesPerson", cashier);
            return States.ConfirmCashier.Instance(confirm);
        }

        private static IState SaveSalesPerson(Hashtable args)
        {
            ICashier cashier = null;
            if (args["SalesPerson"] is ICashier)
            {
                cashier = (ICashier)args["SalesPerson"];
                //if ((document.LastItem != null && document.LastItem.SalesPerson != null) &&
                //     document.LastItem.SalesPerson != cashier)
                //    document.LastItem.VoidSalesPerson();

                currentItem.SalesPerson = cashier;
            }
            return state = ContinueSellCurrentItem();
        }

        private static IState ContinueSellCurrentItem()
        {
            ComplateSellingCurrentItem();
            return state;
        }

        private static void ComplateSellingCurrentItem()
        {
            try
            {
                if (document.CanAddItem(currentItem))
                {
                    Adjustment promoItemAdj = null;
                    if (IsDesktopWindows)
                    {
                        //Get available product promotion 
                        try
                        {
                            promoItemAdj = ApplyProductPromotion();
                        }
                        catch (InvalidSecurityKeyException iske)
                        {
                            throw iske;
                        }
                        catch (ProductPromotionLimitExeedException pplee)
                        {
                            throw pplee;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Promosyon Hatasý: " + ex.Message);
                        }
                    }
                    
                    if (document.IsEmpty)
                    {
                        UpdateUnsavedDocument(currentItem);
                        try
                        {
                            document.CreatedDate = DateTime.Now;
                            document.Id = Printer.CurrentDocumentId;
                            Printer.PrintHeader(document);
                        }
                        catch (CmdSequenceException cse)
                        {
                            SoundManager.Sound(SoundType.FAILED);
                            if (cse.LastCommand != 20)
                                throw cse;
                            document.Id = Printer.CurrentDocumentId;
                        }
                        catch (UnfixedSlipException use)
                        {
                            throw use;
                        }
                        catch (Exception ex)
                        {
                            SoundManager.Sound(SoundType.FAILED);
                            CashRegister.DataConnector.OnDocumentUpdated(document, (int)document.Status);
                            DisplayAdapter.Cashier.ChangeDocumentStatus(document, DisplayDocumentStatus.OnClose);
                            throw ex;
                        }

                        if (IsDesktopWindows && promoClient != null)
                            promoClient.Messages((int)PromoMessageCode.StartDocument, Str.Split(DataConnector.FormatLines(document), "\r\n"));
                    }


                    Printer.Print(currentItem);
                    document.AddItem(currentItem, true);
                    currentItem.OnTotalAmountUpdated += new OnTotalAmountUpdatedEventHandler(currentItem_OnTotalAmountUpdated);
                    currentItem.Show();

                    if (promoItemAdj != null && Document.CanAdjust(promoItemAdj))
                    {
                        promoItemAdj.Target.Adjust(promoItemAdj);
                        Printer.Print(currentItem.Adjustments[0]);
                        DisplayAdapter.Both.Show(promoItemAdj);
                    }
                        

                    #region Print customer promotion earning..
                    if (DataConnector.CurrentSettings.GetProgramOption(Setting.PrintSecondaryPricePromotion) != PosConfiguration.ON && 
                            (currentItem.Product.Status != ProductStatus.Programmable))
                    {
                        if (currentItem.Product.UnitPrice > currentItem.UnitPrice)
                        {
                            decimal totalUnit = Rounder.RoundDecimal(currentItem.Product.UnitPrice * currentItem.Quantity, 2, true);
                            decimal earn = totalUnit - currentItem.TotalAmount;
                            Printer.PrintRemark(String.Format("{0} : *{1:C}",PosMessage.GAINS, new Number(earn)));
                        }
                    }
                    #endregion

                    if (DataConnector.CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON &&
                            currentItem.SalesPerson != null)
                    {
                        Printer.PrintRemark(String.Format("{0} : {1} ", 
                                                                        PosMessage.CLERK, 
                                                                        currentItem.SalesPerson.Id)
                                                                        );
                    }
                }
                State = States.Selling.Instance();
            }
            catch (ReceiptLimitExceededException)
            {
                ////Sales document is a receipt which needs to be 
                ////converted to an invoice
                //Invoice invoice = new Invoice(document);
                //if (!Printer.CanPrint(invoice))
                //    throw new ReceiptLimitExceededException();
                //invoice.AddItem(currentItem, true);
                //MenuList docTypes = new MenuList();
                //docTypes.Add(new MenuLabel(PosMessage.RECEIPT_LIMIT_EXCEEDED_TRANSFER_DOCUMENT, invoice));
                //IDoubleEnumerator ie = (IDoubleEnumerator)docTypes.GetEnumerator();
                //State = States.ListDocument.Instance(ie, new ProcessSelectedItem<SalesDocument>(ChangeDocumentType));

                //DisplayAdapter.Cashier.Show(PosMessage.RECEIPT_LIMIT_EXCEEDED_TRANSFER_DOCUMENT);
                SoundManager.Sound(SoundType.FAILED);
                DisplayAdapter.Cashier.Show(PosMessage.RECEIPT_LIMIT_EXCEEDED);
            }
            catch (CmdSequenceException ex)
            {
                SoundManager.Sound(SoundType.FAILED);
                if (ex.LastCommand == 35)
                {
                    document = new Receipt();
                    SellCurrentItem();
                    return;
                }
                Log.Error(ex);
                State = States.Payment.Continue();
            }
            catch (OverflowException ofe)
            {
                SoundManager.Sound(SoundType.FAILED);
                state = States.AlertCashier.Instance(new Error(ofe));
            }
            catch (SaleClosedException sce)
            {
                SoundManager.Sound(SoundType.FAILED);
                state = States.AlertCashier.Instance(new Error(sce));
            }

            finally
            {
                currentItem = new SalesItem();
            }

        }

        private static Adjustment ApplyProductPromotion()
        {
            SalesDocument tempDocument = (SalesDocument)document.Clone();
            tempDocument.Id = document.Id;

            tempDocument.AddItem(currentItem, false);

            PromotionDocument promoDocument = new PromotionDocument(tempDocument, null, PromotionType.Item);

            foreach (Adjustment promoAdjustment in promoDocument.ItemAdjustments)
            {
                if (promoAdjustment.Target == currentItem)
                {
                    return promoAdjustment;
                }
            }

            return null;
        }

        //For store void document which voided when logo lines prints
        private static void UpdateUnsavedDocument(FiscalItem currentItem)
        {
            SalesDocument tempDoc = (SalesDocument)document.Clone();
            tempDoc.Id = 1;//to save
            tempDoc.AddItem(currentItem, true);
        }

        internal static void VoidCurrentItem()
        {
            try
            {
                FiscalItem tempCurrentItem = (FiscalItem)currentItem.Clone();
                int itemFoundIndex = -1;
                if (document.CanAddItem(currentItem))
                {
                    /**/
                    Adjustment adj = null;
                    for (int i = 0; i < document.Items.Count; i++)
                    {
                        //Todo: fi must be salesitem because fiscalitem.VoidAmount always returns is zero
                        FiscalItem fi = document.Items[i];
                        if (fi is SalesItem &&
                            fi.UnitPrice == Math.Abs(currentItem.UnitPrice) &&
                            fi.Product.Id == currentItem.Product.Id &&
                            fi.SerialNo == currentItem.SerialNo)
                        {
                            if (fi.Adjustments.Count > 0)
                            {
                                if ((fi.Quantity - fi.VoidQuantity) != Math.Abs(currentItem.Quantity))
                                {
                                    continue;
                                }
                                if (fi.Adjustments.Count == 2 &&
                                    (fi.Adjustments[0].NetAmount + fi.Adjustments[1].NetAmount == 0))
                                {
                                    adj = null;
                                }
                                else
                                {
                                    adj = fi.Adjustments[fi.Adjustments.Count - 1];
                                }
                                currentItem.UnitPrice = Rounder.RoundDecimal(fi.TotalAmount / fi.Quantity, 2, true);
                                itemFoundIndex = i;
                                break;
                            }
                            else if ((fi.Quantity - fi.VoidQuantity) == Math.Abs(currentItem.Quantity))
                            {
                                itemFoundIndex = i;
                                break;
                            }

                        }
                    }

                    try
                    {

                        Printer.Void(adj);

                        if (adj != null)
                        {
                            currentItem.UnitPrice = (document.Items[itemFoundIndex].TotalAmount - adj.NetAmount) / (document.Items[itemFoundIndex].Quantity);
                        }

                        //if (Printer.Void(currentItem) == null) return;
                    
                    }
                    catch (NotSupportedException)
                    {
                        if (Printer.Void(tempCurrentItem) == null) return;
                    }

                    document.AddItem(tempCurrentItem, true);
                }

                Decimal quantity = currentItem.Quantity * Decimal.MinusOne;
                Decimal amount = currentItem.TotalAmount * Decimal.MinusOne;

                if (itemFoundIndex >= 0)
                {
                    SalesItem si = (SalesItem)document.Items[itemFoundIndex];

                    si.VoidQuantity += Math.Min(si.Quantity - si.VoidQuantity, quantity);
                    si.VoidAmount += Math.Min(si.TotalAmount - si.VoidAmount, amount);
                }
                else
                {
                    /* search the sale list to find items whose product is equal to product of item to be cancel
                     * and whose unitprice is equal to item to be cancel
                     * if item is found then
                     * select the minimum quantity between remaining quantity of sale item and remaing quantity to be cancel
                     * select the minimum amount between remaining amount of sale item and remaing amount to be cancel
                     * when quantity to be cancel becomes zero break the search.
                     */
                    foreach (FiscalItem item in document.Items)
                    {
                        if ((item is SalesItem) &&
                            (item.Product.Id == currentItem.Product.Id) &&
                            (Rounder.RoundDecimal(item.UnitPrice, 2, true) == currentItem.UnitPrice))
                        {
                            SalesItem si = (SalesItem)item;
                            Decimal decrementQuantity = Math.Min(si.Quantity - si.VoidQuantity, quantity);
                            Decimal decrementAmount = Math.Min(si.TotalAmount - si.VoidAmount, amount);
                            quantity -= decrementQuantity;
                            amount -= decrementAmount;
                            si.VoidQuantity += decrementQuantity;
                            si.VoidAmount += decrementAmount;
                            if (quantity <= 0 || amount <= 0) break;
                        }
                    }
                }

                try
                {
                    MenuList list = null;
                    DisplayAdapter.Cashier.Show(list);
                    tempCurrentItem.Show();
                }
                catch { }
                currentItem = new SalesItem();
                state = States.Selling.Instance();
            }
            catch (VoidException ve)
            {
                SoundManager.Sound(SoundType.FAILED);
                Item = new SalesItem();
                State = States.AlertCashier.Instance(new Error(ve));
            }
            catch (OverflowException ofe)
            {
                SoundManager.Sound(SoundType.FAILED);
                state = States.AlertCashier.Instance(new Error(ofe));
            }
        }
        internal static void AdjustPrinter(SalesDocument document)
        {
            try
            {
                Printer.AdjustPrinter(document);
                Printer.DocumentRequested += new EventHandler(Printer_DocumentRequested);
            }
            catch(Exception ex)
            {
                SoundManager.Sound(SoundType.FAILED);
                throw ex;
            }
        }

        internal static void GetEFTSlipCopy(int acquierID, int batchNo, int stanNo, int zNo, int docNo)
        {
            try
            {
                Printer.GetEFTSlipCopy(acquierID, batchNo, stanNo, zNo, docNo);
            }
            catch (Exception ex)
            {
                SoundManager.Sound(SoundType.FAILED);
                state = States.AlertCashier.Instance(new Error(ex));
            }
        }

        internal static void ChangeDocumentType(SalesDocument document)
        {
            tcknChecked = false;
            Document.Transfer();
            Document = document as SalesDocument;

            if (Document is Receipt ||
                Document is MealTicket ||
                Document is CarParkDocument ||
                Document is CollectionInvoice)
                ChangeDocumentType();
            else
            {
                if (Document.Customer != null) 
                    State = ChangeDocumentType();
                else
                    State = States.CustomerInfo.Instance(new StateInstance(ChangeDocumentType),
                                                        new StateInstance(ChangeDocumentType));
            }
             

        }
        public static IState ChangeDocumentType()
        {
            Printer.ReleasePrinter();

            if (document.Customer != null)
            {
                try
                {
                    String[] contact = document.Customer.Contact;//test info
                    SecurityConnector.AcceptCustomer(document.Customer.Number);
                }
                catch (MissingCardInfoException mse)
                {
                    return States.ConfirmCashier.Instance(
                        new Confirm(mse.Message,
                        new StateInstance(ChangeDocumentType),
                        new StateInstance(CancelSlipSale)));
                }
            }

            if (!(Document is Receipt))
            {
                return State = DocumentPaymentStatus();
            }

            if (!Document.IsEmpty)
            {
                return ContinueSlipSale();
            }

            return States.Start.Instance();
        }

        private static IState DocumentPaymentStatus()
        {
            return State = States.DocumentPaymentStatus.Instance(ContinueStatusChangedSlip, ContinueStatusChangedSlip);
        }

        private static IState ContinueStatusChangedSlip()
        {
            document.Id = Printer.CurrentDocumentId;

            switch (Document.DocumentTypeId)
            {
                case 1: // Invoice
                    return EnterInvoiceSerialNumber();  
                case 2: // E-Invoice
                case 3: // E-Archive

                    /*
                     * if customer is not E-Invoice tax prayer and current doc type is E-Invoice, we change document to E-Invoice
                     * On the opposite state, we change document type to E-Archieve
                     * After these changes we re-start ChangeDocument state again
                     * tcknChecked flag controls tckn or vkn value is checked before
                     */
                     if(false)
                    //if (!tcknChecked)
                    {
                        DisplayAdapter.Cashier.Show(PosMessage.CHECKING_TAXPAYER_STATUS);
                        System.Threading.Thread.Sleep(1000); // for displaying cashier message clearly

                        // For succes print until online e-doc devps completed
                        if (document.Customer == null && String.IsNullOrEmpty(document.TcknVkn))
                            document.TcknVkn = "1234567890";

                        string tcknVkn = document.Customer == null ? document.TcknVkn : document.Customer.Contact[4].Trim(); 

                        if (!EDocumentManager.CheckTaxPayerStatus(tcknVkn))
                        {
                            tcknChecked = true;
                            DisplayAdapter.Cashier.Show(PosMessage.NOT_EINVOICE_TAXPAYER);
                            System.Threading.Thread.Sleep(2000);

                            if (document is EInvoice)
                            {
                                document = ChangeDocumentType(document, DocumentTypes.E_ARCHIEVE);
                                return ChangeDocumentType();
                            }
                        }
                        else
                        {
                            tcknChecked = true;
                            if (document is EArchive)
                            {
                                document = ChangeDocumentType(document, DocumentTypes.E_INVOICE);
                                return ChangeDocumentType();
                            }
                        }
                        
                    }
                    return  States.AdditionalInfoMenu.Instance(new StateInstance(ContinueSlipSale));
                case 4: // Meal Ticket
                    break;
                case 5: // Car Parking
                    return EnterCarPlate();

                case 6: // Advance
                    if (Document.Customer == null)
                        return EnterCustomerName();
                    else
                        return EnterReturnReason();

                case 7: // Collection Invoice
                    return EnterInvoiceSerialNumber();
                case 8: // Return Document
                    return EnterInvoiceSerialNumber();
                case 9: // Current Account Document
                    if (Document.Customer == null)
                        return EnterCustomerName();
                    else
                        return EnterSlipSerialNumber();
                case 10: // Self Emplyoment Invoice
                    Document.IssueDate = DateTime.Now;
                    if (Document.Customer == null)
                        return EnterCustomerName();
                    else
                        return EnterServiceDefinition();
                default:
                    return EnterReturnReason();
            }
            return EnterReturnReason();
        }

        public static SalesDocument ChangeDocumentType(SalesDocument document, DocumentTypes toChangeType)
        {
            string tcknVkn = document.TcknVkn;
            SalesDocument sDoc;
            switch (toChangeType)
            {
                case DocumentTypes.RECEIPT:
                    sDoc = new Receipt(document);
                    break;
                case DocumentTypes.INVOICE:
                    sDoc = new Invoice(document);
                    break;
                case DocumentTypes.E_INVOICE:
                    sDoc = new EInvoice(document);
                    break;
                case DocumentTypes.E_ARCHIEVE:
                    sDoc = new EArchive(document);
                    break;
                case DocumentTypes.MEAL_TICKET:
                    sDoc = new MealTicket(document);
                    break;
                case DocumentTypes.CAR_PARKING:
                    sDoc = new CarParkDocument(document);
                    break;
                case DocumentTypes.ADVANCE:
                    sDoc = new Advance(document);
                    break;
                case DocumentTypes.COLLECTION_INVOICE:
                    sDoc = new CollectionInvoice(document);
                    break;
                default:
                    sDoc = new Receipt(document);
                    break;
            }
            sDoc.TcknVkn = tcknVkn;

            return sDoc;
        }

        private static IState EnterCarPlate()
        {
            return States.EnterString.Instance(PosMessage.CAR_PLATE,  
                                                "34HGN1453", 
                                                new StateInstance<String>(SetCarPlate),
                                                new StateInstance(EnterCarPlate));
        }

        private static IState SetCarPlate(String plate)
        {
            if (String.IsNullOrEmpty(plate))
                return State = EnterCarPlate();

            Document.CustomerTitle = plate;
            return EnterParkingDate();
        }

        private static IState EnterParkingDate()
        {
            return States.EnterInteger.Instance(PosMessage.DATE,
                                                        int.Parse(String.Format("{0:ddMMyyyy}", DateTime.Now)),
                                                        new StateInstance<int>(SetParkingDate),
                                                        new StateInstance(EnterParkingDate));
        }

        private static DateTime parkingDate;
        private static IState SetParkingDate(int date)
        {
            string dateFormat = "ddMMyyyy";
            string strDate = date.ToString();

            if (date.ToString().Length == 7)
                strDate = strDate.Insert(0, "0");
            try
            {
                if (!DateTime.TryParseExact(strDate, dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parkingDate))
                    return EnterParkingDate();
            }
            catch
            { SoundManager.Sound(SoundType.FAILED); return EnterParkingDate(); }

            return EnterParkingTime();
        }

        private static IState EnterParkingTime()
        {
            return States.EnterInteger.Instance(PosMessage.TIME,
                                                        int.Parse(String.Format("{0:HHmm}", DateTime.Now)),
                                                        new StateInstance<int>(SetParkingTime),
                                                        new StateInstance(EnterParkingTime));
        }

        private static IState SetParkingTime(int time)
        {
            string strTime = time.ToString();
            if (strTime.Length == 3)
                strTime = strTime.Insert(0, "0");

            if (!DateTime.TryParseExact(strTime, "HHmm", System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out parkingDate))
                return EnterParkingTime();

            Document.IssueDate = parkingDate;
            return EnterReturnReason();
        }

        private static IState EnterCustomerName()
        {
            if(Document is CollectionInvoice)
                return States.EnterString.Instance(PosMessage.SUBSCRIBER_NO,
                                                new StateInstance<string>(SetCustomerName),
                                                new StateInstance(EnterCustomerName));
            else
                return States.EnterString.Instance(PosMessage.CUSTOMER_NAME_OR_TITLE,
                                                new StateInstance<string>(SetCustomerName),
                                                new StateInstance(EnterCustomerName));
        }

        private static IState SetCustomerName(string customerTitle)
        {
            if (String.IsNullOrEmpty(customerTitle))
                return State = EnterCustomerName();

            if(Document is CollectionInvoice && customerTitle != null && customerTitle.Length > 15)
            {
                return States.AlertCashier.Instance(new Confirm(PosMessage.MAX_LENGTH + ": 15",
                                                                new StateInstance(EnterCustomerName),
                                                                new StateInstance(EnterCustomerName)));
            }

            Document.CustomerTitle = customerTitle;

            if (Document is CollectionInvoice)
                return EnterCollectionAmount();
            else if (Document is CurrentAccountDocument)
                return EnterSlipSerialNumber();
            else if (Document is SelfEmployementInvoice)
                return EnterAddressInfo1();
            else
                return EnterReturnReason();
        }

        private static IState EnterAddressInfo1()
        {
            return States.EnterString.Instance("ADRES BÝLGÝSÝ 1",
                                                new StateInstance<string>(SetAddressInfo1),
                                                new StateInstance(EnterAddressInfo1));
        }

        private static IState SetAddressInfo1(string info)
        {
            Document.ReturnReason = info;

            return EnterAddressInfo2();
        }

        private static IState EnterAddressInfo2()
        {
            return States.EnterString.Instance("ADRES BÝLGÝSÝ 2",
                                                new StateInstance<string>(SetAddressInfo2),
                                                new StateInstance(EnterAddressInfo2));
        }

        private static IState SetAddressInfo2(string info)
        {
            Document.ReturnReason += "|" + info;

            return EnterTaxOffice();
        }

        private static IState EnterTaxOffice()
        {
            return States.EnterString.Instance("VERGÝ DAÝRESÝ",
                                                new StateInstance<string>(SetTaxOffice),
                                                new StateInstance(EnterTaxOffice));
        }

        private static IState SetTaxOffice(string taxOffice)
        {
            Document.ReturnReason += "|" + taxOffice;            

            return EnterReturnReason();
        }

        private static IState EnterCollectionAmount()
        {
            return States.EnterDecimal.Instance(PosMessage.COLLECTION_AMOUNT,
                                                        new StateInstance<decimal>(SetCollectionAmount),
                                                        new StateInstance(EnterCollectionAmount));
        }

        static decimal collectionAmount = 0.0m;
        private static IState SetCollectionAmount(decimal newVal)
        {
            collectionAmount = newVal;

            return EnterComissionAmount();
        }

        private static IState EnterComissionAmount()
        {
            return States.EnterDecimal.Instance(PosMessage.COMISSION_AMOUNT,
                                                        0.0m,
                                                        new StateInstance<decimal>(SetComissionAmount),
                                                        new StateInstance(EnterComissionAmount));
        }

        private static IState SetComissionAmount(decimal comission)
        {
            Document.ComissionAmount = comission;

            return SetSubTotal(collectionAmount);
        }
        
        private static IState EnterServiceDefinition()
        {
            return States.EnterString.Instance(PosMessage.SELF_EMP_INV_SERVICE_DEFINITION,
                                               new StateInstance<String>(SetServiceDefinition),
                                               new StateInstance(EnterServiceDefinition));
        }

        private static IState SetServiceDefinition(String definition)
        {
            if (definition == String.Empty)
                return State = EnterServiceDefinition();

            Document.ServiceDefinition = definition;
            return EnterServiceGrossWages();
        }

        private static IState EnterServiceGrossWages()
        {
            return States.EnterDecimal.Instance(PosMessage.SELF_EMP_INV_SERVICE_GROSS_WAGES,
                                               new StateInstance<decimal>(SetServiceGrossWages),
                                               new StateInstance(EnterServiceGrossWages));
        }

        private static IState SetServiceGrossWages(decimal grossWages)
        {


            Document.ServiceGrossWages = grossWages;
            return EnterServiceStoppageRate();
        }

        private static IState EnterServiceStoppageRate()
        {
            return States.EnterInteger.Instance(PosMessage.SELF_EMP_INV_SERVICE_STOPPAGE_RATE,
                                               new StateInstance<int>(SetServiceStoppageRate),
                                               new StateInstance(EnterServiceStoppageRate));
        }

        private static IState SetServiceStoppageRate(int stoppageRate)
        {


            Document.ServiceStoppageRate = stoppageRate;
            return EnterServiceVATRate();
        }

        private static IState EnterServiceVATRate()
        {
            return States.EnterInteger.Instance(PosMessage.SELF_EMP_INV_SERVICE_VAT_RATE,
                                               new StateInstance<int>(SetServiceVATRate),
                                               new StateInstance(EnterServiceVATRate));
        }

        private static IState SetServiceVATRate(int vatRate)
        {


            Document.ServiceVATRate = vatRate;
            return EnterServiceStoppageOtherRate();
        }

        private static IState EnterServiceStoppageOtherRate()
        {
            return States.EnterInteger.Instance(PosMessage.SELF_EMP_INV_SERVICE_STOPPAGE_OTHER_RATE,
                                               new StateInstance<int>(SetServiceStoppageOtherRate),
                                               new StateInstance(EnterServiceStoppageOtherRate));
        }

        private static IState SetServiceStoppageOtherRate(int stoppageOtherRate)
        {
            Document.ServiceStoppageOtherRate= stoppageOtherRate;

            DataConnector.OnDocumentUpdated(document, (int)DocumentStatus.Paying);
            DisplayAdapter.Customer.ChangeDocumentStatus(document, DisplayDocumentStatus.OnStart);

            Printer.PrintHeader(document);
            DisplayAdapter.Cashier.Show(PosMessage.WAITING_PAYMENT);
            
            return State = States.Selling.Instance();
        }

        private static IState EnterInvoiceSerialNumber()
        {
            return States.EnterString.Instance(PosMessage.INVOICE_SERIAL,
                                                new StateInstance<String>(SetInvoiceSerial),
                                                new StateInstance(EnterInvoiceSerialNumber));
        }

        private static IState SetInvoiceSerial(String serial)
        {
            if (serial == String.Empty)
                return State = EnterInvoiceSerialNumber();

            Document.SlipSerialNo = serial;
            return EnterSlipOrderNumber();
        }

        private static IState EnterIssueDate()
        {
            return States.EnterInteger.Instance(PosMessage.DATE,
                                                        int.Parse(String.Format("{0:ddMMyyyy}", DateTime.Now)),
                                                        new StateInstance<int>(SetInvoiceIssueDate),
                                                        new StateInstance(EnterIssueDate));
        }

        private static IState SetInvoiceIssueDate(int issueDate)
        {
            DateTime date;
            string strDate = issueDate.ToString();
            if (issueDate.ToString().Length == 7)
                strDate = strDate.Insert(0, "0");
            try
            {
                if(!DateTime.TryParseExact(strDate, "ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                    return State = EnterIssueDate();
            }
            catch { SoundManager.Sound(SoundType.FAILED); return State = EnterIssueDate(); }

            Document.IssueDate = date;

            if (Document is CollectionInvoice)
                return EnterInstutionName();
            else
                return EnterReturnReason();
        }

        private static IState EnterSlipSerialNumber()
        {
                return States.EnterString.Instance(PosMessage.SLIP_SERIAL,
                                                    new StateInstance<String>(SetSlipSerialNumber),
                                                    new StateInstance(EnterSlipSerialNumber));
        }

        private static IState EnterSlipOrderNumber()
        {
            return States.EnterString.Instance(PosMessage.SLIP_ORDER_NO,
                                                 new StateInstance<string>(SetSlipOrderNumber),
                                                 new StateInstance(EnterSlipOrderNumber));
        }

        private static IState EnterReturnReason()
        {
            if ((Document is ReturnDocument) &&
                (DataConnector.CurrentSettings.GetProgramOption(Setting.ReturnReason) == PosConfiguration.ON))
            {
                return States.EnterString.Instance(PosMessage.RETURN_REASON,
                                                     new StateInstance<string>(SetReturnReason),
                                                     new StateInstance(ContinueSlipSale));
            }
            else if(Document is Advance || Document is CurrentAccountDocument)
            {
                return States.EnterDecimal.Instance(PosMessage.SUBTOTAL,
                                                        new StateInstance<decimal>(SetSubTotal),
                                                        new StateInstance(ContinueSlipSale));
            }
            else
            {
                return ContinueSlipSale();
            }
        }

        private static IState SetSlipSerialNumber(string serialNo)
        {
            if (serialNo == String.Empty)
                return State = EnterSlipSerialNumber();

            Document.SlipSerialNo = serialNo;
            return State = EnterSlipOrderNumber();
        }

        private static IState SetSlipOrderNumber(string orderNo)
        {
            if (orderNo == String.Empty)
                return state = EnterSlipOrderNumber();

            Document.SlipOrderNo = orderNo;

            return EnterIssueDate();
        }

        private static IState EnterInstutionName()
        {
            return States.EnterString.Instance(PosMessage.INSTUTION_NAME,
                                                    new StateInstance<String>(SetInstutionName),
                                                    new StateInstance(EnterInstutionName));
        }

        private static IState SetInstutionName(String insName)
        {
            if (String.IsNullOrEmpty(insName))
                return EnterInstutionName();

            Document.ReturnReason = insName;

            return EnterCustomerName();
        }

        private static IState SetReturnReason(string reason)
        {
            Document.ReturnReason = reason;
            return ContinueSlipSale();
        }

        private static IState SetSubTotal(decimal subTotal)
        {
            document.TotalAmount = subTotal + document.ComissionAmount;

            DataConnector.OnDocumentUpdated(document, (int)DocumentStatus.Paying);

            DisplayAdapter.Customer.ChangeDocumentStatus(document, DisplayDocumentStatus.OnStart);

            DisplayAdapter.Cashier.Show(PosMessage.WAITING_PAYMENT);

            return State = States.Selling.Instance();
        }

        public static IState CancelSlipSale()
        {
            try
            {
                document.Void();
                document = new Receipt();
                return States.Start.Instance();
            }
            catch (Exception)
            {
                DisplayAdapter.Cashier.Show(PosMessage.PAPER_FOR_VOIDING_SLIP_SALE);
                System.Threading.Thread.Sleep(500);
                return CancelSlipSale();
            }
        }
        public static IState ContinueSlipSale()
        {
            try
            {
                if (Document is CarParkDocument)
                {
                    document.Close();
                    if (DocumentChanged != null)
                        DocumentChanged(document, new EventArgs());
                    return States.Start.Instance();
                }

                if (!document.IsEmpty)
                {
                    document.Print();
                    if (DocumentChanged != null)
                        DocumentChanged(document, new EventArgs());
                    return State;
                }
                else
                {
                    return States.Start.Instance();
                }
            }
            catch (PrinterException pe)
            {
                if (DocumentChanged != null)
                    DocumentChanged(document, new EventArgs());
                throw pe;
            }
            catch (EJException eje)
            {
                if (DocumentChanged != null)
                    DocumentChanged(document, new EventArgs());
                throw eje;
            }
            catch (SlipRowCountExceedException)
            {
                return States.ConfirmCashier.Instance(new Confirm(PosMessage.CONTINUE_OR_VOIDING_SLIP_SALE, ContinueSlipSale, CancelSlipSale));
            }
        }
        public static IState RecoverFromPowerFailure()
        {
            //TODO try catch lazim bi de bu fonksiyon sart mi
            document.Void();
            document = new Receipt();
            return States.Start.Instance();

        }

        public static void CheckDocumentAfterReConnected()
        {
            PrintedDocumentInfo pdi = Printer.GetLastDocumentInfo(false);

            if(pdi.DocId == Document.Id)
            {
                switch(pdi.Type)
                {
                    case ReceiptTypes.SALE:
                        Document.CloseWithoutPrint();
                        break;
                    case ReceiptTypes.VOID:
                        Document.Void();
                        break;
                }
            }
        }

        /// <summary>
        /// When CashRegister starts, it is good practice to compare receipt totals
        /// across cache and fiscalprinter. In case program was halted mid-sales there
        /// can be a discrepency in which case the document should be voided. 
        /// </summary>
        /// <returns> 
        /// The return type is IState so that this method can be used as a parameter to 
        /// PrinterConnectionError Instance(). This way even if CashRegister starts up in 
        /// connection error, this method can run after connection error is resolved.
        /// </returns>
        internal static IState VerifyReceiptTotal()
        {
            try
            {
                Printer.PrintTotals(document, false);
                if (document.TotalAmount > 0)
                    document.Void();
            }
            catch (DocumentTypeException dte)
            {

                if (document is Receipt)
                    document = new Invoice();
                Printer.AdjustPrinter(document);
                //here implement to reload invoice
                document.Id = Printer.CurrentDocumentId;
                while (true)
                {
                    try
                    {
                        document.Void();
                        document = new Receipt();
                        break;
                    }
                    catch {
                        SoundManager.Sound(SoundType.FAILED);
                        DisplayAdapter.Cashier.Show("BELGE IPTAL\nEDÝLEMEDÝ");
                        System.Threading.Thread.Sleep(500);
                    }
                }
                Log.Error(dte);
            }
            catch (SubtotalNotMatchException snmex)
            {
                SoundManager.Sound(SoundType.FAILED);
                FiscalItem fi = new SalesItem();
                fi.Product = DataConnector.CreateProduct("ELEKTRÝK KES", Department.Departments[0], snmex.Difference);
                document.Id = Printer.CurrentDocumentId;
                document.Items.Add(fi);
                document.Void();
                document = new Receipt();
                Log.Error(snmex);
            }
            catch (IncompletePaymentException ipe)
            {
                SoundManager.Sound(SoundType.FAILED);
                Error err = new Error(ipe);
                Log.Warning("Payment is not complete.", err.Message);
                Printer.AdjustPrinter(document);
                if (ipe.Difference == 0)
                {
                    document.Close();
                    document = new Receipt();
                }
                Log.Error(ipe);
            }
            catch (PowerFailureException pfe)
            {
                SoundManager.Sound(SoundType.FAILED);
                Printer.CheckPrinterStatus();
                Log.Error(pfe);
            }
            catch (InvalidProgramException ipe)
            {
                SoundManager.Sound(SoundType.FAILED);
                Log.Error(ipe);
                if (!document.IsEmpty)
                    document.Cancel();
                Printer.PrintRemark("ELEKTRIK KESILDI!");
                document.Void();
                Log.Error(ipe);
            }
            catch (CmdSequenceException cse)
            {
                SoundManager.Sound(SoundType.FAILED);
                if (cse.LastCommand==22 && (!document.IsEmpty))
                {
                    document.BalanceDue = 0;
                    document.Close();
                }
                else
                {
                    document = new Receipt();
                }
                Log.Error(cse);
            }
            catch (Exception e)
            {
                SoundManager.Sound(SoundType.FAILED);
                Log.Error(e);
            }
            return States.Start.Instance();
        }

        public static IPosClient PromoClient 
        {
            get 
            {
                return promoClient;
            }
        }
        public static AuthorizationLevel RegisterAuthorizationLevel 
        {
            get { return registerAuthorizationLevel > currentCashier.AuthorizationLevel ? registerAuthorizationLevel : currentCashier.AuthorizationLevel; }
            set { registerAuthorizationLevel = value; }
        }
 
        public static bool IsDesktopWindows
        {
            get { return Environment.OSVersion.Platform != PlatformID.WinCE; }
        }
    }

}
