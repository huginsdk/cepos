using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS
{
    class SelfEmployementInvoice : SalesDocument, IMenuItem, ICloneable
    {
        int id;

        public SelfEmployementInvoice()
        {
        }

        public SelfEmployementInvoice(SalesDocument doc)
            : base(doc)
        {
        }

        public override object Clone()
        {
            return new Advance(this);
        }

        public sealed override int Id
        {
            get { return id; }
            set { id = value; }
        }

        public override String Name { get { return PosMessage.SELF_EMPLOYEMENT_INVOICE; } }
        public override String Code { get { return PosMessage.HR_CODE_SELF_EMPLOYEMENT_INVOICE; } }
        public override int DocumentTypeId { get { return (int)DocumentTypes.SELF_EMPLYOMENT_INVOICE; } }
        public override bool CanEmpty { get { return true; } }
    }
}
