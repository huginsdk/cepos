using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using Newtonsoft.Json;
using System.IO;

namespace Hugin.POS.Printer
{

    class ReceiptPrinter : FiscalPrinter, IFiscalPrinter
    {
        private static int printedItemCount = 0;
        private static bool isAfterNoRollEx = false;
        //private static bool isAfterInvalidPayment = false;
        private new event OnMessageHandler OnMessage;
        public new event EventHandler DocumentRequested;

        private JSONDocument jsonDocument = null;

        event OnMessageHandler IFiscalPrinter.OnMessage
        {
            add
            {
                OnMessage += value;
                base.OnMessage += value;
            }
            remove
            {
                OnMessage -= value;
                base.OnMessage -= value;
            }
        }

        public new void WaitFixPrinter()
        {
            while (true)
            {
                try
                {
                    CheckPrinterStatus();
                    break;
                }
                catch (PrinterException pe)
                {
                    if (pe is BlockingException)
                    {
                        throw pe;
                    }
                    if (pe is ClearRequiredException)
                    {
                        try
                        {
                            InterruptReport();
                        }
                        catch (System.Exception)
                        {

                        }
                    }
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        public IPrinterResponse PrintHeader(ISalesDocument salesDocument)
        {

            CPResponse response = new CPResponse(FiscalPrinter.CompactPrinter.CheckPrinterStatus());

            if (response.FPUState != State.IDLE)
            {
                RefreshConnection();
                response = new CPResponse(FiscalPrinter.CompactPrinter.CheckPrinterStatus());
            }

            /*
            * PRINT HEADER
            */
            if (response.FPUState == State.IDLE && salesDocument.DocumentTypeId != (int)DocumentTypes.RETURN_DOCUMENT)
            {
                try
                {
                    response = (CPResponse)PrintDocumentHeader(salesDocument);
                }
                catch (PrinterException pe)
                {
                    if (OnMessage != null)
                        OnMessage(this, new OnMessageEventArgs(pe)); ;
                    WaitFixPrinter();
                }
                FiscalPrinter.LineNumber = 0;
            }
            return response;
        }

        public IPrinterResponse PrintFooter(ISalesDocument document, bool isReceipt)
        {
            jsonDocument = new JSONDocument();
            bool hasEFT = false;

            try
            {
                CPResponse response = new CPResponse(FiscalPrinter.CompactPrinter.CheckPrinterStatus());

                if (response.FPUState == State.IDLE && isAfterNoRollEx == true)
                {
                    isAfterNoRollEx = false;
                    return response;
                }
                if (response.FPUState == State.IDLE && isAfterInvalidPayment == true)
                    isAfterInvalidPayment = false;

                Document = document;

                /*
                 * PRINT HEADER
                 */
                if (response.FPUState == State.IDLE)
                {
#if !DEMO_MODE
                    try
                    { 
                        if((document.DocumentTypeId == (int)DocumentTypes.SELF_EMPLYOMENT_INVOICE ||
                            document.DocumentTypeId == (int)DocumentTypes.INVOICE ||
                            document.DocumentTypeId == (int)DocumentTypes.E_INVOICE ||
                            document.DocumentTypeId == (int)DocumentTypes.E_ARCHIEVE ||
                            document.DocumentTypeId == (int)DocumentTypes.RETURN_DOCUMENT) &&
                            !isReceipt)
                        {
                            string[] invoicesLines = null;
                            if(document.DocumentTypeId == (int)DocumentTypes.E_ARCHIEVE ||
                                document.DocumentTypeId == (int)DocumentTypes.E_INVOICE)
                            {
                                invoicesLines = document.ReturnReason.Split('|');
                            }
                            else
                                invoicesLines = GetInvoiceLines(document);

                            // Write a copy as JSON Sale file at local
                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("InvoiceLines.txt", false))
                            {
                                foreach (string s in invoicesLines)
                                    sw.WriteLine(s);
                            }

                            int docType = document.DocumentTypeId;
                            if (document.DocumentTypeId == (int)DocumentTypes.RETURN_DOCUMENT)
                                docType = 9;

                            return (CPResponse)PrintEDocument(document.DocumentTypeId, invoicesLines);
                        }

                        // Ýade belgesi hariciden yazdýrýlýyorsa bilgi fiþi bas ayrýl
                        if (document.DocumentTypeId == (int)DocumentTypes.RETURN_DOCUMENT &&
                            DataConnector.CurrentSettings.GetProgramOption(Setting.PrintInvoicesInternal) == PosConfiguration.OFF)
                            return (CPResponse)PrintDocumentHeader(document);

                        response = (CPResponse)PrintDocumentHeader(document);


                        // Otopark ise header yazdýr ve ayrýl
                        if (document.DocumentTypeId == (int)DocumentTypes.CAR_PARKING)
                            return response;

                        // Avans ve Fatura tahsilatý ise ödeme türüne bak
                        if (document.DocumentTypeId == (int)DocumentTypes.ADVANCE || document.DocumentTypeId == (int)DocumentTypes.COLLECTION_INVOICE)
                        {
                            List<PaymentInfo> payments = FiscalPrinter.GetPayments(Document);

                            //Ödeme yok ise ayrýl
                            if (payments.Count == 0)
                                return response;
                        }
                    }
                    catch (PrinterException pe)
                    {
                        if (OnMessage != null)
                            OnMessage(this, new OnMessageEventArgs(pe));
                        WaitFixPrinter();
                    }
                }

                response = new CPResponse(FiscalPrinter.CompactPrinter.CheckPrinterStatus());
                decimal subTotal = 0.0m;
                decimal paidTotal = 0.0m;
                try
                {
                    response = new CPResponse(FiscalPrinter.CompactPrinter.PrintSubtotal(true));
                    if (response.ErrorCode == 0)
                    {
                        subTotal = Decimal.Parse(response.GetNextParam());
                        paidTotal = Decimal.Parse(response.GetNextParam());  
                    }
                }
                catch 
                {
                    if (response.FPUState == State.OPEN_SALE && paidTotal == 0)
                        hasEFT = true;
                }
                if (paidTotal != 0 && paidTotal < subTotal)
                    hasEFT = true;

                if (response.FPUState == State.SELLING || response.FPUState == State.INVOICE || response.FPUState == State.PAYMENT)
                {
                    if (paidTotal == 0 && document.DocumentTypeId != 6 && document.DocumentTypeId != 7 && document.DocumentTypeId != 9)
                    {
                        if (!FiscalPrinter.isAfterInvalidPayment)
                        {                              
#endif
                            //PRINT FISCAL ITEMS
                            response = PrintItems(Document);

                            // PRINT SUBTOTAL                        
                            List<string> docAdjustments = new List<string>(Document.GetAdjustments());
                            if (docAdjustments.Count > 0)
                            {
                                //response = new CPResponse(FiscalPrinter.CompactPrinter.PrintSubtotal());
                                response = PrintDocumentAdjustments(docAdjustments);
                            }
                            
                        }
                    }

                    // PRINT PAYMENTS
                    {
                        response = PrintPayments(Document, paidTotal);
                    }

                }
                // PRINT FOOTER NOTES
                if (Document.FootNote.Count > 0)
                {
                    foreach (string line in Document.FootNote)
                    {
                        jsonDocument.FooterNotes.Add(line);
                    }
                }             

                // Checking subtotal adjustmenst
                if(jsonDocument.Adjustments.Count > 1)
                {
                    Adjustment totalAdj = new Adjustment();

                    foreach(Adjustment adj in jsonDocument.Adjustments)
                    {
                        if(adj.Type == AdjustmentType.Discount || adj.Type == AdjustmentType.PercentDiscount)
                        {
                            if(adj.Amount > 0)
                                totalAdj.Amount -= adj.Amount;
                            else
                                totalAdj.Amount += adj.Amount;
                        }
                        else
                        {
                            if(adj.Amount < 0)
                                totalAdj.Amount -= adj.Amount;
                            else
                                totalAdj.Amount += adj.Amount;
                        }
                        
                    }

                    if (totalAdj.Amount > 0)
                        totalAdj.Type = AdjustmentType.Fee;
                    else
                    {
                        totalAdj.Type = AdjustmentType.Discount;
                        totalAdj.Amount *= Decimal.MinusOne;
                    }

                    jsonDocument.Adjustments = new List<Adjustment>() { totalAdj };
                }

                // SEND JSON DOCUMENT
                jsonDocument.EndOfReceiptInfo.CloseReceiptFlag = false;
                String jsonStr = JsonConvert.SerializeObject(jsonDocument);

                // Write a copy as JSON Sale file at local
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("JSONSaleDoc.txt", false))
                {
                    sw.WriteLine(jsonStr);
                }    

                decimal totalPaymentAmount = 0.0m;
                foreach (PaymentInfo pi in jsonDocument.Payments)
                {
                    totalPaymentAmount += pi.PaidTotal;
                }

                try
                {
                    response = new CPResponse(FiscalPrinter.CompactPrinter.PrintSubtotal(true));

                    if(response.ParamCount == 1 && Convert.ToDecimal(response.GetNextParam()) == 0)
                    {
                        if(totalPaymentAmount == document.TotalAmount)
                        {
                            // DRAWER
                            if (FiscalPrinter.CanOpenDrawer(document))
                                OpenDrawer();
                        }
                    }
                    else
                    {
                        subTotal = Decimal.Parse(response.GetNextParam());
                        paidTotal = Decimal.Parse(response.GetNextParam());

                        if (subTotal == paidTotal + totalPaymentAmount)
                        {
                            // DRAWER
                            if (FiscalPrinter.CanOpenDrawer(document))
                                OpenDrawer();
                        }
                    }                   
                }
                catch { }               

                if (jsonDocument.Payments.Count > 0 &&
                    totalPaymentAmount == document.TotalAmount &&
                    document.DocumentTypeId != 6 && document.DocumentTypeId != 7 && document.DocumentTypeId != 9 &&
                    !FiscalPrinter.CompactPrinter.IsVx675 &&
                    jsonDocument.FiscalItems.Count > 0 &&
                    jsonDocument.FiscalItems.Count < 128)
                {
                    // if document completed send as sales document
                    // SEND SALES DOCUMENT
                    response = new CPResponse(FiscalPrinter.CompactPrinter.PrintSalesDocument(jsonStr));
                }
                else
                {
                    // DEPT SALE
                    response = new CPResponse(FiscalPrinter.CompactPrinter.PrintJSONDocumentDeptOnly(jsonStr));
                }


                // CLOSE DOCUMENT
                foreach (PaymentInfo pi in jsonDocument.Payments)
                {
                    if (pi.viaByEFT)
                        hasEFT = true;
                }
                jsonDocument = null;

                response = new CPResponse(FiscalPrinter.CompactPrinter.CheckPrinterStatus());
                if (response.FPUState == State.OPEN_SALE)
                {
                    try
                    {
                        if (isAfterNoRollEx)
                            hasEFT = false;

                        response = new CPResponse(FiscalPrinter.CompactPrinter.CloseReceipt(hasEFT));
                    }
                    catch (PrinterException pe)
                    {
                        if (OnMessage != null)
                            OnMessage(this, new OnMessageEventArgs(pe));
                        WaitFixPrinter();
                    }
                }

                isAfterNoRollEx = false;
                isAfterInvalidPayment = false;
                printedItemCount = 0;

                return response;
            }
            catch (NegativeResultException)
            {
                throw new NegativeResultException();
            }
            catch (InvalidPLUNoException pe)
            {
                throw pe;
            }
            catch (NoReceiptRollException nre)
            {
                isAfterNoRollEx = true;
                throw nre;
            }
            catch (ClearRequiredException cre)
            {
                isAfterNoRollEx = true;
                //throw new NoReceiptRollException();
                throw cre;
            }
            catch (InvalidPaymentException ipe)
            {
                FiscalPrinter.isAfterInvalidPayment = true;
                throw ipe;
            }
            catch (Exception ex)
            {
                throw new PrintDocumentException(PosMessage.DOCUMENT_NOT_PRINTED, ex);
            }
        }

        private CPResponse PrintDocumentHeader(ISalesDocument document)
        {
            CPResponse response = null;
            CPResponse responseSubTotal = null;
            if (document.DocumentTypeId < 0) // RECEIPT
            {
                response = new CPResponse(FiscalPrinter.CompactPrinter.PrintDocumentHeader());
            }
            else
            {
                ////Send command  
                //if(!String.IsNullOrEmpty(document.TcknVkn))
                //    response = new CPResponse(FiscalPrinter.CompactPrinter.PrintDocumentHeader(document.TcknVkn.Trim(), document.TotalAmount, document.DocumentTypeId));
                //else if(document.Customer != null)
                //    response = new CPResponse(FiscalPrinter.CompactPrinter.PrintDocumentHeader(document.Customer.Contact[4].Trim(), document.TotalAmount, document.DocumentTypeId));
                //else // Yemek kartý
                //    response = new CPResponse(FiscalPrinter.CompactPrinter.PrintDocumentHeader("", document.TotalAmount, document.DocumentTypeId));

                switch(document.DocumentTypeId)
                {
                    case 1:
                        String tcknVknInvoice;
                        String serialInvoice = "";

                        //Dahiliden ise sadece tek belge , harici aktif ise önce else blogundaki bilgi fiþi basýlacak
                        if (DataConnector.CurrentSettings.GetProgramOption(Setting.PrintInvoicesInternal) == PosConfiguration.ON)
                        {
                            

                            if (!String.IsNullOrEmpty(document.TcknVkn))
                                tcknVknInvoice = document.TcknVkn.Trim();
                            else if (document.Customer != null)
                                tcknVknInvoice = document.Customer.Contact[4].Trim();
                            else
                                throw new ParameterException("TCKN/VKN EKSÝK");

                            Hugin.Common.Customer CustomerInfoInvoice = new Hugin.Common.Customer();
                            List<string> address2 = new List<string>();
                            address2.Add(document.Customer.Contact[0].Trim());
                            address2.Add(document.Customer.Contact[1].Trim());
                            address2.Add(document.Customer.Contact[2].Trim());

                            CustomerInfoInvoice.AddressList = address2;
                            CustomerInfoInvoice.Name = document.Customer.Name;
                            CustomerInfoInvoice.TCKN_VKN = tcknVknInvoice;
                            CustomerInfoInvoice.TaxOffice = document.Customer.Contact[3].Trim();
                            CustomerInfoInvoice.Label = " ";

                            response = new CPResponse(FiscalPrinter.CompactPrinter.PrintInvoiceHeader(document.IssueDate, document.SlipSerialNo, document.SlipOrderNo, CustomerInfoInvoice));
                            
                        }
                        else
                        {
                           
                            
                            if (document.DocumentTypeId == 1)
                            {
                                string slipSerial = document.SlipSerialNo.Length > 2 ? document.SlipSerialNo.Substring(0, 2) : document.SlipSerialNo;
                                string slipOrder = document.SlipOrderNo.Length > 6 ? document.SlipOrderNo.Substring(0, 6) : document.SlipOrderNo;

                                serialInvoice = slipSerial + slipOrder;
                            }

                            if (!String.IsNullOrEmpty(document.TcknVkn))
                                tcknVknInvoice = document.TcknVkn.Trim();
                            else if (document.Customer != null)
                                tcknVknInvoice = document.Customer.Contact[4].Trim();
                            else
                                throw new ParameterException("TCKN/VKN EKSÝK");

                            response = new CPResponse(FiscalPrinter.CompactPrinter.PrintDocumentHeader(document.DocumentTypeId, tcknVknInvoice, serialInvoice, document.IssueDate));
                        }
                        break;
                    case 2:
                    case 3:
                        // INVOICE, E-INVOICE, E-ARCHIVE
                        String tcknVkn = "";
                        String serial = "";
                        if (document.DocumentTypeId == 1)
                        {
                            string slipSerial = document.SlipSerialNo.Length > 2 ? document.SlipSerialNo.Substring(0, 2) : document.SlipSerialNo;
                            string slipOrder = document.SlipOrderNo.Length > 6 ? document.SlipOrderNo.Substring(0, 6) : document.SlipOrderNo;

                            serial = slipSerial + slipOrder;
                        }

                        if(!String.IsNullOrEmpty(document.TcknVkn))
                            tcknVkn = document.TcknVkn.Trim();
                        else if(document.Customer != null)
                            tcknVkn = document.Customer.Contact[4].Trim();
                        else
                            throw new ParameterException("TCKN/VKN EKSÝK");

                        response = new CPResponse(FiscalPrinter.CompactPrinter.PrintDocumentHeader(document.DocumentTypeId, tcknVkn, serial, document.IssueDate));
                        break;
                    case 4:
                        // FOOD DOCUMENT
                        response = new CPResponse(FiscalPrinter.CompactPrinter.PrintFoodDocumentHeader());

                        break;
                    case 5:
                        // CAR PARKING
                        string plate = "";
                        DateTime parkingDT;

                        plate = document.CustomerTitle;
                        parkingDT = document.IssueDate;

                        response = new CPResponse(FiscalPrinter.CompactPrinter.PrintParkDocument(plate, parkingDT));

                        break;
                    case 6:
                        // ADVANCE
                        string title = "";
                        String tcknVkn2 = "";

                        if (document.Customer != null)
                            title = document.Customer.Name;
                        else
                            title = document.CustomerTitle;

                        if (!String.IsNullOrEmpty(document.TcknVkn))
                            tcknVkn2 = document.TcknVkn.Trim();
                        else if (document.Customer != null)
                            tcknVkn2 = document.Customer.Contact[4].Trim();
                        else
                            throw new ParameterException("TCKN/VKN EKSÝK");

                        response = new CPResponse(FiscalPrinter.CompactPrinter.PrintAdvanceDocumentHeader(tcknVkn2, title, document.TotalAmount));

                        break;
                    case 7:
                        // COLLECTION INVOICE

                        String cllctionSerial = "";

                        string slipSerial1 = document.SlipSerialNo.Length > 2 ? document.SlipSerialNo.Substring(0, 2) : document.SlipSerialNo;
                        string slipOrder1 = document.SlipOrderNo.Length > 6 ? document.SlipOrderNo.Substring(0, 6) : document.SlipOrderNo;

                        cllctionSerial = slipSerial1 + slipOrder1;

                        string instutionNAme = document.ReturnReason;

                        string subscriberNo = document.CustomerTitle;

                        decimal comission = document.ComissionAmount;

                        decimal amount = document.TotalAmount - comission;

                        DateTime dt = document.IssueDate;

                        response = new CPResponse(FiscalPrinter.CompactPrinter.PrintCollectionDocumentHeader(cllctionSerial, dt, amount, subscriberNo, instutionNAme, comission));

                        break;
                    case 8:
                        // RETURN DOCUMENT
                        if (DataConnector.CurrentSettings.GetProgramOption(Setting.PrintInvoicesInternal) == PosConfiguration.OFF)
                        {

                            // Start Non-Fiscal receipt
                            response = new CPResponse(FiscalPrinter.CompactPrinter.StartNFReceipt());

                            int returnDocCounter = 0;
                            string[] returnAmounts = DataConnector.GetReturnAmounts(PosConfiguration.Get("RegisterId"));

                            if (returnAmounts.Length == 2)
                            {
                                try
                                {
                                    returnDocCounter = int.Parse(returnAmounts[0]);
                                }
                                catch { }
                            }

                            List<string> lineList = new List<string>()
                        {
                        "*************** MALÝ  DEÐERÝ YOK ***************",
                        "                                                ",
                        "<<<<<<<<<<<<<<< ÝADE  BÝLGÝ FÝÞÝ >>>>>>>>>>>>>>>",
                        "                                                ",

                        "ÝADE SAYACI   :".PadRight(24) + (returnDocCounter + 1).ToString().PadLeft(24),
                        "ÝADE SATTOP   :".PadRight(24) + ("*" + document.TotalAmount).PadLeft(24),
                        "ÝADE KDVTOP   :".PadRight(24) + ("*" + document.TotalVAT).PadLeft(24),
                        "                                                ",
                        "*************** MALÝ  DEÐERÝ YOK ***************",
                        "                                                "

                        };

                            // Send lines
                            response = new CPResponse(FiscalPrinter.CompactPrinter.WriteNFLine(lineList.ToArray()));

                            // Close Non-Fiscal receipt
                            response = new CPResponse(FiscalPrinter.CompactPrinter.CloseNFReceipt());

                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(document.TcknVkn))
                                tcknVkn = document.TcknVkn.Trim();
                            else if (document.Customer != null)
                                tcknVkn = document.Customer.Contact[4].Trim();
                            else
                                throw new ParameterException("TCKN/VKN EKSÝK");

                            Hugin.Common.Customer CustomerInfo = new Hugin.Common.Customer();
                            List<string> address = new List<string>();
                            address.Add(document.Customer.Contact[0].Trim());
                            address.Add(document.Customer.Contact[1].Trim());
                            address.Add(document.Customer.Contact[2].Trim());

                            CustomerInfo.AddressList = address;
                            CustomerInfo.Name = document.Customer.Name;
                            CustomerInfo.TCKN_VKN = tcknVkn;
                            CustomerInfo.TaxOffice = document.Customer.Contact[3].Trim();
                            CustomerInfo.Label = " ";

                            response = new CPResponse(FiscalPrinter.CompactPrinter.PrintReturnDocumentHeader(document.IssueDate, document.SlipSerialNo, document.SlipOrderNo, CustomerInfo));         
                        }

                        break;
                    case 9:
                        // CURRENT ACCOUNT DOCUMENT

                        string docSerial = document.SlipSerialNo + document.SlipOrderNo;

                        string custName = "";
                        if (document.Customer != null)
                            custName = document.Customer.Name;
                        else
                            custName = document.CustomerTitle;

                        string tcknVkn3 = "";
                        if (!String.IsNullOrEmpty(document.TcknVkn))
                            tcknVkn3 = document.TcknVkn.Trim();
                        else if (document.Customer != null)
                            tcknVkn3 = document.Customer.Contact[4].Trim();
                        else
                            throw new ParameterException("TCKN/VKN EKSÝK");

                        DateTime dt2 = document.IssueDate;

                        response = new CPResponse(FiscalPrinter.CompactPrinter.PrintCurrentAccountCollectionDocumentHeader(tcknVkn3, custName, docSerial, dt2, document.TotalAmount));

                        break;

                    case 10:

                        if (!String.IsNullOrEmpty(document.TcknVkn))
                            tcknVkn = document.TcknVkn.Trim();
                        else if (document.Customer != null)
                            tcknVkn = document.Customer.Contact[4].Trim();
                        else
                            throw new ParameterException("TCKN/VKN EKSÝK");

                        Hugin.Common.Customer CustomerInfoSelf = new Hugin.Common.Customer();
                        List<string> addressSelf = new List<string>();
                        addressSelf.Add(document.Customer.Contact[0].Trim());
                        addressSelf.Add(document.Customer.Contact[1].Trim());
                        addressSelf.Add(document.Customer.Contact[2].Trim());

                        CustomerInfoSelf.AddressList = addressSelf;
                        CustomerInfoSelf.Name = document.Customer.Name;
                        CustomerInfoSelf.TCKN_VKN = tcknVkn;
                        CustomerInfoSelf.TaxOffice = document.Customer.Contact[3].Trim();
                        CustomerInfoSelf.Label = " ";


                        List<Hugin.Common.Service> serviceList = new List<Hugin.Common.Service>();

                        Hugin.Common.Service service = new Hugin.Common.Service();
                        service.Definition = document.ServiceDefinition;
                        service.GrossWages = document.ServiceGrossWages;
                        service.VATRate = document.ServiceVATRate;
                        service.StoppageOtherRate = document.ServiceStoppageOtherRate;
                        service.StoppageRate = document.ServiceStoppageRate;
                        serviceList.Add(service);

                        response = new CPResponse(FiscalPrinter.CompactPrinter.PrintSelfEmployementHeader(CustomerInfoSelf, serviceList.ToArray()));
                        responseSubTotal = new CPResponse(FiscalPrinter.CompactPrinter.PrintSubtotal(false));
                        document.TotalAmount = Decimal.Parse(responseSubTotal.GetNextParam());

                        break;
                }
            }

            FiscalPrinter.LineNumber = 0;
            return response;
        }

        private CPResponse PrintRemarkLine(string line)
        {
            CPResponse response = null;

            try
            {
                // SEND COMMAND
                response = new CPResponse(FiscalPrinter.CompactPrinter.PrintRemarkLine(new string[]{line}));
            }
            catch (PrinterException pe)
            {
                if (OnMessage != null)
                {
                    OnMessage(this, new OnMessageEventArgs(pe));
                }
                WaitFixPrinter();
            }
            return response;
        }

        private CPResponse PrintDocumentAdjustments(List<string> docAdjustments)
        {
            CPResponse response = null;
            foreach (String adjLine in docAdjustments)
            {
                try
                {
                    //response = PrintAdjustment(ParseAdjLine(adjLine));
                    jsonDocument.Adjustments.Add(ParseAdjLine(adjLine));
                }
                catch (PrinterException pe)
                {
                    if (OnMessage != null)
                        OnMessage(this, new OnMessageEventArgs(pe)); ;
                    WaitFixPrinter();
                }
            }

            return response;
        }

        private CPResponse PrintAdjustment(Adjustment adj)
        {
            // SEND COMMAND
            return new CPResponse(FiscalPrinter.CompactPrinter.PrintAdjustment((int)adj.Type, adj.Amount, adj.percentage));
        }

        private CPResponse PrintItems(ISalesDocument Document)
        {
            CPResponse response = null;
            List<IFiscalItem> soldItems = new List<IFiscalItem>();

            soldItems = Document.Items.FindAll(delegate(IFiscalItem fi)
            {
                return fi.Quantity > fi.VoidQuantity;
            });

            if (isAfterNoRollEx)
            {
                //if (printedItemCount != soldItems.Count)
                //{
                //    for (int i = 0; i < printedItemCount; i++)
                //    {
                //        soldItems.RemoveAt(0);
                //    }
                //}
                //// When rool exception while item on printing
                //soldItems.RemoveAt(0);

                decimal currentSubTotal = 0.0m;
                response = new CPResponse(FiscalPrinter.CompactPrinter.PrintSubtotal(true));

                if (response.ErrorCode == 0)
                {
                    currentSubTotal = Decimal.Parse(response.GetNextParam());

                    if (currentSubTotal != Document.TotalAmount)
                    {
                        decimal tmpAmount = 0.0m;

                        while (tmpAmount != currentSubTotal && soldItems.Count != 0)
                        {
                            tmpAmount += soldItems[0].TotalAmount;
                            soldItems.RemoveAt(0);
                        }
                    }
                    else
                    {
                        soldItems.Clear();
                        jsonDocument.FiscalItems.Clear();
                    }
                }
            }
            foreach (IFiscalItem fi in soldItems)
            {
                try
                {
                    response = FiscalItemToPrint(fi);

                    printedItemCount++;

                    Adjustment adj = new Adjustment();

                    decimal totalAdjAmount = 0;
                    foreach (string adjOnItem in fi.GetAdjustments())
                    {
                        string[] values = adjOnItem.Split('|');
                        totalAdjAmount += decimal.Parse(values[0]);
                    }

                    if (totalAdjAmount != 0)
                    {
                        // Ürüne birden fazla indirim/arttýrým uygulanabiliyorsa;
                        //foreach (string adjustment in fi.GetAdjustments())
                        //{
                        //    Adjustment tempAdj;
                        //    tempAdj = ParseAdjLine(adjustment);

                        //    adj.Amount += tempAdj.Amount;
                        //}

                        //if (adj.Amount > 0)
                        //    adj.Type = AdjustmentType.Fee;
                        //else
                        //    adj.Type = AdjustmentType.Discount;

                        // Ürüne tek indirim/arttýrým uygulanabiliyorsa;
                        string[] adjustmentsOnItem = fi.GetAdjustments();
                        adj = ParseAdjLine(adjustmentsOnItem[adjustmentsOnItem.Length - 1]); // last adj after corrections                       
                    }

                    if (adj != null && adj.Amount != 0)
                    {
                        //response = PrintAdjustment(adj);

                        jsonDocument.FiscalItems[jsonDocument.FiscalItems.Count - 1].Adj = adj;
                    }                   
                }
                catch (PrinterException pe)
                {
                    if (OnMessage != null)
                        OnMessage(this, new OnMessageEventArgs(pe));
                    WaitFixPrinter();
                }
            }

            return response;
        }

        private CPResponse FiscalItemToPrint(IFiscalItem fi)
        {
            List<byte> productBytes = new List<byte>();

            decimal qtty = fi.Quantity - fi.VoidQuantity;

            decimal totalAdjAmount = 0;
            foreach (string adjOnItem in fi.GetAdjustments())
            {
                string[] values = adjOnItem.Split('|');
                totalAdjAmount += decimal.Parse(values[0]);
            }
            decimal unitPrice = fi.UnitPrice;
            if (fi.GetAdjustments().Length > 0 && totalAdjAmount != 0)
                unitPrice = Math.Round(unitPrice-(totalAdjAmount/ qtty), 2);

            //CPResponse response = new CPResponse(FiscalPrinter.CompactPrinter.PrintItem(fi.Product.Id, qtty, unitPrice, fi.Product.Name, fi.Product.Department.Id, (int)fi.Product.Status));

            JSONItem jsonItem = new JSONItem();
            jsonItem.Id = fi.Product.Id;
            jsonItem.Quantity = qtty;
            jsonItem.Price = unitPrice;
            jsonItem.Name = fi.Product.Name.Trim();
            jsonItem.DeptId = fi.Product.Department.Id;
            jsonItem.Status = (int)fi.Product.Status;

            // Set Flag for print product barcode
            if ((CurrentSettings.GetProgramOption(Setting.PrintProductBarcode) == PosConfiguration.ON))
            {
                jsonItem.NoteLine1 = "## " + fi.Product.Barcode;
            }

            jsonDocument.FiscalItems.Add(jsonItem);

            return null; 
        }

        private CPResponse PrintPayments(ISalesDocument Document, decimal paid)
        {
            List<PaymentInfo> payments = FiscalPrinter.GetPayments(Document);
            CPResponse response = null;
            Decimal paidTotal = 0.00m;

            bool add = false;
            foreach (PaymentInfo pi in payments)
            {               
                try
                {
                    // SEND COMMAND
                    //response = new CPResponse(FiscalPrinter.CompactPrinter.PrintPayment((int)pi.Type, pi.Index, pi.PaidTotal));
                    if (paidTotal == paid)
                        add = true;
                    if (add)
                    {
                        if (pi.PaidTotal < 0 && paid > pi.PaidTotal) // if its voided payment
                        {
                            for (int i = 0; i < payments.Count; i++)
                            {
                                if (pi.PaidTotal == ((-1) * payments[i].PaidTotal) &&
                                    pi.Type == payments[i].Type)
                                {
                                    FiscalPrinter.CompactPrinter.VoidPayment(payments[i].SequenceNo);
                                }
                            }
                        }
                        else
                            jsonDocument.Payments.Add(pi);
                    }

                    if (!add)
                        paidTotal += pi.PaidTotal;

                    
                }
                catch (NegativeResultException)
                {
                    throw new NegativeResultException();
                }
                catch (PrinterException pe)
                {
                    if (OnMessage != null)
                        OnMessage(this, new OnMessageEventArgs(pe)); ;
                    WaitFixPrinter();
                }
            }
            return response;
        }

        public IPrinterResponse Pay(Decimal amount)
        {
            return null;
        }
        public IPrinterResponse Pay(Decimal amount, String refNumber)
        {
            return null;
        }
        public IPrinterResponse Pay(Decimal amount, ICurrency currency)
        {
            return null;
        }

        public IPrinterResponse Pay(Decimal amount, ICredit credit, int installments)
        {
            CPResponse response = null;

            JSONDocument JSONDoc = ConvertSalesDocumentToJSONDocument(FiscalPrinter.Document);
            JSONDoc.EndOfReceiptInfo.CloseReceiptFlag = false;

            if (FiscalPrinter.Document.DocumentTypeId == 6 || FiscalPrinter.Document.DocumentTypeId == 7)
            {
                PrintFooter(FiscalPrinter.Document, true);
            }

            // Eðer yapýlacak EFT ödemesinden önce EFT ödemesi alýndýysa ve o EFT alýnýrken gönderilen 
            // diðer ödeme türlerinin tekrar gönderilmemesi için
            List<PaymentInfo> stackList = new List<PaymentInfo>();
            foreach (PaymentInfo pi in JSONDoc.Payments)
            {
                if (!pi.viaByEFT)
                    stackList.Add(pi);
                else
                    stackList = new List<PaymentInfo>();
            }
            JSONDoc.Payments = stackList;

            try
            {
                if (!isAfterInvalidPayment && FiscalPrinter.Document.DocumentTypeId != 6 && FiscalPrinter.Document.DocumentTypeId != 7)
                {
                    // Check for header is printed?
                    response = new CPResponse(FiscalPrinter.CompactPrinter.CheckPrinterStatus());
                    if (response.FPUState == State.IDLE)
                        response = new CPResponse(FiscalPrinter.CompactPrinter.PrintDocumentHeader());

                    // DEPT SALE
                    response = new CPResponse(FiscalPrinter.CompactPrinter.PrintJSONDocumentDeptOnly(Newtonsoft.Json.JsonConvert.SerializeObject(JSONDoc)));

                    // SALES DOCUMENT
                    //response = new CPResponse(compactPrinter.PrintSalesDocument(Newtonsoft.Json.JsonConvert.SerializeObject(JSONDoc)));
                }

                response = new CPResponse(FiscalPrinter.CompactPrinter.GetEFTAuthorisation(amount, installments, String.Empty));

                // Last EFT Operaiton
                if (response.ErrorCode == 0 && response.ParamCount > 0)
                {
                    string acquierID = response.GetParamByIndex(5);
                    string batchNo = response.GetParamByIndex(9);
                    string stanNo = response.GetParamByIndex(10);

                    response = new CPResponse(FiscalPrinter.CompactPrinter.GetSalesInfo());
                    int docNo = Convert.ToInt32(response.GetNextParam());
                    int zNo = Convert.ToInt32(response.GetNextParam());
                    

                    if (System.IO.File.Exists(PosMessage.LAST_EFT_FILENAME))
                        System.IO.File.Delete(PosMessage.LAST_EFT_FILENAME);
                    using (StreamWriter sw = new StreamWriter(PosMessage.LAST_EFT_FILENAME, true))
                    {
                        sw.WriteLine(acquierID);
                        sw.WriteLine(batchNo);
                        sw.WriteLine(stanNo);
                        sw.WriteLine(zNo);
                        sw.WriteLine(docNo);
                    }
                }
            }
            catch (NegativeResultException nre)
            {
                throw nre;
            }
            catch (InvalidPLUNoException pe)
            {
                throw pe;
            }
            catch (NoReceiptRollException nre)
            {
                throw nre;
            }
            catch (ClearRequiredException)
            {
                throw new NoReceiptRollException();
            }
            catch (OperationCanceledException)
            {
                throw new NoReceiptRollException();
            }
            catch (InvalidPaymentException ipe)
            {
                isAfterInvalidPayment = true;
                throw ipe;
            }
            catch (Exception ex)
            {
                throw new PrintDocumentException(PosMessage.DOCUMENT_NOT_PRINTED, ex);
            }
            finally
            {

            }

            isAfterInvalidPayment = false;
            return response;
        }

        public IPrinterResponse PrintEDocument(int docType, string[] lines)
        {
            CPResponse response = null;

            try
            {
                // SEND COMMAND
                response = new CPResponse(FiscalPrinter.CompactPrinter.PrintEDocumentCopy(docType, lines));
            }
            catch (PrinterException pe)
            {
                if (OnMessage != null)
                {
                    OnMessage(this, new OnMessageEventArgs(pe));
                }
                WaitFixPrinter();
            }
            return response;
        }

        private string[] GetInvoiceLines(ISalesDocument doc)
        {
            List<string> lineList = new List<string>();

            string lineFormat = "";

            /* Header */
            lineFormat = "{0}{1}";

            string title = "";
            if (doc.Customer != null)
                title = doc.Customer.Name;
            else
                title = doc.CustomerTitle;

            string invoiceSerial = "";

            if (doc.DocumentTypeId == (int)DocumentTypes.SELF_EMPLYOMENT_INVOICE ||
                doc.DocumentTypeId == (int)DocumentTypes.RETURN_DOCUMENT)
                invoiceSerial = String.Format("{0}{1:D4}{2:D4}", FiscalPrinter.FiscalRegisterNo, int.Parse((new CPResponse(FiscalPrinter.CompactPrinter.GetLastDocumentInfo(true))).GetParamByIndex(2) + 1), doc.Id);
            else
            {
                string slipSerial = doc.SlipSerialNo.Length > 2 ? doc.SlipSerialNo.Substring(0, 2) : doc.SlipSerialNo;
                string slipOrder = doc.SlipOrderNo.Length > 6 ? doc.SlipOrderNo.Substring(0, 6) : doc.SlipOrderNo;

                invoiceSerial = slipSerial + slipOrder;
            }

            lineList.Add(String.Format(lineFormat, "Sayýn,".PadRight(24), (doc.IssueDate.ToString("dd-MM-yyyy") + " " + doc.IssueDate.ToString("HH:mm:ss")).PadLeft(24)));

            if (String.IsNullOrEmpty(title))
                title = " ";
            lineList.Add(String.Format(lineFormat, title.PadRight(24), invoiceSerial.PadLeft(24)));

            string tcknVkn = "11111111111";
            if (!String.IsNullOrEmpty(doc.TcknVkn))
                tcknVkn = doc.TcknVkn.Trim();
            else if (doc.Customer != null)
                tcknVkn = doc.Customer.Contact[4].Trim();

            if (tcknVkn.Length == 11)
                lineList.Add("TCKN: " + tcknVkn.PadRight(24));
            else
                lineList.Add("VKN: " + tcknVkn.PadRight(24));

            string addressInfo1 = " ";
            string addressInfo2 = " ";
            string addressInfo3 = " ";
            string taxOffice = " ";

            try
            {
                // Address Info
                if (doc.Customer == null)
                {
                    string[] splittedArray = doc.ReturnReason.Split('|');
                    addressInfo1 = splittedArray[0];
                    addressInfo2 = splittedArray[1];
                    taxOffice = splittedArray[2];
                }
                else
                {
                    addressInfo1 = doc.Customer.Contact[0];
                    addressInfo2 = doc.Customer.Contact[1];
                    addressInfo3 = doc.Customer.Contact[2];
                    taxOffice = doc.Customer.Contact[3];
                }
            }
            catch { }


            lineList.Add(addressInfo1);
            lineList.Add(addressInfo2);
            if (!String.IsNullOrEmpty(addressInfo3)) lineList.Add(addressInfo3);
            lineList.Add("V.D. : " + taxOffice);

            lineList.Add(" ");
            lineList.Add(" ");


            /* Products */
            Adjustment adj = new Adjustment();
            foreach (IFiscalItem fi in doc.Items)
            {
                lineFormat = "{0} X {1}";
                if (fi.Quantity != 1)
                {
                    lineList.Add(String.Format(lineFormat, fi.Quantity.ToString().PadLeft(10),
                                                           fi.UnitPrice.ToString()));
                }

                lineFormat = "{0}  {1}  {2}";
                lineList.Add(String.Format(lineFormat, fi.Name.PadRight(20),
                                                       String.Format("%{0}", Department.TaxRates[fi.TaxGroupId - 1].ToString().PadLeft(2, '0')),
                                                       (Math.Round(fi.TotalAmount, 2).ToString() + " TL").PadLeft(21)));

                // adjustment on item
                decimal totalAdjAmount = 0;
                foreach (string adjOnItem in fi.GetAdjustments())
                {
                    string[] values = adjOnItem.Split('|');
                    totalAdjAmount += decimal.Parse(values[0]);
                }

                if (totalAdjAmount != 0)
                {
                    adj = ParseAdjLine(fi.GetAdjustments()[0]);
                    lineList.Add(FormatAdjustment(adj));
                }
            }

            String[] docAdjustments = doc.GetAdjustments();
            adj = null;
            if (docAdjustments.Length > 0)
            {
                adj = ParseAdjLine(docAdjustments[0]);
                doc.TotalAmount -= adj.Amount;
            }

            // PRINT SUBTOTAL            
            lineList.Add(" ");
            lineList.Add(FormatSubTotal(doc));
            if (adj != null)
            {
                lineList.Add(FormatAdjustment(adj));
                doc.TotalAmount += adj.Amount;

                lineList.Add(FormatSubTotal(doc));
            }

            lineList.Add(" ");
            lineList.Add(" ");

            // Totals
            lineList.Add(String.Format("{0}{1}", "KDV".PadRight(24), ("*" + new Number(doc.TotalVAT).ToString("C")).PadLeft(24)));
            lineList.Add(String.Format("{0}{1}", "TOP".PadRight(24), ("*" + new Number(doc.TotalAmount).ToString("C")).PadLeft(24)));

            lineList.Add(" ");
            lineList.Add("*" + WordConversion.ConvertLetter(doc.TotalAmount));
            lineList.Add(" ");

            // Payments
            lineList.AddRange(PrintPayments());
            lineList.Add(" ");

            if (doc.CustomerChange > 0)
            {
                String.Format("{0}{1}",
                                PosMessage.CHANGE.PadRight(24),
                                ("*" + new Number(doc.CustomerChange).ToString("C")).PadLeft(24));
            }
            lineList.Add(" ");

            // Tax Group Totals
            decimal[,] taxRateTotals = doc.TaxRateTotals;

            if (taxRateTotals.Length > 0)
            {
                lineList.Add(String.Format("{0}  {1}  {2}", PosMessage.VAT_DISTRIBUTION.PadRight(20), PosMessage.VAT.PadRight(10), PosMessage.SALE));
            }
            for (int i = 0; i < taxRateTotals.GetLength(0); i++)
            {
                int taxRate = (int)(Math.Round(Department.TaxRates[(int)(taxRateTotals[i, 0])], 0));

                lineList.Add(String.Format("{0}  {1}  {2}", 
                                            String.Format("{0} %{1}", PosMessage.SELLING_VAT, taxRate.ToString().PadLeft(2, '0')).PadRight(20),
                                            String.Format("*{0:C}", new Number(taxRateTotals[i, 1])).PadRight(10),
                                            String.Format("*{0:C}", new Number(taxRateTotals[i, 2]))));
            }
            lineList.Add(" ");

            // Footer Notes
            if (doc.FootNote.Count > 0)
            {
                lineList.Add("## " + String.Empty.PadLeft(32, '-') + " ##");

                foreach (String line in doc.FootNote)
                    lineList.Add("## " + line.PadRight(32, ' ') + " ##");

                lineList.Add("## " + String.Empty.PadLeft(32, '-') + " ##");
            }

            lineList.Add(" ");
            lineList.Add(" ");

            lineList.Add("DÜZENLEYEN: " + FiscalPrinter.Cashier.Name.TrimEnd());
            lineList.Add("ÝMZA : ");

            lineList.Add(" ");
            lineList.Add(" ");

            lineList.Add(String.Format("{0}{1} {2}", " ".PadRight((48 - FiscalPrinter.FiscalRegisterNo.Length) / 2, ' '), FiscalPrinter.FiscalRegisterNo.Substring(0, 2),
                                                         FiscalPrinter.FiscalRegisterNo.Substring(2)));
        
            return lineList.ToArray();
        }

        private string[] GetEInvoicesLines(ISalesDocument doc)
        {
            List<string> eDocLines = null;

            if(!String.IsNullOrEmpty(doc.ReturnReason))
            {
                eDocLines = new List<string>();

                string[] splittedArray = doc.ReturnReason.Split('|');

                foreach (string s in splittedArray)
                    eDocLines.Add(s);
            }

            return eDocLines.ToArray();
        }

        private List<string> PrintPayments()
        {
            List<string> retList = new List<string>();
            decimal paidTotal = 0.00m;
            PaymentInfo pi = null;
            List<PaymentInfo> payments = new List<PaymentInfo>();

            //PAYMENTS WITH CHECK
            List<String> checkpayments = new List<string>(Document.GetCheckPayments());
            while (checkpayments.Count > 0)
            {
                String[] detail = checkpayments[0].Split('|');// Amount | RefNumber | Sequence No
                if (detail[1].Length > 12)
                    detail[1] = detail[1].Substring(0, 12);

                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(detail[0]);
                if (detail[1] == "")
                    pi.Index = 0;
                else
                    pi.Index = int.Parse(detail[1]);
                pi.Type = PaymentType.CHECK;
                pi.SequenceNo = int.Parse(detail[2]);
                payments.Add(pi);

                //Print(amount, label);
                paidTotal += pi.PaidTotal;
                checkpayments.RemoveAt(0);
            }

            //PAYMENTS WITH CURRENCIES
            List<String> currencypayments = new List<string>(Document.GetCurrencyPayments());
            while (currencypayments.Count > 0)
            {
                String[] detail = currencypayments[0].Split('|');// Amount | Exchange Rate | Name | Sequence No               

                decimal amount = Decimal.Parse(detail[0]);
                decimal quantity = Math.Round(amount / decimal.Parse(detail[1]), 2);

                int id = 0;
                Dictionary<int, ICurrency> currencies = DataConnector.GetCurrencies();
                foreach (ICurrency currency in currencies.Values)
                {
                    if (currency.Name == detail[2])
                        break;
                    id++;
                }

                string label = String.Format("{0} {1}", detail[2], quantity);

                retList.Add(FormatPayment(amount, label));
                paidTotal += amount;
                currencypayments.RemoveAt(0);
            }

            //PAYMENTS WITH CREDITS
            List<String> creditpayments = new List<string>(Document.GetCreditPayments());
            while (creditpayments.Count > 0)
            {
                String[] detail = creditpayments[0].Split('|');// Amount | Installments | Id | viaEFT | Sequence No

                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(detail[0]);
                //pi.Index = Int32.Parse(detail[1]);
                pi.Type = PaymentType.CREDIT;
                pi.SequenceNo = int.Parse(detail[4]);
                pi.Index = Int32.Parse(detail[2]); // id

                payments.Add(pi);
                paidTotal += pi.PaidTotal;
                creditpayments.RemoveAt(0);
            }

            //PAYMENTS WITH CASH
            List<String> cashpayments = new List<string>(Document.GetCashPayments());
            while (cashpayments.Count > 0)
            {
                String[] detail = cashpayments[0].Split('|'); // Amount | SequenceNo
                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(detail[0]);
                pi.Type = PaymentType.CASH;
                pi.SequenceNo = int.Parse(detail[1]);

                payments.Add(pi);
                paidTotal += pi.PaidTotal;
                cashpayments.RemoveAt(0);
            }

            // Sort Payments by sequence no
            payments.Sort(delegate (PaymentInfo x, PaymentInfo y)
            {
                return x.SequenceNo.CompareTo(y.SequenceNo);
            });


            // Print Payments
            foreach (PaymentInfo p in payments)
            {
                string label = "";

                switch (p.Type)
                {
                    case PaymentType.CASH:
                        label = PosMessage.CASH;
                        break;
                    case PaymentType.CHECK:
                        label = String.Format("{0} {1}", PosMessage.CHECK, p.Index);
                        break;
                    case PaymentType.CREDIT:
                        String name = "";

                        Dictionary<int, ICredit> credits = DataConnector.GetCredits();
                        foreach (ICredit credit in credits.Values)
                        {
                            if (credit.Id == p.Index)
                            {
                                name = credit.Name;
                                break;
                            }
                        }

                        label = name;
                        break;
                    case PaymentType.FCURRENCY:
                        break;
                }

                retList.Add(FormatPayment(p.PaidTotal, label));
            }

            return retList;
        }
        private string FormatPayment(Decimal amount, String label)
        {
            return String.Format("{0}{1}", label.PadRight(24), ("*" + new Number(amount).ToString("C")).PadLeft(24));
        }

        private string FormatAdjustment(Adjustment adj)
        {
            switch (adj.Type)
            {
                case AdjustmentType.PercentDiscount:
                case AdjustmentType.PercentFee:
                    string sign = adj.Type == AdjustmentType.PercentDiscount ? "-" : "+";
                    return String.Format("{0}{1}",
                                        String.Format("{0}%{1}", sign, adj.percentage.ToString().PadLeft(2, '0')).PadRight(24),
                                        ("*" + new Number(adj.Amount).ToString("C")).PadLeft(24));
                case AdjustmentType.Discount:
                case AdjustmentType.Fee:
                    string type = adj.Type == AdjustmentType.Discount ? PosMessage.REDUCTION : PosMessage.FEE;
                    return String.Format("{0}{1}",
                                            type.PadRight(24),
                                            ("*" + new Number(adj.Amount).ToString("C")).PadLeft(24));
                default:
                    return null;
            }
        }

        private string FormatSubTotal(ISalesDocument doc)
        {
            return String.Format("{0}{1}",
                                    PosMessage.SUBTOTAL.PadRight(24),
                                    ("*" + new Number(doc.TotalAmount).ToString("C")).PadLeft(24));
        }

    }
}
