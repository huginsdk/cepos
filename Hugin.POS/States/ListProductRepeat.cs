using System;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class ListProductRepeat : ListProductBase
    {
        private static IState state = new ListProductRepeat();

        public static new IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<IProduct> psi)
        {
            IState returnedState = ListProductBase.Instance(ide, psi);
            return (returnedState is States.List) ? state : returnedState;
        }

        public override void Repeat()
        {
            base.UpArrow();
        }


    }
}
