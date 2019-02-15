using System;
using Hugin.POS.Common;

namespace Hugin.POS
{
	//TODO: Equality Operator
    public class VoidItem : FiscalItem
    {

        public VoidItem(FiscalItem item) {
            //voidedSalesItem = item;
            barcode = item.Barcode;
            name = item.Name;
            product = item.Product;
            quantity = item.Quantity;
            salesPerson = item.SalesPerson;
            unit = item.Unit;
            unitPrice = item.UnitPrice;
            totalAmount = item.TotalAmount;

            /* to escape any exception i checked the condition that the item is sale item */
            /* quantity should be remaining quantity of sale item and
             * totalAmount should be remaing amount of the sale item
             */
            if (item is SalesItem)
            {
                quantity -= ((SalesItem)item).VoidQuantity;
                totalAmount -= ((SalesItem)item).VoidAmount;
            }
        }

        public override Decimal TotalAmount
        {
            get {
                return -1 * Rounder.RoundDecimal((totalAmount > 0 ? totalAmount : unitPrice * quantity), 2, true);
            }
            set
            {
                totalAmount = value;
            }
        }

        public new Decimal UnitPrice
        {
            get
            {
                return unitPrice;
            }
            set
            {
                unitPrice = value;
                totalAmount = Rounder.RoundDecimal(unitPrice * quantity, 2, true);
            }
        }
        
        public override Decimal ListedAmount
        {
            get { return -1 * base.ListedAmount; }
        }

        public override Decimal Quantity
        {
            get
            {
                return -1 * quantity;
            }
            set
            {
                if (Math.Round(value, 3) >= 100)
                    throw new OutofQuantityLimitException();
                quantity = value;
                totalAmount = 0;
            }
        }
        
        public override void Show() {
        	DisplayAdapter.Cashier.ShowVoid(this);
            DisplayAdapter.Customer.ShowVoid(this);
        }
        
        public override void Show(Target target) {
			if (target == Target.Cashier)
                DisplayAdapter.Cashier.ShowVoid(this);
			else
                DisplayAdapter.Customer.ShowVoid(this);
        }

        public override IState ConfirmSalesPerson(ICashier salesPerson)
        {
            return States.AlertCashier.Instance(new Confirm(PosMessage.CANNOT_ASSIGN_CLERK_TO_VOID_SALE));
        }
        public override IState VoidSalesPerson()
        {
            return States.AlertCashier.Instance(new Confirm(PosMessage.CANNOT_ASSIGN_CLERK_TO_VOID_SALE));
        }

        public override FiscalItem Void()
        {
            //TODO should we be throwing an exception here?
            return this;
        }
    }
}
