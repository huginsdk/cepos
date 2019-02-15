using System;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
    class ListCreditTypes : List
    {
        private static IState state = new ListCreditTypes();
        protected static new ProcessSelectedItem<CreditPaymentInfo> ProcessSelected;

        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<CreditPaymentInfo> psi)
        {
            ProcessSelected = psi;
            for (int i = 0; i < KeyMap.NumberOfCreditKeys; i++)
                ide.MoveNext();
            List.Instance(ide);
            return state;
        }
        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<CreditPaymentInfo> psi, StateInstance returnCancel)
        {
            ProcessSelected = psi;
            ReturnCancel = returnCancel;
            ide.MoveNext();
            List.Instance(ide);          
            return state;
        }
        public override void Pay(CreditPaymentInfo info)
        {
            base.DownArrow();
        }

        public override void Enter()
        {
            CreditPaymentInfo cpi = (CreditPaymentInfo)ie.Current;
            ProcessSelected((CreditPaymentInfo)cpi.Clone());
        }
 
    }
}
