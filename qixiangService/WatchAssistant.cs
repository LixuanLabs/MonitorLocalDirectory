using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace qixiangService 
{
    public class WatchAssistant : ConfigurationSection
    {
        [ConfigurationProperty("WatchDir")]
        public WatchDir watchDir
        {
            get { return (WatchDir)base["WatchDir"]; }
        }
    }

    public class WatchDir : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WatchDirElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            WatchDirElement e = (WatchDirElement)element;
            return e.Key;
        }

        private IDictionary<string, string> settings;

        public IDictionary<string,string> Settings
        {
            get
            {
                if(settings == null)
                {
                    settings = new Dictionary<string, string>();
                    foreach(WatchDirElement e in this)
                    {
                        
                            settings.Add(e.Key, e.Value);
                    }
                }
                return settings;
            }
        }

        public string this[string key]
        {
            get
            {
                string result = "";
                if(settings.TryGetValue(key,out result))
                {
                    return result;
                }else
                {
                    throw new ArgumentException("没有对"+key+"节点进行配置");
                }
            }
        }
    }

    public class WatchDirElement : ConfigurationElement
    {
        [ConfigurationProperty("key",IsRequired = true)]
        public string Key
        {
            get { return (string)base["key"]; }
        }
        [ConfigurationProperty("value",IsRequired = true)]
        public string Value
        {
            get { return (string)base["value"]; }
        }
    }
}
