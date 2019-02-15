using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using Hugin.POS.Common;

namespace Hugin.POS.PromotionServer
{
    internal class Settings
    {
        internal static Encoding DefaultEncoding = PosConfiguration.DefaultEncoding;
        internal const String TOTAL_PROMOTION_FILE_NAME = "PROMO.DAT";
        internal const String PRODUCT_PROMOTION_FILE_NAME = "PROURUN.DAT";
        internal const String CATEGORY_PROMOTION_FILE_NAME = "PROREYON.DAT";
        internal const String SPECIAL_PROMOTION_FILE_NAME = "OZELPROMO.DAT";
        internal const String ERROR_FILE = "PROMO.LOG";

        internal static List<IPromotion> SalePromotions;
        internal static List<IPromotion> TotalPromotions;

        internal static String DataPath
        {
            get
            {                
                return  ProgramDirectory+"Data/";
           }
        }
        internal static String ProgramDirectory 
        {
            get { return IOUtil.ProgramDirectory; }
        }
        internal static String ErrorPath
        {
            get { return ProgramDirectory + "Log/"; }
        }
        internal static String ServerDownloadPath
        {
            get { return String.Format("{0}{1}", ServerUploadPath, "Data/"); }
        }
        internal static String ServerUploadPath
        {
            get { return Settings.Get("OfficePath"); }
        }
        internal static String PointsDatabaseFile
        {
            get { return Settings.Get("PointServerUri"); }
        }

        internal static string PromoKey
        {
            get
            {
                string pwd = Settings.Get("FiscalId") + "HUGIN YAZILIM TEKNOLOJILERI";
                string hashkey = Settings.Get("FiscalId").Substring(2);

                return Encrypt(hashkey, pwd);
            }
        }

        private static string Encrypt(string hashKey, string strQueryStringParameter)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider hash_func = new System.Security.Cryptography.MD5CryptoServiceProvider();

            byte[] key = hash_func.ComputeHash(Encoding.ASCII.GetBytes(hashKey));
            byte[] IV = new byte[hashKey.Length];

            System.Security.Cryptography.SHA1CryptoServiceProvider sha_func = new System.Security.Cryptography.SHA1CryptoServiceProvider();

            byte[] temp = sha_func.ComputeHash(Encoding.ASCII.GetBytes(hashKey));

            for (int i = 0; i < hashKey.Length; i++)
                IV[i] = temp[hashKey.Length];

            byte[] toenc = System.Text.Encoding.UTF8.GetBytes(strQueryStringParameter);

            System.Security.Cryptography.TripleDESCryptoServiceProvider des =
                                new System.Security.Cryptography.TripleDESCryptoServiceProvider();
            des.KeySize = 128;
            MemoryStream ms = new MemoryStream();

            System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(
                                                                ms,
                                                                des.CreateEncryptor(key, IV),
                                                                System.Security.Cryptography.CryptoStreamMode.Write
                                                                );
            cs.Write(toenc, 0, toenc.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }
        internal static string Get(string value)
        {
            ConfigurationManager config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            String s = config.Get(value);
            return (s == null) ? String.Empty : s;
        }
        internal static void Load()
        {
            if (!Directory.Exists(ErrorPath)) Directory.CreateDirectory(ErrorPath);
            SalePromotions = new List<IPromotion>();
            TotalPromotions = new List<IPromotion>();
            try
            {
                TotalPromotion.Load();
            }
            catch (Exception ex)
            {
                Settings.Log(String.Format("Promo.dat dosyasi yuklenemedi!\r{0}", ex.Message));
            }
#if !WindowsCE
            
            try
            {
                ProductPromotion.Load();
            }
            catch (Exception ex)
            {
                Settings.Log(String.Format("Prourun.dat dosyasi yuklenemedi!\r{0}", ex.Message));
            }
            try
            {
                CategoryPromotion.Load();
            }
            catch (Exception ex)
            {
                Settings.Log(String.Format("Proreyon.dat dosyasi yuklenemedi!\r{0}", ex.Message));
            }
            try
            {
                SpecialPromotion.Load();
            }
            catch (Exception ex)
            {
                Settings.Log(String.Format("Ozelpromo.dat dosyasi yuklenemedi!\r{0}", ex.Message));
            }
            
#endif
            try
            {
                FileWatcher.Start();
            }
            catch (Exception ex)
            {
                Settings.Log(String.Format("Promosyon dosyasý bulunamadý.\r{0}", ex.Message));
            }



        }
        static void DownloadIfMissingDataFile(String fileName)
        {
            if (!File.Exists(DataPath + fileName))
                File.Copy(Settings.ServerDownloadPath + fileName, DataPath + fileName);
        }
        internal static void Log(String message) 
        {
            if (!File.Exists(ErrorPath + ERROR_FILE))
                File.Create(ErrorPath + ERROR_FILE).Close();

            StreamWriter sw = File.AppendText(ErrorPath + ERROR_FILE);
             sw.WriteLine(String.Format("{0:G}:  {1}",DateTime.Now,message));
             sw.Close();
        
        }
        internal static IDataConnector DataConnector
        {
            get
            {
                return Data.Connector.Instance();
            }
        }
    }
}
