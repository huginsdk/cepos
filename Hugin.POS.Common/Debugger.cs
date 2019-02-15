using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hugin.POS.Common
{
    public class Debugger
    {
        private static Debugger debugger = null;
        private static string fileName = "cfDebug.txt";

        //SerialPort sp = null;

        private Debugger()
        {
            //if (sp == null)
            //{
            //    sp = new SerialPort(portName, baudrate);
            //    sp.ReadTimeout = 450;
            //    sp.WriteTimeout = 450;
            //    sp.NewLine = "\r";
            //    sp.Encoding = PosConfiguration.DefaultEncoding;
            //    sp.ReadBufferSize = 4060;
            //    sp.WriteBufferSize = 4060;
            //    sp.DtrEnable = true;
            //    sp.PinChanged += new SerialPinChangedEventHandler(sp_PinChanged);
            //}
        }
        public void Clear()
        {
            IOUtil.WriteAllText(fileName, "");
        }
        public void AppendLine(String line)
        {
            FileStream fs = null;

            try
            {
                using (fs = File.OpenWrite(fileName))
                {
                    fs.Seek(0, SeekOrigin.End);

                    fs.Write(PosConfiguration.DefaultEncoding.GetBytes(line), 0, line.Length);
                    fs.Write(PosConfiguration.DefaultEncoding.GetBytes("\r\n"), 0, 2);
                }
            }
            catch
            {
                //Do nothing.
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public static Debugger Instance()
        {
            if (debugger == null)
            {
                debugger = new Debugger();
                debugger.AppendLine("**************************************");
                debugger.AppendLine("DateTime " + DateTime.Now.ToString());
                debugger.AppendLine("**************************************");
            }
            return debugger;
        }
    }
}
