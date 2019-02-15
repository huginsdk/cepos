using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;
using System.IO;

namespace Hugin.POS.Display.Datecs
{
    public enum XPath
    {
        MAIN,
        MESSAGE,
        HEADER,
        HEAD_FIRM,
        HEAD_HUGIN,
        SALE_GRID,
        SALE_COLUMNCELL,
        SALE_ROWCELL,
        CURRENT,
        CURRENT_TOTAL,
        CURRENT_CUST1,
        CURRENT_CUST2,
        DETAIL,
        CUST_INFO,
        CUST_INFO_ASSINGED,
        ADJ_TITLE,
        ADJ_PRODUCT_TITLE,
        ADJ_SUB_TITLE,
        ADJ_TOTAL_TITLE,
        ADJ_PRODUCT_VAL,
        ADJ_SUB_VAL,
        ADJ_TOTAL_VAL,
        DOC_REGISTERID_TITLE,
        DOC_REGISTERID,
        DOC_ID_TITLE,
        DOC_DATE_TITLE,
        DOC_TIME_TITLE,
        DOC_ID_VAL,
        DOC_DATE_VAL,
        DOC_TIME_VAL,
        DOC_SUBTOTAL_TITLE,
        DOC_SUBTOTAL_VAL,
        DETAIL_PRODUCT,

    }

    public partial class CustomerForm : Form
    {
        /// <summary>
        /// signal for program to terminate
        /// </summary>
        //public event KeyEventHandler KeyU;
        private ContextMenuStrip Gui_ContextMenu;
        private ToolStripMenuItem cmSetup;
        private ToolStripMenuItem cmReport;
        private ToolStripMenuItem cmView;
        private ToolStripMenuItem cmView_Logo;
        private ToolStripMenuItem cmView_Grid;
        private ToolStripMenuItem cmView_Current;
        private ToolStripMenuItem cmView_Detail;
        //private ToolStripMenuItem cmView_OtherScreenSettings;
        //private ToolStripSeparator cmViewSeparator1;
        //private ToolStripMenuItem cmView_Save;
        private ToolStripMenuItem cmExit;
        private ToolStripMenuItem cmMinimize;

        decimal globalProductAdjustments = 0;
        decimal productAdjustments = 0;
        decimal subtotalAdjustments = 0;
        static long adsTimeout = 0;
        IFiscalItem lastItem;
        List<string> adsImages = null;
        private static List<IFiscalItem> soldItems = null;

        private bool lockCashier = false;

        //Number of empty rows to add to grid to prepopulate
        //with alternating color view
        private const int MAXGRIDROWS = 25;

        ListForm listForm = null;
        MenuForm menuForm = null;

        public CustomerForm()
        {
            InitializeComponent();

            menuForm = new MenuForm();
            menuForm.Show();
            menuForm.Hide();
            menuForm.KeyUp += new KeyEventHandler(listForm_KeyUp);

            listForm = new ListForm();
            listForm.Show();
            listForm.Hide();
            listForm.KeyUp += new KeyEventHandler(listForm_KeyUp);

            try
            {
                LoadContextMenuStrip();
                LoadTemplate();
                SetContextMenuEvents();
                ClearDetails();
            }
            catch
            {
                EZLogger.Log.Error("Template.xml dosyasýnda hata var.");
            }

            this.Location = new Point(0, 0);
            this.WindowState = FormWindowState.Maximized;

            soldItems = new List<IFiscalItem>();

            string defaultProductPictPath = PosConfiguration.ImagePath + "DefaultProduct.jpg"; ;
            SetItemImage(defaultProductPictPath);
        }

        #region Load Template.xml
        public void LoadTemplate()
        {
            //not to break initialization, try-catch blocks added to LoadLabel(), LoadCell() and twice in LoadTemplate()
            //LoadCell(XPath.SALE_COLUMNCELL, dgSales.ColumnHeadersDefaultCellStyle);
            //LoadCell(XPath.SALE_ROWCELL, dgSales.RowsDefaultCellStyle);
            LoadLabel(XPath.CURRENT_CUST1, lblFirstMessage);
            LoadLabel(XPath.CURRENT_CUST2, lblSecondMessage);

            try
            {
                if (PosConfiguration.Get("AdvertisementPath") != "")
                {
                    string adsPath = PosConfiguration.Get("AdvertisementPath");
                    string[] ads = adsPath.Split(',');
                    string path = ads[0];
                    adsTimeout = long.Parse(ads[1]);
                    EnableAdvertisementPanel(path);
                }
            }
            catch { }
        }
        private void LoadCell(XPath attr, DataGridViewCellStyle cellStyle)
        {
            try
            {
                cellStyle.Font = Template.GetFont(attr);
                cellStyle.ForeColor = Template.GetForecolor(attr);
                cellStyle.BackColor = Template.GetBackcolor(attr);
            }
            catch { /*Display.Log.Error("Gui.LoadCell() : Ekran ayarlarýnda sorun var... " + "Ürün tablosu hücre ayarlarý yüklenemedi.");*/ }
        }

        private void LoadFont(XPath attr, Label label)
        {
            try
            {
                label.Font = Template.GetFont(attr);
                label.ForeColor = Template.GetForecolor(attr);
                label.BackColor = Template.GetBackcolor(attr);
            }
            catch { /*Display.Log.Error("Gui.LoadLabel() : Ekran ayarlarýnda sorun var... Label ayarlarý yüklenemedi: " + label.Name);*/ }
        }

        private void LoadLabel(XPath attr, System.Windows.Forms.Label label)
        {
            try
            {
                label.Text = Template.GetDefaultText(attr);
                LoadFont(attr, label);

                string path = Template.GetBackImage(attr);
                if (System.IO.File.Exists(path))
                {
                    ((Control)label).BackgroundImage = Image.FromFile(path);
                    label.Tag = path;
                }
                else
                {
                    label.Tag = "";//tag is holds the path of the image file
                    ((Control)label).BackgroundImage = null;
                }
            }
            catch { Display.Log.Error("Gui.LoadLabel() : Ekran ayarlarýnda sorun var... Label ayarlarý yüklenemedi: " + label.Name); }
        }

        #endregion
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        void listForm_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void LoadContextMenuStrip()
        {
            this.Gui_ContextMenu = new System.Windows.Forms.ContextMenuStrip();

            this.cmSetup = new System.Windows.Forms.ToolStripMenuItem();
            this.cmReport = new System.Windows.Forms.ToolStripMenuItem();

            this.cmView = new System.Windows.Forms.ToolStripMenuItem();
            this.cmView_Logo = new System.Windows.Forms.ToolStripMenuItem();
            this.cmView_Grid = new System.Windows.Forms.ToolStripMenuItem();
            this.cmView_Current = new System.Windows.Forms.ToolStripMenuItem();
            this.cmView_Detail = new System.Windows.Forms.ToolStripMenuItem();

            this.cmExit = new System.Windows.Forms.ToolStripMenuItem();
            this.cmMinimize = new System.Windows.Forms.ToolStripMenuItem();

            // 
            // cmSetup
            // 
            this.cmSetup.Name = "cmSetup";
            this.cmSetup.Size = new System.Drawing.Size(125, 22);
            this.cmSetup.Text = "Program";
            // 
            // cmReport
            // 
            this.cmReport.Name = "cmReport";
            this.cmReport.Size = new System.Drawing.Size(125, 22);
            this.cmReport.Text = "Rapor";
            // 
            // cmView_Logo
            // 
            this.cmView_Logo.Checked = true;
            this.cmView_Logo.CheckOnClick = true;
            this.cmView_Logo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cmView_Logo.Name = "cmView_Logo";
            this.cmView_Logo.Size = new System.Drawing.Size(174, 22);
            this.cmView_Logo.Text = "Logo";
            // 
            // cmView_Grid
            // 
            this.cmView_Grid.Checked = true;
            this.cmView_Grid.CheckOnClick = true;
            this.cmView_Grid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cmView_Grid.Name = "cmView_Grid";
            this.cmView_Grid.Size = new System.Drawing.Size(174, 22);
            this.cmView_Grid.Text = "Ürün tablosu";
            // 
            // cmView_Current
            // 
            this.cmView_Current.Checked = true;
            this.cmView_Current.CheckOnClick = true;
            this.cmView_Current.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cmView_Current.Name = "cmView_Current";
            this.cmView_Current.Size = new System.Drawing.Size(174, 22);
            this.cmView_Current.Text = "Anlýk iþlem bilgisi";
            // 
            // cmView_Detail
            // 
            this.cmView_Detail.Checked = true;
            this.cmView_Detail.CheckOnClick = true;
            this.cmView_Detail.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cmView_Detail.Name = "cmView_Detail";
            this.cmView_Detail.Size = new System.Drawing.Size(174, 22);
            this.cmView_Detail.Text = "Belge detayý";

            // 
            // cmView
            // 
            this.cmView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.cmView_Logo,
                this.cmView_Grid,
                this.cmView_Current,
                this.cmView_Detail,}
            );
            this.cmView.Name = "cmView";
            this.cmView.Size = new System.Drawing.Size(125, 22);
            this.cmView.Text = "Görünüm";
            // 
            // cmMinimize
            // 
            this.cmMinimize.Name = "cmMinimize";
            this.cmMinimize.Size = new System.Drawing.Size(125, 22);
            this.cmMinimize.Text = "Simge Durumuna Küçült";
            // 
            // cmExit
            // 
            this.cmExit.Name = "cmExit";
            this.cmExit.Size = new System.Drawing.Size(125, 22);
            this.cmExit.Text = "Çýkýþ";

            // 
            // Gui_ContextMenu
            // 
            this.Gui_ContextMenu.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[] {
                    //this.cmMinimize,
                    //this.cmExit
                }
            );

            this.Gui_ContextMenu.Name = "Gui_ContextMenu";
            this.Gui_ContextMenu.Size = new System.Drawing.Size(126, 114);

            this.ContextMenuStrip = this.Gui_ContextMenu;
        }

        private void EnableAdvertisementPanel(string path)
        {
            if (!LoadAdvertisementImages(path)) return;
        }

        private bool LoadAdvertisementImages(string path)
        {
            adsImages = new List<string>();
            String[] validExtension = new String[] { "*.jpg", "*.jpeg", "*.bmp", "*.gif" };
            foreach (String extension in validExtension)
            {
                foreach (string file in Dir.GetFiles(path, extension))
                {
                    adsImages.Add(file);
                }
            }
            return adsImages.Count > 0;
        }

        private void ShowAvertisement()
        {
            //while (true)
            //{
            //    //TODO: 
            //    if (!this.Visible)
            //        break;
            //    try
            //    {
            //        if (new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds - span.TotalMilliseconds < adsTimeout)
            //            throw new TimeoutException();
            //        if (adsIndex >= adsImages.Count)
            //            adsIndex = 0;
            //        pbxAdvertisement.Image = Image.FromFile(adsImages[adsIndex]);
            //        span = new TimeSpan(DateTime.Now.Ticks);
            //        adsIndex++;
            //    }
            //    catch (TimeoutException) { System.Threading.Thread.Sleep(100); }
            //    catch (Exception) { break; }
            //}
        }

        #region Public Functions

        public void Show(String message)
        {
            if (!message.Contains("\n")) message = message + "\n";
            String[] lines = message.Split('\n');

            ToggleHeaderCurrentPanel(message == Common.PosMessage.WELCOME);
            SetFirstMessage(AdjustMessageLine(lines[0]));
            SetSecondMessage(AdjustMessageLine(lines[1]));

            //if (!this.Focused) this.BringToFront();
        }

        public void ShowAlertMessage(String message)
        {
            return;
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
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.PRODUCT_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 adjustment.NetAmount);
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.PRODUCT_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 adjustment.NetAmount);
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n \t{1:C}", Common.PosMessage.PRODUCT_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n \t{1:C}", Common.PosMessage.PRODUCT_PRICE_FEE,
                                              new Number(adjustment.NetAmount));
                    SetLastItemAdjustment(adjustment.NetAmount);

                }
                else if (adjustment.Target is ISalesDocument)
                {
                    if (adjustment.Method == (AdjustmentType.PercentDiscount))
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.SUBTOTAL_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.SUBTOTAL_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n{2}\t{1:C}", Common.PosMessage.SUBTOTAL_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount),
                                             PosMessage.AMOUNT);
                    else
                        Show("{0}\n{2}\t{1:C}", Common.PosMessage.SUBTOTAL_PRICE_FEE,
                                             new Number(adjustment.NetAmount),
                                             PosMessage.AMOUNT);
                    SetSubtotalAdjustment(subtotalAdjustments + adjustment.NetAmount);
                }
                else throw new InvalidProgramException("Adjustment target is incorrectly set");

                UpdateSubtotal();
            }
            catch (FormatException e)
            {
                Display.Log.Error("Display error. {0}", e.Message);
            }
        }

        public void ShowCorrect(IAdjustment adjustment)
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

                if (!(adjustment.Target is ISalesDocument))
                    ShowItems();

                UpdateSubtotal();
            }
            catch (FormatException e)
            {
                EZLogger.Log.Error("Display error. {0}", e.Message);
            }
        }

        public void Show(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)), large ? "X" : fi.Unit, new Number(fi.TotalAmount - fi.VoidAmount));
            }
            catch (Exception fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void ShowSale(IFiscalItem fi)
        {
            Show(fi);
            lastItem = fi;
            soldItems.Add(fi);
            ShowItem(fi);
        }

        public void ShowVoid(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(fi.Quantity), large ? "X" : fi.Unit, new Number(fi.TotalAmount));
                //ShowItem(fi);
                //this.dgSales.Rows[0].DefaultCellStyle.ForeColor = Color.Red;

                soldItems.Add(fi);
                ShowItems();
            }
            catch (FormatException fex)
            {
                Display.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        private void ShowItems()
        {
            ClearProducts();
            List<IFiscalItem> items = new List<IFiscalItem>();

            items = soldItems.FindAll(delegate (IFiscalItem fi)
            {
                return fi.Quantity > fi.VoidQuantity;
            });


            foreach (IFiscalItem fi in items)
            {
                ShowItem(fi);
            }
        }

        public void Show(IProduct p)
        {
            try
            {
                bool large = ((p.Quantity != (long)p.Quantity) && p.UnitPrice * p.Quantity > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", p.Name, new Number(p.Quantity), large ? "X" : p.Unit, new Number(p.UnitPrice * p.Quantity));
                String path = PosConfiguration.ImagePath + p.Barcode + ".jpg";
                SetItemImage(path);
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

        public void ShowMenu(IMenuList menuList)
        {
            if (menuList == null)
            {
                ShowMenuForm(menuList);
                ShowListForm(menuList);
                return;
            }

            IDoubleEnumerator ide = menuList as IDoubleEnumerator;
            if ((ide.Current is IFiscalItem || ide.Current is IProduct))
                ShowListForm(menuList);
            else
                ShowMenuForm(menuList);
        }

        private delegate void ShowMenuFormDelegate(IMenuList menuList);
        private void ShowMenuForm(IMenuList menuList)
        {
            if (menuForm.InvokeRequired)
                menuForm.Invoke(new ShowMenuFormDelegate(ShowMenuForm), menuList);
            else
            {
                menuForm.ShowList(menuList);
                if (menuForm.Opacity > 0)
                    menuForm.Show();
                else
                    this.BringToFront();
            }
        }

        private delegate void ShowListFormDelegate(IMenuList menuList);
        private void ShowListForm(IMenuList menuList)
        {
            if (listForm.InvokeRequired)
                listForm.Invoke(new ShowListFormDelegate(ShowListForm), menuList);
            else
            {
                listForm.SetList(menuList);
            }
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

        public virtual bool KeyAvailable
        {
            get { return false; }
        }

        public void ChangeCurrentDocument(ISalesDocument doc)
        {
            ChangeDocument(doc);
        }

        public void DocumentClose(ISalesDocument doc)
        {
            ClearDetails();
            soldItems = new List<IFiscalItem>();
        }

        public void UndoAdjustment(ISalesDocument doc)
        {
            SetSubtotalAdjustment(0);
        }

        public void LoginCashier(ICashier c)
        {

        }

        #endregion Public Functions


        #region Display Value Settings
        /* Display values are set by a couple of functions : 1) delegate function 2) non-delegate function
         * When non-delegate function is called by external function, if call belongs to same thread sets the value
         * else it calls delegate function
         * */

        private delegate void ToggleHeaderCurrentPanelDelegate(Boolean headerOnTop);
        private void ToggleHeaderCurrentPanel(Boolean headerOnTop)
        {
            if (pnlHeader.InvokeRequired)
                pnlHeader.Invoke(new ToggleHeaderCurrentPanelDelegate(this.ToggleHeaderCurrentPanel), headerOnTop);
            else
            {

                if (pnlCurrent.Visible && pnlHeader.Visible)
                {
                    if (headerOnTop)
                        pnlHeader.BringToFront();
                    else pnlHeader.SendToBack();
                }
            }
        }

        private delegate void SetFirstMessageDelegate(String message);
        private void SetFirstMessage(String message)
        {
            if (lblFirstMessage.InvokeRequired)
                lblFirstMessage.Invoke(new SetFirstMessageDelegate(this.SetFirstMessage), message);
            else
            {
                lblFirstMessage.Text = message;
                lblFirstMessage.Refresh();
            }
        }

        private delegate void SetSecondMessageDelegate(String message);
        private void SetSecondMessage(String message)
        {
            if (lblSecondMessage.InvokeRequired)
                lblSecondMessage.Invoke(new SetSecondMessageDelegate(this.SetSecondMessage), message);
            else
            {
                lblSecondMessage.Text = message;
                lblSecondMessage.Refresh();
            }
        }

        private delegate void ClearProductsDelegate();
        private void ClearProducts()
        {
            if (dgSales.InvokeRequired)
                dgSales.Invoke(new ClearProductsDelegate(this.ClearProducts));
            else
            {
                dgSales.Rows.Clear();
                for (int i = 0; i < MAXGRIDROWS; i++)
                {
                    dgSales.Rows.Add();
                }

                SetDataGridRowSize();
            }
        }

        private delegate void ShowItemDelegate(IFiscalItem fi);
        private void ShowItem(IFiscalItem fi)
        {
            if (dgSales.InvokeRequired)
                this.dgSales.Invoke(new ShowItemDelegate(this.ShowItem), fi);
            else
            {
                if (fi.Quantity > fi.VoidQuantity)
                {
                    if (dgSales.Rows.Count - MAXGRIDROWS == 0)
                    {
                        //ClearDetails();
                        dgSales.Rows[MAXGRIDROWS - 1].Selected = true;

                    }

                    this.dgSales.Rows.Insert(0, new object[] {
                    String.Format("{0:D3}",dgSales.Rows.Count + 1 - MAXGRIDROWS),
                    //fi.Product.Barcode, 
                    fi.Name,
                    fi.Quantity,
                    //fi.Unit, 
                    String.Format("{0:0.00}",fi.UnitPrice),
                    String.Format("{0:0.00}",fi.TotalAmount) });

                    String path = PosConfiguration.ImagePath + fi.Product.Barcode + ".jpg";
                    if (!System.IO.File.Exists(path))
                        path = PosConfiguration.ImagePath + "NoImage.jpg";

                    SetItemImage(path);

                    SetDataGridRowSize();

                    if (productAdjustments != 0)
                        SetProductAdjustment(0);

                    UpdateSubtotal();
                }
            }
        }

        private delegate void SetControlTextDelegate(Control cntrl, string text);
        private void SetControlText(Control cntrl, string text)
        {
            if (cntrl.InvokeRequired)
                cntrl.Invoke(new SetControlTextDelegate(SetControlText), cntrl, text);
            else
                cntrl.Text = text;
        }


        private delegate void SetProductAdjustmentDelegate(Decimal adjustment);
        private void SetProductAdjustment(Decimal adjustment)
        {
            productAdjustments = adjustment;
        }

        private delegate void SetSubotalAdjustmentDelegate(Decimal adjustment);
        private void SetSubtotalAdjustment(Decimal adjustment)
        {           
            subtotalAdjustments = adjustment;
        }


        private delegate void SetLastItemAdjustmentDelegate(Decimal adjustment);
        private void SetLastItemAdjustment(Decimal adjustment)
        {
            if (dgSales.InvokeRequired)
                this.dgSales.Invoke(new SetLastItemAdjustmentDelegate(this.SetLastItemAdjustment), adjustment);
            else
            {
                this.dgSales.Rows[0].Cells[dgSales.ColumnCount - 2].Value = String.Format("{0:N2}", lastItem.UnitPrice);
                this.dgSales.Rows[0].Cells[dgSales.ColumnCount - 1].Value = String.Format("{0:N2}", lastItem.TotalAmount);
                if (lastItem.TotalAmount == lastItem.ListedAmount)
                    this.dgSales.Rows[0].DefaultCellStyle.ForeColor = Color.Black;
                else if (lastItem.TotalAmount < lastItem.ListedAmount)
                    this.dgSales.Rows[0].DefaultCellStyle.ForeColor = Color.Green;
                else this.dgSales.Rows[0].DefaultCellStyle.ForeColor = Color.Blue;
                globalProductAdjustments += adjustment;
                SetProductAdjustment(productAdjustments + adjustment);
            }
        }


        private delegate void RefreshImagesDelegate();
        private void RefreshImages()
        {
            if (pbxProduct.InvokeRequired)
                pbxProduct.Invoke(new RefreshImagesDelegate(this.RefreshImages));
            else
                pbxProduct.Refresh();
        }

        private delegate void SetItemImageDelegate(String imagePath);
        private void SetItemImage(String imagePath)
        {
            if (pbxProduct.InvokeRequired)
                pbxProduct.Invoke(new SetItemImageDelegate(this.SetItemImage), imagePath);
            else
            {
                Bitmap b = null;
                if (System.IO.File.Exists(imagePath))
                    b = LoadBitmap(imagePath);
                pbxProduct.Image = b;
            }
        }

        public static Bitmap LoadBitmap(string path)
        {
            //Open file in read only mode
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            //Get a binary reader for the file stream
            using (BinaryReader reader = new BinaryReader(stream))
            {
                //copy the content of the file into a memory stream
                MemoryStream memoryStream = new MemoryStream(reader.ReadBytes((int)stream.Length));
                //make a new Bitmap object the owner of the MemoryStream
                Bitmap b = new Bitmap(memoryStream);
                memoryStream.Close();
                return b;
            }
        }

        private delegate void ChangeModDelegate(System.Windows.Forms.Label lblLed);
        private void ChangeMod(System.Windows.Forms.Label lblLed)
        {
            if (lblLed.InvokeRequired)
                lblLed.Invoke(new ChangeModDelegate(this.ChangeMod), lblLed);
            else
            {
                if (Convert.ToInt32(lblLed.Tag) == 0)
                {
                    lblLed.Tag = 1;
                    lblLed.ForeColor = Color.Goldenrod;
                    lblLed.BackColor = Color.Green;
                }
                else
                {
                    lblLed.Tag = 0;
                    lblLed.ForeColor = Color.Gray;
                    lblLed.BackColor = Color.Transparent;
                }
            }
        }

        #endregion Display Value Settings

        #region Common Functions
        //when processing document is changed, what should be done on screen?
        private void ChangeDocument(ISalesDocument sDoc)
        {
            ClearDetails();

            foreach (IFiscalItem fi in sDoc.Items)
            {
                ShowItem(fi);
                if (fi.Quantity < 0)
                    this.dgSales.Rows[0].DefaultCellStyle.ForeColor = Color.Red;
            }

            if (sDoc.Items.Count > 0)
            {
                String path = PosConfiguration.ImagePath + sDoc.Items[sDoc.Items.Count - 1].Product.Barcode + ".jpg";
                if (!System.IO.File.Exists(path))
                    path = PosConfiguration.ImagePath + "NoImage.jpg";

                if (System.IO.File.Exists(path))
                {
                    SetItemImage(path);
                }
            }

            if (sDoc.Customer != null)
            {
                String customerInfo = "";
                if (sDoc.Customer.IsDiplomatic)
                {
                    customerInfo = FormatCustomerLine("Sn." + sDoc.Customer.Contact[0]) + " "
                           + FormatCustomerLine(sDoc.Customer.Contact[1]) + " "
                           + FormatCustomerLine(sDoc.Customer.Contact[2]);
                }
                else
                {
                    customerInfo = FormatCustomerLine("Sn." + sDoc.Customer.Identity[1]) + " "
                              + FormatCustomerLine(sDoc.Customer.Contact[0]) + " "
                              + FormatCustomerLine(sDoc.Customer.Contact[1]) + " "
                              + FormatCustomerLine(sDoc.Customer.Contact[2]);
                }
            }
            else
            {

            }
        }


        private void ClearDetails()
        {
            ClearProducts();
            globalProductAdjustments = 0;
            SetProductAdjustment(0);
            SetSubtotalAdjustment(0);

            string defaultProductPictPath = PosConfiguration.ImagePath + "DefaultProduct.jpg"; ;
            SetItemImage(defaultProductPictPath);

            dgSales.Rows[MAXGRIDROWS - 1].Selected = true;

            UpdateSubtotal();
        }

        #endregion Common Functions

        #region Helper Functions

        private string AdjustMessageLine(string line)
        {
            if (line.Contains("\t"))
                line = line.Replace("\t", " ".PadRight(20 - line.Length + 1));
            else
                line = line.PadLeft(line.Length + ((20 - line.Length) / 2), ' ');
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

        #region ContextMenu Events

        protected void SetContextMenuEvents()
        {
            Gui_ContextMenu.Opening += new CancelEventHandler(Gui_ContextMenu_Opening);

            Gui_ContextMenu.Closed += new ToolStripDropDownClosedEventHandler(Gui_ContextMenu_Closed);

            cmExit.Click += new EventHandler(cmExit_Click);
            cmMinimize.Click += new EventHandler(cmMinimize_Click);
            cmView_Current.CheckedChanged += new EventHandler(cmView_Current_CheckedChanged);
            cmView_Detail.CheckedChanged += new EventHandler(cmView_Detail_CheckedChanged);
            cmView_Grid.CheckedChanged += new EventHandler(cmView_Grid_CheckedChanged);
            cmView_Logo.CheckedChanged += new EventHandler(cmView_Logo_CheckedChanged);

            this.Resize += new EventHandler(CustomerForm_Resize);

        }

        void CustomerForm_Resize(object sender, EventArgs e)
        {
            if (lockCashier == true)
            {
                UnlockCashier();
            }
        }

        void cmMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            LockCashier();
        }
        void Gui_ContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.AppClicked)
            {
                UnlockCashier();
            }
        }


        void Gui_ContextMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        private void LockCashier()
        {
            if (Display.Instance().Inactive)
            {
                Display.Instance().Inactive = false;
                System.Threading.Thread.Sleep(250);
            }
            lockCashier = true;
            Display.Instance().Mode = Target.Cashier;
            Display.Instance().Show("KASA ÝÞLEME\nKAPATILMIÞTIR");
            Display.Instance().Pause();
        }

        private void UnlockCashier()
        {
            lockCashier = false;
            Display.Instance().Mode = Target.Cashier;
            Display.Instance().Play();
            base.OnKeyUp(new KeyEventArgs(Keys.Escape));
            base.OnKeyUp(new KeyEventArgs(Keys.Escape));
        }

        void cmView_Logo_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlHeader.Visible = this.cmView_Logo.Checked;
            dgSalesRefreshLayout();
            UnlockCashier();
        }

        void cmView_Grid_CheckedChanged(object sender, EventArgs e)
        {
            this.dgSales.Visible = this.cmView_Grid.Checked;
            UnlockCashier();
        }

        void cmView_Detail_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlDetail.Visible = this.cmView_Detail.Checked;
            dgSalesRefreshLayout();
            UnlockCashier();
        }

        void cmView_Current_CheckedChanged(object sender, EventArgs e)
        {
            this.pnlCurrent.Visible = this.cmView_Current.Checked;
            dgSalesRefreshLayout();
            UnlockCashier();
        }

        void dgSalesRefreshLayout()
        {

            int dgSalesY = 0;
            int dgSalesHeight = CustomerForm.ActiveForm.Height -
                                ((this.pnlCurrent.Visible || this.pnlHeader.Visible) ? this.pnlCurrent.Height : 0) -
                                (this.pnlDetail.Visible ? this.pnlDetail.Height : 0);
            if ((this.pnlCurrent.Visible || this.pnlHeader.Visible))
                dgSalesY = pnlCurrent.Height;

            dgSales.Location = new Point(0, dgSalesY);
            dgSales.Size = new Size(dgSales.Size.Width, dgSalesHeight);
        }

        void cmExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void UpdateSubtotal()
        {
            ////sum clmnAmount values
            //decimal subTotal = 0.00m;
            //for (int i = 0; i < dgSales.Rows.Count; i++)
            //{
            //    if (dgSales["clmnAmount", i].Value != null)
            //        subTotal += Decimal.Parse(dgSales["clmnAmount", i].Value.ToString());
            //}
        }


        #endregion  ContextMenu Events

        int currentWidth = 1024;
        int currentHeight = 768;

        private void CustomerForm_Load(object sender, EventArgs e)
        {
            Rectangle clientResolution = new Rectangle();

            clientResolution = Screen.GetBounds(clientResolution);

            float widthRatio = (float)clientResolution.Width / (float)currentWidth;
            float heightRatio = (float)clientResolution.Height / (float)currentHeight;

            SizeF ratioSize = new SizeF(widthRatio, heightRatio);

            this.Scale(ratioSize);
        }

        static int defaultRowHeight;
        static bool firstTime = true;
        private void SetDataGridRowSize()
        {
            if (firstTime)
            {
                defaultRowHeight = dgSales.Rows[0].Height;   
                firstTime = false;
            }
            float heightRatio = 1.0F;

            Rectangle clientResolution = new Rectangle();
            System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

            if (screens.Length > 1)
            {
                clientResolution = screens[1].Bounds;

                heightRatio = (float)clientResolution.Height / (float)currentHeight;

                foreach (DataGridViewRow dgRow in this.dgSales.Rows)
                {
                    dgRow.Height = (int)(defaultRowHeight * heightRatio * 1.25);
                }
            }

            // Rows Font
            this.dgSales.RowsDefaultCellStyle.Font = new System.Drawing.Font("Lucida Console", (10.0F*heightRatio), System.Drawing.FontStyle.Bold);
        }
    }
}
