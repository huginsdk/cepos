using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class CurrentAccountDocument : SalesDocument, IMenuItem, ICloneable
    {
        int id;

        public CurrentAccountDocument()
        {
        }

        public CurrentAccountDocument(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new CurrentAccountDocument(this);
        }

        public sealed override int Id
        {
            get { return id; }
            set { id = value; }
        }

        public override String Name { get { return PosMessage.CURRENT_ACCOUNT_COLLECTION; } }
        public override String Code { get { return PosMessage.HR_CODE_CURRENT_ACCOUNT_COLLECTION; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.CURRENT_ACCOUNT_COLLECTION; } }
        public override bool CanEmpty { get { return true; } }
    }
}
