using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{

    //public enum IPProtocol
    //{
    //    IPV4 = 1,
    //    IPV6 = 2
    //}

#region EXCEPTION
    public class CryptoException : PosException
    {
        public Byte[] Context;
        public CryptoException(byte[] context)
            : base("Report could not be completed")
        {
            this.Context = context;
        }

        public CryptoException(String message)
            : base(message) { }

        public CryptoException(String message, Exception innerException)
            : base(message, innerException) { }

    }
#endregion

    public enum State
    {
        IDLE = 1,
        SELLING,
        SUBTOTAL,
        PAYMENT,
        OPEN_SALE,
        INFO_RCPT,
        CUSTOM_RCPT,
        IN_SERVICE,
        SRV_REQUIRED,
        LOGIN,
        NONFISCAL,
        ON_PWR_RCOVR,
        INVOICE,
        CONFIRM_REQUIRED
    }

    public class ProgramConfig
    {
        public const int MAX_MAIN_CATEGORY_COUNT = 50;
        public const int MAX_CASHIER_COUNT = 10;
    }

    public class FPUCashier : ICashier
    {
        string id;
        string name;
        string password;
        AuthorizationLevel level;

        public string Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                
            }
        }

        public AuthorizationLevel AuthorizationLevel
        {
            get { return level; }
        }

        public bool Valid
        {
            get { throw new NotImplementedException(); }
        }

        public int PercentAdjustmentLimit
        {
            get { throw new NotImplementedException(); }
        }

        public decimal PriceAdjustmentLimit
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsAuthorisedFor(IAdjustment adjustment)
        {
            throw new NotImplementedException();
        }

        public bool GenerateCashierLine(string name, string id, AuthorizationLevel auth, string passcode, int disPercent, decimal disAmount, bool valid, out string line)
        {
            throw new NotImplementedException();
        }

        public bool UpdateCashier(string line)
        {
            throw new NotImplementedException();
        }

        public bool DeleteCashier(ICashier c)
        {
            throw new NotImplementedException();
        }

        public FPUCashier(string id, string name, string password, AuthorizationLevel level)
        {
            this.id = id;
            this.name = name;
            this.password = password;
            this.level = level;
        }
    }
}
