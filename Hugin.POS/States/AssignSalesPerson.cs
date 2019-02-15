using System;
using System.Collections;
using System.Text;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class AssignSalesPerson : SilentState
    {
        private static IState state = new AssignSalesPerson();
        private static ICashier salesPerson;
        private static StringBuilder password;
        private static bool subtotal;
        
        /// <summary>
        /// - state instance
        /// </summary>
        /// <returns></returns>
        public static IState Instance()
        {
            password = new StringBuilder();
            DisplayAdapter.Cashier.Show(PosMessage.CLERK+" ({0})", subtotal ? PosMessage.TOTAL : PosMessage.PRODUCT);
            return state;
        }
        /// <summary>
        /// -state instance
        /// </summary>
        /// <param name="subtotal"></param>
        /// <returns></returns>
        public static IState Instance(bool subtotal)
        {
            // singleton logic (a state can only exist once)
            // Console.WriteLine("Start State Instance"); 
            AssignSalesPerson.subtotal = subtotal;
            return Instance();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        public override void Numeric(char c)
        {
            password.Append(c.ToString());

            if (password.Length == 6)
            {
                salesPerson = cr.DataConnector.FindCashierByPassword(password.ToString());
                DisplayAdapter.Cashier.Show(PosMessage.CLERK+" ({0})\n{1}", subtotal ? PosMessage.TOTAL : PosMessage.PRODUCT,
                                                          (salesPerson != null) ? salesPerson.Name :
                                                                                PosMessage.INVALID_PASSWORD);
                password = new StringBuilder();
                return;
            }

            DisplayAdapter.Cashier.Show(PosMessage.CLERK+" ({0})\n{1}", subtotal ? PosMessage.TOTAL : PosMessage.PRODUCT, password.ToString());
         }
        /// <summary>
        /// - enter key function
        /// </summary>
        public override void Enter()
        {
            //if (subtotal)
            //{
            //    cr.SalesDoc.IsSubTotal = false;
            //    cr.SalesDoc.SalesPerson = salesPerson;
            //}
            //else
            //    cr.CurrentItem.SalesPerson = salesPerson;

            cr.State = Start.Instance();
        }
        /// <summary>
        /// - ESC function
        /// </summary>
        public override void Escape()
        {
            if (password.Length==0)
            {
                cr.State = States.Start.Instance();
            }
            else
            {
                cr.State = Instance();
            }
        }

    }
}