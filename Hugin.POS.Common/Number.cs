using System;
using System.Collections;
using System.Text;
using System.Globalization;

namespace Hugin.POS.Common
{
    public class Number : IComparable, IFormattable
    {
        StringBuilder sb = new StringBuilder();
        static NumberFormatInfo nfi = PosConfiguration.CultureInfo.NumberFormat;
        String decimalSeperator;

        public Number()
        {
            nfi.CurrencyDecimalDigits = 2;
            nfi.NumberDecimalDigits = 3;
            nfi.CurrencySymbol = "";
            nfi.PercentDecimalDigits = 0;
            nfi.PercentPositivePattern = 2;
            decimalSeperator = nfi.CurrencyDecimalSeparator;
        }

        public Number(Decimal d)
            : this()
        {
            sb.Append(d.ToString("G", nfi));
        }

        public Number(String s)
            : this()
        {
            sb.Append(s);
        }

        public Number(int i)
            : this()
        {
            sb.Append(i);
        }

        public String OverrideDecimalSeperator
        {
            get { return decimalSeperator; }
            set { decimalSeperator = value; }

        }

        public static String DecimalSeperator
        {
            get { return nfi.CurrencyDecimalSeparator; }
        }

        public int CompareTo(Object o)
        {
            return 1;
        }

        public void AppendDecimal(char c)
        {
            sb.Append(c);
        }

        public bool AddSeperator()
        {
            bool hasDecimal = (sb.ToString().IndexOf(decimalSeperator)) > -1;
            if (!hasDecimal)
                sb.Append(decimalSeperator);
            return (!hasDecimal);
        }

        public bool IsEmpty
        {
            get { return sb.Length == 0; }
        }

        public void Clear()
        {
            sb = new StringBuilder();
        }

        public static Number operator +(Number n1, Number n2)
        {
            return new Number(n1.ToDecimal() + n2.ToDecimal());
        }

        public static Number operator -(Number n1, Number n2)
        {
            return new Number(n1.ToDecimal() - n2.ToDecimal());
        }

        public static Number operator /(Number n1, Number n2)
        {
            return new Number(n1.ToDecimal() / n2.ToDecimal());
        }

        public static Number operator /(Number n1, int n2)
        {
            return new Number(n1.ToDecimal() / n2);
        }

        public static Number operator *(Number n1, Number n2)
        {
            return new Number(n1.ToDecimal() * n2.ToDecimal());
        }
        public static Number operator *(Number n1, Decimal n2)
        {
            return new Number(n1.ToDecimal() * n2);
        }
        public static Number operator *(Decimal n1, Number n2)
        {
            return new Number(n1 * n2.ToDecimal());
        }

        public static bool operator ==(Number n1, Number n2)
        {
            return n1.Equals(n2);
        }

        public static bool operator !=(Number n1, Number n2)
        {
            return !n1.Equals(n2);
        }
        public static explicit operator Number(int i)
        {
            return new Number(i);
        }
        public static explicit operator Number(Decimal d)
        {
            return new Number(d);
        }
        public static explicit operator int(Number n)
        {
            return Int32.Parse(n.ToString());
        }
        public override bool Equals(object o)
        {
            if (o is Number)
                return ((Number)o).ToDecimal() == this.ToDecimal();
            if (o is int)
            {
                int i = (int)o;
                return i == this.ToDecimal();
            }
            return false;
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public int Decimals
        {
            get
            {
                int i = sb.ToString().IndexOf(DecimalSeperator);
                if (i < 0) return 0;
                return sb.Length - i - 1;
            }
        }

        public Decimal ToDecimal()
        {

            if (sb.Length == 0) return 0;
            if (sb.Length == 1 && sb.ToString().Equals(decimalSeperator)) return 0;
            return Decimal.Parse(sb.ToString(), nfi);
        }
        public int Length
        {
            get { return sb.Length; }
            set { sb.Length = 0; }
        }
        public int ToInt()
        {
            return (int)ToDecimal();
        }
        public void RemoveLastDigit()
        {
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
        }
        public Double ToDouble()
        {
            if (sb.Length == 0) return 0;
            return Double.Parse(sb.ToString(), NumberStyles.Currency, nfi);
        }

        public void AddSpace()
        {
            sb.Append(" ");
        }

        //from http://www.codeproject.com/csharp/custstrformat.asp

        public override String ToString()
        {
            return ToString("G", null);
        }

        public string ToString(string format)
        {
            return ToString(format, nfi);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) format = "G";
            if (format.Equals("C"))
            {
                return String.Format(nfi, "{0:C}", ToDecimal()).TrimEnd();
            }
            else if (format.Equals("N"))
                return String.Format(nfi, "{0:N}", ToDecimal());
            else if (format.Equals("D"))
                return String.Format(nfi, "{0:N}", ToDecimal());
            else if (format.Equals("P"))
            {
                String str = String.Format(nfi, "{0:P}", ToDecimal());
                if (ToDecimal() < 0.1m)
                    return str.Insert(1, "0");
                else return str;
            }
            else if (format.Equals("B")) //This is used for barcodes
                return sb.ToString();
            else if (format.StartsWith("Q"))
            {

                String s = ToDecimal().ToString();
                if ((s.IndexOf(decimalSeperator)) > -1)
                {
                    char seperator = decimalSeperator.ToCharArray()[0];
                    String[] str = s.Split(seperator);
                    int decimals = (str[1].Length > 3) ? 3 : str[1].Length;
                    Decimal d = Decimal.Parse(str[1].ToString());
                    if (d == 0)
                        s = str[0];
                    else
                        s = str[0] + decimalSeperator + str[1].Substring(0, decimals);
                }
                if (format.Equals("Q")) return s;
                else
                {
                    int i = int.Parse(format.Substring(1, format.Length - 1));
                    if (s.Length < i) return s.PadLeft(i, '0');
                    else return s.Substring(s.Length - i, i);
                }
            }
            else
            {

                String s = ToDecimal().ToString(format);
                if (s.IndexOf(decimalSeperator) < 0) return s;
                else
                {
                    char seperator = decimalSeperator.ToCharArray()[0];
                    String[] str = s.Split(seperator);
                    int decimals = (str[1].Length > 3) ? 3 : str[1].Length;
                    return str[0] + decimalSeperator + str[1].Substring(0, decimals);
                }

            }
        }

    }
}
