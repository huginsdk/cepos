using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime;

namespace Hugin.POS.Common
{
    public class PosException : Exception
    {
        public int ErrorCode;

        public PosException()
            : base("Pos exception occured") { }

        public PosException(String message)
            : base(message) { }

        public PosException(String message, Exception innerException)
            : base(message, innerException) { }

#if WindowsCE
        private Dictionary<String,String> data = new Dictionary<string,string>();

        public Dictionary<String, String> Data
        {
            get { return data; }
        }

#endif
    }
    #region Settings Exception

    public class SettingsException : PosException
    {
        public SettingsException()
            : base("Pos exception occured") { }

        public SettingsException(String message)
            : base(message) { }

        public SettingsException(String message, Exception innerException)
            : base(message, innerException) { }

    }
    public class SaleClosedException : PosException
    {
        public SaleClosedException() { }

        public SaleClosedException(string message)
            : base(message) { }

    }

    public class ReceiptLimitExceededException : PosException
    {
        public ReceiptLimitExceededException() { }

        public ReceiptLimitExceededException(string message)
            : base(message) { }
    }

    public class VoidException : PosException
    {
        public VoidException() { }

        public VoidException(string message)
            : base(message) { }
    }
    public class NoCorrectionException : PosException
    {
        /// <summary>
        /// Only create instance
        /// </summary>
        public NoCorrectionException() { }
    }

    public class ConfigurationFileNotFoundException : SettingsException
    {
        public ConfigurationFileNotFoundException()
            : base("Configuration file does not exists") { }

        public ConfigurationFileNotFoundException(String message)
            : base(message) { }

        public ConfigurationFileNotFoundException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DataInvalidException : SettingsException
    {
        public DataInvalidException()
            : base("Configuration file does not exists") { }

        public DataInvalidException(String message)
            : base(message) { }

        public DataInvalidException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class AuthorizitionNotDefinedException: SettingsException
    {
        public AuthorizitionNotDefinedException()
            : base(PosMessage.AUTH_NOT_DEFINED) { }

        public AuthorizitionNotDefinedException(String message)
            : base(message) { }

        public AuthorizitionNotDefinedException(String message, Exception innerException)
            :base(message, innerException) { }
    }

    public class BackOfficeUnavailableException : Exception
    {
        public BackOfficeUnavailableException()
            : base("Back office connection is not available") { }

        public BackOfficeUnavailableException(String message)
            : base(message) { }

        public BackOfficeUnavailableException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    #endregion Settings Exception

    #region Printer Exceptions

    public class PrinterException : PosException
    {
        public PrinterException()
            : base("Printer exception occured") { }

        public PrinterException(String message)
            : base(message) { }

        public PrinterException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class NoReceiptRollException : PrinterException
    {
        public NoReceiptRollException()
            : base("Receipt roll is not available") { }

        public NoReceiptRollException(String message)
            : base(message) { }

        public NoReceiptRollException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ReceiptSaleCountExceededException : PrinterException
    {
        public ReceiptSaleCountExceededException()
            : base("Max receipt sale count exceeded") { }

        public ReceiptSaleCountExceededException(String message)
            : base(message) { }

        public ReceiptSaleCountExceededException(String message, Exception innerException)
            : base(message, innerException) { }

    }
    
    public class ClearRequiredException : PrinterException
    {
        public ClearRequiredException()
            : base("Receipt roll is not available") { }

        public ClearRequiredException(String message)
            : base(message) { }

        public ClearRequiredException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class NoJournalRollException : PrinterException
    {
        public NoJournalRollException()
            : base("Journal roll is not available") { }

        public NoJournalRollException(String message)
            : base(message) { }

        public NoJournalRollException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class TimeoutOnConnectPrinterException : PrinterException
    {
        public TimeoutOnConnectPrinterException()
            : base("Printer timeout exception on connecting.") { }

        public TimeoutOnConnectPrinterException(String message)
            : base(message) { }

        public TimeoutOnConnectPrinterException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FMNewException : PrinterException
    {
        public FMNewException ()
            : base("FM is new") { }

        public FMNewException (String message)
            : base(message) { }

        public FMNewException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FMLimitWarningException : PrinterException
    {
        public FMLimitWarningException()
            : base("Fiscal memory has less than 50 lines") { }

        public FMLimitWarningException(String message)
            : base(message) { }

        public FMLimitWarningException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FMFullException : PrinterException
    {
        public FMFullException()
            : base("Fiscal memory is full") { }

        public FMFullException(String message)
            : base(message) { }

        public FMFullException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FMInitializeException : PrinterException
    {
        public FMInitializeException()
            : base("FM could not initialize") { }

        public FMInitializeException(String message)
            : base(message) { }

        public FMInitializeException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FMFiscalizeException : PrinterException
    {
        public FMFiscalizeException()
            : base("FM could not fiscalize") { }

        public FMFiscalizeException(String message)
            : base(message) { }

        public FMFiscalizeException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FMSAMCardException : PrinterException
    {
        public FMSAMCardException()
            : base("Could not get certificate SAM card info") { }

        public FMSAMCardException(String message)
            : base(message) { }

        public FMSAMCardException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class PrinterStatusException : PrinterException
    {
        public PrinterStatusException()
            : base("Could not be get response from FPU") { }

        public PrinterStatusException(String message)
            : base(message) { }

        public PrinterStatusException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class RFUException : PrinterException
    {
        public RFUException()
            : base("RFU Exception") { }

        public RFUException(String message)
            : base(message) { }

        public RFUException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class OpenShutterException : PrinterException
    {
        public OpenShutterException()
            : base("Shutter is open") { }

        public OpenShutterException(String message)
            : base(message) { }

        public OpenShutterException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class PrinterOfflineException : PrinterException
    {
        public PrinterOfflineException()
            : base("Printer is not available") { }

        public PrinterOfflineException(String message)
            : base(message) { }

        public PrinterOfflineException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FramingException : PrinterException
    {
        public FramingException()
            : base("Frame exception") { }

        public FramingException(String message)
            : base(message) { }

        public FramingException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ChecksumException : PrinterException
    {
        public ChecksumException()
            : base("Check sum exception") { }

        public ChecksumException(String message)
            : base(message) { }

        public ChecksumException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class UndefinedFunctionException : PrinterException
    {
        public UndefinedFunctionException()
            : base("Undefined function exception") { }

        public UndefinedFunctionException(String message)
            : base(message) { }

        public UndefinedFunctionException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class UndefinedVATRateException : UndefinedFunctionException
    {
        public UndefinedVATRateException()
            : base("Undefined VAT Rate exception") { }

        public UndefinedVATRateException(String message)
            : base(message) { }

        public UndefinedVATRateException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class UndefinedDepartmentException : UndefinedFunctionException
    {
        public UndefinedDepartmentException()
            : base("Undefined VAT Rate exception") { }

        public UndefinedDepartmentException(String message)
            : base(message) { }

        public UndefinedDepartmentException(String message, Exception innerException)
            : base(message, innerException) { }
    }


    public class DocumentTypeException : PrinterException
    {
        public DocumentTypeException()
            : base("Document type exception") { }

        public DocumentTypeException(String message)
            : base(message) { }

        public DocumentTypeException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class CmdSequenceException : PrinterException
    {
        public CmdSequenceException()
            : base("Command sequence exception") { }

        public CmdSequenceException(int lastCommand)
            : base("Command sequence exception")
        {
            this.lastCommand = lastCommand;
        }

        public CmdSequenceException(String message)
            : base(message) { }

        public CmdSequenceException(String message, Exception innerException)
            : base(message, innerException) { }

        public int LastCommand
        {
            get { return lastCommand; }
        }
        int lastCommand;
    }

    public class NegativeResultException : PrinterException
    {
        public NegativeResultException()
            : base("Negative result exception") { }

        public NegativeResultException(String message)
            : base(message) { }

        public NegativeResultException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class PowerFailureException : PrinterException
    {
        public PowerFailureException()
            : base("Power failure exception") { }

        public PowerFailureException(int lastCommand)
            : base("Power failure exception")
        {
            this.lastCommand = lastCommand;
        }

        public PowerFailureException(String message)
            : base(message) { }

        public PowerFailureException(String message, Exception innerException)
            : base(message, innerException) { }

        public int LastCommand
        {
            get { return lastCommand; }
        }
        int lastCommand;
    }

    public class EntryException : PrinterException
    {
        public EntryException()
            : base("Entry exception") { }

        public EntryException(String message)
            : base(message) { }

        public EntryException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class LimitExceededOrZRequiredException : PrinterException
    {
        public LimitExceededOrZRequiredException()
            : base("Z report is required") { }

        public LimitExceededOrZRequiredException(String message)
            : base(message) { }

        public LimitExceededOrZRequiredException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class PluLimitExceededException : PrinterException
    {
        public PluLimitExceededException()
            : base("PLU Limit exceeded") { }

        public PluLimitExceededException(String message)
            : base(message) { }

        public PluLimitExceededException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class BlockingException : PrinterException
    {
        public BlockingException()
            : base("Printer has been blocked") { }

        public BlockingException(String message)
            : base(message) { }

        public BlockingException(String message, Exception innerException)
            : base(message, innerException) { }

    }
    public class FiscalCommException : BlockingException
    {
        public FiscalCommException()
            : base("Fiscal memory connection is open") { }

        public FiscalCommException(String message)
            : base(message) { }

        public FiscalCommException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FiscalUndefinedException : BlockingException
    {
        public FiscalUndefinedException()
            : base("Fiscal memory is undefined.") { }

        public FiscalUndefinedException(String message)
            : base(message) { }

        public FiscalUndefinedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FiscalMismatchException : BlockingException
    {
        public FiscalMismatchException()
            : base("Fiscal memory is mismatch.") { }

        public FiscalMismatchException(String message)
            : base(message) { }

        public FiscalMismatchException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FiscalClosedException : BlockingException
    {
        public FiscalClosedException()
            : base("Fiscal memory is closed.") { }

        public FiscalClosedException(String message)
            : base(message) { }

        public FiscalClosedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CoverOpenedException : BlockingException
    {
        public CoverOpenedException()
            : base("Covers opened") { }

        public CoverOpenedException(String message)
            : base(message) { }

        public CoverOpenedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FMMeshDamagedException : BlockingException
    {
        public FMMeshDamagedException()
            : base("Fiscal Memory mesh damaged") { }

        public FMMeshDamagedException(String message)
            : base(message) { }

        public FMMeshDamagedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class HUBMeshDamagedException : BlockingException
    {
        public HUBMeshDamagedException()
            : base("HUB mesh damaged") { }

        public HUBMeshDamagedException(String message)
            : base(message) { }

        public HUBMeshDamagedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class EJNotFoundException : BlockingException
    {
        public EJNotFoundException()
            : base("Set EJ and Restart") { }

        public EJNotFoundException(String message)
            : base(message) { }

        public EJNotFoundException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CertificateDownloadException : BlockingException
    {
        public CertificateDownloadException()
            : base("Certificates could not be downloaded") { }

        public CertificateDownloadException(String message)
            : base(message) { }

        public CertificateDownloadException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DateTimeException : BlockingException
    {
        public DateTimeException()
            : base("Set date and time") { }

        public DateTimeException(String message)
            : base(message) { }

        public DateTimeException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DailyMemAndFMMismatchException : BlockingException
    {
        public DailyMemAndFMMismatchException()
            : base("Daily Memory and FM mismatch") { }

        public DailyMemAndFMMismatchException(String message)
            : base(message) { }

        public DailyMemAndFMMismatchException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DatabaseMismatchException : BlockingException
    {
        public DatabaseMismatchException()
            : base("DB mismatch") { }

        public DatabaseMismatchException(String message)
            : base(message) { }

        public DatabaseMismatchException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class IncorrectLogException : BlockingException
    {
        public IncorrectLogException()
            : base("Incorrect Log") { }

        public IncorrectLogException(String message)
            : base(message) { }

        public IncorrectLogException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class SRAMErrorException : BlockingException
    {
        public SRAMErrorException()
            : base("SRAM Error") { }

        public SRAMErrorException(String message)
            : base(message) { }

        public SRAMErrorException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CertificateMismatchException : BlockingException
    {
        public CertificateMismatchException()
            : base("Mismatch certificates") { }

        public CertificateMismatchException(String message)
            : base(message) { }

        public CertificateMismatchException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class VersionErrorException : BlockingException
    {
        public VersionErrorException()
            : base("Version Error") { }

        public VersionErrorException(String message)
            : base(message) { }

        public VersionErrorException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DailyLogLimitExceededException : BlockingException
    {
        public DailyLogLimitExceededException()
            : base("Daily Log Limit Exceeded") { }

        public DailyLogLimitExceededException(String message)
            : base(message) { }

        public DailyLogLimitExceededException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ServiceRequiredException : BlockingException
    {
        public ServiceRequiredException()
            : base("Service Required") { }

        public ServiceRequiredException(String message)
            : base(message) { }

        public ServiceRequiredException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class RestartECRException : BlockingException
    {
        public RestartECRException()
            : base("Restart ECR") { }

        public RestartECRException(String message)
            : base(message) { }

        public RestartECRException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class SVCPasswordOrPointException : PrinterException
    {
        public SVCPasswordOrPointException()
            : base("Service password is wrong") { }

        public SVCPasswordOrPointException(String message)
            : base(message) { }

        public SVCPasswordOrPointException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class LowBatteryException : PrinterException
    {
        public LowBatteryException()
            : base("Battery is low") { }

        public LowBatteryException(String message)
            : base(message) { }

        public LowBatteryException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class BBXNotBlankException : PrinterException
    {
        public BBXNotBlankException()
            : base("BBX is not blank") { }

        public BBXNotBlankException(String message)
            : base(message) { }

        public BBXNotBlankException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class BBXFormatFaliureException : PrinterException
    {
        public BBXFormatFaliureException()
            : base("BBX format is wrong") { }

        public BBXFormatFaliureException(String message)
            : base(message) { }

        public BBXFormatFaliureException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class BBXDirectoryException : PrinterException
    {
        public BBXDirectoryException()
            : base("BBX Directory  exception") { }

        public BBXDirectoryException(String message)
            : base(message) { }

        public BBXDirectoryException(String message, Exception innerException)
            : base(message, innerException) { }

    }


    public class MissingCashierException : PrinterException
    {
        public MissingCashierException()
            : base("Cashier is not assigned") { }

        public MissingCashierException(String message)
            : base(message) { }

        public MissingCashierException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class AssignedCashierLimitExeedException : PrinterException
    {
        public AssignedCashierLimitExeedException()
            : base("Cashier id is bigger than max id") { }

        public AssignedCashierLimitExeedException(String message)
            : base(message) { }

        public AssignedCashierLimitExeedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CashierAlreadyAssignedException : PrinterException
    {
        public CashierAlreadyAssignedException()
            : base("Cashier is already assigned") { }

        public CashierAlreadyAssignedException(String message)
            : base(message) { }

        public CashierAlreadyAssignedException(String message, String cashierId)
            : base("Cashier is already assigned")
        {
            this.cashierId = cashierId;
        }

        public CashierAlreadyAssignedException(String message, Exception innerException)
            : base(message, innerException) { }

        public String CashierId
        {
            get { return cashierId; }
        }
        String cashierId;

    }

    public class AlreadyFiscalizedException : PrinterException
    {
        public AlreadyFiscalizedException()
            : base("Printer is already fiscalized") { }

        public AlreadyFiscalizedException(String message)
            : base(message) { }

        public AlreadyFiscalizedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DTGException : PrinterException
    {
        public DTGException()
            : base("DTG exception") { }

        public DTGException(String message)
            : base(message) { }

        public DTGException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class FiscalIdException : PrinterException
    {
        public FiscalIdException()
            : base("Fiscal id does not match") { }

        public FiscalIdException(String message)
            : base(message) { }

        public FiscalIdException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class PrinterTimeoutException : PrinterException
    {
        public PrinterTimeoutException()
            : base("Printer timeout exception") { }

        public PrinterTimeoutException(String message)
            : base(message) { }

        public PrinterTimeoutException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ZReportUnsavedException : PrinterException
    {
        public ZReportUnsavedException()
            : base("Z report could not be saved") { }

        public ZReportUnsavedException(String message)
            : base(message) { }

        public ZReportUnsavedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class IncompleteXReportException : PrinterException
    {
        public IncompleteXReportException()
            : base("X report could not be completed") { }

        public IncompleteXReportException(String message)
            : base(message) { }

        public IncompleteXReportException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class IncompleteEJSummaryReportException : PrinterException
    {
        public IncompleteEJSummaryReportException()
            : base("EJ summary report could not be completed") { }

        public IncompleteEJSummaryReportException(String message)
            : base(message) { }

        public IncompleteEJSummaryReportException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class IncompleteReportException : PrinterException
    {
        public Byte[] Detail;
        public IncompleteReportException(byte[] detail)
            : base("Report could not be completed")
        {
            this.Detail = detail;
        }

        public IncompleteReportException(String message)
            : base(message) { }

        public IncompleteReportException(String message, Exception innerException)
            : base(message, innerException) { }

    }

     public class OrderServerNoMatcedDevice : PrinterException
     {
         public OrderServerNoMatcedDevice()
             : base("EJ summary report could not be completed") { }

         public OrderServerNoMatcedDevice(String message)
             : base(message) { }

         public OrderServerNoMatcedDevice(String message, Exception innerException)
             : base(message, innerException) { }

     }

    public class DocumentNotEmptyException : PrinterException
    {
        public DocumentNotEmptyException()
            : base("Printer has an open document") { }

        public DocumentNotEmptyException(String message)
            : base(message) { }

        public DocumentNotEmptyException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class PrintDocumentException : PrinterException
    {
        public PrintDocumentException()
            : base("Printing document could not be completed") { }

        public PrintDocumentException(String message)
            : base(message) { }

        public PrintDocumentException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ZRequiredException : PrinterException
    {
        public ZRequiredException()
            : base("Z Report must be taken") { }

        public ZRequiredException(String message)
            : base(message) { }

        public ZRequiredException(String message, Exception innerExcepiton)
            : base(message, innerExcepiton) { }
    }

    public class IncompletePaymentException : PrinterException
    {
        public IncompletePaymentException()
            : base("Printing document could not be completed") { }

        public IncompletePaymentException(String message)
            : base(message) { }

        public IncompletePaymentException(String message, Exception innerException)
            : base(message, innerException) { }

        public IncompletePaymentException(decimal difference)
            : base("Payment is not completed")
        {
            this.difference = difference;
        }

        public decimal Difference
        {
            get { return difference; }
        }
        decimal difference;

    }

    public class SubtotalNotMatchException : PrinterException
    {
        public SubtotalNotMatchException()
            : base("Subtotal does not match") { }

        public SubtotalNotMatchException(decimal difference)
            : base("Subtotal does not match")
        {
            this.difference = difference;
        }

        public SubtotalNotMatchException(String message)
            : base(message) { }

        public SubtotalNotMatchException(String message, Exception innerException)
            : base(message, innerException) { }

        public decimal Difference
        {
            get { return difference; }
        }
        decimal difference;
    }
    #endregion Printer Exceptions

    #region Slip Exceptions

    public class SlipException : PosException
    {
        public SlipException()
            : base("Slip exception occured") { }

        public SlipException(String message)
            : base(message) { }

        public SlipException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class NoSlipPortException : PosException
    {
        public NoSlipPortException()
            : base(PosMessage.NO_SLIP_PORT)
        {
        }
    }
    public class DocumentIdNotSetException : SlipException
    {
        public DocumentIdNotSetException()
            : base("Slip has only lines to write subtotal") { }

        public DocumentIdNotSetException(String message)
            : base(message) { }

        public DocumentIdNotSetException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class UnfixedSlipException : SlipException
    {
        public UnfixedSlipException()
            : base("Slip is not fixed") { }

        public UnfixedSlipException(Decimal subtotal)
            : base("Slip is not fixed")
        {
            this.subtotal = subtotal;
        }
        public UnfixedSlipException(String message)
            : base(message) { }

        public UnfixedSlipException(String message, Exception innerException)
            : base(message, innerException) { }

        public Decimal Subtotal
        {
            get { return subtotal; }
        }
        Decimal subtotal;

    }

    public class SlipRowCountExceedException : SlipException
    {
        public SlipRowCountExceedException()
            : base("Slip row has been finished") { }

        public SlipRowCountExceedException(String message)
            : base(message) { }

        public SlipRowCountExceedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class RequestSlipException : SlipException
    {
        public RequestSlipException()
            : base("Slip has only lines to write subtotal") { }

        public RequestSlipException(String message)
            : base(message) { }

        public RequestSlipException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class NegativeCoordinateException : SlipException
    {
        public NegativeCoordinateException()
            : base("Coordinates can not be smaller than zero") { }

        public NegativeCoordinateException(String message)
            : base(message) { }

        public NegativeCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CustomerTaxCoordinateException : SlipException
    {
        public CustomerTaxCoordinateException()
            : base("Customer and tax coordinates are interferenced") { }

        public CustomerTaxCoordinateException(String message)
            : base(message) { }

        public CustomerTaxCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CustomerTimeCoordinateException : SlipException
    {
        public CustomerTimeCoordinateException()
            : base("Customer and time coordinates are interferenced") { }

        public CustomerTimeCoordinateException(String message)
            : base(message) { }

        public CustomerTimeCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class TimeTaxCoordinateException : SlipException
    {
        public TimeTaxCoordinateException()
            : base("Tax and time coordinates are interferenced") { }

        public TimeTaxCoordinateException(String message)
            : base(message) { }

        public TimeTaxCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CustomerDateCoordinateException : SlipException
    {
        public CustomerDateCoordinateException()
            : base("Customer and date coordinates are interferenced") { }

        public CustomerDateCoordinateException(String message)
            : base(message) { }

        public CustomerDateCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DateTaxCoordinateException : SlipException
    {
        public DateTaxCoordinateException()
            : base("Tax and date coordinates are interferenced") { }

        public DateTaxCoordinateException(String message)
            : base(message) { }

        public DateTaxCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CoordinateOutOfInvoiceException : SlipException
    {
        public CoordinateOutOfInvoiceException()
            : base("Coordinates exceeds the slip(invoice)") { }

        public CoordinateOutOfInvoiceException(String message)
            : base(message) { }

        public CoordinateOutOfInvoiceException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class NameVATCoordinateException : SlipException
    {
        public NameVATCoordinateException()
            : base("Name and vat coordinates are interferenced") { }

        public NameVATCoordinateException(String message)
            : base(message) { }

        public NameVATCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class AmountVATCoordinateException : SlipException
    {
        public AmountVATCoordinateException()
            : base("Amount and vat coordinates are interferenced") { }

        public AmountVATCoordinateException(String message)
            : base(message) { }

        public AmountVATCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ProductCoordinateException : SlipException
    {
        public ProductCoordinateException()
            : base("Product Y (line) coordinate is too small") { }

        public ProductCoordinateException(String message)
            : base(message) { }

        public ProductCoordinateException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class NoSlipPrinterOnCOM : SlipException
    {
        public NoSlipPrinterOnCOM()
            : base("Any connected slip printer on COM PORT") { }

        public NoSlipPrinterOnCOM(String message)
            : base(message) { }

        public NoSlipPrinterOnCOM(String message, Exception innerException)
            : base(message, innerException) { }
    }
    #endregion Slip Exceptions

    #region Electronic Journal Exceptions

    public class EJException : PosException
    {
        public EJException()
            : base("Electronic journal exception") { }

        public EJException(String message)
            : base(message) { }

        public EJException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class EJIdMismatchException : EJException
    {
        public EJIdMismatchException()
            : base("Ej id does not match") { }

        public EJIdMismatchException(String message)
            : base(message) { }

        public EJIdMismatchException(String message, String ejId)
            : base(message)
        {
            this.ejId = ejId;
        }

        public EJIdMismatchException(String message, Exception innerException)
            : base(message, innerException) { }

        public String EjId
        {
            get { return ejId; }
        }
        String ejId;

    }

    public class EJFiscalIdMismatchException : EJException
    {

        public EJFiscalIdMismatchException()
            : base("Ej fiscal id does not match") { }

        public EJFiscalIdMismatchException(String message)
            : base(message) { }

        public EJFiscalIdMismatchException(String message, String fiscalId)
            : base(message)
        {
            this.fiscalId = fiscalId;
        }

        public EJFiscalIdMismatchException(String message, Exception innerException)
            : base(message, innerException) { }

        public String FiscalId
        {
            get { return fiscalId; }
        }
        String fiscalId;

    }

    public class EJFullException : EJException
    {
        public EJFullException()
            : base("Ej is full") { }

        public EJFullException(String message)
            : base(message) { }

        public EJFullException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class EJBitmapException : EJException
    {
        public EJBitmapException()
            : base("Ej bitmap exception") { }

        public EJBitmapException(String message)
            : base(message) { }

        public EJBitmapException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class EJFormatException : EJException
    {
        public EJFormatException()
            : base("Ej is not formatted") { }

        public EJFormatException(String message)
            : base(message) { }

        public EJFormatException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class EJCommException : EJException
    {
        public EJCommException()
            : base("Ej is not available") { }

        public EJCommException(String message)
            : base(message) { }

        public EJCommException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class EJChangedException : EJException
    {

        public EJChangedException()
            : base("Ej id has been changed") { }

        public EJChangedException(String message)
            : base(message) { }

        public EJChangedException(String message, String ejId, String prevEJ)
            : base(message)
        {
            this.ejId = ejId;
            this.previousEJ = prevEJ;
        }

        public EJChangedException(String message, Exception innerException)
            : base(message, innerException) { }

        public String EJId
        {
            get { return ejId; }
        }
        public String PreviousEJ
        {
            get { return previousEJ; }
        }
        String ejId;
        String previousEJ;

    }

    public class EJZLimitSettingException : EJException
    {

        public EJZLimitSettingException()
            : base("Zlimit can not be set bigger than default value.") { }

        public EJZLimitSettingException(String message)
            : base(message) { }

        public EJZLimitSettingException(long zLimit)
            : base("Zlimit can not be set bigger than default value.")
        {
            this.zLimit = zLimit;
        }

        public EJZLimitSettingException(String message, Exception innerException)
            : base(message, innerException) { }

        public long ZLimit
        {
            get { return zLimit; }
        }
        long zLimit;

    }

    public class EJWaitingForInitException : EJException
    {
        public EJWaitingForInitException()
            : base("Ej is initializing...") { }

        public EJWaitingForInitException(String message)
            : base(message) { }

        public EJWaitingForInitException(String message, Exception innerException)
            : base(message, innerException) { }

    }
    public class NoEJAreaException : PosException
    {
        public NoEJAreaException()
            : base("No ej area to do this operation")
        {
        }
        public NoEJAreaException(String msg)
            : base(msg)
        {
        }
    }
    public class NoZReportInEJException : ParameterException
    {
        public NoZReportInEJException()
            : base("Ej does not contain any z report") { }

        public NoZReportInEJException(String message)
            : base(message) { }

        public NoZReportInEJException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class EJLimitWarningException : EJException
    {

        public EJLimitWarningException()
            : base("Warning : EJ has a few usable memory") { }

        public EJLimitWarningException(String message)
            : base(message) { }

        public EJLimitWarningException(long usage)
            : base("Warning : EJ has a few usable memory")
        {
            this.usage = usage;
        }

        public EJLimitWarningException(String message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// the percentage of memory used
        /// </summary>
        public long Usage
        {
            get { return usage; }
        }
        long usage;

    }

    #endregion  Electronic Journal Exceptions

    #region Parameter Exceptions

    public class ParameterException : PosException
    {
        public ParameterException()
            : base("Parameter exception occured") { }

        public ParameterException(String message)
            : base(message) { }

        public ParameterException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class NoDocumentFoundException : ParameterException
    {
        public NoDocumentFoundException()
            : base("No document having search criteria is found") { }

        public NoDocumentFoundException(String message)
            : base(message) { }

        public NoDocumentFoundException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DateOutOfRangeException : ParameterException
    {
        public DateOutOfRangeException()
            : base("Printer does not contains such date ") { }

        public DateOutOfRangeException(String message)
            : base(message) { }

        public DateOutOfRangeException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ZNoOutOfRangeException : ParameterException
    {
        public ZNoOutOfRangeException()
            : base("Printer does not contains such z number ") { }

        public ZNoOutOfRangeException(String message)
            : base(message) { }

        public ZNoOutOfRangeException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class DocumentNoOutOfRangeException : ParameterException
    {
        public DocumentNoOutOfRangeException()
            : base("Printer does not contains such document number ") { }

        public DocumentNoOutOfRangeException(String message)
            : base(message) { }

        public DocumentNoOutOfRangeException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class AddressOutOfRangeException : ParameterException
    {
        public AddressOutOfRangeException()
            : base("Printer does not contains such z number ") { }

        public AddressOutOfRangeException(String message)
            : base(message) { }

        public AddressOutOfRangeException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ParameterRelationException : ParameterException
    {
        public ParameterRelationException()
            : base("Parameter sequence is not proper") { }

        public ParameterRelationException(String message)
            : base(message) { }

        public ParameterRelationException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ExcessiveIntervalException : ParameterException
    {
        public ExcessiveIntervalException()
            : base("Parameter interval too long to report") { }

        public ExcessiveIntervalException(String message)
            : base(message) { }

        public ExcessiveIntervalException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class TimeLimitException : ParameterException
    {
        public TimeLimitException()
            : base("Time change can not be bigger than 1 hour") { }

        public TimeLimitException(String message)
            : base(message) { }

        public TimeLimitException(String message, Exception innerException)
            : base(message, innerException) { }

    }
    public class TimeZReportException : ParameterException
    {
        public TimeZReportException()
            : base("Time change can not exceed last z report") { }

        public TimeZReportException(String message)
            : base(message) { }

        public TimeZReportException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class ReceiptBeginningException : ParameterException
    {
        public ReceiptBeginningException()
            : base("Ej documet printing is beginning") { }

        public ReceiptBeginningException(String message)
            : base(message) { }

        public ReceiptBeginningException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class NoProperZFound : PosException
    {
        public NoProperZFound()
            : base("No z report found with your specification") { }

        public NoProperZFound(String message)
            : base(message) { }

        public NoProperZFound(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class LineLengthException : ParameterException
    {
        public LineLengthException()
            : base("Line length is more than expected") { }

        public LineLengthException(String message)
            : base(message) { }

        public LineLengthException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidVATRateException : ParameterException
    {
        public InvalidVATRateException()
            : base("Invalid VAT Rate") { }

        public InvalidVATRateException(String message)
            : base(message) { }

        public InvalidVATRateException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidDepartmentNoException : ParameterException
    {
        public InvalidDepartmentNoException()
            : base("Invalid Department Id") { }

        public InvalidDepartmentNoException(String message)
            : base(message) { }

        public InvalidDepartmentNoException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidPLUNoException : ParameterException
    {
        public InvalidPLUNoException()
            : base("Invalid PLU Number") { }

        public InvalidPLUNoException(String message)
            : base(message) { }

        public InvalidPLUNoException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidNameException : ParameterException
    {
        public InvalidNameException()
            : base("Invalid definition (product name, dept name, credit name...)") { }

        public InvalidNameException(String message)
            : base(message) { }

        public InvalidNameException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidBarcodeException : ParameterException
    {
        public InvalidBarcodeException()
            : base("Invalid barcode") { }

        public InvalidBarcodeException(String message)
            : base(message) { }

        public InvalidBarcodeException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidOptionException : ParameterException
    {
        public InvalidOptionException()
            : base("Invalid option") { }

        public InvalidOptionException(String message)
            : base(message) { }

        public InvalidOptionException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidQuantityException : ParameterException
    {
        public InvalidQuantityException()
            : base("Invalid quantity") { }

        public InvalidQuantityException(String message)
            : base(message) { }

        public InvalidQuantityException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class InvalidAmountException : ParameterException
    {
        public InvalidAmountException()
            : base("Invalid amount") { }

        public InvalidAmountException(String message)
            : base(message) { }

        public InvalidAmountException(String message, Exception innerException)
            : base(message, innerException) { }
    }
    #endregion Parameter Exceptions

    #region Promotion Exceptions

    public class PromotionException : System.Exception
    {
        public PromotionException()
            : base("ÖDEME ÞEKLi\nGEÇERSiZ") { }
        public PromotionException(String message)
            : base(message) { }
    }
    #endregion  Promotion Exceptions

    #region PointDB Exceptions

    public class PointDBException : System.Exception
    {
        public PointDBException()
            : base("Veri\nHatasý") { }
        public PointDBException(String message)
            : base(message) { }
    }

    public class CustomerNotInPointDBException : PointDBException
    {
        public CustomerNotInPointDBException()
            : base(PosMessage.CUSTOMER_NOTIN_POINT_DB) { }
        public CustomerNotInPointDBException(String message)
            : base(message) { }
    }
    public class CardSerialNotInPointDBException : PointDBException
    {
        public CardSerialNotInPointDBException()
            : base(PosMessage.CARDSERIAL_NOTIN_POINT_DB) { }
        public CardSerialNotInPointDBException(String message)
            : base(message) { }
    }

    public class UpdatePointException : PointDBException
    {
        public UpdatePointException()
            : base(PosMessage.FAIL_ON_POINT_UPDATE) { }
        public UpdatePointException(String message)
            : base(message) { }
    }

    public class CardSerialInsertException : PointDBException
    {
        public CardSerialInsertException()
            : base(PosMessage.FAIL_ON_CARDSERIAL_INSERT) { }
        public CardSerialInsertException(String message)
            : base(message) { }
    }

    public class CardSerialAlreadyExistsException : PointDBException
    {
        public CardSerialAlreadyExistsException()
            : base(PosMessage.FAIL_ON_POINT_UPDATE) { }
        public CardSerialAlreadyExistsException(String message)
            : base(message) { }
    }

    #endregion PointDB Exceptions

    #region  ContactlessCard Exceptions

    public class ContactlessCardException : System.Exception
    {
        public ContactlessCardException()
            : base(PosMessage.CONTACTLESS_CARD_ERROR) { }
        public ContactlessCardException(String message)
            : base(message) { }
    }

    public class UpdateCardException : ContactlessCardException
    {
        public UpdateCardException()
            : base(PosMessage.CONFIRM_UPDATE_POINTS) { }
        public UpdateCardException(String message)
            : base(message) { }
    }

    public class MissingCardInfoException : ContactlessCardException
    {
        public MissingCardInfoException()
            : base(PosMessage.MISSING_CARD_INFO) { }
        public MissingCardInfoException(String message)
            : base(message) { }
    }
    
    public class CardMismatchException : ContactlessCardException
    {
        public CardMismatchException()
            : base(PosMessage.CARD_MISMATCH) { }
        public CardMismatchException(String message)
            : base(message) { }
    }

    
    #endregion  ContactlessCard Exceptions

    #region Eft Pos Exceptions
    public class EftPosException : System.Exception
    {
        public EftPosException()
            : base(PosMessage.EFT_POS_ERROR) { }
        public EftPosException(String message)
            : base(message) { }
    }

    public class EftTimeoutException : EftPosException
    {
        public EftTimeoutException()
            : base(PosMessage.EFT_TIMEOUT_ERROR) { }
        public EftTimeoutException(String message)
            : base(message) { }
    }

    public class ProcessRejectedException : EftPosException
    {
        public ProcessRejectedException()
            : base(PosMessage.PROCESS_REJECTED) { }
        public ProcessRejectedException(String message)
            : base(message) { }
    }

    public class AnyConnectedEftPosException : EftPosException
    {
        public AnyConnectedEftPosException()
            : base(PosMessage.ANY_CONNECTED_EFT_POS) { }
        public AnyConnectedEftPosException(String message)
            : base(message) { }
    }

    #endregion

    #region FPU Exceptions

    public class FPUException : PrinterException
    {
        public FPUException ()
            : base("FPU exception occured") { }

        public FPUException (String message)
            : base(message) { }

        public FPUException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class CashierAutorizeException : PosException
    {
        public CashierAutorizeException()
            : base("Cashier has not access right") { }

        public CashierAutorizeException(String message)
            : base(message) { }

        public CashierAutorizeException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ExternalDevMatchException : FPUException
    {
        public ExternalDevMatchException()
            : base(PosMessage.CANNOT_MATCH_EXT_DEV) { }

        public ExternalDevMatchException(String message)
            : base(message) { }

        public ExternalDevMatchException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class EcrNonFiscalException : FPUException
    {
        public EcrNonFiscalException()
            : base("Exception occur when matching with external device") { }

        public EcrNonFiscalException(String message)
            : base(message) { }

        public EcrNonFiscalException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    public class UndefinedTaxRateException : FPUException
    {
        public UndefinedTaxRateException()
            : base("Undefined tax rate") { }

        public UndefinedTaxRateException(String message)
            : base(message) { }

        public UndefinedTaxRateException(String message, Exception innerException)
            : base(message, innerException) { }
    }

    
    #endregion

    #region Sale Exception

    public class ProductNotFoundException : System.Exception
    {
        public ProductNotFoundException() : base(Common.PosMessage.PRODUCT_NOTFOUND) { }
        public ProductNotFoundException(string message)
            : base(message) { }
    }
    public class InvalidPaymentException : System.Exception
    {
        public InvalidPaymentException() : base(Common.PosMessage.PAYMENT_INVALID) { }
        public InvalidPaymentException(string message)
            : base(message) { }
    }
    /// <summary>
    /// Occurs while adjustment
    /// </summary>
    public class AdjustmentException : System.Exception
    {
        /// <summary>
        /// Only create instance
        /// </summary>
        public AdjustmentException() { }
        /// <summary>
        /// instance with a special message
        /// </summary>
        /// <param name="message"></param>
        public AdjustmentException(string message)
            : base(message) { }
    }

    public class InvalidCorrectionException : AdjustmentException
    {
        public InvalidCorrectionException()
            : base("Cannot do correction operation") { }

        public InvalidCorrectionException(String message)
            : base(message) { }
    }

    public class CannotAdjustmenException : AdjustmentException
    {
        public CannotAdjustmenException()
            : base("Cannot do adjustment operation") { }

        public CannotAdjustmenException(String message)
            : base(message) { }
    }


    public class ReceiptRowCountExceedException : PosException
    {
        public ReceiptRowCountExceedException()
            : base("Receipts consists of too many lines") { }

        public ReceiptRowCountExceedException(String message)
            : base(message) { }

        public ReceiptRowCountExceedException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class SaleNotFoundException : System.Exception
    {
        public SaleNotFoundException() : base(Common.PosMessage.SALE_NOT_FOUND) { }
        public SaleNotFoundException(string message)
            : base(message) { }
    }

    public class InvalidSaleException : InvalidOperationException
    {
        public InvalidSaleException()
            : base("Invalid Sale Operation") { }

        public InvalidSaleException(String message)
            : base(message) { }

        public InvalidSaleException(String message, Exception innerException)
            : base(message, innerException) { }

    }

    public class InvalidVoidException : InvalidOperationException
    {
        public InvalidVoidException()
            : base("Invalid Void Operation") { }

        public InvalidVoidException(String message)
            : base(message) { }

        public InvalidVoidException(String message, Exception innerException)
            : base(message, innerException) { }

    }
    #endregion
}
