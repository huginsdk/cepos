using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

namespace Hugin.POS.Common
{
    public class TransferClientFtp : ITransferClient
    {
        static Thread connectionCheckerThread;
        private Uri uri = null;
        private WebClient webClient = null;

        internal void SetCredentials(string address)
        {
            if (webClient == null)
                webClient = new WebClient();
            else if (webClient.Credentials != null)
                return;
            uri = new Uri(address);
            String userName = uri.UserInfo.Split(':')[0];
            String password = "";
            if (uri.UserInfo.Contains(":"))
                password = uri.UserInfo.Split(':')[1];
            webClient.Credentials = new NetworkCredential(userName, password);
        }

        public void Disconnect(){
            webClient.Dispose();
        }

        public String DownloadString(string address)
        {
            if (uri == null) SetCredentials(address);
            return webClient.DownloadString(address);
        }

        public void DownloadFile(String address, String fileName)
        {
            if (uri == null) SetCredentials(address);
            webClient.DownloadFile(address, fileName); //TODO This should overwrite if file exists
        }

        //TODO maybe should return lastwritetime from server
        public void UploadString(String address, String data)
        {
            if (uri == null) SetCredentials(address);
            webClient.UploadString(address, data);
        }

        public void UploadFile(String address, String fileName)
        {
            if (uri == null) SetCredentials(address);
            webClient.UploadFile(address, fileName); //TODO This should overwrite if file exists
        }

        public void AppendString(String address, String data)
        {
            if (uri == null) SetCredentials(address);
            webClient.UploadString(address, "APPE", data);
        }

        public void AppendFile(String address, String fileName)
        {
            if (uri == null) SetCredentials(address);
            webClient.UploadFile(address, "APPE", fileName);
        }

        public Boolean FileExists(string address)
        {
            if (uri == null) SetCredentials(address);
            try
            {
                FileGetLastWriteTime(address);
                return true;
            }
            catch { return false; }
        }

        public DateTime FileGetLastWriteTime(string address)
        {
            if (uri == null) SetCredentials(address);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ConnectFailure ||
                    we.Status == WebExceptionStatus.ConnectionClosed ||
                    we.Status == WebExceptionStatus.NameResolutionFailure ||
                    we.Status == WebExceptionStatus.Timeout)
                    throw new BackOfficeUnavailableException(we.Message);
                else throw new FileNotFoundException("WebException", we);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
            return response.LastModified;
            
        }
        
        public Boolean DirectoryExists(string address, int timeout)
        {
            if (uri == null) SetCredentials(address);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)request.GetResponse();
                response.Close();
                return true;
            }
            catch { return false; }
        }

        public void DownloadDirectory(string Src, string Dst, string pattern)
        {

        }

    }
}
