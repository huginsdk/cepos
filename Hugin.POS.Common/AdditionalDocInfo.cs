using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hugin.POS.Common
{
    public enum DocProfilID
    {
        TEMEL_FATURA,
        TICARI_FATURA
    }

    public enum DocCurrencyCode
    {
        LIRA,
        DOLLAR,
        EURO,
        POUND
    }
    public class AdditionalDocInfo
    {
        private DocProfilID profilID = DocProfilID.TEMEL_FATURA;
        private DocCurrencyCode currencyCode = DocCurrencyCode.LIRA;
        private AccountingParty customerParty;

        public DocProfilID ProfilID
        {
            get { return profilID; }
            set { profilID = value; }
        }

        public DocCurrencyCode CurrencyCode
        {
            get { return currencyCode; }
            set { currencyCode = value; }
        }

        public AccountingParty CustomerParty
        {
            get { return customerParty; }
            set { customerParty = value; }
        }

        public AdditionalDocInfo()
        {
        }
    }
}
