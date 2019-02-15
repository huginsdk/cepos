using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public enum ReportGroup
    {
        NONE,
        X_REPORTS,
        Z_REPORTS,
        EJ_REPORTS
    }

    public interface IReport
    {
        String Name { get;}
        String Method { get;}
        Boolean CanHardCopy { get;}
        Boolean Interruptable { get;}
        AuthorizationLevel Authorization { get;}
        Dictionary<String,Type> Parameters { get;}
        List<IReport> Subreports { get;}
        IReport Parent { get;set;}
        ReportGroup Group { get; }
        String FileName { get; }
    }
}
