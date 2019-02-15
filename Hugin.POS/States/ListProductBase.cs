using System;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    abstract class ListProductBase : List
    {
        protected static new ProcessSelectedItem<IProduct> ProcessSelected;
        
        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<IProduct> psi)
        {
            ide.MoveLast();
            ide.MovePrevious();
            ProcessSelected = psi;
            return List.Instance(ide);
        }

        public override void Enter()
        {
#if !WindowsCE
            DisplayAdapter.Customer.Show(null as MenuList);
#endif
            ProcessSelected(ie.Current as IProduct);
        }
    }
}
