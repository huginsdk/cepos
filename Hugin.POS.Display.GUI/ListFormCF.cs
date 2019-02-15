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
    public partial class ListForm : Form
    {
        private double opacity = 1;

        public double Opacity
        {
            get { return opacity; }
        }

        public ListForm()
        {
            InitializeComponent();
        }

        private void HideForm()
        {
            opacity = 0;
        }

        internal void SetList(IMenuList menuList)
        {
            opacity = 1;
        }
    }
}