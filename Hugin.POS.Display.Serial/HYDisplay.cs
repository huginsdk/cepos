using System;
using System.Text;
using System.Configuration;
using System.IO.Ports;
using System.Collections;
using Hugin.POS.Common;
using Hugin.POS.Data;


namespace Hugin.POS.Display.Serial
{
    public class HYDisplay : IDisplay
    {
        public event ConsumeKeyHandler ConsumeKey;
        public event EventHandler DisplayClosed;
        public event SalesSelectedHandler SalesSelected;
        public event EventHandler SalesFocusLost;

        #region Protect definitions
        protected const byte Off = 0x00,
           On = 0x01,
           Warning = 0x07,
           CursorMoveLeft = 0x08,
           CursorMoveRight = 0x09,
           CursorMoveDown = 0x0A,
           CursorHome = 0x0B,
           ClrScreen = 0x0C,
           BothDisplays = 0x1A,
           CustomerDisplayOnly = 0x1C,
           CashierDisplayOnly = 0x1D,
           ClearWarning = 0x1E,
           Initialize = 0x1F,
           Space = 0x20,
           InitScreen = 0x28;

        protected readonly byte[] normalMode = { ClrScreen, Initialize, 0x01 };
        protected readonly byte[] verticalScrollMode = { ClrScreen, Initialize, 0x02 };
        protected readonly byte[] horizontalScrollMode = { ClrScreen, Initialize, 0x03 };
        protected readonly byte[] initializeDisplay = { ClrScreen, Initialize, On };

        protected readonly static char placer = (char)15;
        protected String messageDisplaying = "".PadLeft(20, placer);

        protected const int ScreenLength = 20;
        protected static System.IO.Ports.SerialPort serialPort;
        protected int currentColumn = 0;
        protected bool errorStatus = false;
        private bool paused = false;
        protected Leds ledStatus = 0;
        protected Target currentTarget = Target.Both;

        protected string cashierMessage;
        protected string customerMessage;
        #endregion

        private static bool inactive = false;
        private static DisplayAttribute attributes = DisplayAttribute.None;

        readonly byte[] ledsOff = { 0xBA, 0x03, 0x98, 0x00, 0x21, 0x0C };
        Object serialLock = new Object();

        public Boolean HasAttribute(DisplayAttribute attr)
        {
            return (attr & attributes) == attr;
        }


        public HYDisplay()
        {
            try
            {
                String portName = PosConfiguration.Get("DisplayComPort");
                serialPort = new SerialPort(portName);
                if (!serialPort.IsOpen)
                {
                    serialPort.ReadTimeout = 2048;
                    serialPort.Open();
                    Write(ledsOff, 0, 6);
                    Write(initializeDisplay, 0, 3);
                    serialPort.Write(horizontalScrollMode, 0, horizontalScrollMode.Length);
                    System.Threading.Thread.Sleep(100);
                    serialPort.Write(normalMode, 0, normalMode.Length);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
            catch
            {
                Display.Log.Fatal("HYDisplay:HyDisplay - Exception {0} is {1}", serialPort.PortName, serialPort.IsOpen ? "Open" : "Closed");
            }
        }

        //TODO Desctructor to set off buzzer

        public Target Mode
        {
            set
            {
                if (currentTarget != value)
                {
                    currentTarget = value;
                    byte[] setDisplay = { BothDisplays };
                    switch (currentTarget)
                    {
                        case Target.Customer:
                            setDisplay[0] = CustomerDisplayOnly;
                            break;
                        case Target.Cashier:
                            setDisplay[0] = CashierDisplayOnly;
                            break;
                        case Target.Both:
                            setDisplay[0] = BothDisplays;
                            break;
                    }
                    Write(setDisplay, 0, 1);
                }
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

        public void Show(String msg)
        {
            if (IsPaused) return;

            if (currentTarget != Target.Customer)
            {
                cashierMessage = msg;
                CursorOff();
            }
            else customerMessage = msg;

            byte[] cursorHome = { CursorHome };
            Write(cursorHome, 0, 1);
            //MoveCursorUpperLeftMostPosition();
            StringBuilder printMsg = new StringBuilder();
            if (!Str.Contains(msg,"\n")) msg += "\n";

            foreach (String line in msg.Split('\n'))
            {
                if (Str.Contains(line,"\t"))
                    printMsg.Append(PadCenter(line));
                else
                    printMsg.Append(AlignCenter(line));
            }
            Write(printMsg.ToString(), 0, 42);

        }

        private String PadCenter(String msg)
        {

            String[] str = msg.Split('\t');
            String s1 = str[0];
            String s2 = str[1];
            int length = s1.Length + s2.Length;
            s1 = s1.PadRight(20 - s2.Length, ' ');
            return s1 + s2;
        }
        private String AlignCenter(String msg)
        {
            int length = msg.Length;
            int spaceLeft = (20 - length) / 2;
            int spaceRight = 20 - length - spaceLeft;
            msg = msg.PadLeft(20 - spaceRight, ' ');
            msg = msg.PadRight(20, ' ');
            return msg;
        }

        private byte[] FixTurkish(String s)
        {
            byte[] b = new byte[s.Length + 2];
            int i = 0;
            foreach (char c in s)
            {
                switch ((int)c)
                {
                    case 286: //G.
                        b[i++] = 166;
                        break;
                    case 287: //g.
                        b[i++] = (byte)'g';
                        break;
                    case 220: //U.
                        b[i++] = 154;
                        break;
                    case 252: //u:
                        goto case 220;
                    case 350: //S.
                        b[i++] = 158;
                        break;
                    case 351: //s.
                        b[i++] = (byte)'s';
                        break;
                    case 304: //I.
                        b[i++] = 152;
                        break;
                    case 305: //i no .
                        b[i++] = (byte)'i';
                        break;
                    case 199: //C.
                        b[i++] = 128;
                        break;
                    case 214: //O:
                        b[i++] = 153;
                        break;
                    case 231: //c.
                        b[i++] = (byte)'c';
                        break;
                    case 246: //o:
                        b[i++] = 153;
                        break;
                    default:
                        b[i++] = (byte)c;
                        break;

                }
            }
            b[s.Length] = CursorHome;
            b[s.Length + 1] = CursorMoveDown;
            return b;
        }

        public void Show(IConfirm err)
        {
            if (IsPaused) return;

            Show(err.Message);
            errorStatus = true;
            byte[] warning = { Warning };
            Write(warning, 0, 1);
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
            try
            {
                bool large = ((p.Quantity != (long)p.Quantity) && p.UnitPrice * p.Quantity > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", p.Name, new Number(p.Quantity), large ? "X" : p.Unit, new Number(p.UnitPrice * p.Quantity));
            }
            catch (FormatException fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }
        public void Show(ICustomer customer)
        {
            try
            {
                Show("{0}", customer.Name);
            }
            catch (FormatException fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }
        public void ShowSale(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)), large ? "X" : fi.Unit, new Number(fi.TotalAmount - fi.VoidAmount));
            }
            catch (FormatException fex)
            {
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void Show(IFiscalItem fi)
        {
            try
            {
                string ttlAmount = new Number(fi.Product.UnitPrice * fi.Quantity).ToString("C");
                if (fi.Product.SecondaryUnitPrice > 0 && fi.Product.UnitPrice != fi.Product.SecondaryUnitPrice)
                    ttlAmount = String.Format("{0:C}/{1:C}", new Number(fi.Product.UnitPrice * fi.Quantity),
                                                                new Number(fi.Product.SecondaryUnitPrice * fi.Quantity));

                if (ttlAmount.Length > 13)
                    ttlAmount = String.Format("{0:C}", new Number(fi.TotalAmount - fi.VoidAmount));

                bool large = ((fi.Quantity != (long)fi.Quantity) && ttlAmount.Length > 9);
                Show("{0}\n{1:G10} {2}\t{3}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)), large ? "X" : fi.Unit, ttlAmount);
            }
            catch (FormatException fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }
        public void ShowVoid(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(fi.Quantity), large ? "X" : fi.Unit, new Number(fi.TotalAmount));
            }
            catch (FormatException fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }
        public void Show(IAdjustment adjustment)
        {
            try
            {
                if (adjustment.Target is IFiscalItem)
                {
                    if (adjustment.Method == (AdjustmentType.PercentDiscount))
                        Show("{0}\n{1:P}\t{2:N2}", PosMessage.PRODUCT_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 adjustment.NetAmount);
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:N2}", PosMessage.PRODUCT_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 adjustment.NetAmount);
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n \t{1:N2}", PosMessage.PRODUCT_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n \t{1:N2}", PosMessage.PRODUCT_PRICE_FEE,
                                              new Number(adjustment.NetAmount));

                }
                else if (adjustment.Target is ISalesDocument)
                {
                    if (adjustment.Method == (AdjustmentType.PercentDiscount))
                        Show("{0}\n{1:P}\t{2:N2}", PosMessage.SUBTOTAL_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:N2}", PosMessage.SUBTOTAL_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n \t{1:N2}", PosMessage.SUBTOTAL_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n \t{1:N2}", PosMessage.SUBTOTAL_PRICE_FEE,
                                             new Number(adjustment.NetAmount));
                }
                else throw new InvalidProgramException("Adjustment target is incorrectly set");
            }
            catch (FormatException e)
            {
                Display.Log.Error("Display error. {0}", e.Message);
            }
        }

        public void ShowCorrect(IAdjustment adjustment, bool isSubTotalAdj)
        {
            try
            {
                if (adjustment.Method == (AdjustmentType.PercentDiscount) ||
                   (adjustment.Method == AdjustmentType.PercentFee))
                    Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.CORRECTION,
                                         new Number(adjustment.RequestValue / 100),
                                         new Number(adjustment.NetAmount));
                else
                    Show("{0}\n \t{1:C}", Common.PosMessage.CORRECTION,
                                          new Number(adjustment.NetAmount));
            }
            catch (FormatException e)
            {
                EZLogger.Log.Error("Display error. {0}", e.Message);
            }
        }


        public void Show(ISalesDocument doc)
        {
            try
            {
                Show("{0}\n{1}", (doc.IsEmpty) ? PosMessage.SELECT_DOCUMENT : PosMessage.TRANSFER_DOCUMENT, doc.Name);
            }
            catch (FormatException fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void Show(IMenuList menu)
        {
            //do nothing
        }

        public void Show(ICredit credit)
        {
            try
            {
                Show("{0}\t{1}\n{2}", PosMessage.CREDIT, credit.Id, credit.Name);
            }
            catch (FormatException fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void Show(ICurrency currency)
        {
            try
            {
                Show("{0}\n{1}\tx {2:C}", PosMessage.FOREIGNCURRENCY, currency.Name, currency.ExchangeRate);
            }
            catch (FormatException fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void Show(String totalMsg, Decimal total)
        {
            Show(Display.FormatTotal(totalMsg, total));
        }
        public void Show(String firstMsg, Decimal firstTotal, String secondMsg, Decimal secondTotal, bool firstLine)
        {
            Show(Display.AmountPairFormat(firstMsg, firstTotal, secondMsg, secondTotal));
        }
        public void ClearError()
        {
            if (IsPaused) return;
            if (!errorStatus) return; 
            byte[] clearWarning = { ClearWarning };
            Write(clearWarning, 0, 1);
            errorStatus = false;
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

            if (messageDisplaying.TrimEnd(placer).Length < 20 && currentColumn < 19)
            {
                if (msg == " ")
                {
                    Write(new byte[] { Space }, 0, 1);
                }
                else
                {
                    Write(msg, 0, msg.Length);

                    if (moveCursor)
                        CursorNext();
                }

            }
            if (currentColumn == 18) CursorOff();
        }
        public void BackSpace()
        {
            if (currentColumn < 0) return;
            if (currentColumn < 20) CursorOn();
            byte[] backspace = { CursorMoveLeft, Space, CursorMoveLeft };
            Write(backspace, 0, 3);
        }
        public void CursorOn()
        {
            byte[] cursorOn = { Initialize, 0x43, On };
            Write(cursorOn, 0, 3);
        }
        public void CursorOff()
        {
            byte[] cursorOff = { Initialize, 0x43, Off };
            Write(cursorOff, 0, 3);
        }
       
        public void CursorNext()
        {
            if (currentColumn > 18) return;
            if (currentColumn == 18) CursorOff();
            byte[] cursorNext = { CursorMoveRight };
            Write(cursorNext, 0, 1);
        }
       
        public void SetBrightness(int val)
        {
            byte[] brightness ={ CustomerDisplayOnly, 42, (byte)val };
            Write(brightness, 0, 3);
        }

        private void MoveCursorRightEndPosition()
        {
            byte[] cursorRightEnd = { CustomerDisplayOnly, 108, 19, 1 };
            Write(cursorRightEnd, 0, 4);
        }
        private void MoveCursorUpperLine()
        {
            byte[] cursorUpperLine = { CustomerDisplayOnly, 108, 20, 1 };
            Write(cursorUpperLine, 0, 4);
        }
        private void MoveCursorUpperLeftMostPosition()
        {
            byte[] cursorUpperLeftMost ={ CustomerDisplayOnly, 108, 1, 1 };
            Write(cursorUpperLeftMost, 0, 4);
        }
        private void CustomerCursorOn()
        {
            byte[] customerCursorOn ={ CustomerDisplayOnly, 95, On };
            Write(customerCursorOn, 0, 3);
        }
        private void CustomerCursorOff()
        {
            byte[] customerCursorOff ={ CustomerDisplayOnly, 95, Off };
            Write(customerCursorOff, 0, 3);
        }
        private void HorizontalScrollMode()
        {
            Write(horizontalScrollMode, 0, 3);
        }
        public void Clear()
        {
            byte[] clear ={ ClrScreen };
            Write(clear, 0, 1);
        }
        public void Reset()
        {
            try
            {
                if (IsPaused) return;
                serialPort.Close();
                serialPort.PortName = PosConfiguration.Get("DisplayComPort");
                serialPort.Open();
                serialPort.Write(initializeDisplay, 0, 3);
            }
            catch (Exception)
            {
                Display.Log.Fatal("HyDisplay.Reset Exception occurred. {0} is {1}", serialPort.PortName, serialPort.IsOpen ? "Open" : "Closed");
                //TODO: printerdan Hata mesaji ver?
            }
        }
        public void ShowAdvertisement()
        {
            if (IsPaused) return;
            Display.Log.Debug("Enter showadvertisement");

            serialPort.Write(horizontalScrollMode, 0, horizontalScrollMode.Length);

            try
            {
                Display.Log.Debug("Enter loop");
                Inactive = true;

                //TODO: while (Inactive && BackgroundWorker.QueueCount == 0)
                while (Inactive)
                {
                    foreach (char c in  Connector.Instance().CurrentSettings.IdleMessage + "          ")
                    {
                        //if (Console.In.Peek() > -1)
                        //{
                        //    Inactive = false; break;
                        //}
                        //if (!Inactive) break; //break immediately

                        //if (BackgroundWorker.QueueCount > 0) break;

                        //if (BackgroundWorker.IsUp) LedOn(Leds.Online);

                        Mode = Target.Customer;
                        serialPort.Write(FixTurkish(c.ToString()), 0, 1);
                        System.Threading.Thread.Sleep(250);
                    }

                }
                Display.Log.Debug("Exit loop");
            }
            finally
            {
                serialPort.Write(normalMode, 0, normalMode.Length);
                if (currentTarget == Target.Cashier)
                    Show(cashierMessage);
                else
                    Show(customerMessage);
                Display.Log.Debug("Exit showadvertisement");
            }

        }
        public string LastMessage
        {
            get { return (currentTarget == Target.Customer) ? customerMessage : cashierMessage; }
        }

        public void CursorPrevious()
        {
            if (currentColumn < 1) return;
            if (currentColumn < 20) CursorOn();
            byte[] cursorPrevious = { CursorMoveLeft };
            Write(cursorPrevious, 0, 1);
        }

        public int CurrentColumn
        {
            get { return currentColumn; }
        }

        private void Write(String msg, int offset, int count)
        {
            if (IsPaused) return;
            byte[] buf;
            buf = FixTurkish(msg);
            int i;
            lock (serialLock)
            {
                if (count == 42)//show
                {
                    for (i = 0; i < count; i++)
                    {
                        if (serialPort.CtsHolding == true)
                        {
                            serialPort.Write(buf, i, 1);
                        }
                        else
                        {
                            i--;
                        }
                    }
                    //	serialPort.Write(FixTurkish(msg), offset, count);
                    messageDisplaying = "".PadLeft(20, placer);
                    currentColumn = 0;
                    return;
                }
                messageDisplaying = messageDisplaying.Remove(currentColumn, msg.Length);
                messageDisplaying = messageDisplaying.Insert(currentColumn, msg);

                if (messageDisplaying.Length > 19)
                {
                    messageDisplaying = Str.Remove(messageDisplaying,19);
                }
                currentColumn += msg.Length - 1;

                for (i = 0; i < count; i++)
                {
                    if (serialPort.CtsHolding == true)
                    {
                        serialPort.Write(buf, i, 1);
                    }
                    else
                    {
                        i--;
                    }
                }
                //	serialPort.Write(FixTurkish(msg), offset, count);

                serialPort.Write(new byte[] { CursorMoveLeft }, 0, 1);
            }

        }
        // backspace and space calls writeALL
        private void WriteALL()
        {
            if (IsPaused) return;
            if (messageDisplaying.Length > 19)
            {
                messageDisplaying = Str.Remove(messageDisplaying,19);
            }
            lock (serialLock)
            {
                serialPort.Write(new byte[] { CursorHome, CursorMoveDown }, 0, 2);
                serialPort.Write(FixTurkish(messageDisplaying.Replace(placer, ' ')), 0, messageDisplaying.Length);

                CursorOff();
                if (currentColumn < 0) currentColumn = 0;

                for (int i = currentColumn; i < messageDisplaying.Length; i++)
                    serialPort.Write(new byte[] { CursorMoveLeft }, 0, 1);
            }
            CursorOn();
        }
        private void Write(byte[] msg, int offset, int count)
        {
            if (IsPaused) return;
            switch (msg[0])
            {
                case CustomerDisplayOnly://set display ccustomer only
                case CashierDisplayOnly://set display cashier only
                case BothDisplays://set display cashier and ccustomer
                case Warning://error
                case ClearWarning://clear error
                case CursorHome://setCursor
                case Initialize://cursor on-off
                    break;
                case ClrScreen://Clear, Initialize
                    messageDisplaying = "".PadLeft(20, placer);
                    currentColumn = 0;
                    return;

                case CursorMoveLeft:
                    if (msg.Length == 1)//cursor move left
                    {
                        currentColumn--;
                        break;
                    }
                    switch (msg[1])
                    {
                        case Initialize://change current character
                            break;
                        case Space://BackSpace
                            CursorPrevious();
                            messageDisplaying = messageDisplaying.Remove(currentColumn, 1) + placer;
                            WriteALL();
                            return;
                    }
                    break;
                case CursorMoveRight://cursor move right
                    currentColumn++;
                    break;
                case Space:
                    messageDisplaying = messageDisplaying.Insert(currentColumn, " ");
                    WriteALL();
                    if (currentColumn == messageDisplaying.Trim(placer).Length - 1)//last character
                        CursorNext();
                    return;
                default:

                    break;
            }
            lock (serialLock)
            {
                for (int i = 0; i < count; i++)
                {
                    if (serialPort.CtsHolding == true)
                    {
                        serialPort.Write(msg, i, 1);
                    }
                    else
                    {
                        i--;
                    }
                }
            }
        }


        private void SetLed(Leds led)
        {

            if (
                     (((led & Leds.Customer) == Leds.Customer) && (GetKeyState((int)PosKey.CapsLock) == 0)) ||
                     (((led & Leds.Customer) != Leds.Customer) && (GetKeyState((int)PosKey.CapsLock) == 1))
                     )
                PressKeyboardButton(PosKey.CapsLock);
            if (
                (((led & Leds.Online) == Leds.Online) && (GetKeyState((int)PosKey.ScrollLock) == 0)) ||
                (((led & Leds.Online) != Leds.Online) && (GetKeyState((int)PosKey.ScrollLock) == 1))
                )
                PressKeyboardButton(PosKey.ScrollLock);
            if (
                (((led & Leds.Sale) == Leds.Sale) && (GetKeyState((int)PosKey.NumLock) == 0)) ||
                (((led & Leds.Sale) != Leds.Sale) && (GetKeyState((int)PosKey.NumLock) == 1))
                )
                PressKeyboardButton(PosKey.NumLock);
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

        private static void PressKeyboardButton(PosKey keyCode)
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            keybd_event((byte)keyCode, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event((byte)keyCode, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
        #region DLL Imports

        /// <summary>
        /// This function retrieves the status of the specified virtual key.
        /// The status specifies whether the key is up, down.
        /// </summary>
        /// <param name="keyCode">Specifies a key code for the button to me checked</param>
        /// <returns>Return value will be 0 if off and 1 if on</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern short GetKeyState(int keyCode);

        /// <summary>
        /// This function is useful to simulate Key presses to the window with focus.
        /// </summary>
        /// <param name="bVk">Specifies a virtual-key code. The code must be a value in the range 1 to 254.</param>
        /// <param name="bScan">Specifies a hardware scan code for the key.</param>
        /// <param name="dwFlags"> Specifies various aspects of function operation. This parameter can be one or more of the following values.
        ///                         <code>KEYEVENTF_EXTENDEDKEY</code> or <code>KEYEVENTF_KEYUP</code>
        ///                         If specified, the key is being released. If not specified, the key is being depressed.</param>
        /// <param name="dwExtraInfo">Specifies an additional value associated with the key stroke</param>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        #endregion


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

        }

        public void ChangeCustomer(ICustomer c)
        {

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
