using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace Hugin.POS.Printer
{
    public partial class GuiPrinterForm
    {
        List<String> lines = null;
        int lastLine = 0;

        public GuiPrinterForm()
        {
			lines = new List<string>();
        }
		public void Show ()
		{
		}
        private void Refresh()
        {
            
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