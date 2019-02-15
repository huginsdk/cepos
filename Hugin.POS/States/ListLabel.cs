using System;
using Hugin.POS.States;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class ListLabel : List
    {
        private static IState state = new ListLabel();
        private static int labelKey;
        private static new ProcessSelectedItem<IProduct> ProcessSelected;


        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<IProduct> psi, int labelKey)
        {
            ListLabel.labelKey = labelKey;
            ProcessSelected = psi;
            List.Instance(ide);
            return state;
        }

        //public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem<Product> psi, int labelKey, int autoEnterMillis)
        //{
        //    ListLabel.labelKey = labelKey;
        //    ProcessSelected = psi;
        //    autoEnterMillis = autoEnterMillis;
        //    List.Instance(ide);
        //    return state;
        //}

        public override void LabelKey(int labelKey)
        {
            if (labelKey == ListLabel.labelKey)
                base.DownArrow();
            else
            {
                System.Collections.Generic.List<IProduct> sList = new System.Collections.Generic.List<IProduct>();
                if (CashRegister.DataConnector.CurrentSettings.GetProgramOption(Setting.DefineBarcodeLabelKeys) == PosConfiguration.ON)
                    sList = CashRegister.DataConnector.SearchProductByBarcode(Label.GetLabel(labelKey));
                else
                    sList = CashRegister.DataConnector.SearchProductByLabel(Label.GetLabel(labelKey));

                MenuList itemList = new MenuList();
                foreach (IProduct p in sList)
                {
                    FiscalItem si = (FiscalItem)CashRegister.Item.Clone();
                    si.Product = p;
                    itemList.Add(si);
                }

                if (itemList.IsEmpty)
                    CashRegister.State = AlertCashier.Instance(new Error(new ProductNotFoundException()));
                else
                    CashRegister.State = ListLabel.Instance(itemList, ProcessSelected, labelKey);
            }
        }

        public override void Enter()
        {
            ProcessSelected(((FiscalItem)ie.Current).Product);
        }

    }
}
