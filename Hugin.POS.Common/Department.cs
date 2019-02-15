using System;

namespace Hugin.POS.Common
{

	/// <summary>
	/// Description of Department.
	/// </summary>
	public class Department
	{
        public const int NUM_DEPARTMENTS = 8;
        public const int NUM_TAXGROUPS = 8;
        private static Decimal[] taxRates = new Decimal[NUM_TAXGROUPS];
        private static Department[] departments = new Department[Department.NUM_DEPARTMENTS];

		int id;
        int taxGroupId;
        String name;
        
        bool valid = true;

        public bool Valid
        {
            get { return valid; }
            set { valid = value; if (!value) name = "KULLANIM DIÞI    "; }
        }

        public Department() { }

        public Department(int id, string name)
        {
            this.id = id;
            this.name = name;
            
        }

        public int Id
        {
            get
            {
                return id;
            }
        }
        public static Decimal[] TaxRates
        {
            get { return taxRates; }
        }


        public static Department[] Departments
        {
            get
            {
                return departments;
            }
        } 

        public int TaxGroupId
        {
            get
            {
                return taxGroupId;
            }
            set
            {
                if (value < 1 && value > NUM_TAXGROUPS)
                    throw new Exception("TAXGROUPID MUST BE BETWEEN [1 & " + (NUM_TAXGROUPS) + "]");
                taxGroupId = value;
            }
        }		
        public string Name
        {
            get
            {
                return name;
            }
        }
        public Decimal TaxRate
        {
            get
            {
                return Department.TaxRates[TaxGroupId - 1];
            }
        }
        public override bool Equals(object obj)
        {
            Department d = obj as Department;
            if (d == null) return false;
            return (d.Id == id && d.Valid == valid && d.TaxRate == TaxRate);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
	}
}
