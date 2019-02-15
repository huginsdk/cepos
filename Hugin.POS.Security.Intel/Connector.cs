using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;

namespace Hugin.POS.Security
{
    public class Connector : ISecurityConnector
    {
        ICustomer currentCustomer;
        ICashier currentCashier;

        private static ISecurityConnector connector = null;

        //public event EventHandler CashierCaptured;
        public event EventHandler CustomerCaptured;

        public static ISecurityConnector Instance()
        {
            if (connector == null)
                connector = new Connector();
            return connector;
        }
        private Connector()
        {
           
        }
        public bool AcceptsPassword
        {
            get { return true;}
        }
        public string CashierMessage
        {
            get { return PosMessage.ENTER_PASSWORD; }
        }

        internal IDataConnector DataConnector
        {
            get
            {
                return Data.Connector.Instance();
            }
        }

        public ICustomer CurrentCustomer { get { return currentCustomer; } }

        public ICashier LoginCashier(String password)
        {
            //return DataConnector.FindCashierById(cashierId);
            return DataConnector.FindCashierByPassword(password);
        }
        public ICashier LoginManager(String managerId)
        {
            return DataConnector.FindCashierById(managerId);
            //return DataConnector.FindCashierByPassword(password);
        }

        public void AcceptCustomer(String customerNumber)
        {
            currentCustomer = DataConnector.FindCustomerByCode(customerNumber);
        }

        public void EscapeCustomer()
        {
            currentCustomer = null;
        }

        public void Close()
        {
        }

        public ICashier CheckAutoLogin()
        {
            // System.Threading.Thread.Sleep(500);
            if (File.Exists(IOUtil.ProgramDirectory + "autologin.dat"))
            {
                String id = IOUtil.ReadAllText(IOUtil.ProgramDirectory + "autologin.dat");
                currentCashier = DataConnector.FindCashierById(id);
                if (currentCashier != null)
                    return currentCashier;
            }
            return null;
        }
    }
}
