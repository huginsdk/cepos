using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.PromotionServer
{
    internal enum LimitType
    {
        NoLimit,
        Quantity,
        Amount
    }

    internal abstract class BasePromotion
    {
        #region "Promotion definitions"
        protected int id;
        protected string code;
        protected PromotionRange range;
        protected DateTime startDate;
        protected DateTime endDate;
        protected int percentDiscount = 0;
        protected decimal discount = 0;
        protected int point = 0;
        protected int extraPoint = 0;
        protected Decimal requiredAmount = 0;
        protected int requiredQuantity;
        protected int giftProductLabelNo;
        protected int giftProductQuantity;
        protected string promoRemark;
        protected List<string> paymentTypes;
        protected List<string> customerGroups;
        protected LimitType limitType = LimitType.NoLimit;

       #endregion

        #region "Promotion accessors"
        public int Id
        {
            get { return id; }
        }
        public PromotionRange Range
        {
            get { return range; }
        }
        public int PercentDiscount
        {
            get { return percentDiscount; }
        }
        public Decimal Discount
        {
            get { return discount; }
        }
        public int GiftProductQuantity
        {
            get { return giftProductQuantity; }

        }
        public int GiftProductLabelNo
        {
            get { return giftProductLabelNo; }
        }
        public int RequiredQuantity
        {
            get { return requiredQuantity; }
        }
        public Decimal RequiredAmount
        {
            get { return this.requiredAmount; }
        }

        public int Points
        {
            get { return point; }
        }

        public int ExtraPoints
        {
            get { return extraPoint; }
        }
        public string PromoRemark
        {
            get { return promoRemark; }
        }

        public LimitType LimitType
        {
            get { return limitType; }
        }

        #endregion

        public virtual bool PromotionCoverAll(PromotionRange range)
        {
            return this.range <= range;
        }
        public virtual String PromotionCode
        {
            get
            {
                return code;
            }
        }
        public virtual bool CheckPaymentType(String type)
        {
            return true;
        }

        public virtual bool CheckCustomerGroup(ICustomer customer)
        {
            if (customerGroups == null || customerGroups.Count == 0) return true;

            return customerGroups.Contains(customer.CustomerGroup);
        }

        public virtual bool HavePayment
        {
            get
            {
                return (paymentTypes.Count < 0);
            }
        }

        protected void SetDateRange(string strStartDate, string strStartTime, string strEndDate, string strEndTime)
        {
            this.startDate = Convert.ToDateTime(String.Format("{0:d} {1:t}", strStartDate, strStartTime), PosConfiguration.GetDefCulture());
            this.endDate = Convert.ToDateTime(String.Format("{0:d} {1:t}", strEndDate, strEndTime), PosConfiguration.GetDefCulture());

            if (startDate > DateTime.Now || endDate < DateTime.Now)
                throw new ParameterRelationException("Promotion date range is not valid");
        }
    }
}
