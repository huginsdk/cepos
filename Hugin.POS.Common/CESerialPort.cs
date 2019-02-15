using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{

    #region Enums
    // Summary:
    //     Specifies the parity bit for a CESerialPort object.
    public enum ParityCE
    {
        // Summary:
        //     No parity check occurs.
        None = 0,
        //
        // Summary:
        //     Sets the parity bit so that the count of bits set is an odd number.
        Odd = 1,
        //
        // Summary:
        //     Sets the parity bit so that the count of bits set is an even number.
        Even = 2,
        //
        // Summary:
        //     Leaves the parity bit set to 1.
        Mark = 3,
        //
        // Summary:
        //     Leaves the parity bit set to 0.
        Space = 4,
    }

    // Summary:
    //     Specifies the number of stop bits used on the CESerialPort
    //     object.
    public enum StopBitsCE
    {
        //
        // Summary:
        //     One stop bit is used.
        One = 0,
        //
        // Summary:
        //     Two stop bits are used.
        Two = 2,
    }
    #endregion

    public class CESerialPort
    {
        int baudrate;
        string portName;
        int portNum;

        int dataBits = 8;
        StopBitsCE stopBits = StopBitsCE.One;
        ParityCE parity = ParityCE.None;
        int readTimeout = 100;
        int writeTimeout = 500;
        int readBuffSize = 1024;

        String newLine = "\r\n";
        System.Text.Encoding encoding = System.Text.Encoding.GetEncoding(1254);
        bool dtrEnable = false;
        bool isOpen = false;

        static bool dllInitialized = false;

        /* managing read buffer */
        String readBuffer = "";
        /**************************/

        /**************** extern methods *************************/
        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern void Init();

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern bool ClosePort(int portNum);

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern bool SetReadTimeout(int portNum, int timeout);

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern int GetReadTimeout(int portNum);

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern bool SetWriteTimeout(int portNum, int timeout);

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern int GetWriteTimeout(int portNum);

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern int GetCts(int portNum);

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern int OpenPort(int portNum, String portName, int baudRate, int dataBits,
                                            int stopBits, int parity,
                                            int readTimeout, int writeTimeout);

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern bool Send(int portNum, byte[] writeBytes, int bufLen);

        [System.Runtime.InteropServices.DllImport("WinCESerialPort.dll")]
        internal static extern int Receive(int portNum, byte[] readBytes, int len);
        /*********************************************************/
        public delegate void CESerialEventHandler(CESerialPort source);
        public event CESerialEventHandler DataReceived;

        public int ReadTimeout
        {
            get
            {
                if (isOpen)
                {
                    readTimeout = GetReadTimeout(portNum);
                }
                return readTimeout;
            }
            set
            {
                bool success = true;
                if (isOpen)
                {
                    success = SetReadTimeout(portNum, value);
                }
                if (success)
                {
                    readTimeout = value;
                }
            }
        }
        public int WriteTimeout
        {
            get
            {
                if (isOpen)
                {
                    writeTimeout = GetWriteTimeout(portNum);
                }
                return writeTimeout;
            }
            set
            {
                bool success = true;
                if (isOpen)
                {
                    success = SetWriteTimeout(portNum, value);
                }
                if (success)
                {
                    writeTimeout = value;
                }
            }
        }
        public int ReadBufferSize
        {
            get { return readBuffSize; }
            set { readBuffSize = value; }
        }
        public int WriteBufferSize
        {
            get { return 1; }
            set { }
        }
        public int BytesToRead
        {
            get { return readBuffer.Length; }
        }
        public void Write(String str)
        {
            byte[] writeBytes = encoding.GetBytes(str);
            Send(portNum, writeBytes, str.Length);
        }
        public int ReadByte()
        {
            int waitTime = 0;
            while (readBuffer.Length < 1)
            {
                System.Threading.Thread.Sleep(20);
                waitTime += 20;
                if (waitTime > ReadTimeout)
                {
                    throw new TimeoutException();
                }
            }
            int byteRead = (int)readBuffer[0];
            readBuffer = readBuffer.Substring(1);
            return byteRead;
        }
        public String ReadTo(String strEnd)
        {
            String retStr = readBuffer;

            int index = -1;
            int waitTime = 0;
            while (index < 0)
            {
                index = readBuffer.IndexOf(strEnd);
                System.Threading.Thread.Sleep(20);
                waitTime += 20;
                if (waitTime > ReadTimeout)
                {
                    throw new TimeoutException();
                }
            }

            if (index > 0)
            {
                retStr = readBuffer.Substring(0, index);
                readBuffer = readBuffer.Substring(index + strEnd.Length);
            }

            return retStr;

        }
        public String ReadLine()
        {
            return ReadTo(NewLine);
        }
        public String ReadExisting()
        {
            String retStr = readBuffer;
            readBuffer = "";

            return retStr;
        }

        public StopBitsCE StopBits
        {
            get { return stopBits; }
            set { stopBits = value; }
        }
        public int DataBits
        {
            get { return dataBits; }
            set { dataBits = value; }
        }
        public ParityCE Parity
        {
            get { return parity; }
            set { parity = value; }
        }
        public String NewLine
        {
            get { return newLine; }
            set { newLine = value; }
        }
        public System.Text.Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }
        public bool DtrEnable
        {
            get { return dtrEnable; }
            set { dtrEnable = value; }
        }
        public bool DsrHolding
        {
            get { return true; }
            set {  }
        }
        public bool CtsHolding
        {
            get
            {
                int ctsVal = GetCts(portNum);
                return ctsVal == 1;
            }
        }
        public bool IsOpen
        {
            get { return isOpen; }
        }

        public String PortName
        {
            get
            {
                return portName;
            }
            set
            {
                if (isOpen)
                {
                    throw new Exception("Port is open");
                }
                portName = value;
            }
        }

        public void Open()
        {
            portNum = Convert.ToInt32(portName.Replace("COM", ""));
            int ret = 0;
            ret = OpenPort(portNum, portName + ":", baudrate, dataBits,
                           (int)stopBits, (int)parity,
                           readTimeout, writeTimeout);
            if (ret == 1)
            {
                isOpen = true;
                System.Threading.Thread thr = new System.Threading.Thread(new System.Threading.ThreadStart(CheckExistingData));
                thr.IsBackground = true;
                thr.Start();
            }
            else
            {
                throw new Exception("Serial port could not be opened");
            }
        }

        public void Close()
        {
            if (ClosePort(portNum))
            {
                isOpen = false;
            }
        }

        public CESerialPort(String portName, int baudrate)
        {
            if (dllInitialized == false)
            {
                Init();
            }
            this.portName = portName;
            this.baudrate = baudrate;
        }

        private void CheckExistingData()
        {
            while (isOpen)
            {
                try
                {
                    byte[] readBytes = new byte[readBuffSize];

                    int len = Receive(portNum, readBytes, readBuffSize);

                    if (len > 0)
                    {

                        readBuffer += encoding.GetString(readBytes, 0, len);
                        //call data received event
                        if (DataReceived != null)
                        {
                            DataReceived(this);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void Write(byte[] buffer, int offset, int length)
        {
            Send(portNum, buffer, length);
        }

        public void DiscardInBuffer()
        {
            readBuffer = "";
        }
    }
}
