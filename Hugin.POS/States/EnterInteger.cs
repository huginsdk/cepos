using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;


namespace Hugin.POS.States
{
    class EnterInteger :State
    {
        private static IState state = new EnterInteger();
        protected static Number input;
        const int MAX_LENGTH = 19;
        protected static String cashierMsg = PosMessage.ENTER_NUMBER;
        private static StateInstance<int> ReturnConfirm;
        protected static StateInstance ReturnCancel;
        private static StateInstance<int> ReturnSaler;       

        protected bool defaultValueOn;

        public override Error NotImplemented { 
            get { 
                return new Error(new InvalidOperationException(),ReturnCancel,ReturnCancel); 
            } 
        }

        /// <summary>
        /// EnterInteger Instance
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>        
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>        
        /// <returns>
        /// EnterInteger State.
        /// </returns>
        public static IState Instance(String message, StateInstance<int> ConfirmState)
        {
        	return Instance(message,ConfirmState, null);
        }
        /// <summary>
        /// EnterInteger Instance
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>        
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>
        /// <param name="CancelState">
        /// CancelState:what state the machine should assume after user hits Escape key.
        /// </param>
        /// <returns>
        /// EnterInteger State.
        /// </returns>
        public static IState Instance(String message, StateInstance<int> ConfirmState, StateInstance CancelState)
        {
            input = new Number();
            ((EnterInteger)state).defaultValueOn = false;
            cashierMsg = message;
            DisplayAdapter.Cashier.Show(message);
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;
            return state;
        }
        /// <summary>
        /// EnterInteger Instance
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>        
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>
        /// <param name="CancelState">
        /// CancelState:what state the machine should assume after user hits Escape key.
        /// </param>
        /// <param name="ConfirmSalerState">
        /// ConfirmSalerState:what state the machine should assume after user hits Saler key.
        /// </param>
        /// <returns>
        /// EnterInteger State.
        /// </returns>
        public static IState Instance(String message, StateInstance<int> ConfirmState, StateInstance<int> ConfirmSalerState, StateInstance CancelState)
        {
            ReturnSaler = ConfirmSalerState;
            return Instance(message, ConfirmState, CancelState);
        }
        /// <summary>
        /// EnterInteger Instance
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>
        /// <param name="defaultValue">
        /// Defoult input value.
        /// </param>
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>        
        /// <returns>
        /// EnterInteger State.
        /// </returns>
        public static IState Instance(String message, int defaultValue, StateInstance<int> ConfirmState)
        {
            return Instance(message, defaultValue, ConfirmState, null);
        }
        /// <summary>
        /// EnterInteger instance.
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>
        /// <param name="defaultValue">
        ///  Default input value.
        /// </param>
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>
        /// <param name="CancelState">
        /// CancelState:what state the machine should assume after user hits Escape key.
        /// </param>
        /// <returns>
        /// EnterInteger State.
        /// </returns>
        public static IState Instance(String message, int defaultValue, StateInstance<int> ConfirmState, StateInstance CancelState)
        {

            return Instance(message, new Decimal(defaultValue), ConfirmState, CancelState);
        }
        /// <summary>
        /// EnterInteger instance.
        /// </summary>
        /// <param name="message">
        /// Cashier message.
        /// </param>
        /// <param name="defaultValue">
        /// Default input string that will be appended.
        /// </param>
        /// <param name="ConfirmState">
        /// ConfirmState:what state the machine should assume after user hits Enter key.
        /// </param>
        /// <param name="CancelState">
        /// CancelState:what state the machine should assume after user hits Escape key.
        /// </param>
        /// <returns>
        /// EnterInteger State
        /// </returns>
        public static IState Instance(String message, decimal defaultValue, StateInstance<int> ConfirmState, StateInstance CancelState)
        {
            input = new Number(defaultValue);
            ((EnterInteger)state).defaultValueOn = true;
            cashierMsg = message;
            DisplayAdapter.Cashier.Show("{0}\n{1}\t", cashierMsg, input.ToString());


            DisplayAdapter.Cashier.Append(input.ToString());

            //TODO: Cursor satirbasi olmali - overwrite mode
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;
            return state;
        }
        /// <summary>
        /// The numeric key and some label key function.
        /// </summary>
        /// <param name="c">
        /// Char that will be appended.
        /// </param>
        public override void Numeric(char c)
       {
           if (input.Length == MAX_LENGTH) return;
           if (!char.IsNumber(c)) return;
           if (((EnterInteger)state).defaultValueOn)
           {
               ((EnterInteger)state).defaultValueOn = false;
               input.Clear();
               DisplayAdapter.Cashier.Show(cashierMsg);
           }
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
                ((EnterInteger)state).defaultValueOn = false;
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
        /// The Escape key function
        /// </summary>
        public override void Escape()
        {
            if (!input.IsEmpty) {
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
                    cr.State = ReturnConfirm(Int32.Parse(input.ToString()));
                }
                catch (ArithmeticException ex)
                {
                    //if Int32.Parse could not be successfull
                    cr.State = AlertCashier.Instance(new Error(ex));
                }
            }
            
        }
        public override void SalesPerson()
        {
            if (ReturnSaler == null) base.SalesPerson();
            try
            {
                input = new Number(cr.CurrentCashier.Id);
                cr.State = ReturnSaler(Int32.Parse(input.ToString()));
            }
            catch (ArithmeticException ex)
            {
                //if Int32.Parse could not be successfull
                cr.State = AlertCashier.Instance(new Error(ex));
            }
        }
        public override void Pay(CashPaymentInfo info)
        {
            return;
        }

        public override void Pay(CheckPaymentInfo info)
        {
            return;
        }

        public override void Pay(CreditPaymentInfo info)
        {
            return;
        }

        public override void Pay(CurrencyPaymentInfo info)
        {
            return;
        }
        #endregion
 	}
}
