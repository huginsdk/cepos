using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS.States
{
	internal delegate void ReturnInstance();

    abstract class BlockingState : SilentState
    {
        protected static StateInstance ReturnInstance;
        //protected abstract void ResolveErrorCondition();

        public override bool IsIdle
        {
            get
            {
                return false;
            }
        }

        protected abstract Boolean BlockRemoved();
        protected virtual void TryRemoveBlock() { ;}       
        public override void Enter()
        {
            TryRemoveBlock();
            Escape();
        }
        public override void Escape()
        {
            if (BlockRemoved())
                if (ReturnInstance != null)
                    cr.State = ReturnInstance();
                else
                    cr.State = States.Start.Instance();
        }
        public override void Command()
        {
            //cr.State = ServiceMenu.Instance();
        }
        public override void End(int keyLevel)
        {
            
        }

    }
}
