namespace Hugin.POS.Display.Datecs
{
    partial class CashierForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CashierForm));
            this.pnlCustomerMessage = new System.Windows.Forms.Panel();
            this.lblSecondLine = new CaretLabel();
            this.lblFirstLine = new CaretLabel();
            this.pnlReceipt = new System.Windows.Forms.Panel();
            this.dgvReceipt = new Hugin.POS.Display.Datecs.GridLineDataGridView();
            this.clmnIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmnQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmnUnitPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmnTotalAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlKeypad = new System.Windows.Forms.Panel();
            this.tblKeypad = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTotals = new System.Windows.Forms.Panel();
            this.tblTotals = new System.Windows.Forms.TableLayoutPanel();
            this.lblTotalAmount = new System.Windows.Forms.Label();
            this.lblAdjustment = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblAdjustmentAmount = new System.Windows.Forms.Label();
            this.gfbUpArrow = new System.Windows.Forms.Button();
            this.gfbDownArrow = new System.Windows.Forms.Button();
            this.pnlFooter = new System.Windows.Forms.FlowLayoutPanel();
            this.pbxOnlineLed = new System.Windows.Forms.PictureBox();
            this.lblOnline = new System.Windows.Forms.Label();
            this.pbxSale = new System.Windows.Forms.PictureBox();
            this.lblSale = new System.Windows.Forms.Label();
            this.pbxCustomer = new System.Windows.Forms.PictureBox();
            this.lblCustmoer = new System.Windows.Forms.Label();
            this.pbxFiscalId = new System.Windows.Forms.PictureBox();
            this.lblFiscalId = new System.Windows.Forms.Label();
            this.pbxRegisterId = new System.Windows.Forms.PictureBox();     
            this.lblRegisterId = new System.Windows.Forms.Label();
            this.pbxDateTime = new System.Windows.Forms.PictureBox();
            this.lblDateTime = new System.Windows.Forms.Label();
            this.pnlCustomerMessage.SuspendLayout();
            this.pnlReceipt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReceipt)).BeginInit();
            this.pnlKeypad.SuspendLayout();
            this.pnlTotals.SuspendLayout();
            this.tblTotals.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxOnlineLed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxSale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxFiscalId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxRegisterId)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxDateTime)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlCustomerMessage
            // 
            this.pnlCustomerMessage.BackColor = System.Drawing.Color.Black;
            this.pnlCustomerMessage.Controls.Add(this.lblSecondLine);
            this.pnlCustomerMessage.Controls.Add(this.lblFirstLine);
            this.pnlCustomerMessage.Location = new System.Drawing.Point(5, 5);
            this.pnlCustomerMessage.Name = "pnlCustomerMessage";
            this.pnlCustomerMessage.Size = new System.Drawing.Size(360, 74);
            this.pnlCustomerMessage.TabIndex = 0;
            // 
            // lblSecondLine
            // 
            this.lblSecondLine.CaretIndex = 1;
            this.lblSecondLine.Font = new System.Drawing.Font("Courier New", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.lblSecondLine.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblSecondLine.IsCaretEnable = true;
            this.lblSecondLine.Location = new System.Drawing.Point(-3, 34);
            this.lblSecondLine.MaximumLength = 100;
            this.lblSecondLine.Name = "lblSecondLine";
            this.lblSecondLine.Size = new System.Drawing.Size(803, 27);
            this.lblSecondLine.TabIndex = 1;
            this.lblSecondLine.Text = "12345678901234567890";
            // 
            // lblFirstLine
            // 
            this.lblFirstLine.CaretIndex = 0;
            this.lblFirstLine.Font = new System.Drawing.Font("Courier New", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.lblFirstLine.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblFirstLine.IsCaretEnable = false;
            this.lblFirstLine.Location = new System.Drawing.Point(-3, 4);
            this.lblFirstLine.MaximumLength = 100;
            this.lblFirstLine.Name = "lblFirstLine";
            this.lblFirstLine.Size = new System.Drawing.Size(366, 27);
            this.lblFirstLine.TabIndex = 0;
            this.lblFirstLine.Text = "12345678901234567890";
            // 
            // pnlReceipt
            // 
            this.pnlReceipt.BackColor = System.Drawing.Color.White;
            this.pnlReceipt.Controls.Add(this.dgvReceipt);
            this.pnlReceipt.Location = new System.Drawing.Point(5, 72);
            this.pnlReceipt.Name = "pnlReceipt";
            this.pnlReceipt.Size = new System.Drawing.Size(360, 548);
            this.pnlReceipt.TabIndex = 1;
            // 
            // dgvReceipt
            // 
            this.dgvReceipt.AllowUserToAddRows = false;
            this.dgvReceipt.AllowUserToDeleteRows = false;
            this.dgvReceipt.AllowUserToResizeColumns = false;
            this.dgvReceipt.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            this.dgvReceipt.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvReceipt.BackgroundColor = System.Drawing.Color.White;
            this.dgvReceipt.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvReceipt.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReceipt.ColumnHeadersVisible = false;
            this.dgvReceipt.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clmnIndex,
            this.clmnName,
            this.clmnQuantity,
            this.clmnUnitPrice,
            this.clmnTotalAmount});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvReceipt.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvReceipt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvReceipt.Location = new System.Drawing.Point(0, 0);
            this.dgvReceipt.MultiSelect = false;
            this.dgvReceipt.Name = "dgvReceipt";
            this.dgvReceipt.ReadOnly = true;
            this.dgvReceipt.RowHeadersVisible = false;
            this.dgvReceipt.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.dgvReceipt.RowTemplate.Height = 30;
            this.dgvReceipt.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dgvReceipt.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvReceipt.ShowEditingIcon = false;
            this.dgvReceipt.ShowRowErrors = false;
            this.dgvReceipt.Size = new System.Drawing.Size(360, 548);
            this.dgvReceipt.TabIndex = 0;
            //
            // clmnIndex
            //
            this.clmnIndex.FillWeight = 34F;
            this.clmnIndex.HeaderText = "No";
            this.clmnIndex.Name = "clmnIndex";
            this.clmnIndex.ReadOnly = true;
            this.clmnIndex.Width = 34;
            // 
            // clmnName
            // 
            this.clmnName.FillWeight = 145F;
            this.clmnName.HeaderText = "Ürün Adı";
            this.clmnName.Name = "clmnName";
            this.clmnName.ReadOnly = true;
            this.clmnName.Width = 145;
            // 
            // clmnQuantity
            // 
            this.clmnQuantity.FillWeight = 45F;
            this.clmnQuantity.HeaderText = "Adet";
            this.clmnQuantity.Name = "clmnQuantity";
            this.clmnQuantity.ReadOnly = true;
            this.clmnQuantity.Width = 45;
            // 
            // clmnUnitPrice
            // 
            this.clmnUnitPrice.FillWeight = 45F;
            this.clmnUnitPrice.HeaderText = "Birim Fiyatı";
            this.clmnUnitPrice.Name = "clmnUnitPrice";
            this.clmnUnitPrice.ReadOnly = true;
            this.clmnUnitPrice.Width = 45;
            // 
            // clmnTotalAmount
            // 
            this.clmnTotalAmount.FillWeight = 80F;
            this.clmnTotalAmount.HeaderText = "Tutar";
            this.clmnTotalAmount.Name = "clmnTotalAmount";
            this.clmnTotalAmount.ReadOnly = true;
            this.clmnTotalAmount.Width = 80;
            // 
            // pnlKeypad
            // 
            this.pnlKeypad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(64)))), ((int)(((byte)(97)))));
            this.pnlKeypad.Controls.Add(this.tblKeypad);
            this.pnlKeypad.Location = new System.Drawing.Point(371, 5);
            this.pnlKeypad.Margin = new System.Windows.Forms.Padding(0);
            this.pnlKeypad.Name = "pnlKeypad";
            this.pnlKeypad.Size = new System.Drawing.Size(649, 708);
            this.pnlKeypad.TabIndex = 3;
            // 
            // tblKeypad
            // 
            this.tblKeypad.ColumnCount = 6;
            this.tblKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tblKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tblKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tblKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tblKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tblKeypad.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tblKeypad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblKeypad.Location = new System.Drawing.Point(0, 0);
            this.tblKeypad.Name = "tblKeypad";
            this.tblKeypad.RowCount = 8;
            this.tblKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblKeypad.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tblKeypad.Size = new System.Drawing.Size(649, 708);
            this.tblKeypad.TabIndex = 0;
            // 
            // pnlTotals
            // 
            this.pnlTotals.BackColor = System.Drawing.Color.White;
            this.pnlTotals.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTotals.Controls.Add(this.tblTotals);
            this.pnlTotals.Location = new System.Drawing.Point(5, 627);
            this.pnlTotals.Name = "pnlTotals";
            this.pnlTotals.Size = new System.Drawing.Size(360, 85);
            this.pnlTotals.TabIndex = 2;
            // 
            // tblTotals
            // 
            this.tblTotals.ColumnCount = 3;
            this.tblTotals.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tblTotals.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblTotals.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tblTotals.Controls.Add(this.lblTotalAmount, 2, 1);
            this.tblTotals.Controls.Add(this.lblAdjustment, 1, 0);
            this.tblTotals.Controls.Add(this.lblTotal, 1, 1);
            this.tblTotals.Controls.Add(this.lblAdjustmentAmount, 2, 0);
            this.tblTotals.Controls.Add(this.gfbUpArrow, 0, 0);
            this.tblTotals.Controls.Add(this.gfbDownArrow, 0, 1);
            this.tblTotals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblTotals.Location = new System.Drawing.Point(0, 0);
            this.tblTotals.Name = "tblTotals";
            this.tblTotals.RowCount = 2;
            this.tblTotals.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblTotals.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblTotals.Size = new System.Drawing.Size(358, 83);
            this.tblTotals.TabIndex = 0;
            // 
            // lblTotalAmount
            // 
            this.lblTotalAmount.AutoSize = true;
            this.lblTotalAmount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalAmount.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTotalAmount.Location = new System.Drawing.Point(145, 41);
            this.lblTotalAmount.Name = "lblTotalAmount";
            this.lblTotalAmount.Size = new System.Drawing.Size(210, 42);
            this.lblTotalAmount.TabIndex = 3;
            this.lblTotalAmount.Text = "0,00";
            this.lblTotalAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAdjustment
            // 
            this.lblAdjustment.AutoSize = true;
            this.lblAdjustment.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAdjustment.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblAdjustment.Location = new System.Drawing.Point(56, 22);
            this.lblAdjustment.Name = "lblAdjustment";
            this.lblAdjustment.Size = new System.Drawing.Size(83, 19);
            this.lblAdjustment.TabIndex = 0;
            this.lblAdjustment.Text = "İNDİRİM";
            this.lblAdjustment.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotal.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTotal.Location = new System.Drawing.Point(56, 41);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(83, 42);
            this.lblTotal.TabIndex = 2;
            this.lblTotal.Text = "TOPLAM";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAdjustmentAmount
            // 
            this.lblAdjustmentAmount.AutoSize = true;
            this.lblAdjustmentAmount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAdjustmentAmount.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblAdjustmentAmount.Location = new System.Drawing.Point(145, 22);
            this.lblAdjustmentAmount.Name = "lblAdjustmentAmount";
            this.lblAdjustmentAmount.Size = new System.Drawing.Size(210, 19);
            this.lblAdjustmentAmount.TabIndex = 1;
            this.lblAdjustmentAmount.Text = "0,00";
            this.lblAdjustmentAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gfbUpArrow
            // 
            this.gfbUpArrow.BackColor = System.Drawing.Color.DimGray;
            this.gfbUpArrow.Dock = System.Windows.Forms.DockStyle.Left;
            this.gfbUpArrow.Location = new System.Drawing.Point(3, 3);
            this.gfbUpArrow.Name = "gfbUpArrow";
            this.gfbUpArrow.Size = new System.Drawing.Size(47, 35);
            //this.gfbUpArrow.StartColor = System.Drawing.Color.DimGray;
            this.gfbUpArrow.TabIndex = 4;
            this.gfbUpArrow.Text = "↑";
            this.gfbUpArrow.UseVisualStyleBackColor = false;
            this.gfbUpArrow.Visible = false;
            this.gfbUpArrow.Click += new System.EventHandler(this.gfbUpArrow_Click);
            // 
            // gfbDownArrow
            // 
            this.gfbDownArrow.BackColor = System.Drawing.Color.DimGray;
            this.gfbDownArrow.Dock = System.Windows.Forms.DockStyle.Left;
            this.gfbDownArrow.Location = new System.Drawing.Point(3, 44);
            this.gfbDownArrow.Name = "gfbDownArrow";
            this.gfbDownArrow.Size = new System.Drawing.Size(47, 36);
            //this.gfbDownArrow.StartColor = System.Drawing.Color.DimGray;
            this.gfbDownArrow.TabIndex = 5;
            this.gfbDownArrow.Text = "↓";
            this.gfbDownArrow.UseVisualStyleBackColor = false;
            this.gfbDownArrow.Visible = false;
            this.gfbDownArrow.Click += new System.EventHandler(this.gfbDownArrow_Click);
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pnlFooter.BackgroundImage")));
            this.pnlFooter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlFooter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFooter.Controls.Add(this.pbxOnlineLed);
            this.pnlFooter.Controls.Add(this.lblOnline);
            this.pnlFooter.Controls.Add(this.pbxSale);
            this.pnlFooter.Controls.Add(this.lblSale);
            this.pnlFooter.Controls.Add(this.pbxCustomer);
            this.pnlFooter.Controls.Add(this.lblCustmoer);
            this.pnlFooter.Controls.Add(this.pbxFiscalId);
            this.pnlFooter.Controls.Add(this.lblFiscalId);
            this.pnlFooter.Controls.Add(this.pbxRegisterId);
            this.pnlFooter.Controls.Add(this.lblRegisterId);
            this.pnlFooter.Controls.Add(this.pbxDateTime);
            this.pnlFooter.Controls.Add(this.lblDateTime);
            this.pnlFooter.Location = new System.Drawing.Point(0, 718);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1024, 50);
            this.pnlFooter.TabIndex = 2;
            // 
            // pbxOnlineLed
            // 
            this.pbxOnlineLed.BackColor = System.Drawing.Color.Transparent;
            this.pbxOnlineLed.Image = global::Hugin.POS.Display.Datecs.Properties.Resources.ledoff;
            this.pbxOnlineLed.Location = new System.Drawing.Point(3, 3);
            this.pbxOnlineLed.Name = "pbxOnlineLed";
            this.pbxOnlineLed.Size = new System.Drawing.Size(38, 38);
            this.pbxOnlineLed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxOnlineLed.TabIndex = 0;
            this.pbxOnlineLed.TabStop = false;
            // 
            // lblOnline
            // 
            this.lblOnline.BackColor = System.Drawing.Color.Transparent;
            this.lblOnline.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblOnline.ForeColor = System.Drawing.Color.White;
            this.lblOnline.Location = new System.Drawing.Point(47, 0);
            this.lblOnline.Name = "lblOnline";
            this.lblOnline.Size = new System.Drawing.Size(80, 41);
            this.lblOnline.TabIndex = 1;
            this.lblOnline.Text = "ONLINE";
            this.lblOnline.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbxSale
            // 
            this.pbxSale.BackColor = System.Drawing.Color.Transparent;
            this.pbxSale.Image = global::Hugin.POS.Display.Datecs.Properties.Resources.ledoff;
            this.pbxSale.Location = new System.Drawing.Point(133, 3);
            this.pbxSale.Name = "pbxSale";
            this.pbxSale.Size = new System.Drawing.Size(38, 38);
            this.pbxSale.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxSale.TabIndex = 2;
            this.pbxSale.TabStop = false;
            // 
            // lblSale
            // 
            this.lblSale.BackColor = System.Drawing.Color.Transparent;
            this.lblSale.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblSale.ForeColor = System.Drawing.Color.White;
            this.lblSale.Location = new System.Drawing.Point(177, 0);
            this.lblSale.Name = "lblSale";
            this.lblSale.Size = new System.Drawing.Size(80, 41);
            this.lblSale.TabIndex = 3;
            this.lblSale.Text = "SATIŞ";
            this.lblSale.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbxCustomer
            // 
            this.pbxCustomer.BackColor = System.Drawing.Color.Transparent;
            this.pbxCustomer.Image = global::Hugin.POS.Display.Datecs.Properties.Resources.nocustomer;
            this.pbxCustomer.Location = new System.Drawing.Point(263, 3);
            this.pbxCustomer.Name = "pbxCustomer";
            this.pbxCustomer.Size = new System.Drawing.Size(38, 38);
            this.pbxCustomer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxCustomer.TabIndex = 4;
            this.pbxCustomer.TabStop = false;
            // 
            // lblCustmoer
            // 
            this.lblCustmoer.BackColor = System.Drawing.Color.Transparent;
            this.lblCustmoer.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblCustmoer.ForeColor = System.Drawing.Color.White;
            this.lblCustmoer.Location = new System.Drawing.Point(307, 0);
            this.lblCustmoer.Name = "lblCustmoer";
            this.lblCustmoer.Size = new System.Drawing.Size(207, 41);
            this.lblCustmoer.TabIndex = 5;
            this.lblCustmoer.Text = "MÜŞTERİ YOK";
            this.lblCustmoer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pbxFiscalId
            //
            this.pbxFiscalId.BackColor = System.Drawing.Color.Transparent;
            this.pbxFiscalId.Image = global::Hugin.POS.Display.Datecs.Properties.Resources.ecr;
            this.pbxFiscalId.Location = new System.Drawing.Point(520, 3);
            this.pbxFiscalId.Name = "pbxFiscalId";
            this.pbxFiscalId.Size = new System.Drawing.Size(38,38);
            this.pbxFiscalId.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxFiscalId.TabIndex = 6;
            this.pbxFiscalId.TabStop = false;
            // 
            // lblFiscalId
            //
            this.lblFiscalId.BackColor = System.Drawing.Color.Transparent;
            this.lblFiscalId.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblFiscalId.ForeColor = System.Drawing.Color.White;
            this.lblFiscalId.Location = new System.Drawing.Point(564,0);
            this.lblFiscalId.Name = "lblFiscalId";
            this.lblFiscalId.Size = new System.Drawing.Size(130, 41);
            this.lblFiscalId.TabIndex = 7;
            this.lblFiscalId.Text = "KASA SERİ NO";
            this.lblFiscalId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbxRegisterId
            // 
            this.pbxRegisterId.BackColor = System.Drawing.Color.Transparent;
            this.pbxRegisterId.Image = global::Hugin.POS.Display.Datecs.Properties.Resources.register;
            this.pbxRegisterId.Location = new System.Drawing.Point(700, 3);
            this.pbxRegisterId.Name = "pbxRegisterId";
            this.pbxRegisterId.Size = new System.Drawing.Size(38, 38);
            this.pbxRegisterId.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxRegisterId.TabIndex = 8;
            this.pbxRegisterId.TabStop = false;
            // 
            // lblRegisterId
            // 
            this.lblRegisterId.BackColor = System.Drawing.Color.Transparent;
            this.lblRegisterId.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblRegisterId.ForeColor = System.Drawing.Color.White;
            this.lblRegisterId.Location = new System.Drawing.Point(744, 0);
            this.lblRegisterId.Name = "lblRegisterId";
            this.lblRegisterId.Size = new System.Drawing.Size(50, 41);
            this.lblRegisterId.TabIndex = 9;
            this.lblRegisterId.Text = "001";
            this.lblRegisterId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbxDateTime
            // 
            this.pbxDateTime.BackColor = System.Drawing.Color.Transparent;
            this.pbxDateTime.Image = global::Hugin.POS.Display.Datecs.Properties.Resources.clock;
            this.pbxDateTime.Location = new System.Drawing.Point(800, 3);
            this.pbxDateTime.Name = "pbxDateTime";
            this.pbxDateTime.Size = new System.Drawing.Size(38, 38);
            this.pbxDateTime.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxDateTime.TabIndex = 10;
            this.pbxDateTime.TabStop = false;
            // 
            // lblDateTime
            // 
            this.lblDateTime.BackColor = System.Drawing.Color.Transparent;
            this.lblDateTime.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblDateTime.ForeColor = System.Drawing.Color.White;
            this.lblDateTime.Location = new System.Drawing.Point(844, 0);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(170, 41);
            this.lblDateTime.TabIndex = 11;
            this.lblDateTime.Text = "1 Mart 2013 Çarşamba\n\t11:23";
            this.lblDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CustomerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(129)))), ((int)(((byte)(189)))));
            this.ClientSize = new System.Drawing.Size(969, 717);
            this.Controls.Add(this.pnlTotals);
            this.Controls.Add(this.pnlKeypad);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlReceipt);
            this.Controls.Add(this.pnlCustomerMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CustomerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CustomerForm";
            this.Load += new System.EventHandler(this.CustomerForm_Load);
            this.pnlCustomerMessage.ResumeLayout(false);
            this.pnlReceipt.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvReceipt)).EndInit();
            this.pnlKeypad.ResumeLayout(false);
            this.pnlTotals.ResumeLayout(false);
            this.tblTotals.ResumeLayout(false);
            this.tblTotals.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxOnlineLed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxSale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxFiscalId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxRegisterId)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxDateTime)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlCustomerMessage;
        private CaretLabel lblFirstLine;
        private CaretLabel lblSecondLine;
        private System.Windows.Forms.Panel pnlReceipt;
        private System.Windows.Forms.Panel pnlKeypad;
        private System.Windows.Forms.TableLayoutPanel tblKeypad;
        private System.Windows.Forms.Panel pnlTotals;
        private GridLineDataGridView dgvReceipt;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnUnitPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnTotalAmount;
        private System.Windows.Forms.TableLayoutPanel tblTotals;
        private System.Windows.Forms.Label lblAdjustmentAmount;
        private System.Windows.Forms.Label lblTotalAmount;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblAdjustment;
        private System.Windows.Forms.Button gfbUpArrow;
        private System.Windows.Forms.Button gfbDownArrow;
        private System.Windows.Forms.FlowLayoutPanel pnlFooter;
        private System.Windows.Forms.PictureBox pbxOnlineLed;
        private System.Windows.Forms.Label lblOnline;
        private System.Windows.Forms.PictureBox pbxSale;
        private System.Windows.Forms.Label lblSale;
        private System.Windows.Forms.PictureBox pbxCustomer;
        private System.Windows.Forms.Label lblCustmoer;
        private System.Windows.Forms.PictureBox pbxFiscalId;
        private System.Windows.Forms.Label lblFiscalId;
        private System.Windows.Forms.PictureBox pbxRegisterId;
        private System.Windows.Forms.Label lblRegisterId;
        private System.Windows.Forms.PictureBox pbxDateTime;
        private System.Windows.Forms.Label lblDateTime;
    }
}