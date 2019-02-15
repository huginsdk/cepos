namespace Hugin.POS.Display.Datecs
{
    partial class MessageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageForm));
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelButtons = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCANCEL = new System.Windows.Forms.Button();
            this.tableLayoutPanelLines = new System.Windows.Forms.TableLayoutPanel();
            this.labelSecondLine = new System.Windows.Forms.Label();
            this.labelFirstLine = new System.Windows.Forms.Label();
            this.tableLayoutPanelMain.SuspendLayout();
            this.tableLayoutPanelButtons.SuspendLayout();
            this.tableLayoutPanelLines.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 3;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelButtons, 1, 2);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelLines, 1, 1);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 3;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(534, 331);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // tableLayoutPanelButtons
            // 
            this.tableLayoutPanelButtons.ColumnCount = 3;
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanelButtons.Controls.Add(this.buttonOK, 0, 1);
            this.tableLayoutPanelButtons.Controls.Add(this.buttonCANCEL, 2, 1);
            this.tableLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelButtons.Location = new System.Drawing.Point(56, 201);
            this.tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
            this.tableLayoutPanelButtons.RowCount = 3;
            this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanelButtons.Size = new System.Drawing.Size(421, 127);
            this.tableLayoutPanelButtons.TabIndex = 1;
            // 
            // buttonOK
            // 
            this.buttonOK.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOK.Image = ((System.Drawing.Image)(resources.GetObject("buttonOK.Image")));
            this.buttonOK.Location = new System.Drawing.Point(3, 9);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(183, 108);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.UseVisualStyleBackColor = false;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCANCEL
            // 
            this.buttonCANCEL.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonCANCEL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCANCEL.Image = ((System.Drawing.Image)(resources.GetObject("buttonCANCEL.Image")));
            this.buttonCANCEL.Location = new System.Drawing.Point(234, 9);
            this.buttonCANCEL.Name = "buttonCANCEL";
            this.buttonCANCEL.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.buttonCANCEL.Size = new System.Drawing.Size(184, 108);
            this.buttonCANCEL.TabIndex = 1;
            this.buttonCANCEL.UseVisualStyleBackColor = false;
            this.buttonCANCEL.Click += new System.EventHandler(this.buttonCANCEL_Click);
            // 
            // tableLayoutPanelLines
            // 
            this.tableLayoutPanelLines.ColumnCount = 1;
            this.tableLayoutPanelLines.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelLines.Controls.Add(this.labelSecondLine, 0, 2);
            this.tableLayoutPanelLines.Controls.Add(this.labelFirstLine, 0, 0);
            this.tableLayoutPanelLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelLines.Location = new System.Drawing.Point(56, 19);
            this.tableLayoutPanelLines.Name = "tableLayoutPanelLines";
            this.tableLayoutPanelLines.RowCount = 4;
            this.tableLayoutPanelLines.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanelLines.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanelLines.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanelLines.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanelLines.Size = new System.Drawing.Size(421, 176);
            this.tableLayoutPanelLines.TabIndex = 2;
            // 
            // labelSecondLine
            // 
            this.labelSecondLine.AutoSize = true;
            this.labelSecondLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSecondLine.Font = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSecondLine.Location = new System.Drawing.Point(3, 87);
            this.labelSecondLine.Name = "labelSecondLine";
            this.labelSecondLine.Size = new System.Drawing.Size(415, 79);
            this.labelSecondLine.TabIndex = 4;
            this.labelSecondLine.Text = "Lütfen can güvenliğiniz için emniyet kemerlerinizin bağlı konumda olduğundan emin" +
    " olunuz. İyi uçuşlar dileriz.";
            this.labelSecondLine.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelFirstLine
            // 
            this.labelFirstLine.AutoSize = true;
            this.labelFirstLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelFirstLine.Font = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFirstLine.Location = new System.Drawing.Point(3, 0);
            this.labelFirstLine.Name = "labelFirstLine";
            this.labelFirstLine.Size = new System.Drawing.Size(415, 79);
            this.labelFirstLine.TabIndex = 3;
            this.labelFirstLine.Text = "Lütfen can güvenliğiniz için emniyet kemerlerinizin bağlı konumda olduğundan emin" +
    " olunuz. İyi uçuşlar dileriz.";
            this.labelFirstLine.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MessageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSkyBlue;
            this.ClientSize = new System.Drawing.Size(534, 331);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Name = "MessageForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelButtons.ResumeLayout(false);
            this.tableLayoutPanelLines.ResumeLayout(false);
            this.tableLayoutPanelLines.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtons;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCANCEL;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelLines;
        private System.Windows.Forms.Label labelSecondLine;
        private System.Windows.Forms.Label labelFirstLine;
    }
}