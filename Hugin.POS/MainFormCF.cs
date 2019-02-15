using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.States;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class MainForm : Form
    {

        private ContextMenu contextMenuStrip1;
        private MenuItem toolStripMenuItem1;

        public bool Terminate = false;

        public MainForm()
            : base()
        {
            CashRegister.DocumentChanged += new EventHandler(CashRegister_DocumentChanged);
            InitializeComponent();
            Chassis.FatalErrorOccured += new FatalEventHandler(CloseOnFatalError);
            this.Location = new Point(-1 * this.Size.Width, -1 * this.Size.Height);//to hide form
        }

        void CashRegister_DocumentChanged(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        private void InitializeComponent()
        {
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenu();
            this.toolStripMenuItem1 = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.MenuItems.Add(this.toolStripMenuItem1);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Text = "Close";
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.Olive;
            this.ClientSize = new System.Drawing.Size(175, 31);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.Text = "Hugin Pos";
            this.ResumeLayout(false);

        }

        void MainForm_Closed(object sender, EventArgs e)
        {
            Chassis.Engine.Terminate = true;
        }

        private void CloseOnFatalError(String DisplayMessage)
        {

            DisplayAdapter.Cashier.Show(DisplayMessage);

            MessageBox.Show("HATA OLUÞTU, PROGRAM KAPATILACAK");
            this.Dispose();
        }

        protected virtual void PreProcess() { }
        public virtual void Process(PosKey key) { }
        protected virtual void PostProcess() { }
        internal virtual void SendErrorMessage(int errorCode) { }

        private void MainForm_Resize(object sender, EventArgs e)
        {
        }

       
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }
        bool readBarcodeMode = false;
        string receipBarcode = "";

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (readBarcodeMode)
            { ReadReceiptBarcode(e.KeyValue); return; }
            if (e.KeyValue == 16)
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

            receipBarcode += (char)p;
        }

        protected override void OnClosed(EventArgs e)
        {
            Chassis.CloseApplication();
            cr.PromoClient.Close();
            base.OnClosed(e);
        }
    }
}