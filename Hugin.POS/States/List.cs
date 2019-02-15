using System;
using System.Collections;
using System.Threading;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;

namespace Hugin.POS
{

    //ProcessSelectedItem is executed when user selects the item by hitting enter
    //in this state. The function it points to get set by the Instance
    //method of this state

    public delegate void ProcessSelectedItem(Object o);
    public delegate void ProcessSelectedItem<TSelectedItem>(TSelectedItem item);
}

namespace Hugin.POS.States
{
    class List : SilentState
    {
        class TimerState
        {
            public DateTime lastKeyPressed = CashRegisterInput.lastKeyPressed;
            public IState createState = cr.State;
            public System.Threading.Timer tmr = null;
            public bool enabled = false;
        }

        private static TimerState ts = new TimerState();

        private static void timerCallback(object state)
        {
            TimerState ts1 = (TimerState)state;
            if (cr.State is List && ((List)cr.State).autoEnter > 0 && ts1.lastKeyPressed == CashRegisterInput.lastKeyPressed)
                try
                {
                    cr.State.Enter();
                }
                catch (Exception e) {
                    cr.Log.Warning(e);
                    cr.State = AlertCashier.Instance(new Error(e));
                }
        }

        private static IState state = new List();
        protected static IDoubleEnumerator ie;
        protected static ProcessSelectedItem ProcessSelected;
        public static StateInstance ReturnCancel;
        protected int autoEnter = 0;

        public static IState Instance()
        {
        	if (ie == null)
        		return AlertCashier.Instance(new Error(new ProductNotFoundException()));
        	ie.Reset();
        	if (ie.MoveNext())
        		ie.ShowCurrent();
            else
                return AlertCashier.Instance(new Confirm(PosMessage.LIST_EMPTY));
            return state;
        } 
        
        //Instance method has 2 parameters
        //1. Enumerator for a list of items to be displayed. Each item implements
        //IMenuItem interface, which means that it knows howto display itself
        //2. Delegate for the method which will be called on the selected item
        //when the user presses enter key
        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem psi)
        {
            ProcessSelected = psi;
            return Instance(ide);
        }
        public static IState Instance(IDoubleEnumerator ide, ProcessSelectedItem psi,StateInstance returnCancel)
        {
            ProcessSelected = psi;
            ReturnCancel = returnCancel;
            return Instance(ide);
        }

        public static IState Instance(IDoubleEnumerator ide)
        {
            ie = ide;
            try
            {
                if (ie.MoveNext())
                    ie.ShowCurrent(Target.Cashier);
                else
                    throw new ListingException();
                ShowList();
            }
            catch (ListingException)
            {
                throw new Exception(PosMessage.LIST_EMPTY);
            }
            catch (Exception ex)
            {
                cr.Log.Error("Exception occured. {0}", ex.Message);
                throw new ListingException();
            }
            return state;
        }

        private static void ShowList()
        {
            if (ie.Current is MenuLabel || ie.Current is SalesItem || ie.Current is IProduct)
                DisplayAdapter.Cashier.Show(ie as MenuList);
        }

        //PSI not supplied in below function - designed to be called from inherited classes such
        //as SetupMenu where returnCancel may be CashRegisterLoadError instance

        //protected static IState Instance(IDoubleEnumerator ide, StateInstance returnCancel)
        //{
        //    ReturnCancel = returnCancel;
        //    return Instance(ide);
        //}  

        public override void Escape()
        {
            if (ie.Current is FiscalItem && cr.Item.TotalAmount>0) return;
       		ie = null;
            if (ReturnCancel == null)
                cr.State = Start.Instance();
            else
                cr.State = ReturnCancel();           
            ReturnCancel = null;

        }
        
        public override void DownArrow()
        {
            if (!ie.MoveNext()) ie.MoveFirst();
            ie.ShowCurrent(Target.Cashier);
            ShowList();
            if (ts != null && ts.tmr != null)
            {
                ts.lastKeyPressed = CashRegisterInput.lastKeyPressed;
                ts.tmr.Change(autoEnter,
                              System.Threading.Timeout.Infinite);
            }
        }
        
        public override void UpArrow()
        {
        	if (!ie.MovePrevious()) ie.MoveLast();
            ie.ShowCurrent(Target.Cashier);
            ShowList();
            if (ts != null && ts.tmr != null)
            {
                ts.lastKeyPressed = CashRegisterInput.lastKeyPressed;
                ts.tmr.Change(autoEnter,
                              System.Threading.Timeout.Infinite);
            }
        }       

        public override void Enter()
        {
        	 ProcessSelected(ie.Current);
        }

        public override void Numeric(char c)
        {            
            ie.Reset();
            for (int i = int.Parse(c.ToString());
                    ie.MoveNext() && i > 0;
                    i--) ;
            if (ie.MovePrevious())
            {
                ie.ShowCurrent(Target.Cashier);
            }
            ShowList();
        }

        public override void OnEntry()
        {
            if (cr.State == state) return;
            autoEnter = 0;
            if ((cr.State is List))
                autoEnter= PosConfiguration.GetListAutoSelect(cr.State);
            if (autoEnter > 0){
                ts.lastKeyPressed = CashRegisterInput.lastKeyPressed;
                if (ts.tmr == null)
                    ts.tmr = new Timer(timerCallback, ts, autoEnter, Timeout.Infinite);
                else 
                    ts.tmr.Change(autoEnter,System.Threading.Timeout.Infinite);
            }
        }

        public override void OnExit()
        {
            DisplayAdapter.Cashier.Show(null as MenuList);
            autoEnter = 0;
            //when a list item is selected from menu, if operation requires confirmation then state changes
            //however the return state should not be changed, because return state can be a blocking state like CashRegisterLoadError
            if (!(cr.State is States.SetupMenu) && !(cr.State is States.ListCommandMenu))
                ReturnCancel = null;
        }
    }
}
