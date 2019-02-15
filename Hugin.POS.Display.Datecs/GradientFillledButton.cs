using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace GradientButton
{
    class GradientFilledButton : Button
    {
        public Color EndColor;

        public Color StartColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }
        protected override void OnBackColorChanged(EventArgs e)
        {
            //base.OnBackColorChanged(e);
        }
         
        bool pressed = false;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Rectangle bounds = new Rectangle(Point.Empty, this.Size);

            //Gradient backcolor
            using (Brush b = new LinearGradientBrush(bounds,EndColor , base.BackColor,
            LinearGradientMode.Vertical))
            {
                g.FillRectangle(b, bounds);
            }

            //Drawstring
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            g.DrawString(base.Text, base.Font, new SolidBrush(base.ForeColor), this.DisplayRectangle, sf);

            //Drawrectangle
            g.DrawRectangle(new Pen(new SolidBrush(Color.White)), this.DisplayRectangle);

            pressed = true;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            pressed = false;
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (pressed)
            {
                base.OnPaint(e);
                return;
            }

            //Get bounds
            Graphics g = e.Graphics;
            Rectangle bounds = new Rectangle(Point.Empty, this.Size);
            
            //Gradient backcolor
            using (Brush b = new LinearGradientBrush(bounds, base.BackColor, EndColor,
            LinearGradientMode.Vertical))
            {
                g.FillRectangle(b, bounds);
            }

            //Drawstring
            StringFormat sf=new StringFormat();
            sf.Alignment= StringAlignment.Center;
            sf.LineAlignment= StringAlignment.Center;

            g.DrawString(base.Text, base.Font, new SolidBrush(base.ForeColor), e.ClipRectangle, sf);

            //Drawrectangle
            g.DrawRectangle(new Pen(new SolidBrush(Color.White)), e.ClipRectangle);
        }
    }
}
