using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Minecraft.Server.Properties
{

    public class Property
    {
        public PropertyType TYPE
        {
            get => PropertyType.STRING;
        }


        private string _key;
        public string Key
        {
            get => _key;
        }
        
        public string DefaultValue
        {
            get => ServerProperties.DefaultProperties[Key];
        }


        private string _value;
        public string Value
        {
            get => _value;
            set => _value = value==null ? (DefaultValue==null?"":DefaultValue) : value;
        }

        protected Property(string key, string value)
        {
            this._key = key;
            this.Value = value;
        }


        public static Property get(string key, string value)
        {
            bool b = false;

            if (bool.TryParse(value, out b))
            {
                return new BoolProperty(key, b);
            }
            else
            {
                return new Property(key, value);
            }

        }
        

        new public string ToString()
        {
            return string.Format("{0}={1}",Key,Value);
        }

    }

}
