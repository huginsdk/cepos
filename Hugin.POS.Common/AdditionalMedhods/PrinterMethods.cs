using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public interface IPeriodicCashierReport
    {
        IPrinterResponse PrintRegisterReportSummary(ICashier cashier, DateTime firstDay, DateTime lastDay, bool hardcopy);
    }

    public interface IServerPrinter
    {
        byte[] SendRawMessage(byte[] messageBuffer);
    }
}
