using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.PromotionServer
{
    internal class PromotionItem : ICloneable
    {
        #region "PromotionItem definitions"
        private int id;
        private Decimal quantity;
        private Decimal totalAmount;
        private int index;
        private Decimal totalAdjustment;
        private Decimal giftPromotionValue;
        private int giftProductLabelNo;
        #endregion

        internal PromotionItem()
        {
        }
        internal PromotionItem(int id, Decimal quantity, Decimal totalAmount)
        {
            this.id = id;
            this.quantity = quantity;
            this.totalAmount = totalAmount;
        }
        internal int Id
        {
            get { return id; }
            set { id = value; }
        }
        internal Decimal Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }
        internal Decimal TotalAmount
        {
            get { return totalAmount; }
            set { totalAmount = value; }
        }
        internal int Index
        {
            get { return index; }
            set { index = value; }
        }
        internal Decimal TotalAdjustment
        {
            get { return totalAdjustment; }
            set { totalAdjustment = value; }
        }
        internal Decimal GiftPromotionValue
        {
            get { return giftPromotionValue; }
            set { giftPromotionValue = value; }
        }
        internal int GiftProductLabelNo
        {
            get { return giftProductLabelNo; }
        }

        #region ICloneable Members

        public object Clone()
        {
            PromotionItem item = new PromotionItem();
            item.Id = this.id;
            item.quantity = this.quantity;
            item.TotalAmount = this.totalAmount;
            item.totalAdjustment = this.totalAdjustment;
            item.giftProductLabelNo = this.giftProductLabelNo;
            item.giftPromotionValue = this.giftPromotionValue;
            item.Index = this.index;
            return item;
        }

        #endregion
    }
}
