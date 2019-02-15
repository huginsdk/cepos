using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;

namespace Hugin.POS.Display
{
    class MenuForm : Form
    {
        private double opacity = 1;

        public double Opacity
        {
            get { return opacity; }
        }


        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlMenuItems;

        int height = 60;
        int width = 600;
        int maxHeight = 360;

        IMenuList lastList = null;
        public MenuForm()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlMenuItems = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.lblTitle.Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Regular);
            this.lblTitle.ForeColor = System.Drawing.Color.Black;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(600, 60);
            this.lblTitle.Text = "BAÞLIK";
            // 
            // pnlMenuItems
            // 
            this.pnlMenuItems.AutoScroll = true;
            this.pnlMenuItems.BackColor = System.Drawing.Color.Silver;
            this.pnlMenuItems.Location = new System.Drawing.Point(0, 60);
            this.pnlMenuItems.Name = "pnlMenuItems";
            this.pnlMenuItems.Size = new System.Drawing.Size(600, 60);
            // 
            // MenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(600, 120);
            this.ControlBox = false;
            this.Controls.Add(this.pnlMenuItems);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(100, 100);
            this.Name = "MenuForm";
            this.Text = "MenuFormCF";
            this.ResumeLayout(false);

        }

        private void HideForm()
        {
            //this.Visible = false;
            this.Hide();
            lastList = null;
            lblTitle.Text = "";
            opacity = 0;
            this.pnlMenuItems.Controls.Clear();
        }

        public void ShowList(IMenuList menuList)
        {
            if (menuList == null)
            {
                this.HideForm();
                return;
            }
            this.BringToFront(); 
            IDoubleEnumerator ide = menuList as IDoubleEnumerator;
            int menuCount = ((System.Collections.ArrayList)menuList).Count;
            if (lastList == menuList)
            {
                int indx = 0;
                foreach (object ml in menuList)
                {
                    if (ml == ide.Current)
                    {
                        pnlMenuItems.Controls[indx].BackColor = Color.Gray;
                        this.pnlMenuItems.AutoScrollPosition = new Point(0, indx * height / 2);
                    }
                    else
                        pnlMenuItems.Controls[indx].BackColor = Color.LightGray;

                }
                return;
            }
            lastList = menuList;
            opacity = 1;
           
            this.pnlMenuItems.Controls.Clear();

            string title = ide.Current.ToString();
            if (title.IndexOf("\n") >= 0)
                title = title.Substring(0, title.IndexOf("\n")).Trim();

            if (title.IndexOf("\t") >= 0)
                title = title.Substring(0, title.IndexOf("\t")).Trim();

            lblTitle.Text = title;

            int yPosition = 0;

            this.pnlMenuItems.Size = new Size(width, Math.Min(menuCount * height, maxHeight));
            this.Size = new Size(width, pnlMenuItems.Height + lblTitle.Height);
            int i = 0;
            foreach (object ml in menuList)
            {
                System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
                lbl.Tag = i;
                lbl.TextAlign = ContentAlignment.TopCenter;

                if (ml == ide.Current)
                {
                    lbl.BackColor = Color.Gray;
                    yPosition = i * height;
                }
                else
                    lbl.BackColor = Color.LightGray;
                
                lbl.Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Regular);
                lbl.Size = new Size(width, height);
                string strItem = ml.ToString();

                lbl.Text = strItem;
                if (strItem.IndexOf("\n") >= 0)
                    lbl.Text = strItem.Substring(strItem.IndexOf("\n")).Trim();
                this.pnlMenuItems.Controls.Add(lbl);
                lbl.Location = new Point(0, i * height);
            }

            this.pnlMenuItems.AutoScrollPosition = new Point(0, yPosition / 2);
            this.Location = new Point(20, 20);
        }

        void btn_Click(object sender, EventArgs e)
        {
            this.HideForm();
        }

    }
}