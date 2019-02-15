using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using cr = Hugin.POS.CashRegister;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using Hugin.POS.States;

namespace Hugin.POS
{
    public class MainForm : Form
    {

        private IContainer components;

        private NotifyIcon niForm;
        private ContextMenuStrip contextMenuGUI;
        private ToolStripMenuItem menuItemExit;

        public int QRPrefix
        { //Prefix character for QR Barcodes  
            get { return 83; } // "S"
        }

        public bool Terminate = false;

        public MainForm()
            : base()
        {
            InitializeComponent();
            Chassis.FatalErrorOccured += new FatalEventHandler(CloseOnFatalError);
            /*hide form*/
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            /*end hide form*/
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.niForm = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuGUI = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuGUI.SuspendLayout();
            this.SuspendLayout();
            // 
            // niForm
            // 
            this.niForm.ContextMenuStrip = this.contextMenuGUI;
            this.niForm.Icon = ((System.Drawing.Icon)(resources.GetObject("niForm.Icon")));
            this.niForm.Text = "Hugin POS";
            this.niForm.Visible = true;
            this.niForm.DoubleClick += new System.EventHandler(this.niForm_DoubleClick);
            // 
            // contextMenuGUI
            // 
            this.contextMenuGUI.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemExit});
            this.contextMenuGUI.Name = "contextMenuStrip1";
            this.contextMenuGUI.Size = new System.Drawing.Size(100, 26);
            // 
            // menuItemExit
            // 
            this.menuItemExit.Name = "menuItemExit";
            this.menuItemExit.Size = new System.Drawing.Size(99, 22);
            this.menuItemExit.Text = "Çýkýþ";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(600, 122);
            this.Name = "MainForm";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.contextMenuGUI.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void CloseOnFatalError(String DisplayMessage)
        {

            DisplayAdapter.Cashier.Show(DisplayMessage);

            MessageBox.Show("HATA OLUÞTU, PROGRAM KAPATILACAK \n" + DisplayMessage);
            this.Dispose();
        }

        protected virtual void PreProcess() { }
        public virtual void Process(PosKey key) { }
        protected virtual void PostProcess() { }
        internal virtual void SendErrorMessage(int errorCode) { }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (niForm != null && niForm.Visible)
                niForm.Visible = false;
           // Chassis.Engine.Terminate = true;
            Chassis.CloseApplication();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }

        void niForm_DoubleClick(object sender, EventArgs e)
        {
            //Show();
            //WindowState = FormWindowState.Normal;
        }

        void menuItemExit_Click(object sender, EventArgs e)
        {
            Chassis.CloseApplication();
        }

        bool readBarcodeMode = false;
        string receipBarcode = "";

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (readBarcodeMode)
            { 
                ReadReceiptBarcode(e.KeyValue); 
                return; 
            }
            if (((e.KeyValue == 16 && !(cr.State is States.EnterQRCode)) || (e.KeyValue == QRPrefix)))
            {
                readBarcodeMode = true;
            }
            else
                Chassis.Engine.Process((PosKey)e.KeyValue);

        }
        private void ReadReceiptBarcode(int p)
        {
            if (p == 13)
            {
                readBarcodeMode = false;
                //cmReceipt.Visible = true;
                if (cr.Document.IsEmpty)
                    cr.State = BarcodeMenu.Instance(receipBarcode);
                receipBarcode = "";
                return;
            }

            if ((char)p != (char)QRPrefix)
            receipBarcode += (char)p;
        }
    }
}
