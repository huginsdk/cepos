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
            DisplayAdapter.Cashier.Show("KAS�YER DOSYASI\nYOK YADA HATALI");
            cr.Log.Fatal("Program ba�lamad�, kasiyer dosyas� hatal� yada bulunamad�");
            exception = e;
            return state;
        }
        public static IState Instance(InvalidProductFileException e)
        {
            DisplayAdapter.Cashier.Show("�R�N DOSYASI\nYOK YADA HATALI");
            cr.Log.Fatal("Program ba�lamad�, �r�n dosyas� hatal� yada bulunamad�");
            exception = e;
            return state;
        }
        public static IState Instance(InvalidSettingsFileException e)
        {
            DisplayAdapter.Cashier.Show("PROGRAM DOSYASI\nYOK YADA HATALI");
            cr.Log.Fatal("Program ba�lamad�, program dosyas� hatal� yada bulunamad�");
            exception = e;
            return state;
        }

        public static IState Instance()
        {
            if (exception is InvalidCashierFileException)
                DisplayAdapter.Cashier.Show("KAS�YER DOSYASI\nBULUNAMADI YA DA HATALI");
            else if (exception is InvalidProductFileException)
                DisplayAdapter.Cashier.Show("�R�N DOSYASI\nBULUNAMADI YA DA HATALI");
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
                    DisplayAdapter.Cashier.Show("KAS�YERLER\nY�KLEN�YOR");
                    cr.DataConnector.LoadCashiers();
                }
                else if (exception is InvalidProductFileException)
                {
                    DisplayAdapter.Cashier.Show("�R�NLER\nY�KLEN�YOR");
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
                cr.Log.Fatal("Y�kleme tamamlanamad�:" + ex.Message);
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
                cr.State = AlertCashier.Instance(new Confirm("Y�KLEME\nTAMAMLANAMADI", 
                                                            new StateInstance(Instance)));

        }
              

    }
}
