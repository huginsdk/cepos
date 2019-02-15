using System;
using System.Collections.Generic;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class EJSummaryReportPE : BlockingState
    {
        static string errorMessage = "YAZICI HATASI";
        static int errorCount = 0;
        private static IState state = new EJSummaryReportPE();

        public static IState Instance()
        {
            DisplayAdapter.Cashier.Show(errorMessage);
            errorCount = 0;
            return state;
        }
        public static IState Instance(IncompleteEJSummaryReportException ex)
        {
            errorMessage = ex.Message;
            DisplayAdapter.Cashier.Show(errorMessage);
            errorCount = 0;
            return state;
        }

        protected override bool BlockRemoved()
        {
            bool retVal = false;
            try
            {
                //cr.Printer.CheckPrinterStatus();
                cr.Printer.PrintEJSummary();
                cr.State = Start.Instance();
                retVal = true;
            }
            catch (CmdSequenceException)
            {
                cr.State = Start.Instance();
            }
            catch (FramingException fe)
            {
                if (errorCount < 5)
                    Enter();
                else
                    cr.State = Instance(new IncompleteEJSummaryReportException("RAPOR TAMAMLANAMADI\nRAPORU SONLANDIR", fe));
            }
            catch (NoReceiptRollException nrre)
            {
                cr.State = Instance(new IncompleteEJSummaryReportException("RAPOR TAMAMLANAMADI\nRAPORU SONLANDIR", nrre));
            }
            catch (PrinterOfflineException pe)
            {
                cr.State = Instance(new IncompleteEJSummaryReportException("printer exception on xreportexception recover enter state", pe));
            }
            return retVal;
        }
    }
}
