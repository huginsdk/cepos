using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface ICredit
    {
        int Id { get;}
        String Name { get;}
        bool IsTicket { get;}
        bool PayViaEft { get;}
        bool IsPointPayment { get; }
    }
}
