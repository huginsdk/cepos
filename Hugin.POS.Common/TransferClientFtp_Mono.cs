using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

namespace Hugin.POS.Common
{
	enum FtpState
	{
		Ready,
		Disconnected,
	}
	class Ftp
	{
		int timeout = 0;
		FtpState state = FtpState.Disconnected;
		bool isAuthenticated = false;
		public Ftp ()
		{
		}
		public FtpState State {
			get{return state;}
			set{}
		}
		public int Timeout {
			get{ return timeout;}
			set{timeout = value;}
		}
		public void Abort ()
		{
		}		
		public void Disconnect ()
		{
		}
		public String Connect (String path)
		{
			throw new Exception("Ftp not available");
		}
		public bool IsAuthenticated {
			get{
				return isAuthenticated;
			}
		}
		public String Login (String user, String pass)
		{
			throw new Exception("Login failed");
		}
	}
    public class TransferClientFtp : ITransferClient
    {
        private Uri uri = null;
        private String userName = "", password = "";
        private List<Ftp> ftpList = null;
        
        private const int FTP_TIMEOUT = 60000;
        private static Object lockObj = new Object();

        public static string MessageFileName
        {
            get { return PosConfiguration.ServerControlPath + "Mesaj." + PosConfiguration.Get("RegisterId"); }
        }

        internal Ftp SetRequest(string address)
        {
            lock (lockObj)
            {
                try
                {
                    EZLogger.Log.Debug("setrequest {0}", address);
                    uri = new Uri(address);
                    Ftp currentFtp = null;
                    if (uri.Scheme != Uri.UriSchemeFtp) return null;

                    if (ftpList == null)
                    {
                        currentFtp = new Ftp();
                        string login = PosConfiguration.Get("OfficeLogin");
                        if (login == "") login = uri.UserInfo;
                        userName = login.Split(':')[0];
                        if (Str.Contains(login, (":")))
                            password = login.Split(':')[1];
                        EZLogger.Log.Debug("user {0}", userName);
                        EZLogger.Log.Debug("pass {0}", password);
                        ftpList = new List<Ftp>();
                        ftpList.Add(currentFtp);
                    }
                    else
                    {
                        currentFtp = ftpList.Find(delegate(Ftp item)
                        {
                            return item.State == FtpState.Ready || item.State == FtpState.Disconnected;
                        });

                        if (currentFtp == null)
                        {
                            currentFtp = new Ftp();
                            ftpList.Add(currentFtp);
                        }
                    }
                    string s = "";

                    EZLogger.Log.Debug("ftp state: {0}", currentFtp.State);
                    
                    try
                    {

                        currentFtp.Timeout = 3000;

                        if (currentFtp.State != FtpState.Ready)
                        {
                            if (currentFtp.State != FtpState.Disconnected)
                                currentFtp.Abort();

                            s = currentFtp.Connect(uri.Host);
                            EZLogger.Log.Debug("connect result: {0}", s);
                        }

                        if (!currentFtp.IsAuthenticated)
                        {
                            s = currentFtp.Login(userName, password);
                            EZLogger.Log.Debug("login result " + s);
                        }
                        return currentFtp;
                    }
                    catch (Exception e)
                    {
                        EZLogger.Log.Debug("exception in setreq : " + e.Message);
                        throw new BackOfficeUnavailableException(e.Message);
                    }
                    finally
                    {
                        if (currentFtp != null)
                            currentFtp.Timeout = FTP_TIMEOUT;
                    }
                }
                catch (Exception ex)
                {
                    throw new BackOfficeUnavailableException(ex.Message);
                }
            }
        }

        public void Disconnect(){
            ftpList = null;
        }

        public String DownloadString(string address, int timeout)
		{
			throw new Exception("function is not supported");
        }

        public void DownloadFile(String address, String fileName)
		{
			throw new Exception("function is not supported");
        }

        //TODO maybe should return lastwritetime from server
        public void UploadString(String address, String data, int timeout)
        {
			throw new Exception("function is not supported");

        }

        public void UploadFile(String address, String fileName)
		{
			throw new Exception("function is not supported");
        }

        public void AppendString(String address, String data)
        {
			throw new Exception("function is not supported");

        }

        public void AppendFile(String address, String fileName)
        {
			throw new Exception("function is not supported");
        }

        public Boolean FileExists(string address)
        {
			return false;
            
        }

        public DateTime FileGetLastWriteTime(string address)
        {
			throw new Exception("function is not supported");
        }

        public Boolean DirectoryExists(string address, int timeout)
        {
			return false;
        }

        public void DownloadDirectory(string address, string localPath, string pattern)
        {
			throw new Exception("function is not supported");     
        }

        public FileHelper[] GetFiles(string address, string pattern, int timeout)
        {
			throw new Exception("function is not supported");
        }

        public void RenameFile(string sourceAddress, string destinationAddress)
        {
			throw new Exception("function is not supported");
        }

        public void DeleteFile(string address)
        {
			throw new Exception("function is not supported");
        }

        public DateTime GetDirectoryLastWriteTime(string address)
        {
			throw new Exception("function is not supported"); 
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
