using System;
using System.Collections.Generic;
using System.Text;
using Hugin.POS.Common;

namespace Hugin.POS.Printer
{
    class ProgramReport
    {
        public static List<String> GetProgramReport()
        {
            String[] separator ={ "", "".PadLeft(40, '-'), "" };

            List<String> programReport = new List<string>();
            programReport.Add(SurroundTitle("PROGRAM B�LG� RAPORU"));
            programReport.AddRange(separator);

            programReport.Add("");
            programReport.Add(SurroundSubtitle("DEPARTMAN B�LG�LER�"));
            programReport.Add("");

            foreach (Department d in Department.Departments)
            {
                if (d != null && d.Valid)
                    programReport.Add(String.Format("{0,-30}%{1:D2}", d.Name, (int)(d.TaxRate * 100)));
            }

            programReport.Add("");
            programReport.Add(SurroundSubtitle("AYARLAR"));
            programReport.Add("");
            ISettings settings = CurrentSettings;
            programReport.Add(String.Format("{0,-30}{1,10}", "KA�IT KES�C�", settings.GetProgramOption(Setting.Autocutter) == 1 ? "A�IK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "�R�N M�N�MUM F�YATI", ((decimal)settings.GetProgramOption(Setting.MinimumPrice)) / 100m));
            programReport.Add(String.Format("{0,-30}{1,10}", "OTOMAT�K M��TER� �ND�R�M�", settings.GetProgramOption(Setting.AutoCustomerAdjustment) == 1 ? "A�IK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "�DEMEDE PARA �EKMECES� A�ILI�I", settings.GetProgramOption(Setting.OpenDrawerOnPayment) == 1 ? "A�IK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "BELGEDE �R�N BARKODU G�Z�KS�N", settings.GetProgramOption(Setting.PrintProductBarcode) == 1 ? "A�IK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "A.TOPLAM TU�UNDA BELGEYE YAZMA", settings.GetProgramOption(Setting.PrintSubtTotal) == 1 ? "A�IK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "K.KARTINDA TAKS�T SAYISI SORMA", settings.GetProgramOption(Setting.PromptCreditInstallments) == 1 ? "A�IK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "BELGEDE KAS�YER �SM� G�Z�KS�N", settings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == 1 ? "A�IK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "BELGE SONU NOTU G�Z�KS�N", settings.GetProgramOption(Setting.ShowFooterNote) == 1 ? "A�IK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10:C}", "F�� L�M�T�", new Number(settings.ReceiptLimit)));
            if (FiscalPrinter.Printer.IsFiscal)
            {
                programReport.Add(String.Format("{0,-30}{1,10}", "F�� SONUNDA BARKOD YAZILMASI", settings.GetProgramOption(Setting.PrintBarcode) == 1 ? "A�IK" : "KAPALI"));
                programReport.Add(String.Format("{0,-30}{1,10}", "GRAF�K LOGO BASIMI", settings.GetProgramOption(Setting.PrintGraphicLogo) == 1 ? "A�IK" : "KAPALI"));
            }
            return programReport;
        }

        private static string SurroundTitle(string title)
        {
            return SurroundwithStars(title, 20) +  "".PadRight(18);

        }
        private static string SurroundSubtitle(string title)
        {
            return SurroundwithStars(title, 40);
        }
        private static string SurroundwithStars(string title, int totalLen)
        {
            if (title.Length < totalLen - 1)
            {
                title = " " + title + " ";
                int left = (totalLen - title.Length) / 2;
                int right = (totalLen - title.Length) - left;
                title = "".PadLeft(left, '*') + title + "".PadRight(right, '*');
            }
            return title;
        }
        protected static ISettings CurrentSettings
        {
            get { return Data.Connector.Instance().CurrentSettings; }
        }
    }
}
