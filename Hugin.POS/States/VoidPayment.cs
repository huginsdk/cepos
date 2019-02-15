using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class VoidPayment : SilentState
    {
        private static IState state = new VoidPayment();
        private static String cashierMsg = PosMessage.VOID;

        public static IState Instance()
        {
            if(cr.Document.Payments.Count > 0)
            {
                DisplayAdapter.Cashier.Show("ÖDEME İPTALİ (GİRİŞ)\n YENİ ÖDEME (ÇIKIŞ)");
            }
            else
            {
                return States.Start.Instance();
            }

            return state;
        }

        public override void UpArrow()
        {
            MenuList pastPayments = new MenuList();
            pastPayments.AddRange(cr.Document.Payments);

            ProcessSelectedItem psi = new ProcessSelectedItem(cr.Document.VoidPayment);
            pastPayments.MoveLast();
            pastPayments.MovePrevious();
            cr.State = List.Instance(pastPayments, psi);
        }

        public override void DownArrow()
        {
            MenuList pastPayments = new MenuList();
            pastPayments.AddRange(cr.Document.Payments);
            cr.State = List.Instance(pastPayments, new ProcessSelectedItem(cr.Document.VoidPayment));
        }

        public override void SalesPerson()
        { 
            cr.Document.VoidSalesPerson();
            cr.State = Payment.Instance();
        }

        public override void Escape()
        {
            cr.State = States.Start.Instance();
        }

        public override void Enter()
        {
            MenuList pastPayments = new MenuList();
            pastPayments.AddRange(cr.Document.Payments);

            ProcessSelectedItem psi = new ProcessSelectedItem(cr.Document.VoidPayment);
            pastPayments.MoveLast();
            pastPayments.MovePrevious();
            cr.State = List.Instance(pastPayments, psi);
        }
    }
}
