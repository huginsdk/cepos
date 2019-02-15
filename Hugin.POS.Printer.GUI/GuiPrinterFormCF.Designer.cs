namespace Hugin.POS.Printer
{
    partial class GuiPrinterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTicket = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTicket
            // 
            this.lblTicket.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular);
            this.lblTicket.ForeColor = System.Drawing.Color.Black;
            this.lblTicket.Location = new System.Drawing.Point(0, 5);
            this.lblTicket.Name = "lblTicket";
            this.lblTicket.Size = new System.Drawing.Size(315, 16);
            this.lblTicket.Text = "LABEL";
            // 
            // GuiDocument
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(320, 300);
            this.Controls.Add(this.lblTicket);
            this.Location = new System.Drawing.Point(400, 150);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GuiDocument";
            this.Text = "Hugin Yazýcý";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTicket;


    }
}