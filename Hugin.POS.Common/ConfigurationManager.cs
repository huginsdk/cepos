using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;

namespace Hugin.POS.Common
{
    public enum ConfigurationUserLevel
    {
        None,
        PerUserRoaming,
        PerUserRoamingAndLocal
    }
    public class ConfigurationManager
    {
        private XmlDocument xmlSettings = null;
        private Dictionary<String, String> appSettings = null;
        private string configFile = "";
        private static ConfigurationManager config = null;
        private static System.Globalization.CultureInfo culture = null;

        private ConfigurationManager(String configFile)
        {
            try
            {
                culture = System.Globalization.CultureInfo.GetCultureInfo("en");

                appSettings = new Dictionary<string, string>();

                this.configFile = configFile;

                xmlSettings = new XmlDocument();

                xmlSettings.Load(configFile);

                String key = "";
                foreach (XmlNode node in xmlSettings.SelectNodes("configuration/appSettings/add"))
                {
                    key = ToUpper(node.Attributes["key"].Value);
                    if (appSettings.ContainsKey(key))
                        continue;
                    appSettings.Add(key, node.Attributes["value"].Value);
                }
            }
            catch (Exception ex)
            {
                Debugger.Instance().AppendLine(ex.Message);
                throw new ConfigurationFileNotFoundException();
            }
        }

        public static ConfigurationManager OpenExeConfiguration(ConfigurationUserLevel level)
        {
            if (config == null)
            {
                String xmlFile = IOUtil.ProgramDirectory + IOUtil.AssemblyName + ".config";

                if (!System.IO.File.Exists(xmlFile))
                    throw new Exception("CONFIG DOSYASI\nBULUNAMADI.");

                config = new ConfigurationManager(xmlFile);
            }

            return config;
        }

        public void Set(String key, String value)
        {
            XmlNode nodeToSet=xmlSettings.SelectSingleNode("configuration/appSettings/add[@key='" + key + "']");
            if(nodeToSet==null)
                nodeToSet=xmlSettings.SelectSingleNode("configuration/appSettings/add[@key='" + ToUpper(key) + "']");
            nodeToSet.Attributes["value"].Value = value;
            xmlSettings.Save(configFile);
            appSettings[ToUpper(key)] = value;
        }
        public string Get(String key)
        {
            key = ToUpper(key);
            if (appSettings.ContainsKey(key))
                return appSettings[key];
            return String.Empty;
        }
        public void Remove(String key)
        {
            key = ToUpper(key);
            xmlSettings.SelectSingleNode("configuration/appSettings").RemoveChild(xmlSettings.SelectSingleNode("add[@key='" + key + "']"));
            xmlSettings.Save(configFile);
            appSettings.Remove(key);
        }

        public void Add(String key, String value)
        {
            key = ToUpper(key);
            XmlElement child = xmlSettings.CreateElement("add");
            XmlAttribute attrKey = xmlSettings.CreateAttribute("key");
            XmlAttribute attrValue = xmlSettings.CreateAttribute("value");
            attrKey.Value = key;
            attrValue.Value = value;
            child.Attributes.Append(attrKey);
            child.Attributes.Append(attrValue);
            xmlSettings.SelectSingleNode("configuration/appSettings").AppendChild(child);
            xmlSettings.Save(configFile);
            appSettings.Add(key, value);
        }

        private string ToUpper(string key)
        {            
            return key.ToUpper(culture);
        }
    }
}
