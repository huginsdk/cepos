using System;
namespace Hugin.POS.Common
{
    public interface ITransferClient
    {
        void AppendFile(string address, string fileName);
        void AppendString(string address, string data);
        bool DirectoryExists(string address, int timeout);
        void Disconnect();
        void DownloadDirectory(string Src, string Dst, string pattern);
        DateTime GetDirectoryLastWriteTime(string address);
        void DownloadFile(string address, string fileName);
        string DownloadString(string address, int timeout);
        bool FileExists(string address);
        DateTime FileGetLastWriteTime(string address);
        void UploadFile(string address, string fileName);
        void UploadString(string address, string data, int timeout);
        void RenameFile(string sourceAddress, string destinationAddress);
        void DeleteFile(string address);
        FileHelper[] GetFiles(string address, string pattern, int timeout);
    }
}
