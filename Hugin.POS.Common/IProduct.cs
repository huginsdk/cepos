using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    [Flags]
    public enum ProductStatus
    {
        None,
        Weighable = 1,
        Programmable = 2
    }

    [Flags]
    public enum ProductRequiredField
    {
        None = 0,
        SerialNumber = 1,
        ExpiryDate = 2,
        BatchNubmber = 4,
        All = SerialNumber | ExpiryDate | BatchNubmber
    }
    
    public interface IProduct
    {
        bool Valid { get;}

        int Id { get;}

        String Barcode { get;}

        String Category { get;}

        String Name { get;}

        Department Department { get;}

        Decimal UnitPrice { get;}

        Decimal SecondaryUnitPrice { get;}

        String Unit { get;}

        Decimal Quantity { get;}

        IProduct Clone();

        ProductRequiredField RequiredField { get; }

        ProductStatus Status { get; }

        bool UpdateProduct(string line);

        bool DeleteProduct(IProduct p);
    }
}
