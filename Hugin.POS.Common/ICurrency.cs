using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface ICurrency
    {
        int Id { get;}
        String Name { get;}
        Decimal ExchangeRate { get;}
    }
}
