using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
namespace Hugin.POS.States
{
    class EnterDecimal : EnterInteger
    {
        private static IState state = new EnterDecimal();
        protected static StateInstance<decimal> ReturnConfirm;

        public static IState Instance(String message, StateInstance<decimal> ConfirmState)
        {
            return Instance(message, ConfirmState, null);
        }

        public static IState Instance(String message, StateInstance<decimal> ConfirmState, StateInstance CancelState)
        {
            IState baseState = EnterInteger.Instance(message, null, CancelState);
            ReturnConfirm = ConfirmState;
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

        public override void Seperator()
        {
            if (input.AddSeperator())
                DisplayAdapter.Cashier.Append(Number.DecimalSeperator);
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
        
        
 	}
}
