using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.UPnP
{
    public class UPnPPort
    {
        public string Description;
        public PortType Type;

        public UInt16 ExternalPort;

        public UInt16 InternalPort;
        public string InternalAddress;


        public static bool operator ==(UPnPPort a, UPnPPort b)
        {
            if (a.InternalPort!=b.InternalPort) return false;
            /*if (a.InternalAddress != b.InternalAddress) return false;

            if (a.Type != b.Type) return false;

            if (a.ExternalPort != b.ExternalPort) return false;

            if (a.Description != b.Description) return false;*/

            return true;
        }

        public static bool operator !=(UPnPPort a, UPnPPort b)
        {
            return !(a == b);
        }

     
    }
}
