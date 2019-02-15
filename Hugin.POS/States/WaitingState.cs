using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS.States
{
    class WaitingState : SilentState
    {
        private static IState state = new WaitingState();
        
        public static IState Instance()
        {
            return state;
          
        }       

    }
}
