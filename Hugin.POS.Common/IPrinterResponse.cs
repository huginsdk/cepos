using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface IPrinterResponse
    {
        bool HasError { get;}

        string Data { get; set;}

        String Detail { get; set;}
    }
}
