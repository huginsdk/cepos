using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface IConnectable
    {
        Boolean IsConnect { get;}
        String Address { get;}

        void Connect(String address);
        void Disconnect();
    }
}
