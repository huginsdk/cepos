using System;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using System.Collections.Generic;
using System.Threading;


namespace Hugin.POS.States
{
    class CustomerInfo:List
    {

        private static IState state = new CustomerInfo();       
        private static StringBuilder customerInput = new StringBuilder();
        //private static Customer newCustomer = new Customer();
        private static StateInstance ReturnConfirm;
        public static  new StateInstance ReturnCancel;
        static string cardNumber = String.Empty;
        
        /// <summary>
        /// Instance of CustomerInfo.
        /// Like a command menu.
        /// Shows list of Customer menu istems.
        /// </summary>
        /// <returns>CustomerInfo state.</returns>
        public static new IState Instance()
        {

            cr.SecurityConnector.CustomerCaptured += new EventHandler(SecurityConnector_CustomerCaptured);

            MenuList menuHeaders = new MenuList();

            if (cr.Document is Receipt)                
                menuHeaders.Add(new MenuLabel(PosMessage.ENTER_CADR_CODE));

            if (cr.Document is Invoice ||
                cr.Document is EInvoice ||
                cr.Document is EArchive ||
                cr.Document is Advance ||
                cr.Document is CurrentAccountDocument ||
                cr.Document is SelfEmployementInvoice)
                menuHeaders.Add(new MenuLabel(PosMessage.ENTER_TCKN_VKN_MENU));

            if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.CustomerSearch) == 0)
            {

                menuHeaders.Add(new MenuLabel(PosMessage.SEARCH_RECORD));
                menuHeaders.Add(new MenuLabel(PosMessage.NEW_RECORD));
            }
            menuHeaders.Add(new MenuLabel(PosMessage.RETURN_TO_SELLING));
            
            List.Instance(menuHeaders);
            return state;
            //Musteri ile ilgili yapilacak islemer menusunu gosterir         
        }

        static void SecurityConnector_CustomerCaptured(object sender, EventArgs e)
        {
            ICustomer customer = sender as ICustomer;
            CustomerInfo.SetCardCustomer(customer);
            cr.SecurityConnector.CustomerCaptured -= new EventHandler(SecurityConnector_CustomerCaptured);
        }

        public override void Numeric(char c)
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IState Continue()
        {
            ie.MovePrevious();
            List.Instance(ie);
            return state;
        }
        /// <summary>
        /// used to assign return and cancel state.
        /// </summary>
        /// <param name="confirmState">
        /// what state the machine should assume after user hits Enter key
        /// </param>
        /// <param name="cancelState">
        /// what state the machine should assume after user hits Escape key
        /// </param>
        /// <returns></returns>
        public static IState Instance(StateInstance confirmState,StateInstance cancelState)
        {            
            ReturnConfirm = confirmState;
            ReturnCancel = cancelState;
            return Instance();
        }
        /// <summary>
        /// jumps next menu item after user hits Customer key.
        /// </summary>
        public override void Customer()
        {
            base.DownArrow();
        }     
        /// <summary>
        /// process selected item after user select a menu item from list and press Enter key.
        /// </summary>
        public override void  Enter()
       {
            //CustomerMenu scit = o as CustomerMenu;
           if (!(ie.Current is MenuLabel)) { ReturnToHome(cr.Document.Customer); return; }
            switch (((MenuLabel)ie.Current).ToString())
            {
                case PosMessage.ENTER_CADR_CODE:
                    cr.State = States.EnterString.Instance(PosMessage.ENTER_NUMBER,
                                                            new StateInstance<String>(SetCurrentCustomer),
                                                            new StateInstance(Continue));
                    break;
                case PosMessage.ENTER_TCKN_VKN_MENU:
                    cr.State = States.EnterString.Instance(PosMessage.ENTER_TCKN_VKN,
                                                            new StateInstance<String>(SetCurrentCustomerByTCKN),
                                                            new StateInstance(Continue));
                    break;
                case PosMessage.SEARCH_RECORD:
                    cr.State = States.EnterString.Instance(PosMessage.SEARCH_QUERY,
                                                            new StateInstance<String>(FindCustomers),
                                                            new StateInstance(Continue));
                    break;
                case PosMessage.NEW_RECORD:
                    if (cr.IsAuthorisedFor(Authorizations.Programing))
                    {
                        if (cr.IsDesktopWindows)
                        {

                            cr.State = States.EnterString.Instance(PosMessage.CUSTOMER_CODE,
                                                                 new StateInstance<String>(EnterCustomerCode),
                                                                 new StateInstance(Continue));
                        }
                        else
                        {
                            cr.State = EnterCustomerCode("");
                        }
                    }
                    else
                        throw new CashierAutorizeException();
                    break;
                case PosMessage.VOID_CUSTOMER:
                    cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_VOID_CUSTOMER,
                                                            new StateInstance(ConfirmVoidCustomer),
                                                            new StateInstance(Continue)));
                    break;
                case PosMessage.RETURN_TO_SELLING:
                    if (ReturnConfirm == null)
                        cr.State = States.Start.Instance();
                    else
                        cr.State = ReturnConfirm();
                    break;
                default:
                    if (ReturnConfirm != null)
                    {
                        cr.State = ReturnConfirm();
                        ReturnConfirm = null;
                        ReturnCancel = null;
                    }
                    else
                    {
                        if (cr.IsDesktopWindows && !(cr.Document is Receipt))
                            cr.State = States.DocumentPaymentStatus.Instance(States.Start.Instance, States.Start.Instance);
                        else
                        {
                            if (cr.Document.Customer.DefaultDocumentType != DocumentTypes.NULL)
                            {
                                DocumentTypes type = DocumentTypes.RECEIPT;
                                switch (cr.Document.Customer.DefaultDocumentType)
                                {
                                    case DocumentTypes.INVOICE:
                                        type = DocumentTypes.INVOICE;
                                        break;
                                    case DocumentTypes.E_INVOICE:
                                        type = DocumentTypes.E_INVOICE;
                                        break;
                                    case DocumentTypes.E_ARCHIEVE:
                                        type = DocumentTypes.E_ARCHIEVE;
                                        break;
                                }
                                cr.ChangeDocumentType(cr.ChangeDocumentType(cr.Document, type));
                            }
                            else
                                cr.State = States.Start.Instance();
                        }
                    }
                    break;

            }
        }
        /// <summary>
        /// Each customer have some sub menu items.
        /// For example:Address,TaxNumber etc.
        /// If user searh customer, may be more than one customer can be found so
        /// use up and down key to select correct customer. After correct customer selected
        /// press Enter key to list sub menu items selected customer's.
        /// So this function used to list sub menu items of selected customer.
        /// </summary>
        /// <param name="c">
        /// Selected customer.
        /// </param>
        /// <returns>
        /// CustomerInfo State.
        /// </returns>
        public static IState SetCardCustomer(ICustomer c)
        {
            cr.Document.Customer = c;
            cr.SecurityConnector.AcceptCustomer(c.Number);
            try
            {
                return SetCustomerInfo(c);
            }
            catch (MissingCardInfoException)
            {
                MenuList menuheaders = new MenuList();
                menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.NAME_FIRM, c.Name)));
                menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.CUSTOMER_NUMBER, c.Number)));
                menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.CUSTOMER_CODE, c.Code)));
                List.Instance(menuheaders);
                return state;
            }
        }
        public static IState SetCustomerInfo(ICustomer c)
        {
            if (cr.Document.TcknVkn != null)
                return ReturnConfirm();

            string defDocType = PosMessage.RECEIPT_TR;
            switch(c.DefaultDocumentType)
            {
                case DocumentTypes.INVOICE:
                    defDocType = PosMessage.INVOICE;
                    break;
                case DocumentTypes.E_INVOICE:
                    defDocType = PosMessage.E_INVOICE;
                    break;
                case DocumentTypes.E_ARCHIEVE:
                    defDocType = PosMessage.E_ARCHIVE;
                    break;
            }
            
            MenuList menuheaders = new MenuList();
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.NAME_FIRM, c.Name)));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.CUSTOMER_NUMBER, c.Number)));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.CUSTOMER_GROUP, c.CustomerGroup)));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.CUSTOMER_CODE, c.Code)));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.ADDRESS, c.Contact[0])));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.ADDRESS, c.Contact[1])));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.ADDRESS, c.Contact[2])));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.TAXOFFICE, c.Contact[3])));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.TAX_NUMBER, c.Contact[4])));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1}", PosMessage.DEFAULT_DOCUMENT_TYPE, defDocType)));
            menuheaders.Add(new MenuLabel(String.Format("{0}\n{1} ({2} {3})",
                                                            PosMessage.CUSTOMER_POINT,
                                                            c.Points,
                                                            cr.Document.PointPrices(c.Points),
                                                            PosMessage.TURKISH_LIRA)
                                                            ));
            if (cr.IsDesktopWindows)
            {
                if (c.Points > 0)
                {
                    DisplayAdapter.Customer.Show("{0}\n{1} : {2}", c.Name, PosMessage.POINT, c.Points);
                }
                else
                {
                    DisplayAdapter.Customer.Show("{0}\n{1}", c.Name, c.Number);
                }
            }

            List.Instance(menuheaders);
            
            return state;
        }
        /// <summary>
        /// return to start state or current state
        /// </summary>
        /// <param name="o">
        /// never used in this function
        /// </param>
        public static void ReturnToHome(Object o) 
        {
            if(ReturnCancel!=null)
             cr.State = ReturnCancel();            
            else
             cr.State = States.Start.Instance();
        }
        /// <summary>
        /// used to set current customer with foud customer
        /// </summary>
        /// <param name="o">
        /// Customer object
        /// </param>
        public static void SetCurrentCustomer(Object o)
        {
            ICustomer customer = o as ICustomer;

            if((customer.DefaultDocumentType == DocumentTypes.E_INVOICE || customer.DefaultDocumentType == DocumentTypes.E_ARCHIEVE) &&
                cr.Document.DocumentTypeId == (int)DocumentTypes.INVOICE)
            {
                cr.State = States.AlertCashier.Instance(new Confirm("CARÝ E-BELGE MÜÞTERÝSÝDÝR", Instance, Instance));
                return;
            }

            cr.Document.Customer = customer;
            cr.SecurityConnector.AcceptCustomer(cr.Document.Customer.Number);
            cr.State = SetCustomerInfo(cr.Document.Customer);
        }
      
        /// <summary>
        /// search customer by customer card number and set it to current customer.
        /// </summary>
        /// <param name="cardNo">
        /// customer card number.
        /// </param>
        /// <returns>
        /// returns a confirm state to set found customer.
        /// </returns>
        public static IState SetCurrentCustomer(String cardNo)
        {
            // if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.WebServicePromotion) == 0)

            cr.Document.Customer = null;
            string line = String.Empty;

            cardNumber = cardNo;

            try
            {
                if (cr.State is States.EnterCardNumber)
                    cardNo = "C" + cardNo;//means that card has been used
                System.Threading.Thread.Sleep(25);

                if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.WebServicePromotion) == 0)
                {
                    ICustomer c = cr.DataConnector.FindCustomerByCardNo(cardNo);
                    if (c == null)
                        c = cr.DataConnector.FindCustomerByCode(cardNo);

                    if (c != null)
                        cr.Document.Customer = c;
                }
                else
                {
                    line = cr.PromoClient.SearchCustomer(cardNo);
                }
            }
            catch (Exception e) 
            { 
                cr.Log.Warning(e); 
            }

            // To avoid null response
            if (line == null)
            {
                line = String.Empty;
            }
            if (cr.Document.Customer == null)
            {
                if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.WebServicePromotion) == 0)
                {
                    Confirm err = new Confirm(PosMessage.CUSTOMER_NOT_FOUND,
                        new StateInstance(Instance),
                        new StateInstance(Instance));
                    return States.AlertCashier.Instance(err);
                }
                else
                {
                    cr.Log.Debug("Promotion Server customer search  response: {0}", line);
                    if (line != String.Empty)
                    {
                        switch (line[0])
                        {
                            case '1':
                                return AssignCustomer(line);
                            case '2':
                                return AskGSMNumber();
                        }
                    }
                    else
                    {
                        return States.Start.Instance();
                    }
                }
            }
            else
            {
                cr.SecurityConnector.AcceptCustomer(cr.Document.Customer.Number);
            }

            return SetCustomerInfo(cr.Document.Customer);
        }

        public static IState SetCurrentCustomerByTCKN(String tcknVkn)
        {
            if (String.IsNullOrEmpty(tcknVkn))
            {
                tcknVkn = "1111111111";
            }

            if (tcknVkn.Trim().Length < 10 ||
                tcknVkn.Trim().Length > 11)
            {
                DisplayAdapter.Cashier.Show(PosMessage.INVALID_ENTRY);
                System.Threading.Thread.Sleep(1500);
                return States.EnterString.Instance(PosMessage.ENTER_TCKN_VKN,
                                                            new StateInstance<String>(SetCurrentCustomerByTCKN),
                                                            new StateInstance(Continue));
            }
            cr.Document.Customer = null;
            cr.Document.TcknVkn = null;
            tcknVkn = tcknVkn.PadRight(15, ' ');
            try
            {
                cr.Document.Customer = cr.DataConnector.FindCustomerByTcknVkn(tcknVkn);
            }
            catch (Exception e)
            {
                cr.Log.Warning(e);
            }

            if (cr.Document.Customer == null)
            {
                if (cr.Document is Invoice ||
                    cr.Document is EInvoice ||
                    cr.Document is EArchive || 
                    cr.Document is Advance ||
                    cr.Document is CurrentAccountDocument ||
                    cr.Document is SelfEmployementInvoice)
                {
                    cr.Document.TcknVkn = tcknVkn;
                }
                else
                {
                    Confirm err = new Confirm(PosMessage.CUSTOMER_NOT_FOUND,
                        new StateInstance(Instance),
                        new StateInstance(Instance));
                    return States.AlertCashier.Instance(err);
                }
            }
            else
            {
                cr.SecurityConnector.AcceptCustomer(cr.Document.Customer.Number);
            }

            return SetCustomerInfo(cr.Document.Customer);
        }
    

        private static IState AssignCustomer(string line)
        {
            cr.Document.Customer = cr.DataConnector.SaveCustomer(line);
            cr.SecurityConnector.AcceptCustomer(cr.Document.Customer.Number);
            return SetCustomerInfo(cr.Document.Customer);
        }

        private static IState AskGSMNumber()
        {
            return States.EnterDecimal.Instance("GSM NUMARASI",
                                                    new StateInstance<Decimal>(CreateDefaultCustomer),
                                                    new StateInstance(Continue));
        }

        private static IState CreateDefaultCustomer(decimal gsm)
        {
            string gsmNumber = gsm.ToString();
            if (gsmNumber.StartsWith("5") && gsmNumber.Length == 10)
            {
                //TODO: Messages must be in Posmessage.cs
                cr.Document.Customer = cr.DataConnector.CreateCustomer(cardNumber, "YENI MUSTERI", "", "", "");
                cr.Document.Customer.GsmNumber = gsmNumber;
                cr.SecurityConnector.AcceptCustomer(cr.Document.Customer.Number);
                return SetCustomerInfo(cr.Document.Customer);
            }
            else
            {
                AskGSMNumber();
                return cr.State;
            }
            
        }
       
        /// <summary>
        /// search customer by part of name, card no, number.
        /// </summary>
        /// <param name="qry">
        /// A keyword to search customer.
        /// </param>
        /// <returns>
        /// State of List.cs to show found customer's sub menu istems.
        /// </returns>
        public static IState FindCustomers(String info)
        {
            List<ICustomer> customers = new List<ICustomer>();

            System.Threading.Thread.Sleep(25);
            customers =cr.DataConnector.SearchCustomersByInfo(info);

            if (customers == null || customers.Count == 0)
            {
                Confirm err = new Confirm(PosMessage.CUSTOMER_NOT_FOUND,
                 new StateInstance(Continue),
                 new StateInstance(Continue));
                return States.AlertCashier.Instance(err);

            }

            CustomerMenuList matchingCustomers = new CustomerMenuList();
            matchingCustomers.AddRange(customers);
            matchingCustomers.Sort();

            return List.Instance(matchingCustomers, new ProcessSelectedItem(SetCurrentCustomer), Instance);

        }
        #region New customer recording methods
        /// <summary>
        /// used for new customer record.
        /// Enter customer code.
        /// </summary>
        /// <param name="code">
        /// customer code
        /// </param>
        /// <returns></returns>
        public static IState EnterCustomerCode(String code)
        {
            List<ICustomer> customers = new List<ICustomer>();

            System.Threading.Thread.Sleep(25);
            // Get All customer on CARI.DAT
            customers = cr.DataConnector.SearchCustomersByInfo("");

            int stanNo = 0;
            if (customers.Count > 0)
            {
                // Get last stan no and increase 1 for new standing
                ICustomer c = customers[customers.Count - 1];
                stanNo = int.Parse(c.Number) + 1;
            }

            customerInput = new StringBuilder();
            customerInput.Append('1');//flag
            customerInput.Append(String.Format("{0:D6}", stanNo));//number
            customerInput.Append(code.PadRight(20, ' '));//code
            
            return States.EnterString.Instance(PosMessage.NAME_FIRM,
                                                 new StateInstance<String>(EnterCustomerName),
                                                new StateInstance(Instance));
        }
        /// <summary>
        /// used for new customer record
        /// Enter name of new customer
        /// </summary>
        /// <param name="name">
        /// name of new customer
        /// </param>
        /// <returns></returns>
        public static IState EnterCustomerName(String name)
        {
            customerInput.Append(name.PadRight(20, ' ')); 
            String code = customerInput.ToString().Substring(7, 20);
            DisplayAdapter.Customer.Show("{0}\n{1}", name, code);
            return States.EnterString.Instance(PosMessage.STREET,
                                                new StateInstance<String>(EnterCustomerStreet),
                                               new StateInstance(Continue));        
        }
        /// <summary>
        /// used for new customer record.
        /// Enter new customer street.
        /// </summary>
        /// <param name="street">
        /// Street of new customer
        /// </param>
        /// <returns></returns>
        public static IState EnterCustomerStreet(String street)
        {
            customerInput.Append(street.PadRight(20,' '));
            return States.EnterString.Instance(PosMessage.STREET_NO,
                                                new StateInstance<String>(EnterCustomerStreetNo));
        }
        /// <summary>
        /// used for new customer record
        /// Enter new customer mainStreet
        /// </summary>
        /// <param name="mainStreet">
        /// main street of new Customer
        /// </param>
        /// <returns></returns>
        public static IState EnterCustomerStreetNo(String streetNo)
        {
            customerInput.Append(streetNo.PadRight(20,' '));
            return States.EnterString.Instance(PosMessage.REGION_CITY,
                                                new StateInstance<String>(EnterCustomerCity));
        }
        /// <summary>
        /// used for new customer record
        /// Enter city of new customer.
        /// </summary>
        /// <param name="city">
        /// City of new customer
        /// </param>
        /// <returns></returns>
        public static IState EnterCustomerCity(String city)
        {
            customerInput.Append(city.PadRight(20,' '));
            return States.EnterString.Instance(PosMessage.TAXOFFICE,
                                                new StateInstance<String>(EnterCustomerTaxInstitution));
        }
        /// <summary>
        /// used for new customer record
        /// Enter TaxInstitution of new customer
        /// </summary>
        /// <param name="taxInstitution">
        /// TaxInstitution of new customer
        /// </param>
        /// <returns></returns>
        public static IState EnterCustomerTaxInstitution(String taxInstitution)
        {
            customerInput.Append(taxInstitution.PadRight(15, ' '));
            return States.EnterString.Instance(PosMessage.TAX_NUMBER,
                                                 new StateInstance<String>(EnterCustomerTaxNumber));
        }
        /// <summary>
        /// used for new customer record
        /// Enter taxNumber of new customer
        /// </summary>
        /// <param name="taxNumber">
        /// Tax Number of new Customer
        /// </param>
        /// <returns></returns>
        public static IState EnterCustomerTaxNumber(String taxNumber)
        {
            long outl = 0;
            if (!Parser.TryLong(taxNumber.Trim(), out outl))
                return AlertCashier.Instance(new Confirm("VERGi NUMARASI\nRAKAMLARDAN OLUÞMALI"
                    , new StateInstance(ErrorCustomerTaxNumber), new StateInstance(ErrorCustomerTaxNumber)));
            customerInput.Append(taxNumber.PadRight(15,' '));

            if (cr.CurrentCashier.AuthorizationLevel == AuthorizationLevel.S ||
                cr.CurrentCashier.AuthorizationLevel == AuthorizationLevel.Z)
            {
                return States.EnterString.Instance(PosMessage.PROMOTION_LIMIT,
                                                    new StateInstance<string>(EnterCustomerPromotionLimit));
            }
            else
            {
                // Append "00" as default promotion limit to customer info string
                customerInput.Append("00");

                if (cr.IsDesktopWindows)
                {
                    return States.EnterString.Instance(PosMessage.CUSTOMER_GROUP + "\n ATLA (GÝRÝÞ)",
                                  new StateInstance<String>(EnterCustomerGroup),
                                 new StateInstance(Instance));
                }
                else
                {
                    return EnterCustomerGroup("".PadLeft(6));
                }
            }
        }
        public static IState ErrorCustomerTaxNumber()
        {
            return States.EnterString.Instance(PosMessage.TAX_NUMBER,
                                                 new StateInstance<String>(EnterCustomerTaxNumber));
        }

        public static IState EnterCustomerPromotionLimit(String promoLimit)
        {
            long outl = 0;
            if (!Parser.TryLong(promoLimit.Trim(), out outl))
                return AlertCashier.Instance(new Confirm("PROMOSYON LIMIT\nRAKAMLARDAN OLUÞMALI",
                                                new StateInstance(EnterCustomerPromotionLimit), new StateInstance(EnterCustomerPromotionLimit)));
            if(outl > 100)
                return AlertCashier.Instance(new Confirm("PROMOSYON LIMIT\n MAX 100 OLMALI",
                                                new StateInstance(EnterCustomerPromotionLimit), new StateInstance(EnterCustomerPromotionLimit)));
            customerInput.Append(promoLimit.PadLeft(2, '0'));

            if (cr.IsDesktopWindows)
            {
                return States.EnterString.Instance(PosMessage.CUSTOMER_GROUP + "\n ATLA (GÝRÝÞ)",
                              new StateInstance<String>(EnterCustomerGroup),
                             new StateInstance(Instance));
            }
            else
            {
                return EnterCustomerGroup("".PadLeft(6));
            }
        }

        public static IState EnterCustomerPromotionLimit()
        {
            return States.EnterString.Instance(PosMessage.PROMOTION_LIMIT, 
                                                new StateInstance<string>(EnterCustomerPromotionLimit));
        }

        public static IState EnterCustomerGroup(String customerGroup)
        {
            customerGroup = customerGroup.Trim();

            if (customerGroup.Length > 0 && customerGroup.Length != 6)
                AlertCashier.Instance(new Confirm("MÜÞTERÝ GRUBU\nALTI KARAKTER OLMALI",
                    new StateInstance(ErrorCustomerGroup),
                    new StateInstance(ErrorCustomerGroup)));

            customerInput.Append(customerGroup.PadLeft(6, ' '));

            MenuList docTypeList = new MenuList();
            String label = "";

            int i = 1;

            string labelFormat = "{0}\t{1}\n{2}";
            label = String.Format(labelFormat, PosMessage.DEFAULT_DOCUMENT_TYPE, i++, PosMessage.RECEIPT_TR);
            docTypeList.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.DEFAULT_DOCUMENT_TYPE, i++, PosMessage.INVOICE);
            docTypeList.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.DEFAULT_DOCUMENT_TYPE, i++, PosMessage.E_ARCHIVE);
            docTypeList.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.DEFAULT_DOCUMENT_TYPE, i++, PosMessage.E_INVOICE);
            docTypeList.Add(new MenuLabel(label));

            return States.List.Instance(docTypeList, EnterDefaultDocumentCode, SaveCustomerInput);
        }
        public static IState ErrorCustomerGroup()
        {
            return States.EnterString.Instance(PosMessage.CUSTOMER_GROUP,
                                                 new StateInstance<String>(EnterCustomerGroup));
        }

        public static void EnterDefaultDocumentCode(Object o)
        {
            MenuLabel label = o as MenuLabel;
            int defDocType;
            String docType = label.Value.ToString().Split('\n')[1].Trim();

            switch(docType)
            {
                case PosMessage.RECEIPT_TR:
                    defDocType = 0;
                    break;
                case PosMessage.INVOICE:
                    defDocType = new Invoice().DocumentTypeId;
                    break;
                case PosMessage.E_ARCHIVE:
                    defDocType = new EArchive().DocumentTypeId;
                    break;
                case PosMessage.E_INVOICE:
                    defDocType = new EInvoice().DocumentTypeId;
                    break;
                case PosMessage.MEAL_TICKET:
                    defDocType = new MealTicket().DocumentTypeId;
                    break;
                case PosMessage.CAR_PARKIMG:
                    defDocType = new CarParkDocument().DocumentTypeId;
                    break;
                case PosMessage.ADVANCE:
                    defDocType = new Advance().DocumentTypeId;
                    break;
                case PosMessage.COLLECTION_INVOICE:
                    defDocType = new CollectionInvoice().DocumentTypeId;
                    break;
                case PosMessage.CURRENT_ACCOUNT_COLLECTION:
                    defDocType = new CurrentAccountDocument().DocumentTypeId;
                    break;
                default:
                    defDocType = 0;
                    break;
            }

            customerInput.Append(defDocType.ToString().PadLeft(2, '0'));

            cr.State = SaveCustomerInput();
        }

        public static IState SaveCustomerInput()
        {
            try
            {
                cr.Document.Customer = cr.DataConnector.SaveCustomer(customerInput.ToString());
                cr.SecurityConnector.AcceptCustomer(cr.Document.Customer.Number);

            }
            catch (Exception)
            {
            }

            DisplayAdapter.Cashier.Show(PosMessage.END_OF_RECORD);
            System.Threading.Thread.Sleep(2000);

            return EndofRecord();
        }

        public static IState EndofRecord()
        {
            // if current document type is return document, do not change type with choosen customer default document type
            if(cr.Document.DocumentTypeId == (int)DocumentTypes.RETURN_DOCUMENT)
                return (ReturnConfirm == null) ? Start.Instance() : ReturnConfirm();

            if (cr.Document.Customer.DefaultDocumentType != (DocumentTypes)cr.Document.DocumentTypeId)
            {
                DocumentTypes type = DocumentTypes.RECEIPT;
                switch(cr.Document.Customer.DefaultDocumentType)
                {
                    case DocumentTypes.INVOICE:
                        type = DocumentTypes.INVOICE;
                        break;
                    case DocumentTypes.E_INVOICE:
                        type = DocumentTypes.E_INVOICE;
                        break;
                    case DocumentTypes.E_ARCHIEVE:
                        type = DocumentTypes.E_ARCHIEVE;
                        break;
                }
                cr.ChangeDocumentType(cr.ChangeDocumentType(cr.Document, type));
                return cr.State;
            }
            else
                return (ReturnConfirm == null) ? Start.Instance() : ReturnConfirm();
        }
        #endregion
        /// <summary>
        /// Operate when user hits Escape key.
        /// Go to selected state
        /// </summary>
        public override void Escape()
        {
            cr.SecurityConnector.CustomerCaptured -= new EventHandler(SecurityConnector_CustomerCaptured);
            cr.SecurityConnector.EscapeCustomer();
            cr.Document.Customer = null;

            if (ReturnCancel != null)
            {
                cr.State = ReturnCancel();
                ReturnCancel = null;
                ReturnConfirm = null;
            }
            else
                cr.State = States.Start.Instance();
        }
        /// <summary>
        /// Confirm cashier to void current customer
        /// </summary>
        /// <returns>
        /// Start state
        /// </returns>
        public static IState ConfirmVoidCustomer()
        {
            cr.SecurityConnector.EscapeCustomer();
            cr.Document.Customer = null;
            return States.Start.Instance();
        }

    }

   
}
