using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Hugin.POS.Common;
using Hugin.POS.Data;
using System.Xml;
namespace Hugin.POS
{
    public enum PromotionType
    {
        Item,
        Document
    }

    public class PromotionDocument
    {
        SalesDocument virtualDoc = null;
        List<Adjustment> itemAdjustments = new List<Adjustment>();
        List<String> logLines = new List<String>();
        bool isFirstPayment = false;
  
        public PromotionDocument(SalesDocument doc,PaymentInfo paymentInfo,PromotionType promoType) 
        {
            if (doc == null) return;
    
            virtualDoc = (SalesDocument)doc.Clone();
            virtualDoc.Customer = doc.Customer;
            virtualDoc.Id = doc.Id;

            if (paymentInfo != null)
                virtualDoc.Payments.Add(paymentInfo);


            if ((doc is ReturnDocument) &&
                (CashRegister.DataConnector.CurrentSettings.GetProgramOption(Setting.ApplyPromotionReturnDocument) == PosConfiguration.OFF))
            {
                return;
            }

            if (doc.CanEmpty)
            {
                return;
            }

            if ((paymentInfo is CreditPaymentInfo) &&
                ((CreditPaymentInfo)paymentInfo).Credit.IsTicket)
            {
                return;
            }

            //if (Math.Abs(doc.PromotedTotal) > 0)
            //{
            //    return;
            //}

            if (doc.Adjustments != null && doc.Adjustments.Length > 0)
            {
                //if (doc.Adjustments[doc.Adjustments.Length - 1].AuthorizingCashierId != CashRegister.PROMOTION_CASHIER_ID)
                {
                    virtualDoc.TotalAmount = doc.BalanceDue;
                    virtualDoc.BalanceDue = doc.BalanceDue;
                }
                //else
                //{
                //    return;
                //}
            }

            Dictionary<int, int> matchIndex = new Dictionary<int, int>();
            int i = 0, j = 0;

            while (j < virtualDoc.Items.Count)
            {
                if (matchIndex.ContainsKey(j))
                    matchIndex[j] = i;
                else
                    matchIndex.Add(j, i);

                if (virtualDoc.Items[j].Quantity <= virtualDoc.Items[j].VoidQuantity || virtualDoc.Items[j].TotalAmount <= 0)
                {
                    virtualDoc.Items.RemoveAt(j);
                }
                else
                {
                    virtualDoc.Items[j].Quantity -= virtualDoc.Items[j].VoidQuantity;
                    j++;
                }
                i++;
            }

            String lines = CashRegister.DataConnector.FormatLines(virtualDoc);
            lines = lines + "\r\n";

            String[] requestValue = Str.Split(lines, "\r\n");
            String[] responseValue = null;
            List<String> salesIndex = new List<String>();
            int temp = 0;
            i = 0;

            foreach (String requestLine in requestValue)
            {
                if (requestLine.Length < 10) continue;
                if (Parser.TryInt(requestLine.Substring(8, 2), out temp) && (temp == 4 || temp == 5))
                   salesIndex.Add(requestLine.Substring(3, 4));
            }

            if (doc.Status != DocumentStatus.Paying)
                isFirstPayment = true;
            else
                isFirstPayment = false;

            try
            {
                if (CashRegister.PromoClient == null || !CashRegister.PromoClient.LogOn())
                    return;
                //Get Promotion list.
                if (promoType == PromotionType.Document)
                    responseValue = CashRegister.PromoClient.DocumentRequest(requestValue, isFirstPayment);
                else
                    responseValue = CashRegister.PromoClient.ItemRequest(requestValue);
            }
            catch (PromotionException pe)
            {
                String[] message = pe.Message.Split('\n');
                CashRegister.Log.Error(pe.Message);
                if (message[0].Length > 20 || (message.Length > 1 && message[1].Length > 20))
                    throw new Exception("PROMOSYON UYGULAMA\nHATASI OLUÞTU".ToUpper());

                throw new Exception(pe.Message.ToUpper());
            }
            catch
            {
                CashRegister.Log.Warning("Could not connect to promotion server at {0}", PosConfiguration.Get("PromoHostUri"));
                throw new Exception("PROMOSYON UYGULAMA\nHATASI OLUÞTU".ToUpper());
            }
            finally { isFirstPayment = false; }

            AdjustmentType promoAdjustType;
            bool hasPromoKey = false;

            if (Math.Abs(doc.PromotedTotal) > 0)
            {
                foreach (string line in responseValue)
                {
                    if (line == null || line == String.Empty || line.StartsWith("0,"))
                        continue;
                    String[] value = line.Split(',');
                    try
                    {
                        switch (value[3])
                        {
                            case PosMessage.PRM: // only point
                                int sign = doc is ReturnDocument ? -1 : 1;
                                virtualDoc.AddPoint(Int32.Parse(value[4]) * sign);
                                break;
                            case PosMessage.KOD:
                                String key = value[4].Trim();
                                hasPromoKey = true;
                                if (key != BackgroundWorker.GetAutoOrderKey())
                                {
                                    virtualDoc = (SalesDocument)doc.Clone();
                                    throw new InvalidSecurityKeyException();
                                }
                                break;
                        }
                    }
                    catch (InvalidSecurityKeyException iske)
                    {
                        throw iske;
                    }
                    catch (ProductPromotionLimitExeedException pplee)
                    {
                        throw pplee;
                    }
                    catch (InvalidPaymentException ipe)
                    {
                        throw ipe;
                    }
                    catch (Exception ex)
                    {
                        CashRegister.Log.Error(String.Format("Promosyon islemi hatali!:{0}", ex));
                        continue;
                    }
                }
            }
            else
            {
                foreach (string line in responseValue)
                {
                    if (line == null || line == String.Empty || line.StartsWith("0,"))
                        continue;
                    String[] value = line.Split(',');
                    try
                    {
                        switch (value[3])
                        {
                            case PosMessage.IND:
                                promoAdjustType = AdjustmentType.Discount;
                                goto Adjust;
                            case PosMessage.ART:
                                promoAdjustType = AdjustmentType.Fee;
                                goto Adjust;
                            Adjust:
                                Decimal adjustmentInput = 0;

                                if (Parser.TryDecimal(value[4].Substring(10), out adjustmentInput) &&
                                    adjustmentInput > 0)
                                    promoAdjustType = (promoAdjustType == AdjustmentType.Discount) ?
                                                                                        AdjustmentType.PercentDiscount :
                                                                                        AdjustmentType.PercentFee;
                                else if (!(Parser.TryDecimal(value[5], out adjustmentInput) &&
                                    adjustmentInput > 0))
                                    continue;
                                else adjustmentInput = Math.Round(adjustmentInput / 100m, 2);
                                if (value[4].Substring(0, 3) == PosMessage.SAT)
                                {
                                    if (virtualDoc.RepeatedDocument) continue;
                                    String salesId = value[4].Substring(4, 4);
                                    i = salesIndex.IndexOf(salesId);
                                    if (i < 0) continue;

                                    SalesItem tempSales = new SalesItem();
                                    SalesItem currentItem = (SalesItem)virtualDoc.Items[i];
                                    if (currentItem.VoidAmount > 0)
                                    {
                                        //to calculate adjustment amount as remain product quantity
                                        tempSales.Product = currentItem.Product;
                                        tempSales.Quantity = currentItem.Quantity;
                                    }
                                    else
                                        tempSales = currentItem;

                                    Adjustment promoAdjustment = new Adjustment(tempSales,
                                                                             promoAdjustType,
                                                                             adjustmentInput,
                                                                             promoType == PromotionType.Document ? CashRegister.PROMOTION_CASHIER_ID :
                                                                                                                  CashRegister.PROMOTION_ITEM_CASHIER_ID);
                                    promoAdjustment.NetAmount = (Decimal.Parse(value[5]) / 100) * (promoAdjustment.NetAmount > 0 ? 1 : -1);
                                    if (!virtualDoc.CanAdjust(promoAdjustment))
                                    {
                                        CashRegister.Log.Error("Hatali promosyon uygulamasi: {0}", line);
                                        continue;
                                    }

                                    int indxOnSalesDoc = matchIndex[i];
                                    if (doc.Items[indxOnSalesDoc].CanAdjust(promoAdjustment))
                                    {
                                        promoAdjustment.Target = currentItem;
                                        promoAdjustment.Target.Adjust(promoAdjustment);
                                        promoAdjustment.Target = doc.Items[indxOnSalesDoc];
                                        itemAdjustments.Add(promoAdjustment);
                                    }
                                }
                                else if (value[4].Substring(0, 3) == PosMessage.TOP)
                                {
                                    Adjustment promoAdjustment = new Adjustment(virtualDoc, promoAdjustType,
                                                                                 adjustmentInput,
                                                                                 CashRegister.PROMOTION_CASHIER_ID);
                                    if (!virtualDoc.CanAdjust(promoAdjustment))
                                    {
                                        CashRegister.Log.Error("Hatali promosyon uygulamasi: {0}", line);
                                        continue;
                                    }
                                    virtualDoc.Adjust(promoAdjustment);
                                    //this.Adjust(promoAdjustment);
                                }
                                break;
                            case PosMessage.PRM:
                                int sign = doc is ReturnDocument ? -1 : 1;
                                virtualDoc.AddPoint(Int32.Parse(value[4]) * sign);
                                break;
                            case PosMessage.NOT:
                                String fnValue = line.Substring(line.IndexOf(PosMessage.NOT) + PosMessage.NOT.Length + 1).Trim();
                                virtualDoc.FootNote.Add(fnValue);
                                break;
                            case PosMessage.MSG:
                                String msg = value[4].Trim();
                                if (msg.Length > 32)
                                    msg = msg.Substring(0, 32);
                                if (msg.Length > 0)
                                    virtualDoc.Remark.Add(msg);
                                break;
                            case "OZL":
                                String warn = value[4].Trim();
                                virtualDoc.FootNote.Add(warn);
                                if (warn.Length > 40) warn = warn.Substring(0, 40);
                                warn = warn.Insert(20, "\n");
                                DisplayAdapter.Cashier.Show(warn);
                                System.Threading.Thread.Sleep(500);
                                break;
                            case PosMessage.KOD:
                                String key = value[4].Trim();
                                hasPromoKey = true;
                                if (key != BackgroundWorker.GetAutoOrderKey())
                                {
                                    virtualDoc = (SalesDocument)doc.Clone();
                                    throw new InvalidSecurityKeyException();
                                }
                                break;
                            case PosMessage.LMT:
                                if (line.Substring(15, 3) == "SAT")
                                {
                                    virtualDoc = (SalesDocument)doc.Clone();
                                    throw new ProductPromotionLimitExeedException();
                                }
                                else if (line.Substring(15, 3) == "TOP")
                                {
                                    virtualDoc = (SalesDocument)doc.Clone();
                                    throw new InvalidPaymentException(PosMessage.PAYMENT_INVALID);
                                }
                                break;
                            case PosMessage.HAR:
                                logLines.Add(line.Substring(line.IndexOf(PosMessage.HAR) + PosMessage.HAR.Length + 1));
                                break;
                        }
                    }
                    catch (InvalidSecurityKeyException iske)
                    {
                        throw iske;
                    }
                    catch (ProductPromotionLimitExeedException pplee)
                    {
                        throw pplee;
                    }
                    catch (InvalidPaymentException ipe)
                    {
                        throw ipe;
                    }
                    catch (Exception ex)
                    {
                        CashRegister.Log.Error(String.Format("Promosyon islemi hatali!:{0}", ex));
                        continue;
                    }
                }
            }

            if (!hasPromoKey && responseValue.Length > 0)
            {
                virtualDoc = (SalesDocument)doc.Clone();
                throw new Exception("PROMOSYON LÝSANSI\nEKSÝK".ToUpper());
            }

        }

        public List<PointObject> Points
        {
            get { return virtualDoc.Points; }
        }

        public List<String> FootNote
        {
            get { return virtualDoc.FootNote; }
        }

        public List<String> Remark
        {
            get { return virtualDoc.Remark; }
        }

        public Decimal BalanceDue
        {
            get { return virtualDoc.BalanceDue; }
        }

        public Adjustment[] ItemAdjustments
        {
            get { return itemAdjustments.ToArray(); }           
        }

        public Adjustment[] SubtotalAdjustments
        {
            get { return virtualDoc.Adjustments; }
        }

        public bool HasAdjustment 
        {
            get { return !(itemAdjustments.Count == 0 && virtualDoc.Adjustments.Length == 0); }
        }

        public List<String> LogLines
        {
            get { return logLines; }
        }
    }
}
