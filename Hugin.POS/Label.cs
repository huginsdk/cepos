using System;

using System.Collections.Specialized;
using Hugin.POS.Common;
namespace Hugin.POS
{
	public static class Label{
        public const int BackSpace = 9;
        public const int Space = 10;
        public const int Enter = 11;

   		static int lastKey;
 		

   		public static int LastKey {
			get {
				return lastKey;
			}
            set { lastKey = value;}
		}
   		
   		public static String[] GetLabel(int label){
            if (!CashRegister.DataConnector.CurrentSettings.LabelMatrix.ContainsKey(label))
                throw new Exception(PosMessage.UNDEFINED_LABEL);
            return CashRegister.DataConnector.CurrentSettings.LabelMatrix[label].Split(',');
   		}
   		
        /// <summary>
        /// Cep telefonu usulu ayni tusa hizli tekrar
        /// basinca sequence artirilip yukarida tanimlanan 
        /// arraylerdeki bir sonraki karaktere geciliyor
        /// </summary>
        /// <param name="label">
        /// Key hits by user.
        /// </param>
        /// <param name="sequence">
        /// Next value index defined in this label.
        /// </param>
        /// <returns>
        /// Gets char value defined in this key's CharMatrix
        /// </returns>
        //public static char GetLetter(int label, ref byte sequence){   			
        //    lastKey = label;
        //    if (cr.DataConnector.CurrentSettings.CharMatrix[label].Length == sequence)
        //        sequence = 0; //labelda tanimlanan son karakterde ise basa don. (A B C A...)
        //    return cr.DataConnector.CurrentSettings.CharMatrix[label][sequence];
        //}
	}
}
