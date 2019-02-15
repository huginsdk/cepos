using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
	//internal delegate void ReturnInstance();
	 
    class CashRegisterLoadError : BlockingState
    {

        private static IState state = new CashRegisterLoadError();
        private static Exception exception = null;
        
        public static IState Instance(InvalidCashierFileException e)
        {
            DisplayAdapter.Cashier.Show("KASÝYER DOSYASI\nYOK YADA HATALI");
            cr.Log.Fatal("Program baþlamadý, kasiyer dosyasý hatalý yada bulunamadý");
            exception = e;
            return state;
        }
        public static IState Instance(InvalidProductFileException e)
        {
            DisplayAdapter.Cashier.Show("ÜRÜN DOSYASI\nYOK YADA HATALI");
            cr.Log.Fatal("Program baþlamadý, ürün dosyasý hatalý yada bulunamadý");
            exception = e;
            return state;
        }
        public static IState Instance(InvalidSettingsFileException e)
        {
            DisplayAdapter.Cashier.Show("PROGRAM DOSYASI\nYOK YADA HATALI");
            cr.Log.Fatal("Program baþlamadý, program dosyasý hatalý yada bulunamadý");
            exception = e;
            return state;
        }

        public static IState Instance()
        {
            if (exception is InvalidCashierFileException)
                DisplayAdapter.Cashier.Show("KASÝYER DOSYASI\nBULUNAMADI YA DA HATALI");
            else if (exception is InvalidProductFileException)
                DisplayAdapter.Cashier.Show("ÜRÜN DOSYASI\nBULUNAMADI YA DA HATALI");
            else if (exception is InvalidSettingsFileException)
                DisplayAdapter.Cashier.Show("PROGRAM DOSYASI\nBULUNAMADI YA DA HATALI");

            return state;
        } 

        protected override Boolean BlockRemoved()
        {
            try
            {
                if (exception is InvalidCashierFileException)
                {
                    DisplayAdapter.Cashier.Show("KASÝYERLER\nYÜKLENÝYOR");
                    cr.DataConnector.LoadCashiers();
                }
                else if (exception is InvalidProductFileException)
                {
                    DisplayAdapter.Cashier.Show("ÜRÜNLER\nYÜKLENÝYOR");
                    cr.DataConnector.LoadProducts();
                }
                else if (exception is InvalidSettingsFileException)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.LOADING_NEW_PROGRAM);
                    cr.DataConnector.LoadSettings();
                }
                return true;
            }
            catch (Exception ex)
            {
                cr.Log.Fatal("Yükleme tamamlanamadý:" + ex.Message);
                return false;
            }

        }

        public override void Program()
        {
            cr.State = States.SetupMenu.Instance(Instance);
        }

        public override void Command()
        {
        }

        public override void Escape()
        {
            if (BlockRemoved())
            {
                CashRegister.Instance();
            }
            else
                cr.State = AlertCashier.Instance(new Confirm("YÜKLEME\nTAMAMLANAMADI", 
                                                            new StateInstance(Instance)));

        }
              

    }
}
