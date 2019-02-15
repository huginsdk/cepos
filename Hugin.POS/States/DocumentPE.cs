using System;
using System.Collections.Generic;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class DocumentPE : BlockingState
    {
        static string errorMessage = PosMessage.DOCUMENT_NOT_PRINTED;

        private static IState state = new DocumentPE();

        public static IState Instance()
        {
            DisplayAdapter.Cashier.Show(errorMessage);
            return state;
        }
        public static IState Instance(PrintDocumentException pde)
        {
            Error err = null;
            //if (pde.InnerException != null)
            //{
            //    err = new Error(pde.InnerException, DisplayMessage);
            //}
            //else
            //{
            //    err = new Error(pde, DisplayMessage);
            //}
            err = new Error(pde, DisplayMessage);
            States.AlertCashier.Instance(err);
            
            return state;
        }

        private static IState DisplayMessage()
        {
            DisplayAdapter.Cashier.Show(PosMessage.DOCUMENT_NOT_PRINTED);
            return state;
        }

        public override void Enter()
        {
            BlockRemoved();
        }
        public override void Command()
        {
            MenuList menuHeaders = new MenuList();
            menuHeaders.Add(new MenuLabel(PosMessage.VOID_DOCUMENT));
            cr.State = List.Instance(menuHeaders,
                       new ProcessSelectedItem(SelectMenu),
                       new StateInstance(Instance));  
        }
        protected override bool BlockRemoved()
        {
            Exception exPrinter = null;

            try
            {
                cr.Printer.CheckPrinterStatus();
                if (cr.Printer.GetLastDocumentInfo(false).Type == ReceiptTypes.VOID)
                    cr.Document.Void();
                else
                    cr.Document.CloseWithoutPrint();

                return true;
            }
            catch (IncompletePaymentException ipe)
            {
                throw ipe;
            }
            catch (Exception ex)
            {
                exPrinter = ex;
            }


            if(exPrinter != null)
            {
                try
                {
                    if (exPrinter.Message == PosMessage.PRINTER_CONNETTION_ERROR || exPrinter is System.Net.Sockets.SocketException)
                    {
                        DisplayAdapter.Cashier.Show(PosMessage.PLEASE_WAIT);
                        cr.SetPrinterPort(PosConfiguration.Get("PrinterComPort"));

                        // After established new connection, re-login manager
                        Login.LogoutManager();

                        // Check if ECR voided or completed last document, if document is not closed on CEPOS already
                        cr.CheckDocumentAfterReConnected();

                        //cr.State = States.Start.Instance();
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        public void SelectMenu(object si)
        {
            switch (si.ToString())
            {
                case PosMessage.VOID_DOCUMENT:
                    try
                    {
                        cr.Document.Void();
                    }
                    catch (CmdSequenceException)
                    {
                        cr.RecoverFromPowerFailure();
                    }
                    cr.State = States.Start.Instance();
                    break;
            }
        } 
    }
}
