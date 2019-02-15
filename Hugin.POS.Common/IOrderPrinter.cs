using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface IOrderPrinter
    {
        void Connect(String address);
        void Print(ISalesDocument document);
    }
}
