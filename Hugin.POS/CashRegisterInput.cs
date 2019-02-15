using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using Hugin.POS.States;

namespace Hugin.POS
{
    public class CashRegisterInput : MainForm
    {
        internal static DateTime lastKeyPressed = new DateTime(1970, 1, 1);
        internal static event OnMessageHandler BarcodeReaded;

        private static SerialPort barcodeComPort;
        private static Object serialLock = new Object();
        private static bool isProcessing = false;

        public CashRegisterInput()
            : base()
        {
           
            this.PreProcess();
        }

        internal static void ConnectBarcode()
        {
            try
            {
                String newLine = PosConfiguration.BarcodeTerminator;
                if (newLine == "") newLine = "\r\n";
                String barcodeComPortName = PosConfiguration.Get("BarcodeComPort");
                barcodeComPort = new SerialPort(barcodeComPortName);
                barcodeComPort.NewLine = newLine;
                barcodeComPort.ReadTimeout = 3000;
                if (!barcodeComPort.IsOpen)
                    barcodeComPort.Open();
                barcodeComPort.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);
            }
            catch { } //log not ready

        }

        internal override void SendErrorMessage(int errorCode)
        {
            if (barcodeComPort != null && barcodeComPort.IsOpen)
                barcodeComPort.Write(errorCode + "");
        }

        private static void serial_DataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            lock (serialLock)
            {
                SerialPort sp = (SerialPort)sender;
                try
                {
                    String barcode = "";
                    
                    // Barcode terminator may be set
                    if (BarcodeReaded !=null)
                    {
                        System.Threading.Thread.Sleep(100);
                        barcode = sp.ReadExisting();
                        BarcodeReaded(null, new OnMessageEventArgs(barcode));
                        Chassis.Engine.Process(PosKey.Enter);
                        return;
                    }
                    else
                    {
                        if (PosConfiguration.BarcodeTerminator == "," && !(cr.IsDesktopWindows))
                        {
                            while (sp.BytesToRead > 0)
                            {
                                barcode += sp.ReadExisting();
                                System.Threading.Thread.Sleep(100);
                            }
                            
                            cr.State = States.PaymentOnBarcode.Instance(barcode);
                            Chassis.Engine.Process(PosKey.Enter);
                            return;
                        }
                        else
                        {
                            barcode = sp.ReadTo(PosConfiguration.BarcodeTerminator);
                        } 
                        
                        if (sp.BytesToRead > 0)
                            cr.Log.Info("Existing serial data on barcode port :" + sp.ReadExisting());
                    }

                    DisplayAdapter.Cashier.Pause();
                    //If barcode reads from receipt
                    if (barcode.StartsWith("I"))
                    {
                        Chassis.Engine.Process(PosKey.BarcodePrefix);
                        barcode = barcode.Substring(1);
                    }
#if WindowsCE
                    else if (barcode.StartsWith("99"))
                    {
                        Chassis.Engine.Process(PosKey.BarcodePrefix);
                        barcode = barcode.Substring(2);
                    }

                    if (cr.State is States.EnterNumber)
                        Chassis.Engine.Process(PosKey.Quantity);

#endif

                    long numericCheck = 0;
                    if (Parser.TryLong(barcode, out numericCheck))
                    {
                        foreach (char c in barcode)
                            Chassis.Engine.Process((PosKey)c);
                    }
                    else //alphanumeric barcode
                        cr.State = States.EnterString.Instance(PosMessage.ENTER_BARCODE,
                                                            barcode,
                                                            new StateInstance<string>(States.EnterString.SellAlphanumericBarcode), null);
                }
                catch (TimeoutException)
                {
                    cr.Log.Error("Timeout exception on serial barcode. Existing data: " + sp.ReadExisting());
                }
                catch (Exception ex)
                {
                    cr.Log.Error(ex);
                    //throw ex;
                }
                finally
                {
                    DisplayAdapter.Cashier.Play();
                }
                Chassis.Engine.Process(PosKey.Enter);
            }
        }

        protected override void PreProcess()
        {
            try
            {
                lastKeyPressed = DateTime.Now;
                KeyMap.Load();
                Debugger.Instance().AppendLine("Started Display: " + DateTime.Now.ToLongTimeString());

                DisplayAdapter.Instance();
                DisplayAdapter.Both.Show(PosMessage.PLEASE_WAIT);
                Application.DoEvents();
                Debugger.Instance().AppendLine("Finished Display" + DateTime.Now.ToLongTimeString());

                if (PosConfiguration.Get("BarcodeComPort") != "")
                {
                    ConnectBarcode();
                }

                //Console.CancelKeyPress += new ConsoleCancelEventHandler(pos_OnClosed);

                CashRegister.Instance();
                CashRegister.Printer.DateTimeChanged += new EventHandler(Printer_DateTimeChanged);

                Debugger.Instance().AppendLine("Started BackgroundWorker: " + DateTime.Now.ToLongTimeString());
                Thread thread = new Thread(new ThreadStart(BackgroundWorker.Start));
                thread.Name = "BackgroundWorker";
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.BelowNormal;
                thread.Start();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
        }
        
        void Printer_DateTimeChanged(object sender, EventArgs e)
        {
            lastKeyPressed = lastKeyPressed.Add((DateTime)sender - DateTime.Now);
        }

        public override void Process(PosKey key)
        {
            if (isProcessing) return; //occurs if pressed key when seral data is executing.

            lock (serialLock)
            {
                lastKeyPressed = DateTime.Now;
                isProcessing = true;

                #region parse user input
                try
                {

                    switch (key)
                    {
                        case PosKey.D0:
                        case PosKey.D1:
                        case PosKey.D2:
                        case PosKey.D3:
                        case PosKey.D4:
                        case PosKey.D5:
                        case PosKey.D6:
                        case PosKey.D7:
                        case PosKey.D8:
                        case PosKey.D9:
                            cr.State.Numeric((char)key);
                            break;
                        case PosKey.DoubleZero:
                            cr.State.Numeric((char)PosKey.D0);
                            cr.State.Numeric((char)PosKey.D0);
                            break;
                        case PosKey.Decimal:
                            cr.State.Seperator();
                            break;
                        case PosKey.Document:
                            cr.State.Document();
                            break;
                        case PosKey.Customer:
                            cr.State.Customer();
                            break;
                        case PosKey.Report:
                            cr.State.Report();
                            break;
                        case PosKey.Program:
                            cr.State.Program();
                            break;
                        case PosKey.Command:
                            ISalesDocument doc = cr.Document;
                            cr.State.Command();
                            break;
                        case PosKey.CashDrawer:
                            cr.State.CashDrawer();
                            break;
                        case PosKey.Void:
                            cr.State.Void();
                            break;
                        case PosKey.PercentDiscount:
                            cr.State.Adjust(AdjustmentType.PercentDiscount);
                            break;
                        case PosKey.Discount:
                            cr.State.Adjust(AdjustmentType.Discount);
                            break;
                        case PosKey.PercentFee:
                            cr.State.Adjust(AdjustmentType.PercentFee);
                            break;
                        case PosKey.Fee:
                            cr.State.Adjust(AdjustmentType.Fee);
                            break;
                        case PosKey.ReceiveOnAcct:
                            cr.State.ReceiveOnAcct();
                            break;
                        case PosKey.PayOut:
                            cr.State.PayOut();
                            break;
                        case PosKey.PriceLookup:
                            cr.State.PriceLookup();
                            break;
                        case PosKey.Price:
                            cr.State.Price();
                            break;
                        case PosKey.Total:
                            cr.State.TotalAmount();
                            break;
                        case PosKey.Repeat:
                            cr.State.Repeat();
                            break;
                        case PosKey.UpArrow:
                            cr.State.UpArrow();
                            break;
                        case PosKey.DownArrow:
                            cr.State.DownArrow();
                            break;
                        case PosKey.Escape:
                            cr.State.Escape();
                            break;
                        case PosKey.Quantity:
                            cr.State.Quantity();
                            break;
                        case PosKey.Cash:
                            cr.State.Pay(new CashPaymentInfo());
                            break;
                        case PosKey.Credit:
                            if (KeyMap.CreditBuffer == -1)
                            {
                                Thread.Sleep(20);//wait some for Console.KeyAvailable
                                if (Console.In.Peek() > -1)
                                    KeyMap.CreditBuffer = Console.In.Read() - 48;
                                else
                                {
                                    cr.State.Alpha('C');
                                    return;
                                }
                            }
                            try
                            {
                                if (KeyMap.CreditBuffer == 0)
                                    cr.State.Pay(new CreditPaymentInfo());
                                else
                                {
                                    Dictionary<int, ICredit> credits = cr.DataConnector.GetCredits();
                                    if (credits.Count > (KeyMap.CreditBuffer - 1))//?-1
                                        cr.State.Pay(new CreditPaymentInfo(credits[KeyMap.CreditBuffer]));
                                }
                            }
                            finally { KeyMap.CreditBuffer = -1; }
                            break;
                        case PosKey.Payment:
                            cr.State.ShowPaymentList();
                            break;
                        case PosKey.Check:
                            cr.State.Pay(new CheckPaymentInfo());
                            break;
                        case PosKey.ForeignCurrency:
                            cr.State.Pay(new CurrencyPaymentInfo());
                            break;
                        case PosKey.SubTotal:
                            cr.State.SubTotal();
                            break;
                        case PosKey.Enter:
                            cr.State.Enter();
                            break;
                        case PosKey.SalesPerson:
                            cr.State.SalesPerson();
                            break;
                        case PosKey.Correction:
                            cr.State.Correction();
                            break;
                        case PosKey.LabelStx:
                            if (KeyMap.LabelBuffer == -1)
                            {
                                Thread.Sleep(20);//wait some for Console.KeyAvailable
                                //Console.In.ReadLine() is changed as Console.In.Read() 
                                //because Readline command opens the text editor in WindowsCE
                                int label = Console.In.Read();
                                if (label == -1)
                                {
                                    cr.State.Alpha('L');
                                    break;
                                }
                                KeyMap.LabelBuffer = label;
                            }
                            try
                            {
                                cr.State.LabelKey(KeyMap.LabelBuffer);
                            }
                            finally { KeyMap.LabelBuffer = -1; }
                            break;
                        case PosKey.Help:
                            MessageBox.Show(cr.State.GetType().ToString());
                            break;
                        case PosKey.MagstripeStx:
                            cr.State.CardPrefix();
                            break;
                        case PosKey.KeyStx:
                            if (KeyMap.KeyLockBuffer == -1)
                            {
                                Thread.Sleep(20);//wait some for Console.KeyAvailable
                                String label = Console.ReadLine();
                                KeyMap.KeyLockBuffer = int.Parse(label);
                            }
                            try
                            {
                                cr.State.End(KeyMap.KeyLockBuffer - 1);
                            }
                            finally
                            {
                                KeyMap.KeyLockBuffer = -1;
                            }
                            break;
                        case PosKey.BarcodePrefix:
                            cr.State.BarcodePrefix();
                            break;
                        case PosKey.UndefinedKey:
                            //do nothing
                            break;
                        case PosKey.SendOrder:
                            cr.State.SendOrder();
                            break;

                        default:
                            switch (key)
                            {
                                case (PosKey)17: cr.State.Alpha('|'); break;
                                case PosKey.MagstripeStx: cr.State.Alpha('\"'); break;
                                case (PosKey)214: cr.State.Alpha('Ö'); break;
                                case (PosKey)286: cr.State.Alpha('Ð'); break;
                                case (PosKey)199: cr.State.Alpha('Ç'); break;
                                case (PosKey)220: cr.State.Alpha('Ü'); break;
                                case (PosKey)304: cr.State.Alpha('Ý'); break;
                                case (PosKey)350: cr.State.Alpha('Þ'); break;
                                case (PosKey)221: break;
                                case (PosKey)46: cr.State.Alpha('.'); break;
                                case (PosKey)47: cr.State.Alpha('/'); break;
                                default:
                                    if (char.IsLetter((char)key) || key == (PosKey)ConsoleKey.Spacebar || char.IsPunctuation((char)key))
                                        cr.State.Alpha((char)key);
                                    else cr.State.UndefinedKey();
                                    break;
                            }
                            break;
                    }
                }
                #endregion

                #region handle errors

                catch (CmdSequenceException csex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.AlertCashier.Instance(new Error(csex));
                    cr.Log.Error("CmdSequenceException occured. Last command: {0}", csex.LastCommand);
                    cr.Log.Error(csex);
                    //to do : cr.State = ex.Recover();
                }
                catch (PowerFailureException pfex)
                {
                    SoundManager.Sound(SoundType.FATAL_ERROR);
                    try
                    {
                        Recover.RecoverPowerFailure(pfex);
                    }
                    catch (EJException eje)
                    {
                        cr.State = States.ElectronicJournalError.Instance(eje);
                    }
                    cr.Log.Warning(pfex);
                }
                catch (UnfixedSlipException ex)
                {
                    try
                    {
                        cr.State = States.BlockOnPaper.Instance();
                        //Recover.RecoverUnfixedSlip(ex);
                    }
                    catch (EJException)
                    {
                        cr.State = States.ElectronicJournalError.Instance();
                    }
                    cr.Log.Warning(ex);
                }
                catch (EJException eje)
                {
                    SoundManager.Sound(SoundType.FATAL_ERROR);
                    cr.State = States.ElectronicJournalError.Instance(eje);
                    cr.Log.Warning(eje);
                }
                catch (SVCPasswordOrPointException ex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    //cr.State = States.ServiceMenu.Instance();
                    States.AlertCashier.Instance(new Confirm(ex.Message));
                    cr.Log.Warning(ex);
                }
                catch (FiscalIdException fie)
                {
                    SoundManager.Sound(SoundType.FATAL_ERROR);
                    cr.State = States.FiscalIdBlock.Instance();
                    cr.Log.Warning(fie);
                }
                catch (BlockingException bex)
                {
                    SoundManager.Sound(SoundType.FATAL_ERROR);
                    cr.State = States.PrinterBlockingError.Instance(new Error(bex));
                }
                catch (NoReceiptRollException nrre)
                {
                    SoundManager.Sound(SoundType.NEED_PROCESS);
                    cr.State = States.PrinterStatusError.Instance(new Error(nrre));
                    cr.Log.Error(nrre);
                }
                catch (PrinterStatusException pse)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.PrinterStatusError.Instance(pse);
                    cr.Log.Error(pse);
                }
                catch (MissingCashierException mcex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.CurrentCashier = null;
                    cr.State = States.AlertCashier.Instance(new Error(mcex, States.Login.Instance));
                    cr.Log.Warning(mcex);
                }
                catch (AssignedCashierLimitExeedException aclee)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.CurrentCashier = null;
                    cr.State = States.AlertCashier.Instance(new Error(aclee, States.Login.Instance));
                    cr.Log.Warning(aclee);
                }
                catch (SlipRowCountExceedException srceex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    States.AlertCashier.Instance(new Error(srceex));
                    cr.State = States.ConfirmSlip.Instance(new Error(srceex));
                    cr.Log.Warning(srceex);
                }
                catch (CashierAlreadyAssignedException caaex)
                {
                    States.AlertCashier.Instance(new Error(caaex));
                    ICashier assignedCashier = cr.DataConnector.FindCashierById(caaex.CashierId);
                    if (assignedCashier != null)
                        cr.Log.Debug("Kasiyer zaten atanmis: {1} ({0})", assignedCashier.Id, assignedCashier.Name);
                    else
                        cr.Log.Error("Kasiyer girisi yapilmali");
                    States.Login.LoginCashier();
                }
                catch (ProductNotWeighableException pnwex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.AlertCashier.Instance(new Error(pnwex));
                }
                catch (DirectoryNotFoundException dnfex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.Log.Fatal(dnfex);
                    cr.State = States.AlertCashier.Instance(new Error(dnfex));
                }
                catch (InvalidOperationException ioex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.AlertCashier.Instance(new Error(ioex));
                }
                catch (NegativeResultException nrex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.AlertCashier.Instance(new Error(nrex));
                    cr.Log.Error(nrex);
                }
                catch (IncompleteXReportException ixrex)
                {
                    SoundManager.Sound(SoundType.FATAL_ERROR);
                    cr.State = States.XReportPE.Instance(ixrex);
                    cr.Log.Warning("Printer exception occured during the x report", ixrex.Message);
                }
                catch (IncompleteEJSummaryReportException iejsex)
                {
                    SoundManager.Sound(SoundType.FATAL_ERROR);
                    cr.State = States.EJSummaryReportPE.Instance(iejsex);
                    cr.Log.Warning("Printer exception occured during the ej summary report", iejsex.Message);
                }
                catch (IncompletePaymentException ipe)
                {
                    SoundManager.Sound(SoundType.NEED_PROCESS);
                    cr.State = States.PaymentAfterPE.Instance(ipe);
                    cr.Log.Warning(ipe.Message);
                }
                catch (PrintDocumentException pde)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.DocumentPE.Instance(pde);
                    cr.Log.Warning("Printer exception occured during the document printing ", pde.Message);
                }
                catch (FMFullException fmfe)
                {
                    SoundManager.Sound(SoundType.FATAL_ERROR);
                    cr.State = States.PrinterStatusError.Instance(new Error(fmfe));
                    cr.Log.Fatal(fmfe);
                }
                catch (FMLimitWarningException fmlwe)
                {
                    SoundManager.Sound(SoundType.FATAL_ERROR);
                    cr.State = States.PrinterStatusError.Instance(new Error(fmlwe));
                    cr.Log.Fatal(fmlwe);
                }
                catch (ZRequiredException zre)
                {
                    SoundManager.Sound(SoundType.NEED_PROCESS);
                    cr.State = States.PrinterStatusError.Instance(new Error(zre));
                    cr.Log.Error(zre);
                }
                catch (CashierAutorizeException cae)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.AlertCashier.Instance(new Error(cae));
                    cr.Log.Error(cae);
                }
                catch (FMNewException fmne)
                {
                    SoundManager.Sound(SoundType.NEED_PROCESS);
                    int fiscalId = int.Parse(PosConfiguration.Get("FiscalId").Substring(2, 8));
                    cr.State = States.EnterInteger.Instance(PosMessage.START_FM, fiscalId,
                                                            new StateInstance<int>(States.Login.AcceptFiscalId),
                                                            new StateInstance(Start.Instance));
                    cr.Log.Error(fmne);
                }
                catch (PrinterTimeoutException pte)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.PrinterConnectionError.Instance(pte);
                    cr.Log.Warning(pte);
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.PrinterConnectionError.Instance(new PrinterException(PosMessage.CANNOT_ACCESS_PRINTER, ex));
                    cr.Log.Warning(new PrinterException(PosMessage.CANNOT_ACCESS_PRINTER, ex));
                }
                catch (InvalidPaymentException ipe)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    Confirm con = new Confirm(String.Format("{0}\n{1}", PosMessage.PAYMENT_INVALID, "ÖDEME ÝPTAL?(GÝRÝÞ)"),
                                                new StateInstance(VoidPayment.Instance));
                    cr.State = States.ConfirmCashier.Instance(con);
                    cr.Log.Error(ipe);
                }
                catch (OperationCanceledException oce)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.PrinterConnectionError.Instance(new PrinterException(PosMessage.CANNOT_ACCESS_PRINTER, oce));
                    cr.Log.Warning(new PrinterException(PosMessage.CANNOT_ACCESS_PRINTER, oce));
                }
                catch(EftPosException epe)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = AlertCashier.Instance(new Error(epe));
                    cr.Log.Error(epe);
                }
                catch (Exception ex)
                {
                    SoundManager.Sound(SoundType.FAILED);
                    cr.State = States.AlertCashier.Instance(new Error(ex));
                    cr.Log.Error(ex);
                }
                finally
                {
                    isProcessing = false;
                }
                #endregion
            }

        }


        protected override void PostProcess()
        {
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CashRegisterInput
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(143, 30);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CashRegisterInput";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CashRegisterInput_FormClosing);
            this.ResumeLayout(false);

        }

        private void CashRegisterInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            cr.Printer.CloseConnection();
        }
    }
}
