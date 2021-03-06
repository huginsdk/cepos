using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;


    namespace Hugin.POS.States
    {
        class EnterTotalAmount : State
        {

            //This state locks all user input until Esc is pressed
            //then returns CashRegister to the state it was in before the alert

            private static IState state = new EnterTotalAmount();
            private static Number input;
            private const String cashierMsg = PosMessage.TOTAL_AMOUNT;
            private Confirm err = new Confirm(PosMessage.DECIMAL_LIMIT,
                           new StateInstance(DecimalOverflow));

            protected bool quantitySet;

            public static IState Instance()
            {
                input = new Number();
                ((EnterTotalAmount)state).quantitySet = false;
                DisplayAdapter.Cashier.Show(cashierMsg);
                return state;
            }
            public static IState Instance(Decimal amount)
            {
                input = new Number(amount);
                 
                return state;
            }
            public override void Correction()
            {
                if (input.Length > 0)
                {
                    input.RemoveLastDigit();
                    DisplayAdapter.Cashier.BackSpace();
                }
            }
            public override void Numeric(char c)
            {
                if (input.Decimals == 2)
                {
                    //TODO:Virgulden sonro 3 hane girilirse eger kullanİcİnİn uyarİlmasi istenirse hata mesaji icin commentin kaldirilmasi yeterli..
                    //AlertCashier.Instance(err);
                    return;
                }
                else if (input.Length > 19) return;
                input.AppendDecimal(c);
                DisplayAdapter.Cashier.Append(c.ToString());
            }
            public override void Alpha(char c)
            {
                if (char.IsDigit(c))
                    Numeric(c);
                else
                    base.Alpha(c);
            }
            public override void Enter()
            {
            	//TODO: Tutarli etiket satislari
                if (!input.IsEmpty)
                {
                    cr.Item.TotalAmount = input.ToDecimal();
                }
                cr.State = EnterNumber.Instance();
            }

            public override void Escape()
            {
                if (input.IsEmpty)
                {
                    cr.State = States.Start.Instance();
                }
                else
                {
                    cr.State = Instance();
                }

            }
            //public override void BackSpace()
            //{
            //    input.RemoveLastDigit();
            //    DisplayAdapter.Cashier.BackSpace();           	
            //}
            public override void LabelKey(int labelKey)
            {
                //switch (label) 
                //{ 
                //    case Label.BackSpace:
                //        input.RemoveLastDigit();
                //        DisplayAdapter.Cashier.BackSpace();         
                //        break;
                //    case Label.Space:
                //        base.LabelKey(label);
                //        break;
                //    default:
                //        base.LabelKey(label);
                //        break;
                //}      

                Quantity();
                if (input.ToDecimal() > 0) return;

                System.Collections.Generic.List<IProduct> sList = new System.Collections.Generic.List<IProduct>();
                if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.DefineBarcodeLabelKeys) == PosConfiguration.ON)
                    sList = cr.DataConnector.SearchProductByBarcode(Label.GetLabel(labelKey));
                else
                    sList = cr.DataConnector.SearchProductByLabel(Label.GetLabel(labelKey));

                MenuList itemList = new MenuList();

                Exception productEx = null;

                foreach (IProduct p in sList)
                {
                    SalesItem si;
                    try
                    {
                        si = (SalesItem)cr.Item.Clone();
                    }
                    catch (InvalidCastException)
                    {
                        throw new CmdSequenceException();
                    }
                    try
                    {
                        si.Product = p;
                        Decimal la = si.ListedAmount;
                        itemList.Add(si);
                    }
                    catch (Exception ex)
                    {
                        productEx = ex;
                    }
                }

                if (itemList.IsEmpty)
                {
                    if (productEx != null)
                        throw productEx;
                    cr.State = AlertCashier.Instance(new Error(new ProductNotFoundException()));
                    return;
                }


                if (sList.Count == 1 && itemList.Count == 1)
                {
                    if (itemList.MoveNext())
                        cr.Execute(((IDoubleEnumerator)itemList).Current);
                    // cr.State = States.Start.Instance();
                }
                else
                {
                    //The below function passes enumerator to a list of 
                    //Sales items. Each sales item has properties of cr.CurrentItem as well
                    //as the product whose id is held in the label configuration file
                    //Second property tells the state to sell the item once it is selected
                    cr.State = ListLabel.Instance(itemList,
                                                  new ProcessSelectedItem<IProduct>(cr.Execute),
                                                  labelKey);
                }
            }
            public override void Seperator()
            {
                input.AddSeperator();
                DisplayAdapter.Cashier.Append(Number.DecimalSeperator);
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

            public override void Quantity()
            {
                if (quantitySet || input.IsEmpty) return;
                DisplayAdapter.Cashier.Append(" X ");
                try
                {
                    cr.Item.TotalAmount = input.ToDecimal();
                    input.Clear();
                    quantitySet = true;
                }
                catch (ArgumentOutOfRangeException ourex) { throw ourex; }
                catch (Exception ex)
                {
                    cr.State = States.AlertCashier.Instance(new Error(ex));
                    cr.Log.Info("Sales quantity limit exceeded: {0}", input);
                }
            }
        }
    

}
