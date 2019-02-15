using System;
using Hugin.POS.Common;
using Hugin.POS.States;
using System.Collections.Generic;
namespace Hugin.POS
{
    public class DisplayAdapter
    {
        private static IDisplay display;

        private DisplayAdapter()
        {
            display = (IDisplay)Chassis.Engine;
        }

        public static IDisplay Instance()
        {
            if (display == null)
            {
                new DisplayAdapter();
            }
            return display;
        }
        public static IDisplay Cashier
        {
            get
            {
                if(display == null)
                {
                    Instance();
                }
                display.Mode = Target.Cashier;
                return display;
            }
        }
        public static IDisplay Customer
        {
            get {

                if (display == null)
                {
                    Instance();
                }
                display.Mode = Target.Customer;
                return display;
            }
        }
        public static IDisplay Both
        {
            get {
                display.Mode = Target.Both;
                return display;
            }
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
        public static String AmountPairFormat(String label1, Decimal amount1, String label2)
        {
            int numberLength = string.Format("{0:C}", new Number(amount1)).Length;
            if (label1.Length > 19 - numberLength)
                label1 = label1.Substring(0, 19 - numberLength);
            if (label2.Length > 20)
                label2 = label2.Substring(0, 20);
            return String.Format("{0}\t{1:C}\n{2}\t", label1,
                                                           new Number(amount1),
                                                           label2);

        }

        public static String FormatTotal(String totalMsg, Decimal total)
        {
            return String.Format("{0}\n{1:C}", totalMsg, new Number(total));
        }

        public static String DocumentFormat(ISalesDocument doc)
        {
            String format = "{0}{4} {1}\t{2:dd/MM/yy}\n{3:C}\t{2:H:mm}";

            return String.Format(format, doc.Name.Substring(0, 3),
                                          doc.Id,
                                          doc.CreatedDate,
                                          new Number(doc.TotalAmount),
                                          (doc.Id > 999) ? ":" : " NO:");

        }

        void CashRegister_DocumentChanged(object sender, EventArgs e)
        {
            display.ChangeDocumentStatus((ISalesDocument)sender, DisplayDocumentStatus.OnChange);
        }

        void SalesDocument_OnUndoAdjustment(object sender, EventArgs e)
        {
            display.ChangeDocumentStatus((ISalesDocument)sender, DisplayDocumentStatus.OnUndoAdjustment);
        }

        void SalesDocument_CustomerChanged(object sender)
        {
            display.ChangeCustomer((ICustomer)sender);
        }

        void SalesDocument_OnClose(object sender)
        {
            display.ChangeDocumentStatus((ISalesDocument)sender, DisplayDocumentStatus.OnClose);
        }

        void Login_OnLogin(ICashier sender)
        {
            if (CashRegister.Document != null && CashRegister.Document.Items.Count > 0)
                display.ChangeDocumentStatus(CashRegister.Document, DisplayDocumentStatus.OnChange);
        }

        void display_DisplayClosed(object sender, EventArgs e)
        {
            Chassis.Engine.Close();
            Chassis.Engine.Terminate = true;
        }
        List<int> keyStack = new List<int>();
        void display_ConsumeKey(object sender, ConsumeKeyEventArgs e)
        {
            PosKey key = (PosKey)e.KeyValue;

            switch (e.KeyValue)
            {
                case 16://Shift
                    if (keyStack.Count == 0 || keyStack.Count == 2)
                    {
                        keyStack.Add(e.KeyValue);
                        return;
                    }
                    break;
                case 66://B
                    if (keyStack.Count == 3)
                    {
                        keyStack.Add(e.KeyValue);
                        return;
                    }
                    break;
                default: break;
            }
            
            if (keyStack.Count > 0)
            {
                switch (key)
                {
                    case PosKey.D0:
                    case PosKey.D1:
                    case PosKey.D2:
                    case PosKey.D3:
                    case PosKey.D4:
                    case PosKey.D5:
                    case PosKey.D6:
                    case PosKey.D7:
                    case PosKey.D8:
                    case PosKey.D9:
                        keyStack.Add(e.KeyValue);
                        return;
                    case PosKey.Enter:
                        int current = 1;
                        if (keyStack.Count > 4 && keyStack[2] == 16)
                        {
                            current = 4;
                            while (true)
                            {
                                if (keyStack[current] < (int)PosKey.D0 ||
                                    keyStack[current] > (int)PosKey.D9)
                                    current++;
                                else
                                    break;
                            }
                            Chassis.Engine.Process(PosKey.MagstripeStx);
                        }
                        else
                        {
                            Chassis.Engine.Process(PosKey.BarcodePrefix);
                        }
                        while (current < keyStack.Count)
                        {
                            if (keyStack[current] < (int)PosKey.D0 || keyStack[current] > (int)PosKey.D9)
                                break;
                            Chassis.Engine.Process((PosKey)(keyStack[current]));
                            current++;
                        }
                        Chassis.Engine.Process(PosKey.Enter);
                        keyStack.Clear();
                        return;
                    default:                        
                        if (keyStack[keyStack.Count - 1] == (int)PosKey.Enter)
                        {
                            return;
                        }
                        if (keyStack.Count > 4 && keyStack[2] == 16 && e.KeyValue == 16)
                        {
                            keyStack.Insert(4, (int)PosKey.MagstripeStx);
                            keyStack.Add((int)PosKey.Enter);
                            return;
                        }
                        foreach (int keyVal in keyStack)
                        {
                            Chassis.Engine.Process((PosKey)keyVal);
                        }
                        keyStack.Clear();

                        return;
                }
            }
            Chassis.Engine.Process(key);
        }

        void SalesDocument_ItemSold(object sender, SaleEventArgs e)
        {
            display.ChangeDocumentStatus((ISalesDocument)sender, DisplayDocumentStatus.OnStart);
        }
    }
}
