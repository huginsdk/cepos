using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;
using System.Data;
using System.Xml;

namespace Hugin.POS.Data
{
    public class Document
    {
        private const int indexCode = 8;
        private const int indexDefn = 11;
        private const int indexPrm1 = 15;
        private const int indexPrm2 = 28;
        private static Dictionary<int, ICurrency> currencies = null;

        private static Decimal inCash = 0;
        private static Decimal outCash = 0;
        private static Dictionary<int, Document> documents = null;
        private static Document totalsDocument = null;//for totals

        private static XmlDocument report = null;

        private int documentTypeId = 0;
        private string name = "";
        private int totalEntry = 0;
        private decimal totalAmount = 0;
        private decimal totalTax = 0;

        private int cashEntry = 0;
        private decimal cashTotal = 0;

        private int checkEntry = 0;
        private decimal checkTotal = 0;

        private Dictionary<int, int> currencyEntry = new Dictionary<int, int>();//currency entries by id
        private Dictionary<int, Decimal> currencyTotal = new Dictionary<int, Decimal>();//currency totals by id

        private int currenciesEntry = 0;//entries of all currencies
        private Decimal currenciesTotal = 0;//totals of all currencies

        private Dictionary<int, int> creditEntry = new Dictionary<int, int>();//credit entries by id
        private Dictionary<int, Decimal> creditTotal = new Dictionary<int, Decimal>();//credit totals by id

        private int creditsEntry = 0;//entries of all credits
        private Decimal creditsTotal = 0;//totals of all credits

        private Dictionary<int, decimal> departmentSales = new Dictionary<int, decimal>();
        private Dictionary<int, decimal> taxGroupSales = new Dictionary<int, decimal>();
        private Dictionary<int, decimal> departmentTax = new Dictionary<int, decimal>();
        private Dictionary<int, decimal> productTotals = new Dictionary<int, decimal>();
        private Dictionary<int, int> productDepartments = new Dictionary<int, int>();

        private Document(int documentTypeId)
        {
            this.documentTypeId = documentTypeId;

            switch (documentTypeId)
            {
                case (int)DocumentTypes.RECEIPT:
                    name = PosMessage.RECEIPT;
                    break;
                case (int)DocumentTypes.INVOICE:
                    name = PosMessage.INVOICE;
                    break;
                case (int)DocumentTypes.E_INVOICE:
                    name = PosMessage.E_INVOICE;
                    break;
                case (int)DocumentTypes.E_ARCHIEVE:
                    name = PosMessage.E_ARCHIVE;
                    break;
                case (int)DocumentTypes.MEAL_TICKET:
                    name = PosMessage.MEAL_TICKET;
                    break;
                case (int)DocumentTypes.CAR_PARKING:
                    name = PosMessage.CAR_PARKIMG;
                    break;
                case (int)DocumentTypes.ADVANCE:
                    name = PosMessage.ADVANCE;
                    break;
                case (int)DocumentTypes.COLLECTION_INVOICE:
                    name = PosMessage.COLLECTION_INVOICE;
                    break;
                case (int)DocumentTypes.RETURN_DOCUMENT:
                    name = PosMessage.RETURN_DOCUMENT;
                    break;
                case (int)DocumentTypes.CURRENT_ACCOUNT_COLLECTION:
                    name = PosMessage.CURRENT_ACCOUNT_COLLECTION;
                    break;
                case (int)DocumentTypes.SELF_EMPLYOMENT_INVOICE:
                    name = PosMessage.SELF_EMPLOYEMENT_INVOICE;
                    break;
                case 1000:
                    name = PosMessage.TOTAL;
                    break;
            }
            this.productTotals = new Dictionary<int, decimal>();
            this.productDepartments = new Dictionary<int, int>();
        }

        private void AddCash(decimal amount, int entry)
        {
            cashEntry += entry;
            cashTotal += amount;
        }

        private void AddCheck(decimal amount, int entry)
        {
            checkEntry += entry;
            checkTotal += amount;
        }

        private void AddCurrency(int id, decimal total, int entry)
        {
            if (!currencyEntry.ContainsKey(id))
            {
                currencyEntry.Add(id, 0);
                currencyTotal.Add(id, 0);
            }
            currencyEntry[id] += entry;
            currencyTotal[id] += total;

            currenciesEntry += entry;
            currenciesTotal += total;
        }

        private void AddCredit(int id, decimal total, int entry)
        {
            if (!creditEntry.ContainsKey(id))
            {
                creditEntry.Add(id, 0);
                creditTotal.Add(id, 0);
            }
            creditEntry[id] += entry;
            creditTotal[id] += total;

            creditsEntry += entry;
            creditsTotal += total;
        }

        private void AddDepartmentTotal(int id, decimal amount)
        {
            if (!departmentSales.ContainsKey(id))
                departmentSales.Add(id, 0);
            departmentSales[id] += amount;

            if (!taxGroupSales.ContainsKey(Department.Departments[id].TaxGroupId))
                taxGroupSales.Add(Department.Departments[id].TaxGroupId, 0);
            taxGroupSales[Department.Departments[id].TaxGroupId] += amount;
        }
        private void AddDepartmentTax(int id, decimal amount)
        {
            if (!departmentTax.ContainsKey(id))
                departmentTax.Add(id, 0);
            departmentTax[id] += amount;
        }

        internal static void PrepareZReport(String registerId)
        {
            String[] files = Directory.GetFiles(Common.PosConfiguration.ArchivePath, "BEK*." + registerId);

            foreach (string filename in files)
            {
                File.Move(filename, filename.Replace("BEK","ZBEK"));
            }
        }
        internal static void AfterZReport(String registerId)
        {
            List<String> sortedFiles = null;
            String[] files = null;

            try
            {
                files = Directory.GetFiles(Common.PosConfiguration.ArchivePath, "ZBEK*." + registerId);
                sortedFiles = new List<string>(files);
                sortedFiles.Sort();

                String contents = "";
                String content = "";
                foreach (string filename in sortedFiles)
                {
                    content = IOUtil.ReadAllText(filename);
                    contents += content;
                    File.Delete(filename);
                }
                //contents = contents.Replace(",TOP,           2,", ",TOP,           1,").TrimEnd(new Char[] { '\r', '\n' });

                //if (!String.IsNullOrEmpty(contents) && contents.Length > 0)
                //{
                //    Logger voidLogger = new Logger(LogType.Void);
                //    voidLogger.SaveLog(contents);
                //}
            }
            catch (Exception ex)
            {
                EZLogger.Log.Error(ex);
            }
        }

        private static String GetLastZLine(String registerId)
        {
            if (File.Exists(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId))
            {

                String[] lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId);
                for (int index = lines.Length - 1; index >= 0; index--)
                {

                    if (lines[index].Trim().Length < indexCode + 2)
                        continue;
                    if (lines[index].Substring(indexCode, 2) == "16")
                        return lines[index];
                }
            }
            return "";
        }
        private static int GetLastZIndex(String [] lines, DateTime lastZDate)
        {
            if (lastZDate == DateTime.MinValue) return 0;
            int lastZIndex = 0;
            DateTime tempZDate = DateTime.Now;
            for (int index = lines.Length - 1; index >= 0; index--)
            {
                if (lines[index].Trim().Length < indexCode + 2)
                    continue;
                if (lines[index].Substring(indexCode, 2) == "16")
                    return index;
                if (lines[index].Substring(indexCode, 2) != "03")
                    continue;

                tempZDate=DateTime.Parse(lines[index].Substring(indexPrm1, 10) + " " + lines[index].Substring(indexPrm2, 8),
                                    PosConfiguration.CultureInfo,
                                    System.Globalization.DateTimeStyles.NoCurrentDateDefault);
                if (lastZDate > tempZDate)
                {
                    if (lastZIndex == 0)
                        lastZIndex = lines.Length;
                    break;
                }
                lastZIndex = index - 1;
            }
            return lastZIndex;
        }
        internal static void LoadCurrencies(Dictionary<int, ICurrency> originialCurrencies)
        {
            currencies = new Dictionary<int, ICurrency>();
            foreach (int key in originialCurrencies.Keys)
                currencies.Add((int)(originialCurrencies[key].Name[0]), originialCurrencies[key]);
        }

        private static void Reset()
        {
            inCash = 0;
            outCash = 0;
            documents = new Dictionary<int, Document>();
            totalsDocument = new Document(1000);
        }
        private static void LoadSaleDocuments(string registerId)
        {
            if (!File.Exists(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId)) return;
            
            int lastZIndex = 0;
            DateTime lastZDate = DateTime.MinValue;

            String[] lines = null;

            String lastZLine = GetLastZLine(registerId);
            if (lastZLine.Length > 0)
                lastZDate = DateTime.Parse(lastZLine.Substring(indexPrm1, 10) + " " + lastZLine.Substring(indexPrm2, 8));

            lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId);
            lastZIndex = GetLastZIndex(lines, lastZDate);
            LoadSale(lines, lastZIndex);
        }

        private static void LoadReturnDocuments(string registerId)
        {
            if (!File.Exists(Common.PosConfiguration.ArchivePath + "HRIADE." + registerId)) return;

            int lastZIndex = 0;
            DateTime lastZDate = DateTime.MinValue;

            String[] lines = null;

            String lastZLine = GetLastZLine(registerId);
            if (lastZLine.Length > 0)
                lastZDate = DateTime.Parse(lastZLine.Substring(indexPrm1, 10) + " " + lastZLine.Substring(indexPrm2, 8));

            lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HRIADE." + registerId);
            lastZIndex = GetLastZIndex(lines, lastZDate);
            LoadSale(lines, lastZIndex);
        }

        internal static decimal GetRegisterCash(String registerId)
        {
            Reset();
            LoadSaleDocuments(registerId);
            return totalsDocument.cashTotal + inCash - outCash;
        }

        internal static int GetLastDocumentId(string registerId)
        {
            int lastDocId = 0;
            List<string> files = new List<string>();
            files.Add(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId);
            files.Add(Common.PosConfiguration.ArchivePath + "HRIPTAL." + registerId);
            files.AddRange(Directory.GetFiles(Common.PosConfiguration.ArchivePath, "BEK*." + registerId));
            
            foreach (string fName in files)
            {
                int temp=GetLastDocId(fName);
                lastDocId = lastDocId < temp ? temp : lastDocId;
            }
            return lastDocId;
        }

        private static int GetLastDocId(string fileName)
        {
            if (File.Exists(fileName))
            {
                String[] lines = IOUtil.ReadAllLines(fileName);
                for (int index = lines.Length - 1; index >= 0; index--)
                {

                    if (lines[index].Trim().Length < indexCode + 2)
                        continue;
                    try
                    {
                        if ((lines[index].Substring(indexCode, 2) == "01") || (lines[index].Substring(indexCode, 2) == "02"))
                            return int.Parse(lines[index].Substring(indexPrm2));
                    }
                    catch { }
                }
            }
            return 0;

        }

        private static void LoadVoid(String[] lines, int lastZIndex)
        {
            LoadVoid(lines, lastZIndex, "", DateTime.MinValue, DateTime.MaxValue);
        }

        private static void LoadVoid(String[] lines, int lastZIndex, String cashierId, DateTime dtStart,DateTime dtEnd)
        {
            Document sdoc = null;
            String line = "";
            for (int index = lastZIndex; index < lines.Length; index++)
            {
                line = lines[index].Trim();
                if (line.Length < indexCode + 2) continue;

                switch (line.Substring(indexCode, 2))
                {
                    case "01"://FIS
                        if (cashierId == "" || line.Substring(indexPrm1 + 8, 4) == cashierId)
                            sdoc = new Document(-1);
                        else
                            sdoc = null;
                        break;
                    case "02"://FAT,IAD,DIP,IRS
                    case "24"://GPS
                        if (cashierId == "" || line.Substring(indexPrm1 + 8, 4) == cashierId)
                            sdoc = new Document(GetDocumentTypeId(line.Substring(indexDefn, 3)));
                        else
                            sdoc = null;
                        break;
                    case "03"://TAR
                        if (dtStart == DateTime.MinValue) continue;
                        DateTime docDay = DateTime.Parse(line.Substring(indexPrm1, 10) + " " + line.Substring(indexPrm2, 8),
                                                            PosConfiguration.CultureInfo,
                                                            System.Globalization.DateTimeStyles.NoCurrentDateDefault);
                        if (!(dtStart <= docDay && dtEnd >= docDay)) sdoc = null;
                        break;
                    case "08"://TOP
                    case "30"://FIP
                        if (sdoc == null) continue;
                        sdoc.totalAmount = Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;
                        break;
                    case "11"://SON
                        if (sdoc == null) continue;
                        if (!documents.ContainsKey(sdoc.documentTypeId))
                            documents.Add(sdoc.documentTypeId, new Document(sdoc.documentTypeId));

                        documents[sdoc.documentTypeId].totalAmount += sdoc.totalAmount;
                        documents[sdoc.documentTypeId].totalEntry++;

                        totalsDocument.totalAmount += sdoc.totalAmount;
                        totalsDocument.totalEntry++;

                        sdoc = null;
                        break;
                    default:
                        continue;
                }

            }
        }

        private static void LoadSale(string[] lines, int lastZIndex)
        {
            LoadSale(lines, lastZIndex, "", DateTime.MinValue, DateTime.MaxValue);
        }


        private static void LoadSale(String[] lines, int lastZIndex,String cashierId,DateTime dtStart, DateTime dtEnd)
        {
            Document sdoc = null;
            String line = "";
            int lastDepartmentId = 0;
            int lastProductId = 0;
            decimal amount = 0;
            for (int index = lastZIndex; index < lines.Length; index++)
            {
                try
                {
                    line = lines[index];
                    if (line.Trim().Length < indexCode + 2) continue;
                    switch (line.Substring(indexCode, 2))
                    {
                        case "01"://FIS
                            if (cashierId == "" || line.Substring(indexPrm1 + 8, 4) == cashierId)
                                sdoc = new Document(-1);
                            else
                                sdoc = null;
                            break;
                        case "02"://FAT,IAD,DIP,IRS
                        case "24"://GPS (Return document - inter file pattern)
                            if (Logger.LogFormatter is HuginLogger && line.Substring(indexCode, 2) == "24") continue;
                            if (cashierId == "" || line.Substring(indexPrm1 + 8, 4) == cashierId)
                                sdoc = new Document(GetDocumentTypeId(line.Substring(indexDefn, 3)));
                            else
                                sdoc = null;
                            break;
                        case "03"://TAR
                            if (dtStart == DateTime.MinValue) continue;
                            DateTime docDay = DateTime.Parse(line.Substring(indexPrm1, 10) + " " + line.Substring(indexPrm2, 8),
                                                                PosConfiguration.CultureInfo,
                                                                System.Globalization.DateTimeStyles.NoCurrentDateDefault);
                            if (!(dtStart <= docDay && dtEnd >= docDay)) sdoc = null;
                            break;
                        case "04"://SAT
                        case "05"://IPT
                        case "25"://GAL (inter file pattern)
                            if (sdoc == null) continue;
                            if (Logger.LogFormatter is HuginLogger && line.Substring(indexCode, 2) == "25") continue;
                            lastDepartmentId = int.Parse(line.Substring(indexPrm2, 2)) - 1;
                            lastProductId = int.Parse(line.Substring(21, 6));
                            amount = Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;
                            if (line.Substring(indexCode, 2) == "05") amount = -1 * amount;//IPT

                            sdoc.totalAmount += amount;
                            sdoc.AddProductTotals(lastProductId, amount, lastDepartmentId);
                            sdoc.AddDepartmentTotal(lastDepartmentId, amount);

                            break;
                        case "06"://IND or ART
                        case "39"://ART (inter file pattern)
                            if (sdoc == null) continue;
                            if (Logger.LogFormatter is HuginLogger && line.Substring(indexCode, 2) == "39") continue;

                            amount = Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;

                            if (line.Substring(indexDefn, 3) == "IND")//discount
                                amount *= -1;

                            if (line.Substring(indexPrm1, 3) == "SNS") //Adjustment on last sale
                            {
                                sdoc.departmentSales[lastDepartmentId] += amount;
                                sdoc.taxGroupSales[Department.Departments[lastDepartmentId].TaxGroupId] += amount;
                                sdoc.productTotals[lastProductId] += amount;
                            }
                            else//adjustment on document
                            {
                                decimal maxSale = 0;
                                int maxSaleDept = 0;
                                decimal totalDeptAdjustment = 0;
                                

                                int[] keyCollection = new int[sdoc.departmentSales.Count];
                                sdoc.departmentSales.Keys.CopyTo(keyCollection, 0);
                                foreach (int dept in keyCollection)
                                {
                                    decimal deptAdjustment = Math.Round(amount * (sdoc.departmentSales[dept] / sdoc.totalAmount), 2);
                                    sdoc.departmentSales[dept] += deptAdjustment;
                                    sdoc.taxGroupSales[Department.Departments[dept].TaxGroupId] += deptAdjustment;

                                    totalDeptAdjustment += deptAdjustment;

                                    if (sdoc.departmentSales[dept] > maxSale)
                                    {
                                        maxSale = sdoc.departmentSales[dept];
                                        maxSaleDept = dept;
                                    }
                                }
                                if (amount != totalDeptAdjustment)
                                {
                                    sdoc.departmentSales[maxSaleDept] += amount - totalDeptAdjustment;
                                    sdoc.taxGroupSales[Department.Departments[maxSaleDept].TaxGroupId] += amount - totalDeptAdjustment;
                                }

                                /*int[] keys = new int[sdoc.taxGroupSales.Keys.Count];
                                sdoc.taxGroupSales.Keys.CopyTo(keys, 0);
                                //adjust departments 
                                foreach (int id in keys)
                                {
                                    decimal deptAdjustment = Math.Round(amount * (sdoc.taxGroupSales[id] / sdoc.totalAmount), 2);
                                    //productDeptNo = sdoc.productDepartments[id];
                                    //sdoc.departmentSales[productDeptNo] += deptAdjustment;
                                    sdoc.taxGroupSales[id] += deptAdjustment;
                                    totalDeptAdjustment += deptAdjustment;
                                    if (Math.Abs(deptAdjustment) > Math.Abs(maxSale))
                                    {
                                        maxSale = deptAdjustment;
                                        maxSaleDept = id;
                                    }
                                }

                                if (amount != totalDeptAdjustment)
                                {
                                    //sdoc.taxGroupSales[maxSaleDept] += amount - totalDeptAdjustment;
                                    sdoc.taxGroupSales[Department.Departments[maxSaleDept].TaxGroupId] += amount - totalDeptAdjustment;
                                }*/
                            }
                            sdoc.totalAmount += amount;
                            break;
                        case "08"://TOP
                        case "30"://FIP (inter file pattern)
                            if (sdoc == null) continue;
                            if (Logger.LogFormatter is InterLoger)
                            {
                                sdoc.totalAmount = Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;
                            }
                            break;
                        case "09"://Payment
                            if (sdoc == null) continue;
                            decimal paymentAmount = Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;
                            switch (line.Substring(indexDefn, 3))
                            {
                                case "NAK":
                                    sdoc.AddCash(paymentAmount, 1);
                                    break;
                                case "DVZ":
                                    sdoc.AddCurrency((int)(line[indexPrm1]), paymentAmount, 1);
                                    break;
                                case "CEK":
                                    sdoc.AddCheck(paymentAmount, 1);
                                    break;
                            }
                            break;
                        case "10"://KRD
                            if (sdoc == null) continue;
                            sdoc.AddCredit(int.Parse(line.Substring(indexPrm1 + 10, 2)), Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100, 1);
                            break;
                        case "11"://SON
                            if (sdoc == null) continue;
                            try
                            {
                                decimal tax = 0;
                                
                                int multiplier = 1;
                                int entryMultiplier = 1;

                                if (sdoc.documentTypeId == 1)//return document
                                {
                                    multiplier = -1;
                                    entryMultiplier = 1;
                                }
                                if (sdoc.documentTypeId == 3)//waybill
                                {
                                    multiplier = 0;
                                    entryMultiplier = 0;
                                }

                                int taxId = 0;
                                Dictionary<int, decimal> taxGroupVat = new Dictionary<int, decimal>();
                                Dictionary<int, decimal> depTotalVat = new Dictionary<int, decimal>();
                                Dictionary<int, decimal> maxDeptSale = new Dictionary<int, decimal>();
                                Dictionary<int, int> maxDept = new Dictionary<int, int>();
                                // calculates departments, taxgroups and totalvat
                                foreach (int department in sdoc.departmentSales.Keys)
                                {
                                    tax = sdoc.departmentSales[department] * Department.Departments[department].TaxRate;
                                    if (sdoc.documentTypeId != 2)
                                        tax = tax / (1 + Department.Departments[department].TaxRate);
                                    sdoc.AddDepartmentTax(department, RoundDecimal(tax, 2));

                                    taxId = Department.Departments[department].TaxGroupId;
                                    if (!taxGroupVat.ContainsKey(taxId))
                                    {
                                        tax = Department.Departments[department].TaxRate;
                                        tax = (sdoc.taxGroupSales[taxId] * tax) / (1 + Department.Departments[department].TaxRate);
                                        taxGroupVat.Add(taxId, RoundDecimal(tax, 2));
                                    }

                                    if (!depTotalVat.ContainsKey(taxId))
                                        depTotalVat.Add(taxId, 0);
                                    depTotalVat[taxId] += sdoc.departmentTax[department];

                                    if (!maxDeptSale.ContainsKey(taxId))
                                    {
                                        maxDeptSale.Add(taxId, sdoc.departmentSales[department]);
                                        maxDept[taxId] = department;
                                    }
                                    else if (maxDeptSale[taxId] < sdoc.departmentSales[department])
                                    {
                                        maxDeptSale[taxId] = sdoc.departmentSales[department];
                                        maxDept[taxId] = department;
                                    }
                                }

                                decimal adj = 0;
                                foreach (int taxgrpId in taxGroupVat.Keys)
                                {
                                    adj = taxGroupVat[taxgrpId] - depTotalVat[taxgrpId];
                                    if (adj == 0) continue;
                                    sdoc.AddDepartmentTax(maxDept[taxgrpId], adj);
                                }
                                if (!documents.ContainsKey(sdoc.documentTypeId))
                                    documents.Add(sdoc.documentTypeId, new Document(sdoc.documentTypeId));

                                documents[sdoc.documentTypeId].totalAmount += sdoc.totalAmount;
                                totalsDocument.totalAmount += multiplier * sdoc.totalAmount;

                                documents[sdoc.documentTypeId].totalEntry++;
                                if (multiplier != 0)
                                    totalsDocument.totalEntry++;

                                documents[sdoc.documentTypeId].AddCash(sdoc.cashTotal, sdoc.cashEntry);
                                totalsDocument.AddCash(multiplier * sdoc.cashTotal, entryMultiplier * sdoc.cashEntry);

                                documents[sdoc.documentTypeId].AddCheck(sdoc.checkTotal, sdoc.checkEntry);
                                totalsDocument.AddCheck(multiplier * sdoc.checkTotal, entryMultiplier * sdoc.checkEntry);

                                foreach (int currencyId in sdoc.currencyTotal.Keys)
                                {
                                    documents[sdoc.documentTypeId].AddCurrency(currencyId, sdoc.currencyTotal[currencyId], sdoc.currencyEntry[currencyId]);
                                    totalsDocument.AddCurrency(currencyId, multiplier * sdoc.currencyTotal[currencyId], entryMultiplier * sdoc.currencyEntry[currencyId]);
                                }

                                foreach (int creditId in sdoc.creditTotal.Keys)
                                {
                                    documents[sdoc.documentTypeId].AddCredit(creditId, sdoc.creditTotal[creditId], sdoc.creditEntry[creditId]);
                                    totalsDocument.AddCredit(creditId, multiplier * sdoc.creditTotal[creditId], entryMultiplier * sdoc.creditEntry[creditId]);
                                }

                                foreach (int department in sdoc.departmentSales.Keys)
                                {
                                    documents[sdoc.documentTypeId].AddDepartmentTotal(department, sdoc.departmentSales[department]);
                                    totalsDocument.AddDepartmentTotal(department, multiplier * sdoc.departmentSales[department]);
                                }

                                foreach (int department in sdoc.departmentTax.Keys)
                                {
                                    documents[sdoc.documentTypeId].AddDepartmentTax(department, sdoc.departmentTax[department]);
                                    totalsDocument.AddDepartmentTax(department, multiplier * sdoc.departmentTax[department]);
                                }
                                //Calculate totaltax
                                foreach (int taxgrpId in sdoc.taxGroupSales.Keys)
                                {
                                    documents[sdoc.documentTypeId].totalTax += taxGroupVat[taxgrpId];
                                    totalsDocument.totalTax += taxGroupVat[taxgrpId];
                                }

                                sdoc = null;
                            }
                            catch
                            {
                            }
                            break;
                        case "12"://KCK
                            outCash += Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;
                            break;
                        case "13"://KGR
                            inCash += Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;
                            break;
                        case "20"://CEK (inter file pattern)
                            if (sdoc == null) continue;
                            if (Logger.LogFormatter is InterLoger)
                            {
                                paymentAmount = Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;
                                sdoc.AddCheck(paymentAmount, 1);
                            }
                            break;
                        case "21"://DVZ (inter file pattern)
                            if (sdoc == null) continue;
                            if (Logger.LogFormatter is InterLoger)
                            {
                                paymentAmount = Decimal.Parse(line.Substring(indexPrm2 + 2, 10)) / 100;
                                sdoc.AddCurrency((int)(line[indexPrm1]), paymentAmount, 1);
                            }
                            break;
                        default:
                            continue;
                    }
                }
                catch (Exception ex)
                {
					Log(ex);
                }
            }
        }
		
		private static void Log (Exception ex)
		{
			/*
			 * this function is implemented to catch warnings
			 * which caused fraom unreferenced "ex" variables
			 */
		}
        private static decimal RoundDecimal(decimal tax, int digit)
        {
            return decimal.Parse(String.Format("{0:0." + "0".PadLeft(digit, '0') + "}", tax));
        }

        private void AddProductTotals(int id, decimal amount, int lastDepartmentId)
        {
            if (!productTotals.ContainsKey(id))
                productTotals.Add(id, 0);
            productTotals[id] += amount;

            if (!productDepartments.ContainsKey(id))
                productDepartments.Add(id, lastDepartmentId);
        }

        private static int GetDocumentTypeId(string type)
        {
            switch (type)
            {
                //case PosMessage.HR_CODE_INVOICE: return 0;
                //case PosMessage.HR_CODE_RETURN:
                //case PosMessage.HR_INTER_CODE_RETURN: return 1;
                //case "DIP": return 2;
                //case PosMessage.HR_CODE_WAYBILL: return 3;
                //default: return 0;

                case PosMessage.HR_CODE_ADVANCE:
                    return (int)DocumentTypes.ADVANCE;
                case PosMessage.HR_CODE_CAR_PARKING:
                    return (int)DocumentTypes.CAR_PARKING;
                case PosMessage.HR_CODE_COLLECTION_INVOICE:
                    return (int)DocumentTypes.COLLECTION_INVOICE;
                case PosMessage.HR_CODE_CURRENT_ACCOUNT_COLLECTION:
                    return (int)DocumentTypes.CURRENT_ACCOUNT_COLLECTION;
                case PosMessage.HR_CODE_E_ARCHIVE:
                    return (int)DocumentTypes.E_ARCHIEVE;
                case PosMessage.HR_CODE_E_INVOICE:
                    return (int)DocumentTypes.E_INVOICE;
                case PosMessage.HR_CODE_INVOICE:
                    return (int)DocumentTypes.INVOICE;
                case PosMessage.HR_CODE_MEAL_TICKET:
                    return (int)DocumentTypes.MEAL_TICKET;
                case PosMessage.HR_CODE_RECEIPT:
                    return (int)DocumentTypes.RECEIPT;
                case PosMessage.HR_CODE_RETURN:
                    return (int)DocumentTypes.RETURN_DOCUMENT;
                case PosMessage.HR_CODE_SELF_EMPLOYEMENT_INVOICE:
                    return (int)DocumentTypes.SELF_EMPLYOMENT_INVOICE;
                default:
                    return (int)DocumentTypes.RECEIPT;
            }
        }

        private static string CreateDocumentsNode(Dictionary<int, Document> documents)
        {
            string strXml = "";
            foreach (int key in documents.Keys)
                strXml += CreateDocumentNode(documents[key]);

            strXml += CreateDocumentNode(totalsDocument);//and totals

            documents.Clear();
            totalsDocument = new Document(1000);

            return strXml;
        }

        private static string CreateDocumentNode(Document document)
        {
            string strXml = "";

            strXml += "<Document TypeId=\"" + document.documentTypeId + "\">";
            strXml += "<Summary>";
            strXml += "<Name>" + document.name + "</Name>";
            strXml += "<Entry>" + document.totalEntry + "</Entry>";
            strXml += "<Total>" + document.totalAmount + "</Total>";
            strXml += "<TotalTax>" + document.totalTax + "</TotalTax>";
            strXml += "</Summary>";

            //department details

            strXml += "<TaxDistribution>";
            foreach (int taxgroup in document.taxGroupSales.Keys)
            {
                strXml += "<TaxGroup>";
                strXml += "<TaxRate>" + Department.TaxRates[taxgroup] + "</TaxRate>";
                strXml += "<Total>" + document.taxGroupSales[taxgroup] + "</Total>";
                strXml += CreateDepartmentsNode(taxgroup, document.departmentSales, document.departmentTax);
                strXml += "</TaxGroup>";
            }
            strXml += "</TaxDistribution>";

            //Payments
            strXml += "<Payments>";

            //cash
            if (document.cashEntry != 0)
            {
                strXml += "<Cash>";
                strXml += "<Description>" + PosMessage.CASH + " ( " + PosMessage.TURKISH_LIRA + " ) </Description>";
                strXml += "<Entry>" + document.cashEntry + "</Entry>";
                strXml += "<Total>" + document.cashTotal + "</Total>";
                strXml += "</Cash>";
            }

            //check 
            if (document.checkEntry != 0)
            {
                strXml += "<Check>";
                strXml += "<Description>" + PosMessage.CHECK + "</Description>";
                strXml += "<Entry>" + document.checkEntry + "</Entry>";
                strXml += "<Total>" + document.checkTotal + "</Total>";
                strXml += "</Check>";
            }
            
            //currency
            if (document.currenciesEntry != 0)
            {
                strXml += "<Exchanges>";
                strXml += "<Summary>";
                strXml += "<Description>" + PosMessage.CURRENCY_RECEIPT + "</Description>";
                strXml += "<Entry>" + document.currenciesEntry + "</Entry>";
                strXml += "<Total>" + document.currenciesTotal + "</Total>";
                strXml += "</Summary>";
                foreach (int id in document.currencyEntry.Keys)
                {
                    if (!currencies.ContainsKey(id)) continue;
                    if (document.currencyEntry[id] == 0) continue;

                    ICurrency currency = currencies[id];
                    strXml += "<Exchange>";
                    strXml += "<Name>" + currency.Name + "</Name>";
                    strXml += "<Entry>" + document.currencyEntry[id] + "</Entry>";
                    strXml += "<Value>" + Math.Round(document.currencyTotal[id] / currency.ExchangeRate, 2) + "</Value>";
                    strXml += "<Total>" + document.currencyTotal[id] + "</Total>";
                    strXml += "</Exchange>";
                }
                strXml += "</Exchanges>";
            }

            Dictionary<int, ICredit> credits = Connector.Instance().GetCredits();
            //credit
            if (document.creditsEntry != 0)
            {
                strXml += "<Credits>";
                strXml += "<Summary>";
                strXml += "<Description>" + PosMessage.CREDIT_RECEIPT + "</Description>";
                strXml += "<Entry>" + document.creditsEntry + "</Entry>";
                strXml += "<Total>" + document.creditsTotal + "</Total>";
                strXml += "</Summary>";
                foreach (int id in document.creditEntry.Keys)
                {
                    if (document.creditEntry[id] == 0) continue;

                    ICredit credit = credits[id];
                    strXml += "<Credit>";
                    strXml += "<Name>" + credit.Name + "</Name>";
                    strXml += "<Entry>" + document.creditEntry[id] + "</Entry>";
                    strXml += "<Total>" + document.creditTotal[id] + "</Total>";
                    strXml += "</Credit>";
                }
                strXml += "</Credits>";
            }

            strXml += "</Payments>";


            strXml += "</Document>";

            return strXml;
        }

        private static string CreateDepartmentsNode(int taxgroup, Dictionary<int, decimal> departmentSales, Dictionary<int, decimal> departmentTax)
        {
            string strXml = "";

            foreach (int key in departmentSales.Keys)
            {
                if (!(Department.Departments[key].TaxGroupId == taxgroup))
                    continue;
                strXml += "<Department>";
                strXml += "<Name>" + Department.Departments[key].Name + "</Name>";
                strXml += "<Total>" + departmentSales[key] + "</Total>";
                strXml += "<Tax>" + departmentTax[key] + "</Tax>";
                strXml += "</Department>";
            }
            return strXml;
        }

        public override bool Equals(object obj)
        {
            if (obj is Document)
            {
                Document doc = (Document)obj;
                if (doc.documentTypeId == this.documentTypeId)
                    return true;
            }
            return false;
        }
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

        internal static string GetReportXml(string registerId, DateTime day)
        {
            DateTime dtStart = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);
            DateTime dtEnd = new DateTime(day.Year, day.Month, day.Day, 23, 59, 0);

            return GetReportXml(registerId, "", dtStart, dtEnd);
        }

        internal static string[] GetReturnAmounts(string registerId)
        {
            List<string> amountList = new List<string>();

            Reset();
            LoadReturnDocuments(registerId);
            
            foreach(KeyValuePair<int,Document> kvp in documents)
            {
                if(kvp.Value.documentTypeId == (int)DocumentTypes.RETURN_DOCUMENT)
                {
                    amountList.Add(kvp.Value.totalEntry.ToString());
                    amountList.Add(kvp.Value.totalAmount.ToString());
                }
            }

            return amountList.ToArray();
        }

        internal static string GetReportXml(string registerId, string cashierCode, DateTime firstDate, DateTime lastDate)
        {
            Reset();

            report = new XmlDocument();
            String strXml = "";
            strXml += "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
            strXml += "<Report>";
            strXml += "<Header></Header>";
            strXml += "<Sale></Sale>";
            strXml += "<Void></Void>";
            strXml += "<Suspended></Suspended>";
            strXml += "<ZSuspended></ZSuspended>";
            strXml += "</Report>";

            report.LoadXml(strXml);

            //Header
            List<String> lstHeader=new List<String>();
            if (!String.IsNullOrEmpty(cashierCode))
            {
                ICashier c = Cashier.FindById(cashierCode);
                if (c != null)
                    lstHeader.Add(PosMessage.CASHIER + " : " + c.Name);
                lstHeader.Add(String.Format("{0:dd/MM/yyyy HH:mm} - {1:dd/MM/yyyy HH:mm}", firstDate, lastDate));
            }
            else
            {
                lstHeader.Add(String.Format("{0:dd/MM/yyyy HH:mm}", firstDate));
            }
            foreach(String line in lstHeader)
            {
                report.SelectSingleNode("Report/Header").InnerXml += String.Format("<Line><Value>{0}</Value></Line>", line);
            }

            String[] lines = null;

            if (File.Exists(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId))
            {
                lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId);
                LoadSale(lines, 0, cashierCode, firstDate, lastDate);
                //KARSILIGI OLMAYAN MALI IADE ALMA
                if (Connector.Instance().CurrentSettings.GetProgramOption(Setting.AddOnlyReturnFile) == PosConfiguration.ON)
                {
                    if (File.Exists(Common.PosConfiguration.ArchivePath + "HRIADE." + registerId))
                    {
                        lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HRIADE." + registerId);
                        LoadSale(lines, 0, cashierCode, firstDate, lastDate);
                    }
                }
                String innerXml = CreateDocumentsNode(documents);
                report.SelectSingleNode("Report/Sale").InnerXml = innerXml.Replace("&", "&amp;");
            }

            if (File.Exists(Common.PosConfiguration.ArchivePath + "HRIPTAL." + registerId))
            {
                lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HRIPTAL." + registerId);
                LoadVoid(lines, 0, cashierCode, firstDate, lastDate);
                report.SelectSingleNode("Report/Void").InnerXml = CreateDocumentsNode(documents);
            }
            //load suspended

            String[] files = Directory.GetFiles(Common.PosConfiguration.ArchivePath, "BEK*." + registerId);
            foreach (string filename in files)
                LoadVoid(IOUtil.ReadAllLines(filename), 0, cashierCode, firstDate, lastDate);
            if (files.Length > 0)
                report.SelectSingleNode("Report/Suspended").InnerXml = CreateDocumentsNode(documents);

            String[] zSuspends = Directory.GetFiles(Common.PosConfiguration.ArchivePath, "ZBEK*." + registerId);
            foreach (string filename in zSuspends)
                LoadVoid(IOUtil.ReadAllLines(filename), 0, cashierCode, firstDate, lastDate);
            if (zSuspends.Length > 0)
                report.SelectSingleNode("Report/ZSuspended").InnerXml = CreateDocumentsNode(documents);

            return report.InnerXml;

        }

        internal static string GetReportXml(String registerId)
        {
            Reset();

            report = new XmlDocument();
            String strXml = "";
            strXml += "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
            strXml += "<Report>";
            strXml += "<Sale></Sale>";
            strXml += "<Void></Void>";
            strXml += "<Suspended></Suspended>";
            strXml += "<ZSuspended></ZSuspended>";
            strXml += "</Report>";

            report.LoadXml(strXml);

            int lastZIndex = 0;
            DateTime lastZDate = DateTime.MinValue;
            String[] lines = null;

            if (File.Exists(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId))
            {
                String lastZLine = GetLastZLine(registerId);
                if (lastZLine.Length > 0)
                    lastZDate = DateTime.Parse(lastZLine.Substring(indexPrm1, 10) + " " + lastZLine.Substring(indexPrm2, 8),
                                        PosConfiguration.CultureInfo,
                                        System.Globalization.DateTimeStyles.NoCurrentDateDefault);

                lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId);
                lastZIndex = GetLastZIndex(lines, lastZDate);
                LoadSale(lines, lastZIndex);
                //KARSILIGI OLMAYAN MALI IADE ALMA
                if (Connector.Instance().CurrentSettings.GetProgramOption(Setting.AddOnlyReturnFile) == PosConfiguration.ON)
                {
                    if (File.Exists(Common.PosConfiguration.ArchivePath + "HRIADE." + registerId))
                    {
                        lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HRIADE." + registerId);
                        lastZIndex = GetLastZIndex(lines, lastZDate);
                        LoadSale(lines, lastZIndex);
                    }
                }
                String innerXml = CreateDocumentsNode(documents);
                report.SelectSingleNode("Report/Sale").InnerXml = innerXml.Replace("&", "&amp;");
            }

            if (File.Exists(Common.PosConfiguration.ArchivePath + "HRIPTAL." + registerId))
            {
                lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HRIPTAL." + registerId);
                lastZIndex = GetLastZIndex(lines, lastZDate);
                LoadVoid(lines, lastZIndex);
                report.SelectSingleNode("Report/Void").InnerXml = CreateDocumentsNode(documents);
            }
            //load suspended
            String[] files = Directory.GetFiles(Common.PosConfiguration.ArchivePath, "BEK*." + registerId);
            foreach (string filename in files)
                LoadVoid(IOUtil.ReadAllLines(filename), 0);
            if (files.Length > 0)
                report.SelectSingleNode("Report/Suspended").InnerXml = CreateDocumentsNode(documents);

            String[] zSuspends = Directory.GetFiles(Common.PosConfiguration.ArchivePath, "ZBEK*." + registerId);
            foreach (string filename in zSuspends)
                LoadVoid(IOUtil.ReadAllLines(filename), 0);
            if (zSuspends.Length > 0)
                report.SelectSingleNode("Report/ZSuspended").InnerXml = CreateDocumentsNode(documents);

            return report.InnerXml;

        }

        internal static void LoadBarcodeListFile()
        {
            string registerId = PosConfiguration.Get("RegisterId");
            int lastZIndex = 0;
            DateTime lastZDate = DateTime.MinValue;
            String[] lines = null;
            SortedList<int, string> barcodeList = new SortedList<int, string>();
            if (File.Exists(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId))
            {
                String lastZLine = GetLastZLine(registerId);
                if (lastZLine.Length > 0)
                    lastZDate = DateTime.Parse(lastZLine.Substring(indexPrm1, 10) + " " + lastZLine.Substring(indexPrm2, 8),
                                        PosConfiguration.CultureInfo,
                                        System.Globalization.DateTimeStyles.NoCurrentDateDefault);

                lines = IOUtil.ReadAllLines(Common.PosConfiguration.ArchivePath + "HAREKET." + registerId);
                lastZIndex = GetLastZIndex(lines, lastZDate);

                String line = "";
                int barcode = 0;

                for (int index = lastZIndex; index < lines.Length; index++)
                {
                    line = lines[index].Trim();
                    if (line.Length < indexCode + 2) continue;

                    switch (line.Substring(indexCode, 2))
                    {
                        case "04"://SAT
                        case "05"://IPT
                        case "25"://GAL (inter file pattern)
                            if (Logger.LogFormatter is HuginLogger && line.Substring(indexCode, 2) == "25") continue;
                            try
                            {
                                barcode = int.Parse(Product.FindByLabel(line.Substring(21, 6).Trim()).Barcode);
                                if (barcodeList.IndexOfKey(barcode) < 0)
                                    barcodeList.Add(barcode, line.Substring(21, 6).Trim());
                            }
                            catch
                            {
                            }
                            break;
                        default:
                            continue;
                    }

                }
            }
            //clean the RAM
            lines = new string[0];
            StringBuilder soldProducts = new StringBuilder();
            Int32 count = 0;
            foreach (KeyValuePair<int, string> kvp in barcodeList)
            {
                soldProducts.Append(String.Format("{0,-6},{1,-20}\r\n", kvp.Value, kvp.Key));

                //May be it results out of memory exception, so write ram to file and then clear the ram
                if (count >= 5000)
                {
                    IOUtil.WriteAllText(PosConfiguration.DataPath + Settings.BarcodeFile, soldProducts.ToString());
                    soldProducts = new StringBuilder();
                    count = 0;
                }
                count++;
            }

            IOUtil.WriteAllText(PosConfiguration.DataPath + Settings.BarcodeFile, soldProducts.ToString());

        }

        internal static int GetLastSlipNo(string registerId)
        {
            int lastSlipNo = 0;
            lastSlipNo = FindLastSlipNo(Common.PosConfiguration.ArchivePath + Logger.MainLogName + "." + registerId);

            if (Connector.Instance().CurrentSettings.GetProgramOption(Setting.AddOnlyReturnFile) == PosConfiguration.ON)
            {
                int lastInvoiceNo = FindLastSlipNo(Common.PosConfiguration.ArchivePath + Logger.ReturnsLogName + "." + registerId);
                lastSlipNo = Math.Max(lastSlipNo, lastInvoiceNo);
            }

            int lastVoidNo = FindLastSlipNo(Common.PosConfiguration.ArchivePath + Logger.VoidedLogName + "." + registerId);
            lastSlipNo = Math.Max(lastSlipNo, lastVoidNo);

            return lastSlipNo;
        }

        private static int FindLastSlipNo(string filePath)
        {
            int slipNo = 0;
            String lastSlipLine = String.Empty;
            
            try
            {
                if (File.Exists(filePath))
                {

                    String[] lines = IOUtil.ReadAllLines(filePath);
                    for (int index = lines.Length - 1; index >= 0; index--)
                    {

                        if (lines[index].Trim().Length < indexCode + 2)
                            continue;
                        if (lines[index].Substring(indexCode, 2) == "16")
                            break;
                        if (lines[index].Substring(indexCode, 2) == "02")
                        {
                            lastSlipLine = lines[index];
                            break;
                        }
                        if (PosConfiguration.Get("Logger") == "1" && lines[index].Substring(indexCode, 2) == "24")
                        {
                            lastSlipLine = lines[index];
                            break;
                        }

                    }
                }

                if (lastSlipLine != String.Empty)
                {
                    Parser.TryInt(lastSlipLine.Substring(indexPrm2, 6), out slipNo);
                }
            }
            catch
            {
                EZLogger.Log.Error("Belge numaras� okuma hatas�.");
            }

            return slipNo;
        }
    }
}
