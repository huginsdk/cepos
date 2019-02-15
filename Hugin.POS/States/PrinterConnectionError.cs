using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{

    class PrinterConnectionError : BlockingState
    {
        private static IState state = new PrinterConnectionError();
        static String errorMessage = "";

        public static IState Instance()
        {
            DisplayAdapter.Cashier.Show(errorMessage);
            return state;
        }

        public static IState Instance(Exception e)
        {
            errorMessage = e.Message;
            if (errorMessage.Length > 20)
            {
                string[] splitted = errorMessage.Split(' ');
                errorMessage = "";
                int i = 0;
                do
                {
                    errorMessage += splitted[i] + " ";
                    i++;
                }
                while (errorMessage.Length < 20);
                errorMessage += "\n";
                while (i < splitted.Length)
                {
                    errorMessage += splitted[i] + " ";
                    i++;
                }
            }
            DisplayAdapter.Cashier.Show(errorMessage);
            cr.Log.Error("Printer Connection error: {0}", e.Message);
            return state;
        }

        public static IState Instance(Exception e, StateInstance RetInstance)
        {
            ReturnInstance = RetInstance;
            return Instance(e);
        }

        protected override Boolean BlockRemoved()
        {
            try
            {
                DisplayAdapter.Cashier.Show(PosMessage.CONNECTING_TO_PRINTER);
                String port = PosConfiguration.Get("PrinterComPort");
                cr.SetPrinterPort(port);
                if (cr.State is PrinterConnectionError)
                    return false;
                cr.Log.Success("Connected to printer. {0}", port);

                cr.CheckDocumentAfterReConnected();

                return true;
            }
            catch (BlockingException)
            {
                cr.State = PrinterBlockingError.Instance();
                return false;
            }
            catch (PowerFailureException)
            {
                cr.Void();
            }
            catch (EJException ej)
            {
                cr.State = ElectronicJournalError.Instance(ej);
            }
            catch (Exception e)
            {
                errorMessage = new Error(e).Message;
                DisplayAdapter.Cashier.Show(errorMessage);
                cr.Log.Error("Error in PrinterConnectionError: {0}", errorMessage);
            }
            return false;
                        
        }

        public override void Command()
        {
            return;
        }

        public override void  Program()
        {
            cr.State = SetupMenu.Instance(new StateInstance(Instance));
        }
    }
}
