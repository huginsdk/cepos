using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Hugin.POS.Common;
using Hugin.POS.Data;
using Hugin.UBLCommon;
using Hugin.UBLManager;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS
{
    public class EDocumentManager
    {

        private static string EInvoiceXMLPath = AppDomain.CurrentDomain.BaseDirectory + "EInvoice.xml";
        const string HUGIN_TITLE_CODE = "HGN";

        //static EArchiveWSF eArchiveWSF = new EArchiveWSF();
        //static EInvoiceWSF eInvoiceWSF = new EInvoiceWSF();
        static UBLManager.UBLManager manager = new UBLManager.UBLManager();

        internal static bool CheckTaxPayerStatus(string tcknVkn)
        {
            //HuginWSFunctions.WS_Result result = WSFunctions.WS_EfaturaKullanicisi(tcknVkn);

            //if (!String.IsNullOrEmpty(result.ResultDetails))
            //{
            //    throw new Exception("E-BELGE SERVİS\nHATASI");
            //}
            //else
            //{
            //    string res = result.ResultObject.ToString();
            //    return Boolean.Parse(res);
            //}

            // Until e-doc developments finished
            return true;
        }

        internal static void SendEDocument(string[] lines)
        {
            //HuginInvoice hInvoice = new HuginInvoice();
            //HuginInvoice.Line hLine = null;

            //int docTypeCode = new EArchive().DocumentTypeId;

            //// Set VAT rates before send
            //List<int> rateList = new List<int>();
            ////int vat;
            ////foreach(decimal rate in cr.DataConnector.CurrentSettings.TaxRates)
            ////{
            ////    if (rate >= 0 && int.TryParse(rate.ToString(), out vat))
            ////        rateList.Add(vat); 
            ////}
            //foreach(Department dept in cr.DataConnector.CurrentSettings.Departments)
            //{
            //    if(dept != null && cr.DataConnector.CurrentSettings.TaxRates[dept.TaxGroupId] != -1)
            //        rateList.Add(Convert.ToInt32(cr.DataConnector.CurrentSettings.TaxRates[dept.TaxGroupId]));
            //}
            //manager.SetVATRates(rateList);

            //foreach(string line in lines)
            //{
            //    hLine = new HuginInvoice.Line(line, hInvoice, out hInvoice);

            //    if(hLine.LineTypeCode == 2)
            //    {
            //        docTypeCode = hLine.LineTypeDef == "EFA" ? new EInvoice().DocumentTypeId : new EArchive().DocumentTypeId;
            //    }
            //}

            //Dictionary<int, string> prodNameDic = new Dictionary<int, string>();
            //foreach(InvoiceLine line in hInvoice.InvoiceLineList)
            //{
            //    int pluNo = Convert.ToInt32(line.ItemIdentificationID);
            //    IProduct product = cr.DataConnector.FindProductByLabel(String.Format("{0:D6}", pluNo));

            //    if (!prodNameDic.ContainsKey(pluNo) && product != null)
            //        prodNameDic.Add(pluNo, product.Name);
            //}

            //manager.ImportHuginInvoice(hInvoice);
            //List<string> eDocLines = manager.GetEDocumentLines(prodNameDic);
            //manager.WriteUBLtoXML(EInvoiceXMLPath);

            //byte[] veri = File.ReadAllBytes(EInvoiceXMLPath);
            //byte[] gelen;
            //using (StreamReader sr = new StreamReader(EInvoiceXMLPath))
            //{
            //    gelen = Encoding.UTF8.GetBytes(sr.ReadToEnd());
            //}
            //string hash = CommonWSF.GetMD5Hash(gelen);

            //string tcknVkn = String.Empty;
            //AccountingParty supplier = Connector.Instance().CurrentSettings.SupplierInfo;
            //// Tckn/Vkn
            //if (!String.IsNullOrEmpty(supplier.TCKN_VKN))
            //    tcknVkn = supplier.TCKN_VKN;
            //else
            //    throw new Exception("SATICI TCKN/VKN\nBULUNAMADI");

            //WS_Result result =  WSFunctions.WS_BelgeGonder(tcknVkn, "FATURA_UBL", hInvoice.UUID, veri, hash, "application/xml", "1.2");

            //cr.Printer.PrintEDocument(docTypeCode, eDocLines.ToArray());

            //if (!String.IsNullOrEmpty(result.ResultObject.ToString()))
            //    DisplayAdapter.Cashier.Show(result.ResultObject.ToString());
        }

        internal static List<String> GetEDocumentLines(string[] lines)
        {
            HuginInvoice hInvoice = new HuginInvoice();
            HuginInvoice.Line hLine = null;

            int docTypeCode = new EArchive().DocumentTypeId;

            // Set VAT rates before send
            List<int> rateList = new List<int>();

            foreach (Department dept in cr.DataConnector.CurrentSettings.Departments)
            {
                if (dept != null && cr.DataConnector.CurrentSettings.TaxRates[dept.TaxGroupId] != -1)
                    rateList.Add(Convert.ToInt32(cr.DataConnector.CurrentSettings.TaxRates[dept.TaxGroupId - 1]));
            }
            manager.SetVATRates(rateList);

            foreach (string line in lines)
            {
                hLine = new HuginInvoice.Line(line, hInvoice, out hInvoice);

                if (hLine.LineTypeCode == 2)
                {
                    docTypeCode = hLine.LineTypeDef == "EFA" ? new EInvoice().DocumentTypeId : new EArchive().DocumentTypeId;
                }
            }

            Dictionary<int, string> prodNameDic = new Dictionary<int, string>();
            foreach (InvoiceLine line in hInvoice.InvoiceLineList)
            {
                int pluNo = Convert.ToInt32(line.ItemIdentificationID);
                IProduct product = cr.DataConnector.FindProductByLabel(String.Format("{0:D6}", pluNo));

                if (!prodNameDic.ContainsKey(pluNo) && product != null)
                    prodNameDic.Add(pluNo, product.Name);
            }

            manager.ImportHuginInvoice(hInvoice);

            return manager.GetEDocumentLines(prodNameDic);
        }
    }
}
