using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;
using Hugin.POS.Common;

namespace Hugin.POS.Display.GUI
{
    class Template
    {
        #region Data
        private static XmlDocument templateFile = null;

        private static String TemplatePath
        {
            get { return PosConfiguration.DataPath + "Template.xml"; }
        }
        private static XmlDocument TemplateFile
        {
            get
            {
                if (templateFile == null)
                {
                    templateFile = new XmlDocument();
                    String template = (String)Hugin.POS.Display.GUI.Properties.Resources.ResourceManager.GetObject("Template");
                    if (!String.IsNullOrEmpty(template))
                        templateFile.LoadXml(template);
                }
                return templateFile;
            }
        }
        private static XmlNode Keys(String section, String key)
        {
            return TemplateFile.SelectSingleNode("Settings/" + section + "/add[@key='" + key + "']");
        }

        public static void SetDefaultText(XPath attr, String text)
        {
            Set(GetXPath(attr), "Text", text);
        }
        public static String GetDefaultText(XPath attr)
        {
            return Get(GetXPath(attr), "Text");
        }

        public static void SetFont(XPath xpath, Font font)
        {
            //font.FontFamily.Name : String
            //font.Size : float
            //font.Style: (int)FontStyle
            //font.Unit: (int)GraphicsUnit
            //font.GdiCharSet : byte
            //font.GdiVerticalFont : bool
            string fnt = font.Name + " | " + font.Size + " | " + (int)(font.Style) + " | " + (int)(font.Unit) + " | " + (int)(font.GdiCharSet)
                + " | " + (font.GdiVerticalFont ? "1" : "0");
            Set(GetXPath(xpath), "Font", fnt);
        }
        public static Font GetFont(XPath xpath)
        {
            string[] fnt = Get(GetXPath(xpath), "Font").Split('|');
            return new Font(fnt[0].Trim(), float.Parse(fnt[1]), (FontStyle)(int.Parse(fnt[2])), (GraphicsUnit)(int.Parse(fnt[3])),
                (byte)(int.Parse(fnt[4])), fnt[5] == "1");
        }
        public static void SetForecolor(XPath xpath, Color col)
        {
            Set(GetXPath(xpath), "Forecolor", "" + col.ToArgb());
        }
        public static Color GetForecolor(XPath xpath)
        {
            return Color.FromArgb(int.Parse(Get(GetXPath(xpath), "Forecolor")));
        }
        public static void SetBackcolor(XPath xpath, Color col)
        {
            Set(GetXPath(xpath), "Backcolor", "" + col.ToArgb());
        }
        public static Color GetBackcolor(XPath xpath)
        {
            return Color.FromArgb(int.Parse(Get(GetXPath(xpath), "Backcolor")));
        }
        public static void SetVisible(XPath xpath, bool visible)
        {
            Set(GetXPath(xpath), "Visible", visible ? "1" : "0");
        }
        public static bool GetVisible(XPath xpath)
        {
            return Get(GetXPath(xpath), "Visible").Trim() == "1";
        }
        public static void SetBackImage(XPath xpath, String path)
        {
            Set(GetXPath(xpath), "BackImage", path);
        }
        public static String GetBackImage(XPath xpath)
        {
            return Get(GetXPath(xpath), "BackImage");
        }

        private static string GetXPath(XPath xpath)
        {
            switch (xpath)
            {
                case XPath.MAIN: return "main";
                case XPath.MESSAGE: return "message";
                case XPath.HEADER: return "header";
                case XPath.HEAD_FIRM: return "header/firmlogo";
                case XPath.HEAD_HUGIN: return "header/huginlogo";
                case XPath.SALE_GRID: return "salegrid";
                case XPath.SALE_COLUMNCELL: return "salegrid/columncell";
                case XPath.SALE_ROWCELL: return "salegrid/rowcell";
                case XPath.CURRENT: return "current";
                case XPath.CURRENT_TOTAL: return "current/total";
                case XPath.CURRENT_CUST1: return "current/customer/first";
                case XPath.CURRENT_CUST2: return "current/customer/second";
                case XPath.DETAIL: return "detail";
                case XPath.CUST_INFO: return "detail/customer";
                case XPath.CUST_INFO_ASSINGED: return "detail/customerassigned";
                case XPath.ADJ_TITLE: return "detail/adjustment/title";
                case XPath.ADJ_PRODUCT_TITLE: return "detail/adjustment/product/title";
                case XPath.ADJ_SUB_TITLE: return "detail/adjustment/sub/title";
                case XPath.ADJ_TOTAL_TITLE: return "detail/adjustment/total/title";
                case XPath.ADJ_PRODUCT_VAL: return "detail/adjustment/product/value";
                case XPath.ADJ_SUB_VAL: return "detail/adjustment/sub/value";
                case XPath.ADJ_TOTAL_VAL: return "detail/adjustment/total/value";
                case XPath.DOC_REGISTERID_TITLE: return "detail/document/registerid/title";
                case XPath.DOC_REGISTERID: return "detail/document/registerid/value";
                case XPath.DOC_ID_TITLE: return "detail/document/id/title";
                case XPath.DOC_DATE_TITLE: return "detail/document/date/title";
                case XPath.DOC_TIME_TITLE: return "detail/document/time/title";
                case XPath.DOC_ID_VAL: return "detail/document/id/value";
                case XPath.DOC_DATE_VAL: return "detail/document/date/value";
                case XPath.DOC_TIME_VAL: return "detail/document/time/value";
                case XPath.DOC_SUBTOTAL_TITLE: return "detail/document/subtotal/title";
                case XPath.DOC_SUBTOTAL_VAL: return "detail/document/subtotal/value";
                case XPath.DETAIL_PRODUCT: return "detail/product";
                default: return "";
            }
        }

        private static String Get(String xpath, String key)
        {
            try
            {
                return Keys(xpath, key).Attributes["value"].Value;
            }
            catch
            {
                Display.Log.Error("Ekran ayarý yüklenemedi : " + xpath + " " + key);
                return "";
            }
        }
        private static void Set(String xpath, String key, String value)
        {
            try
            {
                Keys(xpath, key).Attributes["value"].Value = value;
                TemplateFile.Save(TemplatePath);
            }
            catch { Display.Log.Error("Ekran ayarý kaydedilemedi : " + xpath + " " + key); }
        }

        #endregion Data

    }
}
