using System;
namespace Hugin.POS.Common
{
    public interface IConfirm
    {
        System.Collections.Hashtable Data { get; set; }
        bool HasData { get; }
        string Message { get; set; }
    }
}
