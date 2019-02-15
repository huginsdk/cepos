using System;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class LoginLocked :SilentState
    {
        private static IState state = new LoginLocked();
        private StringBuilder password;

        public override bool IsIdle
        {
            get
            {
                return true;
            }
        }

        public static IState Instance()
        {
            ((LoginLocked)state).password = new StringBuilder();
            return state;
        }
        public override void Numeric(char c)
        {
            password.Append(c.ToString());
            if (password.Length == 1)
            {
                DisplayAdapter.Cashier.Show("{0}{1}" ,PosMessage.ENTER_PASSWORD, "*");
                DisplayAdapter.Cashier.Append("*");
            }
            else if(password.Length==6)
            {
                ICashier cashier = cr.DataConnector.FindCashierByPassword(password.ToString());
                if (cashier != null && cashier.AuthorizationLevel == AuthorizationLevel.Z) {
                    cr.State = States.Login.Instance();
                }
                else 
                {
                    DisplayAdapter.Cashier.Append("*");
                    password = new StringBuilder();
                    DisplayAdapter.Cashier.Show("{0}{1}\n{2}",PosMessage.INVALID_PASSWORD,"*",PosMessage.PROMPT_RETRY);
                }
            }
            else
                DisplayAdapter.Cashier.Append("*");
            return;
 
        } 
        public override void Escape() {
            password = new StringBuilder();
            DisplayAdapter.Cashier.Show("{0}\n{1}", PosMessage.ENTER_PASSWORD , "");
            DisplayAdapter.Cashier.Clear();

        }       
        //public override  void BackSpace()
        //{
        //    if (password.Length > 0)
        //    {
        //        password.Remove(password.Length - 1, 1);
        //        DisplayAdapter.Cashier.BackSpace();
        //    }
        //}
        public override void Correction()
        {
            if (password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                DisplayAdapter.Cashier.BackSpace();
            }
        }
        public override void LabelKey(int label)
        {
            switch (label)
            { 
                case Label.BackSpace:
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        DisplayAdapter.Cashier.BackSpace();
                    }
                    break;
                case Label.Space:                    
                    break;
                default:
                    base.LabelKey(label);
                    break;
            }           
        }
       
      
    }
}
