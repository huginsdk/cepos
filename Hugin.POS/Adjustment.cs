using System;
using Hugin.POS.Common;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS
{
    /// <summary>
    /// Adjustment
    /// </summary>
    public class Adjustment : IMenuItem, IAdjustment
    {
        AdjustmentType method;
        Decimal requestValue;
        Decimal netAmount;
        Decimal adjustedTotal;
        IAdjustable adjustedObject;
        DateTime createdTime;
        String authorizingCashierId;
        String label="";
        decimal[] taxGroupAdjustments = null;
        bool isCorrection = false;

        //Is adjustment type is correction.
        public bool IsCorrection
        {
            get { return isCorrection; }
            set { isCorrection = value; }
        }

        /// <summary>
        /// Id of cashier who applied the adjustment
        /// </summary>
        public String AuthorizingCashierId
        {
            get { return authorizingCashierId; }
            set { authorizingCashierId = value; }
        }
        /// <summary>
        /// Adjust percent discount,discount,percent fee and fee transaction
        /// </summary>
        /// <param name="target">
        /// The target that will be adjusted.
        /// </param>
        /// <param name="type">
        /// Type of adjustment.
        /// </param>
        /// <param name="input">
        /// Adjustment amount.
        /// </param>
        public Adjustment(IAdjustable target, AdjustmentType type, Decimal input)
        {
            this.method = type;
            input = Math.Round(input, 2);
            this.requestValue = input;
            this.adjustedObject = target;
            this.adjustedTotal = target.TotalAmount;
            createdTime = DateTime.Now;

            if (cr.CurrentCashier != null)
                authorizingCashierId = cr.CurrentCashier.Id;
            /*
             *  0.0001m added because of math.round bug
             *  for example: Math.Round(0.145, 2) = 0.14
             *           but Math.Round(0.1451,2) = 0.15
             */
            switch (type) { 
                case AdjustmentType.Discount:
                    netAmount = -1m * input; 
                    break;
                case AdjustmentType.PercentDiscount:
                    requestValue = Math.Round(input, 0);
                    netAmount = (-1m) * Rounder.RoundDecimal((input / 100) * target.TotalAmount + 0.0001m, 2, true);
                    break;
                case AdjustmentType.Fee:
                    netAmount = input; 
                    break;
                case AdjustmentType.PercentFee:
                    requestValue = Math.Round(input, 0);
                    netAmount = Rounder.RoundDecimal((input / 100) * target.TotalAmount + 0.0001m, 2, true);
                    break;
            }

        }
        /// <summary>
        /// Apply adjustment on target
        /// </summary>
        /// <param name="target">such as fiscal item or sale document</param>
        /// <param name="type">fee or discount</param>
        /// <param name="input">amount or percentage</param>
        /// <param name="cashierId">id of cashier who aplies adjustment</param>
        public Adjustment(IAdjustable target, AdjustmentType type, Decimal input, String cashierId)
                         :this(target, type, input) {
            
            if (cashierId != null && cashierId != "")
                authorizingCashierId = cashierId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <returns></returns>
        public Decimal[] GetTaxGroupAdjustments()
        {
            if (taxGroupAdjustments == null)
            {
                SalesDocument document = (SalesDocument)(this.Target);
                taxGroupAdjustments = new Decimal[Department.NUM_TAXGROUPS];
                decimal productAdjustment, totalAdjustment = 0, maxAdjustment = 0;
                int maxAdjustedProduct = 0;

                foreach (int dep in document.TaxGroupTotals.Keys)
                {
                    productAdjustment = Rounder.RoundDecimal(this.NetAmount * (document.TaxGroupTotals[dep] / this.adjustedTotal),
                                                   2,
                                                   true);
                    if (Math.Abs(productAdjustment) > Math.Abs(maxAdjustment))
                    {
                        maxAdjustment = productAdjustment;
                        maxAdjustedProduct = dep;
                    }
                    taxGroupAdjustments[dep] += productAdjustment;
                    totalAdjustment += productAdjustment;
                }
                taxGroupAdjustments[maxAdjustedProduct] += this.NetAmount - totalAdjustment;
            }
            return taxGroupAdjustments;
        }
        /// <summary>
        /// Adjusted object
        /// </summary>
        public IAdjustable Target { get { return adjustedObject; } set { adjustedObject = value; }}       
        /// <summary>
        /// Amount of adjustment
        /// </summary>
        public Decimal NetAmount { get { return netAmount; } set { netAmount = value; } }
        /// <summary>
        /// Percentage or amount of adjustment
        /// </summary>
        public Decimal RequestValue { get { return requestValue; } }
       
        /// <summary>
       /// Percent or Amount, Fee or Discount
       /// </summary>
        public AdjustmentType Method { get { return method; } }
       
        /// <summary>
        /// Short definition of adjustment
        /// </summary>
        public String Label { 
            get {
                if (label.Length > 0) return label;
                Boolean isDiscount = (method == AdjustmentType.Discount || method == AdjustmentType.PercentDiscount);
                return isDiscount ? PosMessage.DISCOUNT : PosMessage.FEE; 
            }
            set { label = value; }
        }
       /// <summary>
        /// Show adjustment on display
       /// </summary>
        public void Show()
        {
            DisplayAdapter.Both.Show(this);
        }
        /// <summary>
        /// Show adjustment on target display
        /// </summary>
        /// <param name="target"></param>
        public void Show(Target target)
        {
            if (target == Common.Target.Cashier)
                DisplayAdapter.Cashier.Show(this);
            else
                DisplayAdapter.Customer.Show(this);
        }

        /// <summary>
        /// Converts amount discount or fee to percent discount or percentfee.
        /// </summary>
        /// <returns>
        /// Adjusted object.
        /// </returns>
        internal Adjustment ToPercent()
        {
            Decimal percentAdjustment;
            switch (method)
            {
                case AdjustmentType.Discount:
                    percentAdjustment = -100 * netAmount / adjustedTotal;
                   //Target.TotalAmount += netAmount + (percentAdjustment * Target.TotalAmount / 100);
                    return new Adjustment(Target, AdjustmentType.PercentDiscount, percentAdjustment, authorizingCashierId);
                case AdjustmentType.Fee:
                    percentAdjustment = 100 * netAmount / adjustedTotal;
                    return new Adjustment(Target, AdjustmentType.PercentFee, percentAdjustment, authorizingCashierId);
                default:
                    return this;
            }

        }
    }

  
}


