using System;

namespace Hugin.POS
{
    
    public class NoAdjustmentException : System.Exception
    {
        /// <summary>
        /// Only create instance
        /// </summary>
        public NoAdjustmentException() { }
    }

    public class AdjustmentLimitException : System.Exception
    {
        /// <summary>
        /// Only create instance
        /// </summary>
        String name = "IND/ART";
        public AdjustmentLimitException() {}
        /// <summary>
        ///instance with a special name
        /// </summary>
        /// <param name="name"></param>
        public AdjustmentLimitException(String name)
        {
            this.name = name;
        }
        public String Name
        {
            get { return name; }
        }
    }

    public class InvalidProductFileException : System.Exception
    {
        public InvalidProductFileException() { }

        public InvalidProductFileException(string message)
            : base(message) { }
    }
   
    public class InvalidSettingsFileException : System.Exception
    {
        public InvalidSettingsFileException() { }

        public InvalidSettingsFileException(string message)
            : base(message) { }
    }
   
    public class InvalidCashierFileException : System.Exception
    {
        public InvalidCashierFileException() { }

        public InvalidCashierFileException(string message)
            : base(message) { }
    }
  
    public class InvalidCurrencyFileException : System.Exception
    {
        public InvalidCurrencyFileException() { }

        public InvalidCurrencyFileException(string message)
            : base(message) { }
    }
   
    public class InvalidCustomerFileException : System.Exception
    {
        public InvalidCustomerFileException() { }

        public InvalidCustomerFileException(string message)
            : base(message) { }
    }

    public class ProductNotWeighableException : System.Exception
    {
        public ProductNotWeighableException() : base(Common.PosMessage.FRACTIONAL_QUANTITY_NOT_ALLOWED) { }
        public ProductNotWeighableException(string message)
            : base(message) { }
    }


    public class SerialNumberNotExistException : System.Exception
    {
        public SerialNumberNotExistException() : base(Common.PosMessage.SERIAL_NUMBER_NOT_FOUND) { }
        public SerialNumberNotExistException(string message)
            : base(message) { }
    }

    class WrongZCountException : System.Exception
    {
        public WrongZCountException(String message)
            : base(message) { }
    }

    class DataChangedException : System.Exception
    {
        public DataChangedException()
            : base("BELGE VARKEN\nDATA DEGÝÞMÝÞ")
        {
        }
    }

    class DocumentSuspendException : System.Exception
    {
        public DocumentSuspendException()
            : base("ASKIYA ALMA HATALI\nBELGE ÝPTAL OLDU")
        {
        }
        public DocumentSuspendException(Exception e)
            : base("ASKIYA ALMA HATALI\nBELGE ÝPTAL OLDU", e)
        {
        }
    }
    public class ListingException : System.Exception
    {
        public ListingException():base(Common.PosMessage.LISTING_ERROR) { }

        public ListingException(string message)
            : base(message) { }
    }

    public class BarcodeNotFoundException : System.Exception
    {
        public BarcodeNotFoundException() : base(Common.PosMessage.BARCODE_NOTFOUND) { }

        public BarcodeNotFoundException(string message)
            : base(message) { }
    }

    public class OutofQuantityLimitException : System.Exception
    {
        public OutofQuantityLimitException() : base(Common.PosMessage.OUTOF_QUANTITY_LIMIT) { }

        public OutofQuantityLimitException(string message)
            : base(message) { }
    }

    public class InvalidQuantityException : System.Exception
    {
        public InvalidQuantityException() : base(Common.PosMessage.INVALID_QUANTITY) { }

        public InvalidQuantityException(string message)
            : base(message) { }
    }

    public class ProductPromotionLimitExeedException : System.Exception
    {
        public ProductPromotionLimitExeedException() : base(Common.PosMessage.EXCEED_PRODUCT_LIMIT) { }
        public ProductPromotionLimitExeedException(string message)
            : base(message) { }
    }

    public class InvalidSecurityKeyException : System.Exception
    {
        public InvalidSecurityKeyException() : base(Common.PosMessage.INVALID_SECURITY_KEY_EXCEPTION) { }
        public InvalidSecurityKeyException(string message)
            : base(message) { }
    }
}
