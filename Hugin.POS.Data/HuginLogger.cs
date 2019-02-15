using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Hugin.POS.Common;

namespace Hugin.POS.Data
{
    class HuginLogger : Logger
    {
        internal override StringWriter LogItems(ISalesDocument document, int docStatus, ref int number)
        {
            StringWriter logWriter = new StringWriter();

            string[] subLines = null;

            //if (document.Id == 0 || document.IsEmpty) : Not possible, The function which calls LogItems checks that condition.
            String documentId = document.Id.ToString().PadLeft(6, '0');

            if (!String.IsNullOrEmpty(document.SlipSerialNo))
            {
                String seriNo = document.SlipSerialNo;
                if (seriNo.Length > 2)
                    seriNo = seriNo.Substring(0, 2);

                String orderNo = document.SlipOrderNo;
                if (orderNo.Length > 6)
                    orderNo = orderNo.Substring(0, 6);

                documentId = String.Format("{0} {1}",
                                                seriNo.PadLeft(2, ' '),
                                                orderNo.PadLeft(6, '0'));
            }
            else if (document.DocumentTypeId >= 0)
            {
#if WindowsCE
                int lastSlipNo = Document.GetLastSlipNo(PosConfiguration.Get("RegisterId"));
                lastSlipNo++;

                documentId = lastSlipNo.ToString().PadLeft(6, '0');
#endif
            }

            logWriter.WriteLine("1,{0:D5},{1},{2},{3}     {4},{5,-12}",
                                                                number++,
                                                                (document.DocumentTypeId < 0) ? "01" : "02",
                //DOcument name ve document shortname gibi iki ayri property olsa
                //daha guzel olur
                                                                document.Code,
                                                                PosConfiguration.Get("RegisterId"),
                                                                (currentCashier == null) ? "0000" : currentCashier.Id,
                                                                documentId);

            logWriter.WriteLine("1,{0},03,TAR,{1:dd}/{1:MM}/{1:yyyy}  ,{1:HH:mm:ss}    ",
                                                                 number++.ToString().PadLeft(5, '0'),
                                                                DateTime.Now);

            /**********  INFO RECEIPTS  ****************/

            // Tckn/Vkn
            if (document.Code == PosMessage.HR_CODE_INVOICE ||
                document.Code == PosMessage.HR_CODE_E_INVOICE ||
                document.Code == PosMessage.HR_CODE_E_ARCHIVE ||
                document.Code == PosMessage.HR_CODE_ADVANCE ||
                document.Code == PosMessage.HR_CODE_CURRENT_ACCOUNT_COLLECTION)
            {
                String tcknVkn = "";
                if (!String.IsNullOrEmpty(document.TcknVkn))
                    tcknVkn = document.TcknVkn.Trim();
                else if (document.Customer != null)
                    tcknVkn = document.Customer.Contact[4].Trim();

                if (tcknVkn.Trim().Length == 10)
                    logWriter.WriteLine("1,{0:D5},60,VKN,{1},{2}", number++, tcknVkn.PadLeft(12), " ".PadLeft(12));
                else if (tcknVkn.Trim().Length == 11)
                    logWriter.WriteLine("1,{0:D5},61,TKN,{1},{2}", number++, tcknVkn.PadLeft(12), " ".PadLeft(12));
            }

            // Issue Datetime
            if (document.IssueDate != null)
            {
                if (document.Code == PosMessage.HR_CODE_INVOICE ||
                    document.Code == PosMessage.HR_CODE_COLLECTION_INVOICE ||
                    document.Code == PosMessage.HR_CODE_CURRENT_ACCOUNT_COLLECTION)
                {
                    logWriter.WriteLine("1,{0:D5},62,BDT,{1:dd}/{1:MM}/{1:yyyy}  ,{2}",
                                                                                                number++,
                                                                                                document.IssueDate,
                                                                                                " ".PadLeft(12));
                }
                else if (document.Code == PosMessage.HR_CODE_CAR_PARKING)
                {
                    logWriter.WriteLine("1,{0:D5},62,BDT,{1:dd}/{1:MM}/{1:yyyy}  ,{1:HH:mm:ss}    ",
                                                                                                number++,
                                                                                                document.IssueDate);
                }
            }

            // Car Plate
            if(!String.IsNullOrEmpty(document.CustomerTitle))
            {
                if (document.Code == PosMessage.HR_CODE_CAR_PARKING)
                {
                    logWriter.WriteLine("1,{0:D5},63,PLK,{1},{2}", number++, document.CustomerTitle.PadLeft(12), " ".PadLeft(12));
                }
            }

            // Customer Name
            if (document.Code == PosMessage.HR_CODE_ADVANCE ||
                document.Code == PosMessage.HR_CODE_COLLECTION_INVOICE ||
                document.Code == PosMessage.HR_CODE_CURRENT_ACCOUNT_COLLECTION)
            {
                string title = "";

                if (document.Customer != null)
                    title = document.Customer.Name;
                else
                    title = document.CustomerTitle;

                if (!String.IsNullOrEmpty(title))
                    logWriter.WriteLine("1,{0:D5},64,MAD,{1}", number++, title.Trim().PadLeft(25));
            }

            
            if (document.Code == PosMessage.HR_CODE_COLLECTION_INVOICE)
            {
                // Instution Name
                if (!String.IsNullOrEmpty(document.ReturnReason))
                {
                    logWriter.WriteLine("1,{0:D5},65,KAD,{1}", number++, document.ReturnReason.PadLeft(25));
                }

                // Amount- Comission
                decimal comission = document.ComissionAmount;
                decimal amount = document.TotalAmount - comission;
                decimal total = document.TotalAmount;

                logWriter.WriteLine("1,{0:D5},66,KMS,  {1:D10},  {2:D10}", number++,
                                                                            (long)Math.Round(100m * amount, 0),
                                                                            (long)Math.Round(100m * comission, 0));
            }


            /**********  E-INVOICE/E-ARCHIVE  ****************/
            if (document.Code == PosMessage.HR_CODE_E_ARCHIVE || document.Code == PosMessage.HR_CODE_E_INVOICE)
            {
                #region SUPPLIER PARTY
                /* Accounting Supplier Party */
                if (Connector.Instance().CurrentSettings.SupplierInfo != null)
                {
                    AccountingParty supplier = Connector.Instance().CurrentSettings.SupplierInfo;
                    
                    // Tckn/Vkn
                    if (!String.IsNullOrEmpty(supplier.TCKN_VKN))
                        logWriter.WriteLine("1,{0:D5},70,ASP,{1},{2}", number++, supplier.TCKN_VKN.PadLeft(12), " ".PadLeft(12));

                    // Title
                    if (!String.IsNullOrEmpty(supplier.Title))
                    {
                        subLines = GetSubStringsAsLen(supplier.Title, 25);
                        foreach(string str in subLines)
                            logWriter.WriteLine("1,{0:D5},71,ASP,{1}", number++, str.PadLeft(25));
                    }

                    // First Name & Family Name
                    if (!String.IsNullOrEmpty(supplier.FirstName) && !String.IsNullOrEmpty(supplier.FamilyName))
                    {
                        string fName = supplier.FirstName;
                        if (supplier.FirstName.Length > 12)
                            fName = supplier.FirstName.Substring(0, 12);

                        string famName = supplier.FamilyName;
                        if (supplier.FamilyName.Length > 12)
                            famName = supplier.FamilyName.Substring(0, 12);

                        logWriter.WriteLine("1,{0:D5},72,ASP,{1},{2}", number++, fName.PadLeft(12), famName.PadLeft(12));
                    }
                    
                    // PostalCode & Room
                    string postal = "";
                    string room = "";
                    
                    if (!String.IsNullOrEmpty(supplier.PostalCode))
                        postal = supplier.PostalCode;
                    
                    if (!String.IsNullOrEmpty(supplier.Room))
                        room = supplier.Room;
                    
                    if (postal.Length > 0 || room.Length > 0)
                        logWriter.WriteLine("1,{0:D5},73,ASP,{1},{2}", number++, postal.PadLeft(12), room.PadLeft(12));
                    
                    // Building No & Building Name
                    string buildingNo = "";
                    if (!String.IsNullOrEmpty(supplier.BuildingNo))
                        buildingNo = supplier.BuildingNo;
                    
                    string buildingName = "";
                    if (!String.IsNullOrEmpty(supplier.BuildingName))
                    {
                        buildingName = supplier.BuildingName;
                        if (supplier.BuildingName.Length > 12)
                            buildingName = supplier.BuildingName.Substring(0, 12);
                    }
                    
                    if (buildingNo.Length > 0 || buildingName.Length > 0)
                        logWriter.WriteLine("1,{0:D5},74,ASP,{1},{2}", number++, buildingNo.PadLeft(12), buildingName.PadLeft(12));

                    // Street
                    if (!String.IsNullOrEmpty(supplier.Street))
                    {
                        subLines = GetSubStringsAsLen(supplier.Street, 25);

                        foreach(string str in subLines)
                            logWriter.WriteLine("1,{0:D5},75,ASP,{1}", number++, str.PadLeft(25));
                    }

                    // District
                    if (!String.IsNullOrEmpty(supplier.District))
                    {
                        subLines = GetSubStringsAsLen(supplier.District, 25);

                        foreach(string str in subLines)
                            logWriter.WriteLine("1,{0:D5},76,ASP,{1}", number++, str.PadLeft(25));
                    }

                    // Village
                    if (!String.IsNullOrEmpty(supplier.Village))
                    {
                        subLines = GetSubStringsAsLen(supplier.Village, 25);

                        foreach(string str in subLines)
                            logWriter.WriteLine("1,{0:D5},77,ASP,{1}", number++, str.PadLeft(25));
                    }

                    // Subcity
                    if (!String.IsNullOrEmpty(supplier.SubCity))
                    {
                        subLines = GetSubStringsAsLen(supplier.SubCity, 25);

                        foreach(string str in subLines)
                            logWriter.WriteLine("1,{0:D5},78,ASP,{1}", number++, str.PadLeft(25));
                    }
                    
                    // City & Country
                    string city = "";
                    string country = "";

                    if (!String.IsNullOrEmpty(supplier.City))
                    {
                        city = supplier.City;
                        if (supplier.City.Length > 12)
                            city = supplier.City.Substring(0,12);
                    }

                    if (!String.IsNullOrEmpty(supplier.Country))
                    {
                        country = supplier.Country;
                        if (supplier.Country.Length > 12)
                            country = supplier.Country.Substring(0, 12);
                    }
                    
                    if (city.Length > 0 || country.Length > 0)
                        logWriter.WriteLine("1,{0:D5},79,ASP,{1},{2}", number++, city.PadLeft(12), country.PadLeft(12));

                    // Telephone
                    if (!String.IsNullOrEmpty(supplier.Telephone))
                    {
                        string tel = supplier.Telephone;
                        if (tel.Length > 25)
                            tel = supplier.Telephone.Substring(0, 25);

                        logWriter.WriteLine("1,{0:D5},80,ASP,{1}", number++, tel.PadLeft(25));
                    }

                    // Fax
                    if (!String.IsNullOrEmpty(supplier.Fax))
                    {
                        string fax = supplier.Fax;
                        if (fax.Length > 25)
                            fax = supplier.Fax.Substring(0, 25);

                        logWriter.WriteLine("1,{0:D5},81,ASP,{1}", number++, fax.PadLeft(25));
                    }

                    // EMail
                    if (!String.IsNullOrEmpty(supplier.EMail))
                    {
                        subLines = GetSubStringsAsLen(supplier.EMail, 25);

                        foreach(string str in subLines)
                            logWriter.WriteLine("1,{0:D5},82,ASP,{1}", number++, str.PadLeft(25));
                    }

                    // WebPage
                    if (!String.IsNullOrEmpty(supplier.WebPage))
                    {
                        subLines = GetSubStringsAsLen(supplier.WebPage, 25);

                        foreach(string str in subLines)
                            logWriter.WriteLine("1,{0:D5},83,ASP,{1}", number++, str.PadLeft(25));
                    }

                    // TaxScheme
                    if (!String.IsNullOrEmpty(supplier.TaxScheme))
                    {
                        subLines = GetSubStringsAsLen(supplier.TaxScheme, 25);

                        foreach(string str in subLines)
                            logWriter.WriteLine("1,{0:D5},84,ASP,{1}", number++, str.PadLeft(25));
                    }
                }
                #endregion

                
                /* Accounting Customer Party */
                if (document.AdditionalInfo != null)
                {
                    #region CUSTOMER PARTY
                    // Customer info
                    if (document.AdditionalInfo.CustomerParty != null)
                    {
                        AccountingParty customer = document.AdditionalInfo.CustomerParty;
                        
                        // Tckn/Vkn
                        if (String.IsNullOrEmpty(customer.TCKN_VKN))
                        {
                            if (document.Customer != null)
                                customer.SetValue(AccountingPartyTag.TCKN_VKN, document.Customer.Contact[4].Trim());
                            else
                                customer.SetValue(AccountingPartyTag.TCKN_VKN, document.TcknVkn);
                        }
                        if (!String.IsNullOrEmpty(customer.TCKN_VKN))
                            logWriter.WriteLine("1,{0:D5},70,ACP,{1},{2}", number++, customer.TCKN_VKN.Trim().PadLeft(12), "".PadLeft(12));

                        // Title
                        if (!String.IsNullOrEmpty(customer.Title))
                        {
                            subLines = GetSubStringsAsLen(customer.Title, 25);

                            foreach(string str in subLines)
                                logWriter.WriteLine("1,{0:D5},71,ACP,{1}", number++, str.PadLeft(25));
                        }

                        // First Name & Family Name
                        if (!String.IsNullOrEmpty(customer.FirstName) && !String.IsNullOrEmpty(customer.FamilyName))
                        {
                            string cName = customer.FirstName;
                            if (cName.Length > 12)
                                cName = customer.FirstName.Substring(0, 12);

                            string cFamName = customer.FamilyName;
                            if (cFamName.Length > 12)
                                cFamName = customer.FamilyName.Substring(0, 12);

                            logWriter.WriteLine("1,{0:D5},72,ACP,{1},{2}", number++, cName.PadLeft(12), cFamName.PadLeft(12));
                        }
                        
                        // PostalCode & Room
                        string postal = "";
                        string room = "";
                        
                        if (!String.IsNullOrEmpty(customer.PostalCode))
                            postal = customer.PostalCode;
                        
                        if (!String.IsNullOrEmpty(customer.Room))
                            room = customer.Room;
                        
                        if (postal.Length > 0 || room.Length > 0)
                            logWriter.WriteLine("1,{0:D5},73,ACP,{1},{2}", number++, postal.PadLeft(12), room.PadLeft(12));
                        
                        // Building No & Building Name
                        string buildingNo = "";
                        if (!String.IsNullOrEmpty(customer.BuildingNo))
                            buildingNo = customer.BuildingNo;
                        
                        string buildingName = "";
                        if (!String.IsNullOrEmpty(customer.BuildingName))
                        {
                            buildingName = customer.BuildingName;
                            if (buildingName.Length > 12)
                                buildingName = customer.BuildingName.Substring(0, 12);
                        }
                        
                        if (buildingNo.Length > 0 || buildingName.Length > 0)
                            logWriter.WriteLine("1,{0:D5},74,ACP,{1},{2}", number++, buildingNo.PadLeft(12), buildingName.PadLeft(12));

                        // Street
                        if (!String.IsNullOrEmpty(customer.Street))
                        {
                            subLines = GetSubStringsAsLen(customer.Street, 25);

                            foreach(string str in subLines)
                                logWriter.WriteLine("1,{0:D5},75,ACP,{1}", number++, str.PadLeft(25));
                        }

                        // District
                        if (!String.IsNullOrEmpty(customer.District))
                        {
                            subLines = GetSubStringsAsLen(customer.District, 25);

                            foreach(string str in subLines)
                                logWriter.WriteLine("1,{0:D5},76,ACP,{1}", number++, str.PadLeft(25));
                        }

                        // Village
                        if (!String.IsNullOrEmpty(customer.Village))
                        {
                            subLines = GetSubStringsAsLen(customer.Village, 25);

                            foreach(string str in subLines)
                                logWriter.WriteLine("1,{0:D5},77,ACP,{1}", number++, str.PadLeft(25));
                        }

                        // Subcity
                        if (!String.IsNullOrEmpty(customer.SubCity))
                        {
                            subLines = GetSubStringsAsLen(customer.SubCity, 25);

                            foreach(string str in subLines)
                                logWriter.WriteLine("1,{0:D5},78,ACP,{1}", number++, str.PadLeft(25));
                        }
                        
                        // City & Country
                        string city = "";
                        string country = "";

                        if (!String.IsNullOrEmpty(customer.City))
                        {
                            city = customer.City;
                            if (city.Length > 12)
                                city = customer.City.Substring(0, 12);
                        }

                        if (!String.IsNullOrEmpty(customer.Country))
                        {
                            country = customer.Country;
                            if (country.Length > 12)
                                country = customer.Country.Substring(0, 12);
                        }
                        
                        if (city.Length > 0 || country.Length > 0)
                            logWriter.WriteLine("1,{0:D5},79,ACP,{1},{2}", number++, city.PadLeft(12), country.PadLeft(12));

                        // Telephone
                        if (!String.IsNullOrEmpty(customer.Telephone))
                        {
                            string cTel = customer.Telephone;
                            if (cTel.Length > 25) cTel = customer.Telephone.Substring(0, 25);
                            logWriter.WriteLine("1,{0:D5},80,ACP,{1}", number++, customer.Telephone.PadLeft(25));
                        }

                        // Fax
                        if (!String.IsNullOrEmpty(customer.Fax))
                        {
                            string cFax = customer.Fax;
                            if (cFax.Length > 25) cFax = customer.Fax.Substring(0, 25);
                            logWriter.WriteLine("1,{0:D5},81,ACP,{1}", number++, customer.Fax.PadLeft(25));
                        }

                        // EMail
                        if (!String.IsNullOrEmpty(customer.EMail))
                        {
                            subLines = GetSubStringsAsLen(customer.EMail, 25);

                            foreach(string str in subLines)
                                logWriter.WriteLine("1,{0:D5},82,ACP,{1}", number++, str.PadLeft(25));
                        }

                        // WebPage
                        if (!String.IsNullOrEmpty(customer.WebPage))
                        {
                            subLines = GetSubStringsAsLen(customer.WebPage, 25);

                            foreach(string str in subLines)
                                logWriter.WriteLine("1,{0:D5},83,ACP,{1}", number++, str.PadLeft(25));
                        }

                        // TaxScheme
                        if (!String.IsNullOrEmpty(customer.TaxScheme))
                        {
                            subLines = GetSubStringsAsLen(customer.TaxScheme, 25);

                            foreach(string str in subLines)
                                logWriter.WriteLine("1,{0:D5},84,ACP,{1}", number++, str.PadLeft(25));
                        }
                    }
                    #endregion

                    // Profil ID
                    string profID = "";
                    switch (document.AdditionalInfo.ProfilID)
                    {
                        case DocProfilID.TEMEL_FATURA:
                            profID = "TEMELFATURA";
                            break;
                        case DocProfilID.TICARI_FATURA:
                            profID = "TÝCARÝFATURA";
                            break;
                    }
                    logWriter.WriteLine("1,{0:D5},90,PID,{1},{2}", number++, profID.PadLeft(12), "".PadLeft(12));
                    
                    
                    // Currency Code
                    string ccode = "";
                    switch (document.AdditionalInfo.CurrencyCode)
                        {
                            case DocCurrencyCode.DOLLAR:
                                ccode = "USD";
                                break;
                            case DocCurrencyCode.EURO:
                                ccode = "EUR";
                                break;
                            case DocCurrencyCode.LIRA:
                                ccode = "TRL";
                                break;
                            case DocCurrencyCode.POUND:
                                ccode = "GBP";
                                break;
                        }
                    logWriter.WriteLine("1,{0:D5},91,FCC,{1},{2}", number++, ccode.PadLeft(12), "".PadLeft(12));
                    
                 }               
             }

            /**********  ITEMS  ****************/
            decimal totalAmount = 0;
            decimal documentTotalAmount = document.TotalAmount;
            decimal documentTotalVAT = document.TotalVAT;

            foreach (IFiscalItem item in document.Items)
            {
                try
                {
                    Decimal lineQuantity = 0m;
                    Decimal remainQuantity = Math.Abs(item.Quantity);

                    List<decimal> appliedAdjTotals = new List<decimal>();
                    while (remainQuantity > 0)
                    {
                        logWriter.Write("1,{0:D5},", number++);
                        if (docStatus != Connector.TEMP_DOCUMENT_STATUS)
                            lineQuantity = (Math.Min(remainQuantity, 99));
                        else
                            lineQuantity = remainQuantity;
                        int quantity = (int)lineQuantity;
                        int rem = (int)Math.Round((lineQuantity - quantity) * 1000, 0);

                        remainQuantity -= Math.Abs(lineQuantity);

                        totalAmount = Math.Round((item.TotalAmount / item.Quantity) * lineQuantity, 2);

                        if (item.Quantity >= 0)
                            logWriter.Write("04,SAT,");
                        else
                            logWriter.Write("05,IPT,");

                        string[] itemAdjustments = item.GetAdjustments();
                        foreach (string adjustment in itemAdjustments)
                            totalAmount -= Decimal.Parse(adjustment.Split('|')[0]) / item.Quantity * lineQuantity;
                        totalAmount = Math.Round(totalAmount, 2);

                        logWriter.WriteLine("{0:D2}.{1:D3}{2:D6},{3:D2}{4:D10}", Math.Abs(quantity), Math.Abs(rem),
                                                               item.Product.Id,
                                                               item.Product.Department.Id,
                                                               (long)Math.Round(100m * totalAmount, 0));

                        //Barcode 1,rrrrr,20,BKD,bbbbbbbbbbbb,bbbbbbbbbbbb
                        if (Connector.Instance().CurrentSettings.GetProgramOption(Setting.BarcodeLineInMainLogFile) == PosConfiguration.ON)
                        {
                            string barcode = item.Product.Barcode.PadRight(20, ' ');
                            string BKDLine = string.Format("1,{0},", number++.ToString().PadLeft(5, '0'));
                            BKDLine += string.Format("38,BKD,{0},{1}", barcode.Substring(0, 12), barcode.Substring(12));
                            logWriter.WriteLine(BKDLine.PadRight(40, ' '));
                        }

                        // It means fiscal printer is Vx675
                        if (!String.IsNullOrEmpty(PosConfiguration.Get("VxOrderPath")))
                        {
                            string remarkLine = string.Format("1,{0},", number++.ToString().PadLeft(5, '0'));
                            remarkLine += string.Format("22,NOT,{0}", item.Name);
                            logWriter.WriteLine(remarkLine.PadRight(40, ' '));
                        }

                        //Satici 1,rrrrr,20,STC,kkk     nnnn,
                        if (item.SalesPerson != null)
                        {
                            logWriter.WriteLine("1,{0},20,STC,{1}     {2},{3,12}", number++.ToString().PadLeft(5, '0'),
                                                        PosConfiguration.Get("RegisterId"),
                                                        item.SalesPerson.Id, " ");
                        }
                        //Seri no veya IMEI numaralarý 
                        //1,rrrrr,36,SNO,bbbbbbbbbbbb,bbbbbbbbbbbb
                        if (!String.IsNullOrEmpty(item.SerialNo))
                        {
                            string serialNo = item.SerialNo.PadRight(24, ' ');
                            logWriter.WriteLine("1,{0},36,SNO,{1},{2}", number++.ToString().PadLeft(5, '0'),
                                    serialNo.Substring(0, 12),
                                    serialNo.Substring(12).PadRight(12, ' '));
                        }

                        if (!String.IsNullOrEmpty(item.BatchNumber))
                        {//PNO , parti no
                            string batchNumber = item.BatchNumber.PadRight(24, ' ');
                            logWriter.WriteLine("1,{0},37,PNO,{1},{2}", number++.ToString().PadLeft(5, '0'),
                                    batchNumber.Substring(0, 12),
                                    batchNumber.Substring(12).PadRight(12, ' '));
                        }

                        if (!String.IsNullOrEmpty(item.ExpiryDate.ToString()) && item.ExpiryDate > DateTime.MinValue)
                        {
                            string expiryDate = item.ExpiryDate.Day.ToString() + "/";

                            if (item.ExpiryDate.Month < 10) expiryDate += "0";
                            expiryDate += item.ExpiryDate.Month + "/" +
                                item.ExpiryDate.Year.ToString();

                            expiryDate = expiryDate.PadRight(24, ' ');
                            logWriter.WriteLine("1,{0},39,SKT,{1},{2}", number++.ToString().PadLeft(5, '0'),
                                    expiryDate.Substring(0, 12),
                                    expiryDate.Substring(12).PadRight(12, ' '));
                        }

                        int index = 0;
                        foreach (string adjustment in itemAdjustments)
                        {
                            string[] detail = adjustment.Split('|');// Amount | Percentage | CashierId
                            decimal itemAdjAmount = decimal.Parse(detail[0]);
                            string direction = itemAdjAmount > 0 ? "ART" : "IND";
                            itemAdjAmount = Math.Abs(itemAdjAmount);
                            decimal lineAdjAmount = Math.Round((itemAdjAmount / item.Quantity) * lineQuantity, 2);

                            if (appliedAdjTotals.Count <= index)
                                appliedAdjTotals.Add(0);

                            if (appliedAdjTotals[index] < itemAdjAmount)
                            {
                                appliedAdjTotals[index] += lineAdjAmount;

                                decimal adjustmentDifference = itemAdjAmount - appliedAdjTotals[index];

                                if (adjustmentDifference < 0)
                                    lineAdjAmount += adjustmentDifference;

                                logWriter.WriteLine("1,{0:D5},06,{1},SNS {2} %{3},  {4:D10}",
                                                        number++,
                                                        direction,
                                                        detail[2],
                                                        detail[1],
                                                        (long)Math.Round(lineAdjAmount * 100m, 0)
                                                        );
                            }
                            index++;
                        }
                    }
                }
                catch { }

            }


            /**********  DOCUMENT ADJUSTMENTS ****************/

            Decimal usedPointPrice = 0m;
            String pointCreditValue = Connector.Instance().CurrentSettings.GetProgramOption(Setting.ConnectPointToCredit).ToString();
            Boolean convertPointToPayment = false;

            if (pointCreditValue[0] == '1' && pointCreditValue.Length == 4)
            {
                convertPointToPayment = true;
            }

            totalAmount = documentTotalAmount;
            string[] documentAdjustments = document.GetAdjustments();
            foreach (string adjustment in documentAdjustments)
            {
                string[] detail = adjustment.Split('|');// Amount | Percentage | CashierId
                decimal amount = decimal.Parse(detail[0]);
                string direction = amount > 0 ? "ART" : "IND";
                if (convertPointToPayment && detail[2] == "9998")
                {
                    usedPointPrice = Math.Abs(amount);
                    documentTotalAmount -= amount;
                    continue;
                }

                totalAmount -= amount;
                amount = Math.Abs(amount);
                logWriter.WriteLine("1,{0:D5},06,{1},TOP {2} %{3},  {4:D10}", number++, direction, detail[2], detail[1], (long)Math.Round(amount * 100m, 0));

            }

            docStatus -= 3;
            //Belge toplami 1,rrrrr,08,TOP,            ,  tttttttttt
            logWriter.WriteLine("1,{0:D5},08,TOP,           {1},  {2:D10}", number++,
                                                                       (docStatus >= 0) ? docStatus.ToString() : " ",
                                                                       (long)Math.Round(documentTotalAmount * 100m, 0));

            //Kasiyer 1,rrrrr,20,STC,kkk     nnnn,
            if (document.SalesPerson != null)
            {
                logWriter.WriteLine("1,{0:D5},20,STC,{1}     {2},            ", number++,
                                                            PosConfiguration.Get("RegisterId"),
                                                            document.SalesPerson.Id);
            }

            if (convertPointToPayment && usedPointPrice > 0)
            {
                String paymentType = pointCreditValue[1] == '0' ? "KRD" : "CHK";
                logWriter.WriteLine("1,{0:D5},10,{1},          {2:D2},  {3:D10}", number++, paymentType, pointCreditValue.Substring(2, 2), (long)Math.Round(usedPointPrice * 100m, 0));
            }

            /*****            PAYMENTS         *******/
            decimal documentBalance = document.TotalAmount;

            String[] checkpayments = document.GetCheckPayments();
            foreach (String checkpayment in checkpayments)
            {
                String[] detail = checkpayment.Split('|');// Amount | RefNumber | SequenceNo
                if (detail[1].Length > 12)
                    detail[1] = detail[1].Substring(0, 12);
                decimal amount = Math.Min(Decimal.Parse(detail[0]), documentBalance);
                logWriter.WriteLine("1,{0:D5},09,CEK,{1,12},  {2:D10}", number++, detail[1], (long)Math.Round(amount * 100m, 0));
                documentBalance = documentBalance - amount;
            }

            String[] currencypayments = document.GetCurrencyPayments();
            foreach (String currencypayment in currencypayments)
            {
                String[] detail = currencypayment.Split('|');// Amount | Exchange Rate | Name | SequenceNo
                decimal amount = Math.Min(Decimal.Parse(detail[0]), documentBalance);
                long quantity = (long)Math.Round(amount * 100m / decimal.Parse(detail[1]), 0);
                int id = 0;
                foreach (ICurrency cur in Connector.Instance().GetCurrencies().Values)
                {
                    if (cur.Name != detail[2]) continue;
                    id = cur.Id;
                    break;
                }
                logWriter.WriteLine("1,{0:D5},09,DVZ,{1}  {2:D9},  {3:D10}", number++, (char)id, quantity, (long)Math.Round(amount * 100m, 0));
                documentBalance = documentBalance - amount;
            }

            String[] creditpayments = document.GetCreditPayments();
            foreach (String creditypayment in creditpayments)
            {
                String[] detail = creditypayment.Split('|');// Amount | Installments | Id | ViaByEFT | SequenceNo
                decimal amount = Math.Min(Decimal.Parse(detail[0]), documentBalance);
                int installments = 0;
                Parser.TryInt(detail[1], out installments);
                if (installments > 0)
                {
                    logWriter.WriteLine("1,{0:D5},10,KRD,{1:D2}        {2:D2},  {3:D10}",
                                                                                   number++,
                                                                                   installments,
                                                                                   int.Parse(detail[2]),
                                                                                   (long)Math.Round(amount * 100m, 0)
                                                                                   );
                }
                else
                {
                    logWriter.WriteLine("1,{0:D5},10,KRD,          {1:D2},  {2:D10}",
                                                                                   number++,
                                                                                   int.Parse(detail[2]),
                                                                                   (long)Math.Round(amount * 100m, 0)
                                                                                   );
                }
                documentBalance = documentBalance - amount;
            }
            //cash payments must be at lasti because customerchanges always cash
            String[] cashpayments = document.GetCashPayments();
            foreach (String cashpayment in cashpayments)
            {
                String[] detail = cashpayment.Split('|');// Amount | SequenceNo
                decimal amount = Math.Min(Decimal.Parse(detail[0]), documentBalance);
                logWriter.WriteLine("1,{0:D5},09,NAK,            ,  {1:D10}", number++, (long)Math.Round(amount * 100m, 0));
                documentBalance = documentBalance - amount;
            }

            foreach (PointObject po in document.Points)
                logWriter.WriteLine("1,{0},22,PRM,{1:D2} {2:D9},{3,12:D10}", number++.ToString().PadLeft(5, '0'), 0, 0, po.Value);

            if (Connector.Instance().CurrentSettings.GetProgramOption(Setting.WriteDocumentID) == PosConfiguration.ON)
            {
                //After power shutdown on POS side. if Doc loaded from file
                if (lastZNo == 0 && document.DocumentFileZNo != 0)
                    lastZNo = document.DocumentFileZNo - 1;

                string BIDLine = PosConfiguration.Get("FiscalId") + (lastZNo + 1).ToString().PadLeft(4, '0') + document.Id.ToString().PadLeft(4, '0');
                BIDLine = string.Format("1,{0:D5},24,BID,{1},{2}", number++, BIDLine.Substring(0, 10).PadRight(12, ' '), BIDLine.Substring(10, BIDLine.Length - 10)).PadRight(40, ' ');
                logWriter.WriteLine(BIDLine);
            }
            //Special Promotion Lines produced by promotion server
            foreach (string promoLog in document.PromoLogLines)
                logWriter.WriteLine("1,{0:D5},{1}", number++, promoLog);

            //GSM NUMBER OF CUSTOMER
            if (document.Customer != null)
            {
                if (document.Customer.GsmNumber != String.Empty)
                {
                    string gsmNumber = document.Customer.GsmNumber.PadRight(24, ' ');
                    logWriter.WriteLine("1,{0:D5},50,GSM,{1},{2}", number++, gsmNumber.Substring(0, 12), gsmNumber.Substring(12, 12));
                }
            }

            //RETURN REASON OF THE DOCUMENT
            if (!String.IsNullOrEmpty(document.ReturnReason))
            {
                string returnReason = document.ReturnReason.PadRight(24, ' ');
                logWriter.WriteLine("1,{0:D5},51,IAN,{1},{2}", number++, returnReason.Substring(0, 12), returnReason.Substring(12, 12));
            }

            //TODO Belge sonu 1,rrrrr,11,SON,mmmmmmmmmmmm,mmmmmmmmssss
            logWriter.Write("1,{0:D5},11,SON,", number++);
            if (document.Customer != null)
            {
                string code = document.Customer.Code.PadRight(20);
                logWriter.Write("{0},{1}", code.Substring(0, 12), code.Substring(12, 8));
            }
            else
                logWriter.Write("            ,        ");

            logWriter.Write(document.IsOpenDocument ? "   A" : "    ");

            return logWriter;
        }

        private string[] GetSubStringsAsLen(string line, int partLen)
        {
            List<string> subStrList = new List<string>();

            if (line.Length <= partLen)
                return new string[]{ line };

            int index = 0;
            string subStr = String.Empty;
            while(index < line.Length)
            {
                if (index + partLen < line.Length)
                    subStr = line.Substring(index, partLen);
                else
                    subStr = line.Substring(index);

                subStrList.Add(subStr);
                index += partLen;
            }

            return subStrList.ToArray();
        }

    }
}
