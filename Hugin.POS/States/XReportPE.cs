using System;
using System.Collections.Generic;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class XReportPE : BlockingState
    {
        static string errorMessage = "YAZICI HATASI";

        private static IState state = new XReportPE();

        public static IState Instance()
        {
            DisplayAdapter.Cashier.Show(errorMessage);
            return state;
        }
        public static IState Instance(IncompleteXReportException ex)
        {
            Error err = null;
            if (ex.InnerException != null)
            {
                err = new Error(ex.InnerException, DisplayMessage);
            }
            else
            {
                err = new Error(ex, DisplayMessage);
            }
            //DisplayAdapter.Cashier.Show(errorMessage);
            AlertCashier.Instance(err);
            return state;
        }
        private static IState DisplayMessage()
        {
            DisplayAdapter.Cashier.Show(PosMessage.INCOMPLETE_XREPORT);
            return state;
        }
        public override void  Enter()
        {
            BlockRemoved();
        }
        protected override bool BlockRemoved()
        {
            try
            {
                cr.Printer.CheckPrinterStatus();
                cr.Printer.PrintXReport(true);
                cr.State = States.Start.Instance();
                return true;
            }
            catch (CmdSequenceException)
            {
                return true;
            }
            catch (PowerFailureException pfe)
            {
                throw pfe;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
