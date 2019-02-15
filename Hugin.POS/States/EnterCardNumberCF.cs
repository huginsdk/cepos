using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class EnterCardNumber : SilentState
    {
        /// <summary>
        /// Global definitions.
        /// </summary>
        static IState state = new EnterCardNumber();
        static StringBuilder input;
        static String cashierMsg;
        static StateInstance<String> ReturnConfirm;
        static StateInstance ReturnCancel;
        Thread t = new Thread(new ThreadStart(AdvanceCursor));
        static byte sequence = 0;
        static bool canAdvance = true;
        static bool readStop = false;
        static TimeSpan doublePressInterval = new TimeSpan((long)(Decimal.Parse(PosConfiguration.Get("DoublePressInterval")) * 10000000m));

        /// <summary>
        /// Instance of EnterCardNumber
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>
        /// <returns>
        /// EnterCardNumber state.
        /// </returns>
        public static IState Instance(String message, StateInstance<String> ConfirmState)
        {
            return Instance(message, ConfirmState, null);
        }
        /// <summary>
        /// Insctance of EnterCardNumber.
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>        
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>
        /// <param name="CancelState">
        /// CancelState:what state the machine should assume after user hits Escape key.
        /// </param>
        /// <returns>
        /// EnterCardNumber state.
        /// </returns>
        public static IState Instance(String message, StateInstance<String> ConfirmState, StateInstance CancelState)
        {
            input = new StringBuilder();
            Label.LastKey = 0;
            sequence = 0;
            cashierMsg = message;
            DisplayAdapter.Cashier.Show("{0}", cashierMsg);
            //TODO: Cursor satirbasi olmali - overwrite mode
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;
            return state;
        }
        /// <summary>
        /// Insctance of EnterCardNumber.
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>
        /// <param name="defaultValue">
        /// Default value of input.
        /// </param>
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>
        /// <param name="CancelState">
        /// CancelState:what state the machine should assume after user hits Escape key.
        /// </param>
        /// <returns>
        /// EnterCardNumber state.
        /// </returns>
        public static IState Instance(String message, String defaultValue, StateInstance<String> ConfirmState, StateInstance CancelState)
        {
            input = new StringBuilder(defaultValue);
            Label.LastKey = 0;
            sequence = 0;
            cashierMsg = message;
            DisplayAdapter.Cashier.Show("{0}\n{1}\t", cashierMsg, input.ToString());
            //TODO: Cursor satirbasi olmali - overwrite mode
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;
            return state;
        }
        #region KeyHandlers
        /// <summary>
        /// Numeric and some label key function.
        /// </summary>
        /// <param name="c"></param>
        public override void Numeric(char c)
        {
            Append(c, true);
            Label.LastKey = 0;
            sequence = 0;
        }
        /// <summary>
        /// Escape key function
        /// </summary>
        public override void Escape()
        {
            
            if (input.Length > 0)
            {
                input.Length = 0;
                Label.LastKey = 0;
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
            try
            {
                if (t != null)
                    t.Abort();
            }
            catch { }
            if (ReturnConfirm != null)
            {
                try
                {
                    System.Threading.Thread.Sleep(1000);

                    if (!(cr.Document is Receipt))
                    {
                        AlertCashier.Instance(new Confirm(PosMessage.CUSTOMER_NOT_BE_CHANGED_IN_DOCUMENT));
                        return;
                    }

                    if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.AssingCustomerInSelling) == PosConfiguration.OFF)
                    {
                        if (cr.Document.Items.Count > 0 && cr.Document.Id > 0)
                        {
                            readStop = false;
                            cr.State = AlertCashier.Instance(new Confirm(PosMessage.CUSTOMER_NOT_BE_CHANGED_IN_DOCUMENT,
                                new StateInstance(States.Start.Instance),
                                new StateInstance(States.Start.Instance)));
                            return;
                        }
                    }
                    cr.State = ReturnConfirm(input.ToString());
                }
                catch
                {
                    cr.State = AlertCashier.Instance(new Error(new InvalidOperationException(),
                                                                ReturnCancel,
                                                                ReturnCancel));
                }
            }
            readStop = false;
        }
        /// <summary>
        /// Enter key function
        /// </summary>

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
        public override void Void()
        {
            WriteChar(0, PosKey.Void);
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
        public override void LabelKey(int label)
        {
            WriteChar(label, PosKey.LabelStx);

        }
        private void OnBackSpace()
        {
            if (input.Length == 0) return;
            if (DisplayAdapter.Cashier.CurrentColumn == 0) return;
            if (input.Length >= DisplayAdapter.Cashier.CurrentColumn)
                input.Remove(DisplayAdapter.Cashier.CurrentColumn - 1, 1);
            DisplayAdapter.Cashier.BackSpace();
        }
        public override void Correction()
        {
            OnBackSpace();
        }
        private bool IsTouchDisplay
        {
            get
            {
                return (DisplayAdapter.Cashier.HasAttribute(DisplayAttribute.TouchKeyboard));
            }
        }

        private void WriteChar(int order, PosKey key)
        {
            if (order == KeyMap.LastOrder && KeyMap.LastPosKey == key)
            {
                canAdvance = false;
                sequence++;
            }
            else
                canAdvance = true;

            char c = KeyMap.GetLetter(key, order, ref sequence);
            switch (c)
            {
                case '\b':
                    if (IsTouchDisplay) return;
                    OnBackSpace();
                    break;
                case ' ':
                    if (IsTouchDisplay) return;
                    Numeric(' ');
                    break;
                case '\n':
                    if (IsTouchDisplay) return;
                    Enter();
                    break;
                default:
                    try
                    {
                        Append(c, false);

                    }
                    catch (ThreadStateException)
                    {
                        //Console.WriteLine("Threadstate Exception");
                    }
                    break;
            }

        }
        #endregion
        /// <summary>
        /// Append one char to current displayer.
        /// </summary>
        /// <param name="c">
        /// Char that will be appended.
        /// </param>
        /// <param name="moveCursor">
        /// Specify whether current displayer move next or not.
        /// </param>
        void Append(char c, bool moveCursor)
        {
            
            if (input.Length == 17) { readStop = true; input = new StringBuilder((input.ToString().Substring(0, 16))); }
            if (!readStop)
            {
                input.Append(c, 1);
                if (input.Length >= 1)
                    DisplayAdapter.Cashier.Append(c.ToString(), moveCursor);
            }

        }
        /// <summary>
        /// Function that adjust cursor position.
        /// </summary>
        static void AdvanceCursor()
        {
            DateTime dt = DateTime.Now;
            try
            {
                Thread.Sleep(doublePressInterval.Milliseconds);
                DisplayAdapter.Cashier.CursorNext();
                sequence = 0;
            }
            catch (ThreadAbortException)
            {
                if (canAdvance && input.Length > 0)
                {
                    DisplayAdapter.Cashier.CursorNext();
                    sequence = 0;
                }
                else
                {
                    if (input.Length > 0)
                        input.Remove(input.Length - 1, 1);
                    canAdvance = true;
                }
            }

        }
    }
}
