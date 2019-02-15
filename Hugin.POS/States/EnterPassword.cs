using System;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class EnterPassword : SilentState
    {        
        private static IState state =  new EnterPassword();
        private static StringBuilder password;
        private static String cashierMsg = PosMessage.ENTER_PASSWORD;
        private static StateInstance<String> ReturnSuccess;
        private static StateInstance ReturnCancel;
        
        public static IState Instance(){
            password = new StringBuilder(cr.MAX_CASHIER_PASSWOR_LENGTH);
            DisplayAdapter.Cashier.Show(cashierMsg);
            return state;
        }
        
        public static IState Instance(String message, StateInstance<String> Success, StateInstance Cancel)
        {
            password = new StringBuilder(cr.MAX_CASHIER_PASSWOR_LENGTH);
            cashierMsg = message;
            DisplayAdapter.Cashier.Show(message);
            ReturnSuccess = Success;
            ReturnCancel = Cancel;
            return state;
        }

        public static IState Retry()
        {
            password = new StringBuilder(cr.MAX_CASHIER_PASSWOR_LENGTH);
            DisplayAdapter.Cashier.Show("{0}\n{1}", PosMessage.INVALID_PASSWORD, PosMessage.PROMPT_RETRY);
            return state;
        }
        #region KeyHandlers
        
        public override void Numeric(char c)
        {
            password.Append(c.ToString());
            if (password.Length == 1)
            {
                DisplayAdapter.Cashier.Show(cashierMsg);
                DisplayAdapter.Cashier.Append("*");
            }
            else DisplayAdapter.Cashier.Append("*");            
        }        
        public override void Escape()
        {
            if (password.Length > 0){
                password = new StringBuilder(cr.MAX_CASHIER_PASSWOR_LENGTH);
                DisplayAdapter.Cashier.Show(cashierMsg); 
            } else {
                cr.State = (ReturnCancel == null) ? States.Start.Instance()
                                                  : ReturnCancel();
            }
            
        }
        public override void Enter()
        {
            cr.State = ReturnSuccess(password.ToString());
        }
        public override void LabelKey(int label)
        {
            switch(label){
                case Label.BackSpace:
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        DisplayAdapter.Cashier.BackSpace();
                    }
                    break;
                case Label.Space:
                    base.LabelKey(label);
                    break;
                default:
                    base.LabelKey(label);
                    break;
            }
        }
        #endregion

        public override void End(int keyLevel)
        {
            if (cr.CurrentCashier != null)
                base.End(keyLevel);
        }

        public override void Correction()
        {
            if (password.Length > 0)
            {
                password.Remove(DisplayAdapter.Cashier.CurrentColumn - 1, 1);
                DisplayAdapter.Cashier.BackSpace();
            }

        }
    }
}
