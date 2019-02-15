using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class FiscalIdBlock : BlockingState
    {
        private static IState state = new FiscalIdBlock();
        private static String LastFiscalId="";
        public static IState Instance(String lastFiscalId)
        {
            LastFiscalId = lastFiscalId;
            return Instance();
        }
        public static IState Instance()
        {
            return ConfirmCashier.Instance(new Confirm("HAFIZA UYUﬁMAZLI–I\nNUMARA GiR(GiRiﬁ)",
                                      new StateInstance(EnterFiscalId),
                                      new StateInstance(FiscalIdBlock.Instance)));
        }
        public static IState EnterFiscalId()
        {
            return EnterString.Instance("MALi HAFIZA NUMARASI",
                LastFiscalId, 
                new StateInstance<String>(CheckFiscalId), 
                new StateInstance(FiscalIdBlock.Instance));
        }
        public static IState CheckFiscalId(String FiscalId)
        {

            try
            {
                CashRegister.FiscalRegisterNo = FiscalId.Trim();
                PosConfiguration.Set("FiscalId", CashRegister.FiscalRegisterNo);
                CashRegister.Instance();
            }
            catch (FiscalIdException)
            {
                return Instance();
            }
            catch (Exception)
            {
            }

            return States.Start.Instance();
        }
        protected override bool BlockRemoved()
        {
            try
            {
                DisplayAdapter.Cashier.Show(PosMessage.CONNECTING_TO_PRINTER);
                CashRegister.Instance();
            }
            catch (FiscalIdException)
            {
                CashRegister.State = Instance();
                return false;
            }
            catch (Exception)
            {
            }
            return true;
        }
    }
}
