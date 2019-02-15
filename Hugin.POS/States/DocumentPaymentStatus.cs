using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS.States
{
    class DocumentPaymentStatus : State
    {
        static StateInstance ReturnConfirm;
        static StateInstance ReturnCancel;

        /// <summary>
        /// Indicating document is open or not.
        /// </summary>
        /// <param name="ConfirmState"></param>
        /// <param name="CancelState"></param>
        /// <returns></returns>
        internal static IState Instance(StateInstance ConfirmState, StateInstance CancelState)
        {
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;

            if (cr.Document.Customer != null && cr.DataConnector.CurrentSettings.GetProgramOption(Setting.AskDocumentState) == PosConfiguration.ON)
            {

                MenuList menuHeaders = new MenuList();

                menuHeaders.Add(new MenuLabel(String.Format(PosMessage.DOCUMENT_STATUS + "\n{0}", PosMessage.OPEN)));
                menuHeaders.Add(new MenuLabel(String.Format(PosMessage.DOCUMENT_STATUS + "\n{0}", PosMessage.CLOSED)));

                return States.ListCommandMenu.Instance(menuHeaders,
                                                    new ProcessSelectedItem<MenuLabel>(SelectDocumentMenuAction),
                                                    CancelState);
            }
            else
            {
                return CancelState();
            }
        }


        private static void SelectDocumentMenuAction(Object menu)
        {
            string message = ((MenuLabel)menu).ToString();
            message = message.Substring(message.IndexOf('\n') + 1);
            switch (message)
            {
                case PosMessage.OPEN:
                    cr.Document.IsOpenDocument = true;
                    break;
                case PosMessage.CLOSED:
                    cr.Document.IsOpenDocument = false;
                    break;
            }

            cr.State = ReturnConfirm();

        }

    }
}
