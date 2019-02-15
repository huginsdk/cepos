using System;
using System.IO;
using Hugin.POS.Common;

namespace Hugin.POS
{

    public delegate void FatalEventHandler(String DisplayMessage);

    public class Chassis
    {
		public static CashRegisterInput Engine = null;

        public static FatalEventHandler FatalErrorOccured;

		public static void Register(CashRegisterInput form)
        {
			Engine = form;
            Engine.SetVisible(false);
        }

        public static void RestartProgram(bool updateProgram)
        {
            
        }

        public static void CloseApplication()
        {
			Engine.Dispose();
        }


        internal static void ShutDown(bool restart)
        {
            
        }
    }        

}

