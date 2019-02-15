using System;
using System.Text;
using System.Configuration;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using Hugin.POS.Common;
using Hugin.POS.Data;


namespace Hugin.POS.Display.GUI
{

    class GuiDisplay : IDisplay
    {
        public event ConsumeKeyHandler ConsumeKey;
        public event EventHandler DisplayClosed;
        public event SalesSelectedHandler SalesSelected;
        public event EventHandler SalesFocusLost;

        bool paused = false;

        private static GuiDisplayForm displayForm;
        private static bool errorRemoved = true;
        bool cursorOn = true;
        bool inactive = false;

        private readonly static char placer = (char)15;
        private String messageDisplaying = "".PadLeft(20, placer);
        private int currentColumn = 0;
        private bool errorStatus = false;
        private Leds ledStatus = 0;
        private Target currentTarget = Target.Both;
        private string cashierMessage;
        private string customerMessage;
        private static DisplayAttribute attributes = DisplayAttribute.None;

        public Boolean HasAttribute(DisplayAttribute attr)
        {
            return (attr & attributes) == attr;
        }


        internal GuiDisplay()
        {
            displayForm = new GuiDisplayForm();
            displayForm.Show();
            displayForm.ConsumeKey += new ConsumeKeyHandler(displayForm_ConsumeKey);
        }

        void displayForm_ConsumeKey(object sender, ConsumeKeyEventArgs e)
        {
            if (ConsumeKey != null)
                ConsumeKey(sender, e);
        }

        void listForm_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //Up Arrow or Down Arrow or Escape or Enter
            if (e.KeyValue == 38 || e.KeyValue == 40 || e.KeyValue == 27 || e.KeyValue == 13)
            {
                if (ConsumeKey != null)
                    ConsumeKey(sender, new ConsumeKeyEventArgs(e.KeyValue));
            }
        }


        //TODO Only here to maintain code compatibility with CF version
        internal GuiDisplay(object o)
        {
            displayForm = o as GuiDisplayForm;
            displayForm.Show();
            displayForm.ConsumeKey += new ConsumeKeyHandler(displayForm_ConsumeKey);
        }

        public Target Mode
        {
            set
            {
                currentTarget = value;
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
            if (paused) return;

            messageDisplaying = "".PadLeft(20, placer);
            currentColumn = 0;

            if (currentTarget != Target.Customer)
                cashierMessage = msg;
            else if (currentTarget != Target.Cashier)
                customerMessage = msg;

            displayForm.Show(msg, currentTarget);

        }

        public void Show(IConfirm err)
        {
            if (paused) return;

            Show(err.Message);
            //displayForm.Refresh();
            if (errorRemoved == true)
            {
                errorRemoved = false;
                System.Threading.ThreadStart thrStart = new System.Threading.ThreadStart(CheckBlock);
                System.Threading.Thread thr = new System.Threading.Thread(thrStart);
                thr.IsBackground = true;
                thr.Start();
            }
        }

        private static void CheckBlock()
        {
            while (true)
            {
                if (errorRemoved)
                {
                    break;
                }
#if !WindowsCE
                System.Console.Beep(400, 100);
#endif
            }
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
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
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
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
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
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void ShowVoid(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(fi.Quantity), large ? "X" : fi.Unit, new Number(fi.TotalAmount));

                String path = PosConfiguration.ImagePath + fi.Product.Barcode + ".jpg";
                System.Drawing.Image img = null;

                if (System.IO.File.Exists(path))
#if WindowsCE
                    img = new System.Drawing.Bitmap(path);
#else
                    img = System.Drawing.Image.FromFile(path);
#endif
                displayForm.ShowImage(img);
            }
            catch (FormatException fex)
            {
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
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
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C}", PosMessage.PRODUCT_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
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
            }
            catch (FormatException e)
            {
                EZLogger.Log.Error("Display error. {0}", e.Message);
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
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }
        public void Show(IMenuList menu)
        {
            try
            {
                ShowMenu(menu);
            }
            catch (FormatException fex)
            {
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void ShowMenu(IMenuList menuList)
        {
        }

        public void Show(ICredit credit)
        {
            try
            {
                Show("{0}\t{1}\n{2}", PosMessage.CREDIT, credit.Id, credit.Name);
            }
            catch (FormatException fex)
            {
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
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
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
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
            errorRemoved = true;
            if (paused) return;
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

            if (messageDisplaying.Trim(placer).Length >= 19)
                return;

            if (messageDisplaying.TrimEnd(placer).Length < 20 && currentColumn < 19)
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
                displayForm.ShowInput(messageDisplaying.Trim(placer), currentColumn, cursorOn);
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
            get { return (currentTarget == Target.Customer) ? customerMessage : cashierMessage; }
        }

        public void LedOn(Leds leds)
        {
            //DisplayCommon.Customer.LedOn(leds);
        }

        public void LedOff(Leds leds)
        {
            //DisplayCommon.Customer.LedOff(leds);
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

        public void LoginCashier(ICashier c)
        {
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
