using System;
using cr = Hugin.POS.CashRegister;
using System.IO;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class Teller : State
    {
        private static IState state = new Teller();
        private static decimal quantity;
        private static IProduct currentProduct = null;

        public override Error NotImplemented
        {
            get
            {
                return new Error(new Exception(PosMessage.INVALID_ENTRY),
                                 new StateInstance(Continue),
                                 new StateInstance(Continue));

            }
        }

        public static IState Instance()
        {
            cr.State = EnterDecimal.Instance(PosMessage.PRODUCT_QUANTITY,
                                            new StateInstance<decimal>(EnterBarcode),
                                            new StateInstance(Quit));
            return cr.State;
        }

        public static IState ChangeQuantity(int value)
        {
            quantity = value;
            return ShowProduct(currentProduct.Barcode);
        }

        public static IState EnterBarcode(decimal value)
        {
            quantity = value;
            cr.State = EnterString.Instance(PosMessage.PRODUCT_BARCODE,
                                                            new StateInstance<String>(ShowProduct),
                                                            new StateInstance(Continue));
            return cr.State;

        }

        public override void SubTotal()
        {
            if (currentProduct != null)
            {
                cr.State = EnterInteger.Instance(PosMessage.PRODUCT_QUANTITY,
                                                new StateInstance<int>(ChangeQuantity),
                                                new StateInstance(Quit));
            }
        }

        public static IState ShowProduct(String barcode)
        {
            try
            {
                currentProduct = cr.DataConnector.FindProductByBarcode(barcode);

                if (currentProduct.Status != ProductStatus.Weighable)
                {
                    int val = (int)quantity;
                    if (quantity > val)
                    {
                        return AlertCashier.Instance(new Confirm(PosMessage.PRODUCT_NOT_WEIGHABLE, Continue)); ;
                    }
                }
                string firstLine = "";
                int maxLength = quantity.ToString().Length + 3;
                maxLength = 20 - maxLength;
                if (currentProduct.Name.Length > maxLength)
                    firstLine = String.Format("{0}\t{1}", currentProduct.Name.Substring(0, maxLength), quantity.ToString());
                DisplayAdapter.Cashier.Show(String.Format("{0}\n{1}\t", firstLine,
                                                               currentProduct.Barcode));
            }
            catch (Exception ex)
            {
                return AlertCashier.Instance(new Confirm(ex.Message, Continue)); ;
            }
            return state;
        }

        public override void Enter()
        {
            if (currentProduct == null) return;
            AppendProduct(currentProduct);
        }

        public static IState Quit()
        {
            cr.State = States.ConfirmCashier.Instance(
                new Confirm(PosMessage.CONFIRM_EXIT_TALLYING,
                            Start.Instance,
                            Teller.Instance)
                            );
            return cr.State;
        }

        public override void Escape()
        {
            Continue();
        }

        public static IState AppendProduct(IProduct product)
        {
            cr.DataConnector.AddToStock(product.Barcode, quantity);
            return Continue();
        }

        public static IState Continue()
        {
            quantity = 0;
            currentProduct = null;
            return Instance();
        }

    }
}
