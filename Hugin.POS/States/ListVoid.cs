using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class ListVoid : List
    {
        private static IState state = new ListVoid();
        private static new ProcessSelectedItem<FiscalItem> ProcessSelected;

        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<FiscalItem> psi)
        {
            ProcessSelected = psi;
            ide.MoveLast();
            ide.MovePrevious();
            IState baseState = List.Instance(ide);

            return (baseState is List) ? state : baseState;
        }

        public override void Enter()
        {
            try
            {
#if !WindowsCE
                DisplayAdapter.Customer.Show(null as MenuList);
#endif
                ProcessSelected((FiscalItem)ie.Current);
            }
            catch (InvalidOperationException) {
                cr.State = AlertCashier.Instance(new Error(new ListingException()));
            }
        }

        public override void Void()
        {
            base.UpArrow();
        }

        public override void Escape()
        {
#if !WindowsCE
            DisplayAdapter.Customer.Show((IMenuList)null);
#endif
            cr.State = Start.Instance();
        }        
    }
}
