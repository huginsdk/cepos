using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface ISecurityConnector
    {
        //event EventHandler CashierCaptured;
        ICashier CheckAutoLogin();
        event EventHandler CustomerCaptured;
        string CashierMessage { get;}
        bool AcceptsPassword { get;}
        ICashier LoginCashier(String password);
        ICashier LoginManager(String managerId);
        ICustomer CurrentCustomer { get;}
        void AcceptCustomer(String customerNumber);
        void EscapeCustomer();
        void Close();
    }
}
