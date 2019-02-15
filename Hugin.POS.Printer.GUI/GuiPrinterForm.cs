using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Hugin.POS.Printer.GUI
{
    public partial class GuiPrinterForm : Form
    {
        List<String> lines = null;
        int lastLine = 0;

        public GuiPrinterForm()
        {
            lines = new List<string>();
            InitializeComponent();
            lblTicket.Text = "";
            this.ShowInTaskbar = false;
        }
        private delegate void RefreshDelegate();

        private void Refresh()
        {
            if (lblTicket.InvokeRequired)
                lblTicket.Invoke(new RefreshDelegate(this.Refresh));
            else
            {
                Application.OpenForms["GuiPrinterForm"].BringToFront();
                for (int i = lastLine; i < lines.Count; i++)
                    lblTicket.Text += lines[i] + "\n";

                this.VerticalScroll.Value = this.VerticalScroll.Maximum;
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