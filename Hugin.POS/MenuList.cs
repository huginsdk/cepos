using System;
using System.Collections;
using Hugin.POS.Common;
namespace Hugin.POS
{
    public class MenuList : ArrayList, IEnumerable, IDoubleEnumerator, IMenuList
    {

        // explicit interface implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;// (IEnumerator)new DoubleEnumerator(this);
        }
        //?why we update this method
        public new IEnumerator GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();// new MenuList.DoubleEnumerator(this);
        }

        public override int Add(object item)
        {
            if (item == null) return -1;
            //if (!this.Contains(item))
            return base.Add(item);
            //	else return -1;
        }
        public bool IsEmpty
        {
            get { return Count == 0; }
        }


        private int currentIndex = -1;


        public void MoveFirst()
        {
            currentIndex = 0;
        }

        public bool MoveNext()
        {
            if (currentIndex == this.Count)
                return false;
            return (++currentIndex < this.Count);
        }

        public bool MovePrevious()
        {
            if (currentIndex < 0)
                return false;
            return (--currentIndex > -1);
        }

        public void MoveLast()
        {
            currentIndex = this.Count - 1;
        }

        public virtual void ShowCurrent()
        {
            Current.Show();
        }
        public virtual void ShowCurrent(Target t)
        {
            Current.Show(t);
        }
        private IMenuItem Current
        {
            get
            {
                Object obj = ((IEnumerator)this).Current;
                if (obj is IMenuItem)
                    return (IMenuItem)obj;
                else
                    throw new ListingException(obj.GetType().Name+"\nIS NOT A IMENUITEM");
            }
        }

        // explicit interface implementation
        object IEnumerator.Current
        {
            get
            {
                if (currentIndex < 0 || currentIndex > this.Count - 1)
                    throw new InvalidOperationException();
                return this[currentIndex];
            }
        }

        public void Reset()
        {
            currentIndex = -1;
        }


        #region IMenuList Members


        #endregion
    }
}
