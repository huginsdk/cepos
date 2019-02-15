using System;
using Hugin.POS.Common;
using System.Collections.Generic;

namespace Hugin.POS.Common
{
    [Flags]
    public enum Target { Cashier = 1, Customer = 2, Both = Cashier | Customer }

    [Flags]
    public enum Leds : byte
    {
        Online = 0x01,
        Sale = 0x02,
        Customer = 0x04,
        OnlineBlink = 0x08,
        Order = 0x10,
        All = Sale | Customer | Online,
        None = 0
    }

    [Flags]
    public enum DisplayAttribute 
    {
        None = 0,
        SerialKeyboard = 1,
        TouchKeyboard = 2,
        CashierKey = 4,
        DataManage = 8
    }

    [Flags]
    public enum DisplayDocumentStatus { OnStart, OnChange, OnClose, OnUndoAdjustment }

    public delegate void ConsumeKeyHandler(object sender, ConsumeKeyEventArgs e);

    public class ConsumeKeyEventArgs : EventArgs
    {
        int key;
        public ConsumeKeyEventArgs(int keyValue)
        {
            this.key = keyValue;
        }

        public int KeyValue
        {
            get { return key; }
        }
    }

    public delegate void SalesSelectedHandler(object sender, SalesSelectedEventArgs e);

    public class SalesSelectedEventArgs : EventArgs
    {
        string retVal;
        public SalesSelectedEventArgs(string retVal)
        {
            this.retVal = retVal;
        }
        public string RetVal
        {
            get { return retVal; }
        }
    }

    public interface IDisplay
    {
        event ConsumeKeyHandler ConsumeKey;
        event EventHandler DisplayClosed;
        event SalesSelectedHandler SalesSelected;
        event EventHandler SalesFocusLost;

        Target Mode { set;}
        Boolean HasAttribute(DisplayAttribute attribute);
        String LastMessage { get;}

        void Pause();
        void Play();

        //Display Info
        void Show(String msg);
        void Show(String msg, object arg0);
        void Show(String msg, params object[] args);
        void Show(IConfirm error);
        void Show(IProduct p);
        void Show(ICustomer customer);
        void Show(IFiscalItem fi);
        void ShowSale(IFiscalItem si);
        void ShowVoid(IFiscalItem vi);
        void Show(IAdjustment ai);
        void Show(ISalesDocument sd);
        void Show(ICredit credit);
        void Show(ICurrency currency);
        void Show(IMenuList menu);
        void Show(String totalMsg, Decimal total);
        void Show(String firstMsg, Decimal firstTotal, String secondMsg, Decimal secondTotal, bool firstLine);
        void ShowCorrect(IAdjustment adjustment, bool isSubTotalAdj);
        void ShowTableContent(ISalesDocument document, decimal totalAdjAmount);
        void ClearTableContent();
        bool ShowAlertMessage(string message);

        //Clear & Reset
        void Clear();
        void ClearError();
        void Reset();
        void SetBrightness(int level);

        //Text entry
        void Append(String msg);
        void Append(String msg, bool moveCursor);
        void BackSpace();
        void CursorNext();
        void CursorPrevious();
        int CurrentColumn { get;}
        void ShowAdvertisement();

        //Leds Control
        void LedOn(Leds leds);
        void LedOff(Leds leds);
        bool IsLedOn(Leds leds);

        //Display state
        bool Inactive { get; set;}
        bool IsPaused { get;}

        //Document 
        void ChangeDocumentStatus(ISalesDocument doc,DisplayDocumentStatus de);
        void ChangeCustomer(ICustomer c);

    }
}
