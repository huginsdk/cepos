using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Hugin.POS.Common;

namespace Hugin.POS.Data
{
    class Customer : ICustomer
    {
        static List<ICustomer> customers;
        static List<String> discountCards;
        static List<ICustomer> backupCustomers;
        static List<String> backupDiscountCards;
        static IPointAdapter adapter = null;

        int flag = 0;
        string code = String.Empty;
        string customerGroup = String.Empty;
        string number = String.Empty;
        string name = String.Empty;
        string address = String.Empty;
        string taxInstitution = String.Empty;
        string taxNumber = String.Empty;
        string gsmNumber = String.Empty;

        int promotionLimit;
        DocumentTypes defaultDocumentType = DocumentTypes.NULL;
        long points = 0;
        Dictionary<string, string> customProperties;

        #region ICustomer Members

        public string Code
        {
            get { return code; }
        }

        public string CustomerGroup
        {
            get { return customerGroup; }
        }

        public string Number
        {
            get { return number; }
        }

        public string Name
        {
            get { return name; }
        }

        public string[] Identity
        {
            get
            {
                String[] identity = new String[2];
                identity[0] = code;
                identity[1] = name;
                return identity;
            }
        }

        public string[] Contact
        {
            get
            {
                String[] contact = new String[5];
                String blankAddress = String.Format("{0,-60}", address);
                contact[0] = blankAddress.Substring(0, 20);
                contact[1] = blankAddress.Substring(20, 20);
                contact[2] = blankAddress.Substring(40, 20);
                contact[3] = taxInstitution;
                contact[4] = taxNumber;

                return contact;
            }
        }

        public long Points
        {
            get { return points; }
            set { points = value; }
        }

        public bool IsDiplomatic
        {
            get { return false; }
        }

        public int PromotionLimit
        {
            get { return promotionLimit; }
        }

        public DocumentTypes DefaultDocumentType
        {
            get { return defaultDocumentType; }
        }

        public string GsmNumber
        {
            get { return gsmNumber; }
            set { gsmNumber = value; }
        }

        #endregion

        public Customer()
        {
            if (adapter == null)
                adapter = Connector.Instance().PointAdapter;

        }

        internal static void Backup()
        {
            backupCustomers = customers;
            backupDiscountCards = discountCards;

            customers = new List<ICustomer>();
            discountCards = new List<string>();
        }

        internal static void Restore()
        {
            if (customers.Count == 0 && backupCustomers != null)
            {
                customers = backupCustomers;
                discountCards = backupDiscountCards;
            }

            if (backupCustomers != null)
                backupCustomers.Clear();
            if (backupDiscountCards != null)
                backupDiscountCards.Clear();
        }

        internal static bool Add(String line)
        {
            try
            {
                ICustomer customer = Parse(line);

                // If customer placed current
                ICustomer result = null;
                if (customers != null && customers.Count > 0)
                {
                    var query = from c in customers
                                where c.Code == customer.Code
                                select c;
                    if (query != null && query.Count() > 0)
                        result = query.First();
                }

                if(result != null)
                {
                    // Update current customer info
                    customers[customers.IndexOf(result)] = customer;
                }
                else
                {
                    customers.Add(customer);
                    string code = customer.Code.Trim();
                    if (code[code.Length - 1] == '*')
                    {
                        if (!discountCards.Contains(customer.Code))
                            discountCards.Add(customer.Code);
                    }
                }

                return true;
                //if (customersByCardNo.ContainsKey(customer.Code))
                //{
                //    customersByCardNo[customer.Code] = customer;

                //    Dictionary<string, ICustomer> tempDic = new Dictionary<string, ICustomer>(customersByCode);
                //    foreach(KeyValuePair<string,ICustomer> kvp in tempDic)
                //    {
                //        if(kvp.Value.Code == customer.Code)
                //        {
                //            customersByCode.Remove(kvp.Key);
                //            customersByCode.Add(customer.Number, customer);
                //        }
                //    }

                //    tempDic = new Dictionary<string, ICustomer>(customersByTcknVkn);
                //    foreach(KeyValuePair<string,ICustomer> kvp in tempDic)
                //    {
                //        if(kvp.Value.Code == customer.Code)
                //        {
                //            customersByTcknVkn.Remove(kvp.Key);
                //            customersByTcknVkn.Add(customer.Contact[4], customer);
                //        }
                //    }

                //    tempDic = new Dictionary<string, ICustomer>(customersByName);
                //    foreach (KeyValuePair<string, ICustomer> kvp in tempDic)
                //    {
                //        if (kvp.Value.Code == customer.Code)
                //        {
                //            customersByName.Remove(kvp.Key);
                //            customersByName.Add(customer.Name, customer);
                //        }
                //    }
                //}
                //else
                //{
                //customersByCode.Add(customer.Number, customer);
                //customersByCardNo.Add(customer.Code, customer);
                //customersByTcknVkn.Add(customer.Contact[4], customer);
                //if (!customersByName.ContainsKey(customer.Name))
                //    customersByName.Add(customer.Name, customer);

                //}


            }
            catch (Exception e)
            {
                PosException lastException = new PosException(e.Message, e);
                lastException.Data.Add("ErrorLine", line);
                EZLogger.Log.Warning(lastException);
                return false;
            }
        }

        internal static ICustomer CreateCustomer(String line)
        {
            return Parse(line);
        }

        private static ICustomer Parse(String line)
        {
            Customer customer = new Customer();
            customer.flag = Int32.Parse(line.Substring(0, 1));
            customer.number = line.Substring(1, 6);
            customer.code = line.Substring(7, 20).Trim();
            customer.name = line.Substring(27, 20).Trim();
            customer.address = line.Substring(47, 60).Trim();
            customer.taxInstitution = line.Substring(107, 15).Trim();
            customer.taxNumber = line.Substring(122, 15);

            customer.promotionLimit = int.Parse(line.Substring(137, 2));
            customer.customerGroup = null;

            if (line.Length >= 145)
                customer.customerGroup = line.Substring(139, 6);

            if (line.Length >= 147)
            {
                try
                {
                    customer.defaultDocumentType = (DocumentTypes)Convert.ToInt32(line.Substring(145, 2));
                }
                catch { }
            }

            int propertyCount = 0;
            customer.customProperties = new Dictionary<string, string>();
            if (line.Length >= 149 && Parser.TryInt(line.Substring(147, 2), out propertyCount))
            {
                string customProps = line.Substring(149, propertyCount * 16);
                for (int i = 0; i < propertyCount; i++)
                {
                    string key = customProps.Substring(i * 8, 8);
                    string value = customProps.Substring((i + 1) * 8, 8);
                    if (!customer.customProperties.ContainsKey(key))
                        customer.customProperties.Add(key.ToUpper(), value.ToUpper());
                }

            }

            return customer;
        }

        internal static ICustomer FindByCode(String customerNumber)
        {
            if (customers == null)
                return null;

            var query = from c in customers
                        where c.Number == customerNumber
                        select c;
            ICustomer customer = null;
            if (query != null && query.Count() > 0)
                customer = query.First();

            if (customer != null)
                ((Customer)customer).RefreshPoint();
            return customer;
        }

        internal static ICustomer FindByCardNo(String code)
        {
            if (customers == null)
                return null;

            ICustomer customer = null;
            bool isCustomerCard = false;

            if (code.StartsWith("C"))//customer card has been used
            {
                code = code.Substring(1);
                isCustomerCard = true;
            }

            var query = from c in customers
                        where c.Code == code
                        select c;
            if (query != null && query.Count() > 0)
                customer = query.First();

            if (customer != null)
                ((Customer)customer).RefreshPoint();

            if (customer == null && isCustomerCard)
            {
                char[] trimChars ={ ' ', '*' };
                foreach (String key in discountCards)
                {
                    if (code.StartsWith(key.TrimEnd(trimChars)))
                    {
                        query = from c in customers
                                where c.Code == key
                                select c;
                        if(query != null && query.Count() > 0)
                            customer = query.First();
                        break;
                    }
                }
            }

            return customer;
        }

        private void RefreshPoint()
        {
            try
            {
                if (adapter != null)
                    points = adapter.GetPoint(this);
            }
            catch (Exception ex)
            {
                points = 0;
                EZLogger.Log.Error("Müþteri puaný güncellenemedi.");
                EZLogger.Log.Error(ex);
            }

        }

        internal static ICustomer FindByName(String nameQry)
        {
            if (customers == null)
                return null;

            ICustomer customer = null;
            var query = from c in customers
                        where c.Name == nameQry
                        select c;
            if (query != null && query.Count() > 0)
                customer = query.First();

            ((Customer)customer).RefreshPoint();

            return customer;
        }

        internal static ICustomer FindByTcknVkn(String tcknVkn)
        {
            if (customers == null)
                return null;

            ICustomer customer = null;
            var query = from c in customers
                        where c.Contact[4] == tcknVkn
                        select c;
            if (query != null && query.Count() > 0)
                customer = query.First();

            ((Customer)customer).RefreshPoint();

            return customer;

        }

        internal static List<ICustomer> SearchCustomers(String info)
        {
            List<ICustomer> customerList = new List<ICustomer>();

            foreach (ICustomer customer in customers)
            {
                if (Str.Contains(customer.Name, info))//search in name
                {
                    ((Customer)customer).RefreshPoint();
                    customerList.Add(customer);
                }
                else if (Str.Contains(customer.Number, info))//search in customer number
                {
                    ((Customer)customer).RefreshPoint();
                    customerList.Add(customer);
                }
                else if (Str.Contains(customer.Code, info))//search in customer code
                {
                    ((Customer)customer).RefreshPoint();
                    customerList.Add(customer);
                }
            }
         
            return customerList;
        }


        public long UpdatePoint(PointObject pointObj)
        {
            IPointAdapter adapter = Connector.Instance().PointAdapter;
            if (adapter != null)
                adapter.UpdatePoint(pointObj);
            points += pointObj.Value;
            return points;
        }
        public string GetCustomValue(String customKey)
        {
            if (customProperties.ContainsKey(customKey))
                return customProperties[customKey];
            return "";
        }

        internal static ICustomer CreateCustomer(string code, string name, string address, string taxInstitution, string taxNumber)
        {
            Customer customer = new Customer();

            customer.flag = 1;
            customer.code = code;
            customer.name = name;
            customer.address = address;
            customer.taxInstitution = taxInstitution;
            customer.taxNumber = taxNumber;

            Customer existingCustomer = null;

            if (name.EndsWith("DYNAMIC"))
            {
                int index = customer.name.LastIndexOf("DYNAMIC");
                customer.name = customer.name.Substring(0, index);
            }
            try
            {
                existingCustomer = (Customer)FindByName(customer.name);
            }
            catch
            {

            }
            if (existingCustomer != null)
            {
                if (existingCustomer.Code == customer.code &&
                    existingCustomer.taxNumber == customer.taxNumber)
                {
                    return existingCustomer;
                }
            }

            int minDynamicNumber = 100000;
            while (true)
            {
                ICustomer cust = null;
                var query = from c in customers
                            where c.Code == minDynamicNumber + ""
                            select c;

                if (query != null && query.Count() > 0)
                    cust = query.First();

                if (cust == null)
                {
                    break;
                }
                minDynamicNumber++;
            }

            customer.number = minDynamicNumber + "";


            /*
             * do not need to save customer in a file
             * because it will be used dynamically                
             */
            customers.Add(customer);

            return customer;
        }

        internal static void CustomersToFile(String fileName)
        {
            StringBuilder strCustomers = new StringBuilder();
            List<ICustomer> writtenCustomers = new List<ICustomer>(customers);

            //Clear file
            IOUtil.WriteAllText(fileName, "", PosConfiguration.DefaultEncoding);
            int count = 0;
            foreach (ICustomer c in customers)
            {
                strCustomers.Append(String.Format(
                    "{0:D1}{1:D6}{2}{3}{4}{5}{6}{7:D2}{8:D6}{9:D2}\r\n",
                    ((Customer)c).flag == 1 ? "1" : "0",
                    c.Number,
                    c.Code.PadRight(20, ' '),
                    c.Name.PadRight(20, ' '),
                    c.Contact[0].PadRight(20, ' ') + c.Contact[1].PadRight(20, ' ') + c.Contact[2].PadRight(20, ' '),
                    c.Contact[3].PadRight(15, ' '),
                    c.Contact[4].PadRight(15, ' '),
                    c.PromotionLimit,
                    c.CustomerGroup,
                    c.DefaultDocumentType
                    ));

                ICustomer cust = null;
                var query = from cc in customers
                            where cc.Number == c.Number
                            select cc;
                if (query != null && query.Count() > 0)
                    cust = query.First();

                if (cust != null)
                    writtenCustomers.Remove(cust);

                //Windows ce memory is less than desktop pc 
                //So OutOfMemoryException occur if stringbuilder capacity is too big.
                if (count >= 5000)
                {
                    //move memory to file than clear momory
                    IOUtil.AppendAllText(fileName, strCustomers.ToString());
                    strCustomers = new StringBuilder();
                    count = 0;
                }
                count++;
            }

            foreach (ICustomer cstmr in writtenCustomers)
            {
                //if (writtenLabels.IndexOf(labelNo) >= 0) continue;
                strCustomers.Append(String.Format(
                    "{0:D1}{1:D6}{2}{3}{4}{5}{6}{7:D2}{8:D6}{9:D2}\r\n",
                    ((Customer)cstmr).flag == 1 ? "1" : "0",
                    cstmr.Number,
                    cstmr.Code.PadRight(20, ' '),
                    cstmr.Name.PadRight(20, ' '),
                    cstmr.Contact[0].PadRight(20, ' ') + cstmr.Contact[1].PadRight(20, ' ') + cstmr.Contact[2].PadRight(20, ' '),
                    cstmr.Contact[3].PadRight(15, ' '),
                    cstmr.Contact[4].PadRight(15, ' '),
                    cstmr.PromotionLimit,
                    cstmr.CustomerGroup,
                    cstmr.DefaultDocumentType
                    ));

                if (count >= 5000)
                {
                    IOUtil.AppendAllText(fileName, strCustomers.ToString());
                    strCustomers = new StringBuilder();
                    count = 0;
                }
                count++;
            }

            IOUtil.AppendAllText(fileName, strCustomers.ToString());


            strCustomers = new StringBuilder();
            writtenCustomers = new List<ICustomer>();
        }
    }
}
