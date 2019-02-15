using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Reflection;
using cr = Hugin.POS.CashRegister;
using System.Collections.Generic;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class BarcodeMenu:List
    {
        private static int numericZNo;
        private static int numericDocId;
        private static string registerNo;
        private static SalesDocument matchingDocument = null;
        private static IState state = new BarcodeMenu();

        public static new IState Instance()
        {
            return States.Start.Instance();
        }
        public static IState Instance(String barcode)
        {
            try
            {
                if (!cr.Document.IsEmpty)
                    return States.Start.Instance();

                if (barcode.Length == 8) // May order
                {
                    return States.CommandMenu.OpenOrderByBarcode(barcode);
                }
                else if (barcode.Length == 14)
                {
                    registerNo = barcode.Substring(0, 6);
                    if (registerNo != cr.FiscalRegisterNo.Substring(2, 2) + cr.FiscalRegisterNo.Substring(6))
                        throw new DocumentIdNotSetException();
                    numericZNo = Convert.ToInt32(barcode.Substring(6, 4));
                    numericDocId = Convert.ToInt32(barcode.Substring(10));
                }
                else
                {
                    registerNo = barcode.Substring(0, 2);
                    if (registerNo != cr.FiscalRegisterNo.Substring(8))
                        throw new DocumentIdNotSetException();
                    numericZNo = Convert.ToInt32(barcode.Substring(2, 4));
                    numericDocId = Convert.ToInt32(barcode.Substring(6, 4));
                }
            }
            catch (DocumentIdNotSetException)
            {
                return AlertCashier.Instance(new Confirm(PosMessage.RECEIPT_NOT_BELONG_TO_CASE));
            }
            if (cr.IsDesktopWindows)
            {
                return Instance(Start.Instance);
            }
            else
            {
                if (cr.Printer.IsFiscal)
                    return cr.State = States.ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_PRINT_RECEIPT_COPY, PrintReceiptCopy, Continue));
                else
                    return Continue();
            }
        }

        public static IState Instance(StateInstance ReturnCancel)
        {
            MenuList menuHeaders = CreateBarcodeMenu();
            if (menuHeaders.Count == 0)
                return AlertCashier.Instance(new Confirm(PosMessage.EJ_NOT_AVAILABLE));


            return cr.State = List.Instance(menuHeaders, new ProcessSelectedItem(SelectMenuAction), ReturnCancel); ;

        }
        public static MenuList CreateBarcodeMenu()
        {
            MenuList menuHeaders = new MenuList();
            int index = 1;
            if (!cr.HasEJ)
                return menuHeaders;
                    
            if (cr.Printer.IsFiscal)
                menuHeaders.Add(new MenuLabel(String.Format("BELGE BARKOD      {0,2}\n{1}", index++, PosMessage.PRINT_RECEIPT_COPY)));

            if (cr.IsDesktopWindows)
            {
                if (cr.IsAuthorisedFor(Authorizations.VoidDocument))
                {
                    menuHeaders.Add(new MenuLabel(String.Format("BELGE BARKOD      {0,2}\n{1}", index++, PosMessage.RETURN_RECEIPT)));
                    menuHeaders.Add(new MenuLabel(String.Format("BELGE BARKOD      {0,2}\n{1}", index++, PosMessage.RETURN_PRODUCT)));
                }
                menuHeaders.Add(new MenuLabel(String.Format("BELGE BARKOD      {0,2}\n{1}", index++, PosMessage.REPEAT_SALE)));
            }
            return menuHeaders;
        }
        public static IState Continue()
        {
            if (cr.IsDesktopWindows)
            {
                ie.MovePrevious();
                List.Instance(ie);
                return state;
            }
            else
            {
                return cr.State = States.Start.Instance();
            }
        }



        private static void SelectMenuAction(Object menu)
        {
            try
            {
                string message = ((MenuLabel)ie.Current).ToString();
                message = message.Substring(message.IndexOf('\n') + 1);
                switch (message)
                {
                    case PosMessage.RETURN_RECEIPT:
                        cr.State = States.ConfirmAuthorization.Instance(ReturnReceipt, ReturnCancel, Authorizations.VoidDocument);
                        break;
                    case PosMessage.RETURN_PRODUCT:
                        cr.State = States.ConfirmAuthorization.Instance(ReturnProduct, ReturnCancel, Authorizations.VoidDocument);
                        break;
                    case PosMessage.PRINT_RECEIPT_COPY:
                        cr.State = PrintReceiptCopy();
                        break;
                    case PosMessage.REPEAT_SALE:
                        cr.State = RepeatSale();
                        break;
                }
            }
            catch (Exception ex)
            {
                cr.State = AlertCashier.Instance(new Error(ex, ReturnCancel, ReturnCancel));
            }
        }

        private static IState RepeatSale()
        {
            return RepeatSelling();
        }

        private static IState PrintReceiptCopy()
        {
            DisplayAdapter.Cashier.Show(PosMessage.PLEASE_WAIT);
            cr.Printer.PrintEJDocument(Convert.ToInt32(numericZNo), Convert.ToInt32(numericDocId), true);
            return cr.State = Start.Instance();
        }

        private static IState FindDocument() 
        {
            matchingDocument = cr.Document.ReadMainLogFile(numericDocId, numericZNo);
            if (matchingDocument == null || matchingDocument.Items.Count == 0)
                return AlertCashier.Instance(new Confirm(PosMessage.DOCUMENT_ID_NOT_FOUND,
                                            new StateInstance(Continue)));

            return cr.State;
        }

        private static IState ReturnReceipt()
        {
            FindDocument();
            cr.ChangeDocumentType(new ReturnDocument(matchingDocument));
            return cr.State;
        }

        private static IState ReturnProduct()
        {
            FindDocument();
            cr.ChangeDocumentType(new ReturnDocument());
            return ListReturnedItems();
        }

        private static IState ListReturnedItems()
        {
            MenuList returnMenu = new MenuList();

            foreach (FiscalItem fiscalItem in matchingDocument.Items)
            {
                if (fiscalItem is SalesItem)
                {
                    SalesItem si = (SalesItem)fiscalItem.Clone();
                    if (si.Quantity <= si.VoidQuantity)
                        continue;

                    si.Quantity -= si.VoidQuantity;

                    /* quantity of void item is equal to remaining quantity of sale item */
                    if (si.TotalAmount > 0 && cr.Document.CanAddItem(si))
                        returnMenu.Add(si);
                }
            }

            cr.Document.Customer = matchingDocument.Customer;
            if (returnMenu.Count > 0)
                cr.State = ListFiscalItem.Instance(returnMenu, new ProcessSelectedItem<FiscalItem>(SellReturnedItem));
            else
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.NOT_SELLING));

            return cr.State;
        }

        public static IState RepeatSelling()
        {
            FindDocument();
            cr.ChangeDocumentType(new Receipt(matchingDocument));
            return cr.State = States.Selling.Instance();
        }

        public static void SellReturnedItem(FiscalItem fi)
        {
            cr.Execute(fi);
            fi.Quantity += ((SalesItem)fi.Clone()).VoidQuantity;
            matchingDocument.Items.Remove(fi);

            ListReturnedItems();
        }

    }
}
