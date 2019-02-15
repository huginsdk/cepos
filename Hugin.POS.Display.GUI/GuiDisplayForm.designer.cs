namespace Hugin.POS.Display.GUI
{
    partial class GuiDisplayForm
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
            this.rtbSecondLine = new System.Windows.Forms.RichTextBox();
            this.rtbFirstLine = new System.Windows.Forms.RichTextBox();
            this.cbxKeypad = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // rtbSecondLine
            // 
            this.rtbSecondLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.rtbSecondLine.Font = new System.Drawing.Font("Lucida Console", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.rtbSecondLine.ForeColor = System.Drawing.Color.White;
            this.rtbSecondLine.Location = new System.Drawing.Point(3, 31);
            this.rtbSecondLine.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.rtbSecondLine.MaxLength = 20;
            this.rtbSecondLine.Multiline = false;
            this.rtbSecondLine.Name = "rtbSecondLine";
            this.rtbSecondLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbSecondLine.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.rtbSecondLine.ReadOnly = true;
            this.rtbSecondLine.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbSecondLine.ShortcutsEnabled = false;
            this.rtbSecondLine.Size = new System.Drawing.Size(288, 34);
            this.rtbSecondLine.TabIndex = 1;
            this.rtbSecondLine.Text = "12345678901234567890";
            // 
            // rtbFirstLine
            // 
            this.rtbFirstLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.rtbFirstLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbFirstLine.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.rtbFirstLine.Font = new System.Drawing.Font("Lucida Console", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.rtbFirstLine.ForeColor = System.Drawing.Color.White;
            this.rtbFirstLine.Location = new System.Drawing.Point(3, 2);
            this.rtbFirstLine.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.rtbFirstLine.MaxLength = 20;
            this.rtbFirstLine.Multiline = false;
            this.rtbFirstLine.Name = "rtbFirstLine";
            this.rtbFirstLine.ReadOnly = true;
            this.rtbFirstLine.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbFirstLine.ShortcutsEnabled = false;
            this.rtbFirstLine.Size = new System.Drawing.Size(288, 31);
            this.rtbFirstLine.TabIndex = 2;
            this.rtbFirstLine.Text = "12345678901234567890";
            // 
            // cbxKeypad
            // 
            this.cbxKeypad.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbxKeypad.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.cbxKeypad.Location = new System.Drawing.Point(0, 55);
            this.cbxKeypad.Name = "cbxKeypad";
            this.cbxKeypad.Size = new System.Drawing.Size(291, 26);
            this.cbxKeypad.TabIndex = 4;
            this.cbxKeypad.Text = "Hugin Klavye";
            this.cbxKeypad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.cbxKeypad.UseVisualStyleBackColor = true;
            this.cbxKeypad.CheckedChanged += new System.EventHandler(this.cbxKeypad_CheckedChanged);
            // 
            // GuiDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 10F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(291, 82);
            this.ControlBox = false;
            this.Controls.Add(this.cbxKeypad);
            this.Controls.Add(this.rtbFirstLine);
            this.Controls.Add(this.rtbSecondLine);
            this.Font = new System.Drawing.Font("MatrixSchedule", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "GuiDisplayForm";
            this.Opacity = 0.5;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Kasiyer Göstergesi";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbSecondLine;
        private System.Windows.Forms.RichTextBox rtbFirstLine;
        private System.Windows.Forms.CheckBox cbxKeypad;

    }
}
