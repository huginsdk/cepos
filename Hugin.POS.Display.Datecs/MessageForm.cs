using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;

namespace Hugin.POS.Display.Datecs
{
    public partial class MessageForm : Form
    {
        private const int LINE_LENGTH = 20;

        public MessageForm(string message)
        {
            InitializeComponent();
            this.CenterToScreen();
            this.TopMost = true;

            labelFirstLine.Font = new System.Drawing.Font(Display.PFC.Families[0], 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            labelSecondLine.Font = new System.Drawing.Font(Display.PFC.Families[0], 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));

            ShowMessage(message);
        }

        private void ShowMessage(string message)
        {
            if (!Str.Contains(message, "\n") && message.Length > LINE_LENGTH)
            {
                String[] words = message.Split(' ');

                String tmp = words[0];
                for (int i = 1; i < words.Length; i++)
                {
                    if (tmp.Length + words[i].Length + 1 > LINE_LENGTH)
                    {
                        tmp = tmp + "\n" + words[i];
                    }
                    else
                    {
                        tmp = tmp + " " + words[i];
                    }
                }

                message = tmp;
            }

            if (!Str.Contains(message, "\n")) message = message + "\n";
            String[] lines = message.Split('\n');
            if (lines[0] != "")
                labelFirstLine.Text =  AdjustMessageLine(lines[0]);

            labelSecondLine.Text = AdjustMessageLine(lines[1]);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCANCEL_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private string AdjustMessageLine(string line)
        {
            if (Str.Contains(line, "\t"))
            {
                if (line.IndexOf("\t") == 0)
                    line = line.Replace("\t", " ".PadRight(LINE_LENGTH - line.Length));
                else
                    line = line.Replace("\t", " ".PadRight(LINE_LENGTH - line.Length + 1));
            }
            else
                line = line.PadLeft(line.Length + ((LINE_LENGTH - line.Length) / 2), ' ');

            if (Str.Contains(line, "&"))
                line = line.Replace("&", " ");
            return String.Format("{0,-" + LINE_LENGTH.ToString() + "}", line);
        }
    }
}
