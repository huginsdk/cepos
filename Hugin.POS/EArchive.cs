using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class EArchive : SalesDocument, IMenuItem, ICloneable
    {
        int id;
    	
        public EArchive(){
        }

        public EArchive(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new EArchive(this);
        }  

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}

        public override String Name { get { return PosMessage.E_ARCHIVE; } }
        public override String Code { get { return PosMessage.HR_CODE_E_ARCHIVE; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.E_ARCHIEVE; } }
    }
}
