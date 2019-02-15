using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{

    class ConfirmVoid : ConfirmCashier
    {
        private static StateInstance ReturnVoid;
        private static IState state = new ConfirmVoid();
        #region Instance

        //Used for custom error messages
        /// <summary>
        /// Used for custom error messages.       
        /// </summary>
        /// <param name="promptMessage">
        /// Contains error message for cashier,void and cancel state.
        /// returnVoid:what state the machine should assume after user hits Void key
        /// cancelState:what state the machine should assume after user hits Escape key
        /// </param>
        /// <returns>returnVoid or cancelState</returns>        
        public static IState Instance(String message, StateInstance returnVoid, StateInstance returnCancel)
        {
            ReturnVoid = returnVoid;
            ReturnCancel = returnCancel;
            DisplayAdapter.Cashier.Show(message);
            return state;

        }
        #endregion

        public override void Void()
        {
            if (ReturnVoid != null)
                cr.State = ReturnVoid();
            else
                cr.State = States.Start.Instance();
        }
        public override void Escape()
        {
            if (ReturnCancel != null)
                cr.State = ReturnCancel();
            else
                cr.State = States.Start.Instance();
        }
        public override void Enter()
        {
            
        }
    }
}
