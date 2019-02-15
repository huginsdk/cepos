using System;

namespace Hugin.POS.States
{
    
	public abstract class DocumentState
    {
        public virtual void AddItem(SalesDocument doc, FiscalItem item){
           
        }
        
        public virtual void ShowSubTotal(SalesDocument doc) {
            doc.State = DocumentSubTotal.Instance();
        }

        public abstract override String ToString();
    }
	
}
