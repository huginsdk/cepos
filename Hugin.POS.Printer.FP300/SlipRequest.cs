using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    internal enum Printmode
    {
        Font5x7 = 0,
        Font7x7 = 1,
        DoubleHeight = 16,
        DoubleWidth = 32,
        Underlined = 128
    }

    internal enum Part
    {
        Printer = 1,
        Offline = 2,
        Error = 3,
        SlipPaper = 5,
    }

    class SlipRequest : IPrinterRequest
    {
        String request;

        private static string strLineFeed = String.Format("{0}", (char)0x000A);//'\n'
        private static string strEject = String.Format("{0}", (char)0x000C);//'\f'
        private static string strStatus = String.Format("{0}{1}", (char)0x0010, (char)0x0004);
        private static string strPrintmode = String.Format("{0}{1}", (char)0x001B, (char)0x0021);//ESC + '!'
        private static string strInitialize = String.Format("{0}{1}", (char)0x001B, (char)0x0040);//ESC + '@'
        private static string strReverseEject = String.Format("{0}{1}", (char)0x001B, (char)0x0046);
        private static string strRelease = String.Format("{0}{1}", (char)0x001B, (char)0x0071);
        private static string strSlipMode5200 = String.Format("{0}{1}", (char)0x001B, (char)0x004C); // ESC + L

        private static string strEnableSensor = String.Format("{0}{1}{2}", (char)0x001B, "c4", (char)48); // Enable a sensor to stop printing due to paper end
        private static string strSttngSheet = String.Format("{0}{1}{2}", (char)0x001B, "c1", (char)4); // Select a setting sheet(slip)
        private static string strSlipSheet = String.Format("{0}{1}{2}", (char)0x001B, "c0", (char)4); // Select a print sheet (slip)

        private static bool checkEJ = false;
        internal bool CheckEJ
        {
            get
            {
                return checkEJ;
            }
        }
        internal SlipRequest(String request)
        {
            checkEJ = false;
            this.request = request;
        }
        internal static IPrinterRequest Initialize()
        {
            return new SlipRequest(strInitialize);
        }
        internal static IPrinterRequest InitSlipPrinter()
        {
            return new SlipRequest(strEnableSensor + strSttngSheet + strSlipSheet);
        }
        internal static IPrinterRequest SetReverseEject(Boolean reverseSet)
        {
            return new SlipRequest(strReverseEject + (char)(reverseSet ? 1 : 0));
        }
        internal static IPrinterRequest EjectSheet()
        {
            return new SlipRequest(strEject);
        }
        internal static IPrinterRequest Release()
        {
            return new SlipRequest(strRelease);
        }
        internal static IPrinterRequest Write(String line)
        {
            return new SlipRequest(line);
        }
        internal static IPrinterRequest WriteLine(String line)
        {
            char[] chars = new char[2]; chars[0] = '²'; chars[1] = '³';
            if (line.IndexOfAny(chars) > -1)
            {
                line = line.Replace(chars[0] + "", strPrintmode + (char)Printmode.DoubleHeight);
                line = line.Replace(chars[1] + "", strPrintmode + (char)Printmode.Font7x7);
            }
            line = FixTurkish(line);
            IPrinterRequest tempReqst = new SlipRequest(line + strLineFeed);
            checkEJ = true;
            return tempReqst;
        }

        internal static IPrinterRequest SwitchPrintMode()
        {
            return new SlipRequest(strSlipMode5200);
        }

        private static string FixTurkish(string line)
        {
            //line = line.Replace('Ý', (char)0x98);
            //line = line.Replace('Þ', (char)0xAD);
            //line = line.Replace('Ð', (char)0x9D);
            //line = line.Replace('Ö', (char)0x99);
            //line = line.Replace('Ü', (char)0x9A);
            //line = line.Replace('Ç', (char)0x80);

            line = line.Replace('Ý', 'I');
            line = line.Replace('Þ', 'S');
            line = line.Replace('Ð', 'G');
            line = line.Replace('Ö', 'O');
            line = line.Replace('Ü', 'U');
            line = line.Replace('Ç', 'C');
            return line;
        }
        internal static IPrinterRequest ChangeFont(Printmode mode)
        {
            return new SlipRequest(strPrintmode + (char)mode);
        }
        internal static IPrinterRequest CheckStatus(Part part)
        {
            return new SlipRequest(strStatus + (char)part);
        }
        //internal static PrinterRequest CancelData()
        //{
        //    return new SlipRequest(strCancelData);
        //}private static string strCancelData = String.Format("{0}", (char)0x0018);

        internal bool StatusCheck
        {
            get { return request.StartsWith(strStatus); }
        }
        public override string ToString()
        {
            return request;
        }
    }
}
