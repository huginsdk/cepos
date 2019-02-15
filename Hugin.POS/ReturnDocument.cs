using System;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class ReturnDocument : SalesDocument, IMenuItem, ICloneable
    {
        int id;
        
        public ReturnDocument(){
        }

        public ReturnDocument(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new ReturnDocument(this);
        }

        public override void Close()
        {
            base.Close();
        }

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}

        public override String Name { get { return PosMessage.RETURN_DOCUMENT_TR; } }
        public override String Code { get { return PosMessage.HR_CODE_RETURN; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.RETURN_DOCUMENT; } }
        
    }
}
