using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;
using Rebex.Net;

namespace Hugin.POS.Common
{
    public class TransferClientFtp : ITransferClient
    {
        private Uri uri = null;
        private String loginInfo = "";
        private String userName = "", password = "";
        private List<Ftp> ftpList = null;
        private int pendingCount = 0;
        
        private const int FTP_TIMEOUT = 60000;
        private static Object lockObj = new Object();

        public static string MessageFileName
        {
            get { return PosConfiguration.ServerControlPath + "Mesaj." + PosConfiguration.Get("RegisterId"); }
        }

        public TransferClientFtp(string loginLine)
        {
            loginInfo = loginLine;
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
                        string login = loginInfo;
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
                    catch (FtpException fe)
                    {
                        EZLogger.Log.Debug("exception in setreq :" + fe.Status + " & " + fe.Response + " & " + fe.Message);
                        if (fe.Status == FtpExceptionStatus.Pending)
                        {
                            pendingCount++;
                            if (pendingCount < 4)
                            {
                                ftpList.Remove(currentFtp);
                                return SetRequest(address);
                            }
                            pendingCount = 0;
                        }
                        throw new BackOfficeUnavailableException(fe.Message);
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
            Ftp currentFtp = null;
            try
            {
                currentFtp = SetRequest(address);

                currentFtp.Timeout = timeout;

                long offset = currentFtp.GetFileLength(uri.LocalPath);

                Stream stream = new MemoryStream();
                Thread t = new Thread(delegate()
                {
                    try
                    {
                        currentFtp.GetFile(uri.LocalPath, stream);
                    }
                    catch { }
                });
                t.Start();
                t.Join(timeout);

                stream.Position = 0;

                StreamReader reader = new StreamReader(stream);
                string context = reader.ReadToEnd();

                if (String.IsNullOrEmpty(context))
                {
                    currentFtp.Disconnect();
                    throw new BackOfficeUnavailableException();
                }

                reader.Close();
                return context;
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug("exception in download string setreq : " + e.Message);
                throw e;
            }
            finally { currentFtp.Timeout = FTP_TIMEOUT; }
        }

        public void DownloadFile(String address, String fileName)
        {
            try
            {
                Ftp currentFtp = SetRequest(address);

                currentFtp.GetFile(uri.LocalPath, fileName);
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug("exception in download file setreq : " + e.Message);
                throw e;
            }
        }

        //TODO maybe should return lastwritetime from server
        public void UploadString(String address, String data, int timeout)
        {
            Ftp currentFtp = null;
            try
            {
                currentFtp = SetRequest(address);
                currentFtp.Timeout = timeout;

                Stream s = new MemoryStream(PosConfiguration.DefaultEncoding.GetBytes(data));
                currentFtp.PutFile(s, uri.LocalPath);
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug("exception in upload string setreq : " + e.Message);
                throw e;
            }
            finally { currentFtp.Timeout = FTP_TIMEOUT; }

        }

        public void UploadFile(String address, String fileName)
        {
            try
            {
                Ftp currentFtp = SetRequest(address);

                currentFtp.PutFile(fileName, uri.LocalPath);
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug("exception in upload file setreq : " + e.Message);
                throw e;
            }
        }

        public void AppendString(String address, String data)
        {
            try
            {
                Ftp currentFtp = SetRequest(address);
                Stream s = new MemoryStream(PosConfiguration.DefaultEncoding.GetBytes(data));
                currentFtp.AppendFile(s, uri.LocalPath);
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug("exception in append string setreq : " + e.Message);
                throw e;
            }

        }

        public void AppendFile(String address, String fileName)
        {
            try
            {
                Ftp currentFtp = SetRequest(address);

                currentFtp.AppendFile(fileName, uri.LocalPath);
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug("exception in append string setreq : " + e.Message);
                throw e;
            }
        }

        public Boolean FileExists(string address)
        {
            Ftp currentFtp = null;
            try
            {
                currentFtp = SetRequest(address);
            }
            catch (Exception ex)
            {
				Log (ex);
                return false;
            }

            currentFtp.Timeout = 5000;
            try
            {
                return currentFtp.FileExists(uri.LocalPath);
            }
			catch (Exception ex) { Log (ex); return false; }
            finally { currentFtp.Timeout = FTP_TIMEOUT; }
            
        }

        public DateTime FileGetLastWriteTime(string address)
        {
            Ftp currentFtp = null;
            try
            {
                currentFtp = SetRequest(address);
                currentFtp.Timeout = 3000;
                return currentFtp.GetFileDateTime(uri.LocalPath);
            }
            catch (FtpException ftpe)
            {
                EZLogger.Log.Debug("ftp.GetFileDateTime " + ftpe.Status.ToString());
                if (ftpe.Status == FtpExceptionStatus.ProtocolError && ftpe.Response.Code == 550) //File not found
                    throw new FileNotFoundException(ftpe.Message);
                else
                    throw new BackOfficeUnavailableException(ftpe.Message);
            }
            catch (Exception e)
            {
                throw new BackOfficeUnavailableException(e.Message);
            }
            finally { currentFtp.Timeout = FTP_TIMEOUT; }
        }

        public Boolean DirectoryExists(string address, int timeout)
        {
            Ftp currentFtp = null;
            try
            {
                currentFtp = SetRequest(address);
                currentFtp.Timeout = timeout;
            }
            catch 
            {
                return false;
            }
            try
            {
                return currentFtp.DirectoryExists(uri.LocalPath);
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug(e.Message);
                return false;
            }
        }

        public void DownloadDirectory(string address, string localPath, string pattern)
        {
            Ftp currentFtp = SetRequest(address);
            currentFtp.GetFiles(uri.LocalPath + pattern, 
                            localPath, 
                            FtpBatchTransferOptions.Default,
                            FtpActionOnExistingFiles.OverwriteAll);     
        }

        public FileHelper[] GetFiles(string address, string pattern, int timeout)
        {
            Ftp currentFtp = SetRequest(address);

            currentFtp.Timeout = timeout;
            List<FileHelper> fileList = new List<FileHelper>();

            try
            {

                currentFtp.ChangeDirectory(uri.LocalPath);

                FtpList items = new FtpList();
                Thread t = new Thread(delegate()
                    {
                        try
                        {
                            items = currentFtp.GetList(pattern);
                        }
                        catch { }
                    });

                t.Start();
                t.Join(timeout);

                foreach (FtpItem item in items)
                    if (item.IsFile)
                    {
                        FileHelper fileInfo = new FileHelper(uri + item.Name);
                        fileInfo.CreationTime = item.Modified;
                        fileList.Add(fileInfo);
                    }
            }
			catch (Exception ex) { Log (ex);}
            finally { currentFtp.Timeout = FTP_TIMEOUT; }

            return fileList.ToArray();
        }

        public void RenameFile(string sourceAddress, string destinationAddress)
        {
            try
            {
                Ftp currentFtp = SetRequest(sourceAddress);
                Thread.Sleep(100);
                Uri destFile = new Uri(destinationAddress);
                currentFtp.Rename(uri.LocalPath, destFile.LocalPath);
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug("exception in renaming file setreq : " + e.Message);
                throw e;
            }
        }

        public void DeleteFile(string address)
        {
            try
            {
                Ftp currentFtp = SetRequest(address);
                currentFtp.DeleteFile(address);
            }
            catch (Exception e)
            {
                EZLogger.Log.Debug("exception in deleting file setreq : " + e.Message);
                throw e;
            }
        }

        public DateTime GetDirectoryLastWriteTime(string address)
        {
            Ftp currentFtp = SetRequest(address);

            List<FileHelper> fileList = new List<FileHelper>();

            currentFtp.ChangeDirectory(uri.LocalPath);
            currentFtp.ChangeDirectory("..");

            DateTime lastWriteTime = DateTime.MinValue;
            currentFtp.Timeout = 3000;

            try
            {
                //Todo: Can't search with pattern for directory, so gets all item and then compare them with names.
                FtpList items = currentFtp.GetList();

                foreach (FtpItem item in items)
                {
                    if (item.IsDirectory && item.Name == uri.LocalPath.Trim(new char[] { '\\', '/' }))
                    {
                        lastWriteTime = item.Modified;
                    }
                }
            }
			catch (Exception ex) { Log (ex);}
            finally { currentFtp.Timeout = FTP_TIMEOUT; }

            return lastWriteTime;
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
