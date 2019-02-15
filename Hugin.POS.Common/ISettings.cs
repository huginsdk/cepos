using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface ISettings
    {
        String FileName { get;}

        String IdleMessage { get;}

        String[] LogoLines { get;}

        Department[] Departments { get;}
        Decimal[] TaxRates { get;}

        String[] CreditLines { get;}

        String[] PLUPageLines { get;}

        Decimal ReceiptLimit { get;}
        /// <summary>
        /// Gets invoice coordinates. Contains two items.
        /// Each item showes different points.
        /// First item showes time,date,customer name/address ,taxinstition/taxnumber points.
        /// Second item showes product start line, product name, product quantity, VAT, unit price points.
        /// </summary>
        /// <example>
        /// <list type="bullet">
        /// <item><description>First item value  : 7002,7001,3001,3007</description></item>
        /// <item><description>Second item value : 3012,45,77,61,050,0</description></item>
        /// </list>
        /// </example>
        String[] InvoiceCoordinates { get;}
        String[] DocumentRemarks { get;}

        Dictionary<int,String> CharMatrix { get;}
        Dictionary<int, String> LabelMatrix { get;}

        AuthorizationLevel GetAuthorizationLevel(Authorizations operation);
        int GetProgramOption(Setting optionId);

        String GetSpecialBarcode(String key);

        AccountingParty SupplierInfo { get; }

    }
}
