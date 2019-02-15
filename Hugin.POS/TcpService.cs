using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS
{

    enum TCPMsgType
    {
        TCP_MSG_NONE,
        TCP_MSG_INVALID,
        TCP_MSG_SALE,
        TCP_MSG_REPORT,
        TCP_MSG_DAILY_REPORT,
        TCP_MSG_CASH_IN,
        TCP_MSG_CASH_OUT,
        TCP_MSG_LOG_IN,
        TCP_MSG_LOG_OUT,
        TCP_MSG_CUT_PAPER,
        TCP_MSG_FEED_PAPER,
        TCP_MSG_CUSTOM_REPORT,
        TCP_LAST_DOC_INFO,
        TCP_RAW_MSG,
        TCP_MSG_EXIT
    }

    internal class TcpService
    {
        enum PosState
        {
            POS_SUCCEED,
            POS_REJECTED,
            POS_BUSY,
            POS_NOT_AVAILABLE
        }

        private const int SOH = 0x01;          //Start of raw message
        private const int STX = 0x02;          //Start index of message
        private const int ETX = 0x03;          //End of message character
        private const int EOT = 0x04;          //Command has executed successfully
        private const int ACK = 0x06;          //Line has executed
        private const int BEL = 0x07;          //Pos has an message.
        private const int NAK = 0x15;          //Pos has an exception. Command has not been executed.
        private const int DLE = 0x20;          //Data format exception
        private const int SYN = 0x26;          //Pos is busy.

        private const int DIGITS_OF_LEN = 5;
        private const int RAW_MSG_LEN = 2;
        
        private TcpListener listener;
        private Socket client = null;
        System.Threading.Thread threadClient = null;

        public TcpService()
        {
            try
            {
                String tcpIp = PosConfiguration.Get("TCPIp");
                int port = int.Parse(PosConfiguration.Get("TCPPort"));

                String[] split = tcpIp.Split('.');

                if (split.Length != 4)
                    throw new Exception("IP not accepted");

                byte[] ip = new byte[4];

                for (int i = 0; i < split.Length; i++)
                {
                    ip[i] = Convert.ToByte(split[i].Trim());
                }
                System.Net.IPAddress addr = new System.Net.IPAddress(ip);

                listener = new TcpListener(addr, port);

                listener.Start();

                //wait for client
                System.Threading.ThreadStart ts = new System.Threading.ThreadStart(AccepClient);
                threadClient = new System.Threading.Thread(ts);
                threadClient.Start();
            }
            catch
            {

            }
        }

        private void AccepClient()
        {
            try
            {

                while (true)
                {
                    client = listener.Server.Accept();
                    client.ReceiveTimeout = 500;
                }
            }catch{}
        }
        
        internal TCPMsgType Play()
        {
            if(client == null)
            {
                return TCPMsgType.TCP_MSG_NONE;
            }

            try
            {
                /*
                 * Pos will sends
                 * - 'STX' if pos is busy
                 * - 'ETX' if data format not accepted
                 * - 'ACK' after printing each line
                 * - 'NAK' if line could not be printed
                 *         if line could not be printed, pos does not sends any more byte
                 * - 'EOT' if receipt printed successfully
                 */
                byte[] buff = new byte[1];
                client.Receive(buff);

                while (buff[0] != STX && buff[0] != SOH)
                {
                    System.Threading.Thread.Sleep(10);
                    client.Receive(buff);
                }
                int length = DIGITS_OF_LEN;
                if (buff[0] == SOH)
                    length = 2;
                byte[] buffLen = new byte[length];
                client.Receive(buffLen);


                if (buff[0] == SOH)
                {
                    int passTime = 0;

                    length = buffLen[0] * 256 + buffLen[1];

                    while (client.Available < length)
                    {
                        System.Threading.Thread.Sleep(10);
                        passTime += 10;
                        if (passTime >= client.ReceiveTimeout)
                        {
                            throw new TimeoutException();
                        }
                    }

                    byte[] rawBuffer = new byte[length];
                    client.Receive(rawBuffer);

                    return PocessRawMessage(rawBuffer, client);
                }


                String strLen = System.Text.Encoding.UTF8.GetString(buffLen);

                int dataLen = int.Parse(strLen);
                byte[] dataBuffer = new byte[dataLen];
                client.Receive(dataBuffer);

                String strClientMsg = System.Text.Encoding.UTF8.GetString(dataBuffer);
                int tryCount = 0;
                while (strClientMsg.Length < dataLen)
                {
                    tryCount++;
                    if (tryCount > 5) break;
                    System.Threading.Thread.Sleep(10);
                    dataBuffer = new byte[dataLen - strClientMsg.Length];
                    client.Receive(dataBuffer);
                    strClientMsg += System.Text.Encoding.UTF8.GetString(dataBuffer);

                }
                if (strClientMsg[dataLen - 1] != ETX)
                {
                    buff[0] = ETX;
                    client.Send(buff);
                    return TCPMsgType.TCP_MSG_INVALID;
                }
                //String strClientMsg = System.Text.Encoding.UTF8.GetString(dataBuffer);
                strClientMsg = strClientMsg.Substring(0, dataLen - 1);

                return ProcessOrder(strClientMsg, client);
            }
            catch (System.Exception ex)
            {
                /*
                 * todo: switch exception types
                 * ie.
                 * before catch (System.Exception ex)
                 * 
                 * catch (System.ExceptionT1 ex){....}
                 * catch (System.ExceptionT2 ex){....}
                 * .....
                 * */
                return TCPMsgType.TCP_MSG_INVALID;
            }

        }

        private TCPMsgType PocessRawMessage(byte[] rawBuffer, Socket client)
        {
            if (!(CashRegister.Printer is IServerPrinter))
            {
                throw new Exception("Incorrect message");
            }

            //first 2 bytes  len
            byte lrc = 0;
            for (int i = 0; i < (rawBuffer.Length - 1); i++)
            {
                lrc ^= rawBuffer[i];
            }
            if (rawBuffer[rawBuffer.Length - 1] != lrc)
            {
                // Throw CRCException 
                throw new Exception("LRC not match");
            }

            Array.Resize(ref rawBuffer, rawBuffer.Length - 1);

            byte[] response = ((IServerPrinter)CashRegister.Printer).SendRawMessage(rawBuffer);

            client.Send(response);
            
            return TCPMsgType.TCP_RAW_MSG;
        }
        
        private TCPMsgType ProcessOrder(String orderLines, Socket client)
        {
            String [] lines = orderLines.Split(new String[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            byte [] buff = new byte[1];

            SalesDocument doc = null;
            
            TCPMsgType retVal = TCPMsgType.TCP_MSG_SALE;
            List<string> reportLines = new List<string>();

            CashRegister.Log.Info(orderLines);
            
            foreach(String orderLine in lines)
            {
                try
                {
                    switch (orderLine.Substring(8, 2))
                    {
                        case "01"://FIS
                        case "02"://FAT,IAD,IRS
                            CheckCashier();
                            switch (orderLine.Substring(11, 3))
                            {
                                case PosMessage.HR_CODE_INVOICE:
                                    doc = new Invoice(); break;
                                case PosMessage.HR_CODE_RETURN:
                                    doc = new ReturnDocument(); break;
                                case PosMessage.HR_CODE_WAYBILL:
                                    doc = new Waybill(); break;
                                default:
                                    doc = new Receipt();
                                    break;
                            }

                            if (doc.DocumentTypeId >= 0)
                            {
                                doc.SlipSerialNo = orderLine.Substring(17, 10);
                                doc.TotalAmount = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            }
                            CashRegister.Printer.AdjustPrinter(doc);
                            CashRegister.Printer.PrintHeader(doc);
                            //doc.SalesPerson = cr.DataConnector.FindCashierById(line.Substring(23, 4));
                            break;
                        case "03"://TAR
                            //1,00011,03,TAR,25/03/2009  ,00:21:26                               
                            break;
                        case "04"://SAT
                        case "05":
                            CheckCashier();
                            if (!(doc is Receipt))
                            {
                                break;
                            }
                            FiscalItem si = new SalesItem();
                            
                            String pluNo  = orderLine.Substring(21, 6);
                            decimal amount = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            decimal quantity = Convert.ToDecimal(orderLine.Substring(15, 6)) / 1000m;
                            int depID = Convert.ToInt32(orderLine.Substring(28, 2));
                            String name = orderLine.Substring(41, 20).Trim().ToUpper();
                            decimal unitPrice =  Math.Round(amount / quantity, 2);
                            String unit=orderLine.Substring(62, 4).Trim().ToUpper();
                            IProduct prd = Data.Connector.Instance().CreateProduct(name + "DYNAMIC", Department.Departments[depID], unitPrice);
                            
                            si.Quantity = quantity;
                            si.Product = prd;
                            si.TotalAmount = amount;
                            si.UnitPrice = unitPrice;
                            si.Unit = unit;
                            if (orderLine.Contains("SAT"))
                            {
                                CashRegister.Printer.Print(si);
                                doc.Items.Add(si);
                                doc.TotalAmount += si.TotalAmount;
                                if(doc.ProductTotals.ContainsKey(si.Product))
                                {
                                    doc.ProductTotals[si.Product]+=si.TotalAmount;
                                }
                                else
                                {
                                    doc.ProductTotals.Add(si.Product, si.TotalAmount);
                                }
                            }
                            else
                            {
                                VoidItem vi = new VoidItem(si);
                                CashRegister.Printer.Print(vi);
                                doc.Items.Add(vi);
                                doc.TotalAmount -= vi.TotalAmount;
                                if (doc.ProductTotals.ContainsKey(vi.Product))
                                {
                                    doc.ProductTotals[vi.Product] -= vi.TotalAmount;
                                }
                            }
                            break;
                        case "06"://IND,ART
                            CheckCashier();
                            AdjustmentType adjustmentType = AdjustmentType.Discount;
                            int percentage = 0;
                            bool isPercent = Parser.TryInt(orderLine.Substring(25, 2), out percentage);

                            IAdjustable target = doc.Items[doc.Items.Count - 1];
                            if (orderLine.Substring(15, 3) == "TOP")
                            {
                                target = doc;
                                decimal total = CashRegister.Printer.PrinterSubTotal;
                                CashRegister.Printer.PrintSubTotal(doc, true);
                            }

                            if (isPercent)
                            {
                                adjustmentType = AdjustmentType.PercentDiscount;
                            }

                            if (orderLine.Substring(11, 3) == "ART")
                            {
                                adjustmentType = isPercent ? AdjustmentType.PercentFee : AdjustmentType.Fee;
                            }

                            decimal adjAmount = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            if (isPercent)
                            {
                                adjAmount = (decimal)percentage;
                            }                            
                            Adjustment adj = new Adjustment(target, adjustmentType, adjAmount);
                            CashRegister.Printer.Print(adj);

                            target.Adjust(adj);
                            
                            if(adj.Target is SalesItem)
                            {
                                SalesItem adjTarget = (SalesItem)adj.Target;
                                if (doc.ProductTotals.ContainsKey(adjTarget.Product))
                                {
                                    doc.ProductTotals[adjTarget.Product] += adj.NetAmount;
                                }
                                doc.TotalAmount += adj.NetAmount;
                            }
                            break;
                        case "08":
                            CheckCashier();
                            decimal totalAmount = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            //todo: compare total amaounts
                            //DocumentStatus status = (DocumentStatus)Convert.ToInt16(orderLine.Substring(26, 1));
                            decimal subTotal = CashRegister.Printer.PrinterSubTotal;
                            CashRegister.Printer.PrintTotals(doc, true);
                            break;
                        case "09":
                            CheckCashier();
                            decimal payment = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            switch (orderLine.Substring(11, 3))
                            {
                                case "NAK":
                                    CashRegister.Printer.Pay(payment);
                                    doc.Payments.Add(new CashPaymentInfo(payment));
                                    break;
                                case "CEK":
                                    String refNum = orderLine.Substring(15, 12).Trim();
                                    CashRegister.Printer.Pay(payment, refNum);
                                    doc.Payments.Add(new CheckPaymentInfo(payment, refNum));
                                    break;
                                case "DVZ":
                                    String exCode = orderLine.Substring(15, 1);
                                    decimal exAmnt = Convert.ToDecimal(orderLine.Substring(18, 9)) / 100m;
                                    String exName = orderLine.Substring(41, 20);
                                    int currId = int.Parse(orderLine.Substring(62, 2));

                                    ICurrency currency =  Data.Connector.Instance().GetCurrencies()[currId];

                                    CashRegister.Printer.Pay(payment, currency);
                                    doc.Payments.Add(new CurrencyPaymentInfo(currency, payment));
                                    break;
                            }
                            break;
                        case "10": 
                            CheckCashier();
                            decimal crdPayment = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            int installments = int.Parse(orderLine.Substring(15, 2));
                            int crdId = int.Parse(orderLine.Substring(25, 2));
                            String crdName = orderLine.Substring(41, 20);

                            ICredit crd = Data.Connector.Instance().GetCredits()[crdId];

                            CashRegister.Printer.Pay(crdPayment, crd, installments);
                            doc.Payments.Add(new CreditPaymentInfo(crd, crdPayment));
                            break;
                        case "11":
                            CheckCashier();
                            CashRegister.Printer.PrintFooter(doc);
                            Data.Connector.Instance().OnDocumentClosed(doc);
                            break;
                        case "22":
                            CheckCashier();
                            String remark = orderLine.Substring(15, orderLine.Length-15).TrimEnd();
                            CashRegister.Printer.PrintRemark(remark);
                            break;
                        case "12"://cash out
                            CheckCashier();
                            decimal cashOut = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            CashRegister.Printer.Withdraw(cashOut);
                            CashRegister.DataConnector.OnWithdrawal(cashOut);
                            retVal = TCPMsgType.TCP_MSG_CASH_OUT;
                            break;
                        case "13"://cash in
                            CheckCashier();
                            decimal cashIn = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            CashRegister.Printer.Deposit(cashIn);
                            CashRegister.DataConnector.OnDeposit(cashIn);
                            retVal = TCPMsgType.TCP_MSG_CASH_IN;
                            break;
                        case "14"://log in
                            if (orderLine.Substring(11, 3) != "LIN")
                            {
                                retVal = TCPMsgType.TCP_MSG_LOG_IN;
                                break;
                            }
                            String cashierID = orderLine.Substring(23, 4);
                            String cashierPass = orderLine.Substring(28, 8);
                            String cashierName = String.Format("KASIYER {0}",int.Parse(cashierID));//orderLine.Substring(41, 20).Trim().ToUpper();
                            ICashier ch;
                            ch = CashRegister.DataConnector.CreateCashier(cashierName + "FPU", cashierID);
                            ch.Password = cashierPass.Trim(new char[] { ' ', ',' });

                            /* ************ TODO: REMOVED ***************/
                            if (ch.Password == String.Empty)
                            {
                                switch (cashierID)
                                {
                                    case "0001":
                                        ch.Password = "1234";
                                        break;
                                    case "0006":
                                        ch.Password = "1357";
                                        break;
                                    case "0009":
                                        ch.Password = "14710";
                                        break;
                                }
                            }
                            /* ************ END TEMPRORARY CODE ***********/
                            
                            try
                            {
                                CashRegister.Printer.SignInCashier(int.Parse(cashierID), ch.Password);
                                CashRegister.CurrentCashier = ch;
                            }
                            catch (CashierAlreadyAssignedException cEx)
                            {
                                if (cEx.CashierId == ch.Id)
                                {
                                    CashRegister.CurrentCashier = ch;
                                }
                            }
                            catch
                            {

                            }
                            retVal = TCPMsgType.TCP_MSG_LOG_IN;
                            break;
                        case "15"://log out
                            //String chID = orderLine.Substring(26, 4);
                            //String chName = orderLine.Substring(41, 20).Trim().ToUpper();
                            //ICashier chOut;
                            //chOut = CashRegister.DataConnector.CreateCashier(chName + "DYNAMIC", chID);
                            try
                            {
                                if (CashRegister.CurrentCashier != null)
                                    CashRegister.Printer.SignOutCashier();
                                CashRegister.CurrentCashier = null;
                            }
                            catch
                            {

                            }
                            retVal = TCPMsgType.TCP_MSG_LOG_OUT;
                            break;
                        case "16"://Daily report
                            CheckCashier();
                            switch (orderLine.Substring(11, 3))
                            {
                                case "ZRP":
                                    CashRegister.Printer.PrintZReport();
                                    break;
                                case "XRP":
                                    CashRegister.Printer.PrintXReport(true);
                                    break;
                            }
                            retVal = TCPMsgType.TCP_MSG_DAILY_REPORT;
                            break;
                        case "81"://reports form
                            CheckCashier();
                            retVal = TCPMsgType.TCP_MSG_REPORT;
                            break;
                        case "82"://paper cut
                            CheckCashier();
                            CashRegister.Printer.CutPaper();
                            retVal = TCPMsgType.TCP_MSG_CUT_PAPER;
                            break;
                        case "83"://custom report
                            CheckCashier();
                            int nfDocType = 1;
                            switch (orderLine.Substring(11,3))
                            {
                                case "ORP"://stands for "OZEL RAPOR"
                                    reportLines.Clear();
                                    nfDocType = int.Parse(orderLine.Substring(15, 1));
                                    break;
                                case "RSA"://stands for "RAPOR SATIRI"
                                    String repLine = orderLine.Substring(15).ToUpper();
                                    reportLines.Add(repLine);
                                    break;
                                case "RSO"://stands for "RAPOR SONU"
                                    if (nfDocType==1)
                                    {
                                        CashRegister.Printer.PrintCustomReport(reportLines.ToArray());
                                    }
                                    else if(nfDocType ==2)
                                    {
                                        CashRegister.Printer.PrintCustomReceipt(reportLines.ToArray());
                                    }
                                    break;
                            }

                            retVal = TCPMsgType.TCP_MSG_CUSTOM_REPORT;
                            break;
                        case "84"://date time setting
                            //1,00011,03,TAR,25/03/2009  ,00:21:26 
                            int day = int.Parse(orderLine.Substring(15,2));
                            int mon = int.Parse(orderLine.Substring(18,2));
                            int year = int.Parse(orderLine.Substring(21,4));
                            
                            int hour = int.Parse(orderLine.Substring(28,2));
                            int min = int.Parse(orderLine.Substring(31,2));
                            int sec = 0;
                            try
                            {
                                sec = int.Parse(orderLine.Substring(34, 2));
                            }
                            catch { }

                            DateTime dtDate = new DateTime(year, mon, day, hour, min, sec);
                            CashRegister.Printer.Time = dtDate;
                            break;
                        case "85"://Payment order
                            CheckCashier();
                            switch (orderLine.Substring(11, 3))
                            {
                                case "ABA"://stands for "ADISYON BASLANGICI"
                                    ISalesDocument invoice = new Invoice();
                                    CashRegister.Printer.AdjustPrinter(invoice);
                                    CashRegister.Printer.StartSlip(invoice);
                                    break;
                                case "ASA"://stands for "ADİSYON SATIRI"
                                    String repLine = orderLine.Substring(15).ToUpper();
                                    CashRegister.Printer.PrintSlipLine(repLine);
                                    break;
                                case "ASO"://stands for "ADİSYON SONU"
                                    CashRegister.Printer.EndSlip(null);
                                    break;
                            }
                            break;
                        case "86"://Last document info
                            CheckCashier();
                            bool zReport = false;
                            switch (orderLine.Substring(11, 3))
                            {
                                case "SBB"://stands for "Son Belge Bilgisi"
                                    zReport = false;
                                    break;
                                case "SZB"://stands for "Son Z Bilgisi"
                                    zReport = true;
                                    break;
                            }
                            
                            PrintedDocumentInfo lastDocInfo = CashRegister.Printer.GetLastDocumentInfo(zReport);
                            SendMessage(String.Format("{0:D4}{1:D4}{2:D4}{3:D2}{4:yyyyMMddHHmm}",
                                                        lastDocInfo.ZNo,
                                                        lastDocInfo.EjNo,
                                                        lastDocInfo.DocId,
                                                        (int)lastDocInfo.Type,
                                                        lastDocInfo.DocDateTime));
                            break;
                        case "99":
                            try
                            {
                                listener.Stop();
                                threadClient.Abort();
                            }catch{}
                            retVal = TCPMsgType.TCP_MSG_EXIT;
                            break;
                        default: break;
                    }

                    buff[0] = ACK;
                    client.Send(buff);
                }
                catch(Exception ex)
                {
                    string errMsg = new Error(ex).Message;
                    if (ex is PosException && ((PosException)ex).ErrorCode >0)
                    {
                        errMsg = String.Format("{0:D4}", ((PosException)ex).ErrorCode);
                    }
                    if(doc != null)
                    {
                        try
                        {
                            CashRegister.Printer.Void();
                            Data.Connector.Instance().OnDocumentVoided(doc, 1);
                        }
                        catch
                        {
                        }
                    }


                    String data = String.Format("{0}{1}{2:D" + DIGITS_OF_LEN + "}{3}{4}",
                                                  (char)NAK,
                                                  (char)STX,
                                                  errMsg.Length + 1,
                                                  errMsg,
                                                  (char)ETX);

                    buff = System.Text.Encoding.UTF8.GetBytes(data);
                    client.Send(buff);
                    break;
                }
            }

            if(buff[0] == ACK)
            {
                buff[0] = EOT;
                client.Send(buff);
            }

            return retVal;
        }

        private void CheckCashier()
        {
            if (!(CashRegister.Printer is IServerPrinter))
            {
                if (CashRegister.CurrentCashier == null)
                {
                    throw new MissingCashierException();
                }
            }
        }
       
        internal void StopOrder()
        {
            try
            {
                if(threadClient!=null)
                {
                    threadClient.Abort();
                }
            }catch{}
        }

        internal void SendMessage(string message)
        {
            String data = String.Format("{0}{1}{2:D" + DIGITS_OF_LEN + "}{3}{4}",
                                          (char)BEL,
                                          (char)STX,
                                          (message.Length + 1),
                                          (message),
                                          (char)ETX);

            client.Send(System.Text.Encoding.UTF8.GetBytes(data));
        }
    }
}
