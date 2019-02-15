using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;

namespace Hugin.POS.Data
{
    class Cashier:ICashier
    {
        static Dictionary<String, ICashier> cashiersByPassword;
        static Dictionary<String, ICashier> cashiersById;
        static Dictionary<String, ICashier> backupCashiersByPassword;
        static Dictionary<String, ICashier> backupCashiersById;

        private bool valid;
        private string id;
        private string name;
        private string password;
        private AuthorizationLevel authorizationLevel;
        private int percentAdjustmentLimit = 100;
        private Decimal priceAdjustmentLimit = 0;

        #region ICashier Members

        public string Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public AuthorizationLevel AuthorizationLevel
        {
            get { return authorizationLevel; }
        }

        public bool Valid 
        {
            get { return valid; }
        }
        public int PercentAdjustmentLimit
        {
            get { return percentAdjustmentLimit;  }
        }

        public Decimal PriceAdjustmentLimit
        {
            get { return priceAdjustmentLimit; }
        }
        /// <summary>
        /// - Checks cashier discount and fee authorization limit
        /// </summary>
        /// <param name="adjustment">
        /// - Authorization adjustment
        /// </param>
        /// <returns>
        /// - bool value whether cashier has authorization or not.
        /// </returns>
        public bool IsAuthorisedFor(IAdjustment adjustment)
        {
            Decimal alreadyAdjustedPrice = adjustment.Target.TotalAmount - adjustment.Target.ListedAmount;
            Decimal absolutePercentAdjustment = 0;
            bool response = false;

            switch (adjustment.Method)
            {
                case AdjustmentType.Discount:
                    decimal netDiscount = -(alreadyAdjustedPrice + adjustment.NetAmount);
                    absolutePercentAdjustment = alreadyAdjustedPrice * -100m / adjustment.Target.ListedAmount
                                               + adjustment.NetAmount * -100m / adjustment.Target.ListedAmount;
                    response = (netDiscount <= this.priceAdjustmentLimit && absolutePercentAdjustment <= this.percentAdjustmentLimit);
                    break;
                case AdjustmentType.Fee:
                    if (this.priceAdjustmentLimit != 100)
                    {
                        response = (alreadyAdjustedPrice + adjustment.NetAmount <= this.priceAdjustmentLimit)
                                && ((alreadyAdjustedPrice + adjustment.NetAmount) * 100m / adjustment.Target.ListedAmount <= this.percentAdjustmentLimit);
                    }
                    else
                        response = true;
                    break;
                case AdjustmentType.PercentDiscount:
                    response = this.percentAdjustmentLimit >= adjustment.RequestValue;
                    //absolutePercentAdjustment = (alreadyAdjustedPrice * -100m / adjustment.Target.ListedAmount);
                    //response = (absolutePercentAdjustment + adjustment.RequestValue <= this.percentAdjustmentLimit);
                    break;
                case AdjustmentType.PercentFee:

                    if (this.percentAdjustmentLimit != 100)
                    {
                        if (adjustment.Target.ListedAmount == 0)
                            absolutePercentAdjustment = 100;
                        else
                            absolutePercentAdjustment = (alreadyAdjustedPrice * 100m / adjustment.Target.ListedAmount);
                        response = (absolutePercentAdjustment + adjustment.RequestValue <= this.percentAdjustmentLimit);
                    }
                    else
                        response = true;
                    break;
                default:
                    break;
            }
            if (!response)
                EZLogger.Log.Info("Net adjustment of {0:C} on {1} not authorised for Cashier Id: {2}", adjustment.NetAmount,
                                                                                                 adjustment.Target.GetType(),
                                                                                                 this.Id);
            return response;
        }

        public bool GenerateCashierLine(string name, string id, AuthorizationLevel auth, 
                                string passcode,int disPercent,Decimal disAmount,
                                bool valid, out string line)
        {
            bool retVal = true;

            try
            {
                line = String.Format(
                              "{0:D1}{1:D4}{2}{3:-6}{4:1}{5:D2}{6:D10}\r\n",
                              valid ? "1" : "0",
                              id,
                              name.PadRight (20,' '),
                              passcode,
                              (auth == AuthorizationLevel.Seller) ? " " : auth.ToString (),
                              (disPercent == 100) ? 0 : disPercent,
                              (disAmount*100 == Decimal.MaxValue)? 0 : (long)(disAmount*100)
                              );

            }
            catch (Exception exc)
            {
				Log (exc);
                line = "";
                retVal = false;
            }

            return retVal;
        }

        public bool UpdateCashier(string line)
        {
            Cashier ch;

            try
            {
                ch = new Cashier();
                ch.valid = Int32.Parse(line.Substring(0, 1)) == 1;
                ch.id = line.Substring(1, 4);
                if (int.Parse(ch.id) > 9990)
                    throw new DataInvalidException("Geçerli kasiyer no aralýðý 0001-9990. No: " + ch.id);
                ch.name = line.Substring(5, 20);
                ch.password = line.Substring(25, 6);

                if (line[31] == ' ')
                    ch.authorizationLevel = AuthorizationLevel.Seller;
                else
                    ch.authorizationLevel = (AuthorizationLevel)Enum.Parse(typeof(AuthorizationLevel), line[31].ToString(), true);

                if (line.Length < 34)
                {
                    ch.percentAdjustmentLimit = 100;
                }
                else
                {//ch.percentAdjustmentLimit = Int32.Parse(line.Substring(32, 2));
                    int perAdjLimit = 0;
                    if (!Parser.TryInt(line.Substring(32, 2), out perAdjLimit))
                        ch.percentAdjustmentLimit = 0;
                    else
                    {
                        ch.percentAdjustmentLimit = perAdjLimit;
                        if (ch.percentAdjustmentLimit == 0)
                            ch.percentAdjustmentLimit = 100;
                    }
                }
                if (line.Length < 44)
                {
                    ch.priceAdjustmentLimit = 0.0m;
                }
                else
                {
                    Decimal prcAdjLimit = 0.0m;
                    if (!Parser.TryDecimal(line.Substring(34, 10), out prcAdjLimit))
                    {
                        ch.priceAdjustmentLimit = 0.0m;
                    }
                    else
                    {
                        if (prcAdjLimit == 0)
                            prcAdjLimit = Decimal.MaxValue;
                        ch.priceAdjustmentLimit = prcAdjLimit / 100m;
                    }
                }

                //try to remove
                if (cashiersById.ContainsKey(ch.Id) && cashiersByPassword.ContainsKey(ch.Password))
                {
                    cashiersById.Remove(ch.Id);
                    cashiersByPassword.Remove(ch.Password);
                }

                cashiersById.Add(ch.id, ch);
                cashiersByPassword.Add(ch.password, ch);

                //update .dat file
                return updateLine(line, false);

            }
            catch (Exception e)
			{
				Log (e);
                return false;
            }

        }

        public bool DeleteCashier(ICashier ch)
        {
            bool retVal = true;

            if (cashiersById.ContainsKey(ch.Id) && cashiersByPassword.ContainsKey(ch.Password))
            {
                cashiersById.Remove(ch.Id);
                cashiersByPassword.Remove(ch.Password);


                string strId = String.Format (" {0:D4}",ch.Id);
                retVal = updateLine(strId, true);
            }
            else 
                retVal = false;


            return retVal;   
        }

        #endregion

        private bool updateLine(string lineToWrite, bool remv)
        {
            bool retVal = true;
            bool idMatch = false;
            int cashierId;
     
    
            if (Parser.TryInt (lineToWrite.Substring(1, 4), out cashierId) == false)
                return false;

            string writePath = PosConfiguration.DataPath + "Temp" + Settings.CashierFile;
            string readPath = PosConfiguration.DataPath + Settings.CashierFile;

            if (File.Exists(writePath))
                File.Delete(writePath);

            StreamReader sr = new StreamReader(readPath, PosConfiguration.DefaultEncoding);

            try
            {
                StreamWriter sw = new StreamWriter(writePath, false, PosConfiguration.DefaultEncoding);


                string line = "";
                Int32 id;
                while ((line = @sr.ReadLine()) != null)
                {
                    //skip trailing blank lines and comments
                    if (line.Trim().Length == 0 || line[0] == '0') continue;

                    //parse line
                    if (Parser.TryInt(line.Substring(1, 4), out id))
                    {
                        if (id == cashierId)
                        {
                            idMatch = true;

                            //removing
                            if (remv) continue;
                            else
                            {
                                //updating
                                line = lineToWrite;
                            }
                        }

                    }

                    sw.WriteLine(line);

                }
                /*
                 * We couldn't find matching ID.
                 * Since we only updating one line on whole file
                 * It's because mathing ID is deleted and is to add new
                 * Cashier.
                 * New cashier line is added to end of the file
                 */
                if (!idMatch)
                    sw.WriteLine(lineToWrite);
       
                

                sw.Close();

            }
            catch (Exception exc)
            {
				Log (exc);
            }

            sr.Close();

         
            File.Delete(readPath);
            File.Move(writePath, readPath);
            File.Delete(writePath);
                
            //inform Cashiers File updated?
            
            return retVal;

        }


        internal static ICashier FindByPassword(String password)
        {
            if (cashiersByPassword.ContainsKey(password))
                return cashiersByPassword[password];
            return null;
        }

        internal static ICashier FindById(String id)
        {
            if (cashiersById.ContainsKey(id))
                return cashiersById[id];
            return null;
        }

        internal static void Backup()
        {
            backupCashiersById = cashiersById;
            backupCashiersByPassword = cashiersByPassword;

            cashiersById = new Dictionary<string, ICashier>();
            cashiersByPassword = new Dictionary<string, ICashier>();
        }

        internal static void Restore()
        {
            if (cashiersById.Count == 0)
            {
                if (backupCashiersById == null || backupCashiersById.Count == 0)
                    return;//if no cashier exists then program is invalid, this can be controlled by looking success count
                cashiersById = backupCashiersById;
                cashiersByPassword = backupCashiersByPassword;
            }

            if (backupCashiersById != null)
                backupCashiersById.Clear();
            if (backupCashiersByPassword != null)
                backupCashiersByPassword.Clear();
        }

        internal static bool Add(string line)
        {
            try
            {
                Cashier ch = new Cashier();
                ch.valid = Int32.Parse(line.Substring(0, 1)) == 1;                
                ch.id = line.Substring(1, 4);
                if (int.Parse(ch.id) > 9990)
                    throw new DataInvalidException("Geçerli kasiyer no aralýðý 0001-9990. No: " + ch.id);
                ch.name = line.Substring(5, 20);
                ch.password = line.Substring(25, 6);

                if (line[31] == ' ')
                    ch.authorizationLevel = AuthorizationLevel.Seller;
                else
                    ch.authorizationLevel = (AuthorizationLevel)Enum.Parse(typeof(AuthorizationLevel), line[31].ToString(), true);

                if (line.Length > 34)
                {
                    int perAdjLimit = 0;
                    if (!Parser.TryInt(line.Substring(32, 2), out perAdjLimit))
                        ch.percentAdjustmentLimit = 0;
                    else
                    {
                        ch.percentAdjustmentLimit = perAdjLimit;
                    }
                }
                if (line.Length < 44)
                {
                    ch.priceAdjustmentLimit = 0.0m;
                }
                else
                {
                    Decimal prcAdjLimit = 0.0m;
                    if (!Parser.TryDecimal(line.Substring(34, 10), out prcAdjLimit))
                    {
                        ch.priceAdjustmentLimit = 0.0m;
                    }
                    else
                    {
                        ch.priceAdjustmentLimit = prcAdjLimit / 100m;
                    }
                }

                if (cashiersById.ContainsKey(ch.id))
                    throw new DataInvalidException("Þifre daha önce tanýmlanmýþ. ÞiFRE: " + ch.id);

                if (cashiersByPassword.ContainsKey(ch.password))
                    throw new DataInvalidException("Id daha önce tanýmlanmýþ. ÞiFRE: " + ch.password);

                cashiersById.Add(ch.id, ch);
                cashiersByPassword.Add(ch.password, ch);

                return true;
            }
            catch (Exception e)
            {
                PosException lastException = new PosException(e.Message, e);
                lastException.Data.Add("ErrorLine", line);
                EZLogger.Log.Warning(lastException);
                return false;
            }
        }

       
        internal static ICashier CreateCashier(string name, string id)
        {
            Cashier ch = new Cashier();
            ch.name = name;
            ch.id = id;

            if (name.EndsWith("DYNAMIC"))
            {

                int index = ch.name.LastIndexOf("DYNAMIC");
                ch.name = ch.name.Substring(0, index);

                try
                {
                    foreach(ICashier csh in cashiersById.Values)
                    {
                        if(csh.Name == ch.name)
                        {
                            return csh;
                        }
                    }
                }
                catch
                {

                }
                
 
                int pswd = 200200;
                while (true)
                {
                    if (!cashiersByPassword.ContainsKey(pswd.ToString()))
                    {
                        break;
                    }
                    pswd++;
                }

                ch.password = pswd.ToString();
                ch.valid = true;
                ch.authorizationLevel = AuthorizationLevel.Z;

                /*
                 * do not need to save product in a file
                 * because it will be used dynamically                
                 */
                cashiersById.Add(ch.id, ch);
                cashiersByPassword.Add(ch.password, ch);

            }
            else if (name.EndsWith("FPU"))
            {
                name = name.Remove(name.Length - 4);
            }
            return ch;

        }
		private void Log (Exception ex)
		{
			/*
			 * this function is implemented to catch warnings
			 * which caused fraom unreferenced "ex" variables
			 */
		}

        internal static List<ICashier> SearchCustomers(string info)
        {
            List<ICashier> cashierList = new List<ICashier>();
            if (cashiersById != null)
            {
                foreach (ICashier c in cashiersById.Values)
                {
                    if (c.Name.Contains(info))
                    {
                        cashierList.Add(c);
                    }
                    else if (c.Id == info)
                    {
                        cashierList.Add(c);
                    }
                }
            }
            return cashierList;
        }
    }
}
