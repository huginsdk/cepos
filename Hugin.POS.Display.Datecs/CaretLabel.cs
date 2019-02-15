using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

public class CaretLabel : Control
{
    int caretIndex = 0;
    bool isCaretEnable = true;
    int maximumLength = 100;

    public int MaximumLength
    {
        get { return maximumLength; }
        set { maximumLength = value; }
    }
    
    public bool IsCaretEnable
    {
        get { return isCaretEnable; }
        set { isCaretEnable = value; }
    }

    public override string Text
    {
        get
        {
            return base.Text;
        }
        set
        {
            base.Text = value.ToUpper();
            if (base.Text.Length > maximumLength)
                base.Text = base.Text.Substring(base.Text.Length - maximumLength);
            Invalidate();
        }
    }

    public int CaretIndex
    {
        get { return caretIndex; }
        set
        {
            caretIndex = value;
            Invalidate();
        }
    }

    public CaretLabel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // 
        // CaretLabel
        //
        this.SuspendLayout();
        this.Name = "CaretLabel";
        this.Size = new System.Drawing.Size(271, 38);
        this.ResumeLayout(false);

    }
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        SizeF charSize = g.MeasureString("X", base.Font);
        g.DrawString(base.Text, base.Font, new SolidBrush(base.ForeColor), this.ClientRectangle.X, e.ClipRectangle.Top);
        string text = base.Text;
        if (text.Length > 0)
        {
            if (text[text.Length - 1] == ' ')
            {
                text = text.Remove(text.Length - 1);
                text += 'X';
            }
        }
        if (isCaretEnable && caretIndex >= 0)
        {
            int numChars = base.Text.Length;
            RectangleF currentCharRect;
            CharacterRange[] characterRanges = new CharacterRange[numChars];
            for (int i = 0; i < numChars; i++)
                characterRanges[i] = new CharacterRange(i, 1);


            StringFormat stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.NoClip;
            stringFormat.SetMeasurableCharacterRanges(characterRanges);


            Region[] region = g.MeasureCharacterRanges(text, base.Font, new Rectangle(0,0,int.MaxValue,int.MaxValue), stringFormat);

            if (caretIndex >= region.Length)
                caretIndex = caretIndex - 1;

            currentCharRect = region[caretIndex].GetBounds(g);
            g.FillRectangle(new SolidBrush(base.ForeColor),
                                currentCharRect.X,
                                charSize.Height + 1,
                                currentCharRect.Width,
                                2);

        }
        g.Dispose();
    }
}
