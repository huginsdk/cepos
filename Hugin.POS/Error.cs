using System;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class Error:Confirm
    {
        internal static Exception lastException = null;

        public Error(){}

        public Error(Exception ex) {
            this.Message = GetMessage(ex);
            lastException = ex;
        }

        internal Error(Exception ex, StateInstance ConfirmState)
            : this(ex)
        {
            ReturnConfirm = ConfirmState;
        }


        internal Error(Exception ex, StateInstance ConfirmState, StateInstance CancelState)
            : this(ex, ConfirmState)
        {
            ReturnCancel = CancelState;
        }

        internal Error(Exception ex, StateInstance<Hashtable> ConfirmState)
            : this(ex)
        {
            Data = new Hashtable();
            ReturnConfirmWithArgs = ConfirmState;
        }

        internal Error(Exception ex, StateInstance<Hashtable> ConfirmState, StateInstance CancelState)
            : this(ex, ConfirmState)
        {
            ReturnCancel = CancelState;
        }

        internal static Exception LastException
        {
            get { return lastException; }
        }
        internal static void ResetLastException()
        {
            lastException = null;
        }

        private string GetMessage(Exception ex)
        {
            try
            {
                throw ex;
            }
            catch (NullReferenceException)
            {
                return PosMessage.NULL_REFERENCE_EXCEPTION;
            }
            catch (InvalidQuantityException)
            {
                return PosMessage.INVALID_QUANTITY;
            }
            catch (OutofQuantityLimitException)
            {
                return PosMessage.OUTOF_QUANTITY_LIMIT;
            }
            catch (NoReceiptRollException)
            {
                return PosMessage.NORECEIPTROLL;
            }
            catch (NoJournalRollException)
            {
                return PosMessage.NOJOURNALROLL;
            }
            catch (PrinterStatusException)
            {
                return PosMessage.CHECK_PRINTER;
            }
            catch (RFUException)
            {
                return PosMessage.RFU_ERROR;
            }
            catch (OpenShutterException)
            {
                return PosMessage.OPEN_SHUTTER;
            }
            catch (PrinterOfflineException)
            {
                return PosMessage.PRINTER_OFFLINE;
            }
            catch (FramingException)
            {
                return PosMessage.UNDEFINED_EXCEPTION;
            }
            catch (ChecksumException)
            {
                return PosMessage.CHECK_SUM_EXCEPTION;
            }
            catch (UndefinedFunctionException)
            {
                return PosMessage.UNDEFINED_FUNCTION_EXCEPTION;
            }
            catch (CmdSequenceException)
            {
                return PosMessage.CMD_SEQUENCE_EXCEPTION;
            }
            catch (NegativeResultException)
            {
                return PosMessage.NEGATIVE_RESULT_EXCEPTION;
            }
            catch (PowerFailureException)
            {
                return PosMessage.POWER_FAIURE_EXCEPTION;
            }
            catch (EntryException)
            {
                return PosMessage.ENTRY_EXCEPTION;
            }
            catch (LimitExceededOrZRequiredException)
            {
                return PosMessage.LIMIT_EXCEEDED_OR_ZREQUIRED_EXCEPTION;
            }
            catch (FiscalCommException)
            {
                return PosMessage.FISCAL_COMM_EXCEPTION;
            }
            catch (FiscalMismatchException)
            {
                return PosMessage.FISCAL_MISMATCH_EXCEPTION;
            }
            catch (FiscalUndefinedException)
            {
                return PosMessage.FISCAL_UNDEFINED_EXCEPTION;
            }
            catch (BlockingException)
            {
                //return PosMessage.BLOCKING_EXCEPTION;
                return ex.Message;
            }
            catch (SVCPasswordOrPointException)
            {
                return PosMessage.SVC_PASSWORD_OR_POINT_EXCEPTION;
            }
            catch (LowBatteryException)
            {
                return PosMessage.LOW_BATTERY_EXCEPTION;
            }
            catch (BBXNotBlankException)
            {
                return PosMessage.BBX_NOT_BLANK_EXCEPTION;
            }
            catch (BBXFormatFaliureException)
            {
                return PosMessage.BBX_FORMAT_FAILURE_EXCEPTION;
            }
            catch (BBXDirectoryException)
            {
                return PosMessage.BBX_DIRECTORY_EXCEPTION;
            }
            catch (MissingCashierException)
            {
                return PosMessage.MISSING_CASHIER_EXCEPTION;
            }
            catch (AssignedCashierLimitExeedException)
            {
                return PosMessage.DIFFERENT_CASHIER_ASSING_LIMIT_EXEED_EXCEPTION;
            }
            catch (CashierAlreadyAssignedException)
            {
                return PosMessage.CASHIER_ALREADY_ASSIGNED_EXCEPTION;
            }
            catch (AlreadyFiscalizedException)
            {
                return PosMessage.ALREADY_FISCALIZED_EXCEPTION;
            }
            catch (DTGException)
            {
                return PosMessage.DTG_EXCEPTION;
            }
            catch (FiscalIdException)
            {
                return PosMessage.FISCAL_ID_EXCEPTION;
            }
            catch (PrinterTimeoutException)
            {
                return PosMessage.PRINTER_TIMEOUT;
            }
            catch (TimeoutOnConnectPrinterException)
            {
                return PosMessage.CHECK_RECEIPT_ROLL;
            }
            catch (ZReportUnsavedException)
            {
                return PosMessage.ZREPORT_UNSAVED;
            }
            catch (IncompleteXReportException)
            {
                return PosMessage.INCOMPLETE_XREPORT;
            }
            catch (IncompletePaymentException)
            {
                return PosMessage.INCOMPLETE_PAYMENT;
            }
            catch (DocumentNotEmptyException)
            {
                return PosMessage.DOCUMENT_NOT_EMPTY;
            }
            catch (PrintDocumentException)
            {
                return PosMessage.DOCUMENT_NOT_PRINTED;
            }
            catch (SubtotalNotMatchException)
            {
                return PosMessage.SUBTOTAL_NOT_MATCH;
            }
            catch (FMFullException)
            {
                return PosMessage.FM_FULL_REGISTER_BLOCKED;
            }
            catch (FMLimitWarningException)
            {
                return PosMessage.FM_HAS_NO_AREA_ZREQUIRED;
            }
            catch (ExternalDevMatchException)
            {
                return PosMessage.CANNOT_MATCH_EXT_DEV;
            }
            catch (EcrNonFiscalException)
            {
                return PosMessage.CANNOT_MATCH_EXT_DEV;
            }
            catch (FMNewException)
            {
                return PosMessage.CANNOT_MATCH_EXT_DEV;
            }
            catch (UndefinedTaxRateException)
            {
                return PosMessage.UNDEFINED_TAX_RATE;
            }
            catch (FPUException)
            {
                return PosMessage.FPU_ERROR;
            }
            catch (PrinterException pe)
            {
                if (pe.Message == PosMessage.Z_REQUIRED_GET_Z)
                {
                    return PosMessage.Z_REQUIRED_GET_Z;
                }
                if (pe is ZRequiredException)
                    return PosMessage.LOGO_DEPARTMENT_CHANGE_Z_REPORT_REQUIRED;
                return PosMessage.PRINTER_EXCEPTION;
            }
            catch (EJIdMismatchException)
            {
                return PosMessage.EJ_MISMATCH;
            }
            catch (EJFiscalIdMismatchException)
            {
                return PosMessage.EJ_FISCALID_MISMATCH;
            }
            catch (EJFullException)
            {
                return PosMessage.EJ_FULL;
            }
            catch (EJBitmapException)
            {
                return PosMessage.EJ_BITMAP_ERROR;
            }
            catch (EJFormatException)
            {
                return PosMessage.EJ_FORMAT_ERROR;
            }
            catch (EJCommException)
            {
                return PosMessage.EJ_NOT_AVAILABLE;
            }
            catch (EJChangedException ejc)
            {
                return PosMessage.EJ_CHANGED + ejc.EJId;
            }
            catch (EJZLimitSettingException)
            {
                return PosMessage.EJ_LIMIT_SETTING;
            }
            catch (NoZReportInEJException)
            {
                return PosMessage.NO_ZREPORT_IN_EJ;
            }
            catch (EJLimitWarningException limitex)
            {
                return String.Format(PosMessage.EJ_AVAILABLE_LINES + "\n%{0}", limitex.Usage);
            }
            catch (EJException)
            {
                return PosMessage.EJ_ERROR_OCCURED;
            }
            catch (NoDocumentFoundException)
            {
                return PosMessage.NO_DOCUMENT_FOUND;
            }
            catch (DateOutOfRangeException)
            {
                return PosMessage.DATE_OUTOFRANGE;
            }
            catch (ZNoOutOfRangeException)
            {
                return PosMessage.Z_OUTOFRANGE;
            }
            catch (DocumentNoOutOfRangeException)
            {
                return PosMessage.DOCUMENT_OUTOFRANGE;
            }
            catch (AddressOutOfRangeException)
            {
                return PosMessage.ADDRESS_OUTOFRANGE;
            }
            catch (ParameterRelationException)
            {
                return PosMessage.FIRST_PARAMETER_BIGGER_LAST_ONE;
            }
            catch (ExcessiveIntervalException)
            {
                return PosMessage.EXCESSIVE_PARAMETER_INTERVAL;
            }
            catch (TimeLimitException)
            {
                return PosMessage.TIME_LIMIT_ERROR;
            }
            catch (TimeZReportException)
            {
                return PosMessage.TIME_ZREPORT_ERROR;
            }
            catch (NoProperZFound)
            {
                return PosMessage.NO_PROPER_Z_FOUND;
            }
            catch (ParameterException)
            {
                return PosMessage.PARAMETER_EXCEPTION;
            }
            catch (DocumentIdNotSetException)
            {
                return PosMessage.DOCUMENT_ID_NOTSET_ERROR;
            }
            catch (UnfixedSlipException)
            {
                return PosMessage.UNFIXED_SLIP_ERROR;
            }
            catch (SlipRowCountExceedException)
            {
                return PosMessage.SLIP_ROWCOUNT_EXCEED_ERROR;
            }
            catch (ReceiptRowCountExceedException)
            {
                return PosMessage.RECEIPT_ROWCOUNT_EXCEED_ERROR;
            }
            catch (RequestSlipException)
            {
                return PosMessage.REQUEST_SLIP_ERROR;
            }
            /***/

            catch (NoSlipPortException)
            {
                return PosMessage.NO_SLIP_PORT;
            }
            /***/
            catch (NegativeCoordinateException)
            {
                return PosMessage.NEGATIVE_COORDINATE_ERROR;
            }
            catch (CustomerTaxCoordinateException)
            {
                return PosMessage.CUSTOMER_TAX_COORDINATE_ERROR;
            }
            catch (CustomerTimeCoordinateException)
            {
                return PosMessage.CUSTOMER_TIME_COORDINATE_ERROR;
            }
            catch (TimeTaxCoordinateException)
            {
                return PosMessage.TIME_TAX_COORDINATE_ERROR;
            }
            catch (CustomerDateCoordinateException)
            {
                return PosMessage.CUSTOMER_DATE_COORDINATE_ERROR;
            }
            catch (DateTaxCoordinateException)
            {
                return PosMessage.DATE_TAX_COORDINATE_ERROR;
            }
            catch (CoordinateOutOfInvoiceException)
            {
                return PosMessage.COORDINATE_OUTOF_INVOICE_ERROR;
            }
            catch (NameVATCoordinateException)
            {
                return PosMessage.NAME_VAT_COORDINATE_ERROR;
            }
            catch (AmountVATCoordinateException)
            {
                return PosMessage.AMOUNT_VAT_COORDINATE_ERROR;
            }
            catch (ProductCoordinateException)
            {
                return PosMessage.PRODUCT_COORDINATE_ERROR;
            }
            catch (SlipException)
            {
                return PosMessage.SLIP_ERROR;
            }
            catch (InvalidOperationException)
            {
                return PosMessage.INVALID_OPERATION;
            }
            catch (ArithmeticException)
            {
                return PosMessage.LIMIT_EXCEED_ERROR;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return PosMessage.OFFICE_PATH_ERROR;
            }
            catch (ProductNotWeighableException)
            {
                return PosMessage.FRACTIONAL_QUANTITY_NOT_ALLOWED;
            }
            catch (ProductNotFoundException)
            {
                return PosMessage.PRODUCT_NOTFOUND;
            }
            catch (SerialNumberNotExistException)
            {
                return PosMessage.SERIAL_NUMBER_NOT_FOUND;
            }
            catch (BarcodeNotFoundException)
            {
                return PosMessage.BARCODE_NOTFOUND;
            }
            catch (ListingException)
            {
                return PosMessage.LISTING_ERROR;
            }
            catch (InvalidProgramException)
            {
                return PosMessage.INVALID_PROGRAM;
            }
            catch (ReceiptLimitExceededException)
            {
                return PosMessage.RECEIPT_LIMIT_EXCEEDED;
            }
            catch (VoidException)
            {
                return PosMessage.CANNOT_VOID_NO_PROPER_SALE;
            }
            catch (NoAdjustmentException)
            {
                return PosMessage.NO_ADJUSTMENT_EXCEPTION;
            }
            catch (AdjustmentLimitException ale)
            {
                return String.Format("{0} {1}", ale.Name, PosMessage.INSUFFICIENT_LIMIT);
            }
            catch (CardMismatchException)
            {
                return PosMessage.CARD_MISMATCH;
            }
            catch (MissingCardInfoException)
            {
                return PosMessage.MISSING_CARD_INFO;
            }
            catch (UpdateCardException)
            {
                return PosMessage.CONFIRM_UPDATE_POINTS;
            }
            catch (ContactlessCardException)
            {
                return PosMessage.CONTACTLESS_CARD_ERROR;
            }
            catch (NoCorrectionException)
            {
                return PosMessage.NO_CORRECTION_EXCEPTION;
            }
            catch (NoEJAreaException)
            {
                return PosMessage.NO_EJ_AREA_FOR_OPERATION;
            }
            catch (ProcessRejectedException)
            {
                return PosMessage.PROCESS_REJECTED;
            }
            catch (EftTimeoutException)
            {
                return PosMessage.EFT_TIMEOUT_ERROR;
            }
            catch (EftPosException)
            {
                if (String.IsNullOrEmpty(ex.Message))
                    return PosMessage.EFT_POS_ERROR;
                else
                    return ex.Message;
            }
            catch (BackOfficeUnavailableException)
            {
                return PosMessage.NO_CONNECTION_WITH_BACKOFFICE;
            }
            catch (ProductPromotionLimitExeedException)
            {
                return PosMessage.EXCEED_PRODUCT_LIMIT;
            }
            catch (InvalidPaymentException)
            {
                return PosMessage.PAYMENT_INVALID;
            }
            catch (InvalidSecurityKeyException)
            {
                return PosMessage.INVALID_SECURITY_KEY_EXCEPTION;
            }
            //catch (ZRequiredException)
            //{
            //    return PosMessage.Z_REQUIRED_GET_Z;
            //}
            catch (CashierAutorizeException)
            {
                return PosMessage.CASHIER_AUTHO_EXC;
            }
            //catch (TimeoutException)
            //{
            //    return PosMessage.TIME_OUT_ERROR;
            //}
            catch (Exception exc)
            {
                if (exc.Message == exc.Message.ToUpper() && exc.Message.Length <= 41)
                    return exc.Message;
                return PosMessage.UNDEFINED_EXCEPTION;
            }
        }
    }
}
