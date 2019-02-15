using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class ScaleCAS : IScale
    {
        private static IScale scaleCas = null;
        private SerialPort serialPort;
        private String request = "$";

        private const int MAX_TRY_COUNT = 3;
        private static int tryCount = 0;
        
        private ScaleCAS()
        {
            //Connect();
        }


        internal static IScale Instance()
        {
            if (scaleCas == null)            
                scaleCas = LoadModule();
            return scaleCas;
        }

        private static IScale LoadModule()
        {
            try
            {
                scaleCas = ModuleManeger.LoadModule("Hugin.POS.Scale", "Scale") as IScale;
            }
            catch (Exception ex)
            {
                CashRegister.Log.Error(ex);
            }

            if (scaleCas == null)
                scaleCas= new ScaleCAS();

            return scaleCas;
        }

        public void Connect()
        {
            string[] scaleParams = PosConfiguration.Get("ScaleComPort").Split(',');

            string portName = scaleParams[0];
            if (scaleParams.Length > 1)
                request = scaleParams[1];

            serialPort = new SerialPort(portName);
            serialPort.ReadTimeout = 1500;
            serialPort.WriteTimeout = 1500;

            if (serialPort.IsOpen)
                serialPort.Close();
            serialPort.Open();
        }

        public decimal GetWeight(decimal unitPrice)
        {
            tryCount = 0;
            return SendData(request);
        }

        private Decimal SendData(String request)
        {
            Decimal response = 1.00m;
            try
            {
                if (!serialPort.IsOpen)
                    Connect();
                CashRegister.Log.Info(serialPort.ReadExisting());
                serialPort.Write(request);
                System.Threading.Thread.Sleep(50);
                String strResponse = serialPort.ReadTo("\r").Replace(".", ",");

                if (!Parser.TryDecimal(strResponse, out response))
                {
                    tryCount++;
                    if (tryCount <= MAX_TRY_COUNT)
                        return SendData(request);
                }
            }
            catch (Exception ex)
            {
                CashRegister.Log.Error("Scale Communication Error: " + ex.Message);
            }

            return response;
        }

        public bool IsConnect
        {
            get { return serialPort.IsOpen; }
        }

    }
}
