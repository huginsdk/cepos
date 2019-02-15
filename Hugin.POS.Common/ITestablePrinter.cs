using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface ITestablePrinter
    {
        IPrinterResponse CheckEJ();
    }
}
