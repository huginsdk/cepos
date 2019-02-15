using System;
using System.Collections;
using System.IO;
using cr = Hugin.POS.CashRegister;
using Hugin.POS.Common;
using System.Collections.Generic;

namespace Hugin.POS.States
{

  class ServiceMenu : List
  {
      private static IState state = new ServiceMenu();
      private static string lastPassword = "";
      private static string ipAddress = "";
      private static int defPort = 0;
      private static String logoLine = "";
      private static String vatIndex = "";

      private static String[] printerLogo = null;
      private static decimal[] printerTaxRates = null;

      public static new IState Instance() {
          lastPassword = "";

          String orderNum = cr.Printer.GetOrderNum();
          String tmp = String.Format("(ÝK:{0})", orderNum);

          return EnterPassword.Instance(PosMessage.SERVICE_PASSWORD + tmp,
                                      new StateInstance<String>(CheckPassword),
                                      new StateInstance(PrinterBlockingError.Instance));
      }

      public static IState CheckPassword(String password)
      {
          if (password.Length < 1 || password.Length > cr.MAX_CASHIER_PASSWOR_LENGTH)
          {
              Confirm wrongPassword = new Confirm(PosMessage.SERVICE_PASSWORD_INVALID, new StateInstance(Instance));
              return AlertCashier.Instance(wrongPassword);
          }
          lastPassword = password;
          return ShowMenu();
      }

      public static IState ShowMenu()
      {         
          try
          {
              cr.Printer.EnterServiceMode(lastPassword);
          }
          catch (CmdSequenceException ex)
          {
              cr.Log.Error("CmdSequenceException occured. {0}",ex);
          }
          catch (MissingCashierException mce)
          {
              cr.Log.Error("MissingCashierException occured. {0}", mce);
              Confirm wrongPassword = new Confirm(PosMessage.SERVICE_PASSWORD_INVALID, 
                                        new StateInstance(Instance));
              return AlertCashier.Instance(wrongPassword);
          }
          catch (SVCPasswordOrPointException ex)
          {
              cr.Log.Error("SVCPasswordOrPointException occured. {0}",ex);
              return ConfirmCashier.Instance(new Confirm(PosMessage.ATTACH_JUMPER_AND_TRY_AGAIN,
                                                       new StateInstance(ShowMenu),
                                                       new StateInstance(ShowMenu)));
          }
          catch (BlockingException ex)
          {
              cr.Log.Error("BlockingException occured. {0}",ex);
              return ConfirmCashier.Instance(new Confirm(PosMessage.ATTACH_JUMPER_AND_RESTART_FPU,
                                             new StateInstance(ShowMenu),
                                             new StateInstance(ShowMenu)));
          }


          ReturnCancel = new StateInstance(Login.Instance);
          MenuList menuHeaders = new MenuList();
          int index = 1;
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.SERVICE, index++, PosMessage.MENU_LOGO)));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.SERVICE, index++, PosMessage.MENU_VAT_RATES)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", 
              PosMessage.SERVICE, index++, PosMessage.MENU_DAILY_MEMORY_FORMAT)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", 
              PosMessage.SERVICE, index++, PosMessage.MENU_DATE_AND_TIME)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.SERVICE, index++, PosMessage.MENU_CREATE_DB)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.SERVICE, index++, PosMessage.MENU_PRINT_LOGS)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}", 
              PosMessage.SERVICE, index++, PosMessage.MENU_FACTORY_SETTING)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.SERVICE, index++, PosMessage.MENU_CLOSE_FM)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.SERVICE, index++, PosMessage.MENU_EXTERNAL_DEV_SETTINGS)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.SERVICE, index++, PosMessage.MENU_UPDATE_FIRMWARE)));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.SERVICE, index++, PosMessage.MENU_START_FM_TEST)));
          menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.SERVICE, index++, PosMessage.MENU_EXIT_SERVICE)));
          
          

          List.Instance(menuHeaders);
          return state;
      }

      public static IState Continue()
      {
          ie.MovePrevious();
          List.Instance(ie);
          return state;
      }
      
      public override void Command()
      {
          cr.State = ShowMenu();
      }

      public override void Enter()
      {         
          String msg = ((MenuLabel)ie.Current).ToString();          

          if (msg.IndexOf("\n") > -1)
          {
              msg = msg.Substring(msg.IndexOf("\n") + 1);
          }

          switch (msg)
          {
              case PosMessage.MENU_DAILY_MEMORY_FORMAT:
                  cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.PROMPT_DAILY_MEMORY_FORMAT,
                                                               new StateInstance(FormatMemory),
                                                               new StateInstance(Continue)));
                  break;
              case PosMessage.MENU_DATE_AND_TIME:
                  cr.State = EnterString.Instance("FORMAT: yyyyMMddHHmm",
                                                  DateTime.Now.ToString("yyyyMMddHHmm"),
                                                  new StateInstance<String>(SetDateAndTime),
                                                  new StateInstance(Continue));
                  break;
              case PosMessage.MENU_FACTORY_SETTING:
                  cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.LOAD_FACTOR_SETTINGS,
                                                               new StateInstance(LoadFactorySettings),
                                                               new StateInstance(Continue)));
                  break;            
              case PosMessage.MENU_CLOSE_FM:
                  cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.CLOSE_FISCAL_MEMORY,
                                                               new StateInstance(CloseFiscalMemory),
                                                               new StateInstance(Continue)));
                  break;
              case PosMessage.MENU_START_FM_TEST:
                  cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.START_FM_TEST,
                                                               new StateInstance(StartFMTest),
                                                               new StateInstance(Continue)));
                  break;
              case PosMessage.MENU_EXTERNAL_DEV_SETTINGS:
                  defPort = 4444;
                  cr.State = EnterIP.Instance(PosMessage.TCP_IP_ADDRESS,
                                                   GetIPAddress(),
                                                   new StateInstance<String>(SetIPAddress),
                                                   new StateInstance(Continue));
                  break;

              case PosMessage.MENU_LOGO:
                  cr.State = ShowLogoMenu();
                  break;
              case PosMessage.MENU_VAT_RATES:
                  cr.State = ShowTaxRatesMenu();
                  break;
              case PosMessage.MENU_PRINT_LOGS:
                  cr.State = EnterString.Instance("FORMAT: ddMMyyyy",
                                                  DateTime.Now.ToString("ddMMyyyy"),
                                                  new StateInstance<String>(PrintLogs),
                                                   new StateInstance(Continue));
                  break;
              case PosMessage.MENU_CREATE_DB:
                  cr.State = ConfirmCashier.Instance(new Confirm(PosMessage.CREATE_DB,
                                                        new StateInstance(CreateDB),
                                                        new StateInstance(Continue)));
                  break;
              case PosMessage.MENU_UPDATE_FIRMWARE:
                  cr.State = EnterIP.Instance(PosMessage.TCP_IP_ADDRESS,
                                                   GetIPAddress(),
                                                   new StateInstance<String>(SetIPAddress),
                                                   new StateInstance(Continue));
                  break;
              case PosMessage.MENU_EXIT_SERVICE:
                  lastPassword = "";
                  cr.State = EnterPassword.Instance(PosMessage.SERVICE_PASSWORD,
                                      new StateInstance<String>(ExitService),
                                      new StateInstance(PrinterBlockingError.Instance));
                  break;
          }

      }
      private string GetIPAddress()
      {
          System.Net.IPHostEntry host;
          string localIP = "?";
          host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
          foreach (System.Net.IPAddress ip in host.AddressList)
          {
              if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
              {
                  localIP = ip.ToString();
              }
          }
          return localIP;
      }
      public override void Escape()
      {
          lastPassword = "";
          cr.State = EnterPassword.Instance(PosMessage.SERVICE_PASSWORD,
                                      new StateInstance<String>(ExitService),
                                      new StateInstance(PrinterBlockingError.Instance));                    
      }

      private static IState ExitService(String password)
      {
          try
          {
              cr.Printer.ExitServiceMode(password);

              cr.SetPrinterPort(PosConfiguration.Get("PrinterComPort"));
              CashRegister.LoadCurrentSettings();
              cr.State = States.Start.Instance();
          }
          catch (CashierAutorizeException cae)
          {
              cr.State = AlertCashier.Instance(new Error(cae,
                                       new StateInstance(Continue),
                                       new StateInstance(Continue)));
              cr.Log.Error("CashierAutorizeException occured. {0}", cae.Message);
          }
          catch (CmdSequenceException ex)
          {
              cr.State = AlertCashier.Instance(new Error(ex,
                                       new StateInstance(Continue),
                                       new StateInstance(Continue)));
              cr.Log.Error("CmdSequenceException occured. {0}", ex.Message);
          }
          catch (SVCPasswordOrPointException ex)
          {
              cr.State = ConfirmCashier.Instance(new Error(ex,
                                        new StateInstance(Continue),
                                        new StateInstance(Continue)));
              cr.Log.Error("SVCPasswordOrPointException occured. {0}", ex);
          }

          return cr.State;
      }

      #region Date and Time
      public static IState SetDateAndTime(String dateTime)
      {
          Confirm err = new Confirm(PosMessage.INVALID_ENTRY,
                                new StateInstance(Continue),
                                new StateInstance(Continue));

          if (dateTime.Length != 12)
          {
              return AlertCashier.Instance(err);
          }
          else
          {
              try
              {
                  cr.Printer.DateTime = DateTime.ParseExact(dateTime, "yyyyMMddHHmm", new System.Globalization.CultureInfo("en-US", true));
              }
              catch (Exception ex)
              {
                  cr.Log.Error("Exception occured.", ex);
                  return AlertCashier.Instance(new Error(ex,
                                             new StateInstance(Continue),
                                             new StateInstance(Continue)));
              }
          }
          return Continue();

      }
     #endregion

      #region Daily memory format
       public static IState FormatMemory() 
      {
          try
          {
            cr.Printer.FormatMemory();
            if (!cr.Document.IsEmpty)
            {
                cr.Document.Cancel();
                cr.Document = new Receipt();
            }
          }
          catch (Exception ex)
          {
              cr.Log.Error("Exception occured. {0}",ex);
          }

         return Continue();
         
      }     
      #endregion
      #region Load factory settings
      public static IState LoadFactorySettings()
      {
          try
          {
              cr.Printer.FactorySettings();
          }
          catch (System.Exception ex)
          {
              cr.Log.Error("Exception occured. {0}", ex);
          }
          return Continue();
      }
      #endregion

      #region Close Fiscal Memory
      public static IState CloseFiscalMemory()
      {
          try
          {
              cr.Printer.CloseFM();
          }
          catch (System.Exception ex)
          {
              cr.Log.Error("Exception occured. {0}", ex);
          }
          return Continue();
      }
      #endregion

      #region Set External TCP/IP 
      public static IState SetIPAddress(string ip)
      {

          ipAddress = ip;
          cr.State = EnterInteger.Instance(PosMessage.PORT,
                                                        defPort,
                                                        new StateInstance<int>(SetPort),
                                                        new StateInstance(Continue));

          return cr.State;
      }

      public static IState SetPort(int port)
      {
          try
          {
              String msg = ((MenuLabel)ie.Current).ToString();
              if (msg.IndexOf("\n") > -1)
                  msg = msg.Substring(msg.IndexOf("\n") + 1);
              switch (msg)
              {
                  case PosMessage.MENU_EXTERNAL_DEV_SETTINGS:
                      cr.Printer.SetExDeviceAddress(ipAddress, port);
                      break;
                  case PosMessage.MENU_UPDATE_FIRMWARE:
                      cr.Printer.UpdateFirmware(ipAddress, port);
                      break;
              }
          }
          catch (System.Exception ex)
          {
              cr.Log.Error("Exception occured. {0}", ex);
          }
          return Continue();
      }
      #endregion

      #region LOGO

      public static IState ShowLogoMenu()
      {

          MenuList logoMenu = new MenuList();
          if (printerLogo == null)
              printerLogo = cr.Printer.Logo;
          int index = 1;

          logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.LOGO_LINE, index++, printerLogo[0])));
          logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.LOGO_LINE, index++, printerLogo[1])));
          logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.LOGO_LINE, index++, printerLogo[2])));
          logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.LOGO_LINE, index++, printerLogo[3])));
          logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.LOGO_LINE, index++, printerLogo[4])));
          logoMenu.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
              PosMessage.LOGO_LINE, index++, printerLogo[5])));

          //return List.Instance(logoMenu, null , new StateInstance(ShowLogoMenu));
          return ListCommandMenu.Instance(logoMenu, new ProcessSelectedItem<MenuLabel>(LogoMenuAction), ShowMenu);
          //return state;
 
      }

      private static void LogoMenuAction(Object menu)
      {
          String msg = ((MenuLabel)menu).ToString();

          logoLine = (msg.Substring(msg.IndexOf("\t") + 1, 2)).Trim();

          EnterString.SetMaxLength(48);
          cr.State = EnterString.Instance( "", PosMessage.LOGO_LINE + "\t " + logoLine + "\n" +
                                            printerLogo[int.Parse(logoLine) - 1],
                                            new StateInstance<String>(SetLogo),
                                            new StateInstance(Continue));

      }

      public static IState SetLogo(String logo)
      {
          if (printerLogo == null)
              printerLogo = cr.Printer.Logo;
          string[] tempArray = printerLogo;

          Confirm err = new Confirm(PosMessage.INVALID_ENTRY,
                                         new StateInstance(Continue),
                                         new StateInstance(Continue));

          if (logo.Length > 48)
          {
              return AlertCashier.Instance(err);
          }
          else
          {
              try
              {
                  tempArray[int.Parse(logoLine) - 1] = logo;
                  cr.Printer.Logo= tempArray;
              }
              catch (Exception ex)
              {
                  cr.Log.Error("Exception occured.", ex);
                  return AlertCashier.Instance(new Error(ex,
                                             new StateInstance(ShowLogoMenu),
                                             new StateInstance(ShowLogoMenu)));
              }
          }
          return ShowLogoMenu();
      }

      #endregion

      #region TAX_RATES

      public static IState ShowTaxRatesMenu()
      {
          printerTaxRates = cr.Printer.TaxRates;
          
          MenuList menuHeaders = new MenuList();
          int index = 1;

          for (index = 1; index <= printerTaxRates.Length; index++)
          {
              if (printerTaxRates[index - 1] == decimal.MinusOne)
              {
                  menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                  PosMessage.VAT_RATE, index, "TANIMSIZ")));
              }
              else
              {
                  menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
                  PosMessage.VAT_RATE, index, printerTaxRates[index - 1])));
              }
          }

          //    menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //        PosMessage.VAT_RATE, index++, printerTaxRates[index - 2])));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.VAT_RATE, index++, printerTaxRates[index - 2])));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.VAT_RATE, index++, printerTaxRates[index - 2])));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.VAT_RATE, index++, printerTaxRates[index - 2])));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.VAT_RATE, index++, printerTaxRates[index - 2])));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.VAT_RATE, index++, printerTaxRates[index - 2])));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.VAT_RATE, index++, printerTaxRates[index - 2])));
          //menuHeaders.Add(new MenuLabel(String.Format("{0}\t{1,2}\n{2}",
          //    PosMessage.VAT_RATE, index++, printerTaxRates[index - 2])));

          //List.Instance(menuHeaders);
          return ListCommandMenu.Instance(menuHeaders, new ProcessSelectedItem<MenuLabel>(TaxRatesMenuAction), ShowMenu);

          //return state;
      }

      private static void TaxRatesMenuAction(Object menu)
      {
          String msg = ((MenuLabel)menu).ToString();          

          vatIndex = (msg.Substring(msg.IndexOf("\t") + 1, 2)).Trim();

          cr.State = EnterDecimal.Instance(PosMessage.VAT_RATE + "\t " + vatIndex,
                                                    printerTaxRates[int.Parse(vatIndex) - 1],
                                                    new StateInstance<decimal>(SetTaxRates),
                                                    new StateInstance(Continue));
      }

      public static IState SetTaxRates(decimal taxRate)
      {
          decimal[] tempArray = cr.Printer.TaxRates;

          Confirm err = new Confirm(PosMessage.INVALID_ENTRY,
                                         new StateInstance(Continue),
                                         new StateInstance(Continue));

          if (taxRate > 100)
          {
              return AlertCashier.Instance(err);
          }
          else
          {
              try
              {
                  tempArray[int.Parse(vatIndex) - 1] = taxRate;
                  cr.Printer.TaxRates = tempArray;
              }
              catch (Exception ex)
              {
                  cr.Log.Error("Exception occured.", ex);
                  return AlertCashier.Instance(new Error(ex,
                                             new StateInstance(ShowTaxRatesMenu),
                                             new StateInstance(ShowTaxRatesMenu)));
              }
          }
          cr.State = ShowTaxRatesMenu();
          return cr.State;
      }

      #endregion

      public static IState PrintLogs(String dateTime)
      {
          try
          {
              cr.Printer.PrintLogs(dateTime);
          }
          catch (System.Exception ex)
          {
              cr.Log.Error("Exception occured. {0}", ex);
          }

          return Continue();
      }

      public static IState CreateDB()
      {
          try
          {
              cr.Printer.CreateDB();
          }
          catch (System.Exception ex)
          {
              cr.Log.Error("Exception occured. {0}", ex);
          }

          return Continue();
      }

      private static IState StartFMTest()
      {
          try
          {
              cr.Printer.StartFMTest();
          }
          catch (Exception ex)
          {
              cr.Log.Error("Exception occured. {0}", ex);
              throw ex;
          }

          return Continue();
      }
  }
}
