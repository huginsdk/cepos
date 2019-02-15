using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.Collections;

namespace Hugin.POS
{
    public class CustomerMenuList : MenuList
    {
        public override void ShowCurrent()
        {
            ShowCurrent(Target.Cashier);
            ShowCurrent(Target.Customer);
        }
        public override void ShowCurrent(Target t)
        {
            if (t == Target.Cashier)
                DisplayAdapter.Cashier.Show(Current);
            if (t == Target.Customer)
                DisplayAdapter.Customer.Show(Current);
            if (t == Target.Both)
                DisplayAdapter.Both.Show(Current);
        }
        public override void Sort()
        {
            base.Sort((IComparer)new CustomerListSorter());
        }
        public class CustomerListSorter : IComparer
        {

            public int Compare(object obj1, object obj2)
            {
                ICustomer c1 = obj1 as ICustomer;
                ICustomer c2 = obj2 as ICustomer;
                return c1.Name.CompareTo(c2.Name);
            }
                        
        }
         
        private ICustomer Current
        {
            get
            {
                return (ICustomer)((IEnumerator)this).Current;
            }
        }
    }
}
