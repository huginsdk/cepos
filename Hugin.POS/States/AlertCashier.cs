using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
	//internal delegate void ReturnInstance();
	 
    class AlertCashier : SilentState
    {
        
        /// <summary>
        ///This state locks all user input until Esc is pressed
        ///then returns CashRegister to the state it was in before the alert
        /// </summary>
        private static IState state = new AlertCashier();
        public static StateInstance<Hashtable> ReturnConfirmWithArgs;
        public static StateInstance ReturnConfirm;
        public static StateInstance ReturnCancel;
        public static Hashtable data;

        private static int alertTimeout = -1;

        //private static readonly String alertTimeoutStr = PosConfiguration.Get("CashierAlertTimeout");

        #region Instance
        public static void SetTimeouts()
        {
            if (alertTimeout == -1)
            {
                try
                {
                    String alertTimeoutStr = PosConfiguration.Get("CashierAlertTimeout");
                    alertTimeout = (int)(decimal.Parse(alertTimeoutStr) * 1000m);
                }
                catch
                {
                    alertTimeout = 1000;
                }
            }
        }
        
        //Hatali islem
        public static IState Instance()
        {
            return state;
        }       
        /// <summary>
        ///    Used for custom error messages
        ///    Confirm state defines what state the machine should assume after 
        ///    user hits Enter key.
        ///    Cancel state defines what state the machine should assume after
        ///    user hits Escape key.
        /// </summary>
        /// <param name="e">
        /// Contains message and,
        /// return to methods after jop is completed
        /// </param>
        /// <returns></returns>
        public static IState Instance(Confirm c)
        {
            
            DisplayAdapter.Cashier.Show(c);
            System.Threading.Thread.Sleep(alertTimeout);
            DisplayAdapter.Cashier.ClearError();
            return States.ConfirmCashier.Instance(c);
            

            //ReturnConfirm = c.ReturnConfirm;
            //ReturnCancel = c.ReturnCancel;
            //ReturnConfirmWithArgs = c.ReturnConfirmWithArgs;
            //data = c.Data;
            //if (DisplayAdapter.Cashier.ShowAlertMessage(c.Message))
            //{
            //    IState retState;
            //    if (ReturnConfirm != null)
            //        retState = ReturnConfirm();
            //    else if (ReturnConfirmWithArgs != null && data != null)
            //        retState = ReturnConfirmWithArgs(data);
            //    else
            //        retState = States.Start.Instance();

            //    data = null;

            //    return retState;
            //}
            //else
            //    return (ReturnCancel == null) ? States.Start.Instance() : ReturnCancel();
        }

        /// <summary>
        /// Returs Cashier alert time..
        /// </summary>
        public static int AlertTimeout {
            get {
                return alertTimeout; 
            }
        }

        #endregion

        #region KeyHandlers
        /// <summary>
        /// Cancel state defines what state the machine should assume after
        /// user hits Escape key.
        /// </summary>
        public override void Escape()
        {
            cr.State = (ReturnCancel == null) ? States.Start.Instance() :
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
