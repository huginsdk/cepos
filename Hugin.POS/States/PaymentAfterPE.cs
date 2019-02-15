using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS.States
{
    class PaymentAfterPE : Payment
    {
        private static IState state = new PaymentAfterPE();

        public override Error NotImplemented
        {
            get
            {
                finalizeSaleErrorStr = PosMessage.PAYMENT_STARTED;

                return new Error(new Exception(finalizeSaleErrorStr),
                                 new StateInstance(PaymentAfterPE.Instance),
                                 new StateInstance(PaymentAfterPE.Instance));

            }
        }

        public static new IState Instance(String message)
        {
            input = new Number();
            if (!message.Equals(String.Empty))
                DisplayAdapter.Cashier.Show(message);
            return state;
        }


        public static IState Instance(IncompletePaymentException ipe)
        {
            input = new Number();
            if (ipe.Difference == 0)
            {
                cr.Document.Close();
                cr.Document = new Receipt();
                return States.Start.Instance();
            }
            else
            {
                cr.Document.BalanceDue = ipe.Difference;
                cr.Document.TotalAmount = ipe.Difference;
                cr.State = state;
            }

            return States.AlertCashier.Instance(new Error(ipe, Instance));
        }

        public override void Command() { }
        public override void SubTotal() { }
        public override void Document() { }
        public override void Void() { }

        public override void Escape()
        {
            Instance(PosMessage.PAYMENT_STARTED);
        }
    }
}
