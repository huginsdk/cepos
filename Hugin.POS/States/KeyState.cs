using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS.States
{
    class KeyState : SilentState
    {
        private static IState state = new KeyState();
        
        public static IState Instance()
        {
            return state;
          
        }
        public override void End(int keyLevel)
        {
            cr.RegisterAuthorizationLevel = (Common.AuthorizationLevel)keyLevel;
            cr.State = States.Start.Instance();
            
        }   
     
    }
}
