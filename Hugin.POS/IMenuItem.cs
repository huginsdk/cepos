using System;
using Hugin.POS.Common;
namespace Hugin.POS
{
	public interface IMenuItem
    {
		void Show();
		void Show(Target t);
    }
		
}
