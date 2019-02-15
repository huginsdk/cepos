using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Hugin.POS.Common;

namespace Hugin.POS.Printer.GUI
{
    class SaleReport
    {
        static int lineLength = 32;

        static bool isReturn = false;
        internal static List<String> GetSaleReport()
        {
            List<String> items = new List<string>();

            String strXml = Hugin.POS.Data.Connector.Instance().GetReportXml(FiscalPrinter.RegisterId);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXml);

            /** create items from xml **/
            //items.Add(SurroundTitle("SATIÞ RAPORU"));
            if (xmlDoc.SelectNodes("Report/Sale/Document").Count == 1)
            {
                if (xmlDoc.SelectSingleNode("Report/Sale/Document").Attributes["TypeId"].Value == "1000")
                {
                    items.AddRange(AddLines(1));
                    string msg = PosMessage.NOT_SELLING;
                    int left = (lineLength - msg.Length) / 2;
                    int right = lineLength - msg.Length - left;
                    items.Add(String.Format("{0,-" + left + "}{1}{0," + right + "}", "##", PosMessage.NOT_SELLING));
                }
                return items;
            }
            foreach (XmlNode nodeDoc in xmlDoc.SelectNodes("Report/Sale/Document"))
            {
                XmlNode nodeSummary = nodeDoc.SelectSingleNode("Summary");
                isReturn = false;
                if (nodeDoc.Attributes["TypeId"].Value.Trim() == "1")
                {
                    isReturn = true;
                }
                if (nodeDoc.Attributes["TypeId"].Value.Trim() == "3")
                {
                    continue;//waybill
                }
                items.AddRange(AddLines(1));
                if (nodeDoc.Attributes["TypeId"].Value.Trim() == "1000")
                {
                    items.Add(SurroundSubtitle(String.Format("{0} ({1})", PosMessage.TOTAL, PosMessage.RECEIPT)));
                }
                else
                {
                    items.Add(SurroundSubtitle(nodeSummary["Name"].InnerText));
                }
                items.AddRange(AddLines(1));
                items.Add(FormatQTitle(nodeSummary["Name"].InnerText));
                items.Add(FormatEntryTotal(nodeSummary));
                
                items.AddRange(AddLines(1));
                foreach (XmlNode taxGroup in nodeDoc.SelectNodes("TaxDistribution/TaxGroup"))
                {
                    items.Add(FormatTaxGroup(Convert.ToDecimal(taxGroup["TaxRate"].InnerText), Convert.ToDecimal(taxGroup["Total"].InnerText)));
                    decimal retTotal = 0;
                    foreach (XmlNode xmlDept in taxGroup.SelectNodes("Department"))
                    {
                        if (isReturn)
                        {
                            retTotal += Convert.ToDecimal(xmlDept["Tax"].InnerText);
                        }
                        else
                        {
                            items.Add(FormatDepartment(xmlDept["Name"].InnerText, Convert.ToDecimal(xmlDept["Total"].InnerText)));
                            items.Add(FormatDepartment(PosMessage.VAT, Convert.ToDecimal(xmlDept["Tax"].InnerText)));
                        }
                    }
                    if (isReturn)
                    {
                        items.Add(FormatDepartment(PosMessage.VAT, retTotal));
                    }
                }
                items.AddRange(AddLines(1));

                if (isReturn)
                {
                    //DOESN'T SUPPORTS RETURN DOCUMENT TYPE
                    items.Add(SurroundTitle(PosMessage.RETURN_PAYMENTS));
                }
                else
                {
                    items.Add(SurroundTitle(PosMessage.RECEIPT_INFORMATION));
                }

                if (nodeDoc.SelectNodes("Payments/Cash").Count > 0)
                {
                    items.AddRange(AddLines(1));

                    items.AddRange(AddPaymentSummary(nodeDoc.SelectSingleNode("Payments/Cash")));
                }
                if (nodeDoc.SelectNodes("Payments/Exchanges/Exchange").Count > 0)
                {
                    items.AddRange(AddPaymentSummary(nodeDoc.SelectSingleNode("Payments/Exchanges/Summary")));
                    foreach (XmlNode nodeExchange in nodeDoc.SelectNodes("Payments/Exchanges/Exchange"))
                        items.AddRange(FormatExchange(nodeExchange));
                }
                if (nodeDoc.SelectNodes("Payments/Check").Count > 0)
                {
                    items.AddRange(AddLines(1));

                    items.AddRange(AddPaymentSummary(nodeDoc.SelectSingleNode("Payments/Check")));
                    items.AddRange(AddLines(1));
                }
                if (nodeDoc.SelectNodes("Payments/Credits/Credit").Count > 0)
                {
                    items.AddRange(AddPaymentSummary(nodeDoc.SelectSingleNode("Payments/Credits/Summary")));
                    foreach (XmlNode nodeCredit in nodeDoc.SelectNodes("Payments/Credits/Credit"))
                        items.AddRange(FormatCredit(nodeCredit));

                    items.AddRange(AddLines(1));
                }

                items.Add(FormatInfoLine(PosMessage.TOTAL, Convert.ToDecimal(nodeSummary["Total"].InnerText)));
                if (nodeDoc.Attributes["TypeId"].Value == "2")
                    items.Add(FormatInfoLine(PosMessage.EXEMPT_TAX, Convert.ToDecimal(nodeSummary["TotalTax"].InnerText)));
                else
                    items.Add(FormatInfoLine(PosMessage.TOTAL + " " + PosMessage.VAT, Convert.ToDecimal(nodeSummary["TotalTax"].InnerText)));
            }
            
            return items;
        }

        private static string[] FormatCredit(XmlNode nodeCredit)
        {
            string[] creditLines = new string[2];
            creditLines[0] = FormatTitle(nodeCredit["Name"].InnerText);
            creditLines[1] = FormatEntryTotal(nodeCredit);
            return creditLines;
        }

        private static string[] FormatExchange(XmlNode nodeExchange)
        {
            string[] exhngLines = new string[3];
            exhngLines[0] = FormatTitle(nodeExchange["Name"].InnerText);
            exhngLines[1] = FormatEntryLine(Convert.ToInt32(nodeExchange["Entry"].InnerText), Convert.ToDecimal(nodeExchange["Value"].InnerText));
            exhngLines[2] = FormatInfoLine(PosMessage.EXCHANGE_PROVISION, Convert.ToDecimal(nodeExchange["Total"].InnerText));
            return exhngLines;
        }

        private static string[] AddPaymentSummary(XmlNode xmlPayment)
        {
            if (xmlPayment == null) return new string[] { };
            string [] summary=new string[2];
            summary[0] = FormatTitle(xmlPayment["Description"].InnerText);
            summary[1] = FormatEntryTotal(xmlPayment);
            return summary;
        }

        private static string FormatEntryTotal(XmlNode nodeTotal)
        {
            int entry = Convert.ToInt32(nodeTotal["Entry"].InnerText);
            decimal total = Convert.ToDecimal(nodeTotal["Total"].InnerText);
            return FormatEntryLine(entry, total);
        }

        public static List<String> AddLines(int count)
        {
            List<String> lines = new List<string>();
            for (int i = 0; i < count; i++)
                lines.Add("".PadLeft(lineLength));
            return lines;
        }

        public static string SurroundTitle(string title)
        {
            if (title.Length > FiscalPrinter.CHAR_PER_BOLD_LINE)
                title = title.Substring(0, FiscalPrinter.CHAR_PER_BOLD_LINE); return SurroundwithChars(title, FiscalPrinter.CHAR_PER_BOLD_LINE, '*');
        }
        public static string SurroundSubtitle(string title)
        {
            if (title.Length > lineLength)
                title = title.Substring(0, lineLength);
            return SurroundwithChars(title, lineLength, '-'); 
        }
        public static string SurroundwithChars(string title, int totalLen, char surroundingChar) 
        {
            if (totalLen > lineLength)
                totalLen = lineLength; 
            if (title.Length > totalLen)
                    title = title.Substring(0, totalLen);
            if (title.Length < totalLen - 1) 
            {
                title = " " + title + " "; 
                int left = (totalLen - title.Length) / 2;
                int right = (totalLen - title.Length) - left; title = "".PadLeft(left, surroundingChar) + title + "".PadRight(right, surroundingChar); 
            }
            return title;
        }
        public static String FormatTitle(String msg)
        {
            if (isReturn)
            {
                msg = msg.Replace(PosMessage.COLLECTION, PosMessage.RETURN_DOCUMENT);
            }
            return msg.PadRight(lineLength);
        }
        public static String FormatQTitle(String msg)
        {
            if (Str.Contains(msg, PosMessage.TOTAL))
                return FormatTitle(PosMessage.DOCUMENT_QUNTITY);
            return FormatTitle(msg + PosMessage.OF_QUNTITY);
        }
        public static String FormatInfoLine(String msg, Decimal amount)
        {
            return String.Format("{0,-14}{1," + (lineLength - 14) + "}", msg, FormatAmount(amount));
        }
        public static String FormatEntryLine(int counter, Decimal amount) 
        {
            return String.Format("{0,6}{1," + (lineLength - 6) + "}", counter, FormatAmount(amount));
        }
        public static String FormatTaxGroup(Decimal taxRate, Decimal amount)
        {
            String strFormat = PosMessage.TOTAL + " %{0,2:00} " + PosMessage.SALE;
            if (isReturn)
                strFormat = PosMessage.TOTAL + " %{0,2:00} " + PosMessage.SALE;
            return String.Format("{0,-17}{1," + (lineLength - 17) + "}", String.Format(strFormat, (int)(taxRate * 100)), FormatAmount(amount));
        }
        public static String FormatDepartment(String msg, Decimal amount)
        {
            return String.Format("  {0,-14}{1," + (lineLength - 14 - 2) + "}", msg, FormatAmount(amount)); 
        }
        public static Decimal ToDecimal(String amount) 
        {
            return Decimal.Parse(amount);
        }
        private static String FormatAmount(decimal amount)
        {
            if (isReturn)
                amount = -1 * Math.Abs(amount);
            return String.Format("*{0:N2}", amount);
        }    

    }
}
