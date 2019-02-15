using System;
using cr = Hugin.POS.CashRegister;
using System.Text;
using System.Threading;
using Hugin.POS.Common;
namespace Hugin.POS.States
{
    class EnterBarcode : EnterString
    {
        static StateInstance ReturnCancel;
        static StringBuilder input;
        private static IState state = new EnterBarcode();
        protected static StateInstance<string> ReturnConfirm;

        public static new IState Instance(String message, StateInstance<string> ConfirmState)
        {
            return Instance(message, ConfirmState, null);
        }

        public static new IState Instance(String message, StateInstance<string> ConfirmState, StateInstance CancelState)
        {
            input = new StringBuilder();

            IState baseState = EnterString.Instance(message, null, CancelState);
            ReturnConfirm = ConfirmState;
            return state;
        }
        public static IState Instance(String message, string defaultValue, StateInstance<string> ConfirmState)
        {
            return Instance(message, defaultValue, ConfirmState, null);
        }

        public static new IState Instance(String message, string defaultValue, StateInstance<string> ConfirmState, StateInstance CancelState)
        {
            input = new StringBuilder();
            ReturnCancel = CancelState;
            IState baseState = EnterString.Instance(message, defaultValue, null, CancelState);
            ReturnConfirm = ConfirmState;
            return state;
        }

        public override void Seperator()
        {
        }

        public override void Numeric(char c)
        {
            Append(c, true);
        }

        void Append(char c, bool p)
        {
            input.Append(c, 1);
            DisplayAdapter.Cashier.Append(c.ToString(), p);
        }

        public override void Enter()
        {

            if (ReturnConfirm != null)
            {
                try
                {
                    cr.State = ReturnConfirm(input.ToString());
                }
                catch (EntryException ex)
                {
                    cr.State = AlertCashier.Instance(new Error(ex,
                                                                ReturnCancel,
                                                                ReturnCancel));
                }
            }

        }


    }

}
