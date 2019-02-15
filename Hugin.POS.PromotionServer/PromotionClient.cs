using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.PromotionServer
{
    public class PromotionClient : IPosClient
    {
        int connectionTimeout=2000;
        public PromotionClient()
        {
            string timeout=PosConfiguration.Get("PromotionClientTimeout");
            
            if (timeout==null || !Parser.TryInt(timeout.Trim(), out connectionTimeout))
                connectionTimeout = 2000;

            PromotionServer.Settings.Load();
        }

        #region IPosClient Members

        public string[] DocumentRequest(string[] requestItems, bool isFirstPayment)
        {
            String[] response = new String[] { };
            Exception promoEx = null;
            System.Threading.Thread t = new System.Threading.Thread(delegate()
            {
                try
                {
                    PromotionServer.Promotion promotion = new PromotionServer.Promotion(requestItems, isFirstPayment);
                    response = promotion.CreatePromotionList();
                }
                catch (Exception ex)
                {
                    promoEx = ex;
                }
            }
            );
            t.Start();
            t.Join(this.ConnectionTimeout);
            if (promoEx != null)
                throw promoEx;
            return response;
        }
        //ItemRequest
        public string[] ItemRequest(string[] requestItems)
        {
            String[] response = new String[] { };
            Exception promoEx = null;
            System.Threading.Thread t = new System.Threading.Thread(delegate()
            {
                try
                {
                    PromotionServer.Promotion promotion = new PromotionServer.Promotion(requestItems, false);
                    response = promotion.CreateProductPromotion();
                }
                catch (Exception ex)
                {
                    promoEx = ex;
                }
            }
            );
            t.Start();
            t.Join(this.ConnectionTimeout);
            if (promoEx != null)
                throw promoEx;

            return response;
        }

        public int ConnectionTimeout
        {
            get { return connectionTimeout; }
        }
        
        public bool LogOn()
        {
            return true;
        }

        public void LogOff()
        {
            //not implemented

        }

        public void Close()
        {
            FileWatcher.Stop();
        }

        public void Messages(int messageCode, string[] messageItems)
        {
            switch (messageCode)
            {
                case 0:     // 0: Document Starts
                    break;
                case 1:     // 1: Document Closed
                    break;
                case 2:     // 2: Document Voided
                    break;
                case 3:     // 3: Document Suspended
                    break;
                case 4:     // 4: Z report Printed
                    break;
            }
        }
        
        public string SearchCustomer(string customerCode)
        {
            return String.Empty;
        }

        #endregion
    }
}
