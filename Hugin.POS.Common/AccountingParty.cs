using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hugin.POS.Common
{
    public enum AccountingPartyTag
    {
        TCKN_VKN = 1,
        TITLE,
        FIRST_NAME,
        FAMILY_NAME,
        ROOM,
        BUILDING_NO,
        BUILDING_NAME,
        STREET,
        DISTRICT,
        VILLAGE,
        SUB_CITY,
        CITY,
        COUNTRY,
        POSTAL_CODE,
        TELEPHONE,
        FAX,
        E_MAIL,
        WEB_PAGE,
        TAX_SCHEME
    }
    public class AccountingParty
    {
        string tckn_vkn;
        string title;
        string firstName;
        string familyName;
        string room;
        string buildingNo;
        string buildingName;
        string street;
        string district;
        string village;
        string subCity;
        string city;
        string country;
        string postalCode;
        string telephone;
        string fax;
        string eMail;
        string webPage;
        string taxScheme;

        public string TCKN_VKN
        {
            get { return tckn_vkn; }
        }
        public string Title
        {
            get { return title; }
        }
        public string FirstName
        {
            get { return firstName; }
        }
        public string FamilyName
        {
            get { return familyName; }
        }
        public string Room
        {
            get { return room; }
        }
        public string BuildingNo
        {
            get { return buildingNo; }
        }
        public string BuildingName
        {
            get { return buildingName; }
        }
        public string Street
        {
            get { return street; }
        }
        public string District
        {
            get { return district; }
        }
        public string Village
        {
            get { return village; }
        }
        public string SubCity
        {
            get { return subCity; }
        }
        public string City
        {
            get { return city; }
        }
        public string Country
        {
            get { return country; }
        }
        public string PostalCode
        {
            get { return postalCode; }
        }
        public string Telephone
        {
            get { return telephone; }
        }
        public string Fax
        {
            get { return fax; }
        }
        public string EMail
        {
            get { return eMail; }
        }
        public string WebPage
        {
            get { return webPage; }
        }
        public string TaxScheme
        {
            get { return taxScheme; }
        }

        public AccountingParty()
        {
        }

        public void SetValue(AccountingPartyTag tag, string value)
        {
            switch (tag)
            {
                case AccountingPartyTag.TCKN_VKN:
                    this.tckn_vkn = value;
                    break;
                case AccountingPartyTag.TITLE:
                    this.title = value;
                    break;
                case AccountingPartyTag.FIRST_NAME:
                    this.firstName = value;
                    break;
                case AccountingPartyTag.FAMILY_NAME:
                    this.familyName = value;
                    break;
                case AccountingPartyTag.ROOM:
                    this.room = value;
                    break;
                case AccountingPartyTag.BUILDING_NO:
                    this.buildingNo = value;
                    break;
                case AccountingPartyTag.BUILDING_NAME:
                    this.buildingName = value;
                    break;
                case  AccountingPartyTag.STREET:
                    this.street = value;
                    break;
                case  AccountingPartyTag.DISTRICT:
                    this.district = value;
                    break;
                case AccountingPartyTag.VILLAGE:
                    this.village = value;
                    break;
                case AccountingPartyTag.SUB_CITY:
                    this.subCity = value;
                    break;
                case AccountingPartyTag.CITY:
                    this.city = value;
                    break;
                case AccountingPartyTag.COUNTRY:
                    this.country = value;
                    break;
                case AccountingPartyTag.POSTAL_CODE:
                    this.postalCode = value;
                    break;
                case AccountingPartyTag.TELEPHONE:
                    this.telephone = value;
                    break;
                case AccountingPartyTag.FAX:
                    this.fax = value;
                    break;
                case AccountingPartyTag.E_MAIL:
                    this.eMail = value;
                    break;
                case AccountingPartyTag.WEB_PAGE:
                    this.webPage = value;
                    break;
                case AccountingPartyTag.TAX_SCHEME:
                    this.taxScheme = value;
                    break;
            }
        }
    }
}
