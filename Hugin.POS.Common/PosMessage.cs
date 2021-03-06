
//#define lang_en
//#define lang_hu

using System;
using System.Collections.Generic;
using System.Text;
namespace Hugin.POS.Common
{
#if lang_en
 public static class PosMessage
 {
    #region LOG AND ALERT
 public const string INVALID_OPERATION = "PROCESS ERROR";
 public const string INVALID_PASSWORD= "PASSWORD INCORRECT";
 public const string PROMPT_RETRY = "TRY AGAIN";
 public const string REGISTER_LOCKED = "POS IS LOCKED";
 public const string ENTER_PASSWORD = "ENTER PASSWORD";
 public const string ACCESS_LEVEL = "ACCESS LEVEL";
 public const string NO_ACCESS_RIGHT = "NOT AUTHORIZED";
 public const String AUTHORIZED_PASSWORD = "AUTHORIZED PASSWORD";
 public const string UNDEFINED_KEY = "IDENTIFICATION KEY";
 public const string PRODUCT_FILE_LOAD_ERROR= "PRODUCT DOSSIER\nERROR FORMED";
 public const string PROGRAM_FILE_LOAD_ERROR = "PROGRAM FILE\nERROR FORMED";
 public const string CUSTOMER_FILE_LOAD_ERROR= "CUSTOMER FOLDER\nERROR FORMED";
 public const string CASHIER_FILE_LOAD_ERROR = "CASHIER FOLDER\nERROR FORMED";
 public const string CURRENCY_FILE_LOAD_ERROR = "CURRENCY EXCHANGE FOLDER\nERROR FORMED";
 public const string PROGRAM_UNLOADED = "PROGRAM INFORMATION\nNOT LOADED";
 public const string PASSWORD_INVALID = "INCORRECT PASSWORD";
 public const string CASHIER_ALREADY_ASSIGNED_EXCEPTION = "CASHIER ENTRY\nALREADY MADE";
 public const string CASHIER_LOGOUT = "CASHIER\nLOGOUT";
 public const String CASHIER_LOGIN_REQUIRED = "CASHIER LOGIN\nREQUIRED";
 public const string CASHIER= "CASHIER";
 public const string STARTUP_MESSAGE = "POS SETTINGS\nLOADING ..."; 
 public const String ENTER_MANAGER_ID = "MANAGER ID";
 public const String INVALID_MANAGER = "INVALID MANAGER";
 public const String INVALID_MANAGER_ID = "INVALID MANAGER ID";
 public const String ENTER_PASS_MNGR = "MANAGER PASSWORD";
 public const string SEND_ORDER = "SEND ORDER";
 public const String ECR_IS_FISCAL = "ECR IS FISCAL";
 public const String INVALID_PASSWORD_TRY_AGAIN = "INVALID PASS\nTRY AGAIN PLS";
 public const String SUCCESS_PROCESS = "SUCCESS PROCESS";
 public const String FPU_SETTINGS_CHECK = "ECR SETTINGS\nCHECKING...";

        public const String MANAGER = "MANAGER";
        public const String NO_DEFINED_MANAGER = "ANY DEFINED MANAGER";
        public const String DEFAULT_LOGIN = "DEFAULT LOGIN?";
        public const String CASHIER_NOT_FOUND = "CASHIER NOT FOUND";
        public const String SWITCH_MANAGER = "SWITCH MANAGER";

    #endregion
    #region SALES
 public const string NOT_SELLING = "NO SALES";
 public const string ENTER_NUMBER = "NUMBER ENTRY";
 public const string ENTER_AMOUNT = "ENTER AMOUNT";
 public const string ENTER_UNIT_PRICE = "UNIT PRICE ENTRY";
 public const string PRODUCT_LIMIT_EXEED = "PRODUCT LIMIT\nEXEED";
 public const string SERIAL_NUMBER_NOT_FOUND = "SERIAL NUMBER\nNOT FOUND";
 public const string PRODUCT_NOTFOUND = "PRODUCT NOT FOUND";
 public const string RECEIVE_PAYMENT = "PAYMENT TO BE";
 public const string LAST_PAYMENT = "{0} MAXIMUM PAYMENT\nREMAINDER";
 public const string PROMPT_FINALIZE_SALE = "SALES TURN OFF";
 public const string BARCODE_NOTFOUND = "BARCODE\n NOT FOUND";
 public const string PLU_NOTFOUND = "PLU NOT FOUND";
 public const string BARCODE_ERROR= "ERROR CODE";
 public const string BARCODE_WEIGHT_ERROR = "ERROR CODE WEIGHT";
 public const string BARCODE_QUANTITY_ERROR= "ERROR CODE NUMBER";
 public const string BARCODE_TOTAL_AMOUNT_ERROR = "WITHOUT ERROR CODE";
 public const string RECEIPT_LIMIT_EXCEEDED = "LIMIT RECEIPT LOWLY";
 public const string TRANSFER_TO_INVOICE = "TRANSFER TO INVOICE";
 public const string PRODUCT_PRICE_NOTFOUND = "PRODUCT PRICE NO";
 public const string INVALID_PRODUCT = "PRODUCT INFORMATION WRONG";
 public const string UNKNOWN_ERROR= "ERROR FORMED";
 public const string PRICE_LOOKUP = "PRICE LOOK";
 public const string UNIT_PRICE = "UNIT PRICE";
 public const string SUBTOTAL = "SUBTOTAL";
 public const string DISCOUNTED = "DISCOUNT";
 public const String REDUCTION = "REDUCTION";
 public const string CLERK_ID = "CLERK CODE";
 public const string SELECT_PRODUCT= "SELECT PRODUCT";
 public const string REGISTER_CLOSED = "POS CLOSED";
 public const string PLEASE_WAIT= "PLEASE WAIT ...";
 public const string TOTAL_AMOUNT = "TOTAL AMOUNT";
 public static String WELCOME = "STORE\n WELCOME";
 public static String WELCOME_LOCKED = "STORE\n WELCOME";
 public const string CLERK = "CLERK";
 public const string PRODUCT = "PRODUCT";
 public const string VAT_DISTRIBUTION = "TAX DISTRIBUTION";
 public const string ZERO_PLU_PRICE_ERROR = "PRODUCT PRICE\nCAN NOT BE ZERO";
 public const string ZERO_DRAWER_IN_ERROR = "AMOUNT OF CASH ENTRY\nSIFIR EXCEED";
 public const string ZERO_DRAWER_OUT_ERROR = "AMOUNT OF CASH OUT\nSIFIR EXCEED";
 public const string ENTER_PRODUCT_PRICE = "PRODUCT PRICE INPUT";
 public const string ENTER_PRODUCT_SERIALNO = "ENTER PRODUCT SERIAL";
 public const string SELLING_VAT = "VAT";
 public const string DOCUMENT_TOTAL_AMOUNT_LIMIT_EXCEED_ERROR = "DOCUMENT AMOUNT\nLIMIT EXCEEDED";
 public const string ITEM_TOTAL_AMOUNT_LIMIT_EXCEED_ERROR= "ITEM AMOUNT\nLIMIT EXCEEDED";
 public const string VOID_AMOUNT_INVALID = "VOID AMOUNT\nINVALID";
 public const string CANNOT_VOID_NO_PROPER_SALE = "NO MATCHING SALE\nVOID INVALID";
 public const string DECIMAL_LIMIT = "DECIMAL LIMIT";
 public const string NO_SALE_INVALID_ACTION = "NO SALES\nINVALID OPERATION";
 public const string INSUFFICIENT_LIMIT = "LIMIT\nINSUFFICIENT";
 public const string VOID_SALESPERSON = "CLERK CANCEL?";
 public const string UNDEFINED_LABEL = "IDENTIFICATION LABEL";
 public const string NOT_ENOUGH_CHARS_FOR_PRICECHECK= "AT LEAST FOR PRICES\n2 SHIFT ENTER";
 public const string WAIT_DOCUMENT_TRANSFER = "PLEASE WAIT\nTRANSFER HELD ...";
 public const string CLERK_FOR_ITEM = "CLERK (PRODUCT)\n";
 public const string CLERK_FOR_DOCUMENT = "CLERK (TOTAL)\n";
 public const string GAINS = "GAINS";
 public const string MIN_AMOUNT_ERROR = "MIN AMOUNT\n MUST BE0,01";
 public const string SERIAL_NUMBER_ALREADY_EXIST = "SERIAL NUMBER IS\nALREADY EXIST";
 public const string SALE_FROM_QR_CODE = "SALE FROM\nQR CODE";
 public const string SALE_NOT_FOUND = "VALID SALE\nNOT FOUND";
 public const string VAT_RATE_DIFFERENT = "VAT RATES\nMISMATCH";
 public const string VAT_RATE_UPDATING = "VAT RATES\nUPDATING..";
 public const string CREDITS_DIFFERENT = "CREDITS VALUE\nDIFFERENT";
 public const string CREDITS_UPDATING = "CREDIT VALUES\nUPDATING..";
 public const string END_OF_RECEIPT_NOTE_SAVING = "END OF RECEIPT\nNOTES SAVING..";
    #endregion
    #region ELECTRONICJOURNALERROR
        public const string NO_EJ_AREA_FOR_OPERATION = "NO_EJ_AREA\nFOR_OPERATION";
 public const string FIX_VALID_EJ_TO_VOID_DOCUMENT = "FIX_VALID_EJ\nTO_VOID_DOCUMENT";
 public const string CONFIRM_NEW_EJ_FORMAT = "CONFIRM_NEW\nEJ_FORMAT";
 public const string ZREPORT_NECCESSARY_FOR_NEW_EJ = "ZREPORT_NECCESSARY\nFOR_NEW_EJ";
 public const string NO_ZREPORT_IN_EJ = "NO_ZREPORT\nIN_EJ";
 public const string CONFIRM_ZREPORT_ON_FULL_EJ = "CONFIRM_ZREPORT\nON_FULL_EJ";
 public const string EJ_PASIVE_ONLY_EJ_REPORTS = "EJ_PASSIVE_ONLY\nEJ_REPORTS";
 public const string EJ_PASIVE_VALID_EJ_REQUIRED = "EJ_PASSIVE\nVALID_EJ_REQUIRED";
 public const string CONNECTING_TO_PRINTER= "CONNECTING_TO\nPRINTER ...";
    #endregion ELECTRONICJOURNALERROR
    #region DOCUMENTS
 public const string SELECT_DOCUMENT= "SELECT DOCUMENT";
 public const string TRANSFER_DOCUMENT = "TRANSFER DOCUMENTS";
 public const string RECEIPT = "RECEIPT";
 public const string RECEIPT_TR = "RCPT";
 public const string WAYBILL = "WAYBILL";
 public const string WAYBILL_TR = "WAYBILL";
 public const string INVOICE = "INVOICE";
 public const string RETURN_DOCUMENT = "REFUND";
 public const string RETURN_DOCUMENT_TR = "RETURN";
 public const String E_INVOICE = "E-INVOICE";
 public const String E_ARCHIVE = "E-ARCHIVE";
 public const String MEAL_TICKET = "MEAL DOCUMENT";
 public const String CAR_PARKIMG = "CAR PARKING";
 public const String ADVANCE = "ADVANCE";
 public const String COLLECTION_INVOICE = "COLLECTION INVOICE";
 public const String CURRENT_ACCOUNT_COLLECTION = "CURRENT ACCOUNT COLLECTION";
 public const string HR_CODE_RETURN = "IAD";
 public const string HR_CODE_WAYBILL = "IRS";
 public const string HR_CODE_INVOICE = "FAT";
 public const string HR_CODE_RECEIPT = "FIS";
 public const String HR_INTER_CODE_RETURN = "GPS";
 public const String HR_CODE_E_INVOICE = "EFA";
 public const String HR_CODE_E_ARCHIVE = "EAR";
 public const String HR_CODE_MEAL_TICKET = "YEM";
 public const String HR_CODE_CAR_PARKING = "OTO";
 public const String HR_CODE_ADVANCE = "AVA";
 public const String HR_CODE_COLLECTION_INVOICE = "COL";
 public const String HR_CODE_CURRENT_ACCOUNT_COLLECTION = "CHT";

 public const string ORDER = "SIP";
 public const string ORDER_TR= "ORDER";
 public const string DOCUMENT_ID_NOT_FOUND = "DOCUMENT NO\n NOT FOUND";
 public const string CHANGE_DOCUMENT = "ACTIVE DOCUMENT\n {0} BE";
 public const string NOT_ENOUGH_MONEY_IN_REGISTER = "NOT_ENOUGH_MONEY\nIN_REGISTER";
 public const string TRANSFER_STARTED_PLEASE_WAIT= "BEGIN TRANSFER\nPLEASE WAIT ..";
 public const string NO_AUTHORIZATION_FOR_SPECIAL_PRICE_SALES = "COST SALES\nNO AUTHORIZATION";
 public const string RECEIPT_LIMIT_EXCEEDED_TRANSFER_DOCUMENT = "RECEIPT LIMIT_EXCEEDED\nTRANSFER DOCUMENT";
 public const string RECEIPT_LIMIT_DOCUMENT_TRANSFER_NOT_ALLOWED = "LIMIT RECEIPT \nNO TRANSFER";
 public const string FRACTIONAL_QUANTITY_NOT_ALLOWED = "FRACTIONAL QUANTITY\nNOT ALLOWED";
 public const string DEPOSITED_AMOUNT = "TRANSFERRED TO TOTAL";
 public const string COORDINATE_ERROR = "COORDINATES\nERROR";
 public const string DOCUMENT_CHANGE_ERROR = "DOCUMENT CHANGE\nERROR";
 public const string DOCUMENT_FOLLOWING_ID = "DOCUMENT FOLLOWING ID";
 public const string SLIP_SERIAL = "SERIAL (AA)";
 public const string SLIP_ORDER_NO = "ORDER NO(123456)";
 public const String RETURN_REASON = "RETURN REASON";
    #endregion DOCUMENTS
    #region DISCOUNT
 public const string DISCOUNT_LIMIT_EXCEEDED = "DISCOUNT LIMIT\nEXCEEDED";
 public const string DISCOUNT_NOT_ALLOWED = "DISCOUNT ALLOWED";
 public const string DISCOUNT = "DISCOUNT";
     public const string DNEY_PERCENTDISCOUNT = "FRACTIONAL DISCOUNT\nNOT ALLOWED";
 public const string DNEY_PERCENTFEE = "FRACTIONAL SURCHARGE\nNOT ALLOWED";
 public const string INSUFFICIENT_ACCESS_LEVEL = "INSUFFICIENT ACCESS";
 public const string PRODUCT_PERCENT_DISCOUNT = "DISCOUNT";
 public const string PRODUCT_PRICE_DISCOUNT = "DISCOUNT";
 public const string SUBTOTAL_PERCENT_DISCOUNT = "DISCOUNT";
 public const string SUBTOTAL_PRICE_DISCOUNT = "DISCOUNT";
 public const string DISCOUNT_ALLOWED = "DISCOUNT ALLOWED";
 public const string COUNT_ALLOWED = "SURCHARGE ALLOWED";
     public const string CORRECTION = "CORRECTION";
    #endregion
    #region FEE
     public const string FEE_LIMIT_EXCEEDED = "SURCHARGE LIMIT\nEXCEEDED";
 public const string PRODUCT_PERCENT_FEE = "SURCHARGE";
 public const string PRODUCT_PRICE_FEE = "SURCHARGE";
 public const string FEE = "SURCHARGE";
 public const string SUBTOTAL_PRICE_FEE = "SURCHARGE";
 public const string SUBTOTAL_PERCENT_FEE = "SURCHARGE";
 public const string FEE_NOT_ALLOWED = "SURCHARGE\nNOT ALLOWED";
    #endregion
    #region VOID
 public const string VOID_AMOUNT_EXCEEDED = "VOID AMOUNT\nEXCEEDED";
 public const string VOID_INVALID = "VOID INVALID";
 public const string CONFIRM_LOGOUT = "CONFIRM LOGOUT";
 public const string PROMPT_ENTER = "PRESS ENTER";
 public const string VOID = "VOID";
 public const string VOID_FIND_PRODUCT = "VOID \nPRODUCT FIND";
 public const string ENTER_LIST_FOR_SPECIAL_PRICED_PRODUCT_VOID = "ENTER LIST\nTO VOID PRODUCT";
 public const string CANNOT_ASSIGN_CLERK_TO_VOID_SALE = "CANNOT ASSIGN CLERK\nTO VOID SALE";
 public const string DOCUMENT_VOID_COUNT = "NUMBER OF VOID";
    #endregion
    #region RECEIPT
 public const string EXEMPT_TAX_TOTAL = "VAT EXEMPT TOTAL";
 public const string EXEMPT_TAX = "VAT EXEMPT";
 public const string TOTAL = "TOTAL";
 public const string TOTALTAX = "VAT";
 public const string TOTAL_BOLD = "² TOTAL ³";
 public const string SHORT_TOTAL_BOLD = "² TOT ³";
 public const string TOTALTAX_BOLD = "² VAT ³";
 public const string TAX_BOLD = "² VAT ³";
 public const string OTHER = "OTHER";
 public const string CASH_RECEIPT= "CASH COLLECTION";
 public const string CURRENCY_RECEIPT = "EXCHANGE PROVISION";
 public const string CREDIT_RECEIPT = "CREDIT COLLECTION";
 public const string CHECK_RECEIPT = "CHECH COLLECTION";
 public const string COLLECTION = "COLLECTION"; 
 public const string CURRENCY_CASH_EXCHANGE = "CASH EQUIVALENT.";
 public const string SALE= "SALES";
 public const string VAT = "VAT";
 public const string PLUS = "+";
 public const string MINUS = "-";
    #endregion
    #region MENU PROGRAM
 public const string PM_FISCALIZATION = "FINANCIAL OPENING";
 public const string PM_VERSION = "VERSION";
 public const string PM_REGISTER = "POS DESCRIPTION";
 public const string PM_DATETIME = "CLOCK SET";
 public const string PM_DATAFILES= "DATA FILES";
 public const string PM_REGISTERFILES = "POS FILES";
 public const string PM_HARDWARE = "HARDWARE CONNECTION";
 public const String PM_COMPORTSETTINGS = "COM PORT SETTINGS";
 public const string PM_RESETDISPLAY = "DISPLAY RESET";
 public const string PM_LOADBITMAP = "LOGO GRAPHIC UPLOAD";
 public const string PM_NEWPROGRAM = "NEW PROGRAM";
 public const string CONFIRM_LOAD_NEW_PROGRAM= "LOAD THE NEW\nPROGRAM? (ENTER)";
 public const string PROGRAM_DATA_LOAD_ERROR = "PROGRAM INFO\nMISSING OR WRONG";
 public const string PROGRAM_ERROR = "PROGRAM ERROR";
 public const string CURRENCY_LIMIT_EXCEEDED_PAYMENT_INVALID = "EXCHANGE LIMIT\nPAYMENT INVALID";
 public const string PM_BARCODE_TERMINATOR = "BARCODE CONFIG";
 public const string ENTER_BARCODE = "ENTER BARCODE";
 public const string CONFIRM_BARCODE_TERMINATOR = "BARCODE\nSEPARATOR? (ENTER)";
 public const string PM_BUZZER_ON_OFF = "BUZZER ON / OFF";
 public const string PM_BUZZER_ON = "BUZZER ON? (ENTER)";
 public const string PM_BUZZER_OFF = "BUZZER OFF? (ENTER)";
 public const String PM_TEST_EJ = "TEST EJ?";
 public const String WAITING_FOR_DEVICE_MATCHNG = "WAITING FOR\nDEVICE MATCHING..";
 public const String OP_ENDED = "OPERATION\nENDED";
 public const String MATCHING_FINISHED_ESCAPE = "MATCHING COMPLETED\n(QUIT?)";
 public const String PM_GMP_TEST_COMMAND = "GMP TEST COMMANDS";
 public const String PM_UPDATE_CATEGORY = "UPDATE CATEGORY";
 public const String UPDATİNG_CATEGORY = "CATEGORIES\nUPDATING..";
 public const String PM_NETWORK_SETTINGS = "NETWORK SETTINGS";
 public const String SAVE_SETTINGS = "SAVE SETTINGS";
 public const String SAVED_SUCCESFULLY = "SETTINGS SAVED\nSUCCESSFULLY!";
 public const String WAIT_FOR_MATCHING = "MATCHING..\nPLEASE WAIT";
 public const String MATCHED_SUCCESSFULL = "MATCHED\nSUCCESSFULLY";
 public const String PM_CONFIG_DATA = "CONFIG DATA";
 public const String PM_CONFIG_VAT_RATE = "CONFIG VAT RATE";
 public const String PM_CONFIG_DEPARTMENT = "CONFIG DEPARTMENT";
 public const String PM_CONFIG_PRODUCT = "CONFIG PRODUCT";
 public const String PM_CONFIG_LOGO = "CONFIG LOGO";
 public const String PM_CONFIG_CASHIER = "CONFIG CASHIER";
 public const String PM_CONFIG_MANAGER = "CONFIG MANAGER";
 public const String PM_CONFIG_GMP_PORT = "CONFIG GMP PORT";

 public const String PM_MATCH_EFT_POS = "MATCH EFT POS";
 public const String GMP_IP = "GMP IP";
 public const String GMP_PORT = "GMP PORT";
        public const String PORT = "PORT";

        public const String CASHIER_AUTHO_EXC = "LOW AUTHORAZATİON\nFOR FPU UPDATE";
    public const String RE_MATCHED_WİTH_FPU = "RE-MATCHED WITH\nFPU (ENTER)";
    public const String MANAGER_AUTHO_EXC = "MANAGER AUTH\nNOT ENOUGH";
    public const String TABLE_NUMBER = "TABLE NO";
    public const String CLOSE_TABLE = "CLOSE TABLE";
    public const String CONFIRM_SEND_ORDER = "SEND ORDER?";
    public const String MOVE_TO_EFT_POS_SIDE = "PLEASE MOVE TO\nEFT-POS SIDE..";
    public const String PRESS_ENTER_TO_CONNECT = "PRESS ENTER\nTO CONNECT";
    public const String FILE_NAME = "FILE NAME";
    public const String PM_FILE_TRANSFER = "FILE TRANSFER";
    public const String MENU_UPDATE_FIRMWARE = "UPDATE FIRMWARE";
        public const String CREATE_DB = "CREATE DB\nCONFIRM (ENTER)";
        public const String START_FM_TEST = "START\nFM TEST (ENTER)";
        public const String CLOSE_FISCAL_MEMORY = "CLOSE\nFM\t(ENTER)";
    #endregion
    #region PAYMENT
        public const string BALANCE = "BALANCE";
 public const string CASH ="CASH";
 public const string FOREIGNCURRENCY = "EXCHANGE";
 public const string TURKISH_LIRA= "TL";
 public const string CHECK = "CHECK";
 public const string CREDIT= "CREDIT";
 public const string PAYMENT= "PAYMENT";
 public const string PAYMENT_INVALID = "PAYMENT INVALID";
 public const string NO_SALE_PAYMENT_INVALID = "NO SALES\nPAYMENT INVALID";
 public const string CHECK_ID = "CHECK NUMBER";
 public const string INSTALLMENT_COUNT = "INSTALLMENTS";
 public const String INSTALLMENT = "INSTALLMENT";
 public const string PAYMENT_INFO = "PAYMENT DETAILS";
 public const string CHANGE = "CHANGE";
 public const String PAYMENT_STARTED = "PAYMENT STARTED";
        public const String PAID_IS_DONE_TY = "PAID IS DONE\nTHANKS";
        public const String WRITING_XPLU_REPORT_PLEASE_WAIT = "PRINTING X PLU\nPLEASE WAIT";
    #endregion
    #region CALCULATOR
        public const string CALCULATOR= "CALCULATOR";
 public const string ENTER_CALCULATOR = "CALCULATOR INPUT";
 public const string EXIT_CALCULATOR = "EXIT CALCULATOR";
 public const string CONFIRM_EXIT_CALCULATOR = "CONFIRM EXIT\nCALCULATOR (ENTER)";

    #endregion
    #region CUSTOMER
     public const string CUSTOMER_NOT_FOUND = "CUSTOMER\nNOT FOUND";
     public const string ENTER_CADR_CODE = "CUSTOMER CARD/CODE\nENTRY";
     public const string SEARCH_RECORD = "CUSTOMER\n SEARCH";
     public const string NEW_RECORD = "CUSTOMER\n NEW ENTRY";
 public const string RETURN_TO_SELLING = "BACK TO SALE";
 public const string VOID_CUSTOMER = "CUSTOMER CANCEL";
 public const string CONFIRM_VOID_CUSTOMER = "CUSTOMER CANCEL?\n (ENTER)";
 public const string NAME_FIRM = "NAME / COMPANY";
 public const string CUSTOMER_GROUP = "CUSTOMER GROUP";
 public const string LEGATION = "EMBASSY / COUNTRY";
 public const string ADDRESS = "ADDRESS";
 public const string STREET = "STREET";
 public const string STREET_NO = "STREET / NO";
 public const string REGION_CITY = "COUNTY / CITY";
 public const string TAXOFFICE = "TAX OFFICE";
 public const string SEARCH_QUERY = "CONTAINING TEXT";
 public const string CUSTOMER_NUMBER = "CUSTOMER NUMBER";
 public const string IDENTITY_NUMBER= "ID NUMBER";
 public const string CUSTOMER_CODE = "CUSTOMER CODE";
 public const string CUSTOMER_POINT = "CUSTOMER POINTS";
 public const string CUSTOMER_DUTY = "POSITION";
 public const string TAX_NUMBER = "TAX NUMBER";
 public const string PROMOTION_LIMIT = "PERCENT REDUCTION";
 public const string DISCOUNT_LIMIT = "DISCOUNT RATE (%)";
 public const string END_OF_RECORD = "REGISTRATION\nCOMPLETED";
 public const string FATAL_ERROR_CUSTOMER_INFO_NOT_FOUND = "CRITICAL ERROR\nCUSTOMER INFORMATION NO"; // SPLIT
 public const string CUSTOMER_NOT_BE_CHANGED_IN_DOCUMENT = "SALE IN PROGRESS\nCANNOT CHANGE CUST.";
 public const string DOCUMENT_NOT_BE_TRANSFERRED = "DOCUMENT\n TRANSFERRED";
 public const string PROMPT_CLOSE_CASHREGISTER = "TURN OFF POS?\n (ENTER)";
 public const string TAX_INSTITUTION = "TAX OFFICE";
 public const string NAME = "NAME";
 public const string CONFIRM_VOID_CURRENT_CUSTOMER = "VOID ASSIGNED\nCUSTOMER?";
 public const string CONFIRM_TRANSFER_CUSTOMER_TO_RECEIPT = "ASSIGN CUSTOMER TO\nCURRENT DOCUMENT";
    #endregion
    #region LIST MESSAGES
 public const string END_OF_LIST = "END OF LIST";
 public const string START_OF_LIST = "START OF LIST";
 public const string LIST_EMPTY = "LIST EMPTY";
 public const string LISTING_ERROR = "LIST ERROR";
 public const string INVALID_PROGRAM = "PROGRAM FILES\nMISSING OR WRONG";
    #endregion
    #region PROCESS MENU
 // FOR SELLING STATE PROCESSES
 public const string VOID_DOCUMENT = "DOCUMENT VOID";
 public const string SUSPEND_DOCUMENT = "DOCUMENT HOLD";
 public const string CONFIRM_VOID_DOCUMENT = "DOCUMENT VOID\nCONFIRM? (ENTER)";
 public const string REPEAT_DOCUMENT = "DOCUMENT REPEAT";
 public const string RESUME_DOCUMENT = "PARKED DOCUMENTS";
 public const string PARKED_DOCUMENT = "PARKED DOCUMENT";
 public const string ORDERED_DOCUMENT = "ORDERED DOCUMENT";
 public const string ORDERS = "ORDERS";
 public const string FAST_PAYMENT = "FAST PAYMENTS";
 public const string ENTER_CASH = "CASH ENTRY";
 public const string RECEIVE_CASH = "CASH OUT";
 public const string RECEIVE_CHECK = "CHECK OUT";
 public const string RECEIVE_CREDIT = "LOAN OUT";
 public const string COMMAND_CALCULATOR = "CALCULATOR";
 public const string FIRST_DOCUMENT = "FIRST DOC (OPTIONAL)";
 public const string LAST_DOCUMENT = "FINAL DOC (OPTIONAL)";
 public const string DOCUMENT_ID = "DOCUMENT NUMBER";
 public const string CONFIRM_SUSPEND_DOCUMENT = "DOCUMENT HOLD\nSUSPEND? (ENTER)";
 public const string CASH_AMOUNT= "INSERT AMOUNT";
 public const string SUSPENDED_DOCUMENT = "DOCUMENT ON HOLD";
 
    #endregion
    #region MENU REPORT
 public const string X_REPORT = "X REPORT";
 public const String X_PLU_REPORT = "X PLU REPORT";
 public const string Z_REPORT = "Z REPORT";
 public const string END_DAY_REPORT = "END OF DAY REPORT";
 public const string FINANCIAL_BETWEEN_Z = "FM REPORT(NUMBER)";
 public const string FINANCIAL_BETWEEN_DATE = "FM REPORT(DATE)";
 public const string PAYMENT_REPORT = "SALES REPORT";
 public const String PAYMENT_REPORT_CURRENT = "CURRENT";
 public const String PAYMENT_REPORT_DAILY = "DAILY";
 public const String PAYMENT_REPORT_DATE = "BETWEEN TWO DATE";
 public const String PAYMENT_REPORT_WITH_DETAIL = "DETAILED";
 public const String PAYMENT_REPORT_JUST_TOTALS = "ONLY TOTALS";
 public const string PROGRAM_REPORT = "PROGRAM INFO RPT";
 public const string EJ_SUMMARY_REPORT = "EJ DETAIL REPORT";
 public const string EJ_DOCUMENT_REPORT = "EJ SINGLE DOCUMENT";
 public const string EJ_DOCUMENT_DATE_TIME = "DATE & TIME";
 public const string EJ_DOCUMENT_Z_DOCID = "Z & RECEIPT NO NO";
 public const string EJ_DOCUMENT_ZREPORT = "Z REPORT REVIEW";
 public const string EJ_PERIODIC_REPORT = "EJ PERIODICAL";
 public const string EJ_PERIODIC_ZREPORT = "EJ BETWEEN 2 Z";
 public const string EJ_PERIODIC_FIRST_Z_NO = "FIRST Z [, RECEIPT]";
 public const string EJ_PERIODIC_LAST_Z_NO = "LAST Z [, RECEIPT]";
 public const string EJ_PERIODIC_DATE = "BETWEEN TWO DATE";
 public const string EJ_PERIODIC_DAILY = "DAILY";
 public const string EJ_LIMIT_SETTING = "EJ LIMIT SETUP";
 public const string CONFIRM_EJ_PERIODIC_REPORT = "EJ PERIODIC\nPRINT?";
 public const string WRITING_EJ_PERIODIC_REPORT = "EJ PERIODIC\nPRINTING...";
 public const string CONFIRM_EJ_DOCUMENT_REPORT = "EJ SINGLE DOCUMENT\nPRINT?";
 public const string WRITING_EJ_DOCUMENT_REPORT = "EJ SINGLE DOCUMENT\nPRINTING....";
 public const string CONFIRM_EJ_SUMMARY_REPORT = "EJ DETAIL REPORT\nPRINT?";
 public const string WRITING_EJ_PERIODIC_REPORT_BETWEEN_ZNO = "PERIODICAL Z\nPRINTING...";
 public const string WRITING_EJ_SUMMARY_REPORT = "EJ DETAIL REPORT\nPRINTING ..";
 public const string WRITE_XREPORT = "X-REPORT\nPRINT?";
 public const string WRITING_PAYMENT_REPORT = "SALES REPORT\nPRINTING ...";
 public const string WRITING_PROGRAM_REPORT = "PROGRAM INFO RPT\nPRINTING ...";
 public const string WRITING_XREPORT = "X-REPORT\nPRINTING ...";
 public const string WRITING_XREPORT_PLEASE_WAIT = "PRINTING X-REPORT\nPLEASE WAIT ...";
 public const string WRITING_CASHIER_REPORT = "PRINTING CASHIER-RPT\nPLEASE WAIT...";
 public const string WRITE_Z_REPORT = "Z-REPORT\nPRINT?";
 public const string WRITING_Z_REPORT = "Z-REPORT\nPRINTING ...";
 public const string Z_NO = "Z NO:";
 public const string FIRST_Z_NO = "NO FIRST Z:";
 public const string LAST_Z_NO = "NO RECENT Z:";
 public const string WRITE_FINANCIAL_Z_REPORT = "FM REPORT\nPRINT?";
 public const string WRITING_FINANCIAL_Z_REPORT = "FM REPORT\nPRINTING...";
 public const string FINANCIAL_Z_REPORT_INVALID_PARAMETER = "VALID RANGE Z\nENTER";
 public const string REPORT_FINISHED = "REPORT RECEIVED";
 public const string TIME = "TIME (HHMM)";
 public const string DATE = "DATE (DDMMYYYY)";
 public const string FIRST_DATE = "FIRST DATE(DDMMYYYY)";
 public const string LAST_DATE = "END DATE (DDMMYYYY)";
 public const string CONFIRM_PAYMENT_REPORT = "SALES REPORT\nPRINT ?";
 public const string CONFIRM_PROGRAM_REPORT = "PROGRAM INFO REPORT\nPRINT?";
 public const string INVALID_DATE_INPUT = "INCORRECT ENTRY DATE";
 public const string SALES_EXIST_REPORT_NOT_ALLOWED = "SALES EXIST\nREPORT NOT ALLOWED";
 public const string SALES_EXIST_COMMANDMENU_NOT_ALLOWED = "SALES EXIST. PROGRAM\nMENU INVALID";
 public const string INVALID_ENTRY = "INCORRECT INPUT";
 public const string NO_SUPPORTED_REPORT = "NO SUPPORTED\nREPORT";
 public const string CONFIRM_START_REPORT = "CONFIRM START REPORT";
 public const string REPORT_PROCESSING = "REPORT PROCESSING";
 public const string REPORT_STOPPING = "REPORT\nSTOPPING";
 public const String FIRST_PLU_NO = "FIRST PLU NO";
 public const String LAST_PLU_NO = "LAST PLU NO";
 public const String FINANCIAL_BETWEEN_Z_DETAIL = "FINANCIAL Z-Z DETAIL";
 public const String FINANCIAL_BETWEEN_DATE_DETAIL = "FINANCIAL DATE-DATE";
 public const String SYSTEM_INFO_REPORT = "SYSTEM INFO REPORT";
 public const String RECEIPT_TOTAL_REPORT = "RECEIPT TOTAL REPORT";
    #endregion
    #region EXCEPTION PRINTER MESSAGES
 public const string PRINTER_EXCEPTION = "PRINTER ERROR";
 public const string FRAMING_EXCEPTION = "FRAMING ERROR";
 public const string CHECK_SUM_EXCEPTION = "CHECK SUM ERROR";
 public const string UNDEFINED_FUNCTION_EXCEPTION = "IDENTIFICATION FUNCTION";
 public const string CMD_SEQUENCE_EXCEPTION = "INCORRECT OPERATION SEQUENCE";
 public const string NEGATIVE_RESULT_EXCEPTION = "NO SUFFICIENT AMOUNT\nAT POS";
 public const string POWER_FAIURE_EXCEPTION = "ELECTRICITY INTERRUPTION\nCANCEL DOCUMENT?";
 public const string UNFIXED_SLIP_EXCEPTION = "DOCUMENT NOT\nFIXED PROPERLY";
 public const string SLIP_COODINATE_EXCEPTION = "SLIP COORDINATE\n ERROR";
 public const string ENTRY_EXCEPTION = "SALES ERROR";
 public const string LIMIT_EXCEEDED_OR_ZREQUIRED_EXCEPTION = "Z REPORT REQUIRED\nBEFORE OPERATION";
 public const string BLOCKING_EXCEPTION = "BODY BLOCKED\nCALL TECH SERV.";
 public const string SVC_PASSWORD_OR_POINT_EXCEPTION = "SERVICE CODE\nERROR";
 public const string LOW_BATTERY_EXCEPTION = "INSUFFICIENT BATTERY";
 public const string BBX_NOT_BLANK_EXCEPTION = "BBX NOT NULL";
 public const string BBX_FORMAT_FAILURE_EXCEPTION = "FORMAT ERROR";
 public const string BBX_DIRECTORY_EXCEPTION = "BBX DIRECTORY ERROR";
 public const string MISSING_CASHIER_EXCEPTION = "CASHIER NO ENTRY";
 public const string ALREADY_FISCALIZED_EXCEPTION = "FINANCIAL FASHION\nCLICK BEFORE PASSED";
 public const string DTG_EXCEPTION = "TIME / DATE ERROR";
 public const string FATAL_ERROR_NO_CASHIER_INFO = "CRITICAL ERROR\nKASÝYER INFORMATION NO";
 public const string TIME_OUT_ERROR = "TIME OUT\nOLUÞTU";
 public const string FATAL_ERROR_PRODUCT_INFO_NOT_FOUND = "CRITICAL ERROR \nPRODUCT INFO NOT FND";
 public const string UNDEFINED_EXCEPTION = "UNDEFINED ERROR";
 public const string NULL_REFERENCE_EXCEPTION = "NULL REFERENCE\nEXCEPTION";
 public const string NOJOURNALROLL = "JOURNAL PAPER NO";
 public const string NORECEIPTROLL = "RECEIPT PAPER NO";
 public const string OFFLINE1 = "OFFLINE1 ERROR";
 public const string OPEN_SHUTTER = "PAPER COVER OPEN";
 public const string RFU_ERROR = "RFU ERROR";
 public const string MECHANICAL_FAILURE = "MECHANICAL ERROR";
 public const string PRINTER_OFFLINE = "PRINTER OFF";
 public const string PRINTER_CONNETTION_ERROR = "CANNOT REACH PRINTER";
 public const string FISCAL_ID_EXCEPTION = "FM\nMISMATCH";
 public const String FISCAL_COMM_EXCEPTION = "FM\nCOMMINUCATION ERROR";
 public const String FISCAL_MISMATCH_EXCEPTION = "FM\nMISMATCH";
 public const String FISCAL_UNDEFINED_EXCEPTION = "UNDEFINED FM";
 public const string SERVICE = "SERVICE";
 public const string MODE = "MODE";
 public const string CANNOT_ACCESS_PRINTER = "PRINTER\nUNAVAILABLE";
 public const string CANNOT_ACCESS_EJ = "EJ UNAVAILABLE";
 public const string COM_PORT_DEFECTIVE_CALL_SERVICE = "COM PORT FAULT\nCALL SERVICE";
 public const string PRINTER_TIMEOUT = "CHECK PRINTER\nAND CONNECTION";
 public const String CHECK_RECEIPT_ROLL = "CHECK RECEIPT\nROLL";
 public const string ZREPORT_UNSAVED = "Z RAPORU\nNOT SAVED";
 public const string INCOMPLETE_XREPORT = "INCOMPLETE\nX REPORT";
 public const string DOCUMENT_NOT_EMPTY = "DOCUMENT NOT EMPTY";
 public const string SUBTOTAL_NOT_MATCH = "SUBTOTAL MISMATCH\nW FISCAL SUBSYSTEM";
 public const string CAN_NOT_ACCESS_TO_DISPLAYS = "CANNOT ACCESS\nDISPLAYS";
 public const string CHECK_PRINTER = "CHECK PRINTER";
 public const string DOCUMENT_NOT_PRINTED = "DOCUMENT COULD NOT\nPRINTED";
 public const string DIFFERENT_CASHIER_ASSING_LIMIT_EXEED_EXCEPTION = "EXEED ASSIGNED\nCASHIER COUNT";
 public const String INCOMPLETE_PAYMENT = "INCOMPLETE PAYMENT";
 public const String INCOMPLETE_PAYMENT_AFTER_EFT_DONE = "EFT AUTHORIZED\nCOMPLETE PAYMENT";
 public const String FM_FULL_REGISTER_BLOCKED = "FM IS FULL\nSYSTEM BLOCKED";
 public const String FM_HAS_NO_AREA_ZREQUIRED = "FM HAS NO AREA\nZ REQUIRED";
     public const String CONTINUE_WRITING = "CONTINUES\nWRITING...";
    public const String Z_REQUIRED_GET_Z = "Z REQUIRED\nPLEASE TAKE (ENTER)";

    #endregion
    #region SLIP MESSAGES
 public const string PAPER_FOR_VOIDING_SLIP_SALE = "PAPER FOR\nVOIDING SLIP SALE";
 public const string CONTINUE_OR_VOIDING_SLIP_SALE = "ENTER:CONTINUE SALE\nESC:VOIDING SLIP";
 public const String CONTINUE_OR_VOIDING_SLIP_SALE_ON_ERROR = "VOID: CANCEL SLIP\nESC: CONTINUE SLIP";
 public const string NEW_SLIP_PAPER = " PUT\nPRESS ENTER";
 public const string PUT_IN = "PUT IN";
 public const string RECOVER_ERROR_AND_PRESS_ESC = "RECOVER ERROR\nAND PRESS ESC";
 public const string END_OF_SLIP_PAYMENT_NOT_ALLOWED = "END OF SLIP\nPAYMENT NOT ALLOWED";
 public const string RETURN_PAYMENTS = "RETURN PAYMENTS";
 public const String CONTINUE_SELLING = "CONTINUE SELLING";
 public const String PUT_PAPER_IN = "PUT PAPER\nIN PRINTER";
    #endregion
    #region SERVICE MESSAGES
 public const String MENU_LOGO = "LOGO";
 public const String MENU_VAT_RATES = "VAT RATES";
 public const String MENU_DAILY_MEMORY_FORMAT = "FORMAT DAILY MEMORY";
 public const String MENU_DATE_AND_TIME = "DATE-TIME";
 public const String MENU_CLOSE_FM = "CLOSE FM";
 public const String MENU_EXTERNAL_DEV_SETTINGS = "EXT DEVICE SETTINGS";
 public const String MENU_FACTORY_SETTING = "FACTORY SETTINGS";
 public const String MENU_CREATE_DB = "CREATE DB"; 
 public const String MENU_PRINT_LOGS = "PRINT LOGS";
 public const String MENU_EXIT_SERVICE = "EXIT SERVICE";
 public const string CLOSE_SERVICE = "CLOSE SERVICE";
 public const string LOAD_FACTOR_SETTINGS = "FACTORY SETTINGS\n(ENTER) LOAD";
 public const string SERVICE_PASSWORD = "SERVICE PASSWORD";
 public const string SERVICE_PASSWORD_INVALID = "INVALID\nSERVICE PASS";
 public const string ATTACH_JUMPER_AND_TRY_AGAIN = "ATTACH JUMPER\nTRY AGAIN";
 public const string RESTART_PRINTER_FOR_SERVICEMODE = "RESTART PRINTER\nFOR SERVICE MODE";
 public const string PROMPT_DAILY_MEMORY_FORMAT = "DAILY MEMORY\nFORMAT(ENTER)";
 public const string REMOVE_JUMPER_AND_TRY_AGAIN = "REMOVE JUMPER\nTRY AGAIN";
 public const string ATTACH_JUMPER_AND_RESTART_FPU = "ATTACH JUMPER\nRESTART ECR";
        public const String VAT_RATE = "VAT RATE";
    #endregion
    #region SETTINGS MESSAGES
 public const string INVALID_ADDRESS_TRY_AGAIN = "INVALID ADDRESS\nTRY AGAIN";
 public const string PROGRAM_VERSION = "VERSION";
 public const string BRANCH_ID = "BRANCH ID";
 public const string BRANCH_ID_MAX_LENGTH = "BRANCH ID\nMAX LENGTH: 3";
 public const string REGISTER_ID = "REGISTER NUMBER";
 public const string REGISTER_ID_MAX_LENGTH = "REGISTER NUMBER\nMAX VALUE: 999";
 public const string PROMPT_DATA_FILE_UPDATED = "UPDATE DATA\nFILES?";
 public const string PROMPT_REGISTER_FILE_TRANSFER = "UPLOAD TO\nBACKOFFICE?";
 public const string SECURITY_CODE = "SECURITY CODE";
 public const string ENTERING_FISCAL_MODE = "ENTERING FISCAL MODE";
 public const string ENTERED_FISCAL_MODE = "ENTERED FISCAL MODE";
 public const string ENTER_FISCAL_MODE = "ENTER FISCAL MODE?";
 public const string NOT_ENTER_FISCAL_MODE = "NOT ENTERED\nFISCAL MODE";
 public const string REGISTER_NO = "REGISTER NO";
 public const string OFFICE_INDEX = "OFFICE PATH";
 public const string TIME_CHANGED = "TIME UPDATED";
 public const String BACKOFFICE_TIME_NOT_SET = "BACKOFFICE TIME\nCOULD NOT SET";
 public const string DATA_FILES_UPDATING = "DATA FILES\nUPDATING...";
 public const string DATA_FILES_UPDATED = "DATA FILES\nUPDATED";
 public const string PROGRAM_FILES_UPDATING = "PROGRAM FILES\nUPDATING...";
 public const string LAST_UPDATE = "LAST UPDATE";
 public const String TCP_IP_ADDRESS = "TCP/IP ADDRESS";
 public const string INVALID_TIME = "INVAID TIME";
 public const string REGISTER_FILES_TRANSFERRED = "POS FILES\nUPLOAD COMPLETE";
 public const string PRINTER_COM_PORT = "PRINTER COM PORT";
 public const string DISPLAY_COM_PORT = "DISPLAY COM PORT";
 public const string SLIP_COM_PORT = "INVOICE COM PORT";
 public const string NO_SLIP_PORT = "NO SLIP PORT";
 public const string SCALE_COM_PORT = "SCALE COM PORT";
 public const string BARCODE_COM_PORT = "BARCODE COM PORT";
 public const String SCALE_CONNECTION_ERROR = "SCALE CONNECTION\nERROR";
 public const String BARCODE_CONNECTION_ERROR = "BARCODE\nCONNECTION ERROR";
 public const string BRIGHTNESS_LEVEL = "BRIGHTNESS";
 public const string DISPLAY_TEST = "ABCDEFGHIJKLMNOPQRST\nUVWXYZ0123456789.:!@";
 public const string TAX_RATE_INTERVAL_ERROR = "VAT RATE 0-99\nRANGE";
 public const string LOADING_DEPARTMENT_AND_VAT_INFO = "DEPARTMENT AND VAT\nLOADING";
 public const string LOADING_NEW_PROGRAM = "LOADING\nNEW PROGRAM...";
 public const string LOADING_FROM_REGISTER = "LOADING\nFROM REGISTER";
 public const string LOADING_LOGO_INFO = "LOADING\nLOGO INFO";
 public const string INVALID_DATE_RANGE = "INVALID\nDATE RANGE";
 public const string LOGO_DEPARTMENT_CHANGE_Z_REPORT_REQUIRED = "LOGO&TAX CHANGED\nZ REPORT REQUIRED";
 public const string SLIP = "SLIP";
 public const string SCALE = "SCALE";
 public const string BARCODE = "BARCODE";
 public const string DISPLAY = "DISPLAY";
 public const string NO_CONNECTION_WITH_BACKOFFICE = "NO CONNECTION\nWITH BACK OFFICE";
 public const String EXCEED_PRODUCT_LIMIT = "EXCEED PRODUCT\nLIMIT";
 public const String INVALID_SECURITY_KEY_EXCEPTION = "INVALID SECURITY\nKEY EXCEPTION";
 public const string RESETED_DISPLAY = "DISPLAY RESETED";
 public const string ENTER_VALUE = "ENTER VALUE";
 public const string CONTINUE = "CONTINUE";
 public const string NOT_TRANSFERED_DATA_FILES = "NOT TRANSFERED\nDATA FILES";
 public const string NOT_PROGRAM_INSTALLED = "NOT PROGRAM\nINSTALLED";//"PROGRAM YÜKLENEMEDİ"
 public const string MAIN_DEPARTMENT = "MAIN DEPART.";
     public const string NO_USING = "NOT ENABLE";
     public const string PROMOTION_CODE = "Promotion Code";
 
    #endregion
    #region EJ MESSAGES
 public const string EJ_FULL = "EJ FULL";
 public const string EJ_CHANGED = "EJ CHANGED\nACTIVE EJ NO: ";
 public const string EJ_LIMIT_WARNING = "EJ USAGE ";
 public const string EJ_AVAILABLE_LINES = "EJ REMAINING LINES";
 public const string EJ_MISMATCH = "INVALID EJ";
 public const string EJ_FORMAT_ERROR = "EJ FORMAT ERROR";
 public const string EJ_NOT_AVAILABLE = "EJ UNAVAILABLE";
 public const string EJ_FISCALID_MISMATCH = "EJ FISCALID\nMISMATCH";
 public const string EJ_BITMAP_ERROR = "BITMAP ERROR";
 public const string EJ_ERROR_OCCURED = "EJ ERROR";
    #endregion
    #region "NUMBER TO WORD"
 public const string ONLY = "ONLY";
 public const string YTL = "€";
 public const string YKR = "C";
 public const string ZERO = "ZERO";
 public const string ONE = "ONE";
 public const string TWO = "TWO";
 public const string THREE = "THREE";
 public const string FOUR = "FOUR";
 public const string FIVE = "FIVE";
 public const string SIX = "SIX";
 public const string SEVEN = "SEVEN";
 public const string EIGHT = "EIGHT";
 public const string NINE = "NINE";
 public const string TEN = "TEN";
 public const string TWENTY = "TWENTY";
 public const string THIRTY = "THIRTY";
 public const string FOURTY = "FOURTY";
 public const string FIFTY = "FIFTY";
 public const string SIXTY = "SIXTY";
 public const string SEVENTY = "SEVENTY";
 public const string EIGHTY = "EIGHTY";
 public const string NINETY = "NINETY";
 public const string HUNDRED = "HUNDRED";
 public const string THOUSAND = "THOUSAND";
 public const string MILLION = "MILLION";
 public const string BILLION = "BILLION";
 public const string TL = " $";
 public const string KURUS = "CENTS";
    #endregion
    #region LOG KEYWORDS
 public const string SAT = "SAT";
 public const string IPT = "IPT";
 public const string ART = "ART";
 public const string IND = "IND";
 public const string SNS = "SNS";
 public const string NAK = "NAK";
 public const string DVZ = "DVZ";
 public const string KRD = "KRD";
 public const string TOP = "TOP";
 public const string PRM = "PRM";
 public const string MSG = "MSG";
 public const string NOT = "NOT";
 public const String KOD = "KOD";
 public const string END = "SON";
 public const String LMT = "LMT";
 public const String HAR = "HAR";
    #endregion
    #region "POS CLIENT TYPE"
 public const string WEBSERVICE = "WEBSERVICE";
 public const string TCPLISTENER = "TCP";
    #endregion
    #region "DIPLOMATICCUSTOMER RELATED"
 public const string CUSTOMER_IDENTITY = "KÝMLÝK NO";
    #endregion
    #region PARAMETEREXCEPTIONS
 public const string PARAMETER_EXCEPTION = "PARAMETRE HATASI";
 public const string NO_DOCUMENT_FOUND = "ARANILAN ÖZELLÝKTE\nBELGE BULUNMUYOR";
 public const string DATE_OUTOFRANGE = "TARÝH ARALIÐINDA\nBELGE BULUNMUYOR";
 public const string Z_OUTOFRANGE = "Z NUMARASI\nBULUNAMADI";
 public const string DOCUMENT_OUTOFRANGE = "ARANILAN BELGE NO\nBULUNAMADI";
 public const string ADDRESS_OUTOFRANGE = "BULUNAN ADRES\nARALIÐIN DIÞINDA";
 public const string PARAMETER_SEQUENCE_ERROR = "PARAMETRE SIRASI\nUYGUN DEÐÝL";
 public const string EXCESSIVE_PARAMETER_INTERVAL = "PARAMETRE ARALIÐI\nÇOK FAZLA";
 public const string TIME_LIMIT_ERROR = "SAAT DEÐÝÞÝKLÝÐÝ\n60 DAKÝKAYI AÞAMAZ";
 public const string TIME_ZREPORT_ERROR = "SAAT SON Z RAPORU\nONCESINE ALINAMAZ";
 public const string NO_PROPER_Z_FOUND = "ARANILAN DÖNEMDE\nMALÝ HFZ KAYDI YOK";
 public const string FIRST_PARAMETER_BIGGER_LAST_ONE = "FIRST PARAMETER\nBIGGER LAST ONE";
 public const string NO_DOCUMENT_FOUND_IN_EJ = "NO DOCUMENT FOUND\nIN EJ (ENTER)";
 
    #endregion
    #region SLIP (PRINTER) ERRORS
 public const string DOCUMENT_ID_NOTSET_ERROR = "DOCUMENT ID\nNOTSET ERROR";
 public const string UNFIXED_SLIP_ERROR = "UNFIXED SLIP\nERROR";
 public const string SLIP_ROWCOUNT_EXCEED_ERROR = "PUT DOCUMENT\nPRESS ENTER";
 public const string REQUEST_SLIP_ERROR = "PUT DOCUMENT\nTHEN WAIT";
 public const string NEGATIVE_COORDINATE_ERROR = "COORDINATE MUST BE\nPOSITIVE NUMBER";
 public const string CUSTOMER_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜÞTERÝ-VERGÝ";
 public const string CUSTOMER_TIME_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜÞTERÝ-SAAT";
 public const string TIME_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nVERGÝ-SAAT";
 public const string CUSTOMER_DATE_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜÞTERÝ-TARÝH";
 public const string DATE_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nVERGÝ-TARÝH";
 public const string COORDINATE_OUTOF_INVOICE_ERROR = "BAZI KOORDÝNATLAR\nBELGEYÝ AÞMAKTADIR";
 public const string NAME_VAT_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nÜRÜN ADI-ÜRÜN KDV";
 public const string AMOUNT_VAT_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nÜRÜN KDV-ÜRÜN TUTAR";
 public const string PRODUCT_COORDINATE_ERROR = "ÜRÜN BAÞLANGIÇ\nSATIRI ÇOK KÜÇÜK";
 public const string SLIP_ERROR = "SLIP\nERROR";
    #endregion SLIP ERRORS
    #region HUGINPOS ERRORS
 public const string LIMIT_EXCEED_ERROR = "LIMIT_EXCEED\nERROR";
 public const string OFFICE_PATH_ERROR = "OFFICE_PATH\nERROR";
 public const string OUTOF_QUANTITY_LIMIT = "OUTOF_QUANTITY\nLIMIT";
 public const string INVALID_QUANTITY = "INVALID_QUANTITY";
 public const string NO_ADJUSTMENT_EXCEPTION = "NO_ADJUSTMENT\nEXCEPTION";
 public const string NO_CORRECTION_EXCEPTION = "NO_CORRECTION\nEXCEPTION";
 public const string PROGRAM_HAS_CLOSED = "PROGRAM CLOSED";
    #endregion
    #region BARCODE MENU
 public const string RETURN_RECEIPT = "RETURN RECEIPT";
 public const string RETURN_PRODUCT = "RETURN PRODUCT";
 public const string REPEAT_SALE = "REPEAT SALE";
 public const string PRINT_RECEIPT_COPY = "PRINT RECEIPT COPY";
 public const string CONFIRM_PRINT_RECEIPT_COPY = "CONFIRM PRINT\nRECEIPT COPY?";
 public const string RECEIPT_NOT_BELONG_TO_CASE = "RECEIPT NOT\nBELONG TO CASE";
    #endregion
    #region CONTACTLESS MESSAGES

 public const string CONTACTLESS_CARD_ERROR = "TEMASSIZ\nKART HATASI";
 public const string CONFIRM_UPDATE_POINTS = "PUAN KAYDEDÝLEMEDÝ\nTEKRAR DENE?";
 public const string MISSING_CARD_INFO = "KART BÝLGÝLERÝ EKSÝK\nYÜKLENSÝN MÝ?";
 public const string CARD_MISMATCH = "AYNI KARTI\nKOYMALISINIZ";
    #endregion
    #region POINT MESSAGES

    public const string CUSTOMER_NOTIN_POINT_DB = "MÜÞTERÝ KAYDI\nVERÝTABANINDA YOK";
    public const string CARDSERIAL_NOTIN_POINT_DB = "BU KART SERÝSÝ\nVERÝTABANINDA YOK";
    public const string FAIL_ON_POINT_UPDATE = "PUAN VERÝTABANINA\nKAYDEDÝLEMEDÝ";
    public const string FAIL_ON_CARDSERIAL_INSERT = "KART SERÝSÝ\nKAYDEDÝLEMEDÝ";
    public const string CARDSERIAL_ALREADY_EXISTS = "VERÝTABANINDA\nBU SERI NUMARASI VAR";
    public const String POINT = "POINT";
    public const String EARNED_POINT = "EARNED POINT";
    public const String USED_POINT = "USED POINT";
    public const String RETURNED_POINT = "RETURNED POINT";
    public const String TOTAL_POINT = "TOTAL POINT";
    public const String DEAR_CUSTOMER = "DEAR";
    public const String CUSTOMER_CODE_SHORTENED = "C.CODE";
    public const String TOTAL_REDUCTION = "TOTAL DISC";
    public const String SUBTOTAL_REDUCTION = "SUBTOTAL DISC";
    public const String PRODUCT_REDUCTION = "PRODUCT DISC";
    public const String TOTAL_FEE = "TOTAL FEE";
    public const String SUBTOTAL_FEE = "SUB TL FEE";
    public const String PRODUCT_FEE = "PRODUCT FEE";
    #endregion
    #region CUSTOMER FORM MESSAGE
     public const string CLOSE_PROGRAM = "ERROR OCCURED\nPROGRAM IS CLOSING";
     public const string RESTART_PROGRAM = "PROGRAM WILL BE\nRESTART";
     public const string RESTART_COMPUTER = "COMPUTER WILL\nBERESTART";
     public const string PORT_IN_USE = "PORT IN USE";
     public const string MISSING_REFERENCE_FILE = "MISSING\nREFERENCE FILE";
 public const string PLU_PAGE = "PLU Page";
 public const string LED_ONLINE = "Online";
 public const string LED_FAST_PAYMENT = "Order";
 public const string LED_RETURN = "Return";
 public const string LED_CUSTOMER = "Customer";
 public const string LED_SALE = "Sale";
 public const string CUSTOMER = "CUSTOMER";
 public const string PROGRAM = "PROGRAM";
 public const string REPORT = "REPORT";
 public const string ENTER = "ENTER";
 public const string UNDO = "UNDO";
 public const string CASH_DRAWER = "DRAWER";
 public const string PAPER_FEED = "FEED";
 public const string AMOUNT = "AMOUNT";
 public const string ESCAPE = "ESC";
 public const string PRICE = "PRICE";
 public const string DOCUMENT = "DOCUMENT";
 public const string SUB_TL = "SUB TL";
 public const string COMMAND = "COMMAND";
 public const string REPEAT = "REPEAT";
 public const string CASH_PAYMENT = "TOTAL";
 public const string PAYOUT = "PO";
 public const string RECEIVE_ACCT_ON_PAYMENT = "RO";
 public const string NO_PRODUCT = "NO PRODUCT";
     public const string DELETE = "Clr";
    public const string CUSTOMER_ENTRY = "CUSTOMER ENTRY";
    #endregion CUSTOMER FORM MESSAGE
    #region EFTPOS
 public const string FAILED_PROVISION_ACCEPT_PAYMENT = "FAILED PROVISION\nACCEPT PAYMENT";
 public const string PROCESS_REJECTED = "PROCESS REJECTED";
 public const string EFT_TIMEOUT_ERROR = "EFT TIMEOUT ERROR";
 public const string EFT_POS_ERROR = "EFT POS ERROR";
 public const String ACCEPT_PAYMENT_OR_REPEAT_VIA_EFT = "ENTER: PAID FINISH\nREPEAT: REPEAT VIA EFT";
 public const String PROCESS_START_WAITING_EFT_POS = "PAYMENT START\nPLEASE WAIT...";
 public const String ANY_CONNECTED_EFT_POS = "BAĞLI BİR CİHAZ\nBULUNMAMAKTADIR";
 public const String NO_MATCHED_EFT_POS = "EŞLENMİŞ EFT-POS\nBULUNMAMAKTADIR";
 public const String DOCUMENT_VOIDED_BY_EFT_POS = "CİHAZ TARAFINDAN FİŞ\nİPTAL EDİLMİŞTİR";
 public const String TIMEOUT_EX_SEND_AGAIN = "ZAMAN AŞIMI\nTEKRAR GÖNDER(GİRİŞ?)";
    #endregion EFTPOS
    
    #region Document Status List Messages
        public const String DOCUMENT_STATUS = "DOCUMENT STATUS";
        public const String OPEN = "OPEN";
        public const String CLOSED = "CLOSED";
    #endregion
    
    #region System Manager List Messages
        public const String SYSTEM_MANAGER = "SYSTEM MANAGER";
        public const String RESTART_POS = "RESTART PROGRAM";
        public const String SHUTDOWN_SYSTEM = "SHUTDOWN SYSTEM";
        public const String RESTART_SYSTEM = "RESTART SYSTEM";
        public const String CONFIRM_RESTART_PROGRAM = "CONFIRM RESTART\nPROGRAM?";
        public const String UPDATE_FOLDER = "UPDATE FROM USB";
        public const String UPDATE_ONLINE = "ONLINE UPDATE";
        public const String SOUND_SETTINGS = "SOUND SETTINGS";
    public const String SOUND_LEVEL = "SOUND LEVEL";
    #endregion

    #region Tallying Menu

        public const String TALLYING = "TALLYING";
        public const String PRODUCT_QUANTITY = "PRODUCT QUANTITY";
        public const String PRODUCT_BARCODE = "PRODUCT BARCODE";
        public const String PRODUCT_NOT_WEIGHABLE = "PRODUCT NOT\nWEIGHABLE";
        public const String CONFIRM_EXIT_TALLYING = "EXIT TALLYING?";

    #endregion

    #region EscPos Receipt Messages
        public const String RECEIPT_TIME = "TIME";
        public const String RECEIPT_INFORMATION = "RECEIPT INFORMATION";
        public const String EXCHANGE_PROVISION = "EXCHANGE PROVISION";
        public const String DOCUMENT_QUNTITY = "DOCUMENT QUNTITY";
        public const String OF_QUNTITY = " QUNTITY";

    #endregion

    #region Common Words
        public const String NUMBER = "NUMBER";
        public const String NO = "NO";
        public const String PAGE = "PAGE";

    #endregion

    #region PROGRAM INFO REPORT
     public const String DEPARTMENT_INFORMATIONS = "DEPARTMENT INFORMATIONS"; //DEPARTMAN BİLGİLERİ
     public const String RECEIPT_LIMIT = "RECEIPT LIMIT";//FİŞ LİMİTİ
     public const String PRODUCT_MINUMUM_PRICE = "PRODUCT MINUMUM PRICE";//ÜRÜN MİNİMUM FİYATI
     public const String AUTO_CUSTOMER_ADJUSTMENT = "APPLY CUST. DISCOUNT";//OTOMATİK MÜŞT. İND.
     public const String OPEN_DRAWER_ON_PAYMENT = "OPEN DRAWER ON PAYMENT";//"ÖDEMEDE PARA ÇEKM. AÇ."
     public const String PRINT_SUBTOTAL = "PRINT SUBTOTAL";//"A.TOPLAMI BELGEYE YAZ."
     public const String PROMPT_CREDIT_INSTALLMENTS = "CREDIT INSTALLMENTS";//"KREDİ TAKSİT SORMA"
     public const String CASHIER_NAME_ON_RECEIPT = "PRINT CASHIER NAME";//BELGEDE KASİYER İSMİ
     public const String PRINT_BARCODE_ON_FOOTER = "PRINT RECEIPT BARCODE";//"FİŞ SONUNDA BARKOD",
     public const String SETTINGS = "SETTINGS";//"AYARLAR",

    public const string LOGO_DIFFERENT = "LOGO DIFFERENT";
    public const string LOGO_UPDATING = "LOGO\nUPDATING..";
    public const string GRAPHIC_LOGO_DIFFERENT = "GRAPHIC LOGO\nDIFFERENT";
    public const string GRAPHIC_LOGO_UPDATING = "GRAPHIC LOGO\nUPDATING..";
    public const string AUTO_CUTTER_DIFFERENT = "AUTO CUTTER\nDIFFERENT";
    public const string AUTO_CUTTER_UPDATING = "AUTO CUTTER\nUPDATING..";
    public const string RECEIPT_BARCODE_DIFFERENT = "RECEIPT BARCODE\nDIFFERENT";
    public const string RECEIPT_BARCODE_UPDATING = "RECEIPT BARCODE\nUPDATING..";
    public const string RECEIPT_LIMIT_DIFFERENT = "RECEIPT LIMIT\nDIFFERENT";
    public const string RECEIPT_LIMIT_UPDATING = "RECEIPT LIMIT\nUPDATING..";
    public const string CURRENCIES_DIFFERENT = "CURRENCIES\nDIFFERENT";
    public const string CURRENCIES_UPDATING = "CURRENCIES\nUPDATING....";
    public const string DEPARTMENT_DIFFERENT = "DEPARTMENT\nDIFFERENT";
    public const string DEPARTMENT_UPDATING = "DEPARTMENT\nUPDATING..";
    #endregion

    #region PROGRAM MENU MESSAGES

     public const String ENTER_CONFIG_1 = "ENTER CONFIG 1";
     public const String ENTER_CONFIG_2 = "ENTER CONFIG 2";
     public const String ENTER_CONFIG_3 = "ENTER CONFIG 3";
     public const String ENTER_CONFIG_4 = "ENTER CONFIG 4";
     public const String ENTER_CONFIG_5 = "ENTER CONFIG 5";

     public const String PM_CONFIG = "UPDATE DATA";
     public const String PM_CONFIG_1 = "UPDATE VAT";
     public const String PM_CONFIG_2 = "UPDATE DEPT.";
     public const String PM_CONFIG_3 = "UPDATE PRODUCT";
     public const String PM_CONFIG_4 = "UPDATE LOGO";
     public const String PM_CONFIG_5 = "UPDATE CASHIER";

     public const String MENU_START_FM_TEST = "START FM TEST";

    #endregion

    #region FPU MESSAGES

        public const String CANNOT_MATCH_EXT_DEV = "CANNOT MATCH\nEXT. DEVICE";
        public const string ALERT_INVALID_VAT_ON_MEAL_TICKET = "CANNOT SALE ON\nTHIS VAT RATE";
        public const string CHECKING_TAXPAYER_STATUS = "TAXPAYER STATUS\nCHECKING...";
        public const string NOT_EINVOICE_TAXPAYER = "NOT E-INVOICE\nTAXPAYER";
        public const string CAR_PLATE = "PLATE INFO";
        public const string SUBSCRIBER_NO = "SUBSCRIBER NO";
        public const string CUSTOMER_NAME_OR_TITLE = "CUSTOMER NAME/TITLE";
        public const string COLLECTION_AMOUNT = "COLLECTION AMOUNT";
        public const string COMISSION_AMOUNT = "COMISSION AMOUNT";
        public const string INVOICE_SERIAL = "INVOICE SERIAL NO";
        public const string INSTUTION_NAME = "INSTUTION NAME";
        public const String WAITING_PAYMENT = "WAITING FOR\nPAYMENT...";
        public const String START_FM = "START FM";
        public const string EFT_SLIP_COPY = "EFT SLIP COPY";
        public const string EFT_POS_OPERATIONS = "EFT-POS OPERATIONS";
        public const String TABLE_MANAGEMENT = "TABLE MANAGEMENT";
        public const string LAST_OPERATION = "LAST OPERATION";
        public const string BATCH_NO = "BATCH NO";
        public const string ACQUIER_ID = "ACQUIER ID";
        public const string STAN_NO = "STAN NO";
        public const string NO_CUSTOMER = "NO CUSTOMER";

        public const string MULTIPLY = "X";
        public const string DIVIDE = "/";
        public const string SUBTRACT = "-";
        public const string ADD = "+";
        public const string EQUAL = "=";
        public const string SERIAL = "SERIAL NO";

        public const String UNDEFINED_TAX_RATE = "UNDEFINED\nTAX RATE";
        public const String LOGO_LINE = "LOGO LINE";
        public const String FPU_ERROR = "ECR ERROR";
        public const String RECEIPT_ROWCOUNT_EXCEED_ERROR = "RECEIPT ROW\nCOUNT EXCEEDED";
        public const String PLU_LIMIT_EXCEEDED = "PLU LIMIT\nEXCEEDED";
        public const String RANGE_OF_NUMBER_EXCEPTION = "INVALID\nRANGE ENTRY";
        public const String CALL_SERVICE = "CALL SERVICE";

        public const String CANNOT_VOID_ITEM = "CANNOT VOID\nITEM";
        public const String INVALID_PASS_ENTRY = "INVALID PASS ENTRY";
        public const String PROGRAM_CLOSING = "PROGRAM\nCLOSING...";
        public const string UPDATE_FOLDER_ERROR = "UPDATE FOLDER\nNOT FOUND";
        public const string UPDATE_STATU = "POS UPDATING...";
        public const string UPDATE_ERROR_404 = "NO CONTENT";
        public const string UPDATE_ERROR_406 = "NO UPDATE";
        public const string UPDATE_ERROR_500 = "SERVER ERROR";
        public const String SHUTDOWN_POS = "SHUTDOWN POS";
        public const string UPDATE_ERROR = "FAIL ON UPDATE";
        public const String INVALID_FISCAL_ID = "INVALID\nFISCAL ID";
        public const String FM_STARTED_SUCCSFLY = "FM STARTED\nSUCCESSFULLY";
        public const String CERTIFICATES_CANNOT_UPLOAD = "CERTIFICATES\nCANNOT DOWNLOAD";
        public const String FM_CANNOT_STARTED = "FM CANNOT STARTED\nTRY AGAIN";

        public const String RIGHT_TO_REST = "RIGHT TO REST:";

        public const String MANAGER_LOGIN_LOCKED = "MANAGER LOGIN\nLOCKED";
        public const String NO_AUTHORIZATION_FOR_LOAD_SETTINGS = "NEED PROGRAM\nAUTHORIZATION <ENTER>";
        public const String DNEY_PERCENT_OVER_AMOUNT = "PERCENT LIMIT\nEXCEEDED";
        public const String CLERK_NOT_FOUND = "CLERK NOT FOUND";
        public const String ENTER_TCKN_VKN = "ENTER TCKN/VKN";
        public const String ENTER_TCKN_VKN_MENU = "CUSTOMER             \nTCKN/VKN ENTRY";
        public const String INVALID_CASHIER = "INVALID CASHIER";
        public const String INVALID_CASHIER_ID = "INVALID CASHIER ID";
        public const String ENTER_CASHIER_ID = "CASHIER ID";
        public const String LAST_EFT_FILENAME = "LAST_EFT_OPT";
        public const String NO_LAST_OPERATION = "THERE IS NO\nEFT OPERATION";
        public const String LAST_OPERATION_FAILED = "LAST OPERATION\nFAILED";

        public const string CUSTOMER_INFO = "CUSTOMER INFO";

        public const string INVOICE_PROFIL = "INVOICE PROFIL";

        public const string ADDRESS_INFO = "ADDRESS INFO";

        public const string CONTACT_INFO = "CONTACT INFO";

        public const string ADDITIONAL_INFO = "ADDITIONAL INFO";

        public const string CLEAR_INPUTS = "CLEAR INPUTS";

        public const string FILL_AUTO = "FILL AUTO";

        public const string APPLY_CLEAR_ADD_INFO = "CLEAR INFO\nAPPLY? (ENTER)";

        public const string INPUTS_CLEAR = "INPUTS CLEAR\nSUCCESSFULLY";

        public const string CI_TITLE = "TITLE";

        public const string CI_NAME = "NAME";

        public const string CI_FAMILY_NAME = "FAMILY NAME";

        public const string CI_TAX_SCHEME = "TAX SCHEME";

        public const string CI_ROOM = "ROOM";

        public const string CI_BUILDING_NO = "BUILDING NO";

        public const string CI_BUILDING_NAME = "BUILDING NAME";

        public const string CI_STREET = "STREET";

        public const string CI_DISTRICT = "DISTRICT";

        public const string CI_VILLAGE = "VILLAGE";

        public const string CI_SUBCITY = "SUBCITY";

        public const string CI_CITY = "CITY";

        public const string CI_COUNTRY = "COUNTRY";

        public const string CI_POSTAL_CODE = "POSTAL CODE";

        public const string CI_TELEPHONE = "TELEPHONE";

        public const string CI_FAX = "FAX";

        public const string CI_EMAIL = "E-MAIL";

        public const string CI_WEB_PAGE = "WEB PAGE";

        public const string BASIC_INVOICE = "BASIC INVOICE";

        public const string TRADING_INVOICE = "TRADE INVOICE";

        public const string TRY_AGAIN_OR_HOLD_DOC = "TRY AGAIN (E)\nHOLD DOCUMENT (Q)";

        public const string SLIP_PRINTING = "SLIP\nPRINTING..";

        public const string DEFAULT_DOCUMENT_TYPE = "DEFAULT DOCUMENT TYPE";

    #endregion

    #region FPU ERROR MESSAGES

        public const string FPU_ERROR_MSG_0 = "OPERATION SUCCESSFULL";
        public const string FPU_ERROR_MSG_1 = "MISSING DATA\nLEN SHOULD COME UP";
        public const string FPU_ERROR_MSG_2 = "CRC ERROR";
        public const string FPU_ERROR_MSG_3 = "INVALID\nFPU STATE";
        public const string FPU_ERROR_MSG_4 = "INVALID\nCOMMAND";
        public const string FPU_ERROR_MSG_5 = "INVALID PARAMETER";
        public const string FPU_ERROR_MSG_6 = "OPERATION FAILS";
        public const string FPU_ERROR_MSG_7 = "CLEAR REQUIRED\n(AFTER ERROR)";
        public const string FPU_ERROR_MSG_8 = "NO PAPER";
        public const string FPU_ERROR_MSG_9 = "UNABLE TO\nMATCH";

        public const string FPU_ERROR_MSG_11 = "FM LOAD ERROR";
        public const string FPU_ERROR_MSG_12 = "FM REMOVED";
        public const string FPU_ERROR_MSG_13 = "FM MISMATCH";
        public const string FPU_ERROR_MSG_14 = "NEW FM";
        public const string FPU_ERROR_MSG_15 = "FM INIT ERROR";
        public const string FPU_ERROR_MSG_16 = "FM COULD NOT FISCALIZE";
        public const string FPU_ERROR_MSG_17 = "DAILY Z LIMIT";
        public const string FPU_ERROR_MSG_18 = "FM FULL";
        public const string FPU_ERROR_MSG_19 = "FM INITIALIZED BEFORE";
        public const string FPU_ERROR_MSG_20 = "FM CLOSED";
        public const string FPU_ERROR_MSG_21 = "FM INVALID";
        public const string FPU_ERROR_MSG_22 = "CERTIFICATES CANNOT BE LOADED";

        public const string FPU_ERROR_MSG_31 = "EJ LOAD ERROR";
        public const string FPU_ERROR_MSG_32 = "EJ REMOVED";
        public const string FPU_ERROR_MSG_33 = "EJ MISMATCH";
        public const string FPU_ERROR_MSG_34 = "EJ OLD\n (EJ REPORTS)";
        public const string FPU_ERROR_MSG_35 = "NEW EJ\nWAITING FOR INIT";
        public const string FPU_ERROR_MSG_36 = "EJ CANNOT CHANGE\nZ REQUIRED";
        public const string FPU_ERROR_MSG_37 = "EJ INIT";
        public const string FPU_ERROR_MSG_38 = "EJ FULL\nZ REQUIRED";
        public const string FPU_ERROR_MSG_39 = "EJ FORMATTED";
        public const string FPU_ERROR_MSG_51 = "RECEIPT LIMIT\nEXCEEDED";
        public const string FPU_ERROR_MSG_52 = "RECEIPT SALE COUNT\nEXCEEDED";
        public const string FPU_ERROR_MSG_53 = "INVALID SALE";
        public const string FPU_ERROR_MSG_54 = "INVALID VOID";
        public const string FPU_ERROR_MSG_55 = "INVALID CORRECTION";
        public const string FPU_ERROR_MSG_56 = "INVALID ADJUSTMENT";
        public const string FPU_ERROR_MSG_57 = "INVALID PAYMENT";
        public const string FPU_ERROR_MSG_58 = "PAYMENT LIMIT\nEXCEEDED";
        public const string FPU_ERROR_MSG_59 = "DAILY SALE\nEXCEEDED";
        public const string FPU_ERROR_MSG_71 = "INVALID VAT RATE";
        public const string FPU_ERROR_MSG_72 = "UNDEFINED DEPARTMENT";
        public const string FPU_ERROR_MSG_73 = "INVALID PLU";
        public const string FPU_ERROR_MSG_74 = "INVALID\nCREDIT PAYMENT";
        public const string FPU_ERROR_MSG_75 = "INVALID F.CURRENCY\nPAYMENT";
        public const string FPU_ERROR_MSG_76 = "NO DOCUMENT FOUND";
        public const string FPU_ERROR_MSG_77 = "NO PROPER Z FOUND";
        public const string FPU_ERROR_MSG_78 = "INVALID SUB CATEGORY";
        public const string FPU_ERROR_MSG_79 = "FILE NOT FOUND";
        public const string FPU_ERROR_MSG_91 = "CASHIER AUTH EXCEPTION";
        public const string FPU_ERROR_MSG_92 = "HAS SALE";
        public const string FPU_ERROR_MSG_93 = "LAST DOCUMENT NOT Z";
        public const string FPU_ERROR_MSG_94 = "NOT ENOUGH\nCASH ON CR";
        public const string FPU_ERROR_MSG_95 = "DAILY RECEIPT LIMIT\nEXCEEDED";
        public const string FPU_ERROR_MSG_96 = "DAILY TOTAL \nEXCEEDED";
        public const string FPU_ERROR_MSG_97 = "ECR NON FISCAL";
        public const string FPU_ERROR_MSG_111 = "LINE LENGTH\nEXCEEDED";
        public const string FPU_ERROR_MSG_112 = "INVALID VAT RATE";
        public const string FPU_ERROR_MSG_113 = "INVALID DEPARTMENT";
        public const string FPU_ERROR_MSG_114 = "INVALID PLU";
        public const string FPU_ERROR_MSG_115 = "INVALID DEFINITION";
        public const string FPU_ERROR_MSG_116 = "INVALID BARCODE";
        public const string FPU_ERROR_MSG_117 = "INVALID OPTION";
        public const string FPU_ERROR_MSG_118 = "TOTAL MISMATCH";
        public const string FPU_ERROR_MSG_119 = "INVALID QUANTITY";
        public const string FPU_ERROR_MSG_120 = "INVALID AMOUNT";
        public const string FPU_ERROR_MSG_121 = "INVALID FISCAL ID";
        public const string FPU_ERROR_MSG_131 = "COVER OPENED";
        public const string FPU_ERROR_MSG_132 = "FM MESH DAMAGED";
        public const string FPU_ERROR_MSG_133 = "HUB MESH DAMAGED";
        public const string FPU_ERROR_MSG_134 = "Z REQUIRED\n(24 HOURS)";
        public const string FPU_ERROR_MSG_135 = "EJ NOT FOUND";
        public const string FPU_ERROR_MSG_136 = "CERTIFICATES NOT\nDOWNLOADED";
        public const string FPU_ERROR_MSG_137 = "SET DATE-TIME";
        public const string FPU_ERROR_MSG_138 = "DAILY-FM MISMATCH";
        public const string FPU_ERROR_MSG_139 = "DB ERROR";
        public const string FPU_ERROR_MSG_140 = "LOG ERROR";
        public const string FPU_ERROR_MSG_141 = "SRAM ERROR";
        public const string FPU_ERROR_MSG_142 = "CERTIFICATE MISMATCH";
        public const string FPU_ERROR_MSG_143 = "VERSION ERROR";
        public const string FPU_ERROR_MSG_144 = "DAILY LOG LIMIT\nEXCEEDED";
        public const string FPU_ERROR_MSG_145 = "RESTART ECR";
        public const string FPU_ERROR_MSG_146 = "DAILY PASS LIMIT\nEXCEEDED";
        public const string FPU_ERROR_MSG_147 = "FISCALIZE SUCCESFULL\nRESTART ECR";
        public const string FPU_ERROR_MSG_148 = "CANNOT CONNECT TO\nGIB SERVER";
        public const string FPU_ERROR_MSG_149 = "CERTS DOWNLOADED\nRESTART ECR";
        public const string FPU_ERROR_MSG_150 = "SAFE ZONE\nCANNOT FORMAT";
        public const string FPU_ERROR_MSG_151 = "JUMPER REMOVE-PUT";
    public const string AUTH_NOT_DEFINED = "UNDEFINED\nAUTHORIZATION ";
    public const string PRINTING_PLS_WAIT = "PRINTING..\nPLEASE WAIT";
    public const string GO_EFT_POS_SIDE = "GO EFT-POS\nSIDE PLEASE";
    public const string MAX_LENGTH = "MAXIMUM LENGTH";
    public const string PRESS_ENTER_TO_AUTH = "ENTER AUTH (ENTER)\nCANCEL (EXIT)";

    #endregion
    }
 
#elif lang_hu
 public static class PosMessage
 {
    #region LOG AND ALERT
 public const string INVALID_OPERATION = "PROCESS ERROR";
 public const string INVALID_PASSWORD= "PASSWORD INCORRECT";
 public const string PROMPT_RETRY = "TRY AGAIN";
 public const string REGISTER_LOCKED = "POS IS LOCKED";
 public const string ENTER_PASSWORD = "ENTER PASSWORD";
 public const string ACCESS_LEVEL = "ACCESS LEVEL";
 public const string NO_ACCESS_RIGHT = "NOT AUTHORIZED";
  public const String AUTHORIZED_PASSWORD = "AUTHORIZED PASSWORD";
public const string UNDEFINED_KEY = "IDENTIFICATION KEY";
 public const string PRODUCT_FILE_LOAD_ERROR= "PRODUCT DOSSIER\nERROR FORMED";
 public const string PROGRAM_FILE_LOAD_ERROR = "PROGRAM FILE\nERROR FORMED";
 public const string CUSTOMER_FILE_LOAD_ERROR= "CUSTOMER FOLDER\nERROR FORMED";
 public const string CASHIER_FILE_LOAD_ERROR = "CASHIER FOLDER\nERROR FORMED";
 public const string CURRENCY_FILE_LOAD_ERROR = "CURRENCY EXCHANGE FOLDER\nERROR FORMED";
 public const string PROGRAM_UNLOADED = "PROGRAM INFORMATION\nNOT LOADED";
 public const string PASSWORD_INVALID = "INCORRECT PASSWORD";
 public const string CASHIER_ALREADY_ASSIGNED_EXCEPTION = "CASHIER ENTRY\nALREADY MADE";
 public const string CASHIER_LOGOUT = "CASHIER\nLOGOUT";
 public const string CASHIER= "CASHIER";
 public const string STARTUP_MESSAGE = "POS SETTINGS\nLOADING ...";
    #endregion
    #region SALES
 public const string NOT_SELLING = "NO SALES";
 public const string ENTER_NUMBER = "NUMBER ENTRY";
 public const string ENTER_AMOUNT = "ENTER AMOUNT";
 public const string ENTER_UNIT_PRICE = "UNIT PRICE ENTRY";
 public const string PRODUCT_LIMIT_EXEED = "PRODUCT LIMIT\nEXEED";
 public const string SERIAL_NUMBER_NOT_FOUND = "SERIAL NUMBER\nNOT FOUND";
public const string PRODUCT_NOTFOUND = "PRODUCT NOT FOUND";
 public const string RECEIVE_PAYMENT = "PAYMENT TO BE";
 public const string LAST_PAYMENT = "{0} MAXIMUM PAYMENT\nREMAINDER";
 public const string PROMPT_FINALIZE_SALE = "SALES TURN OFF";
 public const string BARCODE_NOTFOUND = "BARCODE\n NOT FOUND";
 public const string PLU_NOTFOUND = "PLU NOT FOUND";
 public const string BARCODE_ERROR= "ERROR CODE";
 public const string BARCODE_WEIGHT_ERROR = "ERROR CODE WEIGHT";
 public const string BARCODE_QUANTITY_ERROR= "ERROR CODE NUMBER";
 public const string BARCODE_TOTAL_AMOUNT_ERROR = "WITHOUT ERROR CODE";
 public const string RECEIPT_LIMIT_EXCEEDED = "LIMIT RECEIPT LOWLY";
 public const string TRANSFER_TO_INVOICE = "TRANSFER TO INVOICE";
 public const string PRODUCT_PRICE_NOTFOUND = "PRODUCT PRICE NO";
 public const string INVALID_PRODUCT = "PRODUCT INFORMATION WRONG";
 public const string UNKNOWN_ERROR= "ERROR FORMED";
 public const string PRICE_LOOKUP = "PRICE LOOK";
 public const string UNIT_PRICE = "UNIT PRICE";
 public const string SUBTOTAL = "SUBTOTAL";
 public const string DISCOUNTED = "DISCOUNT";
 public const String REDUCTION = "REDUCTION";
 public const string CLERK_ID = "CLERK CODE";
 public const string SELECT_PRODUCT= "SELECT PRODUCT";
 public const string REGISTER_CLOSED = "POS CLOSED";
 public const string PLEASE_WAIT= "PLEASE WAIT ...";
 public const string TOTAL_AMOUNT = "TOTAL AMOUNT";
 public static String WELCOME = "STORE\n WELCOME";
 public static String WELCOME_LOCKED = "STORE\n WELCOME";
     public const string CLERK = "ELADÓ";
 public const string PRODUCT = "PRODUCT";
 public const string VAT_DISTRIBUTION = "TAX DISTRIBUTION";
 public const string ZERO_PLU_PRICE_ERROR = "PRODUCT PRICE\nCAN NOT BE ZERO";
 public const string ZERO_DRAWER_IN_ERROR = "AMOUNT OF CASH ENTRY\nSIFIR EXCEED";
 public const string ZERO_DRAWER_OUT_ERROR = "AMOUNT OF CASH OUT\nSIFIR EXCEED";
 public const string ENTER_PRODUCT_PRICE = "PRODUCT PRICE INPUT";
 public const string ENTER_PRODUCT_SERIALNO = "ENTER PRODUCT SERIAL";
 public const string SELLING_VAT = "VAT";
 public const string DOCUMENT_TOTAL_AMOUNT_LIMIT_EXCEED_ERROR = "DOCUMENT AMOUNT\nLIMIT EXCEEDED";
 public const string ITEM_TOTAL_AMOUNT_LIMIT_EXCEED_ERROR= "ITEM AMOUNT\nLIMIT EXCEEDED";
 public const string VOID_AMOUNT_INVALID = "VOID AMOUNT\nINVALID";
 public const string CANNOT_VOID_NO_PROPER_SALE = "NO MATCHING SALE\nVOID INVALID";
 public const string DECIMAL_LIMIT = "DECIMAL LIMIT";
 public const string NO_SALE_INVALID_ACTION = "NO SALES\nINVALID OPERATION";
 public const string INSUFFICIENT_LIMIT = "LIMIT\nINSUFFICIENT";
 public const string VOID_SALESPERSON = "CLERK CANCEL?";
 public const string UNDEFINED_LABEL = "IDENTIFICATION LABEL";
 public const string NOT_ENOUGH_CHARS_FOR_PRICECHECK= "AT LEAST FOR PRICES\n2 SHIFT ENTER";
 public const string WAIT_DOCUMENT_TRANSFER = "PLEASE WAIT\nTRANSFER HELD ...";
 public const string CLERK_FOR_ITEM = "CLERK (PRODUCT)\n";
 public const string CLERK_FOR_DOCUMENT = "CLERK (TOTAL)\n";
 public const String GAINS="GAINS";
 public const string MIN_AMOUNT_ERROR = "MIN AMOUNT\n MUST BE0,01";
 public const String SERIAL_NUMBER_ALREADY_EXIST = "SERIAL NUMBER IS\nALREADY EXIST";
 public const String SALE_FROM_QR_CODE = "SALE FROM\nQR CODE";
    #endregion
    #region ELECTRONICJOURNALERROR
 public const string NO_EJ_AREA_FOR_OPERATION = "NO_EJ_AREA\nFOR_OPERATION";
 public const string FIX_VALID_EJ_TO_VOID_DOCUMENT = "FIX_VALID_EJ\nTO_VOID_DOCUMENT";
 public const string CONFIRM_NEW_EJ_FORMAT = "CONFIRM_NEW\nEJ_FORMAT";
 public const string ZREPORT_NECCESSARY_FOR_NEW_EJ = "ZREPORT_NECCESSARY\nFOR_NEW_EJ";
 public const string NO_ZREPORT_IN_EJ = "NO_ZREPORT\nIN_EJ";
 public const string CONFIRM_ZREPORT_ON_FULL_EJ = "CONFIRM_ZREPORT\nON_FULL_EJ";
 public const string EJ_PASIVE_ONLY_EJ_REPORTS = "EJ_PASSIVE_ONLY\nEJ_REPORTS";
 public const string EJ_PASIVE_VALID_EJ_REQUIRED = "EJ_PASSIVE\nVALID_EJ_REQUIRED";
 public const string CONNECTING_TO_PRINTER= "CONNECTING_TO\nPRINTER ...";
    #endregion ELECTRONICJOURNALERROR
    #region DOCUMENTS
 public const string SELECT_DOCUMENT= "SELECT DOCUMENT";
 public const string TRANSFER_DOCUMENT = "TRANSFER DOCUMENTS";
 public const string RECEIPT = "RECEIPT";
 public const string RECEIPT_TR = "RECEIPT";
 public const string WAYBILL = "WAYBILL";
 public const string WAYBILL_TR = "WAYBILL";
 public const string INVOICE = "INVOICE";
 public const string RETURN_DOCUMENT = "REFUND";
 public const string RETURN_DOCUMENT_TR = "RETURN";
 public const string HR_CODE_RETURN = "IAD";
 public const string HR_CODE_WAYBILL = "IRS";
 public const string HR_CODE_INVOICE = "FAT";
 public const string HR_CODE_RECEIPT = "FIS";
 public const String HR_INTER_CODE_RETURN = "GPS";
 public const string ORDER = "SIP";
 public const string ORDER_TR= "ORDER";
 public const string DOCUMENT_ID_NOT_FOUND = "DOCUMENT NO\n NOT FOUND";
 public const string CHANGE_DOCUMENT = "ACTIVE DOCUMENT\n {0} BE";
 public const string NOT_ENOUGH_MONEY_IN_REGISTER = "NOT ENOUGH MONEY\nIN REGISTER";
 public const string TRANSFER_STARTED_PLEASE_WAIT= "BEGIN TRANSFER\nPLEASE WAIT ..";
 public const string NO_AUTHORIZATION_FOR_SPECIAL_PRICE_SALES = "COST SALES\nNO AUTHORIZATION";
 public const string RECEIPT_LIMIT_EXCEEDED_TRANSFER_DOCUMENT = "RCPT LIMIT EXCEEDED\nTRANSFER DOCUMENT";
 public const string RECEIPT_LIMIT_DOCUMENT_TRANSFER_NOT_ALLOWED = "LIMIT RECEIPT\nNO TRANSFER";
 public const string FRACTIONAL_QUANTITY_NOT_ALLOWED = "FRACTIONAL QUANTITY\nNOT ALLOWED";
 public const string DEPOSITED_AMOUNT = "TRANSFERRED TO TOTAL";
 public const string COORDINATE_ERROR = "COORDINATES\nERROR";
 public const string DOCUMENT_CHANGE_ERROR = "DOCUMENT_CHANGE\nERROR";
 public const string DOCUMENT_FOLLOWING_ID = "DOCUMENT FOLLOWING ID";
 public const string SLIP_SERIAL = "SLIP SERIAL (AA)";
 public const string SLIP_ORDER_NO = "SLIP ORDER NO (123456)";
 public const String RETURN_REASON = "RETURN REASON";
    #endregion DOCUMENTS
    #region DISCOUNT
 public const string DISCOUNT_LIMIT_EXCEEDED = "DISCOUNT LIMIT EXCEEDED";
 public const string DISCOUNT_NOT_ALLOWED = "DISCOUNT ALLOWED";
 public const string DISCOUNT = "SALE";
 public const string DNEY_PERCENTDISCOUNT = "FRACTIONAL DISCOUNT\nNOT ALLOWED";
 public const string DNEY_PERCENTFEE = "FRACTIONAL SURCHARGE\nNOT ALLOWED";
 public const string INSUFFICIENT_ACCESS_LEVEL = "INSUFFICIENT ACCESS";
 public const string PRODUCT_PERCENT_DISCOUNT = "DISCOUNT";
 public const string PRODUCT_PRICE_DISCOUNT = "DISCOUNT";
 public const string SUBTOTAL_PERCENT_DISCOUNT = "DISCOUNT";
 public const string SUBTOTAL_PRICE_DISCOUNT = "DISCOUNT";
 public const string DISCOUNT_ALLOWED = "DISCOUNT ALLOWED";
 public const string COUNT_ALLOWED = "SURCHARGE ALLOWED";
 public const string CORRECTION= "CORRECTION";
    #endregion
    #region FEE
 public const string FEE_LIMIT_EXCEEDED = "SURCHARGE LIMIT\nEXCEEDED";
 public const string PRODUCT_PERCENT_FEE = "SURCHARGE";
 public const string PRODUCT_PRICE_FEE = "SURCHARGE";
 public const string FEE = "SURCHARGE";
 public const string SUBTOTAL_PRICE_FEE = "SURCHARGE";
 public const string SUBTOTAL_PERCENT_FEE = "SURCHARGE";
 public const string FEE_NOT_ALLOWED = "SURCHARGE\nNOT ALLOWED";
    #endregion
    #region VOID
 public const string VOID_AMOUNT_EXCEEDED = "VOID_AMOUNT\nEXCEEDED";
 public const string VOID_INVALID = "VOID INVALID";
 public const string CONFIRM_LOGOUT = "CONFIRM LOGOUT";
 public const string PROMPT_ENTER = "PRESS ENTER";
 public const string VOID = "VOID";
 public const string VOID_FIND_PRODUCT = "VOID \nPRODUCT FIND";
 public const string ENTER_LIST_FOR_SPECIAL_PRICED_PRODUCT_VOID = "ENTER LIST\nTO VOID PRODUCT";
 public const string CANNOT_ASSIGN_CLERK_TO_VOID_SALE = "CANNOT_ASSIGN_CLERK\nTO_VOID_SALE";
 public const string DOCUMENT_VOID_COUNT = "NUMBER OF CANCELLATION";
    #endregion
    #region RECEIPT
 public const string EXEMPT_TAX_TOTAL = "VAT EXEMPT TOTAL";
 public const string EXEMPT_TAX = "VAT EXEMPT";
 public const string TOTAL = "TOTAL";
 public const string TOTALTAX = "VAT";
 public const string TOTAL_BOLD = "² ³ TOTAL";
 public const string SHORT_TOTAL_BOLD = "TOT ² ³";
 public const string TOTALTAX_BOLD = "VAT ² ³";
 public const string TAX_BOLD = "² VAT ³";
 public const string OTHER = "OTHER";
 public const string CASH_RECEIPT= "CASH COLLECTION";
 public const string CURRENCY_RECEIPT = "EXCHANGE PROVISION";
 public const string CREDIT_RECEIPT = "CREDIT COLLECTION";
 public const string CHECK_RECEIPT = "CHECH COLLECTION";
 public const string COLLECTION = "COLLECTION"; 
 public const string CURRENCY_CASH_EXCHANGE= "CASH EQUIVALENT.";
 public const string SALE= "SALES";
 public const string VAT = "VAT";
 public const string PLUS = "+";
 public const string MINUS = "-";
    #endregion
    #region MENU PROGRAM
 public const string PM_FISCALIZATION = "FINANCIAL OPENING";
 public const string PM_VERSION = "VERSION";
 public const string PM_REGISTER = "POS DESCRIPTION";
 public const string PM_DATETIME = "CLOCK SET";
 public const string PM_DATAFILES= "DATA FILES";
 public const string PM_REGISTERFILES = "POS FILES";
 public const string PM_HARDWARE = "HARDWARE CONNECTION";
 public const String PM_COMPORTSETTINGS = "COM PORT SETTINGS";
 public const string PM_RESETDISPLAY = "DISPLAY RESET";
 public const string PM_LOADBITMAP = "LOGO GRAPHIC UPLOAD";
 public const string PM_NEWPROGRAM = "NEW PROGRAM";
 public const string CONFIRM_LOAD_NEW_PROGRAM= "LOAD THE NEW\nPROGRAM? (ENTER)";
 public const string PROGRAM_DATA_LOAD_ERROR = "PROGRAM INFO\nMISSING OR WRONG";
 public const string PROGRAM_ERROR = "PROGRAM ERROR";
 public const string CURRENCY_LIMIT_EXCEEDED_PAYMENT_INVALID = "EXCHANGE LIMIT\nPAYMENT INVALID";
 public const string PM_BARCODE_TERMINATOR = "BARCODE CONFIG";
 public const string ENTER_BARCODE = "ENTER BARCODE";
 public const string CONFIRM_BARCODE_TERMINATOR = "BARCODE\nSEPARATOR? (ENTER)";
 public const string PM_BUZZER_ON_OFF = "BUZZER ON / OFF";
 public const string PM_BUZZER_ON = "BUZZER ON? (ENTER)";
 public const string PM_BUZZER_OFF = "BUZZER OFF? (ENTER)";
    public const String PM_CONFIG_MANAGER = "YÖNETİCİ GÜNCELLE";
    public const String GMP_IP = "GMP IP";
        public const String GMP_PORT = "GMP PORT";
    public const String PM_CONFIG_GMP_PORT = "GMP PORT AYARLARI";
    public const String PM_FILE_TRANSFER = "DOSYA TRANSFERİ";
    #endregion
    #region PAYMENT
 public const string BALANCE = "BALANCE";
 public const string CASH ="CASH";
 public const string FOREIGNCURRENCY = "EXCHANGE";
 public const string TURKISH_LIRA= " €";
     public const string CHECK = "ELLEN\n\nŐRZÉS";
     public const string CREDIT = "KÁRTYA ";
 public const string PAYMENT= "PAYMENT";
 public const string PAYMENT_INVALID = "PAYMENT INVALID";
 public const string NO_SALE_PAYMENT_INVALID = "NO SALES\nPAYMENT INVALID";
 public const string CHECK_ID = "CHECK NUMBER";
 public const string INSTALLMENT_COUNT = "INSTALLMENTS";
 public const String INSTALLMENT = "INSTALLMENT";
 public const string PAYMENT_INFO = "PAYMENT DETAILS";
 public const string CHANGE = "CHANGE";
    #endregion
    #region CALCULATOR
 public const string CALCULATOR= "CALCULATOR";
 public const string ENTER_CALCULATOR = "CALCULATOR INPUT";
 public const string EXIT_CALCULATOR= "EXIT CALCULATOR";
 public const string CONFIRM_EXIT_CALCULATOR = "CONFIRM EXIT\nCALCULATOR (ENTER)";
    #endregion
    #region CUSTOMER
 public const string CUSTOMER_NOT_FOUND = "CUSTOMER\nNOT FOUND";
 public const string ENTER_CADR_CODE = "CUSTOMER CARD/CODE\nENTRY";
 public const string SEARCH_RECORD = "CUSTOMER\n SEARCH";
 public const string NEW_RECORD = "CUSTOMER\n NEW ENTRY";
 public const string RETURN_TO_SELLING = "BACK TO SALE";
 public const string VOID_CUSTOMER = "CUSTOMER CANCEL";
 public const string CONFIRM_VOID_CUSTOMER = "CUSTOMER CANCEL?\n (ENTER)";
 public const string NAME_FIRM = "NAME / COMPANY";
 public const string CUSTOMER_GROUP = "CUSTOMER GROUP";
 public const string LEGATION = "EMBASSY / COUNTRY";
 public const string ADDRESS = "ADDRESS";
 public const string STREET = "STREET";
 public const string STREET_NO = "STREET / NO";
 public const string REGION_CITY = "COUNTY / CITY";
 public const string TAXOFFICE = "TAX OFFICE";
 public const string SEARCH_QUERY = "CONTAINING TEXT";
 public const string CUSTOMER_NUMBER = "CUSTOMER NUMBER";
 public const string IDENTITY_NUMBER= "ID NUMBER";
 public const string CUSTOMER_CODE = "CUSTOMER CODE";
 public const string CUSTOMER_POINT = "CUSTOMER POINTS";
 public const string CUSTOMER_DUTY = "POSITION";
 public const string TAX_NUMBER = "TAX NUMBER";
 public const string PROMOTION_LIMIT = "PERCENT REDUCTION";
 public const string DISCOUNT_LIMIT = "DISCOUNT RATE (%)";
 public const string END_OF_RECORD = "REGISTRATION COMPLETED";
 public const string FATAL_ERROR_CUSTOMER_INFO_NOT_FOUND = "CRITICAL ERROR\nCUSTOMER INFORMATION NO"; // SPLIT
 public const string CUSTOMER_NOT_BE_CHANGED_IN_DOCUMENT = "SALE IN PROGRESS\nCANNOT CHANGE CUST.";
 public const string DOCUMENT_NOT_BE_TRANSFERRED = "DOCUMENT\n TRANSFERRED";
 public const string PROMPT_CLOSE_CASHREGISTER = "TURN OFF POS?\n (ENTER)";
 public const string TAX_INSTITUTION = "TAX OFFICE";
 public const string NAME = "NAME";
    #endregion
    #region LIST MESSAGES
 public const string END_OF_LIST = "END OF LIST";
 public const string START_OF_LIST = "START OF LIST";
 public const string LIST_EMPTY = "LIST EMPTY";
 public const string LISTING_ERROR = "LIST ERROR";
 public const string INVALID_PROGRAM = "PROGRAM FILES\nMISSING OR WRONG";
    #endregion
    #region PROCESS MENU
 // FOR SELLING STATE PROCESSES
 public const string VOID_DOCUMENT = "DOCUMENT VOID";
 public const string SUSPEND_DOCUMENT = "DOCUMENT HOLD";
 public const string CONFIRM_VOID_DOCUMENT = "DOCUMENT VOID\nCONFIRM? (ENTER)";
 public const string REPEAT_DOCUMENT = "DOCUMENT REPEAT";
 public const string RESUME_DOCUMENT = "PARKED DOCUMENTS";
 public const string PARKED_DOCUMENT = "PARKED DOCUMENT";
 public const string ORDERED_DOCUMENT = "ORDERED DOCUMENT";
 public const string ORDERS = "ORDERS";
 public const String FAST_PAYMENT = "FAST PAYMENTS";
 public const string ENTER_CASH = "CASH ENTRY";
 public const string RECEIVE_CASH = "CASH OUT";
 public const string RECEIVE_CHECK = "CHECK OUT";
 public const string RECEIVE_CREDIT = "LOAN OUT";
 public const string COMMAND_CALCULATOR = "CALCULATOR";
 public const string FIRST_DOCUMENT = "FIRST DOC (OPTIONAL)";
 public const string LAST_DOCUMENT = "FINAL DOC (OPTIONAL)";
 public const string DOCUMENT_ID = "DOCUMENT NUMBER";
 public const string CONFIRM_SUSPEND_DOCUMENT = "DOCUMENT HOLD\nSUSPEND? (ENTER)";
 public const string CASH_AMOUNT= "INSERT AMOUNT";
 public const string SUSPENDED_DOCUMENT = "DOCUMENT ON HOLD";
 public const String TABLE_MANAGEMENT = "TABLE MANAGEMENT";
    #endregion
    #region MENU REPORT
 public const string X_REPORT = "X REPORT ";
 public const string Z_REPORT = "Z REPORT ";
 public const string END_DAY_REPORT = "END OF DAY REPORT";
 public const string FINANCIAL_BETWEEN_Z = "FM REPORT(NUMBER)";
 public const string FINANCIAL_BETWEEN_DATE = "FM REPORT(DATE)";
 public const string PAYMENT_REPORT = "SALES REPORT";
 public const String PAYMENT_REPORT_CURRENT = "CURRENT";
 public const String PAYMENT_REPORT_DAILY = "DAILY";
 public const String PAYMENT_REPORT_DATE = "BETWEEN TWO DATE";
 public const String PAYMENT_REPORT_WITH_DETAIL = "DETAILED";
 public const String PAYMENT_REPORT_JUST_TOTALS = "ONLY TOTALS";
 public const string PROGRAM_REPORT = "PROGRAM INFO RPT";
 public const string EJ_SUMMARY_REPORT = "EJ DETAIL REPORT";
 public const string EJ_DOCUMENT_REPORT = "EJ SINGLE DOCUMENT";
 public const string EJ_DOCUMENT_DATE_TIME = "DATE & TIME";
 public const string EJ_DOCUMENT_Z_DOCID = "Z & RECEIPT NO NO";
 public const string EJ_DOCUMENT_ZREPORT = "Z REPORT REVIEW";
 public const string EJ_PERIODIC_REPORT = "EJ PERIODICAL";
 public const string EJ_PERIODIC_ZREPORT = "EJ BETWEEN 2 Z";
 public const string EJ_PERIODIC_FIRST_Z_NO = "FIRST Z [, RECEIPT]";
 public const string EJ_PERIODIC_LAST_Z_NO = "LAST Z [, RECEIPT]";
 public const string EJ_PERIODIC_DATE = "BETWEEN TWO DATE";
 public const string EJ_PERIODIC_DAILY = "DAILY";
 public const string EJ_LIMIT_SETTING = "EJ LIMIT SETUP";
 public const string CONFIRM_EJ_PERIODIC_REPORT = "EJ PERIODIC\nPRINT?";
 public const string WRITING_EJ_PERIODIC_REPORT = "EJ PERIODIC\nPRINTING...";
 public const string CONFIRM_EJ_DOCUMENT_REPORT = "EJ SINGLE DOCUMENT\nPRINT?";
 public const string WRITING_EJ_DOCUMENT_REPORT = "EJ SINGLE DOCUMENT\nPRINTING....";
 public const string CONFIRM_EJ_SUMMARY_REPORT = "EJ DETAIL REPORT\nPRINT?";
 public const string WRITING_EJ_PERIODIC_REPORT_BETWEEN_ZNO = "PERIODICAL Z\nPRINTING...";
 public const string WRITING_EJ_SUMMARY_REPORT = "EJ DETAIL REPORT\nPRINTING ..";
 public const string WRITE_XREPORT = "X-REPORT\nPRINT?";
 public const string WRITING_PAYMENT_REPORT = "SALES REPORT\nPRINTING ...";
 public const string WRITING_PROGRAM_REPORT = "PROGRAM INFO RPT\nPRINTING ...";
 public const string WRITING_XREPORT = "X-REPORT\nPRINTING ...";
 public const string WRITING_XREPORT_PLEASE_WAIT = "PRINTING X-REPORT\nPLEASE WAIT ...";
 public const String WRITING_XPLU_REPORT_PLEASE_WAIT = "PRINTING X PLU\nPLEASE WAIT";
 public const string WRITING_CASHIER_REPORT = "PRINTING CASHIER-RPT\nPLEASE WAIT...";
 public const string WRITE_Z_REPORT = "Z-REPORT\nPRINT?";
 public const string WRITING_Z_REPORT = "Z-REPORT\nPRINTING ...";
 public const string Z_NO = "Z NO:";
 public const string FIRST_Z_NO = "NO FIRST Z:";
 public const string LAST_Z_NO = "NO RECENT Z:";
 public const string WRITE_FINANCIAL_Z_REPORT = "FM REPORT\nPRINT?";
 public const string WRITING_FINANCIAL_Z_REPORT = "FM REPORT\nPRINTING...";
 public const string FINANCIAL_Z_REPORT_INVALID_PARAMETER = "VALID RANGE Z\nENTER";
 public const string REPORT_FINISHED = "REPORT RECEIVED";
 public const string TIME = "TIME (HHMM)";
 public const string DATE = "DATE (GGAAYYYY)";
 public const string FIRST_DATE = "FIRST DATE (DDMMYYYY)";
 public const string LAST_DATE = "END DATE (DDMMYYYY)";
 public const string CONFIRM_PAYMENT_REPORT = "SALES REPORT\nPRINT ?";
 public const string CONFIRM_PROGRAM_REPORT = "PROGRAM INFO REPORT\nPRINT?";
 public const string INVALID_DATE_INPUT = "INCORRECT ENTRY DATE";
 public const string SALES_EXIST_REPORT_NOT_ALLOWED = "SALES EXIST\nREPORT NOT ALLOWED";
 public const string SALES_EXIST_COMMANDMENU_NOT_ALLOWED = "SALES EXIST. PROGRAM\nMENU INVALID";
 public const string INVALID_ENTRY = "INCORRECT INPUT";
 public const String NO_SUPPORTED_REPORT = "NO SUPPORTED\nREPORT";
  public const string CONFIRM_START_REPORT = "CONFIRM START REPORT";//"RAPOR ALINACAK"
 public const string REPORT_PROCESSING = "REPORT PROCESSING";//"RAPOR ALINIYOR"
 public const string REPORT_STOPPING = "REPORT\nSTOPPING";//"RAPOR DURDURULUYOR"
    #endregion
    #region EXCEPTION PRINTER MESSAGES
 public const string PRINTER_EXCEPTION = "PRINTER ERROR";
 public const string FRAMING_EXCEPTION = "FRAMING ERROR";
 public const string CHECK_SUM_EXCEPTION = "CHECKSUM ERROR";
 public const string UNDEFINED_FUNCTION_EXCEPTION = "IDENTIFICATION FUNCTION";
 public const string CMD_SEQUENCE_EXCEPTION = "INCORRECT OPERATION SEQUENCE";
 public const string NEGATIVE_RESULT_EXCEPTION = "THE POS SUFFICIENT\nAMOUNT NO";
 public const string POWER_FAIURE_EXCEPTION = "ELECTRICITY INTERRUPTION\nBELGEYÝ CANCEL?";
 public const string UNFIXED_SLIP_EXCEPTION = "DOCUMENT PROPERLY\nYERLESTIRILMEMIS";
 public const string SLIP_COODINATE_EXCEPTION = "SLIP COORDINATE\n ERROR";
 public const string ENTRY_EXCEPTION = "SALES ERROR";
 public const string LIMIT_EXCEEDED_OR_ZREQUIRED_EXCEPTION = "Z REPORT REQUIRED\nBEFORE OPERATION";
 public const string BLOCKING_EXCEPTION = "BODY BLOCKED\nCALL TECH SERV.";
 public const string SVC_PASSWORD_OR_POINT_EXCEPTION = "SERVICE CODE\nERROR";
 public const string LOW_BATTERY_EXCEPTION = "INSUFFICIENT BATTERY";
 public const string BBX_NOT_BLANK_EXCEPTION = "BBX NOT NULL";
 public const string BBX_FORMAT_FAILURE_EXCEPTION = "FORMAT ERROR";
 public const string BBX_DIRECTORY_EXCEPTION = "BBX DIRECTORY ERROR";
 public const string MISSING_CASHIER_EXCEPTION = "CASHIER NO ENTRY";
 public const string ALREADY_FISCALIZED_EXCEPTION = "FINANCIAL FASHION\nCLICK BEFORE PASSED";
 public const string DTG_EXCEPTION = "TIME / DATE ERROR";
 public const string FATAL_ERROR_NO_CASHIER_INFO = "CRITICAL ERROR\nKASÝYER INFORMATION NO";
 public const string TIME_OUT_ERROR = "TIME OUT\nOLUÞTU";
 public const string FATAL_ERROR_PRODUCT_INFO_NOT_FOUND = "CRITICAL ERROR \nPRODUCT INFO NOT FND";
 public const string UNDEFINED_EXCEPTION = "UNDEFINED ERROR";
 public const string NULL_REFERENCE_EXCEPTION = "NULL REFERENCE\nEXCEPTION";
 public const string NOJOURNALROLL = "JOURNAL PAPER NO";
 public const string NORECEIPTROLL = "RECEIPT PAPER NO";
 public const string OFFLINE1 = "OFFLINE1 ERROR";
 public const string OPEN_SHUTTER = "PAPER COVER OPEN";
 public const string RFU_ERROR = "RFU ERROR";
 public const string MECHANICAL_FAILURE = "MECHANICAL ERROR";
 public const string PRINTER_OFFLINE = "PRINTER OFF";
 public const string PRINTER_CONNETTION_ERROR = "CANNOT REACH PRINTER";
 public const string FISCAL_ID_EXCEPTION = "FM\nMISMATCH";
 public const String FISCAL_COMM_EXCEPTION = "FM\nCOMMINUCATION ERROR";
 public const String FISCAL_MISMATCH_EXCEPTION = "FM\nMISMATCH";
 public const String FISCAL_UNDEFINED_EXCEPTION = "UNDEFINED FM";
 public const string SERVICE = "SERVICE";
 public const string MODE = "MODE";
 public const string CANNOT_ACCESS_PRINTER = "PRINTER\nUNAVAILABLE";
 public const string CANNOT_ACCESS_EJ = "EJ UNAVAILABLE";
 public const string COM_PORT_DEFECTIVE_CALL_SERVICE = "COM PORT FAULT\nCALL SERVICE";
 public const string PRINTER_TIMEOUT = "PRINTER ERROR\nTIMEOUT";
 public const String CHECK_RECEIPT_ROLL = "CHECK RECEIPT\nROLL";
 public const string ZREPORT_UNSAVED = "Z RAPORU\nNOT SAVED";
 public const string INCOMPLETE_XREPORT = "INCOMPLETE\nX REPORT";
 public const string DOCUMENT_NOT_EMPTY = "DOCUMENT NOT EMPTY";
 public const string SUBTOTAL_NOT_MATCH = "SUBTOTAL MISMATCH\nW FISCAL SUBSYSTEM";
 public const String INCOMPLETE_PAYMENT = "INCOMPLETE PAYMENT";
 public const String INCOMPLETE_PAYMENT_AFTER_EFT_DONE = "EFT AUTHORIZED\nCOMPLETE PAYMENT";
 public const string CAN_NOT_ACCESS_TO_DISPLAYS = "CANNOT ACCESS\nDISPLAYS";
 public const string CHECK_PRINTER = "CHECK PRINTER";
    #endregion
    #region SLIP MESSAGES
 public const string CONTINUE_OR_VOIDING_SLIP_SALE = "ENTER:CONTINUE SALE\nESC:VOIDING SLIP";
 public const String CONTINUE_OR_VOIDING_SLIP_SALE_ON_ERROR = "VOID: CANCEL SLIP\nESC: CONTINUE SLIP";
 public const string NEW_SLIP_PAPER = " KOYUP\nGIRIÞ'E BASINIZ";
 public const string PUT_IN = "PUT IN";
 public const string RECOVER_ERROR_AND_PRESS_ESC = "HATAYI DUZELTÝP\nESC'YE BASINIZ";
 public const string END_OF_SLIP_PAYMENT_NOT_ALLOWED = "FATURA SONU\nÖDEME GEÇERSÝZ";
 public const string RETURN_PAYMENTS = "RETURN PAYMENTS"; //"İADE ÖDEMELER"
 public const String CONTINUE_SELLING = "CONTINUE SELLING";
 public const String PUT_IN_PAPER = "PUT PAPER IN PRINTER";
    #endregion
    #region SERVICE MESSAGES
 public const string MENU_MEMORY_FORMAT = "DAILY MEMORY FORMAT";
 public const string MENU_DATE_AND_TIME = "DATE AND TIME";
 public const string MENU_READ_FISCAL_MEMORY = "READ FM";
 public const string MENU_READ_DAILY_MEMORY = "READ DAILY MEMORY";
 public const string MENU_FACTORY_SETTING = "FACTORY SETTINGS";
 public const string CLOSE_SERVICE = "CLOSE SERVICE";
 public const string LOAD_FACTOR_SETTINGS = "FACTORY SETTINGS\n(ENTER) UPLOAD";
 public const string SERVICE_PASSWORD = "SERVICE PASSWORD";
 public const string SERVICE_PASSWORD_INVALID = "INVALID SERVICE\nPASSWORD";
 public const string ATTACH_JUMPER_AND_TRY_AGAIN = "ATTACH JUMPER\nTRY AGAIN";
 public const string RESTART_PRINTER_FOR_SERVICEMODE = "RESTART PRINTER\nFOR SERVICE MODE";
 public const string PROMPT_DAILY_MEMORY_FORMAT = "DAILY MEMORY\nFORMAT(ENTER)";
 public const string REMOVE_JUMPER_AND_TRY_AGAIN = "REMOVE JUMPER\nTRY AGAIN";
 public const string ATTACH_JUMPER_AND_RESTART_FPU = "ATTACH JUMPER\nRESTART FPU";
 public const String LOGO_LINE = "LOGO LINE";
 public const String VAT_RATE = "VAT RATE";
 public const String PRINT_LOGS = "PRINT LOGS\n(ENTER)";
 public const String CREATE_DB = "CREATE DB\nCONFIRM (ENTER)";
    public const String MENU_FILE_TRANSFER = "FILE TRANSFER";
    #endregion
    #region SETTINGS MESSAGES
 public const string INVALID_ADDRESS_TRY_AGAIN = "ADRES GÝRÝÞÝ HATALI\nTEKRAR DENEYÝNÝZ";
 public const string PROGRAM_VERSION = "VERSION";
 public const string BRANCH_ID = "BRANCH ID";
 public const string PROMPT_DATA_FILE_UPDATED = "UPDATE DATA\nFILES?";
 public const string REGISTER_ID = "REGISTER ID";
 public const string REGISTER_ID_MAX_LENGTH = "REGISTER ID\nMAX VALUE: 999";
 public const string PROMPT_REGISTER_FILE_TRANSFER = "UPLOAD TO\nBACKOFFICE?";
 public const string SECURITY_CODE = "GÜVENLÝK KODU";
 public const string ENTERING_FISCAL_MODE = "MALÝ MODA\nGEÇECEK?";
 public const string ENTERED_FISCAL_MODE = "MALÝ MODA GEÇÝLDÝ";
 public const string REGISTER_NO = "REGISTER NO";
 public const string OFFICE_INDEX = "OFFICE PATH";
 public const string TIME_CHANGED = "TIME UPDATED";
 public const String BACKOFFICE_TIME_NOT_SET = "BACKOFFICE TIME\nCOULD NOT SET";
 public const string DATA_FILES_UPDATING = "DATA FILES\nUPDATING...";
 public const string DATA_FILES_UPDATED = "DATA FILES\nUPDATED";
 public const string PROGRAM_FILES_UPDATING = "PROGRAM FILES\nUPDATING...";
 public const string LAST_UPDATE = "LAST UPDATE";
 public const String TCP_IP_ADDRESS = "TCP/IP ADDRESS";
 public const string INVALID_TIME = "INVAID TIME";
 public const string REGISTER_FILES_TRANSFERRED = "POS FILES\nUPLOAD COMPLETE";
 public const string DISPLAY_COM_PORT = "DISPLAY COM PORT";
 public const string SLIP_COM_PORT = "INVOICE COM PORT";
 public const string NO_SLIP_PORT = "FATURA BAÐLANTISI\nTAKILI DEÐÝL";
 public const string SCALE_COM_PORT = "SCALE COM PORT";
 public const string BARCODE_COM_PORT = "BARCODE COM PORT";
 public const String SCALE_CONNECTION_ERROR = "SCALE CONNECTION\nERROR";
 public const String BARCODE_CONNECTION_ERROR = "BARCODE\nCONNECTION ERROR";
 public const string BRIGHTNESS_LEVEL = "BRIGHTNESS";
 public const string DISPLAY_TEST = "ABCDEFGHIJKLMNOPQRST\nUVWXYZ0123456789.:!@";
 public const string TAX_RATE_INTERVAL_ERROR = "VAT RATE 0-99\nRANGE";
 public const string LOADING_DEPARTMENT_AND_VAT_INFO = "DEPARTMENT AND VAT\nGÝRÝÞÝ YAPILIYOR";
 public const string LOADING_NEW_PROGRAM = "YENÝ PROGRAM DOSYASI\nYUKLENÝYOR...";
 public const string LOADING_FROM_REGISTER = "KASADAN GEREKLI\nBILGILER ALINIYOR";
 public const string LOADING_LOGO_INFO = "LOGO SATIRLARI\nGÝRÝLÝYOR";
 public const string INVALID_DATE_RANGE = "GÝRÝLEN TARÝH\nARALIÐI GEÇERSÝZ";
 public const string LOGO_DEPARTMENT_CHANGE_Z_REPORT_REQUIRED = "LOGO/DEPARTMAN DEÐ.\nZ RAPORU GEREKLI";
 public const String EXCEED_PRODUCT_LIMIT = "EXCEED PRODUCT\nLIMIT";
 public const String INVALID_SECURITY_KEY_EXCEPTION = "INVALID SECURITY\nKEY EXCEPTION";
 public const string RESETED_DISPLAY = "DISPLAY RESETED";
 public const string ENTER_VALUE = "ENTER VALUE";
 public const string CONTINUE = "CONTINUE";
 public const string NOT_TRANSFERED_DATA_FILES = "NOT TRANSFERED\nDATA FILES";
 public const string NOT_PROGRAM_INSTALLED = "NOT PROGRAM\nINSTALLED";
 public const string MAIN_DEPARTMENT = "MAIN DEPART.";
 public const string NO_USING = "NOT ENABLE";
 public const String SLIP = "SLIP";
 public const string UPDATE_ERROR = "FAIL ON UPDATE";
 public const string UPDATE_STATU = "POS UPDATING...";
 public const string UPDATE_ERROR_404 = "NO CONTENT";
 public const string UPDATE_ERROR_406 = "NO UPDATE";
 public const string UPDATE_ERROR_500 = "SERVER ERROR";
 public const String SCALE = "SCALE";
 public const String BARCODE = "BARCODE";
 public const String DISPLAY = "DISPLAY";
 public const string PROMOTION_CODE = "Promotion Code";
    public const string UPDATE_FOLDER_NOT_FOUND = "UPDATE FOLDER\nNOT FOUND";
    #endregion
    #region EJ MESSAGES
 public const string EJ_FULL = "EJ FULL";
 public const string EJ_CHANGED = "EJ CHANGED\nACTIVE EJ NO: ";
 public const string EJ_LIMIT_WARNING = "EJ USAGE ";
 public const string EJ_AVAILABLE_LINES = "EJ REMAINING LINES";
 public const string EJ_MISMATCH = "INVALID EJ";
 public const string EJ_FORMAT_ERROR = "EJ FORMAT ERROR";
 public const string EJ_NOT_AVAILABLE = "EJ UNAVAILABLE";
 public const string EJ_FISCALID_MISMATCH = "EJ FISCALID\nMISMATCH";
 public const string EJ_BITMAP_ERROR = "BITMAP ERROR";
 public const string EJ_ERROR_OCCURED = "EJ ERROR";
    #endregion
    #region "NUMBER TO WORD"
 public const string ONLY = "ONLY";
 public const string YTL = "€";
 public const string YKR = "C";
 public const string ZERO = "ZERO";
 public const string ONE = "ONE";
 public const string TWO = "TWO";
 public const string THREE = "THREE";
 public const string FOUR = "FOUR";
 public const string FIVE = "FIVE";
 public const string SIX = "SIX";
 public const string SEVEN = "SEVEN";
 public const string EIGHT = "EIGHT";
 public const string NINE = "NINE";
 public const string TEN = "TEN";
 public const string TWENTY = "TWENTY";
 public const string THIRTY = "THIRTY";
 public const string FOURTY = "FOURTY";
 public const string FIFTY = "FIFTY";
 public const string SIXTY = "SIXTY";
 public const string SEVENTY = "SEVENTY";
 public const string EIGHTY = "EIGHTY";
 public const string NINETY = "NINETY";
 public const string HUNDRED = "HUNDRED";
 public const string THOUSAND = "THOUSAND";
 public const string MILLION = "MILLION";
 public const string BILLION = "BILLION";
 public const string TL = " €";
 public const string KURUS = "CENTS";
    #endregion
    #region LOG KEYWORDS
 public const string SAT = "SAT";
 public const string IPT = "IPT";
 public const string ART = "ART";
 public const string IND = "IND";
 public const string SNS = "SNS";
 public const string NAK = "NAK";
 public const string DVZ = "DVZ";
 public const string KRD = "KRD";
 public const string TOP = "TOP";
 public const string PRM = "PRM";
 public const string MSG = "MSG";
 public const string NOT = "NOT";
 public const String KOD = "KOD";
 public const string END = "SON";
 public const String LMT = "LMT";
 public const String HAR = "HAR";
    #endregion
    #region "POS CLIENT TYPE"
 public const string WEBSERVICE = "WEBSERVICE";
 public const string TCPLISTENER = "TCP";
    #endregion
    #region "DIPLOMATICCUSTOMER RELATED"
 public const string CUSTOMER_IDENTITY = "KÝMLÝK NO";
    #endregion
    #region PARAMETEREXCEPTIONS
 public const string PARAMETER_EXCEPTION = "PARAMETRE HATASI";
 public const string NO_DOCUMENT_FOUND = "ARANILAN ÖZELLÝKTE\nBELGE BULUNMUYOR";
 public const string DATE_OUTOFRANGE = "TARÝH ARALIÐINDA\nBELGE BULUNMUYOR";
 public const string Z_OUTOFRANGE = "Z NUMARASI\nBULUNAMADI";
 public const string DOCUMENT_OUTOFRANGE = "ARANILAN BELGE NO\nBULUNAMADI";
 public const string ADDRESS_OUTOFRANGE = "BULUNAN ADRES\nARALIÐIN DIÞINDA";
 public const string PARAMETER_SEQUENCE_ERROR = "PARAMETRE SIRASI\nUYGUN DEÐÝL";
 public const string EXCESSIVE_PARAMETER_INTERVAL = "PARAMETRE ARALIÐI\nÇOK FAZLA";
 public const string TIME_LIMIT_ERROR = "SAAT DEÐÝÞÝKLÝÐÝ\n60 DAKÝKAYI AÞAMAZ";
 public const string TIME_ZREPORT_ERROR = "SAAT SON Z RAPORU\nONCESINE ALINAMAZ";
 public const string NO_PROPER_Z_FOUND = "ARANILAN DÖNEMDE\nMALÝ HFZ KAYDI YOK";
    #endregion
    #region SLIP (PRINTER) ERRORS
 public const string DOCUMENT_ID_NOTSET_ERROR = "BELGE NUMARASI\nYOK";
 public const string UNFIXED_SLIP_ERROR = "BELGE DÜZGÜN\nYERLEÞTÝRÝLMEMÝÞ";
 public const string SLIP_ROWCOUNT_EXCEED_ERROR = "BELGE KOYUP\nGÝRÝÞ'E BASINIZ";
 public const string REQUEST_SLIP_ERROR = "BELGE KOYUP\nBEKLEYÝNÝZ";
 public const string NEGATIVE_COORDINATE_ERROR = "FATURA KOORDINATLARI\nNEGATÝF OLAMAZ";
 public const string CUSTOMER_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜÞTERÝ-VERGÝ";
 public const string CUSTOMER_TIME_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜÞTERÝ-SAAT";
 public const string TIME_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nVERGÝ-SAAT";
 public const string CUSTOMER_DATE_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜÞTERÝ-TARÝH";
 public const string DATE_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nVERGÝ-TARÝH";
 public const string COORDINATE_OUTOF_INVOICE_ERROR = "BAZI KOORDÝNATLAR\nBELGEYÝ AÞMAKTADIR";
 public const string NAME_VAT_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nÜRÜN ADI-ÜRÜN KDV";
 public const string AMOUNT_VAT_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nÜRÜN KDV-ÜRÜN TUTAR";
 public const string PRODUCT_COORDINATE_ERROR = "ÜRÜN BAÞLANGIÇ\nSATIRI ÇOK KÜÇÜK";
 public const string SLIP_ERROR = "FATURA\nHATASI";
    #endregion SLIP ERRORS
    #region HUGINPOS ERRORS
 public const string LIMIT_EXCEED_ERROR = "LIMIT_EXCEED\nERROR";
 public const string OFFICE_PATH_ERROR = "OFFICE_PATH\nERROR";
 public const string OUTOF_QUANTITY_LIMIT = "OUTOF_QUANTITY\nLIMIT";
 public const string INVALID_QUANTITY = "INVALID_QUANTITY";
 public const string NO_ADJUSTMENT_EXCEPTION = "NO_ADJUSTMENT\nEXCEPTION";
 public const string NO_CORRECTION_EXCEPTION = "NO_CORRECTION\nEXCEPTION";
 public const string PROGRAM_HAS_CLOSED = "PROGRAM CLOSED";
    #endregion
    #region BARCODE MENU
 public const string RETURN_RECEIPT = "TÜM FÝÞÝ ÝADE AL";
 public const string RETURN_PRODUCT = "RETURN PRODUCT";
 public const string REPEAT_SALE = "SATIÞI TEKRARLA";
 public const string PRINT_RECEIPT_COPY = "FÝÞ KOPYASI BAS";
 public const String CONFIRM_PRINT_RECEIPT_COPY = "FİŞ KOPYASI\n BAS?";
 public const string RECEIPT_NOT_BELONG_TO_CASE = "FÝÞ KASAYA\n AÝT DEÐÝL";
    #endregion
    #region CONTACTLESS MESSAGES

 public const string CONTACTLESS_CARD_ERROR = "TEMASSIZ\nKART HATASI";
 public const string CONFIRM_UPDATE_POINTS = "PUAN KAYDEDÝLEMEDÝ\nTEKRAR DENE?";
 public const string MISSING_CARD_INFO = "KART BÝLGÝLERÝ EKSÝK\nYÜKLENSÝN MÝ?";
 public const string CARD_MISMATCH = "AYNI KARTI\nKOYMALISINIZ";
    #endregion
    #region PUAN MESSAGES

    public const string CUSTOMER_NOTIN_POINT_DB = "MÜÞTERÝ KAYDI\nVERÝTABANINDA YOK";
    public const string CARDSERIAL_NOTIN_POINT_DB = "BU KART SERÝSÝ\nVERÝTABANINDA YOK";
    public const string FAIL_ON_POINT_UPDATE = "PUAN VERÝTABANINA\nKAYDEDÝLEMEDÝ";
    public const string FAIL_ON_CARDSERIAL_INSERT = "KART SERÝSÝ\nKAYDEDÝLEMEDÝ";
    public const string CARDSERIAL_ALREADY_EXISTS = "VERÝTABANINDA\nBU SERI NUMARASI VAR";
    public const String POINT = "POINT";
    public const String EARNED_POINT = "KAZANILAN PUAN";
    public const String USED_POINT = "HARCANAN PUAN";
    public const String RETURNED_POINT = "İADE ALINAN PUAN";
    public const String TOTAL_POINT = "TOPLAM PUAN";
    public const String DEAR_CUSTOMER = "SAYIN";
    public const String CUSTOMER_CODE_SHORTENED = "M. KODU";
    public const String TOTAL_REDUCTION = "TOPLAM İNDİRİM";
    public const String SUBTOTAL_REDUCTION = "ARATOP. İNDİRİMİ";
    public const String PRODUCT_REDUCTION = "ÜRÜN İNDİRİMİ";
    public const String TOTAL_FEE = "TOTAL FEE";
    public const String SUBTOTAL_FEE = "SUB TL FEE";
    public const String PRODUCT_FEE = "PRODUCT FEE";
    public const String MATCH_EFT_POS = "EFT POS EŞLEME";
    #endregion
    #region CUSTOMER FORM MESSAGE
     public const string CLOSE_PROGRAM = "ERROR OCCURED\nPROGRAM IS CLOSING";
     public const string RESTART_PROGRAM = "PROGRAM WILL BE\nRESTART";
     public const string RESTART_COMPUTER = "COMPUTER WILL\nBERESTART";
     public const string PORT_IN_USE = "PORT IN USE";
     public const string MISSING_REFERENCE_FILE = "MISSING\nREFERENCE FILE";
 public const string PLU_PAGE = "PLU Oldal";
 public const string LED_ONLINE = "Online";
     public const string LED_FAST_PAYMENT = "Rendelés";
     public const string LED_SALE = "Vásárlás";
     public const string LED_RETURN = "?Return";
     public const string LED_CUSTOMER = "Ügyfél";
     public const string CUSTOMER = "ÜGYFÉL";
 public const string PROGRAM = "PROGRAM";
     public const string REPORT = "JELENTÉS";
     public const string ENTER = "BELÉPÉS";
     public const string UNDO = "JAVÍT";
     public const string CASH_DRAWER = "FIÓK";
     public const string PAPER_FEED = "PAPÍR";
     public const string AMOUNT = "ÖSSZEG";
     public const string ESCAPE = "KILÉPÉS";
     public const string PRICE = "ÁR";
     public const string DOCUMENT = "NYUGTA";
     public const string SUB_TL = "RÉSZÖS.";
     public const string COMMAND = "MÜKÖDÉS";
     public const string REPEAT = "REPEAT";
    public const string CASH_PAYMENT = "TOTAL";
    public const string PAYOUT = "PO";
    public const string RECEIVE_ACCT_ON_PAYMENT = "RO";
    public const string DELETE = "Clr";
    public const string NO_PRODUCT = "NO PRODUCT";
    public const String PAID_IS_DONE_TY = "PAID IS DONE\nTHANKS";
    public const String WAITING_PAYMENT = "ÖDEME\nBEKLENİYOR...";
    #endregion CUSTOMER FORM MESSAGE
    
    #region EFTPOS
     public const string FAILED_PROVISION_ACCEPT_PAYMENT = "FAILED PROVISION\nACCEPT PAYMENT";
     public const string PROCESS_REJECTED = "PROCESS REJECTED";
     public const string EFT_TIMEOUT_ERROR = "EFT TIMEOUT ERROR";
     public const string EFT_POS_ERROR = "EFT POS ERROR";
     public const String ACCEPT_PAYMENT_OR_REPEAT_VIA_EFT = "ENTER: PAID FINISH\nREPEAT: REPEAT VIA EFT";
     public const String PROCESS_START_WAITING_EFT_POS = "PAYMENT START\nPLEASE WAIT...";
    public const String EFT = "EFT";
    #endregion EFTPOS

    #region Document Status List Messages
        public const String DOCUMENT_STATUS = "DOCUMENT STATUS";
        public const String OPEN = "OPEN";
        public const String CLOSED = "CLOSED";
    #endregion

    #region System Manager List Messages
        public const String SYSTEM_MANAGER = "SYSTEM MANAGER";
        public const String RESTART_POS = "RESTART PROGRAM";
        public const String SHUTDOWN_SYSTEM = "SHUTDOWN SYSTEM";
        public const String RESTART_SYSTEM = "RESTART SYSTEM";
        public const String CONFIRM_RESTART_PROGRAM = "CONFIRM RESTART\nPROGRAM?";

    #endregion

    #region Tallying Menu

        public const String TALLYING = "TALLYING";
        public const String PRODUCT_QUANTITY = "PRODUCT QUANTITY";
        public const String PRODUCT_BARCODE = "PRODUCT BARCODE";
        public const String PRODUCT_NOT_WEIGHABLE = "PRODUCT NOT\nWEIGHABLE";
        public const String CONFIRM_EXIT_TALLYING = "EXIT TALLYING?";

    #endregion
    #region PROGRAM MENU MESSAGES

     public const String ENTER_CONFIG_1 = "ENTER CONFIG 1";
     public const String ENTER_CONFIG_2 = "ENTER CONFIG 2";
     public const String ENTER_CONFIG_3 = "ENTER CONFIG 3";
     public const String ENTER_CONFIG_4 = "ENTER CONFIG 4";
     public const String ENTER_CONFIG_5 = "ENTER CONFIG 5";

     public const String PM_CONFIG = "UPDATE DATA";
     public const String PM_CONFIG_1 = "UPDATE VAT";
     public const String PM_CONFIG_2 = "UPDATE DEPT.";
     public const String PM_CONFIG_3 = "UPDATE PRODUCT";
     public const String PM_CONFIG_4 = "UPDATE LOGO";
     public const String PM_CONFIG_5 = "UPDATE CASHIER";
     public const String CASHIER_AUTHO_EXC = "LOW AUTHORAZATİON\nFOR FPU UPDATE";
    public const String RE_MATCHED_WİTH_FPU = "RE-MATCHED WITH\nFPU (ENTER)";
    public const String MANAGER_AUTHO_EXC = "MANAGER AUTH\nNOT ENOUGH";

    public const String TABLE_NUMBER = "TABLE NO";
    public const String CLOSE_TABLE = "CLOSE TABLE";
    public const String CONFIRM_SEND_ORDER = "SEND ORDER?";
    public const String MOVE_TO_EFT_POS_SIDE = "PLEASE MOVE TO\nEFT-POS SIDE..";
    public const String SWITCH_MANAGER = "SWITCH MANAGER";

    public const String MENU_START_FM_TEST = "FM TEST BAŞLAT";

    public const String WAIT_FOR_MATCHING = "EŞLEME YAPILIYOR\nBEKLEYİNİZ..";
    public const String PRESS_ENTER_TO_CONNECT = "BAĞLANMAK İÇİN\nGİRİŞ'E BASINIZ";

    public const String FILE_NAME = "FILE NAME";
    #endregion

 }
#else
    public static class PosMessage
    {
    #region Login and Alert
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_OPERATION = "İŞLEM HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_PASSWORD = "ŞİFRE YANLIŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_CASHIER_ID = "KASİYER NO GEÇERSİZ";
        public const String INVALID_MANAGER_ID = "YÖNETİCİ NO GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_CASHIER = "TANIMSIZ KASİYER";
        public const String INVALID_MANAGER = "TANIMSIZ YÖNETİCİ";
        /// <summary>
        /// 
        /// </summary>
        public const String PROMPT_RETRY = "TEKRAR DENEYİNİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String REGISTER_LOCKED = "KASA KİLİTLENDİ";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_PASSWORD = "KASİYER ŞİFRESİ";
        public const String ENTER_PASS_MNGR = "YÖNETİCİ ŞİFRESİ";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_CASHIER_ID = "KASİYER NO";
        public const String ENTER_MANAGER_ID = "YÖNETİCİ NO";
        /// <summary>
        /// 
        /// </summary>
        public const String ACCESS_LEVEL = "YETKİ SEVİYESİ";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_ACCESS_RIGHT = "YETKİLİ DEĞİLSİNİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String AUTHORIZED_PASSWORD = "YETKİLİ ŞİFRESİ";
        /// <summary>
        /// 
        /// </summary>
        public const String UNDEFINED_KEY = "TANIMSIZ TUŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT_FILE_LOAD_ERROR = "ÜRÜN DOSYASINDA\nHATA OLUŞTU";
        /// <summary>
        /// 
        /// </summary>
        public const String PROGRAM_FILE_LOAD_ERROR = "PROGRAM DOSYASINDA\nHATA OLUŞTU";
        /// <summary>
        /// 
        /// </summary>
        public const String CUSTOMER_FILE_LOAD_ERROR = "MÜŞTERİ DOSYASINDA\nHATA OLUŞTU";
        /// <summary>
        /// 
        /// </summary>
        public const String CASHIER_FILE_LOAD_ERROR = "KASİYER DOSYASINDA\nHATA OLUŞTU";
        /// <summary>
        /// 
        /// </summary>
        public const String CURRENCY_FILE_LOAD_ERROR = "DOVİZ KUR DOSYASINDA\nHATA OLUŞTU";
        /// <summary>
        /// 
        /// </summary>
        public const String PROGRAM_UNLOADED = "PROGRAM BİLGİLERİ\nYÜKLENEMEDİ";
        /// <summary>
        /// 
        /// </summary>
        public const String PASSWORD_INVALID = "HATALI ŞİFRE";
        /// <summary>
        /// 
        /// </summary>
        public const String CASHIER_ALREADY_ASSIGNED_EXCEPTION = "KASİYER GİRİŞİ\nZATEN YAPILMIŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String CASHIER_LOGOUT = "KASİYER\nÇIKIŞI";


        public const String CASHIER_LOGIN_REQUIRED = "KASİYER GİRİŞİ\nGEREKİYOR";

        public const String CASHIER = "KASİYER";

        public const String MANAGER = "YÖNETİCİ";

        public const String STARTUP_MESSAGE = "KASA AYARLARI\nYÜKLENİYOR...";

        public const String INVALID_PASSWORD_TRY_AGAIN = "GEÇERSİZ ŞİFRE\nTEKRAR DENEYİNİZ";

        public const String SUCCESS_PROCESS = "İŞLEM BAŞARILI";

        public const String NO_DEFINED_MANAGER = "TANIMLI YÖNETİCİ YOK";

        public const String DEFAULT_LOGIN = "ÖNTANIMLI GİRİŞ? ";

        public const String FPU_SETTINGS_CHECK = "KASA AYARLARI\nKONTROL EDİLİYOR...";
    #endregion

    #region Sales
        /// <summary>
        /// 
        /// </summary>
        public const String NOT_SELLING = "SATIŞ YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_NUMBER = "SAYI GİRİŞİ";

        public const String ENTER_TCKN_VKN = "TCKN/VKN GİRİŞİ";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_AMOUNT = "TUTAR GİRİŞİ";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_UNIT_PRICE = "BİRİM FİYAT GİRİŞİ";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT_NOTFOUND = "ÜRÜN BULUNAMADI";

        public const String SALE_NOT_FOUND = "GEÇERLİ SATIŞ\nBULUNMAMAKTADIR";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT_LIMIT_EXEED = "ÜRÜN SATIŞ\nSAYISI AŞILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String SERIAL_NUMBER_NOT_FOUND = "SERİ NUMARASI\nBULUNAMADI!";
        /// <summary>
        /// 
        /// </summary>
        public const String RECEIVE_PAYMENT = "ÖDEME YAPILIYOR";
        public const String LAST_PAYMENT = "EN FAZLA {0} ÖDEME\nKALAN TUTAR ÖDENMELİ";
        /// <summary>
        /// 
        /// </summary>
        public const String PROMPT_FINALIZE_SALE = "SATIŞI KAPATINIZ";
        /// <summary>
        /// 
        /// </summary>
        public const String BARCODE_NOTFOUND = "BARKOD BULUNAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String PLU_NOTFOUND = "PLU BULUNAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String BARCODE_ERROR = "BARKOD HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String BARCODE_WEIGHT_ERROR = "BARKOD GRAMAJ HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String BARCODE_QUANTITY_ERROR = "BARKOD ADET HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String BARCODE_TOTAL_AMOUNT_ERROR = "BARKOD TUTAR HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String RECEIPT_LIMIT_EXCEEDED = "FİŞ LİMİTİ AŞILIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String TRANSFER_TO_INVOICE = "FATURAYA AKTAR";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT_PRICE_NOTFOUND = "ÜRÜN FİYATI YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_PRODUCT = "ÜRÜN BİLGİSİ HATALI";
        /// <summary>
        /// 
        /// </summary>
        public const String UNKNOWN_ERROR = "HATA OLUŞTU";
        /// <summary>
        /// 
        /// </summary>
        public const String PRICE_LOOKUP = "FİYAT GÖR";
        /// <summary>
        /// 
        /// </summary>
        public const String UNIT_PRICE = "BİRİM FİYAT";
        /// <summary>
        /// 
        /// </summary>
        public const String SUBTOTAL = "ARATOPLAM";
        /// <summary>
        /// 
        /// </summary>
        public const String DISCOUNTED = "İNDİRİMLİ";
        /// <summary>
        /// 
        /// </summary>
        public const String CLERK_ID = "SATICI KODU";
        /// <summary>
        /// 
        /// </summary>
        public const String SELECT_PRODUCT = "ÜRÜNÜ SEÇİN";
        /// <summary>
        /// 
        /// </summary>
        public const String REGISTER_CLOSED = "KASA KAPALI";
        /// <summary>
        /// 
        /// </summary>
        public const String PLEASE_WAIT = "LÜTFEN BEKLEYİN...";
        /// <summary>
        /// 
        /// </summary>
        public const String TOTAL_AMOUNT = "TOPLAM TUTAR";
        /// <summary>
        /// 
        /// </summary>
        public static String WELCOME = "MAĞAZAMIZA\n    HOŞGELDİNİZ";
        /// <summary>
        /// 
        /// </summary>
        public static String WELCOME_LOCKED = "MAĞAZAMIZA\n    HOŞGELDİNİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CLERK = "SATICI";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT = "ÜRÜN";
        /// <summary>
        /// 
        /// </summary>
        public const String VAT_DISTRIBUTION = "VERGİ DAĞILIMI";

        public const String VAT_RATE_DIFFERENT = "KDV ORANLARI\nUYUŞMUYOR";

        public const String VAT_RATE_UPDATING = "KDV ORANLARI\nGÜNCELLENİYOR..";

        public const String END_OF_RECEIPT_NOTE_SAVING = "FİŞ SONU BİLGİLERİ\nKAYDEDİLİYOR..";

        public const String CREDITS_DIFFERENT = "KREDİ DEĞERLERİ\nFARKLI";

        public const String CREDITS_UPDATING = "KREDİ DEĞERLERİ\nGÜNCELLENİYOR..";
        /// <summary>
        /// 
        /// </summary>
        public const String ZERO_PLU_PRICE_ERROR = "ÜRÜN FİYATI \n SIFIR OLAMAZ";
        /// <summary>
        /// 
        /// </summary>
        public const String ZERO_DRAWER_IN_ERROR = "KASA GİRİŞ TUTARI\nSIFIR OLAMAZ";
        /// <summary>
        /// 
        /// </summary>
        public const String ZERO_DRAWER_OUT_ERROR = "KASA ÇIKIŞ TUTARI\nSIFIR OLAMAZ";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_PRODUCT_PRICE = "ÜRÜN FİYATI GİRİŞİ";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_PRODUCT_SERIALNO = "ÜRÜN SERİ NO GİRİŞİ";
        /// <summary>
        /// 
        /// </summary>
        public const String SELLING_VAT = "KDV";
        /// <summary>
        /// 
        /// </summary>
        public const String DOCUMENT_TOTAL_AMOUNT_LIMIT_EXCEED_ERROR = "BELGE TUTAR\nLİMİTİ AŞILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String ITEM_TOTAL_AMOUNT_LIMIT_EXCEED_ERROR = "KALEM TUTAR\nLİMİTİ AŞILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String VOID_AMOUNT_INVALID = "İPTAL TUTARI\nGEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CANNOT_VOID_NO_PROPER_SALE = "UYGUN SATIŞ YOK\nİPTAL GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String DECIMAL_LIMIT = "KÜSÜRAT SINIRI";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_SALE_INVALID_ACTION = "SATIŞ YOK\nİŞLEM GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String INSUFFICIENT_LIMIT = "LİMİTİ\nYETERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String VOID_SALESPERSON = "SATICIYI İPTAL ET?";
        /// <summary>
        /// 
        /// </summary>
        public const String UNDEFINED_LABEL = "TANIMSIZ ETİKET";
        /// <summary>
        /// 
        /// </summary>
        public const String NOT_ENOUGH_CHARS_FOR_PRICECHECK = "FİYAT GÖR İÇİN EN AZ\n2 KARAKTER GİRİN";
        /// <summary>
        /// 
        /// </summary>
        public const String WAIT_DOCUMENT_TRANSFER = "LÜTFEN BEKLEYİN\nAKTARIM YAPILIYOR...";
        /// <summary>
        /// 
        /// </summary>
        public const String CLERK_FOR_ITEM = "SATICI (ÜRÜN)\n";
        /// <summary>
        /// 
        /// </summary>
        public const String CLERK_NOT_FOUND = "SATICI BULUNAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String CASHIER_NOT_FOUND = "KASİYER BULUNAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String CLERK_FOR_DOCUMENT = "SATICI (TOPLAM)\n";
        /// <summary>
        /// 
        /// </summary>
        public const String GAINS="KAZANCINIZ";
        /// <summary>
        /// 
        /// </summary>
        public const String MIN_AMOUNT_ERROR = "TUTAR EN AZ\n0,01 OLABİLİR";
        /// <summary>
        /// 
        /// </summary>
        public const String SERIAL_NUMBER_ALREADY_EXIST = "AYNI SERI NUMARALI\nÜRÜN BULUNMAKTA";
        /// <summary>
        /// 
        /// </summary>
        public const String SALE_FROM_QR_CODE = "LÜTFEN KAREKODU\nOKUTUNUZ";

        public const String PAID_IS_DONE_TY = "ÖDEME ALINMIŞTIR\nTEŞEKKÜRLER";

        public const String WAITING_PAYMENT = "ÖDEME\nBEKLENİYOR...";
    #endregion

    #region ElectronicJournalError

        public const String NO_EJ_AREA_FOR_OPERATION = "BU İŞLEM İÇİN EKÜDE\nYETERLİ ALAN YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String FIX_VALID_EJ_TO_VOID_DOCUMENT = "BELGE iPTALi iÇiN\nGEÇERLi EKÜ TAKINIZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_NEW_EJ_FORMAT = "YENİ EKÜ TAKILDI\nFORMATLA?";
        /// <summary>
        /// 
        /// </summary>
        public const String ZREPORT_NECCESSARY_FOR_NEW_EJ = "Z RAPORU ALMADAN\nEKÜ DEĞİŞTİRİLEMEZ";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_ZREPORT_IN_EJ = "EKÜ'DE Z RAPORU YOK\niŞLEM GEÇERSiZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_ZREPORT_ON_FULL_EJ = "EKÜ DOLU\nZ RAPORU ALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_PASIVE_ONLY_EJ_REPORTS = "EKÜ PASiF\nSADECE EKÜ RAPORU";
        public const String EJ_PASIVE_VALID_EJ_REQUIRED = "EKÜ PASiF\nGEÇERLİ EKÜ TAKINIZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CONNECTING_TO_PRINTER = "BAGLANTI\nKURULUYOR...";

    #endregion ElectronicJournalError

    #region Documents
        /// <summary>
        /// 
        /// </summary>
        public const String SELECT_DOCUMENT = "BELGE SEÇİMİ";
        /// <summary>
        /// 
        /// </summary>
        public const String TRANSFER_DOCUMENT = "BELGE AKTARIM";
        /// <summary>
        /// 
        /// </summary>
        public const String RECEIPT = "FIS";
        public const String RECEIPT_TR = "FİŞ";
        /// <summary>
        /// Turkce karakter kullanilmamali (hareket dosyasi)
        /// </summary>
        public const String WAYBILL = "IRSALIYE";
        public const String WAYBILL_TR = "İRSALİYE";
        /// <summary>
        /// Turkce karakter kullanilmamali (hareket dosyasi)
        /// </summary>
        public const String INVOICE = "FATURA";
        /// <summary>
        /// Turkce karakter kullanilmamali (hareket dosyasi)
        /// </summary>
        public const String RETURN_DOCUMENT = "IADE";
        public const String RETURN_DOCUMENT_TR = "İADE";

        public const String E_INVOICE = "E-FATURA";
        public const String E_ARCHIVE = "E-ARŞİV";
        public const String MEAL_TICKET = "YEMEK FİŞİ";
        public const String CAR_PARKIMG = "OTOPARK";
        public const String ADVANCE = "AVANS";
        public const String COLLECTION_INVOICE = "FATURA TAHSİLATI";
        public const String CURRENT_ACCOUNT_COLLECTION = "CARİ HESAP TAHSİLATI";
        public const String SELF_EMPLOYEMENT_INVOICE = "S. MESLEK MAKBUZU";


        public const String HR_CODE_RETURN = "IAD";
        public const String HR_CODE_WAYBILL = "IRS";
        public const String HR_CODE_INVOICE = "FAT";
        public const String HR_CODE_RECEIPT = "FIS";
        public const String HR_INTER_CODE_RETURN = "GPS";
        public const String HR_CODE_E_INVOICE = "EFA";
        public const String HR_CODE_E_ARCHIVE = "EAR";
        public const String HR_CODE_MEAL_TICKET = "YEM";
        public const String HR_CODE_CAR_PARKING = "OTO";
        public const String HR_CODE_ADVANCE = "AVA";
        public const String HR_CODE_COLLECTION_INVOICE = "TAH";
        public const String HR_CODE_CURRENT_ACCOUNT_COLLECTION = "CHT";
        public const String HR_CODE_SELF_EMPLOYEMENT_INVOICE = "SMM";
        /// <summary>
        /// Turkce karakter kullanilmamali (hareket dosyasi)
        /// </summary>
        public const String ORDER = "SIP";
        public const String ORDER_TR = "SİPARİŞ";
        public const String CONFIRM_SEND_ORDER = "SİPARİŞİ GÖNDER?";
        /// <summary>
        /// Turkce karakter kullanilmamali (hareket dosyasi)
        /// </summary>
        public const String DOCUMENT_ID_NOT_FOUND = "BELGE NO\nBULUNAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String CHANGE_DOCUMENT = "AKTİF BELGE\n{0} YAPILACAK";
        /// <summary>
        /// 
        /// </summary>
        public const String NOT_ENOUGH_MONEY_IN_REGISTER = "KASADA YETERLİ\nMİKTAR YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String TRANSFER_STARTED_PLEASE_WAIT = "AKTARIM BAŞLADI\nLÜTFEN BEKLEYİNİZ..";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_AUTHORIZATION_FOR_SPECIAL_PRICE_SALES = "FİYATLI SATIŞ\nYETKİSİ YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_AUTHORIZATION_FOR_LOAD_SETTINGS = "PROGRAMLAMA YETKİSİ\nGEREKLİ <GİRİŞ>";
        /// <summary>
        /// 
        /// </summary>
        public const String RECEIPT_LIMIT_EXCEEDED_TRANSFER_DOCUMENT = "FİŞ LİMİTİ AŞILIYOR\nFATURAYA AKTAR";
        /// <summary>
        /// 
        /// </summary>
        public const String RECEIPT_LIMIT_DOCUMENT_TRANSFER_NOT_ALLOWED = "FİŞ LİMİTİ SINIRI\nAKTARIM YAPILAMIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String FRACTIONAL_QUANTITY_NOT_ALLOWED = "MİKTARLI ÜRÜN\nKÜSÜRAT GİRİLEMEZ";
        /// <summary>
        /// 
        /// </summary>
        public const String DEPOSITED_AMOUNT = "AKTARILAN TOPLAM";
        /// <summary>
        /// 
        /// </summary>
        public const String COORDINATE_ERROR = "KOORDİNATLAR\nHATALI";
        /// <summary>
        /// 
        /// </summary>
        public const String DOCUMENT_CHANGE_ERROR = " AKTARIMI\nYAPILAMAZ";
        /// <summary>
        /// 
        /// </summary>
        public const String DOCUMENT_FOLLOWING_ID = "TAKİP NO";
        /// <summary>
        /// 
        /// </summary>
        public const String SLIP_SERIAL = "BELGE SERİ (AA)";
        /// <summary>
        /// 
        /// </summary>
        public const String SLIP_ORDER_NO = "SIRA NO (123456)";
        /// <summary>
        /// 
        /// </summary>
        public const String RETURN_REASON = "IADE NEDENI";

    #endregion Documents

    #region Discount
        public const String DISCOUNT_LIMIT_EXCEEDED = "İNDİRİM LİMİTİ AŞILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String DISCOUNT_NOT_ALLOWED = "İNDİRİM YAPILAMAZ";
        /// <summary>
        /// 
        /// </summary>
        public const String DISCOUNT = "İSKONTO ";
        /// <summary>
        /// 
        /// </summary>
        public const String REDUCTION = "İNDİRİM";
        /// <summary>
        /// 
        /// </summary>
        public const String DNEY_PERCENTDISCOUNT = "KÜSÜRATLI YÜZDE\nİND/ART YAPILAMAZ";
        /// <summary>
        /// 
        /// </summary>
        public const String DNEY_PERCENT_OVER_AMOUNT = "YÜZDE LİMİT\nAŞILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String INSUFFICIENT_ACCESS_LEVEL = "YETKİNİZ YETERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT_PERCENT_DISCOUNT = "ÜRÜN YÜZDE İNDİRİMİ";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT_PRICE_DISCOUNT = "ÜRÜN TUTAR İNDİRİMİ";
        /// <summary>
        /// 
        /// </summary>
        public const String SUBTOTAL_PERCENT_DISCOUNT = "ARATOPLAM İNDİRİMİ";
        /// <summary>
        /// 
        /// </summary>
        public const String SUBTOTAL_PRICE_DISCOUNT = "ARATOPLAM İNDİRİMİ";
        /// <summary>
        /// 
        /// </summary>
        public const String DISCOUNT_ALLOWED = "İNDİRİM YAPILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String COUNT_ALLOWED = "ARTIRIM YAPILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String CORRECTION = "DÜZELTME";
    #endregion

    #region Fee
        public const String FEE_LIMIT_EXCEEDED = "ARTIRIM LİMİTİ AŞILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT_PERCENT_FEE = "ÜRÜN YÜZDE ARTIRIMI";
        /// <summary>
        /// 
        /// </summary>
        public const String PRODUCT_PRICE_FEE = "ÜRÜN TUTAR ARTIRIMI";
        /// <summary>
        /// 
        /// </summary>
        public const String FEE = "ARTIRIM";
        /// <summary>
        /// 
        /// </summary>
        public const String SUBTOTAL_PRICE_FEE = "ARATOPLAM ARTIRIM";
        /// <summary>
        /// 
        /// </summary>
        public const String SUBTOTAL_PERCENT_FEE = "ARATOPLAM ARTIRIM";
        /// <summary>
        /// 
        /// </summary>
        public const String FEE_NOT_ALLOWED = "ARTIRIM YAPILAMAZ";
        /// <summary>
        /// 
        /// </summary>
    #endregion

    #region Void
        public const String VOID_AMOUNT_EXCEEDED = "İPTAL TUTARI\nGEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String VOID_INVALID = "İPTAL GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_LOGOUT = "ÇIKIŞ İÇİN\nGİRİŞ'E BASIN";
        /// <summary>
        /// 
        /// </summary>
        public const String PROMPT_ENTER = "GİRİŞ'E BASIN";
        /// <summary>
        /// 
        /// </summary>
        public const String VOID = "İPTAL";
        /// <summary>
        /// 
        /// </summary>
        public const String VOID_FIND_PRODUCT = "İPTAL\nÜRÜN BUL";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_LIST_FOR_SPECIAL_PRICED_PRODUCT_VOID = "FİYATLI ÜRÜN İPTALİ\nİÇİN LİSTEYE GİR?";
        /// <summary>
        /// 
        /// </summary>
        public const String CANNOT_ASSIGN_CLERK_TO_VOID_SALE = "ÜRÜN İPTALİNE\nSATICI ATANAMAZ";
        public const String DOCUMENT_VOID_COUNT = "İPTAL ADEDİ";

        public const String CANNOT_VOID_ITEM = "ÜRÜN İPTALİ\nYAPILAMAZ";

    #region End Receipt
        /// <summary>
        /// 
        /// </summary>
    #endregion
        /// <summary>
        /// 
        /// </summary>
        public const String EXEMPT_TAX_TOTAL = "MUAF KDV TOPLAMI";
         public const string EXEMPT_TAX = "MUAF KDV";
        /// <summary>
        /// 
        /// </summary>
        public const String TOTAL = "TOPLAM";
        /// <summary>
        /// 
        /// </summary>
        public const String TOTALTAX = "TOPKDV";
        /// <summary>
        /// 
        /// </summary>
        public const String TOTAL_BOLD = "²TOPLAM³";
        public const String SHORT_TOTAL_BOLD = "²TOP³";
        /// <summary>
        /// 
        /// </summary>
        public const String TOTALTAX_BOLD = "²TOPKDV³";
        public const String TAX_BOLD = "²KDV³";
        /// <summary>
        /// 
        /// </summary>
        public const String OTHER = "DİĞER";
        /// <summary>
        /// 
        /// </summary>
        public const String CASH_RECEIPT = "TL TAHSİLAT";
        /// <summary>
        /// 
        /// </summary>
        public const String CURRENCY_RECEIPT = "DÖVİZ KARŞILIĞI TL";
        /// 
        /// </summary>
        public const String CREDIT_RECEIPT = "KREDİ TAHSİLAT";
        /// <summary>
        /// 
        /// </summary>
        public const String CHECK_RECEIPT = "ÇEK TAHSİLAT";
        /// <summary>
        /// 
        /// </summary>
        public const string COLLECTION = "TAHSİLAT"; 
        /// <summary>
        /// 
        /// </summary>
        public const String CURRENCY_CASH_EXCHANGE = "TL KARŞ.";
        /// <summary>
        /// 
        /// </summary>
        public const String SALE = "SATIŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String VAT = "KDV";
        /// <summary>
        /// 
        /// </summary>
        public const String PLUS = "+";
        /// <summary>
        /// 
        /// </summary>
        public const String MINUS = "-";
    #endregion

    #region Program Menu
        /// <summary>
        /// 
        /// </summary>
        public const String PM_FISCALIZATION = "MALİ AÇILIŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_VERSION = "SÜRÜM";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_REGISTER = "KASA TANIMI";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_DATETIME = "SAAT AYARI";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_DATAFILES = "DATA DOSYALARI";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_REGISTERFILES = "KASA DOSYALARI";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_HARDWARE = "DONANIM BAĞLANTISI";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_COMPORTSETTINGS = "COM PORT AYARLARI";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_RESETDISPLAY = "GÖSTERGE RESETLE";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_LOADBITMAP = "GRAFİK LOGO YÜKLE";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_NEWPROGRAM = "YENİ PROGRAM";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_LOAD_NEW_PROGRAM = "YENİ PROGRAM YÜKLE?\n(GiRiŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String PROGRAM_DATA_LOAD_ERROR = "PROGRAM BİLGİLERİ\nEKSİK VEYA HATALI";
        /// <summary>
        /// 
        /// </summary>
        public const String PROGRAM_ERROR = "PROGRAM HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String CURRENCY_LIMIT_EXCEEDED_PAYMENT_INVALID = "DOVİZ LİMİTİ AŞILDI\nÖDEME GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_BARCODE_TERMINATOR = "BARKOD OK. TANIMLAMA";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_BARCODE = "BARKODU OKUTUNUZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_BARCODE_TERMINATOR = "BARKOD AYRACI?\n(GiRiŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_BUZZER_ON_OFF = "TUŞ SESİ AÇ/KAPAT";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_BUZZER_ON = "TUŞ SESİ AÇ?\n(GiRiŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_BUZZER_OFF = "TUŞ SESİ KAPAT?\n(GiRiŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_TEST_EJ = "EKÜ'YÜ TEST ET?";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_CONFIG_DATA = "DATA GUNCELLE";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_CONFIG_VAT_RATE = "KDV GUNCELLE";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_CONFIG_DEPARTMENT = "DEPT. GUNCELLE";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_CONFIG_PRODUCT = "URUN GUNCELLE";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_CONFIG_LOGO = "LOGO GUNCELLE";
        /// <summary>
        /// 
        /// </summary>
        public const String PM_CONFIG_CASHIER = "KASIYER GUNCELLE";

        public const String PM_CONFIG_MANAGER = "YÖNETİCİ GÜNCELLE";

        public const String PM_CONFIG_GMP_PORT = "GMP PORT AYARLARI";

        public const String PM_UPDATE_CATEGORY = "KATEGORİ GÜNCELLE";

        public const String PM_NETWORK_SETTINGS = "AĞ AYARLARI";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_CONFIG_1 = "ENTER CONFIG 1";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_CONFIG_2 = "ENTER CONFIG 2";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_CONFIG_3 = "ENTER CONFIG 3";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_CONFIG_4 = "ENTER CONFIG 4";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_CONFIG_5 = "ENTER CONFIG 5";
        public const String PM_MATCH_EFT_POS = "EFT POS EŞLEME";
        public const String WAITING_FOR_DEVICE_MATCHNG = "EŞLEŞTİRME İÇİN\nCİHAZ BEKLENİYOR..";
        public const String OP_ENDED = "İŞLEM\nSONLANDIRILDI";
        public const String MATCHING_FINISHED_ESCAPE = "EŞLEME TAMAMLANDI\n(ÇIKIŞ?)";
        public const String SAVE_SETTINGS = "AYARLARI KAYDET";
        public const String SAVED_SUCCESFULLY = "AYARLAR BAŞARIYLA\nKAYDEDİLDİ!";
        public const String GMP_IP = "GMP IP";
        public const String GMP_PORT = "GMP PORT";

    #endregion

    #region Payment
        /// <summary>
        /// 
        /// </summary>
        public const String BALANCE = "KALAN";
        /// <summary>
        /// para
        /// </summary>
        public const String CASH = "NAKİT";
        /// <summary>
        /// 
        /// </summary>
        public const String FOREIGNCURRENCY = "DÖVİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String TURKISH_LIRA = "TL";
        /// <summary>
        /// 
        /// </summary>
        public const String CHECK = "ÇEK";
        /// <summary>
        /// 
        /// </summary>
        public const String CREDIT = "KREDİ";
        /// <summary>
        /// 
        /// </summary>
        public const String PAYMENT = "ÖDEME";
        /// <summary>
        /// 
        /// </summary>
        public const String TICKET = "TICKET";
        /// <summary>
        /// 
        /// </summary>
        public const String PAYMENT_INVALID = "ÖDEME GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_SALE_PAYMENT_INVALID = "SATIŞ YOK\nÖDEME GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CHECK_ID = "ÇEK NUMARASI";
        /// <summary>
        /// 
        /// </summary>
        public const String INSTALLMENT_COUNT = "TAKSİT SAYISI";
        /// <summary>
        /// 
        /// </summary>
        public const String INSTALLMENT = "TAKSİT";
        /// <summary>
        /// 
        /// </summary>
        public const String PAYMENT_INFO = "ÖDEME BİLGİLERİ";
        /// <summary>
        /// 
        /// </summary>
        public const String CHANGE = "PARA ÜSTÜ";
        /// <summary>
        /// 
        /// </summary>
        public const String PAYMENT_STARTED = "ÖDEME BAŞLADI";
        /// <summary>
        /// 
        /// </summary>
        public const String EFT = "EFT";
    #endregion

    #region Calculator
        /// <summary>
        /// 
        /// </summary>
        public const String CALCULATOR = "HESAP MAKİNESİ";
        public const String ENTER_CALCULATOR = "HESAP MAKİNESİNE GİR";
        public const String EXIT_CALCULATOR = "HESAP MAKİNESİNDEN ÇIK";
      public const string CONFIRM_EXIT_CALCULATOR = "HESAP MAKİNESİNDEN\nÇIKILACAK (GİRİŞ)?";
    #endregion

    #region Customer
        /// <summary>
        /// 
        /// </summary>
        public const String CUSTOMER_NOT_FOUND = "MÜŞTERİ\nBULUNAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_CADR_CODE = "MÜŞTERİ             \nKART/KOD GİRİŞİ";

        public const String ENTER_TCKN_VKN_MENU = "MÜŞTERİ             \nTCKN/VKN GİRİŞİ";
        /// <summary>
        /// 
        /// </summary>
        public const String SEARCH_RECORD = "MÜŞTERİ             \nKAYIT ARAMA";
        /// <summary>
        /// 
        /// </summary>
        public const String NEW_RECORD = "MÜŞTERİ             \nYENİ KAYIT";
        /// <summary>
        /// 
        /// </summary>
        public const String RETURN_TO_SELLING = "SATIŞA DÖN";
        /// <summary>
        /// 
        /// </summary>
        public const String VOID_CUSTOMER = "MÜŞTERİ             \nMÜŞTERİ KAYDI İPTAL";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_VOID_CUSTOMER = "MÜŞTERİ KAYDINI\nİPTAL ET (GİRİŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_VOID_CURRENT_CUSTOMER = "MÜŞTERİ İPTAL ET?";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_TRANSFER_CUSTOMER_TO_RECEIPT = "FİŞE MÜŞTERİ\nAKTARILACAK (GİRİŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String NAME_FIRM = "İSİM/FIRMA";
        /// <summary>
        /// 
        /// </summary>
        public const String CUSTOMER_GROUP = "MÜŞTERİ GRUBU";
        /// <summary>
        /// 
        /// </summary>
        public const String LEGATION = "ELÇİLİK/ÜLKE";
        /// <summary>
        /// 
        /// </summary>
        public const String ADDRESS = "ADRES";
        /// <summary>
        /// 
        /// </summary>
        public const String STREET = "CADDE";
        /// <summary>
        /// 
        /// </summary>
        public const String STREET_NO = "SOKAK/NO";
        /// <summary>
        /// 
        /// </summary>
        public const String REGION_CITY = "İLÇE/İL";
        /// <summary>
        /// 
        /// </summary>
        public const String TAXOFFICE = "VERGİ DAİRESİ";
        /// <summary>
        /// 
        /// </summary>
        public const String SEARCH_QUERY = "ARANACAK METİN";
        /// <summary>
        /// 
        /// </summary>
        public const String CUSTOMER_NUMBER = "MÜŞTERİ NUMARASI";
        /// <summary>
        /// 
        /// </summary>
        public const String IDENTITY_NUMBER = "KİMLİK NUMARASI";
        /// <summary>
        /// 
        /// </summary>
        public const String CUSTOMER_CODE = "MÜŞTERİ KODU";
        /// <summary>
        /// 
        /// </summary>
        public const String CUSTOMER_POINT = "MÜŞTERİ PUANI";
        /// <summary>
        /// 
        /// </summary>
        public const String CUSTOMER_DUTY = "GÖREVİ";
        /// <summary>
        /// 
        /// </summary>
        public const String TAX_NUMBER = "VERGİ NUMARASI";
        /// <summary>
        /// 
        /// </summary>
        public const String PROMOTION_LIMIT = "YÜZDE İNDİRİMİ";
        /// <summary>
        /// 
        /// </summary>
        public const String DISCOUNT_LIMIT = "İNDİRİM ORANI(%)";
        /// <summary>
        /// 
        /// </summary>
        public const String END_OF_RECORD = "KAYIT TAMAMLANDI";
        /// <summary>
        /// 
        /// </summary>
        public const String FATAL_ERROR_CUSTOMER_INFO_NOT_FOUND = "KRİTİK HATA\nMÜŞTERİ BİLGİSİ YOK"; //IKIYE BOL

        public const String CUSTOMER_NOT_BE_CHANGED_IN_DOCUMENT = "BELGE ORTASINDA\nMÜŞTERİ DEĞİŞEMEZ";
        public const String DOCUMENT_NOT_BE_TRANSFERRED = "BELGE \n AKTARILAMAZ";
        /// <summary>
        /// 
        /// </summary>
        public const String PROMPT_CLOSE_CASHREGISTER = "KASA KAPATILACAK?\n(GİRİŞ)";

        public const String TAX_INSTITUTION = "VERGİ DAİRESİ";
        public const String NAME = "ADI";



    #endregion

    #region List Messages
        /// <summary>
        /// 
        /// </summary>
        public const String END_OF_LIST = "LİSTE SONU";
        /// <summary>
        /// 
        /// </summary>
        public const String START_OF_LIST = "LİSTE BAŞI";
        /// <summary>
        /// 
        /// </summary>
        public const String LIST_EMPTY = "LİSTE BOŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String LISTING_ERROR = "LİSTELEME HATASI\nOLUŞTU";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_PROGRAM = "PROGRAM DOSYASI\nEKSiK VEYA HATALI";

    #endregion

    #region Process Menu
        //For selling state processes
        /// <summary>
        /// 
        /// </summary>
        public const String VOID_DOCUMENT = "BELGE İPTALİ";
        /// <summary>
        /// 
        /// </summary>
        public const String SUSPEND_DOCUMENT = "BELGE BEKLETME";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_VOID_DOCUMENT = "BELGE İPTALİ\nİPTAL ET?(GİRİŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String REPEAT_DOCUMENT = "BELGE TEKRARI";
        /// <summary>
        /// 
        /// </summary>
        public const String RESUME_DOCUMENT = "BEKLETİLEN BELGELER";
        public const string PARKED_DOCUMENT = "BEKLETİLEN BELGE";
        public const string ORDERED_DOCUMENT = "SİPARİŞ BELGESİ";
        /// <summary>
        /// 
        /// </summary>
        public const String ORDERS = "SİPARİŞLER";
        /// <summary>
        /// 
        /// </summary>
        public const String FAST_PAYMENT = "HIZLI ÖDEME";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_CASH = "NAKİT GİRİŞİ";
        /// <summary>
        /// 
        /// </summary>
        public const String RECEIVE_CASH = "NAKİT ÇIKIŞI";
        /// <summary>
        /// 
        /// </summary>       
        public const String RECEIVE_CHECK = "ÇEK ÇIKIŞI";
        /// <summary>
        /// 
        /// </summary>
        public const String RECEIVE_CREDIT = "KREDİ ÇIKIŞI";
        /// <summary>
        /// 
        /// </summary>

        public const String COMMAND_CALCULATOR = "HESAP MAKİNESİ";

        /// <summary>
        /// 
        /// </summary>
        public const String FIRST_DOCUMENT = "İLK BELGE(OPSİYONLU)";
        /// <summary>
        /// 
        /// </summary>
        public const String LAST_DOCUMENT = "SON BELGE(OPSİYONLU)";
        /// <summary>
        /// 
        /// </summary>
        public const String DOCUMENT_ID = "BELGE NUMARASI";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_SUSPEND_DOCUMENT = "BELGE BEKLETME\nASKIYA AL?(GİRİŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String CASH_AMOUNT = "TUTARI GİRİNİZ";
        public const String SUSPENDED_DOCUMENT = "BEKLETİLEN SATIŞ FİŞİ";
        public const String TABLE_MANAGEMENT = "MASA YÖNETİMİ";
        public const String SWITCH_MANAGER = "YÖNETİCİ DEĞİŞTİRME";


    #endregion

    #region Report menu
        /// <summary>
        /// 
        /// </summary>
        public const String X_REPORT = "X RAPORU";

        public const String X_PLU_REPORT = "X PLU RAPORU";
        /// <summary>
        /// 
        /// </summary>
        public const String Z_REPORT = "Z RAPORU";
        /// <summary>
        /// 
        /// </summary>
        public const String END_DAY_REPORT = "GÜN SONU RAPORU";
        /// <summary>
        /// 
        /// </summary>
        public const String FINANCIAL_BETWEEN_Z = "MALİ BELLEK (Z NO)";
        /// <summary>
        /// 
        /// </summary>
        public const String FINANCIAL_BETWEEN_DATE = "MALİ BELLEK (TARİH)";
        /// <summary>
        /// 
        /// </summary>
        public const String FINANCIAL_BETWEEN_Z_DETAIL = "MALİ BEL (ZZ) DETAY";
        /// <summary>
        /// 
        /// </summary>
        public const String FINANCIAL_BETWEEN_DATE_DETAIL = "MALİ BEL (TAR) DETAY";
        /// <summary>
        /// 
        /// </summary>
        public const String PAYMENT_REPORT = "SATIŞ RAPORU";

        public const String RECEIPT_TOTAL_REPORT = "FİŞ TOP. RAPORU";

        public const String SYSTEM_INFO_REPORT = "SİSTEM BİLGİ RAPORU";
        /// <summary>
        /// 
        /// </summary>
        public const String PAYMENT_REPORT_CURRENT = "GÜNCEL";
        public const String PAYMENT_REPORT_DAILY = "GÜNLÜK";
        public const String PAYMENT_REPORT_DATE = "İKİ TARİH ARASI";
        public const String PAYMENT_REPORT_WITH_DETAIL = "DETAYLI";
        public const String PAYMENT_REPORT_JUST_TOTALS = "TOPLAM";
        /// <summary>
        /// 
        /// </summary>
        public const String PROGRAM_REPORT = "PROGRAM BİLGİ RAPORU";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_SUMMARY_REPORT = "EKÜ DETAY RAPORU";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_DOCUMENT_REPORT = "EKÜ TEK BELGE";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_DOCUMENT_DATE_TIME = "TARİH & SAAT";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_DOCUMENT_Z_DOCID = "Z NO & FİŞ NO";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_DOCUMENT_ZREPORT = "Z RAPORU TEKRARI";
        /// <summary>
        /// 
        /// </summary>
        /// 
        public const String EJ_PERIODIC_REPORT = "EKÜ DÖNEMSEL";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_PERIODIC_ZREPORT = "İKİ Z NO ARASI";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_PERIODIC_FIRST_Z_NO = "İLK Z NO[,FİŞ NO]";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_PERIODIC_LAST_Z_NO = "SON Z NO[,FİŞ NO]";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_PERIODIC_DATE = "İKİ TARİH ARASI";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_PERIODIC_DAILY = "GÜNLÜK";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_LIMIT_SETTING = "EKÜ LiMiT AYARI";
        /// <summary>
        /// 
        /// </summary>
        /// 
        public const String CONFIRM_EJ_PERIODIC_REPORT = "EKÜ DÖNEMSEL\nDÖKÜM ALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITING_EJ_PERIODIC_REPORT = "EKÜ DÖNEMSEL\nDÖKÜM ALINIYOR...";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_EJ_DOCUMENT_REPORT = "EKÜ TEK BELGE\nDÖKÜM ALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITING_EJ_DOCUMENT_REPORT = "EKÜ TEK BELGE\nDÖKÜM ALINIYOR..";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_EJ_SUMMARY_REPORT = "EKÜ DETAY RAPORU\nALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITING_EJ_PERIODIC_REPORT_BETWEEN_ZNO = "Z NO ARASI DÖNEMSEL\nRAPOR ALINIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITING_EJ_SUMMARY_REPORT = "EKÜ DETAY RAPORU\nALINIYOR..";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITE_XREPORT = "X RAPORU\nRAPOR ALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITING_PAYMENT_REPORT = "SATIŞ RAPORU\nALINIYOR...";
        public const String WRITING_PROGRAM_REPORT = "PROGRAM BİLGİ RAPORU\nALINIYOR...";
        public const String WRITING_XREPORT = "X RAPORU\nRAPOR ALINIYOR...";
        public const String WRITING_XREPORT_PLEASE_WAIT = "X RAPORU ALINIYOR\nLÜTFEN BEKLEYİN...";
        public const String WRITING_XPLU_REPORT_PLEASE_WAIT = "X PLU RAPORU ALINIYOR\nLÜTFEN BEKLEYİN";
        public const String WRITING_CASHIER_REPORT = "KASİYER RAPORU\nRAPOR ALINIYOR...";
        public const String WRITING_REPORT_PLEASE_WAIT = "RAPOR ALINIYOR\nLÜTFEN BEKLEYİN...";

        public const String PLU_LIMIT_EXCEEDED = "PLU LİMİTİ\nAŞILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITE_Z_REPORT = "Z RAPORU\nRAPOR ALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITING_Z_REPORT = "Z RAPORU\nRAPOR ALINIYOR...";
        /// <summary>
        /// 
        /// </summary>
        public const String Z_NO = "Z NO:";
        /// <summary>
        /// 
        /// </summary>
        public const String FIRST_Z_NO = "İLK Z NO:";
        /// <summary>
        /// 
        /// </summary>
        public const String LAST_Z_NO = "SON Z NO:";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITE_FINANCIAL_Z_REPORT = "MALİ BELLEK RAPORU\nRAPOR ALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String WRITING_FINANCIAL_Z_REPORT = "MALİ BELLEK RAPORU\nRAPOR ALINIYOR...";
        /// <summary>
        /// 
        /// </summary>
        /// 
        public const String FINANCIAL_Z_REPORT_INVALID_PARAMETER = "GEÇERLİ Z ARALIĞI\nGİRİN";
        /// <summary>
        /// 
        /// </summary>
        /// 
        public const String REPORT_FINISHED = "RAPOR ALINDI";
        /// <summary>
        /// 
        /// </summary>
        public const String TIME = "SAAT (SSDD)";
        /// <summary>
        /// 
        /// </summary>
        public const String DATE = "TARİH (GGAAYYYY)";
        /// <summary>
        /// 
        /// </summary>
        public const String FIRST_DATE = "İLK TARİH (GGAAYYYY)";
        /// <summary>
        /// 
        /// </summary>
        public const String LAST_DATE = "SON TARİH (GGAAYYYY)";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_PAYMENT_REPORT = "ÇEKMECE RAPORU\nALINACAK?";//"SATIŞ RAPORU\nALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_PROGRAM_REPORT = "PROGRAM BİLGİ RAPORU\nALINACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_DATE_INPUT = "HATALI TARİH GİRİŞİ";
        /// <summary>
        /// 
        /// </summary>
        public const String SALES_EXIST_REPORT_NOT_ALLOWED = "SATIŞ VAR\nRAPOR ALINAMAZ";
        /// <summary>
        /// 
        /// </summary>
        public const String SALES_EXIST_COMMANDMENU_NOT_ALLOWED = "SATIŞ VAR. PROGRAM\nMENÜSÜ GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_ENTRY = "HATALI GİRİŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_SUPPORTED_REPORT = "DESTEKLENEN RAPOR\nBULUNAMADI";
        public const string CONFIRM_START_REPORT = "RAPOR ALINACAK";
        public const string REPORT_PROCESSING = "RAPOR ALINIYOR";
        public const string REPORT_STOPPING = "RAPOR DURDURULUYOR";
        public const string SPECIAL_REPORT = "ÖZEL RAPOR";

        public const String FIRST_PLU_NO = "İLK PLU NO";
        public const String LAST_PLU_NO = "SON PLU NO";

    #endregion

    #region Printer Exception Messages
        /// <summary>
        /// 
        /// </summary>
        public const String PRINTER_EXCEPTION = "YAZICI HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String FRAMING_EXCEPTION = "FRAMING HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String CHECK_SUM_EXCEPTION = "CHECK SUM HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String UNDEFINED_FUNCTION_EXCEPTION = "TANIMSIZ FONKSİYON";
        /// <summary>
        /// 
        /// </summary>
        public const String CMD_SEQUENCE_EXCEPTION = "İŞLEM SIRASI HATALI";
        /// <summary>
        /// 
        /// </summary>
        public const String NEGATIVE_RESULT_EXCEPTION = "İŞLEM HATASI\nKASA NAKİT YETERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String POWER_FAIURE_EXCEPTION = "ELEKTRİK KESİNTİSİ\nBELGEYİ İPTAL ET?";
        /// <summary>
        /// 
        /// </summary>
        public const String UNFIXED_SLIP_EXCEPTION = "BELGE DÜZGÜN\nYERLESTiRiLMEMiS";
        /// <summary>
        /// 
        /// </summary>
        public const String SLIP_COODINATE_EXCEPTION = "SLİP KOORDİNAT\n HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTRY_EXCEPTION = "SATIŞ HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String LIMIT_EXCEEDED_OR_ZREQUIRED_EXCEPTION = "BU İŞLEMDEN ÖNCE\nZ RAPORU ALINMALI";
        /// <summary>
        /// 
        /// </summary>
        public const String BLOCKING_EXCEPTION = "YAZARKASAYI\nYENİDEN BAŞLATIN";
        /// <summary>
        /// 
        /// </summary>
        public const String SVC_PASSWORD_OR_POINT_EXCEPTION = "SERVİS ŞİFRESİ\nHATALI";
        /// <summary>
        /// 
        /// </summary>
        public const String LOW_BATTERY_EXCEPTION = "BATARYA YETERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String BBX_NOT_BLANK_EXCEPTION = "BBX BOŞ DEĞİL";
        /// <summary>
        /// 
        /// </summary>
        public const String BBX_FORMAT_FAILURE_EXCEPTION = "FORMAT HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String BBX_DIRECTORY_EXCEPTION = "BBX DİZİN HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String MISSING_CASHIER_EXCEPTION = "KASİYER GİRİŞİ YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String DIFFERENT_CASHIER_ASSING_LIMIT_EXEED_EXCEPTION = "FARKLI KASİYER GİRİŞ\nLİMİTİ AŞILDI";
        /// <summary>
        /// 
        /// </summary>
        public const String ALREADY_FISCALIZED_EXCEPTION = "MALİ MODA \nDAHA ÖNCE GEÇİLMİŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String DTG_EXCEPTION = "SAAT/TARİH HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String FATAL_ERROR_NO_CASHIER_INFO = "KRİTİK HATA\nKASİYER BİLGİSİ YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String TIME_OUT_ERROR = "SÜRE AŞIMI\nOLUŞTU";
        /// <summary>
        /// 
        /// </summary>
        public const String FATAL_ERROR_PRODUCT_INFO_NOT_FOUND = "KRİTİK HATA\nÜRÜN BİLGİSİ YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String UNDEFINED_EXCEPTION = "TANIMSIZ HATA\nOLUSTU";
        public const String NULL_REFERENCE_EXCEPTION = "DEĞERSİZ NESNE\nHATASI OLUSTU";

        public const String NOJOURNALROLL = "JURNAL KAĞIDI YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String CHECK_PRINTER = "YAZICIYI KONTROL\nEDİNİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String NORECEIPTROLL = "FİŞ KAĞIDI YOK";
        /// <summary>
        /// 
        /// </summary>
        public const String OFFLINE1 = "OFLİNE1 HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String OPEN_SHUTTER = "FIS RULO\nKAPAGI ACIK";
        /// <summary>
        /// 
        /// </summary>
        public const String RFU_ERROR = "RFU HATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String MECHANICAL_FAILURE = "MEKANİK HATA";
        /// <summary>
        /// 
        /// </summary>
        public const String PRINTER_OFFLINE = "YAZICI KAPALI";
        /// <summary>
        /// 
        /// </summary>
        public const String PRINTER_CONNETTION_ERROR = "YAZICIYA ULAŞILAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String FISCAL_ID_EXCEPTION = "MALİ HAFIZA\nUYUŞMAZLIĞI";
        /// <summary>
        /// 
        /// </summary>
        public const String FISCAL_COMM_EXCEPTION = "MALİ HAFIZA\nBAĞLANTISI AÇIK";
        /// <summary>
        /// 
        /// </summary>
        public const String FISCAL_MISMATCH_EXCEPTION = "MALİ BELLEK\nKAYIT UYUŞMAZLIĞI";
        /// <summary>
        /// 
        /// </summary>
        public const String FISCAL_UNDEFINED_EXCEPTION = "TANIMSIZ MALİ\nBELLEK";
        /// <summary>
        /// 
        /// </summary>
        public const String SERVICE = "SERVİS MODU";
        /// <summary>
        /// 
        /// </summary>
        public const String MODE = "MODU";
        /// <summary>
        /// 
        /// </summary>
        public const String CANNOT_ACCESS_PRINTER = "YAZICIYA\nULAŞILAMIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String CANNOT_ACCESS_EJ = "EKÜ'YE ULAŞILAMIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String COM_PORT_DEFECTIVE_CALL_SERVICE = "COM PORT ARIZALI\nSERVİS ÇAĞIRIN";
        /// <summary>
        /// 
        /// </summary>
        public const String PRINTER_TIMEOUT = "YAZICI VE BAĞLANTIYI\nKONTROL EDİNİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CHECK_RECEIPT_ROLL = "FİŞ RULOSUNU\nKONTROL EDİNİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String ZREPORT_UNSAVED = "Z RAPORU\nKAYDEDİLEMEDİ";
        /// <summary>
        /// 
        /// </summary>
        public const String INCOMPLETE_XREPORT = "TAMAMLANAMAMIŞ\nX RAPORU";
        /// <summary>
        /// 
        /// </summary>
        public const String INCOMPLETE_PAYMENT = "ÖDEME TAMAMLANMAMIŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String INCOMPLETE_PAYMENT_AFTER_EFT_DONE = "PROVİZYON ALINDI\nÖDEMEYİ SONLANDIR";
        /// <summary>
        /// 
        /// </summary>
        public const String DOCUMENT_NOT_EMPTY = "SATIS\nVAR";
        /// <summary>
        /// 
        /// </summary>
        public const String DOCUMENT_NOT_PRINTED = "BELGE YAZDIRILAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String SUBTOTAL_NOT_MATCH = "BELGE TOPLAMI\nKASAYLA TUTARSIZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CAN_NOT_ACCESS_TO_DISPLAYS = "GÖSTERGELERE ULAŞILAMIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String FM_FULL_REGISTER_BLOCKED = "MALİ BELLEK DOLU\nKASA KAPALI";
        /// <summary>
        /// 
        /// </summary>
        public const String FM_HAS_NO_AREA_ZREQUIRED= "MALİ BELLEK DOLDU\nZ RAPORU GEREKLİ";
        /// <summary>
        /// 
        /// </summary>
        public const String CONTINUE_WRITING = "YAZMAYA DEVAM\nEDİLİYOR...";
        public const String Z_REQUIRED_GET_Z = "Z RAPORU ALINMAMIS\nRAPOR ALINIZ(GiRiS)";

        public const String WAIT_FOR_MATCHING = "EŞLEME YAPILIYOR\nBEKLEYİNİZ..";

        public const String PRESS_ENTER_TO_CONNECT = "BAĞLANMAK İÇİN\nGİRİŞ'E BASINIZ";

        public const String CALL_SERVICE = "SERVİS ÇAĞIRIN";

        public const String INVALID_PASS_ENTRY = "HATALI ŞİFRE GİRİŞİ";

        public const String RIGHT_TO_REST = "KALAN HAKKINIZ";

        public const String MANAGER_LOGIN_LOCKED = "YÖNETİCİ GİRİŞİ\nKİLİTLENMİŞTİR";

        public const String MATCHED_SUCCESSFULL = "EŞLEME\nBAŞARILI";
    #endregion

    #region Slip Messages
        /// <summary>
        /// 
        /// </summary>
        public const String PAPER_FOR_VOIDING_SLIP_SALE = "İPTAL İÇİN\nBELGE KOYUNUZ";
        /// <summary>
        /// 
        /// </summary>
        public const String CONTINUE_OR_VOIDING_SLIP_SALE = "GİRİŞ: SATIŞ DEVAM\nÇIKIŞ: BELGE İPTAL";
        /// <summary>
        /// 
        /// </summary>
        public const String CONTINUE_OR_VOIDING_SLIP_SALE_ON_ERROR = "İPTAL: İPTAL ET\nÇIKIŞ: DEVAM ET";
        /// <summary>
        /// 
        /// </summary>
        public const String NEW_SLIP_PAPER = " KOYUP\nGiRiŞ'E BASINIZ";
        /// <summary>
        /// 
        /// </summary>
        public const String PUT_IN = "KOYUNUZ";
        /// <summary>
        /// 
        /// </summary>
        public const String RECOVER_ERROR_AND_PRESS_ESC = "HATAYI DUZELTİP\nESC'YE BASINIZ";
        /// <summary>
        /// 
        /// </summary>
        public const String END_OF_SLIP_PAYMENT_NOT_ALLOWED = "FATURA SONU\nÖDEME GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const string RETURN_PAYMENTS = "İADE ÖDEMELERİ";
        /// <summary>
        /// 
        /// </summary>
        public const String CONTINUE_SELLING = "SATIŞ\nDEVAM EDİYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String PUT_PAPER_IN = "YAZICIYA KAĞIT\nYERLEŞTİRİNİZ";

    #endregion

    #region Service messages
        
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_LOGO = "LOGO";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_VAT_RATES = "KDV ORANLARI";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_DAILY_MEMORY_FORMAT = "GÜNLÜK BELLEK FORMAT";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_DATE_AND_TIME = "TARİH VE SAAT";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_CLOSE_FM = "HURDAYA AYIRMA";

        public const String MENU_START_FM_TEST = "FM TEST BAŞLAT";

        public const String MENU_UPDATE_FIRMWARE = "UYGULAMA GÜNCELLE";

        public const String PM_FILE_TRANSFER = "DOSYA TRANSFERİ";

        public const String PM_GMP_TEST_COMMAND = "GMP TEST KOMUTLARI";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_EXTERNAL_DEV_SETTINGS = "BAĞLANTI AYARLARI";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_FACTORY_SETTING = "FABRİKA AYARLARI";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_CREATE_DB = "SATIŞ DB OLUŞTUR";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_PRINT_LOGS = "LOGLARI YAZDIR";
        /// <summary>
        /// 
        /// </summary>
        public const String MENU_EXIT_SERVICE = "ÇIKIŞ";
        /// <summary>
        /// 
        /// </summary>
        public const String CLOSE_SERVICE = "SERVİSİ KAPAT";
        /// <summary>
        /// 
        /// </summary>
        public const String LOAD_FACTOR_SETTINGS = "FABRİKA AYARLARI\nYUKLE\t(GİRİŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String CLOSE_FISCAL_MEMORY = "MALİ BELLEK\nKAPAT\t(GİRİŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String SERVICE_PASSWORD = "ŞİFRE GİRİN";
        /// <summary>
        /// 
        /// </summary>
        public const String PORT = "PORT";
        /// <summary>
        /// 
        /// </summary>
        public const String SERVICE_PASSWORD_INVALID = "SERVİS ŞİFRESİ\nHATALI";
        /// <summary>
        /// 
        /// </summary>
        public const String ATTACH_JUMPER_AND_TRY_AGAIN = "JUMPERI TAKIP\nTEKRAR DENEYİN";
        /// <summary>
        /// 
        /// </summary>
        public const String RESTART_PRINTER_FOR_SERVICEMODE = "SERVİS İÇİN YAZICIYI\nKAPATIP TEKRAR AÇIN";
        /// <summary>
        /// 
        /// </summary>
        public const String PROMPT_DAILY_MEMORY_FORMAT = "GÜNLÜK BELLEK\nFORMATLA(GİRİŞ)";
        /// <summary>
        /// 
        /// </summary>
        public const String REMOVE_JUMPER_AND_TRY_AGAIN = "JUMPERI ÇIKARIP\nTEKRAR DENEYİN";
        /// <summary>
        /// 
        /// </summary>
        public const String ATTACH_JUMPER_AND_RESTART_FPU = "JUMPERI TAKIP\nPRINTERİ KAPAT-AÇ";
        /// <summary>
        /// 
        /// </summary>
        public const String LOGO_LINE = "LOGO SATIRI";
        /// <summary>
        /// 
        /// </summary>
        public const String VAT_RATE = "KDV ORANI";
        /// <summary>
        /// 
        /// </summary>
        public const String PRINT_LOGS = "LOGLARI YAZDIR\n(GİRİŞ)";

        public const String CREATE_DB = "SATIŞ DB OLUŞTUR\nONAYLA (GİRİŞ)";

        public const String START_FM_TEST = "FM TEST\nBAŞLAT (GİRİŞ)";

        public const String SERVER_IP = "SERVER IP";

        public const String SERVER_PORT = "SERVER PORT";
        public const String UPDATİNG_CATEGORY = "KATEGORİLER\nGÜNCELLENİYOR..";

    #endregion

    #region Settings Messages
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_ADDRESS_TRY_AGAIN = "ADRES GİRİŞİ HATALI\nTEKRAR DENEYİNİZ";
        /// <summary>
        /// 
        /// </summary>f
        public const String PROGRAM_VERSION = "VERSİYON";
        /// <summary>
        /// 
        /// </summary>
        public const String BRANCH_ID = "ŞUBE KODU";
        /// <summary>
        /// 
        /// </summary>
        public const String BRANCH_ID_MAX_LENGTH = "SUBE KODU\n3 HANE OLABiLiR";
        /// <summary>
        /// 
        /// </summary>
        public const string REGISTER_ID = "KASA NO";
        /// <summary>
        /// 
        /// </summary>
        public const string REGISTER_ID_MAX_LENGTH = "KASA NO EN FAZLA\n 999 OLABiLİR";
        /// <summary>
        /// 
        /// </summary>
        public const String PROMPT_DATA_FILE_UPDATED = "DATA DOSYALARI\nGÜNCELLENECEK?";
        /// <summary>
        /// 
        /// </summary>
        public const String PROMPT_REGISTER_FILE_TRANSFER = "KASA DOSYALARI\nAKTARILACAK?";
        /// <summary>
        /// 
        /// </summary>
        public const String SECURITY_CODE = "GÜVENLİK KODU";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTER_FISCAL_MODE = "MALİ MODA\nGEÇECEK?";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTERING_FISCAL_MODE = "MALİ MODA\nGEÇİLİYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String ENTERED_FISCAL_MODE = "MALİ MODA GEÇİLDİ";
        /// <summary>
        /// 
        /// </summary>
        public const String NOT_ENTER_FISCAL_MODE = "MALi KONUMA\nGEÇiLEMEDi";
        /// <summary>
        /// 
        /// </summary>
        public const String REGISTER_NO = "SİCİL NO";
        /// <summary>
        /// 
        /// </summary>
        public const String OFFICE_INDEX = "OFİS DİZİNİ";
        /// <summary>
        /// 
        /// </summary>
        public const String TIME_CHANGED = "SAAT DEĞİŞTİ";
        /// <summary>
        /// 
        /// </summary>
        public const String BACKOFFICE_TIME_NOT_SET = "ÖNOFIS SAATI\nAYARLANAMADI";
        /// <summary>
        /// 
        /// </summary>
        public const String DATA_FILES_UPDATING = "DATA DOSYALARI\nGÜNCELLENİYOR...";
        /// <summary>
        /// 
        /// </summary>
        public const String DATA_FILES_UPDATED = "DATA DOSYALARI\nGUNCELLEME BİTTİ";
        /// <summary>
        /// 
        /// </summary>
        public const string PROGRAM_FILES_UPDATING = "DOSYALAR\nKOPYALANIYOR...";
        /// <summary>
        /// 
        /// </summary>
        public const String LAST_UPDATE = "SON GÜNCELLEME";
        /// <summary>
        /// 
        /// </summary>
        public const String TCP_IP_ADDRESS = "TCP/IP ADRESİ";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_TIME = "GEÇERSİZ ZAMAN";
        /// <summary>
        /// 
        /// </summary>
        public const String REGISTER_FILES_TRANSFERRED = "KASA DOSYALARI\nAKTARIM BİTTİ";
        /// <summary>
        /// 
        /// </summary>
        public const String DISPLAY_COM_PORT = "GÖSTERGE COM PORT";
        /// <summary>
        /// 
        /// </summary>
        public const string PRINTER_COM_PORT = "YAZICI COM PORT";
        /// <summary>
        /// 
        /// </summary>
        public const String SLIP = "FATURA";
        /// <summary>
        /// 
        /// </summary>
        public const String SCALE = "TERAZİ";
        /// <summary>
        /// 
        /// </summary>
        public const String BARCODE = "BARKOD";
        /// <summary>
        /// 
        /// </summary>
        public const String DISPLAY = "GÖSTERGE";
        /// <summary>
        /// 
        /// </summary>
        public const String SLIP_COM_PORT = "FATURA COM PORT";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_SLIP_PORT = "BELGE YAZICISI\nBAĞLI DEĞİL";
        /// <summary>
        /// 
        /// </summary>
        public const String SCALE_COM_PORT = "TERAZİ COM PORT";
        /// <summary>
        /// 
        /// </summary>
        public const String BARCODE_COM_PORT = "BARKOD COM PORT";
        /// <summary>
        /// 
        /// </summary>
        public const String SCALE_CONNECTION_ERROR = "TERAZİ BAĞLANTI\nHATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String BARCODE_CONNECTION_ERROR = "BARKOD BAĞLANTI\nHATASI";
        /// <summary>
        /// 
        /// </summary>
        public const String BRIGHTNESS_LEVEL = "PARLAKLIK SEVİYESİ";
        /// <summary>
        /// 
        /// </summary>
        public const String DISPLAY_TEST = "ABCDEFGHIJKLMNOPQRST\nUVWXYZ0123456789.:!@";
        /// <summary>
        /// 
        /// </summary>
        public const string RESETED_DISPLAY = "GÖSTERGE RESETLENDi";
        /// <summary>
        /// 
        /// </summary>
        public const string ENTER_VALUE = "DEĞER GİRİNİZ";
        /// <summary>
        /// 
        /// </summary>
        public const string CONTINUE = "DEVAM";
        /// <summary>
        /// 
        /// </summary>
        public const String TAX_RATE_INTERVAL_ERROR = "VERGİ ORANI 0-99\nARASI OLMALI";
        /// <summary>
        /// 
        /// </summary>
        public const String LOADING_DEPARTMENT_AND_VAT_INFO = "DEPARTMAN VE KDV\nGİRİŞİ YAPILIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String LOADING_NEW_PROGRAM = "YENİ PROGRAM DOSYASI\nYUKLENİYOR...";
        /// <summary>
        /// 
        /// </summary>
        public const String LOADING_FROM_REGISTER = "KASADAN GEREKLi\nBiLGiLER ALINIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String LOADING_LOGO_INFO = "LOGO SATIRLARI\nGİRİLİYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String INVALID_DATE_RANGE = "GİRİLEN TARİH\nARALIĞI GEÇERSİZ";
        /// <summary>
        /// 
        /// </summary>
        public const String LOGO_DEPARTMENT_CHANGE_Z_REPORT_REQUIRED = "LOGO/DEPARTMAN DEĞ.\nZ RAPORU GEREKLi";
        /// <summary>
        /// 
        /// </summary>
        public const String NO_CONNECTION_WITH_BACKOFFICE = "ARKA OFİS İLE\nBAĞLANTI KURULAMADI";
        public const string NOT_TRANSFERED_DATA_FILES = "DATA DOSYALARI\nALINAMADI";
        public const string NOT_PROGRAM_INSTALLED = "PROGRAM YÜKLENEMEDİ";
        public const string MAIN_DEPARTMENT = "ANA DEPARTMAN";
        public const string NO_USING = "KULLANIM DIŞI";
        public const string UPDATE_ERROR = "GÜNCELLEME BAŞARISIZ";
        public const string UPDATE_STATU = "UYGULAMA\nGÜNCELLENİYOR...";
        public const string UPDATE_ERROR_404 = "VERSİYON BULUNAMADI";
        public const string UPDATE_ERROR_406 = "GÜNCEL VERSİYON";
        public const string UPDATE_ERROR_500 = "SERVER GÜNCELLEME\nHATASI";
        public const string UPDATE_FOLDER_ERROR = "GUNCEL ZİP DOSYASI\nBULUNAMADI";
        public const string UPDATE_FOLDER_NOT_FOUND = "GÜNCELLEME DOSYASI\nBULUNAMADI";

        public const String FILE_NAME = "DOSYA ADI";

    #endregion

    #region EJ Messages
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_FULL = "EKÜ DOLU";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_CHANGED = "EKÜ DEĞİŞTİ\nAKTİF EKÜ NO: ";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_LIMIT_WARNING = "EKÜ DOLULUK ORANI";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_AVAILABLE_LINES = "EKÜ KALAN SATIR";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_MISMATCH = "TANIMSIZ EKÜ";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_FORMAT_ERROR = "FORMATSIZ EKÜ";
        /// <summary>
        /// 
        /// </summary>   
        public const String EJ_NOT_AVAILABLE = "EKÜ'YE\nULAŞILAMIYOR";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_FISCALID_MISMATCH = "EKÜ KASAYA\nAiT DEĞiL";
        /// <summary>
        /// 
        /// </summary>
        /// 
        public const String EJ_BITMAP_ERROR = "BITMAP HATASI\nOLUSTU";
        /// <summary>
        /// 
        /// </summary>
        public const String EJ_ERROR_OCCURED = "EKÜ HATASI\nOLUŞTU";

    #endregion

    #region "Number to Word"
        public const String ONLY = "YALNIZ";
        public const String YTL = "TL";
        public const String YKR = "KR";
        public const String ZERO = "SIFIR";
        public const String ONE = "BİR";
        public const String TWO = "İKİ";
        public const String THREE = "ÜÇ";
        public const String FOUR = "DÖRT";
        public const String FIVE = "BEŞ";
        public const String SIX = "ALTI";
        public const String SEVEN = "YEDİ";
        public const String EIGHT = "SEKİZ";
        public const String NINE = "DOKUZ";
        public const String TEN = "ON";
        public const String TWENTY = "YİRMİ";
        public const String THIRTY = "OTUZ";
        public const String FOURTY = "KIRK";
        public const String FIFTY = "ELLİ";
        public const String SIXTY = "ALTMIŞ";
        public const String SEVENTY = "YETMİŞ";
        public const String EIGHTY = "SEKSEN";
        public const String NINETY = "DOKSAN";
        public const String HUNDRED = "YÜZ";
        public const String THOUSAND = "BİN";
        public const String MILLION = "MİLYON";
        public const String BILLION = "MİLYAR";
        public const String TL = "TL";
        public const String KURUS = "KURUŞ";
    #endregion

    #region Log Keywords
        public const String SAT = "SAT";
        public const String IPT = "IPT";
        public const String ART = "ART";
        public const String IND = "IND";
        public const String SNS = "SNS";
        public const String NAK = "NAK";
        public const String DVZ = "DVZ";
        public const String KRD = "KRD";
        public const String TOP = "TOP";
        public const String PRM = "PRM";
        public const String MSG = "MSG";
        public const String NOT = "NOT";
        public const String END = "SON";
        public const String KOD = "KOD";
        public const String LMT = "LMT";
        public const String HAR = "HAR";
    #endregion

    #region "Pos Client Type"
        public const String WebService = "WebService";
        public const String TcpListener = "Tcp";
    #endregion

    #region "DiplomaticCustomer related"
        public const String CUSTOMER_IDENTITY = "KİMLİK NO";
    #endregion

    #region ParameterExceptions

        public const String PARAMETER_EXCEPTION = "PARAMETRE HATASI";
        public const String NO_DOCUMENT_FOUND = "ARANILAN ÖZELLİKTE\nBELGE BULUNMUYOR";
        public const String DATE_OUTOFRANGE = "TARİH ARALIĞINDA\nBELGE BULUNMUYOR";
        public const String Z_OUTOFRANGE = "ARANILAN Z NUMARASI\nEKÜ'DE YER ALMIYO";
        public const String DOCUMENT_OUTOFRANGE = "ARANILAN BELGE NO\nBULUNAMADI";
        public const String ADDRESS_OUTOFRANGE = "BULUNAN ADRES\nARALIĞIN DIŞINDA";
        public const String PARAMETER_SEQUENCE_ERROR = "PARAMETRE SIRASI\nUYGUN DEĞİL";
        public const String FIRST_PARAMETER_BIGGER_LAST_ONE = "İLK DEĞER\nSON DEĞERDEN BÜYÜK";
        public const String EXCESSIVE_PARAMETER_INTERVAL = "PARAMETRE ARALIĞI\nÇOK FAZLA";
        public const String TIME_LIMIT_ERROR = "SAAT DEĞİŞİKLİĞİ\n60 DAKİKAYI AŞAMAZ";
        public const String TIME_ZREPORT_ERROR = "SAAT SON Z RAPORU\nÖNCESİNE ALINAMAZ";
        public const String NO_PROPER_Z_FOUND = "ARANILAN DÖNEMDE\nMALİ HFZ KAYDI YOK";
        public const string NO_DOCUMENT_FOUND_IN_EJ = "ARANILAN FİŞ EKÜ'DE\nBULUNAMADI (GİRİŞ)";
        public const String RANGE_OF_NUMBER_EXCEPTION = "HATALI SAYI\nARALIĞI GİRİŞİ";
    #endregion

    #region Slip (Printer) Errors

        public const String DOCUMENT_ID_NOTSET_ERROR = "BELGE NUMARASI\nYOK";
        public const String UNFIXED_SLIP_ERROR = "BELGE DÜZGÜN\nYERLEŞTİRİLMEMİŞ";
        public const String SLIP_ROWCOUNT_EXCEED_ERROR = "BELGE KOYUP\nGİRİŞ'E BASINIZ";
        public const String REQUEST_SLIP_ERROR = "BELGE KOYUP\nBEKLEYİNİZ";
        public const String NEGATIVE_COORDINATE_ERROR = "FATURA KOORDINATLARI\nNEGATİF OLAMAZ";
        public const String CUSTOMER_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜŞTERİ-VERGİ";
        public const String CUSTOMER_TIME_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜŞTERİ-SAAT";
        public const String TIME_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nVERGİ-SAAT";
        public const String CUSTOMER_DATE_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nMÜŞTERİ-TARİH";
        public const String DATE_TAX_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nVERGİ-TARİH";
        public const String COORDINATE_OUTOF_INVOICE_ERROR = "BAZI KOORDİNATLAR\nBELGEYİ AŞMAKTADIR";
        public const String NAME_VAT_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nÜRÜN ADI-ÜRÜN KDV";
        public const String AMOUNT_VAT_COORDINATE_ERROR = "UYUMSUZ FATURA KOOR.\nÜRÜN KDV-ÜRÜN TUTAR";
        public const String PRODUCT_COORDINATE_ERROR = "ÜRÜN BAŞLANGIÇ\nSATIRI ÇOK KÜÇÜK";
        public const String SLIP_ERROR = "FATURA\nHATASI";

    #endregion Slip Errors

    #region HuginPOS Errors

        public const String LIMIT_EXCEED_ERROR = "GiRiLEN DEGER\nLiMiTiN DIŞINDA";
        public const String OFFICE_PATH_ERROR = "ARKA OFİS\nERİŞİM DIŞI";
        public const String OUTOF_QUANTITY_LIMIT = "MiKTAR LiMiTi AŞILDI";
        public const String INVALID_QUANTITY = "GEÇERSiZ MiKTAR";
        public const String NO_ADJUSTMENT_EXCEPTION = "İPTAL EDİLECEK\nİND/ART YOK";
        public const String NO_CORRECTION_EXCEPTION = "DÜZELTİLECEK İŞLEM\nYOK";
        public const String PROGRAM_HAS_CLOSED = "PROGRAM\nKAPANMIŞTIR";

    #endregion

    #region Barcode Menu
        /// <summary>
        /// 
        /// </summary>
        public const String RETURN_RECEIPT = "TÜM FİŞİ İADE AL";
        /// <summary>
        /// 
        /// </summary>
        public const String RETURN_PRODUCT= "ÜRÜN İADE AL";
        /// <summary>
        /// 
        /// </summary>
        public const String REPEAT_SALE = "SATIŞI TEKRARLA";
        /// <summary>
        /// 
        /// </summary>
        public const String PRINT_RECEIPT_COPY = "FİŞ KOPYASI BAS";
        /// <summary>
        /// 
        /// </summary>
        public const String CONFIRM_PRINT_RECEIPT_COPY = "FİŞ KOPYASI\n BAS?";
        /// <summary>
        /// 
        /// </summary>
        public const String RECEIPT_NOT_BELONG_TO_CASE = "FİŞ KASAYA\n AİT DEĞİL";

    #endregion

    #region Contactless messages
        /// <summary>
        /// 
        /// </summary>
        public const String CONTACTLESS_CARD_ERROR = "TEMASSIZ\nKART HATASI";
        public const String CONFIRM_UPDATE_POINTS = "PUAN KAYDEDİLEMEDİ\nTEKRAR DENE?";
        public const String MISSING_CARD_INFO = "KART BİLGİLERİ EKSİK\nYÜKLENSİN Mİ?";
        public const String CARD_MISMATCH = "AYNI KARTI\nKOYMALISINIZ";

    #endregion

    #region Customer Points messages
        /// <summary>
        /// 
        /// </summary>
        public const String CUSTOMER_NOTIN_POINT_DB = "MÜŞTERİ KAYDI\nVERİTABANINDA YOK";
        public const String CARDSERIAL_NOTIN_POINT_DB = "BU KART SERİSİ\nVERİTABANINDA YOK";
        public const String FAIL_ON_POINT_UPDATE = "PUAN VERİTABANINA\nKAYDEDİLEMEDİ";
        public const String FAIL_ON_CARDSERIAL_INSERT = "KART SERİSİ\nKAYDEDİLEMEDİ";
        public const String CARDSERIAL_ALREADY_EXISTS = "VERİTABANINDA\nBU SERI NUMARASI VAR";
        public const String POINT = "PUAN";
        public const String EARNED_POINT = "KAZANILAN PUAN";
        public const String USED_POINT = "HARCANAN PUAN";
        public const String RETURNED_POINT = "İADE PUAN";
        public const String TOTAL_POINT = "TOPLAM PUAN";
        public const String DEAR_CUSTOMER = "SAYIN";
        public const String CUSTOMER_CODE_SHORTENED = "M. KODU";
        public const String TOTAL_REDUCTION = "TOPLAM İNDİRİM";
        public const String SUBTOTAL_REDUCTION = "ARATOP İNDİRİMİ";
        public const String PRODUCT_REDUCTION = "ÜRÜN İNDİRİMİ";
        public const String TOTAL_FEE = "TOPLAM ARTIRIM";
        public const String SUBTOTAL_FEE = "ARATOP ARTIRIMI";
        public const String PRODUCT_FEE = "ÜRÜN ARTIRIMI";

    #endregion

    #region Customer Form Message

        public const string CLOSE_PROGRAM = "HATA OLUŞTU, PROGRAM KAPATILACAK";
        public const string RESTART_PROGRAM = "PROGRAM YENiDEN\nBAŞLAYACAK";
        public const string RESTART_COMPUTER = "BiLGiSAYAR YENiDEN\nBAŞLAYACAK";
        public const string PORT_IN_USE = "GÖSTERGE VEYA YAZICI PORTU KULLANIMDA";
        public const string MISSING_REFERENCE_FILE = "REFERANS DOSYASI EKSİK";
        public const string PLU_PAGE = "PLU Sayfa";
        public const string LED_ONLINE = "Online";
        public const string LED_FAST_PAYMENT = "Hızlı Öde";
        public const string LED_SALE = "Satış";
        public const string LED_RETURN = "İade";
        public const string LED_CUSTOMER = "Müşteri";
        public const string CUSTOMER = "MÜŞTERİ";
        public const string PROGRAM = "PROGRAM";
        public const string REPORT = "RAPOR";
        public const string MULTIPLY = "X";
		public const string DIVIDE = "/";
		public const string SUBTRACT = "-";
		public const string ADD = "+";
		public const string EQUAL = "=";
        public const string ENTER = "GİRİŞ";
        public const string UNDO = "DÜZELT";
        public const string CASH_DRAWER = "ÇEKMECE";
        public const string PAPER_FEED = "KAĞIT";
        public const string AMOUNT = "TUTAR";
        public const string ESCAPE = "ÇIKIŞ";
        public const string PRICE = "FİYAT";
        public const string DOCUMENT = "BELGE";
        public const string SUB_TL = "ARATOP";
        public const string COMMAND = "İŞLEM";
        public const string REPEAT = "TEKRAR";
        public const string CASH_PAYMENT = "NAKİT";
        public const string PAYOUT = "-TL";
        public const string RECEIVE_ACCT_ON_PAYMENT = "+TL";
        public const string NO_PRODUCT = "ÜRÜN YOK";
        public const string DELETE = "Sil";
        public const string NO_CUSTOMER = "Müşteri Yok";
        public const string SEND_ORDER = "SİPARİŞ GÖNDER";
        public const string CUSTOMER_ENTRY = "MÜŞTERİ GİRİŞİ";
    #endregion Customer Form Message

    #region Eft Pos Messages
        public const String EFT_POS_ERROR = "EFT POS\nHATASI";
        public const String PROCESS_REJECTED = "İŞLEM REDDEDİLDİ";
        public const String EFT_TIMEOUT_ERROR = "EFT ZAMAN AŞIMI";
        public const String FAILED_PROVISION_ACCEPT_PAYMENT = "PROVİZYON BAŞARISIZ!";
        public const String ACCEPT_PAYMENT_OR_REPEAT_VIA_EFT = "GİRİŞ : ÖDEME ALINDI\nTEKRAR: YENİDEN DENE";
        public const String PROCESS_START_WAITING_EFT_POS = "ÖDEME BAŞLADI\nLÜTFEN BEKLEYİNİZ...";
        public const String MOVE_TO_EFT_POS_SIDE = "EFT-POS TARAFINA\nGEÇİNİZ..";
        public const String ANY_CONNECTED_EFT_POS = "BAĞLI BİR CİHAZ\nBULUNMAMAKTADIR";
        public const String NO_MATCHED_EFT_POS = "EŞLENMİŞ EFT-POS\nBULUNMAMAKTADIR";
        public const String DOCUMENT_VOIDED_BY_EFT_POS = "CİHAZ TARAFINDAN FİŞ\nİPTAL EDİLMİŞTİR";
        public const String TIMEOUT_EX_SEND_AGAIN = "ZAMAN AŞIMI\nTEKRAR GÖNDER(GİRİŞ?)";

    #endregion

    #region Document Status List Messages
        public const String DOCUMENT_STATUS = "BELGE DURUMU";
        public const String OPEN = "AÇIK";
        public const String CLOSED = "KAPALI";
    #endregion

    #region System Manager List Messages
        public const String SYSTEM_MANAGER = "SİSTEM YÖNETİCİSİ";
        public const String RESTART_POS = "PROG. TEKRAR BAŞLAT";
        public const String SHUTDOWN_SYSTEM = "SİSTEMİ KAPAT";
        public const String RESTART_SYSTEM = "SİSTEM TEKRAR BAŞLAT";
        public const String UPDATE_FOLDER = "DOSYADAN GÜNCELLE";
        public const String UPDATE_ONLINE = "ONLINE GÜNCELLE";
        public const String SHUTDOWN_POS = "PROGRAMI KAPAT";
        public const String CONFIRM_RESTART_PROGRAM = "PROGRAMI YENİDEN\nBAŞLAT?";
        public const String PROGRAM_CLOSING = "PROGRAM\nKAPATILIYOR...";
        public const String SOUND_SETTINGS = "SES AYARLARI";
        public const String SOUND_LEVEL = "SES SEVİYESİ";
    #endregion

    #region Tallying Menu

    public const String TALLYING = "SAYIM";
    public const String PRODUCT_QUANTITY = "ÜRÜN ADETİ";
    public const String PRODUCT_BARCODE = "ÜRÜN BARKODU";
    public const String PRODUCT_NOT_WEIGHABLE = "ÜRÜN\nTARTILAMAZ";
    public const String CONFIRM_EXIT_TALLYING = "SAYIMDAN ÇIK?";

    #endregion

    #region PromotionServer Messages

    public const String EXCEED_PRODUCT_LIMIT = "ÜRÜN LİMİTİ\nAŞILDI";
    public const String INVALID_SECURITY_KEY_EXCEPTION = "PROMOSYON LİSANSI\nGEÇERLİ DEĞİL";
    public const string PROMOTION_CODE = "Promosyon kodu";

    #endregion

    #region EscPos Receipt Messages

    public const String RECEIPT_TIME = "SAAT";
    public const String RECEIPT_INFORMATION = "TAHSİLAT BİLGİLERİ";
    public const String EXCHANGE_PROVISION = "TAHSİLAT KARŞILIĞI";
    public const String DOCUMENT_QUNTITY = "BELGE ADEDİ";
    public const String OF_QUNTITY = " ADEDİ";
    public const String ECR_IS_FISCAL = "KASA MALİ\nDURUMDADIR";

    #endregion

    #region Common Words
    public const String NUMBER = "NUMARASI";
    public const String NO = "NO";
    public const String PAGE = "SAYFA";

    #endregion

    #region PROGRAM INFO REPORT
        public const String DEPARTMENT_INFORMATIONS = "DEPARTMAN BİLGİLERİ";
        public const String RECEIPT_LIMIT = "FİŞ LİMİTİ";
        public const String PRODUCT_MINUMUM_PRICE = "ÜRÜN MİNİMUM FİYATI";
        public const String AUTO_CUSTOMER_ADJUSTMENT = "OTOMATİK MÜŞT. İND.";
        public const String OPEN_DRAWER_ON_PAYMENT = "ÖDEMEDE PARA ÇEKM. AÇ.";
        public const String PRINT_SUBTOTAL = "A.TOPLAMI BELGEYE YAZ.";
        public const String PROMPT_CREDIT_INSTALLMENTS = "KREDİ TAKSİT SORMA";
        public const String CASHIER_NAME_ON_RECEIPT = "BELGEDE KASİYER İSMİ";
        public const String PRINT_BARCODE_ON_FOOTER = "FİŞ SONUNDA BARKOD";
        public const String SETTINGS = "AYARLAR";
        public const string LOGO_DIFFERENT = "LOGO FARKLI";
        public const string LOGO_UPDATING = "LOGO\nGÜNCELLENİYOR..";
        public const string GRAPHIC_LOGO_DIFFERENT = "GRAFİK LOGO\nFARKLI";
        public const string GRAPHIC_LOGO_UPDATING = "GRAFİK LOGO\nGÜNCELLENİYOR..";
        public const string AUTO_CUTTER_DIFFERENT = "OTO KESİCİ\nFARKLI";
        public const string AUTO_CUTTER_UPDATING = "OTO KESİCİ\nGÜNCELLENİYOR..";
        public const string RECEIPT_BARCODE_DIFFERENT = "FİŞ BARKODU\nFARKLI";
        public const string RECEIPT_BARCODE_UPDATING = "FİŞ BARKODU\nGÜNCELLENİYOR..";
        public const string RECEIPT_LIMIT_DIFFERENT = "FİŞ LİMİTİ\nFARKLI";
        public const string RECEIPT_LIMIT_UPDATING = "FİŞ LİMİTİ\nGÜNCELLENİYOR..";
        public const string CURRENCIES_DIFFERENT = "DÖVİZLER FARKLI";
        public const string CURRENCIES_UPDATING = "DÖVİZLER\nGÜNCELLENİYOR..";
        public const string DEPARTMENT_DIFFERENT = "KISIM BİLGİLERİ\nFARKLI";
        public const string DEPARTMENT_UPDATING = "KISIM BİLGİLERİ\nGÜNCELLENİYOR..";
        #endregion

        #region FPU Message
        public const String FPU_ERROR = "FPU HATASI";
        public const String CANNOT_MATCH_EXT_DEV = "FPU İLE EŞLEME\nYAPILAMADI";
        public const String UNDEFINED_TAX_RATE = "KDV ORANI\nTANIMLI DEĞİL";
        public const String CASHIER_AUTHO_EXC = "KASİYER YETKİSİ\nYETERSİZ";
        public const String MANAGER_AUTHO_EXC = "YÖNETİCİ YETKİSİ\nYETERSİZ";
        public const String RE_MATCH_WITH_FPU = "YENIDEN ESLEME\nYAP (GIRIS)";
        public const String TABLE_NUMBER = "MASA NO";
        public const String CLOSE_TABLE = "MASAYI KAPAT";
        public const String START_FM = "ÖKC BAŞLAT";
        public const String INVALID_FISCAL_ID = "GEÇERLİ BİR MALİ\nNUMARA GİRİNİZ";
        public const String FM_STARTED_SUCCSFLY = "ÖKC BAŞARIYLA\nBAŞLATILMIŞTIR";
        public const String CERTIFICATES_CANNOT_UPLOAD = "SERTİFİKALAR\nYÜKLENEMEDİ";
        public const String FM_CANNOT_STARTED = "ÖKC BAŞLATILAMADI\nTEKRAR DENEYİN";
        public const String RECEIPT_ROWCOUNT_EXCEED_ERROR = "SATIR SAYISI\nAŞILDI";
        public const String LAST_EFT_FILENAME = "LAST_EFT_OPT";
        public const String NO_LAST_OPERATION = "SON İŞLEM\nBULUNMAMAKTADIR";
        public const String LAST_OPERATION_FAILED = "SON İŞLEM\nBAŞARISIZ";
    #endregion

        #region FPU Error Messages

        public const string FPU_ERROR_MSG_0 = "İŞLEM BAŞARILI";
        public const string FPU_ERROR_MSG_1 = "VERi EKSİK GELMİŞ\nUZUNLUK KADAR GELMELİ";
        public const string FPU_ERROR_MSG_2 = "VERİ DEĞİŞMİŞ";
        public const string FPU_ERROR_MSG_3 = "UYGULAMA DURUMU\nUYGUN DEĞİL";
        public const string FPU_ERROR_MSG_4 = "BÖYLE BİR KOMUT\nDESTEKLENMİYOR";
        public const string FPU_ERROR_MSG_5 = "PARAMETRE GEÇERSİZ";
        public const string FPU_ERROR_MSG_6 = "OPERASYON BAŞARISIZ";
        public const string FPU_ERROR_MSG_7 = "SİLME GEREKLİ\n(HATA SONRASI)";
        public const string FPU_ERROR_MSG_8 = "KAĞIT YOK";
        public const string FPU_ERROR_MSG_9 = "CİHAZ EŞLEME\nYAPILAMADI";

        public const string FPU_ERROR_MSG_11 = "MALİ BELLEK BİLGİLERİ\nALINAMADI";
        public const string FPU_ERROR_MSG_12 = "MALİ BELLEK\nTAKILI DEĞİL";
        public const string FPU_ERROR_MSG_13 = "MALİ BELLEK\nUYUMSUZLUĞU";
        public const string FPU_ERROR_MSG_14 = "MALİ BELLEK\nFORMATLANMALI";
        public const string FPU_ERROR_MSG_15 = "MALİ BELLEK\nFORMAT HATASI";
        public const string FPU_ERROR_MSG_16 = "FM MALİLEŞTİRME\nYAPILAMADI";
        public const string FPU_ERROR_MSG_17 = "GÜNLÜK Z LİMİT\nDOLDU";
        public const string FPU_ERROR_MSG_18 = "MALİ BELLEK DOLDU";
        public const string FPU_ERROR_MSG_19 = "MALİ BELLEK\nFORMATLANMAMIŞ";
        public const string FPU_ERROR_MSG_20 = "MALİ BELLEK\nKAPATILMIŞ";
        public const string FPU_ERROR_MSG_21 = "GEÇERSİZ\nMALİ BELLEK";
        public const string FPU_ERROR_MSG_22 = "SERTİFİKALAR\nYÜKLENEMEDİ";

        public const string FPU_ERROR_MSG_31 = "EKÜ BİLGİLERİ\nALINAMADI";
        public const string FPU_ERROR_MSG_32 = "EKÜ ÇIKARILDI";
        public const string FPU_ERROR_MSG_33 = "EKÜ KASAYA\nAİT DEĞİL";
        public const string FPU_ERROR_MSG_34 = "ESKİ EKÜ\n(SADECE EKÜ RAPORLARI)";
        public const string FPU_ERROR_MSG_35 = "YENİ EKÜ TAKILDI\nONAY BEKLİYOR";
        public const string FPU_ERROR_MSG_36 = "EKÜ DEĞİŞTİRİLEMEZ\nZ GEREKLİ";
        public const string FPU_ERROR_MSG_37 = "YENİ EKÜ'YE\nGEÇİLEMİYOR";
        public const string FPU_ERROR_MSG_38 = "EKÜ DOLDU\nZ GEREKLİ";
        public const string FPU_ERROR_MSG_39 = "EKÜ DAHA ÖNCE\nFORMATLANMIŞ";
        public const string FPU_ERROR_MSG_51 = "FİŞ LİMİTİ\nAŞILDI";
        public const string FPU_ERROR_MSG_52 = "FİŞ KALEM ADEDİ\nAŞILDI";
        public const string FPU_ERROR_MSG_53 = "SATIŞ İŞLEMİ\nGEÇERSİZ";
        public const string FPU_ERROR_MSG_54 = "İPTAL İŞLEMİ\nGEÇERSİZ";
        public const string FPU_ERROR_MSG_55 = "DÜZELTME İŞLEMİ\nYAPILAMAZ";
        public const string FPU_ERROR_MSG_56 = "İNDİRİM/ARTTIRIM\nİŞLEMİ YAPILAMAZ";
        public const string FPU_ERROR_MSG_57 = "ÖDEME İŞLEMİ\nGEÇERSİZ";
        public const string FPU_ERROR_MSG_58 = "ASGARİ ÖDEME\nSAYISI AŞILDI";
        public const string FPU_ERROR_MSG_59 = "GÜNLÜK ÜRÜN\nSATIŞI AŞILDI";
        public const string FPU_ERROR_MSG_71 = "KDV ORANI TANIMSIZ";
        public const string FPU_ERROR_MSG_72 = "KISIM TANIMLANMAMIŞ";
        public const string FPU_ERROR_MSG_73 = "TANIMSIZ ÜRÜN";
        public const string FPU_ERROR_MSG_74 = "KREDİ ÖDEME BİLGİSİ\nEKSİK/GEÇERSİZ";
        public const string FPU_ERROR_MSG_75 = "DÖVİZ ÖDEME BİLGİSİ\nEKSİK/GEÇERSİZ";
        public const string FPU_ERROR_MSG_76 = "EKÜ'DE KAYIT\nBULUNAMADI";
        public const string FPU_ERROR_MSG_77 = "MALİ BELLEKTE\nKAYIT BULUNAMADI";
        public const string FPU_ERROR_MSG_78 = "ALT ÜRÜN GRUBU\nTANIMLI DEĞİL";
        public const string FPU_ERROR_MSG_79 = "DOSYA BULUNAMADI";
        public const string FPU_ERROR_MSG_91 = "KASİYER YETKİSİ\nYETERSİZ";
        public const string FPU_ERROR_MSG_92 = "SATIŞ VAR";
        public const string FPU_ERROR_MSG_93 = "SON FİŞ Z DEĞİL";
        public const string FPU_ERROR_MSG_94 = "KASADA YETERLİ\nPARA YOK";
        public const string FPU_ERROR_MSG_95 = "GÜNLÜK FİŞ SAYISI\nLİMİTİ AŞILDI";
        public const string FPU_ERROR_MSG_96 = "GÜNLÜK TOPLAM\nAŞILDI";
        public const string FPU_ERROR_MSG_97 = "KASA MALİ DEĞİL";
        public const string FPU_ERROR_MSG_111 = "SATIR UZUNLUĞU\nBEKLENENDEN FAZLA";
        public const string FPU_ERROR_MSG_112 = "KDV ORANI\nGEÇERSİZ";
        public const string FPU_ERROR_MSG_113 = "DEPARTMAN NUMARASI\nGEÇERSİZ";
        public const string FPU_ERROR_MSG_114 = "PLU NUMARASI\nGEÇERSİZ";
        public const string FPU_ERROR_MSG_115 = "GEÇERSİZ TANIM";
        public const string FPU_ERROR_MSG_116 = "BARKOD GEÇERSİZ";
        public const string FPU_ERROR_MSG_117 = "GEÇERSİZ OPSİYON";
        public const string FPU_ERROR_MSG_118 = "TOPLAM TUTMUYOR";
        public const string FPU_ERROR_MSG_119 = "GEÇERSİZ MİKTAR";
        public const string FPU_ERROR_MSG_120 = "GEÇERSİZ TUTAR";
        public const string FPU_ERROR_MSG_121 = "MALİ NUMARA HATALI";
        public const string FPU_ERROR_MSG_131 = "KAPAKLAR AÇILDI";
        public const string FPU_ERROR_MSG_132 = "MALİ BELLEK MESH\nZARAR VERİLDİ";
        public const string FPU_ERROR_MSG_133 = "HUB MESH\nZARAR VERİLDİ";
        public const string FPU_ERROR_MSG_134 = "Z ALINMALI\n(24 SAAT GEÇTİ)";
        public const string FPU_ERROR_MSG_135 = "DOĞRU EKÜ TAK\nYENİDEN BAŞLAT";
        public const string FPU_ERROR_MSG_136 = "SERTİFİKA YÜKLENEMEDİ";
        public const string FPU_ERROR_MSG_137 = "TARİH-SAAT AYARLAYIN";
        public const string FPU_ERROR_MSG_138 = "GÜNLÜK İLE MALİ\nBELLEK UYUMSUZ";
        public const string FPU_ERROR_MSG_139 = "VERİTABANI HATASI";
        public const string FPU_ERROR_MSG_140 = "LOG HATASI";
        public const string FPU_ERROR_MSG_141 = "SRAM HATASI";
        public const string FPU_ERROR_MSG_142 = "SERTİFİKA UYUMSUZ";
        public const string FPU_ERROR_MSG_143 = "VERSİYON HATASI";
        public const string FPU_ERROR_MSG_144 = "GÜNLÜK LOG SAYISI\nAŞILDI";
        public const string FPU_ERROR_MSG_145 = "YAZARKASAYI YENİDEN\nBAŞLAT";
        public const string FPU_ERROR_MSG_146 = "GÜNLÜK HATALI GİRİŞ\nSAYISI AŞILDI";
        public const string FPU_ERROR_MSG_147 = "MALİLEŞTİRME YAPILDI\nYENİDEN BAŞLAT";
        public const string FPU_ERROR_MSG_148 = "GİB'E BAĞLANILAMADI";
        public const string FPU_ERROR_MSG_149 = "SERTİFİKA İNDİRİLDİ\nYENİDEN BAŞLAT";
        public const string FPU_ERROR_MSG_150 = "GÜVENLİ ALAN\nFORMATLANAMADI";
        public const string FPU_ERROR_MSG_151 = "JUMPER ÇIKART TAK";

        public const string FPU_ERROR_MSG_170 = "BAĞLI EFT\nCİHAZI YOK";
        public const string FPU_ERROR_MSG_171 = "EFT DURUMU\nUYGUN DEĞİL";
        public const string FPU_ERROR_MSG_172 = "HATALI KART";
        public const string FPU_ERROR_MSG_173 = "TUTAR UYUŞMUYOR";
        public const string FPU_ERROR_MSG_174 = "PROVİZYON YOK";
        public const string FPU_ERROR_MSG_175 = "DESTEKLENMEYEN\nTAKSİT SAYISI";
        public const string FPU_ERROR_MSG_176 = "EFT İPTAL BAŞARISIZ";
        public const string FPU_ERROR_MSG_177 = "EFT İADE BAŞARISIZ";
        public const string FPU_ERROR_MSG_178 = "EFT EK NÜSHA BAŞARISIZ";
        public const string FPU_ERROR_MSG_179 = "MEVCUT MODDA GERÇEKLEŞTİRİLEMEZ";
        public const string FPU_ERROR_MSG_180 = "GEÇERSİZ EFT MODU";

        #endregion

        public const string INVOICE_SERIAL = "FATURA SERİ NO (AA)";

        public const string CUSTOMER_NAME_OR_TITLE = "MÜŞTERİ İSİM/ÜNVAN";

        public const string CAR_PLATE = "ARAÇ PLAKASI";

        public const string COLLECTION_AMOUNT = "TAHSİLAT TUTARI";

        public const string COMISSION_AMOUNT = "KOMİSYON TUTARI";

        public const string INSTUTION_NAME = "KURUM ADI";

        public const string SUBSCRIBER_NO = "ABONE NO";

        public const string EFT_SLIP_COPY = "EFT SLİP KOPYASI";

        public const string LAST_OPERATION = "SON İŞLEM";

        public const string EFT_POS_OPERATIONS = "EFT-POS İŞLEMLERİ";

        public const string BATCH_NO = "GRUP NO";

        public const string ACQUIER_ID = "BANKA KODU";

        public const string STAN_NO = "İŞLEM NO";

        public const string ALERT_INVALID_VAT_ON_MEAL_TICKET = "BU KDV GRUBUNDAN\nÜRÜN SATILAMAZ";

        public const string SERIAL = "FATURA SERİ NO";

        public const string CUSTOMER_INFO = "ALICI BİLGİLERİ";

        public const string INVOICE_PROFIL = "FATURA SENARYO";

        public const string ADDRESS_INFO = "ADRES BİLGİLERİ";

        public const string CONTACT_INFO = "İLETİŞİM BİLGİLERİ";

        public const string ADDITIONAL_INFO = "EK BİLGİLER";

        public const string CLEAR_INPUTS = "BİLGİLERİ TEMİZLE";

        public const string FILL_AUTO = "OTOMATİK DOLDUR";

        public const string APPLY_CLEAR_ADD_INFO = "BİLGİLERİ TEMİZLE\nONAY? (GİRİŞ)";

        public const string INPUTS_CLEAR = "BİLGİLER BAŞARIYLA\nTEMİZLENDİ";

        public const string CI_TITLE = "ÜNVAN";

        public const string CI_NAME = "İSİM";

        public const string CI_FAMILY_NAME = "SOYİSİM";

        public const string CI_TAX_SCHEME = "VERGİ DAİRESİ";

        public const string CI_ROOM = "KAPI NO";

        public const string CI_BUILDING_NO = "BİNA NO";

        public const string CI_BUILDING_NAME = "BİNA ADI";

        public const string CI_STREET = "SOKAK/BULVAR ADI";

        public const string CI_DISTRICT = "MAHALLE ADI";

        public const string CI_VILLAGE = "KÖY/SEMT ADI";

        public const string CI_SUBCITY = "İLÇE";

        public const string CI_CITY = "İL";

        public const string CI_COUNTRY = "ÜLKE";

        public const string CI_POSTAL_CODE = "POSTA KODU";

        public const string CI_TELEPHONE = "TELEFON NO";

        public const string CI_FAX = "FAX";

        public const string CI_EMAIL = "ELEKTRONİK POSTA";

        public const string CI_WEB_PAGE = "WEB SAYFASI";

        public const string BASIC_INVOICE = "TEMEL FATURA";

        public const string TRADING_INVOICE = "TİCARİ FATURA";

        public const string CHECKING_TAXPAYER_STATUS = "MÜKELLEF SORGULAMA\nYAPILIYOR...";

        public const string NOT_EINVOICE_TAXPAYER = "KİŞİ E-FATURA\nMÜKELLEFİ DEĞİLDİR";

        public const string TRY_AGAIN_OR_HOLD_DOC = "TEKRAR DENE (G)\nBEKLEMEYE AL (Ç)";

        public const string SLIP_PRINTING = "FATURA\nYAZDIRILIYOR..";

        public const string DEFAULT_DOCUMENT_TYPE = "DEFAULT BELGE TİPİ";

        public const string AUTH_NOT_DEFINED = "TANIMSIZ\n YETKİ SEVİYESİ";

        public const string PRINTING_PLS_WAIT = "YAZDIRILIYOR..\nBEKLEYİNİZ";

        public const string GO_EFT_POS_SIDE = "EFT-POS TARAFINA\nGEÇİNİZ";

        public const string MAX_LENGTH = "MAKSİMUM UZUNLUK";

        public const string PRESS_ENTER_TO_AUTH = "YETKİLİ GİRİŞİ (G)\nİŞLEM İPTAL (Ç)";

        public const string PRINT_AGAIN_OR_CONTINUE = "TEKRAR YAZDIR (G)\nÇIKIŞ (Ç)";

        public const string SELF_EMP_INV_SERVICE_DEFINITION = "HİZMET TANIMI";

        public const string SELF_EMP_INV_SERVICE_GROSS_WAGES = "BRÜT ÜCRET";

        public const string SELF_EMP_INV_SERVICE_STOPPAGE_RATE = "STOPAJ MİKTARI %";

        public const string SELF_EMP_INV_SERVICE_VAT_RATE = "KDV ORANI %";

        public const string SELF_EMP_INV_SERVICE_STOPPAGE_OTHER_RATE = "KDV TEVFİKAT %";
    }
#endif
}
