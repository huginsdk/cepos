using System;
using System.Collections;
using System.Threading;
using cr = Hugin.POS.CashRegister;
using System.Windows.Forms;
using System.Collections.Generic;
using Hugin.POS.Common;


namespace Hugin.POS.States
{
    class GraphicalList : List
    {
        private static IState state = new GraphicalList();

        public static new IState Instance(IDoubleEnumerator ide)
        {
            Instance(ide, new ProcessSelectedItem(cr.Execute));
            return state;
        }

        //Instance method has 2 parameters
        //1. Enumerator for a list of items to be displayed. Each item implements
        //IMenuItem interface, which means that it knows howto display itself
        //2. Delegate for the method which will be called on the selected item
        //when the user presses enter key
        public static new IState Instance(IDoubleEnumerator ide, ProcessSelectedItem psi)
        {
#if WindowsCE
            return List.Instance(ide,psi);
#else
            ListForm.SetList(ide);
            ProcessSelected = psi;
            List.Instance(ide);
            return state;
#endif
        }
        public override void UpArrow()
        {
            base.UpArrow();
#if !WindowsCE
            ie.ShowCurrent(Target.Customer);
#endif
        }
        public override void DownArrow()
        {
            base.DownArrow();
#if !WindowsCE
            ie.ShowCurrent(Target.Customer);
#endif
        }
        public override void Escape()
        {
#if !WindowsCE
                ListForm.SetFormVisible(false);
#endif
            base.Escape();
        }
        public override void Enter()
        {
#if !WindowsCE
                ListForm.SetFormVisible(false);
#endif
            base.Enter();
        }
    }
}