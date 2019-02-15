using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;
using System.Reflection;

namespace Hugin.POS
{
    public class ModuleManeger
    {
        private static string extension = ".dll";
        public static object LoadModule(string moduleName, string typeName)
        {
            //Load dll dynamicly "Hugin.POS.EftPos.dll"
            Assembly assembly = Assembly.LoadFrom(IOUtil.ProgramDirectory + moduleName + extension);

            //Get class type
            Type t = assembly.GetType(moduleName + "." + typeName);

            // Get the default contructor
            ConstructorInfo consInfo = t.GetConstructor(new Type[] { });

            // Invoke the constructor and store the reference
            return consInfo.Invoke(null);
        }
    }
}
