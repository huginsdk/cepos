using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    /// <summary>
    /// Cashier authorization levels
    /// </summary>
    public enum AuthorizationLevel
    {
        O, X, Z, S, P, Seller
    }
    public interface ICashier
    {
        String Id { get;}

        String Name { get; }

        String Password { get; set; }

        AuthorizationLevel AuthorizationLevel { get;}

        bool Valid { get; }

        int PercentAdjustmentLimit { get;}

        Decimal PriceAdjustmentLimit { get;}

        bool IsAuthorisedFor(IAdjustment adjustment);

        bool GenerateCashierLine(string name, string id, AuthorizationLevel auth,
                                string passcode, int disPercent, Decimal disAmount, 
                                bool valid, out string line);

        bool UpdateCashier(string line);

        bool DeleteCashier(ICashier c);


    }
}
