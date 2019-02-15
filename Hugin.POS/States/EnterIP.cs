using System;
using System.Text;
using System.Text.RegularExpressions;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS.States
{
    class EnterIP : SilentState
    {
        private static IState state = new EnterIP();
        private static Number input;
        private static String cashierMsg;
        private static StateInstance<String> ReturnConfirm;
        private static StateInstance ReturnCancel;


        protected bool defaultValueOn;

        public static IState Instance(String message, StateInstance<String> ConfirmState)
        {
            return Instance(message, ConfirmState, null);
        }
        public static IState Instance(String message, StateInstance<String> ConfirmState, StateInstance CancelState)
        {
            input = new Number();
            input.OverrideDecimalSeperator = ".";
            ((EnterIP)state).defaultValueOn = false;
            cashierMsg = message;
            DisplayAdapter.Cashier.Show("{0}", cashierMsg);
            //TODO: Cursor satirbasi olmali - overwrite mode
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;
            return state;
        }
        public static IState Instance(String message, String defaultValue,StateInstance<String> ConfirmState, StateInstance CancelState)
        {
            input = new Number(defaultValue);
            ((EnterIP)state).defaultValueOn = true;
            input.OverrideDecimalSeperator = ".";
            cashierMsg = message;
            DisplayAdapter.Cashier.Show("{0}\n{1}\t", cashierMsg, input.ToString("B"));  
            //TODO: Cursor satirbasi olmali - overwrite mode
            ReturnConfirm = ConfirmState;
            ReturnCancel = CancelState;
            return state;
        }         
        #region KeyHandlers

        public override void Correction()
        {
            OnBackspace();
        }

        private void OnBackspace()
        {

            if (((EnterIP)state).defaultValueOn)
            {
                ((EnterIP)state).defaultValueOn = false;
                input.Clear();
                DisplayAdapter.Cashier.Show(cashierMsg);
                return;
            }
            if (input.Length > 0)
            {
                input.RemoveLastDigit();
                DisplayAdapter.Cashier.BackSpace();
                ((EnterIP)state).defaultValueOn = false;
            }
            else
            {
                Escape();
            }
        }
        public override void Numeric(char c)
        {

            if (((EnterIP)state).defaultValueOn)
            {
                ((EnterIP)state).defaultValueOn = false;
                //DisplayAdapter.Cashier.Show(cashierMsg);

                DisplayAdapter.Cashier.Append(input.ToString("B"));
            }
            input.AppendDecimal(c);
            DisplayAdapter.Cashier.Append(c.ToString());
            //DisplayAdapter.Cashier.Show("{0}\n{1}\t", cashierMsg, input.ToString());
        }

        public override void Escape()
        {
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

        public override void Enter()
        {
            if (IsValidIPAddress(input.ToString("B")))
            {
                if (ReturnConfirm != null)
                {
                    try
                    {
                        cr.State = ReturnConfirm(input.ToString("B"));
                    }
                    catch
                    {
                        cr.State = AlertCashier.Instance(new Error(new InvalidOperationException(),
                                                                    ReturnCancel,
                                                                    ReturnCancel));
                    }
                }
            }
            else 
            {
                Confirm err = new Confirm("HATALI IP",
                             new StateInstance(ReturnCancel),
                             new StateInstance(ReturnCancel));
                cr.State = AlertCashier.Instance(err);
            }

        }

        public override void Seperator()
        {
            input = new Number(input.ToString("B") + ".");
            input.OverrideDecimalSeperator = ".";
            DisplayAdapter.Cashier.Append(".");
        }
        //public override void BackSpace()
        //{
        //    input.RemoveLastDigit();
        //    DisplayAdapter.Cashier.BackSpace();
        //}
        public override void LabelKey(int label)
        {
            if (label == Label.BackSpace){
                OnBackspace();
            }
            else base.LabelKey(label);
        }
        static bool IsValidIPAddress(string strIP)
        {
            // Allows four octets of numbers that contain values between 4 numbers in the IP address to 0-255 and are separated by periods
            string regExPattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(strIP, regExPattern);
        }
        #endregion
    }
}
