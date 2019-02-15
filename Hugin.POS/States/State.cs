/*
 * Hugin POS Project
 * User: nommazve
 * Date: 11/4/2006
 * Time: 10:21 PM
 * 
 */
 
using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
	/// <summary>
	/// Description of State.
	/// </summary>
	/// 
	abstract class State : IState
    {
        public virtual Error NotImplemented { get { return new Error(new InvalidOperationException()); } }

        public virtual bool IsIdle { get { return false; } }

        public virtual void Numeric(char c)
        {
            // sayisal veri girisi
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Alpha(char c)
        {
            // sayisal veri girisi
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Seperator() 
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void LabelKey(int label)
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }   
        public virtual void Document()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Customer()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Report()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Program()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }      
        public virtual void Command()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void CashDrawer()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void ReceiveOnAcct()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void PayOut()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Void()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void Adjust(AdjustmentType method)
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }  
        
        public virtual void PriceLookup()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Price()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void TotalAmount()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Repeat()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void UpArrow()
        {
            cr.Printer.Feed();
        }
        public virtual void DownArrow()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Escape()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Quantity()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        
        public virtual void SubTotal()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void Enter()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        
        public virtual void UndefinedKey()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void Pay(CreditPaymentInfo info)
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void Pay(CurrencyPaymentInfo info)
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void Pay(CheckPaymentInfo info)
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void Pay(CashPaymentInfo info)
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void ShowPaymentList()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void SalesPerson() 
        {
             cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void OnExit()
        {
            //CashRegister stateini degistirirken eski statein Exitini cagirir
        }

        public virtual void OnEntry()
        {
            //CashRegister stateini degistirirken eski statein Exitini cagirir
        }
        public virtual void CardPrefix()
        {
         cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void KeyboardKey(PosKey keyboardKey)
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
        public virtual void End(int keyLevel) 
        {
            if (cr.State != null && !(cr.State is States.Login)) 
            {
                cr.State = States.KeyState.Instance();
                cr.State.End(keyLevel);
            }
        
        }

        public virtual void Correction()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void BarcodePrefix()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }

        public virtual void SendOrder()
        {
            cr.State = AlertCashier.Instance(cr.State.NotImplemented);
        }
    }
}
