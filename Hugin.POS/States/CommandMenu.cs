using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
	/// <summary>
	/// Description of State.
	/// </summary>
	/// 

    class CommandMenu:State
    {
        /// <summary>
        /// - definition of commandmenu evets
        /// </summary>
        private static IState state = new CommandMenu();
        private static IDoubleEnumerator ie;

        private static int countToSellingState = 0;
        /// <summary>
        /// - CommandMenu Instance.
        /// </summary>
        /// <returns>CommandMenu State</returns>
        private static IState Instance()
        {
            if (ie == null)
                return AlertCashier.Instance(new Error(new InvalidProgramException()));
            return ListCommandMenu.Instance(ie, new ProcessSelectedItem<MenuLabel>(SelectActionMenu));            
        }
        /// <summary>
        /// - CommandMenu Instance
        /// </summary>
        /// <param name="ide"></param>
        /// <returns>Command menu State</returns>
        internal static IState Instance(IDoubleEnumerator ide)
        {
            ie = ide;
            return Instance();
        }

        /// <summary>
        /// -Continue from current menu
        /// </summary>
        /// <returns>CommandMenu State</returns>
        public static IState Continue()
        {            
            ie.MovePrevious();
            return Instance();
        }

        internal static MenuLabel AddLabel(int index, String msg)
        {
            return new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.COMMAND, index, msg));
        }

        /// <summary>
        /// - Command menu selection.
        /// - Some of menu items are confirmed to cahier before take action. 
        /// </summary>
        /// <param name="menu">Selected menu</param>
        public static void SelectActionMenu(MenuLabel menu)
        {
            string message = menu.ToString();
            message = message.Substring(message.IndexOf('\n') + 1);

            switch (message)
            {
                case PosMessage.VOID_DOCUMENT:
                    cr.State = ConfirmAuthorization.Instance(new Confirm(PosMessage.CONFIRM_VOID_DOCUMENT,
                                                                       new StateInstance(VoidDocument),
                                                                       new StateInstance(Continue)),
                                                             Authorizations.VoidDocument);                                
                    break;
                case PosMessage.REPEAT_DOCUMENT:
                    if (cr.IsAuthorisedFor(Authorizations.SuspendAndRepeatDocAuth))
                        cr.State = ChooseRepeatType();
                    else
                        throw new CashierAutorizeException();  
                    break;
                case PosMessage.TRANSFER_DOCUMENT:
                    cr.State = TransferDocument();

                    //SalesDocument salesDoc = new Invoice(cr.Document);
                    //String confirmationMessage = String.Format(PosMessage.CHANGE_DOCUMENT, salesDoc.Name);
                    //Confirm e = new Confirm(confirmationMessage,
                    //                    new StateInstance<Hashtable>(ListDocument.LDChangeConfirmed),
                    //                    new StateInstance(Start.Instance));
                    //e.Data["Document"] = salesDoc;
                    //CashRegister.State = ConfirmCashier.Instance(e);
                    return;
                case PosMessage.SUSPEND_DOCUMENT:
                    cr.State = ConfirmAuthorization.Instance(new Confirm(PosMessage.CONFIRM_SUSPEND_DOCUMENT,
                                                                    new StateInstance(SuspendDocument),
                                                                    new StateInstance(Continue)), Authorizations.SuspendAndRepeatDocAuth);
                    break;
                case PosMessage.RESUME_DOCUMENT:
                    cr.State = ResumeDocument();
                    break;
                case PosMessage.CUSTOMER_ENTRY:
                    cr.State = EnterCustomer();
                    break;
                case PosMessage.EFT_SLIP_COPY:
                    cr.State = SlipCopySearchType();
                    break;
                case PosMessage.EFT_POS_OPERATIONS:
                    cr.State = EftPosMenu();
                    break;
                //case PosMessage.FAST_PAYMENT:
                //    cr.State = OpenOrders();
                //    break;
                case PosMessage.TABLE_MANAGEMENT:
                    cr.State = OpenTables();
                    break;
                case PosMessage.ENTER_CASH:
                    if (cr.IsAuthorisedFor(Authorizations.CashInCashOut))
                    {
                        cr.State = EnterCurrency.Instance(PosMessage.CASH_AMOUNT,
                                                       new StateInstance<decimal>(DepositCashToRegister),
                                                       new StateInstance(Continue));
                    }
                    else
                        throw new CashierAutorizeException();
                    break;
                case PosMessage.RECEIVE_CASH:
                    if (cr.IsAuthorisedFor(Authorizations.CashInCashOut))
                    {
                        cr.State = EnterCurrency.Instance(PosMessage.CASH_AMOUNT,
                                                       new StateInstance<decimal>(WithdrawCashFromRegister),
                                                       new StateInstance(Continue));
                    }
                    else
                        throw new CashierAutorizeException();
                    break;             
                //case PosMessage.RECEIVE_CHECK:
                //    cr.State = EnterCurrency.Instance(PosMessage.CASH_AMOUNT,
                //                                   new StateInstance<decimal>(WithdrawCheckFromRegister),
                //                                   new StateInstance(Continue));
                //    break;
                //case PosMessage.RECEIVE_CREDIT:
                //    cr.State = EnterCurrency.Instance(PosMessage.CASH_AMOUNT,
                //                                   new StateInstance<decimal>(WithdrawCreditSlipsFromRegister),
                //                                   new StateInstance(Continue));
                //    break;
                case PosMessage.COMMAND_CALCULATOR:
                    cr.State = States.Calculator.Instance();
                    break;
                case PosMessage.TALLYING:
                    cr.State = States.Teller.Instance();
                    break;
            }
        }

        /// <summary>
        /// - Void current document
        /// </summary>
        /// <returns>Start state</returns>
        public static IState VoidDocument()
        {
            cr.Document.Void();
            return Start.Instance();
        }

        public static IState ChooseRepeatType()
        {
            MenuList searchTypeMenu = new MenuList();
            String label = "";

            int i = 1;

            string labelFormat = "{0}\t{1}\n{2}";
            label = String.Format(labelFormat, PosMessage.REPEAT_DOCUMENT, i++, "BELGE NO ÝLE");
            searchTypeMenu.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.REPEAT_DOCUMENT, i++, "BARKOD ÝLE");
            searchTypeMenu.Add(new MenuLabel(label));

            return List.Instance(searchTypeMenu, GetRepeatType, Continue);
        }

        public static void GetRepeatType(Object o)
        {
            MenuLabel label = o as MenuLabel;

            if (label.Value.ToString().Contains("BELGE NO ÝLE"))
            {
                cr.State = EnterInteger.Instance(PosMessage.DOCUMENT_ID,
                                                   new StateInstance<int>(RepeatDocument),
                                                   new StateInstance(Continue));
            }
            else if (label.Value.ToString().Contains("BARKOD ÝLE"))
            {
                cr.State = EnterString.Instance(PosMessage.BARCODE,
                                                    new StateInstance<string>(RepeatDocument),
                                                    new StateInstance(Continue));
            }

        }

        /// <summary>
        /// - Repeates document by ID printed before.
        /// </summary>
        /// <param name="documentId">
        /// - Document Id that will be printed.
        /// </param>
        /// <returns>returns Start state</returns>
        public static IState RepeatDocument(int documentId)
        {
            SalesDocument doc = cr.Document.GetDocumentFromLastZ(documentId);
            System.Threading.Thread.Sleep(250);
            if (doc != null && doc.Items.Count > 0)
            {

                String label = DisplayAdapter.DocumentFormat(doc);
                Confirm e = new Confirm(label,
                                    new StateInstance<Hashtable>(RepeatConfirmed),
                                    new StateInstance(Continue));
                e.Data["Document"] = doc;
                return ConfirmCashier.Instance(e);
            }
            else
            {
                return AlertCashier.Instance(new Confirm(PosMessage.DOCUMENT_ID_NOT_FOUND,
                                      new StateInstance(Continue)));
            }
        }

        public static IState RepeatDocument(string barcode)
        {
            SalesDocument doc = cr.Document.GetDocumentByBarcode(barcode);
            System.Threading.Thread.Sleep(250);
            if (doc != null && doc.Items.Count > 0)
            {

                String label = DisplayAdapter.DocumentFormat(doc);
                Confirm e = new Confirm(label,
                                    new StateInstance<Hashtable>(RepeatConfirmed),
                                    new StateInstance(Continue));
                e.Data["Document"] = doc;
                return ConfirmCashier.Instance(e);
            }
            else
            {
                return AlertCashier.Instance(new Confirm(PosMessage.BARCODE_NOTFOUND,
                                      new StateInstance(Continue)));
            }
        }

        static IState RepeatConfirmed(Hashtable data)
        {
            SalesDocument document = null;
            DocumentFileHelper helper = null;

            if (data["Document"] is DocumentFileHelper)
            {
                helper = ((DocumentFileHelper)data["Document"]);
                helper.LoadDocument();
            }
            else
               document = (SalesDocument)data["Document"];
            if (document.Status == DocumentStatus.Suspended)
                document.Resume();

            /*ICustomer customer = cr.Document.Customer;
            cr.Document = (SalesDocument)document.Clone();

            if (customer != null)
                cr.Document.Customer = customer;
            */
            if (document.FileOnDisk != null)
            {
                if (document.FileOnDisk.Name.IndexOf("MASA") > -1)
                    DisplayAdapter.Cashier.ClearTableContent();
            }
            cr.ChangeDocumentType((SalesDocument)document.Clone());
            cr.Document.Id = cr.Printer.CurrentDocumentId;

            return cr.State;
        }
        /// <summary>
        /// - repeates document printed before.
        /// - Documents:Receipt,Invoice,Waybill,ReturnDocument,DiplomaticSale
        /// </summary>
        /// <param name="o"></param>
        public static void RepeatDocument(Object o)
        {
            DocumentFileHelper docHelper = null;
            if (o is DocumentFileHelper)
                docHelper = o as DocumentFileHelper;
            else
            {
                MenuLabel label = o as MenuLabel;
                docHelper = label.Value as DocumentFileHelper;
            }
            Hashtable data = new Hashtable();

            SalesDocument document = null;
            document = docHelper.LoadDocument();
            document.FileOnDisk = docHelper.FileInfo;

            

            data.Add("Document", document);
            cr.State = RepeatConfirmed(data);
            //cr.Document.Id = docHelper.Id;
            cr.Document.Id = cr.Printer.CurrentDocumentId;
            docHelper.Remove(cr.Document);

        }

        public static void ShowTableOnDisp(Object o)
        {
            DocumentFileHelper docHelper = null;
            SalesDocument document = null;
            decimal totalAdjAmount = 0.0m;
            
            if (o is DocumentFileHelper)
                docHelper = o as DocumentFileHelper;
            else
            {
                MenuLabel label = o as MenuLabel;
                docHelper = label.Value as DocumentFileHelper;
            }

            if (docHelper.FileInfo.Name.IndexOf("MASA10") > -1)
            {
                cr.State = AlertCashier.Instance(new Confirm("MASA BOÞ", OpenTables));                
            }            
            else if (countToSellingState == 1)
            {
                document = docHelper.LoadDocument();
                document.FileOnDisk = docHelper.FileInfo;
                document.Id = cr.Document.Id;
                String label = "{0}:{1:D3}\t{2}\n{3}?({4})";
                label = String.Format(label, PosMessage.TABLE_NUMBER, docHelper.Id, document.TotalAmount, PosMessage.CLOSE_TABLE, PosMessage.ENTER);
                Confirm e = new Confirm(label,
                                    new StateInstance<Hashtable>(RepeatConfirmed),
                                    new StateInstance(OpenTables));
                
                countToSellingState =0;
                e.Data["Document"] = document;
                cr.State = ConfirmCashier.Instance(e);
                docHelper.Id = cr.Document.Id;
                docHelper.Remove(cr.Document);
            }
            else
            {
                document = docHelper.LoadDocument();
                foreach (Adjustment adj in document.Adjustments)
                {
                    totalAdjAmount += adj.NetAmount;
                }

                DisplayAdapter.Customer.ShowTableContent(document, totalAdjAmount);
                countToSellingState++;
            }

        }
        /// <summary>
        /// - suspend document and it will be repeated later.
        /// </summary>
        /// <returns></returns>
        public static IState SuspendDocument()
        {
            cr.Document.Suspend();
            return Start.Instance();
        }


        private static IState OpenOrders()
        {

            MenuList orderDocsMenu = new MenuList();
            String label = "";

            List<DocumentFileHelper> orderDocuments = new List<DocumentFileHelper>(DocumentFileHelper.GetOpenOrders("", false));
            foreach (DocumentFileHelper helper in orderDocuments)
            {
                //SalesDocument doc = helper.LoadDocument();
                String cashiername = helper.Cashier.Name;
                cashiername = cashiername.Substring(0, Math.Min(14, cashiername.Length));
                label = "BEL NO:{0:D4}\t{1:dd/MM/yy}\n{2}\t{1:HH:mm}";
                label = String.Format(label, helper.Id, helper.CreationTime, cashiername);

                orderDocsMenu.Add(new MenuLabel(label, helper));
            }
            return List.Instance(orderDocsMenu, RepeatDocument) ;
        }

        private static IState EftPosMenu()
        {
            MenuList eftMenu = new MenuList();
            string label = "";
            int i = 1;

            string labelFormat = "{0}\t{1}\n{2}";
            if (cr.IsAuthorisedFor(Authorizations.EFTVoidAndRefundAuth))
            {
                label = String.Format(labelFormat, PosMessage.EFT_POS_OPERATIONS, i++, PosMessage.VOID);
                eftMenu.Add(new MenuLabel(label));

                label = String.Format(labelFormat, PosMessage.EFT_POS_OPERATIONS, i++, PosMessage.RETURN_DOCUMENT_TR);
                eftMenu.Add(new MenuLabel(label));
            }

            label = String.Format(labelFormat, PosMessage.EFT_POS_OPERATIONS, i++, PosMessage.EFT_SLIP_COPY);
            eftMenu.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.EFT_POS_OPERATIONS, i++, PosMessage.LAST_OPERATION);
            eftMenu.Add(new MenuLabel(label));

            return List.Instance(eftMenu, EftOperation, Continue);
        }

        static string selectedEftOperation = "";
        private static void EftOperation(Object o)
        {
            MenuLabel label = o as MenuLabel;
            selectedEftOperation = label.Value.ToString().Split('\n')[1];

            switch(selectedEftOperation)
            {
                case PosMessage.VOID:
                    GetAcquierId(o);
                    break;
                case PosMessage.RETURN_DOCUMENT_TR:
                    GetAcquierId(o);
                    break;
                case PosMessage.EFT_SLIP_COPY:
                    cr.State = SlipCopySearchType();
                    break;
                case PosMessage.LAST_OPERATION:
                    if (!File.Exists(PosMessage.LAST_EFT_FILENAME))
                        cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.NO_LAST_OPERATION, Continue, Continue));
                    else
                    {
                        try
                        {
                            string[] lines      = File.ReadAllLines(PosMessage.LAST_EFT_FILENAME);
                            string acquierID    = lines[0];
                            string batch        = lines[1];
                            string stan         = lines[2];
                            string zNo          = lines[3];
                            string docNo        = lines[4];

                            selectedEftOperation    = PosMessage.EFT_SLIP_COPY;
                            currentAcquierId        = Convert.ToInt32(acquierID);
                            currentBatchNo          = Convert.ToInt32(batch);

                            // Print EJ copy before eft slip copy
                            cr.Printer.PrintEJDocument(Convert.ToInt32(zNo), Convert.ToInt32(docNo), false);
                            cr.State                = SetStanNo(Convert.ToInt32(stan));
                        }
                        catch
                        {
                            cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.LAST_OPERATION_FAILED, Continue, Continue));
                        }
                    }
                    break;
            }
        }

        private static IState SlipCopySearchType()
        {
            MenuList searchTypeMenu = new MenuList();
            String label = "";

            int i = 1;

            string labelFormat = "{0}\t{1}\n{2}";
            label = String.Format(labelFormat, PosMessage.EFT_SLIP_COPY, i++, "Z NO - FÝÞ NO");
            searchTypeMenu.Add(new MenuLabel(label));

            label = String.Format(labelFormat, PosMessage.EFT_SLIP_COPY, i++, "BATCH NO - STAN NO");
            searchTypeMenu.Add(new MenuLabel(label));

            return List.Instance(searchTypeMenu, GetAcquierId, Continue);
        }

        private static void GetAcquierId(Object o)
        {
            MenuLabel label = o as MenuLabel;

            if (label.Value.ToString().Contains("Z NO"))
                isZNoDocNo = true;
            else   
                isZNoDocNo = false;
                         
            cr.State = EnterInteger.Instance(PosMessage.ACQUIER_ID,
                                                   new StateInstance<int>(SetAcquierId),
                                                   new StateInstance(Continue));
        }

        private static IState SetAcquierId(int acquierId)
        {
            currentAcquierId = acquierId;

            if (isZNoDocNo)
                return EnterInteger.Instance(PosMessage.Z_NO,
                                                   new StateInstance<int>(SetZNo),
                                                   new StateInstance(Continue));
            else if (selectedEftOperation == PosMessage.RETURN_DOCUMENT_TR)
                return SetStanNo(0);
            else
                return EnterInteger.Instance(PosMessage.BATCH_NO,
                                                   new StateInstance<int>(SetBatchNo),
                                                   new StateInstance(Continue));
        }

        private static bool isZNoDocNo = false;
        private static int currentAcquierId = 0;
        private static int currentZNo = 0;
        private static int currentBatchNo = 0;
        private static IState SetZNo(int zNo)
        {
            currentZNo = zNo;

            return EnterInteger.Instance(PosMessage.DOCUMENT_ID,
                                                   new StateInstance<int>(SetDocumentNo),
                                                   new StateInstance(Continue));
        }

        private static IState SetDocumentNo(int docNo)
        {
            if (docNo > 0 && currentZNo > 0)
            {
                cr.GetEFTSlipCopy(currentAcquierId, 0, 0, currentZNo, docNo);
                return Continue();
            }
            else
            {
                return AlertCashier.Instance(new Confirm(PosMessage.INVALID_ENTRY, Continue, Continue));
            }
        }

        private static IState SetBatchNo(int batchNo)
        {
            currentBatchNo = batchNo;

            return EnterInteger.Instance(PosMessage.STAN_NO,
                                                   new StateInstance<int>(SetStanNo),
                                                   new StateInstance(Continue));
        }

        private static IState SetStanNo(int stanNo)
        {
            if (selectedEftOperation == PosMessage.LAST_OPERATION ||
                selectedEftOperation == PosMessage.EFT_SLIP_COPY)
                DisplayAdapter.Cashier.Show(PosMessage.PRINTING_PLS_WAIT);
            else
                DisplayAdapter.Cashier.Show(PosMessage.GO_EFT_POS_SIDE);

            if (currentBatchNo > 0 && stanNo > 0)
            {
                switch(selectedEftOperation)
                {
                    case PosMessage.VOID:                        
                        cr.Printer.VoidEFTPayment(currentAcquierId, currentBatchNo, stanNo);
                        break;
                    case PosMessage.EFT_SLIP_COPY:
                        cr.GetEFTSlipCopy(currentAcquierId, currentBatchNo, stanNo, 0, 0);
                        break;
                }              
            }
            else if(selectedEftOperation == PosMessage.RETURN_DOCUMENT_TR)
            {
                cr.Printer.RefundEFTPayment(currentAcquierId);
            }
            else
                return AlertCashier.Instance(new Confirm(PosMessage.INVALID_ENTRY, Continue, Continue));

            return Start.Instance();
        }

        private static IState TransferDocument()
        {
            MenuList toTransferDocsMenu = new MenuList();
            String label = "";
            SalesDocument sDoc;

            int i = 1;

            string labelFormat = "{0}\n{1}-{2}";
            label = String.Format(labelFormat, PosMessage.SELECT_DOCUMENT, i++, PosMessage.INVOICE);
            sDoc = new Invoice(cr.Document);
            toTransferDocsMenu.Add(new MenuLabel(label, sDoc));

            label = String.Format(labelFormat, PosMessage.SELECT_DOCUMENT, i++, PosMessage.E_INVOICE);
            sDoc = new EInvoice(cr.Document);
            toTransferDocsMenu.Add(new MenuLabel(label, sDoc));

            label = String.Format(labelFormat, PosMessage.SELECT_DOCUMENT, i++, PosMessage.E_ARCHIVE);
            sDoc = new EArchive(cr.Document);
            toTransferDocsMenu.Add(new MenuLabel(label, sDoc));

            label = String.Format(labelFormat, PosMessage.SELECT_DOCUMENT, i++, PosMessage.MEAL_TICKET);
            sDoc = new MealTicket(cr.Document);
            toTransferDocsMenu.Add(new MenuLabel(label, sDoc));

            return List.Instance(toTransferDocsMenu, TransferToChoosenDoc, Continue);
        }

        private static void TransferToChoosenDoc( Object o)
        {
            MenuLabel label = o as MenuLabel;
            SalesDocument salesDoc = label.Value as SalesDocument;

            String confirmationMessage = String.Format(PosMessage.CHANGE_DOCUMENT, salesDoc.Name);
            Confirm e = new Confirm(confirmationMessage,
                                new StateInstance<Hashtable>(ListDocument.LDChangeConfirmed),
                                new StateInstance(Start.Instance));
            e.Data["Document"] = salesDoc;
            CashRegister.State = ConfirmCashier.Instance(e);
        }

        private static IState OpenTables()
        {
            countToSellingState = 0;
            MenuList tableDocsMenu = new MenuList();
            String label = "";
            DisplayAdapter.Customer.ClearTableContent();
            List<DocumentFileHelper> tableDocuments = new List<DocumentFileHelper>(DocumentFileHelper.GetOpenTables());

            foreach (DocumentFileHelper helper in tableDocuments)
            {
                if (helper.FileInfo.Name.IndexOf("MASA10") > -1)
                {
                    label = "{0}:{1:D3}\t{2:dd/MM/yy}\n{3}\t{4}";
                    label = String.Format(label, PosMessage.TABLE_NUMBER, helper.Id, helper.CreationTime, PosMessage.TOTAL, 0);                   
                }
                else
                {
                    SalesDocument salesDocument = helper.LoadDocument();

                    label = "{0}:{1:D3}\t{2:dd/MM/yy}\n{3}\t{4}";
                    label = String.Format(label, PosMessage.TABLE_NUMBER, helper.Id, helper.CreationTime, PosMessage.TOTAL, salesDocument.TotalAmount);
                }
                
                tableDocsMenu.Add(new MenuLabel(label, helper));
            }

            return List.Instance(tableDocsMenu, ShowTableOnDisp, Continue);
        }


        public static IState OpenOrderByBarcode(string orderBarcode)
        {

            MenuList orderDocsMenu = new MenuList();
            String label = "";
            String cashierId = orderBarcode.Substring(0, 4);
            int orderId = int.Parse(orderBarcode.Substring(4, 4));

            List<DocumentFileHelper> orderDocuments = new List<DocumentFileHelper>(DocumentFileHelper.GetOpenOrders("", true));
            foreach (DocumentFileHelper helper in orderDocuments)
            {
                if (helper.Cashier.Id == cashierId && 
                    helper.Id == orderId)
                {
                    //SalesDocument doc = helper.LoadDocument();
                    String cashiername = helper.Cashier.Name;
                    cashiername = cashiername.Substring(0, Math.Min(14, cashiername.Length));
                    label = "SIP NO:{0:D4}\t{1:dd/MM/yy}\n{2}\t{1:HH:mm}";
                    label = String.Format(label, helper.Id, helper.CreationTime, cashiername);

                    orderDocsMenu.Add(new MenuLabel(label, helper));
                }
            }
            if (orderDocsMenu.Count == 0)
            {
                return AlertCashier.Instance(new Confirm(PosMessage.NO_DOCUMENT_FOUND));
            }
            else
            {
                return List.Instance(orderDocsMenu, RepeatDocument);
            }
        }
        /// <summary>
        /// - repeats document printed before.
        /// </summary>
        /// <returns>State of List.cs</returns>
        public static IState ResumeDocument()
        {
            List<DocumentFileHelper> suspendedDocuments = new List<DocumentFileHelper>();
            String format = "BEK" + (cr.PrinterLastZ + 1).ToString().PadLeft(4, '0') + "*." + cr.Id;
            String[] fileList = Dir.GetFiles(PosConfiguration.ArchivePath, format);


            foreach (String fileName in fileList)
            {
                int receiptId = 0;
                FileHelper suspendedFile = new FileHelper(fileName);
                DocumentFileHelper receipt = new DocumentFileHelper(suspendedFile);
                if (Parser.TryInt(suspendedFile.Name.Substring(7, 4), out receiptId)
                    && suspendedFile.Name.Length > 14)
                {
                    receipt.Id = receiptId;
                    receipt.ResumedFromDocumentId = receiptId;
                    suspendedDocuments.Add(receipt);
                }
            }

            MenuList suspendedDocsMenu = new MenuList();
            foreach (DocumentFileHelper docFileHelper in suspendedDocuments)
            {
                suspendedDocsMenu.Add(docFileHelper);
            }
            return List.Instance(suspendedDocsMenu, RepeatDocument);
        
        }

        public static IState EnterCustomer()
        {
            if (cr.Document.Items.Count > 0 && cr.Document.Id > 0)
            {
                if (!(cr.Document is Receipt))
                {
                    AlertCashier.Instance(new Confirm(PosMessage.CUSTOMER_NOT_BE_CHANGED_IN_DOCUMENT));
                    return cr.State;
                }
                else
                {
                    if (cr.Document.Customer != null)
                    {
                        String msg = String.Format("{0}\n{1}", cr.Document.Customer.Name, PosMessage.CONFIRM_VOID_CURRENT_CUSTOMER);
                        cr.State = ConfirmCashier.Instance(new Confirm(msg, CustomerInfo.ConfirmVoidCustomer));
                    }
                    else
                    {
                        Confirm e = new Confirm(PosMessage.CONFIRM_TRANSFER_CUSTOMER_TO_RECEIPT,
                                        new StateInstance(Selling.ChangeConfirmed),
                                        new StateInstance(Start.Instance));

                        cr.State = ConfirmCashier.Instance(e);
                    }
                }
            }
            return cr.State;
        }

//todo remove below code 
        /// <summary>
        /// - Deposit Cash to Register.
        /// </summary>
        /// <param name="amount">- deposited cash amount</param>
        /// <returns>Start state</returns>
        public static IState DepositCashToRegister(decimal amount)
        {
            if (amount == 0)
            {
                throw new Exception(PosMessage.ZERO_DRAWER_IN_ERROR);
            }
            cr.Printer.Deposit(amount);
            cr.DataConnector.OnDeposit(amount);
            return Start.Instance();
        }      
        /// <summary>
        /// - withdraw cash from register
        /// </summary>
        /// <param name="amount">
        /// - withdraw amount
        /// </param>
        /// <returns>response from FPU</returns>
        public static IState WithdrawCashFromRegister(decimal amount)
        {
            if (amount == 0)
            {
                throw new Exception(PosMessage.ZERO_DRAWER_OUT_ERROR);
            }
            if(amount>cr.Printer.CashAmountInDrawer)
            {
                return States.AlertCashier.Instance(new Confirm(PosMessage.NOT_ENOUGH_MONEY_IN_REGISTER,
                new StateInstance (Continue)));        
            }
            cr.Printer.Withdraw(amount);
            cr.DataConnector.OnWithdrawal(amount);
            return Start.Instance();
        }
        /// <summary>
        /// - Withdraw cash from register
        /// </summary>
        /// <param name="amount">withdraw amount</param>
        /// <returns>response from FPU</returns>
        public static IState WithdrawCheckFromRegister(decimal amount)
        {
            if (amount == 0)
            {
                throw new Exception(PosMessage.ZERO_DRAWER_OUT_ERROR);
            }
            cr.Printer.Withdraw(amount, "");
            cr.DataConnector.OnWithdrawal(amount,"");
            return Start.Instance();
        }
        /// <summary>
        /// - Withdraw credit slips from register
        /// </summary>
        /// <param name="amount">withdraw amount</param>
        /// <returns>response from FPU</returns>
        public static IState WithdrawCreditSlipsFromRegister(decimal amount)
        {
            if (amount == 0)
            {
                throw new Exception(PosMessage.ZERO_DRAWER_OUT_ERROR);
            }
            Dictionary<int, ICredit> credits = cr.DataConnector.GetCredits();
            ICredit defaultCredit=null;
            foreach (int key in credits.Keys)
            {
                defaultCredit = credits[key];
                break;
            }
            CreditPaymentInfo creditWithdrawal = new CreditPaymentInfo(defaultCredit, amount);
            cr.Printer.Withdraw(creditWithdrawal.Amount, creditWithdrawal.Credit);
            cr.DataConnector.OnWithdrawal(creditWithdrawal.Amount, creditWithdrawal.Credit);
            return Start.Instance();
        }
//end todo - remove above code   
    }
}
