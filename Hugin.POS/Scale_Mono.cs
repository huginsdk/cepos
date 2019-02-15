using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
 
    class ScaleCAS : IScale
    {
		private static ScaleCAS scaleCas = null;
        private ScaleCAS()
        {
            Connect();
        }

        internal static IScale Instance()
        {
            if (scaleCas == null)
                scaleCas = new ScaleCAS();
            return scaleCas;
        }

        public void Connect()
        {
            
        }

        public decimal GetWeight(decimal unitPrice)
        {
			return 0;
        }

        public bool IsConnect
        {
            get { return false; }
        }

    }
}
