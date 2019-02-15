using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;

namespace Hugin.POS.Display.Datecs
{
    public partial class MenuForm : Form
    {

        int height = 60;
        int width = 600;
        int maxHeight = 360;

        IMenuList lastList = null;
        public MenuForm()
        {
            InitializeComponent();
        }

        private void HideForm()
        {
            lblTitle.Text = "";
            this.pnlMenuItems.Controls.Clear();
            lastList = null;
            this.Opacity = 0;
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
                for (int i = 0; i < menuCount; i++)
                {
                    if (((System.Collections.ArrayList)menuList)[i] == ide.Current)
                    {
                        pnlMenuItems.Controls[i].BackColor = Color.DarkGreen;
                        pnlMenuItems.Controls[i].Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Bold);
                        pnlMenuItems.Controls[i].ForeColor = Color.White;

                        if (pnlMenuItems.Controls[i].Top >= maxHeight)
                        {
                            int value = (i + 1) * height - maxHeight;
                            this.pnlMenuItems.AutoScrollPosition = new Point(0, value);
                        }
                        else if (pnlMenuItems.Controls[i].Top < 0)
                        {
                            int value = i * height;
                            this.pnlMenuItems.AutoScrollPosition = new Point(0, value);
                        }
                    }
                    else
                    {
                        pnlMenuItems.Controls[i].BackColor = Color.LightGray;
                        pnlMenuItems.Controls[i].Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Regular);
                        pnlMenuItems.Controls[i].ForeColor = Color.Black;
                    }
                }
                return;
            }
            lastList = menuList;

            this.Opacity = 0.8;
            this.pnlMenuItems.Controls.Clear();
            this.pnlMenuItems.AutoScroll = true;
            string title = ide.Current.ToString();
            if (title.IndexOf("\n") >= 0)
                title = title.Substring(0, title.IndexOf("\n")).Trim();

            if (title.IndexOf("\t") >= 0)
                title = title.Substring(0, title.IndexOf("\t")).Trim();

            lblTitle.Text = title;

            int yPosition = 0;

            this.pnlMenuItems.Size = new Size(width, Math.Min(menuCount * height, maxHeight));
            this.Size = new Size(width, pnlMenuItems.Height + lblTitle.Height);
            for (int i = 0; i < ((System.Collections.ArrayList)menuList).Count; i++)
            {
                System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
                lbl.Tag = i;
                lbl.TextAlign = ContentAlignment.MiddleLeft;
                lbl.BorderStyle = BorderStyle.Fixed3D;
                if (((System.Collections.ArrayList)menuList)[i] == ide.Current)
                {
                    lbl.BackColor = Color.DarkGreen;
                    lbl.Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Bold);
                    lbl.ForeColor = Color.White;
                    yPosition = i * height;
                }
                else
                {
                    lbl.BackColor = Color.LightGray;
                    lbl.Font = new System.Drawing.Font("Tahoma", 30F, System.Drawing.FontStyle.Regular);
                    lbl.ForeColor = Color.Black;
                }

                lbl.Size = new Size(width, height);
                string strItem = ((System.Collections.ArrayList)menuList)[i].ToString();

                lbl.Text = strItem;
                if (strItem.IndexOf("\n") >= 0)
                    lbl.Text = (i + 1) + "- " + strItem.Substring(strItem.IndexOf("\n")).Trim();
                lbl.Location = new Point(0, i * height);
                this.pnlMenuItems.Controls.Add(lbl);
                lbl.Click += new EventHandler(btn_Click);
            }

            this.pnlMenuItems.AutoScrollPosition = new Point(0, yPosition / 2);
            if (this.pnlMenuItems.VerticalScroll.Visible)
            {
                pnlMenuItems.Width = width + 50;
            }
            else
            {
                pnlMenuItems.Width = width;
            }
            this.Location = new Point(SystemInformation.PrimaryMonitorSize.Width / 2 - this.Width / 2,
                SystemInformation.PrimaryMonitorSize.Height / 2 - this.Height / 2);
        }

        void btn_Click(object sender, EventArgs e)
        {
            this.HideForm();
        }
    }
}