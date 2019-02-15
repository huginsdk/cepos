using System;
using System.Text;
using System.Configuration;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.Display
{
    class HuginDisplay: IDisplay
    {
        enum Symbol
        {
            E=1,
            O=2,
            MINUS=4,
            T=8,
            X=16
        }
        enum DisplayType
        {
            Integer = 0,
            Price = 1,
            AlphaNumeric = 2
        }

        public event ConsumeKeyHandler ConsumeKey;
        public event EventHandler DisplayClosed;

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
        List<String> stack;
        DateTime lastMessageSend = DateTime.MinValue;
        bool cursorOn = false;

        public HuginDisplay()
        {
            try
            {
                String portName = PosConfiguration.Get("DisplayComPort").Substring(1);
                serialPort = new SerialPort(portName,19200);

                stack = new List<string>();

                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    TrySerial();
                    serialPort.Encoding = PosConfiguration.DefaultEncoding;
                    Clear();
                }
                System.Threading.Thread serialThread = new System.Threading.Thread(delegate() { KeyUp(); });
                serialThread.IsBackground = true;
                serialThread.Start();
            }
            catch (NotSerialException nse)
            {
                if (serialPort.IsOpen)
                    serialPort.Close();
                throw nse;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw ex;
            }
            catch (System.IO.IOException ex)
            {
                throw new Exception("DÝSPLAY PORTU ARIZALI");
            }
            catch (Exception ex)
            {
                if (Display.Log == null) return;
                Display.Log.Fatal("HYDisplay:HyDisplay - Exception {0} is {1}", serialPort.PortName, serialPort.IsOpen ? "Open" : "Closed");
                //if (cr.Printer == null) return;
                //cr.Printer.PrintRemark(PosMessage.CAN_NOT_ACCESS_TO_DISPLAYS);
                //CashRegister.Void();
            }
        }

        private void TrySerial()
        {
            /* to understand if the keyboard is serial or ps/2 */
            if (serialPort.BytesToRead > 0)
                serialPort.ReadExisting();
        }

        //TODO Desctructor to set off buzzer

        private void KeyUp()
        {
            while (true)
            {

                if (serialPort.BytesToRead == 0)
                {
                    if (stack.Count == 0)
                    {
                        System.Threading.Thread.Sleep(50);
                        continue;
                    }
                }
                else
                {
                    byte [] buffer=new byte[3];
                    serialPort.Read(buffer,0,3);
                    String s="";
                    foreach(byte b in buffer){
                        if(b<48){
                            s=b.ToString();
                            break;
                        }
                    }

                    stack.AddRange(s.Split(' '));
                }

                try
                {
                    String input = ReadNext();

                    
                    if (input.Length == 0) continue;
                    try
                    {                        

                        PosKey key = KeyMap.Get(input);
                        ConsumeKey(this, new ConsumeKeyEventArgs(key));
                        
                    }
                    catch { continue; }
                    
                }
                catch (TimeoutException)
                {
                    Display.Log.Error("Timeout exception on serial keyboard");
                }
                catch (Exception e)
                {
                    Display.Log.Error(e);
                }
            }

        }

        private string ReadNext()
        {
            string input = stack[0];
            stack.RemoveAt(0);
            return input;
        }

        //TODO Desctructor to set off buzzer

        public Target Mode
        {
            set
            {
                if (currentTarget != value)
                {
                    currentTarget = value;
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
            else
            {
                customerMessage = msg;
                return;
            }
            messageDisplaying = "".PadLeft(20, placer);
            currentColumn = 0;

            byte[] cursorHome = { CursorHome };


            if (msg.IndexOf("\n") < 0) msg += "\n";

            string[] lines = msg.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if ((lines[i].IndexOf("\t")) > -1)
                    lines[i] = PadCenter(lines[i]);
                else
                    lines[i] = AlignCenter(lines[i]);
                WriteCashierMessage(FixTurkish(lines[i]), i + 1);
            }

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

        private string FixTurkish(String s)
        {
            s = s.Replace('ç', 'c');
            s = s.Replace('ð', 'g');
            s = s.Replace('ý', 'i');
            s = s.Replace('ö', 'o');
            s = s.Replace('þ', 's');
            s = s.Replace('ü', 'u');
            s = s.Replace('Ç', (char)1);
            s = s.Replace('Ð', (char)2);
            s = s.Replace('Ý', (char)3);
            s = s.Replace('Ö', (char)4);
            s = s.Replace('Þ', (char)5);
            s = s.Replace('Ü', (char)6);
            return s;
        }

        public void Show(IConfirm err)
        {
            if (IsPaused) return;
            if (err.Message != null)
                Show(err.Message);
            errorStatus = true;
            byte[] warning = { Warning };
            Buzz();
        }
        private void Buzz()
        {
            serialPort.Write((char)4 + "1" + (char)13);
        }
        private void ClearBuzzer()
        {
            serialPort.Write((char)4 + "0" + (char)13);
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
                WriteSegmentPrice((Symbol)0, p.UnitPrice);
                
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

                WriteSegmentPrice((Symbol)0, fi.TotalAmount);
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
                WriteSegmentPrice(Symbol.MINUS, fi.TotalAmount);
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
                        Show("{0}\n{1:P}\t{2:C}", PosMessage.PRODUCT_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 adjustment.NetAmount);
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C}", PosMessage.PRODUCT_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 adjustment.NetAmount);
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n \t{1:C}", PosMessage.PRODUCT_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n \t{1:C}", PosMessage.PRODUCT_PRICE_FEE,
                                              new Number(adjustment.NetAmount));

                }
                else if (adjustment.Target is ISalesDocument)
                {
                    if (adjustment.Method == (AdjustmentType.PercentDiscount))
                        Show("{0}\n{1:P}\t{2:C}", PosMessage.SUBTOTAL_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C}", PosMessage.SUBTOTAL_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n \t{1:C}", PosMessage.SUBTOTAL_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n \t{1:C}", PosMessage.SUBTOTAL_PRICE_FEE,
                                             new Number(adjustment.NetAmount));
                }
                else throw new InvalidProgramException("Adjustment target is incorrectly set");
                WriteSegmentPrice((Symbol)0, adjustment.NetAmount);
            }
            catch (FormatException e)
            {
                Display.Log.Error("Display error. {0}", e.Message);
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
            WriteSegmentPrice(Symbol.O, total);
        }
        public void Show(String firstMsg, Decimal firstTotal, String secondMsg, Decimal secondTotal, bool firstLine)
        {
            Show(Display.AmountPairFormat(firstMsg, firstTotal, secondMsg, secondTotal));
            WriteSegmentPrice(Symbol.T, firstLine ? firstTotal : secondTotal);
        }

        public void ClearError()
        {
            if (IsPaused) return;
            if (!errorStatus) return;
            ClearBuzzer();
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
                WriteALL();
            }
        }
        public void BackSpace()
        {
            if (currentColumn < 0) return;
            if (currentColumn < 20) CursorOn();
            messageDisplaying = messageDisplaying.Remove(currentColumn - 1, 1);
            currentColumn--;
            WriteALL();
        }
        public void CursorOn()
        {
            //0 0 0 0 0 0 1 D C B
            //serialPort.Write(new byte[] { 14 }, 0, 1);
            cursorOn = true;
        }
        public void CursorOff()
        {
            //0 0 0 0 0 0 1 D C B
            //serialPort.Write(new byte[] { 12 }, 0, 1);
            cursorOn = false;
        }

        public void CursorNext()
        {
            if (currentColumn >= messageDisplaying.Trim(placer).Length)
                return;
            currentColumn++;
            WriteALL();
            if (currentColumn > messageDisplaying.Trim(placer).Length)
                CursorOff();
        }
        public void CursorPrevious()
        {
            if (currentColumn < 1) return;
            if (messageDisplaying.Trim(placer).Length < 1) return;
            currentColumn--;
            WriteALL();
        }
        public void SetBrightness(int level)
        {

        }
        public void Clear()
        {
            messageDisplaying = "".PadLeft(20, placer);
            currentColumn = 0;
            WriteCashierMessage(" ".PadLeft(20), 1);
            WriteCashierMessage(" ".PadLeft(20), 2);
        }
        public void Reset()
        {
            try
            {
                if (IsPaused) return;
                serialPort.Close();
                serialPort.PortName = PosConfiguration.Get("DisplayComPort").Substring(1);
                serialPort.Open();
                messageDisplaying = "".PadLeft(20, placer);
            }
            catch (Exception)
            {
                Display.Log.Fatal("HuginDisplay.Reset Exception occurred. {0} is {1}", serialPort.PortName, serialPort.IsOpen ? "Open" : "Closed");
                //TODO: printerdan Hata mesaji ver?
            }
        }
        public void ShowAdvertisement()
        {
            
        }
        public string LastMessage
        {
            get { return (currentTarget == Target.Customer) ? customerMessage : cashierMessage; }
        }

        public int CurrentColumn
        {
            get { return currentColumn; }
        }
        private void Write(String msg)
        {
            if (IsPaused) return;

            messageDisplaying = messageDisplaying.Remove(currentColumn, msg.Length);
            messageDisplaying = messageDisplaying.Insert(currentColumn, msg);

            if (messageDisplaying.Length > 20)
            {
                messageDisplaying = messageDisplaying.Substring(0, 20);
            }
            currentColumn += msg.Length;

            msg = messageDisplaying.Trim(placer);
            //if (currentColumn < 20 && cursorOn)
            //    msg = msg.Insert(currentColumn, "|");
            WriteCashierMessage(FixTurkish(msg), 2);
        }
        // backspace and space calls writeALL
        private void WriteALL()
        {
            if (IsPaused) return;

            if (messageDisplaying.Length > 19)
            {
                messageDisplaying = messageDisplaying.Substring(0, 19);
            }
            string msg = messageDisplaying.Trim(placer);
            //if (currentColumn < 20 && cursorOn)
            //    msg = msg.Insert(currentColumn, "|");
            WriteCashierMessage(FixTurkish(msg), 2);
        }

        private void WriteCashierMessage(string data, int lineNumber)
        {
            if (data.Length > 20) data = data.Substring(0, 20);
            if (data.Length < 20) data = data.PadRight(20, ' ');
            string line = (char)lineNumber + data + (char)13;
            Send(line);
        }

        private void WriteSegmentPrice(Symbol symbol, decimal amount)
        {
            if (amount < 0)
            {
                symbol = symbol | Symbol.MINUS;
                amount = Math.Abs(amount);
            }
            int price = (int)(amount * 100);
            string data = (char)3 + "" + (char)(192 + (int)symbol) + "" + (int)(DisplayType.Price);
            data += price.ToString().PadLeft(10);
            data += (char)13;
            Send(data);
        }
        private void WriteSegmentQuantity(Symbol symbol, int quantity)
        {
            if (quantity < 0)
            {
                symbol = symbol | Symbol.MINUS;
                quantity = Math.Abs(quantity);
            }
            string data = (char)3 + "" + (char)(192 + (int)symbol) + "" + (int)(DisplayType.Integer);
            data += quantity.ToString().PadLeft(10);
            data += (char)13;
            Send(data);

        }

        private void Send(string data)
        {
            TimeSpan ts = DateTime.Now - lastMessageSend;
            if (ts.Milliseconds < 50)
                System.Threading.Thread.Sleep(50 - ts.Milliseconds);
            serialPort.Write(data);
            lastMessageSend = DateTime.Now;
        }
        private void SetLed(Leds led)
        {

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

        }

        public void ChangeCustomer(ICustomer c)
        {

        }
    }
}
