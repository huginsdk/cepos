using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class CollectionInvoice : SalesDocument, IMenuItem, ICloneable
    {
        int id;
    	
        public CollectionInvoice(){
        }

        public CollectionInvoice(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new CollectionInvoice(this);
        }  

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}

        public override String Name { get { return PosMessage.COLLECTION_INVOICE; } }
        public override String Code { get { return PosMessage.HR_CODE_COLLECTION_INVOICE; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.COLLECTION_INVOICE; } }
        public override bool CanEmpty { get { return true; } }
    }
}
