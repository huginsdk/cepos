using System;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
    class ListCurrencies : List
    {
        private static IState state = new ListCurrencies();
        private static new ProcessSelectedItem<CurrencyPaymentInfo> ProcessSelected;

        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<CurrencyPaymentInfo> psi)
        {
            ProcessSelected = psi;
            List.Instance(ide);
            return state;
        }
        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<CurrencyPaymentInfo> psi, StateInstance returnCancel)
        {
            ProcessSelected = psi;
            ReturnCancel = returnCancel;
            List.Instance(ide);
            return state;
        }

        public override void Pay(CurrencyPaymentInfo info)
        {
            base.DownArrow();
        }

        public override void Enter()
        {
            CurrencyPaymentInfo cpi = (CurrencyPaymentInfo)ie.Current;
            ProcessSelected((CurrencyPaymentInfo)cpi.Clone());
        }

    }
}

