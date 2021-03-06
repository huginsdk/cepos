using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;


    namespace Hugin.POS.States
    {
        class EnterUnitPrice : State
        {
            private static IState state = new EnterUnitPrice();
            private static Number input;
            private const String cashierMsg = PosMessage.UNIT_PRICE;
            private Confirm err = new Confirm(PosMessage.DECIMAL_LIMIT,
                           new StateInstance(DecimalOverflow));
            public static IState Instance()
            {
                input = new Number();
                DisplayAdapter.Cashier.Show(cashierMsg);
                return state;
            }

            public static IState Instance(Decimal amount)
            {
                input = new Number(amount);

                return state;
            }

            public override void Numeric(char c)
            {
                if (input.Decimals == 2)
                {
                    //TODO:Virgulden sonro 3 hane girilirse eger kullanİcİnİn uyarİlmasi istenirse hata mesaji icin commentin kaldirilmasi yeterli..
                    AlertCashier.Instance(err);
                    return;
                }
                input.AppendDecimal(c);
                DisplayAdapter.Cashier.Append(c.ToString());
            }

            public override void Enter()
            {
                decimal price = input.ToDecimal();
                if (price > 0)
                {
                    cr.Item.UnitPrice = input.ToDecimal();
                    cr.State = States.EnterNumber.Instance();
                }
                else
                {
                    Confirm invalidPrice = new Confirm("HATALI FIYAT\nTEKRAR DENEYIN",
                                                    new StateInstance(Instance));
                    cr.State = AlertCashier.Instance(invalidPrice);
                }
                                                      
            }

            public override void Escape()
            {
                if (input.IsEmpty)
                {
                    cr.State = Start.Instance();
                }
                else
                {
                    cr.State = Instance();
                }
            }
            public static IState DecimalOverflow()
            {
                Number oldValue = new Number(input.ToString());
                Instance();
                foreach (Char c in oldValue.ToString())
                {
                    input.AppendDecimal(c);
                    DisplayAdapter.Cashier.Append(c.ToString());
                }
                return state;
            }          
            public override void LabelKey(int label)
            {
                switch (label) 
                {
                    case Label.BackSpace:
                        input.RemoveLastDigit();
                        DisplayAdapter.Cashier.BackSpace();
                        break;
                    case Label.Space:
                        base.LabelKey(label);
                        break;
                    default:
                        base.LabelKey(label);
                        break;
                }                
            }
            public override void Seperator()
            {
                if (input.AddSeperator())
                    DisplayAdapter.Cashier.Append(Number.DecimalSeperator);
            }
            public override void Correction()
            {
                if (input.Length > 0)
                {
                    input.RemoveLastDigit();
                    DisplayAdapter.Cashier.BackSpace();
                }
            }
        }
    

}
