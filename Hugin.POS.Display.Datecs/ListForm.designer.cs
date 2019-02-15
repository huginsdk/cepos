namespace Hugin.POS.Display.Datecs
{
    partial class ListForm
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
            this.txtLine1 = new System.Windows.Forms.TextBox();
            this.pbxNext = new System.Windows.Forms.PictureBox();
            this.pbxPrevious = new System.Windows.Forms.PictureBox();
            this.pbxCurrent = new System.Windows.Forms.PictureBox();
            this.txtLine2 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbxNext)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPrevious)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCurrent)).BeginInit();
            this.SuspendLayout();
            // 
            // txtLine1
            // 
            this.txtLine1.BackColor = System.Drawing.Color.Black;
            this.txtLine1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLine1.Enabled = false;
            this.txtLine1.Font = new System.Drawing.Font("Lucida Console", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtLine1.ForeColor = System.Drawing.Color.White;
            this.txtLine1.Location = new System.Drawing.Point(89, 216);
            this.txtLine1.Multiline = true;
            this.txtLine1.Name = "txtLine1";
            this.txtLine1.Size = new System.Drawing.Size(314, 26);
            this.txtLine1.TabIndex = 1;
            this.txtLine1.Text = "12345678901234567890";
            // 
            // pbxNext
            // 
            this.pbxNext.Location = new System.Drawing.Point(359, 12);
            this.pbxNext.Name = "pbxNext";
            this.pbxNext.Size = new System.Drawing.Size(90, 90);
            this.pbxNext.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxNext.TabIndex = 5;
            this.pbxNext.TabStop = false;
            // 
            // pbxPrevious
            // 
            this.pbxPrevious.Location = new System.Drawing.Point(11, 12);
            this.pbxPrevious.Name = "pbxPrevious";
            this.pbxPrevious.Size = new System.Drawing.Size(90, 90);
            this.pbxPrevious.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxPrevious.TabIndex = 4;
            this.pbxPrevious.TabStop = false;
            // 
            // pbxCurrent
            // 
            this.pbxCurrent.Location = new System.Drawing.Point(133, 12);
            this.pbxCurrent.Name = "pbxCurrent";
            this.pbxCurrent.Size = new System.Drawing.Size(195, 195);
            this.pbxCurrent.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbxCurrent.TabIndex = 3;
            this.pbxCurrent.TabStop = false;
            // 
            // txtLine2
            // 
            this.txtLine2.BackColor = System.Drawing.Color.Black;
            this.txtLine2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLine2.Enabled = false;
            this.txtLine2.Font = new System.Drawing.Font("Lucida Console", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtLine2.ForeColor = System.Drawing.Color.White;
            this.txtLine2.Location = new System.Drawing.Point(89, 244);
            this.txtLine2.Multiline = true;
            this.txtLine2.Name = "txtLine2";
            this.txtLine2.Size = new System.Drawing.Size(314, 26);
            this.txtLine2.TabIndex = 6;
            this.txtLine2.Text = "12345678901234567890";
            // 
            // ListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(462, 270);
            this.Controls.Add(this.txtLine2);
            this.Controls.Add(this.pbxNext);
            this.Controls.Add(this.pbxPrevious);
            this.Controls.Add(this.pbxCurrent);
            this.Controls.Add(this.txtLine1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ListForm";
            this.Opacity = 0.8;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "List";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pbxNext)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPrevious)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxCurrent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLine1;
        private System.Windows.Forms.PictureBox pbxNext;
        private System.Windows.Forms.PictureBox pbxPrevious;
        private System.Windows.Forms.PictureBox pbxCurrent;
        private System.Windows.Forms.TextBox txtLine2;
    }
}