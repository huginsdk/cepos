using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.PromotionServer
{
   internal static class Message
    {
       internal const String PROMOTION_FILE_LOAD_ERROR = "PROMOSYON DOSYASINDA\nHATA OLUSTU";
       internal const String PROMOTION_OPERATION = "PROMOSYON ÝÞLEMÝ";
       #region Log Keywords
       internal const String SAT = "SAT";
       internal const String IPT = "IPT";
       internal const String NAK = "NAK";
       internal const String DVZ = "DVZ";
       internal const String KRD = "KRD";
       internal const String CHK = "CEK";
       internal const String TOP = "TOP";
       internal const String END = "SON";
       internal const String IND = "IND";
       internal const String SNS = "SNS";
       internal const String ART = "ART";
        #endregion
    }
}
