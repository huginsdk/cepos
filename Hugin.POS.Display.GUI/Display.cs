using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Data;
using Hugin.POS.Common;

namespace Hugin.POS.Display.GUI
{
    public class Display : IDisplay
    {
        public event ConsumeKeyHandler ConsumeKey;
        public event EventHandler DisplayClosed;
        public event SalesSelectedHandler SalesSelected;
        public event EventHandler SalesFocusLost;

        static IDisplay cashierDisplay = null;
        static IDisplay customerDisplay = null;
        static IDisplay display = null;
        static DisplayAttribute attributes = DisplayAttribute.None;

        Target mode;

        public Boolean HasAttribute(DisplayAttribute attr)
        {
            return (attr & attributes) == attr;
        }


        public static IDisplay Instance()
        {
            if (display == null)
            {
                String vga = PosConfiguration.Get("VGA");
                if (vga == "Customer")
                    display = new Display(new GuiDisplay(), new GraphicalDisplay(), Target.Both);
                else
                    display = display = new GuiDisplay(); 
            }
            return display;
        }

        public static IDisplay Instance(Target target)
        {
            if (display == null)
            {
                switch (target)
                {
                    case Target.Cashier:
                        display = new GuiDisplay();
                        break;
                    case Target.Customer:
                        display = new GraphicalDisplay();
                        break;
                    case Target.Both:
                        display = new Display(new GuiDisplay(), new GraphicalDisplay(), target);
                        break;
                }
            }
            return display;
        }

        internal static EZLogger Log
        {
            get { return EZLogger.Log; }
        }

        void customerDisplay_ConsumeKey(object sender, ConsumeKeyEventArgs e)
        {
            if (ConsumeKey != null)
                ConsumeKey(sender, e);
        }
        private Display(IDisplay cashierDisp, IDisplay customerDisp,Target target)
        {
            if ((target & Target.Cashier) == Target.Cashier)
            {
                cashierDisplay = cashierDisp;
                cashierDisplay.Mode = Target.Cashier;
                cashierDisplay.ConsumeKey += new ConsumeKeyHandler(customerDisplay_ConsumeKey);
            }
            if ((target & Target.Customer) == Target.Customer)
            {
                customerDisplay = customerDisp;
                customerDisplay.Mode = Target.Customer;
                customerDisplay.ConsumeKey += new ConsumeKeyHandler(customerDisplay_ConsumeKey);
                customerDisplay.DisplayClosed += new EventHandler(customerDisplay_DisplayClosed);
            }
        }

        void customerDisplay_DisplayClosed(object sender, EventArgs e)
        {
            DisplayClosed(this, e);
        }

        internal static String AmountPairFormat(String label1, Decimal amount1, String label2, Decimal amount2)
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

        internal static String FormatTotal(String totalMsg, Decimal total)
        {
            return String.Format("{0}\n{1:C}", totalMsg, new Number(total));
        }

        public Target Mode
        {
            set { mode = value; }
        }

        public void Pause()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Pause();
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Pause();
        }

        public void Play()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Play();
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Play();
        }

        public string LastMessage
        {
            get { return (mode == Target.Customer) ? customerDisplay.LastMessage : cashierDisplay.LastMessage; }
        }

        public void Show(string msg)
        {
            if ((mode & Target.Cashier) == Target.Cashier) 
                cashierDisplay.Show(msg);
            if ((mode & Target.Customer) == Target.Customer)  
                customerDisplay.Show(msg);
        }

        public void Show(string msg, object arg0)
        {
            if ((mode & Target.Cashier) == Target.Cashier) 
                cashierDisplay.Show(msg, arg0);
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Show(msg, arg0);
        }

        public void Show(string msg, params object[] args)
        {
            if ((mode & Target.Cashier) == Target.Cashier) 
                cashierDisplay.Show(msg, args);
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Show(msg, args);
        }

        public void Show(IConfirm error)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(error);
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Show(error);
        }

        public void Show(IProduct p)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(p);
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Show(p);
        }

        public void Show(ICustomer customer)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(customer);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Show(customer);
        }

        public void Show(IFiscalItem fi)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(fi);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Show(fi);
        }

        public void ShowSale(IFiscalItem fi)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.ShowSale(fi);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.ShowSale(fi);
        }

        public void ShowVoid(IFiscalItem fi)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.ShowVoid(fi);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.ShowVoid(fi);
        }

        public void Show(IAdjustment ai)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(ai);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Show(ai);
        }

        public void ShowCorrect(IAdjustment ai, bool isSubTotalAdj)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.ShowCorrect(ai, isSubTotalAdj);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.ShowCorrect(ai, isSubTotalAdj);
        }

        public void Show(ISalesDocument sd)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(sd);
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Show(sd);
        }

        public void Show(IMenuList menu)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(menu);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Show(menu);
        }

        public void Show(ICredit credit)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(credit);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Show(credit);
        }

        public void Show(ICurrency currency)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(currency);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Show(currency);
        }

        public void Show(String totalMsg, Decimal total)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(totalMsg, total);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Show(totalMsg, total);
        }
        public void Show(String firstMsg, Decimal firstTotal, String secondMsg, Decimal secondTotal, bool firstLine)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Show(firstMsg, firstTotal, secondMsg, secondTotal, firstLine);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.Show(firstMsg, firstTotal, secondMsg, secondTotal, firstLine);
        }
        public void Clear()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Clear();
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Clear();
        }

        public void ClearError()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.ClearError();
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.ClearError();
        }

        public void Reset()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Reset();
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Reset();
        }

        public void Append(string msg)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Append(msg);
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Append(msg);
        }

        public void Append(string msg, bool moveCursor)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.Append(msg, moveCursor);
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.Append(msg, moveCursor);
        }

        public void BackSpace()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.BackSpace();
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.BackSpace();
        }

        public void CursorNext()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.CursorNext();
            if ((mode & Target.Customer) == Target.Customer) 
                customerDisplay.CursorNext();
        }

        public void CursorPrevious()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.CursorPrevious();
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.CursorPrevious();
        }

        public int CurrentColumn
        {
            get
            {
                if ((mode & Target.Cashier) == Target.Cashier)
                    return cashierDisplay.CurrentColumn;
                if ((mode & Target.Customer) == Target.Customer)
                    return customerDisplay.CurrentColumn;
                else throw new Exception("DisplayCommon can not be found");
            }
        }

        public void ShowAdvertisement()
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.ShowAdvertisement();
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.ShowAdvertisement();
        }

        public void SetBrightness(int level)
        {
            if ((mode & Target.Cashier) == Target.Cashier)
                cashierDisplay.SetBrightness(level);
            if ((mode & Target.Customer) == Target.Customer)
                customerDisplay.SetBrightness(level);
        }

        public void LedOn(Leds leds)
        {
            cashierDisplay.LedOn(leds);
            customerDisplay.LedOn(leds);
        }

        public void LedOff(Leds leds)
        {
            cashierDisplay.LedOff(leds);
            customerDisplay.LedOff(leds);
        }

        public bool IsLedOn(Leds leds)
        {
            return cashierDisplay.IsLedOn(leds);
        }

        public bool Inactive
        {
            get
            {
                if ((mode & Target.Cashier) == Target.Cashier)
                    return cashierDisplay.Inactive;
                if ((mode & Target.Customer) == Target.Customer)
                    return customerDisplay.Inactive;
                return false;
            }
            set
            {
                if ((mode & Target.Cashier) == Target.Cashier)
                    cashierDisplay.Inactive = value;
                if ((mode & Target.Customer) == Target.Customer)
                    customerDisplay.Inactive = value;
            }
        }


        public bool IsPaused
        {
            get
            {
                if ((mode & Target.Cashier) == Target.Cashier)
                    return cashierDisplay.IsPaused;
                if ((mode & Target.Customer) == Target.Customer)
                    return customerDisplay.IsPaused;
                return false;
            }
        }

        public void ChangeDocumentStatus(ISalesDocument doc, DisplayDocumentStatus de)
        {
            customerDisplay.ChangeDocumentStatus(doc, de);
        }

        public void ChangeCustomer(ICustomer c)
        {
                customerDisplay.ChangeCustomer(c);
        }
        public bool HasGraphKeyboard()
        {
            return true;
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
