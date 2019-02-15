using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;

namespace Hugin.POS.Display
{
    internal class GuiDisplayForm:Form
    {
        public event ConsumeKeyHandler ConsumeKey;

        internal GuiDisplayForm()
        {
            InitializeComponent();
        }

        public void ShowImage(Image itemImage)
        {

        }

        public void Show(String message, Target target)
        {
            if (target == Target.Customer) return;

            if (!Str.Contains(message,"\n")) message = message + "\n";
            String[] lines = message.Split('\n');

            SetFirstMessage(AdjustMessageLine(lines[0]));
            SetSecondMessage(AdjustMessageLine(lines[1]));
        }
        private delegate void ShowInputDelegate(String message, int currentColumn, bool cursorOn);
        public void ShowInput(String message, int currentColumn, bool cursorOn)
        {
            if (rtbSecondLine.InvokeRequired)
                rtbSecondLine.Invoke(new ShowInputDelegate(this.ShowInput), message, currentColumn, cursorOn);
            else
            {
                if (cursorOn)
                {
                    if (currentColumn >= message.Length)
                        message = message + " ";
                    rtbSecondLine.Text="";
                    rtbSecondLine.Text += message.Substring(0, currentColumn);
                    rtbSecondLine.Text+=message.Substring(currentColumn, 1);
                    rtbSecondLine.Text+=message.Substring(currentColumn + 1);

                }
                else
                    rtbSecondLine.Text = message;
            }
        }
        private string AdjustMessageLine(string line)
        {
            if (Str.Contains(line,"\t"))
                line = line.Replace("\t", " ".PadRight(20 - line.Length + 1));
            int adj = 20 - line.Length;
            int left = adj / 2;
            int right = adj - left;
            return "".PadLeft(left) + line + "".PadRight(right);
        }
        private delegate void SetFirstMessageDelegate(String message);
        private void SetFirstMessage(String message)
        {
            if (rtbFirstLine.InvokeRequired)
                rtbFirstLine.Invoke(new SetFirstMessageDelegate(this.SetFirstMessage), message);
            else
            {
                rtbFirstLine.Text = message;
                rtbFirstLine.Refresh();
            }
        }
        private delegate void SetSecondMessageDelegate(String message);
        private void SetSecondMessage(String message)
        {
            if (rtbSecondLine.InvokeRequired)
                rtbSecondLine.Invoke(new SetSecondMessageDelegate(this.SetSecondMessage), message);
            else
                rtbSecondLine.Text = message;
        }
        private void InitializeComponent()
        {
            this.rtbSecondLine = new System.Windows.Forms.TextBox();
            this.rtbFirstLine = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // rtbSecondLine
            // 
            this.rtbSecondLine.BackColor = System.Drawing.Color.Teal;
            this.rtbSecondLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbSecondLine.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular);
            this.rtbSecondLine.ForeColor = System.Drawing.Color.White;
            this.rtbSecondLine.Location = new System.Drawing.Point(0, 35);
            this.rtbSecondLine.MaxLength = 20;
            this.rtbSecondLine.Name = "rtbSecondLine";
            this.rtbSecondLine.ReadOnly = true;
            this.rtbSecondLine.Size = new System.Drawing.Size(290, 35);
            this.rtbSecondLine.TabIndex = 1;
            this.rtbSecondLine.Text = "12345678901234567890";
            // 
            // rtbFirstLine
            // 
            this.rtbFirstLine.BackColor = System.Drawing.Color.Teal;
            this.rtbFirstLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbFirstLine.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Regular);
            this.rtbFirstLine.ForeColor = System.Drawing.Color.White;
            this.rtbFirstLine.Location = new System.Drawing.Point(0, 0);
            this.rtbFirstLine.MaxLength = 20;
            this.rtbFirstLine.Name = "rtbFirstLine";
            this.rtbFirstLine.ReadOnly = true;
            this.rtbFirstLine.Size = new System.Drawing.Size(290, 35);
            this.rtbFirstLine.TabIndex = 2;
            this.rtbFirstLine.Text = "12345678901234567890";
            // 
            // GuiDisplayForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Teal;
            this.ClientSize = new System.Drawing.Size(290, 70);
            this.ControlBox = false;
            this.Controls.Add(this.rtbFirstLine);
            this.Controls.Add(this.rtbSecondLine);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(300, 150);
            this.Name = "GuiDisplayForm";
            this.Text = "Kasiyer Göstergesi";
            this.TopMost = true;
            this.ResumeLayout(false);

        }


        private System.Windows.Forms.TextBox rtbSecondLine;
        private System.Windows.Forms.TextBox rtbFirstLine;
    }
}
