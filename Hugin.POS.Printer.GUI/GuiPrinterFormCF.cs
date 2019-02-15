using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Hugin.POS.Printer
{
    public partial class GuiPrinterForm : Form
    {
        List<String> lines = null;
        int lastLine = 0;

        const int linesToShow = 20;

        public GuiPrinterForm()
        {
            lines = new List<string>();
            InitializeComponent();
            lblTicket.Text = "";
        }
        private delegate void RefreshDelegate();

        private void Refresh()
        {
            if (lblTicket.InvokeRequired)
                lblTicket.Invoke(new RefreshDelegate(this.Refresh));
            else
            {
                try
                {
                    for (int i = lastLine; i < lines.Count; i++)
                        lblTicket.Text += lines[i] + "\n";
                    lblTicket.Height = lines.Count * 16;
                    this.AutoScrollPosition = new Point(0, lblTicket.Height);
                }
                catch
                {
                }
            }
        }
        public void AddLine(String lineToPrint)
        {
            lastLine = lines.Count;
            lines.Add(lineToPrint);
            Refresh();
        }
        public void AddLines(IEnumerable<String> linesToPrint)
        {
            lastLine = lines.Count;
            lines.AddRange(linesToPrint);
            Refresh();
        }
    }
}