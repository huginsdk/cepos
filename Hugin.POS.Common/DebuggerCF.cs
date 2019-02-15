using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.IO;

namespace Hugin.POS.Common
{
    public class Debugger
    {
        private static Debugger debuggerCF = null;
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
            FileStream fs = File.OpenWrite(fileName);
            fs.Seek(0, SeekOrigin.End);

            fs.Write(PosConfiguration.DefaultEncoding.GetBytes(line), 0, line.Length);
            fs.Write(PosConfiguration.DefaultEncoding.GetBytes("\r\n"), 0, 2);

            fs.Close();
        }

        public static Debugger Instance()
        {
            if (debuggerCF == null)
            {
                debuggerCF = new Debugger();
                debuggerCF.Clear();
            }
            return debuggerCF;
        }
    }
}
