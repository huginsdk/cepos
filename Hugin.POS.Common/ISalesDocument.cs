using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public enum DocumentTypes
    {
        RECEIPT = -1,
        NULL,
        INVOICE,
        E_INVOICE,
        E_ARCHIEVE,
        MEAL_TICKET,
        CAR_PARKING,
        ADVANCE,
        COLLECTION_INVOICE,
        RETURN_DOCUMENT,
        CURRENT_ACCOUNT_COLLECTION,
        SELF_EMPLYOMENT_INVOICE
    }

    public interface ISalesDocument
    {
        int Id { get;set;}

        String Name { get;}

        String Code { get;}

        int DocumentTypeId { get;}

        DateTime CreatedDate { get;set;}

        int ResumedFromDocumentId { get;set;}

        Decimal TotalAmount { get;set;}

        Decimal TotalVAT { get;}

        Decimal BalanceDue { get;set;}

        Decimal[,] TaxRateTotals { get;}

        ICustomer Customer { get; set; }

        Decimal CustomerChange { get;set;}

        List<String> FootNote { get;set;}

        List<String> CurrentLog { get; set; }

        String TcknVkn { get; set; }

        int DocumentFileZNo { get; set; }
        
        Boolean IsEmpty { get;}

        Boolean CanEmpty { get; }
        /// <summary>
        /// Returns : Amount
        /// </summary>
        /// <returns></returns>
        String[] GetCashPayments();
        /// <summary>
        /// Returns : Amount | Reference Number
        /// </summary>
        /// <returns></returns>
        String[] GetCheckPayments();
        /// <summary>
        /// Returns : Amount | Exchange Rate | Name
        /// </summary>
        /// <returns></returns>
        String[] GetCurrencyPayments();
        /// <summary>
        /// Returns : Amount | Installments | Id
        /// </summary>
        /// <returns></returns>
        String[] GetCreditPayments();
        /// <summary>
        /// Returns: Amount | Percent (if type not percentage then uses '--' ) | CashierId
        /// </summary>
        /// <returns></returns>
        String[] GetAdjustments();

        /* added temproraly */
        List<IFiscalItem> Items { get;}
        List<PointObject> Points { get; }
        ICashier SalesPerson { get; }
        /*end :  added temproraly */

        /// <summary>
        /// Cashier can enter serial number of slip document manually(Optional property).
        /// This number added to main log file with SlipSerialNo instead of Document's id.
        /// </summary>
        String SlipSerialNo { get;}
        /// <summary>
        /// Cashier can enter order number of slip document manually(Optional property).
        /// This number added to main log file with SlipSerialNo instead of Document's id.
        /// </summary>
        String SlipOrderNo { get;}
        /// <summary>
        /// Shows to document payment isnot received really but document is closed.(debp)
        /// </summary>
        Boolean IsOpenDocument { get;}
        /// <summary>
        /// HARAKET lines produced by promotionserver
        /// </summary>
        String[] PromoLogLines { get;}
        /// Cashier can enter reason of the return item.
        /// This reason has added to main log file in line of IAN.
        /// </summary>
        String ReturnReason { get; }
        /// <summary>
        /// for invoice documents
        /// </summary>
        DateTime IssueDate { get; set; }
        /// <summary>
        /// For Advance
        /// </summary>
        String CustomerTitle { get; set; }
        /// <summary>
        /// comission amount for collectionInvoice
        /// </summary>
        Decimal ComissionAmount { get; set; }
        /// <summary>
        /// Additional document informations
        /// </summary>
        AdditionalDocInfo AdditionalInfo { get; set; }
        /// <summary>
        /// Flag value for print slip info receipt while printing invoice
        /// </summary>
        bool PrintSlipInfo { get; set; }
        /// <summary>
        /// service definition for SelfEmployementInvoice
        /// </summary>
        String ServiceDefinition { get; set; }
        /// <summary>
        /// gross wages for SelfEmployementInvoice
        /// </summary>
        Decimal ServiceGrossWages { get; set; }
        /// <summary>
        /// stoppagerate for SelfEmployementInvoice
        /// </summary>
        int ServiceStoppageRate { get; set; }
        /// <summary>
        /// vat rate for SelfEmployementInvoice
        /// </summary>
        int ServiceVATRate { get; set; }
        /// <summary>
        /// other rate for SelfEmployementInvoice
        /// </summary>
        int ServiceStoppageOtherRate { get; set; }

    }
}
