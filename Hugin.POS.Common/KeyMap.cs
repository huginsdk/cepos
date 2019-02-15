using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;

namespace Hugin.POS.Common
{
    public class KeyMap
    {
        private static Dictionary<String, int> Label = new Dictionary<string, int>();
        private static Dictionary<String, int> Credit = new Dictionary<string, int>();
        private static Dictionary<String, int> KeyLock = new Dictionary<string, int>();
        private static List<String> CardPrefix = new List<String>();

        private static Dictionary<String, PosKey> PosKeys = new Dictionary<string, PosKey>();
        private static Dictionary<String, String> Chars = new Dictionary<String, String>();

        private static string DoubleZero = "00";

        private static int lastOrder = 0;//i.e. Credits C0, C1, C2....0,1,2 are orders
        private static PosKey lastPosKey = 0;

        public static int LabelBuffer = -1;
        public static int KeyLockBuffer = -1;
        public static int CreditBuffer = -1;

        private static String KeysPath
        {
            get
            {
                return IOUtil.ProgramDirectory + "Properties\\Keyboard.ini";
            }
        }

        public static void Load()
        {

            if (!File.Exists(KeysPath))
            {
                LoadDefaults();
                return;
            }
            StreamReader sr = new StreamReader(KeysPath, PosConfiguration.DefaultEncoding);
            try
            {
                String line = "";
                String key = "";
                String value = "";
                String chars = "";
                while ((line = sr.ReadLine()) != null)
                {
                    int index = line.IndexOf('=');
                    if (index < 1) continue;
                    key = line.Substring(0, index).Trim();
                    value = line.Substring(index + 1).Trim();
                    chars = "";

                    index = value.IndexOf(',');
                    if (index > 0)
                    {
                        chars=value.Substring(index + 1).Trim();
                        value = value.Substring(0, index).Trim();
                    }
                    switch (chars) {
                        case "Enter": chars = "\n"; break;
                        case "Space": chars = " "; break;
                        case "Backspace": chars = "\b"; break; 
                    }

                    if (value.StartsWith("Label"))
                    {
                        int plu = 0;
                        if (Parser.TryInt(value.Substring(5), out plu))
                        {
                            if (!Label.ContainsKey(key))
                                Label.Add(key, plu-1);
                            if (chars.Length > 0)
                                Chars.Add("Label" + plu, chars);
                        }
                        continue;


                    }
                    if (value.StartsWith("KeyLock"))
                    {
                        int auth = 0;
                        if (Parser.TryInt(value.Substring(7), out auth))
                        {
                            if (!KeyLock.ContainsKey(key))
                                KeyLock.Add(key, auth);
                        }
                        continue;
                    }
                    if (value == "Credit") value += "0";
                    if (value.StartsWith("Credit"))
                    {
                        int creditIndex = 0;

                        if (Parser.TryInt(value.Substring(6), out creditIndex))
                        {
                            if (!Credit.ContainsKey(key))
                                Credit.Add(key, creditIndex);
                            if (chars.Length > 0)
                                Chars.Add("Credit" + creditIndex, chars);
                        }
                        continue;
                    }


                    switch (value)
                    {
                        case "0":
                            Add(key, PosKey.D0, chars); break;
                        case "1":
                            Add(key, PosKey.D1, chars); break;
                        case "2":
                            Add(key, PosKey.D2, chars); break;
                        case "3":
                            Add(key, PosKey.D3, chars); break;
                        case "4":
                            Add(key, PosKey.D4, chars); break;
                        case "5":
                            Add(key, PosKey.D5, chars); break;
                        case "6":
                            Add(key, PosKey.D6, chars); break;
                        case "7":
                            Add(key, PosKey.D7, chars); break;
                        case "8":
                            Add(key, PosKey.D8, chars); break;
                        case "9":
                            Add(key, PosKey.D9, chars); break;
                        case "Document":
                            Add(key, PosKey.Document, chars); break;
                        case "Customer":
                            Add(key, PosKey.Customer, chars); break;
                        case "Report":
                            Add(key, PosKey.Report, chars); break;
                        case "Program":
                            Add(key, PosKey.Program, chars); break;
                        case "Command":
                            Add(key, PosKey.Command, chars); break;
                        case "CashDrawer":
                            Add(key, PosKey.CashDrawer, chars); break;
                        case "Void":
                            Add(key, PosKey.Void, chars); break;
                        case "PercentDiscount":
                            Add(key, PosKey.PercentDiscount, chars); break;
                        case "Discount":
                            Add(key, PosKey.Discount, chars); break;
                        case "PercentFee":
                            Add(key, PosKey.PercentFee, chars); break;
                        case "Fee":
                            Add(key, PosKey.Fee, chars); break;
                        case "SalesPerson":
                            Add(key, PosKey.SalesPerson, chars); break;
                        case "PriceLookup":
                            Add(key, PosKey.PriceLookup, chars); break;
                        case "Price":
                            Add(key, PosKey.Price, chars); break;
                        case "Help":
                            Add(key, PosKey.Help, chars); break;
                        case "Total":
                            Add(key, PosKey.Total, chars); break;
                        case "Repeat":
                            Add(key, PosKey.Repeat, chars); break;
                        case "UpArrow":
                            Add(key, PosKey.UpArrow, chars); break;
                        case "DownArrow":
                            Add(key, PosKey.DownArrow, chars); break;
                        case "Escape":
                            Add(key, PosKey.Escape, chars); break;
                        case "Quantity":
                            Add(key, PosKey.Quantity, chars); break;
                        case "Check":
                            Add(key, PosKey.Check, chars); break;
                        case "ForeignCurrency":
                            Add(key, PosKey.ForeignCurrency, chars); break;
                        case "Cash":
                            Add(key, PosKey.Cash, chars); break;
                        case "SubTotal":
                            Add(key, PosKey.SubTotal, chars); break;
                        case "Enter":
                            Add(key, PosKey.Enter, chars); break;
                        case "Payment":
                            Add(key, PosKey.Payment, chars); break;
                        case "Decimal":
                            Add(key, PosKey.Decimal, chars); break;
                        case "ReceiveOnAcct":
                            Add(key, PosKey.ReceiveOnAcct, chars); break;
                        case "PayOut":
                            Add(key, PosKey.PayOut, chars); break;
                        case "Correction":
                            Add(key, PosKey.Correction, chars); break;
                        case "SendOrder":
                            Add(key, PosKey.SendOrder, chars); break;
                        case "MagstripeStx":
                            CardPrefix.Add(key); break;
                        case "DoubleZero":
                            DoubleZero = key; break;
                        default: break;
                    }
                }

            }
            catch { }
            finally
            {
                sr.Close();
            }
           

        }

        private static void Add(string key, PosKey posKey, string chars)
        {
            PosKeys.Add(key, posKey);
            if (chars == "Space")
                chars = " ";
            if (chars == "Enter")
                chars = "\n";
            if (chars == "Backspace")
                chars = "\b";

            if (chars.Length > 0)
                Chars.Add(posKey.ToString(), chars);
        }

        private static void LoadDefaults()
        {
            for (int i = 0; i < 5; i++)
                Credit.Add("C" + i, i);

            for (int i = 0; i < 18; i++)
                Label.Add("L" + i, i);

            for (int i = 0; i < 7; i++)
                KeyLock.Add("K" + i, i);

            CardPrefix.Add("%");
            CardPrefix.Add("%B");
            CardPrefix.Add(";");


            PosKeys.Add("0", PosKey.D0);
            PosKeys.Add("1", PosKey.D1);
            PosKeys.Add("2", PosKey.D2);
            PosKeys.Add("3", PosKey.D3);
            PosKeys.Add("4", PosKey.D4);
            PosKeys.Add("5", PosKey.D5);
            PosKeys.Add("6", PosKey.D6);
            PosKeys.Add("7", PosKey.D7);
            PosKeys.Add("8", PosKey.D8);
            PosKeys.Add("9", PosKey.D9);

            PosKeys.Add("A", PosKey.ReceiveOnAcct);
            PosKeys.Add("V", PosKey.PayOut);

            PosKeys.Add("D", PosKey.Document);
            PosKeys.Add("B", PosKey.Customer);

            PosKeys.Add("R", PosKey.Report);
            PosKeys.Add("P", PosKey.Program);
            PosKeys.Add("O", PosKey.Command);
            PosKeys.Add("F4", PosKey.CashDrawer);

            PosKeys.Add("F3", PosKey.Void);
            PosKeys.Add("F9", PosKey.PercentDiscount);
            PosKeys.Add("F10", PosKey.Discount);
            PosKeys.Add("F11", PosKey.PercentFee);
            PosKeys.Add("F12", PosKey.Fee);
            PosKeys.Add("S", PosKey.SalesPerson);

            PosKeys.Add("F8", PosKey.PriceLookup);
            PosKeys.Add("F6", PosKey.Price);
            PosKeys.Add("F7", PosKey.Total);
            PosKeys.Add("F2", PosKey.Repeat);

            PosKeys.Add("UpArrow", PosKey.UpArrow);
            PosKeys.Add("DownArrow", PosKey.DownArrow);

            PosKeys.Add("Divide", PosKey.Check);
            PosKeys.Add("Multiply", PosKey.ForeignCurrency);
            PosKeys.Add("Subtract", PosKey.Cash);
            PosKeys.Add("Add", PosKey.SubTotal);

            PosKeys.Add("Escape", PosKey.Escape);
            PosKeys.Add("Enter", PosKey.Enter);
            PosKeys.Add("F5", PosKey.Quantity);

            PosKeys.Add("Decimal", PosKey.Decimal);
            PosKeys.Add("F1", PosKey.Help);

            PosKeys.Add("Q", PosKey.SendOrder);
            PosKeys.Add("Backspace", PosKey.Correction);

            DoubleZero = "00";

        }

        public static void LoadCharMatrix(Dictionary<int, string> charMatrix)
        {
            if (File.Exists(KeysPath)) return;

            foreach (int key in charMatrix.Keys)
                Chars.Add("Label" + key, charMatrix[key]);
        }
        private static string CharArrayToString(char[] array)
        {
            string val = "";
            foreach (Char c in array)
                val += c;
            return val;
        }
       
        public static string IsCard(String  input)
        {
            String prefix="";
            foreach (String item in CardPrefix)
            {
                if (input.StartsWith(item) && item.Length > prefix.Length)
                    prefix = item;
            }
            return input.Substring(prefix.Length);
        }
      
        public static PosKey 
            Get(String key)
        {
            if (Label.ContainsKey(key))
            {
                LabelBuffer = Label[key];
                return PosKey.LabelStx;
            }
            if (Credit.ContainsKey(key))
            {
                CreditBuffer = Credit[key];
                return PosKey.Credit;
            }
            if(key == "EFT")
            {
                CreditBuffer = 1;
                return PosKey.Credit;
            }
            if (KeyLock.ContainsKey(key))
            {
                KeyLockBuffer = KeyLock[key];
                return PosKey.KeyStx;
            }
            if (CardPrefix.Contains(key))
                return PosKey.MagstripeStx;

            if (key == DoubleZero || key == "1000")
            {
                return PosKey.DoubleZero;
            }
            if (PosKeys.ContainsKey(key))
                return PosKeys[key];
            int numeric = 0;
            
            if (Parser.TryInt(key, out numeric))
            {
                PosKey numerickey = (PosKey)numeric;
                switch (numerickey)
                {
                    case PosKey.Numpad0:
                    case PosKey.D0: return PosKey.D0;
                    case PosKey.Numpad1:
                    case PosKey.D1: return PosKey.D1;
                    case PosKey.Numpad2:
                    case PosKey.D2: return PosKey.D2;
                    case PosKey.Numpad3:
                    case PosKey.D3: return PosKey.D3;
                    case PosKey.Numpad4:
                    case PosKey.D4: return PosKey.D4;
                    case PosKey.Numpad5:
                    case PosKey.D5: return PosKey.D5;
                    case PosKey.Numpad6:
                    case PosKey.D6: return PosKey.D6;
                    case PosKey.Numpad7:
                    case PosKey.D7: return PosKey.D7;
                    case PosKey.Numpad8:
                    case PosKey.D8: return PosKey.D8;
                    case PosKey.Numpad9:
                    case PosKey.D9: return PosKey.D9;
                    default: break;
                }
            }

            return PosKey.UndefinedKey;

        }

        public static char GetLetter(PosKey key, int order, ref byte sequence)
        {
            lastPosKey = key;
            lastOrder = order;
            string strCriteria = "";
            switch (key)
            {
                case PosKey.LabelStx:
                    strCriteria = "Label" + (order + 1);
                    break;
                case PosKey.Credit:
                    strCriteria = "Credit" + order;
                    break;
                default:
                    strCriteria = key.ToString();
                    break;
            }
            if (Chars[strCriteria].Length == sequence)
                sequence = 0; //labelda tanimlanan son karakterde ise basa don. (A B C A...)
            return Chars[strCriteria][sequence];
        }

        public static int LastOrder
        {
            get { return lastOrder; }
            set { lastOrder = value; }
        }
        public static PosKey LastPosKey
        {
            get { return lastPosKey; }
            set { lastPosKey = value; }
        }
        public static Int32 NumberOfCreditKeys
        {
            get { return Credit.Count - 1; }
        }
    }
}
