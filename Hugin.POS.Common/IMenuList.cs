using System;
using Hugin.POS;
namespace Hugin.POS.Common
{
    public interface IMenuList
    {
        int Add(object item);
        System.Collections.IEnumerator GetEnumerator();
        bool IsEmpty { get; }
        void MoveFirst();
        void MoveLast();
        bool MoveNext();
        bool MovePrevious();
        void Reset();
    }
}
