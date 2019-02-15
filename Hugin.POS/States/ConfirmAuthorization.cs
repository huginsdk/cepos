using System;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
	//internal delegate void ReturnInstance();
	 /// <summary>
     /// Used for custom error messages
	 /// </summary>
    class ConfirmAuthorization : SilentState
    {
        private static IState state = new ConfirmAuthorization();
        private static Authorizations AuthorizationType = Authorizations.VoidDocument;
        public static StateInstance<Hashtable> ReturnConfirmWithArgs;
        public static StateInstance ReturnConfirm;
        public static StateInstance ReturnCancel;
        public static Hashtable data;
        private static int cashierId = -1;
        
#region Instance
        public static IState Instance(StateInstance ConfirmState,  Authorizations authorizations)
        {
            ReturnConfirm = ConfirmState;
            AuthorizationType = authorizations;
            return CheckAuthorization();
        }

        public static IState Instance(StateInstance ConfirmState, StateInstance CancelState, Authorizations authorizations)
        {
            ReturnCancel = CancelState;
            return Instance(ConfirmState, authorizations);
        }

        public static IState Instance(StateInstance ConfirmState, StateInstance<Hashtable> CancelState, Authorizations authorizations)
        {
            ReturnConfirmWithArgs = CancelState;
            return Instance(ConfirmState, authorizations);

        }

        public static IState Instance(Confirm promptMessage, Authorizations authorizations)
        {
            AuthorizationType = authorizations;
            ReturnConfirm = promptMessage.ReturnConfirm;
            ReturnCancel = promptMessage.ReturnCancel;
            ReturnConfirmWithArgs = promptMessage.ReturnConfirmWithArgs;
            data = promptMessage.Data;

            promptMessage.ReturnConfirm = new StateInstance(CheckAuthorization);
            return States.ConfirmCashier.Instance(promptMessage);
        }
#endregion

        public static IState CheckAuthorization()
        {
            if (!(cr.IsAuthorisedFor(AuthorizationType)) &&
                !(DisplayAdapter.Both.HasAttribute(DisplayAttribute.CashierKey)))
            {
                DisplayAdapter.Cashier.Show(PosMessage.NO_ACCESS_RIGHT);
                System.Threading.Thread.Sleep(2000);
                return States.AlertCashier.Instance(new Confirm(PosMessage.PRESS_ENTER_TO_AUTH, 
                                                new StateInstance(EnterCashier),
                                                ReturnCancel));
            }
            return AuthorizedCashier();
        }

        public static IState EnterCashier()
        {

            cashierId = -1;
            //return States.EnterInteger.Instance(PosMessage.ENTER_CASHIER_ID, CheckCashierId, ReturnCancel);
            return States.EnterPassword.Instance(PosMessage.ENTER_PASSWORD, AccecptCashier, ReturnCancel);

        }

        private static IState CheckCashierId(int id)
        {
            if (id < 1 || id > cr.MAX_CASHIER_ID)
            {
                return States.AlertCashier.Instance(new Confirm(PosMessage.INVALID_CASHIER_ID,
                                            EnterCashier));
            }

            cashierId = id;
            return States.EnterPassword.Instance(PosMessage.ENTER_PASSWORD, AccecptCashier, ReturnCancel);
        }

        private static IState AccecptCashier(String password)
        {
            ICashier cashier = null;
            try
            {
                cashier = cr.SecurityConnector.LoginCashier(String.Format("{0:D6}", password));

                if (cashier == null)
                {
                    Confirm err = new Confirm(PosMessage.INVALID_CASHIER,
                                                EnterCashier);

                    cr.State = States.AlertCashier.Instance(err);
                }
                else
                {
                    // For set authorized cashier name before operation
                    try
                    {
                        cr.Printer.SaveCashier(int.Parse(cr.CurrentManager.Id), cashier.Name);                        
                    }
                    catch { }

                    cr.State = CheckCashierAuthorization(cashier);
                }
            }
            catch (CashierAutorizeException cae)
            {
                cr.Log.Error(cae);
                cr.State = States.AlertCashier.Instance(new Confirm(PosMessage.INVALID_CASHIER,
                                                EnterCashier));
            }
            catch (Exception ex)
            {
                cr.Log.Error(ex);
                cr.State = States.AlertCashier.Instance(new Error(ex,
                                                EnterCashier));
            }
            return cr.State;
        }
        public static IState CheckCashierAuthorization(ICashier cashier)
        {
            cr.RegisterAuthorizationLevel = cashier.AuthorizationLevel;
            return CheckAuthorization();
        }

        private static IState AuthorizedCashier()
        {
            if (ReturnConfirm != null)
                cr.State = ReturnConfirm();
            else if (ReturnConfirmWithArgs != null && data != null)
                cr.State = ReturnConfirmWithArgs(data);

            // For set authorized cashier name after operation
            try
            {
                cr.Printer.SaveCashier(int.Parse(cr.CurrentManager.Id), cr.CurrentCashier.Name);
            }
            catch { }
            cr.RegisterAuthorizationLevel = cr.CurrentCashier.AuthorizationLevel;

            data = null;

            return cr.State;
        }

        private static IState UnauthorizedCashier()
        {
            return (ReturnCancel == null) ? States.Start.Instance() :
                                               ReturnCancel();
        }


    }
}
