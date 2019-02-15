using System;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS 
{
	internal delegate IState StateInstance();
    internal delegate IState StateInstance<T>(T t);
	
	public interface IState
    {
        Error NotImplemented{get;}
        bool IsIdle { get;}
        void Alpha(char c);
        void Numeric(char c);
        void Seperator(); 
        void LabelKey(int label);
        void Document();
        void Customer();
        void Report();
        void Program();
        void CashDrawer();
        void ReceiveOnAcct();
        void PayOut();
        void Void();
        void SalesPerson();
        void PriceLookup();
        void Price();
        void Command();
        void TotalAmount();
        void Repeat();
        void UpArrow();
        void DownArrow();
        void Escape();
        void Quantity();
        void SubTotal();
        void Enter();        
        void UndefinedKey();
        void OnExit();
        void OnEntry();
        void Adjust(AdjustmentType method);
        void Pay(CreditPaymentInfo info);
        void Pay(CurrencyPaymentInfo info);
        void Pay(CheckPaymentInfo info);
        void Pay(CashPaymentInfo info);
        void ShowPaymentList();
        void CardPrefix();
        void End(int keyLevel);
        void Correction();
        void BarcodePrefix();
        void SendOrder();

    }
	
}
