using System;
using Hugin.POS.Common;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS.States
{
    class ListCommandMenu : List
    {
        private static IState state = new ListCommandMenu();
        private static new StateInstance ReturnCancel;
        protected static new ProcessSelectedItem<MenuLabel> ProcessSelected;

        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<MenuLabel> psi)
        {
            ProcessSelected = psi;
            List.Instance(ide);
            return state;
        }

        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<MenuLabel> psi, StateInstance returnCancel)
        {
            ReturnCancel = returnCancel;
            return Instance(ide, psi);
        }

        public override void  Command()
        {
 	       base.DownArrow();
        }

        public override void Enter()
        {
            ReturnCancel = null;
            ProcessSelected((MenuLabel)ie.Current);
        }

        public override void Escape()
        {
            if (ReturnCancel != null)
            {
                cr.State = ReturnCancel();
                ReturnCancel = null;
            }
            else
                cr.State = States.Start.Instance();
        }

        public override void OnExit()
        {
            DisplayAdapter.Cashier.Show(null as MenuList);
            autoEnter = 0;
            //when a list item is selected from menu, if operation requires confirmation then state changes
            //however the return state should not be changed, because return state can be a blocking state like CashRegisterLoadError
            if (!(cr.State is States.SetupMenu) && !(cr.State is States.ListCommandMenu))
                ReturnCancel = null;
        }
    }
}
