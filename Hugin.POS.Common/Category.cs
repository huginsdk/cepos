using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public class Category
    {
        public const int NUM_OF_MAIN_CATS = 50;
        public const int NUM_OF_SUB_CATS = 250;

        public bool valid;
        public int CatNo = 0;
        public int MainCatNo = 0;
        public string Name = "";
    }
}
