using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
namespace Hugin.POS.States
{
    //This is not really a state. Just forwards to AlertCashier state
    //displays logout confirmation and returns user to login
    //screen on enter
    
	class ConfirmLogout : SilentState
    {
        /// <summary>
        /// Confirms cashier whether signout or not
        /// </summary>
        /// <returns>confirm state</returns>
        public static IState Instance()
        {
            return ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_LOGOUT,
        	                             new StateInstance(ConfirmLogout.Logout),
        	                             new StateInstance(Start.Instance)));
        }
        
        /// <summary>
        /// Log-Outs cashier from FPU
        /// </summary>
        /// <returns></returns>
        public static IState Logout()
        {
            Login.SignOutCashier();
        	return Login.Instance();
        }
	}
}
