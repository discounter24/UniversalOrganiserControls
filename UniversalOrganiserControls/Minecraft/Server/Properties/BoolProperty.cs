using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Minecraft.Server.Properties
{
    public class BoolProperty : Property
    {
        new public PropertyType TYPE
        {
            get => PropertyType.BOOL;
        }

        new public bool DefaultValue
        {
            get => Boolean.Parse(base.DefaultValue);
        }

        new public bool Value
        {
            get
            {
                try
                {
                    return Boolean.Parse(base.Value);
                }
                catch (Exception)
                {
                    return false;
                }
                
            }
            set => base.Value = value.ToString().ToLower();
        }

        public BoolProperty(string key, bool value) : base(key,"")
        {
            this.Value = value;
        }


    }
}
