using System;
using System.IO;
using Hugin.POS.Common;
using Hugin.POS.Data;
using System.Collections.Generic;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS
{
    class DocumentFileHelper : IMenuItem
    {
        private static List<DocumentFileHelper> orderDocuments = null;

        private static string succesTableFullName = String.Empty;

        int id;
        FileHelper fileInfo;

        SalesDocument doc;
        SalesItem tempSalesItem;
        ICashier salesPerson;
        int resumedFromDocumentId;
        DocumentStatus status;
        DateTime creationTime = DateTime.MinValue;

        public DocumentFileHelper(FileHelper documentOnDisk)
        {
            fileInfo = documentOnDisk;
            creationTime = documentOnDisk.CreationTime;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public ICashier Cashier
        {
            get { return salesPerson; }
        }
        public int ResumedFromDocumentId
        {
            get { return resumedFromDocumentId; }
            set { resumedFromDocumentId = value; }
        }

        public DocumentStatus Status
        {
            get { return status; }
        }
        public DateTime CreationTime
        {
            get { return creationTime; }
        }

        public FileHelper FileInfo
        {
            get { return fileInfo; }
        }
        #region IMenuItem Members

        public void Show()
        {
            Show(Target.Cashier);
        }

        public void Show(Target t)
        {
            LoadDocument();
            String type = PosMessage.ORDERED_DOCUMENT;
            if (fileInfo.Name.StartsWith("BEK"))
                type = PosMessage.PARKED_DOCUMENT;

            String format = String.Format("{0}\n{1} NO:\t{2:D4}", type, doc.Name, doc.Id);
            if (t == Target.Cashier)
                DisplayAdapter.Cashier.Show(format);
            else if (t == Target.Customer)
                DisplayAdapter.Customer.Show(format);
            else
                DisplayAdapter.Both.Show(format);
        }

        #endregion

        //TODO Invalid file exception
        public SalesDocument LoadDocument()
        {
            int docId = 0;
            decimal checkTotals = 0;
            if (doc != null) return doc;
            string context = fileInfo.ReadAllText();
            string[] lines = Str.Split(context, "\r\n", true);
            foreach (String line in lines)
            {
                try
                {
                    switch (line.Substring(8, 2))
                    {
                        case "01"://FIS
                        case "02"://FAT,IAD,IRS
                            switch (line.Substring(11, 3))
                            {
                                case PosMessage.HR_CODE_INVOICE:
                                    doc = new Invoice(); 
                                    break;
                                case PosMessage.HR_CODE_RETURN:
                                    doc = new ReturnDocument(); 
                                    break;
                                case PosMessage.HR_CODE_WAYBILL:
                                    doc = new Waybill(); 
                                    break;
                                default:
                                    doc = new Receipt(); 
                                    break;
                            }
                            if (doc is Invoice)
                            {
                                string serial = line.Substring(28).Trim();
                                doc.SlipSerialNo = serial.Substring(0, 2);
                                doc.SlipOrderNo = serial.Substring(2);
                            }
                            else
                                docId = int.Parse(line.Substring(28, 6));
                            doc.SalesPerson = cr.DataConnector.FindCashierById(line.Substring(23, 4));
                            break;
                        case "03"://TAR
                            //1,00011,03,TAR,25/03/2009  ,00:21:26   
                            DateTime date = DateTime.Now;
                            Parser.TryDate(line.Substring(15, 10).Replace('/', '.') + line.Substring(27, 9), out date);
                            break;
                        case "04"://SAT
                            FiscalItem si = new SalesItem();
                            si.Quantity = Convert.ToDecimal(line.Substring(15, 6)) / 1000;
                            si.Product = cr.DataConnector.FindProductByLabel(line.Substring(21, 6));
                            si.TotalAmount = Convert.ToDecimal(line.Substring(30, 10)) / 100m;
                            checkTotals += si.TotalAmount;
                            si.UnitPrice = Math.Round(si.TotalAmount / si.Quantity, 2);

                            if (si.Quantity == 99)
                            {
                                if (tempSalesItem == null)
                                {
                                    tempSalesItem = new SalesItem();
                                    tempSalesItem.Product = si.Product;
                                }

                                if(tempSalesItem.Quantity >= 99)
                                {
                                    if(tempSalesItem.Product == si.Product)
                                    {
                                        tempSalesItem.Quantity += 99;
                                    }
                                    else
                                    {
                                        doc.AddItem(tempSalesItem, false);
                                        tempSalesItem = new SalesItem();
                                        tempSalesItem = (SalesItem)si;
                                    }
                                }
                                else
                                {
                                    tempSalesItem.Quantity += 99;
                                    tempSalesItem.Quantity -= 1; // constructor's default quantity.
                                }
                            }
                            else
                            {
                                if (tempSalesItem == null) tempSalesItem = (SalesItem)si;

                                if (tempSalesItem.Quantity >= 99)
                                {
                                    if (tempSalesItem.Product == si.Product)
                                    {
                                        tempSalesItem.Quantity += si.Quantity;
                                        doc.AddItem(tempSalesItem, false);
                                        tempSalesItem = null;
                                    }
                                    else
                                    {
                                        doc.AddItem(tempSalesItem, false);
                                        tempSalesItem = null;
                                        doc.AddItem(si, false);
                                    }
                                }
                                else
                                {
                                    doc.AddItem(si, false);
                                    //tempSalesItem = null;
                                }
                            }
                            break;
                        case "05"://IPT
                            if (tempSalesItem != null && tempSalesItem .Quantity >= 99)
                            {
                                doc.AddItem(tempSalesItem, false);
                            }
                            
                            FiscalItem item = new SalesItem();
                            item.Quantity = Convert.ToDecimal(line.Substring(15, 6)) / 1000;
                            item.Product = cr.DataConnector.FindProductByLabel(line.Substring(21, 6));
                            item.TotalAmount = Convert.ToDecimal(line.Substring(30, 10)) / 100m;
                            checkTotals -= item.TotalAmount;
                            FiscalItem vi = new VoidItem(item);
                            vi.UnitPrice = Math.Round(vi.TotalAmount / vi.Quantity, 2);
                            doc.AddItem(vi, false);

                            decimal vq = Math.Abs(vi.Quantity);
                            decimal va = Math.Abs(vi.TotalAmount);
                            for (int i = 0; i < doc.Items.Count; i++)
                            {
                                if (!(doc.Items[i] is SalesItem)) continue;

                                if ((doc.Items[i].Product.Id == vi.Product.Id) &&
                                     (doc.Items[i].UnitPrice == vi.UnitPrice))
                                {
                                    if (vq > doc.Items[i].Quantity)
                                    {
                                        ((SalesItem)doc.Items[i]).VoidQuantity = doc.Items[i].Quantity;
                                        ((SalesItem)doc.Items[i]).VoidAmount = doc.Items[i].TotalAmount;
                                        vq -= doc.Items[i].Quantity;
                                        va -= doc.Items[i].TotalAmount;
                                    }
                                    else
                                    {
                                        if (((SalesItem)doc.Items[i]).VoidQuantity + vq >
                                                        (((SalesItem)doc.Items[i]).Quantity - ((SalesItem)doc.Items[i]).VoidQuantity))
                                            continue;
                                        ((SalesItem)doc.Items[i]).VoidQuantity += vq;
                                        ((SalesItem)doc.Items[i]).VoidAmount += va;
                                        break;
                                    }
                                }
                            }
                            break;
                        case "06"://IND,ART
                            AdjustmentType adjustmentType = AdjustmentType.Discount;
                            int percentage = 0;
                            bool isPercent = Parser.TryInt(line.Substring(25, 2), out percentage);

                            if(tempSalesItem.Quantity >= 99)
                            {
                                doc.AddItem(tempSalesItem, false);
                                tempSalesItem = null;
                            }
                            IAdjustable target = doc.LastItem;
                            if (line.Substring(15, 3) == "TOP")
                                target = doc;

                            if (isPercent)
                                adjustmentType = AdjustmentType.PercentDiscount;

                            if (line.Substring(11, 3) == "ART")
                            {
                                adjustmentType = isPercent ? AdjustmentType.PercentFee : AdjustmentType.Fee;
                            }

                            decimal amount = Convert.ToDecimal(line.Substring(30, 10)) / 100m;

                            if (adjustmentType == AdjustmentType.PercentFee || adjustmentType == AdjustmentType.Fee)
                                checkTotals += amount;
                            else
                                checkTotals -= amount;
                            if (isPercent) amount = (decimal)percentage;
                            Adjustment adj = new Adjustment(target, adjustmentType, amount);
                            if (Math.Abs(adj.NetAmount) != Convert.ToDecimal(line.Substring(30, 10)) / 100m)
                            {
                                bool minus= adj.NetAmount < 0 ? true : false;
                                adj.NetAmount = Convert.ToDecimal(line.Substring(30, 10)) / 100m;
                                if (minus) adj.NetAmount *= -1;
                            }
                            adj.AuthorizingCashierId = line.Substring(19, 4);

                            //Applying subtotal adjustments to retrieved documents
                            //could have side-effects. Only do item adjustments
                            // if (target is FiscalItem) 
                            target.Adjust(adj);
                            break;
                        case "08":
                            if (tempSalesItem != null && tempSalesItem.Quantity >= 99)
                            {
                                doc.AddItem(tempSalesItem, false);
                            }
                            doc.TotalAmount = Convert.ToDecimal(line.Substring(30, 10)) / 100m;
                            int statusCode = 0;
                            if (Parser.TryInt(line.Substring(26, 1), out statusCode))
                                status = (DocumentStatus)statusCode;

                            break;
                        case "09":
                            amount = Convert.ToDecimal(line.Substring(30, 10)) / 100m;
                            switch (line.Substring(11, 3))
                            {
                                case "NAK":
                                    doc.Payments.Add(new CashPaymentInfo(amount));
                                    break;
                                case "CEK":
                                    doc.Payments.Add(new CheckPaymentInfo(amount, line.Substring(15, 12)));
                                    break;
                                case "DVZ":
                                    int currId = 0;
                                    if (Parser.TryInt(line.Substring(15, 1), out currId))
                                    {
                                        ICurrency c = cr.DataConnector.GetCurrencies()[(int)(currId + "")[0]];
                                        doc.Payments.Add(new CurrencyPaymentInfo(c, amount));
                                    }
                                    break;
                            }
                            break;
                        case "10"://KRD
                            amount = Convert.ToDecimal(line.Substring(30, 10)) / 100m;
                            int creditId = 0;
                            int installment = 0;
                            if (Parser.TryInt(line.Substring(25, 2), out creditId))
                            {
                                ICredit c = cr.DataConnector.GetCredits()[creditId];
                                if (c != null)
                                {
                                    Parser.TryInt(line.Substring(15, 2), out installment);
                                    doc.Payments.Add(new CreditPaymentInfo(c, amount, installment));
                                }
                            }
                            break;
                        case "11"://SON
                            decimal paidTotal = 0.00m;
                            if (doc.Payments.Count > 0 && doc is Receipt)
                            {
                                foreach (PaymentInfo pi in doc.Payments)
                                    paidTotal += pi.Amount;

                                //check if there are invalid values on file
                                if (checkTotals != doc.TotalAmount)
                                    throw new Exception();

                                if (paidTotal < doc.TotalAmount)
                                    throw new Exception();

                                if (paidTotal >= doc.TotalAmount)
                                    status = DocumentStatus.Closed;
                            }
                            break;

                        case "29"://NOT
                            doc.FootNote.Add(line.Substring(15, 12) + line.Substring(28, 12));
                            break;

                        case "36"://SNO
                            string serialNo = line.Substring(15, 12) + line.Substring(28);
                            doc.LastItem.SerialNo = serialNo;
                            break;
                        case "37"://PNO
                            string batchNo = line.Substring(15, 12) + line.Substring(28);
                            doc.LastItem.BatchNumber = batchNo;
                            break;

                        case "39": //SKT
                            date = DateTime.Now;
                            Parser.TryDate(line.Substring(15, 10).Replace('/', '.') + line.Substring(27, 9), out date);
                            doc.LastItem.ExpiryDate = date;
                            break;
                        case "24": // BID
                            string zNo = line.Substring(28,4);
                            doc.DocumentFileZNo = int.Parse(zNo);
                            break;
                        default: break;
                    }
                }
                catch (Exception err)
                {
                    cr.Log.Warning("siparis dosyasý hata : \n\t satýr: " + line);
                    VoidOrder();
                    throw err;
                }
            }
            if (id == 0)
                id = docId;
            doc.Id = id;
            doc.SalesPerson = salesPerson;
            doc.ResumedFromDocumentId = resumedFromDocumentId;
            this.creationTime = fileInfo.CreationTime;
            return doc;
        }

        private void VoidTable(string table)
        {
            if (fileInfo.Name.IndexOf("MASA") > -1)
            {
                String nameToMove = String.Format("Ipt{0:D3}{1}{2:MMddyyyy}.dat", fileInfo.Name.Substring(6, 3), Id, DateTime.Now);
                if (!Directory.Exists(PosConfiguration.ServerTablePath + "Failure")) Directory.CreateDirectory(PosConfiguration.ServerTablePath + "Failure");
                File.Copy(table, PosConfiguration.ServerTablePath + "Failure/" + nameToMove, true);
                File.Delete(table);
                nameToMove = String.Format("{0}0{1}", fileInfo.Name.Substring(0, 5), fileInfo.Name.Substring(6));
                fileInfo.Rename(PosConfiguration.ServerTablePath + nameToMove);
                File.WriteAllText(PosConfiguration.ServerTablePath + nameToMove, String.Empty);
            }
        }

        private void VoidOrder()
        {
            if (fileInfo.Name.IndexOf(".dat") > -1)
            {
                string name = fileInfo.FullName.Substring(0, fileInfo.FullName.LastIndexOf('.'));
                String nameToMove = String.Format("{0}{1:_MMddyyyy_HHmmss}.ipt", name, DateTime.Now);
                fileInfo.Rename(nameToMove);
            }
            else fileInfo.Delete();
        }

        public void Remove(SalesDocument sDoc)
        {
            if (sDoc.Id == Id)
            {
                try
                {
                    if (sDoc.Status != DocumentStatus.Voided || sDoc.Status != DocumentStatus.Cancelled)
                    {
                        
                        if (fileInfo.Name.IndexOf("MASA") > -1 && sDoc.Status == DocumentStatus.Active)
                        {
                            String nameToMove = String.Format("Sip{0:D3}{1}{2:MMddyyyy}.dat", fileInfo.Name.Substring(6,3), sDoc.Id, DateTime.Now);
                            if (!Directory.Exists(PosConfiguration.ServerTablePath + "Success")) Directory.CreateDirectory(PosConfiguration.ServerTablePath + "Success");
                            succesTableFullName = PosConfiguration.ServerTablePath + "Success/" + nameToMove;
                            File.Copy(fileInfo.FullName, succesTableFullName, true);
                            nameToMove = String.Format("{0}0{1}", fileInfo.Name.Substring(0,5), fileInfo.Name.Substring(6));
                            fileInfo.Rename(PosConfiguration.ServerTablePath + nameToMove);
                            File.WriteAllText(PosConfiguration.ServerTablePath + nameToMove, String.Empty);
                            
                        }
                        else if (fileInfo.Name.IndexOf("MASA") > -1 && sDoc.Status == DocumentStatus.Voided)
                        {
                            VoidTable(succesTableFullName);
                        }
                        else if (fileInfo.Name.IndexOf(".dat") > -1)
                        {
                            string name = fileInfo.FullName.Substring(0, fileInfo.FullName.LastIndexOf('.'));
                            String nameToMove = String.Format("{0}{1:MMddyyyyHHmmss}.{2}", name, DateTime.Now, cr.Id.PadLeft(3, '0'));
                            fileInfo.Rename(nameToMove);
                        }
                        else fileInfo.Delete();
                    }
                    else //there should be an error at document
                    {
                        VoidOrder();
                    }
                }
                catch { }

            }
        }

        internal static DocumentFileHelper[] GetOpenOrders()
        {
            return GetOpenOrders("", true);
        }
        internal static DocumentFileHelper[] GetOpenOrders(String cashierId, bool checkOrder)
        {
            if (orderDocuments == null)
                orderDocuments = new List<DocumentFileHelper>();

            if (cr.DataConnector.CurrentSettings.GetProgramOption(Setting.FastPaymentControl) == PosConfiguration.ON &&
                                !checkOrder)
                return orderDocuments.ToArray();

            orderDocuments = new List<DocumentFileHelper>();
            if (Data.Connector.FxClient.DirectoryExists(PosConfiguration.ServerOrderPath, 4500))
            {
                FileHelper[] orderFiles = null;
                orderFiles = Data.Connector.FxClient.GetFiles(PosConfiguration.ServerOrderPath, "sip*" + cashierId + ".dat", 10000);

                foreach (FileHelper orderFile in orderFiles)
                {
                    //if (cr.Printer.IsFiscal && orderFile.CreationTime < cr.LastZReportDate) continue;
                    int orderId = 0;
                    DocumentFileHelper orderDocument = new DocumentFileHelper(orderFile);
                    if (Parser.TryInt(orderFile.Name.Substring(3, 4), out orderId) && orderFile.Name.Length > 14)
                    {
                        orderDocument.Id = orderId;
                        orderDocument.salesPerson = CashRegister.DataConnector.FindCashierById(orderFile.Name.Substring(7, 4));
                        if (orderDocument.salesPerson != null)
                            orderDocuments.Add(orderDocument);
                    }
                }
            }
            
            return orderDocuments.ToArray();
        }

        internal static DocumentFileHelper[] GetOpenTables()
        {
            List<DocumentFileHelper> tableDocuments = new List<DocumentFileHelper>();

            if (Data.Connector.FxClient.DirectoryExists(PosConfiguration.ServerTablePath, 4500))
            {
                FileHelper[] tableFiles = null;
                tableFiles = Data.Connector.FxClient.GetFiles(PosConfiguration.ServerTablePath, "Masa1*" + ".dat", 10000);


                foreach (FileHelper tableFile in tableFiles)
                {
                    int tableId = 0;
                    DocumentFileHelper tableDocument = new DocumentFileHelper(tableFile);
                    if (Parser.TryInt(tableFile.Name.Substring(6, 3), out tableId))
                    {
                        tableDocument.Id = tableId;
                        //tableDocument.salesPerson = CashRegister.DataConnector.FindCashierById(tableFile.Name.Substring(6, 3));
                        //if (tableDocument.salesPerson != null)
                        tableDocuments.Add(tableDocument);
                    }
                }
            }

            return tableDocuments.ToArray();
        }


        internal static DocumentFileHelper GetUnsavedDocument()
        {
            FileHelper unsavedFile = new FileHelper(PosConfiguration.ArchivePath + "HRBELGE." + cr.Id);
            return new DocumentFileHelper(unsavedFile);
        }
    }
}
