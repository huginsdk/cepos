using System;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class Waybill : SalesDocument, IMenuItem, ICloneable
    {
    	int id;
    	
        public Waybill(){
        }

        public Waybill(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new Waybill(this);
        }  

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}

        public override String Name { get { return PosMessage.WAYBILL_TR; } }
        public override String Code { get { return PosMessage.HR_CODE_WAYBILL; } }
        public override int DocumentTypeId { get { return 3; } }
        
    }
}
