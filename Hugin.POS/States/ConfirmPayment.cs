using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
namespace Hugin.POS.States
{
    class ConfirmPayment : EnterInteger
    {
        private static IState state = new ConfirmPayment();
        protected static StateInstance<decimal> ReturnConfirm;
        protected static StateInstance<decimal> ReturnRepeat;
        private static bool firstTime = true;

        public static IState Instance(String message, StateInstance<decimal> ConfirmState)
        {
            return Instance(message, ConfirmState, null);
        }

        public static IState Instance(String message, StateInstance<decimal> ConfirmState, StateInstance CancelState)
        {
            IState baseState = EnterInteger.Instance(message, null, CancelState);
            ReturnConfirm = ConfirmState;
            firstTime = true;
            return state;
        }

        public static IState Instance(String message, StateInstance<decimal> ConfirmState, StateInstance CancelState, StateInstance<decimal> RepeatState)
        {
            IState baseState = EnterInteger.Instance(message, null, CancelState);
            ReturnConfirm = ConfirmState;
            ReturnRepeat = RepeatState;
            firstTime = true;
            return state;
        }

        public static IState Instance(String message, decimal defaultValue, StateInstance<decimal> ConfirmState)
        {
            return Instance(message, defaultValue, ConfirmState, null);
        }

        public static IState Instance(String message, decimal defaultValue, StateInstance<decimal> ConfirmState, StateInstance CancelState)
        {
            IState baseState = EnterInteger.Instance(message, defaultValue, null, CancelState);
            ReturnConfirm = ConfirmState;
            return state;
        }
        public override void Numeric(char c)
        {
            if (firstTime)
            {
                DisplayAdapter.Cashier.Show(PosMessage.ENTER_AMOUNT);
                firstTime = false;
            }
            base.Numeric(c);
        }
        public override void Seperator()
        {
            if (input.AddSeperator())
                DisplayAdapter.Cashier.Append(Number.DecimalSeperator);
        }

        public override void Escape()
        {
            if (!input.IsEmpty)
            {
                input.Clear();
                DisplayAdapter.Cashier.Show(PosMessage.ENTER_AMOUNT);                
                return;
            }

            if (ReturnCancel == null)
                cr.State = States.Start.Instance();
            else
            {
                try
                {
                    cr.State = ReturnCancel();
                }
                catch
                {
                    cr.Log.Error("Cannot find instance method of state");
                }
            }
        }
        public override void Enter()
        {       
            if (ReturnConfirm != null)
            {
                try
                {
                    cr.State = ReturnConfirm(input.ToDecimal());
                }
                catch (EntryException)
                {
                    cr.State = AlertCashier.Instance(new Error(new InvalidOperationException(),
                                                                ReturnCancel,
                                                                ReturnCancel));
                }
            }

        }
        public override void Repeat()
        {
            if (ReturnRepeat != null)
            {
                try
                {
                    cr.State = ReturnRepeat(input.ToDecimal());
                }
                catch (EntryException)
                {
                    cr.State = AlertCashier.Instance(new Error(new InvalidOperationException(),
                                                                ReturnCancel,
                                                                ReturnCancel));
                }
            }
           
        }


    }
}
