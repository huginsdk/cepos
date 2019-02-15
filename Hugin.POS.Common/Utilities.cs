using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace Hugin.POS.Common
{
    public class Location
    {
        int x = 0;
        int y = 0;
        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        public int Y
        {
            get { return y; }
            set { y = value; }
        }
    }
    public static class Format
    {
        public static String FormatDate(String date)
        {
            date = date.Insert(2, ".");
            return date.Insert(5, ".");
        }
    }

    public static class Rounder
    {
        public static Decimal RoundDecimal(Decimal d, int decimals, bool midPointFromZero)
        {
            if (midPointFromZero)
                return decimal.Parse(String.Format("{0:0." + "0".PadLeft(decimals, '0') + "}", d));
            else
                return Math.Round(d, decimals);
        }
    }

    public static class Parser
    {
        public static bool TryInt(String s, out int i)
        {
            i = 0;
            try
            {
                i = int.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryInt(String s, System.Globalization.NumberStyles style, System.Globalization.CultureInfo provider, out int i)
        {
            i = 0;
            try
            {
                i = int.Parse(s, style, provider);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryLong(String s, out long i)
        {
            i = 0;
            try
            {
                i = long.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryDecimal(String s, out decimal i)
        {
            i = 0;
            try
            {
                i = decimal.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryDate(String s, out DateTime i)
        {
            i = DateTime.MinValue;
            try
            {
                //todo i = DateTime.Parse(s, Settings.CultureInfo.DateTimeFormat);
                i = DateTime.Parse(s,
                                PosConfiguration.CultureInfo,
                                System.Globalization.DateTimeStyles.NoCurrentDateDefault);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /*
         *      strTime format string is like following;
         *      
         *      <Day>(split0)<Month>(split0)<Year>(split1)<Hour>(split2)<Min>(split0)<Sec>
         */
        public static bool TimeConversion(string strTime, out DateTime dateTime, char[] splitters, bool unixTime)
        {
            int year = 0;
            int month = 0;
            int day = 0;
            int hour = 0;
            int min = 0;
            int sec = 0;

            try
            {
                String[] timeData = strTime.Split(splitters[1]);

                String[] aDate = timeData[0].Split(splitters[0]);


                Parser.TryInt(aDate[0], out day);

                if (Parser.TryInt(aDate[1], out month))
                {
                    if(unixTime)
                        month += 1;
                }

                if (Parser.TryInt(aDate[2], out year))
                {
                    if(unixTime)
                        year += 1900;
                }

                if (timeData.Length > 1)
                {
                    String[] aTime = timeData[1].Split(splitters[2]);

                    if (Parser.TryInt(aTime[0], out hour))
                    {
                        hour = hour % 24;

                    }
                    if (Parser.TryInt(aTime[1], out min))
                    {
                        min = min % 60;
                    }

                }

                dateTime = new DateTime(year, month, day, hour, min, sec);
                return true;
            }
            catch (Exception)
            {
                dateTime = new DateTime();
                return false;
            }
        }
    }

    public static class Str
    {
        public static bool Contains(String text, String seach)
        {
            return text.IndexOf(seach) > -1;
        }
        public static bool Contains(String text, Char search)
        {
            return text.IndexOf(search) > -1;
        }

        public static string Remove(String text, int index)
        {
            return text.Substring(0, index);
        }

        public static string[] Split(string text, char[] separator)
        {
            return Split(text, separator, false);
        }

        public static string[] Split(string text, char[] separator, bool removeEmptyEntries)
        {
            if (removeEmptyEntries)
                return RemoveEmptyEntries(text.Split(separator));

            return text.Split(separator);

        }

        public static string[] Split(string text, char separator)
        {
            return Split(text, separator, false);
        }

        public static string[] Split(string text, char separator, bool removeEmptyEntries)
        {
            if (removeEmptyEntries)
                return RemoveEmptyEntries(text.Split(separator));

            return text.Split(separator);

        }

        public static string[] Split(string text, string separator)
        {
            return Split(text, separator, false);
        }

        public static string[] Split(string text, string separator, bool removeEmptyEntries)
        {
            char c = (char)1;
            text = text.Replace(separator, c.ToString());
            if (removeEmptyEntries)
                return RemoveEmptyEntries(text.Split(c));

            return text.Split(c);
        }

        private static string[] RemoveEmptyEntries(string[] array)
        {
            List<string> splitted = new List<string>();
            foreach (string str in array)
            {
                if (str.Length != 0)
                    splitted.Add(str);
            }

            return splitted.ToArray();
        }

        public static string FixTurkishUpperCase(string text)
        {
            // stack current culture
            System.Globalization.CultureInfo currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            // Set Turkey culture
            System.Globalization.CultureInfo turkey = new System.Globalization.CultureInfo("tr-TR");
            System.Threading.Thread.CurrentThread.CurrentCulture = turkey;

            string cultured = text.ToUpper();

            // Pop old culture
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;

            return cultured;
        }
    }
    
    public static class IOUtil
    {
#if WindowsCE

        [System.Runtime.InteropServices.DllImport("coredll", EntryPoint = "GetFileAttributes", SetLastError = true)]
        private static extern uint GetFileAttributes(string lpFileName);

        [System.Runtime.InteropServices.DllImport("coredll", EntryPoint = "SetFileAttributes", SetLastError = true)]
        private static extern bool SetAttributes(string lpFileName, uint dwFileAttributes);

#endif
        public static FileAttributes GetAttributes(String path)
        {
#if WindowsCE
            return (FileAttributes)GetFileAttributes(path);
#else
            return File.GetAttributes(path);
#endif
        }

        internal static void SetAttributes(string path, FileAttributes fileAttributes)
        {
#if WindowsCE
            SetAttributes(path, (uint)fileAttributes);
#else
            File.SetAttributes(path, fileAttributes);
#endif
        }

        public static byte[] ReadAllBytes(String path)
        {
            FileStream fs;
            byte[] buffer;

            using (fs = File.OpenRead(path))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
                fs.Close();
            }
            return buffer;
        }

        public static String ReadAllText(String path)
        {
            return ReadAllText(path, PosConfiguration.DefaultEncoding);
        }

        public static String ReadAllText(String path, System.Text.Encoding enc)
        {
            return new String(enc.GetChars(ReadAllBytes(path)));
        }

        public static String[] ReadAllLines(String path)
        {
#if (!WindowsCE)
            return File.ReadAllLines(path);
#else
            String s = ReadAllText(path);
            return Str.Split(s, "\r\n");
#endif
        }

        public static void WriteAllText(String path, String data)
        {
            WriteAllText(path, data, PosConfiguration.DefaultEncoding);
        }

        public static void WriteAllText(String path, String data, System.Text.Encoding enc)
        {
            if (File.Exists(path))
                File.Delete(path);
         
            FileStream fs;
            using (fs = File.Create(path))
            {
                fs.Write(enc.GetBytes(data), 0, data.Length);
                fs.Close();
            }
        }

        public static void AppendAllText(String path, String data)
        {
            FileStream fs;

            using (fs = File.OpenWrite(path))
            {
                fs.Seek(0, SeekOrigin.End);
                fs.Write(PosConfiguration.DefaultEncoding.GetBytes(data), 0, data.Length);
                fs.Close();
            }
        }

        public static void AppendAllText(String path, String data, Encoding enc)
        {
            FileStream fs;

            using (fs = File.OpenWrite(path))
            {
                fs.Seek(0, SeekOrigin.End);
                fs.Write(enc.GetBytes(data), 0, data.Length);
                fs.Close();
            }
        }

        public static void WriteAllLines(string path, string[] lines)
        {
#if (!WindowsCE)
            File.WriteAllLines(path, lines);
#else
            WriteAllText(path, "");//clear

            FileStream fs; 
            
            using (fs = File.OpenWrite(path))
            {
                fs.Seek(0, SeekOrigin.End);

                foreach (string line in lines)
                {
                    fs.Write(PosConfiguration.DefaultEncoding.GetBytes(line), 0, line.Length);
                    fs.Write(PosConfiguration.DefaultEncoding.GetBytes("\r\n"), 0, 2);
                }

                fs.Close();
            }
#endif

        }

        public static string ProgramDirectory
        {
            get
            {
#if Mono

				//string dir = "/android/data";//internal path
				string dir = Android.OS.Environment.ExternalStorageDirectory.ToString(); 
				return dir + "/HuginPOS/";
#else
                String location = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
                return System.IO.Path.GetDirectoryName(location).Replace("file:\\", "") + "\\";
#endif
            }
        }
        public static string AssemblyName
        {
            get {
#if !WindowsCE
                return "pos.exe";
#else
                return "pos.exe";
#endif
            }
        }
    }

    public class Dir
    {
        static Thread connectionCheckerThread;

        public static String[] GetFiles(string path, string searchpattern)
        {
#if !WindowsCE
            return Directory.GetFiles(path,searchpattern, SearchOption.TopDirectoryOnly);
#else
            return Directory.GetFiles(path, searchpattern);
#endif
        }

        public static FileInfo[] GetFilesInfo(string path, string searchpattern)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
#if !WindowsCE
            return dirInfo.GetFiles(searchpattern, SearchOption.TopDirectoryOnly);
#else
            return dirInfo.GetFiles(searchpattern);
#endif
        }

        public static Boolean Exists(string path, int timeout)
        {
            bool directoryExists = false;
            Exception exception = null;
            connectionCheckerThread = new System.Threading.Thread(delegate()
            {
                try
                {
                    System.IO.Directory.GetFiles(path);
                    directoryExists = true;
                }
                catch (Exception ex) { exception = ex; }
            }
            );

            if (!IsThreadStarted(connectionCheckerThread))
            {
                try
                {
                    connectionCheckerThread.Start();
                }
				catch (Exception ex) { Log (ex);}
                connectionCheckerThread.Join(timeout);

            }
            connectionCheckerThread.Abort();

            return directoryExists;
        }

        private static bool IsThreadStarted(System.Threading.Thread connectionCheckerThread)
        {
            bool retVal = false;
            try
            {

                retVal = connectionCheckerThread.Join(0);
            }
			catch (Exception ex) {Log (ex); }
            return retVal;
        }

        public static void CopyDirectory(string Src, string Dst, string pattern)
        {
            String[] Files;

            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                Dst += Path.DirectorySeparatorChar;
            if (!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            Files = Directory.GetFileSystemEntries(Src, pattern);
            foreach (string Element in Files)
            {
                // Sub directories
                if (Directory.Exists(Element))
                    CopyDirectory(Element, Dst + Path.GetFileName(Element), pattern);
                // Files in directory
                else
                    File.Copy(Element, Dst + Path.GetFileName(Element), true);
            }
        }

		private static void Log (Exception ex)
		{
			/*
			 * this function is implemented to catch warnings
			 * which caused fraom unreferenced "ex" variables
			 */
		}
    }
    public class Date
    {
        DateTime date;
        public Date(DateTime date)
        {
            this.date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }
        public DateTime Value
        {
            get { return date; }
        }
    }
}
