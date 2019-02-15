using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
    class AdditionalInfoMenu : List
    {
        private static IState state = new AdditionalInfoMenu();

        private static StateInstance retState;

        public static new IState Instance()
        {
            if (retState != null)
                return Instance(retState);
            else
                return Instance(States.Start.Instance);
        }

        public static IState Instance(StateInstance retCancel)
        {
            MenuList menuHeaders = CreateMenu();
            ie = menuHeaders;
            retState = retCancel; 
            List.Instance(menuHeaders, (ProcessSelectedItem)null, retCancel);

            FillCustomerInfo();

            return state;
        }

        public static IState Continue()
        {
            ie.MovePrevious();
            List.Instance(ie);
            return state;
            //return Instance();
        }

        private static void FillCustomerInfo()
        {
            if (cr.Document.Customer != null)
            {
                if (cr.Document.AdditionalInfo == null)
                {
                    cr.Document.AdditionalInfo = new AdditionalDocInfo();
                    cr.Document.AdditionalInfo.CustomerParty = new AccountingParty();
                }
                else if (cr.Document.AdditionalInfo.CustomerParty == null)
                    cr.Document.AdditionalInfo.CustomerParty = new AccountingParty();

                // TCKN-VKN
                if (!String.IsNullOrEmpty(cr.Document.Customer.Contact[4]))
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.TCKN_VKN, cr.Document.Customer.Contact[4]);

                // NAME/TITLE
                if(!String.IsNullOrEmpty(cr.Document.Customer.Name))
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(cr.Document.Customer.Contact[4].Length == 10 ? AccountingPartyTag.TITLE : AccountingPartyTag.FIRST_NAME,
                        cr.Document.Customer.Name);

                // TAX INSTITUTION
                if (!String.IsNullOrEmpty(cr.Document.Customer.Contact[3]))
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.TAX_SCHEME, cr.Document.Customer.Contact[3]);
            }
        }

        public static MenuList CreateMenu()
        {
            MenuList menuList = new MenuList();

            string labelFormat = "{0}\t{1}\n{2}";

            int index = 1;
            string label = String.Format(labelFormat, PosMessage.ADDITIONAL_INFO, index++, PosMessage.CUSTOMER_INFO);
            menuList.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.ADDITIONAL_INFO, index++, PosMessage.ADDRESS_INFO);
            menuList.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.ADDITIONAL_INFO, index++, PosMessage.CONTACT_INFO);
            menuList.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.ADDITIONAL_INFO, index++, PosMessage.INVOICE_PROFIL);
            menuList.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.ADDITIONAL_INFO, index++, PosMessage.CLEAR_INPUTS);
            menuList.Add(new MenuLabel(label));

            if (PosConfiguration.EDocumentAutoFill)
            {
                label = String.Format(labelFormat, PosMessage.ADDITIONAL_INFO, index++, PosMessage.FILL_AUTO);
                menuList.Add(new MenuLabel(label));
            }

            return menuList;
        }

        public override void Enter()
        {
            string message = ((MenuLabel)ie.Current).ToString();
            message = message.Substring(message.IndexOf('\n') + 1);
            switch (message)
            {
                case PosMessage.CUSTOMER_INFO:
                case PosMessage.ADDRESS_INFO:
                case PosMessage.CONTACT_INFO:
                case PosMessage.INVOICE_PROFIL:
                    cr.State = CreateMenuHeaders(message);
                    break;
                case PosMessage.CLEAR_INPUTS:
                    cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.APPLY_CLEAR_ADD_INFO, new StateInstance(ClearInputs), Continue));
                    break;
                case PosMessage.FILL_AUTO:
                    FillAdditionalInfo();
                    Continue();
                    break;
            }
            //ie.MovePrevious();
        }

        public override void Escape()
        {
            ie = null;
            if (retState == null)
                base.Escape();
            else
                cr.State = retState();
            retState = null;
        }

        private static string sMenuType = "";
        private static IState CreateMenuHeaders(string menuType)
        {
            sMenuType = menuType;

            if (cr.Document.AdditionalInfo == null)
            {
                cr.Document.AdditionalInfo = new AdditionalDocInfo();
                cr.Document.AdditionalInfo.CustomerParty = new AccountingParty();
            }
            else if (cr.Document.AdditionalInfo.CustomerParty == null)
                cr.Document.AdditionalInfo.CustomerParty = new AccountingParty();

            MenuList menuList = GetMenuHeaders(menuType);
            return States.List.Instance(menuList, new ProcessSelectedItem(EnterCustomerInfo), new StateInstance(Instance));
        }

        private static MenuList GetMenuHeaders(string menuType)
        {
            MenuList menuList = new MenuList();

            string labelFormat = "{0}-{1}\n{2}";
            string label = "";
            int index = 1;

            switch(menuType)
            {
                case PosMessage.CUSTOMER_INFO:
                    label = String.Format(labelFormat, index++, PosMessage.CI_TITLE, cr.Document.AdditionalInfo.CustomerParty.Title == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.Title);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_NAME, cr.Document.AdditionalInfo.CustomerParty.FirstName == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.FirstName);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_FAMILY_NAME, cr.Document.AdditionalInfo.CustomerParty.FamilyName == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.FamilyName);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_TAX_SCHEME, cr.Document.AdditionalInfo.CustomerParty.TaxScheme == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.TaxScheme);
                    menuList.Add(new MenuLabel(label));
                    break;
                case PosMessage.ADDRESS_INFO:
                    label = String.Format(labelFormat, index++, PosMessage.CI_ROOM, cr.Document.AdditionalInfo.CustomerParty.Room == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.Room);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_BUILDING_NO, cr.Document.AdditionalInfo.CustomerParty.BuildingNo == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.BuildingNo);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_BUILDING_NAME, cr.Document.AdditionalInfo.CustomerParty.BuildingName == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.BuildingName);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_STREET, cr.Document.AdditionalInfo.CustomerParty.Street == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.Street);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_DISTRICT, cr.Document.AdditionalInfo.CustomerParty.District == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.District);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_VILLAGE, cr.Document.AdditionalInfo.CustomerParty.Village == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.Village);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_SUBCITY, cr.Document.AdditionalInfo.CustomerParty.SubCity == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.SubCity);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_CITY, cr.Document.AdditionalInfo.CustomerParty.City == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.City);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_COUNTRY, cr.Document.AdditionalInfo.CustomerParty.Country == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.Country);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_POSTAL_CODE, cr.Document.AdditionalInfo.CustomerParty.PostalCode == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.PostalCode);
                    menuList.Add(new MenuLabel(label));
                    break;

                case PosMessage.CONTACT_INFO:
                    label = String.Format(labelFormat, index++, PosMessage.CI_TELEPHONE, cr.Document.AdditionalInfo.CustomerParty.Telephone == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.Telephone);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_FAX, cr.Document.AdditionalInfo.CustomerParty.Fax == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.Fax);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_EMAIL, cr.Document.AdditionalInfo.CustomerParty.EMail == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.EMail);
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.CI_WEB_PAGE, cr.Document.AdditionalInfo.CustomerParty.WebPage == null ?
                        "" : cr.Document.AdditionalInfo.CustomerParty.WebPage);
                    menuList.Add(new MenuLabel(label));
                    break;

                case PosMessage.INVOICE_PROFIL:
                    label = String.Format(labelFormat, index++, PosMessage.BASIC_INVOICE, "");
                    menuList.Add(new MenuLabel(label));

                    label = String.Format(labelFormat, index++, PosMessage.TRADING_INVOICE, "");
                    menuList.Add(new MenuLabel(label));
                    break;
            }
            return menuList;
        }

        private static string lastLabel = "";
        private static void EnterCustomerInfo(Object menu)
        {
            string message = ((MenuLabel)menu).ToString();
            string strDefault = message.Substring(message.IndexOf('\n') + 1);

            int tmp = message.IndexOf('\n');
            string label = message.Substring(message.IndexOf('-') + 1, tmp - message.IndexOf('-') - 1);
            lastLabel = label;

            if (label == PosMessage.BASIC_INVOICE || label == PosMessage.TRADING_INVOICE)
            {
                cr.Document.AdditionalInfo.ProfilID = label == PosMessage.BASIC_INVOICE ? DocProfilID.TEMEL_FATURA : DocProfilID.TICARI_FATURA;
                cr.State = Instance();
            }
            else
                cr.State = States.EnterString.Instance(label, strDefault, new StateInstance<string>(SetCustomerInfo), Continue);
        }
        private static IState SetCustomerInfo(string value)
        {
            switch (lastLabel)
            {
                case PosMessage.CI_TITLE:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.TITLE, value);
                    break;
                case PosMessage.CI_NAME:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.FIRST_NAME, value);
                    break;
                case PosMessage.CI_FAMILY_NAME:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.FAMILY_NAME, value);
                    break;
                case PosMessage.CI_TAX_SCHEME:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.TAX_SCHEME, value);
                    break;
                case PosMessage.CI_ROOM:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.ROOM, value);
                    break;
                case PosMessage.CI_BUILDING_NO:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.BUILDING_NO, value);
                    break;
                case PosMessage.CI_BUILDING_NAME:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.BUILDING_NAME, value);
                    break;
                case PosMessage.CI_STREET:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.STREET, value);
                    break;
                case PosMessage.CI_DISTRICT:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.DISTRICT, value);
                    break;
                case PosMessage.CI_VILLAGE:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.VILLAGE, value);
                    break;
                case PosMessage.CI_SUBCITY:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.SUB_CITY, value);
                    break;
                case PosMessage.CI_CITY:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.CITY, value);
                    break;
                case PosMessage.CI_COUNTRY:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.COUNTRY, value);
                    break;
                case PosMessage.CI_POSTAL_CODE:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.POSTAL_CODE, value);
                    break;
                case PosMessage.CI_TELEPHONE:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.TELEPHONE, value);
                    break;
                case PosMessage.CI_FAX:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.FAX, value);
                    break;
                case PosMessage.CI_EMAIL:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.E_MAIL, value);
                    break;
                case PosMessage.CI_WEB_PAGE:
                    cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.WEB_PAGE, value);
                    break;
            }

            return CreateMenuHeaders(sMenuType);
        }

        private static IState ClearInputs()
        {
            if (cr.Document.AdditionalInfo != null)
                cr.Document.AdditionalInfo = new AdditionalDocInfo();

            DisplayAdapter.Cashier.Show(PosMessage.INPUTS_CLEAR);
            System.Threading.Thread.Sleep(1000);

            return Continue();
        }

        private void FillAdditionalInfo()
        {
            if (cr.Document.AdditionalInfo == null)
            {
                cr.Document.AdditionalInfo = new AdditionalDocInfo();
                cr.Document.AdditionalInfo.CustomerParty = new AccountingParty();
            }
            else if (cr.Document.AdditionalInfo.CustomerParty == null)
                cr.Document.AdditionalInfo.CustomerParty = new AccountingParty();


            // CUSTOMER INFO
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.TITLE, "GÖKCEN HALI KİLİM TRAVEL ŞİRKETLER GRUBU ");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.FIRST_NAME, "SERDAR");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.FAMILY_NAME, "GOKCEN");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.TAX_SCHEME, "GALATA");

            // ADDRESS INFO
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.ROOM, "8");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.BUILDING_NO, "4");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.BUILDING_NAME, "KAYA APARTMANI");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.STREET, "YILDIZ SOKAK");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.DISTRICT, "IZZETPAŞA MAHALLESİ");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.VILLAGE, "MECIDIYEKOY");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.SUB_CITY, "ŞİŞLİ");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.CITY, "İSTANBUL");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.COUNTRY, "TÜRKİYE");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.POSTAL_CODE, "34123");

            // CONTACT INFO
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.TELEPHONE, "5065727403");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.FAX, "2123658478");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.E_MAIL, "serdar.gokcen@hugin.com.tr");
            cr.Document.AdditionalInfo.CustomerParty.SetValue(AccountingPartyTag.WEB_PAGE, "www.hugin.com.tr");
        }
    }
}
