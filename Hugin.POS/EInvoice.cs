using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class EInvoice : SalesDocument, IMenuItem, ICloneable
    {
        int id;
    	
        public EInvoice(){
        }

        public EInvoice(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new EInvoice(this);
        }  

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}

        public override String Name { get { return PosMessage.E_INVOICE; } }
        public override String Code { get { return PosMessage.HR_CODE_E_INVOICE; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.E_INVOICE; } }
    }
}
