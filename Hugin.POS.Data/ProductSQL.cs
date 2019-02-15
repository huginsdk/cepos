using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;
using System.Globalization;
using System.Data;

namespace Hugin.POS.Data
{
    class Product : IProduct
    {

        internal const string ProductTable = "Products";
        internal const string TempProductTable = "TempProducts"; 

        const long MAX_UNITPRICE = 999999999;
        internal static Decimal MIN_UNITPRICE = 999999999;
        string category;
        bool valid;//is product valid?
        int id;//plu number
        string barcode;
        string name;
        Department department;
        decimal unitPrice = 0.00m;
        decimal secondaryUnitPrice = 0.00m;
        string unit;
        decimal quantity = 1;
        ProductStatus status = ProductStatus.None;
        ProductRequiredField requiredFields = ProductRequiredField.None;

        private static bool inUpdateState = false;

        internal static Boolean departmentNameOnly = false;

        #region IProduct Members

        public bool Valid
        {
            get { return valid; }
        }

        public int Id
        {
            get { return id; }
        }

        public String Barcode
        {
            get { return barcode; }
        }

        public String Category
        {
            get { return category; }
        }

        public String Name
        {
            get { return name; }
        }

        public Department Department
        {
            get { return department; }
        }

        public Decimal UnitPrice
        {
            get { return unitPrice; }
        }

        public Decimal SecondaryUnitPrice
        {
            get { return secondaryUnitPrice; }
        }

        public String Unit
        {
            get { return unit; }
        }

        public Decimal Quantity
        {
            get { return quantity; }
        }

        public ProductStatus Status
        {
            get { return status; }
        }

        public ProductRequiredField RequiredField
        {
            get { return requiredFields; }
        }

        public IProduct Clone()
        {
            return (Product)this.MemberwiseClone();
        }


        public bool GenerateProductLine(bool valid, int id, string barcode,
                                            string name, decimal unitPrice, Department dept,
                                            string unit, bool isWeighable, bool isProgrammable, bool requiredSerialNo,
                                            string category, decimal secondaryUnitPrice, out string line)
        {
            bool retVal = true;

            try
            {
                line = String.Format(
                              "{0:D1}{1:D6}{2,-20}{3,-20}{4:D9}{5:D2}{6,-4}{7,1}{8:D6}{9:D10}\r\n",
                              valid ? "1" : "0",
                              id,
                              barcode,
                              name,
                              (long)(unitPrice * 100),
                              dept.Id,
                              unit,
                              isWeighable ? "E" : isProgrammable ? "P" : requiredSerialNo ? "S" : "H",
                              (category == null) ? 0 : int.Parse(category),
                              (long)(100 * secondaryUnitPrice)
                              );
            }
            catch (Exception exc)
            {
                line = "";
                retVal = false;
            }

            return retVal;
        }


        public bool UpdateProduct(string line)
        {
            Product p = LineToProduct(line);

            return Add(p);
        }

        public bool DeleteProduct(IProduct p)
        {
            try
            {
                String strQuery = "";
                strQuery = String.Format("DELETE FROM {0} WHERE ID ='{0}'", p.Id);
                DBAdapter.Instance().ExecuteNonQuery(strQuery);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj is Product && ((Product)obj).Id == this.Id) return true;
            return false;

        }

        internal static void Backup()
        {

        }
        internal static void Restore()
        {
        }
        internal static Product LineToProduct(string line)
        {
            Product p = new Product();
            try
            {
                int nameLength = 20;
#if WindowsCE
                nameLength = 19;
#endif
                p.valid = Int32.Parse(line.Substring(0, 1)) == 1;
                p.id = Int32.Parse(line.Substring(1, 6));
                p.barcode = line.Substring(7, 20).Trim().ToUpper();
                p.name = line.Substring(27, nameLength).ToUpper();
                p.unitPrice = Decimal.Parse(line.Substring(47, 9)) / 100;
                if (p.unitPrice > MAX_UNITPRICE)
                    throw new Exception("Birim fiyat " + MAX_UNITPRICE + "'dan büyük olamaz");
                if (p.unitPrice < MIN_UNITPRICE)
                    throw new Exception("Birim fiyat " + MIN_UNITPRICE + "'dan küçük olamaz");
                int departmentNo = Int32.Parse(line.Substring(56, 2));
                if (Department.Departments[departmentNo - 1] == null)
                    throw new DataInvalidException("Gecersiz departman");
                p.department=Department.Departments[departmentNo - 1];

                if (!p.Department.Valid)
                    throw new DataInvalidException("Departman aktif deðil");

#if WindowsCE
                p.unit = line.Substring(58, 2);
#else
                p.unit = line.Substring(58, 4);
#endif

                ((Product)p).SetProperties(line[62]);

                //category is optional
                if (line.Trim().Length < 69 || line.Substring(63, 6).Trim() == String.Empty)
                    p.category = "0";
                else
                    p.category = line.Substring(63, 6);

                //2. unit price is optional
                if (line.Trim().Length < 79 || line.Substring(69, 10).Trim() == String.Empty)
                    p.secondaryUnitPrice = p.unitPrice;
                else
                    p.secondaryUnitPrice = Decimal.Parse(line.Substring(69, 10)) / 100;
                
                return p;
            }
            catch (Exception e)
            {
                PosException lastException = new PosException(e.Message, e);
                lastException.Data.Add("ErrorLine", line);
                EZLogger.Log.Warning(lastException);
                return new Product();
            }
        }

        private void SetProperties(char code)
        {
            switch (code)
            {
                case 'E':
                    this.status = ProductStatus.Weighable;
                    break;
                case 'H':
                    this.status = ProductStatus.None;
                    break;
                case 'P':
                    this.status = ProductStatus.Programmable;
                    break;
                case 'S':
                    this.requiredFields = ProductRequiredField.SerialNumber;
                    break;
                case 'D':
                    this.requiredFields = ProductRequiredField.All;
                    break;
            }
        }
        private static bool Add(Product p)
        {
            try
            {
                string strQuery = "";
                string tblName = inUpdateState ? TempProductTable : ProductTable;
                IProduct tempProduct = null;
                
                try
                {
                    tempProduct = FindByLabel(p.id.ToString());
                }
                catch (System.Exception ex)
                {

                }

                CultureInfo ci = CultureInfo.GetCultureInfo("en-US");

                if (tempProduct == null)
                {
                    strQuery += "INSERT INTO " + tblName;
                    strQuery += " (ID,Name,Barcode,DepartmentId,UnitPrice,SecondaryUnitPrice,Unit,ExtraProperty,Valid,CategoryId)";
                    strQuery += " VALUES ";
                    strQuery += String.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",
                                                            p.id,
                                                            p.name.TrimEnd(new char[] { ' ' }),
                                                            p.barcode,
                                                            p.department.Id,
                                                            p.unitPrice.ToString("f",ci),
                                                            p.secondaryUnitPrice.ToString("f", ci),
                                                            p.Unit,
                                                            GetAdditionalProperty(p),
                                                            p.valid,
                                                            p.category
                                                           );
                }
                else
                {
                    strQuery += "UPDATE " + tblName;
                    strQuery += String.Format(" SET Name = '{0}', Barcode = '{1}', DepartmentId = '{2}',UnitPrice = '{3}'," +
                                "SecondaryUnitPrice = '{4}', Unit = '{5}', ExtraProperty = '{6}'," +
                                "Valid = '{7}', CategoryId = '{8}'",
                                                            p.name.TrimEnd(new char[] { ' ' }),
                                                            p.barcode,
                                                            p.department.Id,
                                                            p.unitPrice.ToString("f", ci),
                                                            p.secondaryUnitPrice.ToString("f", ci),
                                                            p.Unit,
                                                            GetAdditionalProperty(p),
                                                            p.valid,
                                                            p.category
                                                           );

                    strQuery += String.Format(" WHERE ID = '{0}'", p.id);

                }


                DataSet ds = DBAdapter.Instance().GetDataSet(strQuery);

                return DBAdapter.Instance().ExecuteNonQuery(strQuery) > 0;
            }
            catch
            {
                EZLogger.Log.Error("Ürün eklenemedi : ", p.ToString());
            }
            return false;
        }

        internal static IProduct FindByName(String nameQry)
        {
            IProduct p = SelectProduct("Name = '{0}'", nameQry);
            if (p == null)
                throw new Exception(PosMessage.PRODUCT_NOTFOUND);

            return p;
        }

        internal static IProduct FindByBarcode(String barcodeQry)
        {
            IProduct p = SelectProduct("Barcode = '{0}'", barcodeQry);
            if (p == null)
                throw new Exception(PosMessage.BARCODE_NOTFOUND);

            return p;
        }

        internal static IProduct FindByLabel(String pluQry)
        {
            int pluNo = 0;
            Parser.TryInt(pluQry, out pluNo);
            IProduct p = SelectProduct("ID = '{0}'", pluQry);
            if (p == null)
                throw new Exception(PosMessage.PLU_NOTFOUND);

            return p;
        }


        private static IProduct SelectProduct(string condition, params object[] args)
        {
            Product c = null;
            String strQuery = "";

            strQuery = "SELECT * FROM " + ProductTable;
            if (!String.IsNullOrEmpty(condition))
            {
                strQuery += " WHERE " + String.Format(condition, args);
            }

            DataSet ds = DBAdapter.Instance().GetDataSet(strQuery);
            if (ds.Tables[0].Rows.Count > 0)
                c = Parse(ds.Tables[0].Rows[0]);
            return c;
        }

        private static Product Parse(DataRow dataRow)
        {
            Product p = new Product();
            
            
            p.id = Convert.ToInt32(dataRow["ID"]);
            p.name = Convert.ToString(dataRow["Name"]);
            p.barcode = Convert.ToString(dataRow["Barcode"]);
            p.department = Connector.Instance().CurrentSettings.Departments[Convert.ToInt32(dataRow["DepartmentId"])];
            p.unitPrice = Decimal.Parse(dataRow["UnitPrice"].ToString());
            p.secondaryUnitPrice = Convert.ToDecimal(dataRow["SecondaryUnitPrice"]);
            p.unit = Convert.ToString(dataRow["Unit"]);
            p.valid = Convert.ToBoolean(dataRow["Valid"]);
            p.category = Convert.ToString(dataRow["CategoryId"]);

            ((Product)p).SetProperties(Convert.ToString(dataRow["ExtraProperty"])[0]);

            return p;
        }

        private static char GetAdditionalProperty(Product p)
        {
            char code = 'H';

            if ((p.requiredFields & ProductRequiredField.All) == ProductRequiredField.All)
            {
                // It means Product is drug and it has all fields.
                code = 'D';
            }
            else if ((p.requiredFields & ProductRequiredField.SerialNumber) == ProductRequiredField.SerialNumber)
            {
                // Product needs to serial number
                code = 'S';
            }
            else if (p.status == ProductStatus.Weighable)
            {
                code = 'E';
            }

            return code;
        }

        internal static List<IProduct> SearchProductByBarcode(String[] barcodeList)
        {
            List<IProduct> productList = new List<IProduct>();

            foreach (String qry in barcodeList)
            {
                try
                {
                    productList.Add(FindByBarcode(qry));
                }
                catch
                {
                }
            }
            return productList;
        }

        internal static List<IProduct> SearchProductByLabel(String[] pluList)
        {
            List<IProduct> productList = new List<IProduct>();
            foreach (String strPlu in pluList)
            {
                try
                {
                    productList.Add(FindByLabel(strPlu));
                }
                catch
                {
                }
            }
            return productList;
        }

        internal static List<IProduct> SearchProductByName(String nameData)
        {
            List<IProduct> productList = new List<IProduct>();
            string strQuery = "";

            strQuery = String.Format("SELECT * FROM {0} ", ProductTable);
            strQuery += String.Format("WHERE Name Like '%{0}%'", nameData);

            try
            {
                DataSet ds = DBAdapter.Instance().GetDataSet(strQuery);
                foreach (DataRow row in ds.Tables[0].Rows)
                    productList.Add(Parse(row));
            }
            catch
            {
                // do nothing
            }
            return productList;
        }

        internal static void ProductsToFile(String fileName)
        {
        }

        internal static IProduct CreateProduct(string name, Department department, decimal price)
        {
            Product p = new Product();
            p.department = department;
            p.name = name;
            p.quantity = 1;
            p.unit = "ADET";
            p.unitPrice = price;

            if(name.EndsWith("DYNAMIC"))
            {
                IProduct existingProduct = null;
                
                int index = p.name.LastIndexOf("DYNAMIC");
                p.name = p.name.Substring(0, index);

                try
                {
                    existingProduct = (Product)FindByName(p.name);
                }
                catch
                {

                }
                if (existingProduct != null)
                {
                    if (existingProduct.Department == p.Department &&
                        existingProduct.UnitPrice == p.UnitPrice)
                    {
                        return existingProduct;
                    }
                }

                int dynamicLabel = (int)DBAdapter.Instance().ExecuteQuery(String.Format("SELECT MAX(ID) FROM", ProductTable));
                dynamicLabel++;
                
                p.id = dynamicLabel;
                p.valid = true;
                p.status = ProductStatus.Weighable;
                p.category = "1";
                p.secondaryUnitPrice = 0;

                Add(p);
            }
            return p;

        }

        public override string ToString()
        {
            return String.Format("{0},{1},{2}", 
                                    this.id, 
                                    this.Barcode, 
                                    this.Name
                                    );
        }


        internal static bool StartUpdate()
        {
            // CREATE TABLE tblNew AS SELECT * FROM Products WHERE 1=2
            int response = 0;
            String strQuery = String.Empty;

            // Delete temprorary table if exist
            response = DropTable(TempProductTable);

            // Create command string
            strQuery =String.Format("CREATE TABLE {0} "+
                                    "AS SELECT * FROM {1} " + 
                                    "WHERE 1=2", 
                                    TempProductTable, ProductTable);
            try
            {
                response = DBAdapter.Instance().ExecuteNonQuery(strQuery);
                response = 1;
                // Set flag for adding products to temp table
                inUpdateState = true;
            }
            catch (System.Exception ex)
            {
            	
            }

            return response>0;
        }

        internal static bool FinalizeUpdate()
        {
            int response = 0;

            inUpdateState = false;
            
            // drop product table
            response = DropTable(ProductTable);
            // rename temp table as product table
            response = RenameTable(TempProductTable, ProductTable);

            return response > 0;
        }

        private static int DropTable(string tableName)
        {
            // Sample: DROP TABLE IF EXISTS TempProducts;

            String strQuery = String.Empty;
            int response = 0;

            // Create command string
            strQuery = String.Format("DROP TABLE IF EXISTS {0} ", tableName);

            try
            {
                // execute drop command
                response = DBAdapter.Instance().ExecuteNonQuery(strQuery);
                response = 1;
            }
            catch (System.Exception ex)
            {

            }

            return response;
        }


        private static int RenameTable(string sourceTblName, string destTblName)
        {
            // Sample: ALTER TABLE sourceTblName RENAME TO destTblName;

            String strQuery = String.Empty;
            int response = 0;

            // Create command string
            strQuery = String.Format("ALTER TABLE {0} RENAME TO {1}", 
                                                sourceTblName,
                                                destTblName
                                                );

            try
            {
                // execute rename table name command
                response = DBAdapter.Instance().ExecuteNonQuery(strQuery);
                response = 1;
            }
            catch (System.Exception ex)
            {

            }

            return response;
        }

        /* TODO: Try to increase speed of transaction
         * using (SQLiteTransaction mytransaction = myconnection.BeginTransaction())
         *  {
         *    using (SQLiteCommand mycommand = new SQLiteCommand(myconnection))
         *    {
         *      SQLiteParameter myparam = new SQLiteParameter();
         *      int n;
         *
         *      mycommand.CommandText = "INSERT INTO [MyTable] ([MyId]) VALUES(?)";
         *      mycommand.Parameters.Add(myparam);
         *
         *      for (n = 0; n < 100000; n ++)
         *      {
         *        myparam.Value = n + 1;
         *        mycommand.ExecuteNonQuery();
         *      }
         *    }
         *    mytransaction.Commit();
         *  }
         */
        internal static void LoadProductFileToDB(string path, out int successCount, out int failCount)
        {
            String line = "";
            String tblName = "";
            Product p;

            successCount = 0;
            failCount = 0;
            tblName = inUpdateState ? TempProductTable : ProductTable;
            
            DataSet dsDocItems = DBAdapter.Instance().GetDataSet("SELECT * FROM " + tblName);

            using (StreamReader sr = new StreamReader(path, PosConfiguration.DefaultEncoding))
            {
                while ((line = @sr.ReadLine()) != null)
                {
                    //line[0] == '0' means invalid line
                    //Skip trailing blank lines and invalid lines
                    if (line.Trim().Length == 0 || line[0] == '0')
                        continue;

                    p = LineToProduct(line);
                    if (AddToProductTable(dsDocItems.Tables[0], p))
                        successCount++;
                    else
                        failCount++;

                }
                sr.Close();
            }
            DBAdapter.Instance().UpdateDataSet(dsDocItems);
        }
        private static bool AddToProductTable(DataTable dt, Product p)
        {
            try
            {
                string tblName = inUpdateState ? TempProductTable : ProductTable;
                DataRow[] rowExists = new DataRow[0];

                try
                {
                    rowExists = dt.Select("ID = '" + p.id + "'");
                }
                catch (System.Exception ex)
                {

                }

                CultureInfo ci = CultureInfo.GetCultureInfo("en-US");

                if (rowExists.Length == 0)
                {
                    DataRow row = dt.NewRow();

                    row["ID"] = p.id;
                    row["Name"] = p.name.TrimEnd(new char[] { ' ' });
                    row["Barcode"] = p.barcode;
                    row["DepartmentId"] = p.department.Id;
                    row["UnitPrice"] = p.unitPrice.ToString("f", ci);
                    row["SecondaryUnitPrice"] = p.secondaryUnitPrice.ToString("f", ci);
                    row["Unit"] = p.Unit;
                    row["ExtraProperty"] = GetAdditionalProperty(p);
                    row["Valid"] = p.valid;
                    row["CategoryId"] = p.category;

                    dt.Rows.Add(row);
                }
                else
                {
                    rowExists[0]["Name"] = p.name.TrimEnd(new char[] { ' ' });
                    rowExists[0]["Barcode"] = p.barcode;
                    rowExists[0]["DepartmentId"] = p.department.Id;
                    rowExists[0]["UnitPrice"] = p.unitPrice.ToString("f", ci);
                    rowExists[0]["SecondaryUnitPrice"] = p.secondaryUnitPrice.ToString("f", ci);
                    rowExists[0]["Unit"] = p.Unit;
                    rowExists[0]["ExtraProperty"] = GetAdditionalProperty(p);
                    rowExists[0]["Valid"] = p.valid;
                    rowExists[0]["CategoryId"] = p.category;

                }
                return true;
            }
            catch
            {
                EZLogger.Log.Error("Ürün eklenemedi : ", p.ToString());
            }
            return false;
        }
    }
}
