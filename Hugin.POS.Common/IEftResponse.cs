using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface IEftResponse
    {
        string CardNumber { get;}
        string AuthorizationNumber { get;}
        bool HasError { get;}
    }
}
