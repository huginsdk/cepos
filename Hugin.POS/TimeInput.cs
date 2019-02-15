using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Hugin.POS
{
    class TimeInput :Form
    {
        private MyButton btn2;
        private MyButton btn3;
        private MyButton btn6;
        private MyButton btn5;
        private MyButton btn4;
        private MyButton btn9;
        private MyButton btn8;
        private MyButton btn7;
        private MyButton btn0;
        private MyButton btn1;
        private MyButton btnClose;
        private MyButton btnBckSpce;

        private TimePicker ownDT = null;
        String input = "";

        public TimeInput(TimePicker control)
        {
            ownDT = control;
            ownDT.Value = new DateTime(2000, 1, 1, 0, 0, 0);

            InitializeComponent();
            this.TopMost = true;

            OnFirstDigit();
        }

        private void OnFirstDigit()
        {
            btn0.Enabled = true;
            btn1.Enabled = true;
            btn2.Enabled = true;

            btn3.Enabled = false;
            btn4.Enabled = false;
            btn5.Enabled = false;
            btn6.Enabled = false;
            btn7.Enabled = false;
            btn8.Enabled = false;
            btn9.Enabled = false;
        }

        private void OnSecondDigit()
        {
            if(input.Length<1)
            {
                OnFirstDigit();
                return;
            }
            btn0.Enabled = true;
            btn1.Enabled = true;
            btn2.Enabled = true;
            btn3.Enabled = true;

            if (input[0] == '2')
            {
                btn4.Enabled = false;
                btn5.Enabled = false;
                btn6.Enabled = false;
                btn7.Enabled = false;
                btn8.Enabled = false;
                btn9.Enabled = false;
            }
            else
            {
                btn4.Enabled = true;
                btn5.Enabled = true;
                btn6.Enabled = true;
                btn7.Enabled = true;
                btn8.Enabled = true;
                btn9.Enabled = true;
            }
        }

        private void OnThirdDigit()
        {
            if (input.Length < 2)
            {
                OnSecondDigit();
                return;
            }
            btn0.Enabled = true;
            btn1.Enabled = true;
            btn2.Enabled = true;
            btn3.Enabled = true;
            btn4.Enabled = true;
            btn5.Enabled = true;

            btn6.Enabled = false;
            btn7.Enabled = false;
            btn8.Enabled = false;
            btn9.Enabled = false;
        }

        private void OnFourthDigit()
        {
            if (input.Length < 3)
            {
                OnThirdDigit();
                return;
            }
            btn0.Enabled = true;
            btn1.Enabled = true;
            btn2.Enabled = true;
            btn3.Enabled = true;

            btn4.Enabled = true;
            btn5.Enabled = true;
            btn6.Enabled = true;
            btn7.Enabled = true;
            btn8.Enabled = true;
            btn9.Enabled = true;
        }

        private void OnComplete()
        {
            if (input.Length < 4)
            {
                OnFourthDigit();
                return;
            }
            btn0.Enabled = false;
            btn1.Enabled = false;
            btn2.Enabled = false;
            btn3.Enabled = false;

            btn4.Enabled = false;
            btn5.Enabled = false;
            btn6.Enabled = false;
            btn7.Enabled = false;
            btn8.Enabled = false;
            btn9.Enabled = false;
        }

        private void UpdateDT()
        {
            String strDT = input.PadRight(4, '0');
            strDT = strDT.Insert(2, ":");
            strDT = "01.01.2000 " + strDT;
            ownDT.Value = Convert.ToDateTime(strDT);

            OnComplete();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimeInput));
            this.btn1 = new Hugin.POS.MyButton();
            this.btn2 = new Hugin.POS.MyButton();
            this.btn3 = new Hugin.POS.MyButton();
            this.btn6 = new Hugin.POS.MyButton();
            this.btn5 = new Hugin.POS.MyButton();
            this.btn4 = new Hugin.POS.MyButton();
            this.btn9 = new Hugin.POS.MyButton();
            this.btn8 = new Hugin.POS.MyButton();
            this.btn7 = new Hugin.POS.MyButton();
            this.btn0 = new Hugin.POS.MyButton();
            this.btnClose = new Hugin.POS.MyButton();
            this.btnBckSpce = new Hugin.POS.MyButton();
            this.SuspendLayout();
            // 
            // btn1
            // 
            this.btn1.BackColor = System.Drawing.Color.Transparent;
            this.btn1.DisableColor = System.Drawing.Color.Black;
            this.btn1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn1.Location = new System.Drawing.Point(5, 90);
            this.btn1.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn1.MForeColor = System.Drawing.Color.Black;
            this.btn1.MHeight = 32;
            this.btn1.MText = "";
            this.btn1.MWidth = 48;
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(48, 32);
            this.btn1.TabIndex = 0;
            this.btn1.Tag = "1";
            this.btn1.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn2
            // 
            this.btn2.BackColor = System.Drawing.Color.Transparent;
            this.btn2.DisableColor = System.Drawing.Color.Black;
            this.btn2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn2.Location = new System.Drawing.Point(63, 90);
            this.btn2.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn2.MForeColor = System.Drawing.Color.Black;
            this.btn2.MHeight = 32;
            this.btn2.MText = "";
            this.btn2.MWidth = 48;
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(48, 32);
            this.btn2.TabIndex = 1;
            this.btn2.Tag = "2";
            this.btn2.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn3
            // 
            this.btn3.BackColor = System.Drawing.Color.Transparent;
            this.btn3.DisableColor = System.Drawing.Color.Black;
            this.btn3.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn3.Location = new System.Drawing.Point(121, 90);
            this.btn3.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn3.MForeColor = System.Drawing.Color.Black;
            this.btn3.MHeight = 32;
            this.btn3.MText = "";
            this.btn3.MWidth = 48;
            this.btn3.Name = "btn3";
            this.btn3.Size = new System.Drawing.Size(48, 32);
            this.btn3.TabIndex = 2;
            this.btn3.Tag = "3";
            this.btn3.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn6
            // 
            this.btn6.BackColor = System.Drawing.Color.Transparent;
            this.btn6.DisableColor = System.Drawing.Color.Black;
            this.btn6.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn6.Location = new System.Drawing.Point(121, 47);
            this.btn6.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn6.MForeColor = System.Drawing.Color.Black;
            this.btn6.MHeight = 32;
            this.btn6.MText = "";
            this.btn6.MWidth = 48;
            this.btn6.Name = "btn6";
            this.btn6.Size = new System.Drawing.Size(48, 32);
            this.btn6.TabIndex = 5;
            this.btn6.Tag = "6";
            this.btn6.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn5
            // 
            this.btn5.BackColor = System.Drawing.Color.Transparent;
            this.btn5.DisableColor = System.Drawing.Color.Black;
            this.btn5.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn5.Location = new System.Drawing.Point(63, 47);
            this.btn5.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn5.MForeColor = System.Drawing.Color.Black;
            this.btn5.MHeight = 32;
            this.btn5.MText = "";
            this.btn5.MWidth = 48;
            this.btn5.Name = "btn5";
            this.btn5.Size = new System.Drawing.Size(48, 32);
            this.btn5.TabIndex = 4;
            this.btn5.Tag = "5";
            this.btn5.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn4
            // 
            this.btn4.BackColor = System.Drawing.Color.Transparent;
            this.btn4.DisableColor = System.Drawing.Color.Black;
            this.btn4.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn4.Location = new System.Drawing.Point(5, 47);
            this.btn4.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn4.MForeColor = System.Drawing.Color.Black;
            this.btn4.MHeight = 32;
            this.btn4.MText = "";
            this.btn4.MWidth = 48;
            this.btn4.Name = "btn4";
            this.btn4.Size = new System.Drawing.Size(48, 32);
            this.btn4.TabIndex = 3;
            this.btn4.Tag = "4";
            this.btn4.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn9
            // 
            this.btn9.BackColor = System.Drawing.Color.Transparent;
            this.btn9.DisableColor = System.Drawing.Color.Black;
            this.btn9.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn9.Location = new System.Drawing.Point(121, 4);
            this.btn9.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn9.MForeColor = System.Drawing.Color.Black;
            this.btn9.MHeight = 32;
            this.btn9.MText = "";
            this.btn9.MWidth = 48;
            this.btn9.Name = "btn9";
            this.btn9.Size = new System.Drawing.Size(48, 32);
            this.btn9.TabIndex = 8;
            this.btn9.Tag = "9";
            this.btn9.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn8
            // 
            this.btn8.BackColor = System.Drawing.Color.Transparent;
            this.btn8.DisableColor = System.Drawing.Color.Black;
            this.btn8.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn8.Location = new System.Drawing.Point(63, 4);
            this.btn8.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn8.MForeColor = System.Drawing.Color.Black;
            this.btn8.MHeight = 32;
            this.btn8.MText = "";
            this.btn8.MWidth = 48;
            this.btn8.Name = "btn8";
            this.btn8.Size = new System.Drawing.Size(48, 32);
            this.btn8.TabIndex = 8;
            this.btn8.Tag = "8";
            this.btn8.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn7
            // 
            this.btn7.BackColor = System.Drawing.Color.Transparent;
            this.btn7.DisableColor = System.Drawing.Color.Black;
            this.btn7.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn7.Location = new System.Drawing.Point(5, 4);
            this.btn7.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn7.MForeColor = System.Drawing.Color.Black;
            this.btn7.MHeight = 32;
            this.btn7.MText = "";
            this.btn7.MWidth = 48;
            this.btn7.Name = "btn7";
            this.btn7.Size = new System.Drawing.Size(48, 32);
            this.btn7.TabIndex = 6;
            this.btn7.Tag = "7";
            this.btn7.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btn0
            // 
            this.btn0.BackColor = System.Drawing.Color.Transparent;
            this.btn0.DisableColor = System.Drawing.Color.Black;
            this.btn0.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btn0.Location = new System.Drawing.Point(5, 133);
            this.btn0.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btn0.MForeColor = System.Drawing.Color.Black;
            this.btn0.MHeight = 32;
            this.btn0.MText = "";
            this.btn0.MWidth = 48;
            this.btn0.Name = "btn0";
            this.btn0.Size = new System.Drawing.Size(48, 32);
            this.btn0.TabIndex = 9;
            this.btn0.Tag = "0";
            this.btn0.Click += new System.EventHandler(this.btnNum_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.DisableColor = System.Drawing.Color.Black;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btnClose.Location = new System.Drawing.Point(121, 133);
            this.btnClose.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnClose.MForeColor = System.Drawing.Color.Black;
            this.btnClose.MHeight = 32;
            this.btnClose.MText = "";
            this.btnClose.MWidth = 48;
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(48, 32);
            this.btnClose.TabIndex = 10;
            this.btnClose.Tag = "";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnBckSpce
            // 
            this.btnBckSpce.BackColor = System.Drawing.Color.Transparent;
            this.btnBckSpce.DisableColor = System.Drawing.Color.Black;
            this.btnBckSpce.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btnBckSpce.Location = new System.Drawing.Point(63, 133);
            this.btnBckSpce.MFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.btnBckSpce.MForeColor = System.Drawing.Color.Black;
            this.btnBckSpce.MHeight = 32;
            this.btnBckSpce.MText = "";
            this.btnBckSpce.MWidth = 48;
            this.btnBckSpce.Name = "btnBckSpce";
            this.btnBckSpce.Size = new System.Drawing.Size(48, 32);
            this.btnBckSpce.TabIndex = 11;
            this.btnBckSpce.Tag = "";
            this.btnBckSpce.Click += new System.EventHandler(this.btnBckSpce_Click);
            // 
            // TimeInput
            // 
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(173, 172);
            this.ControlBox = false;
            this.Controls.Add(this.btnBckSpce);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btn0);
            this.Controls.Add(this.btn9);
            this.Controls.Add(this.btn8);
            this.Controls.Add(this.btn7);
            this.Controls.Add(this.btn6);
            this.Controls.Add(this.btn5);
            this.Controls.Add(this.btn4);
            this.Controls.Add(this.btn3);
            this.Controls.Add(this.btn2);
            this.Controls.Add(this.btn1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TimeInput";
            this.ResumeLayout(false);

        }

        private void btnNum_Click(object sender, EventArgs e)
        {
            MyButton btnNum = (MyButton)sender;
            int val = Convert.ToInt32(btnNum.Tag);

            input = input + val;

            UpdateDT();
        }


        private void btnBckSpce_Click(object sender, EventArgs e)
        {
            if(input.Length>0)
            {
                input = input.Substring(0, input.Length - 1);
            }
            UpdateDT();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
