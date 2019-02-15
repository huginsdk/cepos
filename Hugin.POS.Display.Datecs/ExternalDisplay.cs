using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO.Ports;

namespace Hugin.POS.Display.Datecs
{
    class ExternalDisplay
    {

        protected const byte Off = 0x00,
           On = 0x01,
           Warning = 0x07,
           CursorMoveLeft = 0x08,
           CursorMoveRight = 0x09,
           CursorMoveDown = 0x0A,
           CursorHome = 0x0B,
           ClrScreen = 0x0C,
           BothDisplays = 0x1A,
           CustomerDisplayOnly = 0x1C,
           CashierDisplayOnly = 0x1D,
           ClearWarning = 0x1E,
           Initialize = 0x1F,
           Space = 0x20,
           InitScreen = 0x28;

        protected readonly byte[] normalMode = { ClrScreen, Initialize, 0x01 };
        protected readonly byte[] verticalScrollMode = { ClrScreen, Initialize, 0x02 };
        protected readonly byte[] horizontalScrollMode = { ClrScreen, Initialize, 0x03 };
        protected readonly byte[] initializeDisplay = { 0x1B, 0x40 };

        Object serialLock = new Object();

        SerialPort serialPort = null;

        public ExternalDisplay()
        {
            String portName = PosConfiguration.Get("DisplayComPort");
            serialPort = new SerialPort(portName);
            if (!serialPort.IsOpen)
            {
                serialPort.ReadTimeout = 2048;

                try
                {
                    serialPort.Open();
                }
                catch (System.IO.IOException ex)
                {
                    if (!serialPort.IsOpen)
                        throw ex;
                }

                //Write(initializeDisplay, 0, 3);
                //serialPort.Write(horizontalScrollMode, 0, horizontalScrollMode.Length);
                //System.Threading.Thread.Sleep(100);
                //serialPort.Write(normalMode, 0, normalMode.Length);
            }

        }

        internal void Show(String msg)
        {            
            byte[] cursorHome = { CursorHome };
            Write(cursorHome, 0, 1);
            
            StringBuilder printMsg = new StringBuilder();
            if (!Str.Contains(msg, "\n")) msg += "\n";

            foreach (String line in msg.Split('\n'))
            {
                if (Str.Contains(line, "\t"))
                    printMsg.Append(PadCenter(line));
                else
                    printMsg.Append(AlignCenter(line));
            }
            Write(printMsg.ToString(), 0, 42);

        }


        internal void Show(String format, params object[] args)
        {
            Show(String.Format(format, args));
        }

        internal void Show(IAdjustment adjustment)
        {
            try
            {
                if (adjustment.Target is IFiscalItem)
                {
                    if (adjustment.Method == (AdjustmentType.PercentDiscount))
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.PRODUCT_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.PRODUCT_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n \t{1:C} ", Common.PosMessage.PRODUCT_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n \t{1:C} ", Common.PosMessage.PRODUCT_PRICE_FEE,
                                              new Number(adjustment.NetAmount));

                }
                else if (adjustment.Target is ISalesDocument)
                {
                    if (adjustment.Method == (AdjustmentType.PercentDiscount))
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.SUBTOTAL_PERCENT_DISCOUNT,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.PercentFee)
                        Show("{0}\n{1:P}\t{2:C}", Common.PosMessage.SUBTOTAL_PERCENT_FEE,
                                                 new Number(adjustment.RequestValue / 100),
                                                 new Number(adjustment.NetAmount));
                    else if (adjustment.Method == AdjustmentType.Discount)
                        Show("{0}\n \t{1:C}", Common.PosMessage.SUBTOTAL_PRICE_DISCOUNT,
                                             new Number(adjustment.NetAmount));
                    else
                        Show("{0}\n \t{1:C}", Common.PosMessage.SUBTOTAL_PRICE_FEE,
                                             new Number(adjustment.NetAmount));

                }
            }
            catch (FormatException)
            {
            }
        }

        internal void ShowSale(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(Math.Round(fi.Quantity - fi.VoidQuantity, 3)), 
                    large ? "X" : fi.Unit, new Number(fi.TotalAmount - fi.VoidAmount));
                
            }
            catch (FormatException)
            {
            }
        }

        internal void ShowVoid(IFiscalItem fi)
        {
            try
            {
                bool large = ((fi.Quantity != (long)fi.Quantity) && fi.TotalAmount > 1000000);
                Show("{0}\n{1:G10} {2}\t{3:C}", fi.Name, new Number(fi.Quantity), large ? "X" : fi.Unit, new Number(fi.TotalAmount));
            }
            catch (FormatException fex)
            {
                EZLogger.Log.Error("FormatException occured. {0}", fex.Message);
            }
        }

        internal void ShowCorrect(IAdjustment adjustment)
        {
            try
            {
                if (adjustment.Method == (AdjustmentType.PercentDiscount) ||
                   (adjustment.Method == AdjustmentType.PercentFee))
                    Show("{0}\n{1:P}\t{2:C} ", Common.PosMessage.CORRECTION,
                                         new Number(adjustment.RequestValue / 100),
                                         new Number(adjustment.NetAmount));
                else
                    Show("{0}\n \t{1:C} ", Common.PosMessage.CORRECTION,
                                          new Number(adjustment.NetAmount));
            }
            catch (FormatException)
            {
            }
        }

        private String PadCenter(String msg)
        {
            String[] str = msg.Split('\t');
            String s1 = str[0];
            String s2 = str[1];
            int length = s1.Length + s2.Length;
            s1 = s1.PadRight(20 - s2.Length, ' ');
            return s1 + s2;
        }
        private String AlignCenter(String msg)
        {
            int length = msg.Length;
            int spaceLeft = (20 - length) / 2;
            int spaceRight = 20 - length - spaceLeft;
            msg = msg.PadLeft(20 - spaceRight, ' ');
            msg = msg.PadRight(20, ' ');
            return msg;
        }

        private byte[] FixTurkish(String s)
        {
            byte[] b = new byte[s.Length + 2];
            int i = 0;
            foreach (char c in s)
            {
                switch ((int)c)
                {
                    case 286: //G.
                        b[i++] = (byte)'G';
                        break;
                    case 287: //g.
                        b[i++] = (byte)'g';
                        break;
                    case 220: //U.
                        b[i++] = 154;
                        break;
                    case 252: //u:
                        goto case 220;
                    case 350: //Þ.
                        b[i++] = (byte)'S';
                        break;
                    case 351: //s.
                        b[i++] = (byte)'s';
                        break;
                    case 304: //I.
                        b[i++] = (byte)'i';
                        break;
                    case 305: //i no .
                        b[i++] = (byte)'i';
                        break;
                    case 199: //C.
                        b[i++] = 128;
                        break;
                    case 214: //O:
                        b[i++] = 153;
                        break;
                    case 231: //c.
                        b[i++] = (byte)'c';
                        break;
                    case 246: //o:
                        b[i++] = 153;
                        break;
                    default:
                        b[i++] = (byte)c;
                        break;

                }
            }
            b[s.Length] = CursorHome;
            b[s.Length + 1] = CursorMoveDown;
            return b;
        }

        private void Write(String msg, int offset, int count)
        {
            byte[] buf;
            buf = FixTurkish(msg);
            int i;
            lock (serialLock)
            {
                for (i = 0; i < count; i++)
                {
                    serialPort.Write(buf, i, 1);
                }
            
            }

        }
        
        
        private void Write(byte[] msg, int offset, int count)
        {
            lock (serialLock)
            {
                for (int i = 0; i < count; i++)
                {
                    serialPort.Write(msg, i, 1);
                }
            }
        }

    }

}
