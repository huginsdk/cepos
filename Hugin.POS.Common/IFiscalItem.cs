using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface IFiscalItem
    {
        String Name { get;set;}

        String Barcode { get;set;}

        int TaxGroupId { get;}//Product.Department.TaxGroupId

        String Unit { get;set;}

        Decimal UnitPrice { get;set;}

        Decimal Quantity { get;set;}

        Decimal TotalAmount { get;set;}

        Decimal ListedAmount { get;}

        /// <summary>
        /// Returns: Amount | Percent (if type not percentage then uses '--' ) | CashierId
        /// </summary>
        /// <returns></returns>
        String[] GetAdjustments();

        ICashier SalesPerson { get; }

        IProduct Product { get; }

        Decimal VoidAmount { get; set; }
        
        Decimal VoidQuantity { get; set; }

        String SerialNo { get; set; }

        DateTime ExpiryDate { get; set; }

        String BatchNumber { get; set; }
    }
}
