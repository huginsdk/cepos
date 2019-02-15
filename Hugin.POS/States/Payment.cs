using System;
using cr = Hugin.POS.CashRegister;
using System.Collections.Generic;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
    class Payment : State
    {  
        private static IState state = new Payment();
        public override Error NotImplemented { 
            get { 
                return new Error(new Exception(finalizeSaleErrorStr), 
                                 new StateInstance(Payment.Instance),
                                 new StateInstance(Payment.Instance));
            }   
        }
        protected static Number input;
        protected static PaymentInfo paymentInfo;
        public static PaymentInfo TempPaymentMethod;
        protected static bool paymentEventRegistered = false;
        protected static PromotionDocument promoDocument;
        protected static StateInstance cancelState = Instance;

        protected static String finalizeSaleErrorStr = String.Format("{0}\n{1}", PosMessage.RECEIVE_PAYMENT,
                                                                                 PosMessage.PROMPT_FINALIZE_SALE);
                
        public static IState Instance()
        {
            cr.Printer.CheckPrinterStatus();
            if (cr.Document.BalanceDue > 0 || cr.Document.Payments.Count == 0)
                return Instance(PosMessage.ENTER_AMOUNT);
            else
            {
                Confirm err = new Confirm("ODEME ALINDI\nSATISI KAPAT(GiRiS)",
                                      new StateInstance(Continue),
                                      new StateInstance(Payment.Instance));
                return ConfirmCashier.Instance(err);
            }
        }

        public static IState Continue()
        {
            //In case there is an error after full payment is applied to FPU
            //and document is not closed -  we have to close it here.
            if (cr.Document.BalanceDue == 0 && cr.Document.Payments.Count > 0)
            {
                try
                {
                    cr.Document.Close();
                    return Start.Instance();
                }
                catch (PowerFailureException)
                {
                    cr.Document.Void();
                    return Start.Instance();
                }
                catch (ClearRequiredException)
                {
                    cr.Printer.InterruptReport();
                    cr.Document.Close();
                    return cr.State;
                }
                catch (Exception e)
                {
                    cr.Log.Warning(e);
                    return Instance();
                }
            }

            if (paymentInfo is CreditPaymentInfo)
            {
                CreditPaymentInfo cpi = paymentInfo as CreditPaymentInfo;
                if (cpi.IsPaymentMade)
                {
                    return States.PrintEftPaymentAfterPE.Instance();
                }
            }
            //Otherwise show remaining payment reqired to cashier
            String msg = String.Format("{0}\n{1}\t{2:C}", PosMessage.RECEIVE_PAYMENT,
                                                          PosMessage.BALANCE, new Number(cr.Document.BalanceDue));
            return Instance(msg);
        }
                    
        public static IState Instance(String message)
        {
            if (!paymentEventRegistered)
            {
                SalesDocument.PaymentMade += new PaymentEventHandler(salesDoc_PaymentMade);
                paymentEventRegistered = true;
            }

            input = new Number();

            if (!message.Equals(String.Empty))
            {
                DisplayAdapter.Cashier.Show(message);
            }
            else
            {
                try
                {
                    cr.Printer.PrintSubTotal(cr.Document, false);
                    DisplayAdapter.Customer.Show(String.Format("{0}\n{1:C}", PosMessage.SUBTOTAL, new Number(cr.Document.BalanceDue)));
                }
                catch { }
            }
            return state;
        }

        #region Key Handlers  

        public override void Escape()
        {
            Instance();
        }        
        public override void Numeric(char c)
        {
            if (cr.Document.BalanceDue == 0 && cr.Document.Payments.Count > 0)
            {
                base.Enter();
            }
            else
            {
                if (input.Decimals == 2)
                {
                    //TODO: Simdilik hata uyari mesaji vermiyor. Eger istenirse commentin kaldirilmasi yeterli..
                   // AlertCashier.Instance(err);
                    return;
                }
                if (input.IsEmpty) DisplayAdapter.Cashier.Show(PosMessage.ENTER_AMOUNT);
                DisplayAdapter.Cashier.Append(c.ToString());
                input.AppendDecimal(c);
            }
        }
        public override void Seperator()
        {
            if (input.IsEmpty) DisplayAdapter.Cashier.Show(PosMessage.ENTER_AMOUNT);
            if (input.AddSeperator())
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

        public override void Correction()
        {
            if (input.Length > 0)
            {
                input.RemoveLastDigit();
                DisplayAdapter.Cashier.BackSpace();
            }
        }
        public override void LabelKey(int label)
        {
            if (label == Label.BackSpace)
                switch (input.Length)
                {
                    case 0:
                        break;
                    case 1:
                        Escape();
                        break;
                    default:
                        input.RemoveLastDigit();
                        DisplayAdapter.Cashier.BackSpace();
                        break;
                }
            else base.LabelKey(label);
        }

        public override void SubTotal()
        {
            CashRegister.Document.ShowSubTotal();
        }

        public override void Command()
        {
            if(cr.Document.Payments.Count>0)
            {
                throw new InvalidOperationException();
            }
            else
            {
                MenuList commandMenu = new MenuList();
                int index = 1;
                commandMenu.Clear();
                {
                    if (cr.IsAuthorisedFor(Authorizations.VoidDocument))
                    {
                        commandMenu.Add(CommandMenu.AddLabel(index++, PosMessage.VOID_DOCUMENT));

                        if (cr.Document is Receipt && cr.Printer.CanPrint(cr.Document))
                        {
                            commandMenu.Add(CommandMenu.AddLabel(index++, PosMessage.SUSPEND_DOCUMENT));
                        }
                    }

                }

                cr.State = CommandMenu.Instance(commandMenu);
            }
        }

        public override void Document()
        {
            if (!(cr.Document is Receipt))
            {
                base.Document();
                return;
            }
            MenuList docTypes = new MenuList();
            Start.AddMenuLabel(docTypes, PosMessage.TRANSFER_DOCUMENT + "\n" + PosMessage.INVOICE, new Invoice(cr.Document));

            if (docTypes.Count > 0)
            {
                cr.State = ListDocument.Instance(docTypes, new ProcessSelectedItem<SalesDocument>(cr.ChangeDocumentType));
            }
            else
                cr.State = AlertCashier.Instance(new Confirm("BELGE \n AKTARILAMAZ"));//:to do: do better
        }

        public override void Void()
        {
            if (cr.Document.Adjustments == null || cr.Document.Adjustments.Length == 0 || cr.Document.Payments.Count > 0)
            {
                 base.Void();
                 return;
            }
            cr.Document.UndoAdjustment(true);
            cr.State = Selling.Instance();
        }
        #endregion
        
        #region Payment Related

        //Cleanup static variables
        static void salesDoc_PaymentMade(object sender, PaymentEventArgs args)
        {
            input.Clear();
            if (paymentInfo != null)
                paymentInfo.Amount = 0;
        }
        public override void Pay(CreditPaymentInfo info)
        {
            paymentInfo = info;

            if(input != null && !input.IsEmpty)
                info.Amount = input.ToDecimal();

            if (info.Id == -1)//Credit not assigned
            {
                paymentInfo = info;
                cr.State = ListCreditTypes.Instance(CreditPaymentInfo.GetCredits(),
                                                    new ProcessSelectedItem<CreditPaymentInfo>(Payment.GetCreditInstallments));
            }
            else//Called from K1..K4
            {
                CreditPaymentInfo cInfo = (CreditPaymentInfo)info.Clone();

                if(input != null && !input.IsEmpty)
                    cInfo.Amount = input.ToDecimal();

                input = new Number();
                if (cInfo.Amount >= 0)
                    GetCreditInstallments(cInfo);
                else
                    cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.PAYMENT_INVALID));

            }
        }
        public override void Pay(CurrencyPaymentInfo info)
        {
            paymentInfo = info;
            MenuList tempCurrencies = CurrencyPaymentInfo.GetCurrencies();
            MenuList currencies = new MenuList();
            foreach (CurrencyPaymentInfo cpi in tempCurrencies)
            {
                CurrencyPaymentInfo currency = (CurrencyPaymentInfo)cpi.Clone();
                currency.Amount = input.ToDecimal();
                currencies.Add(currency);
            }
            paymentInfo.Amount = 0;
            input = new Number();
            promoDocument = null;
            cr.State = ListCurrencies.Instance(currencies,
                                     new ProcessSelectedItem<CurrencyPaymentInfo>(PayByForeignCurrency)); 
        }
        public override void Pay(CashPaymentInfo info)
        {
            info.Amount = input.ToDecimal();
            paymentInfo = info;
            cr.State = CalculateTotal(paymentInfo);
        }
        public override void Pay(CheckPaymentInfo info)
        {
            if (input == null) 
                input = new Number(0);
            info.Amount = input.ToDecimal();
            paymentInfo = info;
            cr.State = EnterInteger.Instance(PosMessage.CHECK_ID,
                                             new StateInstance<int>(PayByCheck));
        }

        public static IState ApplyPointDiscount(Decimal discountAmount)
        {
            if (discountAmount==0)
                discountAmount = cr.Document.PointPrices(cr.Document.Customer.Points);

            if (discountAmount > cr.Document.TotalAmount)
                discountAmount = cr.Document.TotalAmount;

            long usingPoint = cr.Document.PriceToPoint(discountAmount * -1);
            if (Math.Abs(usingPoint) > cr.Document.Customer.Points)
                throw new InvalidProgramException("YETERSÝZ PUAN");

            //cr.Document.AddPoint(usingPoint);
            cr.Document.State = DocumentPaying.Instance();

            CreditPaymentInfo cpi = null;

            List<ICredit> creditList = new List<ICredit>(cr.DataConnector.GetCredits().Values);
            ICredit crd = creditList.Find(delegate (ICredit c)
            {
                return c.IsPointPayment;
            });

            if (crd != null)
                cpi = new CreditPaymentInfo(crd, discountAmount);
            else
                throw new InvalidPaymentException();

            state.Pay((CreditPaymentInfo)cpi);

            if (cr.Document.IsEmpty && cr.Document.TotalAmount == Decimal.Zero)
                return States.Start.Instance();

            TempPaymentMethod = new CashPaymentInfo();
            input = new Number();
            //TODO Tell promoserver that the points are used iff document is closed
            //return PayAfterPointDiscount();

            if (cr.Document.TotalAmount == 0m)
                cr.State.Enter();

            return cr.State;
        }

        public static IState PayAfterPointDiscount()
        {
            if (TempPaymentMethod is CreditPaymentInfo)
                state.Pay((CreditPaymentInfo)TempPaymentMethod);
            if (TempPaymentMethod is CurrencyPaymentInfo)
                state.Pay((CurrencyPaymentInfo)TempPaymentMethod);
            if (TempPaymentMethod is CheckPaymentInfo)
                state.Pay((CheckPaymentInfo)TempPaymentMethod);
            if (TempPaymentMethod is CashPaymentInfo)
                state.Pay((CashPaymentInfo)TempPaymentMethod);

            if (cr.Document.TotalAmount == 0m)
                cr.State.Enter();
            return cr.State;
        }

        public override void ShowPaymentList()
        {
            cr.State = PaymentList.Instance(input);
        }
        //Called from EnterNumber and Selling states
        public void Pay(Number input, PaymentInfo info)
        {
            Payment.input = input;
            if (info is CreditPaymentInfo)
                Pay((CreditPaymentInfo)info);
            if (info is CurrencyPaymentInfo)
                Pay((CurrencyPaymentInfo)info);
            if (info is CheckPaymentInfo)
                Pay((CheckPaymentInfo)info);
            if (info is CashPaymentInfo)
                Pay((CashPaymentInfo)info);
        }
        
        //Called from this, EnterNumber and Selling
        public static void GetCreditInstallments(CreditPaymentInfo creditInfo)
        {
            if (creditInfo.Amount==0 &&paymentInfo!= null)
                creditInfo.Amount = paymentInfo.Amount;
          
            paymentInfo = creditInfo;
            if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.PromptCreditInstallments) == PosConfiguration.ON && 
                !creditInfo.Credit.IsTicket &&
                cr.IsAuthorisedFor(Authorizations.InstallOptAuth))
            {
                cr.State = EnterInteger.Instance(PosMessage.INSTALLMENT_COUNT, 0,
                                                 new StateInstance<int>(PayByCredit));
            }
            else
               cr.State= PayByCredit(0);
        }

        public static IState PayByCredit(int installments)
        {
            if (installments < 0) installments = 0;
            if (installments > 99)
                return AlertCashier.Instance(new Confirm("TAKSiT SAYISI\n99'DAN BÜYÜK OLAMAZ"));
            ((CreditPaymentInfo)paymentInfo).Installments = installments;
            try
            {
                return CalculateTotal(paymentInfo);
            }
            catch (Exception e)
            {
                paymentInfo.Amount = 0;
                throw e;
            }
        }
        public static IState PayByCheck(int refNo)
        {
            String checkNo = String.Format("{0}", refNo);
            if (refNo <= 0)
                checkNo = "";
            ((CheckPaymentInfo)paymentInfo).RefNumber = checkNo;
            try
            {
               // return cr.Document.Pay((PaymentInfo)paymentInfo.Clone());
                return CalculateTotal(paymentInfo);
            }
            catch (Exception e)
            {
                paymentInfo.Amount = 0;
                throw e;
            }
        }
        public static void PayByForeignCurrency(CurrencyPaymentInfo currency)
        {
            if (currency.Amount >= 10000000){
                //Should probably be in document.pay
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.CURRENCY_LIMIT_EXCEEDED_PAYMENT_INVALID));
                return;
            }
            currency.Amount = Rounder.RoundDecimal(currency.Amount * currency.ExchangeRate, 2, true);
            
            //cr.State = cr.Document.Pay(currency);
            cr.State = CalculateTotal(currency);
        }
        private static IState CalculateTotal(PaymentInfo pInfo)
        {
            paymentInfo = pInfo;
            Decimal balanceDue = 0;

            if (!(cr.Document.CanEmpty))
            {
                PaymentInfo currentPaymentInfo = paymentInfo.Clone();
                if (currentPaymentInfo.Amount == 0)
                    currentPaymentInfo.Amount = cr.Document.BalanceDue;
                promoDocument = new PromotionDocument(cr.Document, currentPaymentInfo, PromotionType.Document);
                if (promoDocument.HasAdjustment)
                    balanceDue = promoDocument.BalanceDue;
                else
                    balanceDue = cr.Document.BalanceDue;
                if (!(cr.State is PaymentAfterTotalAdjustment))
                    cancelState = States.Start.Instance;
            }
            else
                balanceDue = cr.Document.BalanceDue;

            // Ýf payment is point payment, dont gain point so clear it
            if(paymentInfo is CreditPaymentInfo && ((CreditPaymentInfo)paymentInfo).Credit.IsPointPayment)
            {
                if (promoDocument != null && promoDocument.Points != null && promoDocument.Points.Count > 0)
                    promoDocument.Points.Clear();

                // And if point payment amount bigger than balanceDue, fix it
                if (paymentInfo.Amount > balanceDue)
                {
                    paymentInfo.Amount = balanceDue;
                }
            }

            if (paymentInfo.Amount == 0)
            {
                if (paymentInfo is CurrencyPaymentInfo)
                {
                    decimal dec = Math.Round(balanceDue / ((CurrencyPaymentInfo)paymentInfo).ExchangeRate, 2);
                    paymentInfo.Amount = Math.Round(dec * ((CurrencyPaymentInfo)paymentInfo).ExchangeRate, 2);
                    //paymentInfo.Amount = Math.Truncate(100 * (dec * ((CurrencyPaymentInfo)paymentInfo).ExchangeRate)) / 100;
                }
                else
                {
                    paymentInfo.Amount = balanceDue;
                }
            }

            //TODO This crap should be in HYDisplay
            DisplayAdapter.Customer.Show(PosMessage.SUBTOTAL, balanceDue);

            return cr.State = States.ConfirmPayment.Instance(DisplayAdapter.AmountPairFormat(PosMessage.TOTAL,
                                                                            balanceDue,
                                                                            paymentInfo.ToString(),
                                                                            paymentInfo.Amount),
                                                            new StateInstance<Decimal>(Pay),
                                                            new StateInstance(cancelState));

        }
        public static IState Pay(Decimal amount)
        {
            List<String> promotionRemark = new List<string>();
            if (amount != 0)
            {
                if (paymentInfo is CurrencyPaymentInfo)
                    paymentInfo.Amount = Rounder.RoundDecimal(amount * ((CurrencyPaymentInfo)paymentInfo).ExchangeRate, 2, true);
                else
                    paymentInfo.Amount = amount;
            }
            if (paymentInfo.MinimumPayment > paymentInfo.Amount)
                throw new OverflowException("ÖDEME MÝKTARI\nLÝMÝTÝN ALTINDA");
            if (paymentInfo.MaximumPayment < paymentInfo.Amount)
                throw new OverflowException("ÖDEME MÝKTARI\nLÝMÝTÝN ÜZERÝNDE");

            //Payment on eft pos
            try
            {
                if ((paymentInfo is CreditPaymentInfo) && 
                    (cr.EftPos != null) && 
                    (((CreditPaymentInfo)paymentInfo).Credit.PayViaEft))
                {
                    IEftResponse response = cr.EftPos.Pay(paymentInfo.Amount, ((CreditPaymentInfo)paymentInfo).Installments);
                    if (response.HasError)
                        throw new Exception("Exception occured when payment via EftPos.");
                    ((CreditPaymentInfo)paymentInfo).IsPaymentMade = true;
                    ((CreditPaymentInfo)paymentInfo).Remark = response.CardNumber;
                }
            }
            catch (Exception)
            {
                return cr.State = States.ConfirmPayment.Instance(PosMessage.ACCEPT_PAYMENT_OR_REPEAT_VIA_EFT,
                                                                        new StateInstance<Decimal>(ComplateCreditPayment),
                                                                        new StateInstance(cancelState),
                                                                        new StateInstance<Decimal>(Pay));
            }

            // Add using points after confirm
            if(paymentInfo is CreditPaymentInfo && ((CreditPaymentInfo)paymentInfo).Credit.IsPointPayment)
            {
                long usingPoint = cr.Document.PriceToPoint(paymentInfo.Amount * -1);
                cr.Document.AddPoint(usingPoint);
            }

            // Add promo document 
            if (!cr.Document.CanEmpty)
            {
                cr.Document.Append(promoDocument);
                promoDocument = null;
            }

            if (paymentInfo.Amount >= cr.Document.BalanceDue)
                promoDocument = null;

            if (cr.Document.Status != DocumentStatus.Paying && cr.Document.Remark != null)
            {
                foreach (String remark in cr.Document.Remark)
                    cr.Printer.PrintRemark(remark);
            }

            return cr.Document.Pay(paymentInfo);
        }
        private static IState ComplateCreditPayment(Decimal amount)
        {
            return ComplateCreditPayment();
        }
        public static IState ComplateCreditPayment()
        {
            if (cr.Document.Status != DocumentStatus.Paying)
            {
                //First payment
                cr.Document.Append(promoDocument);
                promoDocument = null;
            }

            if (paymentInfo.Amount >= cr.Document.BalanceDue)
                promoDocument = null;

            if (cr.Document.Status != DocumentStatus.Paying && cr.Document.Remark != null)
            {
                foreach (String remark in cr.Document.Remark)
                    cr.Printer.PrintRemark(remark);
            }

            return cr.Document.Pay(paymentInfo);
        }

        public static void ResetPayment()
        {
            if (paymentInfo != null)
                paymentInfo.Amount = 0;
            TempPaymentMethod = null;
        }

        #endregion    

    }
}
