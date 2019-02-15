using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.PromotionServer
{
    internal class AccessViolationException : Exception
    {
        internal AccessViolationException()
            : base()
        {
        }
        internal AccessViolationException(String message)
            : base(message)
        {
        }
    }
    internal class InvalidDataException : Exception
    {
        internal InvalidDataException()
            : base()
        {
        }
        internal InvalidDataException(String message)
            : base(message)
        {
        }
    }
}
