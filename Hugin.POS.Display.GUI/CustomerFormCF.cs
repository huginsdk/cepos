using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;
using System.Data;
using System.Drawing;
using Hugin.POS.Data;

namespace Hugin.POS.Display
{
    class CustomerForm : Form
    {

        public event ConsumeKeyHandler ConsumeKey;

        private System.Windows.Forms.Label lblFirstMessage;
        private System.Windows.Forms.Label lblSecondMessage;

        private const int MAXGRIDROWS = 5;
        protected Panel pnlDetail;
        protected Panel pnlProductImage;
        protected PictureBox pbxProduct;
        protected PictureBox pbxNextProduct;
        protected Panel pnlDocumentInfo;
        protected System.Windows.Forms.Label lblDocumentTime;
        protected System.Windows.Forms.Label lblDocumentDate;
        protected System.Windows.Forms.Label lblDocumentId;
        protected System.Windows.Forms.Label lblTime;
        protected System.Windows.Forms.Label lblDate;
        protected System.Windows.Forms.Label lblDocument;
        protected System.Windows.Forms.Label lblCustomer;
        protected Panel pnlAdjustments;
        protected System.Windows.Forms.Label lblAdjustment;
        protected System.Windows.Forms.Label lblTotalAdjustAmount;
        protected System.Windows.Forms.Label lblSubtotalAdjustAmount;
        protected System.Windows.Forms.Label lblProductAdjustAmount;
        protected System.Windows.Forms.Label lblTotalAdjustment;
        protected System.Windows.Forms.Label lblSubtotalAdjustment;
        protected System.Windows.Forms.Label lblProductAdjustment;
        protected Panel pnlHeader;
        protected Panel pnlCurrent;
        protected PictureBox pbxHuginLogo;
        protected System.Windows.Forms.Label lblLogo;
        private ColumnHeader clmnNo;
        private ColumnHeader clmnBarcode;
        private ColumnHeader clmnProductName;
        private ColumnHeader clmnQuantity;
        private ColumnHeader clmnUnit;
        private ColumnHeader clmnPrice;
        private ColumnHeader clmnAmount;
        private ListView listViewSales;

        decimal globalProductAdjustments = 0;
        decimal productAdjustments = 0;
        decimal subtotalAdjustments = 0;
        protected System.Windows.Forms.Label lblSaleLed;
        protected System.Windows.Forms.Label lblOnlineLed;
        protected System.Windows.Forms.Label lblCustomerLed;
        protected PictureBox pbxAdvertisement;

        MenuForm menuForm = null;

        static int adsIndex = 0;
        static long adsTimeout = 0;

        List<string> adsImages = null;

        public CustomerForm()
        {
            InitializeComponent();
            lblSaleLed.Tag = 0;
            lblOnlineLed.Tag = 0;
            lblCustomerLed.Tag = 0;

            menuForm = new MenuForm();
            //States.Login.OnLogin += new LoginEventHandler(Login_OnLogin);
            //ISalesDocument.OnClose += new OnCloseEventHandler(SalesDocument_OnClose);
            //ISalesDocument.OnVoid += new OnCloseEventHandler(SalesDocument_OnClose);
            //ISalesDocument.OnSuspend += new OnCloseEventHandler(SalesDocument_OnClose);
            //ISalesDocument.CustomerChanged += new CustomerEventHandler(SalesDocument_CustomerChanged);
            //ISalesDocument.OnUndoAdjustment += new EventHandler(SalesDocument_OnUndoAdjustment);
            //CashRegister.DocumentChanged += new EventHandler(CurrentDocument_Changed);
            ClearDetails();

            if (!Str.Contains(lblLogo.Text, "\n"))
                lblLogo.Text = "" + lblLogo.Text;
            String dir = "";

            dir = IOUtil.ProgramDirectory + "Image\\DefaultProduct.jpg";
            if (System.IO.File.Exists(dir))
                this.pbxProduct.Image = new Bitmap(dir);

            dir = IOUtil.ProgramDirectory + "Image\\HuginLogo.jpg";
            if (System.IO.File.Exists(dir))
                this.pbxHuginLogo.Image = new Bitmap(dir);
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

        private void InitializeComponent()
        {
            this.lblFirstMessage = new System.Windows.Forms.Label();
            this.lblSecondMessage = new System.Windows.Forms.Label();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.lblSaleLed = new System.Windows.Forms.Label();
            this.lblOnlineLed = new System.Windows.Forms.Label();
            this.lblCustomerLed = new System.Windows.Forms.Label();
            this.pnlProductImage = new System.Windows.Forms.Panel();
            this.pbxProduct = new System.Windows.Forms.PictureBox();
            this.pbxNextProduct = new System.Windows.Forms.PictureBox();
            this.pnlDocumentInfo = new System.Windows.Forms.Panel();
            this.lblDocumentTime = new System.Windows.Forms.Label();
            this.lblDocumentDate = new System.Windows.Forms.Label();
            this.lblDocumentId = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.lblDocument = new System.Windows.Forms.Label();
            this.lblCustomer = new System.Windows.Forms.Label();
            this.pnlAdjustments = new System.Windows.Forms.Panel();
            this.lblAdjustment = new System.Windows.Forms.Label();
            this.lblTotalAdjustAmount = new System.Windows.Forms.Label();
            this.lblSubtotalAdjustAmount = new System.Windows.Forms.Label();
            this.lblProductAdjustAmount = new System.Windows.Forms.Label();
            this.lblTotalAdjustment = new System.Windows.Forms.Label();
            this.lblSubtotalAdjustment = new System.Windows.Forms.Label();
            this.lblProductAdjustment = new System.Windows.Forms.Label();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pbxHuginLogo = new System.Windows.Forms.PictureBox();
            this.lblLogo = new System.Windows.Forms.Label();
            this.pnlCurrent = new System.Windows.Forms.Panel();
            this.clmnNo = new System.Windows.Forms.ColumnHeader();
            this.clmnBarcode = new System.Windows.Forms.ColumnHeader();
            this.clmnProductName = new System.Windows.Forms.ColumnHeader();
            this.clmnQuantity = new System.Windows.Forms.ColumnHeader();
            this.clmnUnit = new System.Windows.Forms.ColumnHeader();
            this.clmnPrice = new System.Windows.Forms.ColumnHeader();
            this.clmnAmount = new System.Windows.Forms.ColumnHeader();
            this.listViewSales = new System.Windows.Forms.ListView();
            this.pbxAdvertisement = new System.Windows.Forms.PictureBox();
            this.pnlDetail.SuspendLayout();
            this.pnlProductImage.SuspendLayout();
            this.pnlDocumentInfo.SuspendLayout();
            this.pnlAdjustments.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlCurrent.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFirstMessage
            // 
            this.lblFirstMessage.BackColor = System.Drawing.Color.Black;
            this.lblFirstMessage.Font = new System.Drawing.Font("Courier New", 32F, System.Drawing.FontStyle.Bold);
            this.lblFirstMessage.ForeColor = System.Drawing.Color.White;
            this.lblFirstMessage.Location = new System.Drawing.Point(40, 0);
            this.lblFirstMessage.Name = "lblFirstMessage";
            this.lblFirstMessage.Size = new System.Drawing.Size(572, 50);
            this.lblFirstMessage.Text = "Welcome";
            // 
            // lblSecondMessage
            // 
            this.lblSecondMessage.BackColor = System.Drawing.Color.Black;
            this.lblSecondMessage.Font = new System.Drawing.Font("Courier New", 32F, System.Drawing.FontStyle.Bold);
            this.lblSecondMessage.ForeColor = System.Drawing.Color.White;
            this.lblSecondMessage.Location = new System.Drawing.Point(40, 50);
            this.lblSecondMessage.Name = "lblSecondMessage";
            this.lblSecondMessage.Size = new System.Drawing.Size(586, 50);
            this.lblSecondMessage.Text = "Hoþgeldiniz";
            // 
            // pnlDetail
            // 
            this.pnlDetail.Controls.Add(this.lblSaleLed);
            this.pnlDetail.Controls.Add(this.lblOnlineLed);
            this.pnlDetail.Controls.Add(this.lblCustomerLed);
            this.pnlDetail.Controls.Add(this.pnlProductImage);
            this.pnlDetail.Controls.Add(this.pnlDocumentInfo);
            this.pnlDetail.Controls.Add(this.lblCustomer);
            this.pnlDetail.Controls.Add(this.pnlAdjustments);
            this.pnlDetail.Location = new System.Drawing.Point(0, 428);
            this.pnlDetail.Name = "pnlDetail";
            this.pnlDetail.Size = new System.Drawing.Size(800, 58);
            // 
            // lblSaleLed
            // 
            this.lblSaleLed.BackColor = System.Drawing.Color.Transparent;
            this.lblSaleLed.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular);
            this.lblSaleLed.ForeColor = System.Drawing.Color.Gray;
            this.lblSaleLed.Location = new System.Drawing.Point(49, 37);
            this.lblSaleLed.Name = "lblSaleLed";
            this.lblSaleLed.Size = new System.Drawing.Size(66, 15);
            this.lblSaleLed.Text = "Satýþ Var";
            this.lblSaleLed.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblOnlineLed
            // 
            this.lblOnlineLed.BackColor = System.Drawing.Color.Transparent;
            this.lblOnlineLed.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular);
            this.lblOnlineLed.ForeColor = System.Drawing.Color.Gray;
            this.lblOnlineLed.Location = new System.Drawing.Point(115, 37);
            this.lblOnlineLed.Name = "lblOnlineLed";
            this.lblOnlineLed.Size = new System.Drawing.Size(50, 15);
            this.lblOnlineLed.Text = "On-line";
            this.lblOnlineLed.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblCustomerLed
            // 
            this.lblCustomerLed.BackColor = System.Drawing.Color.Transparent;
            this.lblCustomerLed.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular);
            this.lblCustomerLed.ForeColor = System.Drawing.Color.Gray;
            this.lblCustomerLed.Location = new System.Drawing.Point(-1, 37);
            this.lblCustomerLed.Name = "lblCustomerLed";
            this.lblCustomerLed.Size = new System.Drawing.Size(50, 15);
            this.lblCustomerLed.Text = "Müþteri";
            this.lblCustomerLed.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlProductImage
            // 
            this.pnlProductImage.Controls.Add(this.pbxProduct);
            this.pnlProductImage.Controls.Add(this.pbxNextProduct);
            this.pnlProductImage.Location = new System.Drawing.Point(650, 0);
            this.pnlProductImage.Name = "pnlProductImage";
            this.pnlProductImage.Size = new System.Drawing.Size(150, 57);
            // 
            // pbxProduct
            // 
            this.pbxProduct.Location = new System.Drawing.Point(0, 0);
            this.pbxProduct.Name = "pbxProduct";
            this.pbxProduct.Size = new System.Drawing.Size(147, 57);
            this.pbxProduct.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pbxNextProduct
            // 
            this.pbxNextProduct.Location = new System.Drawing.Point(0, 0);
            this.pbxNextProduct.Name = "pbxNextProduct";
            this.pbxNextProduct.Size = new System.Drawing.Size(150, 57);
            this.pbxNextProduct.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pnlDocumentInfo
            // 
            this.pnlDocumentInfo.Controls.Add(this.lblDocumentTime);
            this.pnlDocumentInfo.Controls.Add(this.lblDocumentDate);
            this.pnlDocumentInfo.Controls.Add(this.lblDocumentId);
            this.pnlDocumentInfo.Controls.Add(this.lblTime);
            this.pnlDocumentInfo.Controls.Add(this.lblDate);
            this.pnlDocumentInfo.Controls.Add(this.lblDocument);
            this.pnlDocumentInfo.Location = new System.Drawing.Point(455, 0);
            this.pnlDocumentInfo.Name = "pnlDocumentInfo";
            this.pnlDocumentInfo.Size = new System.Drawing.Size(185, 52);
            // 
            // lblDocumentTime
            // 
            this.lblDocumentTime.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Bold);
            this.lblDocumentTime.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDocumentTime.Location = new System.Drawing.Point(91, 34);
            this.lblDocumentTime.Name = "lblDocumentTime";
            this.lblDocumentTime.Size = new System.Drawing.Size(94, 10);
            this.lblDocumentTime.Text = "00:00:00";
            this.lblDocumentTime.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDocumentDate
            // 
            this.lblDocumentDate.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Bold);
            this.lblDocumentDate.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDocumentDate.Location = new System.Drawing.Point(90, 17);
            this.lblDocumentDate.Name = "lblDocumentDate";
            this.lblDocumentDate.Size = new System.Drawing.Size(94, 17);
            this.lblDocumentDate.Text = "01.01.2000";
            this.lblDocumentDate.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblDocumentId
            // 
            this.lblDocumentId.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Bold);
            this.lblDocumentId.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDocumentId.Location = new System.Drawing.Point(90, 3);
            this.lblDocumentId.Name = "lblDocumentId";
            this.lblDocumentId.Size = new System.Drawing.Size(94, 28);
            this.lblDocumentId.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblTime
            // 
            this.lblTime.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Bold);
            this.lblTime.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblTime.Location = new System.Drawing.Point(1, 34);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(90, 10);
            this.lblTime.Text = "Saat     :";
            // 
            // lblDate
            // 
            this.lblDate.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Bold);
            this.lblDate.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDate.Location = new System.Drawing.Point(0, 17);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(90, 17);
            this.lblDate.Text = "Tarih    :";
            // 
            // lblDocument
            // 
            this.lblDocument.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Bold);
            this.lblDocument.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblDocument.Location = new System.Drawing.Point(0, 3);
            this.lblDocument.Name = "lblDocument";
            this.lblDocument.Size = new System.Drawing.Size(90, 14);
            this.lblDocument.Text = "Belge No :";
            // 
            // lblCustomer
            // 
            this.lblCustomer.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblCustomer.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblCustomer.Location = new System.Drawing.Point(0, 0);
            this.lblCustomer.Name = "lblCustomer";
            this.lblCustomer.Size = new System.Drawing.Size(166, 75);
            this.lblCustomer.Text = "Müþteri Giriþi Yok";
            this.lblCustomer.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlAdjustments
            // 
            this.pnlAdjustments.Controls.Add(this.lblAdjustment);
            this.pnlAdjustments.Controls.Add(this.lblTotalAdjustAmount);
            this.pnlAdjustments.Controls.Add(this.lblSubtotalAdjustAmount);
            this.pnlAdjustments.Controls.Add(this.lblProductAdjustAmount);
            this.pnlAdjustments.Controls.Add(this.lblTotalAdjustment);
            this.pnlAdjustments.Controls.Add(this.lblSubtotalAdjustment);
            this.pnlAdjustments.Controls.Add(this.lblProductAdjustment);
            this.pnlAdjustments.Location = new System.Drawing.Point(166, 0);
            this.pnlAdjustments.Name = "pnlAdjustments";
            this.pnlAdjustments.Size = new System.Drawing.Size(274, 52);
            // 
            // lblAdjustment
            // 
            this.lblAdjustment.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblAdjustment.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.lblAdjustment.Location = new System.Drawing.Point(0, 3);
            this.lblAdjustment.Name = "lblAdjustment";
            this.lblAdjustment.Size = new System.Drawing.Size(300, 15);
            this.lblAdjustment.Text = "Ýndirim ve Artýrýmlar";
            this.lblAdjustment.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblTotalAdjustAmount
            // 
            this.lblTotalAdjustAmount.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblTotalAdjustAmount.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblTotalAdjustAmount.Location = new System.Drawing.Point(200, 35);
            this.lblTotalAdjustAmount.Name = "lblTotalAdjustAmount";
            this.lblTotalAdjustAmount.Size = new System.Drawing.Size(82, 22);
            this.lblTotalAdjustAmount.Text = "0.00";
            this.lblTotalAdjustAmount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblSubtotalAdjustAmount
            // 
            this.lblSubtotalAdjustAmount.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblSubtotalAdjustAmount.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblSubtotalAdjustAmount.Location = new System.Drawing.Point(99, 35);
            this.lblSubtotalAdjustAmount.Name = "lblSubtotalAdjustAmount";
            this.lblSubtotalAdjustAmount.Size = new System.Drawing.Size(102, 22);
            this.lblSubtotalAdjustAmount.Text = "0.00";
            this.lblSubtotalAdjustAmount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblProductAdjustAmount
            // 
            this.lblProductAdjustAmount.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblProductAdjustAmount.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblProductAdjustAmount.Location = new System.Drawing.Point(0, 35);
            this.lblProductAdjustAmount.Name = "lblProductAdjustAmount";
            this.lblProductAdjustAmount.Size = new System.Drawing.Size(101, 22);
            this.lblProductAdjustAmount.Text = "0.00";
            this.lblProductAdjustAmount.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblTotalAdjustment
            // 
            this.lblTotalAdjustment.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblTotalAdjustment.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblTotalAdjustment.Location = new System.Drawing.Point(200, 17);
            this.lblTotalAdjustment.Name = "lblTotalAdjustment";
            this.lblTotalAdjustment.Size = new System.Drawing.Size(82, 14);
            this.lblTotalAdjustment.Text = "Net";
            this.lblTotalAdjustment.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblSubtotalAdjustment
            // 
            this.lblSubtotalAdjustment.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblSubtotalAdjustment.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblSubtotalAdjustment.Location = new System.Drawing.Point(99, 17);
            this.lblSubtotalAdjustment.Name = "lblSubtotalAdjustment";
            this.lblSubtotalAdjustment.Size = new System.Drawing.Size(102, 14);
            this.lblSubtotalAdjustment.Text = "Aratoplam";
            this.lblSubtotalAdjustment.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblProductAdjustment
            // 
            this.lblProductAdjustment.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblProductAdjustment.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblProductAdjustment.Location = new System.Drawing.Point(0, 18);
            this.lblProductAdjustment.Name = "lblProductAdjustment";
            this.lblProductAdjustment.Size = new System.Drawing.Size(101, 13);
            this.lblProductAdjustment.Text = "Ürün";
            this.lblProductAdjustment.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.pbxHuginLogo);
            this.pnlHeader.Controls.Add(this.lblLogo);
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(800, 100);
            // 
            // pbxHuginLogo
            // 
            this.pbxHuginLogo.Location = new System.Drawing.Point(525, 12);
            this.pbxHuginLogo.Name = "pbxHuginLogo";
            this.pbxHuginLogo.Size = new System.Drawing.Size(253, 87);
            // 
            // lblLogo
            // 
            this.lblLogo.BackColor = System.Drawing.Color.SlateGray;
            this.lblLogo.Font = new System.Drawing.Font("Cambria", 16F, System.Drawing.FontStyle.Bold);
            this.lblLogo.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lblLogo.Location = new System.Drawing.Point(0, 0);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(525, 97);
            this.lblLogo.Text = "HUGIN YAZILIM TEKNOLOJÝLERÝ";
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlCurrent
            // 
            this.pnlCurrent.BackColor = System.Drawing.Color.Black;
            this.pnlCurrent.Controls.Add(this.lblSecondMessage);
            this.pnlCurrent.Controls.Add(this.lblFirstMessage);
            this.pnlCurrent.Location = new System.Drawing.Point(0, 0);
            this.pnlCurrent.Name = "pnlCurrent";
            this.pnlCurrent.Size = new System.Drawing.Size(800, 100);
            // 
            // clmnNo
            // 
            this.clmnNo.Text = "No";
            this.clmnNo.Width = 50;
            // 
            // clmnBarcode
            // 
            this.clmnBarcode.Text = "Barkod";
            this.clmnBarcode.Width = 145;
            // 
            // clmnProductName
            // 
            this.clmnProductName.Text = "Ürün Adý";
            this.clmnProductName.Width = 215;
            // 
            // clmnQuantity
            // 
            this.clmnQuantity.Text = "Adet";
            this.clmnQuantity.Width = 65;
            // 
            // clmnUnit
            // 
            this.clmnUnit.Text = "Birim";
            this.clmnUnit.Width = 75;
            // 
            // clmnPrice
            // 
            this.clmnPrice.Text = "Fiyat";
            this.clmnPrice.Width = 110;
            // 
            // clmnAmount
            // 
            this.clmnAmount.Text = "Tutar";
            this.clmnAmount.Width = 120;
            // 
            // listViewSales
            // 
            this.listViewSales.BackColor = System.Drawing.Color.White;
            this.listViewSales.Columns.Add(this.clmnNo);
            this.listViewSales.Columns.Add(this.clmnBarcode);
            this.listViewSales.Columns.Add(this.clmnProductName);
            this.listViewSales.Columns.Add(this.clmnQuantity);
            this.listViewSales.Columns.Add(this.clmnUnit);
            this.listViewSales.Columns.Add(this.clmnPrice);
            this.listViewSales.Columns.Add(this.clmnAmount);
            this.listViewSales.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.listViewSales.ForeColor = System.Drawing.Color.Black;
            this.listViewSales.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewSales.Location = new System.Drawing.Point(0, 100);
            this.listViewSales.Name = "listViewSales";
            this.listViewSales.Size = new System.Drawing.Size(640, 322);
            this.listViewSales.TabIndex = 3;
            this.listViewSales.View = System.Windows.Forms.View.Details;
            this.listViewSales.Visible = false;
            // 
            // pbxAdvertisement
            // 
            this.pbxAdvertisement.Location = new System.Drawing.Point(333, 102);
            this.pbxAdvertisement.Name = "pbxAdvertisement";
            this.pbxAdvertisement.Size = new System.Drawing.Size(306, 322);
            this.pbxAdvertisement.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxAdvertisement.Visible = false;
            // 
            // CustomerForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.ControlBox = false;
            this.Controls.Add(this.pbxAdvertisement);
            this.Controls.Add(this.pnlCurrent);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlDetail);
            this.Controls.Add(this.listViewSales);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimizeBox = false;
            this.Name = "CustomerForm";
            this.pnlDetail.ResumeLayout(false);
            this.pnlProductImage.ResumeLayout(false);
            this.pnlDocumentInfo.ResumeLayout(false);
            this.pnlAdjustments.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlCurrent.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        private void EnableAdvertisementPanel(string path)
        {
            //if (!LoadAdvertisementImages(path)) return;
            clmnNo.Width = 0;//dgSales.Columns["clmnOrder"].Visible = false; 
            clmnBarcode.Width = 0;//dgSales.Columns["clmnBarcode"].Visible = false; 
            clmnProductName.Width = 180;//dgSales.Columns["clmnProductName"].Width = 180; 
            clmnQuantity.Width = 55;//dgSales.Columns["clmnQuantity"].Width = 55;
            clmnUnit.Width = 0; //dgSales.Columns["clmnUnit"].Visible = false; 
            clmnPrice.Width = 72;//dgSales.Columns["clmnPrice"].Width = 72; 
            clmnAmount.Width = 85;//dgSales.Columns["clmnAmount"].Width = 85; 
            listViewSales.Width = 410;//dgSales.Width = 410; 
            pbxAdvertisement.Left = 410;
            pbxAdvertisement.Visible = true;
            pbxAdvertisement.BringToFront();
            pbxAdvertisement.SizeMode = PictureBoxSizeMode.CenterImage;
            //System.Threading.Thread serialThread = new System.Threading.Thread(delegate() { ShowAvertisement(); });
            //serialThread.Start();


        }

        #region Public Functions

        public void Show(String message)
        {
            if (!Str.Contains(message, "\n")) message = message + "\n";
            String[] lines = message.Split('\n');

            ToggleHeaderCurrentPanel(message == Common.PosMessage.WELCOME);
            SetFirstMessage(AdjustMessageLine(lines[0]));
            SetSecondMessage(AdjustMessageLine(lines[1]));
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
                        Show("{0}\n \t{1:C}", Common.PosMessage.SUBTOTAL_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n \t{1:C}", Common.PosMessage.SUBTOTAL_PRICE_FEE,
                                             new Number(adjustment.NetAmount));

                    SetSubtotalAdjustment(subtotalAdjustments + adjustment.NetAmount);
                }
                else throw new InvalidProgramException("Adjustment target is incorrectly set");
            }
            catch (FormatException e)
            {
                EZLogger.Log.Error("Display error. {0}", e.Message);
            }
        }

        public void ShowSale(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)), large ? "X" : fi.Unit, new Number(fi.TotalAmount - fi.VoidAmount));
                ShowItem(fi);
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
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)), large ? "X" : fi.Unit, new Number(fi.TotalAmount - fi.VoidAmount));
            }
            catch (FormatException fex)
            {
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        public void ShowVoid(IFiscalItem fi)
        {// Todo: Format problem
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(fi.Quantity), large ? "X" : fi.Unit, new Number(fi.TotalAmount));
                ShowItem(fi);

            }
            catch (System.NotSupportedException) { }
            catch (FormatException fex)
            {
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        internal void ShowCorrect(IAdjustment adjustment)
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

        public void Show(IProduct p)
        {
            try
            {
                bool large = ((p.Quantity != (long)p.Quantity) && p.UnitPrice * p.Quantity > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", p.Name, new Number(p.Quantity), large ? "X" : p.Unit, new Number(p.UnitPrice * p.Quantity));
                String path = PosConfiguration.ImagePath + p.Barcode + ".jpg";
                if (!System.IO.File.Exists(path))
                    path = PosConfiguration.ImagePath + p.Barcode + ".jpeg";
                SetImageSlide(path);

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

        public delegate void ShowMenuDelegate(IMenuList menuList);
        public void ShowMenu(IMenuList menuList)
        {
            if (menuForm.InvokeRequired)
                menuForm.Invoke(new ShowMenuDelegate(ShowMenu), menuList);
            else
            {
                menuForm.ShowList(menuList);
                if (menuForm.Opacity == 1)
                    menuForm.Show();
                else
                    this.BringToFront();
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

        public virtual bool KeyAvailable
        {
            get { return false; }
        }
        #endregion Public Functions

        #region External Events

        public void SetDocumentInfos(ISalesDocument doc)
        {
            SetDocumentId(doc.Id);
            SetDocumentDate(doc.CreatedDate);
            SetDocumentTime(doc.CreatedDate);
        }

        public void ChangeCurrentDocument(ISalesDocument doc)
        {
            ChangeDocument(doc);
        }

        public void DocumentClose(ISalesDocument doc)
        {
            ClearDetails();
        }

        public void UndoAdjustment(ISalesDocument doc)
        {
            SetSubtotalAdjustment(0);
            SetTotalAdjustment();
        }

        public void ChangeCustomer(ICustomer c)
        {
            if (c == null)
                SetDocumentCustomer("Müþteri Giriþi Yok");
            else
            {
                SetDocumentCustomer(c.Name);
            }

        }

        public void LoginCashier(ICashier c)
        {

        }

        #endregion External Events

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
            if (listViewSales.InvokeRequired)
                listViewSales.Invoke(new ClearProductsDelegate(this.ClearProducts));
            else
                listViewSales.Items.Clear();
        }

        private delegate void ShowItemDelegate(IFiscalItem fi);
        private void ShowItem(IFiscalItem fi)
        {
            if (listViewSales.InvokeRequired)
                this.listViewSales.Invoke(new ShowItemDelegate(this.ShowItem), fi);
            else
            {
                //TODO:
                //if (cr.Document.Items.Count == 1)
                //{
                //    SetDocumentId(cr.Document.Id);
                //    SetDocumentDate(cr.Document.CreatedDate);
                //    SetDocumentTime(cr.Document.CreatedDate);
                //}
                ListViewItem item = new ListViewItem();
                item.Text = String.Format("{0:D3}", listViewSales.Items.Count + 1);
                item.SubItems.Add(fi.Product.Barcode);
                item.SubItems.Add(fi.Name);
                item.SubItems.Add(String.Format("{0}", fi.Quantity));
                item.SubItems.Add(fi.Unit);
                item.SubItems.Add(String.Format("{0:0.00}", fi.UnitPrice));
                item.SubItems.Add(String.Format("{0:0.00}", fi.TotalAmount));

                listViewSales.Items.Insert(0, item);

                listViewSales.Items[0].BackColor = (listViewSales.Items.Count % 2 == 0) ? Color.White : Color.LightGray;

                if (!(fi is IFiscalItem))
                    listViewSales.Items[1].ForeColor = Color.Red;
                if (productAdjustments != 0)
                    SetProductAdjustment(0);

                String path = PosConfiguration.ImagePath + fi.Product.Barcode + ".jpg";
                if (!System.IO.File.Exists(path))
                    path = PosConfiguration.ImagePath + fi.Barcode + ".jpeg";
                if (!System.IO.File.Exists(path))
                    SetItemImage(null);
                else
                {
                    Image img = new Bitmap(path);
                    //if (Document.Items.Count <= listViewSales.Items.Count)
                    //    SetImageSlide(path);
                    //else
                    //    SetItemImage(img);
                }

            }
        }

        private delegate void SetDocumentCustomerDelegate(String customerInfo);
        private void SetDocumentCustomer(String customerInfo)
        {
            if (lblCustomer.InvokeRequired)
                lblCustomer.Invoke(new SetDocumentCustomerDelegate(this.SetDocumentCustomer), customerInfo);
            else
                lblCustomer.Text = customerInfo;
        }

        private delegate void SetProductAdjustmentDelegate(Decimal adjustment);
        private void SetProductAdjustment(Decimal adjustment)
        {
            if (lblProductAdjustAmount.InvokeRequired)
                lblProductAdjustAmount.Invoke(new SetProductAdjustmentDelegate(this.SetProductAdjustment), adjustment);
            else
            {
                productAdjustments = adjustment;
                lblProductAdjustAmount.Text = String.Format("{0:0.00}", productAdjustments);
                SetTotalAdjustment();
            }
        }

        private delegate void SetSubotalAdjustmentDelegate(Decimal adjustment);
        private void SetSubtotalAdjustment(Decimal adjustment)
        {
            if (lblSubtotalAdjustAmount.InvokeRequired)
                lblSubtotalAdjustAmount.Invoke(new SetSubotalAdjustmentDelegate(this.SetSubtotalAdjustment), adjustment);
            else
            {
                subtotalAdjustments = adjustment;
                lblSubtotalAdjustAmount.Text = String.Format("{0:0.00}", subtotalAdjustments);
                SetTotalAdjustment();
            }
        }

        private delegate void SetTotalAdjustmentDelegate();
        private void SetTotalAdjustment()
        {
            if (lblTotalAdjustAmount.InvokeRequired)
                lblTotalAdjustAmount.Invoke(new SetTotalAdjustmentDelegate(this.SetTotalAdjustment));
            else
                lblTotalAdjustAmount.Text = String.Format("{0:0.00}", subtotalAdjustments + globalProductAdjustments);
        }

        private delegate void SetLastItemAdjustmentDelegate(Decimal adjustment);
        private void SetLastItemAdjustment(Decimal adjustment)
        {
            if (listViewSales.InvokeRequired)
                this.listViewSales.Invoke(new SetLastItemAdjustmentDelegate(this.SetLastItemAdjustment), adjustment);
            else
            {
                int priceIndex = listViewSales.Items[0].SubItems.Count - 2;
                //listViewSales.Items[0].SubItems[priceIndex].Text = String.Format("{0:N2}", Document.LastItem.UnitPrice);
                //listViewSales.Items[0].SubItems[priceIndex + 1].Text = String.Format("{0:N2}", Document.LastItem.TotalAmount);
                //if (Document.LastItem.TotalAmount == Document.LastItem.ListedAmount)
                //    listViewSales.Items[0].ForeColor = Color.Black;
                //else if (Document.LastItem.TotalAmount < Document.LastItem.ListedAmount)
                //    listViewSales.Items[0].ForeColor = Color.Green;
                //else this.listViewSales.Items[0].ForeColor = Color.Blue;
                globalProductAdjustments += adjustment;
                SetProductAdjustment(productAdjustments + adjustment);
            }
        }

        private delegate void SetDocumentIdDelegate(int id);
        private void SetDocumentId(int id)
        {
            if (lblDocumentId.InvokeRequired)
                lblDocumentId.Invoke(new SetDocumentIdDelegate(this.SetDocumentId), id);
            else
                lblDocumentId.Text = id > 0 ? String.Format("{0:D4}", id) : "";
        }

        private delegate void SetDocumentDateDelegate(DateTime date);
        private void SetDocumentDate(DateTime date)
        {
            if (lblDocumentDate.InvokeRequired)
                lblDocumentDate.Invoke(new SetDocumentDateDelegate(this.SetDocumentDate), date);
            else
                lblDocumentDate.Text = date != DateTime.MinValue ? String.Format("{0:d}", date) : "";
        }

        private delegate void SetDocumentTimeDelegate(DateTime date);
        private void SetDocumentTime(DateTime date)
        {
            if (lblDocumentTime.InvokeRequired)
                lblDocumentTime.Invoke(new SetDocumentTimeDelegate(this.SetDocumentTime), date);
            else
                lblDocumentTime.Text = date != DateTime.MinValue ? String.Format("{0:T}", date) : "";
        }

        private static Image lastImage = null;
        public void SetImageSlide(String path)
        {
            Image img = null;
            if (System.IO.File.Exists(path))
                img = new Bitmap(path);

            if (lastImage == null)
            {
                SetItemImage(img);
                return;
            }

            SetTempItemImage(img);
            for (int i = 0; i < pnlProductImage.Height; i++)
            {
                SetProductImageTopPosition(pbxProduct.Top - 3);
                SetNextProductImageTopPosition(pbxNextProduct.Top - 3);
                RefreshImages();
                System.Threading.Thread.Sleep(1);
                if (pbxNextProduct.Top <= pnlProductImage.Top)
                    break;
            }
            pbxProduct.Image = img;
            //pbxAdvertisement.Image = img;
            SetProductImageTopPosition(pnlProductImage.Top);
            SetNextProductImageTopPosition(pnlProductImage.Height);
            RefreshImages();
            lastImage = img;
        }

        private delegate void RefreshImagesDelegate();
        private void RefreshImages()
        {
            if (pbxProduct.InvokeRequired)
                pbxProduct.Invoke(new RefreshImagesDelegate(this.RefreshImages));
            else
                pbxProduct.Refresh();

            if (pbxNextProduct.InvokeRequired)
                pbxNextProduct.Invoke(new RefreshImagesDelegate(this.RefreshImages));
            else
                pbxNextProduct.Refresh();

        }

        private delegate void SetProductImageTopPositionDelegate(int top);
        private void SetProductImageTopPosition(int top)
        {
            if (pbxProduct.InvokeRequired)
                pbxProduct.Invoke(new SetProductImageTopPositionDelegate(this.SetProductImageTopPosition), top);
            else
                pbxProduct.Top = top;
        }

        private delegate void SetItemImageDelegate(Image img);
        private void SetItemImage(Image img)
        {
            if (pbxProduct.InvokeRequired)
                pbxProduct.Invoke(new SetItemImageDelegate(this.SetItemImage), img);
            else
                pbxProduct.Image = img;
            lastImage = img;

        }

        private delegate void SetTempItemImageDelegate(Image img);
        private void SetTempItemImage(Image img)
        {
            if (pbxNextProduct.InvokeRequired)
                pbxNextProduct.Invoke(new SetItemImageDelegate(this.SetTempItemImage), img);
            else
                pbxNextProduct.Image = img;
        }

        private delegate void SetNextProductImageTopPositionDelegate(int top);
        private void SetNextProductImageTopPosition(int top)
        {
            if (pbxNextProduct.InvokeRequired)
                pbxNextProduct.Invoke(new SetNextProductImageTopPositionDelegate(this.SetNextProductImageTopPosition), top);
            else
                pbxNextProduct.Top = top;
        }
        internal void SetLed(Leds led)
        {
            if (
                (((led & Leds.Customer) == Leds.Customer) && (Convert.ToInt32(lblCustomerLed.Tag) == 0)) ||
                (((led & Leds.Customer) != Leds.Customer) && (Convert.ToInt32(lblCustomerLed.Tag) == 1))
               )
                ChangeMod(lblCustomerLed);
            if (
                (((led & Leds.Online) == Leds.Online) && (Convert.ToInt32(lblOnlineLed.Tag) == 0)) ||
                (((led & Leds.Online) != Leds.Online) && (Convert.ToInt32(lblOnlineLed.Tag) == 1))
                )
                ChangeMod(lblOnlineLed);
            if (
                (((led & Leds.Sale) == Leds.Sale) && (Convert.ToInt32(lblSaleLed.Tag) == 0)) ||
                (((led & Leds.Sale) != Leds.Sale) && (Convert.ToInt32(lblSaleLed.Tag) == 1))
                )
                ChangeMod(lblSaleLed);
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
                ShowItem(fi);

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
                SetDocumentCustomer(customerInfo);
            }
            else
                SetDocumentCustomer("Müþteri Giriþi Yok");

            SetDocumentId(sDoc.Id);
            SetDocumentDate(sDoc.CreatedDate);
            SetDocumentTime(sDoc.CreatedDate);

        }

        private void ClearDetails()
        {
            ClearProducts();
            SetDocumentCustomer("Müþteri Giriþi Yok");
            globalProductAdjustments = 0;
            SetProductAdjustment(0);
            SetSubtotalAdjustment(0);
            SetDocumentDate(DateTime.MinValue);
            SetDocumentId(0);
            SetDocumentTime(DateTime.MinValue);

            string path = PosConfiguration.ImagePath + "DefaultProduct.jpg";
            if (!System.IO.File.Exists(path))
                path = PosConfiguration.ImagePath + "DefaultProduct.jpeg";
            if (System.IO.File.Exists(path))
                SetItemImage(new Bitmap(path));
            else
                SetItemImage(null);
        }

        #endregion Common Functions

        #region Helper Functions

        private string AdjustMessageLine(string line)
        {
            if (Str.Contains(line, "\t"))
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

    }
}
