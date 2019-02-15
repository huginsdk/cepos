using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class MealTicket : SalesDocument, IMenuItem, ICloneable
    {
        int id;
    	
        public MealTicket(){
        }

        public MealTicket(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new MealTicket(this);
        }  

   		public sealed override int Id {
    		get{return id;}
            set { id = value; }
    	}

        public override String Name { get { return PosMessage.MEAL_TICKET; } }
        public override String Code { get { return PosMessage.HR_CODE_MEAL_TICKET; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.MEAL_TICKET; } }
    }
}
