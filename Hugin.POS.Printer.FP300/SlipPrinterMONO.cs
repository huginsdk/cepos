using System;
using System.Collections.Generic;
//using System.IO.Ports;
using System.Text;
using Hugin.POS.Common;
using Hugin.POS;

namespace Hugin.POS.Printer
{
	class SlipPrinter : FiscalPrinter, IFiscalPrinter
	{

		// Reports an event to request new paper
		public event EventHandler DocumentRequested;
		public event OnMessageHandler OnMessage;

		// when an array (document totals) will be printed and paper is finished, stores last
		// printed index
		private int line_index_of_totals_to_print = 0;

		// stores the documents totals (as formatted lines)
		private List<String> totalLines = null;

		//protected new SerialPort sp = null;
		protected const int serialTimeout = 4500;

		private static InvoicePage invoicePage = null;
		private DateTime lastWRTime = DateTime.Now;
		private const int slipDelay = 150;

		internal SlipPrinter()
		{

		}
		internal bool CheckConnection()
		{
			return true;
		}
		internal static InvoicePage Invoicepage
		{
			get { return invoicePage; }
			set { invoicePage = value; }
		}

		#region IFiscalPrinter Members (Slip Printer)

		public IPrinterResponse Feed()
		{
			SlipResponse response = null;
			return response;
		}

		public IPrinterResponse CutPaper()
		{
			SlipResponse response = null;
			return response;
		}

		public decimal PrinterSubTotal
		{
			get { return SlipPrinter.Invoicepage.SubTotal; }
		}

		public IPrinterResponse Suspend()
		{
			Send(SlipRequest.WriteLine("                                                   BEKLETMEYE ALINAN"));
			return Void();
		}

		internal void VoidOnOpen()
		{
			try
			{
				WriteLine("                BELGE İPTAL             ");
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

		public IPrinterResponse PrintFooter(ISalesDocument document)
		{
			FiscalPrinter.Document = document;

			//PRINT HEADER            
			try
			{
				this.PrintHeader(document, true);
				WaitForSlip();
			}
			catch (PrinterException pe)
			{
				if (OnMessage != null)
					OnMessage(this, new OnMessageEventArgs(pe)); ;
			}

			//PRINT FISCAL ITEMS
			foreach (IFiscalItem fi in document.Items)
			{               
				this.Print(fi);                              
			}
			WaitForSlip();

			String[] docAdjustments = document.GetAdjustments();
			Adjustment adj = null;
			if (docAdjustments.Length > 0)
			{
				adj = ParseAdjLine(docAdjustments[0]);

				if (adj.Type == AdjustmentType.Fee)
				{
					document.TotalAmount -= adj.Amount;
				}
				else
				{
					document.TotalAmount += adj.Amount;
				}
			}

			// PRINT SUBTOTAL            
			WriteLine("");
			PrintSubTotal(document, true);
			WaitForSlip();
			if (adj != null)
			{
				Print(adj);

				if (adj.Type == AdjustmentType.Fee)
				{
					document.TotalAmount += adj.Amount;
				}
				else
				{
					document.TotalAmount -= adj.Amount;
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
			SlipPrinter.Invoicepage.ClearInvoice();
			IPrinterResponse response = null;

			foreach (String s in footerItems)
			{
				response = Send(SlipRequest.WriteLine(s));
			}
			WaitForSlip();
			response = PrintFiscalId();
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
			try
			{
				response = PrintSlipInfoReceipt(document);
				response = EndSlipInfoReceipt();
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return new SlipResponse();
		}

		private IPrinterResponse PrintHeader(ISalesDocument document, bool startSlipInfo)
		{
			IPrinterResponse response = null;

			//if document is not slip document, change printer
			if (document.DocumentTypeId < 0)
			{
				AdjustPrinter(document);
				return Printer.PrintHeader(document);
			}

			try
			{
				if (Invoicepage.Id == 0)
				{
					SlipPrinter.Document.Id = FiscalPrinter.Printer.CurrentDocumentId;
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
					if (!response.HasError)
						document.Id = 0;
				}
				catch (CmdSequenceException cse)
				{
					//if command sequence exception occurs
					switch ((Command)cse.LastCommand)
					{
						//if last command is 37, initialize invoice page
						case Command.START_SLİP:
						if (SlipPrinter.Invoicepage.SubTotal == 0 && Invoicepage.Id == 0)
							SlipPrinter.Invoicepage.ClearInvoice();//after page request
						break;
						case Command.X_DAILY://if x report has not been ended yet
						PrintXReport(true);//print x report
						return PrintHeader(document, false);//then start to print document
						case Command.START_RCPT://if custom report has not been ended yet
						//case Command.CustomReportLine:
						//    RecoverCustomReport();//print do required processes
						//    return PrintHeader(document);//then start to print document
						default:
						throw cse;
					}
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
				response = Send(SlipRequest.WriteLine(rowList[lineCount]));
			}

			return response;
		}

		private IPrinterResponse Print(IFiscalItem fi)
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

		private IPrinterResponse Print(IAdjustment[] ai)
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
			string fiscalNo = String.Format("              {0} {1}", FiscalPrinter.FiscalRegisterNo.Substring(0, 2),
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
			return Send(SlipRequest.WriteLine(line));
		}

		private bool SlipReady()
		{
			return true;
		}

		internal void RequestSlip()
		{

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
			if (irequest is FPURequest) return base.Send((FPURequest)irequest);

			SlipResponse response = new SlipResponse();

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



		}

		private void Initialize()
		{        
			Send(SlipRequest.Initialize());
			Send(SlipRequest.ChangeFont(Printmode.Font7x7));
			Send(SlipRequest.SetReverseEject(true));
		}

		/*void sp_PinChanged(object sender, SerialPinChangedEventArgs e)
		{
			if (!sp.DsrHolding)
			{
				FiscalPrinter.Log.Error("Serial port disconnected");
			}
			else FiscalPrinter.Log.Error("Serial port connected");
		}*/

		private IPrinterResponse Pay(Decimal amount)
		{
			return Print(amount, PosMessage.CASH);
		}
		private IPrinterResponse Pay(Decimal amount, String refNumber)
		{
			return Print(amount, PosMessage.CHECK);
		}
		private IPrinterResponse Pay(Decimal amount, ICurrency currency)
		{
			String label = String.Empty;

			Number currencyPayment = new Number(amount / currency.ExchangeRate);

			if (currencyPayment.ToDecimal() > currencyPayment.ToInt())
				label = String.Format("{0} {1:C}", currency.Name, currencyPayment);

			label = String.Format("{0} {1}", currency.Name, currencyPayment);

			return Print(amount, label);
		}
		private IPrinterResponse Pay(Decimal amount, ICredit credit, int installments)
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

		private List<PaymentInfo> GetPayments(ISalesDocument Document)
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
				if (amount < 0)
				{
					adj.Amount = -1 * amount;
					adj.Type = AdjustmentType.Discount;
				}
				else
				{
					adj.Amount = amount;
					adj.Type = AdjustmentType.Fee;
				}
			}
			else
			{
				if (amount < 0)
				{
					adj.Amount = -1 * int.Parse(splitted[1]);
					adj.Type = AdjustmentType.PercentDiscount;
				}
				else
				{
					adj.Amount = int.Parse(splitted[1]);
					adj.Type = AdjustmentType.PercentFee;
				}
			}

			return adj;
		}

		private IPrinterResponse PrintSubTotal(ISalesDocument document, bool hardcopy)
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

		private IPrinterResponse PrintTotals(ISalesDocument document, bool hardcopy)
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

			//PAYMENTS WITH CHECK
			List<String> checkpayments = new List<string>(Document.GetCheckPayments());
			while (checkpayments.Count > 0)
			{
				String[] detail = checkpayments[0].Split('|');// Amount | RefNumber
				if (detail[1].Length > 12)
					detail[1] = detail[1].Substring(0, 12);
				decimal amount = Decimal.Parse(detail[0]);

				Print(amount, PosMessage.CHECK);
				paidTotal += amount;
				checkpayments.RemoveAt(0);
			}

			//PAYMENTS WITH CURRENCIES
			List<String> currencypayments = new List<string>(Document.GetCurrencyPayments());
			while (currencypayments.Count > 0)
			{
				String[] detail = currencypayments[0].Split('|');// Amount | Exchange Rate | Name
				Decimal amount = Decimal.Parse(detail[0]);
				Decimal exchangeRate = Decimal.Parse(detail[1]);

				String label = String.Empty;
				Number currencyPayment = new Number(amount / exchangeRate);

				label = String.Format("{0} {1}", detail[2], currencyPayment);

				Print(amount, label);
				paidTotal += amount;
				currencypayments.RemoveAt(0);
			}

			//PAYMENTS WITH CREDITS
			List<String> creditpayments = new List<string>(Document.GetCreditPayments());
			while (creditpayments.Count > 0)
			{
				String[] detail = creditpayments[0].Split('|');// Amount | Installments | Id
				Decimal amount = Decimal.Parse(detail[0]);
				Int32 installments = Int32.Parse(detail[1]);
				Int32 id = Int32.Parse(detail[2]);
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

				String label = name + (installments == 0 ? String.Empty : "/" + installments.ToString());

				Print(amount, label);
				paidTotal += amount;
				creditpayments.RemoveAt(0);
			}

			//PAYMENTS WITH CASH
			List<String> cashpayments = new List<string>(Document.GetCashPayments());
			while (cashpayments.Count > 0)
			{
				decimal amount = Decimal.Parse(cashpayments[0]);
				Print(amount, PosMessage.CASH);
				paidTotal += amount;
				cashpayments.RemoveAt(0);
			}
		}
	}
}
