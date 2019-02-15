using System;
using System.Collections.Generic;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class PaymentList: List
    {
        private static IState state = new PaymentList();
        static Number input = new Number();
        public static IState Instance(Number amount)
        {
            input = amount;
            
            MenuList list = new MenuList();
            list.Add(new CheckPaymentInfo());
            list.AddRange(CurrencyPaymentInfo.GetCurrencies());
            list.AddRange(CreditPaymentInfo.GetCredits());
            States.List.Instance(list); 
            return state; 
        }
        public override void Enter()
        {
            PaymentInfo info = (PaymentInfo)(ie.Current);
           
            if (cr.Document.IsEmpty)
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.NO_SALE_PAYMENT_INVALID));
            else
            {
                if (input == null) input = new Number();
                cr.State = Payment.Instance("");
                ((Payment)cr.State).Pay(input, info);
            }
            input.Clear();
        }
    }
}
