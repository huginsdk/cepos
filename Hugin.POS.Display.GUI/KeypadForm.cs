using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hugin.POS.Common;

namespace Hugin.POS.Display.GUI
{
    public partial class KeypadForm : Form
    {
        public event ConsumeKeyHandler ConsumeKey;

        public KeypadForm()
        {
            InitializeComponent();
        }

        private void tlpPoskeys_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            PosKey key = PosKey.CapsLock;
            int buffer = -1;
            string butonName = ((Button)sender).Name;
            switch (butonName)
            {
                case "btnD00":
                    key = PosKey.D0;
                    SendKeys(key);
                    break;
                case "btnD9":
                    key = PosKey.D9;
                    break;
                case "btnD8":
                    key = PosKey.D8;
                    break;
                case "btnD7":
                    key = PosKey.D7;
                    break;
                case "btnD6":
                    key = PosKey.D6;
                    break;
                case "btnD5":
                    key = PosKey.D5;
                    break;
                case "btnD4":
                    key = PosKey.D4;
                    break;
                case "btnD3":
                    key = PosKey.D3;
                    break;
                case "btnD2":
                    key = PosKey.D2;
                    break;
                case "btnD1":
                    key = PosKey.D1;
                    break;
                case "btnD0":
                    key = PosKey.D0;
                    break;

                case "btnPLU18":
                case "btnPLU17":
                case "btnPLU16":
                case "btnPLU15":
                case "btnPLU14":
                case "btnPLU13":
                case "btnPLU12":
                case "btnPLU11":
                case "btnPLU10":
                case "btnPLU9":
                case "btnPLU8":
                case "btnPLU7":
                case "btnPLU6":
                case "btnPLU5":
                case "btnPLU4":
                case "btnPLU3":
                case "btnPLU2":
                case "btnPLU1":
                    buffer = int.Parse(butonName.Substring(6));
                    KeyMap.LabelBuffer = buffer - 1;

                    key = PosKey.LabelStx;
                    break;
                case "btnEnter":
                    key = PosKey.Enter;
                    break;
                case "btnEsc":
                    key = PosKey.Escape;
                    break;
                case "btnSeperator":
                    key = PosKey.Decimal;
                    break;
                case "btnDown":
                    key = PosKey.DownArrow;
                    break;
                case "btnUp":
                    key = PosKey.UpArrow;
                    break;
                case "btnFee":
                    key = PosKey.Fee;
                    break;
                case "btnPercentFee":
                    key = PosKey.PercentFee;
                    break;
                case "btnDiscount":
                    key = PosKey.Discount;
                    break;
                case "btnPercentDisc":
                    key = PosKey.PercentDiscount;
                    break;
                case "btnProcess":
                    key = PosKey.Command;
                    break;
                case "btnCash":
                    key = PosKey.Cash;
                    break;
                case "btnRepeat":
                    key = PosKey.Repeat;
                    break;
                case "btnProgram":
                    key = PosKey.Program;
                    break;
                case "btnCurrency":
                    key = PosKey.ForeignCurrency;
                    break;
                case "btnAmount":
                    key = PosKey.Total;
                    break;
                case "btnSubtotal":
                    key = PosKey.SubTotal;
                    break;
                case "btnReport":
                    key = PosKey.Report;
                    break;
                case "btnQuantity":
                    key = PosKey.Quantity;
                    break;
                case "btnPrice":
                    key = PosKey.Price;
                    break;
                case "btnCustomer":
                    key = PosKey.Customer;
                    break;
                case "btnSalesPerson":
                    key = PosKey.SalesPerson;
                    break;
                case "btnDrawer":
                    key = PosKey.CashDrawer;
                    break;
                case "btnCheck":
                    key = PosKey.Check;
                    break;

                case "btnCredit4":
                case "btnCredit3":
                case "btnCredit2":
                case "btnCredit1":
                    buffer = int.Parse(butonName.Substring(9));
                    KeyMap.CreditBuffer = buffer;
                    key = PosKey.Credit;
                    break;
                case "btnCredit":
                    KeyMap.CreditBuffer = 0;
                    key = PosKey.Credit;
                    break;
                case "btnPriceLookup":
                    key = PosKey.PriceLookup;
                    break;
                case "btnVoid":
                    key = PosKey.Void;
                    break;
                case "btnDoc":
                    key = PosKey.Document;
                    break;
            }
            SendKeys(key);

        }

        private void SendKeys(PosKey key)
        {
            if (ConsumeKey != null)
                ConsumeKey(this, new ConsumeKeyEventArgs((int)key));
        }
    }
}