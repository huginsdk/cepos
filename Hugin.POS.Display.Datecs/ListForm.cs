using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Hugin.POS.Common;
using System.Collections;

namespace Hugin.POS.Display.Datecs
{
    public partial class ListForm : Form
    {
        protected static IDoubleEnumerator ide;
        private static IMenuList lastList = null;
        private static int menuItemCount = 0;

        public ListForm()
        {
            InitializeComponent();
        }

        public void SetList(IMenuList menuList)
        {

            if (menuList == null)
            {
                this.Hide();
                lastList = null;
                ClearImages();
                return;
            }

            ide = menuList as IDoubleEnumerator;
            menuItemCount = ((System.Collections.ArrayList)menuList).Count;

            for (int i = 0; i < menuItemCount; i++)
            {
                if (((ArrayList)menuList)[i] == ide.Current)
                {
                    Image image = null;
                    if (menuItemCount > 1)
                    {
                        image = GetProductImage(((ArrayList)menuList)[PreviousIndex(i)]);
                        SetPreviousImage(image);
                    }
                    if (menuItemCount > 2)
                    {
                        image = GetProductImage(((ArrayList)menuList)[NextIndex(i)]);
                        SetNextImage(image);
                    }
                    image = GetProductImage(((ArrayList)menuList)[i]);
                    SetCurrentImage(image);

                    if (ide.Current is IProduct)
                        Show((IProduct)ide.Current);
                    else
                        Show((IFiscalItem)ide.Current);
                }
            }
            if (lastList != menuList)
            {
                this.Show();
            }
            lastList = menuList;
        }

        public void Show(String message)
        {
            String[] messages = Str.Split(message, '\n');
            SetTxtCurrentText(txtLine1, AdjustMessageLine(messages[0]));
            if (messages.Length == 2)
                SetTxtCurrentText(txtLine2, AdjustMessageLine(messages[1]));
        }

        private string AdjustMessageLine(string line)
        {
            if (line.Contains("\t"))
                line = line.Replace("\t", " ".PadRight(20 - line.Length + 1));
            else
                line = line.PadLeft(line.Length + ((20 - line.Length) / 2), ' ');
            return String.Format("{0,-20}", line);
        }

        public void Show(String format, params object[] args)
        {
            Show(String.Format(format, args));
        }

        public void Show(IProduct p)
        {
            bool large = ((p.Quantity != (long)p.Quantity) && p.UnitPrice * p.Quantity > 1000000);
            Show("{0}\n{1:G10} {2}\t{3:C}", p.Name, new Number(p.Quantity), large ? "X" : p.Unit, new Number(p.UnitPrice * p.Quantity));
        }

        public void Show(IFiscalItem fi)
        {
            bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
            Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)), large ? "X" : fi.Unit, new Number(fi.TotalAmount - fi.VoidAmount));
        }

        private int NextIndex(int index)
        {
            if (index == menuItemCount - 1)
                index = -1;
            return ++index;

        }

        private int PreviousIndex(int index)
        {
            if (index == 0)
                index = menuItemCount;
            return --index;
        }

        private Image GetProductImage(Object obj)
        {
            string barcode = "";
            string name = "";
            string amount = "";

            if (obj is IProduct)
            {
                barcode = ((IProduct)obj).Barcode;
                name = ((IProduct)obj).Name;
                amount = String.Format("{0:C}", new Number(((IProduct)obj).UnitPrice * ((IProduct)obj).Quantity));
            }
            else
            {
                barcode = ((IFiscalItem)obj).Product.Barcode;
                name = ((IFiscalItem)obj).Product.Name;
                amount = String.Format("{0:C}", new Number(((IFiscalItem)obj).TotalAmount - ((IFiscalItem)obj).VoidAmount));
            }
            string imagePath = PosConfiguration.ImagePath + barcode + ".jpg";
            Bitmap b = new Bitmap(135, 135);

            if (System.IO.File.Exists(imagePath))
                b = new Bitmap(imagePath);
            else if (System.IO.File.Exists(PosConfiguration.ImagePath + "NoImage.jpg"))
                b = new Bitmap(PosConfiguration.ImagePath + "NoImage.jpg");
            else
            {
                Graphics g = Graphics.FromImage(b);
                name = name.Trim();
                SizeF nameSize;
                int fittedFontSize = 16;
                while (true)
                {
                    nameSize = g.MeasureString(name, new Font("Verdana", fittedFontSize, FontStyle.Bold));
                    if (nameSize.Width > b.Size.Width)
                        fittedFontSize--;
                    else
                        break;
                }
                g.DrawString(name, new Font("Verdana", fittedFontSize, FontStyle.Bold), new SolidBrush(Color.White), 0, 50);

                float top = nameSize.Height + 50;

                fittedFontSize = 18;
                while (true)
                {
                    nameSize = g.MeasureString(amount, new Font("Verdana", fittedFontSize, FontStyle.Bold));
                    if (nameSize.Width > b.Size.Width)
                        fittedFontSize--;
                    else
                        break;
                }
                g.DrawString(amount, new Font("Verdana", fittedFontSize, FontStyle.Bold), new SolidBrush(Color.Red), 0, top);

                g.Dispose();
            }

            return b;
        }

        private void ClearImages()
        {
            SetPreviousImage(null);
            SetNextImage(null);
            SetCurrentImage(null);
        }

        #region Display Value Settings
        private delegate void RefreshImageDelegate(PictureBox pbx);
        private void RefreshImage(PictureBox pbx)
        {
            if (pbx.InvokeRequired)
                pbx.Invoke(new RefreshImageDelegate(this.RefreshImage), pbx);
            else
                pbx.Refresh();
        }


        private delegate void SetNextImageDelegate(Image image);
        private void SetNextImage(Image image)
        {
            if (pbxNext.InvokeRequired)
                pbxNext.Invoke(new SetPreviousImageDelegate(this.SetNextImage), image);
            else
                pbxNext.Image = image;

        }

        private delegate void SetPreviousImageDelegate(Image image);
        private void SetPreviousImage(Image image)
        {
            if (pbxPrevious.InvokeRequired)
                pbxPrevious.Invoke(new SetPreviousImageDelegate(this.SetPreviousImage), image);
            else
                pbxPrevious.Image = image;
        }

        private delegate void SetCurrentImageDelegate(Image img);
        private void SetCurrentImage(Image img)
        {
            if (pbxCurrent.InvokeRequired)
                pbxCurrent.Invoke(new SetCurrentImageDelegate(this.SetCurrentImage), img);
            else
                pbxCurrent.Image = img;
        }

        private delegate void SetTxtCurrentTextDelegate(TextBox textBox, string str);
        private void SetTxtCurrentText(TextBox textBox, string str)
        {
            if (textBox.InvokeRequired)
                textBox.Invoke(new SetTxtCurrentTextDelegate(this.SetTxtCurrentText), textBox, str);
            else
                textBox.Text = str;
        }

        private delegate void SetFormVisibleDelegate(bool visible);
        internal static void SetFormVisible(bool visible)
        {
            if (Application.OpenForms["ListForm"] == null)
                return;
            if (Application.OpenForms["ListForm"].InvokeRequired)
                Application.OpenForms["ListForm"].Invoke(new SetFormVisibleDelegate(SetFormVisible), visible);
            else
                Application.OpenForms["ListForm"].Visible = visible;
        }
        #endregion

    }
}