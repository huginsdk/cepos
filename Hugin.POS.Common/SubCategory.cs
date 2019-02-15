using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public class SubCategory
    {
        public int CatNo = 0;
        public int MainCatNo = 0;
        public string Name = "";
        public override bool Equals(object obj)
        {
            bool response = false;

            if (this == null && obj == null)
            {
                response = true;
            }
            else if( this != null &&
                obj != null &&
                this.CatNo ==((SubCategory)obj).CatNo && 
                this.MainCatNo ==((SubCategory)obj).MainCatNo &&
                this.Name == ((SubCategory)obj).Name)
            {
                response = true;
            }

            return response;
        }
    }
}
