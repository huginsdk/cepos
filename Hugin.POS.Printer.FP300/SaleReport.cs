using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    class SaleReport
    {
        internal static List<String> GetSaleReport()
        {
            String strXml = Hugin.POS.Data.Connector.Instance().GetReportXml(FiscalPrinter.RegisterId);

            return GetXmlReport(strXml, false);
        }

        internal static List<String> GetSaleReport(String cashierId, DateTime firstDate, DateTime lastDate, bool onlyTotals)
        {
            String strXml = Hugin.POS.Data.Connector.Instance().GetReportXml(
                                FiscalPrinter.RegisterId, cashierId, firstDate, lastDate);

            return GetXmlReport(strXml, onlyTotals);
        }

        internal static List<string> GetSaleReport(DateTime day)
        {
            String strXml = Hugin.POS.Data.Connector.Instance().GetReportXml(FiscalPrinter.RegisterId, day);

            return GetXmlReport(strXml, false);
        }

        private static List<string> GetXmlReport(string strXml, bool onlyTotals)
        {
            List<String> items = new List<string>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXml);

            String strXsl = GetSaleReportXsl(onlyTotals);
            XmlDocument xslDoc = new XmlDocument();
            xslDoc.LoadXml(strXsl);

            XslCompiledTransform xtrans = new XslCompiledTransform();
            String tmpfile = AppDomain.CurrentDomain.BaseDirectory + String.Format("{0:ddMMyyHHmmss}.txt", DateTime.Now);
            FileStream fs = null;

            try
            {
                XsltSettings xslt_settings = new XsltSettings();
                xslt_settings.EnableScript = true;

                XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc.SelectSingleNode("/"));
                XmlNodeReader xslReader = new XmlNodeReader(xslDoc.SelectSingleNode("/"));

                fs = File.Open(tmpfile, FileMode.Create);

                xtrans.Load(xslReader, xslt_settings, new XmlUrlResolver());
                xtrans.Transform(xmlReader, new XsltArgumentList(), fs);

                fs.Close();

                items.AddRange(File.ReadAllLines(tmpfile));

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Contains("char27"))
                    {
                        items[i] = items[i].Replace("char27", (char)27 + "");
                    }
                    if (items[i].Contains("char1"))
                    {
                        items[i] = items[i].Replace("char1", (char)1 + "");
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (fs != null)
                    fs.Close();

                if (File.Exists(tmpfile))
                    File.Delete(tmpfile);
            }

            if (items.Count == 1)
            {
                int lineLength = 48;
                items.Add("".PadRight(lineLength));
                string msg = "SATIÞ YOK";
                int left = (lineLength - msg.Length) / 2;
                int right = lineLength - msg.Length - left;
                items.Add(String.Format("{0,-" + left + "}{1}{0," + right + "}", "##", "SATIÞ YOK"));
            }
            return items;

        }
        private static string GetSaleReportXsl(bool onlyTotals)
        {
            string strXsl = "";
            strXsl += "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
            strXsl += "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:msxsl=\"urn:schemas-microsoft-com:xslt\" xmlns:csharp=\"http://csharp.org\" xmlns:fn=\"http://www.w3.org/2005/xpath-functions\">";
            strXsl += "  <xsl:output method=\"text\" omit-xml-declaration=\"yes\" />";
            strXsl += "";
            strXsl += "  <msxsl:script language=\"C#\" implements-prefix=\"csharp\">";
            strXsl += "    <msxsl:using namespace=\"System.Text\" />";
            strXsl += "    <![CDATA[";
            strXsl += "    ";
            strXsl += "        public String Newline()";
            strXsl += "        {";
            strXsl += "            return \"\\n\";";
            strXsl += "        }";
            strXsl += "        public String AddLines(int count)";
            strXsl += "        {";
            strXsl += "            if(count<=0) return \"\";";
            strXsl += "            ";
            strXsl += "            string lines=\"\";";
            strXsl += "            for(int i=0;i<count; i++)";
            strXsl += "               lines += FormatTitle(\"\");";
            strXsl += "            return lines;";
            strXsl += "        }";
            strXsl += "        public string SurroundTitle(string title)";
            strXsl += "        {";
            strXsl += "            return \"\".PadRight(1) + SurroundwithChars(title, 20,'*') + \"\".PadRight(1)+ Newline();";
            strXsl += "";
            strXsl += "        }";
            strXsl += "        public string SurroundSubtitle(string title)";
            strXsl += "        {";
            strXsl += "            return SurroundwithChars(title, 48, '-')+ Newline();";
            strXsl += "        }";
            strXsl += "        public string SurroundwithChars(string title, int totalLen, char paddingChar)";
            strXsl += "        {";
            strXsl += "            if (title.Length < totalLen-1)";
            strXsl += "            {";
            strXsl += "                title = \" \" + title + \" \";";
            strXsl += "                int left = (totalLen - title.Length) / 2;";
            strXsl += "                int right = (totalLen - title.Length) - left;";
            strXsl += "                title = \"\".PadLeft(left, paddingChar) + title + \"\".PadRight(right, paddingChar);";
            strXsl += "            }";
            strXsl += "            return title;";
            strXsl += "        }";
            strXsl += "        public string PaymentTitle()";
            strXsl += "        {";
            strXsl += "            return \"\".PadRight(10) + \"char\"+27 + \"E\" + \"char\"+1 + \"TAHSÝLAT BÝLGÝLERÝ\" + \"char\"+27 + \"E\" + \"\".PadRight(10)+ Newline();";
            strXsl += "";
            strXsl += "        }";
            strXsl += "        public String FormatTitle(String msg)";
            strXsl += "        {";
            strXsl += "            return String.Format(\"{0,-48}\", msg)+ Newline();";
            strXsl += "        }";
            strXsl += "        public String FormatQTitle(String msg)";
            strXsl += "        {";
            strXsl += "            if(msg.Contains(\"TOPLAM\"))";
            strXsl += "                   return FormatTitle(\"BELGE ADEDÝ\");";
            strXsl += "            return FormatTitle(msg + \" ADEDÝ\");";
            strXsl += "        }";
            strXsl += "        public String FormatInfoLine(String msg, Decimal amount)";
            strXsl += "        {";
            strXsl += "            return String.Format(\"{0,-22}{1,26}\", msg, String.Format(\"*{0:f}\", amount))+ Newline();";
            strXsl += "        }";
            strXsl += "        public String FormatEntryLine(int counter, Decimal amount)";
            strXsl += "        {";
            strXsl += "            return String.Format(\"{0,14}{1,34}\", counter, String.Format(\"*{0:f}\", amount))+ Newline();";
            strXsl += "        }";
            strXsl += "        ";
            strXsl += "        public String FormatTaxGroup(Decimal taxRate, Decimal amount)";
            strXsl += "        {";
            strXsl += "            return String.Format(\"{0,-33}{1,15}\",String.Format(\"TOPLAM %{0,2} SATIÞ\",(int)(taxRate*100)),String.Format(\"*{0:f}\",amount)) + Newline();";
            strXsl += "        }";
            strXsl += "        public String FormatDepartment(String msg, Decimal amount)";
            strXsl += "        {";
            strXsl += "            return String.Format(\"     {0,-28}{1,15}\", msg, String.Format(\"*{0:f}\", amount)) + Newline();";
            strXsl += "        }";
            strXsl += "        public Decimal ToDecimal(String amount)";
            strXsl += "        {";
            strXsl += "            return Decimal.Parse(amount);";
            strXsl += "        }";
            strXsl += "    ]]>";
            strXsl += "  </msxsl:script>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"/Report\">";
            strXsl += "    <xsl:value-of select=\"csharp:SurroundTitle('SATIÞ RAPORU')\"/>";
            strXsl += "    <xsl:apply-templates select=\"Header\"/>";

            if (!onlyTotals)
            {
                strXsl += "    <xsl:apply-templates select=\"Sale/Document[@TypeId='-1']\"/>";
                strXsl += "    <xsl:apply-templates select=\"Sale/Document[@TypeId='0']\"/>";
                strXsl += "    <xsl:apply-templates select=\"Sale/Document[@TypeId='1']\"/>";
                strXsl += "    <xsl:apply-templates select=\"Sale/Document[@TypeId='2']\"/>";
                strXsl += "    <xsl:apply-templates select=\"Sale/Document[@TypeId='3']\"/>";
                strXsl += "    <xsl:if test=\"count(Sale/Document)>1\">";
                strXsl += "      <xsl:apply-templates select=\"Sale/Document[@TypeId='1000']\"/>";
                strXsl += "    </xsl:if>";
            }
            else
            {
                strXsl += "      <xsl:apply-templates select=\"Sale/Document[@TypeId='1000']\"/>";
            }

            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Document\">";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    <xsl:for-each select=\"Summary\">";
            strXsl += "      <xsl:value-of select=\"csharp:SurroundSubtitle(Name)\"/>";
            strXsl += "      <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "      <xsl:value-of select=\"csharp:FormatQTitle(Name)\"/>";
            strXsl += "      <xsl:call-template name=\"EntryTotal\"/>";
            strXsl += "    </xsl:for-each>";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    <xsl:apply-templates select=\"TaxDistribution/TaxGroup\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:PaymentTitle()\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    <xsl:apply-templates select=\"Payments\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:FormatInfoLine('TOPLAM',csharp:ToDecimal(Summary/Total))\"/>";
            strXsl += "    <xsl:if test=\"@TypeId='2'\">";
            strXsl += "      <xsl:value-of select=\"csharp:FormatInfoLine('MUAF KDV',csharp:ToDecimal(Summary/TotalTax))\"/>      ";
            strXsl += "    </xsl:if>";
            strXsl += "    <xsl:if test=\"@TypeId!='2'\">";
            strXsl += "    <xsl:value-of select=\"csharp:FormatInfoLine('TOPLAM KDV',csharp:ToDecimal(Summary/TotalTax))\"/>";
            strXsl += "    </xsl:if>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"TaxGroup\">";
            strXsl += "    <xsl:value-of select=\"csharp:FormatTaxGroup(csharp:ToDecimal(TaxRate),csharp:ToDecimal(Total))\"/>";
            strXsl += "    <xsl:apply-templates select=\"Department\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Department\">";
            strXsl += "    <xsl:value-of select=\"csharp:FormatDepartment(Name,csharp:ToDecimal(Total))\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:FormatDepartment('KDV',csharp:ToDecimal(Tax))\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Payments\">";
            strXsl += "    <xsl:apply-templates select=\"Cash\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    <xsl:if test=\"Exchanges/Summary/Entry>0\">";
            strXsl += "      <xsl:apply-templates select=\"Exchanges\"/>";
            strXsl += "    </xsl:if>";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    <xsl:apply-templates select=\"Check\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    <xsl:if test=\"count(Credits/Summary/Entry)>0\">";
            strXsl += "      <xsl:apply-templates select=\"Credits\"/>";
            strXsl += "    </xsl:if>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Cash\">";
            strXsl += "    <xsl:call-template name=\"PaymentSummary\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Exchanges\">";
            strXsl += "    <xsl:for-each select=\"Summary\">";
            strXsl += "      <xsl:call-template name=\"PaymentSummary\"/>";
            strXsl += "    </xsl:for-each>";
            strXsl += "    <xsl:apply-templates select=\"Exchange\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Check\">";
            strXsl += "    <xsl:call-template name=\"PaymentSummary\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "  ";
            strXsl += "  <xsl:template match=\"Credits\">";
            strXsl += "    <xsl:for-each select=\"Summary\">";
            strXsl += "      <xsl:call-template name=\"PaymentSummary\"/>";
            strXsl += "    </xsl:for-each>";
            strXsl += "    <xsl:apply-templates select=\"Credit\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Header\">";
            strXsl += "    <xsl:for-each select=\"Line\">";
            strXsl += "    <xsl:value-of select=\"csharp:FormatTitle(Value)\"/>";
            strXsl += "    </xsl:for-each>";
            strXsl += "    <xsl:apply-templates select=\"Credit\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Exchange\">";
            strXsl += "    <xsl:value-of select=\"csharp:FormatTitle(Name)\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:FormatEntryLine(Entry,csharp:ToDecimal(Value))\"/>";
            strXsl += "    <xsl:value-of select=\"csharp:FormatInfoLine('TL KARÞILIÐI',csharp:ToDecimal(Total))\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Credit\">";
            strXsl += "    <xsl:value-of select=\"csharp:FormatTitle(Name)\"/>";
            strXsl += "    <xsl:call-template name=\"EntryTotal\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template name=\"PaymentSummary\">";
            strXsl += "    <xsl:value-of select=\"csharp:FormatTitle(Description)\"/>";
            strXsl += "    <xsl:call-template name=\"EntryTotal\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template name=\"EntryTotal\">";
            strXsl += "    <xsl:value-of select=\"csharp:FormatEntryLine(Entry,csharp:ToDecimal(Total))\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Void\">";
            strXsl += "    <xsl:if test=\"count(Document[@TypeId='-1']/Summary)>0\">";
            strXsl += "      <xsl:for-each select=\"Document[@TypeId='-1']/Summary\">";
            strXsl += "        <xsl:call-template name=\"EntryTotal\"/>";
            strXsl += "      </xsl:for-each>";
            strXsl += "    </xsl:if>";
            strXsl += "    <xsl:if test=\"count(Document[@TypeId='-1']/Summary)=0\">";
            strXsl += "      <xsl:value-of select=\"csharp:AddLines(1)\"/>";
            strXsl += "    </xsl:if>";
            strXsl += "    <xsl:value-of select=\"csharp:AddLines(9)\"/>";
            strXsl += "  </xsl:template>";
            strXsl += "";
            strXsl += "  <xsl:template match=\"Suspended\">";
            strXsl += "    <xsl:if test=\"count(Document[@TypeId='-1']/Summary)>0\">";
            strXsl += "      <xsl:for-each select=\"Document[@TypeId='-1']/Summary\">";
            strXsl += "        <xsl:value-of select=\"csharp:FormatTitle('BEKLETÝLEN SATIÞ FÝÞÝ')\"/>";
            strXsl += "        <xsl:call-template name=\"EntryTotal\"/>";
            strXsl += "      </xsl:for-each>";
            strXsl += "    </xsl:if>";
            strXsl += "    <xsl:if test=\"count(Document[@TypeId='-1']/Summary)=0\">";
            strXsl += "      <xsl:value-of select=\"csharp:AddLines(2)\"/>";
            strXsl += "    </xsl:if>";
            strXsl += "  </xsl:template>";
            strXsl += "  ";
            strXsl += "</xsl:stylesheet>";

            return strXsl;
        }

    }
}
