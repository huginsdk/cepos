using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS.Common
{
    public class Confirm : IConfirm
    {
        String message;
        Hashtable data;
        internal StateInstance<Hashtable> ReturnConfirmWithArgs;
        internal StateInstance ReturnConfirm;
        internal StateInstance ReturnCancel;

        public Confirm(){}

        public Confirm(String message)
        {
            this.Message = message;
        }

        internal Confirm(String message, StateInstance ConfirmState)
            : this(message)
        {
            ReturnConfirm = ConfirmState;
        }


        internal Confirm(String message, StateInstance ConfirmState, StateInstance CancelState)
            : this(message, ConfirmState)
        {
            ReturnCancel = CancelState;
        }

        internal Confirm(String message, StateInstance<Hashtable> ConfirmState)
            : this(message)
        {
            data = new Hashtable();
            ReturnConfirmWithArgs = ConfirmState;
        }

        internal Confirm(String message, StateInstance<Hashtable> ConfirmState, StateInstance CancelState)
            : this(message, ConfirmState)
        {
            ReturnCancel = CancelState;
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public Hashtable Data {
            get { if (data == null) data = new Hashtable(); return data; }
            set { data = value; }
        }

        public bool HasData {
            get { return (data != null); }
        }

    }
}
