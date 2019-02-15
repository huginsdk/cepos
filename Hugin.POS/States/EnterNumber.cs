using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
    class EnterNumber : State
    {
        private const int MAX_LENGTH = 19;
        private const int MAX_DECIMALS = 3;
        private const String CASHIER_MESSAGE = PosMessage.ENTER_NUMBER;

        protected static Number input;
        protected bool quantitySet;

        private static IState state = new EnterNumber();
        private Error err = new Error(new Exception(PosMessage.DECIMAL_LIMIT),
                           new StateInstance(DecimalOverflow));
        public static IState Instance()
        {
            input = new Number();
            ((EnterNumber)state).quantitySet = false;
            if (cr.Item.Product != null)
                DisplayAdapter.Cashier.Show("M›KTAR G›R›ﬁ›");
            else
                DisplayAdapter.Cashier.Show(CASHIER_MESSAGE);
            return state;
        }
        public static IState Instance(String message) 
        {
            DisplayAdapter.Cashier.Show(input.ToString());
            return state;
        }
        public override Error NotImplemented { get { input.Clear(); return new Error(new InvalidOperationException()); } }

        public override void Numeric(char c)
        {
            if (input.Decimals == MAX_DECIMALS || input.Length > MAX_LENGTH)
                return;
            input.AppendDecimal(c);
            DisplayAdapter.Cashier.Append(c.ToString());

            if (input.Length == 18 && isQRCode(input.ToString("B")))
            {
                cr.State = States.EnterQRCode.Instance(input.ToString("B"));
            }
        }

        private bool isQRCode(string barcode)
        {
            bool response = false;
            if (barcode.Length == 18 &&
                barcode[0] == '0' &&
                barcode[1] == '1' &&
                barcode[16] == '2' &&
                barcode[17] == '1')
                response = true;

            return response;
        }

        public override void Alpha(char c)
        {
            int i = 0;
            if (Parser.TryInt(c.ToString(), out i))
                Numeric(c);
            else base.Alpha(c);
        }

        public override void Seperator()
        {

            if (input.AddSeperator())
            {
                DisplayAdapter.Cashier.Append(Number.DecimalSeperator);
            }
            else
            {
                cr.State = Calculator.Instance();
                return;
            }
        }
        public override void Enter()
        {
            // if a sale have been selected on grid sets a new quantity
            if (cr.Item.Product != null)
            {
                if(cr.Item.Product.Status != ProductStatus.Weighable && input.ToDecimal() %1 != 0)
                    throw new ProductNotWeighableException();

                cr.UpdateSelectedSaleQuantity(input);
                return;
            }
            IProduct p = null;
            try
            {
                String barcode = input.Length > 1 ? GetSpecial(input.ToString("B").Substring(0, 2)) : "";

                try
                {
                    p = cr.DataConnector.FindProductByBarcode(input.ToString("B"));
                }
                catch { }

                // Gets quantity from Scale if type is weight and printer has scale
                if (cr.Scale != null && p != null && barcode != "")
                {
                    if (BarcodeAdjustment.IsWeightType(input))
                    {
                        input = new Number(cr.Scale.GetWeight(p.UnitPrice));
                        Quantity();
                    }
                }
                if (p == null && barcode != "")
                {
                    quantitySet = false;
                    p = FindSpecialProduct(input);
                    BarcodeAdjustment adjustment = new BarcodeAdjustment(input);
                    switch (adjustment.Type)
                    {
                        case BarcodeType.ByGramma:
                        case BarcodeType.ByQuantity:
                            input = adjustment.Quantity;
                            Quantity();
                            break;
                        case BarcodeType.ByTotalAmount:
                            cr.State = States.EnterTotalAmount.Instance(adjustment.Amount);
                            cr.State.Enter();
                            break;
                        case BarcodeType.ByPrice:
                            cr.State = States.EnterUnitPrice.Instance(adjustment.Price);
                            cr.State.Enter();
                            break;
                    }
                }
                else if (p == null)
                    throw new BarcodeNotFoundException();
            }
            catch (Exception)
            {
                SoundManager.Sound(SoundType.NOT_FOUND);
                cr.Log.Warning("Barcode not found: {0}", input);
                cr.State = AlertCashier.Instance(new Error(new BarcodeNotFoundException()));
                return;
            }       
            cr.Execute(p);
        }

        protected IProduct FindSpecialProduct(Number input)
        {
            String barcode = GetSpecial(input.ToString().Substring(0,2));
            int labelLength = Int32.Parse(barcode.Substring(4,1));
            return cr.DataConnector.FindProductByBarcode(input.ToString().Substring(0, 2 + labelLength));
        }
       
        public override void Escape()
        {
            if (input.IsEmpty)
            {
            	cr.Item.Reset();
                cr.State = States.Start.Instance();
            }
            else
            {
                cr.State = Instance();
            }
        }
        public override void Quantity()
        {
            if (quantitySet || input.IsEmpty) return;
            DisplayAdapter.Cashier.Append(" X ");
            try
            {
                if (cr.Item.Product == null)
                    cr.Item.Quantity = input.ToDecimal();
                input.Clear();
                quantitySet = true;
            }
            catch (ArgumentOutOfRangeException ourex) { throw ourex; }
            catch (InvalidQuantityException iqe) { throw iqe; }
            catch (Exception ex)
            {
                cr.State = States.AlertCashier.Instance(new Error(ex));
                cr.Log.Info("Sales quantity limit exceeded: {0}", input);
            }
        }


        public override void Adjust(AdjustmentType method)
        {
            if (input.ToString().Length < input.Length || Str.Contains(input.ToString(), ','))
            {
                if (method == AdjustmentType.PercentDiscount)
                    method = AdjustmentType.Discount;
                else if (method == AdjustmentType.PercentFee)
                    method = AdjustmentType.Fee;
            }
            if (cr.Document.IsEmpty)
            {
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.NO_SALE_INVALID_ACTION));
                return;
            }
            /*
            //Check whether cashier has authorization or not.
            if (input.ToDecimal() == 100)
            {
                if (method == AdjustmentType.PercentDiscount)
                {
                    input = new Number(cr.Document.State is DocumentOpen ? cr.Document.LastItem.TotalAmount : cr.Document.BalanceDue);
                    method = AdjustmentType.Discount;
                }
                if (method == AdjustmentType.PercentFee)
                {
                    input = new Number(cr.Document.State is DocumentOpen ? cr.Document.LastItem.TotalAmount : cr.Document.BalanceDue);
                    method = AdjustmentType.Fee;
                }
            }
            */
            if (method == AdjustmentType.PercentDiscount || method == AdjustmentType.Discount)
            {
                //check whether cashier has percentDiscount or discount.
                if (!cr.IsAuthorisedFor(Authorizations.Discount))
                {
                    cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.INSUFFICIENT_ACCESS_LEVEL));
                    return;
                }
            }
            else if (method == AdjustmentType.PercentFee || method == AdjustmentType.Fee)
            {
                //check whether cashier has percentFee or fee.
                if (!cr.IsAuthorisedFor(Authorizations.Fee))
                {
                    cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.INSUFFICIENT_ACCESS_LEVEL));
                    return;
                }
            }

            //Deny if percentFee or percentDiscount is not integer.
            if (method == AdjustmentType.PercentDiscount || method == AdjustmentType.PercentFee)
            {
                if (input.Decimals > 0)
                {
                    cr.State = AlertCashier.Instance(new Confirm(PosMessage.DNEY_PERCENTDISCOUNT));
                    return;
                }
                if (input.Length > 2)
                {
                    cr.State = AlertCashier.Instance(new Confirm(PosMessage.DNEY_PERCENT_OVER_AMOUNT));
                    return;
                }
            }

            Adjustment adjustment = null;
            try
            {
                if (cr.Document.State is DocumentOpen)
                {
                    if (cr.Document.LastItem.Adjustments.Count > 0 && 
                        !cr.Document.LastItem.Adjustments[cr.Document.LastItem.Adjustments.Count-1].IsCorrection)
                    {
                        cr.State = States.AlertCashier.Instance(new Confirm("DAHA ÷NCE ‹R‹NE\n›ND/ART YAPILMIﬁ"));
                        return;
                    }
                    if (cr.Document.LastItem is VoidItem)
                    {
                        cr.State = States.AlertCashier.Instance(new Confirm("‹R‹N ›PTAL›NE\n›ND/ART YAPILAMAZ"));
                        return;
                    }
                    if (input.ToDecimal() < 0.01m)
                    {
                        cr.State = States.AlertCashier.Instance(new Confirm("M›N ›ND/ART TUTARI\n0,01"));
                        return;
                    }
                    adjustment = new Adjustment(cr.Document.LastItem, method, input.ToDecimal());
                    if (!(cr.CurrentCashier.IsAuthorisedFor(adjustment) &&
                          cr.Document.CanAdjust(adjustment)))
                    {
                        cr.State = States.AlertCashier.Instance(new Confirm(String.Format("{0} {1}", adjustment.Label, PosMessage.INSUFFICIENT_LIMIT)));
                        return;
                    }

                    if (method == AdjustmentType.PercentFee && input.ToDecimal() > 99)
                    {
                        adjustment = new Adjustment(cr.Document.LastItem, AdjustmentType.Fee, adjustment.NetAmount);
                    }

                    cr.Printer.Print(adjustment);
                    cr.Document.LastItem.Adjust(adjustment); //TODO - if printerresponse is OK
                    DisplayAdapter.Both.Show(adjustment);
                    cr.State = Selling.Instance();
                }

                else if (cr.Document.State is DocumentSubTotal)
                {
                    adjustment = new Adjustment(cr.Document, method, input.ToDecimal());
                    if (!(cr.CurrentCashier.IsAuthorisedFor(adjustment) &&
                          cr.Document.CanAdjust(adjustment)))
                    {
                        cr.State = States.AlertCashier.Instance(new Confirm(String.Format("{0} {1}", adjustment.Label, PosMessage.INSUFFICIENT_LIMIT)));
                        return;
                    }
                    cr.Printer.Print(adjustment);
                    cr.Document.Adjust(adjustment);
                    DisplayAdapter.Both.Show(adjustment);
                    cr.State = States.PaymentAfterTotalAdjustment.Instance(adjustment);
                    cr.Document.State = DocumentPaying.Instance();

                }
            }
            catch (CmdSequenceException)
            {
                String adj = (method == AdjustmentType.Discount ||
                              method == AdjustmentType.PercentDiscount) ? PosMessage.DISCOUNT :
                                                                          PosMessage.FEE;
                cr.State = AlertCashier.Instance(new Confirm(String.Format("ARATOPLAM ONCESINE\n{0} GECERSIZ", adj)));
            }
            catch (ReceiptLimitExceededException)
            {
                //Sales document is a receipt which needs to be 
                //converted to an invoice
                Invoice invoice = new Invoice(cr.Document);
                if (adjustment.Target is SalesItem)
                    invoice.LastItem.Adjust(adjustment);

                if (!cr.Printer.CanPrint(invoice))
                    throw new ReceiptLimitExceededException();

                MenuList docTypes = new MenuList();
                docTypes.Add(new MenuLabel(PosMessage.TRANSFER_DOCUMENT + "\n" + PosMessage.INVOICE, invoice));

                cr.State = States.ListDocument.Instance(docTypes, new ProcessSelectedItem<SalesDocument>(cr.ChangeDocumentType));
                DisplayAdapter.Cashier.Show(PosMessage.RECEIPT_LIMIT_EXCEEDED_TRANSFER_DOCUMENT);


            }
            finally
            {
                input.Clear();
            }
        }

        public override void Void()
        {
            if (cr.Document.Items.Count == 0)
            {
                cr.State = AlertCashier.Instance(new Confirm("SATIﬁ YOK\nIPTAL GE«ERS›Z"));
                return;
            }
            if (cr.Item.TotalAmount > 0)
            {
                cr.Item.TotalAmount = 0;
            }
            //Check Cashier authorizationLvel whether has authorization or not.

            if (!cr.IsAuthorisedFor(Authorizations.VoidSale))
            {
                cr.State = AlertCashier.Instance(new Confirm(PosMessage.INSUFFICIENT_ACCESS_LEVEL));
                return;
            }
            Quantity();
            cr.Item = cr.Item.Void();
            cr.State = VoidSale.Instance();

            // If void sale have already selected on grid
            if (cr.Item.Product != null)
                cr.State.LabelKey(cr.Item.Product.Id - 1);
        }
        public static IState DecimalOverflow() 
        { 
            Number oldValue = new Number(input.ToString());
            Instance();
            foreach(Char c in oldValue.ToString())
            {
                input.AppendDecimal(c);
                DisplayAdapter.Cashier.Append(c.ToString());
            }
            return state;
        }

        public override void ReceiveOnAcct()
        {
            if (input.Decimals == 3)
                input.RemoveLastDigit();
            decimal amount = input.ToDecimal();

            if (amount == 0)
            {
                throw new Exception(PosMessage.ZERO_DRAWER_IN_ERROR);
            }
            DisplayAdapter.Cashier.Show(PosMessage.ENTER_CASH, amount);
            cr.Printer.Deposit(amount);
            cr.DataConnector.OnDeposit(amount);
            cr.State = Start.Instance();
        }

        public override void PayOut()
        {
            if (input.Decimals == 3)
                input.RemoveLastDigit();

            decimal amount = input.ToDecimal();

            if (amount == 0)
            {
                throw new Exception(PosMessage.ZERO_DRAWER_OUT_ERROR);
            }
            if (amount > cr.Printer.CashAmountInDrawer)
            {
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.NEGATIVE_RESULT_EXCEPTION));
                return;
            }

            DisplayAdapter.Cashier.Show(PosMessage.RECEIVE_CASH, -1 * amount);
            cr.Printer.Withdraw(amount);
            cr.DataConnector.OnWithdrawal(amount);
            cr.State = Start.Instance();
        }


        #region functions also implemented in Start and Selling
        public override void PriceLookup()
        {
            Quantity();
            cr.State = EnterString.Instance(PosMessage.PRICE_LOOKUP, new StateInstance<String>(cr.PriceLookup));
        }
        public override void Price()
        {
            if (!input.IsEmpty)
                Quantity();
            cr.State = EnterUnitPrice.Instance();
        }
        public override void LabelKey(int labelKey)
        {
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
                catch(InvalidCastException)
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


            if (sList.Count==1 && itemList.Count == 1)
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
        public override void Repeat()
        {
            Quantity();
            ProductMenuList soldProducts = new ProductMenuList();
            foreach (FiscalItem fi in cr.Document.Items)
                if (fi is SalesItem) soldProducts.Add(fi.Product);
            cr.State = ListProductRepeat.Instance(soldProducts, new ProcessSelectedItem(cr.Execute));
        }
        
        public override void Pay(CreditPaymentInfo info)
        {
            Pay(info);
        }
        public override void Pay(CurrencyPaymentInfo info)
        {
            Pay(info);
        }
        public override void Pay(CheckPaymentInfo info)
        {
            Pay(info);
        }
        public override void Pay(CashPaymentInfo info)
        {
            Pay(info);
        }
        public override void ShowPaymentList()
        {
            cr.State = PaymentList.Instance(input);
        }
        private void Pay(PaymentInfo info)
        {
            if (cr.Document.IsEmpty && !(cr.Document.CanEmpty))
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.NO_SALE_PAYMENT_INVALID));
            else
            {
                cr.State = Payment.Instance("");
                if(input.Decimals == 3)
                    input.RemoveLastDigit();
                ((Payment)cr.State).Pay(input, info);
            }
            input.Clear();
        }       
        #endregion 

        protected String GetSpecial(String key)
        {
            return cr.DataConnector.CurrentSettings.GetSpecialBarcode(key);// SpecialBarcodes.SplitedBarcode.ContainsKey(key);
        }
        private String GetSpecialBarcodeKey(String key)
        {
            String barcode = GetSpecial(key);
            if (barcode != "")
                return barcode.Substring(0, 2);
            return barcode;
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
