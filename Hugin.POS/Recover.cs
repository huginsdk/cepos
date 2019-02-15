using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using cr = Hugin.POS.CashRegister;

namespace Hugin.POS
{
    public class Recover
    {        
        public static void RecoverPowerFailure(PowerFailureException pfe)
        {
            try
            {
                cr.Document.Cancel();
                cr.Document.Void();
                cr.State = States.Start.Instance();
            }
            catch (EJException eje)
            {
                throw eje;
            }
            catch (Exception e)
            {
                cr.Log.Warning("Error on RecoverPowerFailure(...)");
                cr.Log.Warning(e);
                cr.State = States.Start.Instance();
            }
        }
        public static void RecoverUnfixedSlip(UnfixedSlipException use)
        {
            /*
             * When the printer has no paper but slip signal does not alarm (light on-off regularly)
             * or paper is not fixed properly
             * the printer sends "32" ,it means no paper
             * this function recovers the problem 
             */
            try
            {
                /* if program is loading and register has active invoice
                 * if the printer slip signal does not alarm, that means paper was pull out by hand
                 * so alert cashier to put a paper to cancel invoice
                 */
                if (use.Subtotal == 0 && cr.Document.TotalAmount != 0)
                {
                    cr.State = States.AlertCashier.Instance(new Confirm("YAZICIYA KAGIT\nYERLESTiRiNiZ"));
                    cr.Document.Void();
                }
                /*
                 * when the printer could not write to paper, printer thinks no paper
                 * if printer has paper, cancel the invoice, paper will be push out
                 */
                else
                {
                    cr.Document.Void();
                    cr.State = States.AlertCashier.Instance(new Confirm(Common.PosMessage.UNFIXED_SLIP_EXCEPTION));
                }

            }
            catch (EJException eje)
            {
                throw eje;
            }
            catch (Exception e)
            {
                cr.Log.Warning("Error on RecoverUnfixedSlip(...)");
                cr.Log.Warning(e);
                cr.State = States.Start.Instance();
            }
        }
    }
}
