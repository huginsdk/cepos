using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
    class Selling : Start
    {
        private static IState state = new Selling();

        public override bool IsIdle
        {
            get
            {
                return false;
            }
        }

        public static new IState Instance()
        {
            DisplayAdapter.Cashier.LedOn(Leds.Sale);
            return state;
        }
        public static new IState Instance(String msg)
        {
            DisplayAdapter.Cashier.Show("{0}", msg);
            return Instance();
        }
        public override void Escape()
        {
            cr.Item.Reset();
            cr.State = Start.Instance();
        }
        public override void Repeat()
        {
            ProductMenuList soldProducts = new ProductMenuList();
            foreach (FiscalItem fi in cr.Document.Items)
                if (fi is SalesItem) soldProducts.Add(fi.Product);
            cr.State = ListProductRepeat.Instance(soldProducts, new ProcessSelectedItem<IProduct>(cr.Execute));
        }
        public override void Customer()
        {
            if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.AssingCustomerInSelling) == PosConfiguration.OFF)
            {
                AlertCashier.Instance(new Confirm(PosMessage.CUSTOMER_NOT_BE_CHANGED_IN_DOCUMENT));
                return;
            }

            if (cr.Document.Items.Count > 0 && cr.Document.Id > 0)
            {
                if (!(cr.Document is Receipt))
                {
                    AlertCashier.Instance(new Confirm(PosMessage.CUSTOMER_NOT_BE_CHANGED_IN_DOCUMENT));
                    return;
                }
                else
                {
                    if (cr.Document.Customer != null)
                    {
                        String msg = String.Format("{0}\n{1}", cr.Document.Customer.Name, PosMessage.CONFIRM_VOID_CURRENT_CUSTOMER);
                        cr.State = ConfirmCashier.Instance(new Confirm(msg, CustomerInfo.ConfirmVoidCustomer));
                    }
                    else
                    {
                        Confirm e = new Confirm(PosMessage.CONFIRM_TRANSFER_CUSTOMER_TO_RECEIPT,
                                        new StateInstance(ChangeConfirmed),
                                        new StateInstance(Start.Instance));

                        cr.State = ConfirmCashier.Instance(e);
                    }
                }
            }
            else
                base.Customer();
        }


        internal static IState ChangeConfirmed()
        {
            if (cr.Document.Customer != null)
                cr.State = States.Start.Instance();
            else
                cr.State = CustomerInfo.Instance();

            return cr.State;
        }

        public override void Document()
        {
            if (!(cr.Document is Receipt))
            {
                States.AlertCashier.Instance(new Confirm(cr.Document.Name + PosMessage.DOCUMENT_CHANGE_ERROR));
                cr.State = States.Start.Instance();
                return;
            }

            MenuList docTypes = new MenuList();
            Start.AddMenuLabel(docTypes, PosMessage.TRANSFER_DOCUMENT + "\n" + PosMessage.INVOICE, new Invoice(cr.Document));

            if (docTypes.Count > 0)
            {
                cr.State = ListDocument.Instance(docTypes, new ProcessSelectedItem<SalesDocument>(cr.ChangeDocumentType));
                DisplayAdapter.Customer.Clear();
            }
            else
                cr.State = AlertCashier.Instance(new Confirm(PosMessage.DOCUMENT_NOT_BE_TRANSFERRED));//:to do: do better
        }

        public override void SubTotal()
        {
            cr.Document.ShowSubTotal();
        }
        public override void SalesPerson()
        {

            //Check whether cashier has authorization to assign salesPerson


            if (cr.Document.State is DocumentSubTotal)
            {
                if (cr.Document.SalesPerson == null)
                    cr.State = EnterClerkNumber.Instance(PosMessage.CLERK_ID,
                                                new StateInstance<ICashier>(cr.Document.ConfirmSalesPerson),
                                                new StateInstance(Start.Instance));
                else
                {
                    String prompt = String.Format("{0}\n{1}", cr.Document.SalesPerson.Name.TrimEnd(), PosMessage.VOID_SALESPERSON);
                    Confirm confirmVoidSalesPerson = new Confirm(prompt,
                                                             new StateInstance(cr.Document.VoidSalesPerson),
                                                             new StateInstance(Start.Instance));
                    cr.State = ConfirmCashier.Instance(confirmVoidSalesPerson);
                }
            }
            else
            {

                if (cr.Document.LastItem.SalesPerson == null)
                    cr.State = EnterClerkNumber.Instance(PosMessage.CLERK_ID,
                                                new StateInstance<ICashier>(cr.Document.LastItem.ConfirmSalesPerson),
                                                new StateInstance(Start.Instance));
                else
                {
                    String prompt = String.Format("{0}\n{1}", cr.Document.LastItem.SalesPerson.Name.TrimEnd(), PosMessage.VOID_SALESPERSON);
                    Confirm confirmVoidSalesPerson = new Confirm(prompt,
                                                             new StateInstance(cr.Document.LastItem.VoidSalesPerson),
                                                             new StateInstance(Start.Instance));
                    cr.State = ConfirmCashier.Instance(confirmVoidSalesPerson);
                }
            }
        }
        public override void Command()
        {
            MenuList commandMenu = new MenuList();
            commandMenu.Clear();
          
            int index = 1;
            if (cr.IsAuthorisedFor(Authorizations.VoidDocument) ||
                    !DisplayAdapter.Both.HasAttribute(DisplayAttribute.CashierKey))
            {
                commandMenu.Add(CommandMenu.AddLabel(index++, PosMessage.VOID_DOCUMENT));

                if (cr.Document is Receipt && cr.Printer.CanPrint(cr.Document))
                {
                    commandMenu.Add(CommandMenu.AddLabel(index++, PosMessage.SUSPEND_DOCUMENT));
                }
            }
            if ((cr.Document is Receipt) && cr.Printer.CanPrint(new Invoice()))
            {
                commandMenu.Add(CommandMenu.AddLabel(index++, PosMessage.TRANSFER_DOCUMENT));
            }
            
            if(cr.DataConnector.CurrentSettings.GetProgramOption(Setting.AssingCustomerInSelling) == PosConfiguration.ON)
            {
                commandMenu.Add(CommandMenu.AddLabel(index++, PosMessage.CUSTOMER_ENTRY));
            }

            cr.State = CommandMenu.Instance(commandMenu);
        }
        public override void Pay(CreditPaymentInfo info) 
        {
            cr.State = Payment.Instance(String.Empty);
            cr.State.Pay(info);
        }
        public override void Pay(CurrencyPaymentInfo info)
        {
            cr.State = Payment.Instance(String.Empty);
            cr.State.Pay(info);
        }
        public override void Pay(CheckPaymentInfo info)
        {
            if (cr.Document.Customer != null &&
                   cr.Document.Customer.Points > 0 &&
                   cr.Document.Adjustments.Length == 0 &&
                   !(cr.Document is ReturnDocument)
               )
            {
                Payment.TempPaymentMethod = info;
                decimal customerPointsPrice = cr.Document.PointPrices(cr.Document.Customer.Points);
                cr.State = EnterDecimal.Instance(String.Format("({0} PUAN)\t{1:N2}", cr.Document.Customer.Points, customerPointsPrice),
                                             new StateInstance<Decimal>(Payment.ApplyPointDiscount),
                                             new StateInstance(Payment.PayAfterPointDiscount));
                return;
            }
            cr.State = Payment.Instance(String.Empty);
            cr.State.Pay(info);
        }
        public override void Pay(CashPaymentInfo info)
        {
            cr.State = Payment.Instance(String.Empty);
            cr.State.Pay(info);
        }

        public override void SendOrder()
        {
            cr.State = States.ConfirmVoid.Instance(new Confirm(String.Format("{0}\t{1}\n{2}", PosMessage.ORDER_TR, cr.Document.TotalAmount, PosMessage.CONFIRM_SEND_ORDER), new StateInstance(CloseOrder)));
        }

        public static IState CloseOrder()
        {
            try
            {
                cr.Document.CloseOrder();
            }
            catch (OrderServerNoMatcedDevice)
            {
                return States.AlertCashier.Instance(new Confirm(PosMessage.NO_MATCHED_EFT_POS, States.Start.Instance, States.Start.Instance));
            }
            catch (AnyConnectedEftPosException)
            {
                return States.AlertCashier.Instance(new Confirm(PosMessage.ANY_CONNECTED_EFT_POS, States.Start.Instance, States.Start.Instance));
            }
            catch (TimeoutException)
            {
                return States.ConfirmCashier.Instance(new Confirm(PosMessage.TIMEOUT_EX_SEND_AGAIN, new StateInstance(CloseOrder), States.Start.Instance));
            }

            if (cr.Document.Status == DocumentStatus.Voided)
            {
                cr.Document.Void();
                return States.AlertCashier.Instance(new Confirm(PosMessage.DOCUMENT_VOIDED_BY_EFT_POS, States.Start.Instance, States.Start.Instance));
            }

            return States.AlertCashier.Instance(new Confirm(PosMessage.PAID_IS_DONE_TY, States.Start.Instance, States.Start.Instance));
        }

        public override void ShowPaymentList()
        {
            cr.State = PaymentList.Instance(new Number());
        }

        public static IState VoidSale()
        {
            DisplayAdapter.Cashier.Pause();
            cr.State = EnterNumber.Instance();
            cr.State.Numeric('1');
            DisplayAdapter.Cashier.Play();
            cr.State.Void();
            return cr.State;         
        }

        public static IState Continue()
        {
            return Start.Instance() ;
        }

        public override void Void()
        {
            /*Check cashier authorization level whether has authority or not.*/

            if (!cr.IsAuthorisedFor(Authorizations.VoidSale))
            {
                 cr.State = ConfirmAuthorization.Instance(VoidSale, Continue, Authorizations.VoidSale);
            }
            else
            {
                //do as 1X[IPTAL]
                DisplayAdapter.Cashier.Pause();
                cr.State = EnterNumber.Instance();
                cr.State.Numeric('1');
                DisplayAdapter.Cashier.Play();
                cr.State.Void();
            };
        }

        public static IState VoidAdjustedItem(Hashtable data)
        {
            /* when the cancel menu(key) selected with quantity
             * search the sale list to find items 
             * whose product is equal to product to be cancel and
             * whose remaing quantity is equal to at least quantity to be cancel and
             * whose unit price has not been found yet in other words cancel menu does not contains the item with same unit price
             * define the quantity and amount of item to be cancel and add the cancel menu
             */
            IProduct adjustedPoduct = (IProduct)data["Product"];
            Decimal quantity = (Decimal)data["Quantity"];

            /* unitprices contains the information of how much item was sold with the corresponding unitprice
             * when an item is added to cancel menu relating data is removed 
             * so no item with same price could be add to list
             */
            System.Collections.Generic.Dictionary<decimal, decimal> unitprices
                = (System.Collections.Generic.Dictionary<decimal, decimal>)data["UnitPrices"];

            /* Cancel menu contains the items with 
             * quantity to be cancel
             * item name
             * unit price * quantity to be cancel
             */
            MenuList cancelMenu = new MenuList();

            foreach (FiscalItem fiscalItem in cr.Document.Items)
                if (fiscalItem is SalesItem && fiscalItem.Product.Id == adjustedPoduct.Id && unitprices.ContainsKey(fiscalItem.UnitPrice))
                {
                    if (unitprices[fiscalItem.UnitPrice]>=quantity)
                    {
                        SalesItem temp = (SalesItem)(fiscalItem.Clone());
                        temp.Quantity = quantity;
                        temp.TotalAmount = quantity * temp.UnitPrice;
                        temp.VoidAmount = 0;
                        temp.VoidQuantity = 0;

                        unitprices.Remove(fiscalItem.UnitPrice);
                        cancelMenu.Add(temp);
                    }
                }
            
            return ListVoid.Instance(cancelMenu, new ProcessSelectedItem<FiscalItem>(VoidSelectedItem));
        }

        internal static void VoidSelectedItem(FiscalItem fi)
        {
            cr.Execute(new VoidItem(fi));
        }

        public override void Correction()
        {
            if (cr.Document.LastItem == null)
                throw new NoCorrectionException();

            try
            {
                if (cr.Document.LastItem.Adjustments == null || cr.Document.LastItem.Adjustments.Count == 0)
                {

                    if(cr.Document.LastItem is VoidItem)
                        throw new CmdSequenceException();
                    
                    cr.Printer.Correct(cr.Document.LastItem);
                    FiscalItem voidItem = new VoidItem(cr.Document.LastItem);
                    ((SalesItem)cr.Document.LastItem).VoidQuantity = cr.Document.LastItem.Quantity;
                    ((SalesItem)cr.Document.LastItem).VoidAmount = cr.Document.LastItem.TotalAmount;
                    cr.Document.AddItem(voidItem,true);
                    DisplayAdapter.Both.ShowVoid(voidItem);
                    return;
                }

                FiscalItem fi = cr.Document.LastItem;

                Adjustment lastAdjustment = fi.Adjustments[fi.Adjustments.Count - 1];
                if (lastAdjustment.IsCorrection)
                    throw new CmdSequenceException();

                Adjustment adjustment = null;
                AdjustmentType adjType = lastAdjustment.NetAmount > 0 ? AdjustmentType.Discount : AdjustmentType.Fee;
                if (cr.Document.State is DocumentOpen)
                {
                    adjustment = new Adjustment(cr.Document.LastItem, adjType, Math.Abs(lastAdjustment.NetAmount));
                    adjustment.IsCorrection = true;

                    if (!(cr.Document.CanAdjust(adjustment)))
                    {
                        cr.State = States.AlertCashier.Instance(new Error(new AdjustmentLimitException(adjustment.Label)));
                        return;
                    }

                    IPrinterResponse printerReponse = cr.Printer.Correct(lastAdjustment);

                    cr.Document.LastItem.Adjust(adjustment);

                    DisplayAdapter.Both.ShowCorrect(adjustment, false);

                    cr.State = Selling.Instance();
                }
                if (cr.Document.State is DocumentSubTotal)
                    throw new CmdSequenceException();
            }
            catch (CmdSequenceException)
            {
                cr.State = AlertCashier.Instance(new Error(new NoCorrectionException()));
            }
            catch (NoAdjustmentException ae)
            {
                AlertCashier.Instance(new Error(ae));
            }

        }
   
        #region disable start functions
        public override void Report()
        {
            cr.State = AlertCashier.Instance(new Confirm(PosMessage.SALES_EXIST_REPORT_NOT_ALLOWED));
        }

        public override void Program()
        {
            cr.State = AlertCashier.Instance(new Confirm(PosMessage.SALES_EXIST_COMMANDMENU_NOT_ALLOWED));
        }
        #endregion

    }
}
