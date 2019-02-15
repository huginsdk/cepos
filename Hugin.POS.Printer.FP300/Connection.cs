
using System;
using System.Collections.Generic;
using System.Text;
#if !MONO
using System.IO.Ports;
#endif
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    enum ContentType
    {
        NONE,
        REPORT,
        FILE
    }

    public interface IConnection
    {
        void Open();
        //FPUResponse Send(FPURequest request);
        void Close();
        bool IsOpen { get; }
        int FPUTimeout { get; set; }
        Encoding DefaultEncoding { get; set; }
        Object ToObject();
    }
#if !MONO
    public class SerialConnection : IConnection
    {
        private string portName = String.Empty;
        private int baudRate = 115200;
        private SerialPort sp = null;
        private Encoding encoding = Encoding.GetEncoding(1254);

        public SerialConnection(string portName, int baudrate)
        {
            this.portName = portName;
            this.baudRate = baudrate;
        }

        public void Open()
        {
            sp = new SerialPort(portName, baudRate);
            sp.WriteTimeout = 4600;
            sp.ReadTimeout = 4600;
            sp.ReadBufferSize = 40000;
            sp.Encoding = encoding;
            sp.Open();
        }

        public bool IsOpen
        {
            get 
            {
                if (sp!= null && sp.IsOpen)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Close()
        {
            if (sp != null && sp.IsOpen)
            {
                sp.Close();
            }
        }

        public int FPUTimeout
        {
            get
            {
                return sp.ReadTimeout;
            }
            set
            {
                sp.ReadTimeout = value;
            }
        }

        public Encoding DefaultEncoding
        {
            get
            {
                return encoding;
            }
            set
            {
                encoding = value;
            }
        }

        public Object ToObject()
        {
            return sp;
        }
    }
#endif

    public class TCPConnection : IConnection,IDisposable
    {
        private Socket client = null;
        private string ipAddress = String.Empty;
        private int port = 0;
        private Encoding encoding = Encoding.GetEncoding(1252);

        public TCPConnection(String ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }
        // Destructor of this class.
        ~ TCPConnection()
        {
            Dispose();
        }

        public void Open()
        {
            if (IsOpen)
                this.Close();

            client = null;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(this.ipAddress), this.port);
            client = new Socket(AddressFamily.InterNetwork,
                              SocketType.Stream, ProtocolType.Tcp);
            client.ReceiveTimeout = 4500;
            client.Connect(ipep);

        }

        private static List<byte> detail = new List<byte>();
        public bool IsOpen
        {
            get
            {
                if (client != null && client.Connected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Close()
        {
            //if (IsOpen)
            //{
            //    client.Shutdown(SocketShutdown.Both);
            //    client.Close();
            //}

            try
            {
                client.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }     
        }

        public int FPUTimeout
        {
            get
            {
                return client.ReceiveTimeout;
            }
            set
            {
                client.ReceiveTimeout = value;
            }
        }
        public Encoding DefaultEncoding
        {
            get
            {
                return encoding;
            }
            set
            {
                encoding = value;
            }
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (System.Exception)
            {
            	
            }
        }

        public Object ToObject()
        {
            return client;
        }
    }
}
