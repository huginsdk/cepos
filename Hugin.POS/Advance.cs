using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class Advance : SalesDocument, IMenuItem, ICloneable
    {
        int id;
    	
        public Advance(){
        }

        public Advance(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new Advance(this);
        }  

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}

        public override String Name { get { return PosMessage.ADVANCE; } }
        public override String Code { get { return PosMessage.HR_CODE_ADVANCE; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.ADVANCE; } }
        public override bool CanEmpty { get { return true; } }
    }
}
