using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
	//internal delegate void ReturnInstance();

    class PrinterStatusError : BlockingState
    {
        private static IState state = new PrinterStatusError();
        private static Error printerError = null;

        public static IState Instance()
        {
            return state;
        }

        public static IState Instance(Error error)
        {
            ReturnInstance = error.ReturnCancel;
            error.ReturnConfirm = Instance;
            printerError = error;

            
            cr.State = States.AlertCashier.Instance(error);
            return state;
        }

        public static IState Instance(PrinterException pe)
        {
            Error err = null;
            if (pe.InnerException == null)
                err = new Error(pe);
            else
                err = new Error(pe.InnerException);

            printerError = err;
            err.ReturnConfirm = Instance;

            cr.State = States.AlertCashier.Instance(err);
            return state;
        }        

        protected override void TryRemoveBlock() { ;}

        protected override Boolean BlockRemoved()
        {
            try
            {
                cr.Printer.InterruptReport();

                IPrinterResponse response = cr.Printer.CheckPrinterStatus();
                //if (Str.Contains(response.Detail, PosMessage.EJ_FULL))
                //    throw new EJFullException();
                return true;
            }
            catch (EJFullException)
            {
                throw new EJFullException();
            }
            catch (CmdSequenceException)
            {
                return true;
            }
            catch (PowerFailureException pfe)
            {
                throw pfe;
            }
            catch (EJException ejex)
            {
                //    if (printerError != null && printerError.Message == PosMessage.NORECEIPTROLL)
                //        return true;
                throw ejex;
            }
            catch (NoReceiptRollException nrre)
            {
                Error err = new Error(nrre);
                if (err.Message != printerError.Message)
                    Instance(err);
                return false;
            }
            catch (FMFullException fmfe)
            {
                Error err = new Error(fmfe);
                if (err.Message != printerError.Message)
                    Instance(new Error(fmfe));
                return false;
            }
            catch (FMLimitWarningException fmlwe)
            {
                try
                {
                    cr.State = States.ReportMenu.PrintZReport();
                }
                catch (Exception) { }
                Error err = new Error(fmlwe);
                if (err.Message != printerError.Message)
                    cr.State = Instance(err);
                return false;
            }
            catch (ClearRequiredException)
            {
                cr.Printer.InterruptReport();
                return true;
            }
            catch (PrinterException pe)
            {
                Instance(pe);
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public override void Command() { }
   
    }
}
