using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class CarParkDocument : SalesDocument, IMenuItem, ICloneable
    {
        int id;
    	
        public CarParkDocument(){
        }

        public CarParkDocument(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new CarParkDocument(this);
        }  

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}

        public override String Name { get { return PosMessage.CAR_PARKIMG; } }
        public override String Code { get { return PosMessage.HR_CODE_CAR_PARKING; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.CAR_PARKING; } }
        public override bool CanEmpty { get { return true; } }
    }
}
