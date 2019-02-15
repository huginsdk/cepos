using System;
using System.Collections;
using Hugin.POS.Common;

namespace Hugin.POS
{
    public class Invoice : SalesDocument, ICloneable
    {
    	static String[] coords = new String[2];        
        int id;
        public static String[] Coordinates {
			get {
				return coords;
			}
			set {
				coords = value;
			}
		}    
        public Invoice() {
        }

        public Invoice(SalesDocument doc)
            : base(doc)
        {
        }


        public override object Clone()
        {
            return new Invoice(this);
        }

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}
        public override String Name { get { return PosMessage.INVOICE; } }
        public override String Code { get { return PosMessage.HR_CODE_INVOICE; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.INVOICE; } }
       
    }
}
