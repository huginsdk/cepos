using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{

    public enum ReceiptTypes
    {
        Z_REPORT = 0,       //Z raporu
        SALE,               //Satış fişi
        VOID,               //Satış fişi iptal
        INFO,               //Bilgi fişi 
        FISC_REPORT,        //Mali rapor (zz veya tarih)
        EJ_REPORT,          //Ekü raporu
        POWER_FAIL,         //Elektrik kesintisi satış iptal
        X_REPORT,           //X raporları
        REP_VOID,           //Rapor iptal
        INFO_VOID,          //bilgi fişi iptal
        INVOICE             //fatura
    }
    public class PrintedDocumentInfo
    {
        public int DocId = 0;
        public int ZNo = 0;
        public int EjNo = 0;
        public DateTime DocDateTime = new DateTime();
        public ReceiptTypes Type = ReceiptTypes.Z_REPORT;
    }
}
