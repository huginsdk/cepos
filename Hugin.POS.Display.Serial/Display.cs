using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Data;
using Hugin.POS.Common;

namespace Hugin.POS.Display.Serial
{
    public class NotSerialException : Exception
    {
        public NotSerialException()
            : base("The keyboard is not serial")
        {
        }
    }

    public class Display : IDisplay
    {
        public event ConsumeKeyHandler ConsumeKey;
        public event EventHandler DisplayClosed;
        public event SalesSelectedHandler SalesSelected;
        public event EventHandler SalesFocusLost;

        IDisplay primaryDisplay;
        List<IDisplay> auxilaryDisplays;

        static IDisplay display = null;
        internal static bool Terminate = false;
        private static DisplayAttribute attributes = DisplayAttribute.SerialKeyboard;

        public Boolean HasAttribute(DisplayAttribute attr)
        {
            return (attr & attributes) == attr;
        }


        public static IDisplay Instance()
        {
            if (display == null)
            {
                try
                {
                    if (String.IsNullOrEmpty(PosConfiguration.Get("DisplayComPort")))
                    {
                        display = GUI.Display.Instance();
                        return display;
                    }
                    IDisplay d;
                    
                    d = new SerialDisplay();

                    String vga = PosConfiguration.Get("VGA");
                    if (vga == "Customer")
                        display = new Display(d,
                                             GUI.Display.Instance(Target.Customer),
                                             Target.Both);
                    else if (vga == "Cashier")
                        display = new Display(d,
                                             GUI.Display.Instance(Target.Cashier),
                                             Target.Both);
                    else display = new SerialDisplay();
                }
                catch (NotSerialException)
                {
                    display = new HYDisplay();
                }


            }
            return display;
        }

        private Display(IDisplay primaryDisplay, Target mode)
        {
            this.primaryDisplay = primaryDisplay;
            this.primaryDisplay.ConsumeKey += new ConsumeKeyHandler(primaryDisplay_ConsumeKey);
            this.Mode = mode;
        }

        private Display(IDisplay primaryDisplay, IDisplay secondaryDisplay, Target mode)
            : this(primaryDisplay, mode)
        {
            this.auxilaryDisplays = new List<IDisplay>();
            this.auxilaryDisplays.Add(secondaryDisplay);
            secondaryDisplay.ConsumeKey += new ConsumeKeyHandler(primaryDisplay_ConsumeKey);
            secondaryDisplay.DisplayClosed += new EventHandler(secondaryDisplay_DisplayClosed);
        }

        private Display(IDisplay primaryDisplay, IDisplay[] auxDisplays, Target mode)
            : this(primaryDisplay, mode)
        {
            this.auxilaryDisplays = new List<IDisplay>(auxDisplays);
        }

        public void AddAuxDisplay(IDisplay auxDisplay){
            if (auxilaryDisplays == null) auxilaryDisplays = new List<IDisplay>();
            auxilaryDisplays.Add(auxDisplay);
        }

        void primaryDisplay_ConsumeKey(object sender, ConsumeKeyEventArgs e)
        {
            ConsumeKey(sender, e);
        }

        void secondaryDisplay_DisplayClosed(object sender, EventArgs e)
        {
            Terminate = true;
            DisplayClosed(sender, e);
        }

        public Target Mode
        {   
            set { 
                primaryDisplay.Mode = value;
                if (auxilaryDisplays == null) return;
                foreach (IDisplay d in auxilaryDisplays)
                    d.Mode = value;
            }
        }

        public void Pause()
        {
            primaryDisplay.Pause();
            foreach (IDisplay d in auxilaryDisplays)
                d.Pause();
        }

        public void Play()
        {
            primaryDisplay.Play();
            foreach (IDisplay d in auxilaryDisplays)
                d.Play();
        }

        public string LastMessage
        {
            get { return primaryDisplay.LastMessage; }
        }

        public void Show(string msg)
        {
            primaryDisplay.Show(msg);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(msg);
        }

        public void Show(string msg, object arg0)
        {
            primaryDisplay.Show(msg, arg0);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(msg,arg0);
        }

        public void Show(string msg, params object[] args)
        {
            primaryDisplay.Show(msg, args);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(msg,args);
        }

        public void Show(IConfirm error)
        {
            primaryDisplay.Show(error);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(error);
        }

        public void Show(IProduct p)
        {
            primaryDisplay.Show(p);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(p);
        }

        public void Show(ICustomer customer)
        {
            primaryDisplay.Show(customer);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(customer);
        }

        public void ShowSale(IFiscalItem fi)
        {
            primaryDisplay.ShowSale(fi);
            foreach (IDisplay d in auxilaryDisplays)
                d.ShowSale(fi);
        }

        public void Show(IFiscalItem fi)
        {
            primaryDisplay.Show(fi);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(fi);
        }

        public void ShowVoid(IFiscalItem fi)
        {
            primaryDisplay.ShowVoid(fi);
            foreach (IDisplay d in auxilaryDisplays)
                d.ShowVoid(fi);
        }

        public void Show(IAdjustment ai)
        {
            primaryDisplay.Show(ai);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(ai);
        }

        public void ShowCorrect(IAdjustment ai, bool isSubTotalAdj)
        {
            primaryDisplay.ShowCorrect(ai, isSubTotalAdj);
            foreach (IDisplay d in auxilaryDisplays)
                d.ShowCorrect(ai, isSubTotalAdj);
        }

        public void Show(ISalesDocument sd)
        {
            primaryDisplay.Show(sd);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(sd);
        }
        
        public void Show(ICurrency currency)
        {
            primaryDisplay.Show(currency);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(currency);
        }

        public void Show(ICredit credit)
        {
            primaryDisplay.Show(credit);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(credit);
        }

        public void Show(String totalMsg, Decimal total)
        {
            primaryDisplay.Show(totalMsg, total);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(totalMsg, total);
        }
        public void Show(String firstMsg, Decimal firstTotal, String secondMsg, Decimal secondTotal, bool firstLine)
        {
            primaryDisplay.Show(firstMsg, firstTotal, secondMsg, secondTotal,firstLine);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(firstMsg, firstTotal, secondMsg, secondTotal, firstLine);
        }
        public void Show(IMenuList menu)
        {
            primaryDisplay.Show(menu);
            foreach (IDisplay d in auxilaryDisplays)
                d.Show(menu);
        }

        public void Clear()
        {
            primaryDisplay.Clear();
            foreach (IDisplay d in auxilaryDisplays)
                d.Clear();
        }

        public void ClearError()
        {
            primaryDisplay.ClearError();
            foreach (IDisplay d in auxilaryDisplays)
                d.ClearError();
        }

        public void Reset()
        {
            primaryDisplay.Reset();
            foreach (IDisplay d in auxilaryDisplays)
                d.Reset();
        }

        public void Append(string msg)
        {
            primaryDisplay.Append(msg);
            foreach (IDisplay d in auxilaryDisplays)
                d.Append(msg);
        }

        public void Append(string msg, bool moveCursor)
        {
            primaryDisplay.Append(msg, moveCursor);
            foreach (IDisplay d in auxilaryDisplays)
                d.Append(msg, moveCursor);
        }

        public void BackSpace()
        {
            primaryDisplay.BackSpace();
            foreach (IDisplay d in auxilaryDisplays)
                d.BackSpace();
        }

        public void CursorNext()
        {
            primaryDisplay.CursorNext();
            foreach (IDisplay d in auxilaryDisplays)
                d.CursorNext();
        }

        public void CursorPrevious()
        {
            primaryDisplay.CursorPrevious();
            foreach (IDisplay d in auxilaryDisplays)
                d.CursorPrevious();
        }

        public int CurrentColumn
        {
            get
            {
                return primaryDisplay.CurrentColumn;
            }
        }

        public void ShowAdvertisement()
        {
            primaryDisplay.ShowAdvertisement();
            foreach (IDisplay d in auxilaryDisplays)
                d.ShowAdvertisement();
        }

        public void SetBrightness(int level)
        {
            primaryDisplay.SetBrightness(level);
            foreach (IDisplay d in auxilaryDisplays)
                d.SetBrightness(level);
        }

        public void LedOn(Leds leds)
        {
            primaryDisplay.LedOn(leds);
            foreach (IDisplay d in auxilaryDisplays)
                d.LedOn(leds);
        }

        public void LedOff(Leds leds)
        {
            primaryDisplay.LedOff(leds);
            foreach (IDisplay d in auxilaryDisplays)
                d.LedOff(leds);
        }

        public bool IsLedOn(Leds leds)
        {
            return primaryDisplay.IsLedOn(leds);
        }

        public bool Inactive
        {
            get
            {
                return primaryDisplay.Inactive;
            }
            set
            {
                primaryDisplay.Inactive = value;
            }
        }

        public bool IsPaused
        {
            get
            {
                return primaryDisplay.IsPaused;
            }
        }

        public void ChangeDocumentStatus(ISalesDocument doc, DisplayDocumentStatus de)
        {
            primaryDisplay.ChangeDocumentStatus(doc, de);
            foreach (IDisplay d in auxilaryDisplays)
                d.ChangeDocumentStatus(doc, de);
        }

        public void ChangeCustomer(ICustomer c)
        {
            primaryDisplay.ChangeCustomer(c);
            foreach (IDisplay d in auxilaryDisplays)
                d.ChangeCustomer(c);
        }

        internal static EZLogger Log
        {
            get { return EZLogger.Log; }
        }
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
