using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace Hugin.POS.PromotionServer
{
    internal class FileWatcher
    {
        //private static FileSystemWatcher fileWatcher;

        //static bool watchInitialized = false;
        static Dictionary<String, int> contentHashCodes = null;
        static Dictionary<String, DateTime> proFiles = null;
        static bool terminate = false;
        internal static void Start()
        {
            SetPromoFiles();
            contentHashCodes = new Dictionary<string, int>();
            if (proFiles.Count == 0)
                return;
            System.Threading.Thread fileWatcher = new System.Threading.Thread(
                                                      new System.Threading.ThreadStart(Watcher));
            fileWatcher.Name = "FileWatcher";
            fileWatcher.IsBackground = true;
            fileWatcher.Start();

            Settings.Log("File watching has been starting.");
        }

        private static void Watcher()
        {
            DateTime lastWriteTime = DateTime.Now;
            Dictionary<String, DateTime> tempFile = new Dictionary<string, DateTime>();
            while (true)
            {
                if (terminate)
                    break;
                //Controls all promo files
                foreach (string str in proFiles.Keys)
                {
                    lastWriteTime = File.GetLastWriteTime(str);
                    if (lastWriteTime == proFiles[str])
                        continue;
                    LoadChangedFile(str);
                    tempFile.Add(str, lastWriteTime);
                }
                //updates new files' last write times.
                if (tempFile.Count > 0)
                {
                    foreach (string str in tempFile.Keys)
                        proFiles[str] = tempFile[str];
                    tempFile = new Dictionary<string, DateTime>();
                }
                System.Threading.Thread.Sleep(5000);
            }
        }

        private static void SetPromoFiles()
        {
            string filename;
            proFiles = new Dictionary<string, DateTime>();

            try
            {
                filename = Settings.DataPath + Settings.TOTAL_PROMOTION_FILE_NAME;
                proFiles.Add(filename, File.GetLastWriteTime(filename));
            }
            catch (Exception ex)
            {
                Settings.Log(ex.Message);
            }

#if !WindowsCE
            
            try
            {

                filename = Settings.DataPath + Settings.PRODUCT_PROMOTION_FILE_NAME;
                proFiles.Add(filename, File.GetLastWriteTime(filename));
            }
            catch (Exception ex)
            {
                Settings.Log(ex.Message);
            }

            try
            {
                filename = Settings.DataPath + Settings.CATEGORY_PROMOTION_FILE_NAME;
                proFiles.Add(filename, File.GetLastWriteTime(filename));

            }
            catch (Exception ex)
            {
                Settings.Log(ex.Message);
            }

            try
            {
                filename = Settings.DataPath + Settings.SPECIAL_PROMOTION_FILE_NAME;
                proFiles.Add(filename, File.GetLastWriteTime(filename));
            }
            catch (Exception ex)
            {
                Settings.Log(ex.Message);
            }
            
#endif
        }

        static void LoadChangedFile(string filePath)
        {
            string fileName = filePath.Substring(filePath.LastIndexOfAny(new char[] { '\\', '/' }) + 1);
            int fileContentHashCode = String.Empty.GetHashCode();
            StreamReader sr = null;
            string context;
            using (sr = new StreamReader(filePath, Settings.DefaultEncoding))
            {
                context = sr.ReadToEnd();
            }
            fileContentHashCode = context.GetHashCode();
            if (contentHashCodes.ContainsKey(fileName))
            {
                if (contentHashCodes[fileName] == fileContentHashCode)
                    return;
                contentHashCodes.Remove(fileName);
            }
            Settings.Log("File is changed : " + fileName);
            try
            {
                switch (fileName)
                {
                    case Settings.PRODUCT_PROMOTION_FILE_NAME:
                    case Settings.CATEGORY_PROMOTION_FILE_NAME:
                        Settings.SalePromotions = new List<IPromotion>();
                        CategoryPromotion.Load();
                        ProductPromotion.Load();
                        break;
                    case Settings.TOTAL_PROMOTION_FILE_NAME:
                    case Settings.SPECIAL_PROMOTION_FILE_NAME:
                        Settings.TotalPromotions = new List<IPromotion>();
                        TotalPromotion.Load();
                        SpecialPromotion.Load();
                        break;
                }
                contentHashCodes.Add(fileName, fileContentHashCode);
            }
            catch (Exception ex)
            {
                Settings.Log(ex.Message);
            }
        }

        internal static void Stop()
        {
            terminate = true;
        }
    }
}
