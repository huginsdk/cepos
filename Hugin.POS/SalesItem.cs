using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS
{

    public class SalesItem : FiscalItem
    {
        public SalesItem() {
            Reset();
        }

        public override void Show()
        {
            DisplayAdapter.Cashier.ShowSale(this);
            DisplayAdapter.Customer.ShowSale(this);
        }
        public override void Show(Target target)
        {
            if (target == Target.Cashier)
                DisplayAdapter.Cashier.Show(this);
            else
                DisplayAdapter.Customer.Show(this);
        }

        public override IState ConfirmSalesPerson(ICashier salesPerson)
        {
            if (salesPerson == null) return States.Start.Instance();
            Confirm confirm = new Confirm(String.Format("{0}{1}", PosMessage.CLERK_FOR_ITEM, salesPerson.Name.TrimEnd()),
                                                          new StateInstance<Hashtable>(SaveSalesPerson),
                                                          new StateInstance(States.EnterPassword.Instance));
            confirm.Data.Add("SalesPerson", salesPerson);
            return States.ConfirmCashier.Instance(confirm);
        }

        public IState SaveSalesPerson(Hashtable args)
        {
            SalesPerson = args["SalesPerson"] as ICashier;

            if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON && SalesPerson != null)
                cr.Printer.PrintRemark(String.Format("{0} : {1} {2} ", PosMessage.CLERK, SalesPerson.Id, SalesPerson.Name.Trim()));

            return States.Start.Instance();
        }

        public override IState VoidSalesPerson()
        {
           // if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON && SalesPerson != null)
           //     cr.Printer.PrintRemark(String.Format("{0} : {1} {2} ", PosMessage.VOID, SalesPerson.Id, SalesPerson.Name.Trim()));

            SalesPerson = null;

            return States.EnterClerkNumber.Instance(PosMessage.CLERK_ID,
                                          new StateInstance<ICashier>(ConfirmSalesPerson),
                                          new StateInstance(States.Start.Instance));
        }

        public override FiscalItem Void()
        {
            return new VoidItem(this);
        }
    }
}
