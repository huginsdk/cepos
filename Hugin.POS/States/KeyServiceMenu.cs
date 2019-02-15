using System;
using System.Collections;
using System.IO;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{

    class KeyServiceMenu : ServiceMenu
    {
        private static IState state = new KeyServiceMenu();

        public static new IState Instance()
        {
            try
            {

                States.ServiceMenu.ShowMenu();
            }
            catch 
            {
                cr.State = States.AlertCashier.Instance(new Error(new InvalidOperationException()));
            
            }
            return state;
        }   

    }
}
