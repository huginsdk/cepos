using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
	//internal delegate void ReturnInstance();
	 
    class ConfirmSlip : ConfirmCashier
    {
        private static IState state = new ConfirmSlip();
        private static StateInstance<PromotionDocument> ReturnConfirmWithObject;
        private static StateInstance<PromotionDocument> ReturnCancelWithObject;
        private static Error error;
        private static PromotionDocument document;
   
        #region Instance

        //Used for custom error messages
        /// <summary>
        /// Used for custom error messages.       
        /// </summary>
        /// <param name="promptMessage">
        /// Contains error message for cashier,return and cancel state.
        /// returnState:what state the machine should assume after user hits Enter key
        /// cancelState:what state the machine should assume after user hits Escape key
        /// </param>
        /// <returns>returnState or cancelState</returns>        
        public static IState Instance(Error promptMessage)
        {
            ReturnConfirm = promptMessage.ReturnConfirm;
            ReturnCancel = promptMessage.ReturnCancel;
            ReturnConfirmWithArgs = promptMessage.ReturnConfirmWithArgs;
            data = promptMessage.Data;
            error = promptMessage;
            DisplayAdapter.Cashier.Show(promptMessage.Message);
            return state;
        }
        public static IState Instance(String message, StateInstance<PromotionDocument> returnConfirmWithObject, StateInstance<PromotionDocument> returnCancelWithObject, PromotionDocument doc) 
        {
            ReturnConfirmWithObject = returnConfirmWithObject;
            ReturnCancelWithObject = returnCancelWithObject;
            document = doc;
            DisplayAdapter.Cashier.Show(message);
            return state;
        
        }
        public static IState Instance(String message, StateInstance returnConfirm, StateInstance returnCancel)
        {
            ReturnConfirm = returnConfirm;
            ReturnCancel = returnCancel;           
            DisplayAdapter.Cashier.Show(message);
            return state;

        }
        public static new IState Instance()
        {
            Instance(error);
            return state;
        }
        #endregion
        /// <summary>
        /// Command key function
        /// used only when FPU slip is active.
        /// used to void slip while slip is active
        /// </summary>
        public override void Command()
        {
            MenuList menuHeaders = new MenuList();
            menuHeaders.Add(new MenuLabel(PosMessage.VOID_DOCUMENT));
            cr.State = List.Instance(menuHeaders,
                       new ProcessSelectedItem(SelectMenu),
                       new StateInstance(Instance));                     
        }
        public static IState CommandMenu()
        {
            cr.State = state;
           cr.State.Command();
           return cr.State;
        
        }
        /// <summary>
        /// select menu. When user press command key if user select a menu from list the selected item is checked in this function.
        /// </summary>
        /// <param name="si"></param>
        public void SelectMenu(object si) 
        {
            switch (si.ToString())
            {
                case PosMessage.VOID_DOCUMENT:
                    try{
                    cr.Document.Void();
                    }
                   catch(CmdSequenceException)
                   {
                   cr.RecoverFromPowerFailure();
                   }
                    cr.State= States.Start.Instance();
                    break;
            }        
        }       
        public IState CancelConfirm() 
        {
            this.Command();
            return state;
        }
        public override void Enter()
        {
            if (ReturnConfirmWithObject != null)
                cr.State = ReturnConfirmWithObject(document);
            else if (ReturnConfirm != null)
                cr.State = ReturnConfirm();
            else
                cr.State = States.Start.Instance();
            ClearEventsLog();
        }
        public override void Escape()
        {
            if (ReturnCancelWithObject != null)
                cr.State = ReturnCancelWithObject(document);
            else if (ReturnCancel != null)
                cr.State = ReturnCancel();
            else
                cr.State = States.Start.Instance();
            ClearEventsLog();
        }
        private void ClearEventsLog()
        {
            if (cr.State == state)
                return;
            ReturnConfirmWithObject = null;
            ReturnCancelWithObject = null;
            ReturnCancel = null;
            ReturnConfirm = null;
            document = null;        
        }
    }
}
