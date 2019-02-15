using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public enum DataTypes
    {
        Product = 1,
        Cashier = 2,
        Customer = 4,
        Diplomatic = 8,
        Currency = 16,
        Credit = 32,
        Serial = 64,
        Category = 128
    }

    public delegate void LogSavedEventHandler(object sender, LogSavedEventArgs e);
    public class LogSavedEventArgs : EventArgs
    {
        string log;
        public LogSavedEventArgs(string log)
        {
            this.log = log;
        }

        public string Log
        {
            get { return log; }
        }
    }

    public interface IDataConnector
    {
        int GetLastSuccess(DataTypes type);
        int GetLastFail(DataTypes type);
        event EventHandler ProductsUpdated;
        event LogSavedEventHandler LogSaved;

        #region Settings

        void LoadSettings();
        ISettings LoadNewSettings();
        void AcceptNewSettings();
        ISettings NewSettings { get;}
        ISettings CurrentSettings { get;}

        #endregion Settings 

        #region Category
        List<Category> LoadCategories();
        #endregion

        #region Product

        IProduct CreateProduct(string name, Department department, decimal price);
        ICashier CreateCashier(string name, String id);
        ICustomer CreateCustomer(string code, string name, string address, string taxInstitution, string taxNumber);

        void LoadProducts();
        void UpdateProducts();
        void LoadSerialNumbers();
        void AddToStock(String barcode, decimal quantity);
        bool AvailableSerialNumber(string serial);

        IProduct FindProductByName(String name);
        IProduct FindProductByBarcode(String barcode);
        IProduct FindProductByLabel(String plu);
        List<IProduct> SearchProductByBarcode(String[] barcodeData);
        List<IProduct> SearchProductByLabel(String[] pluList);
        List<IProduct> SearchProductByName(String nameData);

        #endregion Product

        #region Currency

        int MaxCurrencyCount { get;set;}
        void LoadCurrencies();       
        Dictionary<int, ICurrency> GetCurrencies();

        #endregion Currency
            
        #region Credit

        int MaxCreditCount { get;set;}
        Dictionary<int, ICredit> GetCredits();

        #endregion Credit
        
        #region Cashier

        void LoadCashiers();
        ICashier FindCashierByPassword(String password);
        ICashier FindCashierById(String id);
        List<ICashier> SearchCashiersByInfo(String info);

        #endregion Cashier
        
        #region Customer

        void LoadCustomers();
        void UpdateCustomers();

        ICustomer FindCustomerByCode(string customerNumber);
        ICustomer FindCustomerByCardNo(String code);
        ICustomer FindCustomerByName(String name);
        ICustomer FindCustomerByTcknVkn(String tcknVkn);
        List<ICustomer> SearchCustomersByInfo(String info);
        ICustomer SaveCustomer(String line);

        #endregion Customer
                
        #region All Data

        void LoadAll();

        #endregion All Data
                
        #region Communication

        void Connect();
        bool IsOnline { get;}
        string BackOfficeCommand { get;}
        void TransferOfflineData();
        int ProcessRequest(String request);
        void SendWaitingMessage(String message);
        void SendMessage(bool success, String message);

        #endregion Communication
        
        #region Log

        void StartLog(String exeVersion);
        void OnDocumentClosed(ISalesDocument document);
        void OnDocumentVoided(ISalesDocument document, int voidedReason);
        void OnDocumentUpdated(ISalesDocument document, int documentStatus);
        void OnDocumentSuspended(ISalesDocument document, int zReportNo);
        void OnReturnDocumentClosed(ISalesDocument document);
        void OnCashierLogin(ICashier cashier, int zReportNo);
        void OnCashierLogout(ICashier cashier);
        void OnDeposit(Decimal amount);
        void OnWithdrawal(Decimal amount);
        void OnWithdrawal(Decimal amount, String refNumber);
        void OnWithdrawal(Decimal amount, ICredit credit);
        void CheckZWritten(int zNo, int documentId);
        void OnZReportComplete(int zReportNo, DateTime ZReportDate, bool isFiscal);
        void OnNetworkDown();
        string FormatLines(ISalesDocument document);
        void SaveReport(String filename, String reportText);
        void ResetSequenceNumber();

        #endregion Log

        #region Program

        void GetNewProgram();

        #endregion Program

        #region RegisterFile

        void UploadRegisterFile(int lastZNo);

        #endregion Program
        
        #region Report

        void PrepareZReport(String registerId);
        void AfterZReport(String registerId);
        decimal GetRegisterCash(String registerId);
        int GetLastDocumentId(String registerId);
        string GetReportXml(String registerId);
        string GetReportXml(String registerId, DateTime day);
        string GetReportXml(String registerId, String cashierCode, DateTime firstDate, DateTime lastDate);
        string[] GetReturnAmounts(String registerId);

        #endregion Report

        IPointAdapter PointAdapter { get;}
    }
}
