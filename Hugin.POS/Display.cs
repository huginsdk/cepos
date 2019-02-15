using System;
using Hugin.POS.Data;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public abstract class Display
    {
        [Flags]
        public enum Target { Cashier = 1, Customer = 2, Both = Cashier | Customer }

        [Flags]
        public enum Leds : byte { Online=0x01, Sale=0x02, Customer=0x04, OnlineBlink=0x08, All= Sale|Customer|Online, None = 0 }

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

        public static bool Inactive;

        protected readonly static char placer = (char)15;
        protected String messageDisplaying = "".PadLeft(20, placer);

        protected const int ScreenLength = 20;
        protected static System.IO.Ports.SerialPort serialPort;
        protected int currentColumn = 0;
        protected bool errorStatus = false;
        private bool paused = false;
        protected Leds ledStatus = 0;
        protected Display.Target currentTarget = Target.Both;

        private static Display display;
        protected string cashierMessage;
        protected string customerMessage;
        public static Display Cashier
        {
            get {
                display.Mode = Target.Cashier;
                return display;
            }
        }
        public static Display Customer
        {
            get {
                display.Mode = Target.Customer;
                return display;
            }
        }
        public static Display Both
        {
            get {
                display.Mode = Target.Both;
                return display;
            }
        }

        public static Display Instance {
            set {
                display = value;
            }
        }
        abstract public Target Mode{set;}

        abstract public String LastMessage { get;}

        abstract public void Pause();
        abstract public void Play();

        public bool IsPaused
        {
            get { return paused; }
            set { paused = value; }
        }

        //Display Info
        abstract public void Show(String msg);
        abstract public void Show(String msg, object arg0);
        abstract public void Show(String msg, params object[] args);
        abstract public void Show(Confirm error);
        abstract public void Show(IProduct p);
        abstract public void Show(ICustomer customer);
        abstract public void Show(SalesItem si);
        abstract public void Show(VoidItem vi);
        abstract public void Show(Adjustment ai);
        abstract public void Show(SalesDocument sd);
        abstract public void Show(ICredit credit);
        abstract public void Show(ICurrency currency);
        abstract public void Show(MenuList menu);
        abstract public void Show(String totalMsg, Decimal total);
        abstract public void Show(String firstMsg,Decimal firstTotal,String secondMsg,Decimal secondTotal, bool firstLine);

        //Clear & Reset
        abstract public void Clear();
        abstract public void ClearError();
        abstract public void Reset();
        abstract public void SetBrightness(int level);

        //Text entry
        abstract public void Append(String msg);
        abstract public void Append(String msg, bool moveCursor);
        abstract public void BackSpace();
        abstract public void CursorNext();
        abstract public void CursorPrevious();
        abstract public int CurrentColumn { get;}
        abstract public void ShowAdvertisement();

        abstract public void LedOn (Leds leds);
        abstract public void LedOff(Leds leds);
        abstract public bool IsLedOn(Leds leds);

        //TODO This is butt-ugly code for HYDisplay!!
        public static String AmountPairFormat(String label1, Decimal amount1, String label2, Decimal amount2)
        {
            int numberLength = string.Format("{0:C}", new Number(amount1)).Length;
            if (label1.Length > 19 - numberLength)
                label1 = label1.Substring(0, 19 - numberLength);
            numberLength = string.Format("{0:C}", new Number(amount2)).Length;
            if (label2.Length > 19 - numberLength)
                label2 = label2.Substring(0, 19 - numberLength);
            return String.Format("{0}\t{1:C}\n{2}\t{3:C}", label1,
                                                           new Number(amount1),
                                                           label2,
                                                           new Number(amount2));

        }
        public static String FormatTotal(String totalMsg, Decimal total)
        {
            return String.Format("{0}\n{1:C}", totalMsg, new Number(total));
        }
        public static String DocumentFormat(SalesDocument doc)
        {
            String format = "{0}{4} {1}\t{2:dd/MM/yy}\n{3:C}\t{2:H:mm}";
            
            return String.Format(format, doc.Name.Substring(0, 3),
                                          doc.Id,
                                          doc.CreatedDate,
                                          new Number(doc.TotalAmount),
                                          (doc.Id > 999) ? ":" : " NO:");

        }
    }
}
