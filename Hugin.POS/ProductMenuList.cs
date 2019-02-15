using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.Collections;

namespace Hugin.POS
{
    public class ProductMenuList : MenuList
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
        private IProduct Current
        {
            get
            {
                return (IProduct)((IEnumerator)this).Current;
            }
        }
    }
}
