using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;
using GradientButton;

namespace Hugin.POS.Display
{

    public partial class CustomerForm : Form
    {
        #region Declerations

        public event ConsumeKeyHandler ConsumeKey;
        public event EventHandler DisplayClosed;

        private const int LINE_LENGTH = 20;
        private const int MAX_ITEM_ON_GRID = 13;

        private static bool isSelling = false;
        private static bool onList = false;
        private static List<IFiscalItem> soldItems = null;
        private static decimal adjustmentAmount = 0.00m;

        private static Queue<String> pressedKeys = null;
        private static System.Threading.Thread threadExecuteKey = null;

        private Panel pnlKeyboard = null;
        
#endregion

        public CustomerForm()
        {
            InitializeComponent();

            if (IsMatrixAvailable())
            {
                this.lblSecondLine.Font = new System.Drawing.Font("MatrixSchedule", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
                this.lblFirstLine.Font = new System.Drawing.Font("MatrixSchedule", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            }

            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            this.Location = new Point(0, 0);

            //this.WindowState = FormWindowState.Maximized;

            soldItems = new List<IFiscalItem>();
            
            //Events
            this.KeyUp += new KeyEventHandler(CustomerForm_KeyUp);
            
            DataConnector.ProductsUpdated += new EventHandler(CustomerForm_ProductsUpdated);
            dgvReceipt.RowsAdded += new DataGridViewRowsAddedEventHandler(dgvReceipt_RowsAdded);
            dgvReceipt.LostFocus += new EventHandler(dgvReceipt_LostFocus);
            dgvReceipt.Click += new EventHandler(dgvReceipt_Click);
            this.KeyPreview = true;

            //Create keys
            AddWindow(WindowType.Function);
            AddWindow(WindowType.Program);
            AddWindow(WindowType.PluList);

            //Grid fonts
            clmnName.CellTemplate.Style.Font = new Font("Tahoma",10, FontStyle.Regular);
            clmnName.CellTemplate.Style.Alignment = DataGridViewContentAlignment.BottomLeft;
            clmnQuantity.CellTemplate.Style.Alignment = DataGridViewContentAlignment.BottomRight;
            clmnUnitPrice.CellTemplate.Style.Alignment = DataGridViewContentAlignment.BottomRight;
            clmnTotalAmount.CellTemplate.Style.Font = new Font("Tahoma", 10, FontStyle.Bold);
            clmnTotalAmount.CellTemplate.Style.Alignment = DataGridViewContentAlignment.BottomRight;

            //Footer instance
            InitializeFooter();

            
            //Date time timer
            Timer t = new Timer();
            t.Interval = 15000;
            t.Tick += new EventHandler(t_Tick);
            t.Enabled = true;

            // Keyboard, it must be initialized here for invoke required ability on next processes
            if (pnlKeyboard == null)
            {
                if (keyboard == null)
                    keyboard = new Keyboard();

                pnlKeyboard = new Panel();
                pnlKeyboard.Name = "pnlKeyboard";
                pnlKeyboard.Bounds = keyboard.Bounds;
                keyboard.Parent = pnlKeyboard;
                pnlKeyboard.Parent = this;
                //pnlKeyboard.BringToFront();
                pnlKeyboard.Visible = false;
                pnlKeyboard.Top = this.Height - keyboard.Height - 50;
                keyboard.ConsumeKey += new ConsumeKeyHandler(keyboard_ConsumeKey);
            }

            //Execute keys from queue one by one

            pressedKeys = new Queue<String>();

            threadExecuteKey = new System.Threading.Thread(new System.Threading.ThreadStart(ExecuteKeys));
            threadExecuteKey.IsBackground = true;
            threadExecuteKey.Start();
        }

        void CustomerForm_KeyUp(object sender, KeyEventArgs e)
        {
            ConsumeKey(sender, new ConsumeKeyEventArgs(e.KeyValue));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DisplayClosed != null)
                DisplayClosed(this, e);
        }

        private void InitializeFooter()
        {
            pbxOnlineLed.Tag = 0;
            pbxSale.Tag = 0;
            pbxCustomer.Tag = 0;

            SetControlText(lblRegisterId, PosConfiguration.Get("RegisterId"));
            SetControlText(lblDateTime, String.Format("{0}\n\t{1:HH.mm}", DateTime.Now.ToLongDateString(), DateTime.Now));
        }

        #region Form Events

        void t_Tick(object sender, EventArgs e)
        {
            SetControlText(lblDateTime, String.Format("{0}\n\t{1:HH.mm}", DateTime.Now.ToLongDateString(), DateTime.Now));
        }

        void dgvReceipt_Click(object sender, EventArgs e)
        {
            if (onList && dgvReceipt.SelectedRows.Count > 0 && ConsumeKey != null)
            {
                PosKey key = (PosKey)Enum.Parse(typeof(PosKey), "D" + (dgvReceipt.SelectedRows[0].Index + 1));
                ConsumeKey(this, new ConsumeKeyEventArgs((int)key));
            }
        }

        void dgvReceipt_LostFocus(object sender, EventArgs e)
        {
            dgvReceipt.ClearSelection();
        }

        void dgvReceipt_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (dgvReceipt.Rows.Count > MAX_ITEM_ON_GRID)
            {
                dgvReceipt.FirstDisplayedScrollingRowIndex = dgvReceipt.Rows.Count - 1;
                if (!gfbDownArrow.Visible)
                {
                    gfbDownArrow.Visible = true;
                    gfbUpArrow.Visible = true;
                }
            }
            if (dgvReceipt.Rows.Count <= MAX_ITEM_ON_GRID)
            {
                if (gfbDownArrow.Visible)
                {
                    gfbDownArrow.Visible = false;
                    gfbUpArrow.Visible = false;
                }
            }
        }

        #endregion

        #region Main Panel Control

        void CustomerForm_ProductsUpdated(object sender, EventArgs e)
        {
            try
            {
                Skin.UpdateProducts();
                AddWindow(WindowType.PluList);

            }
            catch (Exception ex)
            {
                EZLogger.Log.Error(ex);
            }
        }

        private void AddWindow(WindowType windowsType)
        {
            List<Skin> skins = Skin.GetSkin(windowsType);
            ReplaceBottons(skins);
        }

        private void ApplySkin(List<Skin> skins)
        {
            ReplaceBottons(skins);
        }

        private void ReplaceBottons(List<Skin> skins)
        {
            foreach (Skin skin in skins)
            {
                Control c = tblKeypad.GetControlFromPosition(skin.CellInfo.X, skin.CellInfo.Y);

                if (c == null)
                {
                    CreateButton(skin);
                }
                else
                {
                    SetControlProperty(c, "Name", skin.Name);
                    SetControlProperty(c, "Text", skin.Text);
                    SetControlProperty(c, "Font", skin.Font);
                    SetControlProperty(c, "ForeColor", skin.ForeColor); ;
                    SetControlProperty(((GradientFilledButton)c), "StartColor", skin.StartColor);
                    SetControlProperty(((GradientFilledButton)c), "EndColor", skin.EndColor);
                }
            }
        }

        private void CreateButton(List<Skin> skins)
        {
            foreach (Skin skin in skins)
            {
                CreateButton(skin);
            }
        }

        private void CreateButton(Skin skin)
        {
            GradientFilledButton gfb = new GradientFilledButton();
            gfb.Name = skin.Name;
            gfb.Text = skin.Text;
            gfb.StartColor = skin.StartColor;
            gfb.EndColor = skin.EndColor;
            gfb.ForeColor = skin.ForeColor;
            gfb.Dock = DockStyle.Fill;
            gfb.Font = skin.Font;
            gfb.Margin = new System.Windows.Forms.Padding(1);
            gfb.AutoSize = false;
            gfb.Click += new EventHandler(gfb_Click);
            gfb.GotFocus += new EventHandler(gfb_GotFocus);
            tblKeypad.Controls.Add(gfb, skin.CellInfo.X, skin.CellInfo.Y);
        }

        #endregion

        #region Public Functions

        public void Show(String message)
        {
            //For initalize messageDisplaying
            Display.Instance().ClearError();

            if (!Str.Contains(message, "\n") && message.Length > 20)
            {
                String[] words = message.Split(' ');

                String tmp = words[0];
                for (int i = 1; i < words.Length; i++)
                {
                    if (tmp.Length + words[i].Length + 1 > 20)
                    {
                        tmp = tmp + "\n" + words[i];
                    }
                    else
                    {
                        tmp = tmp + " " + words[i];
                    }               
                }

                message = tmp;
            }

            if (!Str.Contains(message, "\n")) message = message + "\n";
            String[] lines = message.Split('\n');
            if (lines[0] != "")
                SetControlText(lblFirstLine, AdjustMessageLine(lines[0]));
            lblSecondLine.IsCaretEnable = false;
            SetControlText(lblSecondLine, AdjustMessageLine(lines[1]));
            if (isSelling &&
             GetButtonText("btnFee") == "+" &&
             !Str.Contains(GetControlText(lblSecondLine), ','))
                ChangeAdjustmentText(AdjustmentFactor.Percentage);
        }

        private string GetButtonText(string buttonName)
        {
            string text="";
            Control[] controls = tblKeypad.Controls.Find(buttonName, false);
            if (controls.Length > 0)
                text = controls[0].Text;
            return text;
        }

        public void Show(String format, params object[] args)
        {
            Show(String.Format(format, args));
        }

        public void Show(IAdjustment adjustment)
        {
            try
            {
                if (adjustment.Target is IFiscalItem)
                {
                    if (adjustment.Method == (AdjustmentType.PercentDiscount))
                        Show("{0}\n-{1:P}\t{2:C} ", Common.PosMessage.PRODUCT_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C} ", Common.PosMessage.PRODUCT_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n{1}\t{2:C}", Common.PosMessage.SUBTOTAL_PRICE_DISCOUNT, PosMessage.AMOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n{1}\t{2:C}", Common.PosMessage.SUBTOTAL_PRICE_FEE, PosMessage.AMOUNT,
                                             new Number(adjustment.NetAmount));
                    AddAdjustment(adjustment);

                }
                else if (adjustment.Target is ISalesDocument)
                {
                    if (adjustment.Method == (AdjustmentType.PercentDiscount))
                        Show("{0}\n-{1:P}\t{2:C}", Common.PosMessage.SUBTOTAL_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.SUBTOTAL_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n{1}\t{2:C}", Common.PosMessage.SUBTOTAL_PRICE_DISCOUNT, PosMessage.AMOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n{1}\t{2:C}", Common.PosMessage.SUBTOTAL_PRICE_FEE, PosMessage.AMOUNT,
                                             new Number(adjustment.NetAmount));
                    ShowAdjustment(adjustment.NetAmount);
                }
                else throw new InvalidProgramException("Adjustment target is incorrectly set");
                UpdateTotals();
            }
            catch (FormatException e)
            {
                CustomerForm.Log.Error("DisplayBase error. {0}", e.Message);
            }
        }

        private void ShowAdjustment(decimal netAmount)
        {
            adjustmentAmount += netAmount;
            SetControlText(lblAdjustmentAmount, String.Format("{0:C}", new Number(adjustmentAmount)));
            SetControlText(lblAdjustment, adjustmentAmount > 0 ? PosMessage.FEE : PosMessage.REDUCTION);
        }

        public void Show(IFiscalItem fi)
        {
            try
            {
                string specifier = (fi.Quantity != (long)fi.Quantity) ? "F4" : "F0";
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount.ToString(specifier).Length > 8);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)).ToString(specifier), large ? "X" : fi.Unit, new Number(fi.TotalAmount - fi.VoidAmount));
            }
            catch (FormatException fex)
            {
                CustomerForm.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }


        public void ShowSale(IFiscalItem fi)
        {
            try
            {
                string specifier = (fi.Quantity != (long)fi.Quantity) ? "F4" : "F0";
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount.ToString(specifier).Length > 8);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)).ToString(specifier), large ? "X" : fi.Unit, new Number(fi.TotalAmount - fi.VoidAmount));
                soldItems.Add(fi);
                //ShowItems();
                AddItem(fi);
                UpdateTotals();
            }
            catch (FormatException fex)
            {
                CustomerForm.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void ShowVoid(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount.ToString().Length > 8);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(fi.Quantity), large ? "X" : fi.Unit, new Number(fi.TotalAmount));
                soldItems.Add(fi);
                ShowItems();
                //AddItem(fi);
                //UpdateTotals();
            }
            catch (FormatException fex)
            {
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        private void ShowItems()
        {
            ClearGrid();
            List<IFiscalItem> items = new List<IFiscalItem>();

            items = soldItems.FindAll(delegate(IFiscalItem fi)
                        {
                            return fi.Quantity > fi.VoidQuantity;
                        });


            foreach (IFiscalItem fi in items)
            {
                AddItem(fi);
            }

            UpdateTotals();
        }

        public void ShowTableContent(ISalesDocument document, decimal totalAdjAmount)
        {
            ClearGrid();

            foreach (IFiscalItem fi in document.Items)
            {
                AddItem(fi);
            }

            UpdateTotals(document.TotalAmount, totalAdjAmount);
        }

        private delegate void ClearGridDelegate();
        public void ClearGrid()
        {

            if (dgvReceipt.InvokeRequired)
            {
                dgvReceipt.Invoke(new ClearGridDelegate(ClearGrid));
            }
            else
            {
                dgvReceipt.Rows.Clear();
            }
        }

        private delegate void RefreshButtonDelegate(Button btn);
        public void RefreshButton(Button btn)
        {
            if (btn.InvokeRequired)
            {
                btn.Invoke(new RefreshButtonDelegate(RefreshButton), btn);
            }
            else
            {
                btn.Refresh();
            }
        }

        private delegate void BringToFrontControlDelegate(Control control);
        public void BringToFront(Control control)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new BringToFrontControlDelegate(BringToFront), control);
            }
            else
            {
                control.BringToFront();
            }
        }

        private delegate void ShowControlDelegate(Control control);
        public void ShowControl(Control control)
        {
            if (control.InvokeRequired)
                control.Invoke(new ShowControlDelegate(ShowControl), control);
            else
                control.Show();
        }

        private delegate void HideControlDelegate(Control control);
        public void HideControl(Control control)
        {
            if (control.InvokeRequired)
                control.Invoke(new HideControlDelegate(HideControl), control);
            else
                control.Hide();
        }


              
        private delegate void AddItemDelegate(IFiscalItem fi);
        private void AddItem(IFiscalItem fi)
        {
            if (dgvReceipt.InvokeRequired)
            {
                dgvReceipt.Invoke(new AddItemDelegate(AddItem), fi);
            }
            else
            {
                string specifier = (fi.Quantity != (long)fi.Quantity) ? "F4" : "F0";
                dgvReceipt.Rows.Add(new object[] { fi.Name,
                                        String.Format("{0:P}", new Number(Department.TaxRates[fi.TaxGroupId])),
                                        String.Format("{0:G} x", new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)).ToString(specifier))+ 
                                        String.Format("{0:C}",new Number(fi.UnitPrice)), 
                                        String.Format("{0:C}",new Number(fi.TotalAmount - fi.VoidAmount)) });

                string[] itemAdjustments = fi.GetAdjustments();
                foreach (string adjustment in itemAdjustments)
                {
                    string label = "";
                    string sign = "";
                    string[] splitted = adjustment.Split('|');
                    decimal amount = decimal.Parse(splitted[0]);
                    if (splitted[1] == "--")
                    {
                        label = amount > 0 ? PosMessage.FEE : PosMessage.REDUCTION;
                    }
                    else
                    {
                        sign = amount > 0 ? "+" : "-";
                        label = String.Format("{0}%{1}", sign, splitted[1]);
                    }

                    dgvReceipt.Rows.Add(new object[] { label, 
                                            null,
                                            null,
                                            String.Format("{0:C}",new Number(amount)) });

                }

                dgvReceipt.ClearSelection();

            }
        }

        private delegate void AddAdjustmentDelegate(IAdjustment adj);
        private void AddAdjustment(IAdjustment adj)
        {
            if (dgvReceipt.InvokeRequired)
            {
                dgvReceipt.Invoke(new AddAdjustmentDelegate(AddAdjustment), adj);
            }
            else
            {
                string label = "";
                string sign = "";
                if (adj.Method== AdjustmentType.Discount ||
                    adj.Method == AdjustmentType.Fee)
                {
                    label = adj.NetAmount > 0 ? PosMessage.FEE : PosMessage.REDUCTION;
                }
                else
                {
                    sign = adj.NetAmount > 0 ? "+" : "-";
                    label = String.Format("{0}%{1:D2}", sign, (int)adj.RequestValue);
                }
                dgvReceipt.Rows.Add(new object[] { label, 
                                            null,
                                            null,
                                            String.Format("{0:C}",new Number(adj.NetAmount)) });
            }
        }

        private delegate void RemoveItemDelegate(int index);
        private void RemoveItem(int index)
        {
            if (dgvReceipt.InvokeRequired)
            {
                dgvReceipt.Invoke(new RemoveItemDelegate(RemoveItem), index);
            }
            else
            {
                dgvReceipt.Rows.RemoveAt(index);
            }
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
                CustomerForm.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        private static Keyboard keyboard = null;
        public void Show(ICustomer customer)
        {
            try
            {
                if (customer == null)
                {
                    ShowKeyboard();
                    return;
                }

                Show("{0}", customer.Name);
            }
            catch (FormatException fex)
            {
                CustomerForm.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        private void ShowKeyboard()
        {
            /*if (pnlKeyboard == null)
            {
                if (keyboard == null)
                    keyboard = new Keyboard();

                pnlKeyboard = new Panel();
                //pnlKeyboard.Name = "pnlKeyboard";
                SetControlProperty(pnlKeyboard, "Name", "pnlKeyboard");
                //pnlKeyboard.Bounds = keyboard.Bounds;
                SetControlProperty(pnlKeyboard, "Bounds", keyboard.Bounds);
                //keyboard.Parent = pnlKeyboard;
                SetControlProperty(keyboard, "Parent", pnlKeyboard);
                //pnlKeyboard.Parent = this;
                //SetControlParent(pnlKeyboard, GetControlProperty(pnlKeypad, "Parent"));
                SetControlProperty(pnlKeyboard, "Parent", GetControlProperty(pnlKeypad, "Parent"));
                pnlKeyboard.BringToFront();
                pnlKeyboard.Top = this.Height - keyboard.Height - pnlFooter.Height;
                keyboard.ConsumeKey += new ConsumeKeyHandler(keyboard_ConsumeKey);
            }*/
            //pnlKeyboard.Visible = true;
            SetControlProperty(pnlKeyboard, "Visible", true);
            //pnlKeyboard.BringToFront();
            BringToFront(pnlKeyboard);
            //pnlKeyboard.Show();
            ShowControl(pnlKeyboard);
            //tblKeypad.Enabled = false;
            SetControlProperty(tblKeypad, "Enabled", false);
        }

        void keyboard_ConsumeKey(object sender, ConsumeKeyEventArgs e)
        {
            if (ConsumeKey != null)
                ConsumeKey(sender, e);
        }

        private void HideKeyboard()
        {
            //pnlKeyboard.Hide();
            HideControl(pnlKeyboard);
            //tblKeypad.Enabled = true;
            SetControlProperty(tblKeypad, "Enabled", true);

        }

        public void Show(ISalesDocument doc)
        {
            try
            {
                Show("{0}\n{1}", (doc.IsEmpty) ? PosMessage.SELECT_DOCUMENT : PosMessage.TRANSFER_DOCUMENT, doc.Name);
            }
            catch (FormatException fex)
            {
                CustomerForm.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        internal void ShowCorrect(IAdjustment adjustment, bool isSbTotalAdj)
        {
            try
            {
                if (adjustment.Method == (AdjustmentType.PercentDiscount) ||
                   (adjustment.Method == AdjustmentType.PercentFee))
                    Show("{0}\n{1:P}\t{2:C} ", Common.PosMessage.CORRECTION,
                                         new Number(adjustment.RequestValue / 100),
                                         new Number(adjustment.NetAmount));
                else
                    Show("{0}\n \t{1:C} ", Common.PosMessage.CORRECTION,
                                          new Number(adjustment.NetAmount));

                if (adjustment.Target is ISalesDocument)
                {
                    ShowAdjustment(adjustment.NetAmount * Decimal.MinusOne);
                }
            }
            catch (FormatException e)
            {
                CustomerForm.Log.Error("Display error. {0}", e.Message);
            }

            if (!isSbTotalAdj)
                RemoveItem(dgvReceipt.Rows[dgvReceipt.Rows.Count - 1].Index);

            UpdateTotals();
        }
        public void ShowMenu(IMenuList menuList)
        {
            if (menuList == null)
            {
                onList = false;
                return;
            }
            try
            {
                string str = "";
                try
                {
                    str = menuList.GetEnumerator().Current.ToString();
                }
                catch
                {
                    menuList.MoveFirst();
                    str = menuList.GetEnumerator().Current.ToString();
                }
                if (str == PosMessage.ENTER_CALCULATOR)
                {
                    //TODO Sets Calculator.
                    return;
                }
                else if (str == PosMessage.EXIT_CALCULATOR)
                {
                    ShowFuncKeys(!isSelling);
                    return;
                }
            }
            catch { }

            onList = true;
        }

        public void Show(ICredit credit)
        {
            try
            {
                if (credit == null)
                {
                    HideKeyboard();
                    return;
                }
                onList = true;
                Show("{0}\t{1}\n{2}", PosMessage.CREDIT, credit.Id, credit.Name);
            }
            catch (FormatException fex)
            {
                CustomerForm.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        private void ShowFuncKeys(bool enable)
        {
            isSelling = !enable;
            AddWindow(isSelling ? WindowType.Payment : WindowType.Program);
        }

        public void Show(ICurrency currency)
        {
            try
            {
                Show("{0}\n{1}\tx {2:C}", PosMessage.FOREIGNCURRENCY, currency.Name, currency.ExchangeRate);
            }
            catch (FormatException fex)
            {
                CustomerForm.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }
        private delegate void ShowInputDelegate(String message, int currentColumn, bool cursorOn);
        public void ShowInput(String message, int currentColumn, bool cursorOn)
        {
            if (lblSecondLine.InvokeRequired)
                lblSecondLine.Invoke(new ShowInputDelegate(this.ShowInput), message, currentColumn, cursorOn);
            else
            {
                lblSecondLine.IsCaretEnable = cursorOn;
                if (cursorOn)
                {
                    if (currentColumn >= message.Length)
                        message = message + " ";

                    if (currentColumn < LINE_LENGTH)
                        lblSecondLine.Text = message.Substring(0, currentColumn);
                    else
                        lblSecondLine.Text = message.Substring(currentColumn - LINE_LENGTH + 1, LINE_LENGTH);

                    lblSecondLine.CaretIndex = Math.Min(currentColumn, (LINE_LENGTH-1));
                    lblSecondLine.Text += message.Substring(currentColumn, message.Length - currentColumn);
                }
                else
                    lblSecondLine.Text = message;
                //lblSecondLine.Refresh();
            }

            if (isSelling && (GetButtonText("btnFee") == "+") & !Str.Contains(lblSecondLine.Text, ','))
                ChangeAdjustmentText(AdjustmentFactor.Percentage);

            else if (isSelling &&
                        GetButtonText("btnFee") == "+%" &&
                        lblSecondLine.Text.Trim().Length > 0 &&
                        Str.Contains(lblSecondLine.Text, ',') &&
                        Str.Contains(lblSecondLine.Text, PosMessage.ENTER_NUMBER))
                ChangeAdjustmentText(AdjustmentFactor.Amount);
        }
        internal static EZLogger Log
        {
            get { return EZLogger.Log; }
        }
        public virtual bool KeyAvailable
        {
            get { return false; }
        }
        #endregion Public Functions

        #region Document States

        internal void SetDocumentInfos(ISalesDocument doc)
        {
            ShowFuncKeys(false);
        }

        internal void ChangeDocument(ISalesDocument doc)
        {
        }

        internal void ChangeCustomer(ICustomer c)
        {
            if (c != null)
            {
                pbxCustomer.Image = Hugin.POS.Display.Properties.Resources.customer;
                SetControlText(lblCustmoer, c.Name);
            }
            else
            {
                pbxCustomer.Image = Hugin.POS.Display.Properties.Resources.nocustomer;
                SetControlText(lblCustmoer, PosMessage.NO_CUSTOMER);
            }
        }

        private void InitializeDocument()
        {
            soldItems = new List<IFiscalItem>();
            //dgvReceipt.Rows.Clear();
            ClearGrid();  
            adjustmentAmount = 0.00m;
            ShowAdjustment(0);
            //gfbDownArrow.Visible = false;
            //gfbUpArrow.Visible = false;
            SetControlVisible(gfbDownArrow, false);
            SetControlVisible(gfbUpArrow, false);
            UpdateTotals();
            ChangeCustomer(null as ICustomer);
        }

        private void UpdateTotals()
        {
            decimal subTotal = 0.00m;
            soldItems.ForEach(delegate(IFiscalItem fi)
            {
                if (fi.TotalAmount > fi.VoidAmount)
                    subTotal += (fi.TotalAmount - fi.VoidAmount);
            });

            subTotal += adjustmentAmount;
            SetControlText(lblTotalAmount, String.Format("{0:C}", new Number(subTotal)));

        }

        private void UpdateTotals(decimal total, decimal adjustmentAmount)
        {
            SetControlText(lblTotalAmount, String.Format("{0:C}", new Number(total)));
            SetControlText(lblAdjustmentAmount, String.Format("{0:C}", new Number(adjustmentAmount)));
        }

        internal void DocumentClose(ISalesDocument doc)
        {
            InitializeDocument();
            ShowFuncKeys(true);
        }

        #endregion

        #region DisplayBase Value Settings
        /* DisplayBase values are set by a couple of functions : 1) delegate function 2) non-delegate function
         * When non-delegate function is called by external function, if call belongs to same thread sets the value
         * else it calls delegate function
         * */
        private delegate void SetControlTextDelegate(Control cntrl, String text);
        private void SetControlText(Control cntrl, String text)
        {
            if (cntrl.InvokeRequired)
                cntrl.Invoke(new SetControlTextDelegate(SetControlText), cntrl, text);
            else
            {
                cntrl.Text = text;
                //?why // ed// cntrl.Invalidate();
                cntrl.Refresh();
            }
        }
        public delegate void MethodInvoker();
        private string GetControlText(Control cntrl)
        {
            string text = "";
            if (cntrl.InvokeRequired)
            {
                cntrl.Invoke(new MethodInvoker(delegate { text = cntrl.Text; }));
            }

            return text;
        }

        private object GetControlProperty(Control cntrl, String properyName)
        {
            object propertyValue = null;

            if (cntrl.InvokeRequired)
            {
                cntrl.Invoke(new MethodInvoker(delegate
                {
                    System.Reflection.PropertyInfo pi = cntrl.GetType().GetProperty(properyName);
                    if (pi.PropertyType.IsAssignableFrom(properyName.GetType()))
                    {
                        propertyValue = pi.GetValue(cntrl, null);
                    }
                }));
            }

            return propertyValue;
        }

        private delegate void SetControlPropertyDelegate(Control control, string propertyName, object propertyValue);
        private void SetControlProperty(Control control, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
                control.Invoke(new SetControlPropertyDelegate(SetControlProperty), control, propertyName, propertyValue);
            else
            {
                System.Reflection.PropertyInfo pi = control.GetType().GetProperty(propertyName);
                if (pi != null)
                {
                    if (pi.PropertyType.IsAssignableFrom(propertyValue.GetType()))
                        pi.SetValue(control, propertyValue, null);
                    control.Refresh();
                }
            }
        }

        private delegate void SetControlParentDelegate(Control childControl, Control parentControl);
        private void SetControlParent(Control childControl, Control parentControl)
        {
            if (childControl.InvokeRequired)
            {
                childControl.Invoke(new SetControlParentDelegate(SetControlParent), childControl, parentControl);
            }
            else
                childControl.Parent = parentControl;

        }


        private delegate void SetPictureBoxImageDelegate(PictureBox pbx, Image img);
        private void SetPictureBoxImage(PictureBox pbx, Image img)
        {
            if (pbx.InvokeRequired)
                pbx.Invoke(new SetPictureBoxImageDelegate(SetPictureBoxImage), pbx, img);
            else
            {
                pbx.Image = img;
                pbx.Invalidate();
            }
        }
        private delegate void SetGradientImageDelegate(GradientFilledButton cntrl, Image img);
        private void SetGradientBoxImage(GradientFilledButton cntrl, Image img)
        {
            if (cntrl.InvokeRequired)
                cntrl.Invoke(new SetGradientImageDelegate(SetGradientBoxImage), cntrl, img);
            else
            {
                cntrl.Image = img;
                cntrl.Invalidate();
            }
        }

        private delegate void SetGradientColorDelegate(GradientFilledButton cntrl, Color startColor, Color endColor);
        private void SetGradientColor(GradientFilledButton cntrl, Color startColor, Color endColor)
        {
            if (cntrl.InvokeRequired)
                cntrl.Invoke(new SetGradientColorDelegate(SetGradientColor), cntrl, startColor, endColor);
            else
            {
                cntrl.StartColor = startColor;
                cntrl.EndColor = endColor;
                cntrl.Invalidate();
            }
        }

        private delegate void SetControlVisibleDelegate(GradientFilledButton gfb, bool enable);
        private void SetControlVisible(GradientFilledButton gfb, bool enable)
        {
            if (gfb.InvokeRequired)
                gfb.Invoke(new SetControlVisibleDelegate(SetControlVisible), gfb, enable);
            else
            {
                gfb.Visible = enable;
                gfb.Refresh();
            }
        }

        internal void SetLed(Leds led)
        {
            if (
                (((led & Leds.Online) == Leds.Online) && (Convert.ToInt32(pbxOnlineLed.Tag) == 0)) ||
                (((led & Leds.Online) != Leds.Online) && (Convert.ToInt32(pbxOnlineLed.Tag) == 1))
                )
                ChangeMod(pbxOnlineLed);
            if (
                (((led & Leds.Sale) == Leds.Sale) && (Convert.ToInt32(pbxSale.Tag) == 0)) ||
                (((led & Leds.Sale) != Leds.Sale) && (Convert.ToInt32(pbxSale.Tag) == 1))
                )
                ChangeMod(pbxSale);
        }

        private delegate void ChangeModDelegate(System.Windows.Forms.PictureBox lblLed);
        private void ChangeMod(System.Windows.Forms.PictureBox lblLed)
        {
            if (lblLed.InvokeRequired)
                lblLed.Invoke(new ChangeModDelegate(this.ChangeMod), lblLed);
            else
            {
                if (Convert.ToInt32(lblLed.Tag) == 0)
                {
                    lblLed.Tag = 1;
                    lblLed.Image = Hugin.POS.Display.Properties.Resources.ledon;
                }
                else
                {
                    lblLed.Tag = 0;
                    lblLed.Image = Hugin.POS.Display.Properties.Resources.ledoff;
                }
            }
        }


        #endregion DisplayBase Value Settings

        #region Helper Functions

        private IDataConnector DataConnector
        {
            get
            {
                return Data.Connector.Instance();
            }
        }

        private string AdjustMessageLine(string line)
        {
            if (Str.Contains(line, "\t"))
                line = line.Replace("\t", " ".PadRight(20 - line.Length + 1));
            else
                line = line.PadLeft(line.Length + ((20 - line.Length) / 2), ' ');
            if (Str.Contains(line, "&"))
                line = line.Replace("&", " ");
            return String.Format("{0,-20}", line);
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
        private string FormatCustomerLine(string customerline)
        {
            if (customerline.Length > 16) customerline = customerline.Substring(0, 16);
            if (customerline.Length < 16)
            {
                int left = (16 - customerline.Length) / 2;
                int right = 16 - customerline.Length - left;
                customerline = "".PadLeft(left) + customerline + "".PadRight(right);
            }
            return customerline;
        }
        private decimal CalculateAdjustment(IAdjustment[] list)
        {
            return CalculateAdjustment(new List<IAdjustment>(list));
        }
        private decimal CalculateAdjustment(List<IAdjustment> list)
        {
            decimal totalAdj = 0;
            foreach (IAdjustment adj in list)
                totalAdj = totalAdj + adj.NetAmount;
            return totalAdj;
        }

        #endregion Helper Functions

        #region Consume Key Functions

        void gfb_GotFocus(object sender, EventArgs e)
        {
            this.lblFirstLine.Focus();
        }

        private void ExecuteKeys()
        {
            while (true)
            {
                try
                {
                    if (pressedKeys.Count > 0)
                        SendButton(pressedKeys.Dequeue());
                    System.Threading.Thread.Sleep(50);
                }
                catch
                {
                }
            }
        }

        private void SendButton(string buttonName)
        {
            //if (Display.Instance().IsPaused) return;
            //if (!(sender is GradientFilledButton)) return;

            PosKey key = PosKey.UndefinedKey;
            int buffer = -1;
            //string buttonName = ((GradientFilledButton)sender).Name;
            switch (buttonName)
            {
                case "btnPLU20":
                case "btnPLU19":
                case "btnPLU18":
                case "btnPLU17":
                case "btnPLU16":
                case "btnPLU15":
                case "btnPLU14":
                case "btnPLU13":
                case "btnPLU12":
                case "btnPLU11":
                case "btnPLU10":
                case "btnPLU9":
                case "btnPLU8":
                case "btnPLU7":
                case "btnPLU6":
                case "btnPLU5":
                case "btnPLU4":
                case "btnPLU3":
                case "btnPLU2":
                case "btnPLU1":
                    buffer = int.Parse(buttonName.Substring(6)) - 1;
                    KeyMap.LabelBuffer = buffer + ((Skin.PLUPageNo - 1) * Skin.NUMBER_OF_PLU_ON_PAGE);
                    key = PosKey.LabelStx;
                    break;
                case "btnF3":
                case "btnF2":
                case "btnF1":
                    buffer = int.Parse(buttonName.Substring(4));
                    KeyMap.LabelBuffer = buffer;
                    key = PosKey.LabelStx;
                    break;
                case "btnD0":
                case "btnD1":
                case "btnD2":
                case "btnD3":
                case "btnD4":
                case "btnD5":
                case "btnD6":
                case "btnD7":
                case "btnD8":
                case "btnD9":
                    key = (PosKey)Enum.Parse(typeof(PosKey), buttonName.Substring(3));
                    break;
                case "btnPage1":
                case "btnPage2":
                case "btnPage3":
                case "btnPage4":
                case "btnPage5":
                case "btnPage6":
                    int pageNo = Skin.PLUPageNo;
                    Parser.TryInt(buttonName.Substring(7, 1), out pageNo);
                    Skin.PLUPageNo = pageNo;
                    AddWindow(WindowType.PluList);
                    return;
                case "btnPager":
                    AddWindow(WindowType.PluPage);
                    return;
                case "btnEnter":
                    key = PosKey.Enter;
                    break;
                case "btnEscape":
                    key = PosKey.Escape;
                    break;
                case "btnSeperator":
                    key = PosKey.Decimal;

                    if (isSelling && Str.Contains(lblFirstLine.Text, PosMessage.ENTER_NUMBER))
                    {
                        string text = GetButtonText("btnFee");
                        if (text == "+%")
                            ChangeAdjustmentText(AdjustmentFactor.Amount);
                    }
                    break;
                case "btnDownArrow":
                    key = PosKey.DownArrow;
                    break;
                case "btnUpArrow":
                    key = PosKey.UpArrow;
                    break;
                case "btnReceiveOnAcct":
                    key = PosKey.ReceiveOnAcct;
                    break;
                case "btnPayOut":
                    key = PosKey.PayOut;
                    break;
                case "btnFee":
                    key = Str.Contains(lblSecondLine.Text, ',') ? PosKey.Fee : PosKey.PercentFee;
                    ChangeAdjustmentText(AdjustmentFactor.Percentage);
                    break;
                case "btnDiscount":
                    key = Str.Contains(lblSecondLine.Text, ',') ? PosKey.Discount : PosKey.PercentDiscount;
                    ChangeAdjustmentText(AdjustmentFactor.Percentage);
                    break;
                case "btnCommand":
                    key = PosKey.Command;
                    break;
                case "btnCash":
                    key = PosKey.Cash;
                    break;
                case "btnRepeat":
                    key = PosKey.Repeat;
                    break;
                case "btnProgram":
                    key = PosKey.Program;
                    break;
                case "btnAmount":
                    key = PosKey.Total;
                    break;
                case "btnReport":
                    key = PosKey.Report;
                    break;
                case "btnPrice":
                    key = PosKey.Price;
                    break;
                case "btnCustomer":
                    key = PosKey.Customer;
                    break;
                case "btnClerk":
                    key = PosKey.SalesPerson;
                    break;
                case "btnCheck":
                    key = PosKey.Check;
                    break;
                case "btnCredit2":
                case "btnCredit1":
                    buffer = int.Parse(buttonName.Substring(9));
                    KeyMap.CreditBuffer = buffer;
                    key = PosKey.Credit;
                    break;
                case "btnCredit":
                    KeyMap.CreditBuffer = 0;
                    key = PosKey.Credit;
                    break;
                case "btnPriceLookup":
                    key = PosKey.PriceLookup;
                    break;
                case "btnVoid":
                    key = isSelling ? PosKey.Void : PosKey.Document;
                    break;
                case "btnCorrection":
                    key = PosKey.Correction;
                    break;
                case "btnQuantity":
                    key = PosKey.Quantity;
                    break;
                case "btnDrawer":
                    key = PosKey.CashDrawer;
                    break;
                case "btnDivide":
                    key = PosKey.Check;
                    break;
                case "btnMultiply":
                    key = PosKey.ForeignCurrency;
                    break;
                case "btnSubtract":
                    key = PosKey.Cash;
                    break;
                case "btnAdd":
                    key = PosKey.SubTotal;
                    break;
                case "btnSubtotal":
                    key = PosKey.SubTotal;
                    break;
                case "btnEquals":
                    key = PosKey.Enter;
                    break;
                case "btnSpace":
                    key = (PosKey)ConsoleKey.Spacebar; ;
                    break;
                case "btnSendOrder":
                    key = PosKey.SendOrder;
                    break;
                default:
                    if (char.IsLetter(buttonName, 3))
                        key = (PosKey)buttonName[3];
                    break;

            }

            ConsumeKey(this, new ConsumeKeyEventArgs((int)key));
        }

        void gfb_Click(object sender, EventArgs e)
        {

            if (Display.Instance().IsPaused) return;
            if (!(sender is GradientFilledButton)) return;

            System.Media.SoundPlayer sp = new System.Media.SoundPlayer(Properties.Resources.beep);
            sp.Play();

            //int buzzerOn = 0;
            //Parser.TryInt(PosConfiguration.Get("Buzzer"), out buzzerOn);

            /*if (buzzerOn == PosConfiguration.ON)
            {
                try
                {
                    Display.PlayBuzzer(KEY_FREQUENCY, KEYPRESS_TIMEOUT);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }*/

            pressedKeys.Enqueue(((GradientFilledButton)sender).Name);
        }


        private void ChangeAdjustmentText(AdjustmentFactor af)
        {
            GradientFilledButton btnFee = GetButtonByName("btnFee");
            GradientFilledButton btnDiscount = GetButtonByName("btnDiscount");
            if (GetButtonText("btnFee") == null) return;
            
            if (af == AdjustmentFactor.Percentage)
            {
                //btnFee.Text = "+%";
                //btnDiscount.Text = "-%";
                SetControlText(btnFee, "+%");
                SetControlText(btnDiscount, "-%");
            }
            else
            {
                //btnFee.Text = "+";
                //btnDiscount.Text = "-";
                SetControlText(btnFee, "+");
                SetControlText(btnDiscount, "-");
            }
            //btnFee.Refresh();
            //btnDiscount.Refresh();
            RefreshButton(btnFee);
            RefreshButton(btnDiscount);
        }

        private GradientFilledButton GetButtonByName(string name)
        {
            GradientFilledButton gfb = null;

            Control[] controls = tblKeypad.Controls.Find(name, false);
            if (controls.Length > 0)
                gfb = controls[0] as GradientFilledButton;

            return gfb;
        }

        private void gfbUpArrow_Click(object sender, EventArgs e)
        {
            if (dgvReceipt.FirstDisplayedScrollingRowIndex > 1)
                dgvReceipt.FirstDisplayedScrollingRowIndex -= Math.Min(3, dgvReceipt.FirstDisplayedScrollingRowIndex);
        }

        private void gfbDownArrow_Click(object sender, EventArgs e)
        {
            if (dgvReceipt.FirstDisplayedScrollingRowIndex + 3 < dgvReceipt.Rows.Count)
                dgvReceipt.FirstDisplayedScrollingRowIndex += 3;
        }

        #endregion

        public bool IsMatrixAvailable()
        {
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                if (font.Name == "MatrixSchedule")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
