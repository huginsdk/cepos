using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;


namespace Hugin.POS
{
    public delegate void EJChangeHandler(String newseq);
}

namespace Hugin.POS.States
{

    class ElectronicJournalError : BlockingState
    {
        private static IState state = new ElectronicJournalError();
        private static EJException EJEXCEPTION;

        public static IState Instance()
        {
            return Instance(EJEXCEPTION);
        }
        private static void DisplayError(String msg)
        {
            Confirm c = new Confirm(msg);
            try
            {
                DisplayAdapter.Cashier.Show(c);
                System.Threading.Thread.Sleep(1000);
            }
            finally
            {
                DisplayAdapter.Cashier.ClearError();
            }
        }
        public static IState NewEJZRequired()
        {
            return state;
        }
        public static IState ValidEJRequired()
        {
            return state;
        }
        public static IState EscapeFormatJournal()
        {
            return state;
        }
        public static IState FormatJournal()
        {
            try
            {
                IPrinterResponse response = cr.Printer.InitEJ();
                cr.Printer.SetZLimit(-1);
            }
            catch (EJChangedException ejce)
            {
                cr.Printer.SetZLimit(-1);
                throw ejce;
            }
            catch (Exception)
            {
            }
            state.Escape();//cr.State.Escape();
            /*
            if (cr.CurrentCashier != null)
                cr.Printer.SignInCashier(cr.CurrentCashier);
            */
            return States.Start.Instance(); 
        }
        public static IState BlockFullMemory()
        {
            try
            {
                cr.Printer.CheckPrinterStatus();
            }
            catch(EJException eje) 
            {
                return Instance(eje);
            }
            if (cr.Printer.CurrentDocumentId > 0)
                Instance(EJEXCEPTION);
            else
                DisplayAdapter.Cashier.Show(PosMessage.EJ_PASIVE_ONLY_EJ_REPORTS);

            return state;
        }
        public static IState PrintZReport()
        {
            ICashier cashier = cr.CurrentCashier;
            try
            {
                cr.Printer.CheckPrinterStatus();
                ReportMenu.PrintZReport();
            }
            catch (EJException eje)
            {
                return ElectronicJournalError.Instance(eje);
            }
            catch (CashierAlreadyAssignedException)
            {
                States.Login.SignOutCashier();
                ReportMenu.PrintZReport();
            }

            return cr.State = States.Login.Instance();
        }
        public static IState Instance(Exception e)
        {
            if (!(e is EJException)) return Start.Instance();
            EJEXCEPTION = (EJException)e;

            if (!(cr.State is States.ElectronicJournalError))
            {
                oldState = cr.State;
            }

            if (e is EJFormatException) return Instance((EJFormatException)e);
            if (e is EJFiscalIdMismatchException) return Instance((EJFiscalIdMismatchException)e);
            if (e is EJIdMismatchException) return Instance((EJIdMismatchException)e);
            if (e is EJChangedException) return Instance((EJChangedException)e);
            if (e is EJCommException) return Instance((EJCommException)e);
            if (e is EJFullException) return Instance((EJFullException)e);
            if (e is EJLimitWarningException) return Instance((EJLimitWarningException)e);
            throw new Exception("BÝLÝNMEYEN \n EKÜ HATASI");
        }
        private static IState Instance(EJFormatException e)
        {
            EJEXCEPTION = (EJException)e;
            if (cr.Printer.CurrentDocumentId == 0)
                return ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_NEW_EJ_FORMAT,
                                                        new StateInstance(FormatJournal),
                                                        new StateInstance(EscapeFormatJournal)));
            else
                return AlertCashier.Instance(new Confirm(PosMessage.ZREPORT_NECCESSARY_FOR_NEW_EJ, NewEJZRequired));
        }
        private static IState Instance(EJFiscalIdMismatchException e)
        {
            EJEXCEPTION = (EJException)e;
            return AlertCashier.Instance(new Error(e, ValidEJRequired));
        }
        static IState oldState;
        private static IState Instance(EJIdMismatchException e)
        {
            EJEXCEPTION = (EJException)e;
            /*if ej is changed and it is not new
             * allow only ej reports menu
             * if system under power failuere error
             * allow nothing, because power failure cannot be cleaned at this mode
             */
            try
            {
                cr.Printer.CheckPrinterStatus();
            }
            catch (PowerFailureException pfe)
            {
                switch (pfe.LastCommand)
                {
                    case 40:
                    case 45:
                        DisplayError(PosMessage.EJ_PASIVE_VALID_EJ_REQUIRED);
                        return state;
                }
            }
            catch { }
            if (oldState is States.Selling || oldState is States.Payment || 
                (cr.Document != null && cr.Document.Items.Count > 0))
            {
                DisplayError(PosMessage.EJ_PASIVE_VALID_EJ_REQUIRED);
            }
            else
            {
                DisplayError(PosMessage.EJ_PASIVE_ONLY_EJ_REPORTS);
            }
            return state;

        }
        private static IState Instance(EJLimitWarningException e)
        {
            EJEXCEPTION = (EJException)e;
            /*if current address of ej is higer then ej limit */

            return AlertCashier.Instance(new Confirm(String.Format("{0}\n{1}", PosMessage.EJ_AVAILABLE_LINES, e.Usage)));
        }
        static String activeEJ = "";
        private static IState Instance(EJChangedException e)
        {
            EJEXCEPTION = (EJException)e;                
            /*if login fails then it will be caught case 1 else condition
             * if sucessful login means that ej is active ej
             */
            int id = int.Parse(e.EJId);
            DisplayError(e.Message + id);

            if (cr.Document != null && cr.Document.Items.Count > 0)
            {
                if (activeEJ == "" && int.Parse(e.EJId.Trim()) < int.Parse(e.PreviousEJ))
                {
                    activeEJ = e.PreviousEJ;
                    return Instance(new EJIdMismatchException());
                }
                if (activeEJ != "" && activeEJ != e.EJId)
                {
                    return Instance(new EJIdMismatchException());
                }

                activeEJ = "";
            }

            if (cr.CurrentCashier != null)
            {
                try
                {
                    Login.SignInCashier(cr.CurrentCashier);
                    try
                    {
                        cr.Printer.PrintSubTotal(cr.Document, false);
                    }
                    catch (CmdSequenceException)
                    {
                        cr.Document.Void();
                    }
                    return States.Start.Instance();
                }
                catch { }
            }
            state.Escape();
            return cr.State = States.Start.Instance();
        }
        private static IState Instance(EJCommException e)
        {
            EJEXCEPTION = (EJException)e;
            DisplayError(PosMessage.EJ_NOT_AVAILABLE);
            return state;
        }
        private static IState Instance(EJFullException e)
        {
            /* ElectronicJournal is full, 
                         * if there is an incomplete document, cancel it
                         * if latest document is not Z report, confirm cashier to report 
                         * allow only ej reports menu
                         */
            try
            {
                if (cr.Document != null && cr.Document.Items.Count > 0 && activeEJ != "")
                {
                    return Instance(new EJIdMismatchException());
                }
                DisplayAdapter.Cashier.LedOff(Leds.Sale);
                if (cr.Document.Id != 0)
                {
                    cr.Document.Void();
                    return States.ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_ZREPORT_ON_FULL_EJ,
                                        new StateInstance(PrintZReport),
                                        new StateInstance(Instance)));
                }
            }
            catch (EJLimitWarningException) { }
            catch (Exception) { }
            /* 
             * if any document after z report, confirm cashier to take z report
             * else allow only ej reports menu
             */
            if (cr.Printer.CurrentDocumentId > 0)
            {
                return States.ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_ZREPORT_ON_FULL_EJ,
                                                    new StateInstance(PrintZReport),
                                                    new StateInstance(Instance)));
            }
            else
                state = BlockFullMemory();

            return state;
        }

        public static IState Instance(Exception e, StateInstance RetInstance)
        {
            ReturnInstance = RetInstance;
            return Instance(e);
        }

        protected override Boolean BlockRemoved()
        {
            IPrinterResponse response;
            try
            {
                response = cr.Printer.CheckPrinterStatus();
                //if (Str.Contains(response.Detail, PosMessage.EJ_FULL))
                //{
                //    throw new EJFullException();
                //}
                DisplayAdapter.Cashier.Show(PosMessage.CONNECTING_TO_PRINTER);
                String port = PosConfiguration.Get("ReceiptPrinterComPort");
                cr.SetPrinterPort(port);
                response = cr.Printer.CheckPrinterStatus();
                //if (Str.Contains(response.Detail, PosMessage.EJ_FULL))
                //{
                //    throw new EJFullException();
                //}

                cr.Log.Success("Connected to printer. {0}", port);
                if (cr.Document.Items.Count > 0 && activeEJ != "")
                    throw EJEXCEPTION;
                return true;
            }
            catch (EJFullException)
            {
                throw new EJFullException();
            }
            catch (BlockingException)
            {
                cr.State = PrinterBlockingError.Instance();
                return false;
            }
            catch (EJException eje)
            {
                cr.State = Instance(eje);
            }
            catch (PowerFailureException pfe)
            {
                try
                {
                    Recover.RecoverPowerFailure(pfe);
                    return true;
                }
                catch (EJException eje)
                {
                    cr.State = Instance(eje);
                }
            }
            catch (PrinterException pe)
            {
                Error e = new Error(pe);
                DisplayError(e.Message);
                cr.Log.Error(e.Message);
            }
            catch (Exception e)
            {
                DisplayError(PosMessage.EJ_ERROR_OCCURED);
                cr.Log.Error(e.Message);
            }
            return false;

        }

        public override void Quantity()
        {
            cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_NEW_EJ_FORMAT,
                                                    new StateInstance(FormatJournal),
                                                    new StateInstance(Instance)));
        }
        public override void Correction()
        {
            Quantity();
        }
        public override void Command()
        {
            return;
        }
        public override void Program()
        {
            return;
        }
        public override void Report()
        {
            if (EJEXCEPTION is EJFormatException)
                return;
            if (EJEXCEPTION is EJFiscalIdMismatchException)
                return;
            if (!cr.Document.IsEmpty)
                return;
            try
            {
                //This is in try catch b/c Nullref exception occured 
                //around ej change time. Exact cause was undetermined
                cr.State = ReportMenu.EJOnly();
            }
            catch (NullReferenceException) { }
        }
    }
}
