using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;

namespace Hugin.POS.Display.GUI
{
    public partial class GuiDisplayForm : Form
    {
        KeypadForm kpForm = null;
        public event ConsumeKeyHandler ConsumeKey;

        public GuiDisplayForm()
        {
            InitializeComponent(); 
            kpForm = new KeypadForm();
            kpForm.Show();
            kpForm.Hide();
            kpForm.ConsumeKey += new ConsumeKeyHandler(kpForm_ConsumeKey);
        }

        void kpForm_ConsumeKey(object sender, ConsumeKeyEventArgs e)
        {
            if (ConsumeKey != null)
                ConsumeKey(sender, e);
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
                    rtbSecondLine.Clear();
                    rtbSecondLine.AppendText(message.Substring(0, currentColumn));
                    rtbSecondLine.SelectionFont = new Font(rtbSecondLine.Font.FontFamily, rtbSecondLine.Font.Size, FontStyle.Underline);
                    rtbSecondLine.AppendText(message.Substring(currentColumn, 1));
                    rtbSecondLine.SelectionFont = new Font(rtbSecondLine.Font.FontFamily, rtbSecondLine.Font.Size, FontStyle.Regular);
                    rtbSecondLine.AppendText(message.Substring(currentColumn + 1));

                }
                else
                    rtbSecondLine.Text = message;
            }
        }
        private string AdjustMessageLine(string line)
        {
            if (line.Contains("\t"))
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

        private void cbxKeypad_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbxKeypad.Checked)
                {
                    kpForm.Top = this.Top + this.Height + 5;
                    kpForm.Left = this.Left - ((kpForm.Width - this.Width) / 2);
                    kpForm.Show();
                    this.TopMost = true;
                }
                else
                {
                    kpForm.Hide();
                    this.TopMost = false;
                }
            }
            catch { }
        }
    }
}