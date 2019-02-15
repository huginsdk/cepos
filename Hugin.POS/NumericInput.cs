using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Hugin.POS
{
    class NumericInput: Form
    {
        private MyButton myBtn7;
        private MyButton myBtn8;
        private MyButton myBtn9;
        private MyButton myBtn6;
        private MyButton myBtn5;
        private MyButton myBtn4;
        private MyButton myBtn3;
        private MyButton myBtn2;
        private MyButton myBtn1;
        private MyButton myBtn0;
        private MyButton myBtnDel;
        private MyButton myBtnClose;

        private NumericUpDown ownNU = null;
        bool first = true;
    
        public NumericInput(NumericUpDown control)
        {
            ownNU = control;
           // ownNU.Value = ownNU.Minimum;
            if(ownNU.Value != ownNU.Minimum)
            {
                first = false;
            }

            InitializeComponent();
            this.TopMost = true;
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NumericInput));
            this.myBtnDel = new Hugin.POS.MyButton();
            this.myBtnClose = new Hugin.POS.MyButton();
            this.myBtn0 = new Hugin.POS.MyButton();
            this.myBtn3 = new Hugin.POS.MyButton();
            this.myBtn2 = new Hugin.POS.MyButton();
            this.myBtn1 = new Hugin.POS.MyButton();
            this.myBtn6 = new Hugin.POS.MyButton();
            this.myBtn5 = new Hugin.POS.MyButton();
            this.myBtn4 = new Hugin.POS.MyButton();
            this.myBtn9 = new Hugin.POS.MyButton();
            this.myBtn8 = new Hugin.POS.MyButton();
            this.myBtn7 = new Hugin.POS.MyButton();
            this.SuspendLayout();
            // 
            // myBtnDel
            // 
            this.myBtnDel.BackColor = System.Drawing.Color.Transparent;
            this.myBtnDel.DisableColor = System.Drawing.Color.Black;
            this.myBtnDel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtnDel.Location = new System.Drawing.Point(63, 133);
            this.myBtnDel.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtnDel.MForeColor = System.Drawing.Color.Black;
            this.myBtnDel.MHeight = 32;
            this.myBtnDel.MText = "";
            this.myBtnDel.MWidth = 48;
            this.myBtnDel.Name = "myBtnDel";
            this.myBtnDel.Size = new System.Drawing.Size(48, 32);
            this.myBtnDel.TabIndex = 11;
            this.myBtnDel.Tag = "";
            this.myBtnDel.Click += new System.EventHandler(this.myBtnDel_Click);
            // 
            // myBtnClose
            // 
            this.myBtnClose.BackColor = System.Drawing.Color.Transparent;
            this.myBtnClose.DisableColor = System.Drawing.Color.Black;
            this.myBtnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtnClose.Location = new System.Drawing.Point(121, 133);
            this.myBtnClose.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtnClose.MForeColor = System.Drawing.Color.Black;
            this.myBtnClose.MHeight = 32;
            this.myBtnClose.MText = "";
            this.myBtnClose.MWidth = 48;
            this.myBtnClose.Name = "myBtnClose";
            this.myBtnClose.Size = new System.Drawing.Size(48, 32);
            this.myBtnClose.TabIndex = 10;
            this.myBtnClose.Tag = "";
            this.myBtnClose.Click += new System.EventHandler(this.myBtnClose_Click);
            // 
            // myBtn0
            // 
            this.myBtn0.BackColor = System.Drawing.Color.Transparent;
            this.myBtn0.DisableColor = System.Drawing.Color.Black;
            this.myBtn0.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn0.Location = new System.Drawing.Point(5, 133);
            this.myBtn0.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn0.MForeColor = System.Drawing.Color.Black;
            this.myBtn0.MHeight = 32;
            this.myBtn0.MText = "";
            this.myBtn0.MWidth = 48;
            this.myBtn0.Name = "myBtn0";
            this.myBtn0.Size = new System.Drawing.Size(48, 32);
            this.myBtn0.TabIndex = 9;
            this.myBtn0.Tag = "0";
            this.myBtn0.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn3
            // 
            this.myBtn3.BackColor = System.Drawing.Color.Transparent;
            this.myBtn3.DisableColor = System.Drawing.Color.Black;
            this.myBtn3.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn3.Location = new System.Drawing.Point(121, 90);
            this.myBtn3.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn3.MForeColor = System.Drawing.Color.Black;
            this.myBtn3.MHeight = 32;
            this.myBtn3.MText = "";
            this.myBtn3.MWidth = 48;
            this.myBtn3.Name = "myBtn3";
            this.myBtn3.Size = new System.Drawing.Size(48, 32);
            this.myBtn3.TabIndex = 8;
            this.myBtn3.Tag = "3";
            this.myBtn3.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn2
            // 
            this.myBtn2.BackColor = System.Drawing.Color.Transparent;
            this.myBtn2.DisableColor = System.Drawing.Color.Black;
            this.myBtn2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn2.Location = new System.Drawing.Point(63, 90);
            this.myBtn2.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn2.MForeColor = System.Drawing.Color.Black;
            this.myBtn2.MHeight = 32;
            this.myBtn2.MText = "";
            this.myBtn2.MWidth = 48;
            this.myBtn2.Name = "myBtn2";
            this.myBtn2.Size = new System.Drawing.Size(48, 32);
            this.myBtn2.TabIndex = 8;
            this.myBtn2.Tag = "2";
            this.myBtn2.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn1
            // 
            this.myBtn1.BackColor = System.Drawing.Color.Transparent;
            this.myBtn1.DisableColor = System.Drawing.Color.Black;
            this.myBtn1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn1.Location = new System.Drawing.Point(5, 90);
            this.myBtn1.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn1.MForeColor = System.Drawing.Color.Black;
            this.myBtn1.MHeight = 32;
            this.myBtn1.MText = "";
            this.myBtn1.MWidth = 48;
            this.myBtn1.Name = "myBtn1";
            this.myBtn1.Size = new System.Drawing.Size(48, 32);
            this.myBtn1.TabIndex = 6;
            this.myBtn1.Tag = "1";
            this.myBtn1.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn6
            // 
            this.myBtn6.BackColor = System.Drawing.Color.Transparent;
            this.myBtn6.DisableColor = System.Drawing.Color.Black;
            this.myBtn6.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn6.Location = new System.Drawing.Point(121, 47);
            this.myBtn6.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn6.MForeColor = System.Drawing.Color.Black;
            this.myBtn6.MHeight = 32;
            this.myBtn6.MText = "";
            this.myBtn6.MWidth = 48;
            this.myBtn6.Name = "myBtn6";
            this.myBtn6.Size = new System.Drawing.Size(48, 32);
            this.myBtn6.TabIndex = 5;
            this.myBtn6.Tag = "6";
            this.myBtn6.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn5
            // 
            this.myBtn5.BackColor = System.Drawing.Color.Transparent;
            this.myBtn5.DisableColor = System.Drawing.Color.Black;
            this.myBtn5.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn5.Location = new System.Drawing.Point(63, 47);
            this.myBtn5.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn5.MForeColor = System.Drawing.Color.Black;
            this.myBtn5.MHeight = 32;
            this.myBtn5.MText = "";
            this.myBtn5.MWidth = 48;
            this.myBtn5.Name = "myBtn5";
            this.myBtn5.Size = new System.Drawing.Size(48, 32);
            this.myBtn5.TabIndex = 4;
            this.myBtn5.Tag = "5";
            this.myBtn5.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn4
            // 
            this.myBtn4.BackColor = System.Drawing.Color.Transparent;
            this.myBtn4.DisableColor = System.Drawing.Color.Black;
            this.myBtn4.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn4.Location = new System.Drawing.Point(5, 47);
            this.myBtn4.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn4.MForeColor = System.Drawing.Color.Black;
            this.myBtn4.MHeight = 32;
            this.myBtn4.MText = "";
            this.myBtn4.MWidth = 48;
            this.myBtn4.Name = "myBtn4";
            this.myBtn4.Size = new System.Drawing.Size(48, 32);
            this.myBtn4.TabIndex = 3;
            this.myBtn4.Tag = "4";
            this.myBtn4.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn9
            // 
            this.myBtn9.BackColor = System.Drawing.Color.Transparent;
            this.myBtn9.DisableColor = System.Drawing.Color.Black;
            this.myBtn9.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn9.Location = new System.Drawing.Point(121, 4);
            this.myBtn9.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn9.MForeColor = System.Drawing.Color.Black;
            this.myBtn9.MHeight = 32;
            this.myBtn9.MText = "";
            this.myBtn9.MWidth = 48;
            this.myBtn9.Name = "myBtn9";
            this.myBtn9.Size = new System.Drawing.Size(48, 32);
            this.myBtn9.TabIndex = 2;
            this.myBtn9.Tag = "9";
            this.myBtn9.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn8
            // 
            this.myBtn8.BackColor = System.Drawing.Color.Transparent;
            this.myBtn8.DisableColor = System.Drawing.Color.Black;
            this.myBtn8.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn8.Location = new System.Drawing.Point(63, 4);
            this.myBtn8.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn8.MForeColor = System.Drawing.Color.Black;
            this.myBtn8.MHeight = 32;
            this.myBtn8.MText = "";
            this.myBtn8.MWidth = 48;
            this.myBtn8.Name = "myBtn8";
            this.myBtn8.Size = new System.Drawing.Size(48, 32);
            this.myBtn8.TabIndex = 1;
            this.myBtn8.Tag = "8";
            this.myBtn8.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // myBtn7
            // 
            this.myBtn7.BackColor = System.Drawing.Color.Transparent;
            this.myBtn7.DisableColor = System.Drawing.Color.Black;
            this.myBtn7.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.myBtn7.Location = new System.Drawing.Point(5, 4);
            this.myBtn7.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.myBtn7.MForeColor = System.Drawing.Color.Black;
            this.myBtn7.MHeight = 32;
            this.myBtn7.MText = "";
            this.myBtn7.MWidth = 48;
            this.myBtn7.Name = "myBtn7";
            this.myBtn7.Size = new System.Drawing.Size(48, 32);
            this.myBtn7.TabIndex = 0;
            this.myBtn7.Tag = "7";
            this.myBtn7.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // NumericInput
            // 
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(172, 171);
            this.ControlBox = false;
            this.Controls.Add(this.myBtnDel);
            this.Controls.Add(this.myBtnClose);
            this.Controls.Add(this.myBtn0);
            this.Controls.Add(this.myBtn3);
            this.Controls.Add(this.myBtn2);
            this.Controls.Add(this.myBtn1);
            this.Controls.Add(this.myBtn6);
            this.Controls.Add(this.myBtn5);
            this.Controls.Add(this.myBtn4);
            this.Controls.Add(this.myBtn9);
            this.Controls.Add(this.myBtn8);
            this.Controls.Add(this.myBtn7);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "NumericInput";
            this.ResumeLayout(false);

        }

        private void btnNum_Click(object sender, EventArgs e)
        {
            MyButton btnNum = (MyButton)sender;
            int val = Convert.ToInt32(btnNum.Tag);

            if(!first)
            {
                val = (int)ownNU.Value * 10 + val;
            }
            else
            {
                first = false;
            }
            if(val > ownNU.Maximum)
            {
                val = (int)ownNU.Maximum;
            }
            if(val < ownNU.Minimum)
            {
                val = (int)ownNU.Minimum;
            }
            ownNU.Value = val;
        }

        private void myBtnDel_Click(object sender, EventArgs e)
        {
            int val = (int)ownNU.Value;
            val = val / 10;
            if(val == 0)
            {
                first = true;
            }
            if(val < ownNU.Minimum)
            {
                val = (int)ownNU.Minimum;
            }
            ownNU.Value = val;
        }

        private void myBtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
