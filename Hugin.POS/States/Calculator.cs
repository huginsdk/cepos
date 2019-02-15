using System;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using Hugin.POS.Data;

namespace Hugin.POS.States
{
    class Calculator : SilentState
    {
        private static IState state = new Calculator();
        private static Number input;
        private static Number currentInput;
        public const Char ADD = '+';
        public const Char SUBTRACT = '-';
        public const Char DIVIDE = '/';
        public const Char MULTIPLY = '*';
        public Char opType = '?';

        public static IState Instance()
        {
            //Show main menu to cashier and customer
            DisplayAdapter.Cashier.Show(PosMessage.CALCULATOR);
            input = new Number();
            currentInput = new Number();

            return state;
        }
        public override void OnEntry()
        {
            //Display knowes application enter calculator states
            MenuList menuCalculator = new MenuList();
            menuCalculator.Add(PosMessage.ENTER_CALCULATOR);
            menuCalculator.MoveFirst();
            DisplayAdapter.Cashier.Show(menuCalculator);
            DisplayAdapter.Cashier.Show(null as MenuList);
        }
        public override void OnExit()
        {
            //Display knowes application exit from calculator states
            MenuList menuCalculator = new MenuList();
            menuCalculator.Add(PosMessage.EXIT_CALCULATOR);
            menuCalculator.MoveFirst();
            DisplayAdapter.Cashier.Show(menuCalculator);
            DisplayAdapter.Cashier.Show(null as MenuList);
        }
        public override void Escape()
        {
            cr.State = States.ConfirmCashier.Instance(
                new Confirm(PosMessage.CONFIRM_EXIT_CALCULATOR,
                Start.Instance,
                Calculator.Instance)
                );

        }
        public override void Numeric(char c)
        {
            input.AppendDecimal(c);
            DisplayAdapter.Cashier.Show("{0}\n\t{1}", PosMessage.CALCULATOR, input);
            if (opType == '?')
            {
                currentInput.Clear();
            }

        }
        public override void Seperator()
        {
            input.AddSeperator();
            DisplayAdapter.Cashier.Show("{0}\n\t{1}", PosMessage.CALCULATOR, input);
        }
        public override void Enter()
        {
            switch (opType)
            {
                case ADD:
                    currentInput = currentInput + input;
                    break;
                case SUBTRACT:
                    currentInput = currentInput - input;
                    break;
                case DIVIDE:
                    currentInput = currentInput / input;
                    break;
                case MULTIPLY:
                    currentInput = currentInput * input;
                    break;
                default:
                    cr.State = States.AlertCashier.Instance(new Error(new InvalidOperationException(), Instance));
                    break;
            }
            DisplayAdapter.Cashier.Show("{0}\n\t{1}", PosMessage.CALCULATOR, currentInput);
            input.Clear();
            opType = '?';
        }

        public override void Correction()
        {
            decimal dec = input.ToDecimal();
            if (dec == 0)
            {
                currentInput = new Number();
                DisplayAdapter.Cashier.Show(PosMessage.CALCULATOR);
            }
            else
            {
                input.RemoveLastDigit();
                DisplayAdapter.Cashier.Show("{0}\n\t{1}", PosMessage.CALCULATOR, input);
            }
        }

        public void Operation(char op)
        {
            if (currentInput.IsEmpty)
            {
                currentInput = new Number(input.ToDecimal());
                opType = op;
            }
            else
            {
                if (opType != '?')
                    Enter();
                opType = op;
            }
            input.Clear();
        }

        public override void  Pay(CheckPaymentInfo info)
        {
            Operation(DIVIDE);
        }
        public override void  Pay(CurrencyPaymentInfo info)
        {
            Operation(MULTIPLY);
        }
        public override void  SubTotal()
        {
            Operation(ADD);
        }
        public override void  Pay(CashPaymentInfo info)
        {
            Operation(SUBTRACT);
        }

        private void WriteChar(int order, PosKey key)
        {
            byte sequence = 0;
            char c = KeyMap.GetLetter(key, order, ref sequence);
            switch (c)
            {
                case SUBTRACT:
                case ADD:
                case DIVIDE:
                case MULTIPLY: Operation(c);
                    break;                   
                default: break;
            }
        }

    }
}
