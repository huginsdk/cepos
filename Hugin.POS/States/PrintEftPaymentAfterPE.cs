using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class PrintEftPaymentAfterPE : BlockingState
    {
        private static IState state = new PrintEftPaymentAfterPE();

        public static IState Instance()
        {
            DisplayAdapter.Cashier.Show(PosMessage.INCOMPLETE_PAYMENT_AFTER_EFT_DONE);
            return state;
        }

        protected override void TryRemoveBlock() { ;}
        protected override Boolean BlockRemoved()
        {
            States.Payment.ComplateCreditPayment();
            return true;

        }
    }
}
