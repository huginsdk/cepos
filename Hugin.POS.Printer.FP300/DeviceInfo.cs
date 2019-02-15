using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Printer
{

    public enum IPProtocol
    {
        IPV4 = 1,
        IPV6 = 2
    }

    public class DeviceInfo
    {
        private IPProtocol devIPProtocol = 0;
        private System.Net.IPAddress devIP = null;
        private int devPort = 0;
        private String devModel = "";
        private String devBrand = "";
        private String terminalNo = "";


        public DeviceInfo()
        {
            devIPProtocol = IPProtocol.IPV4;
            devIP = System.Net.IPAddress.Any;
        }

        public IPProtocol DevIPProtocol
        {
            get { return devIPProtocol; }
            set { devIPProtocol = value; }
        }

        public System.Net.IPAddress DevIP
        {
            get { return devIP; }
            set { devIP = value; }
        }

        public int DevPort
        {
            get { return devPort; }
            set { devPort = value; }
        }

        public String TerminalNo
        {
            get { return terminalNo; }
            set { terminalNo = value; }
        }

        public String DevModel
        {
            get { return devModel; }
            set { devModel = value; }
        }

        public String DevBrand
        {
            get { return devBrand; }
            set { devBrand = value; }
        }
    }
}
