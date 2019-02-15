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
            programReport.Add(SurroundTitle("PROGRAM BÝLGÝ RAPORU"));
            programReport.AddRange(separator);

            programReport.Add("");
            programReport.Add(SurroundSubtitle("DEPARTMAN BÝLGÝLERÝ"));
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
            programReport.Add(String.Format("{0,-30}{1,10}", "KAÐIT KESÝCÝ", settings.GetProgramOption(Setting.Autocutter) == 1 ? "AÇIK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "ÜRÜN MÝNÝMUM FÝYATI", ((decimal)settings.GetProgramOption(Setting.MinimumPrice)) / 100m));
            programReport.Add(String.Format("{0,-30}{1,10}", "OTOMATÝK MÜÞTERÝ ÝNDÝRÝMÝ", settings.GetProgramOption(Setting.AutoCustomerAdjustment) == 1 ? "AÇIK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "ÖDEMEDE PARA ÇEKMECESÝ AÇILIÞI", settings.GetProgramOption(Setting.OpenDrawerOnPayment) == 1 ? "AÇIK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "BELGEDE ÜRÜN BARKODU GÖZÜKSÜN", settings.GetProgramOption(Setting.PrintProductBarcode) == 1 ? "AÇIK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "A.TOPLAM TUÞUNDA BELGEYE YAZMA", settings.GetProgramOption(Setting.PrintSubtTotal) == 1 ? "AÇIK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "K.KARTINDA TAKSÝT SAYISI SORMA", settings.GetProgramOption(Setting.PromptCreditInstallments) == 1 ? "AÇIK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "BELGEDE KASÝYER ÝSMÝ GÖZÜKSÜN", settings.GetProgramOption(Setting.ShowCashierNameOnReceipt) == 1 ? "AÇIK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10}", "BELGE SONU NOTU GÖZÜKSÜN", settings.GetProgramOption(Setting.ShowFooterNote) == 1 ? "AÇIK" : "KAPALI"));
            programReport.Add(String.Format("{0,-30}{1,10:C}", "FÝÞ LÝMÝTÝ", new Number(settings.ReceiptLimit)));
            if (FiscalPrinter.Printer.IsFiscal)
            {
                programReport.Add(String.Format("{0,-30}{1,10}", "FÝÞ SONUNDA BARKOD YAZILMASI", settings.GetProgramOption(Setting.PrintBarcode) == 1 ? "AÇIK" : "KAPALI"));
                programReport.Add(String.Format("{0,-30}{1,10}", "GRAFÝK LOGO BASIMI", settings.GetProgramOption(Setting.PrintGraphicLogo) == 1 ? "AÇIK" : "KAPALI"));
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
