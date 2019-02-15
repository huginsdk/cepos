using System;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class ListDocument : List
    {
        private static IState state = new ListDocument();
        private static new ProcessSelectedItem<SalesDocument> ProcessSelected;
        private static SalesDocument salesDoc;
        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<SalesDocument> psi)
        {
            List.Instance(ide);
            ProcessSelected = psi;
            return state;
        }

        public override void Document()
        {
            base.DownArrow();
        }

        public override void Enter()
        {
            MenuLabel label = (MenuLabel)ie.Current;
            salesDoc = label.Value as SalesDocument;
            if (CashRegister.Document.IsEmpty)
            {
                if (salesDoc is Receipt)
                {
                    ProcessSelected(salesDoc);
                    CashRegister.State = Start.Instance();
                }
                else
                {
                    ProcessSelected(salesDoc);
                }
            }
            else
            {
                String confirmationMessage = String.Format(PosMessage.CHANGE_DOCUMENT, salesDoc.Name);
                Confirm e = new Confirm(confirmationMessage,
                                    new StateInstance<Hashtable>(LDChangeConfirmed),
                                    new StateInstance(Start.Instance));
                e.Data["Document"] = salesDoc;
                CashRegister.State = ConfirmCashier.Instance(e);
            }
        }

        public static IState LDChangeConfirmed(Hashtable data)
        {

            if (!((SalesDocument)data["Document"] is Receipt) && CashRegister.Document is Receipt)
            {
                CashRegister.Document.Transfer();
                CashRegister.Document.Void();
            }
            DisplayAdapter.Cashier.Show(PosMessage.TRANSFER_STARTED_PLEASE_WAIT);
            if (ProcessSelected != null)
            {
                ProcessSelected((SalesDocument)data["Document"]);
            }
            else
            {
                SalesDocument doc = (SalesDocument)data["Document"];
                CashRegister.ChangeDocumentType(doc);
            }
            if (salesDoc is Receipt)
                return Start.Instance();
            return CashRegister.State;

        }
        public static SalesDocument SalesDoc 
        {
            get { return salesDoc; }
            set { salesDoc = value; }
        }
        public override void Escape()
        {
            CashRegister.State = States.Start.Instance();
        }

    }
}
