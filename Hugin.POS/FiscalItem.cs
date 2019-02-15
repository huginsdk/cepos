using System;
using System.Collections.Generic;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public delegate void OnTotalAmountUpdatedEventHandler(object sender, PriceUpdateEventArgs e);

    public class PriceUpdateEventArgs : EventArgs
    {
        Decimal oldPrice, newPrice;

        public PriceUpdateEventArgs(Decimal oldPrice, Decimal newPrice)
        {
            this.oldPrice = oldPrice;
            this.newPrice = newPrice;
        }

        public Decimal OldPrice
        {
            get { return oldPrice; }
        }
        public Decimal NewPrice
        {
            get { return newPrice; }
        }

    }
    
    public abstract class FiscalItem : IMenuItem, IAdjustable, IFiscalItem
    {
        const int MAX_QUANTITY = 10000;
        const decimal MIN_QUANTITY = 0.001M;
        const long MAX_TOTALAMOUNT = 9999999999;
        protected Decimal quantity;
        protected Decimal unitPrice;
        protected Decimal totalAmount;
        protected Decimal vatRate;
        protected List<Adjustment> adjustments;
        protected String name;
        protected String barcode;
        protected String unit;
        protected IProduct product;
        protected ICashier salesPerson;
        protected String serialNo;
        protected String batchNumber;
        protected DateTime expiryDate;
        protected Decimal voidQuantity = 0;
        protected Decimal voidAmount = 0;

        public event OnTotalAmountUpdatedEventHandler OnTotalAmountUpdated;
        
        public FiscalItem() {
        	Reset();
        }        

        public void Reset(){
            TotalAmount=0;
            Quantity=1;
            UnitPrice=0;
            Name = "";
            Barcode = "";
            unit = "";
            vatRate = 0;
            product = null;
            adjustments = new List<Adjustment>();
        }

        public Decimal Adjust(IAdjustment a)
        {
            //TODO: Necessary business rules to check if adjustment is valid
            Adjustment adjustment = (Adjustment)a;
            if (adjustment.Target != this)
            {
                adjustment = new Adjustment(this, adjustment.Method, 
                                                  adjustment.RequestValue, 
                                                  adjustment.AuthorizingCashierId);
            }
            if (adjustment.NetAmount == 0) return 0;
            if (TotalAmount + adjustment.NetAmount < 0)
                throw new NegativeResultException();
            adjustments.Add(adjustment);

            Decimal oldAmount = TotalAmount;
            totalAmount = TotalAmount + adjustment.NetAmount;
            //TODO unit price 2 basamaga yuvarlanmali mi?
            unitPrice = totalAmount / quantity;
            if (OnTotalAmountUpdated != null)
                OnTotalAmountUpdated(this, new PriceUpdateEventArgs(oldAmount, totalAmount));
            //gerekesiz tekrar gibi gozukuyor ama degil total - Total farki ikisi de lazim
            unitPrice = TotalAmount / quantity;
            return adjustment.NetAmount;
        }
    
		public String Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}
       
		public String Barcode {
			get {
				return barcode;
			}
			set {
				barcode = value;
			}
		}
		public String Unit {
			get {
				return unit;
			}
			set {
				unit = value;
			}
		}
       
        public override String ToString() {
            return Name;
        }

        public IProduct Product 
        {
            get { return product; }
            set { 
            	product = value;
            	if (product == null) return;
            	name = product.Name;
                unit=product.Unit;
                vatRate = product.Department.TaxRate;
                barcode = product.Barcode;
                if(product.Quantity!=1)
                {
                    quantity = product.Quantity;
                    //bu islem ozel barkodlu urunler icin gecerli
                }
                if (this.product.Status != ProductStatus.Weighable)
                {
                    if (HasSeperator)
                    {
                        throw new ProductNotWeighableException();
                    }
                }
                if(unitPrice==0)
                {
                    //fiyatli satis degil, urunun standart fiyati kullanilacak
                	unitPrice = product.UnitPrice;
                    //1.fiyat girilmemiþse, 2.fiyat geçerlidir.
                    //1.fiyat girilmiþ olsa bile, müþteri giriþi varsa, sadece müþteride geçerli olsun 

                    if (unitPrice == 0 || (cr.Document != null && cr.Document.Customer != null && product.SecondaryUnitPrice != 0))
                        unitPrice = product.SecondaryUnitPrice;
                }
                if (totalAmount>0){
                    //Todo quantity 3 basamak degil
                    decimal temp = totalAmount / unitPrice;
                    if (Math.Round(temp, 3) >= MAX_QUANTITY)
                        throw new OutofQuantityLimitException();
                    if (temp < MIN_QUANTITY)
                        throw new InvalidQuantityException();
                	quantity = temp;
                 
                }
            
            }
        }

        public virtual Decimal ListedAmount
        {
            get
            {
                if (unitPrice == 0 || (cr.Document != null && cr.Document.Customer != null && product.SecondaryUnitPrice != 0))
                    return Rounder.RoundDecimal(this.product.SecondaryUnitPrice * quantity, 2, true);
                return Rounder.RoundDecimal(this.product.UnitPrice * quantity, 2, true);
            }
        }

        public virtual Decimal TotalAmount
        {
            get
            {
                return (totalAmount > 0) ? totalAmount : Rounder.RoundDecimal(UnitPrice * quantity, 2, true);
            }
        	set {
                if (TotalAmount == value) { totalAmount = value; return; }
                Decimal oldAmount = TotalAmount;
                totalAmount = value;
                if (OnTotalAmountUpdated != null)
                    OnTotalAmountUpdated(this, new PriceUpdateEventArgs(oldAmount, totalAmount));
        	}
        }

        public decimal VAT
        {           
            get { return ((decimal)TotalAmount * vatRate) /(1+vatRate); }
                    
        }
        public virtual Decimal Quantity 
        {
            get { return quantity;
        	}
            set
            {
                decimal qValue = Math.Round(value, 3);
                if (qValue >= MAX_QUANTITY)
                    throw new OutofQuantityLimitException();
                if (qValue < MIN_QUANTITY)
                    throw new InvalidQuantityException();
                quantity = qValue; 
                totalAmount = 0;
            }
        }
        public virtual bool HasSeperator
        {
            get
            {
                if (product.UnitPrice == 0)
                    return quantity % 1 != 0;
                return quantity % 1 != 0 || totalAmount/product.UnitPrice % 1 !=0;
            }
        }
         
		public Decimal UnitPrice {
			get {
				return unitPrice;
			}
			set {
                decimal oldUnitPrice = unitPrice;
                unitPrice = Rounder.RoundDecimal(value, 2, true);
                //if totalprice is set we need to fix it otherwise leave it alone
                //needed for diplomaticsale document.additem
                if (oldUnitPrice == unitPrice) return;
                if (totalAmount != 0)
                    totalAmount = Rounder.RoundDecimal(unitPrice * quantity, 2, true);
			}
		}

        public List<Adjustment> Adjustments { get { return adjustments; } }


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

        public virtual Object Clone(){
        	FiscalItem cloneItem = (FiscalItem)this.MemberwiseClone();
            cloneItem.adjustments = new List<Adjustment>(this.adjustments);
            return cloneItem;
        }        

		public override bool Equals(object obj)
		{
			if (obj == null) 
				return false;
			if (!(obj is FiscalItem))
				return false;
			FiscalItem fi = (FiscalItem) obj;
			return (this.TotalAmount == fi.TotalAmount && this.Product == fi.Product);
		}	

		public override int GetHashCode()
		{
            return (int)TotalAmount * product.GetHashCode();
		}

        abstract public FiscalItem Void();
        abstract public void Show();
        abstract public void Show(Target target);

        public ICashier SalesPerson
        {
            get { return salesPerson; }
            set { salesPerson = value; }
        }
        public void ClearEventLog() 
        {
            OnTotalAmountUpdated = null;
        }
        public abstract IState ConfirmSalesPerson(ICashier salesPerson);
        public abstract IState VoidSalesPerson();

        #region IFiscalItem Members

        public int TaxGroupId
        {
            get { return Product.Department.TaxGroupId; }
        }

        /// <summary>
        /// gets or sets the quantity how much the item was canceled
        /// </summary>
        public Decimal VoidQuantity
        {
            get { return voidQuantity; }
            set { voidQuantity = value; }
        }
        /// <summary>
        /// gets or sets the amount how much the item was canceled
        /// </summary>
        public Decimal VoidAmount
        {
            get { return voidAmount; }
            set { voidAmount = value; }
        }

        public string SerialNo
        {
            get { return serialNo; }
            set { serialNo = value; }
        }

        public string BatchNumber
        {
            get { return batchNumber; }
            set { batchNumber = value; }
        }

        public DateTime ExpiryDate
        {
            get { return expiryDate; }
            set { expiryDate = value; }
        }

        #endregion

        internal bool CanAdjust(Adjustment promoAdjustment)
        {
            List<Adjustment> itemAdjs = this.Adjustments.FindAll(delegate(Adjustment adj)
            {
                return adj.AuthorizingCashierId == cr.PROMOTION_ITEM_CASHIER_ID;
            });

            if (itemAdjs.Count > 0)
                return false;
            else
                return true;
        }
    }
}
