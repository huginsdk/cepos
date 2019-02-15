using System;
using System.Text;
using System.Configuration;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using Hugin.POS.Common;
using System.Windows.Forms;

namespace Hugin.POS.Display.Datecs
{

    public class Display : IDisplay
    {
        private const int KEYPRESS_TIMEOUT = 100;
        private const int MAX_LEN = 60;
        private const int KEY_FREQUENCY = 3000;
        private const int ALERT_FREQUENCY = 400;

        public event ConsumeKeyHandler ConsumeKey;
        public event EventHandler DisplayClosed;
        public event SalesSelectedHandler SalesSelected;
        public event EventHandler SalesFocusLost;

        private readonly static char placer = (char)15;
        
        private String messageDisplaying = "".PadLeft(MAX_LEN, placer);
        private String cashierMessage;
        private String customerMessage;
        private int currentColumn = 0;
        private Leds ledStatus = 0;
        private Target currentTarget = Target.Both;
        private Target mode = Target.Both;
        private bool paused = false;
        private bool inactive = false;
        private bool cursorOn = true;
        private bool errorStatus = false;

        private static CashierForm touchForm;
        private static CustomerForm customerForm;
        private static ExternalDisplay external = null;
        private static IDisplay display = null;
        private static DisplayAttribute attributes = DisplayAttribute.TouchKeyboard;

        private static Screen[] screens;
        private static int primaryScreenId = 0;
        private static int secondaryScreenId = 1;

        static PrivateFontCollection pfc = null;

        public static PrivateFontCollection PFC
        {
            get { return pfc; }
        }

        public Boolean HasAttribute(DisplayAttribute attr)
        {
            return (attr & attributes) == attr;
        }

        public static IDisplay Instance()
        {
            if (display == null)
                display = new Display(Target.Both);
            return display;
        }

        internal static Screen[] Screens
        {
            get { return screens; }
        }
        internal static int PrimaryScreenID
        {
            get { return primaryScreenId; }
        }
        internal static int SecondaryScreenID
        {
            get { return secondaryScreenId; }
        }

        private Display(Target mode)
        {
            this.mode = mode;

            // Screens
            screens = Screen.AllScreens;

            primaryScreenId = 0;
            secondaryScreenId = 1;

            if (screens.Length > 1 &&
                PosConfiguration.ScreenIdentity == 2)
            {
                primaryScreenId = 1;
                secondaryScreenId = 0;
            }

            // Font
            pfc = new PrivateFontCollection();
            pfc.AddFontFile(@"Resources/MATRS.TTF");

            // Primary Screen
            touchForm = new CashierForm();
            System.Drawing.Rectangle boundsP = screens[primaryScreenId].Bounds;
            touchForm.SetBounds(boundsP.X, boundsP.Y, boundsP.Width, boundsP.Height);
            touchForm.StartPosition = FormStartPosition.Manual;
            touchForm.Show();
            touchForm.Focus();
            touchForm.ConsumeKey += new ConsumeKeyHandler(touchForm_ConsumeKey);
            touchForm.DisplayClosed += new EventHandler(touchForm_DisplayClosed);
            touchForm.SaleSelected += new SalesSelectedHandler(touchForm_SaleSelected);
            touchForm.SalesFocusLost += new EventHandler(touchForm_SalesFocusLost);

            // Secondary Screen           
            if (screens.Length > 1)
            {
                try
                {
                    customerForm = new CustomerForm();

                    System.Drawing.Rectangle bounds = screens[secondaryScreenId].Bounds;
                    customerForm.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    customerForm.StartPosition = FormStartPosition.Manual;
                    customerForm.Show();
                }
                catch { }
            }

            // External Display
            if (PosConfiguration.Get("DisplayComPort") != "")
            {
                try
                {
                    external = new ExternalDisplay();
                }
                catch { }
            }        
        }

        void touchForm_DisplayClosed(object sender, EventArgs e)
        {
            if (DisplayClosed != null)
                DisplayClosed(sender, e);
            Application.Exit();
        }

        void touchForm_SaleSelected(object sender, SalesSelectedEventArgs e)
        {
            if (SalesSelected != null)
                SalesSelected(sender, e);
        }

        void touchForm_SalesFocusLost(object sender, EventArgs e)
        {
            if (SalesFocusLost != null)
                SalesFocusLost(sender, e);
        }

        void touchForm_ConsumeKey(object sender, ConsumeKeyEventArgs e)
        {
            if (IsPaused) return;

            if (ConsumeKey != null)
                ConsumeKey(sender, e);
        }

        internal static String AmountPairFormat(String label1, Decimal amount1, String label2, Decimal amount2)
        {
            int numberLength = string.Format("{0:C}", new Number(amount1)).Length;
            if (label1.Length > (MAX_LEN - 1) - numberLength)
                label1 = label1.Substring(0, (MAX_LEN - 1) - numberLength);
            numberLength = string.Format("{0:C}", new Number(amount2)).Length;
            if (label2.Length > (MAX_LEN - 1) - numberLength)
                label2 = label2.Substring(0, (MAX_LEN - 1) - numberLength);
            return String.Format("{0}\t{1:C}\n{2}\t{3:C}", label1,
                                                           new Number(amount1),
                                                           label2,
                                                           new Number(amount2));

        }

        internal static String FormatTotal(String totalMsg, Decimal total)
        {
            return String.Format("{0}\n{1:C}", totalMsg, new Number(total));
        }

        public Target Mode
        {
            set
            {
                mode = value;
            }
        }

        public void Pause()
        {
            paused = true;
        }

        public void Play()
        {
            paused = false;
        }

        internal static EZLogger Log
        {
            get { return EZLogger.Log; }
        }

        public void Show(String msg)
        {
            if (paused) return;

            messageDisplaying = "".PadLeft(MAX_LEN, placer);
            currentColumn = 0;

            if (currentTarget != Target.Customer)
                cashierMessage = msg;
            else if (mode != Target.Cashier)
                customerMessage = msg;

            if ((mode & Target.Cashier) == Target.Cashier)
                touchForm.Show(msg);

            if (external != null)
            {
                if ((mode & Target.Customer) == Target.Customer)
                    external.Show(msg);
            }

            if (customerForm != null)
            {
                if ((mode & Target.Customer) == Target.Customer)
                    customerForm.Show(msg);
            }
            
        }

        public void Show(IConfirm err)
        {
            if (paused) return;
            Show(err.Message);
            errorStatus = true;
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
            touchForm.Show(p);

            if (customerForm != null && (mode & Target.Customer) == Target.Customer)
                customerForm.Show(p);
        }

        public void Show(ICustomer customer)
        {
            touchForm.Show(customer);
        }

        public void Show(IFiscalItem si)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.Show(si);
            }
            else
            {
                if (external != null)
                {
                    external.ShowSale(si);
                }

                if (customerForm != null)
                {
                    customerForm.ShowSale(si);
                }
            }
        }

        public bool ShowAlertMessage(String message)
        {
            //BlurForm bf = new BlurForm();
            //bf.Show();
            //bf.Blur(touchForm);

            return touchForm.ShowAlertMessage(message);

            //bf.UnBlur();
        }

        public void ShowSale(IFiscalItem si)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.ShowSale(si);
            }

            if (external != null)
            {
                external.ShowSale(si);
            }

            if (customerForm != null && (mode & Target.Customer) == Target.Customer)
            {
                customerForm.ShowSale(si);
            }
            
        }

        public void ShowVoid(IFiscalItem vi)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.ShowVoid(vi);
            }

            if (external != null)
            {
                external.ShowVoid(vi);
            }

            if (customerForm != null && (mode & Target.Customer) == Target.Customer)
                customerForm.ShowVoid(vi);
        }

        public void Show(IAdjustment adjustment)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                touchForm.Show(adjustment);

            if (customerForm != null && (mode & Target.Customer) == Target.Customer)
                customerForm.Show(adjustment);

            if (external != null)
                external.Show(adjustment);                    
        }

        public void ShowCorrect(IAdjustment adjustment, bool isSubTotalAdj)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                touchForm.ShowCorrect(adjustment, isSubTotalAdj);

            if (customerForm != null && (mode & Target.Customer) == Target.Customer)
                customerForm.ShowCorrect(adjustment);

            if (external != null)
                external.ShowCorrect(adjustment);
        }

        public void Show(ISalesDocument doc)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.Show(doc);
            }

            if (customerForm != null && (mode & Target.Customer) == Target.Customer)
                customerForm.Show(doc);
            
        }

        public void Show(IMenuList menu)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.ShowMenu(menu);
            }
        }

        public void Show(ICredit credit)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.Show(credit);
            }
        }

        public void Show(ICurrency currency)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.Show(currency);
            }
        }

        public void Show(String totalMsg, Decimal total)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.Show(Display.FormatTotal(totalMsg, total));
            }
        }
        public void Show(String firstMsg, Decimal firstTotal, String secondMsg, Decimal secondTotal, bool firstLine)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
            {
                touchForm.Show(Display.AmountPairFormat(firstMsg, firstTotal, secondMsg, secondTotal));

                if (external != null)
                {
                    external.Show(Display.AmountPairFormat(firstMsg, firstTotal, secondMsg, secondTotal));
                }
            }

            if (customerForm != null && (mode & Target.Customer) == Target.Customer)
                customerForm.Show(Display.AmountPairFormat(firstMsg, firstTotal, secondMsg, secondTotal));
        }

        public void ClearError()
        {
            if (paused) return;
            messageDisplaying = "".PadLeft(MAX_LEN, placer);
            if (!errorStatus) return;
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

            if (messageDisplaying.Trim(placer).Length >= MAX_LEN)
                return;

            if (messageDisplaying.TrimEnd(placer).Length <= MAX_LEN && currentColumn < MAX_LEN)
            {
                if (msg != " ")
                {
                    int length = msg.Length > messageDisplaying.Length ? messageDisplaying.Length : msg.Length;
                    messageDisplaying = messageDisplaying.Remove(currentColumn, length); 
                }
                messageDisplaying = messageDisplaying.Insert(currentColumn, msg);

                currentColumn += msg.Length;

                if (!moveCursor)
                    CursorPrevious();

                if (messageDisplaying.Trim(placer).Length >= MAX_LEN)
                    CursorOff();

                ShowInput();
            }

        }
        public void BackSpace()
        {
            if (currentColumn == 0) return;
            if (currentColumn <= MAX_LEN) CursorOn();
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
                touchForm.ShowInput(messageDisplaying.Trim(placer), currentColumn, cursorOn);
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
            touchForm.SetLed(led);
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
                    touchForm.SetDocumentInfos(doc);
                    break;
                case DisplayDocumentStatus.OnChange:
                    touchForm.ChangeDocument(doc);
                    if (customerForm != null)
                        customerForm.ChangeCurrentDocument(doc);
                    break;
                case DisplayDocumentStatus.OnUndoAdjustment:
                    break;
                case DisplayDocumentStatus.OnClose:
                    if (doc.Id == -1) break;
                    touchForm.DocumentClose(doc);
                    if (customerForm != null)
                        customerForm.DocumentClose(doc);
                    break;
            }
        }

        public void ChangeCustomer(ICustomer c)
        {
            touchForm.ChangeCustomer(c);
        }

        public bool HasGraphKeyboard()
        {
            return true;
        }

        public void ShowTableContent(ISalesDocument document, decimal totalAdjAmount)
        {
            touchForm.ShowTableContent(document, totalAdjAmount);
        }

        public void ClearTableContent()
        {
            touchForm.ClearGrid();
        }
    }
}
