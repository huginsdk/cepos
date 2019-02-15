using System;
using System.Collections.Generic;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class BlockOnPaper : BlockingState
    {
        private static IState state = new BlockOnPaper();
        private static IState prevState = null;

        private static IState OnVoidDocument()
        {
            cr.Document.Void();
            return Start.Instance();
        }
        private static IState OnContinue()
        {
            DisplayAdapter.Cashier.Show(PosMessage.CONTINUE_SELLING);
            return Selling.Instance();
        }
        public static IState Instance()
        {
            prevState = cr.State;
            DisplayAdapter.Cashier.Show(PosMessage.PUT_PAPER_IN);
            return state;
        }
        public static IState OnSuccess()
        {
            return ConfirmVoid.Instance(PosMessage.CONTINUE_OR_VOIDING_SLIP_SALE_ON_ERROR,
                    new StateInstance(OnVoidDocument), new StateInstance(OnContinue));
        }
        public override void Command()
        {

        }
        protected override bool BlockRemoved()
        {
            try
            {
                cr.Printer.PrintRemark(" ".PadLeft(40, ' '));
                
                ReturnInstance = new StateInstance(OnSuccess);

                return true;
            }
            catch (SlipException) { }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
