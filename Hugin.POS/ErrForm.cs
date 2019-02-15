using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Hugin.POS
{
    class ErrForm: Form
    {
        private const int LCD_LENGTH = 20;

        private Button btnClose;
        private System.Windows.Forms.Label lblLCD1;
        private System.Windows.Forms.Label lblLCD2;
        private Button btnOK;

        static ErrForm errForm = null;

        internal static ErrForm Instance()
        {
            if (errForm == null)
                errForm = new ErrForm();

            return errForm;
        }

        public ErrForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.TopMost = true;

        }

        public void Show(String errMsg)
        {
            this.lblLCD1.Text = "";
            this.lblLCD2.Text = "".PadLeft(LCD_LENGTH);

            String[] strMsg = errMsg.Split(new char[] { '\n' });

            this.lblLCD1.Text = FormatMsg(strMsg[0]);

            if (strMsg.Length > 1)
            {
                this.lblLCD2.Text = FormatMsg(strMsg[1]);

            }

            btnOK.Visible = false;
            this.ShowDialog();
        }

        public DialogResult ConfirmError(String errMsg)
        {
            this.lblLCD1.Text = "";
            this.lblLCD2.Text = "".PadLeft(LCD_LENGTH);

            String[] strMsg = errMsg.Split(new char[] { '\n' });

            this.lblLCD1.Text = FormatMsg(strMsg[0]);

            if (strMsg.Length > 1)
            {
                this.lblLCD2.Text = FormatMsg(strMsg[1]);

            }

            btnOK.Visible = true;
            return this.ShowDialog();
        }
        private string FormatMsg(string msg)
        {
            if(msg.Length > LCD_LENGTH)
            {
                msg = msg.Substring(0, LCD_LENGTH);
            }

            if(msg.Contains("\t"))
            {
                int diff = LCD_LENGTH - msg.Length + 1;
                String strIn = "".PadLeft(diff);
                msg = msg.Replace("\t", strIn);
                if (msg.Length > LCD_LENGTH)
                {
                    msg = msg.Substring(0, LCD_LENGTH);
                }
            }

            if(msg.Length < LCD_LENGTH)
            {
                int left = (LCD_LENGTH - msg.Length) / 2;
                String strLeft = "".PadLeft(left);
                msg = strLeft + msg;
                msg = msg.PadRight(LCD_LENGTH);
            }

            return msg;
        }

        private void InitializeComponent()
        {
            this.btnClose = new System.Windows.Forms.Button();
            this.lblLCD1 = new System.Windows.Forms.Label();
            this.lblLCD2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnClose.Location = new System.Drawing.Point(167, 69);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(92, 40);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "ÇIKIŞ";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblLCD1
            // 
            this.lblLCD1.AutoSize = true;
            this.lblLCD1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLCD1.Font = new System.Drawing.Font("Courier New", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblLCD1.ForeColor = System.Drawing.Color.Coral;
            this.lblLCD1.Location = new System.Drawing.Point(0, 0);
            this.lblLCD1.Name = "lblLCD1";
            this.lblLCD1.Size = new System.Drawing.Size(377, 36);
            this.lblLCD1.TabIndex = 1;
            this.lblLCD1.Text = "12345678901234567890";
            // 
            // lblLCD2
            // 
            this.lblLCD2.AutoSize = true;
            this.lblLCD2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLCD2.Font = new System.Drawing.Font("Courier New", 18F, System.Drawing.FontStyle.Bold);
            this.lblLCD2.ForeColor = System.Drawing.Color.Coral;
            this.lblLCD2.Location = new System.Drawing.Point(0, 30);
            this.lblLCD2.Name = "lblLCD2";
            this.lblLCD2.Size = new System.Drawing.Size(377, 36);
            this.lblLCD2.TabIndex = 2;
            this.lblLCD2.Text = "12345678901234567890";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnOK.Location = new System.Drawing.Point(36, 70);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(107, 40);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "TAMAM";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Visible = false;
            // 
            // ErrForm
            // 
            this.ClientSize = new System.Drawing.Size(296, 110);
            this.ControlBox = false;
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblLCD2);
            this.Controls.Add(this.lblLCD1);
            this.Controls.Add(this.btnClose);
            this.Name = "ErrForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
