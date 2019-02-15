using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;
using System.Xml;
using System.IO;


namespace Hugin.POS.Common
{
    public enum PromoMessageCode : int
    {
        StartDocument = 0,      //Document Starts
        CloseDocument = 1,      //Document Closed
        VoidDocument = 2,       //Document Voided
        SuspendDocument = 3,    //Document Suspended
        ZReport = 4             //Z report printed
    }

    public interface IPosClient
    {
        String[] DocumentRequest(String[] requestItems, bool isFirstPayment);
        String[] ItemRequest(String[] requestItems);
        void Close();
        bool LogOn();
        void LogOff();
        int ConnectionTimeout { get; }
        /// <summary>
        /// Sends promotion server document and reports status
        /// </summary>
        /// <param name="messageCode">
        /// 0: Document Starts
        /// 1: Document Closed
        /// 2: Document Voided
        /// 3: Document Suspended
        /// 4: Z report printed
        /// </param>
        /// <param name="messageItems"></param>
        void Messages(int messageCode, String[] messageItems);
        string SearchCustomer(String customerCode);
    }
}
