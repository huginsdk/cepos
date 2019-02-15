using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Hugin.POS.Data;
using Hugin.POS.Common;
using System.Xml.Xsl;
using System.Xml;
using System.IO;


namespace Hugin.POS.Printer.GUI
{
    /// <summary>
    /// 
    /// </summary>
    public enum ReportType
    {
        //Add Report type in the future.
        PaymentReport,
        FPUReport
    }
    /// <summary>
    /// 
    /// </summary>
    public class SpecialReport
    {
        public const  int MaxCharsAtLine = 40;
        public static decimal CustomerChange = 0;
        public List<String> Items = null;

        public SpecialReport()
        {
            Items = new List<string>();

            String strXml = Connector.Instance().GetReportXml(FiscalPrinter.RegisterId);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXml);

            XmlNode node = xmlDoc.SelectSingleNode("Report/Sale/Document[@TypeId='-1']/Payments/Exchanges");
            if (node != null)
            {
                if (node.SelectNodes("Exchange").Count > 4)
                    xmlDoc.SelectSingleNode("Report/Sale/Document[@TypeId='-1']/Payments/Exchanges").InnerXml = AdjustOther(node, "Exchange", 3);
            }

            node = xmlDoc.SelectSingleNode("Report/Sale/Document[@TypeId='-1']/Payments/Credits");
            if (node != null)
            {
                if (node.SelectNodes("Credit").Count > 4)
                    xmlDoc.SelectSingleNode("Report/Sale/Document[@TypeId='-1']/Payments/Credits").InnerXml = AdjustOther(node, "Credit", 3);
            }

            Items.AddRange(AddVoid(xmlDoc.SelectSingleNode("Report/Void")));
            Items.AddRange(AddPayments(xmlDoc.SelectSingleNode("Report/Sale/Document[@TypeId='-1']/Payments")));
            Items.AddRange(AddSuspended(xmlDoc.SelectSingleNode("Report/Suspended")));
        }

        private List<string> AddVoid(XmlNode nodeVoids)
        {
            List<string> listVoids = new List<string>();

            XmlNode nodeVoidReceipt = nodeVoids.SelectSingleNode("Document[@TypeId='-1']");

            if (nodeVoidReceipt == null)
                listVoids.AddRange(AddLines(1));
            else
                listVoids.Add(AddEntryTotal(nodeVoidReceipt.SelectSingleNode("Summary")));

            listVoids.AddRange(AddLines(9));

            return listVoids;
        }

        private List<string> AddSuspended(XmlNode nodeSuspendeds)
        {
            List<string> listSuspendeds = new List<string>();

            XmlNode nodeSuspendedReceipt = nodeSuspendeds.SelectSingleNode("Document[@TypeId='-1']");

            if (nodeSuspendedReceipt == null)
                listSuspendeds.AddRange(AddLines(2));
            else
            {
                listSuspendeds.Add(FormatTitle("BEKLETÝLEN SATIÞ FÝÞÝ"));
                listSuspendeds.Add(AddEntryTotal(nodeSuspendedReceipt.SelectSingleNode("Summary")));
            }

            return listSuspendeds;
        }

        private List<string> AddPayments(XmlNode nodePayments)
        {
            List<string> listPayments = new List<string>();

            if (nodePayments == null)
                listPayments.AddRange(AddLines(30));
            else
            {
                listPayments.AddRange(AddExchanges(nodePayments));
                listPayments.AddRange(AddCredits(nodePayments));
            }

            return listPayments;
        }

        private List<string> AddExchanges(XmlNode nodePayments)
        {
            List<string> listExchanges = new List<string>();

            XmlNode nodeExchangeEntry = nodePayments.SelectSingleNode("Exchanges/Summary/Entry");

            if (nodeExchangeEntry != null)
            {
                int entry = 0;
                Parser.TryInt(nodeExchangeEntry.InnerText, out entry);

                if (entry > 0)
                {
                    listExchanges.AddRange(AddPaymentSummary(nodePayments.SelectSingleNode("Cash")));

                    XmlNode nodeExchanges = nodePayments.SelectSingleNode("Exchanges");
                    listExchanges.AddRange(AddPaymentSummary(nodeExchanges.SelectSingleNode("Summary")));

                    foreach (XmlNode nodeExchange in nodeExchanges.SelectNodes("Exchange"))
                    {
                        listExchanges.Add(FormatTitle(nodeExchange.SelectSingleNode("Name").InnerText));
                        entry = 0;
                        Parser.TryInt(nodeExchange.SelectSingleNode("Entry").InnerText, out entry);
                        listExchanges.Add(FormatEntryLine(entry, ToDecimal(nodeExchange.SelectSingleNode("Value").InnerText)));
                        listExchanges.Add(FormatInfoLine("TL KARÞILIÐI", ToDecimal(nodeExchange.SelectSingleNode("Total").InnerText)));
                    }

                    if (nodeExchanges.SelectSingleNode("Other") != null)
                        listExchanges.AddRange(AddOther(nodeExchanges.SelectSingleNode("Other")));
                }

            }

            listExchanges.AddRange(AddLines(16 - listExchanges.Count));

            return listExchanges;
        }

        private List<string> AddCredits(XmlNode nodePayments)
        {//to continue
            List<string> listCredits = new List<string>();

            XmlNode nodeCreditEntry = nodePayments.SelectSingleNode("Credits/Summary/Entry");
            
            if (nodeCreditEntry != null)
            {
                int entry = 0;
                Parser.TryInt(nodeCreditEntry.InnerText, out entry);

                if (entry > 0)
                {
                    XmlNode nodeCredits = nodePayments.SelectSingleNode("Credits");
                    
                    foreach (XmlNode nodeCredit in nodeCredits.SelectNodes("Credit"))
                    {
                        listCredits.Add(FormatTitle(nodeCredit.SelectSingleNode("Name").InnerText));
                        listCredits.Add(AddEntryTotal(nodeCredit));
                    }

                    if (nodeCredits.SelectSingleNode("Other") != null)
                        listCredits.AddRange(AddOther(nodeCredits.SelectSingleNode("Other")));
                }

            }

            listCredits.AddRange(AddLines(14 - listCredits.Count));

            return listCredits;
        }

        private List<string> AddOther(XmlNode nodeOther)
        {
            List<string> listOther = new List<string>();

            listOther.Add(FormatTitle("DÝÐER"));
            listOther.Add(AddEntryTotal(nodeOther));

            return listOther;
        }
        private List<string> AddPaymentSummary(XmlNode nodeSummary)
        {
            List<string> listSummary = new List<string>();

            listSummary.Add(FormatTitle(nodeSummary.SelectSingleNode("Description").InnerText));
            listSummary.Add(AddEntryTotal(nodeSummary));

            return listSummary;
        }

        private string AddEntryTotal(XmlNode nodeEntryTotal)
        {
            int entry = 0;
            Parser.TryInt(nodeEntryTotal.SelectSingleNode("Entry").InnerText, out entry);
            decimal amount = ToDecimal(nodeEntryTotal.SelectSingleNode("Total").InnerText);
            
            return FormatEntryLine(entry, amount);
        }

        private string AdjustOther(XmlNode node, string childName, int maxToShow)
        {
            String xml = node.SelectSingleNode("Summary").OuterXml;
            int entry = 0;
            decimal total = 0;
            int counter = 0;
            foreach (XmlNode childNode in node.SelectNodes(childName))
            {
                if (counter < maxToShow)
                {
                    xml += childNode.OuterXml;
                    entry += int.Parse(childNode.SelectSingleNode("Entry").InnerText);
                    total += decimal.Parse(childNode.SelectSingleNode("Total").InnerText);
                    counter++;
                }
                else
                {
                    entry = int.Parse(node.SelectSingleNode("Summary/Entry").InnerText) - entry;
                    total = decimal.Parse(node.SelectSingleNode("Summary/Total").InnerText) - total;
                    xml += "<Other>";
                    xml += "<Entry>" + entry + "</Entry>";
                    xml += "<Total>" + total + "</Total>";
                    xml += "</Other>";
                    break;
                }
            }
            return xml;
        }
        
        public List<String> AddLines(int count)
        {
            List<string> lines = new List<string>();

            if (count <= 0) return lines;

            for (int i = 0; i < count; i++)
                lines.Add(FormatTitle(""));

            return lines;
        }
        public String FormatTitle(String msg)
        {
            return String.Format("{0,-" + MaxCharsAtLine + "}", msg);
        }
        public String FormatInfoLine(String msg, Decimal amount)
        {
            return String.Format("{0,-14}{1," + (MaxCharsAtLine - 14) + "}", msg, String.Format("*{0:f}", amount));
        }
        public String FormatEntryLine(int counter, Decimal amount)
        {
            return String.Format("{0,6}{1," + (MaxCharsAtLine - 6) + "}", counter, String.Format("*{0:f}", amount));
        }

        public Decimal ToDecimal(String strValue)
        {
            decimal amount = 0;

            Parser.TryDecimal(strValue, out amount);

            return amount;
        }
    }
}

