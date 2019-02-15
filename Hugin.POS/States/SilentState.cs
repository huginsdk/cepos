using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS
{
    abstract class SilentState : IState
    {
        public Error NotImplemented { get { return new Error(new InvalidOperationException()); } }
        public virtual bool IsIdle { get { return false; } }
        public virtual void Alpha(char c) { }
        public virtual void Numeric( char c) { }
        public virtual void Seperator() { }
        public virtual void LabelKey(int label) { }      
        public virtual void Document() { }
        public virtual void Customer() { }
        public virtual void Report() { }
        public virtual void Program() { }
        public virtual void CashDrawer() { }
        public virtual void ReceiveOnAcct() { }
        public virtual void PayOut() { }
        public virtual void Void() { }
        public virtual void Adjustment( AdjustmentType am) { }
        public virtual void Command() { }
        public virtual void PriceLookup() { }
        public virtual void Price() { }
        public virtual void TotalAmount() { }
        public virtual void Repeat() { }
        public virtual void UpArrow() { }
        public virtual void DownArrow() { }
        public virtual void Escape() { }
        public virtual void Quantity() { }
        public virtual void Pay(CreditPaymentInfo info) { }
        public virtual void Pay(CurrencyPaymentInfo info) { }
        public virtual void Pay(CheckPaymentInfo info) { }
        public virtual void Pay(CashPaymentInfo info) { }
        public virtual void Adjust(AdjustmentType method) { }
        public virtual void ShowPaymentList() { } 
        public virtual void SubTotal() { }
        public virtual void Enter() { }
        public virtual void UndefinedKey() { }
        public virtual void OnExit() { }
        public virtual void OnEntry() { }
        public virtual void SalesPerson() { }
        public virtual void CardPrefix() { }
        public virtual void BarcodePrefix() { }
        public virtual void Correction() { }
        public virtual void SendOrder() { }
        public virtual void End(int keyLevel)
        {
            if (cr.State != null && !(cr.State is States.Login))
            {
                cr.State = States.KeyState.Instance();
                cr.State.End(keyLevel);
            }
        }      
    }

}
