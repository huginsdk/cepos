using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
	//internal delegate void ReturnInstance();
	 /// <summary>
     /// Used for custom error messages
	 /// </summary>
    class ConfirmCashier : SilentState
    {
        private static IState state = new ConfirmCashier();
        public static StateInstance<Hashtable> ReturnConfirmWithArgs;
        public static StateInstance ReturnConfirm;
        public static StateInstance ReturnCancel;       
        public static Hashtable data;
        #region Instance
        
        public static IState Instance()
        {
            return state;
        }  

        /// <summary>
        /// Defines instance of ConfirmCashier state. 
        /// This state is used for cashier error messages.
        /// </summary>
        /// <param name="promptMessage">
        /// is a parameter that contains error message,
        /// return and cancel state. It means that what state the machine should assume after
        /// user hits Escape key or Enter key.
        /// </param>
        /// <returns>confirm or cancel satate</returns>
        public static IState Instance(Confirm promptMessage)
        {
            
            ReturnConfirm = promptMessage.ReturnConfirm;
            ReturnCancel = promptMessage.ReturnCancel;
            ReturnConfirmWithArgs = promptMessage.ReturnConfirmWithArgs;
            data = promptMessage.Data;
            DisplayAdapter.Cashier.Show(promptMessage.Message);
            return Instance();
        }        

        #endregion
        
        #region KeyHandlers
        /// <summary>
        /// Cancel state defines what state the machine should assume after
        /// user hits Escape key.
        /// </summary>
        public override void Escape()
        {
            cr.State = (ReturnCancel == null)? States.Start.Instance():
                                               ReturnCancel();
        }
        /// <summary>
        /// Confirm state defines what state the machine should assume after 
        /// user hits Enter key. 
        /// </summary>
        public override void Enter()
        {
            if (ReturnConfirm != null)
                cr.State = ReturnConfirm();
            else if (ReturnConfirmWithArgs != null && data != null)
                cr.State = ReturnConfirmWithArgs(data);
            data = null;
        }        
        #endregion

    }
}
