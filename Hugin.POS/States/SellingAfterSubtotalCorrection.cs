using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class SellingAfterSubtotalCorrection : Selling
    {
        private static IState state = new SellingAfterSubtotalCorrection();

        public override Error NotImplemented
        {
            get
            {
                return new Error(new Exception(PosMessage.INVALID_OPERATION),
                                 new StateInstance(Instance),
                                 new StateInstance(Instance));

            }
        }

        public static new IState Instance()
        {
            return state;
        }
        public override void Correction()
        {
            AlertCashier.Instance(new Error(new NoCorrectionException()));
        }
    }
}
