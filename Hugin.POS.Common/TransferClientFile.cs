using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using Rebex.Net;

namespace Hugin.POS.Common
{
    public class TransferClientFile : ITransferClient
    {
        private static Thread connectionCheckerThread;

        public void Disconnect()
        {
   
        }

        public String DownloadString(string address, int timeout)
        {
            return IOUtil.ReadAllText(address, PosConfiguration.DefaultEncoding);
        }

        public void DownloadFile(String address, String fileName)
        {
            File.Copy(address, fileName, true);
        }

        //TODO maybe should return lastwritetime from server
        public void UploadString(String address, String data, int timeout)
        {
            IOUtil.WriteAllText(address, data, PosConfiguration.DefaultEncoding);
        }

        public void UploadFile(String address, String fileName)
        {
            File.Copy(fileName, address, true);
        }

        public void AppendString(String address, String data)
        {
            IOUtil.AppendAllText(address, data);
        }

        public void AppendFile(String address, String fileName)
        {
            IOUtil.AppendAllText(address, IOUtil.ReadAllText(fileName, PosConfiguration.DefaultEncoding));
        }

        public Boolean FileExists(string address)
        {
            try
            {
                return Dir.GetFiles(Path.GetDirectoryName(address), Path.GetFileName(address)).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public DateTime FileGetLastWriteTime(string address)
        {
            DateTime lastMessageTime = DateTime.MinValue;
            connectionCheckerThread = new System.Threading.Thread(delegate()
            {
                try
                {
                    lastMessageTime = File.GetLastWriteTime(address);
                }
				catch (Exception ex){ Log (ex);}
            }
            );
            
            if (!IsThreadStarted(connectionCheckerThread))
            {
                try
                {
                    connectionCheckerThread.Start();
                }
                catch (IOException ioe)
                {
                    throw new BackOfficeUnavailableException(ioe.Message);
                }
                catch { }
                connectionCheckerThread.Join(5000);

            }
            connectionCheckerThread.Abort();

            //file does not exist, verify connection
            if (DateTime.Now.Year - lastMessageTime.Year > 100)
            {
                try
                {
                    if (!DirectoryExists(Path.GetDirectoryName(address), 5000))
                        throw new BackOfficeUnavailableException();
                }
                catch
                {
                    throw new BackOfficeUnavailableException();
                }
                throw new FileNotFoundException("Mesaj dosyası bulunamadı");
            }
            else return lastMessageTime;
        }

        public Boolean DirectoryExists(string address, int timeout)
        {
            return Dir.Exists(address, timeout);
        }

        public void DownloadDirectory(string address, string localPath, string pattern)
        {
            Dir.CopyDirectory(address, localPath, pattern);
        }

        public FileInfo[] ListDirectory(string address, string pattern)
        {
            return Dir.GetFilesInfo(address, pattern);
        }

        private static bool IsThreadStarted(System.Threading.Thread connectionCheckerThread)
        {
            bool retVal = false;

            try
            {
                retVal = connectionCheckerThread.Join(0);
            }
            catch (Exception) { }

            return retVal;
        }

        public void RenameFile(string sourceAddress, string destinationAddress)
        {
            File.Move(sourceAddress, destinationAddress);
        }

        public void DeleteFile(string address)
        {
            File.Delete(address);
        }

        public FileHelper[] GetFiles(string address, string pattern, int timeout)
        {
            string[] files = Dir.GetFiles(address, pattern);
            FileHelper[] helpers = new FileHelper[files.Length];

            for (int i = 0; i < files.Length;i++ )
            {
                helpers[i] = new FileHelper(files[i]);
            }

            return helpers;
        }

        public DateTime GetDirectoryLastWriteTime(string address)
        {
            return Directory.GetLastWriteTime(address);
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
