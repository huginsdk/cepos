using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;


namespace Hugin.POS.States
{
    class EnterClerkNumber : EnterInteger
    {
        private static IState state = new EnterClerkNumber();
        protected new static Number input;
        const int MAX_LENGTH = 4;
        protected new static String cashierMsg = PosMessage.CLERK_ID;
        private static StateInstance<ICashier> ReturnConfirm;
        protected new static StateInstance ReturnCancel;    

        public override Error NotImplemented { 
            get { 
                return new Error(new InvalidOperationException(),ReturnCancel,ReturnCancel); 
            } 
        }
        public static IState Instance(String message, StateInstance<ICashier> ConfirmState, StateInstance CancelState)
        {
            input = new Number();
            cashierMsg = message;
            DisplayAdapter.Cashier.Show(message);
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;
            return state;
        }

        public static IState Retry()
        {
            input = new Number();
            DisplayAdapter.Cashier.Show("{0}\n{1}", PosMessage.CLERK_NOT_FOUND , PosMessage.PROMPT_RETRY);
            return state;
        }
        public override void Numeric(char c)
        {
            if (input.Length == MAX_LENGTH) return;
            if (!char.IsNumber(c)) return;
            input.AppendDecimal(c);
            DisplayAdapter.Cashier.Append(c.ToString());
        }
        #region KeyHandlers

        private void OnBackspace()
        {
            if (input.Length > 0)
            {
                input.RemoveLastDigit();
                DisplayAdapter.Cashier.BackSpace();
            }
            else
            {
                Escape();
            }
        }
        /// <summary>
        /// The label key function.
        /// </summary>
        /// <param name="label">
        /// The key hit by user.
        /// </param>
        public override void LabelKey(int label)
        {
            if (DisplayAdapter.Customer.HasAttribute(DisplayAttribute.TouchKeyboard)) return;

            if (label == Label.BackSpace)
            {
                OnBackspace();
            }
        }

        public override void Correction()
        {
            OnBackspace();
        }


        /// <summary>
        /// The Enter key function
        /// </summary>
         public override void Enter()
        {
            if (ReturnConfirm != null)
            {
                try
                {
                    if (input.Length == 0)
                        input = new Number(-1);
                    String id = String.Format("{0:D4}", input.ToInt());
                    ICashier cashier = cr.DataConnector.FindCashierById(id);
                    if (cashier !=null)
                    {
                        if (cashier.AuthorizationLevel != AuthorizationLevel.Seller)
                        {
                            cr.State = States.AlertCashier.Instance(
                                new Error(
                                new Exception(
                                String.Format("{0}\n{1}", cashier.Name.Trim(), PosMessage.NO_ACCESS_RIGHT))
                                )
                                );
                        }
                        else
                        {
                            cr.State = ReturnConfirm(cashier);
                        }
                    } 
                    else
                    {
                        cr.State = Retry();
                    }
                }
                catch (ArithmeticException ex)
                {
                    //if Int32.Parse could not be successfull
                    cr.State = AlertCashier.Instance(new Error(ex));
                }
            }
            
        }
         /// <summary>
         /// The Escape key function
         /// </summary>
         public override void Escape()
         {
             if (!input.IsEmpty)
             {
                 input.Clear();
                 DisplayAdapter.Cashier.Show(cashierMsg);
                 return;
             }

             if (ReturnCancel == null)
                 cr.State = States.Start.Instance();
             else
             {
                 try
                 {
                     cr.State = ReturnCancel();
                 }
                 catch
                 {
                     cr.Log.Error("Cannot find instance method of state");
                 }
             }

         }
        #endregion
 	}
}
