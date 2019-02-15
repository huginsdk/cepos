namespace Hugin.POS.Display
{
    partial class CustomerForm
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
            this.pnlCustomerMessage = new System.Windows.Forms.Panel();
            this.lblSecondLine = new CaretLabel();
            this.lblFirstLine = new CaretLabel();
            this.pnlReceipt = new System.Windows.Forms.Panel();
            this.dgvReceipt = new Hugin.POS.Display.GridLineDataGridView();
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
            this.gfbUpArrow = new GradientButton.GradientFilledButton();
            this.gfbDownArrow = new GradientButton.GradientFilledButton();
            this.pbxOnlineLed = new System.Windows.Forms.PictureBox();
            this.lblOnline = new System.Windows.Forms.Label();
            this.pbxSale = new System.Windows.Forms.PictureBox();
            this.lblSale = new System.Windows.Forms.Label();
            this.pbxCustomer = new System.Windows.Forms.PictureBox();
            this.lblCustmoer = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.pbxOnlineLed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxSale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCustomer)).BeginInit();
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
            this.pnlCustomerMessage.Size = new System.Drawing.Size(540, 111);
            this.pnlCustomerMessage.TabIndex = 0;
            // 
            // lblSecondLine
            // 
            this.lblSecondLine.CaretIndex = 1;
            this.lblSecondLine.Font = new System.Drawing.Font("Courier New", 26.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.lblSecondLine.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblSecondLine.IsCaretEnable = true;
            this.lblSecondLine.Location = new System.Drawing.Point(-3, 51);
            this.lblSecondLine.MaximumLength = 100;
            this.lblSecondLine.Name = "lblSecondLine";
            this.lblSecondLine.Size = new System.Drawing.Size(1204, 40);
            this.lblSecondLine.TabIndex = 1;
            this.lblSecondLine.Text = "12345678901234567890";
            // 
            // lblFirstLine
            // 
            this.lblFirstLine.CaretIndex = 0;
            this.lblFirstLine.Font = new System.Drawing.Font("Courier New", 26.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.lblFirstLine.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblFirstLine.IsCaretEnable = false;
            this.lblFirstLine.Location = new System.Drawing.Point(-3, 4);
            this.lblFirstLine.MaximumLength = 100;
            this.lblFirstLine.Name = "lblFirstLine";
            this.lblFirstLine.Size = new System.Drawing.Size(549, 40);
            this.lblFirstLine.TabIndex = 0;
            this.lblFirstLine.Text = "12345678901234567890";
            // 
            // pnlReceipt
            // 
            this.pnlReceipt.BackColor = System.Drawing.Color.White;
            this.pnlReceipt.Controls.Add(this.dgvReceipt);
            this.pnlReceipt.Location = new System.Drawing.Point(5, 108);
            this.pnlReceipt.Name = "pnlReceipt";
            this.pnlReceipt.Size = new System.Drawing.Size(540, 585);
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
            this.clmnName,
            this.clmnQuantity,
            this.clmnUnitPrice,
            this.clmnTotalAmount});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.375F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
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
            this.dgvReceipt.RowTemplate.Height = 45;
            this.dgvReceipt.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dgvReceipt.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvReceipt.ShowEditingIcon = false;
            this.dgvReceipt.ShowRowErrors = false;
            this.dgvReceipt.Size = new System.Drawing.Size(360, 300);
            this.dgvReceipt.TabIndex = 0;
            // 
            // clmnName
            // 
            this.clmnName.FillWeight = 253F;
            this.clmnName.HeaderText = "Ürün Adı";
            this.clmnName.Name = "clmnName";
            this.clmnName.ReadOnly = true;
            this.clmnName.Width = 253;
            // 
            // clmnQuantity
            // 
            this.clmnQuantity.FillWeight = 75F;
            this.clmnQuantity.HeaderText = "Adet";
            this.clmnQuantity.Name = "clmnQuantity";
            this.clmnQuantity.ReadOnly = true;
            this.clmnQuantity.Width = 75;
            // 
            // clmnUnitPrice
            // 
            this.clmnUnitPrice.FillWeight = 75F;
            this.clmnUnitPrice.HeaderText = "Birim Fiyatı";
            this.clmnUnitPrice.Name = "clmnUnitPrice";
            this.clmnUnitPrice.ReadOnly = true;
            this.clmnUnitPrice.Width = 75;
            // 
            // clmnTotalAmount
            // 
            this.clmnTotalAmount.FillWeight = 135F;
            this.clmnTotalAmount.HeaderText = "Tutar";
            this.clmnTotalAmount.Name = "clmnTotalAmount";
            this.clmnTotalAmount.ReadOnly = true;
            this.clmnTotalAmount.Width = 135;
            // 
            // pnlKeypad
            // 
            this.pnlKeypad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(64)))), ((int)(((byte)(97)))));
            this.pnlKeypad.Controls.Add(this.tblKeypad);
            this.pnlKeypad.Location = new System.Drawing.Point(551, 5);
            this.pnlKeypad.Margin = new System.Windows.Forms.Padding(0);
            this.pnlKeypad.Name = "pnlKeypad";
            this.pnlKeypad.Size = new System.Drawing.Size(730, 787);
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
            this.tblKeypad.Size = new System.Drawing.Size(487, 525);
            this.tblKeypad.TabIndex = 0;
            // 
            // pnlTotals
            // 
            this.pnlTotals.BackColor = System.Drawing.Color.White;
            this.pnlTotals.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlTotals.Controls.Add(this.tblTotals);
            this.pnlTotals.Location = new System.Drawing.Point(5, 705);
            this.pnlTotals.Name = "pnlTotals";
            this.pnlTotals.Size = new System.Drawing.Size(540, 90);
            this.pnlTotals.TabIndex = 2;
            // 
            // tblTotals
            // 
            this.tblTotals.ColumnCount = 3;
            this.tblTotals.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tblTotals.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tblTotals.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
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
            this.tblTotals.Size = new System.Drawing.Size(358, 58);
            this.tblTotals.TabIndex = 0;
            // 
            // lblTotalAmount
            // 
            this.lblTotalAmount.AutoSize = true;
            this.lblTotalAmount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalAmount.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTotalAmount.Location = new System.Drawing.Point(217, 29);
            this.lblTotalAmount.Name = "lblTotalAmount";
            this.lblTotalAmount.Size = new System.Drawing.Size(138, 29);
            this.lblTotalAmount.TabIndex = 3;
            this.lblTotalAmount.Text = "0,00";
            this.lblTotalAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAdjustment
            // 
            this.lblAdjustment.AutoSize = true;
            this.lblAdjustment.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAdjustment.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblAdjustment.Location = new System.Drawing.Point(74, 10);
            this.lblAdjustment.Name = "lblAdjustment";
            this.lblAdjustment.Size = new System.Drawing.Size(137, 19);
            this.lblAdjustment.TabIndex = 0;
            this.lblAdjustment.Text = "İNDİRİM";
            this.lblAdjustment.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotal.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTotal.Location = new System.Drawing.Point(74, 29);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(137, 29);
            this.lblTotal.TabIndex = 2;
            this.lblTotal.Text = "TOPLAM";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAdjustmentAmount
            // 
            this.lblAdjustmentAmount.AutoSize = true;
            this.lblAdjustmentAmount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblAdjustmentAmount.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblAdjustmentAmount.Location = new System.Drawing.Point(217, 10);
            this.lblAdjustmentAmount.Name = "lblAdjustmentAmount";
            this.lblAdjustmentAmount.Size = new System.Drawing.Size(138, 19);
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
            this.gfbUpArrow.Size = new System.Drawing.Size(48, 23);
            this.gfbUpArrow.StartColor = System.Drawing.Color.DimGray;
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
            this.gfbDownArrow.Location = new System.Drawing.Point(3, 32);
            this.gfbDownArrow.Name = "gfbDownArrow";
            this.gfbDownArrow.Size = new System.Drawing.Size(48, 23);
            this.gfbDownArrow.StartColor = System.Drawing.Color.DimGray;
            this.gfbDownArrow.TabIndex = 5;
            this.gfbDownArrow.Text = "↓";
            this.gfbDownArrow.UseVisualStyleBackColor = false;
            this.gfbDownArrow.Visible = false;
            this.gfbDownArrow.Click += new System.EventHandler(this.gfbDownArrow_Click);
            // 
            // pbxOnlineLed
            // 
            this.pbxOnlineLed.BackColor = System.Drawing.Color.Transparent;
            this.pbxOnlineLed.Image = global::Hugin.POS.Display.Properties.Resources.ledoff;
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
            this.lblOnline.Size = new System.Drawing.Size(100, 41);
            this.lblOnline.TabIndex = 1;
            this.lblOnline.Text = "ONLINE";
            this.lblOnline.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbxSale
            // 
            this.pbxSale.BackColor = System.Drawing.Color.Transparent;
            this.pbxSale.Image = global::Hugin.POS.Display.Properties.Resources.ledoff;
            this.pbxSale.Location = new System.Drawing.Point(153, 3);
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
            this.lblSale.Location = new System.Drawing.Point(197, 0);
            this.lblSale.Name = "lblSale";
            this.lblSale.Size = new System.Drawing.Size(100, 41);
            this.lblSale.TabIndex = 3;
            this.lblSale.Text = "SATIŞ";
            this.lblSale.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbxCustomer
            // 
            this.pbxCustomer.BackColor = System.Drawing.Color.Transparent;
            this.pbxCustomer.Image = global::Hugin.POS.Display.Properties.Resources.nocustomer;
            this.pbxCustomer.Location = new System.Drawing.Point(303, 3);
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
            this.lblCustmoer.Location = new System.Drawing.Point(347, 0);
            this.lblCustmoer.Name = "lblCustmoer";
            this.lblCustmoer.Size = new System.Drawing.Size(247, 41);
            this.lblCustmoer.TabIndex = 5;
            this.lblCustmoer.Text = "Müşteri Yok";
            this.lblCustmoer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbxRegisterId
            // 
            this.pbxRegisterId.BackColor = System.Drawing.Color.Transparent;
            this.pbxRegisterId.Image = global::Hugin.POS.Display.Properties.Resources.register;
            this.pbxRegisterId.Location = new System.Drawing.Point(600, 3);
            this.pbxRegisterId.Name = "pbxRegisterId";
            this.pbxRegisterId.Size = new System.Drawing.Size(38, 38);
            this.pbxRegisterId.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxRegisterId.TabIndex = 6;
            this.pbxRegisterId.TabStop = false;
            // 
            // lblRegisterId
            // 
            this.lblRegisterId.BackColor = System.Drawing.Color.Transparent;
            this.lblRegisterId.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblRegisterId.ForeColor = System.Drawing.Color.White;
            this.lblRegisterId.Location = new System.Drawing.Point(644, 0);
            this.lblRegisterId.Name = "lblRegisterId";
            this.lblRegisterId.Size = new System.Drawing.Size(85, 41);
            this.lblRegisterId.TabIndex = 7;
            this.lblRegisterId.Text = "001";
            this.lblRegisterId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pbxDateTime
            // 
            this.pbxDateTime.BackColor = System.Drawing.Color.Transparent;
            this.pbxDateTime.Image = global::Hugin.POS.Display.Properties.Resources.clock;
            this.pbxDateTime.Location = new System.Drawing.Point(735, 3);
            this.pbxDateTime.Name = "pbxDateTime";
            this.pbxDateTime.Size = new System.Drawing.Size(38, 38);
            this.pbxDateTime.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxDateTime.TabIndex = 8;
            this.pbxDateTime.TabStop = false;
            // 
            // lblDateTime
            // 
            this.lblDateTime.BackColor = System.Drawing.Color.Transparent;
            this.lblDateTime.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblDateTime.ForeColor = System.Drawing.Color.White;
            this.lblDateTime.Location = new System.Drawing.Point(3, 44);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(240, 41);
            this.lblDateTime.TabIndex = 9;
            this.lblDateTime.Text = "1 Mart 2013 11.00";
            this.lblDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CustomerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(129)))), ((int)(((byte)(189)))));
            this.ClientSize = new System.Drawing.Size(1294, 801);
            this.Controls.Add(this.pnlTotals);
            this.Controls.Add(this.pnlKeypad);
            this.Controls.Add(this.pnlReceipt);
            this.Controls.Add(this.pnlCustomerMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CustomerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CustomerForm";
            this.pnlCustomerMessage.ResumeLayout(false);
            this.pnlReceipt.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvReceipt)).EndInit();
            this.pnlKeypad.ResumeLayout(false);
            this.pnlTotals.ResumeLayout(false);
            this.tblTotals.ResumeLayout(false);
            this.tblTotals.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxOnlineLed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxSale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCustomer)).EndInit();
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
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnUnitPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnTotalAmount;
        private System.Windows.Forms.TableLayoutPanel tblTotals;
        private System.Windows.Forms.Label lblAdjustmentAmount;
        private System.Windows.Forms.Label lblTotalAmount;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblAdjustment;
        private GradientButton.GradientFilledButton gfbUpArrow;
        private GradientButton.GradientFilledButton gfbDownArrow;
        private System.Windows.Forms.PictureBox pbxOnlineLed;
        private System.Windows.Forms.Label lblOnline;
        private System.Windows.Forms.PictureBox pbxSale;
        private System.Windows.Forms.Label lblSale;
        private System.Windows.Forms.PictureBox pbxCustomer;
        private System.Windows.Forms.Label lblCustmoer;
        private System.Windows.Forms.PictureBox pbxRegisterId;
        private System.Windows.Forms.Label lblRegisterId;
        private System.Windows.Forms.PictureBox pbxDateTime;
        private System.Windows.Forms.Label lblDateTime;
    }
}