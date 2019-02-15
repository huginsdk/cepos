using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using System.Reflection;

namespace Hugin.POS
{   
    public delegate void ZEventHandler(int ZReportId);
 }

namespace Hugin.POS.States
{
    class ReportMenu : List
    {
        private static IState state = new ReportMenu();
        public static ZEventHandler OnZReportComplete;
        private static bool ejOnly = false;
        private static IReport displayingReport = null;
        private static int parameterCount = 0;
        private static String[] keys = null;
        private static Type[] types = null;
        private static Object[] values = null;

        public static new IState Instance()
        {
            ejOnly = false;
            LoadMenu();
            return state;
        }

        public static IState EJOnly()
        {
            ejOnly = true;
            LoadMenu();
            return state;
        }

        private static void LoadMenu()
        {
            MenuList menuHeaders = new MenuList();
            int index = 1;

            displayingReport = cr.Printer.GetReports(ejOnly);
            
            foreach (IReport report in displayingReport.Subreports)
            {
                if (ejOnly || cr.CurrentCashier.AuthorizationLevel >= report.Authorization || cr.RegisterAuthorizationLevel >= report.Authorization)
                {
                    menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", PosMessage.REPORT, index++, report.Name)));
                }
            }

            if (menuHeaders.IsEmpty)
            {
                if (displayingReport.Subreports.Count > 0)
                {
                    throw new CashierAutorizeException();
                }
                else
                {
                    throw new Exception(PosMessage.NO_SUPPORTED_REPORT);
                }
            }

            List.Instance(menuHeaders);

            //:to do: think EJ_LIMIT_SETTING
            //menuHeaders.Add(new MenuLabel(String.Format("RAPOR             {0,2}\n{1}", index++, PosMessage.EJ_LIMIT_SETTING)));
        }

        public static IState Continue()
        {
            if(ie!=null)
                ie.ShowCurrent();
            if (displayingReport != null)
                displayingReport = displayingReport.Parent;
            return state;
        }

        static bool onReporting = false;
        public static IState OnReporting()
        {
            onReporting = true;
            return state;
        }

        public static IState ReportFinished(IPrinterResponse response)
        {
            if (CashRegister.CurrentCashier == null && !ejOnly)
                return cr.State;
            onReporting = false;
            return Continue();
        }      

        public override void  Enter()
        {
            if (onReporting) 
                return;

            parameterCount = 0;
            string message = ((MenuLabel)ie.Current).ToString();
            message = message.Substring(message.IndexOf('\n') + 1);

            MenuList menuHeaders = new MenuList();
            int index = 1;
            if (displayingReport.Name != message)
            {
                foreach (IReport report in displayingReport.Subreports)
                {
                    if (report.Name == message)
                    {
                        displayingReport = report; 
                        
                        if (displayingReport.Subreports.Count > 0)
                        {
                            foreach (IReport sub in displayingReport.Subreports)
                                menuHeaders.Add(new MenuLabel(String.Format(report.Name.PadRight(18) + "{0,2}\n{1}", index++, sub.Name)));

                            List.Instance(menuHeaders);
                        }
                        break;
                    }
                }
            }

            if (displayingReport.Subreports.Count == 0)
            {
                keys = new String[displayingReport.Parameters.Count];
                displayingReport.Parameters.Keys.CopyTo(keys, 0);
                types = new Type[displayingReport.Parameters.Count + (displayingReport.CanHardCopy ? 1 : 0)];
                values = new Object[types.Length];
                cr.State = GetParameters();
            }

            
        }

        private static IState GetParameters()
        {
            if (parameterCount == displayingReport.Parameters.Count)
            {
                if (displayingReport.CanHardCopy)
                {
                    types[types.Length - 1] = typeof(Boolean);
                    values[values.Length - 1] = true;
                }

                if(displayingReport.Name == PosMessage.Z_REPORT)
                {
                    // for adding return docs params
                    string[] returnAmountList = cr.DataConnector.GetReturnAmounts(cr.Id);

                    if(returnAmountList.Length == 2)
                    {
                        try
                        {
                            int count = int.Parse(returnAmountList[0]);
                            decimal amount = Decimal.Parse(returnAmountList[1]);

                            types = new Type[3];
                            values = new Object[types.Length];

                            types[0] = typeof(int);
                            values[0] = count;

                            types[1] = typeof(decimal);
                            values[1] = amount;

                            types[2] = typeof(Boolean);
                            values[2] = true;
                        }
                        catch { }
                        
                    }
                }

                return States.ConfirmCashier.Instance(new Confirm(PosMessage.CONFIRM_START_REPORT + "\n" + displayingReport.Name,
                                                            new StateInstance(GetReport),
                                                            new StateInstance(Continue)));

            }

            String key = keys[parameterCount];
            types[parameterCount] = displayingReport.Parameters[key];

            if (types[parameterCount] == typeof(int))
            {
                return States.EnterInteger.Instance(key,
                                                    new StateInstance<int>(GetIntValue),
                                                    new StateInstance(Continue));
            }
            else if (types[parameterCount] == typeof(long))
            {
                return States.EnterString.Instance(key,
                                                  new StateInstance<String>(GetLongValue),
                                                  new StateInstance(Continue));
            }
            else if (types[parameterCount] == typeof(ICashier))
            {
                return GetCashierValue(cr.CurrentCashier);
            }
            else
            {
                return States.EnterInteger.Instance(key,
                                                   new StateInstance<int>(GetDateValue),
                                                   new StateInstance(Continue)
                                                   );
            }
        }
        public static IState GetReport()
        {
            try
            {
                cr.Printer.AdjustPrinter(new Receipt());
            }
            catch { }

            MethodInfo mi = cr.Printer.GetType().GetMethod(displayingReport.Method, types);

            IPrinterResponse response = null;
            try
            {
                cr.State = States.WaitingState.Instance();

                if (displayingReport.Name != PosMessage.EJ_LIMIT_SETTING)
                {
                    DisplayAdapter.Cashier.Show(PosMessage.REPORT_PROCESSING + "\n" + displayingReport.Name);
                    System.Threading.Thread.Sleep(50);
                }

                if (displayingReport.Interruptable)
                {
                    exReporting = null;
                    System.Threading.Thread reportThread = new System.Threading.Thread(delegate()
                    {
                        try
                        {
                            response = PrintInvokedReport(mi, values);
                            ReportFinished(response);
                        }
                        catch (Exception ex)
                        {
                            exReporting = ex;
                            Chassis.Engine.Process(PosKey.Escape);
                        }
                    });
                    reportThread.Start();
                    return OnReporting();
                }
                else
                {
                    response = PrintInvokedReport(mi, values);
                }

                // if report not Z report, because of Z report loging on After_ZReport event in CashRegister
                if (displayingReport.FileName == "Z_RAPORU")
                {
                    BackgroundWorker.IsfterZreport = true;
                }
                else
                {
                    // Log report content
                    if (!response.HasError && !String.IsNullOrEmpty(response.Detail))
                        cr.DataConnector.SaveReport(displayingReport.FileName, response.Detail);
                }

            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                    ex = new Exception(ex.Message, ex);
                if (ex is IncompleteXReportException)
                    throw ex;
                if (!(ex.InnerException is ParameterRelationException) && ex.InnerException is ParameterException)
                    //return States.ConfirmCashier.Instance(new Confirm(PosMessage.NO_DOCUMENT_FOUND_IN_EJ, Continue, Continue));
                    return States.AlertCashier.Instance(new Confirm(PosMessage.RANGE_OF_NUMBER_EXCEPTION, Continue));
                if(ex.InnerException is PluLimitExceededException)
                    return States.AlertCashier.Instance(new Confirm(PosMessage.PLU_LIMIT_EXCEEDED, Continue));
                throw ex.InnerException;
            }
            if (CashRegister.CurrentCashier == null && !ejOnly)
                return cr.State;
            return Continue();
        }

        private static IPrinterResponse PrintInvokedReport(MethodInfo mi, object[] values)
        {
            if (values.Length == 0)
                return (IPrinterResponse)(mi.Invoke(cr.Printer, null));
            else
                return (IPrinterResponse)(mi.Invoke(cr.Printer, values));
        }
        public static IState GetIntValue(int value)
        {
            
            values[parameterCount] = value;
            parameterCount++;

            return GetParameters();
        }
        public static IState GetLongValue(String value)
        {
            values[parameterCount] = long.Parse(value);
            parameterCount++;

            return GetParameters();
        }


        public static IState GetDateValue(int date)
        {
            try
            {
                string value = date.ToString();
                if (value.Length > 8)
                    throw new FormatException();
                try
                {
                    if (date < 0)
                        values[parameterCount] = DateTime.Today;
                    else
                        values[parameterCount] = DateTime.Parse(FormatDate(value), PosConfiguration.CultureInfo.DateTimeFormat);
                }
                catch
                {
                    throw new FormatException();
                }

                if (types[parameterCount] == typeof(Date))
                {
                    types[parameterCount] = typeof(DateTime);
                    parameterCount++;
                    return GetParameters();
                }

                return States.EnterInteger.Instance(PosMessage.TIME,
                                                    new StateInstance<int>(GetTimeValue),
                                                    new StateInstance(Continue));
            }
            catch (FormatException)
            {
                Confirm err = new Confirm(PosMessage.INVALID_DATE_INPUT,
                     new StateInstance(Continue),
                     new StateInstance(Continue));
                return AlertCashier.Instance(err);
            }
        }

        public static IState GetTimeValue(int time)
        {
            if (time == -1) time = 0;
            DateTime dt = (DateTime)values[parameterCount];
            values[parameterCount] = dt.AddHours(time / 100).AddMinutes(time % 100);

            parameterCount++;
            return GetParameters();
        }

        public static IState GetCashierValue(ICashier cashier)
        {
            values[parameterCount] = cashier;

            parameterCount++;
            return GetParameters();
        }

        static Exception exReporting = null;
        public override void Escape()
        {
            if (onReporting)
            {
                DisplayAdapter.Cashier.Show(PosMessage.REPORT_STOPPING);
                onReporting = false;
                if (exReporting != null)
                {
                    if (exReporting.InnerException == null)
                    {
                        exReporting = new Exception(exReporting.Message, exReporting);
                    }

                    Exception ex = exReporting.InnerException;
                    exReporting = null;
                    if (!(ex is ParameterRelationException) && ex is ParameterException)
                        cr.State =States.ConfirmCashier.Instance(
                                        new Confirm(PosMessage.NO_DOCUMENT_FOUND_IN_EJ, 
                                        Continue));
                    else
                        throw ex;
                }
                else
                {
                    cr.Printer.InterruptReport();
                }
                return;
            }
            if (ejOnly)
            {
                cr.State = ElectronicJournalError.Instance();
                ejOnly = false;
                return;
            }
            if (displayingReport.Subreports.Count > 0 && displayingReport.Parent != null)
            {
                displayingReport = displayingReport.Parent;
                cr.State = Instance();
                return;
            }
            base.Escape();
        }
      
        public override void Report()
        {
            base.DownArrow();
        }

        public static String FormatDate(String date)
        {
            int loc = 1;
            if (date.Length == 8)
                loc++;
            date = date.Insert(loc, ".");
            return date.Insert(loc + 3, ".");
        }

        internal static string FormatTime(int time)
        {
            if (time == -1) time = 0;
            return String.Format("{0}:{1}", (time / 100), (time % 100));
        }

        public override void End(int keyLevel)
        {
            if (ejOnly) return;
            base.End(keyLevel);
        }

        #region Printer Reports

        public static IState PrintXReport(bool hardcopy)
        {
            cr.State = States.WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_XREPORT_PLEASE_WAIT);
            IPrinterResponse response = cr.Printer.PrintXReport(hardcopy);
            if (!response.HasError || !hardcopy)
               cr.DataConnector.SaveReport("XRAPORU", response.Detail);
            return States.Start.Instance();
        }

        public static IState PrintXPluReport(int firstPLU, int lastPLU, bool hardcopy)
        {
            cr.State = States.WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_XPLU_REPORT_PLEASE_WAIT);
            IPrinterResponse response = cr.Printer.PrintXPluReport(firstPLU, lastPLU, hardcopy);
            if (!response.HasError || !hardcopy)
               cr.DataConnector.SaveReport("XPLURAPORU", response.Detail);
            return States.Start.Instance();
        }

        public static IState PrintZReport()
        {
            IPrinterResponse response = null;
            
            try
            {
                cr.State = States.WaitingState.Instance();
                DisplayAdapter.Cashier.Show(PosMessage.WRITING_Z_REPORT);
                response = cr.Printer.PrintZReport();
                if (!response.HasError)
                    cr.DataConnector.SaveReport("ZRAPORU", response.Detail);
            }
            catch (EJFullException eje) { cr.State = ElectronicJournalError.Instance(eje); }
            finally
            {
                if (cr.CurrentCashier == null && !(cr.State is ElectronicJournalError))
                {
                    cr.State = States.Login.Instance();
                }
            }
            return cr.State;
        }
        
        public static IState PrintPeriodicReportByZNumber(int firstZNumber, int lastZNumber,bool hardcopy)
        {
            IPrinterResponse response = null;
            try
            {
                cr.State = States.WaitingState.Instance();
                DisplayAdapter.Cashier.Show(PosMessage.WRITING_FINANCIAL_Z_REPORT);
                response = cr.Printer.PrintPeriodicReport(firstZNumber, lastZNumber, hardcopy);
                firstZNumber = lastZNumber = 0;
                //if (!response.StatusCode || !hardcopy)
                //    cr.DataConnector.SaveReport("MBRAPORU", response.Detail);
            }
            catch (WrongZCountException ex)
            {
                throw ex;
            }
            if (hardcopy)
                return cr.State;
            return cr.State = States.Start.Instance();
        }
               
        public static IState PrintPeriodicReportByDate(DateTime firstZDate, DateTime lastZDate, bool hardcopy)
        {
            IPrinterResponse response = null;
            try
            {
                if (firstZDate > lastZDate) throw new ParameterException("SON TARiH, iLK\n TARiHTEN KÜÇÜK");
                cr.State = States.WaitingState.Instance();
                DisplayAdapter.Cashier.Show(PosMessage.WRITING_FINANCIAL_Z_REPORT);
                response = cr.Printer.PrintPeriodicReport(firstZDate, lastZDate, hardcopy);
                if (!response.HasError || !hardcopy)
                    cr.DataConnector.SaveReport("MBRAPORU", response.Detail);
            }
            catch (WrongZCountException ex)
            {
                throw ex;
            }
            catch (ParameterException pe)
            {
                if (!hardcopy)
                    AlertCashier.Instance(new Error(pe));
                throw pe;
            }
            if (hardcopy)
                return cr.State;
            return cr.State = States.Start.Instance();
        }
       
        public static IState PrintRegisterReport(bool hardcopy)
        {
            cr.State = States.WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_PAYMENT_REPORT);
            IPrinterResponse response = cr.Printer.PrintRegisterReport(hardcopy);
            if (!response.HasError || !hardcopy)
                cr.DataConnector.SaveReport("ORAPORU", response.Detail);
            return States.Start.Instance();
        }

        public static IState PrintProgramReport(bool hardcopy)
        {
            cr.State = States.WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_PROGRAM_REPORT);
            IPrinterResponse response = cr.Printer.PrintProgramReport(hardcopy);
            if (!response.HasError || !hardcopy)
               cr.DataConnector.SaveReport("PRAPORU", response.Detail);
            return States.Start.Instance();
        }

        internal static IState PrintRegisterReportSummary(ICashier cashier, DateTime startZDate, DateTime endZDate, bool hardcopy)
        {
            if (cr.Printer is IPeriodicCashierReport)
            {
                cr.State = States.WaitingState.Instance();
                DisplayAdapter.Cashier.Show(PosMessage.WRITING_PROGRAM_REPORT);
                IPrinterResponse response = ((IPeriodicCashierReport)cr.Printer).PrintRegisterReportSummary(cashier, startZDate, endZDate, hardcopy);
                if (!response.HasError || !hardcopy)
                    cr.DataConnector.SaveReport("ORAPORU", response.Detail);
            }
            return States.Start.Instance();
        }

        #endregion
        
        #region EJ Reports

        public static IState PrintEJSummary()
        {
            cr.State = States.WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_EJ_SUMMARY_REPORT);
            cr.Printer.PrintEJSummary();
            return Continue();
        }


        public static IState PrintEJDocumentByTime(DateTime documentDate, bool hardcopy)
        {
            cr.State = WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_EJ_DOCUMENT_REPORT);
            IPrinterResponse response = cr.Printer.PrintEJDocument(documentDate, hardcopy);
            cr.DataConnector.SaveReport(String.Format("EKURAPORU{0:ddMMyyyyHHmm}", documentDate), response.Detail);
            return Continue();

        }

        public static IState PrintEJDocumentById(int zno, int docId, bool hardcopy)
        {
            cr.State = WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_EJ_DOCUMENT_REPORT);
            IPrinterResponse response = cr.Printer.PrintEJDocument(zno, docId, hardcopy);
            cr.DataConnector.SaveReport(String.Format("EKURAPORU{0:D4}{1:D4}", zno, docId), response.Detail);
            return Continue();
        }

        public static IState PrintEJZReport(int id)
        {
            cr.State = WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_EJ_DOCUMENT_REPORT);
            cr.Printer.PrintEJZReport(id);
            return Continue();
        }

        public static IState PrintEJPeriodicByZNumber(int firstZNumber, int firstDocId, int lastZNumber, int lastDocId, bool hardcopy)
        {
            cr.State = WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_EJ_PERIODIC_REPORT_BETWEEN_ZNO);
            IPrinterResponse response = cr.Printer.PrintEJPeriodic(firstZNumber, firstDocId, lastZNumber, lastDocId, hardcopy);
            cr.DataConnector.SaveReport(String.Format("EKURAPORU{0:D4}{1:D4}{2:D4}{3:D4}", firstZNumber, firstDocId, lastZNumber, lastDocId), response.Detail);
            return Continue();
        }

        public static IState GetEJDaily(DateTime day, bool hardcopy)
        {
            cr.State = States.WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_EJ_PERIODIC_REPORT);
            IPrinterResponse response = cr.Printer.PrintEJPeriodic(day, hardcopy);
            cr.DataConnector.SaveReport(String.Format("EKURAPORU{0:ddMMyyyy}", day), response.Detail);
            return Continue();
        }

        public static IState PrintEJPeriodicByDate(DateTime firstDate, DateTime lastDate, bool hardcopy)
        {
            cr.State = States.WaitingState.Instance();
            DisplayAdapter.Cashier.Show(PosMessage.WRITING_EJ_PERIODIC_REPORT);
            IPrinterResponse response = cr.Printer.PrintEJPeriodic(firstDate, lastDate, hardcopy);
            cr.DataConnector.SaveReport(String.Format("EKURAPORU{0:ddMMyyyyHHmm}{1:ddMMyyyyHHmm}", firstDate, lastDate), response.Detail);
            return Continue();
        }
        
        #endregion

        #region EJ Limit
        public static IState CheckZLimit(String zlimit)
        {
            try
            {
                //throw new Exception("CheckZLimit()\n NOT IMPLEMENTED");
                //to do
                long newzlimit = 0;

                if (!Parser.TryLong(zlimit.Trim(), out newzlimit))
                    newzlimit = -1;
                cr.Printer.SetZLimit(newzlimit);
                return Continue();
            }
            catch (Exception ex)
            {
                return AlertCashier.Instance(new Error(ex, new StateInstance(Instance)));
            }
        }
        #endregion
    }
}
