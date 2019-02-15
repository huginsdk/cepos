using System;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class MenuLabel: IMenuItem
    { 
        private String message;
        private Object value;
        public MenuLabel(String s) { message = s; value = s; }
        public MenuLabel(String s, Object o) { message = s; value = o; }
        public void Show()
        {
            DisplayAdapter.Cashier.Show(message);
        }
        public void Show(Target t) { Show(); }
        public override String ToString() { return message; }
        public Object Value { get { return value; } }
    }    
}
