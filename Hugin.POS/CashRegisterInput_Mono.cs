using System;
using System.IO;
using System.Threading;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using Hugin.POS.States;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Hugin.POS
{
	[Activity (Label = "HUGİN POS", MainLauncher = true)]
    public class CashRegisterInput:Activity, IMainForm_Mono
    {
        internal static DateTime lastKeyPressed = new DateTime(1970, 1, 1);
        internal static event OnMessageHandler BarcodeReaded;

        private static Object serialLock = new Object();
        private static bool isProcessing = false;

		public bool Terminate = false;

		private void OnBarcode (String barcode)
		{
			if (BarcodeReaded != null) {
				BarcodeReaded (this, new OnMessageEventArgs (barcode));
			}
		}
		private void ShowError (String message)
		{
			bool dialogResult=false; 
				
			AlertDialog.Builder builder = new AlertDialog.Builder (this); 
			builder.SetTitle (Android.Resource.String.DialogAlertTitle); 
			builder.SetMessage (message); 
			builder.SetPositiveButton ("OK",(sender,e)=>{ 
				dialogResult=true; 
			}); 
			builder.SetNegativeButton ("NO",(sender,e)=>{ 
				dialogResult=false; 
			}); 
			
			builder.Show(); 
						
			this.Dispose(); 
	
		}
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Display.Display.Instance(this);
			Chassis.Register (this);
			Chassis.FatalErrorOccured+= new FatalEventHandler(ShowError);
			ChangeDisplay();
		}
		internal void ChangeDisplay ()
		{
			try {
				StartActivity (typeof(Display.Display));
			} catch {
				ShowError("Hata oluştu:\nDISPLAY YÜKLENEMEDİ");
			}
		}
        internal static void ConnectBarcode()
        {
            try
            {
            }
            catch { } //log not ready

        }

        internal void SendErrorMessage(int errorCode)
        {
          
        }        
		public void PreProcess_Mono ()
		{
			try {
				this.PreProcess ();
			} catch (TypeInitializationException tie) {
				
				Log(tie);
				ShowError("Hata oluştu:\nCONFIG DOSYASI BOZUK VEYA HATALI");
			}
			catch(Exception ex)
			{
				ShowError("Hata oluştu:\n"+ex.Message);
			}

		}
        protected void PreProcess()
        {
            try
            {
                lastKeyPressed = DateTime.Now;
                KeyMap.Load();
                Debugger.Instance().AppendLine("Started Display: " + DateTime.Now.ToLongTimeString());

                DisplayAdapter.Instance();
                DisplayAdapter.Both.Show(PosMessage.PLEASE_WAIT);
                
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
                thread.Priority = System.Threading.ThreadPriority.BelowNormal;
                thread.Start();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
        }
        static void Log (Exception ex)
		{
			//this function is added for only warnings
			//try-catch has exception definitions
			//but they are not used
			// so then call this function
		}
        void Printer_DateTimeChanged(object sender, EventArgs e)
        {
            lastKeyPressed = lastKeyPressed.Add((DateTime)sender - DateTime.Now);
        }

        public void Process(PosKey key)
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
                            //MessageBox.Show(cr.State.GetType().ToString());
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
                        default:
                            switch (key)
                            {
                                case PosKey.MagstripeStx: cr.State.Alpha('\"'); break;
                                case (PosKey)214: cr.State.Alpha('Ö'); break;
                                case (PosKey)286: cr.State.Alpha('Ð'); break;
                                case (PosKey)199: cr.State.Alpha('Ç'); break;
                                case (PosKey)220: cr.State.Alpha('Ü'); break;
                                case (PosKey)304: cr.State.Alpha('Ý'); break;
                                case (PosKey)350: cr.State.Alpha('Þ'); break;
                                case (PosKey)46: cr.State.Alpha('.'); break;
                                case (PosKey)47: cr.State.Alpha('/'); break;
                                default:
                                    if (char.IsLetter((char)key) || key == (PosKey)ConsoleKey.Spacebar)
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
                    cr.State = States.AlertCashier.Instance(new Error(csex));
                    cr.Log.Error("CmdSequenceException occured. Last command: {0}", csex.LastCommand);
                    cr.Log.Error(csex);
                    //to do : cr.State = ex.Recover();
                }
                catch (PowerFailureException pfex)
                {
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
                    catch (EJException eje)
                    {
                        cr.State = States.ElectronicJournalError.Instance(eje);
                    }
                    cr.Log.Warning(ex);
                }
                catch (EJException eje)
                {
                    cr.State = States.ElectronicJournalError.Instance(eje);
                    cr.Log.Warning(eje);
                }
                catch (SVCPasswordOrPointException ex)
                {
                    cr.State = States.ServiceMenu.Instance();
                    cr.Log.Warning(ex);
                }
                catch (FiscalIdException fie)
                {
                    cr.State = States.FiscalIdBlock.Instance();
                    cr.Log.Warning(fie);
                }
                catch (BlockingException bex)
                {
                    cr.State = States.PrinterBlockingError.Instance(new Error(bex));
                }
                catch (NoReceiptRollException nrre)
                {
                    cr.State = States.PrinterStatusError.Instance(new Error(nrre));
                    cr.Log.Error(nrre);
                }
                catch (PrinterStatusException pse)
                {
                    cr.State = States.PrinterStatusError.Instance(pse);
                    cr.Log.Error(pse);
                }
                catch (MissingCashierException mcex)
                {
                    cr.CurrentCashier = null;
                    cr.State = States.AlertCashier.Instance(new Error(mcex, States.Login.Instance));
                    cr.Log.Warning(mcex);
                }
                catch (AssignedCashierLimitExeedException aclee)
                {
                    cr.CurrentCashier = null;
                    cr.State = States.AlertCashier.Instance(new Error(aclee, States.Login.Instance));
                    cr.Log.Warning(aclee);
                }
                catch (SlipRowCountExceedException srceex)
                {
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
                    cr.State = States.AlertCashier.Instance(new Error(pnwex));
                }
                catch (DirectoryNotFoundException dnfex)
                {
                    cr.Log.Fatal(dnfex);
                    cr.State = States.AlertCashier.Instance(new Error(dnfex));
                }
                catch (InvalidOperationException ioex)
                {
                    cr.State = States.AlertCashier.Instance(new Error(ioex));
                }
                catch (NegativeResultException nrex)
                {
                    cr.State = States.AlertCashier.Instance(new Error(nrex));
                    cr.Log.Error(nrex);
                }
                catch (IncompleteXReportException ixrex)
                {
                    cr.State = States.XReportPE.Instance(ixrex);
                    cr.Log.Warning("Printer exception occured during the x report", ixrex.Message);
                }
                catch (IncompleteEJSummaryReportException iejsex)
                {
                    cr.State = States.EJSummaryReportPE.Instance(iejsex);
                    cr.Log.Warning("Printer exception occured during the ej summary report", iejsex.Message);
                }
                catch (IncompletePaymentException ipe)
                {
                    cr.State = States.PaymentAfterPE.Instance(ipe);
                    cr.Log.Warning(ipe.Message);
                }
                catch (PrintDocumentException pde)
                {
                    cr.State = States.DocumentPE.Instance(pde);
                    cr.Log.Warning("Printer exception occured during the document printing ", pde.Message);
                }
                catch (FMFullException fmfe)
                {
                    cr.State = States.PrinterStatusError.Instance(new Error(fmfe));
                    cr.Log.Fatal(fmfe);
                }
                catch (FMLimitWarningException fmlwe)
                {
                    cr.State = States.PrinterStatusError.Instance(new Error(fmlwe));
                    cr.Log.Fatal(fmlwe);
                }

				catch (ZRequiredException zre)
				{
					cr.State = States.PrinterStatusError.Instance(new Error(zre));
					cr.Log.Error(zre);
				}
				catch (CashierAutorizeException cae)
				{
					cr.State = States.PrinterStatusError.Instance(new Error(cae));
					cr.Log.Error(cae);
				}
                catch (Exception ex)
                {
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


        protected void PostProcess()
        {
        }

    }
}
