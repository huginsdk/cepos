using System;
using System.Text;
using System.Threading;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class EnterString : SilentState
    {
        private static int MAX_LENGTH = 20;
        private const int DEF_MAX_LENGTH = 20;
        
        static IState state = new EnterString();
        static StringBuilder input;
        static String cashierMsg;
        static StateInstance<String> ReturnConfirm;
        static StateInstance ReturnCancel;
        Thread t = new Thread(new ThreadStart(AdvanceCursor));
        static byte sequence = 0;
        static bool canAdvance = true;
        static readonly TimeSpan doublePressInterval = new TimeSpan((long)(Decimal.Parse(PosConfiguration.Get("DoublePressInterval")) * 10000000m));

        public static IState Instance(String message, StateInstance<String> ConfirmState)
        {
            return Instance(message, ConfirmState, null);
        }

        public static IState Instance(String message, StateInstance<String> ConfirmState, StateInstance CancelState)
        {
            input = new StringBuilder();
            KeyMap.LastOrder = 0;
            sequence = 0;
            cashierMsg = message;
            DisplayAdapter.Cashier.Show(cashierMsg);
            //TODO: Cursor satirbasi olmali - overwrite mode
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;
            return state;
        }

        public static IState Instance(String message, String defaultValue, StateInstance<String> ConfirmState, StateInstance CancelState)
        {
            IState returnState = Instance(message, ConfirmState, CancelState);
            input.Append(defaultValue);
            //DisplayAdapter.Cashier.Show(defaultValue);
            DisplayAdapter.Cashier.Show("{0}\n{1}\t", message, defaultValue);

            return returnState;
        }

        #region KeyHandlers

        public override void Numeric(char c)
        {
            Append(c, true);
            KeyMap.LastOrder = 0;
            sequence = 0;
        }

        public override void Escape()
        {
            if (t != null || t.ThreadState == ThreadState.WaitSleepJoin)
            {
                if (t.ThreadState == ThreadState.Unstarted)
                {
                    t.Start();
                }
                t.Interrupt();
                t.Join();
            }
            if (input.Length > 0)
            {
                input.Length = 0;
                KeyMap.LastOrder = 0;
                sequence = 0;
                DisplayAdapter.Cashier.Show("{0}", cashierMsg);
                return;
            }
            if (ReturnCancel == null)
                cr.State = States.Start.Instance();
            else
            {
                try
                {
                    cr.State = ReturnCancel();
                }
                catch
                {
                    cr.Log.Error("Cannot find instance method of state");
                }
            }

        }

        public override void Enter()
        {
            if (t != null || t.ThreadState == ThreadState.WaitSleepJoin)
            {
                if (t.ThreadState == ThreadState.Unstarted)
                {
                    t.Start();
                }
                t.Interrupt();
                t.Join();
            }
            if (ReturnConfirm != null)
            {
                try
                {
                    cr.State = ReturnConfirm(input.ToString().Trim());
                }
                catch (Exception ex)
                {
                    cr.Log.Warning(ex);
                    cr.State = AlertCashier.Instance(new Error(ex,
                                                                ReturnCancel,
                                                                ReturnCancel));
                }
            }

        }

        public override void Seperator()
        {
            WriteChar(0, PosKey.Decimal);
        }
        public override void Alpha(char c)
        {
            Append(c, true);
        }
        public override void Document()
        {
            WriteChar(0, PosKey.Document);
        }
        public override void Customer()
        {
            WriteChar(0, PosKey.Customer);
        }
        public override void Report()
        {
            WriteChar(0, PosKey.Report);
        }
        public override void Program()
        {
            WriteChar(0, PosKey.Program);
        }
        public override void Command()
        {
            WriteChar(0, PosKey.Command);
        }
        public override void SalesPerson()
        {
            WriteChar(0, PosKey.SalesPerson);
        }
        public override void ReceiveOnAcct()
        {
            WriteChar(0, PosKey.ReceiveOnAcct);
        }
        public override void PayOut()
        {
            WriteChar(0, PosKey.PayOut);
        }
        public override void Void()
        {
            WriteChar(0, PosKey.Void);
        }
        public override void SendOrder()
        {
            WriteChar(0, PosKey.SendOrder);
        }
        public override void Adjust(AdjustmentType method)
        {
            switch (method)
            {
                case AdjustmentType.Discount:
                    WriteChar(0, PosKey.Discount); break;
                case AdjustmentType.Fee:
                    WriteChar(0, PosKey.Fee); break;
                case AdjustmentType.PercentDiscount:
                    WriteChar(0, PosKey.PercentDiscount); break;
                case AdjustmentType.PercentFee:
                    WriteChar(0, PosKey.PercentFee); break;
                default: break;

            }
        }
        public override void PriceLookup()
        {
            WriteChar(0, PosKey.PriceLookup);
        }
        public override void Price()
        {
            WriteChar(0, PosKey.Price);
        }
        public override void TotalAmount()
        {
            WriteChar(0, PosKey.Total);
        }
        public override void Repeat()
        {
            WriteChar(0, PosKey.Repeat);
        }
        public override void Pay(CreditPaymentInfo info)
        {
            int id = info.Id < 0 ? 0 : info.Id;
            WriteChar(id, PosKey.Credit);
        }
        public override void Pay(CurrencyPaymentInfo info)
        {

            WriteChar(0, PosKey.ForeignCurrency);
        }
        public override void Pay(CheckPaymentInfo info)
        {
            WriteChar(0, PosKey.Check);
        }
        public override void Pay(CashPaymentInfo info)
        {
            WriteChar(0, PosKey.Cash);
        }

        public override void SubTotal()
        {
            WriteChar(0, PosKey.SubTotal);
        }
        public override void DownArrow()
        {
            if (DisplayAdapter.Cashier.CurrentColumn < input.Length)
                DisplayAdapter.Cashier.CursorNext();
        }
        public override void UpArrow()
        {
            if (DisplayAdapter.Cashier.CurrentColumn > 0)
                DisplayAdapter.Cashier.CursorPrevious();
        }
        public override void LabelKey(int label)
        {
            WriteChar(label, PosKey.LabelStx);

        }

        private void WriteChar(int order, PosKey key)
        {
            if (order == KeyMap.LastOrder && KeyMap.LastPosKey == key)
            {
                canAdvance = false;
                sequence++;
            }
            else
            {
                canAdvance = true;
                sequence = 0;
            }
            ThreadState tState = t.ThreadState;
            if (tState == ThreadState.Unstarted || tState == ThreadState.Stopped)
                sequence = 0;
            char c = KeyMap.GetLetter(key, order, ref sequence);

            switch (c)
            {
                case '\b':
                    if (input.Length == 0) return;
                    if (DisplayAdapter.Cashier.CurrentColumn == 0) return;
                    if (t != null || t.ThreadState == ThreadState.WaitSleepJoin)
                    {
                        if (t.ThreadState != ThreadState.Unstarted)
                        {
                            t.Interrupt();
                            t.Join();
                        }
                    }
                    if (input.Length >= DisplayAdapter.Cashier.CurrentColumn)
                        input.Remove(DisplayAdapter.Cashier.CurrentColumn - 1, 1);
                    DisplayAdapter.Cashier.BackSpace();
                    break;
                case ' ':
                    Numeric(' ');
                    break;
                case '\n':
                    Enter();
                    break;
                default:
                    try
                    {
                        if (DisplayAdapter.Cashier.HasAttribute(DisplayAttribute.TouchKeyboard))
                        {
                            Append(c, true);
                            return;
                        }
                        if (tState == ThreadState.Unstarted)
                        {
                            Append(c, false);
                            t.Start();
                            return;
                        }
                        else if (tState == ThreadState.Stopped)
                        {
                            canAdvance = true;
                            sequence = 0;
                            t = new Thread(new ThreadStart(AdvanceCursor));
                            t.Start();
                            Append(c, false);
                            return;
                        }
                        else if (tState == ThreadState.WaitSleepJoin)
                        {
                            t.Interrupt();
                            t.Join(doublePressInterval);
                            t = new Thread(new ThreadStart(AdvanceCursor));
                            t.Start();
                            Append(c, false);
                        }

                    }
                    catch (ThreadStateException)
                    {
                        //Console.WriteLine("Threadstate Exception");
                    }
                    break;
            }
        }
        #endregion

        void Append(char c, bool moveCursor)
        {

            if (!DisplayAdapter.Cashier.IsPaused && DisplayAdapter.Cashier.CurrentColumn >= MAX_LENGTH) return;
            if (moveCursor && !DisplayAdapter.Cashier.HasAttribute(DisplayAttribute.TouchKeyboard))
                if (t.ThreadState == ThreadState.WaitSleepJoin)
                {
                    canAdvance = true;
                    t.Interrupt();
                    t.Join(1000);

                }

            if (!DisplayAdapter.Cashier.IsPaused && DisplayAdapter.Cashier.CurrentColumn != input.Length)
            {
                Insert(c, moveCursor); return;
            }
            input.Append(c, 1);
            DisplayAdapter.Cashier.Append(c.ToString(), moveCursor);
        }
        void Insert(char c, bool moveCursor)
        {
            if (c == ' ')
                if (input.Length < MAX_LENGTH)
                    input.Insert(DisplayAdapter.Cashier.CurrentColumn, c + "");
                else
                    return;
            else
                input[DisplayAdapter.Cashier.CurrentColumn] = c;
            DisplayAdapter.Cashier.Append(c.ToString(), moveCursor);
        }
        static void AdvanceCursor()
        {
            DateTime dt = DateTime.Now;
            try
            {
                Thread.Sleep(doublePressInterval);
                DisplayAdapter.Cashier.CursorNext();
                sequence = 0;
            }
            catch (ThreadInterruptedException)
            {
                if (canAdvance && input.Length > 0)
                {
                    //TimeSpan ts = DateTime.Now - dt;
                    //Console.WriteLine("CursorNext from timer in {0}", ts.TotalMilliseconds);
                    DisplayAdapter.Cashier.CursorNext();
                    sequence = 0;
                }
                else
                {
                    //Console.WriteLine("Thread cannot advance");
                    if (DisplayAdapter.Cashier.CurrentColumn > MAX_LENGTH)
                        input.Remove(DisplayAdapter.Cashier.CurrentColumn, 1);
                    canAdvance = true;
                }
            }

        }
        public override void CardPrefix()
        {
            cr.State = EnterCardNumber.Instance(PosMessage.ENTER_CADR_CODE,
                                                    new StateInstance<string>(CustomerInfo.SetCurrentCustomer),
                                                    new StateInstance(CustomerInfo.Instance));
        }
        public static IState SellAlphanumericBarcode(String barcode)
        {
            IProduct p = null;

            try
            {
                p = cr.DataConnector.FindProductByBarcode(barcode);

            }
            catch (Exception)
            {
                cr.Log.Warning("Barcode not found: {0}", input);
                return AlertCashier.Instance(new Error(new BarcodeNotFoundException()));
            }
            cr.Execute(p);
            return cr.State;

        }
        public override void Correction()
        {
            if (input.Length > 0)
            {
                if (DisplayAdapter.Cashier.CurrentColumn > 0)
                {
                    input.Remove(DisplayAdapter.Cashier.CurrentColumn - 1, 1);
                    DisplayAdapter.Cashier.BackSpace();
                }
            }

        }

        internal static void SetMaxLength(int length)
        {
            MAX_LENGTH = length;
        }

        public override void OnEntry()
        {
            try
            {
                if (DisplayAdapter.Cashier.HasAttribute(DisplayAttribute.TouchKeyboard))
                    DisplayAdapter.Cashier.Show(null as ICustomer);
            }
            catch { }
            base.OnEntry();

        }
        public override void OnExit()
        {
            try
            {
                if (DisplayAdapter.Cashier.HasAttribute(DisplayAttribute.TouchKeyboard))
                    DisplayAdapter.Cashier.Show(null as ICredit);
            }
            catch { }
            if (MAX_LENGTH != DEF_MAX_LENGTH)
                MAX_LENGTH = DEF_MAX_LENGTH;
            base.OnExit();
        }

    }
}
