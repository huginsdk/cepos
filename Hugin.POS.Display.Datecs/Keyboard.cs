using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using GradientButton;
using Hugin.POS.Common;

namespace Hugin.POS.Display.Datecs
{

    public class Keyboard : UserControl
    {
        private static int COLUMN_COUNT = 13;
        private static int ROW_COUNT = 5;

        private TableLayoutPanel tblKeyboard;
        public event ConsumeKeyHandler ConsumeKey;

        public Keyboard()
        {
            InitializeComponent();
            InitializeKeyboard();

            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(64)))), ((int)(((byte)(97)))));
        }

        private void InitializeKeyboard()
        {
            //Create Columns
            tblKeyboard.ColumnCount = COLUMN_COUNT;
            tblKeyboard.ColumnStyles.Clear();
            for (int i = 0; i < COLUMN_COUNT; i++)
            {
                tblKeyboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, (float)(100 / COLUMN_COUNT)));
            }

            //Create Rows
            tblKeyboard.RowCount = ROW_COUNT;
            tblKeyboard.RowStyles.Clear();
            for (int i = 0; i < ROW_COUNT; i++)
            {
                tblKeyboard.RowStyles.Add(new RowStyle(SizeType.Percent, (float)(100 / ROW_COUNT)));
            }

            AddWindow(WindowType.Input);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tblKeyboard = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // tblKeyboard
            // 
            this.tblKeyboard.ColumnCount = 1;
            this.tblKeyboard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblKeyboard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblKeyboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblKeyboard.Location = new System.Drawing.Point(0, 0);
            this.tblKeyboard.Name = "tblKeyboard";
            this.tblKeyboard.RowCount = 1;
            this.tblKeyboard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblKeyboard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblKeyboard.Size = new System.Drawing.Size(1024, 380);
            this.tblKeyboard.TabIndex = 0;
            // 
            // Keyboard
            // 
            this.Controls.Add(this.tblKeyboard);
            this.Name = "Keyboard";
            this.Size = new System.Drawing.Size(1024, 380);
            this.ResumeLayout(false);

        }

        #endregion

        #region Main Panel Control

        private void AddWindow(WindowType windowsType)
        {
            List<Skin> skins = Skin.GetSkin(windowsType);
            CreateButton(skins);
        }

        private void CreateButton(List<Skin> skins)
        {
            GradientFilledButton gfb = null;
            foreach (Skin skin in skins)
            {
                gfb = new GradientFilledButton();
                gfb.Name = skin.Name;
                gfb.Text = skin.Text;
                gfb.StartColor = skin.StartColor;
                gfb.EndColor = skin.EndColor;
                gfb.ForeColor = skin.ForeColor;
                gfb.Dock = DockStyle.Fill;
                gfb.Font = skin.Font;
                gfb.Click += new EventHandler(gfb_Click);
                tblKeyboard.Controls.Add(gfb, skin.CellInfo.X, skin.CellInfo.Y);

                if (skin.ColSpan > 1)
                    tblKeyboard.SetColumnSpan(gfb, skin.ColSpan);
                if (skin.RowSpan > 1)
                    tblKeyboard.SetRowSpan(gfb, skin.RowSpan);
            }
        }


        void gfb_Click(object sender, EventArgs e)
        {
            PosKey key = PosKey.UndefinedKey;
            string keyText = ((GradientFilledButton)sender).Name.Substring(3);
            switch (keyText)
            {
                case "Enter":
                    key = PosKey.Enter;
                    break;
                case "Backspace":
                    key = PosKey.Correction;
                    break;
                case "Delete":
                    key = PosKey.Escape;
                    break;
                case "UpArrow":
                    key = PosKey.UpArrow;
                    break;
                case "DownArrow":
                    key = PosKey.DownArrow;
                    break;
                case "Space":
                    key = (PosKey)ConsoleKey.Spacebar;
                    break;
                case "Comma":
                    key = (PosKey)ConsoleKey.Decimal;
                    break;
                case "Slash":
                    key = (PosKey)ConsoleKey.ForwardSlash;
                    break;
                case "Backslash":
                    key = (PosKey)'\\';
                    break;
                case "Colon":
                    key = (PosKey)':';
                    break;
                case "SemiColon":
                    key = (PosKey)';';
                    break;
                case "Minus":
                    key = (PosKey)'-';
                    break;
                case "Dot":
                    key = (PosKey)46;
                    break;
                  case "UnderScore":
                    key = (PosKey)'_';
                    break;
              case "D0":
                case "D1":
                case "D2":
                case "D3":
                case "D4":
                case "D5":
                case "D6":
                case "D7":
                case "D8":
                case "D9":
                    key = (PosKey)Enum.Parse(typeof(PosKey), keyText);
                    break;
                default:
                    if (char.IsLetter(keyText[0]))
                        key = (PosKey)keyText[0];
                    break;

            }
            ConsumeKey(sender, new ConsumeKeyEventArgs((int)key));

        }

        #endregion

    }
}
