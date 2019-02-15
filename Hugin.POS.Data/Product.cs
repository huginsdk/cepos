using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.IO;
using System.Globalization;

namespace Hugin.POS.Data
{
    [Serializable]
    class Product : IProduct
    {
        const long MAX_UNITPRICE = 999999999;
        internal static Decimal MIN_UNITPRICE = 999999999;
        string category;
        bool valid;//is product valid?
        int id;//plu number
        string barcode;
        string name;
        Department department;
        decimal unitPrice;
        decimal secondaryUnitPrice;
        string unit;
        decimal quantity=1;
        ProductStatus status = ProductStatus.None;
        ProductRequiredField requiredFields = ProductRequiredField.None;
        static Dictionary<String, IProduct> productsByBarcode;
        static Dictionary<String, IProduct> productsByName;
        static SortedList<int, IProduct> productsByLabel;
        static Dictionary<String, IProduct> backupProductsByBarcode;
        static Dictionary<String, IProduct> backupProductsByName;
        static SortedList<int, IProduct> backupProductsByLabel;

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

        public Hugin.POS.Common.ProductStatus Status
        {
            get { return status; }
        }

        public Hugin.POS.Common.ProductRequiredField RequiredField
        {
            get { return requiredFields; }
        }

        public IProduct Clone()
        {
            return (Product)this.MemberwiseClone();
        }

        public bool UpdateProduct(string line)
        {
            if (Add(line))
            {
                ProductsToFile(PosConfiguration.DataPath + Settings.ProductFile);

                return true;
            }
            else
                return false;
        }

        public bool DeleteProduct(IProduct p)
        {
            if (productsByLabel.IndexOfKey(p.Id) >= 0)
            {
                productsByLabel.Remove(p.Id);
                productsByBarcode.Remove(p.Barcode);
                productsByName.Remove(p.Name);

                ProductsToFile(PosConfiguration.DataPath + Settings.ProductFile);

                return true;
            }
            else
                return false;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj is Product && ((Product)obj).Id == this.Id) return true;
            return false;

        }
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

        internal static void Backup()
        {
            backupProductsByBarcode = productsByBarcode;
            backupProductsByName = productsByName;
            backupProductsByLabel = productsByLabel;

            productsByBarcode = new Dictionary<String, IProduct>();
            productsByName = new Dictionary<String, IProduct>();
            productsByLabel = new SortedList<int, IProduct>();

        }
        internal static void Restore()
        {
            if (productsByBarcode.Count == 0)
            {
                
                if (backupProductsByBarcode == null || backupProductsByBarcode.Count == 0)
                    return;//if no product exists then program is invalid, this can be controlled by looking success count
                productsByBarcode = backupProductsByBarcode;
                productsByName = backupProductsByName;
                productsByLabel = backupProductsByLabel;
            }
            
            if (backupProductsByBarcode != null)
                backupProductsByBarcode.Clear();
            if (backupProductsByName != null)
                backupProductsByName.Clear();
            if (backupProductsByLabel != null)
                backupProductsByLabel.Clear();
        }
        internal static bool Add(string line)
        {
            try
            {
                int nameLength = 20;
#if WindowsCE
                nameLength = 19;
#endif
                Product p = new Product();
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

                if (departmentNameOnly)
                    p.name = p.department.Name;

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

                if (p.secondaryUnitPrice == 0 && p.UnitPrice > 0)
                {
                    p.secondaryUnitPrice = p.unitPrice;
                }
                //id is key word for product
                //if product with same pluno exists then remove old product, and put new product
                //and assign the old barcode to new product in barcode dictionary
                //and assign the old name to new product in name dictionary
                if (productsByLabel.IndexOfKey(p.id) >= 0)
                {
                    IProduct pOld = productsByLabel[p.id];

                    productsByLabel.Remove(pOld.Id);
                    //productsByBarcode.Remove(pOld.Barcode);
                    //productsByName.Remove(pOld.Name);

                }
                productsByLabel.Add(p.id, p);

                if (productsByBarcode.ContainsKey(p.barcode))
                    productsByBarcode.Remove(p.barcode);
                productsByBarcode.Add(p.barcode, p);

                if (productsByName.ContainsKey(p.name))
                    productsByName.Remove(p.name);
                productsByName.Add(p.name, p);

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

        internal static IProduct FindByName(String nameQry)
        {
            if (productsByName.ContainsKey(nameQry))
                return productsByName[nameQry];
            throw new Exception(PosMessage.PRODUCT_NOTFOUND);
        }

        internal static IProduct FindByBarcode(String barcodeQry)
        {
            //Özel tanimli barkodlari destekle 
            if (barcodeQry.Length == 0 || !productsByBarcode.ContainsKey(barcodeQry))
                throw new Exception(PosMessage.BARCODE_NOTFOUND);
            return productsByBarcode[barcodeQry];
        }

        internal static IProduct FindByLabel(String pluQry)
        {
            int pluNo = 0;
            Parser.TryInt(pluQry, out pluNo);
            if (!productsByLabel.ContainsKey(pluNo))
                throw new Exception(PosMessage.PLU_NOTFOUND);
            return productsByLabel[pluNo];
        }

        internal static List<IProduct> SearchProductByBarcode(String[] barcodeList)
        {
            List<IProduct> productList = new List<IProduct>();
            foreach (String qry in barcodeList)
                if (productsByBarcode.ContainsKey(qry))
                    productList.Add(productsByBarcode[qry]);
            return productList;
        }

        internal static List<IProduct> SearchProductByLabel(String[] pluList)
        {
            List<IProduct> productList = new List<IProduct>();
            int productId = 0;
            foreach (String qry in pluList)
                if (Parser.TryInt(qry, out productId) &&
                    productsByLabel.ContainsKey(productId))
                    productList.Add(productsByLabel[productId]);
            return productList;
        }

        internal static List<IProduct> SearchProductByName(String nameData)
        {
            List<IProduct> productList = new List<IProduct>();
            foreach (String name in productsByName.Keys)
            {
                if (Str.Contains(name, nameData))
                {
                    productList.Add(productsByName[name]);
                }
            }
            return productList;
        }

        internal static void ProductsToFile(String fileName)
        {
            StringBuilder strProducts = new StringBuilder();
            SortedList<int, IProduct> writtenLabels =new SortedList<int,IProduct>(productsByLabel);
            
            //Clear file
            IOUtil.WriteAllText(fileName, "", PosConfiguration.DefaultEncoding);
            int count = 0;
            char code = 'H';
            foreach (IProduct p in productsByBarcode.Values)
            {
                code = ((Product)p).GetPropertyCode();
                strProducts.Append(String.Format(
                    "{0:D1}{1:D6}{2,-20}{3,-20}{4:D9}{5:D2}{6,-4}{7,1}{8:D6}{9:D10}\r\n",
                    p.Valid ? "1" : "0",
                    p.Id,
                    p.Barcode,
                    p.Name,
                    (long)(p.UnitPrice * 100),
                    p.Department.Id + 1,
                    p.Unit,
                    code,
                    int.Parse(p.Category),
                    (long)(100 * p.SecondaryUnitPrice)
                    ));

                if (productsByLabel.IndexOfKey(p.Id) >= 0)
                    writtenLabels.Remove(p.Id);
                
                //Windows ce memory is less than desktop pc 
                //So OutOfMemoryException occur if stringbuilder capacity is too big.
                if (count >= 5000)
                {
                    //move memory to file than clear momory
                    IOUtil.AppendAllText(fileName, strProducts.ToString());
                    strProducts = new StringBuilder();
                    count = 0;
                }
                count++;
            }

            IProduct prdct;
            foreach (int labelNo in writtenLabels.Keys)
            {
                //if (writtenLabels.IndexOf(labelNo) >= 0) continue;
                prdct = productsByLabel[labelNo];
                code = ((Product)prdct).GetPropertyCode();
                strProducts.Append(String.Format(
                    "{0:D1}{1:D6}{2,-20}{3,-20}{4:D9}{5:D2}{6,-4}{7,1}{8:D6}{9:D10}\r\n",
                    prdct.Valid ? "1" : "0",
                    prdct.Id,
                    prdct.Barcode,
                    prdct.Name,
                    (long)(prdct.UnitPrice * 100),
                    prdct.Department.Id + 1,
                    prdct.Unit,
                    code,
                    int.Parse(prdct.Category),
                    (long)(100 * prdct.SecondaryUnitPrice)
                    ));

                if (count >= 5000)
                {
                    IOUtil.AppendAllText(fileName, strProducts.ToString());
                    strProducts = new StringBuilder();
                    count = 0;
                }
                count++;
            }

            IOUtil.AppendAllText(fileName, strProducts.ToString());
            
            
            strProducts = new StringBuilder();
            writtenLabels = new SortedList<int, IProduct>();
        }

        private char GetPropertyCode()
        {
            char code = 'H';

            if ((this.requiredFields & ProductRequiredField.All) == ProductRequiredField.All)
            {
                // It means Product is drug and it has all fields.
                code = 'D';
            }
            else if ((this.requiredFields & ProductRequiredField.SerialNumber) == ProductRequiredField.SerialNumber)
            {
                // Product needs to serial number
                code = 'S';
            }
            else if (this.status  == ProductStatus.Weighable)
            {
                code = 'E';
            }

            return code;
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
                
                int minDynamicLabel = 100000;
                while(true)
                {
                    if(!productsByLabel.ContainsKey(minDynamicLabel))
                    {
                        break;
                    }
                    minDynamicLabel++;
                }
                
                p.id = minDynamicLabel;
                p.valid = true;
                p.status = ProductStatus.Weighable;
                p.category = "1";
                p.secondaryUnitPrice = 0;

                /*
                 * do not need to save product in a file
                 * because it will be used dynamically                
                 */
                productsByLabel.Add(p.id, p);
                
                if (!productsByName.ContainsKey(p.name))
                    productsByName.Add(p.name, p);
            }
            return p;

        }
		
		private void Log (Exception ex)
		{
			/*
			 * this function is implemented to catch warnings
			 * which caused fraom unreferenced "ex" variables
			 */
		}
    }
}
