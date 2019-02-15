using System;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class ListProductPriceLookup : ListFiscalItem
    {
        private static IState state = new ListProductPriceLookup();

        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<FiscalItem> psi)
        {
            ProcessSelected = psi;
            List.Instance(ide);
            ShowCurrent();
            return state;
        }

        public override void Numeric(char c)
        {
            CashRegister.State = EnterString.Instance(PosMessage.PRICE_LOOKUP,
                                                        c.ToString(),
                                                        new StateInstance<String>(CashRegister.PriceLookup),
                                                        null);
        }

        public override void UpArrow()
        {
            base.UpArrow();
            ShowCurrent();
        }

        public override void DownArrow()
        {
            base.DownArrow();
            ShowCurrent();
        }

        private static void ShowCurrent()
        {
            ie.ShowCurrent(Target.Customer);
        }
    }
}
