using System;
using System.Collections.Generic;
using System.Text;

namespace Hugin.POS.Common
{
    public class WordConversion
    {
        public static string strResponse;
        public static int linemax = 48;
        public static int counter = 0;

        public WordConversion()
        {
        }
        public static string ConvertLetter(decimal amount)
        {
            int whole = (int)amount;
            strResponse = "";
            counter = 0;

            int fraction = Convert.ToInt32((amount - whole) * 100);

            int step = 1000000;

            add(PosMessage.ONLY + " ");

            while (step > 0)
            {
                int currentStep = whole / step;

                if (currentStep > 0)
                {
                    GetInWord(currentStep, (int)(Math.Log(step)/Math.Log(100))); //TODO CF ,1000
                    whole = whole % step;
                }
                step = step / 1000;
            }
            if ((int)amount > 0)
            {
                add(" " + PosMessage.YTL + " ");
            }

            if (fraction > 0)
            {
                GetInWord(fraction, 0);

                add(" " + PosMessage.YKR);
            }
            if ((int)amount == 0 && fraction == 0)
                add(PosMessage.ZERO + " " + PosMessage.YTL + " ");
            add("'DiR");

            return strResponse;
        }
        public static void GetInWord(int currentStep, int step)
        {
            string strStep = "";

            switch (step)
            {
                case 0:
                    break;
                case 1:
                    if (currentStep == 1)
                    {
                        add(PosMessage.THOUSAND);
                        return;
                    }
                    strStep = PosMessage.THOUSAND;
                    break;
                case 2:
                    strStep = PosMessage.MILLION;
                    break;
                case 3:
                    strStep = PosMessage.BILLION;
                    break;
                default: break;
            }

            int divisor = 100;

            while (divisor > 0)
            {
                int digit = currentStep / divisor;

                if (digit > 0)
                {
                    GetDigitLetter(digit, divisor);
                    currentStep = currentStep % divisor;
                }
                divisor = divisor / 10;
            }

            add(strStep);
        }

        public static void GetDigitLetter(int digit, int divisor)
        {
            switch (divisor)
            {
                case 100:
                    add(digit > 1 ? TranslateOne(digit) : "");
                    add(PosMessage.HUNDRED);
                    break;
                case 10: add(TranslateTen(digit));
                    break;
                case 1: add(TranslateOne(digit));
                    break;
                default: break;
            }
        }

        public static String TranslateTen(int digit)
        {
            switch (digit)
            {
                case 1: return PosMessage.TEN;
                case 2: return PosMessage.TWENTY;
                case 3: return PosMessage.THIRTY;
                case 4: return PosMessage.FOURTY;
                case 5: return PosMessage.FIFTY;
                case 6: return PosMessage.SIXTY;
                case 7: return PosMessage.SEVENTY;
                case 8: return PosMessage.EIGHTY;
                case 9: return PosMessage.NINETY;
                default: return "";

            }
        }

        public static string TranslateOne(int digit)
        {
            switch (digit)
            {
                case 1: return PosMessage.ONE;
                case 2: return PosMessage.TWO;
                case 3: return PosMessage.THREE;
                case 4: return PosMessage.FOUR;
                case 5: return PosMessage.FIVE;
                case 6: return PosMessage.SIX;
                case 7: return PosMessage.SEVEN;
                case 8: return PosMessage.EIGHT;
                case 9: return PosMessage.NINE;
                default: return "";

            }
        }



        private static void add(string p)
        {
            if ((counter + p.Length) > linemax)
            {
                strResponse = strResponse + "\n";
                counter = 0;
            }
            strResponse = strResponse + p;
            counter += p.Length;
        }
    }
}
