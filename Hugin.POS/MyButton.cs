using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Hugin.POS
{
    class MyButton: UserControl
    {
        Color disabledClr = Color.LightGray;
        Color bckClr = Color.LightGray;
        Color foreClr = Color.Black;
        Font fnt = new Font("Microsoft Sans Serif", 8.25f);
        String txt = "My Button";
                
        public String MText
        {
            get { return txt; }
            set
            {
                txt = value;
                this.Refresh();
            }
        }
        
        public Font MFont
        {
            get { return fnt; }
            set
            {
                fnt = value;
                this.Refresh();
            }
        }
        
        public Color MForeColor
        {
            get { return foreClr; }
            set
            {
                foreClr = value;
                this.Refresh();
            }
        }

        public int MWidth
        {
            get { return this.Width; }
            set
            {
                this.Width = value;
                this.Refresh();
            }
        }
        public int MHeight
        {
            get { return this.Height; }
            set
            {
                this.Height = value;
                this.Refresh();
            }
        }

        public float MFontWidth
        {
            get
            {
                if(Font.Bold == true)
                {
                    return Font.Size;
                }
                return (float)Math.Round((double)Font.Size * 0.9, 2);
            }
        }
        
        public Color DisableColor
        {
            get { return disabledClr; }
            set
            {
                disabledClr = value;
                this.Refresh();
            }
        }
        public MyButton():base()
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

        }
        protected String [] GetLines(String strText)
        {
            String[] words = strText.Split(new char[] { ' ' });
            int i = 0;

            for (i = 0; i < words.Length - 1; i++)
            {
                words[i] += " ";
            }

            List<String> lWords = new List<String>(words);

            int charInLine = (int)(this.Width / MFontWidth);

            i = 0;
            while(true)
            {
                if(i >= lWords.Count)
                {
                    break;
                }
                if (lWords[i].Length> charInLine)
                {
                    String wrd2 = lWords[i].Substring(charInLine);
                    lWords[i] = lWords[i].Substring(0, charInLine);
                    lWords.Insert(i + 1, wrd2);
                }
                i++;
            }

            int numOfLine = 0;
            int charCovered = 0;
            List<String> lines = new List<String>();

            for (i = 0; i < lWords.Count;i++ )
            {
                if(charCovered + lWords[i].Length > charInLine)
                {
                    numOfLine++;
                    charCovered = 0;
                }
                if(charCovered == 0)
                {                    
                    lines.Add("");
                }
                charCovered += lWords[i].Length;
                lines[numOfLine] += lWords[i];
            }

            return lines.ToArray();
        }
        protected override void OnEnabledChanged(EventArgs e)
        {
            if(Enabled == false)
            {
                bckClr = this.BackColor;
                this.BackColor = disabledClr;
            }
            else
            {
                this.BackColor = bckClr;
            }
            base.OnEnabledChanged(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                String[] lines = GetLines(MText);
                int y = this.Height / 2 - lines.Length * (MFont.Height / 2);
                if (y < 0)
                {
                    y = 0;
                }
                for (int i = 0; i < lines.Length; i++)
                {
                    String line = lines[i].TrimEnd();
                    int x = (int)(this.Width / 2 - line.Length * MFontWidth / 2);
                    e.Graphics.DrawString(line, MFont, Brushes.Black, x, y);
                    y += MFont.Height;
                }
            }
            catch { }
            //e.Graphics.DrawString(MText, MFont, Brushes.Black, this.Width / 2, this.Height / 2);
            base.OnPaint(e);
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MyButton
            // 
            this.Name = "MyButton";
            this.ResumeLayout(false);

        }
    }
}
