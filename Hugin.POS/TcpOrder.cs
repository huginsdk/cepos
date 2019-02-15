using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS
{
    internal class TcpOrder
    {
        enum OrderState
        {
            IDLE,
            ORDER_RECEIVED,
            WAITING_PROCESS,
            WAITING_STOP,
            STOPPED
        }

        enum PosState
        {
            POS_SUCCEED,
            POS_REJECTED,
            POS_BUSY,
            POS_NOT_AVAILABLE
        }

        private OrderState orderState = OrderState.IDLE;
        
        private const int ACK = 0x06;
        private const int NAK = 0x15; //decimal 21
        private const int EOT = 0x04;

        private const int STX = 0x02;
        private const int ETX = 0x03;
        private const int DIGITS_OF_LEN = 5;
        
        private TcpListener listener;
        private Socket client = null;

        public TcpOrder()
        {
            System.Threading.ThreadStart ts = new System.Threading.ThreadStart(ListenClient);
            System.Threading.Thread threadOrder = new System.Threading.Thread(ts);
            threadOrder.IsBackground = true;
            threadOrder.Start();
        }

        private void AccepClient()
        {
            while (true)
            {
                client = listener.Server.Accept();
                client.ReceiveTimeout = 500;
            }
        }
        private void ListenClient()
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
                System.Threading.Thread threadClient = new System.Threading.Thread(ts);
                threadClient.Start();
                
                while(client == null)
                {
                    System.Threading.Thread.Sleep(1000);
                }

                while (true)
                {
                    if (orderState == OrderState.WAITING_STOP)
                    {
                        break;
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

                        while (buff[0] != STX)
                        {
                            System.Threading.Thread.Sleep(10);
                            client.Receive(buff);
                        }
                        byte[] buffLen = new byte[DIGITS_OF_LEN];
                        client.Receive(buffLen);
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
                            throw new Exception("Data must be ended with ETX");
                        }
                        //String strClientMsg = System.Text.Encoding.UTF8.GetString(dataBuffer);
                        strClientMsg = strClientMsg.Substring(0, dataLen - 1);

                        orderState = OrderState.ORDER_RECEIVED;
                        
                        if (CashRegister.State is States.Start)
                        {
                            DisplayAdapter.Cashier.Show("BEKLEYEN SİPARİŞ\nVAR");
                        }

                        while(true)
                        {
                            buff[0] = STX;
                            client.Send(buff);
                            System.Threading.Thread.Sleep(100);
                            if(orderState == OrderState.WAITING_PROCESS)
                            {
                                break;
                            }
                        }

                        ProcessOrder(strClientMsg, client);
                    }
                    catch (System.Exception)
                    {
                        System.Threading.Thread.Sleep(100);
                    }

                    try
                    {
                        States.Start.Instance();
                    }
                    catch { }
                }
                orderState = OrderState.STOPPED;

            }
            catch
            {

            }
        }
        private void ProcessOrder(String orderLines, Socket client)
        {
            String [] lines = orderLines.Split(new String[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            byte [] buff = new byte[1];

            SalesDocument doc = null;
            
            foreach(String orderLine in lines)
            {
                try
                {
                    switch (orderLine.Substring(8, 2))
                    {
                        case "01"://FIS
                        case "02"://FAT,IAD,IRS
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
                            CashRegister.Printer.AdjustPrinter(doc);
                            CashRegister.Printer.PrintHeader(doc);
                            //doc.SalesPerson = cr.DataConnector.FindCashierById(line.Substring(23, 4));
                            break;
                        case "03"://TAR
                            //1,00011,03,TAR,25/03/2009  ,00:21:26                               
                            break;
                        case "04"://SAT
                        case "05":
                            FiscalItem si = new SalesItem();
                            
                            String pluNo  = orderLine.Substring(21, 6);
                            decimal amount = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            decimal quantity = Convert.ToDecimal(orderLine.Substring(15, 6)) / 1000m;
                            int depID = Convert.ToInt32(orderLine.Substring(28, 2));
                            String name = orderLine.Substring(41, 20).Trim().ToUpper();
                            decimal unitPrice =  Math.Round(amount / quantity, 2);

                            IProduct prd = Data.Connector.Instance().CreateProduct(name + "DYNAMIC", Department.Departments[depID], unitPrice);
                            
                            si.Quantity = quantity;
                            si.Product = prd;
                            si.TotalAmount = amount;
                            si.UnitPrice = unitPrice;

                            if (orderLine.Contains("SAT"))
                            {
                                CashRegister.Printer.Print(si);
                                doc.Items.Add(si);
                            }
                            else
                            {
                                VoidItem vi = new VoidItem(si);
                                CashRegister.Printer.Print(vi);
                                doc.Items.Add(vi);
                            }
                            break;
                        case "06"://IND,ART
                            AdjustmentType adjustmentType = AdjustmentType.Discount;
                            int percentage = 0;
                            bool isPercent = Parser.TryInt(orderLine.Substring(25, 2), out percentage);

                            IAdjustable target = doc.Items[doc.Items.Count - 1];
                            if (orderLine.Substring(15, 3) == "TOP")
                            {
                                target = doc;
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
                            break;
                        case "08":
                            doc.TotalAmount = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            //DocumentStatus status = (DocumentStatus)Convert.ToInt16(orderLine.Substring(26, 1));
                            CashRegister.Printer.PrintTotals(doc, true);
                            break;
                        case "09":
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
                            decimal crdPayment = Convert.ToDecimal(orderLine.Substring(30, 10)) / 100m;
                            int installments = int.Parse(orderLine.Substring(15, 2));
                            int crdId = int.Parse(orderLine.Substring(25, 2));
                            String crdName = orderLine.Substring(41, 20);

                            ICredit crd = Data.Connector.Instance().GetCredits()[crdId];

                            CashRegister.Printer.Pay(crdPayment, crd, installments);
                            doc.Payments.Add(new CreditPaymentInfo(crd, crdPayment));
                            break;
                        case "11":
                            CashRegister.Printer.PrintFooter(doc, true);
                            Data.Connector.Instance().OnDocumentClosed(doc);
                            break;
                        case "22":
                            String remark = orderLine.Substring(15, orderLine.Length-15).TrimEnd();
                            CashRegister.Printer.PrintRemark(remark);
                            break;
                        default: break;
                    }

                    buff[0] = ACK;
                    client.Send(buff);
                }
                catch
                {
                    if(doc != null)
                    {
                        CashRegister.Printer.Void();
                        Data.Connector.Instance().OnDocumentVoided(doc, (int)doc.Status);
                    }
                    buff[0] = NAK;
                    client.Send(buff);
                    break;
                }
            }

            if(buff[0] == ACK)
            {
                buff[0] = EOT;
                client.Send(buff);
            }
            orderState = OrderState.IDLE;
        }
        public void Play()
        {
            if(orderState == OrderState.ORDER_RECEIVED)
            {
                orderState = OrderState.WAITING_PROCESS;
                while (true)
                {
                    System.Threading.Thread.Sleep(100);
                    if (orderState == OrderState.IDLE)
                    {
                        break;
                    }
                }
            }
        }
        internal void StopOrder()
        {
            orderState = OrderState.WAITING_STOP;
            while (true)
            {
                if (orderState == OrderState.STOPPED)
                {
                    break;
                }
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
