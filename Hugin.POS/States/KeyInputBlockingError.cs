using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS.States
{

    class KeyInputBlockingError : BlockingState
    {
        private static IState state = new KeyInputBlockingError();

        public static IState Instance()
        {
            return state;
        }
        public static IState Instance(Error promptMessage)
        {
            return Instance();// state;
        }

        protected override void TryRemoveBlock() {;}
        protected override Boolean BlockRemoved()
        {
            return false;
                        
        }
        public override void Command()
        {
        }
    }
}
