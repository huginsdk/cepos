using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    class Report : IReport
    {

        String name;
        String method;
        Boolean canHardCopy;
        Boolean interruptable = false;
        AuthorizationLevel authorization;
        Dictionary<String, Type> parameters;
        List<IReport> subreports;
        IReport parent = null;
        ReportGroup group;
        String fileName;

        public Report(String name, String method, Boolean canHardCopy, AuthorizationLevel authorization, ReportGroup group, String fName)
        {
            this.name = name;
            this.method = method;
            this.canHardCopy = canHardCopy;
            this.authorization = authorization;
            this.parameters = new Dictionary<string, Type>();
            this.subreports = new List<IReport>();
            this.group = group;
            this.fileName = fName;
        }
        internal void SetInterruptable(Boolean val)
        {
            interruptable = val;
        }

        #region IReport Members

        public string Name
        {
            get { return name; }
        }

        public string Method
        {
            get { return method; }
        }

        public Boolean CanHardCopy
        {
            get { return canHardCopy; }
        }

        public Boolean Interruptable
        {
            get { return interruptable; }
        }
        public AuthorizationLevel Authorization
        {
            get { return authorization; }
        }

        public Dictionary<String, Type> Parameters
        {
            get { return parameters; }
        }

        public List<IReport> Subreports
        {
            get { return subreports; }
        }

        public IReport Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                parent.Subreports.Add(this);
            }
        }

        public ReportGroup Group
        {
            get { return group; }
        }

        public String FileName
        {
            get { return fileName; }
        }

        #endregion

    }
}
