using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Hugin.POS.States;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;
using System.Text;

namespace Hugin.POS
{
    public delegate void SaleEventHandler(object sender, SaleEventArgs e);
    public delegate void PaymentEventHandler(object sender, PaymentEventArgs e);
    public delegate void CustomerEventHandler(object sender);
    public delegate void OnCloseEventHandler(object sender);

    public class SaleEventArgs : EventArgs
    {
        FiscalItem newItem;
        public SaleEventArgs(FiscalItem newItem)
        {
            this.newItem = newItem;
        }

        public FiscalItem NewItem
        {
            get { return newItem; }
        }

    }
    public class PaymentEventArgs : EventArgs
    {
        PaymentInfo newPayment;
        public PaymentEventArgs(PaymentInfo newPayment)
        {
            this.newPayment = newPayment;
        }

        public PaymentInfo NewPayment
        {
            get { return newPayment; }
        }

    }

    public enum DocumentStatus : byte { Active, Paying, Closed, Voided, Suspended, Transferred, Cancelled };
    
    enum OrderCommand
    {
        PrintHeader,
        SaleItem,
        SubTotal,
        Adjustment,
        Payment,
        Remark,
        Close
    }

    public abstract class SalesDocument : IMenuItem, IAdjustable, ISalesDocument
    {
        public static event SaleEventHandler ItemSold;
        public static event SaleEventHandler ItemUpdated;
        public static event PaymentEventHandler PaymentMade;
        public static event OnCloseEventHandler OnClose;
        public static event OnCloseEventHandler OnVoid;
        public static event OnCloseEventHandler OnSuspend;
        public static event CustomerEventHandler CustomerChanged;
        public static event EventHandler OnUndoAdjustment;

        const long MAXTOTALAMOUNT = 9999999999;
        const int MAX_PAYMENT_COUNT = 10;

        protected List<FiscalItem> items = new List<FiscalItem>();
        protected List<Adjustment> adjustments = new List<Adjustment>();
        protected List<PaymentInfo> payments = new List<PaymentInfo>();
        protected DateTime createdDate;
        protected Decimal subTotal;
        protected ICashier salesPerson;
        protected Decimal balanceDue;
        protected Decimal totalVAT;
        protected DocumentState state;
        protected DocumentStatus status;
        protected FiscalItem lastItem;
        protected IProduct soldProduct;
        protected ICustomer currentCustomer;
        protected Decimal customerChange;
        protected Dictionary<IProduct, Decimal> productTotals;
        protected List<String> footNotes = new List<string>();
        protected List<String> currentLog = new List<string>();
        protected List<String> remark = new List<string>();
        protected List<PointObject> points = new List<PointObject>();

        protected int resumedFromDocumentId;

        private String registerAddress;
        private String slipSerialNo;
        private DateTime issueDate;
        private String customerTitle;
        private Decimal comissionAmount;
        private String slipOrderNo;
        private String tcknVkn;
        private String returnReason;
        private DateTime registerDate;
        private Boolean isOpenDocument = false;
        private Boolean repeatedDocument = false;
        private Boolean addSalesPersonEachSales = false;
        private FileHelper fileOnDisk = null;
        private Decimal promotedTotal = 0.00m;
        private List<String> promoLogLines = new List<string>();
        private AdditionalDocInfo additionalInfo = null;
        private bool printSlipInfo = false;
        private static string customerChangeMsg = String.Empty;
        private static bool isTryInvoiceAgain = false;
        private static bool isInvoicePrinted = true;
        private int documentFileZNo = 0;
        private static bool continueExternalPrinter = true;
        private String serviceDefinition;
        private Decimal serviceGrossWages = 0.0M;
        private int serviceStoppageRate = 0;
        private int serviceVATRate = 0;
        private int serviceStoppageOtherRate = 0;

        public SalesDocument()
        {
            createdDate = DateTime.Now;
            status = DocumentStatus.Active;
            state = States.DocumentOpen.Instance();
            productTotals = new Dictionary<IProduct, Decimal>();
            footNotes = new List<string>();
        }
        //Belge transferlerinde kullaniliyor
        public SalesDocument(SalesDocument doc)
        {

            createdDate = DateTime.Now;
            SalesPerson = doc.salesPerson;
            Customer = doc.Customer;
            resumedFromDocumentId = doc.ResumedFromDocumentId;
            items = new List<FiscalItem>();
            fileOnDisk = doc.FileOnDisk;
            slipOrderNo = doc.slipOrderNo;
            slipSerialNo = doc.slipSerialNo;
            isOpenDocument = doc.isOpenDocument;
            repeatedDocument = doc.repeatedDocument;
            promotedTotal = doc.promotedTotal;
            returnReason = doc.returnReason;
            tcknVkn = doc.tcknVkn;
            issueDate = doc.issueDate;
            customerTitle = doc.customerTitle;

            if (doc.Items != null)
            {
                foreach (FiscalItem fi in doc.Items)
                {
                    FiscalItem cloneItem = (FiscalItem)fi.Clone();
                    cloneItem.ClearEventLog();
                    //cloneItem.Adjustments.Clear();

                    CanAddItem(cloneItem);

                    items.Add(cloneItem);
                    cloneItem.OnTotalAmountUpdated += new OnTotalAmountUpdatedEventHandler(fiscalItem_OnTotalAmountUpdated);

                }
            }
            if (items.Count > 0)
            {
                lastItem = items[items.Count - 1];
            }
            subTotal = doc.TotalAmount;
            if (doc.Adjustments != null)
                foreach (Adjustment adj in doc.Adjustments)
                    subTotal -= adj.NetAmount;
            productTotals = new Dictionary<IProduct, decimal>();

            if (doc.ProductTotals != null)
            {
                foreach (IProduct p in doc.productTotals.Keys)
                    productTotals.Add(p, doc.productTotals[p]);
            }

            foreach (string footNote in doc.FootNote)
            {
                footNotes.Add(footNote);
            }

            // Belgenin odeme islemlerinden onceki stateinde klonlamasi gerekiyor
            payments = new List<PaymentInfo>();
            //points = doc.Points;
            balanceDue = subTotal;
            State = DocumentOpen.Instance();
            Status = DocumentStatus.Active;

        }

        public abstract object Clone();

        public static SalesDocument CreateByName(String docType)
        {
            switch (docType)
            {
                case PosMessage.RECEIPT:
                    return new Receipt();
                case PosMessage.INVOICE:
                    return new Invoice();
                case PosMessage.WAYBILL:
                    return new Waybill();
                case PosMessage.RETURN_DOCUMENT:
                    return new ReturnDocument();
                default:
                    return new Receipt();

            }
        }
        public ICustomer Customer
        {
            get { return currentCustomer; }
            set
            {
                if (currentCustomer == value) return;
                if (CustomerChanged != null)
                    CustomerChanged(value);
                currentCustomer = value;
                if (this == cr.Document)
                    if (currentCustomer == null)
                        DisplayAdapter.Cashier.LedOff(Leds.Customer);
                    else DisplayAdapter.Cashier.LedOn(Leds.Customer);
            }
        }
        public Decimal CustomerChange
        {
            get { return customerChange; }
            set { customerChange = value; }
        }

        public DateTime CreatedDate
        {
            get { return createdDate; }
            set { createdDate = value; }
        }

        public abstract int Id { get;set;}
        public abstract String Name { get;}
        public abstract String Code { get;}
        public virtual int DocumentTypeId { get { return -1; } }
        public virtual bool CanEmpty { get { return false; } }
        public virtual bool IsEmpty
        {
            get 
            {
                if (this.CanEmpty)
                    return false;
                else
                    return items.Count == 0; 
            }
        }

        public int ResumedFromDocumentId
        {
            get { return resumedFromDocumentId; }
            set { resumedFromDocumentId = value; }
        }

        public FileHelper FileOnDisk
        {
            get { return fileOnDisk; }
            set { fileOnDisk = value; }
        }

        public List<FiscalItem> Items
        {
            get { return items; }
        }
        public Dictionary<IProduct, Decimal> ProductTotals
        {
            get { return productTotals; }
            set { productTotals = value; }
        }

        public string SlipSerialNo
        {
            get { return slipSerialNo; }
            set { slipSerialNo = value; }
        }

        public DateTime IssueDate
        {
            get { return issueDate; }
            set { issueDate = value; }
        }

        public String CustomerTitle
        {
            get { return customerTitle; }
            set { customerTitle = value; }
        }

        public Decimal ComissionAmount
        {
            get { return comissionAmount; }
            set { comissionAmount = value; }
        }

        public string SlipOrderNo
        {
            get { return slipOrderNo; }
            set { slipOrderNo = value; }
        }

        public Boolean IsOpenDocument
        {
            get { return isOpenDocument; }
            set { isOpenDocument = value; }
        }

        public string ReturnReason
        {
            get { return returnReason; }
            set { returnReason = value; }
        }

        public Boolean RepeatedDocument
        {
            get { return repeatedDocument; }
        }

        public Boolean AddSalesPersonEachSales
        {
            get { return addSalesPersonEachSales; }
        }

        public Decimal PromotedTotal
        {
            get { return promotedTotal; }
        }

        public String TcknVkn
        {
            get { return tcknVkn; }
            set { tcknVkn = value; }
        }

        public AdditionalDocInfo AdditionalInfo
        {
            get { return additionalInfo; }
            set { additionalInfo = value; }
        }

        public bool PrintSlipInfo
        {
            get { return printSlipInfo; }
            set { printSlipInfo = value; }
        }

        public List<string> CurrentLog
        {
            get { return currentLog; }
            set { currentLog = value; }
        }
        
        public int DocumentFileZNo
        {
            get { return documentFileZNo; }
            set { documentFileZNo = value; }
        }

        public string ServiceDefinition
        {
            get { return serviceDefinition; }
            set { serviceDefinition = value; }
        }

        public Decimal ServiceGrossWages
        {
            get { return serviceGrossWages; }
            set { serviceGrossWages = value; }
        }

        public int ServiceStoppageRate
        {
            get { return serviceStoppageRate; }
            set { serviceStoppageRate = value; }
        }

        public int ServiceVATRate
        {
            get { return serviceVATRate; }
            set { serviceVATRate = value; }
        }

        public int ServiceStoppageOtherRate
        {
            get { return serviceStoppageOtherRate; }
            set { serviceStoppageOtherRate = value; }
        }

        public Dictionary<Department, Decimal> DepartmentTotals
        {
            get
            {
                Dictionary<Department, decimal> depSales = new Dictionary<Department, decimal>();
                Dictionary<int, decimal> tgSales = new Dictionary<int, decimal>();

                foreach (IProduct p in ProductTotals.Keys)
                {
                    if (depSales.ContainsKey(p.Department))
                        depSales[p.Department] += ProductTotals[p];
                    else
                        depSales.Add(p.Department, ProductTotals[p]);
                    if (tgSales.ContainsKey(p.Department.TaxGroupId))
                        tgSales[p.Department.TaxGroupId] += ProductTotals[p];
                    else
                        tgSales.Add(p.Department.TaxGroupId, ProductTotals[p]);
                }

                //apply adjustments
                //Dictionary<int, decimal> tgTotals = TaxGroupTotals;
                Dictionary<Department, decimal> depTotals = new Dictionary<Department, decimal>();

                //copy because dictionary property can not be changed in foreach loop
                foreach (Department d in depSales.Keys)
                    depTotals.Add(d, depSales[d]);

                /*
                foreach (int key in tgSales.Keys)
                {
                    decimal adj = tgTotals[key] - tgSales[key];
                    if (adj == 0) continue;
                    decimal remaing = adj;
                    Department maxSaleDep = null;
                    foreach (Department d in depSales.Keys)
                    {
                        if (d.TaxGroupId != key) continue;
                        depTotals[d] += Math.Round(adj * depSales[d] / tgSales[key], 2);
                        remaing -= Math.Round(adj * depSales[d] / tgSales[key], 2);
                        if (maxSaleDep == null)
                            maxSaleDep = d;
                        else if (depSales[maxSaleDep] < depSales[d])
                            maxSaleDep = d;
                    }
                    //apply remaining amount to max sale department
                    if (remaing != 0)
                        depTotals[maxSaleDep] += remaing;
                }
                */
                return depTotals;
            }
        }

        public Dictionary<int, Decimal> TaxGroupTotals
        {
            get
            {
                Dictionary<int, Decimal> taxGroupTotals = new Dictionary<int, decimal>();
                foreach (IProduct p in productTotals.Keys)
                {
                    if (p == null || p.Id == -1)
                        throw new DataChangedException();
                    if (taxGroupTotals.ContainsKey(p.Department.TaxGroupId - 1))
                        taxGroupTotals[p.Department.TaxGroupId - 1] += productTotals[p];
                    else taxGroupTotals.Add(p.Department.TaxGroupId - 1, productTotals[p]);
                }
                if (this.Adjustments != null)
                {
                    for (int i = 0; i < this.Adjustments.Length; i++)
                    {
                        decimal[] temp = this.Adjustments[i].GetTaxGroupAdjustments();
                        int[] keys = new int[taxGroupTotals.Count];
                        taxGroupTotals.Keys.CopyTo(keys, 0);

                        for (int j = 0; j < keys.Length; j++)
                        {
                            taxGroupTotals[keys[j]] += temp[keys[j]];
                        }
                    }
                }
                return taxGroupTotals;
            }
        }

        public Decimal[,] TaxRateTotals
        {
            get
            {

                Dictionary<int, decimal> tgTotals = TaxGroupTotals;
                List<decimal> taxRates = new List<decimal>();

                foreach (Decimal tr in Department.TaxRates)
                {
                    if (!taxRates.Contains(tr))
                        taxRates.Add(tr);
                }
                taxRates.Sort();
                decimal[,] taxRateTotals = new Decimal[taxRates.Count, 3];
                foreach (int taxGroupId in tgTotals.Keys)
                {
                    if (taxGroupId > -1)
                    {
                        decimal taxRate = Department.TaxRates[taxGroupId];
                        int index = taxRates.IndexOf(taxRate);
                        taxRateTotals[index, 0] = taxGroupId;
                        taxRateTotals[index, 1] += Rounder.RoundDecimal(tgTotals[taxGroupId] * taxRate / (100 + taxRate), 2, true);
                        taxRateTotals[index, 2] += tgTotals[taxGroupId];
                    }
                }
                int validTotals = 0;
                int k = 0;
                for (int i = 0; i < taxRates.Count; i++)
                    if (taxRateTotals[i, 2] > 0) validTotals++;
                decimal[,] taxRateTotals2 = new Decimal[validTotals, 3];
                for (int i = 0; i < taxRates.Count; i++)
                    if (taxRateTotals[i, 2] > 0)
                    {
                        for (int j = 0; j < 3; j++)
                            taxRateTotals2[k, j] = taxRateTotals[i, j];
                        k++;
                    }
                return taxRateTotals2;
            }
        }

        public FiscalItem LastItem { get { return lastItem; } set { lastItem = value; } }

        public List<PaymentInfo> Payments
        {
            get { return payments; }
        }

        #region ISalesDocument Members


        public string[] GetCashPayments()
        {
            List<String> cashpayments = new List<string>();

            foreach (PaymentInfo pi in Payments)
            {
                if (pi is CashPaymentInfo)
                    cashpayments.Add(pi.Amount.ToString() + "|" + pi.SequenceNo.ToString());
            }
            return cashpayments.ToArray();
        }

        public string[] GetCheckPayments()
        {
            List<String> checkpayments = new List<string>();

            foreach (PaymentInfo pi in Payments)
            {
                if (pi is CheckPaymentInfo)
                    checkpayments.Add(pi.Amount.ToString() + "|" + ((CheckPaymentInfo)pi).RefNumber + "|" + pi.SequenceNo.ToString());
            }
            return checkpayments.ToArray();
        }

        public string[] GetCurrencyPayments()
        {
            List<String> currencypayments = new List<string>();

            foreach (PaymentInfo pi in Payments)
            {
                if (pi is CurrencyPaymentInfo)
                    currencypayments.Add(pi.Amount.ToString() + "|" + ((CurrencyPaymentInfo)pi).ExchangeRate + "|" + pi.Name + "|" + pi.SequenceNo.ToString());
            }
            return currencypayments.ToArray();
        }

        public string[] GetCreditPayments()
        {
            List<String> creditpayments = new List<string>();

            foreach (PaymentInfo pi in Payments)
            {
                if (pi is CreditPaymentInfo)
                    creditpayments.Add(pi.Amount.ToString() + "|" + ((CreditPaymentInfo)pi).Installments + "|" + ((CreditPaymentInfo)pi).Id + "|" + ((CreditPaymentInfo)pi).Credit.PayViaEft.ToString() + "|" + pi.SequenceNo.ToString());
            }
            return creditpayments.ToArray();
        }

        public string[] GetAdjustments()
        {

            List<String> adjs = new List<string>();
            if (Adjustments != null)
            {
                foreach (Adjustment adj in Adjustments)
                {
                    string percentage = "--";
                    if (adj.Method == AdjustmentType.PercentFee || adj.Method == AdjustmentType.PercentDiscount)
                        percentage = String.Format("{0:D2}", (int)Math.Round(adj.RequestValue, 0));
                    adjs.Add(adj.NetAmount.ToString() + "|" + percentage + "|" + adj.AuthorizingCashierId);
                }
            }
            return adjs.ToArray();
        }

        List<IFiscalItem> ISalesDocument.Items
        {
            get
            {
                List<IFiscalItem> ifiscalitems = new List<IFiscalItem>();
                foreach (FiscalItem fi in Items)
                    ifiscalitems.Add(fi);
                return ifiscalitems;
            }
        }

        #endregion

        public virtual Decimal TotalAmount
        {

            get
            { //return subTotal;
                return Math.Round(subTotal, 2);
            }
            set
            {
                balanceDue = value;
                subTotal = value;
            }
        }

        public Decimal ListedAmount
        {
            get
            {
                Decimal listedSum = 0m;
                foreach (FiscalItem item in items)
                    listedSum += item.ListedAmount;
                return listedSum;
            }
        }

        public virtual Decimal TotalVAT
        {
            //Would have been better to use the TotalVAT function of each salesitem
            //individually and sum them up but according to Turkish fiscal regulation 
            //we need to first sum up all department totals and sum up
            get
            {
                totalVAT = 0;
                Dictionary<int, Decimal> tgTotals = TaxGroupTotals;
                foreach (KeyValuePair<int, Decimal> taxGroupTotal in tgTotals)
                {
                    //if (taxGroupTotal.Key == 0) continue; //not sure how this happened but it did (invoice)
                    decimal taxRate = Department.TaxRates[taxGroupTotal.Key];
                    decimal dVAT = (taxRate * taxGroupTotal.Value) / (100 + taxRate);
                    totalVAT += Rounder.RoundDecimal(dVAT, 2, true);
                }
                totalVAT = (totalVAT < 0m) ? 0m : totalVAT;
                return totalVAT;
            }
        }

        public Decimal BalanceDue
        {
            get { return balanceDue; }
            set { balanceDue = value; }
        }
        public Adjustment[] Adjustments
        {
            get { return (adjustments == null) ? null : adjustments.ToArray(); }
        }
        public ICashier SalesPerson
        {
            get { return salesPerson; }
            set { salesPerson = value; }
        }
        internal DocumentState State
        {
            get { return state; }
            set { state = value; }
        }
        protected internal DocumentStatus Status
        {
            get { return status; }
            protected set { status = value; }
        }

        public List<String> FootNote
        {
            get { return footNotes == null ? new List<String>() : footNotes; }
            set { footNotes = value; }
        }

        public string[] PromoLogLines
        {
            get { return promoLogLines.ToArray(); }
            set { promoLogLines = new List<string>(value); }
        }

        public List<String> Remark
        {
            get { return remark; }
            set { remark = value; }
        }
        //isRequired
        /*  public override String ToString() 
          {
              String format = "{0}{4}{1}\t*{3}\n{5}\t{2:H:mm}";
              String cashiername = SalesPerson.CashierName;
              cashiername = cashiername.Substring(0, Math.Min(14, cashiername.Length));
              return String.Format(format, Name.Substring(0, 3),
                                            Id,
                                            CreatedDate,
                                            new Number(TotalAmount),
                                            (Id > 999) ? ":" : " NO:",
                                            cashiername);
        
          }*/
        public virtual bool CanAddItem(FiscalItem item)
        {
            if (state is DocumentPaying) return false;
            if (item.TotalAmount >= 10000000) return false;
            if (item.Product == null || item.Product.Id == -1)
                return false;

            if (item is VoidItem)
            {
                if (ProductTotals == null) return false;

                if (ProductTotals.ContainsKey(item.Product))
                    return ((ProductTotals[item.Product] + item.TotalAmount) >= 0);
                else return false;
            }
            return item.TotalAmount + this.TotalAmount < MAXTOTALAMOUNT;
        }

        public virtual bool CanAdjust(Adjustment adjustment)
        {
            if (state is DocumentPaying) return false;
            if (adjustment.NetAmount == 0) return false;
            if (TotalAmount + adjustment.NetAmount < 0) return false;
            if (TotalAmount + adjustment.NetAmount >= MAXTOTALAMOUNT) return false;
            return true;
        }

        public virtual void AddItem(FiscalItem item, bool itemSold)
        {
            if (item.Product == null || item.Product.Id == -1)
                throw new DataChangedException();
            state.AddItem(this, item);
            TotalAmount += item.TotalAmount;
            BalanceDue = TotalAmount; // TODO belki TotalAmount.set in icine girebilir
            lastItem = item;
            item.OnTotalAmountUpdated += new OnTotalAmountUpdatedEventHandler(fiscalItem_OnTotalAmountUpdated);
            if (itemSold)
                ItemSold(this, new SaleEventArgs(item));
            status = DocumentStatus.Active;
        }

        public virtual void UpdateItem(FiscalItem item, Number quantity)
        {
            if (item.Product == null || item.Product.Id == -1)
                throw new DataChangedException();

            foreach (FiscalItem i in items)
            {
                if(i.Product.Id == item.Product.Id &&
                    i.Quantity == item.Quantity)
                {
                    TotalAmount -= i.TotalAmount;
                    i.Quantity = quantity.ToDecimal();
                    TotalAmount += i.TotalAmount;
                    BalanceDue = TotalAmount;

                    if (ItemUpdated != null)
                        ItemUpdated(this, new SaleEventArgs(item));
                }
            }
        }

        public void fiscalItem_OnTotalAmountUpdated(object sender, PriceUpdateEventArgs e)
        {

            FiscalItem fiscalItem = sender as FiscalItem;
            if (productTotals.ContainsKey(fiscalItem.Product) &&
                productTotals[fiscalItem.Product] + e.NewPrice - e.OldPrice >= 0)
            {
                productTotals[fiscalItem.Product] += (e.NewPrice - e.OldPrice);
                TotalAmount += (e.NewPrice - e.OldPrice);
            }
        }

        public IState ConfirmSalesPerson(ICashier salesPerson)
        {
            if (salesPerson != null)
            {
                Confirm confirm = new Confirm(String.Format("{0}{1}", PosMessage.CLERK_FOR_DOCUMENT, salesPerson.Name.TrimEnd()),
                                                              new StateInstance<Hashtable>(SaveSalesPerson),
                                                              new StateInstance(States.Start.Instance));
                confirm.Data.Add("SalesPerson", salesPerson);
                return States.ConfirmCashier.Instance(confirm);
            }
            else
            {
                addSalesPersonEachSales = true;
                return cr.State = States.Start.Instance();
            }
        }

        public IState SaveSalesPerson(Hashtable args)
        {
            SalesPerson = args["SalesPerson"] as ICashier;
            State = DocumentOpen.Instance();
            return Start.Instance();
        }

        public virtual IState VoidSalesPerson()
        {
            SalesPerson = null;
            return EnterClerkNumber.Instance(PosMessage.CLERK_ID,
                                          new StateInstance<ICashier>(ConfirmSalesPerson),
                                          new StateInstance(Start.Instance));
        }

        public Decimal Adjust(IAdjustment adjustment)
        {
            //document total could have changed since adjustment was created
            //in case of percent adjustments, the netvalue needs to be recalculated
            adjustments.Add((Adjustment)adjustment);
            TotalAmount += adjustment.NetAmount;
            state = States.DocumentPaying.Instance();
            return adjustment.NetAmount;

        }

        public virtual IState Pay(PaymentInfo paymentInfo)
        {
            if (subTotal == 0)
            {
                state = States.DocumentOpen.Instance();
                return AlertCashier.Instance(new Confirm(PosMessage.SALE_NOT_FOUND));
            }


            decimal amountControl = paymentInfo.Amount - balanceDue;
            if (amountControl > cr.Printer.CashAmountInDrawer)
            {
                return States.AlertCashier.Instance(new Confirm(PosMessage.NEGATIVE_RESULT_EXCEPTION));
            }

            bool isPaymentbyTicket = false;
            if (paymentInfo.Amount == 0 ||
                (paymentInfo.Amount > balanceDue && (paymentInfo is CheckPaymentInfo ||
                                                     paymentInfo is CreditPaymentInfo)))
            {
                //1) Non-cash payments cannot result in a cash remainder
                //2) If payment amount unspecified then payment equals subtotal
                paymentInfo.Amount = balanceDue;
            }

            //if ((paymentInfo is CreditPaymentInfo) && ((CreditPaymentInfo)paymentInfo).Credit.IsTicket)
            //{
            //    IAdjustment adj = new Adjustment(cr.Document, AdjustmentType.Discount, Math.Min(paymentInfo.Amount, cr.Document.BalanceDue));
            //    cr.Printer.Print(adj);
            //    cr.Document.Adjust(adj);

            //    isPaymentbyTicket = true;
            //}

            //if (this.status != DocumentStatus.Paying || isPaymentbyTicket)
            //{
            //    //First payment
            //    try
            //    {
            //        cr.Printer.PrintTotals(this, false);
            //    }
            //    catch (InvalidProgramException ipe)
            //    {
            //        cr.Log.Error(ipe);
            //        Void();
            //        return States.Start.Instance();
            //    }
            //    balanceDue = subTotal;
            //}
            if (balanceDue >= 0)
            {
                try
                {
                    if (payments.Count >= (MAX_PAYMENT_COUNT - 1))
                    {
                        if (paymentInfo.Amount < balanceDue)
                        {
                            throw new Exception(String.Format(PosMessage.LAST_PAYMENT, MAX_PAYMENT_COUNT));
                        }
                    }
                    if (paymentInfo is CashPaymentInfo)
                        cr.Printer.Pay(paymentInfo.Amount);
                    else if (paymentInfo is CheckPaymentInfo)
                        cr.Printer.Pay(paymentInfo.Amount, ((CheckPaymentInfo)paymentInfo).RefNumber);
                    else if (paymentInfo is CurrencyPaymentInfo)
                    {
                        if (cr.Printer.MaxNumberOfCurrencies > 0)
                            cr.Printer.Pay(Math.Round(paymentInfo.Amount / ((CurrencyPaymentInfo)paymentInfo).ExchangeRate, 3),
                                ((CurrencyPaymentInfo)paymentInfo).Currency);
                        else
                            cr.Printer.Pay(paymentInfo.Amount, ((CurrencyPaymentInfo)paymentInfo).Currency);
                    }
                    else if (paymentInfo is CreditPaymentInfo)
                    {
                        CreditPaymentInfo cpi = (CreditPaymentInfo)paymentInfo;
                        decimal amount = paymentInfo.Amount;

                        if (isPaymentbyTicket)
                        {
                            paymentInfo.Amount = 0;
                            if (paymentInfo.Amount >= balanceDue)
                                cr.Printer.Pay(0m, cpi.Credit, cpi.Installments);
                            cpi.Remark = String.Format("{0}\t{1}", paymentInfo.Name, ("*" + amount.ToString("0.00")));
                            System.Threading.Thread.Sleep(50);
                        }
                        else
                        {
                            if (cpi.Credit.PayViaEft)
                            {
                                // Release adjusted printer if doc is invoice
                                if (cr.Document is Invoice ||cr.Document is ReturnDocument)
                                    cr.Printer.ReleasePrinter();

                                // Get EFT 
                                cr.Printer.SaleDocument = this;

                                //if (cr.Document is ReturnDocument)
                                //{
                                //    GetAcquierID();
                                //    cr.Printer.RefundEFTPayment(currentAcquierID, paymentInfo.Amount);
                                //}
                                //else
                                    cr.Printer.Pay(paymentInfo.Amount, cpi.Credit, cpi.Installments);
                            }

                            if(PosConfiguration.IsPrinterGUIActive)
                                cr.Printer.Pay(paymentInfo.Amount, cpi.Credit, cpi.Installments);

                            if (!String.IsNullOrEmpty(((CreditPaymentInfo)paymentInfo).Remark))
                                cr.Printer.PrintRemark(((CreditPaymentInfo)paymentInfo).Remark);
                        }

                    }

                    if (this.status != DocumentStatus.Paying)
                    {
                        this.status = DocumentStatus.Paying;
                        this.state = States.DocumentPaying.Instance();
                    }
                }
                catch (CmdSequenceException cse)
                {
                    //Should only occur due to power cut during or right before MF printing
                    cr.Log.Error(cse);
                }
                paymentInfo.SequenceNo = payments.Count; // Add same sequence to pi on payments list
                payments.Add(paymentInfo.Clone());
            }

            if (paymentInfo.Amount >= balanceDue)
            {
                try
                {
                    List<PaymentInfo> creditPayments = this.Payments.FindAll(delegate(PaymentInfo pi)
                    {
                        return (pi is CreditPaymentInfo) && ((CreditPaymentInfo)pi).Credit.IsTicket;
                    });
                    foreach (CreditPaymentInfo ci in creditPayments)
                    {
                        System.Threading.Thread.Sleep(50);
                        cr.Printer.PrintRemark(ci.Remark);
                    }
                }
                catch (Exception ex)
                {
                    cr.Log.Error(ex);
                }

                //Customer paid in full or more than the due balance
                //Display appropriate change set balance to 0 and close sale
                customerChange = paymentInfo.Amount - balanceDue;

                // Customer change msg before printing document
                string label1 = "";
                if (paymentInfo is CheckPaymentInfo)
                {
                    if (((CheckPaymentInfo)paymentInfo).RefNumber != String.Empty)
                        label1 = String.Format("(NO: {0})", ((CheckPaymentInfo)paymentInfo).RefNumber);
                    customerChangeMsg = DisplayAdapter.AmountPairFormat(paymentInfo.ToString(), paymentInfo.Amount, label1);
                }
                else if (paymentInfo is CreditPaymentInfo)
                {
                    if (((CreditPaymentInfo)paymentInfo).Installments > 0)
                        label1 = String.Format("{0} {1}", ((CreditPaymentInfo)paymentInfo).Installments, PosMessage.INSTALLMENT);
                    customerChangeMsg = DisplayAdapter.AmountPairFormat(paymentInfo.ToString(), paymentInfo.Amount, label1);
                }
                else
                    customerChangeMsg = DisplayAdapter.AmountPairFormat(paymentInfo.ToString(), paymentInfo.Amount, PosMessage.CHANGE, customerChange);

                DisplayAdapter.Both.Show(customerChangeMsg);
                customerChange = 0;

                balanceDue = 0;

                PaymentMade(this, new PaymentEventArgs(paymentInfo));

                return ManagePoint();
            }
            else
            {
                balanceDue -= paymentInfo.Amount;

                String tmpStrPaymentInfo = paymentInfo.ToString();
                decimal tmpPaymentAmount = paymentInfo.Amount;
                decimal tmpBalanceDue = balanceDue;

                PaymentMade(this, new PaymentEventArgs(paymentInfo));
                DisplayAdapter.Both.Show(tmpStrPaymentInfo, tmpPaymentAmount, PosMessage.BALANCE, tmpBalanceDue, true);
                return States.Payment.Instance(String.Empty);
            }
        }

        private void GetAcquierID()
        {
            cr.State = EnterInteger.Instance(PosMessage.ACQUIER_ID,
                                                   new StateInstance<int>(SetAcquierId));
        }

        private static int currentAcquierID;
        private static IState SetAcquierId(int acquierId)
        {
            currentAcquierID = acquierId;
            return cr.State;
        }

        public virtual void Cancel()
        {
            //TODO Check doccount and doctotal on FPU to decide what to do with this doc
            status = DocumentStatus.Cancelled;
            cr.Log.Error("Following document was cancelled\r\n" + cr.DataConnector.FormatLines(cr.Document));
        }
        public virtual void VoidPayment(Object o)
        {
            Pay(((PaymentInfo)o).Void());
        }
        public virtual void Void()
        {
            try
            {
                try
                {
                    TotalAmount = cr.Printer.PrinterSubTotal;
                }
                catch (PowerFailureException) { }

                if (!(this is ReturnDocument))
                {
                    if (this.Items != null && this.items.Count > 0 && 
                        this.Payments.Count == 0)
                        cr.Printer.PrintFooter(this, true);
                }

                cr.Void();
            }
            catch (CmdSequenceException cse)
            {
                try
                {
                    if (cr.Document.Id == 0)
                        cr.Document.Id = cr.Printer.CurrentDocumentId;//onvoid
                }
                catch { cr.Document.Id++; }

                cr.Log.Warning(cse);
            }

            if (!(status == DocumentStatus.Suspended ||
                status == DocumentStatus.Transferred ||
                status == DocumentStatus.Cancelled
                ))
                status = DocumentStatus.Voided;

            cr.SecurityConnector.EscapeCustomer();
            if (cr.Document.Id == 0 && cr.Document.IsEmpty)//if report
                return;
            OnVoid(this);
            if (ResumedFromDocumentId > 0 || FileOnDisk != null)
            {
                try
                {
                    DocumentFileHelper helper = new DocumentFileHelper(FileOnDisk);
                    helper.Id = Id;
                    helper.Remove(this);

                }
                catch (Exception) { }
            }
        }

        public void VoidSuspended()
        {
            if (status == DocumentStatus.Suspended)
            {
                try
                {
                    TotalAmount = cr.Printer.PrinterSubTotal;
                }
                catch (PowerFailureException) { }
                if (this.Items != null && this.items.Count > 0)
                    cr.Printer.PrintFooter(this, true);
                cr.Void();

                status = DocumentStatus.Voided;

                OnVoid(this);
            }
        }
        public virtual void Suspend()
        {

            TotalAmount = cr.Printer.PrinterSubTotal;
            cr.DataConnector.OnDocumentUpdated(this, (int)DocumentStatus.Suspended);

            EJException eje = null;
            try
            {
                cr.Printer.Suspend();
            }
            catch (EJFullException ejfe)
            {
                eje = ejfe;
            }
            status = DocumentStatus.Suspended;
            try
            {
                OnSuspend(this);
                VoidSuspended();
            }
            catch (Exception)
            {
                try
                {
                    Cancel();
                }
                catch { }
            }

            if (eje != null)
                throw eje;
            /*
        catch (DocumentSuspendException dse)
        {
            Cancel();
            throw dse;
        }*/
        }

        public virtual void Resume()
        {
            status = DocumentStatus.Cancelled;
        }

        public virtual void Close()
        {
            if (balanceDue > 0)
                throw new InvalidOperationException("Insufficient payment to close sale");

            try
            {
                if (!(this.fileOnDisk != null && this is Invoice))
                {
                    if (!isTryInvoiceAgain)
                    {
                        //Print Sales Person Info On Document
                        if ((cr.DataConnector.CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON) &&
                        (this.SalesPerson != null))
                        {
                            String cashierInfo = String.Format("{0} : {1} {2} ", PosMessage.CLERK, this.SalesPerson.Id, this.SalesPerson.Name.Trim());
                            cr.Printer.PrintRemark(cashierInfo);
                        }

                        if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.ShowFooterNote) == PosConfiguration.ON)
                        {
                            cr.Printer.PrintFooterNotes();
                        }

                        if (!CanPrintRemark)
                        {
                            this.footNotes = new List<string>();
                        }

                        cr.Printer.PrintFooter(this, true);
                    }
                    else
                        isTryInvoiceAgain = false;
                }
                BackgroundWorker.IsfterZreport = false;

                // If document is invoice, set printer to slip printer, print document and re-set printer
                if (this is Invoice || this is ReturnDocument)
                {
                    if((cr.DataConnector.CurrentSettings.GetProgramOption(Setting.PrintInvoicesInternal) != PosConfiguration.ON) && continueExternalPrinter)
                    {
                        cr.AdjustPrinter(this);
                  
                        DisplayAdapter.Cashier.Show(PosMessage.SLIP_PRINTING);

                        if (this.fileOnDisk != null)
                        {
                            string invID = this.fileOnDisk.Name.Substring(7, 4);
                            this.Id = int.Parse(invID);
                        }

                        cr.Printer.PrintFooter(this, false);

                        cr.Printer.ReleasePrinter();

                        if(this is Invoice)
                        {
                            cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.PRINT_AGAIN_OR_CONTINUE,
                                                                                    TryToPrintInvoice, ContinueForExternalPrinter));

                            isInvoicePrinted = false;

                            return;
                        }
                        else
                        {
                            isInvoicePrinted = true;
                        }

                            
                    }
                }                
            }
            catch (CmdSequenceException cse)
            {
                cr.Log.Warning(cse);
            }
            catch (Exception ex)
            {               
                if((this is Invoice || this is ReturnDocument) &&
                    (ex is NoSlipPrinterOnCOM || ex is NoSlipPortException || ex is PrinterOfflineException))
                {
                    if (!String.IsNullOrEmpty(PosConfiguration.Get("SlipComPort")))
                    {
                        // Alert Cashier
                        DisplayAdapter.Cashier.Show(PosMessage.NO_SLIP_PORT);
                        System.Threading.Thread.Sleep(2000);

                        cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.TRY_AGAIN_OR_HOLD_DOC,
                                                                            TryToPrintInvoice,
                                                                            SuspendInvoice));
                        isInvoicePrinted = false;
                        return;
                    }
                }
                else
                    throw ex;
            }

            if (!(this is Invoice) || isInvoicePrinted)
            {
                //Display customer change message again
                DisplayAdapter.Both.Show(customerChangeMsg);
            }

            status = DocumentStatus.Closed;

            if (Payments.Count == 0)
            {
                if (this is CarParkDocument)
                    OnClose(this);
                else
                    Void();

                cr.State = States.Start.Instance();
                return;
            }

            OnClose(this);

            if ((this is EInvoice || this is EArchive) &&
                    (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.PrintInvoicesInternal) == PosConfiguration.ON))
            {
                returnReason = "";

                List<string> eDocLines = EDocumentManager.GetEDocumentLines(currentLog.ToArray());
                foreach (string s in eDocLines)
                {
                    returnReason += s + "|";
                }

                cr.Printer.PrintFooter(this, false);
            }
            if (this is EInvoice || this is EArchive)
            {
                //EDocumentManager.SendEDocument(currentLog.ToArray());
            }

            if (ResumedFromDocumentId > 0 && FileOnDisk != null)
            {
                try
                {
                    DocumentFileHelper helper = new DocumentFileHelper(FileOnDisk);
                    helper.Id = Id;
                    helper.Remove(this);

                }
                catch (Exception) { }
            }
          
            cr.SecurityConnector.EscapeCustomer();
            States.Payment.ResetPayment();
        }

        public virtual void CloseWithoutPrint()
        {
            if(this != null && this.Id > 0)
            {
                status = DocumentStatus.Closed;

                OnClose(this);

                cr.SecurityConnector.EscapeCustomer();
                States.Payment.ResetPayment();
            }
        }

        public virtual IState SuspendInvoice()
        {
            //try
            //{
            //    TotalAmount = cr.Printer.PrinterSubTotal;
            //}
            //catch (PowerFailureException) { }
            //if (this.Items != null && this.items.Count > 0)
            //    cr.Printer.PrintFooter(this);
            //cr.Void();

            //cr.Document.Suspend();

            TotalAmount = cr.Printer.PrinterSubTotal;
            cr.DataConnector.OnDocumentUpdated(this, (int)DocumentStatus.Suspended);

            EJException eje = null;
            try
            {
                cr.Printer.Suspend();
            }
            catch (EJFullException ejfe)
            {
                eje = ejfe;
            }
            status = DocumentStatus.Suspended;
            try
            {
                OnSuspend(this);     
            }
            catch (Exception)
            {
                try
                {
                    Cancel();
                }
                catch { }
            }

            if (eje != null)
                throw eje;

            return Start.Instance();
        }

        private IState ContinueForExternalPrinter()
        {         
            continueExternalPrinter = false;
            isTryInvoiceAgain = true;
            this.Close();
            continueExternalPrinter = true;// Other invoices and return documents using external printer after the current invoice closed
            return Start.Instance();
        }


        private IState TryToPrintInvoice()
        {
            continueExternalPrinter = true;
            isTryInvoiceAgain = true;
            this.Close();
            return cr.State;
        }

        public virtual void CloseOrder()
        {
            try
            {
                //Print Sales Person Info On Document
                if ((cr.DataConnector.CurrentSettings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == PosConfiguration.ON) &&
                    (this.SalesPerson != null))
                {
                    String cashierInfo = String.Format("{0} : {1} {2} ", PosMessage.CLERK, this.SalesPerson.Id, this.SalesPerson.Name.Trim());
                    cr.Printer.PrintRemark(cashierInfo);
                }

                if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.ShowFooterNote) == PosConfiguration.ON)
                {
                    cr.Printer.PrintFooterNotes();
                }

                if (CanPrintRemark)
                {
                    foreach (String remark in cr.DataConnector.CurrentSettings.DocumentRemarks)
                        cr.Printer.PrintRemark(remark);
                }

                if (this.SalesPerson == null)
                {
                    this.salesPerson = cr.CurrentCashier;
                }

                // 675 gonderim problemi icin
                //SalesDocument sd = (SalesDocument)this.Clone();
                List<FiscalItem> removeList = new List<FiscalItem>();

                decimal tmpSubTotal = this.subTotal;
                decimal tmpBalanceDue = this.balanceDue;

                foreach (FiscalItem fi in this.items)
                {
                    if (fi is VoidItem)
                    {
                        removeList.Add(fi);
                        foreach (FiscalItem fi2 in this.items)
                        {
                            if (fi2 is SalesItem && fi.Product.Id == fi2.Product.Id)
                            {
                                fi2.TotalAmount -= fi2.VoidAmount;
                                fi2.Quantity -= fi2.VoidQuantity;

                                fi2.VoidAmount = 0;
                                fi2.VoidQuantity = 0;
                            }
                        }
                    }

                }
                
                foreach (FiscalItem fi in removeList)
                {
                    this.items.Remove(fi);
                }

                this.balanceDue = tmpBalanceDue;
                this.subTotal = tmpSubTotal;

                cr.DataConnector.OnDocumentUpdated(this, (int)DocumentStatus.Paying + 3);
                DisplayAdapter.Cashier.Show(PosMessage.MOVE_TO_EFT_POS_SIDE);
                IPrinterResponse response = cr.Printer.PrintFooter(this, true);

            }
            catch (CmdSequenceException cse)
            {
                cr.Log.Warning(cse);
            }

            if (status == DocumentStatus.Voided)
            {
                //OnVoid(this);
                return;
            }

            status = DocumentStatus.Closed;
            int zNo = cr.PrinterLastZ;

            //add zrp line if not added after previous z 
            //Removed because it is controlling after login process
            //cr.DataConnector.CheckZWritten(zNo, this.Id);

            OnClose(this);

            if (ResumedFromDocumentId > 0 && FileOnDisk != null)
            {
                try
                {
                    DocumentFileHelper helper = new DocumentFileHelper(FileOnDisk);
                    helper.Id = Id;
                    helper.Remove(this);

                }
                catch (Exception) { }
            }
            /*List<String> payments = new List<string>();
            foreach (PaymentInfo pi in this.Payments)
            {
                String msg = "";
                if (installment > 0)
                    msg = String.Format("{0}\t{1}",pi.Name + " x " + installment.ToString(), pi.Amount);
                else
                    msg = String.Format("{0}\t{1}", pi.Name, pi.Amount);

                payments.Add(msg);
            }*/

            customerChange = 0;
            cr.SecurityConnector.EscapeCustomer();
            States.Payment.ResetPayment();

        }

        internal IState PrintOrder()
        {
            IState state = null;
            try
            {
                if (cr.OrderPrinter != null)
                    cr.OrderPrinter.Print(this);

                state = States.Start.Instance();
            }
            catch
            {
                Confirm confirm = new Confirm("SÝPARÝÞ YAZILAMADI\nTEKRAR YAZDIR(GÝRÝÞ)",
                                                new StateInstance(PrintOrder));
                state = States.ConfirmCashier.Instance(confirm);
            }
            return state;
        }

        internal IState FailPoint()
        {
            this.FootNote.Add("PUAN KAYIT EDÝLEMEDÝ");
            Close();
            //in Close() DisplayAdapter.Both.Show(tmpStrPaymentInfo, tmpPaymentAmount, PosMessage.CHANGE, tmpCustomerChange,true);
            return States.Start.Instance(String.Empty);
        }

        internal IState ManagePoint()
        {
            try
            {
                long pointsToUpdate = 0;
                Decimal itemPromotedTotal = 0m;
                bool customerDefined = Customer != null && !Str.Contains(Customer.Code, "*");

                List<FiscalItem> adjustedItem = items.FindAll(delegate(FiscalItem fi)
                {
                    return (fi.Quantity > fi.VoidQuantity) &&
                            fi.Adjustments.Count > 0;
                });

                foreach (FiscalItem item in adjustedItem)
                {
                    foreach (Adjustment adj in item.Adjustments)
                    {
                        itemPromotedTotal += adj.NetAmount;
                        if (adj.AuthorizingCashierId == cr.PROMOTION_ITEM_CASHIER_ID)
                        {
                            adj.AuthorizingCashierId = cr.PROMOTION_CASHIER_ID;
                        }
                    }
                }

                //// promotedtotal already setted if customer defined
                //if (!customerDefined)
                //{
                //    foreach (Adjustment adj in this.adjustments)
                //    {
                //        promotedTotal += adj.NetAmount;
                //    }
                //}

                if (customerDefined)
                {
                    //if (points.Count > 0 || promotedTotal < 0m || itemPromotedTotal < 0m)
                    {
                        footNotes.Add(String.Format("{0} {1}", PosMessage.DEAR_CUSTOMER, cr.Document.Customer.Name));
                        if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.CustomerCodeOnRemark) == PosConfiguration.ON)
                            footNotes.Add(String.Format("{0} :{1}", PosMessage.CUSTOMER_CODE_SHORTENED, cr.Document.Customer.Code));                            
                    }
                }

                if (cr.DataConnector.CurrentSettings.GetProgramOption((Setting)8) == 1)
                {
                    String msg = "";
                    if (cr.IsDesktopWindows)
                    {
                        if (Math.Abs(itemPromotedTotal) > 0)
                        {
                            msg = itemPromotedTotal > 0 ? PosMessage.PRODUCT_FEE : PosMessage.PRODUCT_REDUCTION;
                            footNotes.Add(String.Format("{0,-15}:{1,9}",
                                                                msg,
                                                                String.Format("*{0:C}", new Number(Math.Abs(itemPromotedTotal)))));
                        }
                        if (Math.Abs(promotedTotal) > 0)
                        {
                            msg = promotedTotal > 0 ? PosMessage.SUBTOTAL_FEE : PosMessage.SUBTOTAL_REDUCTION;
                            footNotes.Add(String.Format("{0,-15}:{1,9}",
                                                               msg,
                                                               String.Format("*{0:C}", new Number(Math.Abs(promotedTotal)))));
                        }
                    }

                    if (Math.Abs(promotedTotal + itemPromotedTotal) > 0)
                    {
                        msg = (promotedTotal + itemPromotedTotal) > 0 ? PosMessage.TOTAL_FEE : PosMessage.TOTAL_REDUCTION;
                        footNotes.Add(String.Format("{0,-15}:{1,9}",
                                                               msg,
                                                               String.Format("*{0:C}", new Number(Math.Abs(promotedTotal + itemPromotedTotal)))));
                    }
                }
                if (customerDefined && points.Count > 0)
                {
                    long earnedPoint = 0;
                    long usedPoint = 0;
                    foreach (PointObject po in points)
                    {
                        try
                        {
                            UpdateCustomerPoint(po);
                        }
                        catch (UpdatePointException upe)
                        {
                            cr.Log.Error("Puan Kayýt Hatasý. ");
                            cr.Log.Error(upe.Message);
                        }
                        if (po.Value > 0)
                            earnedPoint += po.Value;
                        else
                            usedPoint += Math.Abs(po.Value);
                    }

                    String usedMessage = "";

                    if (earnedPoint > 0)
                        footNotes.Add(String.Format("{0,-15}:{1,9}", PosMessage.EARNED_POINT, "*" + earnedPoint));

                    if (usedPoint > 0)
                    {
                        usedMessage = this is ReturnDocument ? PosMessage.RETURNED_POINT : PosMessage.USED_POINT;
                        footNotes.Add(String.Format("{0,-15}:{1,9}", usedMessage, "*" + usedPoint));
                    }

                    footNotes.Add(String.Format("{0,-15}:{1,9}", PosMessage.TOTAL_POINT, "*" + (Customer.Points + pointsToUpdate)));

                    String format = "{0}\t{1}\n{2}\t{3}";
                    String definition = earnedPoint - usedPoint > 0 ? PosMessage.EARNED_POINT : usedMessage;
                    Decimal value = earnedPoint - usedPoint > 0 ? earnedPoint - usedPoint : usedPoint;

                    DisplayAdapter.Cashier.Show(format, definition, value, PosMessage.TOTAL_POINT, Customer.Points);
                    System.Threading.Thread.Sleep(1000);
                }
                string orderMsg = string.Empty;
#if ORDER
                CloseOrder();
#else
                Close();
#endif
                //in Close() DisplayAdapter.Both.Show(tmpStrPaymentInfo, tmpPaymentAmount, PosMessage.CHANGE, tmpCustomerChange,true);
                if (cr.OrderPrinter != null)
                    return PrintOrder();
                else if (!isInvoicePrinted)
                {
                    isInvoicePrinted = true;
                    return cr.State;
                }
                else
                    return States.Start.Instance(String.Empty);
            }
            catch (ContactlessCardException cce)
            {

                Confirm confirm = new Confirm(cce.Message,
                    new StateInstance(ManagePoint),
                    new StateInstance(FailPoint));
                return States.ConfirmCashier.Instance(confirm);
            }
        }

        private void UpdateCustomerPoint(PointObject po)
        {
            po.Customer = this.Customer;
            po.DocumentDate = this.CreatedDate;
            po.DocumentNo = this.Id;
            po.DocumentTypeID = this.DocumentTypeId;
            po.DocumentTotal = this.TotalAmount;
            po.RegisterFiscalID = cr.FiscalRegisterNo;
            po.RegisterID = cr.Id;
            po.OfficeID = PosConfiguration.Get("OfficeNo");
            po.ZNo = cr.PrinterLastZ + 1;

            Customer.UpdatePoint(po);
        }

        private bool CanPrintRemark
        {
            get
            {
                DocumentRemarkType condition = DocumentRemarkType.NoRemark;
                switch (DocumentTypeId)
                {
                    case -1:        //RECEIPT:
                        condition = DocumentRemarkType.Receipt;
                        break;
                    case 0:         //INVOICE:
                        condition = DocumentRemarkType.Invoice;
                        break;
                    case 1:         //RETURN_DOCUMENT:
                        condition = DocumentRemarkType.ReturnDocument;
                        break;
                    case 3:         //WAYBILL:
                        condition = DocumentRemarkType.Waybill;
                        break;
                }
                return (((DocumentRemarkType)cr.DataConnector.CurrentSettings.GetProgramOption(Setting.PrintDocumentRemark) & condition) == condition);
            }
        }
        internal void ShowSubTotal()
        {
            state.ShowSubTotal(this);
        }

        public virtual void Show()
        {
            DisplayAdapter.Cashier.Show(this);
        }

        public void Show(Target target)
        {
            Show();
        }

        internal void UndoAdjustment(bool isSubTotalAdj)
        {
            if (adjustments == null || adjustments.Count == 0) return;
            Adjustment lastAdjustment = adjustments[adjustments.Count - 1];
            cr.Printer.Correct(lastAdjustment);
            TotalAmount -= lastAdjustment.NetAmount;
            adjustments.Remove(lastAdjustment);
            //String label = lastAdjustment.NetAmount > 0 ? "   ARTIRIM iPTAL   " : "   iNDiRiM iPTAL    ";
            //int gap = (20 - label.Length) / 2;
            //label = String.Format("{0," + (20 - gap) + "}\n{1:N}", label, lastAdjustment.NetAmount);
            DisplayAdapter.Both.ShowCorrect(lastAdjustment, isSubTotalAdj);
            OnUndoAdjustment(this, null);
            state = DocumentOpen.Instance();
        }

        public void Append(PromotionDocument doc)
        {
            try
            {
                points.AddRange(doc.Points);
                footNotes = doc.FootNote;
                promoLogLines = doc.LogLines;

                state = DocumentPaying.Instance();

                Dictionary<int, Decimal> tgNetAdjustments = new Dictionary<int, decimal>();

                foreach (Adjustment a in doc.ItemAdjustments)
                {
                    if (a.Target is SalesItem)
                    {
                        SalesItem item = a.Target as SalesItem;
                        if (tgNetAdjustments.ContainsKey(item.Product.Department.TaxGroupId))
                            tgNetAdjustments[item.Product.Department.TaxGroupId] += a.NetAmount;
                        else tgNetAdjustments.Add(item.Product.Department.TaxGroupId, a.NetAmount);
                    }
                }


                List<Adjustment> promotionAdjustments = new List<Adjustment>();
                promotionAdjustments.AddRange(doc.ItemAdjustments);
                promotionAdjustments.AddRange(doc.SubtotalAdjustments);
                if (promotionAdjustments.Count > 0)
                {
                    cr.Printer.PrintSubTotal(cr.Document, true);
                    foreach (String s in doc.Remark)
                        cr.Printer.PrintRemark(s);

                    cr.Printer.Print(promotionAdjustments.ToArray());
                }

                foreach (Adjustment promoAdjustment in doc.ItemAdjustments)
                {
                    promoAdjustment.Target.Adjust(promoAdjustment);
                }
                foreach (Adjustment promoAdjustment in doc.SubtotalAdjustments)
                {
                    Adjustment adjustment = new Adjustment(this, promoAdjustment.Method,
                                                                 promoAdjustment.RequestValue,
                                                                 promoAdjustment.AuthorizingCashierId);

                    Adjust(adjustment);
                    promotedTotal += adjustment.NetAmount;
                }

            }
            catch (SlipRowCountExceedException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                cr.Log.Error(String.Format("Promosyon indirim hatasi:{0}", ex));
            }
        }

        public List<PointObject> Points
        {
            get { return points; }
        }

        public void AddPoint(long point)
        {
            PointObject po = new PointObject();

            po.Value = point;
            po.Description = point > 0 ? PosMessage.EARNED_POINT : this is ReturnDocument ? PosMessage.RETURNED_POINT : PosMessage.USED_POINT;

            points.Add(po);
        }

        public Decimal PointPrices(long point)
        {
            double aPoint = (double)cr.DataConnector.CurrentSettings.GetProgramOption(Setting.PointPrice);
            if ((aPoint == 0) || (((int)aPoint % 1000) == 0)) return 0;
            aPoint = ((double)((int)aPoint % 1000)) / (((int)aPoint / 1000) % 100);
            decimal diff = (decimal)((double)point * aPoint / 1000) % 0.01m;
            if (((decimal)((double)point * aPoint / 1000) - diff) == 0m)
                diff = 0m;
            return (decimal)((double)point * aPoint / 1000) - diff;
        }

        public long PriceToPoint(Decimal price)
        {
            bool isNegative = false;
            if (price < 0)
            {
                isNegative = true;
                price = Math.Abs(price);
            }

            double point = (double)(price / this.PointPrices(1));

            if (point > Math.Round(point))
                point = Math.Round(point) + 1;
            else
                point = Math.Round(point);

            return ((long)(isNegative ? -point : point));
        }

        //this function only printing document because of receipt to slip
        public IState Print()
        {
            List<FiscalItem> itemList = null;
            try
            {
                this.TotalAmount = 0;

                // Adjust Printer
                //cr.Printer.AdjustPrinter(this);
                cr.DataConnector.OnDocumentUpdated((SalesDocument)this, (int)this.Status);

                if (this is Receipt)
                {
                    // Print Header
                    ExecuteOrderCommand(OrderCommand.PrintHeader, this);
                }

                //Print items
                itemList = ((SalesDocument)this.Clone()).items;

                //For update "HRBELGE" file.
                SalesDocument virtualDoc = null;
                if (this is Receipt)
                    virtualDoc = new Receipt();
                else
                    virtualDoc = new Invoice();

                virtualDoc.Id = this.Id;

                // To show referenced item 
                int itemIndexToShow = 0;

                while (itemList.Count > 0)
                {
                    FiscalItem fi = itemList[0];
                    if (fi is SalesItem)
                    {
                        SalesItem si = (SalesItem)fi.Clone();
                        if (si.Quantity <= si.VoidQuantity)
                        {
                            itemList.Remove(fi);
                            itemIndexToShow++;
                            continue;
                        }

                        si.TotalAmount -= si.VoidAmount;
                        if (si.TotalAmount > 0)
                        {
                            System.Threading.Thread.Sleep(100);
                            ExecuteOrderCommand(OrderCommand.SaleItem, si);
                            //DisplayAdapter.Cashier.ShowSale(si);
                            //DisplayAdapter.Cashier.ShowSale(this.Items[itemIndexToShow]);
                            this.Items[itemIndexToShow].Show();
                        }
                        virtualDoc.AddItem(si, true);
                        this.TotalAmount += si.TotalAmount;
                        si.VoidQuantity = 0;
                        si.VoidAmount = 0;

                        decimal currentSubtotal = (Decimal)ExecuteOrderCommand(OrderCommand.SubTotal);
                        currentSubtotal = virtualDoc.TotalAmount;
                        if (currentSubtotal == virtualDoc.TotalAmount)
                        {
                            itemList.Remove(fi);
                            itemIndexToShow++;
                        }
                        else
                            this.TotalAmount -= si.TotalAmount;
                    }
                    else
                    {
                        itemList.Remove(fi);
                        itemIndexToShow++;
                    }
                }

                //Print document adjustments
                foreach (Adjustment adj in this.Adjustments)
                {
                    System.Threading.Thread.Sleep(100);
                    ExecuteOrderCommand(OrderCommand.Adjustment, adj);
                    TotalAmount += adj.NetAmount;
                    state = States.DocumentPaying.Instance();
                }

                //Print document remark lines
                String[] tempRemarks = new String[this.remark.Count];
                this.Remark.CopyTo(tempRemarks);
                foreach (String remark in tempRemarks)
                {
                    System.Threading.Thread.Sleep(100);
                    ExecuteOrderCommand(OrderCommand.Remark, remark);
                    this.Remark.Remove(remark);
                }
                //Print payments
                PaymentInfo[] tempPayments = new PaymentInfo[this.Payments.Count];
                this.Payments.CopyTo(tempPayments);
                this.Payments.Clear();

                foreach (PaymentInfo pi in tempPayments)
                {
                    System.Threading.Thread.Sleep(100);
                    ExecuteOrderCommand(OrderCommand.Payment, pi);
                }

                return cr.State = States.Start.Instance();
            }
            catch (PrinterException pe)
            {
                System.Threading.Thread.Sleep(250);
                try
                {
                    cr.Log.Error("Sipariþ Yazdýrma Hatasý: {0}", pe.Message);
                    cr.Document.Void();
                }
                catch { }
                throw pe; // return cr.State = States.ConfirmCashier.Instance(new Confirm("BELGEYÝ TEKRAR\nYAZDIR?", this.Print));
            }
            finally
            {
                if (itemList != null)
                {
                    try
                    {
                        if (balanceDue == 0)
                            DisplayAdapter.Cashier.ChangeDocumentStatus(this, DisplayDocumentStatus.OnClose);
                    }
                    catch { }
                }
            }
        }

        private object ExecuteOrderCommand(OrderCommand orderCommand, params object[] args)
        {
            bool response = true;

            while (true)
            {
                try
                {
                    switch (orderCommand)
                    {
                        case OrderCommand.PrintHeader:
                            cr.Printer.PrintHeader((ISalesDocument)args[0]);
                            return response;
                        case OrderCommand.SaleItem:
                            cr.Printer.Print((SalesItem)args[0]);
                            return response;
                        case OrderCommand.Payment:
                            cr.State = cr.Document.Pay((PaymentInfo)args[0]);
                            return response;
                        case OrderCommand.SubTotal:
                            return cr.Printer.PrinterSubTotal;
                        case OrderCommand.Adjustment:
                            cr.Printer.Print((IAdjustment)args[0]);
                            return response;
                        case OrderCommand.Remark:
                            cr.Printer.PrintRemark((String)args[0]);
                            return response;

                    }
                }
                catch (PrinterException pe)
                {
                    if ((pe is NoReceiptRollException) ||
                        (pe is OpenShutterException) ||
                        (pe is FramingException) ||
                        (pe is PrinterTimeoutException))
                    {
                        ManagePrinterException(pe);
                    }
                }
                catch (PosException pe)
                {
                    if ((pe is UnfixedSlipException) ||
                        (pe is SlipRowCountExceedException))
                    {
                        ManagePrinterException(pe);
                    }
                    else
                    {
                        throw pe;
                    }
                }
            }

        }

        private void ManagePrinterException(PosException pe)
        {
            int sleepTime = 2000;
            if (!Parser.TryInt(PosConfiguration.Get("SleepTime"), out sleepTime))
                sleepTime = 2000;

            DisplayAdapter.Cashier.Show(new Error(pe));
            while (true)
            {
                try
                {
                    cr.Printer.CheckPrinterStatus();
                    break;
                }
                catch
                {
                    System.Threading.Thread.Sleep(sleepTime);
                }
            }

            DisplayAdapter.Cashier.ClearError();
        }

        internal void Transfer()
        {
            status = DocumentStatus.Transferred;
        }
        public DateTime RegisterDate
        {
            get { return registerDate; }
            set { registerDate = value; }
        }
        public String RegisterAddress
        {
            get { return registerAddress; }
            set { registerAddress = value; }
        }

        public SalesDocument ReadMainLogFile(int docId, int docZNo)
        {

            int lastZNo = cr.PrinterLastZ + 1;
            String mainLogFileUrl = "";

            if (cr.IsDesktopWindows)
                mainLogFileUrl = String.Format("{0}HR{1:ddMMyy}.{2}", PosConfiguration.ArchivePath, DateTime.Now, cr.Id);
            else
                mainLogFileUrl = PosConfiguration.ArchivePath + "HAREKET." + cr.Id;

            String line = "";
            fs = new FileStream(mainLogFileUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(0, SeekOrigin.End);
            bool isCorrectDoc = false;
            while (fs.Position != 0 && !isCorrectDoc)
            {
                line = BackwardReadline();
                if (Str.Contains(line, "ZRP"))
                {
                    if (lastZNo == docZNo)
                        isCorrectDoc = true;
                    else
                        lastZNo--;

                }

            }
            long startPosition = fs.Position + 42;

            fs.Close();

            return ParseDocument(mainLogFileUrl, docId, startPosition);
        }
        /*
         * TODO: function assumes HIddMMyy file will be searched for
         * voided documents
         * for windowsce, function must be corrected
         */
        public SalesDocument GetDocumentFromLastZ(int docId)
        {
            String searchFile = "";

            if (cr.IsDesktopWindows)
                searchFile = String.Format("{0}HR{1:ddMMyy}.{2}", PosConfiguration.ArchivePath, DateTime.Now, cr.Id);
            else
                searchFile = PosConfiguration.ArchivePath + "HAREKET." + cr.Id;

            long startPosition = SearchDocument(searchFile, docId);

            if (startPosition == -1)
            {
                try
                {
                    if (cr.IsDesktopWindows)
                        searchFile = String.Format("{0}HI{1:ddMMyy}.{2}", PosConfiguration.ArchivePath, DateTime.Now, cr.Id);
                    else
                        searchFile = PosConfiguration.ArchivePath + "HRIPTAL." + cr.Id;

                    startPosition = SearchDocument(searchFile, docId);
                }
                catch
                {
                    return null;
                }
            }

            if (startPosition > -1)
            {
                return ParseDocument(searchFile, docId, startPosition);
            }
            else
            {
                return null;
            }
        }

        public SalesDocument GetDocumentByBarcode(string barcode)
        {
            String searchFile = "";

            if (cr.IsDesktopWindows)
                searchFile = String.Format("{0}HR{1:ddMMyy}.{2}", PosConfiguration.ArchivePath, DateTime.Now, cr.Id);
            else
                searchFile = PosConfiguration.ArchivePath + "HAREKET." + cr.Id;

            long startPosition = SearchOnDocument(searchFile, barcode);

            if (startPosition == -1)
            {
                try
                {
                    if (cr.IsDesktopWindows)
                        searchFile = String.Format("{0}HI{1:ddMMyy}.{2}", PosConfiguration.ArchivePath, DateTime.Now, cr.Id);
                    else
                        searchFile = PosConfiguration.ArchivePath + "HRIPTAL." + cr.Id;

                    startPosition = SearchOnDocument(searchFile, barcode);
                }
                catch
                {
                    return null;
                }
            }

            if (startPosition > -1)
            {
                return ParseDocument(searchFile, startPosition);
            }
            else
            {
                return null;
            }
        }

        public long SearchOnDocument(String path, string barcode)
        {
            String line = "";
            string bcode = "";
            String[] parseLine;

            fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(0, SeekOrigin.End);

            /*
             * searchState 
             *             0 if search is not completed
             *             1 if search is completed, and document has been found
             *            -1 if search is completed, and document could not be found
             */
            int searchState = 0;
            long startPosition = 0;
            while (fs.Position != 0)
            {
                line = BackwardReadline();
                switch (line.Substring(8, 2))
                {
                    case "01":
                    case "02":
                        startPosition = fs.Position;
                        break;
                    case "24"://BID
                        parseLine = line.Split(',');
                        try
                        {
                            string fiscal = parseLine[4].Trim();
                            string zNoDocId = parseLine[5].Trim();

                            bcode = fiscal.Substring(2) + zNoDocId;
 
                            if (bcode == barcode)
                            {
                                searchState = 1;
                            }
                        }
                        catch
                        {

                        }
                        break;
                    case "16":
                        searchState = -1;
                        break;
                }
                if (searchState != 0)
                {
                    break;
                }
            }
            fs.Close();

            if (searchState == 1)
            {
                return startPosition;
            }
            else
            {
                return -1;
            }
        }
        public long SearchDocument(String path, int docId)
        {
            String line = "";
            int documentId = 0;
            String[] parseLine;

            fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(0, SeekOrigin.End);

            /*
             * searchState 
             *             0 if search is not completed
             *             1 if search is completed, and document has been found
             *            -1 if search is completed, and document could not be found
             */
            int searchState = 0;

            while (fs.Position != 0)
            {
                line = BackwardReadline();
                switch (line.Substring(8, 2))
                {
                    case "01"://FIS
                    case "02"://FAT,IAD,IRS
                    case "24"://GPS
                        parseLine = line.Split(',');
                        try
                        {
                            documentId = int.Parse(parseLine[5].Substring(0, 6).Trim());
                            if (docId == documentId)
                            {
                                searchState = 1;
                            }
                            else if (docId > documentId)
                            {
                                searchState = -1;
                            }
                        }
                        catch
                        {

                        }
                        break;
                    case "16":
                        searchState = -1;
                        break;
                }
                if (searchState != 0)
                {
                    break;
                }
            }
            long startPosition = fs.Position;
            fs.Close();

            if (searchState == 1)
            {
                return startPosition;
            }
            else
            {
                return -1;
            }
        }
        public SalesDocument ParseDocument(String docPath, int docId, long startPosition)
        {
            SalesDocument doc = null;
            int documentId = 0, lastDepartmentId = 0;
            int indexPrm2 = 28;
            String[] parseLine;
            Adjustment adj = null;
            FiscalItem item = null;
            Decimal totalAmount = 0m;

            String line = "";
            StreamReader sr = new StreamReader(docPath, System.Text.Encoding.GetEncoding(1254));
            sr.BaseStream.Position = startPosition;

            line = sr.ReadLine();

            int lastCancel = 0;

            try
            {
                do
                {
                    parseLine = line.Split(',');
                    switch (line.Substring(8, 2))
                    {
                        case "01"://FIS
                        case "02"://FAT,IAD,IRS
                        case "24"://GPS
                            parseLine = line.Split(',');
                            try
                            {
                                documentId = int.Parse(parseLine[5].Substring(0, 6).Trim());
                            }
                            catch
                            {
                                break;
                            }
                            if (docId != documentId) break;

                            switch (line.Substring(11, 3))
                            {
                                case PosMessage.HR_CODE_RECEIPT:
                                    doc = new Receipt();
                                    break;
                                case PosMessage.HR_CODE_INVOICE:
                                    //doc = new Invoice();
                                    return null;
                                case PosMessage.HR_CODE_E_INVOICE:
                                    doc = new EInvoice();
                                    break;
                                case PosMessage.HR_CODE_E_ARCHIVE:
                                    doc = new EArchive();
                                    break;
                                case PosMessage.HR_CODE_MEAL_TICKET:
                                    doc = new MealTicket();
                                    break;
                                case PosMessage.HR_CODE_ADVANCE:
                                    //doc = new Advance();
                                    return null;
                                case PosMessage.HR_CODE_CAR_PARKING:
                                    //doc = new CarParkDocument();
                                    return null;
                                case PosMessage.HR_CODE_COLLECTION_INVOICE:
                                    //doc = new CollectionInvoice();
                                    return null;
                            }

                            doc.Id = (int)documentId;
                            doc.salesPerson = cr.DataConnector.FindCashierById(line.Substring(23, 4));
                            doc.repeatedDocument = true;
                            break;

                        case "04"://SAT
                        case "25"://GAL
                            if (doc == null) break;
                            item = new SalesItem();
                            item.Quantity = Decimal.Parse(parseLine[4].Substring(0, 6)) / 1000;
                            item.Product = cr.DataConnector.FindProductByLabel(parseLine[4].Substring(6, 6));
                            item.TotalAmount = Decimal.Parse(parseLine[5].Substring(2)) / 100;
                            item.UnitPrice = Math.Round(item.TotalAmount / item.Quantity, 2);
                            totalAmount = Decimal.Round(item.TotalAmount + totalAmount, 2);
                            lastDepartmentId = int.Parse(line.Substring(indexPrm2, 2)) - 1;
                            doc.AddItem(item, false);
                            break;

                        case "05"://IPT
                            if (doc == null) break;
                            item = new SalesItem();
                            item.Quantity = Decimal.Parse(parseLine[4].Substring(0, 6)) / 1000;
                            item.Product = cr.DataConnector.FindProductByLabel(parseLine[4].Substring(6, 6));
                            item.TotalAmount = Decimal.Parse(parseLine[5].Substring(2)) / 100;
                            totalAmount = Decimal.Round(totalAmount - item.TotalAmount, 2);
                            FiscalItem vi = new VoidItem(item);
                            vi.UnitPrice = Math.Round(vi.TotalAmount / vi.Quantity, 2);
                            doc.AddItem(vi, false);
                            decimal vq = Math.Abs(vi.Quantity);
                            decimal va = Math.Abs(vi.TotalAmount);


                            for (int i = lastCancel; i < doc.Items.Count; i++)
                            {
                                if (!(doc.Items[i] is SalesItem)) continue;

                                if (doc.Items[i].Product.Id == vi.Product.Id)
                                {
                                    decimal adjustmentTotal = 0m;
                                    foreach (IAdjustment itemAdjustment in doc.Items[i].Adjustments)
                                        adjustmentTotal += itemAdjustment.NetAmount * decimal.MinusOne;

                                    if (Math.Round(doc.Items[i].UnitPrice * doc.Items[i].Quantity + adjustmentTotal, 2) == Math.Round(vi.UnitPrice * doc.Items[i].Quantity, 2))
                                    {
                                        if (vq > doc.Items[i].Quantity)
                                        {
                                            ((SalesItem)doc.Items[i]).VoidQuantity = doc.Items[i].Quantity;
                                            ((SalesItem)doc.Items[i]).VoidAmount = doc.Items[i].TotalAmount;
                                            vq -= doc.Items[i].Quantity;
                                            va -= doc.Items[i].TotalAmount;
                                        }
                                        else
                                        {
                                            if (((SalesItem)doc.Items[i]).VoidQuantity != 0)
                                                continue;
                                            ((SalesItem)doc.Items[i]).VoidQuantity = vq;
                                            ((SalesItem)doc.Items[i]).VoidAmount = va;
                                            lastCancel++;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "06"://IND,ART
                        case "39"://ART (inter pattern)
                            if (doc == null) break;
                            AdjustmentType adjType;
                            decimal discountAmount;
                            if (parseLine[4].Substring(10) == "--")
                            {
                                discountAmount = Decimal.Parse(parseLine[5]) / 100m;
                                if (parseLine[3] == "IND")
                                    adjType = AdjustmentType.Discount;
                                else
                                    adjType = AdjustmentType.Fee;
                            }
                            else
                            {
                                discountAmount = Decimal.Parse(parseLine[4].Substring(10));
                                if (parseLine[3] == "IND")
                                    adjType = AdjustmentType.PercentDiscount;
                                else
                                    adjType = AdjustmentType.PercentFee;

                            }
                            if (parseLine[4].Substring(0, 3) == "SNS")
                            {
                                adj = new Adjustment(item, adjType, discountAmount);
                                //item.Adjust(adj); 
                                doc.LastItem.Adjust(adj);
                                totalAmount = Decimal.Round(adj.NetAmount + totalAmount, 2);
                                // <belge tekrarinda tutar farki fix>
                                // doc.LastItem.UnitPrice = Decimal.Round((doc.LastItem.TotalAmount - adj.NetAmount) / doc.LastItem.Quantity, 2); 

                                break;
                            }
                            break;

                        case "60":
                        case "61":
                            doc.tcknVkn = parseLine[4].Trim();
                            break;
                            
                        case "62":
                            string[] date = parseLine[4].Split('/');
                            doc.issueDate = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));

                            if (parseLine.Length > 5)
                            {
                                if (!String.IsNullOrEmpty(parseLine[5]))
                                {
                                    string[] time = parseLine[5].Split(':');
                                    doc.issueDate = new DateTime(doc.issueDate.Year, doc.issueDate.Month, doc.issueDate.Day, int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
                                }
                            }
                            break;

                        case "63":
                            doc.customerTitle = parseLine[4];
                            break;

                        case "64":
                            doc.customerTitle = parseLine[4];
                            break;

                        case "65":
                            doc.returnReason = parseLine[4];
                            break;

                        case "66":
                            decimal comission = Decimal.Parse(parseLine[5]);
                            doc.comissionAmount = comission;
                            break;

                        case "11"://SON
                            if (doc == null) break;
                            doc.TotalAmount = totalAmount;
                            sr.Close();
                            return doc;
                    }

                    line = sr.ReadLine();
                } while (line != null);

            }
            catch (Exception)
            {
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }

            return doc;
        }

        public SalesDocument ParseDocument(String docPath, long startPosition)
        {
            SalesDocument doc = null;
            int documentId = 0, lastDepartmentId = 0;
            int indexPrm2 = 28;
            String[] parseLine;
            Adjustment adj = null;
            FiscalItem item = null;
            Decimal totalAmount = 0m;

            String line = "";
            StreamReader sr = new StreamReader(docPath, System.Text.Encoding.GetEncoding(1254));
            sr.BaseStream.Position = startPosition;

            line = sr.ReadLine();

            int lastCancel = 0;

            try
            {
                do
                {
                    parseLine = line.Split(',');
                    switch (line.Substring(8, 2))
                    {
                        case "01"://FIS
                        case "02"://FAT,IAD,IRS
                        case "24"://GPS
                            switch (line.Substring(11, 3))
                            {
                                case PosMessage.HR_CODE_RECEIPT:
                                    doc = new Receipt();
                                    break;
                                case PosMessage.HR_CODE_INVOICE:
                                    //doc = new Invoice();
                                    return null;
                                case PosMessage.HR_CODE_E_INVOICE:
                                    doc = new EInvoice();
                                    break;
                                case PosMessage.HR_CODE_E_ARCHIVE:
                                    doc = new EArchive();
                                    break;
                                case PosMessage.HR_CODE_MEAL_TICKET:
                                    doc = new MealTicket();
                                    break;
                                case PosMessage.HR_CODE_ADVANCE:
                                    //doc = new Advance();
                                    return null;
                                case PosMessage.HR_CODE_CAR_PARKING:
                                    //doc = new CarParkDocument();
                                    return null;
                                case PosMessage.HR_CODE_COLLECTION_INVOICE:
                                    //doc = new CollectionInvoice();
                                    return null;
                            }

                            doc.Id = (int)documentId;
                            doc.salesPerson = cr.DataConnector.FindCashierById(line.Substring(23, 4));
                            doc.repeatedDocument = true;
                            break;

                        case "04"://SAT
                        case "25"://GAL
                            if (doc == null) break;
                            item = new SalesItem();
                            item.Quantity = Decimal.Parse(parseLine[4].Substring(0, 6)) / 1000;
                            item.Product = cr.DataConnector.FindProductByLabel(parseLine[4].Substring(6, 6));
                            item.TotalAmount = Decimal.Parse(parseLine[5].Substring(2)) / 100;
                            item.UnitPrice = Math.Round(item.TotalAmount / item.Quantity, 2);
                            totalAmount = Decimal.Round(item.TotalAmount + totalAmount, 2);
                            lastDepartmentId = int.Parse(line.Substring(indexPrm2, 2)) - 1;
                            doc.AddItem(item, false);
                            break;

                        case "05"://IPT
                            if (doc == null) break;
                            item = new SalesItem();
                            item.Quantity = Decimal.Parse(parseLine[4].Substring(0, 6)) / 1000;
                            item.Product = cr.DataConnector.FindProductByLabel(parseLine[4].Substring(6, 6));
                            item.TotalAmount = Decimal.Parse(parseLine[5].Substring(2)) / 100;
                            totalAmount = Decimal.Round(totalAmount - item.TotalAmount, 2);
                            FiscalItem vi = new VoidItem(item);
                            vi.UnitPrice = Math.Round(vi.TotalAmount / vi.Quantity, 2);
                            doc.AddItem(vi, false);
                            decimal vq = Math.Abs(vi.Quantity);
                            decimal va = Math.Abs(vi.TotalAmount);


                            for (int i = lastCancel; i < doc.Items.Count; i++)
                            {
                                if (!(doc.Items[i] is SalesItem)) continue;

                                if (doc.Items[i].Product.Id == vi.Product.Id)
                                {
                                    decimal adjustmentTotal = 0m;
                                    foreach (IAdjustment itemAdjustment in doc.Items[i].Adjustments)
                                        adjustmentTotal += itemAdjustment.NetAmount * decimal.MinusOne;

                                    if (Math.Round(doc.Items[i].UnitPrice * doc.Items[i].Quantity + adjustmentTotal, 2) == Math.Round(vi.UnitPrice * doc.Items[i].Quantity, 2))
                                    {
                                        if (vq > doc.Items[i].Quantity)
                                        {
                                            ((SalesItem)doc.Items[i]).VoidQuantity = doc.Items[i].Quantity;
                                            ((SalesItem)doc.Items[i]).VoidAmount = doc.Items[i].TotalAmount;
                                            vq -= doc.Items[i].Quantity;
                                            va -= doc.Items[i].TotalAmount;
                                        }
                                        else
                                        {
                                            if (((SalesItem)doc.Items[i]).VoidQuantity != 0)
                                                continue;
                                            ((SalesItem)doc.Items[i]).VoidQuantity = vq;
                                            ((SalesItem)doc.Items[i]).VoidAmount = va;
                                            lastCancel++;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;

                        case "06"://IND,ART
                        case "39"://ART (inter pattern)
                            if (doc == null) break;
                            AdjustmentType adjType;
                            decimal discountAmount;
                            if (parseLine[4].Substring(10) == "--")
                            {
                                discountAmount = Decimal.Parse(parseLine[5]) / 100m;
                                if (parseLine[3] == "IND")
                                    adjType = AdjustmentType.Discount;
                                else
                                    adjType = AdjustmentType.Fee;
                            }
                            else
                            {
                                discountAmount = Decimal.Parse(parseLine[4].Substring(10));
                                if (parseLine[3] == "IND")
                                    adjType = AdjustmentType.PercentDiscount;
                                else
                                    adjType = AdjustmentType.PercentFee;

                            }
                            if (parseLine[4].Substring(0, 3) == "SNS")
                            {
                                adj = new Adjustment(item, adjType, discountAmount);
                                //item.Adjust(adj); 
                                doc.LastItem.Adjust(adj);
                                totalAmount = Decimal.Round(adj.NetAmount + totalAmount, 2);
                                // <belge tekrarinda tutar farki fix>
                                // doc.LastItem.UnitPrice = Decimal.Round((doc.LastItem.TotalAmount - adj.NetAmount) / doc.LastItem.Quantity, 2); 

                                break;
                            }
                            break;

                        case "60":
                        case "61":
                            doc.tcknVkn = parseLine[4].Trim();
                            break;

                        case "62":
                            string[] date = parseLine[4].Split('/');
                            doc.issueDate = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]));

                            if (parseLine.Length > 5)
                            {
                                if (!String.IsNullOrEmpty(parseLine[5]))
                                {
                                    string[] time = parseLine[5].Split(':');
                                    doc.issueDate = new DateTime(doc.issueDate.Year, doc.issueDate.Month, doc.issueDate.Day, int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
                                }
                            }
                            break;

                        case "63":
                            doc.customerTitle = parseLine[4];
                            break;

                        case "64":
                            doc.customerTitle = parseLine[4];
                            break;

                        case "65":
                            doc.returnReason = parseLine[4];
                            break;

                        case "66":
                            decimal comission = Decimal.Parse(parseLine[5]);
                            doc.comissionAmount = comission;
                            break;

                        case "11"://SON
                            if (doc == null) break;
                            doc.TotalAmount = totalAmount;
                            sr.Close();
                            return doc;
                    }

                    line = sr.ReadLine();
                } while (line != null);

            }
            catch (Exception)
            {
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }

            return doc;
        }

        private FileStream fs = null;

        private string BackwardReadline()
        {

            byte[] line;
            byte[] text = new byte[1];
            long position = 0;
            int count;
            fs.Seek(0, SeekOrigin.Current);
            position = fs.Position;

            //do we have trailing \r\n?

            if (fs.Length > 1)
            {

                byte[] vagnretur = new byte[2];
                fs.Seek(-2, SeekOrigin.Current);
                fs.Read(vagnretur, 0, 2);
                if (System.Text.ASCIIEncoding.ASCII.GetString(vagnretur, 0, vagnretur.Length).Equals("\r\n"))
                {
                    //move it back

                    fs.Seek(-2, SeekOrigin.Current);
                    position = fs.Position;

                }

            }

            while (fs.Position > 0)
            {

                text.Initialize();
                //read one char
                fs.Read(text, 0, 1);
                string asciiText = System.Text.ASCIIEncoding.ASCII.GetString(text, 0, text.Length);
                //moveback to the charachter before
                fs.Seek(-2, SeekOrigin.Current);

                if (asciiText.Equals("\n"))
                {

                    fs.Read(text, 0, 1);

                    asciiText = System.Text.ASCIIEncoding.ASCII.GetString(text, 0, text.Length);

                    if (asciiText.Equals("\r"))
                    {

                        fs.Seek(1, SeekOrigin.Current);

                        break;

                    }

                }

            }

            count = int.Parse((position - fs.Position).ToString());

            line = new byte[count];

            fs.Read(line, 0, count);

            fs.Seek(-count, SeekOrigin.Current);

            return System.Text.ASCIIEncoding.ASCII.GetString(line, 0, line.Length);


        }
    }
}
