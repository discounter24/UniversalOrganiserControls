using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Minecraft.Server.Properties
{
    public class IntProperty : Property
    {
        new public PropertyType TYPE
        {
            get => PropertyType.INT;
        }

        new public int DefaultValue
        {
            get => int.Parse(base.DefaultValue);
        }

        new public int Value
        {
            get => int.Parse(base.Value);
            set => base.Value = value.ToString().ToLower();
        }

        public IntProperty(string key, int value) : base(key,"")
        {
            this.Value = value;
        }
    }
}
