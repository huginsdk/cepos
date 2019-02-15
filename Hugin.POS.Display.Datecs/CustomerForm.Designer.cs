namespace Hugin.POS.Display.Datecs
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgSales = new System.Windows.Forms.DataGridView();
            this.clmnOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmnProductName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmnQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmnPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmnAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlCurrent = new System.Windows.Forms.Panel();
            this.tableLayoutPanelMessages = new System.Windows.Forms.TableLayoutPanel();
            this.lblFirstMessage = new System.Windows.Forms.Label();
            this.lblSecondMessage = new System.Windows.Forms.Label();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.pbxProduct = new System.Windows.Forms.PictureBox();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelMiddle = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dgSales)).BeginInit();
            this.pnlCurrent.SuspendLayout();
            this.tableLayoutPanelMessages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxProduct)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            this.tableLayoutPanelMiddle.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgSales
            // 
            this.dgSales.AllowUserToAddRows = false;
            this.dgSales.AllowUserToDeleteRows = false;
            this.dgSales.AllowUserToResizeColumns = false;
            this.dgSales.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgSales.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgSales.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgSales.BackgroundColor = System.Drawing.Color.White;
            this.dgSales.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.SlateGray;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Lucida Console", 14.25F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgSales.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgSales.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgSales.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clmnOrder,
            this.clmnProductName,
            this.clmnQuantity,
            this.clmnPrice,
            this.clmnAmount});
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Arial Rounded MT Bold", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgSales.DefaultCellStyle = dataGridViewCellStyle7;
            this.dgSales.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgSales.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgSales.Enabled = false;
            this.dgSales.GridColor = System.Drawing.SystemColors.Highlight;
            this.dgSales.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.dgSales.Location = new System.Drawing.Point(3, 3);
            this.dgSales.MultiSelect = false;
            this.dgSales.Name = "dgSales";
            this.dgSales.ReadOnly = true;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgSales.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            this.dgSales.RowHeadersVisible = false;
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Lucida Console", 30F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle9.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.Black;
            this.dgSales.RowsDefaultCellStyle = dataGridViewCellStyle9;
            this.dgSales.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgSales.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgSales.ShowCellToolTips = false;
            this.dgSales.Size = new System.Drawing.Size(392, 389);
            this.dgSales.TabIndex = 2;
            // 
            // clmnOrder
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.clmnOrder.DefaultCellStyle = dataGridViewCellStyle3;
            this.clmnOrder.FillWeight = 51.44144F;
            this.clmnOrder.HeaderText = "No";
            this.clmnOrder.Name = "clmnOrder";
            this.clmnOrder.ReadOnly = true;
            // 
            // clmnProductName
            // 
            this.clmnProductName.FillWeight = 180.2725F;
            this.clmnProductName.HeaderText = "Ürün Adý";
            this.clmnProductName.Name = "clmnProductName";
            this.clmnProductName.ReadOnly = true;
            // 
            // clmnQuantity
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.clmnQuantity.DefaultCellStyle = dataGridViewCellStyle4;
            this.clmnQuantity.FillWeight = 74.63321F;
            this.clmnQuantity.HeaderText = "Adet";
            this.clmnQuantity.Name = "clmnQuantity";
            this.clmnQuantity.ReadOnly = true;
            // 
            // clmnPrice
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.clmnPrice.DefaultCellStyle = dataGridViewCellStyle5;
            this.clmnPrice.FillWeight = 91.9473F;
            this.clmnPrice.HeaderText = "Fiyat";
            this.clmnPrice.Name = "clmnPrice";
            this.clmnPrice.ReadOnly = true;
            // 
            // clmnAmount
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.clmnAmount.DefaultCellStyle = dataGridViewCellStyle6;
            this.clmnAmount.FillWeight = 111.6531F;
            this.clmnAmount.HeaderText = "Tutar";
            this.clmnAmount.Name = "clmnAmount";
            this.clmnAmount.ReadOnly = true;
            // 
            // pnlCurrent
            // 
            this.pnlCurrent.BackColor = System.Drawing.Color.Black;
            this.pnlCurrent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlCurrent.Controls.Add(this.tableLayoutPanelMessages);
            this.pnlCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCurrent.Location = new System.Drawing.Point(0, 0);
            this.pnlCurrent.Name = "pnlCurrent";
            this.pnlCurrent.Size = new System.Drawing.Size(796, 92);
            this.pnlCurrent.TabIndex = 5;
            // 
            // tableLayoutPanelMessages
            // 
            this.tableLayoutPanelMessages.ColumnCount = 1;
            this.tableLayoutPanelMessages.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMessages.Controls.Add(this.lblFirstMessage, 0, 0);
            this.tableLayoutPanelMessages.Controls.Add(this.lblSecondMessage, 0, 1);
            this.tableLayoutPanelMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMessages.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMessages.Name = "tableLayoutPanelMessages";
            this.tableLayoutPanelMessages.RowCount = 2;
            this.tableLayoutPanelMessages.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMessages.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMessages.Size = new System.Drawing.Size(792, 88);
            this.tableLayoutPanelMessages.TabIndex = 0;
            // 
            // lblFirstMessage
            // 
            this.lblFirstMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblFirstMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFirstMessage.Font = new System.Drawing.Font("Lucida Console", 32F);
            this.lblFirstMessage.ForeColor = System.Drawing.Color.White;
            this.lblFirstMessage.Location = new System.Drawing.Point(3, 0);
            this.lblFirstMessage.Name = "lblFirstMessage";
            this.lblFirstMessage.Size = new System.Drawing.Size(786, 44);
            this.lblFirstMessage.TabIndex = 2;
            this.lblFirstMessage.Text = "MAÐAZAMIZA HOÞ GELDÝNÝZ";
            this.lblFirstMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSecondMessage
            // 
            this.lblSecondMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblSecondMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSecondMessage.Font = new System.Drawing.Font("Lucida Console", 32F);
            this.lblSecondMessage.ForeColor = System.Drawing.Color.White;
            this.lblSecondMessage.Location = new System.Drawing.Point(3, 44);
            this.lblSecondMessage.Name = "lblSecondMessage";
            this.lblSecondMessage.Size = new System.Drawing.Size(786, 44);
            this.lblSecondMessage.TabIndex = 4;
            this.lblSecondMessage.Text = "LÜTFEN BEKLEYÝNÝZ";
            this.lblSecondMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlDetail
            // 
            this.pnlDetail.BackColor = System.Drawing.SystemColors.Control;
            this.pnlDetail.Location = new System.Drawing.Point(3, 502);
            this.pnlDetail.Name = "pnlDetail";
            this.pnlDetail.Size = new System.Drawing.Size(794, 95);
            this.pnlDetail.TabIndex = 6;
            // 
            // pbxProduct
            // 
            this.pbxProduct.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxProduct.Location = new System.Drawing.Point(401, 3);
            this.pbxProduct.Name = "pbxProduct";
            this.pbxProduct.Size = new System.Drawing.Size(392, 389);
            this.pbxProduct.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxProduct.TabIndex = 0;
            this.pbxProduct.TabStop = false;
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.pnlCurrent);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(3, 3);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(796, 92);
            this.pnlHeader.TabIndex = 7;
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelMiddle, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.pnlDetail, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.pnlHeader, 0, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 3;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.74865F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80.25134F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(802, 600);
            this.tableLayoutPanelMain.TabIndex = 8;
            // 
            // tableLayoutPanelMiddle
            // 
            this.tableLayoutPanelMiddle.ColumnCount = 2;
            this.tableLayoutPanelMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMiddle.Controls.Add(this.pbxProduct, 1, 0);
            this.tableLayoutPanelMiddle.Controls.Add(this.dgSales, 0, 0);
            this.tableLayoutPanelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMiddle.Location = new System.Drawing.Point(3, 101);
            this.tableLayoutPanelMiddle.Name = "tableLayoutPanelMiddle";
            this.tableLayoutPanelMiddle.RowCount = 1;
            this.tableLayoutPanelMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMiddle.Size = new System.Drawing.Size(796, 395);
            this.tableLayoutPanelMiddle.TabIndex = 9;
            // 
            // CustomerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(802, 600);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CustomerForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CustomerForm";
            this.Load += new System.EventHandler(this.CustomerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgSales)).EndInit();
            this.pnlCurrent.ResumeLayout(false);
            this.tableLayoutPanelMessages.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxProduct)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMiddle.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Label lblSecondMessage;
        protected System.Windows.Forms.Label lblFirstMessage;
        protected System.Windows.Forms.PictureBox pbxProduct;
        protected System.Windows.Forms.Panel pnlCurrent;
        protected System.Windows.Forms.Panel pnlDetail;
        protected System.Windows.Forms.Panel pnlHeader;

        protected System.Windows.Forms.DataGridView dgSales;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMessages;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMiddle;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnOrder;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnProductName;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmnAmount;
    }
}
