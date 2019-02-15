using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Data
{
    class Serial
    {
        static List<string> serialNumbers;
        static List<string> backupSerialNumbers;

        internal static void Backup()
        {
            backupSerialNumbers = serialNumbers;
            serialNumbers = new List<string>();
        }

        internal static void Restore()
        {
            if (serialNumbers.Count == 0)
                serialNumbers = backupSerialNumbers;

            if (backupSerialNumbers != null)
                backupSerialNumbers.Clear();
        }

        internal static bool Add(String line)
        {
            try
            {
                serialNumbers.Add(line);

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

        internal static bool CheckSerial(string serial)
        {
            if (serialNumbers.Count == 0)
                return true;
            return serialNumbers.Contains(serial);
        }
    }
}
