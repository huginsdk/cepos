using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Hugin.POS.Common;
using Hugin.POS;

namespace Hugin.POS.Printer
{
    class SlipPrinter : FiscalPrinter, IFiscalPrinter
    {
       
        // Reports an event to request new paper
        public new event EventHandler DocumentRequested;
        public new event OnMessageHandler OnMessage;
       
        // when an array (document totals) will be printed and paper is finished, stores last
        // printed index
        private int line_index_of_totals_to_print = 0;
       
        // stores the documents totals (as formatted lines)
        private List<String> totalLines = null;

        protected SerialPort sp = null;
        protected const int serialTimeout = 4500;

        private static InvoicePage invoicePage = null;
        private DateTime lastWRTime = DateTime.Now;
        private const int SLIP_DELAY = 350;

        internal SlipPrinter()
        {
            
        }
        internal bool CheckConnection()
        {
            if (sp == null)
                return true;
            //asking slip is not for slip request,
            //it is only for to reach printer
            IPrinterResponse response = Send(SlipRequest.CheckStatus(Part.SlipPaper));
            if (response.Detail == null)
            {
                return false;
            }
            return true;
        }
        internal static InvoicePage Invoicepage
        {
            get { return invoicePage; }
            set { invoicePage = value; }
        }

        #region IFiscalPrinter Members (Slip Printer)
        
        public new IPrinterResponse Feed()
        {
            SlipResponse response = null;
            return response;
        }

        public new IPrinterResponse CutPaper()
        {
            SlipResponse response = null;
            return response;
        }

        public new decimal PrinterSubTotal
        {
            get { return SlipPrinter.Invoicepage.SubTotal; }
        }

        public new IPrinterResponse Suspend()
        {
            Send(SlipRequest.WriteLine("                                                   BEKLETMEYE ALINAN"));
            return Void();
        }

        internal void VoidOnOpen()
        {
            try
            {
                WriteLine("                BELGE ÝPTAL             ");
                PrintFiscalId();
                ReleaseSheet();
            }
            catch (Exception ex) { FiscalPrinter.Log.Error(ex.Message); }
        }
        public new IPrinterResponse Void()
        {
            invoicePage.ClearInvoice();

            SlipPrinter.Document = null;
            return new SlipResponse();
        }

        public IPrinterResponse PrintHeader(ISalesDocument salesDocument)
        {
            return CheckPrinterStatus();
        }

        public IPrinterResponse PrintFooter(ISalesDocument document, bool isReceipt)
        {
            FiscalPrinter.Document = document;
            decimal tempBalance=0;
            //if document is not slip document, change printer
            if (document.DocumentTypeId < 0)
            {
                AdjustPrinter(document);
                return Printer.PrintFooter(document, true);
            }

            // SET READY TO PRINT
            //Send(SlipRequest.InitSlipPrinter());

            //PRINT HEADER            
            try
            {
                this.PrintHeader(document, document.PrintSlipInfo);
                WaitForSlip();
            }
            catch(PrinterOfflineException poe)
            {
                invoicePage.ClearInvoice();
                throw poe;
            }
            catch (PrinterException pe)
            {
                if (OnMessage != null)
                    OnMessage(this, new OnMessageEventArgs(pe));
            }

            //PRINT FISCAL ITEMS
            foreach (IFiscalItem fi in document.Items)
            {       
                // Dont print voided items   
                if(fi.Quantity > fi.VoidQuantity)     
                    this.Print(fi);                              
            }
            WaitForSlip();

            String[] docAdjustments = document.GetAdjustments();
            Adjustment adj = null;
            if (docAdjustments.Length > 0)
            {
                adj = ParseAdjLine(docAdjustments[0]);
                tempBalance = document.BalanceDue;
                document.TotalAmount -= adj.Amount;
                if (tempBalance == 0)
                {
                    document.BalanceDue = 0; // this line has been added to prevent the error( it was waiting on payment again ) while printing same invoice again.
                }
            }

            // PRINT SUBTOTAL            
            WriteLine("");
            PrintSubTotal(document, true);
            WaitForSlip();
            if (adj != null)
            {
                Print(adj);
                tempBalance = document.BalanceDue;
                document.TotalAmount += adj.Amount;

                PrintSubTotal(document, true);
                if (tempBalance == 0)
                {
                    document.BalanceDue = 0; // this line has been added to prevent the error( it was waiting on payment again ) while printing same invoice again.
                }
            }
            

            // PRINT TOTAL
            PrintTotals(document, true);
            WaitForSlip();

            // PRINT PAYMENT
            PrintPayments();
            WaitForSlip();

            // PRINT FOOTER
            List<String> footerItems = SlipPrinter.Invoicepage.FormatFooter(document);
            
            IPrinterResponse response = null;

            foreach (String s in footerItems)
            {
                response = Send(SlipRequest.WriteLine(s));
            }
            WaitForSlip();
            response = PrintFiscalId();
            SlipPrinter.Invoicepage.ClearInvoice();
            try
            {
                ReleaseSheet();
            }
            catch (PrinterOfflineException)
            {
                FiscalPrinter.Log.Error("Fatura kagidini printerdan zamaninda aliniz!");
            }
            finally
            { //sp.ReadTimeout = FPU_TIMEOUT;
            }
            
            while (true)
            {
                if (!SlipReady())
                    break;
                System.Threading.Thread.Sleep(100);
            }            
            
            if (FiscalPrinter.CanOpenDrawer(document))
                OpenDrawer();

            // SLIP INFO RECEIPT
            if (document.PrintSlipInfo)
            {
                try
                {
                    response = PrintSlipInfoReceipt(document);
                    response = EndSlipInfoReceipt();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return null;
        }

        private IPrinterResponse PrintHeader(ISalesDocument document, bool startSlipInfo)
        {
            IPrinterResponse response = null;

            // Initialize printer before start to print
            Initialize();

            try
            {
                if (Invoicepage.Id == 0)
                {
                    SlipPrinter.Document.Id = document.Id;
                    Invoicepage.SubTotal = 0;
                    totalLines = null;
                    line_index_of_totals_to_print = 0;
                }
            }
            catch (NullReferenceException ex)
            {
                if (Invoicepage != null)
                {
                    Invoicepage = new InvoicePage();
                    return PrintHeader(document, false);
                }
                throw ex;
            }

            if (startSlipInfo)
            {
                try
                {
                    //Send data to start document as the following format
                    response = StartSlipInfoReceipt(document);
                    //get document id from response data
                    if (response.HasError)
                        document.Id = 0;
                }
                catch (CmdSequenceException)
                {
                    ////if command sequence exception occurs
                    //switch ((Command)cse.LastCommand)
                    //{
                    //    //if last command is 37, initialize invoice page
                    //    case Command.START_SLÝP:
                    //        if (SlipPrinter.Invoicepage.SubTotal == 0 && Invoicepage.Id == 0)
                    //            SlipPrinter.Invoicepage.ClearInvoice();//after page request
                    //        break;
                    //    case Command.X_DAILY://if x report has not been ended yet
                    //        PrintXReport(true);//print x report
                    //        return PrintHeader(document, false);//then start to print document
                    //    case Command.START_RCPT://if custom report has not been ended yet
                    //    //case Command.CustomReportLine:
                    //    //    RecoverCustomReport();//print do required processes
                    //    //    return PrintHeader(document);//then start to print document
                    //    default:
                    //        throw cse;
                    //}
                }
                catch (NoReceiptRollException nrre)
                {
                    throw nrre;
                }
                catch (Exception ex)
                {
                    //store the data (write to a log file)
                    FiscalPrinter.Log.Error(ex.Message);
                }
                finally
                {//if some error occured during the receive document id, get current document id
                    if (document.Id == 0)
                    {
                        while (true)
                        {
                            try
                            {
                                document.Id = FiscalPrinter.Printer.CurrentDocumentId;
                                break;
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(100);
                            }
                        }
                    }
                }
            }

            FiscalPrinter.Document = document;
            List<String> rowList = Invoicepage.FormatHeader(document);

            if (!SlipReady())
            {
                DocumentRequested(new RequestSlipException(), new EventArgs());
                WaitForSlip();
            }


            for (int lineCount = 0; lineCount < rowList.Count; lineCount++)
            {
                Send(SlipRequest.WriteLine(rowList[lineCount]));
            }

            return response;
        }

        private new IPrinterResponse Print(IFiscalItem fi)
        {
            if (SlipPrinter.Document.Id == 0)
            {
                SlipPrinter.Document.Id = FiscalPrinter.Printer.CurrentDocumentId; 
                throw new DocumentIdNotSetException();
            }
            List<String> fiscalItems = SlipPrinter.Invoicepage.Format(fi);
            IPrinterResponse response = null;
            foreach (String row in fiscalItems)
            {
                if (!SlipReady())
                {
                    DocumentRequested(new RequestSlipException(), new EventArgs());
                    WaitForSlip();
                }

                response = Send(SlipRequest.WriteLine(row));
            }
            SlipPrinter.Invoicepage.SubTotal += fi.TotalAmount;

            // adjustment on item
            Adjustment adj = new Adjustment();

            decimal totalAdjAmount = 0;
            foreach (string adjOnItem in fi.GetAdjustments())
            {
                string[] values = adjOnItem.Split('|');
                totalAdjAmount += decimal.Parse(values[0]);
            }

            if (totalAdjAmount != 0)
            {
                adj = ParseAdjLine(fi.GetAdjustments()[0]);
                Print(adj);
            }

            return response;
        }

        private IPrinterResponse Print(Adjustment ai)
        {
            List<String> adjustmentItems = SlipPrinter.Invoicepage.Format(ai);
            IPrinterResponse response = null;

            foreach (String row in adjustmentItems)
            {
                response = Send(SlipRequest.WriteLine(row));
            }
            SlipPrinter.Invoicepage.SubTotal += ai.Amount;

            //ReceiptPrinter prn = receiptPrinter as ReceiptPrinter;
            //if (ai.NetAmount < 0)
            //{
            //    prn.ShowDiscount(Math.Abs(ai.NetAmount));
            //}
            //else
            //{
            //    prn.ShowFee(Math.Abs(ai.NetAmount));
            //}
            return response;
        }

        private new IPrinterResponse Print(IAdjustment[] ai)
        {
            if ((ai == null || ai.Length == 0)) return new SlipResponse();
            List<String> adjustmentItems = SlipPrinter.Invoicepage.Format(ai);
            IPrinterResponse response = null;

            foreach (String row in adjustmentItems)
            {
                response = Send(SlipRequest.WriteLine(row));
            }
            foreach (IAdjustment adj in ai)
                SlipPrinter.Invoicepage.SubTotal += adj.NetAmount;
            return response;
        }


        private IPrinterResponse PrintFiscalId()
        {
            IPrinterResponse response = null;
            string fiscalNo = String.Format("{0}{1} {2}", " ".PadRight((InvoicePage.MaxCharsInLine - FiscalPrinter.FiscalRegisterNo.Length)/2, ' ') ,FiscalPrinter.FiscalRegisterNo.Substring(0, 2),
                                                         FiscalPrinter.FiscalRegisterNo.Substring(2));

            response = Send(SlipRequest.WriteLine("      "));
            System.Threading.Thread.Sleep(100);
            response = Send(SlipRequest.WriteLine(fiscalNo));
            System.Threading.Thread.Sleep(100);
            return response;
        }
        
        #endregion  IFiscalPrinter Members (Slip Printer)

        private IPrinterResponse WriteLine(string line)
        {
            IPrinterResponse responseOffline = Send(SlipRequest.CheckStatus(Part.Offline));

            if (sp == null)
                return responseOffline;

            if (responseOffline.Data == null && responseOffline.Detail == null)
                throw new PowerFailureException();
            if (!SlipReady())
            {
                DocumentRequested(new RequestSlipException(), new EventArgs());
                WaitForSlip();
            }
            return Send(SlipRequest.WriteLine(line));
        }

        private bool SlipReady()
        {

            IPrinterResponse response = Send(SlipRequest.CheckStatus(Part.SlipPaper));
            if (response.Detail == null)
                throw new PrinterOfflineException();
            if (response.Detail[6] == '1')//Slip is not detected by TOF sensor
            {
                response.Data = "Slip is not detected by TOF sensor";
                return false;
            }

            if (response.Detail[5] == '1')//Slip is not detected by BOF sensor
            {
                response.Data = "Slip is not detected by BOF sensor";
                return false;
            }

            if (response.Detail[3] == '1')//Waits for slip insertion
            {
                response.Data = "Waits for slip insertion";
                return false;
            }
            return true;
        }

        internal void RequestSlip()
        {
            List<String> postedPage = new List<string>();
            //Format sub totals, page number
            if (line_index_of_totals_to_print == 0)
                postedPage = SlipPrinter.Invoicepage.FormatPageDeposit(SlipPrinter.Invoicepage.SubTotal);

            postedPage.AddRange(SlipPrinter.Invoicepage.FormatPageNo(line_index_of_totals_to_print == 0));

            IPrinterResponse response = null;
            foreach (String row in postedPage)
            {
                response = Send(SlipRequest.WriteLine(row));
                System.Threading.Thread.Sleep(100);
            }
            try
            {
                response = PrintFiscalId();
                System.Threading.Thread.Sleep(500);
                ReleaseSheet();
                System.Threading.Thread.Sleep(1000);
                while (SlipReady()) { }
            }
            catch { }

            DocumentRequested(new RequestSlipException(), new EventArgs());

            WaitForSlip();

            PrintHeader(FiscalPrinter.Document, false);

            System.Threading.Thread.Sleep(5000);

        }

        private void ReleaseSheet()
        {
            Send(SlipRequest.SetReverseEject(true));
            Send(SlipRequest.EjectSheet());
            Send(SlipRequest.Release());
        }

        private void WaitForSlip()
        {
            int wait = 50;
            try
            {
                while (!SlipReady())
                {
                    //if (wait > 1000)
                    //{
                    //    throw new SlipRowCountExceedException();
                    //}
                    //wait some (it changes 50ms to 1sec)
                    System.Threading.Thread.Sleep(wait);

                    //increase wait time, cashier may not insert paper suddenly, so do not exhaust printer
                    wait = wait + 20;

                }
                System.Threading.Thread.Sleep(1000);
                if (!SlipReady())
                    WaitForSlip();
            }
            finally
            {
                wait = 0;
            }
        }
        private IPrinterResponse Send(IPrinterRequest irequest)
        {
            //if (irequest is FPURequest) return base.Send((FPURequest)irequest);


            SlipResponse response = new SlipResponse();
            if (sp == null)
            {
                response.Detail = "serial port has not been initialized yet";
                return response;
            }

            SlipRequest request = (SlipRequest)irequest;

            byte[] b = StringToByteArray(request.ToString());
            sp.Write(b, 0, b.Length);

            if (request.StatusCheck)
            {
                System.Threading.Thread.Sleep(100);
            }
            else
            {
                TimeSpan ts = DateTime.Now.Subtract(lastWRTime);
                if (ts.TotalMilliseconds < SLIP_DELAY)
                {
                    System.Threading.Thread.Sleep(SLIP_DELAY - (int)ts.TotalMilliseconds);
                }
            }

            lastWRTime = DateTime.Now;
            if (sp.BytesToRead > 0)
                response = new SlipResponse(sp.ReadByte().ToString());

            return response;
        }

        private byte[] StringToByteArray(string str)
        {
            byte[] data = new byte[str.Length];
            for (int i = 0; i < str.Length; ++i)
                data[i] = (byte)(str[i] & 0xFF);
            return data;
        }

        
        public void Connect(String[] values)
        {

            int baudrate = 9600;
            if (values.Length < 2 || !Parser.TryInt(values[1], out baudrate))
                baudrate = 9600;
            String portName = values[0];
            if (sp == null)
            {
                sp = new SerialPort(portName, baudrate);
                sp.ReadTimeout = serialTimeout;
                sp.WriteTimeout = serialTimeout;
                sp.NewLine = "\r";
                sp.Encoding = DefaultEncoding;
                sp.ReadBufferSize = 4060;
                sp.WriteBufferSize = 384;
                sp.DtrEnable = true;
                sp.PinChanged += new SerialPinChangedEventHandler(sp_PinChanged);
            }
            else if (sp.PortName != portName)
            {
                if (sp.IsOpen) sp.Close();
                sp.PortName = portName;

            }
            else return;

            if (!sp.IsOpen)
            {
                try
                {
                    sp.Open();
                }
                catch (System.IO.IOException ex)
                {
                    if (!sp.IsOpen)
                        throw ex;
                }
                catch (Exception ex)
                {
                    sp.Close();
                    throw ex;
                } 
            }

            if (sp.DsrHolding == false)
            {
                FiscalPrinter.Log.Warning("Printer.Connect: Faulty connection cable. Dsr link down");
            }
            Initialize();
            
        }

        private void Initialize()
        {        
            Send(SlipRequest.Initialize());
            Send(SlipRequest.InitSlipPrinter());
            Send(SlipRequest.ChangeFont(Printmode.Font7x7));
            Send(SlipRequest.SetReverseEject(true));
        }

        void sp_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            if (!sp.DsrHolding)
            {
                FiscalPrinter.Log.Error("Serial port disconnected");
            }
            else FiscalPrinter.Log.Error("Serial port connected");
        }

        public IPrinterResponse Pay(Decimal amount)
        {
            return Print(amount, PosMessage.CASH);
        }
        public IPrinterResponse Pay(Decimal amount, String refNumber)
        {
            return Print(amount, PosMessage.CHECK);
        }
        public IPrinterResponse Pay(Decimal amount, ICurrency currency)
        {
            String label = String.Empty;

            Number currencyPayment = new Number(amount / currency.ExchangeRate);

            if (currencyPayment.ToDecimal() > currencyPayment.ToInt())
                label = String.Format("{0} {1:C}", currency.Name, currencyPayment);

            label = String.Format("{0} {1}", currency.Name, currencyPayment);

            return Print(amount, label);
        }
        public IPrinterResponse Pay(Decimal amount, ICredit credit, int installments)
        {
            String label = credit.Name + (installments == 0 ? String.Empty : "/" + installments.ToString());

            return Print(amount, label);
        }

        private IPrinterResponse Print(Decimal amount, String label)
        {
            List<String> paymentItems = SlipPrinter.Invoicepage.FormatPayment(amount, label);
            IPrinterResponse response = null;

            foreach (String row in paymentItems)
            {
                response = Send(SlipRequest.WriteLine(row));
            }

            //ReceiptPrinter prn = receiptPrinter as ReceiptPrinter;
            //prn.ShowPayment(amount);

            return response;
        }

        private new List<PaymentInfo> GetPayments(ISalesDocument Document)
        {
            List<PaymentInfo> payments = new List<PaymentInfo>();
            decimal paidTotal = 0.00m;
            PaymentInfo pi = null;

            //PAYMENTS WITH CHECK
            String[] checkpayments = Document.GetCheckPayments();
            foreach (String checkpayment in checkpayments)
            {
                String[] detail = checkpayment.Split('|');// Amount | RefNumber
                if (detail[1].Length > 12)
                    detail[1] = detail[1].Substring(0, 12);
                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(detail[0]);
                pi.Type = PaymentType.CHECK;
                payments.Add(pi);

                paidTotal += pi.PaidTotal;
            }

            //PAYMENTS WITH CURRENCIES
            String[] currencypayments = Document.GetCurrencyPayments();
            foreach (String currencypayment in currencypayments)
            {
                String[] detail = currencypayment.Split('|');// Amount | Exchange Rate | Name
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
                pi = new PaymentInfo();
                pi.PaidTotal = quantity;
                pi.Type = PaymentType.FCURRENCY;
                pi.Index = id;
                payments.Add(pi);

                paidTotal += amount;
            }

            //PAYMENTS WITH CREDITS
            String[] creditpayments = Document.GetCreditPayments();
            foreach (String creditypayment in creditpayments)
            {
                String[] detail = creditypayment.Split('|');// Amount | Installments | Id
                int id = int.Parse(detail[2]) - 1;

                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(detail[0]);
                pi.Type = PaymentType.CREDIT;
                pi.Index = id;
                payments.Add(pi);

                paidTotal += pi.PaidTotal;
            }

            //PAYMENTS WITH CASH
            String[] cashpayments = Document.GetCashPayments();
            foreach (String cashpayment in cashpayments)
            {
                pi = new PaymentInfo();
                pi.PaidTotal = Decimal.Parse(cashpayment);
                pi.Type = PaymentType.CASH;
                payments.Add(pi);

                paidTotal += pi.PaidTotal;
            }

            return payments;
        }

        private Adjustment ParseAdjLine(string adjLine)
        {
            string[] splitted = adjLine.Split('|');
            decimal amount = decimal.Parse(splitted[0]);

            Adjustment adj = new Adjustment();

            if (splitted[1] == "--")
            {
                adj.Amount = amount;
                if (amount < 0)
                {
                    adj.Type = AdjustmentType.Discount;
                }
                else
                {
                    adj.Type = AdjustmentType.Fee;
                }
            }
            else
            {
                adj.Amount = decimal.Parse(splitted[0]);
                adj.percentage = int.Parse(splitted[1]);
                if (amount < 0)
                {
                    adj.Type = AdjustmentType.PercentDiscount;
                }
                else
                {
                    adj.Type = AdjustmentType.PercentFee;
                }
            }

            return adj;
        }

        private new IPrinterResponse PrintSubTotal(ISalesDocument document, bool hardcopy)
        {
            IPrinterResponse response = null;
            FiscalPrinter.Document = document;

            List<String> subtotalItems = Invoicepage.FormatSubTotal(document);
            if (hardcopy)
            {
                foreach (String s in subtotalItems)
                {
                    response = Send(SlipRequest.WriteLine(s));
                }
            }
            return response;
        }

        private new IPrinterResponse PrintTotals(ISalesDocument document, bool hardcopy)
        {
            SlipPrinter.Document = document;
            if (totalLines == null)
            {
                totalLines = SlipPrinter.Invoicepage.FormatTotals(document);
                line_index_of_totals_to_print = 0;
            }
            IPrinterResponse response = null;

            int start = line_index_of_totals_to_print;
            for (int i = start; i < totalLines.Count; i++)
            {
                response = Send(SlipRequest.WriteLine(totalLines[i]));
                line_index_of_totals_to_print++;
            }

            return response;

        }

        private void PrintPayments()
        {
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
                //pi = new PaymentInfo();
                //pi.PaidTotal = quantity;
                //pi.Type = PaymentType.FCURRENCY;
                //pi.Index = id;
                //pi.SequenceNo = int.Parse(detail[3]);
                //payments.Add(pi);

                string label = String.Format("{0} {1}", detail[2], quantity);

                Print(amount, label);
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
                /*
                String name = "";

                Dictionary<int, ICredit> credits = DataConnector.GetCredits();
                foreach (ICredit credit in credits.Values)
                {
                    if (credit.Id == id)
                    {
                        name = credit.Name;
                        break;
                    }
                }
                */

                //String label = name + (installments == 0 ? String.Empty : "/" + installments.ToString());

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
            foreach(PaymentInfo p in payments)
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

                Print(p.PaidTotal, label);
            }
        }

        public IPrinterResponse PrintEDocument(int docType, string[] lines)
        {
            throw new UndefinedFunctionException();
        }
    }
}
