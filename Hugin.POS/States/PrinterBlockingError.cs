using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{

    class PrinterBlockingError : BlockingState
    {
        private static IState state = new PrinterBlockingError();

        public static IState Instance()
        {
            return state;
        }
        public static IState Instance(Error promptMessage)
        {
            ReturnInstance = promptMessage.ReturnCancel;

            promptMessage.ReturnConfirm = Instance;

            if(promptMessage.Message != PosMessage.FPU_ERROR_MSG_145)
                DisplayAdapter.Cashier.Show(PosMessage.CALL_SERVICE);
            System.Threading.Thread.Sleep(2000);
            return States.AlertCashier.Instance(promptMessage);
        }

        protected override void TryRemoveBlock() {;}
        int attemptCount = 0;
        protected override Boolean BlockRemoved()
        {
            try
            {
                attemptCount++;
                cr.Printer.CheckPrinterStatus();
                return true;
            }
            catch (ServiceRequiredException sre)
            {
                Login.LogoutManager();
                //cr.State = ServiceMenu.Instance();
                States.AlertCashier.Instance(new Confirm(sre.Message));
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message == PosMessage.PRINTER_CONNETTION_ERROR || ex is System.Net.Sockets.SocketException ||
                    (attemptCount == 1 && ex is TimeoutException))
                {
                    DisplayAdapter.Cashier.Show(PosMessage.PLEASE_WAIT);
                    cr.SetPrinterPort(PosConfiguration.Get("PrinterComPort"));
                    Login.LogoutManager();
                    cr.State = States.Start.Instance();
                    attemptCount = 0;
                    return true;
                }

                return false;
            }

                        
        }
    }
}
