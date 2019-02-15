using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace Hugin.POS
{
    public class TimePicker : TextBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public TimePicker()
        {
            InitializeComponent();
            this.Text = String.Format("{0:HH:mm}", DateTime.Now);
        }

        public TimePicker(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public DateTime Value
        {
            get
            {
                String time = this.Text;
                time = "01.01.2000 " + time;
                return Convert.ToDateTime(time);
            }
            set
            {
                this.Text = String.Format("{0:HH:mm}", value);
                this.Refresh();
            }
        }

        #region Component Designer generated code

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}
