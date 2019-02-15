using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class VoidSale : State
    {
        private static IState state = new VoidSale();
        private static String cashierMsg = PosMessage.VOID_FIND_PRODUCT;
        
        public static IState Instance()
        {
            DisplayAdapter.Cashier.Show(cashierMsg);
        	return state;
        }
        public override void Escape()
        {
            cr.Item = new SalesItem();
        	cr.State = Start.Instance();
        }
        public override Error NotImplemented
        {
            get
            {
                cr.Item = new SalesItem();
                return base.NotImplemented;
            }
        }
        public override void Repeat()
        {
            ProductMenuList soldProducts = new ProductMenuList();
        	foreach (FiscalItem fi in cr.Document.Items)
                if (fi is SalesItem) {
                    VoidItem voidItem = cr.Item.Clone() as VoidItem;
                    voidItem.Product = fi.Product;
                    if (cr.Document.CanAddItem(voidItem))
                        soldProducts.Add(fi.Product);
                }
            cr.State = ListProductRepeat.Instance(soldProducts, new ProcessSelectedItem<IProduct>(cr.Execute));
        }
        public override void LabelKey(int labelKey)
        {
            System.Collections.Generic.List<IProduct> sList = new System.Collections.Generic.List<IProduct>();
            if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.DefineBarcodeLabelKeys) == PosConfiguration.ON)
                sList = cr.DataConnector.SearchProductByBarcode(Label.GetLabel(labelKey));
            else
                sList = cr.DataConnector.SearchProductByLabel(Label.GetLabel(labelKey));

            if (sList.Count == 0)
            {
                cr.State = AlertCashier.Instance(new Error(new ProductNotFoundException()));
                return;
            }
            MenuList itemList = new MenuList();
            foreach (IProduct p in sList)
            {
                FiscalItem fi = (FiscalItem)cr.Item.Clone();
                fi.Product = p;
                itemList.Add(fi);
            }
            IDoubleEnumerator listEnumerator = (IDoubleEnumerator)itemList;
            if (itemList.Count == 1)
            {
                if (listEnumerator.MoveNext())
                    cr.Execute(((FiscalItem)listEnumerator.Current).Product);
            }
            else
            {
                cr.State = ListLabel.Instance(itemList,
                                               new ProcessSelectedItem<IProduct>(cr.Execute),
                                               labelKey);
            }
        }
        public override void PriceLookup()
        {
            cr.State = EnterString.Instance(PosMessage.PRICE_LOOKUP, new StateInstance<String>(cr.PriceLookup));
        }

        public override void Numeric(char c)
        {
            cr.State = States.EnterNumber.Instance();
            cr.State.Numeric(c);
        }
        public override void Seperator()
        {
            cr.State = States.EnterNumber.Instance();
            cr.State.Seperator();
        }
        public override void Enter()
        {
            /* when the cancel menu(key) selected without quantity 
             * create a list which contains the items whose remaining quantity is bigger than zero
             */
            MenuList cancelMenu = new MenuList();

            foreach (FiscalItem fiscalItem in cr.Document.Items)
            {
                if (fiscalItem is SalesItem)
                {
                    VoidItem voidItem = new VoidItem(fiscalItem);
                    /* quantity of void item is equal to remaining quantity of sale item */
                    if (Math.Abs(voidItem.Quantity) > 0 && cr.Document.CanAddItem(voidItem))
                        cancelMenu.Add(fiscalItem);
                }
            }

            cr.State = ListVoid.Instance(cancelMenu, new ProcessSelectedItem<FiscalItem>(VoidSelectedItem));
        }

        
        internal static void VoidSelectedItem(FiscalItem fi)
        {
            cr.Execute(new VoidItem(fi));
        }
    }
}
