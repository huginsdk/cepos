using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hugin.POS.Common
{
    public class FileHelper 
    {
        private String loginLine = PosConfiguration.Get("OfficeLogin");
        private String fileName = "";
        private DateTime creationTime = DateTime.MinValue;
        private ITransferClient fxClient = null;

		//assigned but never used
        //private static ITransferClient LastClient = null;

        public String FullName
        {
            get { return fileName; }
        }

        public DateTime CreationTime
        {
            get { return creationTime; }
            set { creationTime = value; }
        }

        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(fileName);
            }
        }

        public System.String LoginLine
        {
            get { return loginLine; }
            set {
                loginLine = value;
                fxClient = new TransferClientFtp(loginLine);
            }
        }

        /// <summary>
        /// Initializes a new instance of the FileHelper class.
        /// </summary>
        /// <param name="fileName">
        ///     The fully qualified name of the new file, or the relative file name.
        /// </param>
        public FileHelper(string fileName)
        {
            this.fileName = fileName;

            Uri uri = new Uri(fileName);
            if (uri.Scheme == Uri.UriSchemeFile)
            {
                fxClient = new TransferClientFile();

                try
                {
                    creationTime = fxClient.FileGetLastWriteTime(fileName);
                }
                catch { }

            }
            else if (uri.Scheme == Uri.UriSchemeFtp)
            {
                fxClient = new TransferClientFtp(loginLine);
            }
            else if (uri.Scheme == Uri.UriSchemeHttp)
            {

            }

        }

        public void Rename(string destFileName)
        {
            try
            {
                fxClient.RenameFile(fileName, destFileName);
            }
            catch (Exception ex)
            {
				Log (ex);
                throw new IOException();
            }
        }

        public void Delete()
        {
            try
            {
                if (Exists)
                    fxClient.DeleteFile(fileName);
            }
            catch (Exception ex)
			{
				Log (ex);
                throw new IOException();
            }
        }

        public String ReadAllText()
        {
            try
            {
                return fxClient.DownloadString(fileName, 4000);
            }
            catch (Exception)
            {
                throw new IOException();
            }
        }


        public void WriteAllText(string text)
        {
            try
            {
                if (this.Exists)
                {
                    this.Delete();
                }
                fxClient.AppendString(fileName, text);
            }
            catch (Exception)
            {
                throw new IOException();
            }
        }

        public bool Exists
        {
            get
            {
                return fxClient.FileExists(fileName);
            }
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