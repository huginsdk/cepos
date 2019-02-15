using System;
using cr = Hugin.POS.CashRegister;
namespace Hugin.POS.States
{
    class EnterCurrency : EnterDecimal
    {
        private static IState state = new EnterCurrency();

        public static new IState Instance(String message, StateInstance<decimal> ConfirmState)
        {
            return Instance(message, ConfirmState, null);
        }

        public static new IState Instance(String message, StateInstance<decimal> ConfirmState, StateInstance CancelState)
        {
            IState baseState = EnterDecimal.Instance(message, ConfirmState, CancelState);
            return state;
        }

        public static new IState Instance(String message, decimal defaultValue, StateInstance<decimal> ConfirmState)
        {
            return Instance(message, defaultValue, ConfirmState, null);
        }

        public static new IState Instance(String message, decimal defaultValue, StateInstance<decimal> ConfirmState, StateInstance CancelState)
        {
            IState baseState = EnterDecimal.Instance(message, defaultValue, ConfirmState, CancelState);
            return state;
        }

        public override void Numeric(char c)
        {
            if (input.Decimals == 2) return;
            base.Numeric(c);
        }

    }
}