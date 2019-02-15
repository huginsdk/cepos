using System;
using System.Text;
using System.Configuration;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using Hugin.POS.Data;
using Hugin.POS.Common;
using System.Threading;


namespace Hugin.POS.Display.GUI
{

    #if WindowsCE
    public enum ThreadState
    {
        // Summary:
        //     The thread has been started, it is not blocked, and there is no pending System.Threading.ThreadAbortException.
        Running = 0,
        //
        // Summary:
        //     The thread is being requested to stop. This is for internal use only.
        StopRequested = 1,
        //
        // Summary:
        //     The thread is being requested to suspend.
        SuspendRequested = 2,
        //
        // Summary:
        //     The thread is being executed as a background thread, as opposed to a foreground
        //     thread. This state is controlled by setting the System.Threading.Thread.IsBackground
        //     property.
        Background = 4,
        //
        // Summary:
        //     The System.Threading.Thread.Start() method has not been invoked on the thread.
        Unstarted = 8,
        //
        // Summary:
        //     The thread has stopped.
        Stopped = 16,
        //
        // Summary:
        //     The thread is blocked as a result of a call to System.Threading.Monitor.Wait(System.Object,System.Int32,System.Boolean),
        //     System.Threading.Thread.Sleep(System.Int32), or System.Threading.Thread.Join().
        WaitSleepJoin = 32,
        //
        // Summary:
        //     The thread has been suspended.
        Suspended = 64,
        //
        // Summary:
        //     The System.Threading.Thread.Abort(System.Object) method has been invoked
        //     on the thread, but the thread has not yet received the pending System.Threading.ThreadAbortException
        //     that will attempt to terminate it.
        AbortRequested = 128,
        //
        // Summary:
        //     The thread is in the Stopped state.
        Aborted = 256,
    }
    #endif

    class GraphicalDisplay : IDisplay
    {
        public event ConsumeKeyHandler ConsumeKey;
        public event EventHandler DisplayClosed;
        public event SalesSelectedHandler SalesSelected;
        public event EventHandler SalesFocusLost;

        private readonly static char placer = (char)15;
        private String messageDisplaying = "".PadLeft(20, placer);
        private int currentColumn = 0;
        private Leds ledStatus = 0;
        private string cashierMessage;
        private string customerMessage;

        Target mode = Target.Customer;
        bool paused = false;
        bool cursorOn = false;
        bool inactive = false;
        bool showCashierMessage = false;

        private static CustomerForm customerForm;
        private static DisplayAttribute attributes = DisplayAttribute.None;

        public Boolean HasAttribute(DisplayAttribute attr)
        {
            return (attr & attributes) == attr;
        }


        internal GraphicalDisplay()
        {
            customerForm = new CustomerForm();
            customerForm.Show();
            customerForm.Focus();
            customerForm.KeyUp += new System.Windows.Forms.KeyEventHandler(customerForm_KeyUp);
            customerForm.Closed += new EventHandler(customerForm_Closed);
        }

        void customerForm_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            ConsumeKey(sender, new ConsumeKeyEventArgs(e.KeyValue));
        }

        void customerForm_Closed(object sender, EventArgs e)
        {
            DisplayClosed(sender, e);
        }

        public Target Mode
        {
            set { mode = value; }
        }

        public void Pause()
        {
            paused = true;
        }

        public void Play()
        {
            paused = false;
        }

        public void Show(String msg)
        {
            if (paused) return;

            messageDisplaying = "".PadLeft(20, placer);
            currentColumn = 0;

            if (mode != Target.Customer)
                cashierMessage = msg;
            else if (mode != Target.Cashier)
                customerMessage = msg;

            if (Connector.Instance().CurrentSettings != null)
                showCashierMessage = (Connector.Instance().CurrentSettings.GetProgramOption(Setting.DisplayHeaderMessageMode) == PosConfiguration.ON);

            if ((mode & Target.Customer) == Target.Customer || showCashierMessage)
                customerForm.Show(msg, mode);
        }

        public void Show(IConfirm err)
        {
            if (paused) return;

            Show(err.Message);
            if ((mode & Target.Customer) == Target.Customer)
                customerForm.Refresh();
        }

        public void Show(string format, object arg0)
        {
            Show(String.Format(format, arg0));
        }
        public void Show(string format, params object[] args)
        {
            Show(String.Format(format, args));
        }

        public void Show(IProduct p)
        {
            if ((mode & Target.Customer) == Target.Customer || showCashierMessage)
                customerForm.Show(p);
        }

        public void Show(ICustomer customer)
        {
            if ((mode & Target.Customer) != Target.Customer)
                customerForm.Show(customer);
        }

        public void ShowSale(IFiscalItem fi)
        {
            if ((mode & Target.Customer) == Target.Customer)
                customerForm.ShowSale(fi);
        }

        public void Show(IFiscalItem fi)
        {
            if ((mode & Target.Customer) == Target.Customer)
                customerForm.Show(fi);
        }

        public void ShowVoid(IFiscalItem fi)
        {
            if ((mode & Target.Customer) == Target.Customer)
                customerForm.ShowVoid(fi);
        }

        public void Show(IAdjustment adjustment)
        {
            if ((mode & Target.Customer) == Target.Customer)
                customerForm.Show(adjustment);
        }

        public void ShowCorrect(IAdjustment adjustment, bool isSubTotalAdj)
        {
            if ((mode & Target.Customer) == Target.Customer)
                customerForm.ShowCorrect(adjustment);
        }

        public void Show(ISalesDocument doc)
        {
            if ((mode & Target.Customer) == Target.Customer || showCashierMessage)
                customerForm.Show(doc);
        }

        public void Show(IMenuList menu)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                customerForm.ShowMenu(menu);
        }

        public void Show(ICredit credit)
        {
            if ((mode & Target.Customer) == Target.Customer || showCashierMessage)
                customerForm.Show(credit);
        }

        public void Show(ICurrency currency)
        {
            if ((mode & Target.Customer) == Target.Customer || showCashierMessage)
                customerForm.Show(currency);
        }

        public void Show(String totalMsg, Decimal total)
        {
            if ((mode & Target.Customer) == Target.Customer)
                customerForm.Show(Display.FormatTotal(totalMsg, total));
        }
        public void Show(String firstMsg, Decimal firstTotal, String secondMsg, Decimal secondTotal, bool firstLine)
        {
            if ((mode & Target.Customer) == Target.Customer)
                customerForm.Show(Display.AmountPairFormat(firstMsg, firstTotal, secondMsg, secondTotal));
        }

        public void ClearError()
        {
            //if (paused) return;
            //if (!errorStatus) return;
            //Show("");
        }
        //Always assumes that appending starts at the leftmost point of the 2nd line

        public void Append(string msg)
        {
            Append(msg, true);
        }
        public void Append(String msg, bool moveCursor)
        {
            if (messageDisplaying.TrimEnd(placer).Length == 0)
            {
                Show(cashierMessage.Split('\n')[0]);
                CursorOn();
            }

            if (messageDisplaying.Trim(placer).Length >= 19)
                return;

            if (messageDisplaying.TrimEnd(placer).Length < 20 && currentColumn < 19)
            {
                if (msg != " ")
                    messageDisplaying = messageDisplaying.Remove(currentColumn, msg.Length);
                messageDisplaying = messageDisplaying.Insert(currentColumn, msg);

                currentColumn += msg.Length;

                if (!moveCursor)
                    CursorPrevious();

                if (messageDisplaying.Trim(placer).Length >= 19)
                    CursorOff();

                ShowInput();
            }

        }
        public void BackSpace()
        {
            if (currentColumn < 0) return;
            if (currentColumn < 20) CursorOn();
            messageDisplaying = messageDisplaying.Remove(currentColumn - 1, 1);
            currentColumn--;
            ShowInput();
        }
        public void CursorOn()
        {
            cursorOn = true;
        }
        public void CursorOff()
        {
            cursorOn = false;
        }

        public void CursorNext()
        {
            if (currentColumn >= messageDisplaying.Trim(placer).Length)
                return;
            currentColumn++;
            ShowInput();
            if (currentColumn > messageDisplaying.Trim(placer).Length)
                CursorOff();
        }

        public void CursorPrevious()
        {
            if (currentColumn < 1) return;
            if (messageDisplaying.Trim(placer).Length < 1) return;
            currentColumn--;
            ShowInput();

        }
        public int CurrentColumn
        {
            get { return currentColumn; }
        }

        private void ShowInput()
        {
            try
            {
                if (IsPaused) return;
                if (showCashierMessage || ((mode & Target.Customer) == Target.Customer))
                    customerForm.Show(cashierMessage + "\n" + messageDisplaying, currentColumn, cursorOn);
            }
            catch
            {
            }
        }

        private void MoveCursorRightEndPosition()
        {

        }
        private void MoveCursorUpperLine()
        {

        }
        private void MoveCursorUpperLeftMostPosition()
        {
        }
        public void SetBrightness(int level)
        {

        }
        public void Clear()
        {
            Show("");
        }
        public void Reset()
        {

        }
        public void ShowAdvertisement()
        {

        }
        public string LastMessage
        {
            get { return (mode == Target.Customer) ? customerMessage : cashierMessage; }
        }

        private void SetLed(Leds led)
        {
            customerForm.SetLed(led);
        }

        public void LedOn(Leds leds)
        {
            Leds myLeds = ledStatus | leds;
            SetLed(myLeds);
            ledStatus = myLeds;
        }

        public void LedOff(Leds leds)
        {
            Leds myLeds = ledStatus & ~leds;
            SetLed(myLeds);
            ledStatus = myLeds;
        }

        public bool IsLedOn(Leds leds)
        {
            return (ledStatus & leds) == leds;
        }

        public bool Inactive
        {
            get
            {
                return inactive;
            }
            set
            {
                inactive = value;
            }
        }

        public bool IsPaused
        {
            get { return paused; }
        }

        public void ChangeDocumentStatus(ISalesDocument doc, DisplayDocumentStatus de)
        {
            switch (de)
            {
                case DisplayDocumentStatus.OnStart:
                    customerForm.SetDocumentInfos(doc);
                    break;
                case DisplayDocumentStatus.OnChange:
                    customerForm.ChangeCurrentDocument(doc);
                    break;
                case DisplayDocumentStatus.OnUndoAdjustment:
                    customerForm.UndoAdjustment(doc);
                    break;
                case DisplayDocumentStatus.OnClose:
                    customerForm.DocumentClose(doc);
                    break;
            }
        }

        public void ChangeCustomer(ICustomer c)
        {
            customerForm.ChangeCustomer(c);
        }

        public void LoginCashier(ICashier c)
        {
            customerForm.LoginCashier(c);
        }

        public bool HasGraphKeyboard()
        {
            return false;
        }

        public void ShowTableContent(ISalesDocument document, decimal totalAdjAmount)
        {
            throw new NotImplementedException();
        }

        public void ClearTableContent()
        {
            throw new NotImplementedException();
        }

        public bool ShowAlertMessage(String message)
        {
            throw new NotImplementedException();
        }
    }
}
